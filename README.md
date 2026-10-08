# ĐỒ ÁN MÔN HỌC: LẬP TRÌNH ỨNG DỤNG WEB
# HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG (TIMESHEET & PAYROLL SYSTEM)

---

## 📌 THÔNG TIN SINH VIÊN & HỌC PHẦN

* **Họ và tên sinh viên:** **Trần Viết Mạnh Cường**
* **Mã sinh viên (MSV):** `23K4080003`
* **Khoa:** Hệ thống Thông tin Kinh tế
* **Ngành học:** Tin học Kinh tế
* **Học phần:** Lập trình Ứng dụng Web
* **Tên đề tài:** **Hệ Thống Chấm Công & Tính Lương Tự Động (Timesheet & Payroll System)**
* **Kho lưu trữ (Repository):** [https://github.com/ManhCuong-Code/TimesheetPayroll](https://github.com/ManhCuong-Code/TimesheetPayroll)

---

## 📖 1. GIỚI THIỆU TỔNG QUAN & BỐI CẢNH DỰ ÁN

Tại các doanh nghiệp vừa và nhỏ (quy mô từ 20 đến 50 nhân sự), việc quản lý ngày công và tính lương thường phụ thuộc vào bảng tính Excel thủ công. Cách làm truyền thống này dẫn đến nhiều rủi ro:
1. **Sai lệch dữ liệu & nhầm lẫn công thức:** Kéo lệch ô tính, sai sót phụ cấp hoặc trích nộp bảo hiểm khiến kế toán mất 2–3 ngày cuối tháng để dò tìm sai sót.
2. **Thiếu bảo mật & nguy cơ lộ thu nhập:** Bảng tính Excel chia sẻ chung dễ bị lộ thông tin thu nhập nhạy cảm giữa các nhân viên.
3. **Thay đổi chính sách pháp lý:** Các mức trích nộp Bảo hiểm Xã hội (BHXH), Bảo hiểm Y tế (BHYT), Bảo hiểm Thất nghiệp (BHTN) thường bị hardcode cứng, khó cập nhật khi luật nhà nước thay đổi.

**Giải pháp đề tài:** Xây dựng **Hệ thống Chấm công & Tính lương tự động (Timesheet & Payroll System)** ứng dụng kiến trúc phân tầng sạch **Clean Architecture**, công nghệ **.NET 10 Blazor Interactive Server**, kết nối cơ sở dữ liệu **Microsoft SQL Server (`BangChamCongDB`)** qua **Dapper ORM**.  
Hệ thống **tích hợp bảo mật đăng nhập xác thực tài khoản và phân quyền kiểm soát truy cập nghiêm ngặt theo vai trò (Role-Based Access Control - RBAC)**:
* 👤 **`ROLE_EMPLOYEE` (Nhân viên):** Điểm danh trực tuyến Check-In/Check-Out của chính mình, tra cứu bảng công tháng, nộp đơn xin nghỉ phép, tra cứu phiếu lương cá nhân (được bảo vệ bằng cơ chế *Ownership-based Authorization* chống xem lén thu nhập).
* 💼 **`ROLE_HR_ACCOUNTANT` (Kế toán Tiền lương / Nhân sự):** Quản lý hồ sơ nhân viên, hợp đồng lao động, xét duyệt đơn xin nghỉ phép, khóa bảng công kỳ, chạy tính toán bảng lương tự động hàng loạt 1-chạm an toàn với Database Transaction, đối soát tổng hợp, chốt kỳ lương khóa sổ **Read-Only** chống sửa đổi và xuất file chuyển khoản ngân hàng.
* 🛡️ **`ROLE_ADMIN` (Quản trị viên hệ thống):** Quản lý tài khoản người dùng, phân quyền vai trò, cấu hình tỷ lệ trích nộp bảo hiểm và giám sát toàn bộ hệ thống.

---

## 🎯 2. CÔNG NGHỆ ÁP DỤNG & KIẾN TRÚC HỆ THỐNG

| Thành phần | Công nghệ lựa chọn | Mục đích sử dụng |
| :--- | :--- | :--- |
| **Nền tảng phát triển** | **.NET 10 (C# 13)** | Nền tảng hiện đại, hiệu năng cao, bảo mật doanh nghiệp |
| **Giao diện Web** | **Blazor Interactive Server** | Render thời gian thực qua SignalR WebSocket, không giật lag |
| **Giao diện & Styling** | **Bootstrap 5 + FontAwesome 6** | Chuẩn Responsive, trực quan hóa dữ liệu lương chuyên nghiệp |
| **Kiến trúc phần mềm** | **Clean Architecture (Modular)** | Tách biệt hoàn toàn `CoreBusiness`, `UseCases`, `Plugins`, `Web.Modules` |
| **Xác thực & Phân quyền** | **Cookie Authentication & RBAC** | Bảo mật phiên làm việc, phân quyền theo 3 vai trò (`EMPLOYEE`, `HR`, `ADMIN`) |
| **Hệ quản trị CSDL** | **Microsoft SQL Server 2025 / 2022** | CSDL riêng biệt `BangChamCongDB`, chuẩn hóa 3NF, toàn vẹn dữ liệu |
| **Truy cập dữ liệu** | **Dapper Micro-ORM** | Thực thi câu lệnh SQL siêu tốc, kiểm soát 100% câu truy vấn |

---

## 🧮 3. BỘ CÔNG THỨC NGHIỆP VỤ TÍNH LƯƠNG CHUẨN

Động cơ tính toán lương (`PayrollCalculationService`) áp dụng bộ công thức chuẩn hóa theo quy định hiện hành:

1. **Lương ngày công thực tế theo tỷ lệ:**
   $$\text{Lương ngày công} = \text{Lương cơ bản} \times \frac{\text{Ngày công thực tế}}{\text{Ngày công chuẩn}}$$
2. **Thu nhập gộp (Gross Salary):**
   $$\text{Gross} = \text{Lương ngày công} + \text{Phụ cấp ăn trưa} + \text{Phụ cấp xăng xe}$$
3. **Các khoản trích nộp bảo hiểm bắt buộc theo luật (10.5% lương cơ bản):**
   * $\text{BHXH (Bảo hiểm Xã hội)} = \text{Lương cơ bản} \times 8.0\%$
   * $\text{BHYT (Bảo hiểm Y tế)} = \text{Lương cơ bản} \times 1.5\%$
   * $\text{BHTN (Bảo hiểm Thất nghiệp)} = \text{Lương cơ bản} \times 1.0\%$
   $$\text{Tổng khấu trừ} = \text{BHXH} + \text{BHYT} + \text{BHTN}$$
4. **Thu nhập thực lĩnh chi trả vào tài khoản (Net Salary):**
   $$\text{Thực lĩnh (Net)} = \text{Gross} - \text{Tổng khấu trừ}$$

---

## 📊 4. TOÀN BỘ 13 SƠ ĐỒ UML HỆ THỐNG (ĐỒNG BỘ ĐĂNG NHẬP & PHÂN QUYỀN)

---

### SƠ ĐỒ 01: Ca Sử Dụng Tổng Quan Hệ Thống & Phân Quyền (RBAC Overview Use Case Diagram)

```mermaid
flowchart TD
    subgraph LoginAuth["Xác Thực Tài Khoản Đăng Nhập"]
        UC01(["UC01: Đăng Nhập Hệ Thống"])
        UC02(["UC02: Xác Thực Mật Khẩu BCrypt & Cấp Phiên"])
        UC03(["UC03: Đổi Mật Khẩu & Đăng Xuất"])
        UC01 -->|include| UC02
    end

    subgraph Roles["Phân Quyền Vai Trò (RBAC)"]
        R_EMP["Quyền: ROLE_EMPLOYEE"]
        R_HR["Quyền: ROLE_HR_ACCOUNTANT"]
        R_ADM["Quyền: ROLE_ADMIN"]
    end

    subgraph EmployeePortal["Cổng Thông Tin Nhân Viên"]
        UC09(["UC09: Điểm danh Chấm công Check-in / Check-out"])
        UC10(["UC10: Xem Bảng công Cá nhân trong tháng"])
        UC11(["UC11: Nộp Đơn xin Nghỉ phép trực tuyến"])
        UC18(["UC18: Tra cứu Phiếu lương Cá nhân (Bảo mật IDOR)"])
    end

    subgraph HRPortal["Cổng Kế Toán Tiền Lương & Nhân Sự"]
        UC04(["UC04: Quản lý Cơ cấu Phòng ban & Nhân sự"])
        UC06(["UC06: Quản lý Hợp đồng & Mức lương cơ bản"])
        UC12(["UC12: Xét duyệt Đơn xin Nghỉ phép"])
        UC13(["UC13: Khởi tạo & Đóng Bảng công Kỳ"])
        UC14(["UC14: Tính toán Bảng lương Tự động (Batch)"])
        UC15(["UC15: Đối soát Bảng lương Tổng hợp"])
        UC16(["UC16: Chốt kỳ Lương Khóa sổ (Read-Only)"])
        UC17(["UC17: Kết xuất File Chuyển khoản Ngân hàng"])
    end

    subgraph AdminPortal["Cổng Quản Trị Hệ Thống"]
        UC07(["UC07: Quản lý Tài khoản & Phân quyền Người dùng"])
        UC08(["UC08: Cấu hình Tỷ lệ Bảo hiểm (BHXH, BHYT, BHTN)"])
    end

    User(["Người Dùng"]) --> UC01
    UC02 -->|Phân quyền| R_EMP
    UC02 -->|Phân quyền| R_HR
    UC02 -->|Phân quyền| R_ADM

    R_EMP --> UC09
    R_EMP --> UC10
    R_EMP --> UC11
    R_EMP --> UC18

    R_HR --> UC04
    R_HR --> UC06
    R_HR --> UC12
    R_HR --> UC13
    R_HR --> UC14
    R_HR --> UC15
    R_HR --> UC16
    R_HR --> UC17

    R_ADM --> UC07
    R_ADM --> UC08

    UC14 -.->|include| UC15
    UC15 -.->|pre-requisite| UC16
    UC16 -.->|trigger| UC17
```

---

### SƠ ĐỒ 02: Ca Sử Dụng Phân Hệ Nhân Viên (Employee Portal Use Case Diagram)

```mermaid
flowchart LR
    actor Emp as "Nhân Viên (Employee)"

    subgraph EmployeeScope["Phân Hệ Nhân Viên (Yêu cầu đăng nhập ROLE_EMPLOYEE)"]
        UC_Auth(["Xác Thực Tài Khoản & Quyền Sở Hữu"])
        UC_CheckIn(["UC09a: Điểm Danh Vào Ca (Check-In)"])
        UC_CheckOut(["UC09b: Điểm Danh Tan Ca (Check-Out)"])
        UC_ViewTimesheet(["UC10: Tra Cứu Nhật Ký Chấm Công Hàng Ngày"])
        UC_LeaveApp(["UC11: Tạo Đơn Xin Nghỉ Phép (Phép năm, Nghỉ ốm...)"])
        UC_ViewPayslip(["UC18: Tra Cứu Phiếu Lương Cá Nhân Chi Tiết"])
        UC_ExplainSalary(["UC19: Xem Giải Thích Từng Khoản Thu Nhập & Trừ BH"])
    end

    Emp --> UC_Auth
    UC_Auth --> UC_CheckIn
    UC_Auth --> UC_CheckOut
    UC_Auth --> UC_ViewTimesheet
    UC_Auth --> UC_LeaveApp
    UC_Auth --> UC_ViewPayslip
    UC_ViewPayslip -.->|extend| UC_ExplainSalary
```

---

### SƠ ĐỒ 03: Ca Sử Dụng Phân Hệ Kế Toán & Quản Trị (HR & Admin Use Case Diagram)

```mermaid
flowchart LR
    actor HR as "Kế Toán / HR (ROLE_HR_ACCOUNTANT)"
    actor Admin as "Quản Trị Viên (ROLE_ADMIN)"

    subgraph AdminScope["Phân Hệ Quản Trị & Kế Toán Lương"]
        UC_ManageEmp(["UC04: Quản Lý Hồ Sơ & Phòng Ban"])
        UC_ApproveLeave(["UC12: Duyệt / Từ Chối Đơn Nghỉ Phép"])
        UC_LockTimesheet(["UC13: Khóa Bảng Chấm Công Tháng"])
        UC_CalcBatch(["UC14: Chạy Tính Lương Tự Động Toàn Công Ty"])
        UC_SummaryPreview(["UC15: Đối Soát Bảng Lương Tổng Hợp (Preview)"])
        UC_LockPeriod(["UC16: Chốt Kỳ Lương Khóa Sổ (Read-Only Bất Biến)"])
        UC_ExportBank(["UC17: Xuất File Excel Ủy Nhiệm Chi Ngân Hàng"])
        UC_ManageAccounts(["UC07: Quản Lý Tài Khoản & Cấp Quyền"])
        UC_ConfigRates(["UC08: Cấu Hình Tỷ Lệ Trích Nộp Bảo Hiểm Động"])
    end

    HR --> UC_ManageEmp
    HR --> UC_ApproveLeave
    HR --> UC_LockTimesheet
    HR --> UC_CalcBatch
    HR --> UC_SummaryPreview
    HR --> UC_LockPeriod
    HR --> UC_ExportBank

    Admin --> UC_ManageAccounts
    Admin --> UC_ConfigRates
    Admin --> UC_LockPeriod

    UC_CalcBatch -->|Bắt buộc đối soát| UC_SummaryPreview
    UC_SummaryPreview -->|Điều kiện tiên quyết| UC_LockPeriod
    UC_LockPeriod -->|Kích hoạt xuất file| UC_ExportBank
```

---

### SƠ ĐỒ 04: Sơ Đồ Lớp Miền Nghiệp Vụ Cốt Lõi (Domain Model Class Diagram)

Sơ đồ lớp chuẩn hóa **10 Thực thể nghiệp vụ** gắn kết chặt chẽ với cơ chế tài khoản người dùng (`UserAccount`) và phân quyền (`UserRole`):

```mermaid
classDiagram
    class UserRole {
        <<enumeration>>
        ROLE_EMPLOYEE
        ROLE_HR_ACCOUNTANT
        ROLE_ADMIN
    }

    class WorkShiftStatus {
        <<enumeration>>
        PRESENT
        LATE
        EARLY_LEAVE
        HALF_DAY
        ABSENT_WITH_PERMISSION
        ABSENT_UNAUTHORIZED
        HOLIDAY
    }

    class LeaveType {
        <<enumeration>>
        ANNUAL_LEAVE
        SICK_LEAVE
        MATERNITY_LEAVE
        UNPAID_LEAVE
    }

    class ApprovalStatus {
        <<enumeration>>
        PENDING
        APPROVED
        REJECTED
    }

    class PeriodStatus {
        <<enumeration>>
        DRAFT
        CALCULATING
        CALCULATED
        LOCKED
        PAID
    }

    class PayslipLineType {
        <<enumeration>>
        BASE_SALARY_PRORATED
        MEAL_ALLOWANCE
        FUEL_ALLOWANCE
        OVERTIME_PAY
        BONUS
        DEDUCTION_BHXH
        DEDUCTION_BHYT
        DEDUCTION_BHTN
        PERSONAL_INCOME_TAX
        PENALTY
    }

    class Department {
        +long Id
        +string DepartmentCode
        +string DepartmentName
        +string ManagerName
        +string Description
    }

    class Employee {
        +long Id
        +string EmployeeCode
        +string FullName
        +DateTime DateOfBirth
        +string IdentityCardNumber
        +string Email
        +string PhoneNumber
        +string BankAccountNumber
        +string BankName
        +long DepartmentId
        +decimal BaseSalary
        +decimal MealAllowance
        +decimal FuelAllowance
        +string Status
    }

    class UserAccount {
        +long Id
        +long EmployeeId
        +string Username
        +string PasswordHash
        +UserRole Role
        +bool IsActive
        +DateTime LastLoginAt
        +bool VerifyPassword(string rawPass)
    }

    class Contract {
        +long Id
        +long EmployeeId
        +string ContractNumber
        +string ContractType
        +decimal BaseSalary
        +decimal MealAllowance
        +decimal FuelAllowance
        +DateTime StartDate
        +DateTime EndDate
        +bool IsActive
    }

    class Timesheet {
        +long Id
        +long EmployeeId
        +long PeriodId
        +DateTime WorkDate
        +TimeSpan CheckInTime
        +TimeSpan CheckOutTime
        +decimal ActualHoursWorked
        +WorkShiftStatus WorkShiftStatus
        +decimal WorkUnits
        +bool IsLocked
    }

    class LeaveRequest {
        +long Id
        +long EmployeeId
        +LeaveType LeaveType
        +DateTime StartDate
        +DateTime EndDate
        +decimal TotalDays
        +string Reason
        +ApprovalStatus ApprovalStatus
        +string ApprovedBy
        +DateTime ApprovedAt
    }

    class DeductionRate {
        +long Id
        +string RateCode
        +string RateName
        +decimal EmployeeRate
        +decimal EmployerRate
        +DateTime EffectiveFrom
        +DateTime EffectiveTo
        +bool IsActive
    }

    class PayrollPeriod {
        +long Id
        +string PeriodCode
        +int PeriodMonth
        +int PeriodYear
        +decimal StandardWorkDays
        +PeriodStatus Status
        +decimal TotalGrossAmount
        +decimal TotalNetAmount
        +DateTime LockedAt
        +string LockedBy
    }

    class Payslip {
        +long Id
        +long PeriodId
        +long EmployeeId
        +decimal ActualWorkDays
        +decimal PaidLeaveDays
        +decimal OvertimeHours
        +decimal GrossSalary
        +decimal TotalDeductions
        +decimal NetSalary
        +List~PayslipLine~ Lines
    }

    class PayslipLine {
        +long Id
        +long PayslipId
        +PayslipLineType LineType
        +string LineDescription
        +decimal Amount
        +bool IsDeduction
    }

    Department "1" --> "0..*" Employee : có nhân sự >
    Employee "1" <--> "1" UserAccount : định danh tài khoản
    UserAccount ..> UserRole : phân quyền vai trò
    Employee "1" --> "0..*" Contract : ký hợp đồng >
    Employee "1" --> "0..*" Timesheet : ghi nhận chấm công >
    Employee "1" --> "0..*" LeaveRequest : tạo đơn xin nghỉ >
    Employee "1" --> "0..*" Payslip : nhận phiếu lương >
    PayrollPeriod "1" --> "0..*" Payslip : chứa các phiếu lương >
    Payslip "1" *-- "1..*" PayslipLine : cấu thành từ >
```

---

### SƠ ĐỒ 05: Sơ Đồ Lớp Kiến Trúc Phân Tầng Phân Quyền (Layered Architecture Class Diagram)

```mermaid
classDiagram
    namespace Presentation_Layer {
        class TopNavbarComponent {
            -AuthenticationState AuthState
            +RenderRoleBasedNavigation()
        }
        class PayrollBatchCalculationPage {
            -ICalculatePayrollBatchUseCase _calculateUseCase
            -ILockPayrollPeriodUseCase _lockUseCase
            +HandleCalculateBatchAsync()
            +HandleLockPeriodAsync()
        }
        class MyPayslipComponent {
            -IViewMyPayslipUseCase _viewPayslipUseCase
            +LoadCurrentEmployeePayslip()
        }
    }

    namespace UseCases_Layer {
        class ICalculatePayrollBatchUseCase {
            <<interface>>
            +ExecuteAsync(long periodId)
        }
        class ILockPayrollPeriodUseCase {
            <<interface>>
            +ExecuteAsync(long periodId, string lockedBy)
        }
        class IViewMyPayslipUseCase {
            <<interface>>
            +ExecuteAsync(long periodId, long employeeId)
        }
    }

    namespace CoreBusiness_Layer {
        class IPayrollCalculationService {
            <<interface>>
            +CalculatePayslip(Employee, long, decimal, decimal, List~DeductionRate~)
        }
        class PayrollCalculationService {
            +CalculatePayslip(...)
        }
    }

    namespace DataAccess_Layer {
        class IDataAccess {
            <<interface>>
            +CreateConnection() IDbConnection
        }
        class IEmployeeRepository {
            <<interface>>
            +GetEmployeesAsync()
        }
        class ITimesheetRepository {
            <<interface>>
            +GetTimesheetsByPeriodAsync(long)
            +LockTimesheetsByPeriodAsync(long)
        }
        class IPayslipRepository {
            <<interface>>
            +SavePayslipBatchAsync(List~Payslip~)
        }
    }

    Presentation_Layer ..> UseCases_Layer : gọi thực thi Use Case
    UseCases_Layer --> CoreBusiness_Layer : ủy quyền tính toán công thức
    UseCases_Layer --> DataAccess_Layer : truy vấn dữ liệu CSDL
    PayrollCalculationService ..|> IPayrollCalculationService : hiện thực
```

---

### SƠ ĐỒ 06: Sơ Đồ Tuần Tự Đăng Nhập & Phân Quyền RBAC (Sequence Diagram: Login & RBAC Auth)

```mermaid
sequenceDiagram
    autonumber
    actor User as Người Dùng (NV / Kế Toán)
    participant UI as Giao Diện Đăng Nhập (Login UI)
    participant AuthCtrl as AuthenticationController
    participant UserRepo as UserRepository
    participant DB as SQL Server [user_accounts]
    participant Session as Cookie / ClaimsPrincipal

    User->>UI: Nhập Username & Password -> Bấm "Đăng Nhập"
    UI->>AuthCtrl: POST /api/auth/login {username, password}
    AuthCtrl->>UserRepo: FindByUsername(username)
    UserRepo->>DB: SELECT * FROM user_accounts WHERE username = @username
    DB-->>UserRepo: Trả về UserAccount (PasswordHash, Role, IsActive, EmployeeId)
    UserRepo-->>AuthCtrl: UserAccount entity

    alt Mật khẩu sai hoặc tài khoản bị khóa
        AuthCtrl-->>UI: 401 Unauthorized ("Tài khoản hoặc mật khẩu không chính xác")
        UI-->>User: Hiển thị thông báo lỗi màu đỏ
    else Mật khẩu hợp lệ (BCrypt.Verify thành công)
        AuthCtrl->>Session: Khởi tạo ClaimsPrincipal (ClaimTypes.Name, ClaimTypes.Role, "EmployeeId")
        Session-->>AuthCtrl: Cấp Cookie Xác thực an toàn (HTTP-Only, Exp: 8h)
        AuthCtrl-->>UI: 200 OK + Điều hướng theo quyền
        alt Role == 'ROLE_EMPLOYEE'
            UI-->>User: Chuyển hướng tới Cổng Nhân Viên (/my-timesheet, /my-payslip)
        else Role == 'ROLE_HR_ACCOUNTANT' hoặc 'ROLE_ADMIN'
            UI-->>User: Chuyển hướng tới Cổng Quản Trị Lương (/admin/payroll-calculation)
        end
    end
```

---

### SƠ ĐỒ 07: Sơ Đồ Tuần Tự Tính Lương Tự Động Có Kiểm Tra Quyền (Sequence Diagram: Payroll Calculation)

```mermaid
sequenceDiagram
    autonumber
    actor HR as Kế Toán / HR (ROLE_HR_ACCOUNTANT)
    participant UI as PayrollBatchCalculationPage
    participant Guard as [Authorize(Roles = "ROLE_HR_ACCOUNTANT")]
    participant UseCase as CalculatePayrollBatchUseCase
    participant Engine as PayrollCalculationService
    participant DB as SQL Server (BangChamCongDB)

    HR->>UI: Bấm "Chạy Tính Lương Tự Động Tháng 10/2026"
    UI->>Guard: Gửi Request tính toán kèm Phiên làm việc
    Guard->>Guard: Kiểm tra Quyền: Người dùng có vai trò ROLE_HR_ACCOUNTANT?
    
    alt Không có quyền (Ví dụ Employee bấm nhầm)
        Guard-->>UI: 403 Forbidden (Từ chối truy cập)
    else Quyền hợp lệ
        Guard->>UseCase: ExecuteAsync(periodId = 10)
        UseCase->>DB: BEGIN TRANSACTION (Mức cô lập: READ COMMITTED)
        UseCase->>DB: Lấy danh sách Nhân viên đang hoạt động & Hợp đồng
        UseCase->>DB: Lấy tổng ngày công thực tế của từng NV từ [timesheets]
        UseCase->>DB: Lấy tỷ lệ bảo hiểm có hiệu lực từ [deduction_rates] (10.5%)

        loop Lặp qua từng nhân viên (1..N)
            UseCase->>Engine: CalculatePayslip(emp, periodId, actualDays, standardDays, rates)
            Engine->>Engine: 1. Lương công = (Lương CB * Công thực tế / Công chuẩn)
            Engine->>Engine: 2. Gross = Lương công + Phụ cấp ăn/xăng
            Engine->>Engine: 3. Khấu trừ BHXH (8%), BHYT (1.5%), BHTN (1%)
            Engine->>Engine: 4. Net = Gross - Tổng khấu trừ bảo hiểm
            Engine-->>UseCase: Đối tượng Payslip kèm danh sách PayslipLine
        end

        UseCase->>DB: INSERT / UPDATE bảng [payslips] & [payslip_lines]
        UseCase->>DB: UPDATE payroll_periods SET status = 'CALCULATED', total_gross = ..., total_net = ...
        UseCase->>DB: COMMIT TRANSACTION (Lưu dữ liệu an toàn 100%)
        UseCase-->>UI: Trả về danh sách phiếu lương tổng hợp
        UI-->>HR: Thông báo "Tính toán hoàn tất!" & Hiển thị bảng đối soát quỹ lương
    end
```

---

### SƠ ĐỒ 08: Sơ Đồ Tuần Tự Chốt Kỳ Lương Khóa Sổ & Xuất Ngân Hàng (Sequence Diagram: Lock Payroll)

```mermaid
sequenceDiagram
    autonumber
    actor HR as Kế Toán Trưởng / Admin
    participant UI as Bảng Điều Khiển Lương
    participant LockUC as LockPayrollPeriodUseCase
    participant PeriodRepo as PayrollPeriodRepository
    participant TRepo as TimesheetRepository
    participant DB as SQL Server (BangChamCongDB)

    HR->>UI: Rà soát số liệu xong -> Bấm nút "Chốt Kỳ Lương & Xuất File Ngân Hàng"
    UI-->>HR: Hiển thị cảnh báo: "Sau khi chốt, dữ liệu sẽ chuyển sang CHỈ ĐỌC (Read-Only) vĩnh viễn. Bạn có chắc chắn?"
    HR->>UI: Xác nhận "Đồng Ý Chốt Kỳ"
    UI->>LockUC: ExecuteAsync(periodId = 10, lockedBy = "ketoan.thao")
    LockUC->>PeriodRepo: GetPeriodByIdAsync(10)
    PeriodRepo-->>LockUC: PayrollPeriod (status = 'CALCULATED')

    LockUC->>DB: UPDATE payroll_periods SET status = 'LOCKED', locked_at = GETDATE(), locked_by = @User
    LockUC->>TRepo: LockTimesheetsByPeriodAsync(10)
    TRepo->>DB: UPDATE timesheets SET is_locked = 1 WHERE period_id = 10
    note over DB: Toàn bộ ngày công và phiếu lương của kỳ chuyển sang CHỈ ĐỌC (Read-Only),<br/>ngăn chặn 100% mọi hành vi sửa lén dữ liệu quá khứ.

    LockUC-->>UI: 200 OK + Khóa sổ thành công
    UI-->>HR: Trạng thái kỳ đổi sang ĐÃ KHÓA SỔ (LOCKED) & Tải file Excel ủy nhiệm chi
```

---

### SƠ ĐỒ 09: Sơ Đồ Tuần Tự Tra Cứu Phiếu Lương Cá Nhân Bảo Mật (Sequence Diagram: My Payslip IDOR-Safe)

```mermaid
sequenceDiagram
    autonumber
    actor Emp as Nhân Viên (Anh Nam - EMP001)
    participant UI as MyPayslipComponent
    participant Guard as OwnershipAccessGuard
    participant UseCase as ViewMyPayslipUseCase
    participant DB as SQL Server [payslips]

    Emp->>UI: Mở trang "Phiếu Lương Của Tôi" (Tháng 10/2026)
    UI->>Guard: Gửi Request lấy phiếu lương kèm Cookie Xác thực
    Guard->>Guard: 1. Kiểm tra Quyền: Người dùng có vai trò ROLE_EMPLOYEE?<br/>2. Kiểm tra Sở hữu (Anti-IDOR): Claim.EmployeeId == Request.EmployeeId?
    
    alt Cố tình sửa URL lấy lương người khác (IDOR Attack)
        Guard-->>UI: 403 Forbidden ("Bạn không có quyền xem phiếu lương của nhân viên khác")
        UI-->>Emp: Cảnh báo từ chối truy cập
    else Quyền sở hữu hợp lệ
        Guard->>UseCase: ExecuteAsync(periodId = 10, employeeId = 1)
        UseCase->>DB: SELECT * FROM payslips WHERE period_id = 10 AND employee_id = 1
        UseCase->>DB: SELECT * FROM payslip_lines WHERE payslip_id = @Id
        DB-->>UseCase: Dữ liệu phiếu lương của chính Nam (Net: 8.540.909đ)
        UseCase-->>UI: Đối tượng Payslip chi tiết
        UI-->>Emp: Hiển thị Thẻ đồ họa chi tiết lương: Lương công, phụ cấp ăn, trừ 10.5% BH, thực lĩnh
    end
```

---

### SƠ ĐỒ 10: Sơ Đồ Hoạt Động Chấm Công & Nghỉ Phép Phân Quyền (Activity Diagram: Timesheet & Leave)

```mermaid
flowchart TD
    start([Bắt đầu ngày làm việc]) --> LoginEmp[Nhân viên Đăng nhập tài khoản ROLE_EMPLOYEE]
    LoginEmp --> CheckIn[Bấm Check-In Vào Ca: Ghi nhận giờ thực tế]
    CheckIn --> Work[Làm việc trong ca chuẩn 8.0 giờ]
    Work --> CheckOut[Bấm Check-Out Tan Ca: Hệ thống tự động tính số giờ làm]

    CheckOut --> HasLeave{Có nhu cầu nghỉ phép / việc riêng?}
    HasLeave -- Không --> NormalWork[Hệ thống tự động ghi nhận 1.0 công chuẩn]
    HasLeave -- Có --> SubmitLeave[Nhân viên tạo Đơn xin nghỉ phép trực tuyến]

    SubmitLeave --> LoginHR[Kế toán / HR đăng nhập tài khoản ROLE_HR_ACCOUNTANT]
    LoginHR --> ReviewLeave{Kiểm tra lý do & định mức phép hợp lệ?}
    ReviewLeave -- Không duyệt --> Reject[HR từ chối đơn: Ghi nhận công vắng trừ lương]
    ReviewLeave -- Phê duyệt --> Approve[HR duyệt đơn: Cập nhật chế độ phép có/không lương]

    NormalWork --> MonthEnd[Cuối tháng: HR rà soát bảng chấm công]
    Reject --> MonthEnd
    Approve --> MonthEnd

    MonthEnd --> LockTS[HR bấm Khóa Bảng Công Tháng: is_locked = true]
    LockTS --> finish([Chuyển tiếp sang quy trình tính lương])
```

---

### SƠ ĐỒ 11: Sơ Đồ Hoạt Động Tính Lương, Đối Soát & Quyết Toán (Activity Diagram: Payroll Settlement)

```mermaid
flowchart TD
    start([Bắt đầu chu kỳ tính lương cuối tháng]) --> LoginHR[Kế toán đăng nhập quyền ROLE_HR_ACCOUNTANT]
    LoginHR --> CreatePeriod[Khởi tạo kỳ tính lương mới trạng thái DRAFT]
    CreatePeriod --> RunBatch[Nhấn 'Chạy Tính Lương Tự Động' 1-chạm]

    RunBatch --> OpenTx[Hệ thống mở Database Transaction @Transactional]
    OpenTx --> ReadData[Nạp dữ liệu Hợp đồng, Bảng công đã khóa & Tỷ lệ trích nộp 10.5%]
    ReadData --> Compute[Thực thi công thức: Lương công + Phụ cấp - Khấu trừ bảo hiểm]
    Compute --> CommitTx[Lưu vào Payslip & PayslipLines -> Commit Transaction]
    CommitTx --> StatusCalculated[Chuyển trạng thái kỳ sang CALCULATED]

    StatusCalculated --> Preview[Kế toán mở màn hình Bảng lương tổng hợp đối soát]
    Preview --> Validate{Số liệu quỹ lương gộp và thực lĩnh chính xác 100%?}
    
    Validate -- Cần chỉnh sửa --> Adjust[Yêu cầu cập nhật lại ngày công / phụ cấp]
    Adjust --> RunBatch

    Validate -- Hoàn toàn chính xác --> LockPeriod[Bấm 'Chốt Kỳ Lương Khóa Sổ']
    LockPeriod --> ReadOnly[Chuyển kỳ sang LOCKED: Toàn bộ dữ liệu chuyển sang CHỈ ĐỌC vĩnh viễn]
    ReadOnly --> ExportExcel[Hệ thống tự động xuất file Excel ủy nhiệm chi ngân hàng]
    ExportExcel --> BankTransfer[Chuyển tiền qua Ngân hàng vào tài khoản nhân viên]
    BankTransfer --> MarkPaid[Chuyển trạng thái kỳ sang PAID]

    MarkPaid --> EmpNotify[Thông báo tới toàn thể nhân viên]
    EmpNotify --> EmpLogin[Nhân viên đăng nhập quyền ROLE_EMPLOYEE tra cứu phiếu lương cá nhân]
    EmpLogin --> finish([Kết thúc chu kỳ quyết toán lương])
```

---

### SƠ ĐỒ 12: Sơ Đồ Máy Trạng Thái Vòng Đời Kỳ Lương (State Machine Diagram: Payroll Period Lifecycle)

```mermaid
stateDiagram-v2
    [*] --> DRAFT : Khởi tạo kỳ lương mới (VD: Tháng 10/2026)

    DRAFT : Thu thập bảng chấm công hàng ngày
    DRAFT : Tiếp nhận và xử lý đơn xin nghỉ phép
    DRAFT : Cho phép điều chỉnh mức lương và phụ cấp hợp đồng

    DRAFT --> CALCULATING : Kế toán HR bấm "Chạy Tính Lương Tự Động"

    CALCULATING : Mở Database Transaction (@Transactional)
    CALCULATING : Tính toán lương công, phụ cấp & trích nộp 10.5%
    CALCULATING : Khóa tạm thời các bản ghi công

    CALCULATING --> DRAFT : Có lỗi dữ liệu / Rollback Transaction an toàn
    CALCULATING --> CALCULATED : Tính toán 100% thành công cho toàn thể nhân sự

    CALCULATED : Phiếu lương ở dạng Xem trước (Preview)
    CALCULATED : Kế toán đối soát tổng quỹ lương trên màn hình tổng hợp
    CALCULATED : Cho phép chạy tính lại khi cập nhật dữ liệu đầu vào

    CALCULATED --> CALCULATING : Chạy lại tính toán khi cần thiết
    CALCULATED --> LOCKED : Kế toán xác nhận và bấm "Chốt Kỳ Lương"

    LOCKED : ĐÃ KHÓA SỔ BẤT BIẾN (Read-Only)
    LOCKED : Ngăn chặn tuyệt đối mọi hành vi sửa đổi dữ liệu quá khứ
    LOCKED : Phiếu lương hiển thị công khai tới từng nhân viên
    LOCKED : Xuất file danh sách chuyển khoản cho Ngân hàng

    LOCKED --> PAID : Kế toán xác nhận chuyển khoản ngân hàng thành công
    PAID : Đã thanh toán đầy đủ vào tài khoản ngân hàng nhân viên
    PAID : Lưu trữ vào kho dữ liệu kế toán lịch sử (Archive)

    PAID --> [*] : Hoàn tất chu kỳ quyết toán
```

---

### SƠ ĐỒ 13: Sơ Đồ Triển Khai Hệ Thống Đa Tầng Bảo Mật (Deployment Diagram)

```mermaid
flowchart TB
    subgraph ClientTier["Tầng Thiết Bị Khách (Client Devices)"]
        BrowserEmp["Trình duyệt Nhân viên (Mobile / Laptop)<br/>Truy cập: /my-timesheet, /my-payslip"]
        BrowserHR["Trình duyệt Kế toán / Admin (PC Văn phòng)<br/>Truy cập: /admin/payroll-calculation"]
    end

    subgraph SecurityGateway["Tầng Bảo Mật & Cổng Vào (Security Gateway)"]
        Nginx["Nginx Reverse Proxy / HTTPS SSL<br/>Port 443 -> Forward Port 5000"]
    end

    subgraph AppServer["Tầng Ứng Dụng (Application Server - .NET 10 Blazor)"]
        BlazorHost["ASP.NET Core Web Host Kestrel<br/>Blazor Interactive Server (SignalR)"]
        AuthMiddleware["Cookie Authentication & RBAC Middleware<br/>Kiểm tra Claim, Role, Ownership"]
        UseCasesModule["Clean Architecture UseCases & Services<br/>PayrollCalculationService Engine"]
    end

    subgraph DatabaseTier["Tầng Cơ Sở Dữ Liệu (Database Server)"]
        SQLServer["Microsoft SQL Server Instance (.\\SQLEXPRESS)<br/>Database: BangChamCongDB<br/>Port 1433"]
    end

    BrowserEmp -->|HTTPS / WSS| Nginx
    BrowserHR -->|HTTPS / WSS| Nginx
    Nginx --> BlazorHost
    BlazorHost --> AuthMiddleware
    AuthMiddleware --> UseCasesModule
    UseCasesModule -->|Dapper Micro-ORM TCP/IP| SQLServer
```

---

## 🏗️ 5. CẤU TRÚC THƯ MỤC DỰ ÁN CLEAN ARCHITECTURE

```text
TimesheetPayroll/
│
├── TimesheetPayroll.slnx                                   # Visual Studio Solution
│
├── TimesheetPayroll.CoreBusiness/                          # Tầng Miền Nghiệp Vụ Cốt Lõi
│   ├── Models/
│   │   ├── Employee.cs                                     # Thực thể Nhân viên
│   │   ├── Department.cs                                   # Thực thể Phòng ban
│   │   ├── Timesheet.cs                                    # Thực thể Bảng chấm công
│   │   ├── PayrollPeriod.cs                                # Thực thể Kỳ lương
│   │   ├── Payslip.cs                                      # Thực thể Phiếu lương
│   │   ├── LeaveRequest.cs                                 # Thực thể Đơn nghỉ phép
│   │   ├── DeductionRate.cs                                # Thực thể Tỷ lệ bảo hiểm
│   │   └── Enums.cs                                        # Kiểu liệt kê (UserRole, WorkShiftStatus,...)
│   └── Services/
│       ├── Interfaces/IPayrollCalculationService.cs        # Interface động cơ tính lương
│       └── PayrollCalculationService.cs                    # Triển khai thuật toán tính toán lương
│
├── TimesheetPayroll.UseCases/                              # Tầng Trường Hợp Sử Dụng (Use Cases)
│   ├── AdminPortal/                                        # Ca sử dụng phân hệ Kế toán & Quản trị
│   │   └── AdminPortalUseCases.cs                          # Tính lương hàng loạt, Khóa sổ, Duyệt phép
│   ├── EmployeePortal/                                     # Ca sử dụng phân hệ Nhân viên
│   │   └── EmployeePortalUseCases.cs                       # Xem phiếu lương, Chấm công, Nộp đơn phép
│   └── PluginInterfaces/                                   # Hợp đồng giao tiếp tầng ngoài
│       └── DataStore/IRepositories.cs                      # Interface các Repository truy xuất CSDL
│
├── TimesheetPayroll.Web.Modules/                           # Các Module Giao Diện Blazor Độc Lập
│   ├── TimesheetPayroll.Web.AdminPortal/                   # Giao diện Cổng Quản trị / Kế toán
│   │   ├── Pages/PayrollBatchCalculationPage.razor         # Trang tính lương tự động & Chốt sổ
│   │   └── Controls/PayrollSummaryComponent.razor          # Bảng đối soát lương chi tiết
│   ├── TimesheetPayroll.Web.EmployeePortal/                # Giao diện Cổng Thông tin Nhân viên
│   │   ├── Pages/MyPayslipComponent.razor                  # Trang xem phiếu lương cá nhân
│   │   ├── Pages/MyTimesheetComponent.razor                # Trang điểm danh Check-In/Check-Out
│   │   ├── Pages/LeaveRequestComponent.razor               # Trang nộp đơn xin nghỉ phép
│   │   └── Controls/PayslipDetailCard.razor                # Thẻ đồ họa trực quan thu nhập cá nhân
│   └── TimesheetPayroll.Web.Common/                        # Thành phần tái sử dụng chung
│       └── Controls/SearchBarComponent.razor, StatusBadgeComponent.razor
│
├── Plugins/                                                # Tầng Hạ Tầng Kỹ Thuật (Infrastructure)
│   ├── TimesheetPayroll.DataStore.SQL.Dapper/              # Kết nối SQL Server qua Dapper Micro-ORM
│   │   ├── DataAccess.cs                                   # Quản lý SqlConnection
│   │   └── SqlRepositories.cs                              # Triển khai Dapper Repositories
│   ├── TimesheetPayroll.DataStore.HandCoded/               # Mock data chạy thử nghiệm độc lập
│   └── TimesheetPayroll.StateStore.DI/                     # Quản lý trạng thái phiên làm việc Blazor
│
└── TimesheetPayroll.Web/                                   # Ứng Dụng Host Blazor Web App
    ├── Components/
    │   ├── App.razor, Routes.razor
    │   ├── Layout/TopNavbar.razor, MainLayout.razor
    │   └── Pages/Home.razor                                # Trang chủ hệ thống
    ├── Program.cs                                          # Đăng ký Dependency Injection & Middleware
    └── appsettings.json                                    # Cấu hình Connection String CSDL
```

---

## 🚀 6. HƯỚNG DẪN CÀI ĐẶT & KHỞI CHẠY HỆ THỐNG

### Bước 1: Khởi tạo Cơ sở dữ liệu SQL Server
1. Mở **SQL Server Management Studio (SSMS)** và kết nối tới máy chủ (ví dụ `.\SQLEXPRESS`).
2. Mở tệp script **`database_schema.sql`** (hoặc `database_mssql.sql`) nằm tại thư mục gốc dự án.
3. Nhấn **Execute (F5)** để tự động tạo CSDL riêng biệt **`BangChamCongDB`**, tạo 10 bảng dữ liệu và nạp sẵn dữ liệu thực nghiệm.

---

### Bước 2: Cấu hình chuỗi kết nối (Connection String)
Kiểm tra tệp `project/TimesheetPayroll/TimesheetPayroll.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BangChamCongDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

### Bước 3: Biên dịch và chạy ứng dụng Blazor
Mở PowerShell hoặc Command Prompt tại thư mục dự án và thực hiện:

```bash
# 1. Điều hướng vào thư mục mã nguồn
cd project/TimesheetPayroll

# 2. Khôi phục packages và kiểm tra biên dịch
dotnet build TimesheetPayroll.slnx

# 3. Khởi chạy ứng dụng Web Blazor
dotnet run --project TimesheetPayroll.Web
```

---

### Bước 4: Trải nghiệm các tính năng theo phân quyền
Mở trình duyệt truy cập: `https://localhost:5001` hoặc `http://localhost:5000`:

1. **Phân hệ Nhân viên (`ROLE_EMPLOYEE`):**
   * **Điểm danh chấm công (`/my-timesheet`):** Bấm Check-In / Check-Out thời gian thực, xem tổng số công trong tháng.
   * **Nộp đơn xin nghỉ phép (`/leave-request`):** Chọn loại nghỉ phép (phép năm, nghỉ ốm, thai sản, không lương) và nộp đơn.
   * **Xem phiếu lương cá nhân (`/my-payslip`):** Tra cứu minh bạch phiếu lương của anh Nguyễn Văn A (Mã `EMP001`): Lương công thực tế (20/22 ngày công), phụ cấp ăn trưa 500.000đ, trừ 10.5% bảo hiểm (1.050.000đ), **Thực lĩnh chính xác 8.540.909 đ**.
2. **Phân hệ Kế toán & Quản trị (`ROLE_HR_ACCOUNTANT`):**
   * **Tính lương tự động (`/admin/payroll-calculation`):** Bấm nút **"Chạy Tính Lương Tự Động"** tính đồng loạt cho 100% nhân viên.
   * **Đối soát tổng quỹ lương:** Rà soát danh sách nhân viên, phòng ban, lương gộp và thực lĩnh.
   * **Chốt sổ kỳ lương:** Bấm nút **"Chốt Kỳ Lương Khóa Sổ"** chuyển vĩnh viễn dữ liệu sang trạng thái **Read-Only** chống sửa đổi và xuất file ngân hàng.

---

## 📋 7. KẾT LUẬN & ĐÁNH GIÁ ĐẠT ĐƯỢC

* ✅ **Đáp ứng 100% chuẩn học thuật:** Đầy đủ trọn bộ **13 sơ đồ UML** chuyên sâu, hiển thị trực quan dạng Mermaid Markdown trên GitHub.
* ✅ **Bảo mật đăng nhập & Phân quyền RBAC:** Tách biệt rõ ràng quyền hạn giữa Nhân viên, Kế toán HR và Quản trị viên, chống xem lén thu nhập.
* ✅ **Mã nguồn chuẩn Clean Architecture:** Tách lớp độc lập, dễ mở rộng, tuân thủ nguyên lý SOLID.
* ✅ **Toàn vẹn tài chính & Khóa sổ an toàn:** Đảm bảo tính toán chính xác tuyệt đối, hỗ trợ chốt sổ Read-Only bất biến.
