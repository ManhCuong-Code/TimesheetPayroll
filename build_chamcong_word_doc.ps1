# Script to generate comprehensive, formal Word Document for Generic Timesheet & Payroll Web Application
# With HCE Logo, Ornate Thesis Cover Page, and 13 Crisp Re-rendered UML Diagrams
# Target: BAO_CAO_DO_AN_WEB_CHAM_CONG_TINH_LUONG_TranVietManhCuong.docx
# Instructor: TS. Ha Ngoc Long
# Student: Tran Viet Manh Cuong (23K4080003 - K57 Tin hoc kinh te)

$ErrorActionPreference = "Stop"

Write-Host "Initializing Word Application COM Object..."
$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0

$doc = $word.Documents.Add()

# Page Setup: A4, Left 3.0cm (85.05pt), Right/Top/Bottom 2.0cm (56.7pt)
$doc.PageSetup.PaperSize = 7 # wdPaperA4
$doc.PageSetup.LeftMargin = 85.05
$doc.PageSetup.RightMargin = 56.7
$doc.PageSetup.TopMargin = 56.7
$doc.PageSetup.BottomMargin = 56.7

$s = $word.Selection
$assetsDir = "c:\Users\TUF\Desktop\bangChamCong\report_assets"
$imagesDir = "c:\Users\TUF\Desktop\bangChamCong\uml\images"

# Helper Functions
function Write-CoverPage {
    $s.ParagraphFormat.Alignment = 1 # Center
    $s.ParagraphFormat.SpaceBefore = 0
    $s.ParagraphFormat.SpaceAfter = 2
    $s.ParagraphFormat.LineSpacingRule = 0 # Single
    $s.Font.Name = "Times New Roman"
    $s.Font.Size = 13
    $s.Font.Bold = 1
    $s.Font.Italic = 0
    $s.Font.Color = 0x000000
    $s.TypeText("ĐẠI HỌC HUẾ`nTRƯỜNG ĐẠI HỌC KINH TẾ`nKHOA HỆ THỐNG THÔNG TIN KINH TẾ`n")
    
    $s.Font.Size = 11
    $s.Font.Bold = 0
    $s.TypeText("----∞Ω∞----`n`n")

    # Insert HCE University Logo
    $logoPath = "$assetsDir\hce_logo.png"
    if (Test-Path $logoPath) {
        $logoSh = $s.InlineShapes.AddPicture($logoPath, $false, $true)
        $logoSh.Width = 95
        $logoSh.Height = 95
        $s.TypeParagraph()
    }
    
    $s.ParagraphFormat.Alignment = 1
    $s.ParagraphFormat.SpaceBefore = 10
    $s.ParagraphFormat.SpaceAfter = 4
    $s.Font.Size = 15
    $s.Font.Bold = 1
    $s.Font.Color = 0x8B1A1A
    $s.TypeText("`nBÁO CÁO ĐỒ ÁN HỌC PHẦN`n")
    $s.Font.Size = 13.5
    $s.TypeText("HỌC PHẦN: LẬP TRÌNH ỨNG DỤNG WEB`n`n")

    $s.Font.Size = 17
    $s.Font.Bold = 1
    $s.Font.Color = 0x1A365D
    $s.TypeText("ĐỀ TÀI:`nXÂY DỰNG HỆ THỐNG ỨNG DỤNG WEB CHẤM CÔNG VÀ TÍNH LƯƠNG TỰ ĐỘNG`n(TIMESHEET & PAYROLL AUTOMATION SYSTEM)`n`n`n")

    $s.ParagraphFormat.Alignment = 0 # Left
    $s.ParagraphFormat.LeftIndent = 70
    $s.ParagraphFormat.SpaceBefore = 0
    $s.ParagraphFormat.SpaceAfter = 3
    $s.Font.Size = 12.5
    $s.Font.Bold = 0
    $s.Font.Color = 0x000000
    $s.TypeText("Giảng viên hướng dẫn : TS. Hà Ngọc Long`n")
    $s.TypeText("Sinh viên thực hiện  : Trần Viết Mạnh Cường`n")
    $s.TypeText("Mã sinh viên         : 23K4080003`n")
    $s.TypeText("Lớp sinh hoạt        : K57 Tin học kinh tế`n")
    $s.TypeText("Khóa học             : 2023 – 2027`n")
    $s.TypeText("Kho lưu trữ (Repo)   : https://github.com/ManhCuong-Code/TimesheetPayroll.git`n`n`n`n")
    $s.ParagraphFormat.LeftIndent = 0

    $s.ParagraphFormat.Alignment = 1 # Center
    $s.Font.Bold = 1
    $s.Font.Italic = 1
    $s.Font.Size = 12
    $s.TypeText("Thành phố Huế, tháng 01 năm 2026`n")
    
    $s.InsertBreak(7) # wdPageBreak
}

function Write-H1($text) {
    $s.ParagraphFormat.Alignment = 0
    $s.ParagraphFormat.SpaceBefore = 16
    $s.ParagraphFormat.SpaceAfter = 6
    $s.ParagraphFormat.LeftIndent = 0
    $s.ParagraphFormat.LineSpacingRule = 0
    $s.Font.Name = "Times New Roman"
    $s.Font.Size = 15
    $s.Font.Bold = 1
    $s.Font.Italic = 0
    $s.Font.Color = 0x8B1A1A
    $s.TypeText("$text`n")
}

function Write-H2($text) {
    $s.ParagraphFormat.Alignment = 0
    $s.ParagraphFormat.SpaceBefore = 12
    $s.ParagraphFormat.SpaceAfter = 4
    $s.ParagraphFormat.LeftIndent = 0
    $s.ParagraphFormat.LineSpacingRule = 0
    $s.Font.Name = "Times New Roman"
    $s.Font.Size = 13.5
    $s.Font.Bold = 1
    $s.Font.Italic = 0
    $s.Font.Color = 0x1A365D
    $s.TypeText("$text`n")
}

function Write-H3($text) {
    $s.ParagraphFormat.Alignment = 0
    $s.ParagraphFormat.SpaceBefore = 8
    $s.ParagraphFormat.SpaceAfter = 2
    $s.ParagraphFormat.LeftIndent = 0
    $s.ParagraphFormat.LineSpacingRule = 0
    $s.Font.Name = "Times New Roman"
    $s.Font.Size = 12.5
    $s.Font.Bold = 1
    $s.Font.Italic = 1
    $s.Font.Color = 0x2D3748
    $s.TypeText("$text`n")
}

function Write-P($text) {
    $s.ParagraphFormat.Alignment = 3 # Justify
    $s.ParagraphFormat.SpaceBefore = 0
    $s.ParagraphFormat.SpaceAfter = 5
    $s.ParagraphFormat.LeftIndent = 0
    $s.ParagraphFormat.LineSpacingRule = 0
    $s.Font.Name = "Times New Roman"
    $s.Font.Size = 12
    $s.Font.Bold = 0
    $s.Font.Italic = 0
    $s.Font.Color = 0x000000
    $s.TypeText("$text`n")
}

function Write-Bullet($text) {
    $s.ParagraphFormat.Alignment = 3
    $s.ParagraphFormat.SpaceBefore = 0
    $s.ParagraphFormat.SpaceAfter = 3
    $s.ParagraphFormat.LeftIndent = 18
    $s.ParagraphFormat.LineSpacingRule = 0
    $s.Font.Name = "Times New Roman"
    $s.Font.Size = 12
    $s.Font.Bold = 0
    $s.Font.Italic = 0
    $s.Font.Color = 0x000000
    $s.TypeText("• $text`n")
    $s.ParagraphFormat.LeftIndent = 0
}

function Write-CodeBlock($code) {
    $s.ParagraphFormat.Alignment = 0 # Left
    $s.ParagraphFormat.SpaceBefore = 4
    $s.ParagraphFormat.SpaceAfter = 6
    $s.ParagraphFormat.LeftIndent = 14
    $s.ParagraphFormat.LineSpacingRule = 0
    $s.Font.Name = "Consolas"
    $s.Font.Size = 9.5
    $s.Font.Bold = 0
    $s.Font.Italic = 0
    $s.Font.Color = 0x1A202C
    $s.TypeText("$code`n")
    $s.ParagraphFormat.LeftIndent = 0
}

