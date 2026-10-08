# HỆ THỐNG MÔ HÌNH HÓA UML TOÀN DIỆN (ĐÃ HOÀN THIỆN THEO BÁO CÁO ĐÁNH GIÁ)
## Dự Án 17: Hệ Thống Chấm Công & Tính Lương Tự Động (Timesheet & Payroll)

Thư mục này chứa đầy đủ toàn bộ **13 sơ đồ UML chuẩn PlantUML**, **13 tệp hình ảnh PNG chất lượng cao** và **tài liệu đặc tả chi tiết** đã được hoàn thiện 100% theo các góp ý chuyên sâu của Báo cáo kiểm tra và đánh giá học thuật:

---

### Danh Mục 13 Sơ Đồ UML Chuẩn Học Thuật

| Nhóm Sơ Đồ | Tên Tệp PlantUML | Tên Tệp Ảnh PNG | Nội Dung & Điểm Cải Tiến Học Thuật |
| :--- | :--- | :--- | :--- |
| **1. Ca Sử Dụng (Use Case)** | **[01_usecase_overview.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/01_usecase_overview.puml)** | `01_usecase_overview.png` | Sơ đồ Use Case tổng quan: Loại bỏ include chuỗi; bổ sung UC07 & UC15; thống nhất mã UC01 - UC19. |
| | **[02_usecase_employee.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/02_usecase_employee.puml)** | `02_usecase_employee.png` | Phân hệ Nhân viên: Tách độc lập Use Case AI, mô hình hóa các câu hỏi chuyên sâu bằng `<<extend>>`. |
| | **[03_usecase_hr_admin.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/03_usecase_hr_admin.puml)** | `03_usecase_hr_admin.png` | Phân hệ Kế toán HR & Admin: Bổ sung Quản lý phụ cấp/khấu trừ (UC07) và Xem bảng lương tổng hợp (UC15). |
| **2. Cấu Trúc Lớp (Class)** | **[04_class_domain_model.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/04_class_domain_model.puml)** | `04_class_domain_model.png` | **10 Thực thể nghiệp vụ cốt lõi**; chuẩn hóa Cardinality `Employee 1 -- 0..1 UserAccount` và các Enums. |
| | **[05_class_layered_architecture.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/05_class_layered_architecture.puml)** | `05_class_layered_architecture.png` | Sơ đồ Lớp thiết kế phân tầng: Thống nhất hoàn toàn công nghệ **Java Spring Boot 3 & Spring Data JPA**. |
| **3. Tuần Tự (Sequence)** | **[06_sequence_login.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/06_sequence_login.puml)** | `06_sequence_login.png` | Sequence 1: Đăng nhập, băm BCrypt, JWT Token và kiểm soát phân quyền RBAC. |
| | **[07_sequence_payroll_calculation.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/07_sequence_payroll_calculation.puml)** | `07_sequence_payroll_calculation.png` | Sequence 2: Tách độc lập cho Tính lương tự động hàng loạt & Cơ chế an toàn `@Transactional` (Commit/Rollback). |
| | **[08_sequence_lock_payroll.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/08_sequence_lock_payroll.puml)** | `08_sequence_lock_payroll.png` | Sequence 3: Tách độc lập cho Đối soát tổng hợp (UC15), Chốt sổ Read-Only (UC16) và Xuất file Excel ngân hàng (UC17). |
| | **[09_sequence_ai_inquiry.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/09_sequence_ai_inquiry.puml)** | `09_sequence_ai_inquiry.png` | Sequence 4: Tra cứu phiếu lương, bảo mật chống IDOR và kết nối FastAPI + Google Gemini API. |
| **4. Hoạt Động (Activity)** | **[10_activity_timesheet_leave.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/10_activity_timesheet_leave.puml)** | `10_activity_timesheet_leave.png` | Activity 1: Quy trình Chấm công & Nghỉ phép (Phân làn Swimlane rõ ràng, không bị nhỏ chữ). |
| | **[11_activity_payroll_settlement.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/11_activity_payroll_settlement.puml)** | `11_activity_payroll_settlement.png` | Activity 2: Quy trình Tính lương, Đối soát bảng tổng hợp, Chốt kỳ và Quyết toán chi trả ngân hàng. |
| **5. Trạng Thái (State)** | **[12_state_payroll_period.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/12_state_payroll_period.puml)** | `12_state_payroll_period.png` | Vòng đời Kỳ lương: Chuẩn hóa `LOCKED` là Read-Only (không sửa dữ liệu), chuyển tiếp hợp lý sang `PAID` (Đã thanh toán). |
| **6. Triển Khai (Deployment)** | **[13_deployment_diagram.puml](file:///c:/Users/TUF/Desktop/bangChamCong/uml/13_deployment_diagram.puml)** | `13_deployment_diagram.png` | Thống nhất 100% ngăn xếp công nghệ: Client $\rightarrow$ Nginx $\rightarrow$ Spring Boot 3 $\rightarrow$ PostgreSQL 16 $\rightarrow$ FastAPI $\rightarrow$ Gemini Cloud. |
| **7. Tài Liệu Đặc Tả** | **[TIEU_CHI_CHAM_THI_VA_DAC_TA.md](file:///c:/Users/TUF/Desktop/bangChamCong/uml/TIEU_CHI_CHAM_THI_VA_DAC_TA.md)** | N/A | Bảng đặc tả 4 Use Case then chốt, Rubric chấm thi và Ma trận truy vết khớp nối mã UC01 - UC19. |

---

### Báo Cáo Word Hoàn Chỉnh

* Tệp tài liệu Word chính thức: **[BAO_CAO_SO_DO_UML_DU_AN_17.docx](file:///c:/Users/TUF/Desktop/bangChamCong/BAO_CAO_SO_DO_UML_DU_AN_17.docx)**
* Toàn bộ 13 hình ảnh sơ đồ vẽ sắc nét được lưu trữ tại: **[uml/images/](file:///c:/Users/TUF/Desktop/bangChamCong/uml/images/)**
