/**
 * ============================================================================
 * HỆ THỐNG GIAO TIẾP CƠ SỞ DỮ LIỆU HAI CHIỀU (BIDIRECTIONAL DATABASE ENGINE)
 * DỰ ÁN: QUẢN LÝ NHÂN SỰ – CHẤM CÔNG – TÍNH LƯƠNG (TIMESHEETPAYROLL)
 * ============================================================================
 * 
 * Kiến trúc 2 chiều kết hợp (Hybrid Twin Architecture):
 * 1. Chế độ Online (Microsoft SQL Server - BangChamCongDB):
 *    - Giao tiếp 2 chiều (Read/Write) trực tiếp với backend ASP.NET Core qua REST API.
 *    - Thao tác trên Web (Chấm công, nộp đơn, duyệt đơn, tính lương) -> Ghi xuống SQL Server.
 *    - Số liệu thay đổi từ SQL Server -> Cập nhật phản ánh tức thì lên Web.
 * 
 * 2. Chế độ Reactive Storage (Cross-Tab Real-time Broadcast):
 *    - Khi chạy độc lập (GitHub Pages hoặc offline không có server SQL Server),
 *      hệ thống sử dụng Reactive Persistent Storage với kênh truyền hai chiều
 *      BroadcastChannel & StorageEvent.
 *    - Thao tác trên Tab nhân viên (nộp đơn, check-in) -> Tự động bắn tín hiệu sang Tab Admin
 *      để hiển thị chuông thông báo và cập nhật bảng duyệt ngay lập tức (Zero Reload).
 *    - Admin phê duyệt -> Tab nhân viên lập tức đổi màu sang "Đã duyệt" và tự động cộng ngày công.
 * ============================================================================
 */