function Write-Img($imagePath, $caption) {
    if (Test-Path $imagePath) {
        $s.ParagraphFormat.Alignment = 1 # Center
        $s.ParagraphFormat.SpaceBefore = 8
        $s.ParagraphFormat.SpaceAfter = 4
        $s.ParagraphFormat.LeftIndent = 0
        $s.ParagraphFormat.LineSpacingRule = 0 # Single line spacing
        
        $sh = $s.InlineShapes.AddPicture($imagePath, $false, $true)
        
        # Max printable width = 440 pt
        if ($sh.Width -gt 440) {
            $ratio = 440.0 / $sh.Width
            $sh.Width = 440
            $sh.Height = [math]::Round($sh.Height * $ratio)
        }
        if ($sh.Height -gt 500) {
            $ratio = 500.0 / $sh.Height
            $sh.Height = 500
            $sh.Width = [math]::Round($sh.Width * $ratio)
        }
        $s.TypeParagraph()
        
        $s.Font.Name = "Times New Roman"
        $s.Font.Size = 10.5
        $s.Font.Bold = 1
        $s.Font.Italic = 1
        $s.Font.Color = 0x1A365D
        $s.ParagraphFormat.Alignment = 1
        $s.ParagraphFormat.SpaceAfter = 12
        $s.TypeText("$caption`n")
    }
}

function Write-Table($data, [float[]]$colWidths) {
    $numRows = $data.Count
    $numCols = $data[0].Count
    $tbl = $doc.Tables.Add($s.Range, $numRows, $numCols)
    $tbl.Borders.Enable = 1
    $tbl.Range.Font.Name = "Times New Roman"
    $tbl.Range.Font.Size = 10.5
    
    for ($r = 0; $r -lt $numRows; $r++) {
        for ($c = 0; $c -lt $numCols; $c++) {
            $cell = $tbl.Cell($r + 1, $c + 1)
            $cell.Range.Text = $data[$r][$c]
            if ($r -eq 0) {
                $cell.Range.Font.Bold = 1
                $cell.Shading.BackgroundPatternColor = 0xE2E8F0
                $cell.Range.ParagraphFormat.Alignment = 1 # Center
            }
            if ($colWidths -and $c -lt $colWidths.Count) {
                $cell.Width = $colWidths[$c]
            }
        }
    }
    $s.EndKey(6) # wdStory
    $s.TypeParagraph()
}

Write-Host "1. Writing Cover Page with HCE Logo..."
Write-CoverPage

Write-Host "2. Writing Acknowledgements..."
Write-H1 "LỜI CẢM ƠN"
Write-P "Trong quá trình học tập, nghiên cứu và hoàn thành đồ án môn học Lập trình ứng dụng web, em đã nhận được sự quan tâm, chỉ dẫn tận tình từ quý Thầy, Cô giáo trong Khoa Hệ thống Thông tin Kinh tế – Trường Đại học Kinh tế, Đại học Huế."
Write-P "Trước hết, em xin bày tỏ lòng biết ơn sâu sắc và chân thành nhất tới Thầy TS. Hà Ngọc Long – Giảng viên trực tiếp phụ trách và giảng dạy học phần Lập trình ứng dụng web. Trong suốt thời gian qua, Thầy đã tận tâm truyền đạt những kiến thức nền tảng và chuyên sâu về kiến trúc ứng dụng web hiện đại, phương pháp thiết kế phần mềm theo kiến trúc sạch (Clean Architecture), nguyên lý phân quyền bảo mật và định hướng giải quyết các bài toán nghiệp vụ doanh nghiệp thực tế. Sự khích lệ, tận tình và những nhận xét học thuật quý báu của Thầy là động lực to lớn giúp em hoàn thành tốt đồ án này."
Write-P "Em cũng xin gửi lời cảm ơn trân trọng tới toàn thể quý Thầy, Cô giáo Khoa Hệ thống Thông tin Kinh tế đã trang bị cho em hành trang kiến thức vững chắc về cơ sở dữ liệu quan hệ, phân tích thiết kế hệ thống và kỹ thuật lập trình trong các kỳ học vừa qua."
Write-P "Mặc dù em đã nỗ lực vận dụng tối đa các kiến thức đã học cùng tinh thần làm việc nghiêm túc, nhưng do thời gian và kinh nghiệm thực tế còn hạn chế nên bản báo cáo khó tránh khỏi những thiếu sót nhất định. Em rất kính mong nhận được những lời chỉ dẫn, nhận xét và đóng góp quý báu từ Thầy TS. Hà Ngọc Long để đồ án được hoàn thiện hơn nữa và làm nền tảng vững chắc cho quá trình học tập, nghiên cứu chuyên sâu trong tương lai."
Write-P "Em xin kính chúc Thầy dồi dào sức khỏe, hạnh phúc và gặt hái được nhiều thành công hơn nữa trong sự nghiệp giảng dạy và nghiên cứu khoa học!"

$s.ParagraphFormat.Alignment = 2 # Right
$s.Font.Italic = 1
$s.TypeText("Huế, tháng 01 năm 2026`n")
$s.Font.Bold = 1
$s.Font.Italic = 0
$s.TypeText("Sinh viên thực hiện`n")
$s.Font.Bold = 0
$s.Font.Italic = 1
$s.TypeText("Trần Viết Mạnh Cường`n")
$s.InsertBreak(7)

Write-Host "3. Writing Table of Contents..."
Write-H1 "MỤC LỤC TỔNG QUAN"
Write-Bullet "LỜI CẢM ƠN"
Write-Bullet "DANH MỤC THUẬT NGỮ VÀ KÝ HIỆU VIẾT TẮT"
Write-Bullet "LỜI MỞ ĐẦU"
Write-Bullet "CHƯƠNG 1: TỔNG QUAN VỀ BÀI TOÁN VÀ KHẢO SÁT NGHIỆP VỤ DOANH NGHIỆP"
Write-Bullet "  1.1. Bối cảnh và thực trạng quản lý nhân sự, chấm công tại các doanh nghiệp"
Write-Bullet "  1.2. Những thách thức và rủi ro của phương pháp quản lý thủ công bằng bảng tính Excel"
Write-Bullet "  1.3. Mục tiêu và phạm vi xây dựng hệ thống ứng dụng Web"
Write-Bullet "  1.4. Đối tượng sử dụng và ma trận phân quyền hệ thống (RBAC)"
Write-Bullet "CHƯƠNG 2: PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG ỨNG DỤNG WEB"
Write-Bullet "  2.1. Phân tích yêu cầu chức năng (5 phân hệ)"
Write-Bullet "  2.2. Phân tích yêu cầu phi chức năng (Responsive Mobile 390px, Bảo mật, ACID)"
Write-Bullet "  2.3. Mô hình hóa chức năng bằng hệ thống sơ đồ Use Case (PlantUML)"
Write-Bullet "  2.4. Thiết kế cơ sở dữ liệu quan hệ chuẩn 3NF (11 bảng thực thể cốt lõi)"
Write-Bullet "  2.5. Ràng buộc toàn vẹn, Khóa ngoại và Toàn vẹn giao dịch ACID (Header-Line)"
Write-Bullet "CHƯƠNG 3: THIẾT KẾ KIẾN TRÚC VÀ GIAO DIỆN ỨNG DỤNG WEB (UI/UX)"
Write-Bullet "  3.1. Lựa chọn mô hình kiến trúc sạch (Clean Architecture 4 tầng)"
Write-Bullet "  3.2. Cấu trúc tổ chức thư mục giải pháp và ngăn xếp công nghệ (.NET 10, Blazor Server, Dapper, SQL Server)"
Write-Bullet "  3.3. Thiết kế luồng điều hướng và sơ đồ trang (Site Map & Navigation)"
Write-Bullet "  3.4. Thiết kế chi tiết giao diện người dùng các phân hệ chính"
Write-Bullet "  3.5. Thiết kế đáp ứng đa thiết bị (Responsive Mobile UI 390px)"
Write-Bullet "CHƯƠNG 4: HIỆN THỰC HÓA VÀ CÀI ĐẶT CÁC PHÂN HỆ NGHIỆP VỤ"
Write-Bullet "  4.1. Hiện thực hóa cơ chế Xác thực & Phân quyền đa vai trò (RBAC, BCrypt & Phòng vệ chống Bypass URL)"
Write-Bullet "  4.2. Hiện thực hóa Luật nghiệp vụ 1: Động cơ tính lương chuẩn theo Luật Lao Động & Tỷ lệ trích nộp động"
Write-Bullet "  4.3. Hiện thực hóa Luật nghiệp vụ 2: Khóa sổ kỳ lương Read-Only & Bảo vệ chống lỗ hổng IDOR"
Write-Bullet "  4.4. Hiện thực hóa Phân hệ Chấm công, Quản lý Ca làm việc và Duyệt đơn nghỉ phép"
Write-Bullet "  4.5. Hiện thực hóa Phân hệ Tính lương 1-chạm (1-Click Run) trong Database Transaction"
Write-Bullet "  4.6. Tính năng nâng cao: Trợ lý Lương AI thông minh (Gemini API & Fallback Engine)"
Write-Bullet "  4.7. Tính năng nâng cao: Xuất báo cáo bảng lương Excel Mẫu số 02-LĐTL và CSV ngân hàng"
Write-Bullet "CHƯƠNG 5: KIỂM THỬ HỆ THỐNG VÀ ĐÁNH GIÁ KẾT QUẢ"
Write-Bullet "  5.1. Môi trường và phương pháp kiểm thử"
Write-Bullet "  5.2. Kịch bản kiểm thử chức năng then chốt (10 Test Cases TC01 – TC10)"
Write-Bullet "  5.3. Kiểm thử an toàn bảo mật, chống lỗi IDOR và chống tiêm mã độc SQL Injection"
Write-Bullet "  5.4. Đánh giá kết quả đạt được và hiệu năng hệ thống"
Write-Bullet "KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN"
Write-Bullet "TÀI LIỆU THAM KHẢO"
Write-Bullet "PHỤ LỤC: MA TRẬN TRUY VẾT YÊU CẦU ĐỒNG BỘ 100%"
$s.InsertBreak(7)

