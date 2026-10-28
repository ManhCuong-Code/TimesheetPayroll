// ==============================================================================
// HRMS PRO - BIOMETRIC FACE RECOGNITION, QUALITY GATE & 128-D EMBEDDING ENGINE
// Tối ưu hóa Camera Reuse, 8 FPS Detection / 60 FPS HUD, Zero-GC Pre-allocated Buffers,
// Căn chỉnh tâm khuôn mặt (Centroid Alignment) & Trích xuất Vector 128-D L2-Normalized
// ==============================================================================

window.FaceRecognitionHelper = (function () {
    const activeStreams = {};
    const animationFrames = {};
    const previousFrames = {};
    const currentGrayBuffers = {};
    const livenessHistories = {};
    const lastDetectionResults = {};
    const lastDetectionTimestamps = {};
    const lastVideoTimestamps = {};
    const inFlightEvaluations = {};
    const inFlightCaptures = {};
    const hudPausedFlags = {};
    const cachedFrameEmbeddings = {};

    // Chu kỳ xử lý phát hiện khuôn mặt & kiểm tra chất lượng: 8 FPS (125ms/frame)
    // Tách biệt hoàn toàn với vòng lặp vẽ giao diện HUD 60 FPS (requestAnimationFrame)
    const DETECTION_INTERVAL_MS = 125;

    // Pre-allocated Offscreen Canvases & TypedArrays (Singleton trong suốt vòng đời trang)
    let _livenessCanvas = null;
    let _livenessCtx = null;
    let _descriptorCanvas = null;
    let _descriptorCtx = null;
    let _photoCanvas = null;
    let _photoCtx = null;
    const _rawDescriptor128 = new Float32Array(128);
    const _centeredDescriptor128 = new Float32Array(128);

    let audioCtx = null;

    function ensureLivenessCanvas() {
        if (!_livenessCanvas) {
            _livenessCanvas = document.createElement('canvas');
            _livenessCanvas.width = 120;
            _livenessCanvas.height = 140;
            _livenessCtx = _livenessCanvas.getContext('2d', { willReadFrequently: true });
        }
        return _livenessCtx;
    }

    function ensureDescriptorCanvas() {
        if (!_descriptorCanvas) {
            _descriptorCanvas = document.createElement('canvas');
            _descriptorCanvas.width = 160;
            _descriptorCanvas.height = 160;
            _descriptorCtx = _descriptorCanvas.getContext('2d', { willReadFrequently: true });
        }
        return _descriptorCtx;
    }

    function ensurePhotoCanvas(width, height) {
        if (!_photoCanvas) {
            _photoCanvas = document.createElement('canvas');
            _photoCtx = _photoCanvas.getContext('2d');
        }
        if (_photoCanvas.width !== width || _photoCanvas.height !== height) {
            _photoCanvas.width = width;
            _photoCanvas.height = height;
        }
        return _photoCtx;
    }

    function getAudioContext() {
        if (!audioCtx) {
            const AudioContextClass = window.AudioContext || window.webkitAudioContext;
            if (AudioContextClass) {
                audioCtx = new AudioContextClass();
            }
        }
        if (audioCtx && audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        return audioCtx;
    }

    // Phát âm thanh phản hồi tổng hợp (Web Audio API Synthesizer)
    function playChime(type) {
        try {
            const ctx = getAudioContext();
            if (!ctx) return;

            const now = ctx.currentTime;
            if (type === 'success') {
                [523.25, 659.25, 783.99].forEach((freq, index) => {
                    const osc = ctx.createOscillator();
                    const gain = ctx.createGain();
                    osc.type = 'sine';
                    osc.frequency.value = freq;
                    gain.gain.setValueAtTime(0, now + index * 0.08);
                    gain.gain.linearRampToValueAtTime(0.18, now + index * 0.08 + 0.03);
                    gain.gain.exponentialRampToValueAtTime(0.001, now + index * 0.08 + 0.35);
                    osc.connect(gain);
                    gain.connect(ctx.destination);
                    osc.start(now + index * 0.08);
                    osc.stop(now + index * 0.08 + 0.36);
                });
            } else if (type === 'error') {
                [330, 260].forEach((freq, index) => {
                    const osc = ctx.createOscillator();
                    const gain = ctx.createGain();
                    osc.type = 'triangle';
                    osc.frequency.value = freq;
                    gain.gain.setValueAtTime(0.2, now + index * 0.14);
                    gain.gain.exponentialRampToValueAtTime(0.001, now + index * 0.14 + 0.2);
                    osc.connect(gain);
                    gain.connect(ctx.destination);
                    osc.start(now + index * 0.14);
                    osc.stop(now + index * 0.14 + 0.22);
                });
            } else if (type === 'scan') {
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.type = 'sine';
                osc.frequency.value = 880;
                gain.gain.setValueAtTime(0.08, now);
                gain.gain.exponentialRampToValueAtTime(0.001, now + 0.06);
                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.start(now);
                osc.stop(now + 0.07);
            }
        } catch (e) { }
    }

    // Dọn dẹp toàn bộ camera khi đóng trang hoặc chuyển hướng
    function stopAllCameras() {
        Object.keys(activeStreams).forEach(videoId => {
            stopCamera(videoId);
        });
    }

    window.addEventListener('beforeunload', stopAllCameras);
    window.addEventListener('pagehide', stopAllCameras);

    // Tạm dừng vòng lặp HUD nhưng giữ nguyên luồng phần cứng Webcam để tái sử dụng tức thì
    function pauseHud(videoId) {
        hudPausedFlags[videoId] = true;
        if (animationFrames[videoId]) {
            cancelAnimationFrame(animationFrames[videoId]);
            delete animationFrames[videoId];
        }
        return true;
    }

    // Tiếp tục vòng lặp HUD trên luồng Webcam đang mở sẵn
    function resumeHud(videoId, canvasId, dotNetHelper) {
        hudPausedFlags[videoId] = false;
        if (canvasId) {
            startHudLoop(videoId, canvasId, dotNetHelper);
        }
        return true;
    }

    // Tắt camera an toàn và giải phóng tài nguyên phần cứng khi đóng modal hoặc rời trang
    function stopCamera(videoId) {
        if (animationFrames[videoId]) {
            cancelAnimationFrame(animationFrames[videoId]);
            delete animationFrames[videoId];
        }

        if (activeStreams[videoId]) {
            try {
                activeStreams[videoId].getTracks().forEach(track => {
                    try { track.stop(); } catch (e) { }
                });
            } catch (e) { }
            delete activeStreams[videoId];
        }

        const video = document.getElementById(videoId);
        if (video) {
            video.srcObject = null;
        }

        delete previousFrames[videoId];
        delete currentGrayBuffers[videoId];
        delete livenessHistories[videoId];
        delete lastDetectionResults[videoId];
        delete lastDetectionTimestamps[videoId];
        delete lastVideoTimestamps[videoId];
        delete inFlightEvaluations[videoId];
        delete inFlightCaptures[videoId];
        delete hudPausedFlags[videoId];
        delete cachedFrameEmbeddings[videoId];
        return true;
    }

    function isStreamLive(stream) {
        if (!stream || typeof stream.getVideoTracks !== 'function') return false;
        const tracks = stream.getVideoTracks();
        return tracks.length > 0 && tracks[0].readyState === 'live';
    }

    // Khởi tạo camera một lần và tái sử dụng xuyên suốt phiên đăng ký
    async function startCamera(videoId, canvasId, dotNetHelper) {
        const initStart = performance.now();
        try {
            const video = document.getElementById(videoId);

            // TÁI SỬ DỤNG STREAM ĐANG HOẠT ĐỘNG (Tránh khởi tạo lại phần cứng webcam mỗi lần chuyển bước)
            const existingStream = activeStreams[videoId];
            if (video && existingStream && isStreamLive(existingStream)) {
                if (video.srcObject !== existingStream) {
                    video.srcObject = existingStream;
                }
                if (video.paused) {
                    try { await video.play(); } catch (e) { }
                }
                hudPausedFlags[videoId] = false;
                if (canvasId) {
                    startHudLoop(videoId, canvasId, dotNetHelper);
                }
                return {
                    success: true,
                    width: video.videoWidth || 640,
                    height: video.videoHeight || 480,
                    reusedStream: true,
                    cameraInitMs: Math.round((performance.now() - initStart) * 100) / 100
                };
            }

            // Dọn dẹp stream cũ nếu đã ngắt kết nối
            stopCamera(videoId);

            if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
                const isHttp = window.location.protocol === 'http:' && window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1';
                if (isHttp) {
                    return { success: false, error: "Trình duyệt chặn truy cập camera qua kết nối HTTP không bảo mật. Vui lòng truy cập qua HTTPS hoặc localhost." };
                }
                return { success: false, error: "Trình duyệt của bạn không hỗ trợ API truy cập Camera (navigator.mediaDevices.getUserMedia)." };
            }

            // Độ phân giải 640x480 tối ưu cho phát hiện khuôn mặt & trích xuất 160x160
            const constraints = {
                video: {
                    width: { ideal: 640 },
                    height: { ideal: 480 },
                    frameRate: { ideal: 30, max: 30 },
                    facingMode: "user"
                },
                audio: false
            };

            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            const targetVideo = document.getElementById(videoId);
            if (!targetVideo) {
                stream.getTracks().forEach(t => t.stop());
                return { success: false, error: `Không tìm thấy phần tử video #${videoId} trên giao diện.` };
            }

            targetVideo.srcObject = stream;
            activeStreams[videoId] = stream;
            livenessHistories[videoId] = [];
            hudPausedFlags[videoId] = false;

            await new Promise((resolve) => {
                if (targetVideo.readyState >= 1 && targetVideo.videoWidth > 0) {
                    targetVideo.play().then(resolve).catch(resolve);
                    return;
                }
                const onLoaded = () => {
                    targetVideo.removeEventListener('loadedmetadata', onLoaded);
                    targetVideo.play().then(resolve).catch(resolve);
                };
                targetVideo.addEventListener('loadedmetadata', onLoaded);
                setTimeout(() => {
                    targetVideo.removeEventListener('loadedmetadata', onLoaded);
                    targetVideo.play().then(resolve).catch(resolve);
                }, 2500);
            });

            if (canvasId) {
                startHudLoop(videoId, canvasId, dotNetHelper);
            }

            return {
                success: true,
                width: targetVideo.videoWidth || 640,
                height: targetVideo.videoHeight || 480,
                reusedStream: false,
                cameraInitMs: Math.round((performance.now() - initStart) * 100) / 100
            };
        } catch (err) {
            let msg = "Không thể mở camera. Vui lòng kiểm tra quyền cấp phép camera trên trình duyệt.";
            if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
                msg = "Bạn đã từ chối quyền truy cập camera. Vui lòng bấm vào biểu tượng ổ khóa hoặc cài đặt cạnh thanh địa chỉ để cấp quyền Camera cho trang web.";
            } else if (err.name === 'NotFoundError' || err.name === 'DevicesNotFoundError') {
                msg = "Không tìm thấy thiết bị camera nào được kết nối với máy tính này.";
            } else if (err.name === 'NotReadableError' || err.name === 'TrackStartError') {
                msg = "Camera đang bị một ứng dụng khác chiếm dụng (Zoom, Teams, Google Meet, Zalo...). Vui lòng tắt ứng dụng đó rồi thử lại.";
            } else if (err.name === 'OverconstrainedError') {
                msg = "Độ phân giải camera yêu cầu không được thiết bị hỗ trợ.";
            }
            return {
                success: false,
                error: msg,
                cameraInitMs: Math.round((performance.now() - initStart) * 100) / 100
            };
        }
    }

    // Phân tích frame ảnh tìm khuôn mặt, kiểm tra chất lượng đầu vào & tính sống (Zero-GC Allocation)
    function evaluateFrameLiveness(video, x, y, w, h, videoId, forceEvaluate = false) {
        const now = performance.now();

        // Nếu chưa tới chu kỳ 8 FPS (125ms) hoặc frame video chưa đổi, trả về kết quả cache gần nhất
        if (!forceEvaluate && lastDetectionResults[videoId]) {
            const elapsed = now - (lastDetectionTimestamps[videoId] || 0);
            const sameVideoTime = (video.currentTime === lastVideoTimestamps[videoId]);
            if (elapsed < DETECTION_INTERVAL_MS || sameVideoTime || inFlightEvaluations[videoId]) {
                return lastDetectionResults[videoId];
            }
        }

        inFlightEvaluations[videoId] = true;
        try {
            const offCtx = ensureLivenessCanvas();
            offCtx.drawImage(video, x, y, w, h, 0, 0, 120, 140);

            const imgData = offCtx.getImageData(0, 0, 120, 140);
            const data = imgData.data;

            const totalPixels = 120 * 140;
            let prev = previousFrames[videoId];
            let currentGray = currentGrayBuffers[videoId];
            if (!currentGray) {
                currentGray = new Uint8Array(totalPixels);
                currentGrayBuffers[videoId] = currentGray;
            }

            let totalBrightness = 0;
            let skinPixels = 0;
            let leftSkinPixels = 0;
            let rightSkinPixels = 0;
            let centerSkinPixels = 0;
            let skinSumX = 0;
            let skinSumY = 0;
            let motionDiff = 0;
            let edgeSharpnessSum = 0;

            for (let i = 0, pixelIdx = 0; i < data.length; i += 4, pixelIdx++) {
                const px = pixelIdx % 120;
                const py = (pixelIdx / 120) | 0;
                const r = data[i];
                const g = data[i + 1];
                const b = data[i + 2];
                const gray = (r * 299 + g * 587 + b * 114) / 1000;
                currentGray[pixelIdx] = gray;
                totalBrightness += gray;

                // Độ sắc nét cục bộ (Gradient ngang chống ảnh mờ/rung)
                if (px > 20 && px < 100 && py > 20 && py < 120 && pixelIdx > 0) {
                    edgeSharpnessSum += Math.abs(gray - currentGray[pixelIdx - 1]);
                }

                // Nhận diện vùng sắc tố da người trong không gian RGB
                const isSkin = (r > 60 && g > 40 && b > 20 && (r - g) > 10 && r > b);
                if (isSkin) {
                    skinPixels++;
                    skinSumX += px;
                    skinSumY += py;
                    if (px < 42) leftSkinPixels++;
                    if (px > 78) rightSkinPixels++;
                    if (px >= 24 && px <= 96 && py >= 20 && py <= 122) centerSkinPixels++;
                }

                if (prev) {
                    const diff = Math.abs(gray - prev[pixelIdx]);
                    if (diff > 12) {
                        motionDiff += diff;
                    }
                }
            }

            // Sao lưu sang buffer previousFrames mà không cấp phát mảng mới mỗi frame
            if (!prev) {
                previousFrames[videoId] = new Uint8Array(currentGray);
            } else {
                prev.set(currentGray);
            }

            const avgBrightness = totalBrightness / totalPixels;
            const skinRatio = skinPixels / totalPixels;
            const centerSkinRatio = centerSkinPixels / (72 * 102);
            const avgSharpness = edgeSharpnessSum / (80 * 100);

            // Tâm trọng lực khuôn mặt để căn chỉnh (Face Centroid Alignment)
            const centroidX = skinPixels > 0 ? (skinSumX / skinPixels) : 60;
            const centroidY = skinPixels > 0 ? (skinSumY / skinPixels) : 70;
            const centroidOffsetX = (centroidX - 60) / 120; // Tỷ lệ lệch ngang [-0.5, 0.5]
            const centroidOffsetY = (centroidY - 70) / 140; // Tỷ lệ lệch dọc [-0.5, 0.5]

            // Kiểm tra phát hiện nhiều khuôn mặt ở 2 rìa khung hình
            const isMultipleFaces = (leftSkinPixels / totalPixels > 0.15) && (rightSkinPixels / totalPixels > 0.15) && (skinRatio > 0.36);

            // Kiểm tra ánh sáng
            const isTooDark = avgBrightness < 36;
            const isTooBright = avgBrightness > 238;
            const lightingOk = !isTooDark && !isTooBright;

            // Kiểm tra sự hiện diện & vị trí trung tâm của khuôn mặt
            const hasFace = skinRatio > 0.14 && avgBrightness > 25 && avgBrightness < 245;
            const isPositionCentered = Math.abs(centroidOffsetX) <= 0.18 && Math.abs(centroidOffsetY) <= 0.18;
            const isFaceSizeValid = skinRatio >= 0.20 && centerSkinRatio >= 0.22;
            const isCentered = hasFace && !isMultipleFaces && isPositionCentered && isFaceSizeValid;

            // Tính toán chỉ số chuyển động tự nhiên & độ ổn định khung hình
            const rawMotion = motionDiff / (totalPixels * 255);
            let history = livenessHistories[videoId] || [];
            history.push(rawMotion);
            if (history.length > 12) history.shift();
            livenessHistories[videoId] = history;

            let motionVariance = 0;
            const avgMotion = history.reduce((a, b) => a + b, 0) / (history.length || 1);
            for (let m of history) {
                motionVariance += Math.pow(m - avgMotion, 2);
            }
            motionVariance = Math.sqrt(motionVariance / (history.length || 1));

            // Kiểm tra khuôn mặt có đang bị rung/nhòe chuyển động mạnh không
            const isStable = rawMotion < 0.085 && avgSharpness >= 2.2;

            // Điểm số tính sống (Liveness Score)
            let liveness = 0.50;
            if (hasFace) {
                liveness += 0.25;
                if (motionVariance > 0.0002 && motionVariance < 0.09) {
                    liveness += 0.18;
                }
                if (avgMotion > 0.0008 && avgMotion < 0.065) {
                    liveness += 0.10;
                }
            }
            liveness = Math.min(0.99, Math.max(0.10, liveness));

            // Thông báo trạng thái rõ ràng cho người dùng theo đúng ưu tiên kiểm tra chất lượng
            let statusText = "Đưa khuôn mặt vào giữa khung hình";
            let qualityCode = "OK";

            if (isMultipleFaces) {
                statusText = "Phát hiện nhiều khuôn mặt, vui lòng chỉ để một người trong khung hình";
                qualityCode = "MULTIPLE_FACES";
            } else if (isTooDark) {
                statusText = "Vui lòng tăng ánh sáng";
                qualityCode = "LOW_LIGHT";
            } else if (isTooBright) {
                statusText = "Ánh sáng quá chói, vui lòng tránh ngược sáng";
                qualityCode = "OVEREXPOSED";
            } else if (!hasFace) {
                statusText = "Không phát hiện được khuôn mặt";
                qualityCode = "NO_FACE";
            } else if (!isCentered) {
                statusText = "Đưa khuôn mặt vào giữa khung hình";
                qualityCode = "OFF_CENTER";
            } else if (!isStable) {
                statusText = "Giữ khuôn mặt ổn định";
                qualityCode = "UNSTABLE";
            } else {
                statusText = `✓ Khuôn mặt đạt chuẩn • Độ sống ${(liveness * 100).toFixed(0)}%`;
                qualityCode = "READY";
            }

            const result = {
                hasFace: hasFace,
                isCentered: isCentered,
                isMultipleFaces: isMultipleFaces,
                isStable: isStable,
                lightingOk: lightingOk,
                livenessScore: liveness,
                qualityOk: hasFace && !isMultipleFaces && lightingOk && isStable,
                qualityCode: qualityCode,
                statusText: statusText,
                centroidOffsetX: centroidOffsetX,
                centroidOffsetY: centroidOffsetY,
                avgBrightness: avgBrightness,
                sharpness: avgSharpness
            };

            lastDetectionResults[videoId] = result;
            lastDetectionTimestamps[videoId] = now;
            lastVideoTimestamps[videoId] = video.currentTime;
            return result;
        } finally {
            inFlightEvaluations[videoId] = false;
        }
    }

    // Vòng lặp vẽ HUD Canvas 60 FPS mượt mà, tách biệt với chu kỳ AI 8 FPS
    function startHudLoop(videoId, canvasId, dotNetHelper) {
        const video = document.getElementById(videoId);
        const canvas = document.getElementById(canvasId);
        if (!video || !canvas) return;

        if (animationFrames[videoId]) {
            cancelAnimationFrame(animationFrames[videoId]);
        }

        const ctx = canvas.getContext('2d');
        let scanLineY = 0;
        let scanDir = 1;
        let lastReportTime = 0;

        function renderFrame(timestamp) {
            if (hudPausedFlags[videoId]) {
                return;
            }

            if (!video.videoWidth || !video.videoHeight || video.paused || video.ended) {
                animationFrames[videoId] = requestAnimationFrame(renderFrame);
                return;
            }

            if (canvas.width !== video.videoWidth || canvas.height !== video.videoHeight) {
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
            }

            const w = canvas.width;
            const h = canvas.height;
            ctx.clearRect(0, 0, w, h);

            const boxW = Math.min(w * 0.54, 270);
            const boxH = Math.min(h * 0.70, 335);
            const boxX = (w - boxW) / 2;
            const boxY = (h - boxH) / 2;

            // 1. Lấy kết quả phát hiện khuôn mặt (Throttled ở 8 FPS, không gây nghẽn UI)
            const livenessData = evaluateFrameLiveness(video, boxX, boxY, boxW, boxH, videoId, false);

            // 2. Lớp phủ viền tinh tế
            ctx.fillStyle = 'rgba(15, 23, 42, 0.18)';
            ctx.fillRect(0, 0, w, boxY);
            ctx.fillRect(0, boxY + boxH, w, h - (boxY + boxH));
            ctx.fillRect(0, boxY, boxX, boxH);
            ctx.fillRect(boxX + boxW, boxY, w - (boxX + boxW), boxH);

            // 3. Khung định vị trạng thái trực quan
            const isReady = livenessData.isCentered && !livenessData.isMultipleFaces && livenessData.lightingOk && livenessData.isStable;
            let strokeColor = '#eab308';
            if (livenessData.isMultipleFaces || !livenessData.lightingOk) {
                strokeColor = '#ef4444';
            } else if (isReady) {
                strokeColor = '#10b981';
            } else if (livenessData.hasFace) {
                strokeColor = '#06b6d4';
            }

            ctx.save();
            ctx.strokeStyle = strokeColor;
            ctx.lineWidth = 3.0;
            ctx.lineCap = 'round';
            ctx.lineJoin = 'round';

            const cornerLen = 28;
            ctx.beginPath();
            ctx.moveTo(boxX, boxY + cornerLen);
            ctx.lineTo(boxX, boxY);
            ctx.lineTo(boxX + cornerLen, boxY);

            ctx.moveTo(boxX + boxW - cornerLen, boxY);
            ctx.lineTo(boxX + boxW, boxY);
            ctx.lineTo(boxX + boxW, boxY + cornerLen);

            ctx.moveTo(boxX, boxY + boxH - cornerLen);
            ctx.lineTo(boxX, boxY + boxH);
            ctx.lineTo(boxX + cornerLen, boxY + boxH);

            ctx.moveTo(boxX + boxW - cornerLen, boxY + boxH);
            ctx.lineTo(boxX + boxW, boxY + boxH);
            ctx.lineTo(boxX + boxW, boxY + boxH - cornerLen);
            ctx.stroke();

            ctx.lineWidth = 1.8;
            ctx.setLineDash([7, 6]);
            ctx.beginPath();
            ctx.ellipse(boxX + boxW / 2, boxY + boxH / 2, boxW * 0.45, boxH * 0.46, 0, 0, Math.PI * 2);
            ctx.stroke();
            ctx.setLineDash([]);
            ctx.restore();

            // 4. Đường quét sinh trắc học
            scanLineY += 3.0 * scanDir;
            if (scanLineY > boxH) {
                scanLineY = boxH;
                scanDir = -1;
            } else if (scanLineY < 0) {
                scanLineY = 0;
                scanDir = 1;
            }

            const currentScanY = boxY + scanLineY;
            const grad = ctx.createLinearGradient(boxX, currentScanY, boxX + boxW, currentScanY);
            grad.addColorStop(0, 'rgba(16, 185, 129, 0.0)');
            grad.addColorStop(0.5, isReady ? 'rgba(16, 185, 129, 0.70)' : (livenessData.isMultipleFaces ? 'rgba(239, 68, 68, 0.70)' : 'rgba(6, 182, 212, 0.70)'));
            grad.addColorStop(1, 'rgba(16, 185, 129, 0.0)');

            ctx.save();
            ctx.strokeStyle = grad;
            ctx.lineWidth = 2.0;
            ctx.beginPath();
            ctx.moveTo(boxX + 16, currentScanY);
            ctx.lineTo(boxX + boxW - 16, currentScanY);
            ctx.stroke();
            ctx.restore();

            // 5. Nhãn hướng dẫn trạng thái tiếng Việt trên HUD
            ctx.save();
            ctx.font = '600 13px "Inter", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';

            const statusText = livenessData.statusText;
            const textWidth = ctx.measureText(statusText).width;
            const pillW = textWidth + 30;
            const pillH = 30;
            const pillY = Math.max(10, boxY - 38);

            ctx.fillStyle = 'rgba(15, 23, 42, 0.90)';
            ctx.strokeStyle = strokeColor;
            ctx.lineWidth = 1.2;
            ctx.beginPath();
            ctx.roundRect(w / 2 - pillW / 2, pillY, pillW, pillH, 15);
            ctx.fill();
            ctx.stroke();

            ctx.fillStyle = '#ffffff';
            ctx.fillText(statusText, w / 2, pillY + pillH / 2);
            ctx.restore();

            // 6. Báo cáo trạng thái định kỳ (400ms) nếu component đăng ký callback
            if (dotNetHelper && timestamp - lastReportTime > 400) {
                lastReportTime = timestamp;
                try {
                    dotNetHelper.invokeMethodAsync('OnFaceDetectedCallback', {
                        hasFace: livenessData.hasFace,
                        isCentered: isReady,
                        isMultipleFaces: livenessData.isMultipleFaces,
                        livenessScore: livenessData.livenessScore,
                        qualityOk: livenessData.qualityOk,
                        statusText: livenessData.statusText
                    });
                } catch (e) { }
            }

            animationFrames[videoId] = requestAnimationFrame(renderFrame);
        }

        animationFrames[videoId] = requestAnimationFrame(renderFrame);
    }

    // Trích xuất vector đặc trưng sinh trắc học 128 chiều Zero-Centered và L2-Normalized
    // Bao gồm: Kiểm tra chất lượng ảnh đầu vào, Căn chỉnh khuôn mặt (Face Alignment), Tiền xử lý 160x160 & Đo thời gian từng bước
    async function captureFaceDescriptor(videoId, options) {
        const totalStart = performance.now();
        const opts = options || {};
        const mode = opts.mode || 'checkin'; // 'enroll' hoặc 'checkin'

        // Chống chạy đồng thời nhiều tác vụ trích xuất trên cùng một camera
        if (inFlightCaptures[videoId]) {
            return {
                success: false,
                error: "Hệ thống đang xử lý khung hình trước đó, vui lòng giữ yên trong giây lát."
            };
        }

        const video = document.getElementById(videoId);
        if (!video || !video.videoWidth || !video.videoHeight) {
            return {
                success: false,
                error: "Camera chưa sẵn sàng hoặc không đọc được khung hình."
            };
        }

        inFlightCaptures[videoId] = true;
        try {
            const w = video.videoWidth;
            const h = video.videoHeight;
            const baseBoxW = Math.min(w * 0.58, 290);
            const baseBoxH = Math.min(h * 0.72, 350);
            const baseBoxX = (w - baseBoxW) / 2;
            const baseBoxY = (h - baseBoxH) / 2;

            // BƯỚC 1: PHÁT HIỆN KHUÔN MẶT & KIỂM TRA CHẤT LƯỢNG ĐẦU VÀO (Face Detection & Quality Gate)
            const tDetectStart = performance.now();
            const qualityData = evaluateFrameLiveness(video, baseBoxX, baseBoxY, baseBoxW, baseBoxH, videoId, true);
            const faceDetectionMs = Math.round((performance.now() - tDetectStart) * 100) / 100;

            if (qualityData.isMultipleFaces) {
                return {
                    success: false,
                    error: "Phát hiện nhiều khuôn mặt, vui lòng chỉ để một người trong khung hình.",
                    faceDetectionMs: faceDetectionMs
                };
            }
            if (!qualityData.lightingOk) {
                return {
                    success: false,
                    error: qualityData.statusText,
                    faceDetectionMs: faceDetectionMs
                };
            }
            if (!qualityData.hasFace) {
                return {
                    success: false,
                    error: "Không phát hiện được khuôn mặt. Vui lòng nhìn thẳng vào giữa khung hình.",
                    faceDetectionMs: faceDetectionMs
                };
            }
            if (!qualityData.isStable) {
                return {
                    success: false,
                    error: "Giữ khuôn mặt ổn định trong khung hình để trích xuất đặc trưng chính xác.",
                    faceDetectionMs: faceDetectionMs
                };
            }

            // Nếu khung hình video chưa thay đổi và đã có kết quả hợp lệ vừa trích xuất (< 300ms), tái sử dụng ngay
            const cached = cachedFrameEmbeddings[videoId];
            if (cached && cached.videoTime === video.currentTime && (performance.now() - cached.timestamp) < 300) {
                return {
                    success: true,
                    descriptor: cached.descriptor,
                    livenessScore: qualityData.livenessScore,
                    qualityOk: true,
                    photoBase64: cached.photoBase64,
                    faceDetectionMs: faceDetectionMs,
                    faceAlignmentMs: cached.faceAlignmentMs,
                    preprocessingMs: cached.preprocessingMs,
                    embeddingInferenceMs: cached.embeddingInferenceMs,
                    extractDurationMs: Math.max(1, Math.round(performance.now() - totalStart))
                };
            }

            // BƯỚC 2: CĂN CHỈNH KHUÔN MẶT (Face Alignment theo tâm trọng lực sắc tố da)
            const tAlignStart = performance.now();
            const shiftX = (qualityData.centroidOffsetX || 0) * baseBoxW * 0.35;
            const shiftY = (qualityData.centroidOffsetY || 0) * baseBoxH * 0.35;
            const alignedBoxX = Math.max(0, Math.min(w - baseBoxW, baseBoxX + shiftX));
            const alignedBoxY = Math.max(0, Math.min(h - baseBoxH, baseBoxY + shiftY));
            const faceAlignmentMs = Math.round((performance.now() - tAlignStart) * 100) / 100;

            // BƯỚC 3: TIỀN XỬ LÝ ẢNH ĐẦU VÀO 160x160 (Image Preprocessing)
            const tPrepStart = performance.now();
            const ctx = ensureDescriptorCanvas();
            ctx.drawImage(video, alignedBoxX, alignedBoxY, baseBoxW, baseBoxH, 0, 0, 160, 160);
            const imgData = ctx.getImageData(0, 0, 160, 160);
            const data = imgData.data;
            const preprocessingMs = Math.round((performance.now() - tPrepStart) * 100) / 100;

            // BƯỚC 4: TRÍCH XUẤT VECTOR ĐẶC TRƯNG 128-D ZERO-CENTERED & L2-NORMALIZED (Embedding Inference)
            // Giữ nguyên 100% ý nghĩa toán học 4x4 ô x 8 moment không gian = 128 chiều
            const tInferStart = performance.now();
            const gridSize = 4;
            const cellW = 40; // 160 / 4
            const cellH = 40; // 160 / 4

            let descIdx = 0;
            let sumTotal = 0;

            for (let gy = 0; gy < gridSize; gy++) {
                const startY = gy * cellH;
                const endY = startY + cellH;
                for (let gx = 0; gx < gridSize; gx++) {
                    const startX = gx * cellW;
                    const endX = startX + cellW;

                    let rSum = 0, gSum = 0, bSum = 0;
                    let hEdgeSum = 0, vEdgeSum = 0;
                    const count = 1600; // 40 * 40

                    for (let y = startY; y < endY; y++) {
                        const rowOffset = y * 160;
                        const hasNextRow = y < 159;
                        for (let x = startX; x < endX; x++) {
                            const idx = (rowOffset + x) << 2;
                            const r = data[idx];
                            rSum += r;
                            gSum += data[idx + 1];
                            bSum += data[idx + 2];

                            if (x < 159) {
                                hEdgeSum += Math.abs(data[idx + 4] - r);
                            }
                            if (hasNextRow) {
                                vEdgeSum += Math.abs(data[idx + 640] - r);
                            }
                        }
                    }

                    const rMean = (rSum / count) / 255.0;
                    const gMean = (gSum / count) / 255.0;
                    const bMean = (bSum / count) / 255.0;
                    const hEdge = (hEdgeSum / count) / 128.0;
                    const vEdge = (vEdgeSum / count) / 128.0;
                    const edgeMag = Math.sqrt(hEdge * hEdge + vEdge * vEdge);
                    const rgDiff = (rMean - gMean);
                    const gbDiff = (gMean - bMean);

                    _rawDescriptor128[descIdx++] = rMean;
                    _rawDescriptor128[descIdx++] = gMean;
                    _rawDescriptor128[descIdx++] = bMean;
                    _rawDescriptor128[descIdx++] = hEdge;
                    _rawDescriptor128[descIdx++] = vEdge;
                    _rawDescriptor128[descIdx++] = edgeMag;
                    _rawDescriptor128[descIdx++] = rgDiff;
                    _rawDescriptor128[descIdx++] = gbDiff;

                    sumTotal += (rMean + gMean + bMean + hEdge + vEdge + edgeMag + rgDiff + gbDiff);
                }
            }

            // Zero-centering: Khử nhiễu độ sáng môi trường toàn cục
            const mean = sumTotal / 128.0;
            let normSq = 0;
            for (let i = 0; i < 128; i++) {
                const c = _rawDescriptor128[i] - mean;
                _centeredDescriptor128[i] = c;
                normSq += c * c;
            }

            // L2-Normalization: Chuẩn hóa về mặt cầu đơn vị 128 chiều (||V|| = 1.0)
            const norm = Math.sqrt(normSq) || 1.0;
            const normalized = new Array(128);
            for (let i = 0; i < 128; i++) {
                normalized[i] = Number((_centeredDescriptor128[i] / norm).toFixed(6));
            }
            const embeddingInferenceMs = Math.round((performance.now() - tInferStart) * 100) / 100;

            // BƯỚC 5: TẠO ẢNH XÁC MINH NÉN GỌN (Tối ưu băng thông SignalR)
            // Chế độ 'enroll' tái sử dụng trực tiếp _descriptorCanvas 160x160 đã vẽ ở Bước 3 (~2KB, 0 lần drawImage phụ)
            // Chế độ 'checkin' dùng 320x240 (~12KB) thay vì 640x480 (~80KB) để truyền qua SignalR nhanh gấp 6 lần
            let photoBase64 = null;
            try {
                if (mode === 'enroll') {
                    photoBase64 = _descriptorCanvas.toDataURL('image/jpeg', 0.65);
                } else {
                    const snapW = Math.min(w, 320);
                    const snapH = Math.min(h, 240);
                    const pCtx = ensurePhotoCanvas(snapW, snapH);
                    pCtx.drawImage(video, 0, 0, snapW, snapH);
                    photoBase64 = _photoCanvas.toDataURL('image/jpeg', 0.80);
                }
            } catch (e) { }

            playChime('scan');
            const totalDurationMs = Math.max(1, Math.round(performance.now() - totalStart));

            cachedFrameEmbeddings[videoId] = {
                videoTime: video.currentTime,
                timestamp: performance.now(),
                descriptor: normalized,
                photoBase64: photoBase64,
                faceAlignmentMs: faceAlignmentMs,
                preprocessingMs: preprocessingMs,
                embeddingInferenceMs: embeddingInferenceMs
            };

            return {
                success: true,
                descriptor: normalized,
                livenessScore: Number(qualityData.livenessScore.toFixed(2)),
                qualityOk: true,
                photoBase64: photoBase64,
                faceDetectionMs: faceDetectionMs,
                faceAlignmentMs: faceAlignmentMs,
                preprocessingMs: preprocessingMs,
                embeddingInferenceMs: embeddingInferenceMs,
                extractDurationMs: totalDurationMs
            };
        } finally {
            inFlightCaptures[videoId] = false;
        }
    }

    // Chụp ảnh tức thì từ camera (trả về Base64 JPEG)
    function capturePhotoBase64(videoId, quality = 0.80) {
        const video = document.getElementById(videoId);
        if (!video || !video.videoWidth) return null;
        try {
            const snapW = Math.min(video.videoWidth, 320);
            const snapH = Math.min(video.videoHeight, 240);
            const ctx = ensurePhotoCanvas(snapW, snapH);
            ctx.drawImage(video, 0, 0, snapW, snapH);
            return _photoCanvas.toDataURL('image/jpeg', quality);
        } catch (err) {
            return null;
        }
    }

    return {
        startCamera: startCamera,
        stopCamera: stopCamera,
        pauseHud: pauseHud,
        resumeHud: resumeHud,
        stopAllCameras: stopAllCameras,
        captureFaceDescriptor: captureFaceDescriptor,
        capturePhotoBase64: capturePhotoBase64,
        playChime: playChime
    };
})();