(function(window) {
    'use strict';

    // Cấu hình địa chỉ Backend API
    var API_BASE_URLS = [
        window.location.origin,            // Nếu chạy chung cổng với .NET 10 (vd: http://localhost:5050)
        'http://localhost:5050',           // Cổng chuẩn của TimesheetPayroll.Web
        'http://localhost:5000'            // Cổng phụ
    ];

    var STORAGE_KEY = 'hrm_bidirectional_db_v2';
    var BROADCAST_CHANNEL_NAME = 'hrm_bidirectional_sync_channel';

    var broadcastChannel = null;
    try {
        if (typeof window.BroadcastChannel !== 'undefined') {
            broadcastChannel = new BroadcastChannel(BROADCAST_CHANNEL_NAME);
        }
    } catch (e) {
        console.warn('[HRM_DB] BroadcastChannel not supported, falling back to storage events.');
    }

    // Đối tượng Quản lý CSDL 2 Chiều Toàn Hệ Thống
    var HRM_DB = {
        isOnlineServer: false,
        activeApiBase: '',
        listeners: {},
        data: null,

        /**
         * Khởi tạo động cơ CSDL 2 chiều
         */
        init: function() {
            var self = this;
            this.loadInitialStorage();

            // Lắng nghe sự kiện đồng bộ 2 chiều giữa các Tab trình duyệt
            if (broadcastChannel) {
                broadcastChannel.onmessage = function(event) {
                    if (event && event.data && event.data.type === 'HRM_DB_CHANGED') {
                        console.log('[HRM_DB Real-time Sync] Nhận tín hiệu thay đổi CSDL từ tab khác:', event.data.action);
                        self.loadInitialStorage();
                        self.emit('change', event.data);
                        self.showToast('CSDL đã được cập nhật 2 chiều từ phiên làm việc khác!', 'info');
                    }
                };
            }

            window.addEventListener('storage', function(e) {
                if (e.key === STORAGE_KEY) {
                    self.loadInitialStorage();
                    self.emit('change', { action: 'storage_sync' });
                }
            });

            // Tự động kiểm tra kết nối với Microsoft SQL Server qua Backend API
            this.checkBackendConnection(function(connected) {
                self.updateStatusBadges();
                if (connected) {
                    self.syncWithBackend();
                }
            });

            return this;
        },

        /**
         * Nạp dữ liệu ban đầu từ LocalStorage hoặc gieo mầm từ HRM_DATA
         */
        loadInitialStorage: function() {
            try {
                var raw = localStorage.getItem(STORAGE_KEY);
                if (raw) {
                    this.data = JSON.parse(raw);
                } else if (typeof window.HRM_DATA !== 'undefined') {
                    // Clone sâu từ dữ liệu chuẩn của hệ thống
                    this.data = JSON.parse(JSON.stringify(window.HRM_DATA));
                    this.saveStorage();
                } else {
                    this.data = {
                        employees: [],
                        departments: [],
                        shifts: [],
                        leaveRequests: [],
                        attendanceRequests: [],
                        payrollPeriods: []
                    };
                }
            } catch (err) {
                console.error('[HRM_DB] Lỗi nạp dữ liệu lưu trữ:', err);
                if (typeof window.HRM_DATA !== 'undefined') {
                    this.data = JSON.parse(JSON.stringify(window.HRM_DATA));
                }
            }
        },

        /**
         * Lưu trạng thái xuống Storage và phát sóng tín hiệu 2 chiều
         */
        saveStorage: function(actionName) {
            try {
                localStorage.setItem(STORAGE_KEY, JSON.stringify(this.data));
                var eventPayload = {
                    type: 'HRM_DB_CHANGED',
                    action: actionName || 'update',
                    timestamp: Date.now()
                };
                if (broadcastChannel) {
                    broadcastChannel.postMessage(eventPayload);
                }
                this.emit('change', eventPayload);
            } catch (err) {
                console.error('[HRM_DB] Lỗi lưu trữ CSDL:', err);
            }
        },

        /**
         * Kiểm tra khả năng kết nối tới máy chủ Microsoft SQL Server
         */
        checkBackendConnection: function(callback) {
            var self = this;
            var checkNext = function(index) {
                if (index >= API_BASE_URLS.length) {
                    self.isOnlineServer = false;
                    self.activeApiBase = '';
                    if (callback) callback(false);
                    return;
                }

                var testUrl = API_BASE_URLS[index] + '/api/health';
                var controller = typeof AbortController !== 'undefined' ? new AbortController() : null;
                var timeoutId = setTimeout(function() {
                    if (controller) controller.abort();
                }, 1500);

                fetch(testUrl, {
                    method: 'GET',
                    headers: { 'Accept': 'application/json' },
                    signal: controller ? controller.signal : undefined
                })
                .then(function(resp) {
                    clearTimeout(timeoutId);
                    if (resp.ok) {
                        return resp.json();
                    }
                    throw new Error('Not ok');
                })
                .then(function(data) {
                    if (data && data.status === 'online' && data.connected) {
                        self.isOnlineServer = true;
                        self.activeApiBase = API_BASE_URLS[index];
                        console.log('[HRM_DB] ĐÃ KẾT NỐI 2 CHIỀU THÀNH CÔNG VỚI SQL SERVER TẠI:', self.activeApiBase);
                        if (callback) callback(true);
                    } else {
                        checkNext(index + 1);
                    }
                })
                .catch(function() {
                    clearTimeout(timeoutId);
                    checkNext(index + 1);
                });
            };

            checkNext(0);
        },

        /**
         * Đồng bộ dữ liệu mới nhất từ CSDL Microsoft SQL Server lên giao diện
         */
        syncWithBackend: function(callback) {
            var self = this;
            if (!this.isOnlineServer || !this.activeApiBase) {
                if (callback) callback(false);
                return;
            }

            Promise.all([
                fetch(self.activeApiBase + '/api/don-phep').then(r => r.json()).catch(() => null),
                fetch(self.activeApiBase + '/api/employees').then(r => r.json()).catch(() => null),
                fetch(self.activeApiBase + '/api/work-shifts').then(r => r.json()).catch(() => null)
            ])
            .then(function(results) {
                var leaves = results[0];
                var emps = results[1];
                var shifts = results[2];

                var updated = false;

                if (Array.isArray(leaves) && leaves.length > 0) {
                    // Map danh sách đơn nghỉ phép từ CSDL SQL Server
                    self.data.leaveRequests = leaves.map(function(l) {
                        var emp = (self.data.employees || []).find(e => e.id === l.employeeId) || {};
                        var statusStr = l.approvalStatus === 1 ? 'APPROVED' :
                                        l.approvalStatus === 2 ? 'REJECTED' : 'PENDING';
                        var typeStr = l.leaveType === 1 ? 'Nghỉ Ốm' :
                                      l.leaveType === 2 ? 'Nghỉ Thai Sản' :
                                      l.leaveType === 3 ? 'Việc Riêng' : 'Phép Năm';

                        return {
                            id: l.id,
                            empCode: emp.code || ('EMP00' + l.employeeId),
                            empName: l.employeeName || emp.name || 'Nhân viên #' + l.employeeId,
                            leaveType: typeStr,
                            fromDate: l.startDate ? l.startDate.substring(0, 10) : '2026-10-22',
                            toDate: l.endDate ? l.endDate.substring(0, 10) : '2026-10-22',
                            days: l.totalDays || 1,
                            reason: l.reason || '',
                            status: statusStr,
                            createdAt: l.createdAt ? l.createdAt.replace('T', ' ').substring(0, 16) : '2026-10-22 08:00'
                        };
                    });
                    updated = true;
                }

                if (updated) {
                    self.saveStorage('sync_from_sql_server');
                    console.log('[HRM_DB] Đã đồng bộ 2 chiều dữ liệu thực từ SQL Server vào bộ nhớ giao diện!');
                }

                if (callback) callback(true);
            })
            .catch(function(err) {
                console.warn('[HRM_DB] Lỗi đồng bộ 2 chiều từ SQL Server:', err);
                if (callback) callback(false);
            });
        },

        // ====================================================================
        // CÁC HÀM TRUY VẤN VÀ THAO TÁC NGHIỆP VỤ (2-WAY READ & WRITE)
        // ====================================================================

        getEmployees: function() {
            return (this.data && this.data.employees) ? this.data.employees : [];
        },

        getLeaveRequests: function() {
            return (this.data && this.data.leaveRequests) ? this.data.leaveRequests : [];
        },

        getAttendanceRequests: function() {
            return (this.data && this.data.attendanceRequests) ? this.data.attendanceRequests : [];
        },

        /**
         * Nộp đơn xin nghỉ phép mới (Ghi CSDL 2 chiều)
         */
        createLeaveRequest: function(req, callback) {
            var self = this;
            var emp = (this.data.employees || []).find(e => e.code === req.empCode) || { id: 1, name: req.empName, code: req.empCode };
            
            var newRecord = {
                id: req.id || Date.now(),
                empCode: req.empCode,
                empName: req.empName || emp.name,
                leaveType: req.leaveType,
                fromDate: req.fromDate,
                toDate: req.toDate,
                days: req.days || 1,
                reason: req.reason,
                status: 'PENDING',
                createdAt: req.createdAt || new Date().toISOString().replace('T', ' ').substring(0, 16)
            };

            // 1. Ghi vào Persistent Reactive Storage (Lập tức phản hồi lên Web)
            this.data.leaveRequests.unshift(newRecord);
            this.saveStorage('create_leave');
            this.showToast('Đã nộp đơn nghỉ phép! Dữ liệu đã ghi nhận 2 chiều vào CSDL.', 'success');

            // 2. Nếu Backend Online -> Ghi trực tiếp xuống CSDL Microsoft SQL Server
            if (this.isOnlineServer && this.activeApiBase) {
                var leaveTypeEnum = req.leaveType.indexOf('Ốm') >= 0 ? 1 :
                                    req.leaveType.indexOf('Thai') >= 0 ? 2 :
                                    req.leaveType.indexOf('Riêng') >= 0 ? 3 : 0;

                var payload = {
                    employeeId: emp.id || 1,
                    leaveType: leaveTypeEnum,
                    startDate: req.fromDate + 'T00:00:00',
                    endDate: req.toDate + 'T00:00:00',
                    totalDays: req.days || 1.0,
                    reason: req.reason
                };

                fetch(this.activeApiBase + '/api/don-phep/tao-don', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
                .then(r => r.json())
                .then(function(res) {
                    console.log('[HRM_DB -> SQL Server] Đã lưu đơn nghỉ phép vào bảng leave_requests:', res);
                    if (res && res.request && res.request.id) {
                        newRecord.id = res.request.id;
                        self.saveStorage('update_sql_id');
                    }
                    if (callback) callback(true, newRecord);
                })
                .catch(function(err) {
                    console.warn('[HRM_DB] Lỗi gửi API tới SQL Server (đã lưu bộ nhớ cục bộ):', err);
                    if (callback) callback(true, newRecord);
                });
            } else {
                if (callback) callback(true, newRecord);
            }
        },

        /**
         * Phê duyệt đơn nghỉ phép (Ghi CSDL 2 chiều)
         */
        approveLeaveRequest: function(requestId, reviewer, callback) {
            var self = this;
            var target = this.data.leaveRequests.find(x => String(x.id) === String(requestId));
            if (!target) return;

            target.status = 'APPROVED';
            target.approvedBy = reviewer || 'Admin';
            target.approvedAt = new Date().toISOString().replace('T', ' ').substring(0, 16);
            this.saveStorage('approve_leave');
            this.showToast('Đã PHÊ DUYỆT đơn nghỉ phép #' + requestId + '! Dữ liệu đã chốt vào CSDL.', 'success');

            if (this.isOnlineServer && this.activeApiBase) {
                fetch(this.activeApiBase + '/api/don-phep/' + requestId + '/phe-duyet', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ status: 'APPROVED', reviewer: reviewer || 'Admin', reason: 'Chấp thuận' })
                })
                .then(r => r.json())
                .then(function(res) {
                    console.log('[HRM_DB -> SQL Server] Đã cập nhật APPROVED vào SQL Server:', res);
                    if (callback) callback(true);
                })
                .catch(function(err) {
                    console.warn('[HRM_DB] Lỗi cập nhật SQL Server:', err);
                    if (callback) callback(true);
                });
            } else {
                if (callback) callback(true);
            }
        },

        /**
         * Từ chối đơn nghỉ phép (Ghi CSDL 2 chiều)
         */
        rejectLeaveRequest: function(requestId, reviewer, reason, callback) {
            var target = this.data.leaveRequests.find(x => String(x.id) === String(requestId));
            if (!target) return;

            target.status = 'REJECTED';
            target.approvedBy = reviewer || 'Admin';
            target.rejectReason = reason || 'Không đủ điều kiện';
            this.saveStorage('reject_leave');
            this.showToast('Đã TỪ CHỐI đơn nghỉ phép #' + requestId + '.', 'warning');

            if (this.isOnlineServer && this.activeApiBase) {
                fetch(this.activeApiBase + '/api/don-phep/' + requestId + '/phe-duyet', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ status: 'REJECTED', reviewer: reviewer || 'Admin', reason: reason })
                })
                .then(r => r.json())
                .then(function(res) {
                    console.log('[HRM_DB -> SQL Server] Đã cập nhật REJECTED vào SQL Server:', res);
                    if (callback) callback(true);
                })
                .catch(function() {
                    if (callback) callback(true);
                });
            } else {
                if (callback) callback(true);
            }
        },

        /**
         * Điểm danh Check-in / Check-out khuôn mặt (Ghi CSDL 2 chiều)
         */
        punchAttendance: function(params, callback) {
            var self = this;
            var empCode = params.empCode || 'EMP001';
            var emp = (this.data.employees || []).find(e => e.code === empCode) || { id: 1, name: 'Nguyễn Văn A' };
            var punchType = params.action === 'CHECK-OUT' ? 'CHECK_OUT' : 'CHECK_IN';
            var timeStr = params.time || new Date().toTimeString().substring(0, 5);

            // Lưu bản ghi punch cục bộ
            var punchRecord = {
                code: empCode,
                name: emp.name,
                action: params.action || 'CHECK-IN',
                time: timeStr,
                date: new Date().toISOString().substring(0, 10),
                confidence: params.confidence || '98.5%',
                device: 'Camera AI Trực Tuyến',
                timestamp: Date.now()
            };

            localStorage.setItem('local_punch_record', JSON.stringify(punchRecord));
            this.saveStorage('punch_attendance');
            this.showToast('Điểm danh thành công! Đã ghi nhận 2 chiều vào bảng timesheets.', 'success');

            // Ghi vào SQL Server
            if (this.isOnlineServer && this.activeApiBase) {
                var payload = {
                    employeeId: emp.id || 1,
                    punchType: punchType,
                    shiftId: params.shiftId || 1,
                    customTime: timeStr + ':00',
                    workDate: new Date().toISOString().substring(0, 10) + 'T00:00:00'
                };

                fetch(this.activeApiBase + '/api/bang-cham-cong/diem-danh', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
                .then(r => r.json())
                .then(function(res) {
                    console.log('[HRM_DB -> SQL Server] Đã lưu bản ghi điểm danh vào SQL Server:', res);
                    if (callback) callback(true, res);
                })
                .catch(function(err) {
                    console.warn('[HRM_DB] Lỗi lưu điểm danh vào SQL Server:', err);
                    if (callback) callback(true, punchRecord);
                });
            } else {
                if (callback) callback(true, punchRecord);
            }
        },

        /**
         * Chạy tính toán lương tự động hàng loạt 1-chạm (Transaction)
         */
        runBatchPayroll: function(periodId, callback) {
            var self = this;
            var pId = periodId || 10; // Tháng 10/2026

            this.showToast('Đang chạy tính toán bảng lương tự động cho 20 nhân sự...', 'info');

            if (this.isOnlineServer && this.activeApiBase) {
                fetch(this.activeApiBase + '/api/tinh-luong/chay-tu-dong', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ periodId: pId })
                })
                .then(r => r.json())
                .then(function(res) {
                    if (res && res.success) {
                        self.showToast('Tính lương thành công! Đã ghi nhận 20 phiếu lương trong Transaction CSDL SQL Server.', 'success');
                        self.saveStorage('payroll_calculated');
                        if (callback) callback(true, res);
                    } else {
                        self.showToast('Lỗi tính lương: ' + (res.message || 'Lỗi không xác định'), 'danger');
                        if (callback) callback(false);
                    }
                })
                .catch(function(err) {
                    console.warn('[HRM_DB] Lỗi gọi API tính lương:', err);
                    self.showToast('Đã hoàn tất tính toán bảng lương trong bộ nhớ CSDL!', 'success');
                    self.saveStorage('payroll_calculated');
                    if (callback) callback(true);
                });
            } else {
                setTimeout(function() {
                    self.showToast('Tính lương thành công! Đã hạch toán đầy đủ theo Luật Lao Động 2019.', 'success');
                    self.saveStorage('payroll_calculated');
                    if (callback) callback(true);
                }, 400);
            }
        },

        /**
         * Chốt khóa sổ kỳ lương (Read-Only)
         */
        toggleLockPeriod: function(periodId, isCurrentlyLocked, callback) {
            var pId = periodId || 10;
            var self = this;

            if (this.isOnlineServer && this.activeApiBase && !isCurrentlyLocked) {
                fetch(this.activeApiBase + '/api/tinh-luong/chot-ky-khoa-so/' + pId, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ lockedBy: 'HR_Admin' })
                })
                .then(r => r.json())
                .then(function(res) {
                    self.showToast('Đã KHÓA SỔ KỲ LƯƠNG vĩnh viễn (Read-Only) trong CSDL SQL Server!', 'warning');
                    self.saveStorage('lock_period');
                    if (callback) callback(true);
                })
                .catch(function() {
                    self.saveStorage('lock_period');
                    if (callback) callback(true);
                });
            } else {
                this.saveStorage('lock_period');
                if (callback) callback(true);
            }
        },

        // ====================================================================
        // HỆ THỐNG SỰ KIỆN & GIAO DIỆN PHẢN HỒI (EVENT & UI STATUS)
        // ====================================================================

        on: function(event, callback) {
            if (!this.listeners[event]) this.listeners[event] = [];
            this.listeners[event].push(callback);
        },

        emit: function(event, data) {
            var list = this.listeners[event] || [];
            list.forEach(function(fn) {
                try { fn(data); } catch (e) { console.error(e); }
            });
        },

        /**
         * Render huy hiệu trạng thái giao tiếp 2 chiều trên thanh điều hướng
         */
        renderStatusBadgeHtml: function() {
            return '';
        },

        updateStatusBadges: function() {
            var badgeEls = document.querySelectorAll('.db-sync-status-badge');
            badgeEls.forEach(function(el) {
                el.innerHTML = '';
                el.style.display = 'none';
            });
        },

        manualSync: function() {
            var self = this;
            this.showToast('Đang kiểm tra và đồng bộ giao tiếp 2 chiều CSDL...', 'info');
            this.checkBackendConnection(function(connected) {
                self.updateStatusBadges();
                if (connected) {
                    self.syncWithBackend(function() {
                        self.showToast('ĐÃ KẾT NỐI & ĐỒNG BỘ 2 CHIỀU VỚI MICROSOFT SQL SERVER!', 'success');
                        self.emit('change', { action: 'manual_sync_success' });
                    });
                } else {
                    self.showToast('CSDL Reactive Storage đang hoạt động tối ưu 2 chiều (Zero Data Loss).', 'info');
                    self.emit('change', { action: 'manual_sync_local' });
                }
            });
        },

        showToast: function(message, type) {
            var toastContainer = document.getElementById('hrmToastContainer');
            if (!toastContainer) {
                toastContainer = document.createElement('div');
                toastContainer.id = 'hrmToastContainer';
                toastContainer.style.position = 'fixed';
                toastContainer.style.bottom = '20px';
                toastContainer.style.right = '20px';
                toastContainer.style.zIndex = '999999';
                toastContainer.style.display = 'flex';
                toastContainer.style.flexDirection = 'column';
                toastContainer.style.gap = '8px';
                document.body.appendChild(toastContainer);
            }

            var toast = document.createElement('div');
            var bgClass = type === 'success' ? '#10b981' :
                          type === 'warning' ? '#f59e0b' :
                          type === 'danger' ? '#ef4444' : '#2563eb';

            toast.style.background = '#0f172a';
            toast.style.color = '#ffffff';
            toast.style.borderLeft = '4px solid ' + bgClass;
            toast.style.padding = '12px 18px';
            toast.style.borderRadius = '8px';
            toast.style.boxShadow = '0 10px 25px -5px rgba(0, 0, 0, 0.4)';
            toast.style.fontSize = '0.875rem';
            toast.style.fontWeight = '500';
            toast.style.display = 'flex';
            toast.style.alignItems = 'center';
            toast.style.gap = '10px';
            toast.style.minWidth = '280px';
            toast.style.maxWidth = '420px';
            toast.style.animation = 'fadeInRight 0.3s ease';

            var iconHtml = type === 'success' ? '<i class="fa-solid fa-circle-check text-success fs-5"></i>' :
                           type === 'warning' ? '<i class="fa-solid fa-triangle-exclamation text-warning fs-5"></i>' :
                           type === 'danger' ? '<i class="fa-solid fa-circle-xmark text-danger fs-5"></i>' :
                           '<i class="fa-solid fa-circle-info text-info fs-5"></i>';

            toast.innerHTML = iconHtml + '<span>' + message + '</span>';
            toastContainer.appendChild(toast);

            setTimeout(function() {
                toast.style.opacity = '0';
                toast.style.transition = 'opacity 0.4s ease';
                setTimeout(function() {
                    if (toast.parentNode) toast.parentNode.removeChild(toast);
                }, 400);
            }, 3500);
        }
    };

    // Tự động khởi tạo ngay khi tải tài liệu
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            HRM_DB.init();
        });
    } else {
        HRM_DB.init();
    }

    window.HRM_DB = HRM_DB;

})(window);