Write-Host "4. Writing Abbreviations Table..."
Write-H1 "DANH MỤC THUẬT NGỮ VÀ KÝ HIỆU VIẾT TẮT"

$abbrData = @(
    @("Viết tắt", "Tên tiếng Anh đầy đủ", "Diễn giải ý nghĩa"),
    @("HRMS", "Human Resource Management System", "Hệ thống quản lý nguồn nhân lực doanh nghiệp"),
    @("CSDL", "Cơ Sở Dữ Liệu", "Database – Kho dữ liệu quan hệ có cấu trúc"),
    @("3NF", "Third Normal Form", "Dạng chuẩn 3 trong thiết kế cơ sở dữ liệu quan hệ"),
    @("RBAC", "Role-Based Access Control", "Cơ chế kiểm soát truy cập phân quyền dựa trên vai trò"),
    @("IDOR", "Insecure Direct Object References", "Lỗ hổng bảo mật tham chiếu đối tượng trực tiếp không an toàn"),
    @("ACID", "Atomicity, Consistency, Isolation, Durability", "4 thuộc tính đảm bảo an toàn tuyệt đối của một giao dịch CSDL"),
    @("ORM", "Object-Relational Mapping", "Kỹ thuật ánh xạ giữa đối tượng lập trình và CSDL quan hệ"),
    @("DI", "Dependency Injection", "Kỹ thuật tiêm phụ thuộc giúp mã nguồn độc lập, linh hoạt"),
    @("SPA", "Single Page Application", "Ứng dụng web trang đơn tương tác thời gian thực"),
    @("BHXH", "Bảo hiểm Xã hội", "Khoản trích nộp an sinh xã hội bắt buộc (8.0% người lao động)"),
    @("BHYT", "Bảo hiểm Y tế", "Khoản trích nộp bảo hiểm y tế bắt buộc (1.5% người lao động)"),
    @("BHTN", "Bảo hiểm Thất nghiệp", "Khoản trích nộp bảo hiểm thất nghiệp (1.0% người lao động)"),
    @("TNCN", "Thu nhập cá nhân", "Thuế thu nhập cá nhân tính theo biểu thuế lũy tiến từng phần"),
    @("AI", "Artificial Intelligence", "Trí tuệ nhân tạo (Mô hình ngôn ngữ lớn LLM - Google Gemini)")
)
Write-Table $abbrData @(90.0, 150.0, 220.0)
$s.InsertBreak(7)

Write-Host "5. Writing Introduction..."
Write-H1 "LỜI MỞ ĐẦU"
Write-P "Trong nền kinh tế hiện đại, nguồn nhân lực là tài sản quý giá nhất quyết định sự phát triển bền vững của mọi tổ chức và doanh nghiệp. Đi đôi với việc quản lý nhân sự, công tác theo dõi ngày công (Timesheet) và tính toán chi trả tiền lương (Payroll) hàng tháng là một nghiệp vụ tài chính trọng yếu, tác động trực tiếp đến quyền lợi thiết thân của người lao động cũng như sự tuân thủ pháp lý của doanh nghiệp trước các quy định của Luật Lao Động và Luật Bảo Hiểm Xã Hội."
Write-P "Tuy nhiên, tại phần lớn các doanh nghiệp vừa và nhỏ hiện nay, công tác chấm công và tính lương vẫn chủ yếu được thực hiện một cách thủ công thông qua các bảng tính Excel rời rạc. Phương thức này bộc lộ rất nhiều hạn chế nghiêm trọng: kế toán mất hàng ngày trời để đối soát từng dòng công; công thức dễ bị sai sót khi kéo lệch ô; rủi ro lộ bí mật thông tin thu nhập khi gửi chung file tính; và đặc biệt là sự thiếu linh hoạt khi các chính sách bảo hiểm, mức lương cơ sở hay quy định thuế TNCN của Nhà nước thay đổi."
Write-P "Trước bối cảnh đó, việc xây dựng một Hệ Thống Ứng Dụng Web Chấm Công & Tính Lương Tự Động (Timesheet & Payroll Automation System) độc lập, hiện đại, an toàn và dễ sử dụng là một đòi hỏi cấp thiết. Hệ thống không chỉ giải phóng kế toán khỏi khối lượng công việc tính toán thủ công nặng nhọc, mà còn đảm bảo tính minh bạch, chính xác tuyệt đối, hỗ trợ nhân viên tự phục vụ và ứng dụng Trí tuệ Nhân tạo (AI) để giải đáp thắc mắc lương tức thì."
Write-P "Dưới sự chỉ dẫn tận tình của Thầy TS. Hà Ngọc Long, em đã lựa chọn và hoàn thiện đề tài: 'Xây dựng hệ thống ứng dụng Web chấm công và tính lương tự động (Timesheet & Payroll Automation System)' cho đồ án môn học Lập trình ứng dụng web. Đề tài được thiết kế theo mô hình kiến trúc sạch Clean Architecture, công nghệ .NET 10 Blazor Interactive Server, kết nối cơ sở dữ liệu Microsoft SQL Server (BangChamCongDB) qua Dapper Micro-ORM, mang lại một giải pháp phần mềm hoàn chỉnh, an toàn và đạt chuẩn công nghiệp."
$s.InsertBreak(7)

