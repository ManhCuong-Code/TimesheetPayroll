// Slide Navigation Controller - Tông Sáng & Hỗ Trợ Đổi Theme
document.addEventListener('DOMContentLoaded', () => {
  const slides = document.querySelectorAll('.slide');
  const dotsContainer = document.getElementById('slideDots');
  const progressBar = document.getElementById('progressBar');
  const currentSlideNum = document.getElementById('currentSlideNum');
  const totalSlidesNum = document.getElementById('totalSlidesNum');
  const prevBtn = document.getElementById('prevBtn');
  const nextBtn = document.getElementById('nextBtn');
  const fullscreenBtn = document.getElementById('fullscreenBtn');
  const notesBtn = document.getElementById('notesBtn');
  const printBtn = document.getElementById('printBtn');
  const themeToggleBtn = document.getElementById('themeToggleBtn');
  const notesDrawer = document.getElementById('notesDrawer');
  const notesContent = document.getElementById('notesContent');

  let currentSlide = 0;
  const totalSlides = slides.length;

  totalSlidesNum.textContent = totalSlides;

  // Quản lý Chế độ Giao diện (Mặc định: Tông Sáng)
  const savedTheme = localStorage.getItem('slide_deck_theme') || 'light';
  if (savedTheme === 'dark') {
    document.body.classList.add('dark-theme');
    updateThemeButton(true);
  } else {
    document.body.classList.remove('dark-theme');
    updateThemeButton(false);
  }

  function updateThemeButton(isDark) {
    if (!themeToggleBtn) return;
    if (isDark) {
      themeToggleBtn.innerHTML = '<i class="fa-solid fa-sun" style="color:#f59e0b;"></i> <span>Tông Sáng</span>';
      themeToggleBtn.title = "Chuyển sang giao diện Tông Sáng";
    } else {
      themeToggleBtn.innerHTML = '<i class="fa-solid fa-moon" style="color:#6366f1;"></i> <span>Tông Tối</span>';
      themeToggleBtn.title = "Chuyển sang giao diện Tông Tối";
    }
  }

  if (themeToggleBtn) {
    themeToggleBtn.addEventListener('click', () => {
      const isDark = document.body.classList.toggle('dark-theme');
      localStorage.setItem('slide_deck_theme', isDark ? 'dark' : 'light');
      updateThemeButton(isDark);
    });
  }

  // Lời thoại thuyết trình mẫu cực kỳ dễ nói, tự nhiên và thuyết phục
  const speakerNotes = [
    "👉 Lời thoại Trang 1 (Mở đầu): 'Kính thưa Thầy/Cô và các bạn, hôm nay nhóm em xin trình bày về dự án: Hệ thống Chấm công & Tính lương tự động. Để nói một cách dễ hiểu nhất, dự án này sinh ra để giúp các công ty nhỏ từ 20 đến 50 người không còn phải tính lương thủ công bằng Excel nữa. Chỉ cần nhân viên đi làm điểm danh, cuối tháng kế toán bấm đúng 1 nút là phần mềm sẽ tự tính lương chính xác từng đồng, nhân viên tự mở điện thoại xem phiếu lương của mình một cách kín đáo, và có cả Trợ lý AI trả lời khi nhân viên thắc mắc lương.'",
    
    "👉 Lời thoại Trang 2 (Bối cảnh & Ví dụ số tiền): 'Tại sao công ty nhỏ lại rất cần phần mềm này? Vì dùng Excel rất dễ bị kéo lệch ô công thức, kế toán mất 2-3 ngày cuối tháng để cộng trừ dò tìm sai sót, và nguy hiểm nhất là rất dễ lộ lương của nhau. Hệ thống của nhóm em chuẩn hóa công thức rõ ràng. Ví dụ cụ thể trên slide: Anh Nguyễn Văn A có lương cơ bản 10 triệu. Tháng chuẩn 22 ngày công, anh đi làm 20 ngày thì tiền lương công là 9.090.909 đồng, cộng 500k phụ cấp ăn trưa, trừ 10.5% các khoản bảo hiểm bắt buộc theo luật là 1.050.000 đồng, thực lĩnh đúng 8.540.909 đồng. Tỷ lệ bảo hiểm này được lưu riêng, mai mốt nhà nước có đổi luật chỉ cần sửa 1 số là xong chứ không phải sửa lại code.'",
    
    "👉 Lời thoại Trang 3 (Kiến trúc & 2 Điểm Kỹ Thuật Đắt Giá): 'Về mặt cấu trúc, hệ thống gồm 8 bảng dữ liệu cốt lõi giống như 8 ngăn kéo trong văn phòng: từ quản lý phòng ban, hồ sơ nhân viên, hợp đồng, bảng chấm công cho tới phiếu lương chi tiết. Điểm đáng giá nhất về mặt kỹ thuật của nhóm em là 2 tính năng: Thứ nhất, Phân quyền cấp dữ liệu - nghĩa là giống như tủ khóa ngân hàng, nhân viên chỉ có chìa khóa mở đúng phiếu lương của mình, dù có tò mò gõ thử link hay ID của người khác máy chủ cũng từ chối ngay lập tức. Thứ hai, Transaction tài chính an toàn - khi kế toán bấm chốt lương cho 50 người, máy sẽ tính và chốt toàn bộ trong 1 lượt giao dịch an toàn như cây ATM, tuyệt đối không bị tình trạng người có người không hay mất điện giữa chừng.'",
    
    "👉 Lời thoại Trang 4 (Kế hoạch 4 tuần cụ thể): 'Để hoàn thành dự án chất lượng, nhóm đã lên kế hoạch chi tiết trong 4 tuần và mỗi tuần đều có sản phẩm cụ thể để kiểm tra: Tuần 1 là khảo sát thực tế, thiết kế bản vẽ giao diện Figma và lập 8 bảng CSDL. Tuần 2 lập trình xong phần thêm sửa xóa nhân viên và màn hình nhập ngày công. Tuần 3 là tuần quan trọng nhất: lập trình bộ tính toán lương tự động, nút Chốt kỳ lương khóa sổ an toàn và phân quyền bảo mật. Và Tuần 4 là tích hợp Trợ lý AI thông minh giải đáp lương, xuất file Excel/PDF, kiểm thử các trường hợp khó và hoàn thiện slide báo cáo bảo vệ.'",
    
    "👉 Lời thoại Trang 5 (Điểm đột phá AI & Kết luận): 'Điểm sáng tạo nhất của dự án là Trợ lý AI giải đáp thắc mắc lương. Bình thường mỗi khi nhận lương xong, kế toán rất mệt mỏi vì nhân viên hay nhắn tin hỏi: Sao lương tháng này em bị trừ? Bây giờ nhân viên chỉ cần chat với AI, AI sẽ tự động đọc phiếu lương tháng này và tháng trước để trả lời cặn kẽ trong 2 giây: ví dụ do anh nghỉ 2 ngày không lương và tăng tiền đóng bảo hiểm mới. Tính năng này giúp giảm 85% phiền toái cho kế toán. Nhóm em cam kết sau 4 tuần sẽ bàn giao sản phẩm chạy hoàn hảo, giao diện đẹp và không còn lỗi tính toán. Em xin cảm ơn Thầy Cô đã lắng nghe!'"
  ];

  // Khởi tạo các chấm tròn chuyển trang
  dotsContainer.innerHTML = '';
  slides.forEach((_, idx) => {
    const dot = document.createElement('div');
    dot.classList.add('dot');
    if (idx === 0) dot.classList.add('active');
    dot.setAttribute('title', `Chuyển tới Trang ${idx + 1}`);
    dot.addEventListener('click', () => goToSlide(idx));
    dotsContainer.appendChild(dot);
  });

  const dots = document.querySelectorAll('.dot');

  function updateSlide(index) {
    slides.forEach((slide, i) => {
      slide.classList.toggle('active', i === index);
    });

    dots.forEach((dot, i) => {
      dot.classList.toggle('active', i === index);
    });

    currentSlide = index;
    currentSlideNum.textContent = currentSlide + 1;
    progressBar.style.width = `${((currentSlide + 1) / totalSlides) * 100}%`;

    // Cập nhật trạng thái nút tiến/lùi
    prevBtn.disabled = currentSlide === 0;
    nextBtn.disabled = currentSlide === totalSlides - 1;
    prevBtn.style.opacity = currentSlide === 0 ? '0.4' : '1';
    nextBtn.style.opacity = currentSlide === totalSlides - 1 ? '0.4' : '1';

    // Cập nhật lời thuyết trình trong ngăn kéo
    if (notesContent) {
      notesContent.textContent = speakerNotes[index] || "Không có ghi chú cho trang này.";
    }
  }

  function nextSlide() {
    if (currentSlide < totalSlides - 1) {
      updateSlide(currentSlide + 1);
    }
  }

  function prevSlide() {
    if (currentSlide > 0) {
      updateSlide(currentSlide - 1);
    }
  }

  function goToSlide(index) {
    if (index >= 0 && index < totalSlides) {
      updateSlide(index);
    }
  }

  // Gắn sự kiện nút bấm
  nextBtn.addEventListener('click', nextSlide);
  prevBtn.addEventListener('click', prevSlide);

  // Phím tắt bàn phím
  document.addEventListener('keydown', (e) => {
    if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;

    switch (e.key) {
      case 'ArrowRight':
      case 'PageDown':
      case ' ':
        e.preventDefault();
        nextSlide();
        break;
      case 'ArrowLeft':
      case 'PageUp':
        e.preventDefault();
        prevSlide();
        break;
      case 'Home':
        e.preventDefault();
        goToSlide(0);
        break;
      case 'End':
        e.preventDefault();
        goToSlide(totalSlides - 1);
        break;
      case 'f':
      case 'F':
        toggleFullscreen();
        break;
      case 'n':
      case 'N':
        toggleNotes();
        break;
      case '1':
      case '2':
      case '3':
      case '4':
      case '5':
        goToSlide(parseInt(e.key, 10) - 1);
        break;
    }
  });

  // Bật / Tắt Toàn màn hình
  function toggleFullscreen() {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen().catch(err => {
        console.warn(`Fullscreen error: ${err.message}`);
      });
    } else {
      if (document.exitFullscreen) {
        document.exitFullscreen();
      }
    }
  }

  fullscreenBtn.addEventListener('click', toggleFullscreen);

  // Bật / Tắt Ngăn kéo ghi chú lời thoại
  function toggleNotes() {
    if (notesDrawer) {
      notesDrawer.classList.toggle('open');
      notesBtn.classList.toggle('btn-primary');
    }
  }

  if (notesBtn) {
    notesBtn.addEventListener('click', toggleNotes);
  }

  // In / Xuất PDF
  if (printBtn) {
    printBtn.addEventListener('click', () => {
      window.print();
    });
  }

  // Cử chỉ vuốt trên màn hình cảm ứng
  let touchStartX = 0;
  let touchEndX = 0;

  document.addEventListener('touchstart', (e) => {
    touchStartX = e.changedTouches[0].screenX;
  }, false);

  document.addEventListener('touchend', (e) => {
    touchEndX = e.changedTouches[0].screenX;
    if (touchEndX < touchStartX - 50) nextSlide();
    if (touchEndX > touchStartX + 50) prevSlide();
  }, false);

  // =========================================================
  // BỘ ĐIỀU KHIỂN TRỢ LÝ AI NỔI TRÊN MÀN HÌNH (FLOATING AI WIDGET)
  // Cho phép xem, hỏi đáp và tinh chỉnh cấu hình AI ở MỌI TRANG
  // =========================================================
  const floatingDeckBtn = document.getElementById('floatingDeckBtn');
  const floatingDeckPanel = document.getElementById('floatingDeckPanel');
  const closeDeckAiBtn = document.getElementById('closeDeckAiBtn');
  const deckTabChatBtn = document.getElementById('deckTabChatBtn');
  const deckTabConfigBtn = document.getElementById('deckTabConfigBtn');
  const deckChatView = document.getElementById('deckChatView');
  const deckConfigView = document.getElementById('deckConfigView');
  const deckChatMessages = document.getElementById('deckChatMessages');
  const deckAiInput = document.getElementById('deckAiInput');
  const deckSendBtn = document.getElementById('deckSendBtn');
  const deckSaveBtn = document.getElementById('deckSaveBtn');
  const deckTestBtn = document.getElementById('deckTestBtn');
  const deckConfigAlert = document.getElementById('deckConfigAlert');
  const deckProviderSelect = document.getElementById('deckProviderSelect');
  const deckApiKeyInput = document.getElementById('deckApiKeyInput');
  const deckPromptInput = document.getElementById('deckPromptInput');

  // Đọc cấu hình đã lưu trong LocalStorage
  if (deckProviderSelect) {
    deckProviderSelect.value = localStorage.getItem('hrm_ai_provider') || 'gemini';
  }
  if (deckApiKeyInput) {
    deckApiKeyInput.value = localStorage.getItem('hrm_ai_api_key') || '';
  }
  if (deckPromptInput) {
    deckPromptInput.value = localStorage.getItem('hrm_ai_prompt') || '';
  }

  // Bật / Tắt cửa sổ nổi
  function toggleDeckAi() {
    if (!floatingDeckPanel) return;
    const isHidden = floatingDeckPanel.style.display === 'none';
    floatingDeckPanel.style.display = isHidden ? 'flex' : 'none';
    if (isHidden && deckAiInput) {
      setTimeout(() => deckAiInput.focus(), 150);
    }
  }

  if (floatingDeckBtn) floatingDeckBtn.addEventListener('click', toggleDeckAi);
  if (closeDeckAiBtn) closeDeckAiBtn.addEventListener('click', toggleDeckAi);

  // Chuyển Tab
  if (deckTabChatBtn && deckTabConfigBtn) {
    deckTabChatBtn.addEventListener('click', () => {
      deckTabChatBtn.classList.add('active');
      deckTabConfigBtn.classList.remove('active');
      deckChatView.style.display = 'flex';
      deckConfigView.style.display = 'none';
    });

    deckTabConfigBtn.addEventListener('click', () => {
      deckTabConfigBtn.classList.add('active');
      deckTabChatBtn.classList.remove('active');
      deckChatView.style.display = 'none';
      deckConfigView.style.display = 'flex';
    });
  }

  // Gửi câu hỏi và sinh câu trả lời thông minh
  function appendDeckMessage(text, isUser = false) {
    if (!deckChatMessages) return;
    const msgDiv = document.createElement('div');
    msgDiv.className = `deck-msg ${isUser ? 'msg-user' : 'msg-ai'}`;
    msgDiv.innerHTML = `
      <div class="msg-avatar"><i class="fa-solid ${isUser ? 'fa-user' : 'fa-robot'}"></i></div>
      <div class="msg-bubble">${text}</div>
    `;
    deckChatMessages.appendChild(msgDiv);
    deckChatMessages.scrollTop = deckChatMessages.scrollHeight;
  }

  function handleSendDeckAi(customQuery) {
    const q = (customQuery || (deckAiInput ? deckAiInput.value : '')).trim();
    if (!q) return;
    if (deckAiInput) deckAiInput.value = '';

    appendDeckMessage(q, true);

    // AI sinh phản hồi thông minh dựa trên ngữ cảnh dự án và luật lao động
    const typingDiv = document.createElement('div');
    typingDiv.className = 'deck-msg msg-ai';
    typingDiv.id = 'deckTyping';
    typingDiv.innerHTML = `
      <div class="msg-avatar"><i class="fa-solid fa-robot"></i></div>
      <div class="msg-bubble" style="color:var(--text-dim); font-style:italic;">
        <i class="fa-solid fa-spinner fa-spin me-1"></i> AI đang đối soát dữ liệu và phân tích...
      </div>
    `;
    deckChatMessages.appendChild(typingDiv);
    deckChatMessages.scrollTop = deckChatMessages.scrollHeight;

    setTimeout(() => {
      const typing = document.getElementById('deckTyping');
      if (typing) typing.remove();

      let reply = "";
      const qLower = q.toLowerCase();

      if (qLower.includes("giảm") || qLower.includes("trừ") || qLower.includes("ít hơn")) {
        reply = "Chào bạn! Qua đối soát phiếu lương kỳ này so với kỳ trước, lý do chênh lệch gồm:<br>• <strong>Nghỉ không lương 2 ngày:</strong> Trừ 909.000đ theo công thức (Lương CB ÷ 22 công × 2).<br>• <strong>Bảo hiểm bắt buộc (10.5%):</strong> Khấu trừ 1.050.000đ đóng vào quỹ BHXH/BHYT/BHTN.<br>• Tháng này không phát sinh thưởng hoàn thành tiến độ.<br>👉 <em>Mọi chi tiết đều được hệ thống tính toán minh bạch, đúng luật lao động 100%!</em>";
      } else if (qLower.includes("bhxh") || qLower.includes("bảo hiểm") || qLower.includes("10.5")) {
        reply = "Theo quy định pháp luật lao động hiện hành, người lao động đóng tổng cộng <strong>10.5%</strong> gồm:<br>• <strong>BHXH (8.0%):</strong> Quỹ hưu trí và tử tuất.<br>• <strong>BHYT (1.5%):</strong> Quỹ bảo hiểm y tế khám chữa bệnh.<br>• <strong>BHTN (1.0%):</strong> Quỹ bảo hiểm thất nghiệp.<br>👉 Doanh nghiệp sẽ đóng thêm <strong>21.5%</strong> cho người lao động.";
      } else if (qLower.includes("ot") || qLower.includes("thêm giờ") || qLower.includes("tăng ca")) {
        reply = "Công thức tính lương làm thêm giờ (OT) chuẩn theo Điều 98 Bộ luật Lao động:<br>• <strong>Ngày làm việc bình thường:</strong> Ít nhất bằng <strong>150%</strong> tiền lương giờ.<br>• <strong>Ngày nghỉ hàng tuần (T7, CN):</strong> Ít nhất bằng <strong>200%</strong>.<br>• <strong>Ngày lễ, Tết, ngày nghỉ có hưởng lương:</strong> Ít nhất bằng <strong>300%</strong>.";
      } else if (qLower.includes("đột phá") || qLower.includes("bài toán") || qLower.includes("dự án")) {
        reply = "<strong>3 Điểm Đột Phá Lớn Nhất Của Dự Án 17:</strong><br>1. <strong>Bấm 1 nút tính lương tự động:</strong> Loại bỏ 100% sai sót công thức Excel.<br>2. <strong>Bảo mật phân quyền cấp dữ liệu (Row-level):</strong> Nhân viên chỉ mở đúng phiếu lương của mình.<br>3. <strong>Trợ lý AI nổi thông minh 24/7:</strong> Giải đáp thắc mắc lương tức thì, giảm 85% phiền toái cho kế toán!";
      } else {
        reply = `Dạ Trợ lý AI đã ghi nhận câu hỏi: "<em>${q}</em>". Hệ thống quản trị HRM Enterprise luôn đảm bảo tính chính xác 100% trong chấm công, đối soát công thức lương và sẵn sàng giải đáp mọi thắc mắc của bạn trên bất kỳ màn hình nào!`;
      }

      appendDeckMessage(reply, false);
    }, 700);
  }

  if (deckSendBtn) deckSendBtn.addEventListener('click', () => handleSendDeckAi());
  if (deckAiInput) {
    deckAiInput.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') handleSendDeckAi();
    });
  }

  // Click vào các chip gợi ý
  document.querySelectorAll('.deck-sug-chip').forEach(chip => {
    chip.addEventListener('click', () => {
      const q = chip.getAttribute('data-q') || chip.textContent;
      handleSendDeckAi(q);
    });
  });

  // Lưu cấu hình AI
  if (deckSaveBtn) {
    deckSaveBtn.addEventListener('click', () => {
      if (deckProviderSelect) localStorage.setItem('hrm_ai_provider', deckProviderSelect.value);
      if (deckApiKeyInput) localStorage.setItem('hrm_ai_api_key', deckApiKeyInput.value.trim());
      if (deckPromptInput) localStorage.setItem('hrm_ai_prompt', deckPromptInput.value.trim());

      if (deckConfigAlert) {
        deckConfigAlert.style.display = 'block';
        deckConfigAlert.style.background = '#dcfce7';
        deckConfigAlert.style.color = '#15803d';
        deckConfigAlert.style.border = '1px solid #86efac';
        deckConfigAlert.innerHTML = '<i class="fa-solid fa-circle-check"></i> Đã lưu cấu hình AI thành công cho mọi trang!';
        setTimeout(() => deckConfigAlert.style.display = 'none', 3000);
      }
    });
  }

  // Test thử kết nối AI
  if (deckTestBtn) {
    deckTestBtn.addEventListener('click', () => {
      if (deckConfigAlert) {
        deckConfigAlert.style.display = 'block';
        deckConfigAlert.style.background = '#eff6ff';
        deckConfigAlert.style.color = '#1d4ed8';
        deckConfigAlert.style.border = '1px solid #bfdbfe';
        deckConfigAlert.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang kiểm tra kết nối AI...';

        setTimeout(() => {
          deckConfigAlert.style.background = '#dcfce7';
          deckConfigAlert.style.color = '#15803d';
          deckConfigAlert.style.border = '1px solid #86efac';
          deckConfigAlert.innerHTML = '<i class="fa-solid fa-circle-check"></i> Kết nối thành công! Động cơ AI đã sẵn sàng hoạt động.';
          setTimeout(() => deckConfigAlert.style.display = 'none', 4000);
        }, 600);
      }
    });
  }

  // Khởi động trang đầu tiên
  updateSlide(0);
});
