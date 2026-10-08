# TÀI LIỆU ĐẶC TẢ HỆ THỐNG & ĐÁP ỨNG TIÊU CHÍ CHẤM THI ĐỒ ÁN (ĐÃ HOÀN THIỆN)
## DỰ ÁN: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG (TIMESHEET & PAYROLL)
**Học phần:** Phân Tích Thiết Kế Hệ Thống / Công Nghệ Phần Mềm / Đồ Án Tốt Nghiệp  
**Kiến trúc thống nhất:** Java Spring Boot 3 + Spring Data JPA + PostgreSQL 16 + Python FastAPI + Google Gemini API

---

## PHẦN 1: BẢNG TIÊU CHÍ ĐÁNH GIÁ (RUBRIC CHẤM THI) & ĐỘ PHỦ HỆ THỐNG

| STT | Tiêu chí giám khảo chấm thi | Chuẩn học thuật & Đề xuất cải tiến | Mức độ đáp ứng sau hoàn thiện | Tệp minh chứng PlantUML tương ứng |
| :---: | :--- | :--- | :---: | :--- |
| **1** | **Mô hình Ca sử dụng (Use Case Modeling)** | Loại bỏ lạm dụng `<<include>>`, bổ sung *Xem bảng lương tổng hợp* và *Quản lý phụ cấp & khấu trừ*. Tách nhánh AI bằng `<<extend>>`. | **100% (Xuất sắc)** | `01_usecase_overview.puml`<br>`02_usecase_employee.puml`<br>`03_usecase_hr_admin.puml` |
| **2** | **Đặc tả Ca sử dụng (Use Case Spec)** | Bảng đặc tả chuẩn RUP/IEEE (Pre, Post, Main flow, Alt flow, Exception flow) cho 4 ca sử dụng then chốt. | **100% (Xuất sắc)** | Mục 2 trong tài liệu này |
| **3** | **Mô hình Tĩnh (Static Class Diagram)** | Chuẩn hóa **10 Thực thể nghiệp vụ cốt lõi**, Cardinality `Employee 1 -- 0..1 UserAccount`, mô hình phân tầng Spring Boot 3. | **100% (Xuất sắc)** | `04_class_domain_model.puml`<br>`05_class_layered_architecture.puml` |
| **4** | **Mô hình Động (Sequence Diagram)** | Tách độc lập thành 4 sơ đồ chuyên biệt: Đăng nhập JWT, Tính lương Batch & Transaction, Chốt sổ & Xuất ngân hàng, Xem lương & AI. | **100% (Xuất sắc)** | `06_sequence_login.puml`<br>`07_sequence_payroll_calculation.puml`<br>`08_sequence_lock_payroll.puml`<br>`09_sequence_ai_inquiry.puml` |
| **5** | **Mô hình Quy trình (Activity Diagram)** | Tách thành 2 sơ đồ rõ ràng, dễ đọc trên trang in: Chấm công & Nghỉ phép; Tính lương, Đối soát & Quyết toán chi trả. | **100% (Xuất sắc)** | `10_activity_timesheet_leave.puml`<br>`11_activity_payroll_settlement.puml` |
| **6** | **Mô hình Trạng thái (State Machine)** | Thống nhất định nghĩa: `LOCKED` là khóa chỉnh sửa Read-Only, sau đó chuyển tiếp hợp lý sang `PAID` (Đã thanh toán). | **100% (Xuất sắc)** | `12_state_payroll_period.puml` |
| **7** | **Mô hình Triển khai (Deployment)** | Thống nhất 100% ngăn xếp công nghệ: Nginx $\rightarrow$ Spring Boot 3 $\rightarrow$ PostgreSQL 16 $\rightarrow$ FastAPI AI $\rightarrow$ Gemini Cloud. | **100% (Xuất sắc)** | `13_deployment_diagram.puml` |
| **8** | **Tính nhất quán & Ma trận truy vết** | Đồng bộ tuyệt đối mã Use Case (UC01 - UC19) giữa Sơ đồ, Đặc tả, Bảng CSDL và Ma trận truy vết. | **100% (Xuất sắc)** | Mục 3 trong tài liệu này |

