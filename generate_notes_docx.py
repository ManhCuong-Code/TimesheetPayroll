import os
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import nsdecls, qn

def set_cell_background(cell, fill_hex):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{fill_hex}"/>')
    tcPr.append(shd)

def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
    tcPr.append(tcMar)

def create_notes_document():
    doc = Document()
    
    # Page setup
    sections = doc.sections
    for section in sections:
        section.top_margin = Inches(0.8)
        section.bottom_margin = Inches(0.8)
        section.left_margin = Inches(0.8)
        section.right_margin = Inches(0.8)
        
    # Styles setup
    style_normal = doc.styles['Normal']
    font = style_normal.font
    font.name = 'Calibri'
    font.size = Pt(11)
    font.color.rgb = RGBColor(15, 23, 42)
    
    # Title Block
    p_top = doc.add_paragraph()
    p_top.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r_top1 = p_top.add_run("BỘ GIÁO DỤC VÀ ĐÀO TẠO  •  ĐẠI HỌC HUẾ\n")
    r_top1.font.bold = True
    r_top1.font.size = Pt(10)
    r_top1.font.color.rgb = RGBColor(100, 116, 139)
    
    r_top2 = p_top.add_run("TRƯỜNG ĐẠI HỌC KINH TẾ — KHOA HỆ THỐNG THÔNG TIN KINH TẾ\n")
    r_top2.font.bold = True
    r_top2.font.size = Pt(12)
    r_top2.font.color.rgb = RGBColor(30, 41, 59)
    
    r_div = p_top.add_run("══════════════════════════════════════════\n\n")
    r_div.font.color.rgb = RGBColor(203, 213, 225)
    
    r_main = p_top.add_run("TÀI LIỆU HƯỚNG DẪN & KỊCH BẢN THUYẾT TRÌNH BẢO VỆ ĐỒ ÁN\n")
    r_main.font.bold = True
    r_main.font.size = Pt(16)
    r_main.font.color.rgb = RGBColor(37, 99, 235)
    
    r_sub = p_top.add_run("HỌC PHẦN: LẬP TRÌNH ỨNG DỤNG WEB\n")
    r_sub.font.bold = True
    r_sub.font.size = Pt(12)
    r_sub.font.color.rgb = RGBColor(14, 165, 233)
    
    r_topic = p_top.add_run("ĐỀ TÀI: XÂY DỰNG HỆ THỐNG ỨNG DỤNG WEB CHẤM CÔNG VÀ TÍNH LƯƠNG TỰ ĐỘNG\n(Timesheet & Payroll Automation System)")
    r_topic.font.bold = True
    r_topic.font.size = Pt(13)
    r_topic.font.color.rgb = RGBColor(15, 23, 42)
    
    doc.add_paragraph()
    
    # Metadata Table
    tbl_meta = doc.add_table(rows=4, cols=2)
    tbl_meta.alignment = WD_TABLE_ALIGNMENT.CENTER
    meta_info = [
        ("Giảng viên hướng dẫn:", "TS. Hà Ngọc Long"),
        ("Sinh viên thực hiện:", "Trần Viết Mạnh Cường — Mã sinh viên: 23K4080003"),
        ("Ngành / Chuyên ngành:", "Hệ thống Thông tin Quản lý / Chuyên ngành: Tin học Kinh tế (K57)"),
        ("Thời gian báo cáo dự kiến:", "15 – 20 phút (15 phút trình bày + 5 phút Q&A thảo luận)")
    ]
    for idx, (label, val) in enumerate(meta_info):
        row = tbl_meta.rows[idx]
        cell_lbl, cell_val = row.cells[0], row.cells[1]
        cell_lbl.width = Inches(2.3)
        cell_val.width = Inches(4.5)
        set_cell_background(cell_lbl, "F1F5F9")
        set_cell_background(cell_val, "FFFFFF")
        set_cell_margins(cell_lbl, 80, 80, 120, 120)
        set_cell_margins(cell_val, 80, 80, 120, 120)
        
        p0 = cell_lbl.paragraphs[0]
        r0 = p0.add_run(label)
        r0.font.bold = True
        r0.font.size = Pt(10)
        r0.font.color.rgb = RGBColor(30, 41, 59)
        
        p1 = cell_val.paragraphs[0]
        r1 = p1.add_run(val)
        r1.font.size = Pt(10.5)
        r1.font.color.rgb = RGBColor(15, 23, 42)
        
    doc.add_paragraph()
    
    # Section 1: Overview
    h1 = doc.add_heading("I. TỔNG QUAN CHIẾN LƯỢC PHÂN PHỐI THỜI GIAN (15 PHÚT)", level=1)
    h1.style.font.color.rgb = RGBColor(37, 99, 235)
    
    p_ov = doc.add_paragraph(
        "Bài thuyết trình gồm 15 slide được thiết kế theo tỷ lệ chuẩn màn chiếu 16:9, màu sắc công nghệ hiện đại. "
        "Để đạt điểm tuyệt đối trước Hội đồng chấm điểm, người thuyết trình cần phân bổ thời gian hợp lý, tập trung sâu vào "
        "Kiến trúc phần mềm (Clean Architecture), Cơ sở dữ liệu ACID, Đoạn mã nguồn then chốt và Kết quả kiểm thử thực tế."
    )
    
    # Timeline Table
    tbl_time = doc.add_table(rows=6, cols=4)
    tbl_time.alignment = WD_TABLE_ALIGNMENT.CENTER
    t_headers = ["Phần", "Nội Dung Slide Bao Quát", "Số Lượng Slide", "Thời Lượng Khuyến Nghị"]
    for c_i, th in enumerate(t_headers):
        cell = tbl_time.cell(0, c_i)
        set_cell_background(cell, "1E293B")
        set_cell_margins(cell, 100, 100, 120, 120)
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r = p.add_run(th)
        r.font.bold = True
        r.font.size = Pt(9.5)
        r.font.color.rgb = RGBColor(255, 255, 255)
        
    time_data = [
        ("Phần 1: Mở đầu & Tổng quan", "Slide 1 – Slide 4 (Trang bìa, Bối cảnh, Kiến thức cốt lõi, Yêu cầu)", "4 Slides", "3 phút"),
        ("Phần 2: Công nghệ & Thiết kế", "Slide 5 – Slide 7 (Ngăn xếp công nghệ, Clean Architecture, CSDL 3NF)", "3 Slides", "4 phút"),
        ("Phần 3: UI/UX & Quy trình", "Slide 8 – Slide 9 (Giao diện Responsive 390px, Luồng nghiệp vụ)", "2 Slides", "2.5 phút"),
        ("Phần 4: Lập trình & Kiểm thử", "Slide 10 – Slide 12 (Code Transaction ACID, Tính năng AI, 10 Test Cases)", "3 Slides", "3.5 phút"),
        ("Phần 5: Đánh giá & Kết luận", "Slide 13 – Slide 15 (Đánh giá ưu/nhược, Hướng phát triển, Tri ân & Q&A)", "3 Slides", "2 phút")
    ]
    for r_i, (col1, col2, col3, col4) in enumerate(time_data, start=1):
        row = tbl_time.rows[r_i]
        bg_col = "FFFFFF" if r_i % 2 == 1 else "F8FAFC"
        for c_i, text_val in enumerate([col1, col2, col3, col4]):
            cell = row.cells[c_i]
            set_cell_background(cell, bg_col)
            set_cell_margins(cell, 80, 80, 100, 100)
            p = cell.paragraphs[0]
            if c_i in [0, 2, 3]:
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            r = p.add_run(text_val)
            r.font.size = Pt(9.5)
            if c_i == 0:
                r.font.bold = True
                r.font.color.rgb = RGBColor(30, 41, 59)
            elif c_i == 3:
                r.font.bold = True
                r.font.color.rgb = RGBColor(37, 99, 235)
                
    doc.add_paragraph()
    
    # Section 2: Detailed Script Slide by Slide
    h2 = doc.add_heading("II. KỊCH BẢN THUYẾT TRÌNH CHI TIẾT TỪNG SLIDE (SLIDE 1 ĐẾN SLIDE 15)", level=1)
    h2.style.font.color.rgb = RGBColor(37, 99, 235)
    
    slide_scripts = [
        (1, "TRANG BÌA (COVER SLIDE - DARK NAVY THEME)", "0:00 – 1:00 (1 phút)",
         "Kính thưa Thầy TS. Hà Ngọc Long cùng toàn thể quý Thầy Cô và các bạn sinh viên!\n"
         "Em tên là Trần Viết Mạnh Cường, sinh viên lớp K57 Tin học Kinh tế, ngành Hệ thống Thông tin Quản lý, Trường Đại học Kinh tế – Đại học Huế.\n"
         "Hôm nay, em rất vinh dự được trình bày báo cáo đồ án môn học Lập trình ứng dụng Web với đề tài: 'Xây dựng hệ thống ứng dụng Web Chấm công và Tính lương tự động (Timesheet & Payroll Automation System)'.\n"
         "Đề tài được phát triển trên ngăn xếp công nghệ hiện đại .NET 10 Blazor Server, mô hình kiến trúc sạch Clean Architecture 4 tầng, truy xuất Microsoft SQL Server qua Dapper Micro-ORM và tích hợp Trợ lý Lương AI thông minh.",
         "Phong thái tự tin, mắt nhìn thẳng vào Hội đồng, giới thiệu rành mạch thông tin cá nhân và giảng viên hướng dẫn."),
         
        (2, "BỐI CẢNH & MỤC TIÊU ĐỀ TÀI", "1:00 – 2:00 (1 phút)",
         "Kính thưa Thầy Cô, đối với mọi tổ chức doanh nghiệp, quản lý ngày công và tính lương hàng tháng là một nghiệp vụ tài chính xương sống. Tuy nhiên, tại hầu hết các doanh nghiệp SME quy mô từ 20 đến 50 nhân sự, việc tính lương vẫn dựa hoàn toàn vào bảng tính Excel thủ công.\n"
         "Phương thức này gây ra 3 hệ lụy nghiêm trọng: một là rất dễ kéo nhầm ô công thức làm sai lương nhân viên; hai là kế toán tiêu tốn từ 2 đến 3 ngày cuối tháng để đối soát phiếu chấm công; và ba là nguy cơ rò rỉ thu nhập cá nhân khi chia sẻ chung file tính.\n"
         "Chính vì vậy, mục tiêu của đề tài là xây dựng ứng dụng Web tự động hóa toàn diện với 3 trụ cột: Tính lương 1-chạm (1-Click Run) trong chưa đầy 1.5 giây; chuẩn hóa 100% theo Luật Lao Động; và bảo vệ an toàn quyền riêng tư thu nhập.",
         "Nhấn mạnh vào con số thực tế: '20-50 nhân sự', '2-3 ngày giảm xuống 1.5 giây' để làm nổi bật giá trị thực tiễn."),
         
        (3, "TỔNG QUAN KIẾN THỨC CỐT LÕI", "2:00 – 3:00 (1 phút)",
         "Để hiện thực hóa giải pháp này, em đã vận dụng 4 nền tảng kỹ thuật lập trình web hiện đại:\n"
         "1. Clean Architecture của Robert C. Martin: Phân tách 4 tầng độc lập, tuân thủ nghiêm ngặt nguyên lý Dependency Inversion giúp mã nguồn cô lập, dễ viết Unit Test và bảo trì lâu dài.\n"
         "2. Nền tảng tương tác thời gian thực Blazor Server: Kết nối kênh truyền hai chiều SignalR WebSocket, mang lại trải nghiệm ứng dụng trang đơn SPA mượt mà không tải lại trang.\n"
         "3. Bảo mật đa tầng: Kết hợp giải thuật băm mật khẩu một chiều BCrypt với Salt ngẫu nhiên, cơ chế phân quyền theo vai trò RBAC và phòng vệ chống lộ dữ liệu IDOR.\n"
         "4. Cơ sở dữ liệu quan hệ chuẩn hóa 3NF: Truy xuất dữ liệu tốc độ cao qua Dapper Micro-ORM với 100% truy vấn tham số hóa và bảo đảm tính toàn vẹn giao dịch ACID.",
         "Nói rõ thuật ngữ chuyên ngành (Clean Architecture, SignalR, BCrypt, 3NF, ACID) để thể hiện sự vững vàng về lý thuyết."),
         
        (4, "PHÂN TÍCH YÊU CẦU & MA TRẬN PHÂN QUYỀN (RBAC)", "3:00 – 4:00 (1 phút)",
         "Qua khảo sát nghiệp vụ, hệ thống giải quyết triệt để 4 nỗi đau của bảng tính Excel và phân định rạch ròi 3 nhóm vai trò người dùng:\n"
         "- Vai trò Nhân viên (ROLE_EMPLOYEE): Điểm danh Check-in/out, nộp đơn phép trực tuyến, xem phiếu lương cá nhân an toàn (Anti-IDOR) và hỏi đáp cùng Trợ lý AI.\n"
         "- Vai trò Kế toán / HR (ROLE_HR_ACCOUNTANT): Quản lý ca làm, duyệt đơn phép, bấm nút chạy tính lương tự động 1-chạm, đối soát quỹ lương, khóa sổ kỳ lương (LOCKED) và xuất báo cáo Excel Mẫu 02-LĐTL.\n"
         "- Vai trò Quản trị viên (ROLE_ADMIN): Quản lý tài khoản, cấp quyền vai trò và đặc biệt là cấu hình các tỷ lệ trích nộp bảo hiểm động.",
         "Chỉ rõ sự phân tách quyền: Nhân viên tuyệt đối không thể xem lương của người khác, và kỳ lương đã khóa sổ thì kế toán cũng không được phép tính lại."),
         
        (5, "NGĂN XẾP CÔNG NGHỆ CHUẨN CÔNG NGHIỆP", "4:00 – 5:00 (1 phút)",
         "Slide số 5 trình bày ngăn xếp công nghệ được lựa chọn:\n"
         "- Tầng Giao diện: .NET 10 Blazor Server kết hợp Bootstrap 5.3 và CSS Responsive chuẩn di động 390px.\n"
         "- Tầng Backend: C# 13, .NET 10 Core với cơ chế Dependency Injection tích hợp sẵn.\n"
         "- Tầng CSDL: Microsoft SQL Server (BangChamCongDB) kết nối qua Dapper Micro-ORM giúp tối ưu tốc độ đọc ghi dữ liệu.\n"
         "- Tầng Bảo mật & AI: Thư viện BCrypt.Net-Next, Google Gemini API kết hợp Local Grounded Rule Engine fallback.\n"
         "Nguyên tắc cốt lõi: Tầng Core Domain hoàn toàn độc lập, không phụ thuộc vào bất kỳ thư viện UI hay Database nào (Zero Dependency).",
         "Nhấn mạnh chữ 'Zero Dependency' của tầng Core Business – đây là tiêu chí rất được giảng viên đánh giá cao."),
         
        (6, "THIẾT KẾ KIẾN TRÚC CLEAN ARCHITECTURE & USE CASE", "5:00 – 6:30 (1.5 phút)",
         "Kính thưa Thầy Cô, đây là kiến trúc tổng thể của giải pháp. Toàn bộ mã nguồn được chia thành 4 project rành mạch:\n"
         "- Vòng trong cùng là TimesheetPayroll.CoreBusiness: Chứa các Entity và động cơ tính lương độc lập.\n"
         "- Tầng kế tiếp là TimesheetPayroll.UseCases: Đóng gói toàn bộ các ca sử dụng nghiệp vụ như CalculatePayrollBatchUseCase, ExportPayrollExcelUseCase và khai báo các Repository Interface.\n"
         "- Tầng ngoài cùng gồm TimesheetPayroll.Web phụ trách giao diện người dùng và TimesheetPayroll.DataStore.SQL.Dapper chịu trách nhiệm lưu trữ dữ liệu.\n"
         "Bên phải slide là sơ đồ Use Case tổng quan gồm 19 Use Cases, mô hình hóa đầy đủ các tương tác của 3 tác nhân người dùng và Trợ lý AI.",
         "Dùng chuột hoặc bút laser chỉ vào vòng tròn phụ thuộc: chiều mũi tên luôn hướng vào trong tâm Core Domain."),
         
        (7, "THIẾT KẾ CƠ SỞ DỮ LIỆU CHUẨN 3NF & GIAO DỊCH ACID", "6:30 – 8:00 (1.5 phút)",
         "Về thiết kế cơ sở dữ liệu:\n"
         "Hệ thống gồm 11 bảng thực thể được chuẩn hóa đạt chuẩn 3NF. Đặc biệt, để giải quyết bài toán kế toán, em áp dụng mô hình Header - Line giữa bảng payslips và bảng payslip_lines:\n"
         "- Bảng payslips đóng vai trò Header: Lưu tổng thu nhập Gross, tổng khấu trừ và thực lĩnh Net của từng nhân viên.\n"
         "- Bảng payslip_lines đóng vai trò Line: Lưu chi tiết từng khoản lương ngày công, phụ cấp ăn trưa, tiền trừ BHXH 8%, BHYT 1.5%, BHTN 1% và thuế TNCN.\n"
         "Toàn bộ quá trình tính lương được thực thi trong một Database Transaction duy nhất, bảo đảm tính toàn vẹn ACID: nếu có bất kỳ lỗi nào xảy ra thì hệ thống Rollback 100%, bảo vệ tuyệt đối số liệu tài chính.",
         "Nêu bật bảng `deduction_rates` lưu tỷ lệ bảo hiểm động: khi Nhà nước tăng giảm mức đóng, chỉ cần update CSDL, không phải build lại code."),
         
        (8, "THIẾT KẾ GIAO DIỆN & RESPONSIVE MOBILE 390PX", "8:00 – 9:00 (1 phút)",
         "Giao diện người dùng được thiết kế hiện đại và tối ưu trải nghiệm:\n"
         "- Màn hình Dashboard với 4 thẻ KPI nổi bật và bảng tổng hợp đối soát chi tiết từng nhân viên.\n"
         "- Cổng tự phục vụ nhân viên cho phép điểm danh 1 chạm và tra cứu phiếu lương riêng tư an toàn.\n"
         "- Đạt chuẩn thiết kế đáp ứng Responsive Mobile (kiểm thử chuẩn 390px trên iPhone): Menu tự động thu gọn hamburger, bảng dữ liệu cuộn ngang cảm ứng mượt mà.\n"
         "- Widget Trợ lý Lương AI nổi cố định góc dưới màn hình, bấm vào mở khung chat kính mờ hiện đại.",
         "Nhấn mạnh việc đã kiểm thử thực tế trên độ phân giải mobile 390px, chứng minh giao diện không bị tràn viền hay méo mó."),
         
        (9, "QUY TRÌNH NGHIỆP VỤ CHẤM CÔNG, TÍNH LƯƠNG & KHÓA SỔ", "9:00 – 10:00 (1 phút)",
         "Quy trình nghiệp vụ của hệ thống được khép kín qua 3 luồng:\n"
         "- Luồng 1: Nhân viên điểm danh Check-in/out, nộp đơn phép online, kế toán phê duyệt -> Tự động tính ngày công thực tế.\n"
         "- Luồng 2: Kế toán bấm 1 nút chạy tính lương tự động -> Động cơ tính Gross, trừ bảo hiểm 10.5% và thuế TNCN, lưu trong 1 Transaction.\n"
         "- Luồng 3: Quản lý vòng đời kỳ lương theo máy trạng thái: DRAFT sang CALCULATED, và chốt khóa sổ kỳ lương LOCKED (Read-Only bất biến), không ai được phép can thiệp hay sửa đổi số liệu đã chốt.",
         "Chỉ vào sơ đồ trạng thái (State Diagram) ở dưới slide: DRAFT -> CALCULATED -> LOCKED -> PAID."),
         
        (10, "MINH HỌA LẬP TRÌNH & ĐẢM BẢO TOÀN VẸN", "10:00 – 11:30 (1.5 phút)",
         "Kính thưa Thầy Cô, trên slide số 10 là 2 đoạn mã nguồn tiêu biểu nhất của dự án:\n"
         "Bên trái là phương thức SavePayslipBatchAsync trong SqlRepositories.cs: Toàn bộ quá trình lưu Header phiếu lương và các dòng chi tiết Line đều được gắn chặt với đối tượng tx (Transaction). Lệnh tx.Commit() chỉ kích hoạt khi mọi dòng dữ liệu đã lưu thành công; nếu có ngoại lệ, lệnh tx.Rollback() sẽ phục hồi nguyên trạng.\n"
         "Bên phải là động cơ tính lương SalaryFormulaEngine.cs: Tính chính xác lương theo ngày công tỷ lệ, khấu trừ bảo hiểm nạp động từ CSDL, và tính thuế TNCN theo biểu thuế lũy tiến từng phần 7 bậc.\n"
         "Đồng thời, tại Routes.razor, thẻ AuthorizeRouteView bảo vệ URL đa tầng: nhân viên thường gõ URL quản trị sẽ lập tức nhận cảnh báo HTTP 403 Forbidden.",
         "Giải thích ngắn gọn, súc tích từng dòng code trọng yếu. Đây là slide khẳng định năng lực lập trình của sinh viên."),
         
        (11, "KẾT QUẢ THỰC HIỆN & TÍNH NĂNG ĐỘT PHÁ", "11:30 – 12:30 (1 phút)",
         "Ứng dụng đã hoàn thành 100% với 3 năng lực vượt trội:\n"
         "1. Tính lương tự động 1-chạm hoàn tất trong chưa đầy 1.5 giây cho toàn bộ công ty, triệt tiêu hoàn toàn sai sót kéo lệch ô của Excel.\n"
         "2. Tích hợp Trợ lý Lương AI thông minh (Google Gemini API): Nhân viên có thể trò chuyện tự nhiên để hỏi 'Vì sao lương tháng này bị trừ?' hoặc mô phỏng What-If 'Nếu làm thêm 10 giờ OT thì nhận thêm bao nhiêu tiền?'. Hệ thống có bộ Fallback Engine đảm bảo luôn hoạt động kể cả khi mất mạng.\n"
         "3. Xuất bảng thanh toán tiền lương Excel đúng chuẩn Mẫu số 02-LĐTL của Bộ Tài chính với định dạng tiền tệ chuyên nghiệp, và xuất file CSV chuyển khoản ngân hàng.",
         "Nhấn mạnh tính sáng tạo của tính năng Trợ lý AI và tính thực tế của việc xuất Excel Mẫu 02-LĐTL."),
         
        (12, "KIỂM THỬ HỆ THỐNG & AN TOÀN BẢO MẬT", "12:30 – 13:30 (1 phút)",
         "Về kiểm thử chất lượng phần mềm:\n"
         "Em đã thực hiện kiểm thử hộp đen và kiểm thử đầu cuối với 10 Test Cases (TC01 đến TC10). Bảng trên slide tổng hợp 7 ca kiểm thử then chốt:\n"
         "- TC01: Đăng nhập băm BCrypt thành công (ĐẠT).\n"
         "- TC03: CSDL từ chối hợp đồng có lương âm (ĐẠT).\n"
         "- TC05 & TC06: Tính lương tự động trong Transaction và trừ bảo hiểm 10.5% chuẩn xác (ĐẠT).\n"
         "- TC07: Chặn tính lại kỳ lương đã khóa sổ LOCKED (ĐẠT).\n"
         "- TC08: Chặn vượt quyền URL bằng HTTP 403 Forbidden (ĐẠT).\n"
         "- TC10: Giao diện co giãn chuẩn Responsive Mobile 390px (ĐẠT).\n"
         "Tỷ lệ kiểm thử đạt 10/10 kịch bản, khẳng định độ tin cậy và tính ổn định cao của phần mềm.",
         "Khẳng định kết quả 100% ĐẠT với các bằng chứng kiểm thử rõ ràng từ tài liệu báo cáo đồ án."),
         
        (13, "ĐÁNH GIÁ ƯU ĐIỂM & HẠN CHẾ CẦN HOÀN THIỆN", "13:30 – 14:15 (45 giây)",
         "Đánh giá một cách khách quan:\n"
         "- Ưu điểm: Kiến trúc Clean Architecture chuẩn mực, tự động hóa 100% theo Luật Lao Động, bảo mật đa tầng chống lộ dữ liệu IDOR và trải nghiệm người dùng hiện đại với Trợ lý AI.\n"
         "- Hạn chế: Do phạm vi đồ án môn học, hệ thống hiện chưa kết nối trực tiếp với phần cứng máy chấm công vân tay/FaceID, và chưa tích hợp cổng Open Banking API để chuyển khoản ngân hàng tự động. Đây là những hạn chế em đã đúc rút để tiếp tục nghiên cứu nâng cấp.",
         "Thái độ cầu thị, thẳng thắn thừa nhận hạn chế thực tế để tạo sự tin cậy cao trước Hội đồng."),
         
        (14, "ĐỊNH HƯỚNG PHÁT TRIỂN TRONG TƯƠNG LAI", "14:15 – 14:45 (30 giây)",
         "Trong giai đoạn tiếp theo, em định hướng phát triển 4 tính năng lớn:\n"
         "1. Tích hợp SDK máy chấm công vân tay, khuôn mặt FaceID tại cửa văn phòng.\n"
         "2. Kết nối Open Banking API chuyển khoản tiền lương tự động vào tài khoản nhân viên.\n"
         "3. Phát triển ứng dụng di động Mobile App với định vị GPS Geofencing cho nhân viên chấm công từ xa.\n"
         "4. Ứng dụng Machine Learning phân tích xu hướng chi phí nhân sự và dự báo ngân sách tiền lương.",
         "Trình bày ngắn gọn, thể hiện tầm nhìn mở rộng đề tài thành đồ án tốt nghiệp trong tương lai."),
         
        (15, "KẾT LUẬN & LỜI CẢM ƠN (Q&A SESSION)", "14:45 – 15:30 (45 giây)",
         "Kính thưa Thầy TS. Hà Ngọc Long cùng toàn thể quý Thầy Cô trong Hội đồng!\n"
         "Đồ án học phần Lập trình ứng dụng Web đã giúp em làm chủ toàn diện từ kiến trúc Clean Architecture, .NET 10 Blazor, Dapper Micro-ORM đến giải quyết bài toán nghiệp vụ tài chính tiền lương thực tế.\n"
         "Em xin bày tỏ lòng biết ơn chân thành nhất tới Thầy TS. Hà Ngọc Long đã luôn tận tâm chỉ dẫn, định hướng học thuật cho em trong suốt thời gian qua.\n"
         "Em xin trân trọng cảm ơn quý Thầy Cô và các bạn sinh viên đã chú ý lắng nghe. Em rất kính mong nhận được những nhận xét, góp ý quý báu từ Thầy Cô!\n"
         "Em xin trân trọng cảm ơn!",
         "Cúi đầu chào trân trọng, chuyển sang trạng thái sẵn sàng lắng nghe câu hỏi phản biện từ Giảng viên.")
    ]
    
    for s_num, s_title, s_time, s_speech, s_tip in slide_scripts:
        p_h = doc.add_paragraph()
        p_h.paragraph_format.space_before = Pt(14)
        p_h.paragraph_format.space_after = Pt(2)
        r_num = p_h.add_run(f"SLIDE {s_num:02d}: {s_title}\n")
        r_num.font.bold = True
        r_num.font.size = Pt(11.5)
        r_num.font.color.rgb = RGBColor(30, 41, 59)
        
        r_time = p_h.add_run(f"⏱ Thời lượng: {s_time}\n")
        r_time.font.bold = True
        r_time.font.size = Pt(9.5)
        r_time.font.color.rgb = RGBColor(37, 99, 235)
        
        tbl_s = doc.add_table(rows=2, cols=2)
        tbl_s.alignment = WD_TABLE_ALIGNMENT.CENTER
        
        # Row 1: Speech script
        r1_c0, r1_c1 = tbl_s.rows[0].cells[0], tbl_s.rows[0].cells[1]
        r1_c0.width = Inches(1.5)
        r1_c1.width = Inches(5.3)
        set_cell_background(r1_c0, "F8FAFC")
        set_cell_background(r1_c1, "FFFFFF")
        set_cell_margins(r1_c0, 80, 80, 100, 100)
        set_cell_margins(r1_c1, 80, 80, 100, 100)
        p0 = r1_c0.paragraphs[0]
        p0.add_run("Lời thoại thuyết trình:\n(Nói tự nhiên)").font.bold = True
        p0.runs[0].font.size = Pt(9)
        p0.runs[0].font.color.rgb = RGBColor(71, 85, 105)
        
        p1 = r1_c1.paragraphs[0]
        r_sp = p1.add_run(s_speech)
        r_sp.font.size = Pt(9.5)
        r_sp.font.color.rgb = RGBColor(15, 23, 42)
        
        # Row 2: Tips & Notes
        r2_c0, r2_c1 = tbl_s.rows[1].cells[0], tbl_s.rows[1].cells[1]
        r2_c0.width = Inches(1.5)
        r2_c1.width = Inches(5.3)
        set_cell_background(r2_c0, "EFF6FF")
        set_cell_background(r2_c1, "F0F9FF")
        set_cell_margins(r2_c0, 60, 60, 100, 100)
        set_cell_margins(r2_c1, 60, 60, 100, 100)
        p20 = r2_c0.paragraphs[0]
        p20.add_run("Lưu ý & Điểm nhấn:").font.bold = True
        p20.runs[0].font.size = Pt(9)
        p20.runs[0].font.color.rgb = RGBColor(37, 99, 235)
        
        p21 = r2_c1.paragraphs[0]
        r_tp = p21.add_run(s_tip)
        r_tp.font.size = Pt(9)
        r_tp.font.italic = True
        r_tp.font.color.rgb = RGBColor(3, 105, 161)
        
    doc.add_paragraph()
    
    # Section 3: Frequently Asked Questions & Model Answers
    h3 = doc.add_heading("III. TOP 5 CÂU HỎI PHẢN BIỆN THƯỜNG GẶP CỦA GIẢNG VIÊN & GỢI Ý TRẢ LỜI", level=1)
    h3.style.font.color.rgb = RGBColor(37, 99, 235)
    
    faqs = [
        ("Câu 1: 'Tại sao em lại chọn mô hình Clean Architecture 4 tầng mà không viết theo mô hình 3 tầng truyền thống (MVC/3-Tier)?'",
         "Thưa Thầy Cô, mô hình 3 tầng truyền thống thường khiến tầng nghiệp vụ (Business) bị phụ thuộc ngược vào tầng dữ liệu (Database DataAccess). "
         "Khi áp dụng Clean Architecture, em đảo ngược phụ thuộc bằng Dependency Inversion: tầng Core Business nằm ở trung tâm và hoàn toàn cô lập (Zero Dependency). "
         "Tầng Use Cases chỉ định nghĩa các Interface, còn việc dùng Dapper, Entity Framework hay SQL Server chỉ là chi tiết ở tầng ngoài cùng. "
         "Nhờ đó, khi cần thay đổi CSDL hoặc nâng cấp công nghệ UI thì toàn bộ luật tính lương ở Core Business không bị ảnh hưởng, và việc viết Unit Test cho công thức lương cực kỳ dễ dàng."),
         
        ("Câu 2: 'Hệ thống của em bảo vệ an toàn dữ liệu phiếu lương cá nhân chống lỗ hổng IDOR (Insecure Direct Object References) như thế nào?'",
         "Thưa Thầy Cô, lỗ hổng IDOR xảy ra khi người dùng thay đổi tham số ID trên URL (ví dụ từ /my-payslip?id=1 sang id=2) để xem lén lương của đồng nghiệp. "
         "Hệ thống của em phòng vệ bằng cách: Tuyệt đối không nhận EmployeeId từ tham số URL phía Client. "
         "Tại MyPayslipComponent.razor, mã định danh nhân viên được trích xuất trực tiếp từ Claims định danh của phiên đăng nhập (AuthenticationStateProvider) do Server quản lý. "
         "Vì vậy, dù người dùng có cố tình sửa URL hay can thiệp Javascript trên trình duyệt thì Server vẫn chỉ truy vấn và hiển thị phiếu lương của chính người đó."),
         
        ("Câu 3: 'Tại sao việc lưu trữ bảng lương lại cần dùng Database Transaction và mô hình Header-Line?'",
         "Thưa Thầy Cô, khi kế toán chạy tính lương cho toàn bộ công ty, số lượng bản ghi phát sinh rất lớn: gồm 50 phiếu lương tổng hợp (Header) và hàng trăm dòng chi tiết phụ cấp, bảo hiểm, thuế (Line). "
         "Nếu không dùng Transaction, khi mất điện hoặc lỗi kết nối mạng ở nhân viên thứ 25, CSDL sẽ bị rơi vào trạng thái 'lương tính dở dang' (nửa công ty có lương, nửa công ty chưa, hoặc có Header mà mất Line). "
         "Em sử dụng `using var tx = conn.BeginTransaction()` để bảo đảm tính toàn vẹn ACID: toàn bộ 50 nhân viên và các dòng chi tiết phải được ghi thành công thì mới Commit, nếu có bất kỳ lỗi nào thì Rollback 100%, bảo vệ tuyệt đối tính nhất quán kế toán."),
         
        ("Câu 4: 'Tại sao kỳ lương ở trạng thái LOCKED lại không được phép tính lại?'",
         "Thưa Thầy Cô, đây là quy tắc nghiệp vụ bất biến trong kế toán tài chính. "
         "Khi kỳ lương chuyển sang trạng thái LOCKED nghĩa là kế toán và giám đốc đã đối soát xong, số liệu đã được chốt để chuyển tiền ngân hàng và kê khai thuế. "
         "Nếu cho phép tính lại sau khi đã khóa sổ thì sẽ làm sai lệch báo cáo tài chính và gây rủi ro gian lận. "
         "Trong mã nguồn, phương thức `Period.CanCalculate()` kiểm tra trạng thái: nếu là LOCKED hoặc PAID thì ném ngoại lệ InvalidOperationException và giao diện web hiển thị alert cảnh báo thân thiện."),
         
        ("Câu 5: 'Nếu mất kết nối Internet hoặc hết hạn ngạch Gemini API thì Trợ lý Lương AI hoạt động như thế nào?'",
         "Thưa Thầy Cô, để bảo đảm ứng dụng hoạt động ổn định trong môi trường doanh nghiệp, em đã xây dựng kiến trúc Fallback đa tầng trong `SalaryAiAssistantService.cs`. "
         "Hệ thống sẽ thử gọi Google Gemini API trước để sinh phản hồi tự nhiên. "
         "Nếu gặp lỗi mất mạng hoặc lỗi Quota 429, hệ thống sẽ tự động bắt ngoại lệ và kích hoạt Local Grounded Rule Engine nội bộ. "
         "Bộ động cơ nội bộ này dựa trên các quy tắc nghiệp vụ đã phân tích để giải thích chi tiết mức khấu trừ bảo hiểm 10.5% và biểu thuế TNCN 7 bậc, đảm bảo nhân viên luôn nhận được câu trả lời chính xác mà không bao giờ bị văng lỗi.")
    ]
    
    for q_text, a_text in faqs:
        pq = doc.add_paragraph()
        pq.paragraph_format.space_before = Pt(8)
        pq.paragraph_format.space_after = Pt(2)
        rq = pq.add_run(q_text + "\n")
        rq.font.bold = True
        rq.font.size = Pt(10.5)
        rq.font.color.rgb = RGBColor(185, 28, 28)
        
        pa = doc.add_paragraph()
        pa.paragraph_format.space_before = Pt(0)
        pa.paragraph_format.space_after = Pt(6)
        ra = pa.add_run("➔ Gợi ý trả lời: " + a_text)
        ra.font.size = Pt(10)
        ra.font.color.rgb = RGBColor(30, 41, 59)
        
    doc.add_paragraph()
    
    # Section 4: Editing Guide
    h4 = doc.add_heading("IV. HƯỚNG DẪN TÙY BIẾN VÀ BỔ SUNG THÔNG TIN TRÊN FILE SLIDE (.PPTX)", level=1)
    h4.style.font.color.rgb = RGBColor(37, 99, 235)
    
    p_edit = doc.add_paragraph(
        "File slide PowerPoint `Bai_trinh_bay_Lap_trinh_ung_dung_Web.pptx` được thiết kế dưới dạng các khối Text Box và Shape chuẩn của Microsoft PowerPoint. "
        "Sinh viên có thể mở file trực tiếp bằng PowerPoint để chỉnh sửa linh hoạt theo các hướng dẫn sau:"
    )
    
    edit_steps = [
        ("Chỉnh sửa Giảng viên hướng dẫn:", "Trên Slide 1 (Trang bìa), bấm vào khung thông tin bên dưới ở cột 'GIẢNG VIÊN HƯỚNG DẪN' để sửa đổi họ tên hoặc học hàm/học vị (mặc định đã điền: TS. Hà Ngọc Long)."),
        ("Chỉnh sửa Thông tin Sinh viên:", "Trên Slide 1 và Footer các slide, bấm vào cụm văn bản 'Trần Viết Mạnh Cường (23K4080003)' để điều chỉnh nếu có thay đổi mã sinh viên hoặc lớp sinh hoạt."),
        ("Chỉnh sửa Tên đề tài:", "Tại Slide 1 và Slide 15, nhấp đúp vào hộp tiêu đề chính để cập nhật tên đề tài."),
        ("Thay đổi hình ảnh minh họa:", "Các hình ảnh sơ đồ UML tại Slide 6 (Use Case) và Slide 9 (State Machine) có thể nhấp chuột phải chọn 'Change Picture' để thay bằng hình chụp màn hình thực tế của ứng dụng nếu muốn."),
        ("Xem ghi chú thuyết trình khi trình chiếu:", "Khi trình chiếu trên máy chiếu hoặc Microsoft Teams/Zoom, nhấn tổ hợp phím Alt + F5 để bật chế độ Presenter View. Toàn bộ lời thoại kịch bản đã được tích hợp sẵn ở phần Notes của từng slide!")
    ]
    for e_h, e_b in edit_steps:
        pe = doc.add_paragraph()
        pe.paragraph_format.space_before = Pt(4)
        pe.paragraph_format.space_after = Pt(2)
        re1 = pe.add_run("• " + e_h + " ")
        re1.font.bold = True
        re1.font.size = Pt(10)
        re1.font.color.rgb = RGBColor(37, 99, 235)
        re2 = pe.add_run(e_b)
        re2.font.size = Pt(9.5)
        re2.font.color.rgb = RGBColor(15, 23, 42)
        
    output_docx = "Ghi_chu_thuyet_trinh_Lap_trinh_ung_dung_Web.docx"
    doc.save(output_docx)
    print(f"Notes document saved successfully as: {output_docx}")

if __name__ == "__main__":
    create_notes_document()