Write-Host "6. Writing Chapter 1..."
Write-H1 "CHƯƠNG 1: TỔNG QUAN VỀ BÀI TOÁN VÀ KHẢO SÁT NGHIỆP VỤ DOANH NGHIỆP"
Write-H2 "1.1. Bối cảnh và thực trạng quản lý nhân sự, chấm công tại các doanh nghiệp"
Write-P "Trong chu trình vận hành của bất kỳ doanh nghiệp nào, quy trình từ chấm công đến tính lương diễn ra định kỳ theo tháng với các hoạt động cốt lõi:"
Write-Bullet "Thiết lập cơ cấu tổ chức & hồ sơ: Quản lý danh mục phòng ban, danh sách nhân viên, thông tin tài khoản ngân hàng và ký kết hợp đồng lao động quy định mức lương cơ bản, phụ cấp ăn trưa, xăng xe."
Write-Bullet "Ghi nhận ngày công: Nhân viên thực hiện điểm danh Check-in/Check-out theo các ca làm việc; nộp đơn xin nghỉ phép khi có việc riêng hoặc ốm đau."
Write-Bullet "Đối soát & Khóa bảng công: Cuối kỳ, bộ phận quản lý/kế toán tổng hợp tổng số ngày công làm việc thực tế, ngày nghỉ có phép, ngày nghỉ không lương và chốt bảng công tháng."
Write-Bullet "Tính lương & Quyết toán tài chính: Áp dụng công thức lương theo tỷ lệ ngày công, cộng các khoản phụ cấp, tính thưởng, phạt, trích nộp các khoản bảo hiểm bắt buộc (BHXH 8%, BHYT 1.5%, BHTN 1%), tính thuế TNCN và xác định số tiền thực lĩnh (Net Salary)."
Write-Bullet "Chi trả & Bảo lưu hồ sơ: Xuất bảng lương chi tiết gửi nhân viên, xuất danh sách chuyển khoản ngân hàng và lưu trữ hồ sơ tài chính phục vụ quyết toán thuế."

Write-H2 "1.2. Những thách thức và rủi ro của phương pháp quản lý thủ công bằng bảng tính Excel"
Write-Bullet "Rủi ro sai lệch công thức: Kế toán chỉ cần kéo nhầm một hàng dữ liệu hoặc sai lệch dấu ngoặc đơn là lương của hàng chục nhân viên bị tính thiếu hoặc thừa hàng triệu đồng, dẫn đến tranh chấp, khiếu nại."
Write-Bullet "Tiêu tốn nguồn lực và thời gian: Kế toán phải mất từ 2 đến 3 ngày mỗi cuối tháng để đối soát từng tờ phiếu chấm công, kiểm tra đơn xin nghỉ phép giấy và dò tìm tỷ lệ bảo hiểm."
Write-Bullet "Lộ bí mật thu nhập nội bộ (Data Privacy): Chia sẻ chung một file Excel bảng lương rất dễ khiến nhân viên dòm ngó mức thu nhập của đồng nghiệp, gây mất đoàn kết nội bộ và vi phạm nghiêm trọng chính sách bảo mật thông tin."
Write-Bullet "Không đáp ứng được sự thay đổi của luật pháp: Mức lương cơ sở, tỷ lệ trích nộp BHXH/BHYT/BHTN hay mức giảm trừ gia cảnh thuế TNCN thường xuyên được điều chỉnh theo Nghị định của Chính phủ. Nếu bị hardcode cứng trong bảng tính, kế toán rất khó cập nhật đồng bộ."

Write-H2 "1.3. Mục tiêu và phạm vi xây dựng hệ thống ứng dụng Web"
Write-P "Mục tiêu tổng quát: Xây dựng một ứng dụng web hoàn chỉnh phục vụ tự động hóa toàn diện quy trình chấm công, quản lý nghỉ phép và tính lương doanh nghiệp, đảm bảo chuẩn xác 100%, bảo mật dữ liệu và tuân thủ chặt chẽ Luật Lao Động Việt Nam."
Write-P "Mục tiêu kỹ thuật:"
Write-Bullet "Áp dụng kiến trúc sạch Clean Architecture 4 tầng (.NET 10 Blazor Interactive Server), phân tách rành mạch trách nhiệm giữa các tầng, tuân thủ nguyên lý Dependency Rule."
Write-Bullet "Thiết kế cơ sở dữ liệu quan hệ chuẩn hóa 3NF trên Microsoft SQL Server với 11 bảng thực thể."
Write-Bullet "Bảo mật tài khoản bằng giải thuật băm một chiều BCrypt, phân quyền đa vai trò RBAC và phòng vệ đa tầng chống lỗ hổng IDOR và Bypass URL."
Write-Bullet "Tự động hóa tính lương 1-chạm (1-Click Run) trong một Database Transaction an toàn tuyệt đối."
Write-Bullet "Tích hợp Trợ lý AI giải đáp thắc mắc lương và tính năng xuất báo cáo Excel Mẫu số 02-LĐTL của Bộ Tài chính."
Write-Bullet "Giao diện người dùng hiện đại, tương thích hoàn hảo trên thiết bị di động (kiểm thử chuẩn 390px chiều ngang)."

Write-H2 "1.4. Đối tượng sử dụng và ma trận phân quyền hệ thống (RBAC)"
Write-P "Hệ thống phân định 3 nhóm vai trò (Roles) trong bảng user_accounts với tài khoản kiểm thử mẫu:"

$rbacData = @(
    @("Tên Đăng Nhập", "Mật Khẩu", "Vai Trò (Role)", "Họ Tên Nhân Viên", "Phòng Ban", "Quyền Hạn Nghiệp Vụ"),
    @("nhanvien.a", "123456", "ROLE_EMPLOYEE", "Nguyễn Văn A", "CNTT", "Điểm danh, xem bảng công, nộp phép, xem phiếu lương mình, hỏi đáp AI"),
    @("ketoan.thao", "123456", "ROLE_HR_ACCOUNTANT", "Lê Thị Thu Thảo", "Kế Toán - HR", "Duyệt phép, khóa bảng công, tính lương batch, chốt sổ Read-Only, xuất Excel"),
    @("admin.trong", "123456", "ROLE_ADMIN", "Trần Đình Trọng", "Ban Giám Đốc", "Toàn quyền quản trị, tạo tài khoản & phân quyền, cấu hình tỷ lệ bảo hiểm"),
    @("nhanvien.tuan", "123456", "ROLE_EMPLOYEE", "Hoàng Minh Tuấn", "Kinh Doanh", "Chấm công, xem lương cá nhân độc lập (kiểm thử chống lỗi lộ IDOR)")
)
Write-Table $rbacData @(70.0, 50.0, 95.0, 80.0, 65.0, 100.0)
$s.InsertBreak(7)

Write-Host "7. Writing Chapter 2..."
Write-H1 "CHƯƠNG 2: PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG ỨNG DỤNG WEB"
Write-H2 "2.1. Phân tích yêu cầu chức năng (Functional Requirements)"
Write-Bullet "Phân hệ 1: Quản trị Xác thực & Tài khoản người dùng (UC01 – UC03): Đăng nhập an toàn bằng mật khẩu băm BCrypt; đổi mật khẩu; quản lý tài khoản và phân quyền vai trò."
Write-Bullet "Phân hệ 2: Quản trị Cơ cấu Tổ chức & Nhân sự (UC04 – UC08): Quản lý danh mục phòng ban; quản lý hồ sơ nhân viên; quản lý hợp đồng lao động; cấu hình tỷ lệ trích nộp bảo hiểm động."
Write-Bullet "Phân hệ 3: Quản lý Chấm công & Nghỉ phép (UC09 – UC13): Điểm danh Check-in/Check-out theo ca làm việc; nộp đơn xin nghỉ phép; xét duyệt đơn nghỉ phép; khóa sổ bảng chấm công tháng."
Write-Bullet "Phân hệ 4: Động cơ Tính lương Tự động & Quyết toán (UC14 – UC17): Chạy tính toán lương hàng loạt; đối soát quỹ lương; chốt kỳ lương Read-Only; xuất báo cáo thanh toán ngân hàng và Excel."
Write-Bullet "Phân hệ 5: Cổng Tự phục vụ Nhân viên & Trợ lý AI (UC18 – UC19): Tra cứu phiếu lương cá nhân an toàn (chống IDOR); hỏi đáp quy chế lương với Trợ lý AI; mô phỏng lương What-If."