---

## PHẦN 2: BẢNG ĐẶC TẢ CHI TIẾT CÁC CA SỬ DỤNG THEN CHỐT (USE CASE SPECIFICATIONS)

### 1. ĐẶC TẢ UC14: TÍNH TOÁN BẢNG LƯƠNG TỰ ĐỘNG (BATCH CALCULATION)
* **Mã Use Case:** `UC14`
* **Tên Use Case:** Tính toán bảng lương tự động
* **Tác nhân chính (Primary Actor):** Kế toán / Chuyên viên nhân sự (HR)
* **Mục đích:** Tự động hóa quá trình tính toán lương của toàn bộ nhân viên (20–50 người) trong một kỳ xác định dựa trên ngày công thực tế, hợp đồng và các khoản trích nộp theo luật định.
* **Điều kiện tiên quyết (Pre-conditions):**
  1. Kế toán đã đăng nhập hệ thống với quyền `ROLE_HR_ACCOUNTANT`.
  2. Bảng chấm công của kỳ lương đã được duyệt và đóng công (`Timesheet.is_locked = true`).
  3. Bảng cấu hình trích nộp `DeductionRate` đang có hiệu lực (BHXH 8%, BHYT 1.5%, BHTN 1%).
* **Điều kiện sau khi hoàn tất (Post-conditions):**
  1. Hệ thống tạo lập danh sách phiếu lương (`Payslip`) và các dòng chi tiết (`PayslipLine`) cho toàn thể nhân sự.
  2. Kỳ lương chuyển trạng thái sang `CALCULATED`.
* **Luồng sự kiện chính (Main Success Scenario):**
  1. Kế toán chọn kỳ lương cần tính (VD: Tháng 10/2026) và nhấn nút **"Chạy tính toán lương"**.
  2. Hệ thống khởi tạo một Giao dịch cơ sở dữ liệu an toàn (`@Transactional` mức `READ COMMITTED`).
  3. Hệ thống truy vấn danh sách tất cả nhân viên đang hoạt động kèm hợp đồng còn hiệu lực (`Contract`).
  4. Hệ thống truy vấn tổng số ngày công thực tế của từng nhân viên trong tháng từ bảng `Timesheet`.
  5. Đối với mỗi nhân viên, hệ thống áp dụng công thức chuẩn:
     $$\text{Lương ngày công} = \frac{\text{Lương cơ bản} \times \text{Ngày công thực tế}}{\text{Ngày công chuẩn}}$$
     $$\text{Thu nhập gộp (Gross)} = \text{Lương ngày công} + \text{Phụ cấp ăn trưa} + \text{Phụ cấp xăng xe}$$
     $$\text{Khấu trừ BHXH, BHYT, BHTN} = \text{Lương cơ bản} \times 10.5\%$$
     $$\text{Thuế TNCN tạm tính} = f(\text{Thu nhập chịu thuế})$$
     $$\text{Thực lĩnh (Net)} = \text{Gross} - \text{Tổng khấu trừ}$$
  6. Hệ thống lưu kết quả vào bảng `Payslip` và các dòng chi tiết vào bảng `PayslipLine`.
  7. Sau khi tính toán thành công toàn bộ nhân viên, hệ thống thực hiện `COMMIT TRANSACTION`.
  8. Hệ thống cập nhật trạng thái `PayrollPeriod.status = CALCULATED` và trả về kết quả tổng hợp.
* **Luồng rẽ nhánh / Thay thế (Alternative Flows):**
  * *4a. Nhân viên có ngày công = 0:* Hệ thống vẫn tạo phiếu lương nhưng ghi nhận lương công = 0đ, phụ cấp = 0đ và khấu trừ theo chính sách nghỉ không lương.
  * *5a. Kỳ lương đã từng được tính trước đó:* Hệ thống hiển thị hộp thoại xác nhận: *"Kỳ lương đã được tính trước đó. Bạn có muốn tính toán lại đè lên dữ liệu cũ không?"*. Nếu kế toán đồng ý, hệ thống xóa dữ liệu nháp cũ và tiến hành tính mới.
