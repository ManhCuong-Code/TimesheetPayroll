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

  // Khởi động trang đầu tiên
  updateSlide(0);
});