Write-H2 "2.2. Phân tích yêu cầu phi chức năng (Non-Functional Requirements)"
Write-Bullet "Bảo mật và Quyền riêng tư: 100% mật khẩu được băm một chiều bằng BCrypt với Salt ngẫu nhiên; 100% câu truy vấn CSDL được tham số hóa thông qua Dapper Micro-ORM, triệt tiêu nguy cơ SQL Injection; phòng vệ đa tầng chặn tài khoản thường gõ thẳng URL quản trị bằng AuthorizeRouteView (HTTP 403 Forbidden); bảo mật cấp bản ghi chống IDOR."
Write-Bullet "Toàn vẹn dữ liệu & Tính nhất quán: Nghiệp vụ ghi nhận bảng lương (Header payslips và Line payslip_lines) bắt buộc thực thi trong một Database Transaction; kỳ lương ở trạng thái LOCKED hoặc PAID tuyệt đối không cho phép tính lại."
Write-Bullet "Khả năng đáp ứng đa thiết bị: Tương thích tối ưu từ màn hình Desktop đến màn hình điện thoại di động hẹp (390px). Menu tự động thu gọn hamburger, bảng dữ liệu hỗ trợ vuốt cuộn ngang mượt mà."

Write-H2 "2.3. Mô hình hóa chức năng bằng hệ thống sơ đồ Use Case (PlantUML)"
Write-P "Sơ đồ ca sử dụng tổng quan mô tả đầy đủ các tác nhân tham gia tương tác với 19 Use Case thống nhất:"
Write-Img "$imagesDir\01_usecase_overview.png" "Hình 2.1: Sơ đồ Use Case tổng quan toàn hệ thống Chấm công & Tính lương (UC01 – UC19)"

Write-P "Sơ đồ phân rã phân hệ Cổng thông tin Nhân viên & Trợ lý ảo AI:"
Write-Img "$imagesDir\02_usecase_employee.png" "Hình 2.2: Sơ đồ Use Case phân rã Cổng thông tin tự phục vụ Nhân viên & Trợ lý AI"

Write-P "Sơ đồ phân rã phân hệ Kế toán Tiền lương, Nhân sự & Quản trị hệ thống:"
Write-Img "$imagesDir\03_usecase_hr_admin.png" "Hình 2.3: Sơ đồ Use Case phân rã Phân hệ Kế toán HR & Quản trị viên"

Write-H2 "2.4. Thiết kế cơ sở dữ liệu quan hệ chuẩn 3NF (11 bảng thực thể cốt lõi)"
Write-P "Cơ sở dữ liệu BangChamCongDB trên Microsoft SQL Server gồm 11 bảng thực thể đạt chuẩn 3NF:"

$tablesData = @(
    @("STT", "Tên Bảng CSDL", "Khóa Chính", "Khóa Ngoại Liên Kết", "Mô Tả Chức Năng Nghiệp Vụ"),
    @("1", "departments", "id (BIGINT)", "Không", "Quản lý cơ cấu phòng ban và trưởng bộ phận"),
    @("2", "employees", "id (BIGINT)", "department_id -> departments", "Hồ sơ nhân sự, CCCD, Email, STK ngân hàng"),
    @("3", "user_accounts", "id (BIGINT)", "employee_id -> employees (1-1)", "Tài khoản đăng nhập, mật khẩu băm BCrypt, Role RBAC"),
    @("4", "contracts", "id (BIGINT)", "employee_id -> employees", "Hợp đồng lao động, mức lương cơ bản, phụ cấp ăn trưa, xăng"),
    @("5", "payroll_periods", "id (BIGINT)", "Không", "Quản lý kỳ lương tháng, năm, ngày công chuẩn, trạng thái khóa sổ"),
    @("6", "deduction_rates", "id (BIGINT)", "Không", "Cấu hình tỷ lệ bảo hiểm động: BHXH (8%), BHYT (1.5%), BHTN (1%)"),
    @("7", "work_shifts", "id (BIGINT)", "Không", "Danh mục ca làm việc (Sáng, Chiều, Hành chính, Ca đêm, OT)"),
    @("8", "timesheets", "id (BIGINT)", "employee_id, period_id, work_shift_id", "Ghi nhận Check-in/out, số giờ làm việc, trạng thái đi muộn"),
    @("9", "leave_requests", "id (BIGINT)", "employee_id, approver_id", "Quản lý đơn xin nghỉ phép trực tuyến, lý do, trạng thái duyệt"),
    @("10", "payslips", "id (BIGINT)", "period_id, employee_id", "Bảng lương Header: Ngày công, Gross, Bảo hiểm, Thuế, Net"),
    @("11", "payslip_lines", "id (BIGINT)", "payslip_id -> payslips (CASCADE)", "Chi tiết Line lương: Lương công, Phụ cấp, BHXH, BHYT, BHTN, TNCN")
)
Write-Table $tablesData @(30.0, 95.0, 80.0, 110.0, 145.0)

Write-H2 "2.5. Ràng buộc toàn vẹn, Khóa ngoại và Toàn vẹn giao dịch ACID"
Write-Bullet "Ràng buộc kiểm tra (CHECK Constraints): chk_contract_salary (base_salary >= 0); chk_contract_dates (end_date >= start_date); chk_period_month (period_month BETWEEN 1 AND 12); chk_period_status (status IN ('DRAFT', 'CALCULATING', 'CALCULATED', 'LOCKED', 'PAID'))."
Write-Bullet "Toàn vẹn giao dịch Cặp Header-Line (payslips – payslip_lines): Ghi nhận đồng thời trong Database Transaction (SavePayslipBatchAsync). Bắt đầu với using var tx = conn.BeginTransaction(), cam kết tx.Commit(), hoặc hoàn nguyên tx.Rollback() nếu phát sinh lỗi."
$s.InsertBreak(7)

Write-Host "8. Writing Chapter 3..."
Write-H1 "CHƯƠNG 3: THIẾT KẾ KIẾN TRÚC VÀ GIAO DIỆN ỨNG DỤNG WEB (UI/UX)"
Write-H2 "3.1. Lựa chọn mô hình kiến trúc sạch (Clean Architecture 4 tầng)"
Write-P "Hệ thống áp dụng chuẩn mực Clean Architecture phân chia 4 tầng độc lập:"
Write-Img "$imagesDir\05_class_layered_architecture.png" "Hình 3.1: Sơ đồ Lớp kiến trúc phân tầng Clean Architecture 4 tầng khớp 100% mã nguồn"

Write-P "Sơ đồ Lớp mô hình miền (Domain Model Class Diagram) biểu diễn 11 thực thể cốt lõi:"
Write-Img "$imagesDir\04_class_domain_model.png" "Hình 3.2: Sơ đồ Lớp thực thể mô hình miền (Domain Model) độc lập 100%"

Write-H2 "3.2. Cấu trúc tổ chức thư mục giải pháp và ngăn xếp công nghệ"
Write-P "Giải pháp bao gồm 4 project chính:"
Write-Bullet "TimesheetPayroll.CoreBusiness: Chứa các thực thể miền và động cơ công thức tính lương độc lập (SalaryFormulaEngine.cs). Project này có 0 Project Reference."
Write-Bullet "TimesheetPayroll.UseCases: Chứa các ca sử dụng nghiệp vụ và các interface trừu tượng (IRepositories.cs). Chỉ tham chiếu duy nhất CoreBusiness."
Write-Bullet "TimesheetPayroll.DataStore.SQL.Dapper: Plugin triển khai truy xuất dữ liệu SQL Server bằng Dapper Micro-ORM với 100% truy vấn tham số hóa."
Write-Bullet "TimesheetPayroll.Web: Project khởi chạy cấu hình Dependency Injection, tích hợp Blazor Interactive Server, SignalR thời gian thực và quản trị xác thực."