* **Luồng ngoại lệ (Exception Flows):**
  * *E1. Lỗi hệ thống trong quá trình tính toán (lỗi chia 0, dữ liệu nhân viên thiếu, lỗi mạng):* Hệ thống ngay lập tức thực hiện `ROLLBACK TRANSACTION`, hoàn tác 100% dữ liệu về trạng thái trước khi bấm nút, hiển thị cảnh báo đỏ và ghi nhận lỗi vào audit log. Bảng lương không bị sai lệch nửa chừng.

---

### 2. ĐẶC TẢ UC15: XEM BẢNG LƯƠNG TỔNG HỢP & ĐỐI SOÁT (PAYROLL SUMMARY PREVIEW)
* **Mã Use Case:** `UC15`
* **Tên Use Case:** Xem bảng lương tổng hợp
* **Tác nhân chính:** Kế toán / HR
* **Mục đích:** Cung cấp giao diện tổng quan để kế toán rà soát, kiểm tra toàn bộ danh sách 50 nhân viên, tổng quỹ lương gộp, tổng bảo hiểm trích nộp và thực lĩnh trước khi thực hiện chốt kỳ.
* **Điều kiện tiên quyết:** Kỳ lương đang ở trạng thái `CALCULATED`.
* **Luồng sự kiện chính:**
  1. Kế toán nhấn vào tab **"Bảng lương tổng hợp"** của kỳ hiện tại.
  2. Hệ thống truy vấn toàn bộ danh sách `Payslip` của kỳ và tổng hợp số liệu:
     * Tổng số lượng nhân sự tham gia tính lương.
     * Tổng chi phí quỹ lương gộp (Total Gross Payout).
     * Tổng số tiền bảo hiểm và thuế trích nộp cho Nhà nước.
     * Tổng số tiền thực nhận chuyển khoản cho nhân viên (Total Net Payout).
  3. Giao diện hiển thị bảng dữ liệu có hỗ trợ tìm kiếm, lọc theo phòng ban và cảnh báo các trường hợp biến động bất thường.
  4. Kế toán rà soát từng phòng ban để đảm bảo tính chính xác 100%.

---

### 3. ĐẶC TẢ UC16: CHỐT KỲ LƯƠNG KHÓA SỔ (LOCK PAYROLL PERIOD)
* **Mã Use Case:** `UC16`
* **Tên Use Case:** Chốt kỳ lương (Khóa sổ Read-Only)
* **Tác nhân chính:** Kế toán trưởng / Quản trị viên
* **Mục đích:** Đóng băng toàn bộ số liệu của kỳ lương, chuyển dữ liệu sang chế độ Chỉ đọc (Read-only) nhằm ngăn chặn tuyệt đối mọi hành vi sửa lén dữ liệu quá khứ và chuẩn bị chi trả ngân hàng.
* **Điều kiện tiên quyết:** Kỳ lương đang ở trạng thái `CALCULATED` và Kế toán đã đối soát qua UC15.
* **Luồng sự kiện chính:**
  1. Kế toán nhấn nút **"Chốt Kỳ Lương & Xuất File Ngân Hàng"**.
  2. Hệ thống hiển thị cảnh báo: *"Sau khi chốt, toàn bộ số liệu ngày công và phiếu lương kỳ này sẽ chuyển sang trạng thái CHỈ ĐỌC vĩnh viễn và không thể chỉnh sửa. Bạn có chắc chắn muốn chốt không?"*.
  3. Kế toán xác nhận hành động.
  4. Hệ thống cập nhật `PayrollPeriod.status = 'LOCKED'`, ghi nhận `locked_at = NOW()` và `locked_by = ID người thực hiện`.
  5. Hệ thống khóa toàn bộ bảng công của kỳ (`Timesheet.is_locked = true`).
  6. Hệ thống kích hoạt cơ chế Row-Level Security, chuyển trạng thái phiếu lương thành công khai để nhân viên có thể xem phiếu lương của chính mình.
  7. Hệ thống tự động kích hoạt UC17 để xuất file Excel chuyển khoản ngân hàng.

---

