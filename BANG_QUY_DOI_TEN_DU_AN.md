# BẢNG QUY CHUẨN ĐỔI TÊN CÁC HẠNG MỤC THEO DỰ ÁN
## DỰ ÁN: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG (TIMESHEET & PAYROLL)

Tài liệu này chuẩn hóa toàn bộ tên gọi từ **Khái niệm nghiệp vụ dự án**, **Lớp đối tượng (Class/Entity)**, **Bảng CSDL (Database Table)** cho tới **Đường dẫn API (Endpoints)** để phục vụ báo cáo đồ án và lập trình.

---

### 1. Bảng Quy Đổi 10 Thực Thể Cốt Lõi (Domain Classes & Tables)

| STT | Nghiệp vụ thực tế của Dự án | Tên Lớp UML (Class chuẩn Quốc tế) | Tên Lớp Tiếng Việt (Thuần dự án) | Tên Bảng CSDL Hiện tại (BangChamCongDB) | Tên Bảng Thuần Việt (Tùy chọn) | Tiền tố dự án (`bcc_`) |
| :---: | :--- | :--- | :--- | :--- | :--- | :--- |
| **1** | **Quản lý Phòng ban** | `Department` | `PhongBan` | `departments` | `phong_ban` | `bcc_phong_ban` |
| **2** | **Hồ sơ Nhân sự** | `Employee` | `NhanVien` | `employees` | `nhan_vien` | `bcc_nhan_vien` |
| **3** | **Tài khoản Đăng nhập** | `UserAccount` | `TaiKhoan` | `user_accounts` | `tai_khoan` | `bcc_tai_khoan` |
| **4** | **Hợp đồng Lao động** | `Contract` | `HopDongLaoDong` | `contracts` | `hop_dong` | `bcc_hop_dong` |
| **5** | **Kỳ Tính Lương** | `PayrollPeriod` | `KyTinhLuong` | `payroll_periods` | `ky_luong` | `bcc_ky_luong` |
| **6** | **Chấm công Hàng ngày** | `Timesheet` | `BangChamCong` | `timesheets` | `bang_cham_cong` | `bcc_cham_cong` |
| **7** | **Đơn Xin Nghỉ Phép** | `LeaveRequest` | `DonXinNghiPhep` | `leave_requests` | `don_nghi_phep` | `bcc_don_nghi_phep` |
| **8** | **Tỷ lệ Trích nộp Bảo hiểm** | `DeductionRate` | `TyLeBaoHiem` | `deduction_rates` | `ty_le_bao_hiem` | `bcc_ty_le_bao_hiem` |
| **9** | **Phiếu Lương Cá nhân** | `Payslip` | `PhieuLuong` | `payslips` | `phieu_luong` | `bcc_phieu_luong` |
| **10**| **Chi tiết Dòng Lương** | `PayslipLine` | `ChiTietPhieuLuong`| `payslip_lines` | `chi_tiet_luong` | `bcc_chi_tiet_luong` |

---

### 2. Quy Đổi Tên Các Kiểu Liệt Kê (Enums)