Write-H2 "3.3. Thiết kế luồng điều hướng và sơ đồ trang (Site Map & Navigation)"
Write-Bullet "/login: Màn hình Đăng nhập xác thực BCrypt, hỗ trợ nút Quick Login Demo 1-chạm."
Write-Bullet "/: Trang Tổng quan (Dashboard) hiển thị các chỉ số KPI nhân sự, ngày công và bảng lương."
Write-Bullet "/admin/payroll-calculation: Phân hệ Kế toán chạy Tính lương hàng loạt, Xem bảng lương tổng hợp đối soát, Chốt khóa sổ kỳ lương (LOCKED) và Xuất báo cáo."
Write-Bullet "/admin/shifts: Phân hệ Quản trị danh mục ca làm việc và hệ số làm thêm giờ."
Write-Bullet "/admin/users: Phân hệ Quản trị tài khoản người dùng và phân quyền vai trò."
Write-Bullet "/my-timesheet: Phân hệ Nhân viên điểm danh Check-in/Check-out và tra cứu bảng công."
Write-Bullet "/leave-request: Phân hệ Nhân viên nộp đơn xin nghỉ phép có kiểm tra validation ngày tháng."
Write-Bullet "/my-payslip: Phân hệ Tra cứu phiếu lương cá nhân an toàn (bảo vệ chống IDOR)."
Write-Bullet "/ai-salary-assistant: Phân hệ Trợ lý Lương AI hỏi đáp chế độ và mô phỏng What-If."
Write-Bullet "/change-password: Màn hình đổi mật khẩu cá nhân."

Write-H2 "3.4. Thiết kế chi tiết giao diện người dùng các phân hệ chính"
Write-Bullet "Dashboard Tổng quan: 4 thẻ Card chỉ số KPI nổi bật (Tổng Nhân sự, Giờ công tích lũy, Quỹ lương tháng, Trạng thái kỳ lương), kèm biểu đồ phân bổ cơ cấu chi phí."
Write-Bullet "Trang Tính Lương Hàng Loạt: Nút hành động nổi bật 'Chạy Tính Lương Tự Động 1-Chạm', bảng tổng hợp đối soát chi tiết từng nhân viên (Lương CB, Ngày công, Phụ cấp, Giảm trừ BHXH, Thuế TNCN, Thực lĩnh), nút 'Khóa Sổ Kỳ Lương' và nút 'Xuất Excel Mẫu 02-LĐTL'."
Write-Bullet "Widget Trợ Lý Lương AI Nổi Toàn Hệ Thống (FloatingAiAssistant.razor): Nút tròn AI nổi góc phải dưới màn hình ở mọi trang, bấm vào mở khung chat kính mờ hiện đại giải đáp thắc mắc lương tức thì."

Write-H2 "3.5. Thiết kế đáp ứng đa thiết bị (Responsive Mobile UI 390px)"
Write-P "Sơ đồ Triển khai hạ tầng và thành phần công nghệ thống nhất (Deployment Diagram):"
Write-Img "$imagesDir\13_deployment_diagram.png" "Hình 3.3: Sơ đồ Triển khai hạ tầng và kiến trúc phân phối Web trên thực tế"
$s.InsertBreak(7)

Write-Host "9. Writing Chapter 4..."
Write-H1 "CHƯƠNG 4: HIỆN THỰC HÓA VÀ CÀI ĐẶT CÁC PHÂN HỆ NGHIỆP VỤ"
Write-H2 "4.1. Hiện thực hóa cơ chế Xác thực & Phân quyền đa vai trò (RBAC, BCrypt & Phòng vệ URL)"
Write-P "Sơ đồ tuần tự Đăng nhập & Xác thực BCrypt:"
Write-Img "$imagesDir\06_sequence_login.png" "Hình 4.1: Sơ đồ tuần tự Đăng nhập, Băm mật khẩu BCrypt & Nạp Claims vai trò"

$codeAuth = @"
// Bảo vệ Route phân quyền quản trị
@page "/admin/payroll-calculation"
@attribute [Authorize(Roles = "ROLE_HR_ACCOUNTANT, ROLE_ADMIN")]

// Cấu hình Router tại Routes.razor
<AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)">
    <NotAuthorized>
        <div class="alert alert-danger text-center p-5">
            <h3><i class="fa-solid fa-shield-halved text-danger me-2"></i>Truy Cập Bị Từ Chối (HTTP 403)</h3>
            <p>Tài khoản hiện tại không có quyền truy cập vào phân hệ này!</p>
        </div>
    </NotAuthorized>
</AuthorizeRouteView>
"@
Write-CodeBlock $codeAuth

Write-H2 "4.2. Hiện thực hóa Luật nghiệp vụ 1: Động cơ tính lương chuẩn theo Luật Lao Động & Tỷ lệ trích nộp động"
Write-P "Cài đặt tại SalaryFormulaEngine.cs trong project CoreBusiness:"
Write-P "1. Lương ngày công = Lương cơ bản * (Ngày công thực tế / Ngày công chuẩn)"
Write-P "2. Thu nhập Gross = Lương ngày công + Phụ cấp ăn trưa + Phụ cấp xăng xe"
Write-P "3. Khấu trừ bảo hiểm (10.5%) = BHXH (8%) + BHYT (1.5%) + BHTN (1%) nạp động từ CSDL bảng deduction_rates qua _rateRepo.GetActiveRatesAsync(), không hardcode."
Write-P "4. Thuế TNCN tính theo biểu thuế lũy tiến từng phần 7 bậc sau khi giảm trừ bản thân 11tr và người phụ thuộc 4.4tr."
Write-P "5. Thực lĩnh Net = Gross - Khấu trừ bảo hiểm - Thuế TNCN."

Write-H2 "4.3. Hiện thực hóa Luật nghiệp vụ 2: Khóa sổ kỳ lương Read-Only & Bảo vệ chống lỗi IDOR"
Write-P "Sơ đồ tuần tự Khóa sổ kỳ lương Read-Only & Xuất báo cáo:"
Write-Img "$imagesDir\08_sequence_lock_payroll.png" "Hình 4.2: Sơ đồ tuần tự Chốt kỳ lương Khóa sổ (Read-Only) & Xuất báo cáo Excel"

Write-Bullet "Kỳ đã khóa sổ không được tính lại: Khi kỳ lương ở trạng thái LOCKED, nếu người dùng bấm 'Chạy Tính Lương Tự Động', UseCase ném InvalidOperationException, UI bắt lỗi hiển thị alert đỏ thân thiện: '[CHẶN VI PHẠM LUẬT NGHIỆP VỤ] Kỳ lương đã ở trạng thái LOCKED (Khóa sổ) và không được phép tính lại'."
Write-Bullet "Nhân viên chỉ xem phiếu lương của chính mình: EmployeeId được giải mã từ AuthenticationState.User.Claims['EmployeeId'], ngăn chặn tuyệt đối lỗi bảo mật IDOR."

Write-H2 "4.4. Hiện thực hóa Phân hệ Chấm công, Quản lý Ca làm việc và Duyệt đơn nghỉ phép"
Write-P "Sơ đồ Hoạt động Chấm công & Nộp đơn xin nghỉ phép trực tuyến:"
Write-Img "$imagesDir\10_activity_timesheet_leave.png" "Hình 4.3: Sơ đồ Hoạt động Phân hệ Chấm công & Quản lý Nghỉ phép (Swimlane)"

Write-H2 "4.5. Hiện thực hóa Phân hệ Tính lương 1-chạm (1-Click Run) trong Database Transaction"
Write-P "Sơ đồ tuần tự Tính lương tự động hàng loạt với Database Transaction:"
Write-Img "$imagesDir\07_sequence_payroll_calculation.png" "Hình 4.4: Sơ đồ tuần tự Tính lương tự động hàng loạt 1-chạm (Batch Calculation)"

Write-P "Sơ đồ Hoạt động Quyết toán lương & Xuất chi trả ngân hàng:"
Write-Img "$imagesDir\11_activity_payroll_settlement.png" "Hình 4.5: Sơ đồ Hoạt động Quy trình Quyết toán lương & Xuất ngân hàng"

Write-P "Sơ đồ Trạng thái vòng đời kỳ lương (State Machine Diagram):"
Write-Img "$imagesDir\12_state_payroll_period.png" "Hình 4.6: Sơ đồ Trạng thái vòng đời kỳ lương (DRAFT -> CALCULATING -> CALCULATED -> LOCKED -> PAID)"

