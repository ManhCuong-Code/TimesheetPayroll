/**
 * DỮ LIỆU ĐỒ ÁN HỆ THỐNG QUẢN LÝ NHÂN SỰ – CHẤM CÔNG – TÍNH LƯƠNG
 * Nạp từ cơ sở dữ liệu Microsoft SQL Server (BangChamCongDB)
 */

const HRM_DATA = {
    company: {
        name: "CÔNG TY CỔ PHẦN CÔNG NGHỆ & THƯƠNG MẠI TIMESHEETPAYROLL",
        shortName: "TimesheetPayroll Corp",
        address: "Tòa nhà Innovation Hub, Số 88 Đường Lê Lợi, TP. Huế",
        taxCode: "3301234567",
        standardWorkDays: 22
    },
    
    // 6 Ca làm việc chuẩn hóa
    shifts: [
        {
            id: 1,
            shift_code: "CA_HANH_CHINH",
            shift_name: "Ca Hành Chính (08:00 - 17:30)",
            start_time: "08:00",
            end_time: "17:30",
            break_minutes: 90,
            checkin_start_window_minutes: 60,
            checkin_late_grace_minutes: 15,
            checkout_early_grace_minutes: 15,
            checkout_end_window_minutes: 120,
            standard_hours: 8.0,
            is_overnight: false,
            color: "primary"
        },
        {
            id: 2,
            shift_code: "CA_SANG",
            shift_name: "Ca Sáng (08:00 - 12:00)",
            start_time: "08:00",
            end_time: "12:00",
            break_minutes: 0,
            checkin_start_window_minutes: 45,
            checkin_late_grace_minutes: 15,
            checkout_early_grace_minutes: 10,
            checkout_end_window_minutes: 60,
            standard_hours: 4.0,
            is_overnight: false,
            color: "info"
        },
        {
            id: 3,
            shift_code: "CA_CHIEU",
            shift_name: "Ca Chiều (13:30 - 17:30)",
            start_time: "13:30",
            end_time: "17:30",
            break_minutes: 0,
            checkin_start_window_minutes: 30,
            checkin_late_grace_minutes: 15,
            checkout_early_grace_minutes: 10,
            checkout_end_window_minutes: 90,
            standard_hours: 4.0,
            is_overnight: false,
            color: "warning"
        },
        {
            id: 4,
            shift_code: "CA_TOI",
            shift_name: "Ca Tối (18:00 - 22:00)",
            start_time: "18:00",
            end_time: "22:00",
            break_minutes: 0,
            checkin_start_window_minutes: 30,
            checkin_late_grace_minutes: 15,
            checkout_early_grace_minutes: 10,
            checkout_end_window_minutes: 60,
            standard_hours: 4.0,
            is_overnight: false,
            color: "secondary"
        },
        {
            id: 5,
            shift_code: "CA_DEM",
            shift_name: "Ca Đêm (22:00 - 06:00)",
            start_time: "22:00",
            end_time: "06:00",
            break_minutes: 0,
            checkin_start_window_minutes: 60,
            checkin_late_grace_minutes: 15,
            checkout_early_grace_minutes: 15,
            checkout_end_window_minutes: 120,
            standard_hours: 8.0,
            is_overnight: true,
            color: "dark"
        },
        {
            id: 6,
            shift_code: "CA_GAY",
            shift_name: "Ca Gãy (10:00 - 14:00 & 17:00 - 21:00)",
            start_time: "10:00",
            end_time: "21:00",
            break_minutes: 180,
            checkin_start_window_minutes: 45,
            checkin_late_grace_minutes: 15,
            checkout_early_grace_minutes: 15,
            checkout_end_window_minutes: 60,
            standard_hours: 8.0,
            is_overnight: false,
            color: "danger"
        }
    ],

    // 20 Nhân sự chuẩn hóa từ database
    employees: [
        { id: 1, code: "EMP001", name: "Nguyễn Văn An", department: "Ban Giám Đốc", role: "ROLE_ADMIN", username: "admin.trong", baseSalary: 25000000, lunchAllowance: 1000000, transportAllowance: 1500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 2, code: "EMP002", name: "Trần Thị Bích", department: "Phòng Hành Chính Nhân Sự & Kế Toán", role: "ROLE_HR_ACCOUNTANT", username: "ketoan.thao", baseSalary: 14000000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 21.5, lateHours: 0.5, earlyHours: 0, status: "ACTIVE" },
        { id: 3, code: "EMP003", name: "Lê Hoàng Nam", department: "Phòng Công Nghệ Thông Tin", role: "ROLE_MANAGER", username: "quanly.nam", baseSalary: 18000000, lunchAllowance: 800000, transportAllowance: 600000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 4, code: "EMP004", name: "Hoàng Minh Tuấn", department: "Phòng Kinh Doanh & Tiếp Thị", role: "ROLE_EMPLOYEE", username: "nhanvien.tuan", baseSalary: 11000000, lunchAllowance: 800000, transportAllowance: 800000, workDays: 20, lateHours: 1.0, earlyHours: 0, status: "ACTIVE" },
        { id: 5, code: "EMP005", name: "Phạm Hoàng Nam", department: "Phòng Kinh Doanh & Tiếp Thị", role: "ROLE_EMPLOYEE", username: "nhanvien.a", baseSalary: 10500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 6, code: "EMP006", name: "Đỗ Thị Mai Linh", department: "Phòng Hành Chính Nhân Sự & Kế Toán", role: "ROLE_EMPLOYEE", username: "nhanvien.linh", baseSalary: 12000000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 7, code: "EMP007", name: "Vũ Quốc Anh", department: "Phòng Công Nghệ Thông Tin", role: "ROLE_EMPLOYEE", username: "nhanvien.anh", baseSalary: 13500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 8, code: "EMP008", name: "Bùi Thị Bích Ngọc", department: "Phòng Kinh Doanh & Tiếp Thị", role: "ROLE_MANAGER", username: "quanly.ngoc", baseSalary: 16000000, lunchAllowance: 800000, transportAllowance: 700000, workDays: 21, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 9, code: "EMP009", name: "Đinh Quang Huy", department: "Phòng Vận Hành & Dịch Vụ Khách Hàng", role: "ROLE_EMPLOYEE", username: "nhanvien.huy", baseSalary: 9500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 10, code: "EMP010", name: "Nguyễn Hải Yến", department: "Phòng Kế Toán Quản Trị & Thuế", role: "ROLE_HR_ACCOUNTANT", username: "ketoan.yen", baseSalary: 13000000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 11, code: "EMP011", name: "Phạm Minh Đức", department: "Phòng Kỹ Thuật & Hạ Tầng Thiết Bị", role: "ROLE_EMPLOYEE", username: "nhanvien.duc", baseSalary: 11500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 12, code: "EMP012", name: "Lương Kim Ngân", department: "Phòng Hành Chính Nhân Sự & Kế Toán", role: "ROLE_HR_ACCOUNTANT", username: "ketoan.ngan", baseSalary: 12500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 13, code: "EMP013", name: "Tạ Văn Kiên", department: "Phòng Kinh Doanh & Tiếp Thị", role: "ROLE_EMPLOYEE", username: "nhanvien.kien", baseSalary: 10000000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 20.5, lateHours: 0.5, earlyHours: 0, status: "ACTIVE" },
        { id: 14, code: "EMP014", name: "Hà Phương Thảo", department: "Phòng Vận Hành & Dịch Vụ Khách Hàng", role: "ROLE_MANAGER", username: "quanly.thao", baseSalary: 15500000, lunchAllowance: 800000, transportAllowance: 600000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 15, code: "EMP015", name: "Ngô Thanh Tùng", department: "Phòng Công Nghệ Thông Tin", role: "ROLE_EMPLOYEE", username: "nhanvien.tung", baseSalary: 12500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 16, code: "EMP016", name: "Dương Mỹ Hạnh", department: "Phòng Kinh Doanh & Tiếp Thị", role: "ROLE_EMPLOYEE", username: "nhanvien.hanh", baseSalary: 10500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 21, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 17, code: "EMP017", name: "Trịnh Quốc Bảo", department: "Phòng Kỹ Thuật & Hạ Tầng Thiết Bị", role: "ROLE_MANAGER", username: "quanly.bao", baseSalary: 17000000, lunchAllowance: 800000, transportAllowance: 700000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 18, code: "EMP018", name: "Cao Thu Trang", department: "Phòng Kế Toán Quản Trị & Thuế", role: "ROLE_HR_ACCOUNTANT", username: "ketoan.trang", baseSalary: 13000000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 19, code: "EMP019", name: "Mai Hoàng Long", department: "Phòng Vận Hành & Dịch Vụ Khách Hàng", role: "ROLE_EMPLOYEE", username: "nhanvien.long", baseSalary: 9500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" },
        { id: 20, code: "EMP020", name: "Võ Như Quỳnh", department: "Phòng Công Nghệ Thông Tin", role: "ROLE_EMPLOYEE", username: "nhanvien.quynh", baseSalary: 11500000, lunchAllowance: 800000, transportAllowance: 500000, workDays: 22, lateHours: 0, earlyHours: 0, status: "ACTIVE" }
    ],

    // Đơn xin nghỉ phép mẫu
    leaveRequests: [
        { id: 101, empCode: "EMP004", empName: "Hoàng Minh Tuấn", leaveType: "Phép năm", fromDate: "2026-10-24", toDate: "2026-10-25", days: 2, reason: "Giải quyết việc cá nhân gia đình", status: "PENDING", createdAt: "2026-10-20 09:30" },
        { id: 102, empCode: "EMP013", empName: "Tạ Văn Kiên", leaveType: "Việc riêng", fromDate: "2026-10-28", toDate: "2026-10-28", days: 1, reason: "Khám sức khỏe định kỳ", status: "PENDING", createdAt: "2026-10-21 08:15" },
        { id: 103, empCode: "EMP002", empName: "Trần Thị Bích", leaveType: "Phép năm", fromDate: "2026-10-15", toDate: "2026-10-15", days: 0.5, reason: "Nghỉ việc gia đình buổi sáng", status: "APPROVED", createdAt: "2026-10-14 14:00" },
        { id: 104, empCode: "EMP016", empName: "Dương Mỹ Hạnh", leaveType: "Nghỉ ốm", fromDate: "2026-10-10", toDate: "2026-10-10", days: 1, reason: "Sốt cảm cúm có giấy bác sĩ", status: "APPROVED", createdAt: "2026-10-09 18:20" }
    ],

    // Yêu cầu bổ sung công (quên quẹt thẻ)
    attendanceRequests: [
        { id: 201, empCode: "EMP005", empName: "Phạm Hoàng Nam", date: "2026-10-19", shiftName: "Ca Hành Chính", requestType: "Bổ sung giờ vào (Check-in)", recordedTime: "08:02", reason: "Camera Kiosk bị mất mạng tạm thời lúc vào sảnh", status: "PENDING" },
        { id: 202, empCode: "EMP007", empName: "Vũ Quốc Anh", date: "2026-10-18", shiftName: "Ca Đêm", requestType: "Bổ sung giờ ra (Check-out)", recordedTime: "06:05", reason: "Bàn giao ca trực IT quên quẹt thẻ Kiosk", status: "APPROVED" }
    ]
};

// Hàm tính lương theo công thức luật lao động Việt Nam
function calculateSalaryRecord(emp) {
    const stdDays = HRM_DATA.company.standardWorkDays;
    const actualDays = emp.workDays;
    const dailyWage = emp.baseSalary / stdDays;
    const grossBase = Math.round(dailyWage * actualDays);
    const allowances = emp.lunchAllowance + emp.transportAllowance;
    
    // Thưởng chuyên cần (đi làm đủ 100% công chuẩn)
    const attendanceBonus = actualDays >= stdDays ? 300000 : 0;
    
    const totalIncome = grossBase + allowances + attendanceBonus;
    
    // Khấu trừ bắt buộc (trên lương cơ bản theo luật BHXH)
    const bhxh = Math.round(emp.baseSalary * 0.08);   // 8%
    const bhyt = Math.round(emp.baseSalary * 0.015);  // 1.5%
    const bhtn = Math.round(emp.baseSalary * 0.01);   // 1%
    const totalDeduction = bhxh + bhyt + bhtn;
    
    const netSalary = totalIncome - totalDeduction;

    return {
        empId: emp.id,
        code: emp.code,
        name: emp.name,
        department: emp.department,
        baseSalary: emp.baseSalary,
        workDays: actualDays,
        grossBase: grossBase,
        allowances: allowances,
        attendanceBonus: attendanceBonus,
        totalIncome: totalIncome,
        bhxh: bhxh,
        bhyt: bhyt,
        bhtn: bhtn,
        totalDeduction: totalDeduction,
        netSalary: netSalary
    };
}

// Format tiền tệ VNĐ
function formatVND(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// =========================================================================
// HÀM TẠO DỮ LIỆU CHẤM CÔNG CHI TIẾT THEO THÁNG CHO TỪNG NHÂN VIÊN
// =========================================================================
function getEmployeeTimesheetRecords(empId, month = 10, year = 2026) {
    var emp = HRM_DATA.employees.find(x => x.id === empId) || HRM_DATA.employees[0];
    var daysInMonth = new Date(year, month, 0).getDate();
    var records = [];
    var weekdaysVi = ["Chủ Nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy"];

    var totalHours = 0;
    var totalUnits = 0;
    var lateCount = 0;
    var lateMinutesTotal = 0;
    var paidLeaveCount = 0;

    // Kiểm tra bản ghi điểm danh hôm nay từ Kiosk Face Check-In (nếu có)
    var localPunch = null;
    try {
        var punchStr = localStorage.getItem('hrm_last_punch');
        if (punchStr) {
            var parsed = JSON.parse(punchStr);
            if (parsed.empId === emp.id) localPunch = parsed;
        }
    } catch(e) {}

    for (var d = 1; d <= daysInMonth; d++) {
        var dateObj = new Date(year, month - 1, d);
        var dayOfWeek = dateObj.getDay();
        var dayOfWeekStr = weekdaysVi[dayOfWeek];
        var dateFormatted = `${d.toString().padStart(2, '0')}/${month.toString().padStart(2, '0')}/${year}`;

        var rec = {
            day: d,
            dateStr: `${dateFormatted} (${dayOfWeekStr})`,
            rawDate: dateObj,
            dayOfWeek: dayOfWeek,
            shiftName: "Ca Hành Chính (08:00 - 17:30)",
            shiftCode: "CA_HANH_CHINH",
            checkIn: "--",
            checkOut: "--",
            hours: 0,
            workUnits: 0,
            status: "PRESENT",
            statusLabel: "Hợp lệ",
            badgeClass: "bg-success"
        };

        // 1. Ngày Chủ Nhật: Nghỉ tuần
        if (dayOfWeek === 0) {
            rec.shiftName = "Nghỉ Hàng Tuần (Chủ Nhật)";
            rec.shiftCode = "OFF";
            rec.status = "WEEKEND";
            rec.statusLabel = "Nghỉ tuần";
            rec.badgeClass = "bg-secondary-subtle text-secondary";
            records.push(rec);
            continue;
        }

        // 2. Ngày Thứ 7
        if (dayOfWeek === 6) {
            if ([4, 9, 13, 19].includes(emp.id)) {
                // Ca Gãy dịch vụ
                rec.shiftName = "Ca Gãy (10:00 - 14:00 & 17:00 - 21:00)";
                rec.shiftCode = "CA_GAY";
                if (d < 22) {
                    rec.checkIn = "09:56";
                    rec.checkOut = "21:04";
                    rec.hours = 8.0;
                    rec.workUnits = 1.0;
                    rec.status = "PRESENT";
                    rec.statusLabel = "Hợp lệ (Ca gãy)";
                    rec.badgeClass = "bg-primary";
                }
            } else {
                // Ca Sáng
                rec.shiftName = "Ca Sáng (08:00 - 12:00)";
                rec.shiftCode = "CA_SANG";
                if (d < 22) {
                    rec.checkIn = "07:56";
                    rec.checkOut = "12:05";
                    rec.hours = 4.0;
                    rec.workUnits = 0.5;
                    rec.status = "PRESENT";
                    rec.statusLabel = "Hợp lệ (Nửa ca)";
                    rec.badgeClass = "bg-info text-dark";
                }
            }
        } 
        // 3. Thứ 6 đối với nhân viên kỹ thuật trực ca đêm (EMP011, EMP017)
        else if (dayOfWeek === 5 && [11, 17].includes(emp.id)) {
            rec.shiftName = "Ca Đêm (22:00 - 06:00)";
            rec.shiftCode = "CA_DEM";
            if (d < 22) {
                rec.checkIn = "21:52";
                rec.checkOut = "06:08";
                rec.hours = 8.0;
                rec.workUnits = 1.0;
                rec.status = "PRESENT";
                rec.statusLabel = "Ca đêm (+30%)";
                rec.badgeClass = "bg-dark text-white";
            }
        }
        // 4. Các ngày trong tuần bình thường (Thứ 2 - Thứ 6)
        else {
            rec.shiftName = "Ca Hành Chính (08:00 - 17:30)";
            rec.shiftCode = "CA_HANH_CHINH";

            // Trường hợp đặc biệt 1: EMP002 nghỉ phép nửa buổi sáng ngày 15
            if (emp.id === 2 && d === 15) {
                rec.shiftName = "Ca Chiều (Nghỉ phép sáng)";
                rec.checkIn = "13:28";
                rec.checkOut = "17:32";
                rec.hours = 4.0;
                rec.workUnits = 1.0; // 0.5 công làm + 0.5 phép
                rec.status = "HALF_LEAVE";
                rec.statusLabel = "Nghỉ phép 0.5d";
                rec.badgeClass = "bg-info text-dark";
            }
            // Trường hợp đặc biệt 2: EMP005 (hoặc nhân viên đang chọn) đi muộn ngày 19
            else if ((emp.id === 5 || emp.id === 4) && d === 19) {
                rec.checkIn = "08:25";
                rec.checkOut = "17:30";
                rec.hours = 7.6;
                rec.workUnits = 0.95;
                rec.status = "LATE";
                rec.statusLabel = "Đi muộn 25p";
                rec.badgeClass = "bg-warning text-dark";
                lateCount++;
                lateMinutesTotal += 25;
            }
            // Ngày hiện tại (22/10)
            else if (d === 22) {
                if (localPunch) {
                    rec.checkIn = localPunch.time || "08:02";
                    rec.checkOut = localPunch.action === "CHECK-OUT" ? localPunch.time : "--:-- (Đang làm việc)";
                    rec.status = localPunch.action === "CHECK-OUT" ? "PRESENT" : "IN_PROGRESS";
                    rec.statusLabel = localPunch.action === "CHECK-OUT" ? "Đã check-out" : "Đang trong ca";
                    rec.badgeClass = localPunch.action === "CHECK-OUT" ? "bg-success" : "bg-primary";
                    rec.hours = localPunch.action === "CHECK-OUT" ? 8.0 : "--";
                    rec.workUnits = localPunch.action === "CHECK-OUT" ? 1.0 : "--";
                } else {
                    rec.checkIn = "08:02";
                    rec.checkOut = "--:-- (Đang làm việc)";
                    rec.hours = "--";
                    rec.workUnits = "--";
                    rec.status = "IN_PROGRESS";
                    rec.statusLabel = "Đang trong ca";
                    rec.badgeClass = "bg-primary";
                }
            }
            // Các ngày tương lai (> 22)
            else if (d > 22) {
                // Kiểm tra xem có đơn xin nghỉ phép đã duyệt không
                var hasLeave = HRM_DATA.leaveRequests.some(l => l.empCode === emp.code && l.status === "APPROVED" && d >= parseInt(l.fromDate.split('-')[2]) && d <= parseInt(l.toDate.split('-')[2]));
                if (hasLeave) {
                    rec.shiftName = "Nghỉ Phép Đã Duyệt";
                    rec.checkIn = "--";
                    rec.checkOut = "--";
                    rec.hours = 0;
                    rec.workUnits = 1.0;
                    rec.status = "PAID_LEAVE";
                    rec.statusLabel = "Nghỉ phép năm";
                    rec.badgeClass = "bg-info text-dark";
                } else {
                    rec.checkIn = "--";
                    rec.checkOut = "--";
                    rec.hours = "--";
                    rec.workUnits = "--";
                    rec.status = "FUTURE";
                    rec.statusLabel = "Kế hoạch ca";
                    rec.badgeClass = "bg-light text-muted border";
                }
            }
            // Các ngày đã qua (< 22)
            else {
                rec.checkIn = (d % 4 === 0) ? "08:03" : "07:55";
                rec.checkOut = (d % 5 === 0) ? "17:35" : "17:30";
                rec.hours = 8.0;
                rec.workUnits = 1.0;
                rec.status = "PRESENT";
                rec.statusLabel = "Hợp lệ";
                rec.badgeClass = "bg-success";
            }
        }

        // Cộng dồn thống kê cho các ngày có tính công thực tế
        if (typeof rec.workUnits === "number" && rec.workUnits > 0) {
            totalUnits += rec.workUnits;
        }
        if (typeof rec.hours === "number" && rec.hours > 0) {
            totalHours += rec.hours;
        }
        if (rec.status === "HALF_LEAVE" || rec.status === "PAID_LEAVE") {
            paidLeaveCount += (rec.status === "HALF_LEAVE" ? 0.5 : 1.0);
        }

        records.push(rec);
    }

    return {
        employee: emp,
        month: month,
        year: year,
        daysInMonth: daysInMonth,
        records: records,
        metrics: {
            accumulatedUnits: Math.min(22, Number(totalUnits.toFixed(2))),
            standardUnits: HRM_DATA.company.standardWorkDays,
            totalHours: Number(totalHours.toFixed(1)),
            lateCount: lateCount || (emp.lateHours > 0 ? 1 : 0),
            lateMinutes: lateMinutesTotal || Math.round(emp.lateHours * 60),
            paidLeaveDays: paidLeaveCount || 0.5,
            completionRate: Number(((Math.min(22, totalUnits) / HRM_DATA.company.standardWorkDays) * 100).toFixed(1))
        }
    };
}

// =========================================================================
// HÀM TỔNG HỢP BẢNG CHẤM CÔNG CẢ CÔNG TY (20 NHÂN SỰ)
// =========================================================================
function getAllEmployeesTimesheetSummary(month = 10, year = 2026) {
    var list = [];
    var order = 1;

    HRM_DATA.employees.forEach(function(emp) {
        var tsData = getEmployeeTimesheetRecords(emp.id, month, year);
        var recs = tsData.records.filter(r => typeof r.workUnits === "number" && r.workUnits > 0);

        var standard8h = recs.filter(r => r.shiftCode === "CA_HANH_CHINH" && r.workUnits === 1.0).length;
        var splitShift = recs.filter(r => r.shiftCode === "CA_GAY").length;
        var halfShift = recs.filter(r => r.workUnits === 0.5).length;
        var nightShift = recs.filter(r => r.shiftCode === "CA_DEM").length;
        var lateEarly = recs.filter(r => r.status === "LATE").length;
        var paidLeave = tsData.metrics.paidLeaveDays;

        list.push({
            order: order++,
            id: emp.id,
            code: emp.code,
            name: emp.name,
            department: emp.department,
            role: emp.role,
            standard8h: standard8h,
            splitShift: splitShift,
            halfShift: halfShift,
            nightShift: nightShift,
            lateEarly: lateEarly,
            paidLeave: paidLeave,
            totalHours: tsData.metrics.totalHours,
            totalUnits: tsData.metrics.accumulatedUnits,
            completionRate: tsData.metrics.completionRate,
            isLocked: false,
            statusLabel: "Chờ Duyệt"
        });
    });

    return list;
}

