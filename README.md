# Timesheet & Payroll System (Hệ Thống Chấm Công & Tính Lương Tự Động)

Dự án phần mềm Chấm công và Tính lương tự động xây dựng trên nền tảng **.NET 10 Blazor Interactive Server** và **Microsoft SQL Server (Dapper ORM)** theo kiến trúc **Clean Architecture** phân tầng độc lập.

---

## 1. Cấu Trúc Mã Nguồn (Source Code)

Toàn bộ mã nguồn nằm tại thư mục `project/TimesheetPayroll/`:

```text
TimesheetPayroll/
├── TimesheetPayroll.slnx                                   # Solution chính
├── TimesheetPayroll.CoreBusiness/                          # Domain Entities & Business Services
│   ├── Models/                                             # Employee, Timesheet, Payslip, PayrollPeriod...
│   └── Services/                                           # PayrollCalculationService (Động cơ tính lương)
├── TimesheetPayroll.UseCases/                              # Clean Architecture Use Cases
│   ├── AdminPortal/                                        # Tính lương hàng loạt, Khóa sổ, Đối soát
│   ├── EmployeePortal/                                     # Xem phiếu lương, Chấm công, Xin nghỉ phép
│   └── PluginInterfaces/                                   # Interfaces cho Repositories & State
├── TimesheetPayroll.Web.Modules/                           # Các module giao diện Blazor
│   ├── TimesheetPayroll.Web.AdminPortal/                   # Cổng Quản trị / Kế toán
│   ├── TimesheetPayroll.Web.EmployeePortal/                # Cổng Nhân viên
│   └── TimesheetPayroll.Web.Common/                        # Điều khiển & thành phần dùng chung
├── Plugins/                                                # Tầng cắm ghép hạ tầng (Infrastructure)
│   ├── TimesheetPayroll.DataStore.SQL.Dapper/              # Kết nối SQL Server qua Dapper
│   ├── TimesheetPayroll.DataStore.HandCoded/               # Mock data in-memory thử nghiệm
│   └── TimesheetPayroll.StateStore.DI/                     # Quản lý phiên làm việc & State
└── TimesheetPayroll.Web/                                   # Host ASP.NET Core Blazor Web App
    ├── Components/                                         # App, Routes, Layouts, Pages
    ├── Program.cs                                          # Dependency Injection & Middleware
    └── appsettings.json                                    # Chuỗi kết nối Database
```

---

## 2. Cơ Sở Dữ Liệu (Database Scripts)

Dự án cung cấp sẵn các tệp DDL tạo bảng và nạp dữ liệu mẫu khởi tạo:

* **`database_schema.sql`** (hoặc **`database_mssql.sql`**):
  * Dành cho **Microsoft SQL Server** (SSMS).
  * Tự động tạo CSDL riêng biệt **`BangChamCongDB`**.
  * Chứa 10 bảng chuẩn hóa bậc 3 (3NF): `departments`, `employees`, `user_accounts`, `contracts`, `payroll_periods`, `deduction_rates`, `timesheets`, `leave_requests`, `payslips`, `payslip_lines`.
  * Có sẵn dữ liệu hạt giống (Seed Data) chuẩn đối soát.
* **`database_postgres.sql`**: Bản script tương ứng dành cho **PostgreSQL 16**.

---

## 3. Hướng Dẫn Khởi Chạy (Getting Started)

### Bước 1: Khởi tạo Cơ sở dữ liệu
Mở SSMS và thực thi tệp `database_schema.sql` vào máy chủ SQL Server của bạn (mặc định instance `.\SQLEXPRESS`).

### Bước 2: Biên dịch và chạy ứng dụng Blazor
```bash
cd project/TimesheetPayroll

# Khôi phục và biên dịch
dotnet build TimesheetPayroll.slnx

# Chạy ứng dụng web
dotnet run --project TimesheetPayroll.Web
```

Mở trình duyệt truy cập: `https://localhost:5001` hoặc `http://localhost:5000`.