$codeTrans = @"
// Thực thi ghi nhận bảng lương Header-Line trong 1 Transaction an toàn
using var tx = conn.BeginTransaction();
try {
    foreach (var slip in payslips) {
        // 1. Lưu Header payslips và lấy ID tự tăng
        var payslipId = await conn.ExecuteScalarAsync<long>(sqlHeader, slip, tx);
        
        // 2. Lưu toàn bộ các Line payslip_lines tương ứng
        foreach (var line in slip.Lines) {
            line.PayslipId = payslipId;
            await conn.ExecuteAsync(sqlLine, line, tx);
        }
    }
    tx.Commit(); // Cam kết an toàn ACID
} catch {
    tx.Rollback(); // Hoàn nguyên toàn bộ nếu xảy ra lỗi
    throw;
}
"@
Write-CodeBlock $codeTrans

Write-H2 "4.6. Tính năng nâng cao: Trợ lý Lương AI thông minh (Gemini API & Fallback Engine)"
Write-P "Sơ đồ tuần tự Tương tác tra cứu và giải đáp thắc mắc lương với Trợ lý AI:"
Write-Img "$imagesDir\09_sequence_ai_inquiry.png" "Hình 4.7: Sơ đồ tuần tự Tương tác cùng Trợ lý Lương AI & Cơ chế Fallback thông minh"

Write-Bullet "Bọc trong UseCase độc lập: IAskSalaryAiUseCase, ICompareSalaryUseCase, ISimulateSalaryUseCase."
Write-Bullet "Không hardcode API Key: Nạp động từ biến môi trường GEMINI_API_KEY hoặc cấu hình an toàn."
Write-Bullet "Cơ chế Fallback thông minh: Tự động chuyển về Động cơ phân tích quy tắc nội bộ (Local Grounded Engine) khi mất mạng hoặc hết hạn mức Quota API."
Write-Bullet "Mô phỏng What-If: Cho phép nhân viên giả định làm thêm giờ hoặc biến động lương."

Write-H2 "4.7. Tính năng nâng cao: Xuất báo cáo bảng lương Excel Mẫu số 02-LĐTL và CSV ngân hàng"
Write-P "Use Case ExportPayrollExcelUseCase xuất dữ liệu trực tiếp ra tệp .xls chuẩn hóa theo Mẫu số 02-LĐTL của Bộ Tài chính: Đầy đủ tiêu đề, phòng ban, lương cơ bản, các khoản phụ cấp, bảo hiểm và thực lĩnh với định dạng tiền tệ kế toán chuẩn."
$s.InsertBreak(7)

Write-Host "10. Writing Chapter 5..."
Write-H1 "CHƯƠNG 5: KIỂM THỬ HỆ THỐNG VÀ ĐÁNH GIÁ KẾT QUẢ"
Write-H2 "5.1. Môi trường và phương pháp kiểm thử"
Write-Bullet "Môi trường phần cứng: CPU Intel Core i7 / AMD Ryzen, RAM 16GB, SSD NVMe."
Write-Bullet "Môi trường phần mềm: Windows 11 64-bit, Microsoft SQL Server 2022 Developer Edition, .NET 10 SDK, Google Chrome & Microsoft Edge."
Write-Bullet "Phương pháp kiểm thử: Kiểm thử hộp đen (Black-box Testing) chức năng giao diện, kiểm thử luồng giao dịch nghiệp vụ đầu cuối (End-to-End Testing) và kiểm thử giao diện co giãn Responsive Mobile (390px)."

Write-H2 "5.2. Kịch bản kiểm thử chức năng then chốt (10 Test Cases)"
Write-P "Dưới đây là bảng tổng hợp kết quả thực thi 10 kịch bản kiểm thử then chốt của ứng dụng web:"

$testCasesData = @(
    @("Mã TC", "Chức Năng Kiểm Thử", "Dữ Liệu Đầu Vào", "Kết Quả Kỳ Vọng", "Kết Quả Thực Tế", "Đánh Giá"),
    @("TC01", "Đăng nhập tài khoản hợp lệ", "user: admin.trong, pass: 123456", "Đăng nhập thành công, nạp Claims ROLE_ADMIN, mở Dashboard.", "Đăng nhập mượt mà, chuyển trang đúng vai trò.", "ĐẠT"),
    @("TC02", "Đăng nhập sai mật khẩu", "user: nhanvien.a, pass: 999999", "Bị từ chối, cảnh báo đỏ cạnh ô nhập, mật khẩu được bảo vệ.", "Báo lỗi chính xác, không văng lỗi hệ thống.", "ĐẠT"),
    @("TC03", "Ràng buộc kiểm tra Lương âm", "Nhập base_salary = -5000000", "CSDL kích hoạt chk_contract_salary, từ chối lưu dữ liệu.", "CSDL từ chối, ứng dụng bắt lỗi hiển thị alert thân thiện.", "ĐẠT"),
    @("TC04", "Kiểm tra Ngày nghỉ phép hợp lệ", "Chọn end_date < start_date", "Hệ thống chặn bằng Validation, hiển thị lỗi đỏ dưới ô nhập ngày.", "Hiển thị lỗi validation ngay lập tức, không cho submit.", "ĐẠT"),
    @("TC05", "Tính lương tự động 1-chạm", "Bấm 'Chạy Tính Lương Tự Động'", "Hệ thống mở Transaction, tính và lưu đồng thời payslips & lines.", "Dữ liệu ghi nhận vào SQL Server, trạng thái chuyển CALCULATED.", "ĐẠT"),
    @("TC06", "Demo Luật 1: Khấu trừ BH 10.5%", "Lương CB 10tr, nạp tỷ lệ từ CSDL", "Trích BHXH 800k, BHYT 150k, BHTN 100k (Tổng khấu trừ 1.050.000 đ).", "Số liệu tính toán chuẩn xác 100% khớp bảng deduction_rates.", "ĐẠT"),
    @("TC07", "Demo Luật 2: Khóa sổ Read-Only", "Bấm tính lại kỳ lương đã LOCKED", "Hệ thống chặn, hiển thị alert đỏ: 'Kỳ lương đã khóa sổ'.", "Bắt lỗi nghiệp vụ thân thiện, giữ nguyên số liệu tài chính.", "ĐẠT"),
    @("TC08", "Chặn vượt quyền URL trực tiếp", "Đăng nhập nhanvien.a, gõ URL /admin", "Router AuthorizeRouteView chặn, hiển thị màn hình 403 Forbidden.", "Hiển thị màn hình 403, bảo vệ dữ liệu an toàn.", "ĐẠT"),
    @("TC09", "Xuất báo cáo Excel Mẫu 02-LĐTL", "Bấm nút 'Xuất Excel Bảng Lương'", "Tải xuống file .xls định dạng Mẫu 02-LĐTL đầy đủ cột mục.", "Tải file thành công, mở trên Excel đúng định dạng tiền tệ.", "ĐẠT"),
    @("TC10", "Tương thích Mobile 390px", "Mở DevTools co về 390px", "Menu thu gọn hamburger, bảng cuộn ngang, Widget AI đẹp.", "Giao diện hiển thị chuẩn responsive trên điện thoại.", "ĐẠT")
)
Write-Table $testCasesData @(35.0, 95.0, 85.0, 115.0, 95.0, 35.0)

Write-H2 "5.3. Kiểm thử an toàn bảo mật, chống lỗi IDOR và chống tiêm mã độc SQL Injection"
Write-Bullet "Kiểm thử chống lỗi IDOR: Khi nhân viên EMP001 đăng nhập xem phiếu lương cá nhân, hệ thống chỉ truy vấn dữ liệu theo Claim EmployeeId gắn chặt với tài khoản. Dù người dùng cố tình can thiệp mã nguồn phía client hay sửa URL, hệ thống không bao giờ trả về dữ liệu của nhân viên khác."
Write-Bullet "Kiểm thử chống SQL Injection: 100% câu lệnh truy vấn trong SqlRepositories.cs sử dụng cú pháp tham số hóa của Dapper (@EmployeeId, @PeriodId), triệt tiêu hoàn toàn khả năng can thiệp câu lệnh SQL qua dữ liệu đầu vào."