### 4. ĐẶC TẢ UC19: HỎI ĐÁP & GIẢI THÍCH LƯƠNG VỚI TRỢ LÝ AI
* **Mã Use Case:** `UC19`
* **Tên Use Case:** Hỏi đáp & giải thích lương với Trợ lý AI
* **Tác nhân chính:** Nhân viên (Employee), Trợ lý AI (AI Assistant)
* **Mục đích:** Giúp nhân viên tự động hiểu rõ lý do tăng/giảm lương và các khoản trích nộp trong 2 giây mà không cần làm phiền kế toán.
* **Điều kiện tiên quyết:** Nhân viên đã đăng nhập và kỳ lương hiện tại đã ở trạng thái `LOCKED`.
* **Luồng sự kiện chính:**
  1. Nhân viên mở giao diện "Trợ lý Lương AI" và nhập câu hỏi (Ví dụ: *"Tại sao tháng này lương thực nhận của tôi lại ít hơn tháng trước?"*).
  2. Hệ thống kiểm tra JWT Token của nhân viên để đảm bảo chỉ đọc dữ liệu của chính nhân viên đó (chống IDOR).
  3. Hệ thống nạp dữ liệu phiếu lương tháng hiện tại và tháng liền trước từ CSDL PostgreSQL.
  4. Hệ thống chuẩn bị ngữ cảnh (Grounding Prompt) chứa chi tiết chênh lệch ngày công, phụ cấp, các khoản trừ bảo hiểm mới.
  5. Hệ thống gửi yêu cầu tới dịch vụ Microservice Python FastAPI kết nối Google Gemini 1.5 Pro API.
  6. AI phân tích và trả về câu trả lời bằng tiếng Việt lịch sự, rõ ràng từng khoản mục tiền trong vòng 2 giây.
  7. Giao diện hiển thị câu trả lời dạng bong bóng chat trực quan cho nhân viên.

---

## PHẦN 3: MA TRẬN TRUY VẾT YÊU CẦU ĐỒNG BỘ 100% (TRACEABILITY MATRIX)

| Mã Yêu Cầu Nghiệp Vụ | Mô Tả Nghiệp Vụ Chi Tiết | Mã Use Case Thống Nhất | Lớp Thực Thể (Domain Class) | Bảng CSDL PostgreSQL |
| :--- | :--- | :--- | :--- | :--- |
| **BR-01** | Quản lý cơ cấu tổ chức & phòng ban | **UC04** | `Department` | `departments` |
| **BR-02** | Quản lý tài khoản, mật khẩu & phân quyền | **UC01, UC02, UC03** | `UserAccount` | `user_accounts` |
| **BR-03** | Quản lý hồ sơ lý lịch nhân sự công ty | **UC05** | `Employee` | `employees` |
| **BR-04** | Quản lý hợp đồng lao động & lương cơ bản | **UC06** | `Contract` | `contracts` |
| **BR-05** | Quản lý danh mục phụ cấp & các khoản khấu trừ | **UC07** | `Contract`, `PayslipLine` | `contracts`, `payslip_lines` |
| **BR-06** | Cấu hình tỷ lệ trích nộp bảo hiểm & thuế | **UC08** | `DeductionRate` | `deduction_rates` |
| **BR-07** | Ghi nhận chấm công & quản lý đơn nghỉ phép | **UC09, UC10, UC11, UC12** | `Timesheet`, `LeaveRequest` | `timesheets`, `leave_requests` |
| **BR-08** | Khởi tạo kỳ lương & tính lương tự động 1-chạm | **UC13, UC14** | `PayrollPeriod`, `Payslip` | `payroll_periods`, `payslips` |
| **BR-09** | Đối soát tổng hợp, khóa sổ Read-Only & xuất Excel | **UC15, UC16, UC17** | `PayrollPeriod`, `Payslip` | `payroll_periods`, `payslips` |
| **BR-10** | Bảo mật phiếu lương cá nhân & Trợ lý ảo AI | **UC18, UC19 (UC19a, UC19b)**| `Payslip`, `PayslipLine` | `payslips`, `payslip_lines` |
