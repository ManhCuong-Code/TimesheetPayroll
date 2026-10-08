# ĐỒ ÁN MÔN HỌC: LẬP TRÌNH ỨNG DỤNG WEB
# HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG (TIMESHEET & PAYROLL SYSTEM)

---

## 📌 THÔNG TIN SINH VIÊN & HỌC PHẦN

* **Họ và tên sinh viên:** **Trần Viết Mạnh Cường**
* **Mã sinh viên (MSV):** `23K4080003`
* **Khoa:** **Hệ thống Thông tin Kinh tế**
* **Ngành học:** **Tin học Kinh tế**
* **Học phần:** **Lập trình Ứng dụng Web**
* **Tên đề tài:** **Hệ Thống Chấm Công & Tính Lương Tự Động (Timesheet & Payroll System)**
* **Kho lưu trữ (Repository):** [https://github.com/ManhCuong-Code/TimesheetPayroll.git](https://github.com/ManhCuong-Code/TimesheetPayroll.git)
* **Cơ sở dữ liệu:** **Microsoft SQL Server (`BangChamCongDB`)** *(Độc lập 100% với HomeStayHueDB)*
* **Nền tảng công nghệ:** **.NET 10 Blazor Interactive Server + Dapper Micro-ORM + Clean Architecture**

---

## 🔐 BẢNG TÀI KHOẢN ĐĂNG NHẬP THỬ NGHIỆM & PHÂN QUYỀN HỆ THỐNG (RBAC)

Hệ thống được thiết kế với cơ chế **Xác thực và Phân quyền dựa trên vai trò (Role-Based Access Control - RBAC)** kết hợp **Ownership-based Access Control** (kiểm soát quyền sở hữu dữ liệu cá nhân). Mọi thông tin tài khoản được lưu trữ và băm mật khẩu chuẩn **BCrypt** trong bảng `user_accounts` tại CSDL SQL Server `BangChamCongDB`.

Người dùng có thể đăng nhập tại đường dẫn: **`/login`** (hỗ trợ nút bấm **Đăng nhập nhanh 1-Click** hoặc nhập biểu mẫu kiểm tra trực tiếp vào SQL Server).

| STT | Tên Đăng Nhập | Mật Khẩu | Vai Trò (Role) | Họ Và Tên Nhân Viên | Mã NV | Phòng Ban | Quyền Hạn Nghiệp Vụ Trong Hệ Thống |
| :---: | :--- | :---: | :--- | :--- | :---: | :--- | :--- |
| **1** | `nhanvien.a` | `123456` | **`ROLE_EMPLOYEE`** | **Nguyễn Văn A** | `EMP001` | Phòng Công Nghệ Thông Tin | • Điểm danh Check-In/Check-Out hàng ngày<br/>• Tra cứu nhật ký chấm công tháng cá nhân<br/>• Tạo đơn xin nghỉ phép trực tuyến<br/>• Tra cứu phiếu lương chi tiết của chính mình (chống lộ IDOR) |
| **2** | `ketoan.thao` | `123456` | **`ROLE_HR_ACCOUNTANT`** | **Lê Thị Thu Thảo** | `EMP002` | Phòng Nhân Sự & Kế Toán | • Toàn bộ quyền của nhân viên<br/>• Xét duyệt/từ chối đơn xin nghỉ phép<br/>• Khóa bảng chấm công kỳ<br/>• **Chạy tính toán lương tự động hàng loạt 1-chạm**<br/>• Đối soát quỹ lương tổng hợp toàn công ty<br/>• **Chốt kỳ lương Khóa sổ (Read-Only)** chống sửa đổi<br/>• Xuất file chuyển khoản ngân hàng |
| **3** | `admin.trong` | `123456` | **`ROLE_ADMIN`** | **Trần Đình Trọng** | `EMP003` | Ban Giám Đốc / QTV | • Toàn quyền quản trị cao nhất toàn hệ thống<br/>• Quản lý tài khoản người dùng và cấp phát vai trò<br/>• Cấu hình các mức tỷ lệ trích nộp bảo hiểm (BHXH, BHYT, BHTN)<br/>• Giám sát tính lương và chốt sổ tài chính |
| **4** | `nhanvien.tuan` | `123456` | **`ROLE_EMPLOYEE`** | **Hoàng Minh Tuấn** | `EMP004` | Phòng Kinh Doanh & Tiếp Thị | • Chấm công, nộp đơn nghỉ phép và tra cứu phiếu thu nhập cá nhân độc lập của nhân viên Tuấn |

---

## 📖 1. GIỚI THIỆU TỔNG QUAN & BỐI CẢNH DỰ ÁN

Tại các doanh nghiệp vừa và nhỏ (quy mô 20 – 100 nhân sự), việc quản lý ngày công và tính lương thường phụ thuộc vào bảng tính Excel thủ công dẫn đến 3 vấn đề nhức nhối:
1. **Sai lệch công thức & rủi ro kéo lệch ô tính:** Mất từ 2 đến 3 ngày mỗi kỳ lương để kế toán đối soát thủ công ngày công, phụ cấp và các tỷ lệ bảo hiểm.
2. **Thiếu bảo mật & nguy cơ lộ thông tin thu nhập:** Chia sẻ file Excel nội bộ dễ khiến nhân viên xem được lương của nhau, vi phạm chính sách bảo mật doanh nghiệp.
3. **Luật lao động & tỷ lệ bảo hiểm thay đổi:** Các mức trích nộp Bảo hiểm Xã hội (BHXH 8%), Bảo hiểm Y tế (BHYT 1.5%), Bảo hiểm Thất nghiệp (BHTN 1%) thường bị hardcode cứng, khó tùy biến khi chính sách nhà nước điều chỉnh.

**Mục tiêu giải pháp:** Xây dựng **Hệ thống Chấm công & Tính lương tự động (Timesheet & Payroll System)** theo chuẩn kiến trúc sạch **Clean Architecture**, công nghệ **.NET 10 Blazor Interactive Server**, kết nối cơ sở dữ liệu **Microsoft SQL Server (`BangChamCongDB`)** qua **Dapper ORM**. Hệ thống tự động hóa toàn diện từ khâu điểm danh hàng ngày đến khâu quyết toán lương và bảo vệ dữ liệu tài chính bất biến bằng cơ chế khóa sổ kỳ lương (Read-Only Lock).

---

## 🧮 2. BỘ CÔNG THỨC NGHIỆP VỤ TÍNH LƯƠNG CHUẨN DOANH NGHIỆP

Động cơ tính toán lương (`PayrollCalculationService`) áp dụng bộ công thức chuẩn hóa theo quy định của Luật Lao động và Bảo hiểm:

1. **Lương ngày công thực tế theo tỷ lệ:**
   $$\text{Lương ngày công} = \text{Lương cơ bản} \times \frac{\text{Ngày công thực tế}}{\text{Ngày công chuẩn}}$$
2. **Tổng thu nhập gộp (Gross Salary):**
   $$\text{Gross} = \text{Lương ngày công} + \text{Phụ cấp ăn trưa} + \text{Phụ cấp xăng xe}$$
3. **Các khoản trích nộp bảo hiểm bắt buộc của người lao động (10.5% lương cơ bản):**
   * $\text{BHXH (Bảo hiểm Xã hội)} = \text{Lương cơ bản} \times 8.0\%$
   * $\text{BHYT (Bảo hiểm Y tế)} = \text{Lương cơ bản} \times 1.5\%$
   * $\text{BHTN (Bảo hiểm Thất nghiệp)} = \text{Lương cơ bản} \times 1.0\%$
   $$\text{Tổng khấu trừ bảo hiểm} = \text{BHXH} + \text{BHYT} + \text{BHTN} = \text{Lương cơ bản} \times 10.5\%$$
4. **Thu nhập thực lĩnh chuyển khoản (Net Salary):**
   $$\text{Thực lĩnh (Net)} = \text{Gross} - \text{Tổng khấu trừ bảo hiểm}$$

*Ví dụ thực tế với Nhân viên Nguyễn Văn A (Mã `EMP001`):*
* Lương cơ bản: **10.000.000 đ** | Phụ cấp ăn trưa: **500.000 đ** | Phụ cấp xăng: **0 đ**.
* Ngày công chuẩn: **22 ngày** | Ngày công thực tế đi làm: **20 ngày** (2 ngày nghỉ việc riêng có phép không lương).
* Lương ngày công: $10.000.000 \times \frac{20}{22} \approx \mathbf{9.090.909\text{ đ}}$.
* Thu nhập Gross: $9.090.909 + 500.000 = \mathbf{9.590.909\text{ đ}}$.
* Khấu trừ bảo hiểm 10.5%: $800.000\text{ (BHXH)} + 150.000\text{ (BHYT)} + 100.000\text{ (BHTN)} = \mathbf{1.050.000\text{ đ}}$.
* **Thực lĩnh Net:** $9.590.909 - 1.050.000 = \mathbf{8.540.909\text{ đ}}$ *(Khớp chính xác 100% trong CSDL và giao diện Web)*.

---

## 📊 3. TOÀN BỘ 13 SƠ ĐỒ UML CHUẨN MERMAID (ĐỒNG BỘ ĐĂNG NHẬP & PHÂN QUYỀN)

---

### SƠ ĐỒ 01: Sơ Đồ Ca Sử Dụng Tổng Quan Hệ Thống & Phân Quyền (System RBAC Overview Use Case Diagram)

```mermaid
flowchart TD
    subgraph AuthBoundary["Xác Thực Tài Khoản & Quản Lý Phiên"]
        UC01(["UC01: Đăng Nhập Hệ Thống"])
        UC02(["UC02: Xác Thực BCrypt & Cấp Quyền"])
        UC03(["UC03: Đổi Mật Khẩu & Đăng Xuất"])
        UC01 -->|include| UC02
    end

    subgraph Roles["Phân Quyền Vai Trò (RBAC)"]
        R_EMP["Vai Trò: ROLE_EMPLOYEE"]
        R_HR["Vai Trò: ROLE_HR_ACCOUNTANT"]
        R_ADM["Vai Trò: ROLE_ADMIN"]
    end

    subgraph EmployeePortal["Phân Hệ Cổng Thông Tin Nhân Viên"]
        UC09(["UC09: Điểm danh Chấm công Check-in / Check-out"])
        UC10(["UC10: Tra cứu Bảng công Tháng cá nhân"])
        UC11(["UC11: Nộp Đơn xin Nghỉ phép trực tuyến"])
        UC18(["UC18: Tra cứu Phiếu lương Cá nhân"])
    end

    subgraph HRPortal["Phân Hệ Kế Toán Tiền Lương & Nhân Sự"]
        UC04(["UC04: Quản lý Hồ sơ Nhân sự & Phòng ban"])
        UC06(["UC06: Quản lý Hợp đồng Lao động & Lương"])
        UC12(["UC12: Xét duyệt Đơn xin Nghỉ phép"])
        UC13(["UC13: Khóa Bảng chấm công Tháng"])
        UC14(["UC14: Tính toán Bảng lương Tự động Hàng loạt"])
        UC15(["UC15: Đối soát Quỹ lương Tổng hợp"])
        UC16(["UC16: Chốt kỳ Lương Khóa sổ Read-Only"])
        UC17(["UC17: Xuất File Chuyển khoản Ngân hàng"])
    end

    subgraph AdminPortal["Phân Hệ Quản Trị Hệ Thống"]
        UC07(["UC07: Quản lý Tài khoản & Cấp vai trò"])
        UC08(["UC08: Cấu hình Tỷ lệ Bảo hiểm Động"])
    end

    ActorUser(["Người Dùng"]) --> UC01
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

### SƠ ĐỒ 02: Sơ Đồ Ca Sử Dụng Chi Tiết - Phân Hệ Nhân Viên (Employee Portal Use Case Diagram)

```mermaid
flowchart LR
    actor Emp as "Nhân Viên (ROLE_EMPLOYEE)"

    subgraph ScopeEmp["Cổng Nhân Viên (Yêu cầu Xác thực & Quyền Sở hữu)"]
        Auth(["Xác Thực Tài Khoản Đăng Nhập"])
        CI(["UC09a: Điểm Danh Vào Ca (Check-In)"])
        CO(["UC09b: Điểm Danh Tan Ca (Check-Out)"])
        VT(["UC10: Tra Cứu Nhật Ký Chấm Công Hàng Ngày"])
        LR(["UC11: Tạo Đơn Xin Nghỉ Phép Trực Tuyến"])
        VP(["UC18: Tra Cứu Phiếu Lương Cá Nhân Minh Bạch"])
        DT(["UC19: Xem Giải Thích Chi Tiết Thu Nhập & Bảo Hiểm"])
    end

    Emp --> Auth
    Auth --> CI
    Auth --> CO
    Auth --> VT
    Auth --> LR
    Auth --> VP
    VP -.->|extend| DT
```

---

### SƠ ĐỒ 03: Sơ Đồ Ca Sử Dụng Chi Tiết - Phân Hệ Kế Toán & Quản Trị (HR & Admin Payroll Use Case Diagram)

```mermaid
flowchart LR
    actor HR as "Kế Toán / HR (ROLE_HR_ACCOUNTANT)"
    actor Admin as "Quản Trị Viên (ROLE_ADMIN)"

    subgraph ScopeAdmin["Phân Hệ Quản Trị & Kế Toán Tiền Lương"]
        UC_Emp(["UC04: Quản Lý Hồ Sơ & Phòng Ban"])
        UC_Leave(["UC12: Duyệt / Từ Chối Đơn Nghỉ Phép"])
        UC_LockTS(["UC13: Khóa Bảng Chấm Công Tháng"])
        UC_Batch(["UC14: Chạy Tính Lương Tự Động Toàn Doanh Nghiệp"])
        UC_Preview(["UC15: Đối Soát Bảng Lương Tổng Hợp"])
        UC_LockPR(["UC16: Chốt Kỳ Lương Khóa Sổ (Read-Only)"])
        UC_Bank(["UC17: Xuất File Ủy Nhiệm Chi Ngân Hàng"])
        UC_User(["UC07: Quản Lý Người Dùng & Phân Quyền"])
        UC_Rate(["UC08: Cấu Hình Tỷ Lệ Bảo Hiểm Động"])
    end

    HR --> UC_Emp
    HR --> UC_Leave
    HR --> UC_LockTS
    HR --> UC_Batch
    HR --> UC_Preview
    HR --> UC_LockPR
    HR --> UC_Bank

    Admin --> UC_User
    Admin --> UC_Rate
    Admin --> UC_LockPR

    UC_Batch -->|Bắt buộc| UC_Preview
    UC_Preview -->|Điều kiện tiên quyết| UC_LockPR
    UC_LockPR -->|Kích hoạt| UC_Bank
```

---

### SƠ ĐỒ 04: Sơ Đồ Hoạt Động - Quy Trình Xác Thực Đăng Nhập & Phân Quyền (Login & RBAC Activity Diagram)

```mermaid
flowchart TD
    start([Bắt đầu: Mở ứng dụng Web]) --> NavLogin[Truy cập đường dẫn /login]
    NavLogin --> ChooseMode{Chọn cách thức đăng nhập?}
    
    ChooseMode -- 1-Click Demo --> ClickDemo[Nhấp chọn tài khoản mẫu: nhanvien.a / ketoan.thao / admin.trong]
    ChooseMode -- Nhập Form --> InputCreds[Nhập Username và Mật khẩu vào form]

    ClickDemo --> Submit[Gửi yêu cầu xác thực tới LoginUseCase]
    InputCreds --> Submit

    Submit --> QueryDB[(Truy vấn CSDL BangChamCongDB: user_accounts)]
    QueryDB --> UserExist{Tài khoản tồn tại và đang hoạt động?}

    UserExist -- Không --> FailMsg[Hiển thị thông báo: Tên đăng nhập không chính xác hoặc bị khóa]
    FailMsg --> NavLogin

    UserExist -- Có --> VerifyPass{Mật khẩu khớp với BCrypt Hash?}
    VerifyPass -- Không --> FailPass[Hiển thị thông báo: Sai mật khẩu!]
    FailPass --> NavLogin

    VerifyPass -- Đúng --> CreatePrincipal[Khởi tạo ClaimsPrincipal: Id, Username, GivenName, Role, EmployeeId]
    CreatePrincipal --> SetAuthState[Cập nhật CustomAuthenticationStateProvider & LastLoginAt]
    SetAuthState --> RouteRole{Kiểm tra vai trò người dùng?}

    RouteRole -- ROLE_EMPLOYEE --> GoEmp[Chuyển hướng tới Cổng nhân viên: /my-timesheet]
    RouteRole -- ROLE_HR_ACCOUNTANT hoặc ROLE_ADMIN --> GoHR[Chuyển hướng tới Cổng quản trị: /admin/payroll-calculation]

    GoEmp --> finish([Người dùng bắt đầu thao tác theo quyền hạn])
    GoHR --> finish
```

---

### SƠ ĐỒ 05: Sơ Đồ Hoạt Động - Quy Trình Điểm Danh Chấm Công & Nghỉ Phép (Timesheet & Leave Activity Diagram)

```mermaid
flowchart TD
    start([Bắt đầu ngày làm việc]) --> LoginEmp[Nhân viên đăng nhập tài khoản ROLE_EMPLOYEE]
    LoginEmp --> CheckIn[Bấm Check-In Vào Ca: Ghi nhận thời gian thực tế]
    CheckIn --> Work[Làm việc theo ca chuẩn 8.0 giờ]
    Work --> CheckOut[Bấm Check-Out Tan Ca: Hệ thống tự động tính số giờ làm]

    CheckOut --> HasLeave{Có nhu cầu nghỉ phép / việc riêng?}
    HasLeave -- Không --> NormalWork[Hệ thống tự động ghi nhận 1.0 công chuẩn]
    HasLeave -- Có --> SubmitLeave[Nhân viên tạo Đơn xin nghỉ phép trực tuyến]

    SubmitLeave --> LoginHR[Kế toán / HR đăng nhập tài khoản ROLE_HR_ACCOUNTANT]
    LoginHR --> ReviewLeave{Kiểm tra lý do & số ngày phép hợp lệ?}
    ReviewLeave -- Từ chối --> Reject[HR từ chối đơn: Ghi nhận công vắng trừ lương]
    ReviewLeave -- Phê duyệt --> Approve[HR duyệt đơn: Cập nhật chế độ phép có/không lương]

    NormalWork --> MonthEnd[Cuối tháng: HR rà soát bảng công tháng]
    Reject --> MonthEnd
    Approve --> MonthEnd

    MonthEnd --> LockTS[HR bấm Khóa Bảng Công Tháng: is_locked = true]
    LockTS --> finish([Sẵn sàng cho quy trình tính toán lương])
```

---

### SƠ ĐỒ 06: Sơ Đồ Hoạt Động - Quy Trình Tự Động Hóa Tính Lương & Quyết Toán (Payroll Settlement Activity Diagram)

```mermaid
flowchart TD
    start([Bắt đầu chu kỳ tính lương cuối tháng]) --> LoginHR[Kế toán đăng nhập quyền ROLE_HR_ACCOUNTANT]
    LoginHR --> OpenPeriod[Mở kỳ tính lương tháng 10/2026]
    OpenPeriod --> RunBatch[Nhấn 'Chạy Tính Lương Tự Động' 1-chạm]

    RunBatch --> OpenTx[Hệ thống mở Database Transaction an toàn]
    OpenTx --> ReadData[Nạp dữ liệu Hợp đồng, Ngày công đã khóa & Tỷ lệ trích nộp 10.5%]
    ReadData --> Compute[Tính toán theo công thức: Lương công + Phụ cấp - Khấu trừ bảo hiểm]
    Compute --> CommitTx[Lưu vào bảng payslips & payslip_lines -> Commit Transaction]
    CommitTx --> StatusCalculated[Chuyển trạng thái kỳ sang CALCULATED]

    StatusCalculated --> Preview[Kế toán mở màn hình Bảng lương tổng hợp đối soát]
    Preview --> Validate{Số liệu quỹ lương gộp và thực lĩnh chính xác 100%?}
    
    Validate -- Cần chỉnh sửa --> Adjust[Cập nhật lại ngày công hoặc phụ cấp]
    Adjust --> RunBatch

    Validate -- Hoàn toàn chính xác --> LockPeriod[Bấm 'Chốt Kỳ Lương Khóa Sổ']
    LockPeriod --> ReadOnly[Chuyển kỳ sang LOCKED: Dữ liệu chuyển sang CHỈ ĐỌC vĩnh viễn]
    ReadOnly --> ExportBank[Hệ thống xuất danh sách ủy nhiệm chi ngân hàng]
    ExportBank --> BankTransfer[Chuyển tiền qua Ngân hàng vào tài khoản nhân viên]
    BankTransfer --> MarkPaid[Chuyển trạng thái kỳ sang PAID]

    MarkPaid --> EmpNotify[Thông báo tới toàn thể nhân viên]
    EmpNotify --> EmpLogin[Nhân viên đăng nhập quyền ROLE_EMPLOYEE tra cứu phiếu lương]
    EmpLogin --> finish([Hoàn tất chu kỳ tài chính lương])
```

---

### SƠ ĐỒ 07: Sơ Đồ Tuần Tự - Xác Thực Đăng Nhập & Cấp Phiên (Authentication & Session Sequence Diagram)

```mermaid
sequenceDiagram
    autonumber
    actor User as Người Dùng (NV / Kế Toán)
    participant UI as LoginPage.razor
    participant LoginUC as LoginUseCase
    participant UserRepo as UserAccountRepository
    participant DB as SQL Server [user_accounts]
    participant AuthProvider as CustomAuthenticationStateProvider

    User->>UI: Chọn tài khoản demo hoặc nhập Username/Password
    UI->>LoginUC: ExecuteAsync(username, password)
    LoginUC->>UserRepo: AuthenticateAsync(username, password)
    UserRepo->>DB: SELECT * FROM user_accounts WHERE username = @u AND is_active = 1
    DB-->>UserRepo: Trả về UserAccount (PasswordHash, Role, EmployeeId)
    
    UserRepo->>UserRepo: So khớp BCrypt.Verify(password, password_hash)
    
    alt Sai mật khẩu hoặc tài khoản bị khóa
        UserRepo-->>LoginUC: Trả về null
        LoginUC-->>UI: null
        UI-->>User: Hiển thị thông báo lỗi màu đỏ
    else Xác thực thành công
        UserRepo->>DB: UPDATE user_accounts SET last_login_at = GETDATE()
        UserRepo-->>LoginUC: Trả về UserAccount entity
        LoginUC-->>UI: Trả về đối tượng UserAccount
        UI->>AuthProvider: SignInAsync(user)
        AuthProvider->>AuthProvider: Tạo ClaimsPrincipal (Name, Role, GivenName, EmployeeId)
        AuthProvider-->>UI: Cập nhật AuthenticationState thành công
        alt Role == 'ROLE_EMPLOYEE'
            UI-->>User: Điều hướng tới /my-timesheet
        else Role == 'ROLE_HR_ACCOUNTANT' hoặc 'ROLE_ADMIN'
            UI-->>User: Điều hướng tới /admin/payroll-calculation
        end
    end
```

---

### SƠ ĐỒ 08: Sơ Đồ Tuần Tự - Điểm Danh Chấm Công Check-In / Check-Out Trực Tuyến (Daily Attendance Sequence Diagram)

```mermaid
sequenceDiagram
    autonumber
    actor Emp as Nhân Viên (ROLE_EMPLOYEE)
    participant UI as MyTimesheetComponent.razor
    participant AuthState as CascadingParameter AuthState
    participant CheckInUC as CheckInCheckOutUseCase
    participant StateStore as TimesheetStateStore
    participant TRepo as TimesheetRepository
    participant DB as SQL Server [timesheets]

    Emp->>UI: Bấm nút "Check-In (Vào Ca)"
    UI->>AuthState: Lấy Claim EmployeeId của người dùng hiện tại
    AuthState-->>UI: EmployeeId = 1 (Nguyễn Văn A)
    UI->>CheckInUC: ExecuteCheckInAsync(employeeId = 1)
    CheckInUC->>TRepo: GetTimesheetByDateAsync(1, Today)
    TRepo->>DB: SELECT * FROM timesheets WHERE employee_id = 1 AND work_date = @Today
    DB-->>TRepo: Trả về null (Chưa có bản ghi điểm danh hôm nay)
    
    CheckInUC->>TRepo: AddOrUpdateTimesheetAsync(new Timesheet { CheckInTime = Now, Status = 'PRESENT' })
    TRepo->>DB: INSERT INTO timesheets (employee_id, work_date, check_in_time, status) VALUES (...)
    DB-->>TRepo: Thành công
    CheckInUC->>StateStore: NotifyTimesheetUpdated()
    CheckInUC-->>UI: Trả về bản ghi Timesheet vừa tạo
    UI-->>Emp: Thông báo xanh: "Check-in thành công lúc HH:mm:ss!" & Cập nhật bảng công
```

---

### SƠ ĐỒ 09: Sơ Đồ Tuần Tự - Kế Toán Tính Lương Hàng Loạt & Khóa Sổ Read-Only (Payroll Batch & Lock Sequence Diagram)

```mermaid
sequenceDiagram
    autonumber
    actor HR as Kế Toán / HR (ROLE_HR_ACCOUNTANT)
    participant UI as PayrollBatchCalculationPage.razor
    participant Guard as [Authorize(Roles = "ROLE_HR_ACCOUNTANT, ROLE_ADMIN")]
    participant CalcBatchUC as CalculatePayrollBatchUseCase
    participant Engine as PayrollCalculationService
    participant LockUC as LockPayrollPeriodUseCase
    participant DB as SQL Server (BangChamCongDB)

    HR->>UI: Bấm nút "Chạy Tính Lương Tự Động"
    UI->>Guard: Kiểm tra vai trò của người dùng
    Guard-->>UI: Quyền hợp lệ
    UI->>CalcBatchUC: ExecuteAsync(periodId = 10)
    CalcBatchUC->>DB: BEGIN TRANSACTION (Mức cô lập ACID)
    CalcBatchUC->>DB: Lấy danh sách Nhân viên, Hợp đồng lương, Bảng công & Tỷ lệ bảo hiểm 10.5%
    
    loop Duyệt từng nhân sự công ty (1..N)
        CalcBatchUC->>Engine: CalculatePayslip(emp, actualDays, standardDays, rates)
        Engine->>Engine: Lương ngày công = BaseSalary * (actualDays / standardDays)
        Engine->>Engine: Gross = Lương ngày công + Phụ cấp ăn/xăng
        Engine->>Engine: Khấu trừ BHXH (8%), BHYT (1.5%), BHTN (1%)
        Engine->>Engine: Net = Gross - Tổng bảo hiểm
        Engine-->>CalcBatchUC: Trả về Payslip và chi tiết PayslipLines
    end

    CalcBatchUC->>DB: Lưu hàng loạt vào bảng [payslips] & [payslip_lines]
    CalcBatchUC->>DB: UPDATE payroll_periods SET status = 'CALCULATED'
    CalcBatchUC->>DB: COMMIT TRANSACTION (An toàn 100%)
    CalcBatchUC-->>UI: Danh sách bảng lương tổng hợp
    UI-->>HR: Hiển thị bảng đối soát quỹ lương chi tiết

    HR->>UI: Bấm "Chốt Kỳ Lương (Khóa Sổ)"
    UI->>LockUC: ExecuteAsync(periodId = 10, lockedBy = "ketoan.thao")
    LockUC->>DB: UPDATE payroll_periods SET status = 'LOCKED', locked_at = GETDATE()
    LockUC->>DB: UPDATE timesheets SET is_locked = 1 WHERE period_id = 10
    DB-->>LockUC: Thành công
    LockUC-->>UI: Hoàn tất chốt kỳ
    UI-->>HR: Giao diện chuyển sang trạng thái ĐÃ KHÓA SỔ (CHỈ ĐỌC - READ ONLY)
```

---

### SƠ ĐỒ 10: Sơ Đồ Tuần Tự - Tra Cứu Phiếu Lương Cá Nhân Bảo Mật Chống Lộ IDOR (My Payslip Ownership Security Sequence Diagram)

```mermaid
sequenceDiagram
    autonumber
    actor Emp as Nhân Viên (Nguyễn Văn A)
    participant UI as MyPayslipComponent.razor
    participant AuthState as CascadingParameter AuthState
    participant ViewPayslipUC as ViewMyPayslipUseCase
    participant DB as SQL Server (BangChamCongDB)

    Emp->>UI: Truy cập trang "/my-payslip"
    UI->>AuthState: Lấy Claim "EmployeeId" từ phiên đăng nhập an toàn
    AuthState-->>UI: EmployeeId = 1 (Thuộc sở hữu của Nguyễn Văn A)
    
    note over UI: Cơ chế Ownership-based Security đảm bảo nhân viên chỉ được<br/>truy vấn đúng EmployeeId của mình, ngăn chặn 100% tấn công IDOR xem lén lương.
    
    UI->>ViewPayslipUC: ExecuteAsync(periodId = 10, employeeId = 1)
    ViewPayslipUC->>DB: SELECT * FROM payslips WHERE period_id = 10 AND employee_id = 1
    ViewPayslipUC->>DB: SELECT * FROM payslip_lines WHERE payslip_id = 101
    DB-->>ViewPayslipUC: Dữ liệu phiếu lương của nhân viên A (Thực lĩnh: 8.540.909 đ)
    ViewPayslipUC-->>UI: Đối tượng Payslip kèm danh sách chi tiết các dòng lương
    UI-->>Emp: Render thẻ đồ họa PayslipDetailCard: Lương ngày công, phụ cấp ăn trưa, trừ bảo hiểm, thực lĩnh
```

---

### SƠ ĐỒ 11: Sơ Đồ Lớp Miền Nghiệp Vụ & Phân Quyền (Domain Entities & Security Class Diagram)

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
        +string Role
        +bool IsActive
        +DateTime LastLoginAt
        +string FullName
        +string EmployeeCode
        +string DepartmentName
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
        +string Note
    }

    class LeaveRequest {
        +long Id
        +long EmployeeId
        +LeaveType LeaveType
        +DateTime StartDate
        +DateTime EndDate
        +decimal TotalDays
        +string Reason
        +string ApprovalStatus
        +string ApprovedBy
    }

    class DeductionRate {
        +long Id
        +string RateCode
        +string RateName
        +decimal EmployeeRate
        +decimal EmployerRate
        +DateTime EffectiveFrom
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
    Employee "1" <--> "1" UserAccount : tài khoản đăng nhập
    UserAccount ..> UserRole : kiểm soát vai trò
    Employee "1" --> "0..*" Contract : ký kết hợp đồng >
    Employee "1" --> "0..*" Timesheet : ghi nhận chấm công >
    Employee "1" --> "0..*" LeaveRequest : nộp đơn xin nghỉ >
    Employee "1" --> "0..*" Payslip : nhận phiếu lương >
    PayrollPeriod "1" --> "0..*" Payslip : chứa các phiếu lương >
    Payslip "1" *-- "1..*" PayslipLine : cấu thành từ >
```

---

### SƠ ĐỒ 12: Sơ Đồ Kiến Trúc Thành Phần Hệ Thống Clean Architecture (System Component Diagram)

```mermaid
flowchart TB
    subgraph PresentationTier["1. Tầng Trình Diễn (Presentation - Blazor Interactive Server)"]
        Host["TimesheetPayroll.Web<br/>(Host App, SignalR WebSocket, Program.cs)"]
        AuthComp["CustomAuthenticationStateProvider<br/>(Quản lý phiên đăng nhập, Claims & RBAC)"]
        ModAdmin["TimesheetPayroll.Web.AdminPortal<br/>(PayrollBatchCalculationPage, SummaryControls)"]
        ModEmp["TimesheetPayroll.Web.EmployeePortal<br/>(MyTimesheet, MyPayslip, LeaveRequest)"]
        ModCommon["TimesheetPayroll.Web.Common<br/>(SearchBar, StatusBadge, Controls)"]
    end

    subgraph UseCasesTier["2. Tầng Trường Hợp Sử Dụng (Use Cases)"]
        UCAdmin["AdminPortal Use Cases<br/>(CalculatePayrollBatch, LockPeriod, ProcessLeave)"]
        UCEmp["EmployeePortal Use Cases<br/>(ViewMyPayslip, CheckInCheckOut, ViewTimesheet)"]
        UCAuth["Authentication Use Cases<br/>(LoginUseCase, GetUserAccountsUseCase)"]
        PluginIntf["PluginInterfaces.DataStore<br/>(IRepositories: User, Employee, Timesheet, Payslip)"]
    end

    subgraph CoreBusinessTier["3. Tầng Miền Nghiệp Vụ Cốt Lõi (Core Business)"]
        DomainModels["Domain Entities<br/>(Employee, UserAccount, Timesheet, Payslip, PayrollPeriod)"]
        CalcEngine["PayrollCalculationService<br/>(IPayrollCalculationService: Động cơ tính lương chuẩn)"]
    end

    subgraph InfrastructureTier["4. Tầng Hạ Tầng Kỹ Thuật & CSDL (Infrastructure Plugins)"]
        DapperRepo["TimesheetPayroll.DataStore.SQL.Dapper<br/>(UserAccountRepository, PayslipRepo, TimesheetRepo)"]
        StateStore["TimesheetPayroll.StateStore.DI<br/>(TimesheetStateStore)"]
        HandCoded["TimesheetPayroll.DataStore.HandCoded<br/>(Mock Data Testing)"]
        SQLDB[("Microsoft SQL Server Instance<br/>Database: BangChamCongDB (10 Bảng)")]
    end

    Host --> AuthComp
    Host --> ModAdmin
    Host --> ModEmp
    Host --> ModCommon

    ModAdmin --> UCAdmin
    ModEmp --> UCEmp
    AuthComp --> UCAuth
    ModCommon --> DomainModels

    UCAdmin --> PluginIntf
    UCAdmin --> CalcEngine
    UCEmp --> PluginIntf
    UCAuth --> PluginIntf

    PluginIntf --> DomainModels
    CalcEngine --> DomainModels

    DapperRepo ..|> PluginIntf
    HandCoded ..|> PluginIntf
    StateStore --> DomainModels
    DapperRepo --> SQLDB
```

---

### SƠ ĐỒ 13: Sơ Đồ Quan Hệ Thực Thể CSDL 10 Bảng Chuẩn Hóa (Entity Relationship Diagram - ERD)

```mermaid
erDiagram
    departments ||--o{ employees : "thuộc về"
    employees ||--|| user_accounts : "định danh tài khoản"
    employees ||--o{ contracts : "ký kết hợp đồng"
    employees ||--o{ timesheets : "chấm công hàng ngày"
    employees ||--o{ leave_requests : "nộp đơn xin nghỉ"
    employees ||--o{ payslips : "nhận phiếu lương"
    payroll_periods ||--o{ timesheets : "gom nhóm công kỳ"
    payroll_periods ||--o{ payslips : "chứa bảng lương kỳ"
    payslips ||--|{ payslip_lines : "chi tiết các dòng thu nhập"

    departments {
        bigint id PK
        nvarchar department_code UK
        nvarchar department_name
        nvarchar manager_name
        nvarchar description
    }

    employees {
        bigint id PK
        nvarchar employee_code UK
        nvarchar full_name
        date date_of_birth
        nvarchar gender
        nvarchar identity_card_number UK
        nvarchar email UK
        nvarchar phone_number
        nvarchar bank_account_number
        nvarchar bank_name
        bigint department_id FK
        date hire_date
        nvarchar status
    }

    user_accounts {
        bigint id PK
        bigint employee_id FK,UK
        nvarchar username UK
        nvarchar password_hash
        nvarchar role "ROLE_EMPLOYEE | ROLE_HR_ACCOUNTANT | ROLE_ADMIN"
        bit is_active
        datetime2 last_login_at
    }

    contracts {
        bigint id PK
        bigint employee_id FK
        nvarchar contract_number UK
        nvarchar contract_type "PROBATION | FULL_TIME | INDEFINITE"
        decimal base_salary
        decimal meal_allowance
        decimal fuel_allowance
        date start_date
        date end_date
        bit is_active
    }

    payroll_periods {
        bigint id PK
        nvarchar period_code UK
        int period_month
        int period_year
        decimal standard_work_days
        nvarchar status "DRAFT | CALCULATING | CALCULATED | LOCKED | PAID"
        decimal total_gross_amount
        decimal total_net_amount
        datetime2 locked_at
        nvarchar locked_by
    }

    deduction_rates {
        bigint id PK
        nvarchar rate_code UK
        nvarchar rate_name
        decimal employee_rate "Ví dụ: 0.08 BHXH, 0.015 BHYT, 0.01 BHTN"
        decimal employer_rate "Ví dụ: 0.175 BHXH, 0.03 BHYT, 0.01 BHTN"
        date effective_from
        bit is_active
    }

    timesheets {
        bigint id PK
        bigint employee_id FK
        bigint period_id FK
        date work_date
        time check_in_time
        time check_out_time
        decimal actual_hours_worked
        nvarchar work_shift_status "PRESENT | LATE | EARLY_LEAVE | ABSENT"
        decimal work_units "1.0 công hoặc 0.0 công"
        bit is_locked
    }

    leave_requests {
        bigint id PK
        bigint employee_id FK
        nvarchar leave_type "ANNUAL_LEAVE | SICK_LEAVE | UNPAID_LEAVE"
        date start_date
        date end_date
        decimal total_days
        nvarchar reason
        nvarchar approval_status "PENDING | APPROVED | REJECTED"
        nvarchar approved_by
    }

    payslips {
        bigint id PK
        bigint period_id FK
        bigint employee_id FK
        decimal actual_work_days
        decimal paid_leave_days
        decimal overtime_hours
        decimal gross_salary
        decimal total_deductions
        decimal net_salary
    }

    payslip_lines {
        bigint id PK
        bigint payslip_id FK
        nvarchar line_type "BASE_SALARY_PRORATED | MEAL_ALLOWANCE | DEDUCTION_BHXH..."
        nvarchar line_description
        decimal amount
        bit is_deduction
    }
```

---

## 🏗️ 4. CẤU TRÚC THƯ MỤC MÃ NGUỒN CLEAN ARCHITECTURE

```text
TimesheetPayroll/
│
├── TimesheetPayroll.slnx                                   # Visual Studio Solution File
│
├── TimesheetPayroll.CoreBusiness/                          # TẦNG MIỀN NGHIỆP VỤ CỐT LÕI (CORE BUSINESS)
│   ├── Models/
│   │   ├── Employee.cs                                     # Thực thể Nhân viên
│   │   ├── Department.cs                                   # Thực thể Phòng ban
│   │   ├── UserAccount.cs                                  # Thực thể Tài khoản người dùng & Phân quyền
│   │   ├── Timesheet.cs                                    # Thực thể Bảng chấm công
│   │   ├── PayrollPeriod.cs                                # Thực thể Kỳ lương
│   │   ├── Payslip.cs                                      # Thực thể Phiếu lương & Dòng chi tiết
│   │   ├── LeaveRequest.cs                                 # Thực thể Đơn nghỉ phép
│   │   ├── DeductionRate.cs                                # Thực thể Tỷ lệ bảo hiểm
│   │   └── Enums.cs                                        # Kiểu liệt kê (UserRole, WorkShiftStatus,...)
│   └── Services/
│       ├── Interfaces/IPayrollCalculationService.cs        # Interface động cơ tính toán lương
│       └── PayrollCalculationService.cs                    # Triển khai thuật toán tính lương luật định
│
├── TimesheetPayroll.UseCases/                              # TẦNG TRƯỜNG HỢP SỬ DỤNG (USE CASES)
│   ├── Authentication/                                     # Ca sử dụng xác thực người dùng
│   │   └── LoginUseCase.cs                                 # Đăng nhập, kiểm tra mật khẩu BCrypt, lấy DS User
│   ├── AdminPortal/                                        # Ca sử dụng phân hệ Kế toán & Quản trị
│   │   └── AdminPortalUseCases.cs                          # Tính lương hàng loạt, Khóa sổ, Duyệt phép
│   ├── EmployeePortal/                                     # Ca sử dụng phân hệ Nhân viên
│   │   └── EmployeePortalUseCases.cs                       # Xem phiếu lương, Điểm danh công, Nộp đơn phép
│   └── PluginInterfaces/                                   # Hợp đồng giao tiếp tầng ngoài (Repository Interfaces)
│       └── DataStore/IRepositories.cs                      # IUserAccountRepository, IEmployeeRepository,...
│
├── TimesheetPayroll.Web.Modules/                           # CÁC MODULE GIAO DIỆN BLAZOR ĐỘC LẬP
│   ├── TimesheetPayroll.Web.AdminPortal/                   # Cổng Kế toán & Quản trị Lương
│   │   ├── Pages/PayrollBatchCalculationPage.razor         # Trang tính lương tự động & Chốt sổ Read-Only
│   │   └── Controls/PayrollSummaryComponent.razor          # Bảng đối soát lương chi tiết
│   ├── TimesheetPayroll.Web.EmployeePortal/                # Cổng Thông tin Nhân viên Cá nhân
│   │   ├── Pages/MyPayslipComponent.razor                  # Trang xem phiếu lương cá nhân (Anti-IDOR)
│   │   ├── Pages/MyTimesheetComponent.razor                # Trang điểm danh Check-In/Check-Out
│   │   ├── Pages/LeaveRequestComponent.razor               # Trang nộp đơn xin nghỉ phép trực tuyến
│   │   └── Controls/PayslipDetailCard.razor                # Thẻ đồ họa trực quan thu nhập & bảo hiểm
│   └── TimesheetPayroll.Web.Common/                        # Thành phần tái sử dụng chung
│       └── Controls/SearchBarComponent.razor, StatusBadgeComponent.razor
│
├── Plugins/                                                # TẦNG HẠ TẦNG KỸ THUẬT & TRUY XUẤT CSDL
│   ├── TimesheetPayroll.DataStore.SQL.Dapper/              # Kết nối SQL Server qua Dapper Micro-ORM
│   │   ├── DataAccess.cs                                   # Quản lý kết nối SqlConnection
│   │   └── SqlRepositories.cs                              # Triển khai các Repositories với Dapper & BCrypt
│   ├── TimesheetPayroll.DataStore.HandCoded/               # Dữ liệu giả lập chạy thử nghiệm offline
│   └── TimesheetPayroll.StateStore.DI/                     # Quản lý trạng thái SignalR StateStore
│
└── TimesheetPayroll.Web/                                   # ỨNG DỤNG HOST BLAZOR WEB APP (.NET 10)
    ├── Components/
    │   ├── App.razor, Routes.razor                         # Cấu hình AuthorizeRouteView bảo vệ phân quyền
    │   ├── Layout/TopNavbar.razor, MainLayout.razor        # Thanh điều hướng phân quyền & Đăng xuất
    │   └── Pages/
    │       ├── Home.razor                                  # Trang chủ giới thiệu hệ thống & đồ án
    │       └── LoginPage.razor                             # Trang đăng nhập hỗ trợ 1-Click Demo & Form SQL
    ├── CustomAuthenticationStateProvider.cs                # Triển khai AuthenticationStateProvider Blazor
    ├── Program.cs                                          # Đăng ký Dependency Injection & Middleware
    └── appsettings.json                                    # Cấu hình kết nối tới BangChamCongDB
```

---

## 🚀 5. HƯỚNG DẪN CÀI ĐẶT & KHỞI CHẠY HỆ THỐNG

### Bước 1: Khởi tạo Cơ sở dữ liệu SQL Server
1. Mở **SQL Server Management Studio (SSMS)** và kết nối tới máy chủ (ví dụ: `.\SQLEXPRESS`).
2. Mở tệp script **`database_schema.sql`** (hoặc `database_mssql.sql`) tại thư mục gốc của dự án.
3. Bấm **Execute (F5)** để tự động tạo CSDL riêng biệt **`BangChamCongDB`**, tạo đầy đủ 10 bảng dữ liệu chuẩn và nạp sẵn 4 tài khoản người dùng mẫu cùng dữ liệu chấm công.

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

### Bước 3: Biên dịch và chạy ứng dụng Blazor Server
Mở terminal (PowerShell hoặc Command Prompt) tại thư mục dự án:

```powershell
# 1. Di chuyển vào thư mục mã nguồn
cd project/TimesheetPayroll

# 2. Biên dịch toàn bộ Solution (đảm bảo 0 Warnings, 0 Errors)
dotnet build TimesheetPayroll.slnx

# 3. Khởi chạy ứng dụng Web Host
dotnet run --project TimesheetPayroll.Web
```

---

### Bước 4: Trải nghiệm các tính năng theo phân quyền
Mở trình duyệt truy cập: `https://localhost:5001` (hoặc `http://localhost:5000`):

1. **Truy cập trang Đăng nhập (`/login`):**
   * Sử dụng nút **Đăng nhập nhanh 1-Click** để thử nghiệm tức thì giữa các tài khoản:
     * Bấm **[Nguyễn Văn A]** (`nhanvien.a` / `123456`) để vào vai trò Nhân viên.
     * Bấm **[Lê Thị Thu Thảo]** (`ketoan.thao` / `123456`) để vào vai trò Kế toán.
     * Bấm **[Trần Đình Trọng]** (`admin.trong` / `123456`) để vào vai trò Quản trị viên.
2. **Trải nghiệm phân hệ Nhân viên (`ROLE_EMPLOYEE`):**
   * **Điểm danh (`/my-timesheet`):** Bấm Check-In / Check-Out thời gian thực, xem tổng ngày công tích lũy trong tháng 10/2026.
   * **Nộp đơn xin nghỉ phép (`/leave-request`):** Chọn loại nghỉ phép (phép năm, nghỉ ốm, thai sản, không lương) và gửi đơn.
   * **Tra cứu phiếu lương (`/my-payslip`):** Xem minh bạch phiếu lương của nhân viên đang đăng nhập: Lương công thực tế (20/22 ngày), phụ cấp ăn trưa 500.000đ, trừ 10.5% bảo hiểm (1.050.000đ), **Thực lĩnh chính xác 8.540.909 đ**.
3. **Trải nghiệm phân hệ Kế toán & Quản trị (`ROLE_HR_ACCOUNTANT`):**
   * Menu **"Quản Trị: Tính Lương Hàng Loạt"** sẽ xuất hiện trên thanh điều hướng.
   * **Chạy tính lương (`/admin/payroll-calculation`):** Bấm nút **"Chạy Tính Lương Tự Động"** tính toán đồng loạt cho 100% nhân viên trong công ty qua Database Transaction.
   * **Đối soát quỹ lương:** Rà soát danh sách nhân viên, phòng ban, lương gộp và thực lĩnh.
   * **Chốt sổ kỳ lương:** Bấm nút **"Chốt Kỳ Lương Khóa Sổ"** chuyển vĩnh viễn dữ liệu sang trạng thái **Read-Only** chống sửa đổi và xuất file chuyển khoản ngân hàng.

---

## 📋 6. KẾT LUẬN & ĐÁNH GIÁ KẾT QUẢ ĐẠT ĐƯỢC

* ✅ **Đáp ứng 100% chuẩn học thuật môn học Lập trình Ứng dụng Web:** Đầy đủ trọn bộ **13 sơ đồ UML chuẩn hóa**, kết xuất trực quan bằng cú pháp Mermaid Markdown tương thích hoàn hảo trên GitHub.
* ✅ **Tách biệt hoàn toàn CSDL:** Cơ sở dữ liệu **`BangChamCongDB`** độc lập 100% với các đồ án khác trên máy chủ SQL Server.
* ✅ **Bảo mật xác thực & Phân quyền RBAC thực tế:** Tích hợp màn hình đăng nhập `/login`, băm mật khẩu BCrypt, kiểm soát quyền theo vai trò (`ROLE_EMPLOYEE`, `ROLE_HR_ACCOUNTANT`, `ROLE_ADMIN`) và bảo vệ chống xem lén dữ liệu lương (Anti-IDOR).
* ✅ **Mã nguồn chuẩn Clean Architecture:** Tách lớp độc lập (`CoreBusiness`, `UseCases`, `Plugins`, `Web.Modules`), tuân thủ triệt để các nguyên lý SOLID, sẵn sàng mở rộng quy mô.
* ✅ **Tính toán chính xác & Khóa sổ an toàn:** Thuật toán tính lương bám sát thực tế luật lao động, hỗ trợ cơ chế khóa sổ bất biến đảm bảo tính toàn vẹn dữ liệu tài chính doanh nghiệp.