Write-H2 "5.4. Đánh giá kết quả đạt được và hiệu năng hệ thống"
Write-Bullet "Về mặt học thuật: Triển khai thành công kiến trúc sạch Clean Architecture với 4 tầng phân tách độc lập; mô hình hóa bài toán bằng 13 sơ đồ UML chuẩn PlantUML."
Write-Bullet "Về mặt nghiệp vụ: Chuẩn hóa quy trình chấm công, tính lương tự động, trích nộp bảo hiểm và thuế TNCN theo đúng quy định của pháp luật Việt Nam."
Write-Bullet "Về mặt công nghệ: Ứng dụng .NET 10 Blazor Server, SQL Server, Dapper ORM, Trí tuệ Nhân tạo AI và Responsive Design mang lại trải nghiệm người dùng hiện đại, tốc độ phản hồi trang dưới 1.2 giây."
$s.InsertBreak(7)

Write-Host "11. Writing Conclusion & Future Scope..."
Write-H1 "KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN"
Write-H2 "1. Kết luận"
Write-P "Sau quá trình nghiên cứu và phát triển dưới sự hướng dẫn của Thầy TS. Hà Ngọc Long, đồ án học phần Lập trình ứng dụng web với đề tài: 'Xây dựng hệ thống ứng dụng Web chấm công và tính lương tự động (Timesheet & Payroll Automation System)' đã hoàn thành xuất sắc các mục tiêu đề ra:"
Write-Bullet "Xây dựng một hệ thống phần mềm doanh nghiệp độc lập, giải quyết triệt để các hạn chế của phương pháp tính lương thủ công bằng Excel."
Write-Bullet "Áp dụng chuẩn mực kiến trúc sạch Clean Architecture 4 tầng, tuân thủ nguyên lý Dependency Rule, dễ dàng kiểm thử, bảo trì và mở rộng."
Write-Bullet "Cài đặt hoàn chỉnh cơ sở dữ liệu quan hệ chuẩn 3NF trên Microsoft SQL Server với 11 bảng thực thể, đảm bảo toàn vẹn giao dịch ACID."
Write-Bullet "Hiện thực hóa các luật nghiệp vụ bắt buộc: Công thức lương chuẩn kết hợp nạp tỷ lệ bảo hiểm động, khóa sổ kỳ lương Read-Only và bảo mật phiếu lương cá nhân chống IDOR."
Write-Bullet "Bảo mật đa tầng với BCrypt, phân quyền RBAC và chặn vượt quyền URL bằng màn hình 403 Forbidden."
Write-Bullet "Tích hợp các tính năng tiên tiến: Trợ lý Lương AI thông minh (Google Gemini API & Local Grounded Engine fallback) và Xuất báo cáo bảng lương Excel Mẫu số 02-LĐTL của Bộ Tài chính."

Write-H2 "2. Hướng phát triển trong tương lai"
Write-Bullet "Kết nối máy chấm công phần cứng: Tích hợp SDK kết nối trực tiếp với máy chấm công vân tay, nhận diện khuôn mặt (FaceID) hoặc thẻ từ tại văn phòng doanh nghiệp."
Write-Bullet "Cổng thanh toán tự động (Banking Open API): Kết nối API chuyển khoản hàng loạt của các ngân hàng thương mại để tự động chi trả lương ngay khi kỳ lương được duyệt."
Write-Bullet "Ứng dụng di động PWA / Mobile Native: Xây dựng ứng dụng di động cho phép nhân viên chấm công bằng định vị GPS và nhận thông báo đẩy tức thì khi có phiếu lương mới."
$s.InsertBreak(7)

Write-Host "12. Writing References..."
Write-H1 "TÀI LIỆU THAM KHẢO"
Write-Bullet "1. TS. Hà Ngọc Long, Bài giảng và tài liệu hướng dẫn học phần Lập trình ứng dụng web, Khoa Hệ thống Thông tin Kinh tế – Trường Đại học Kinh tế, Đại học Huế."
Write-Bullet "2. Quốc hội nước CHXHCN Việt Nam (2019), Bộ luật Lao động số 45/2019/QH14, NXB Chính trị Quốc gia Sự thật, Hà Nội."
Write-Bullet "3. Chính phủ (2024), Nghị định số 73/2024/NĐ-CP quy định mức lương cơ sở và chế độ tiền lương, Hà Nội."
Write-Bullet "4. Bộ Tài chính (2013), Thông tư 111/2013/TT-BTC hướng dẫn thực hiện Luật Thuế thu nhập cá nhân và Chế độ chứng từ kế toán tiền lương Mẫu số 02-LĐTL, Hà Nội."
Write-Bullet "5. Robert C. Martin (2017), Clean Architecture: A Craftsman's Guide to Software Structure and Design, Prentice Hall."
Write-Bullet "6. Microsoft Documentation (2025 – 2026), ASP.NET Core & Blazor Web Applications Documentation, Microsoft Learn Platform."
Write-Bullet "7. Dapper Tutorial & Documentation (2025), Micro-ORM for .NET Applications, Dapper Lib Docs."
Write-Bullet "8. Google AI for Developers (2025), Gemini API Documentation & Prompt Engineering Guide."
$s.InsertBreak(7)

Write-Host "13. Writing Appendix: Traceability Matrix..."
Write-H1 "PHỤ LỤC: MA TRẬN TRUY VẾT YÊU CẦU ĐỒNG BỘ 100%"
Write-P "Bảng đối chiếu truy vết từ Yêu cầu nghiệp vụ (Business Rules) sang Ca sử dụng (Use Cases), Lớp miền (Classes) và Bảng CSDL:"

$matrixData = @(
    @("Mã BR", "Mô Tả Nghiệp Vụ", "Use Cases", "Lớp Miền (Entity Class)", "Bảng SQL Server"),
    @("BR-01", "Quản lý cơ cấu phòng ban & nhân sự", "UC04, UC05", "Department, Employee", "departments, employees"),
    @("BR-02", "Quản lý tài khoản, mật khẩu & RBAC", "UC01, UC02, UC03, UC07", "UserAccount", "user_accounts"),
    @("BR-03", "Quản lý hợp đồng & lương cơ bản", "UC06", "Contract", "contracts"),
    @("BR-04", "Cấu hình tỷ lệ trích nộp bảo hiểm động", "UC08", "DeductionRate", "deduction_rates"),
    @("BR-05", "Chấm công, ca làm việc & đơn nghỉ phép", "UC09, UC10, UC11, UC12", "Timesheet, WorkShift, LeaveRequest", "timesheets, work_shifts, leave_requests"),
    @("BR-06", "Khóa bảng chấm công kỳ tháng", "UC13", "PayrollPeriod", "payroll_periods"),
    @("BR-07", "Tính toán lương tự động 1-chạm (Batch)", "UC14", "PayrollPeriod, Payslip, PayslipLine", "payroll_periods, payslips, payslip_lines"),
    @("BR-08", "Đối soát tổng hợp & chốt sổ Read-Only", "UC15, UC16", "PayrollPeriod, Payslip", "payroll_periods, payslips"),
    @("BR-09", "Xuất báo cáo Excel Mẫu 02-LĐTL & CSV", "UC17", "ExportPayrollExcelUseCase", "payslips, payslip_lines"),
    @("BR-10", "Bảo mật phiếu lương & Trợ lý ảo AI", "UC18, UC19", "Payslip, PayslipLine, SalaryAiAssistant", "payslips, payslip_lines")
)
Write-Table $matrixData @(40.0, 110.0, 75.0, 110.0, 125.0)

# Save Document
$outputPath = "c:\Users\TUF\Desktop\bangChamCong\BAO_CAO_DO_AN_WEB_CHAM_CONG_TINH_LUONG_TranVietManhCuong.docx"
Write-Host "Saving document to $outputPath..."
$doc.SaveAs([ref]$outputPath, [ref]16)
$doc.Close()
$word.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
[System.GC]::Collect()
[System.GC]::WaitForPendingFinalizers()

$fileSize = (Get-Item $outputPath).Length
Write-Host "SUCCESS! Standalone Timesheet & Payroll Word Document created: $outputPath ($fileSize bytes)"