| Tên Enum Tiếng Anh | Tên Enum Dự Án (Tiếng Việt) | Các Giá Trị (Values) | Ý Nghĩa Trong Dự Án Chấm Công |
| :--- | :--- | :--- | :--- |
| `UserRole` | `VaiTroNguoiDung` | `ROLE_EMPLOYEE`<br>`ROLE_HR_ACCOUNTANT`<br>`ROLE_ADMIN` | • Nhân viên thường<br>• Kế toán tiền lương / HR<br>• Quản trị viên hệ thống |
| `WorkShiftStatus` | `TrangThaiCaCong` | `PRESENT`<br>`LATE`<br>`EARLY_LEAVE`<br>`HALF_DAY`<br>`ABSENT_WITH_PERMISSION`<br>`ABSENT_UNAUTHORIZED`<br>`HOLIDAY` | • Đúng giờ (1.0 công)<br>• Đi trễ<br>• Về sớm<br>• Nửa ngày (0.5 công)<br>• Nghỉ có phép<br>• Nghỉ không phép (trừ lương)<br>• Nghỉ lễ/Tết |
| `LeaveType` | `LoaiNghiPhep` | `ANNUAL_LEAVE`<br>`SICK_LEAVE`<br>`MATERNITY_LEAVE`<br>`UNPAID_LEAVE` | • Nghỉ phép năm<br>• Nghỉ ốm đau<br>• Nghỉ thai sản<br>• Nghỉ không lương |
| `ApprovalStatus` | `TrangThaiPheDuyet` | `PENDING`<br>`APPROVED`<br>`REJECTED` | • Đang chờ duyệt<br>• Đã duyệt<br>• Bị từ chối |
| `PeriodStatus` | `TrangThaiKyLuong` | `DRAFT`<br>`CALCULATING`<br>`CALCULATED`<br>`LOCKED`<br>`PAID` | • Nháp<br>• Đang tính toán<br>• Đã tính (Xem trước)<br>• Đã chốt khóa sổ (Read-Only)<br>• Đã chi trả ngân hàng |
| `PayslipLineType` | `LoaiKhoanMucLuong` | `BASE_SALARY_PRORATED`<br>`MEAL_ALLOWANCE`<br>`FUEL_ALLOWANCE`<br>`OVERTIME_PAY`<br>`BONUS`<br>`DEDUCTION_BHXH`<br>`DEDUCTION_BHYT`<br>`DEDUCTION_BHTN`<br>`PERSONAL_INCOME_TAX`<br>`PENALTY` | • Lương ngày công thực tế<br>• Phụ cấp ăn trưa<br>• Phụ cấp xăng xe<br>• Tiền làm thêm giờ<br>• Thưởng hiệu suất<br>• Trừ BHXH (8%)<br>• Trừ BHYT (1.5%)<br>• Trừ BHTN (1%)<br>• Thuế TNCN<br>• Phạt đi trễ / vi phạm |

---

### 3. Quy Đổi Tên Các Tầng Kiến Trúc & Gói Mã Nguồn (Package & Architecture)

| Tầng Kiến Trúc | Tên Package Chuẩn Dự Án | Các Class Đại Diện Của Dự Án Chấm Công |
| :--- | :--- | :--- |
| **Giao diện & API Controller** | `com.bangchamcong.controller` | `BangChamCongController`, `TinhLuongController`, `DonPhepController`, `BaoCaoLuongController` |
| **Dịch vụ Nghiệp vụ (Service)** | `com.bangchamcong.service` | `ChamCongService`, `TinhLuongService`, `ChotSoKyLuongService`, `XuatNganHangService` |
| **Động cơ Tính Toán Công thức** | `com.bangchamcong.engine` | `CongThucLuongEngine` (Xử lý: Lương công, Phụ cấp, Bảo hiểm 10.5%, Thuế) |
| **Kho Lưu Trữ CSDL (Repository)** | `com.bangchamcong.repository` | `NhanVienRepository`, `BangChamCongRepository`, `KyLuongRepository`, `PhieuLuongRepository` |
| **Thực Thể Dữ Liệu (Entity/Model)** | `com.bangchamcong.entity` | `NhanVien`, `BangChamCong`, `KyTinhLuong`, `PhieuLuong`, `HopDongLaoDong` |
| **Đối Tượng Truyền Nhận (DTO)** | `com.bangchamcong.dto` | `ChamCongRequestDTO`, `TinhLuongBatchDTO`, `BangTongHopLuongDTO`, `PhieuLuongCaNhanDTO` |

---

### 4. Quy Đổi Tên API Endpoints Theo Dự Án

* `POST /api/bang-cham-cong/diem-danh`: Điểm danh check-in / check-out hàng ngày
* `GET  /api/bang-cham-cong/thang/{thang}/{nam}`: Xem bảng công chi tiết tháng của nhân viên
* `POST /api/don-phep/tao-don`: Nhân viên nộp đơn xin nghỉ phép
* `PUT  /api/don-phep/{id}/phe-duyet`: Quản lý duyệt/từ chối đơn phép
* `POST /api/tinh-luong/chay-tu-dong`: Kế toán chạy tính lương tự động cho toàn công ty
* `GET  /api/tinh-luong/bang-tong-hop/{kyLuongId}`: Xem bảng lương tổng hợp đối soát (Preview)
* `POST /api/tinh-luong/chot-ky-khoa-so/{kyLuongId}`: Khóa sổ kỳ lương Read-Only bất biến
* `GET  /api/tinh-luong/xuat-excel-ngan-hang/{kyLuongId}`: Xuất file ủy nhiệm chi Vietcombank/MB
* `GET  /api/phieu-luong/cua-toi`: Nhân viên xem phiếu lương cá nhân (bảo mật chống xem lén)
* `POST /api/ai/giai-thich-luong`: Hỏi đáp với Trợ lý AI giải thích chênh lệch tiền lương
