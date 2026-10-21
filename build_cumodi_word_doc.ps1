# Script to generate comprehensive, formal Word Document for CUMODI Web Application Project Report
# Target: BAO_CAO_DO_AN_WEB_CUMODI_TranVietManhCuong.docx
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

# Helper Functions
function Write-CoverPage {
    $s.ParagraphFormat.Alignment = 1 # Center
    $s.ParagraphFormat.SpaceBefore = 0
    $s.ParagraphFormat.SpaceAfter = 3
    $s.Font.Name = "Times New Roman"
    $s.Font.Size = 13
    $s.Font.Bold = 1
    $s.Font.Italic = 0
    $s.Font.Color = 0x000000
    $s.TypeText("BỘ GIÁO DỤC VÀ ĐÀO TẠO - ĐẠI HỌC HUẾ`nTRƯỜNG ĐẠI HỌC KINH TẾ`nKHOA HỆ THỐNG THÔNG TIN KINH TẾ`n`n")
    
    $s.ParagraphFormat.Alignment = 1
    $s.Font.Size = 15
    $s.Font.Bold = 1
    $s.Font.Color = 0x8B1A1A
    $s.TypeText("BÁO CÁO ĐỒ ÁN HỌC PHẦN`n")
    $s.Font.Size = 14
    $s.TypeText("HỌC PHẦN: LẬP TRÌNH ỨNG DỤNG WEB`n`n`n")

    $s.Font.Size = 18
    $s.Font.Bold = 1
    $s.Font.Color = 0x1A365D
    $s.TypeText("ĐỀ TÀI:`nXÂY DỰNG HỆ THỐNG ỨNG DỤNG WEB QUẢN LÝ HOẠT ĐỘNG TRUYỀN THÔNG VÀ SẢN XUẤT NỘI DUNG SỐ CÔNG TY TNHH MTV CỨ MƠ ĐI (CUMODI)`n`n`n`n")

    $s.ParagraphFormat.Alignment = 0 # Left
    $s.ParagraphFormat.LeftIndent = 60
    $s.Font.Size = 13
    $s.Font.Bold = 0
    $s.Font.Color = 0x000000
    $s.TypeText("Giảng viên hướng dẫn : TS. Hà Ngọc Long`n")
    $s.TypeText("Sinh viên thực hiện  : Trần Viết Mạnh Cường`n")
    $s.TypeText("Mã sinh viên         : 23K4080003`n")
    $s.TypeText("Lớp sinh hoạt        : K57 Tin học kinh tế`n")
    $s.TypeText("Khóa học             : 2023 – 2027`n`n`n`n`n")
    $s.ParagraphFormat.LeftIndent = 0

    $s.ParagraphFormat.Alignment = 1 # Center
    $s.Font.Bold = 1
    $s.Font.Italic = 1
    $s.TypeText("Thành phố Huế, Năm 2026`n")
    
    $s.InsertBreak(7) # wdPageBreak
}

function Write-H1($text) {
    $s.ParagraphFormat.Alignment = 0
    $s.ParagraphFormat.SpaceBefore = 16
    $s.ParagraphFormat.SpaceAfter = 6
    $s.ParagraphFormat.LeftIndent = 0
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
        $s.ParagraphFormat.SpaceBefore = 6
        $s.ParagraphFormat.SpaceAfter = 4
        $s.ParagraphFormat.LeftIndent = 0
        $sh = $s.InlineShapes.AddPicture($imagePath, $false, $true)
        if ($sh.Width -gt 460) {
            $ratio = 460.0 / $sh.Width
            $sh.Width = 460
            $sh.Height = [math]::Round($sh.Height * $ratio)
        }
        if ($sh.Height -gt 500) {
            $ratio = 500.0 / $sh.Height
            $sh.Height = 500
            $sh.Width = [math]::Round($sh.Width * $ratio)
        }
        $s.TypeParagraph()
        $s.Font.Name = "Times New Roman"
        $s.Font.Size = 11
        $s.Font.Bold = 1
        $s.Font.Italic = 1
        $s.Font.Color = 0x2D3748
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

$assetsDir = "c:\Users\TUF\Desktop\bangChamCong\cumodi_assets"

Write-Host "1. Writing Cover Page..."
Write-CoverPage

Write-Host "2. Writing Acknowledgements..."
Write-H1 "LỜI CẢM ƠN"
Write-P "Trong quá trình học tập, nghiên cứu và hoàn thành đồ án môn học Lập trình ứng dụng web, em đã nhận được sự quan tâm, chỉ dẫn tận tình từ quý Thầy, Cô giáo trong Khoa Hệ thống Thông tin Kinh tế – Trường Đại học Kinh tế, Đại học Huế."
Write-P "Trước hết, em xin bày tỏ lòng biết ơn sâu sắc và chân thành nhất tới Thầy TS. Hà Ngọc Long – Giảng viên trực tiếp phụ trách và giảng dạy học phần Lập trình ứng dụng web. Trong suốt thời gian qua, Thầy đã tận tâm truyền đạt những kiến thức nền tảng và chuyên sâu về kiến trúc ứng dụng web, phương pháp phân tích thiết kế hệ thống, công nghệ xây dựng phần mềm hiện đại và định hướng giải quyết các vấn đề kỹ thuật phát sinh. Sự khích lệ và những định hướng học thuật quý báu của Thầy là động lực to lớn giúp em hoàn thành tốt đồ án này."
Write-P "Em cũng xin gửi lời cảm ơn trân trọng tới toàn thể quý Thầy, Cô giáo Khoa Hệ thống Thông tin Kinh tế đã trang bị cho em hành trang kiến thức vững chắc về cơ sở dữ liệu, phân tích thiết kế hệ thống thông tin và kỹ thuật lập trình trong các kỳ học vừa qua."
Write-P "Mặc dù em đã nỗ lực vận dụng tối đa các kiến thức đã học cùng tinh thần làm việc nghiêm túc, nhưng do thời gian và kinh nghiệm thực tế còn hạn chế nên bản báo cáo đồ án khó tránh khỏi những thiếu sót nhất định. Em rất kính mong nhận được những lời chỉ dẫn, nhận xét và đóng góp quý báu từ Thầy TS. Hà Ngọc Long để đồ án được hoàn thiện hơn nữa và làm nền tảng vững chắc cho quá trình học tập, nghiên cứu chuyên sâu trong tương lai."
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
Write-Bullet "  1.1. Giới thiệu tổng quan về Công ty TNHH MTV Cứ Mơ Đi (CUMODI)"
Write-Bullet "  1.2. Hiện trạng hoạt động và những thách thức trong công tác quản trị"
Write-Bullet "  1.3. Mục tiêu và phạm vi xây dựng ứng dụng Web"
Write-Bullet "  1.4. Đối tượng sử dụng và phân quyền hệ thống"
Write-Bullet "CHƯƠNG 2: PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG ỨNG DỤNG WEB"
Write-Bullet "  2.1. Phân tích yêu cầu chức năng (5 phân hệ)"
Write-Bullet "  2.2. Phân tích yêu cầu phi chức năng (Responsive 390px, Performance, Security)"
Write-Bullet "  2.3. Mô hình hóa chức năng bằng sơ đồ Use Case"
Write-Bullet "  2.4. Thiết kế cơ sở dữ liệu quan hệ (Kế thừa cấu trúc 13 bảng thực thể 3NF)"
Write-Bullet "  2.5. Ràng buộc toàn vẹn, Chỉ mục và Tối ưu hóa truy vấn CSDL"
Write-Bullet "CHƯƠNG 3: THIẾT KẾ KIẾN TRÚC VÀ GIAO DIỆN ỨNG DỤNG WEB (UI/UX)"
Write-Bullet "  3.1. Lựa chọn mô hình kiến trúc phần mềm (Clean Architecture / N-tier Web Application)"
Write-Bullet "  3.2. Cấu trúc tổ chức thư mục và giải pháp công nghệ (.NET 10, Blazor, SQL Server)"
Write-Bullet "  3.3. Thiết kế luồng điều hướng (Site Map & Navigation)"
Write-Bullet "  3.4. Thiết kế chi tiết giao diện người dùng các phân hệ chính"
Write-Bullet "  3.5. Thiết kế đáp ứng đa thiết bị (Responsive Mobile 390px – Desktop)"
Write-Bullet "CHƯƠNG 4: HIỆN THỰC HÓA VÀ CÀI ĐẶT CÁC PHÂN HỆ NGHIỆP VỤ"
Write-Bullet "  4.1. Hiện thực hóa cơ chế Xác thực & Phân quyền đa vai trò (RBAC & BCrypt)"
Write-Bullet "  4.2. Phân hệ Quản trị Nhân sự, Phòng ban và Bảng phân công công việc"
Write-Bullet "  4.3. Phân hệ Quản lý Kênh truyền thông, Nền tảng số và Thống kê Video"
Write-Bullet "  4.4. Phân hệ Quản trị Hợp đồng kinh tế và Nhà sáng tạo nội dung (Creators)"
Write-Bullet "  4.5. Phân hệ Tự động tính lương, Thưởng/Phạt và Báo cáo quyết toán"
Write-Bullet "  4.6. Tích hợp tính năng Xuất Báo Cáo Excel Kế Toán (Mẫu 02-LĐTL)"
Write-Bullet "CHƯƠNG 5: KIỂM THỬ HỆ THỐNG VÀ ĐÁNH GIÁ KẾT QUẢ"
Write-Bullet "  5.1. Môi trường và phương pháp kiểm thử"
Write-Bullet "  5.2. Kịch bản kiểm thử chức năng then chốt (Bảng Test Cases TC01 – TC10)"
Write-Bullet "  5.3. Kiểm thử an toàn bảo mật, chống lỗi IDOR và vượt quyền URL"
Write-Bullet "  5.4. Đánh giá kết quả đạt được và hiệu năng hệ thống"
Write-Bullet "KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN"
Write-Bullet "TÀI LIỆU THAM KHẢO"
$s.InsertBreak(7)

Write-Host "4. Writing Abbreviations Table..."
Write-H1 "DANH MỤC THUẬT NGỮ VÀ KÝ HIỆU VIẾT TẮT"

$abbrData = @(
    @("Viết tắt", "Tên tiếng Anh đầy đủ", "Diễn giải ý nghĩa"),
    @("CUMODI", "Cứ Mơ Đi", "Tên giao dịch Công ty TNHH MTV Cứ Mơ Đi"),
    @("CSDL", "Cơ Sở Dữ Liệu", "Database – Kho dữ liệu quan hệ có cấu trúc"),
    @("ERD", "Entity Relationship Diagram", "Sơ đồ thực thể mối quan hệ"),
    @("MCN", "Multi-Channel Network", "Mạng lưới đa kênh truyền thông kỹ thuật số trực tuyến"),
    @("NSTND / Creator", "Content Creator", "Nhà sáng tạo nội dung tham gia sản xuất video"),
    @("RBAC", "Role-Based Access Control", "Cơ chế kiểm soát truy cập phân quyền dựa trên vai trò"),
    @("API", "Application Programming Interface", "Giao diện lập trình ứng dụng"),
    @("UI / UX", "User Interface / User Experience", "Giao diện người dùng và Trải nghiệm người dùng"),
    @("ORM", "Object-Relational Mapping", "Kỹ thuật ánh xạ đối tượng với cơ sở dữ liệu quan hệ"),
    @("RDBMS", "Relational Database Management System", "Hệ quản trị cơ sở dữ liệu quan hệ (SQL Server, MySQL)"),
    @("IDOR", "Insecure Direct Object References", "Lỗ hổng tham chiếu đối tượng trực tiếp không an toàn"),
    @("REST", "Representational State Transfer", "Phong cách kiến trúc phần mềm mạng phân tán"),
    @("SPA", "Single Page Application", "Ứng dụng web trang đơn tương tác thời gian thực"),
    @("SQL", "Structured Query Language", "Ngôn ngữ truy vấn dữ liệu có cấu trúc chuẩn")
)
Write-Table $abbrData @(90.0, 150.0, 220.0)
$s.InsertBreak(7)

Write-Host "5. Writing Introduction..."
Write-H1 "LỜI MỞ ĐẦU"
Write-P "Trong kỷ nguyên kinh tế số và bùng nổ của mạng xã hội hiện đại, ngành công nghiệp truyền thông số, giải trí đa phương tiện và sản xuất video trực tuyến đã và đang có những bước tiến vượt bậc. Các nền tảng toàn cầu như YouTube, TikTok, Facebook, Instagram trở thành mảnh đất màu mỡ cho các doanh nghiệp sáng tạo nội dung phát triển mô hình kinh doanh, tiếp cận hàng chục triệu người xem và hợp tác với các nhãn hàng danh tiếng."
Write-P "Công ty TNHH MTV Cứ Mơ Đi (CUMODI) là một doanh nghiệp sáng tạo nội dung số tiêu biểu, sở hữu mạng lưới nhiều kênh truyền thông sở hữu lượng người theo dõi lớn (như TogaTV, Hiha, Hachi, Mazk, Mel, Nabee, Sầu Riêng, Yummie, Buron...). Với mô hình hoạt động đa dạng từ việc ký kết hợp đồng quảng cáo tài trợ với đối tác, phân công nhiệm vụ sản xuất video chuyên sâu (viết kịch bản, làm model 3D, dựng bối cảnh, vẽ thumbnail, edit video), phối hợp với đội ngũ nhà sáng tạo nội dung (Creators), quản lý đa nền tảng cho đến tính toán chi trả tiền lương và doanh thu kênh, khối lượng dữ liệu phát sinh hàng ngày là vô cùng lớn và phức tạp."
Write-P "Ở giai đoạn trước, việc thiết kế và xây dựng cơ sở dữ liệu quan hệ cho Công ty Cứ Mơ Đi trong học phần Cơ sở dữ liệu nâng cao đã hoàn thành việc chuẩn hóa cấu trúc 13 bảng thực thể, thiết lập các ràng buộc toàn vẹn, các khung nhìn (Views), thủ tục lưu trữ (Stored Procedures), hàm (Functions) và Trigger tự động hóa tính toán. Tuy nhiên, một hệ thống CSDL chỉ thực sự phát huy tối đa giá trị thực tiễn khi được tích hợp vào một Ứng dụng Web hoàn chỉnh (Full-stack Web Application) – nơi người dùng ở từng vị trí công việc có thể tương tác trực quan, thao tác thời gian thực thông qua trình duyệt web trên máy tính lẫn thiết bị di động."
Write-P "Xuất phát từ yêu cầu thực tiễn đó, dưới sự hướng dẫn của Thầy TS. Hà Ngọc Long, em đã lựa chọn và triển khai đề tài: 'Xây dựng hệ thống ứng dụng Web quản lý hoạt động truyền thông và sản xuất nội dung số Công ty TNHH MTV Cứ Mơ Đi (CUMODI)' cho đồ án học phần Lập trình ứng dụng web. Đề tài giải quyết bài toán chuyển hóa toàn bộ nền tảng dữ liệu nghiệp vụ thành một ứng dụng web hiện đại, an toàn, giao diện thân thiện, phân quyền chặt chẽ và tự động hóa quy trình nghiệp vụ của doanh nghiệp."
$s.InsertBreak(7)

Write-Host "6. Writing Chapter 1..."
Write-H1 "CHƯƠNG 1: TỔNG QUAN VỀ BÀI TOÁN VÀ KHẢO SÁT NGHIỆP VỤ DOANH NGHIỆP"
Write-H2 "1.1. Giới thiệu tổng quan về Công ty TNHH MTV Cứ Mơ Đi (CUMODI)"
Write-P "Công ty TNHH MTV Cứ Mơ Đi là doanh nghiệp hoạt động chuyên nghiệp trong lĩnh vực truyền thông, giải trí kỹ thuật số và sản xuất nội dung đa phương tiện trên các nền tảng mạng xã hội:"
Write-Bullet "Tên doanh nghiệp: CÔNG TY TNHH MTV CỨ MƠ ĐI"
Write-Bullet "Mã doanh nghiệp: 110"
Write-Bullet "Mã số thuế: 0315813288"
Write-Bullet "Ngày thành lập: 30/07/2019"
Write-Bullet "Địa chỉ trụ sở: 378/5 An Dương Vương, Phường Chợ Quán, Thành phố Hồ Chí Minh"
Write-Bullet "Số điện thoại hotline: 0876590909"
Write-Bullet "Email liên hệ: kydstudio@gmail.com"

Write-P "Cơ cấu tổ chức của công ty bao gồm Ban Giám đốc và hệ thống các phòng ban chuyên môn hóa cao:"
Write-Bullet "Phòng Nội dung (Content - Mã: CT01): Phụ trách nghiên cứu xu hướng, lên ý tưởng kịch bản, biên kịch câu chuyện."
Write-Bullet "Phòng Kỹ thuật & Thiết kế (Thiết kế - Mã: TK01 & Animation - Mã: AMT01): Phụ trách dựng model nhân vật 3D, thiết kế đồ họa, vẽ diễn hoạt chuyển động."
Write-Bullet "Phòng Hậu kỳ & Biên tập (Edit - Mã: ED01): Phụ trách cắt ghép, chèn hiệu ứng âm thanh, xử lý kỹ xảo video và xuất bản sản phẩm."
Write-Bullet "Phòng Setup Bối cảnh (Set up - Mã: SU01 & Vẽ - Mã: VE01): Phụ trách xây dựng bối cảnh trường quay, thiết lập map game Minecraft/Roblox và vẽ thumbnail đại diện."
Write-Bullet "Phòng Kế toán (Mã: KT01): Phụ trách quản lý hợp đồng tài trợ, quản trị công nợ, tính lương nhân viên và tổng hợp doanh thu kênh."
Write-Bullet "Phòng Nhân sự (Mã: NS01): Phụ trách hồ sơ nhân viên, chế độ bảo hiểm xã hội, theo dõi tiến độ công việc và hợp tác Creator."

Write-H2 "1.2. Hiện trạng hoạt động và những thách thức trong công tác quản trị"
Write-P "Trong mô hình hoạt động của mạng lưới sáng tạo nội dung số (MCN), công ty phải đối mặt với các bài toán quản lý đặc thù:"
Write-Bullet "Quản lý đa kênh trên đa nền tảng: Mỗi kênh (như TogaTV, Mazk, Hachi...) đồng thời phát sóng trên nhiều nền tảng (YouTube, TikTok, Facebook, Instagram) với các chỉ số lượt xem (views), lượt tương tác (likes, comments) và doanh thu biến động liên tục theo thời gian thực."
Write-Bullet "Mạng lưới cộng tác viên và Creators phong phú: Bên cạnh nhân sự chính thức tại các phòng ban, công ty hợp tác với hàng chục nhà sáng tạo nội dung (Creators) bên ngoài. Mỗi video có thể có sự tham gia của một hoặc nhiều Creators, đòi hỏi cơ chế theo dõi sự đóng góp và thanh toán rõ ràng."
Write-Bullet "Quy trình sản xuất video phân rã nhiều công đoạn: Một video từ lúc lên ý tưởng đến khi phát hành phải trải qua ít nhất 5-7 công việc con (Kịch bản, Model 3D, Setup Map, Quay bối cảnh, Thu âm lồng tiếng, Edit, Thiết kế Thumbnail). Việc quản lý thủ công dễ dẫn đến trễ tiến độ, thất lạc tài nguyên và phân chia khối lượng công việc không đồng đều giữa các nhân sự."
Write-Bullet "Quyết toán tiền lương gắn liền với thưởng phạt và doanh thu: Thu nhập của nhân sự và Creators gồm lương cơ bản, các khoản thưởng theo hiệu suất video/tiến độ công việc và các khoản khấu trừ phạt do vi phạm quy chế hoặc trễ hạn. Việc tính toán thủ công bằng bảng tính Excel rời rạc tiềm ẩn nhiều sai sót và khó bảo mật thông tin thu nhập."

Write-H2 "1.3. Mục tiêu và phạm vi xây dựng ứng dụng Web"
Write-P "Mục tiêu tổng quát: Xây dựng một Cổng thông tin điện tử (Enterprise Web Portal) đồng bộ, hiện đại, cho phép số hóa toàn diện quy trình quản lý nhân sự, phân công công việc, theo dõi hiệu suất video, quản lý hợp đồng đối tác và tự động hóa tính lương cho Công ty Cứ Mơ Đi."
Write-P "Mục tiêu kỹ thuật:"
Write-Bullet "Ứng dụng kiến trúc phân tầng chuẩn mực, phân tách độc lập giữa giao diện người dùng (UI), tầng xử lý nghiệp vụ (Business Logic) và tầng truy xuất cơ sở dữ liệu (Data Access)."
Write-Bullet "Khai thác trọn vẹn và kế thừa cấu trúc CSDL 13 bảng thực thể đã được chuẩn hóa 3NF trong đồ án CSDL nâng cao, tích hợp các View, Stored Procedure, Function và Trigger tự động."
Write-Bullet "Xây dựng giao diện web thân thiện, tương thích tối ưu trên cả máy tính bàn và màn hình điện thoại di động (Responsive Design)."
Write-Bullet "Thiết lập cơ chế xác thực an toàn, mã hóa mật khẩu một chiều BCrypt và phân quyền kiểm soát truy cập nghiêm ngặt theo vai trò (RBAC)."

Write-H2 "1.4. Đối tượng sử dụng và phân quyền hệ thống"
Write-P "Hệ thống phân định 3 nhóm người dùng chính:"
Write-Bullet "Quản trị viên hệ thống (Admin): Toàn quyền cấu hình hệ thống, quản lý tài khoản người dùng, phân quyền vai trò, quản lý danh mục phòng ban, kênh truyền thông và nền tảng."
Write-Bullet "Kế toán & Trưởng phòng Quản lý (Manager / HR-Accountant): Quản lý hợp đồng đối tác, phê duyệt và giao việc cho nhân sự, kiểm soát tiến độ sản xuất video, chạy bảng tính lương tự động, chốt khóa sổ kỳ lương và xuất báo cáo doanh thu/chi trả ngân hàng."
Write-Bullet "Nhân viên & Nhà sáng tạo nội dung (Staff / Creator): Đăng nhập xem bảng công việc được giao, cập nhật tiến độ công việc, tra cứu thông tin kênh/video phụ trách và tra cứu phiếu lương cá nhân (bảo mật nghiêm ngặt chống xem lén lương đồng nghiệp)."
$s.InsertBreak(7)

Write-Host "7. Writing Chapter 2..."
Write-H1 "CHƯƠNG 2: PHÂN TÍCH VÀ THIẾT KẾ HỆ THỐNG ỨNG DỤNG WEB"
Write-H2 "2.1. Phân tích yêu cầu chức năng (Functional Requirements)"
Write-P "Hệ thống được thiết kế với 5 phân hệ chức năng cốt lõi:"
Write-Bullet "Phân hệ 1 - Xác thực người dùng và Quản trị bảo mật (UC_AUTH): Đăng nhập bằng tên đăng nhập và mật khẩu băm mã hóa BCrypt; quản lý phiên làm việc; lưu trữ vai trò người dùng trong Claims; hỗ trợ đổi mật khẩu và đăng xuất an toàn."
Write-Bullet "Phân hệ 2 - Quản lý Nhân sự, Phòng ban và Tiến độ công việc (UC_STAFF_TASK): Quản lý danh bạ nhân sự theo 8 phòng ban; giao việc sản xuất video (Kịch bản, Model 3D, Setup Map, Edit, Vẽ Thumbnail); cập nhật tiến độ hoàn thành (0% – 100%), ngày giao, ngày kết thúc."
Write-Bullet "Phân hệ 3 - Quản trị Kênh truyền thông, Nền tảng và Video (UC_CHANNEL_VIDEO): Quản lý danh mục nền tảng số; quản lý các kênh thương hiệu và nhân sự phụ trách; quản lý video phát hành (views, likes, bình luận); lọc video trending (> 100,000 views) và thống kê lượt xem trung bình."
Write-Bullet "Phân hệ 4 - Quản lý Hợp đồng đối tác và Nhà sáng tạo nội dung (UC_PARTNER_CREATOR): Quản lý hồ sơ đối tác tài trợ; theo dõi hợp đồng kinh tế và giá trị thanh toán; quản lý hồ sơ Creators và danh sách video tham gia."
Write-Bullet "Phân hệ 5 - Tính toán Lương tự động, Thưởng/Phạt và Báo cáo Doanh thu (UC_PAYROLL_REV): Tự động tính lương theo công thức (Lương thành tiền = Lương CB + Thưởng - Phạt); tự động cập nhật doanh thu kênh quy đổi (1 view = 100 VNĐ); xuất báo cáo Excel Kế toán (Mẫu 02-LĐTL) và CSV ngân hàng."

Write-H2 "2.2. Phân tích yêu cầu phi chức năng (Non-Functional Requirements)"
Write-Bullet "Tính tương thích và Responsive: Ứng dụng web hiển thị chuẩn xác trên cả màn hình Desktop (1920x1080) lẫn thiết bị di động (kiểm thử chuẩn DevTools 390px chiều ngang)."
Write-Bullet "Hiệu năng và Tốc độ xử lý: Thời gian phản hồi trang dưới 1.5 giây; các thao tác cập nhật bảng lương và giao việc hàng loạt được quản lý bằng Transaction đảm bảo chuẩn ACID."
Write-Bullet "Bảo mật và An toàn thông tin: 100% mật khẩu được băm một chiều qua BCrypt với Salt ngẫu nhiên; 100% truy vấn CSDL được tham số hóa chống SQL Injection; phòng vệ đa tầng chống lỗ hổng IDOR và chặn người dùng gõ trực tiếp URL vào các phân hệ quản trị (HTTP 403 Forbidden)."

Write-H2 "2.3. Mô hình hóa chức năng bằng sơ đồ Use Case"
Write-P "Sơ đồ ca sử dụng tổng quan mô tả đầy đủ các tác nhân tham gia tương tác với 15 Use Case then chốt thuộc 5 phân hệ chức năng:"
Write-Img "$assetsDir\cumodi_usecase.png" "Hình 2.1: Sơ đồ Use Case tổng quan Cổng thông tin điện tử CUMODI Web Portal"

Write-H2 "2.4. Thiết kế cơ sở dữ liệu quan hệ (Kế thừa và chuẩn hóa 13 bảng thực thể 3NF)"
Write-P "Hệ thống CSDL ứng dụng web kế thừa và đồng bộ 100% từ hồ sơ cơ sở dữ liệu Công ty Cứ Mơ Đi đã đạt chuẩn 3NF, bao gồm 13 bảng thực thể nghiệp vụ:"

$tableSummary = @(
    @("STT", "Tên Bảng CSDL", "Khóa Chính", "Khóa Ngoại Liên Kết", "Mô Tả Chức Năng Nghiệp Vụ"),
    @("1", "CONGTY", "MaCT", "Không", "Lưu thông tin pháp lý công ty Cứ Mơ Đi (MST: 0315813288)"),
    @("2", "PHONGBAN", "MaPB", "MaCT -> CONGTY", "Quản lý 8 phòng ban chức năng và số lượng nhân sự"),
    @("3", "NHANSU", "MaNS", "PHONGBAN_MaPB -> PHONGBAN", "Hồ sơ 21 nhân viên chính thức, chức vụ, lương cơ bản"),
    @("4", "DOITAC", "MaDT", "Không", "Hồ sơ đối tác doanh nghiệp, khách hàng tài trợ quảng cáo"),
    @("5", "HOPDONG", "MaHD", "MaCT, MaDT", "Hợp đồng kinh tế tài trợ, ngày ký, nội dung và giá trị"),
    @("6", "NENTANG", "MaNT", "Không", "Danh mục nền tảng số: YouTube, Facebook, TikTok, Instagram"),
    @("7", "KENH", "MaKenh", "NHANSU_MaNS, CONGTY_MaCT", "Danh mục kênh thương hiệu (TogaTV, Mazk, Hachi, Hiha...)"),
    @("8", "CHITIETKENH", "(MaKenh, MaNT)", "MaKenh, MaNT", "Bảng quan hệ đa kênh phát sóng trên nhiều nền tảng số"),
    @("9", "VIDEO", "MaVideo", "KENH_MaKenh -> KENH", "Quản lý video xuất bản, lượt xem, lượt thích, bình luận"),
    @("10", "NHASANGTAOND", "MaNSTND", "CONGTY_MaCT -> CONGTY", "Hồ sơ Nhà sáng tạo nội dung (Creators), biệt danh, hợp tác"),
    @("11", "CHITIETVIDEO", "(MaVideo, MaNSTND)", "MaVideo, MaNSTND", "Bảng quan hệ video và các Creators tham gia sản xuất"),
    @("12", "CONGVIEC", "MaCV", "MaVideo, MaNSTND", "Công việc sản xuất video (Kịch bản, 3D, Setup, Edit...)"),
    @("13", "CHITIETCONGVIEC", "(MaNS, MaCV)", "MaNS, MaCV", "Phân công nhân sự thực hiện công việc và ngày hoàn thành"),
    @("14", "BANGLUONG", "MaLuong", "MaNS -> NHANSU", "Bảng tính lương từng kỳ, thưởng, phạt, lương thực lĩnh"),
    @("15", "DOANHTHUKENH", "MaKenh", "MaKenh -> KENH", "Doanh thu tích lũy kênh quy đổi theo lượt xem thực tế")
)
Write-Table $tableSummary @(30.0, 90.0, 80.0, 110.0, 150.0)

Write-H2 "2.5. Ràng buộc toàn vẹn, Chỉ mục và Tối ưu hóa truy vấn CSDL"
Write-P "Các ràng buộc toàn vẹn (Integrity Constraints) được cài đặt trực tiếp tại mức CSDL đảm bảo dữ liệu luôn hợp lệ:"
Write-Bullet "CHK_Luong_Duong: Bắt buộc mức lương nhân sự phải luôn không âm: CHECK (Luong >= 0)."
Write-Bullet "CHK_ThoiGianCongViec: Thời gian kết thúc công việc không được trước ngày bắt đầu: CHECK (NgayKetThuc >= NgayBatDau)."
Write-Bullet "CHK_GioiTinh: Giới tính nhân viên chỉ nhận 'Nam' hoặc 'Nữ': CHECK (GioiTinh IN (N'Nam', N'Nữ'))."
Write-Bullet "CHK_EmailDinhDang: Đảm bảo định dạng email hợp lệ: CHECK (Email LIKE '_%@gmail.com')."
Write-Bullet "CHK_NgayKiHopDong: Ngày ký kết hợp đồng phải sau hoặc bằng ngày khởi tạo: CHECK (NgayKiHD >= NgayTaoHD)."
Write-Bullet "CHK_SDT_DinhDang: Số điện thoại liên hệ phải có đúng 10 chữ số: CHECK (SDT LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]')."
$s.InsertBreak(7)

Write-Host "8. Writing Chapter 3..."
Write-H1 "CHƯƠNG 3: THIẾT KẾ KIẾN TRÚC VÀ GIAO DIỆN ỨNG DỤNG WEB (UI/UX)"
Write-H2 "3.1. Lựa chọn mô hình kiến trúc phần mềm"
Write-P "Hệ thống được thiết kế theo mô hình Kiến trúc Phân tầng Hiện đại (Clean Architecture / N-tier Web Application) giúp tách biệt hoàn toàn giữa giao diện, nghiệp vụ và cơ sở dữ liệu:"
Write-Img "$assetsDir\cumodi_architecture.png" "Hình 3.1: Kiến trúc phân tầng Clean Architecture ứng dụng web CUMODI"

Write-P "Nguyên tắc kiến trúc phân tầng áp dụng:"
Write-Bullet "Tầng 1 - Presentation Layer (Giao diện người dùng): Blazor Components, Razor Pages, Bootstrap 5 CSS, ApexCharts / Chart.js Dashboard, Responsive Mobile UI 390px."
Write-Bullet "Tầng 2 - Application Layer (Nghiệp vụ ứng dụng): Các Use Case Services (StaffTaskService, ChannelVideoService, PayrollService, SecurityService) thực thi quy tắc nghiệp vụ và kiểm soát an toàn."
Write-Bullet "Tầng 3 - Core Domain Layer (Mô hình miền & Thực thể): Chứa các lớp thực thể thuần túy (Company, Department, Employee, Channel, Video, Creator, Contract, Task, Payroll) độc lập với công nghệ bên ngoài."
Write-Bullet "Tầng 4 - Infrastructure Layer (Hạ tầng & Truy xuất CSDL): Quản trị kết nối cơ sở dữ liệu, thực thi Dapper Micro-ORM, Database Transactions, gọi Views, Stored Procedures và Triggers."

Write-H2 "3.2. Cấu trúc tổ chức thư mục và giải pháp công nghệ"
Write-P "Dự án sử dụng nền tảng công nghệ Microsoft .NET 10 hiện đại kết hợp C# 13:"
Write-Bullet "Framework: ASP.NET Core Blazor Server với kết nối WebSocket / SignalR cập nhật dữ liệu hai chiều thời gian thực."
Write-Bullet "Hệ quản trị CSDL: Microsoft SQL Server 2022 Enterprise / Express."
Write-Bullet "Thư viện Data Access: Dapper Micro-ORM hiệu năng cao, thực thi câu lệnh SQL tham số hóa chống SQL Injection tuyệt đối."
Write-Bullet "Bảo mật tài khoản: Thư viện BCrypt.Net-Next thực hiện băm mật khẩu với Salt ngẫu nhiên và độ phức tạp WorkFactor = 11."

Write-H2 "3.3. Thiết kế luồng điều hướng (Site Map & Navigation)"
Write-Bullet "Trang Chủ (Dashboard - '/'): Thẻ KPI tổng quan (Số kênh, video xuất bản, tổng lượt xem, tổng doanh thu quy đổi, nhân sự)."
Write-Bullet "Phân hệ Nhân sự & Phân công ('/staff-tasks'): Danh bạ nhân sự theo phòng ban; bảng phân công công việc theo video; cập nhật tiến độ hoàn thành."
Write-Bullet "Phân hệ Kênh & Video Đa Nền Tảng ('/channels-videos'): Danh mục kênh; danh sách video; bộ lọc trending (> 100k views); thống kê lượt xem trung bình."
Write-Bullet "Phân hệ Đối Tác & Hợp Đồng ('/partners-contracts'): Hồ sơ đối tác; danh mục hợp đồng kinh tế; danh sách Creators hợp tác."
Write-Bullet "Phân hệ Tiền Lương & Quyết Toán ('/payroll'): Bảng tính lương tự động; điều chỉnh thưởng/phạt; xuất Excel Mẫu 02-LĐTL và CSV ngân hàng."
Write-Bullet "Trang Cá Nhân & Tra Cứu ('/my-profile', '/my-payslip'): Tra cứu hồ sơ nhân viên; xem phiếu lương cá nhân (bảo vệ Anti-IDOR); đổi mật khẩu."

Write-H2 "3.4. Thiết kế chi tiết giao diện người dùng các phân hệ chính"
Write-P "Giao diện được thiết kế theo phong cách hiện đại (Modern Enterprise SaaS), sử dụng bảng màu sắc chuyên nghiệp (Indigo / Slate Blue), thẻ Card bo góc mềm mại, độ tương phản cao và hiệu ứng chuyển động mượt mà."
Write-P "1. Màn hình Đăng nhập: Form chuẩn với tính năng ẩn/hiện mật khẩu, thông báo lỗi xác thực trực tiếp và thanh Demo Quick-Login chuyển đổi vai trò nhanh chóng phục vụ báo cáo."
Write-P "2. Dashboard Quản trị: 4 thẻ thống kê trực quan (Tổng Lượt Xem, Doanh Thu Quy Đổi, Tổng Video, Tổng Nhân Sự & Creators) kèm biểu đồ phân bổ thị phần lượt xem giữa YouTube, TikTok, Facebook."
Write-P "3. Bảng Phân công Công việc: Bảng quản lý tiến độ trực quan với thanh Progress Bar nhiều màu sắc và Modal cập nhật tiến độ thời gian thực."
Write-P "4. Bảng Tính Lương & Xuất Báo Cáo: Thống kê Quỹ lương gộp, Tổng tiền thưởng, Tổng tiền phạt, Thực lĩnh chuyển khoản và nút xuất Excel 1-chạm."

Write-H2 "3.5. Thiết kế đáp ứng đa thiết bị (Responsive Design 390px Mobile - Desktop)"
Write-P "Hệ thống đáp ứng trọn vẹn tiêu chuẩn kiểm thử di động bề ngang hẹp 390px (chuẩn iPhone):"
Write-Bullet "Thanh điều hướng Top Navigation tự động thu gọn vào nút Menu Hamburger khi màn hình dưới 768px."
Write-Bullet "Các bảng dữ liệu được bao bọc trong khối .table-responsive, cho phép vuốt cuộn ngang mượt mà bằng thao tác ngón tay mà không làm tràn khung hình."
Write-Bullet "Các nút bấm hành động (Chi tiết, Cập nhật, Xuất file) có kích thước tối thiểu 40x40px, thân thiện tuyệt đối với thao tác chạm cảm ứng."
$s.InsertBreak(7)

Write-Host "9. Writing Chapter 4..."
Write-H1 "CHƯƠNG 4: HIỆN THỰC HÓA VÀ CÀI ĐẶT CÁC PHÂN HỆ NGHIỆP VỤ"
Write-H2 "4.1. Hiện thực hóa cơ chế Xác thực & Phân quyền đa vai trò (RBAC & BCrypt)"
Write-P "Hệ thống cài đặt dịch vụ xác thực tùy biến CustomAuthenticationStateProvider. Mật khẩu được băm và đối soát qua thuật toán BCrypt.Net. Khi đăng nhập thành công, Claims của người dùng được lưu trữ bao gồm Tên tài khoản, Vai trò (ROLE_ADMIN, ROLE_MANAGER, ROLE_EMPLOYEE) và Mã nhân sự liên kết."
Write-P "Tại tầng Router, hệ thống thiết lập bộ lọc AuthorizeRouteView chặn đứng các truy cập trái phép khi người dùng cố tình gõ trực tiếp URL vào thanh địa chỉ trình duyệt:"

$codeAuth = @"
// Bảo vệ Route phân quyền quản trị
@page "/admin/payroll"
@attribute [Authorize(Roles = "ROLE_ADMIN, ROLE_MANAGER")]

// Cấu hình Router tại App.razor / Routes.razor
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

Write-H2 "4.2. Phân hệ Quản trị Nhân sự, Phòng ban và Bảng phân công công việc"
Write-P "Ứng dụng web hiển thị danh sách nhân sự từ bảng NHANSU kết hợp PHONGBAN. Tích hợp Stored Procedure USP_TimKiemNhanSu hỗ trợ tìm kiếm nhanh theo phòng ban hoặc vị trí chức vụ:"

$codeProc1 = @"
CREATE PROCEDURE USP_TimKiemNhanSu
    @p_TenPB NVARCHAR(50),
    @p_ChucVu NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ns.MaNS, ns.HoTenNS, ns.ChucVu, pb.TenPB, ns.Luong
    FROM nhansu ns 
    JOIN phongban pb ON ns.PHONGBAN_MaPB = pb.MaPB
    WHERE pb.TenPB LIKE '%' + @p_TenPB + '%'
      AND (@p_ChucVu IS NULL OR ns.ChucVu = @p_ChucVu);
END;
"@
Write-CodeBlock $codeProc1
Write-P "Khi phân công một công việc mới trong bảng CHITIETCONGVIEC, Trigger UTG_CapNhatSoCongViec_Them sẽ tự động kích hoạt và tăng số đếm TongCongViec của nhân viên trong bảng NHANSU."

Write-H2 "4.3. Phân hệ Quản lý Kênh truyền thông, Nền tảng số và Thống kê Video"
Write-P "Phân hệ quản lý kênh và video tích hợp các View phân tích dữ liệu trực tiếp từ CSDL:"
Write-Bullet "View SoLuong_VideoTrendingNenTang: Lọc danh sách các video có trên 100,000 lượt xem và gom nhóm theo kênh và nền tảng phát sóng."
Write-Bullet "View ThongKeKenh: Thống kê tổng số video, tổng lượt xem, tổng lượt thích và tính toán lượt xem trung bình cho từng kênh:"

$codeView = @"
CREATE VIEW ThongKeKenh AS
SELECT
    k.MaKenh,
    k.TenKenh,
    k.NgayTaoKenh,
    COUNT(v.MaVideo) AS TongSoVideo,
    SUM(v.LuotXem) AS TongLuotXem,
    SUM(v.LuotThich) AS TongLuotThich,
    ISNULL(ROUND(AVG(CAST(v.LuotXem AS FLOAT)), 0), 0) AS ViewTrungBinh
FROM KENH k
LEFT JOIN VIDEO v ON k.MaKenh = v.KENH_MaKenh
GROUP BY k.MaKenh, k.TenKenh, k.NgayTaoKenh;
"@
Write-CodeBlock $codeView

Write-H2 "4.4. Phân hệ Quản trị Hợp đồng kinh tế và Nhà sáng tạo nội dung (Creators)"
Write-P "Hệ thống quản lý hợp đồng tài trợ và hồ sơ Creators, tích hợp hàm Function UF_DemSoVideoCuaCreator đếm tổng số video mà Creator đã tham gia sản xuất:"

$codeFunc = @"
CREATE FUNCTION UF_DemSoVideoCuaCreator (@p_MaNSTND VARCHAR(10))
RETURNS INT
AS
BEGIN
    DECLARE @v_SoLuong INT;
    SELECT @v_SoLuong = COUNT(*)
    FROM chitietvideo 
    WHERE NHASANGTAOND_MaNSTND = @p_MaNSTND;
    RETURN ISNULL(@v_SoLuong, 0);
END;
"@
Write-CodeBlock $codeFunc

Write-H2 "4.5. Phân hệ Tự động tính lương, Thưởng/Phạt và Báo cáo quyết toán"
Write-P "Quy trình tính lương và quyết toán tài chính được thực thi thông qua cơ chế Database Transaction đảm bảo nguyên tắc ACID:"
Write-Img "$assetsDir\cumodi_sequence_payroll.png" "Hình 4.1: Sơ đồ tuần tự xử lý tính lương hàng loạt với Database Transaction & Trigger"

$codeTrans = @"
// Thực thi tính toán bảng lương trong Database Transaction
using var connection = new SqlConnection(_connectionString);
await connection.OpenAsync();
using var transaction = connection.BeginTransaction();

try
{
    // 1. Ghi nhận dữ liệu vào bảng BANGLUONG và kích hoạt Trigger
    await connection.ExecuteAsync(sqlInsertSalary, parameters, transaction);

    // 2. Cập nhật trạng thái quyết toán kỳ lương
    await connection.ExecuteAsync(sqlUpdateStatus, statusParams, transaction);

    // 3. Cam kết giao dịch thành công (COMMIT)
    transaction.Commit();
}
catch (Exception ex)
{
    // Hoàn nguyên toàn bộ số liệu nếu có lỗi phát sinh (ROLLBACK)
    transaction.Rollback();
    throw new ApplicationException("Lỗi trong quá trình tính lương: " + ex.Message);
}
"@
Write-CodeBlock $codeTrans

Write-P "Cặp Trigger UTG_TinhLuong_Them và UTG_TinhLuong_Sua tự động tính toán mức lương thực lĩnh ngay trong CSDL theo công thức chuẩn:"
Write-P "LuongThanhTien = LuongCoBan + Thuong - Phat"

Write-H2 "4.6. Tích hợp tính năng Xuất Báo Cáo Excel Kế Toán (Mẫu 02-LĐTL)"
Write-P "Ứng dụng web cung cấp tính năng xuất bảng lương ra tệp Excel định dạng Mẫu số 02-LĐTL của Bộ Tài chính: Tiêu đề công ty Cứ Mơ Đi, đầy đủ thông tin nhân viên, phòng ban, lương cơ bản, các khoản thưởng/phạt và lương thực lĩnh với định dạng tiền tệ kế toán chuẩn (#,##0 VNĐ)."
$s.InsertBreak(7)

Write-Host "10. Writing Chapter 5..."
Write-H1 "CHƯƠNG 5: KIỂM THỬ HỆ THỐNG VÀ ĐÁNH GIÁ KẾT QUẢ"
Write-H2 "5.1. Môi trường và phương pháp kiểm thử"
Write-Bullet "Môi trường phần cứng: Máy tính CPU Intel Core i7 / AMD Ryzen, RAM 16GB, SSD NVMe."
Write-Bullet "Môi trường phần mềm: Windows 11 64-bit, Microsoft SQL Server 2022 Developer Edition, .NET 10 SDK, Google Chrome & Microsoft Edge."
Write-Bullet "Phương pháp kiểm thử: Kiểm thử hộp đen (Black-box Testing) chức năng giao diện, kiểm thử quy trình nghiệp vụ đầu cuối (End-to-End Testing) và kiểm thử giao diện co giãn Responsive trên thiết bị di động (390px)."

Write-H2 "5.2. Kịch bản kiểm thử chức năng then chốt (Test Cases)"
Write-P "Dưới đây là bảng tổng hợp kết quả thực thi 10 kịch bản kiểm thử then chốt của ứng dụng web:"

$testCasesData = @(
    @("Mã TC", "Chức Năng Kiểm Thử", "Dữ Liệu Đầu Vào", "Kết Quả Kỳ Vọng", "Kết Quả Thực Tế", "Đánh Giá"),
    @("TC01", "Đăng nhập tài khoản hợp lệ", "user: admin.trong, pass: 123456", "Đăng nhập thành công, nạp Claims ROLE_ADMIN, mở Dashboard.", "Đăng nhập thành công, điều hướng chuẩn xác.", "ĐẠT"),
    @("TC02", "Đăng nhập mật khẩu sai", "user: nhanvien.a, pass: 999999", "Bị từ chối, cảnh báo đỏ cạnh ô nhập, mật khẩu được bảo vệ.", "Báo lỗi chính xác, không văng lỗi hệ thống.", "ĐẠT"),
    @("TC03", "Ràng buộc kiểm tra Lương âm", "Cập nhật Luong = -100 trên bảng NHANSU", "Ràng buộc CHK_Luong_Duong kích hoạt, từ chối cập nhật.", "CSDL từ chối, web hiển thị cảnh báo thân thiện.", "ĐẠT"),
    @("TC04", "Ràng buộc Ngày kết thúc việc", "Nhập NgayKetThuc < NgayBatDau", "Ràng buộc CHK_ThoiGianCongViec chặn lưu dữ liệu.", "Hệ thống chặn và báo lỗi validation ngày tháng.", "ĐẠT"),
    @("TC05", "Phân công việc & Trigger", "Thêm công việc cho nhân viên NS018", "Trigger UTG_CapNhatSoCongViec tự động tăng TongCongViec.", "Số lượng công việc tăng tức thì trên giao diện.", "ĐẠT"),
    @("TC06", "Thống kê Video Trending", "Truy vấn View Trending (> 100k views)", "Chỉ hiển thị các video có LuotXem > 100.000 theo kênh.", "Hiển thị chính xác 23 video trending mẫu.", "ĐẠT"),
    @("TC07", "Tự động tính lương Thưởng/Phạt", "Thưởng 500k, Phạt 100k cho NS012", "Trigger tính: Lương CB (13tr) + 500k - 100k = 13.400.000 đ.", "Bảng lương hiển thị chính xác 13.400.000 đ.", "ĐẠT"),
    @("TC08", "Chặn vượt quyền URL trực tiếp", "Đăng nhập nhanvien.a, gõ URL /admin/payroll", "Hệ thống chặn tại Router, hiển thị màn hình 403 Forbidden.", "Hiển thị màn hình 403, bảo vệ dữ liệu an toàn.", "ĐẠT"),
    @("TC09", "Xuất bảng lương Excel", "Bấm nút Xuất Excel Bảng Lương", "Tải xuống file .xls chuẩn Mẫu 02-LĐTL đầy đủ cột mục.", "Tải file thành công, định dạng số tiền chuẩn xác.", "ĐẠT"),
    @("TC10", "Hiển thị trên Mobile 390px", "Mở DevTools co về bề ngang 390px", "Menu thu gọn hamburger, bảng cuộn ngang mượt mà.", "Giao diện hiển thị chuẩn đẹp, không vỡ layout.", "ĐẠT")
)
Write-Table $testCasesData @(35.0, 95.0, 85.0, 115.0, 95.0, 35.0)

Write-H2 "5.3. Kiểm thử an toàn bảo mật, chống lỗi IDOR và vượt quyền URL"
Write-P "1. Kiểm thử chống lỗi IDOR: Khi nhân viên NS005 đăng nhập vào xem phiếu lương cá nhân, hệ thống tự động gắn chặt truy vấn với Claim EmployeeId của chính tài khoản đó. Nếu nhân viên cố tình thay đổi tham số ID trên thanh địa chỉ thành mã nhân viên khác (ví dụ: NS002), hệ thống lập tức từ chối và báo lỗi 'Bạn không có quyền xem thông tin của nhân sự này'."
Write-P "2. Kiểm thử an toàn cơ sở dữ liệu: 100% các thao tác tìm kiếm nhân sự, lọc video, cập nhật công việc và tính lương đều truyền qua tham số hóa SQL (Parameters), ngăn chặn triệt để mọi hành vi tiêm mã độc SQL Injection."

Write-H2 "5.4. Đánh giá kết quả đạt được và hiệu năng hệ thống"
Write-Bullet "Về mặt học thuật và nghiệp vụ: Kế thừa và phát triển trọn vẹn toàn bộ 13 bảng thực thể nghiệp vụ, hệ thống ràng buộc toàn vẹn, khung nhìn, thủ tục và trigger của đồ án CSDL Công ty Cứ Mơ Đi sang một ứng dụng web hoàn chỉnh; phân quyền chặt chẽ 3 vai trò Admin, Manager và Staff."
Write-Bullet "Về mặt công nghệ và kỹ thuật web: Áp dụng thành công mô hình Clean Architecture; bảo mật xác thực BCrypt và phân quyền RBAC; tự động hóa tính lương và doanh thu; giao diện trực quan và tương thích hoàn hảo trên thiết bị di động 390px."
$s.InsertBreak(7)

Write-Host "11. Writing Conclusion & Future Scope..."
Write-H1 "KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN"
Write-H2 "1. Kết luận"
Write-P "Sau quá trình nghiên cứu lý thuyết và hiện thực hóa dưới sự hướng dẫn của Thầy TS. Hà Ngọc Long, đồ án học phần Lập trình ứng dụng web với đề tài: 'Xây dựng hệ thống ứng dụng Web quản lý hoạt động truyền thông và sản xuất nội dung số Công ty TNHH MTV Cứ Mơ Đi (CUMODI)' đã hoàn thành đầy đủ các mục tiêu đề ra:"
Write-Bullet "Phân tích và mô hình hóa thành công bài toán quản trị thực tế của một doanh nghiệp truyền thông số đa kênh hiện đại."
Write-Bullet "Chuyển hóa toàn diện hệ thống CSDL quan hệ 13 bảng 3NF thành các phân hệ nghiệp vụ web tương tác trực quan thời gian thực."
Write-Bullet "Cài đặt thành công các cơ chế bảo mật quan trọng: Xác thực BCrypt, phân quyền vai trò RBAC, ngăn chặn truy cập trái phép mức Router và mức trang."
Write-Bullet "Tự động hóa các nghiệp vụ phức tạp như phân công công việc, thống kê hiệu suất video trending, cập nhật doanh thu kênh và tính toán lương thưởng phạt bằng Database Triggers và Transactions."
Write-Bullet "Xây dựng thành công tính năng xuất báo cáo tài chính kế toán định dạng Excel chuẩn Mẫu số 02-LĐTL và file CSV chuyển khoản ngân hàng."
Write-P "Đồ án là minh chứng rõ nét cho việc kết hợp nhuần nhuyễn giữa kiến thức cơ sở dữ liệu nâng cao và kỹ năng lập trình phát triển ứng dụng web hiện đại."

Write-H2 "2. Hướng phát triển trong tương lai"
Write-Bullet "Tích hợp API trực tiếp từ các nền tảng mạng xã hội: Kết nối YouTube Data API v3, TikTok for Developers API và Facebook Graph API để tự động đồng bộ lượt xem, lượt thích và doanh thu theo thời gian thực."
Write-Bullet "Ứng dụng Trí tuệ nhân tạo (AI): Tích hợp mô hình ngôn ngữ lớn (Gemini API) hỗ trợ gợi ý kịch bản video theo xu hướng thị trường, tự động tạo mô tả video chuẩn SEO và phân tích cảm xúc bình luận của khán giả."
Write-Bullet "Cổng thanh toán điện tử: Tích hợp các cổng thanh toán số (VNPay, VietQR) để chi trả lương và tiền tài trợ hợp đồng tức thì cho các Creators."
Write-Bullet "Phát triển ứng dụng di động bản địa (Native Mobile App / PWA): Tối ưu hóa tính năng thông báo đẩy (Push Notification) khi nhân viên được giao việc mới hoặc khi có biến động doanh thu kênh."
$s.InsertBreak(7)

Write-Host "12. Writing References..."
Write-H1 "TÀI LIỆU THAM KHẢO"
Write-Bullet "1. TS. Hà Ngọc Long, Bài giảng và tài liệu hướng dẫn học phần Lập trình ứng dụng web, Khoa Hệ thống Thông tin Kinh tế – Trường Đại học Kinh tế, Đại học Huế."
Write-Bullet "2. TS. Hoàng Hữu Trung, Bài giảng và giáo trình Cơ sở dữ liệu nâng cao, Trường Đại học Kinh tế – Đại học Huế."
Write-Bullet "3. Trần Viết Mạnh Cường (2026), Báo cáo đồ án Cơ sở dữ liệu nâng cao: Xây dựng cơ sở dữ liệu quản lý Công ty TNHH MTV Cứ Mơ Đi, Đại học Huế."
Write-Bullet "4. Microsoft Documentation (2025 – 2026), ASP.NET Core & Blazor Web Applications Documentation, Microsoft Learn Platform."
Write-Bullet "5. Robert C. Martin (2017), Clean Architecture: A Craftsman's Guide to Software Structure and Design, Prentice Hall."
Write-Bullet "6. Bộ Tài chính (2013 – 2024), Thông tư 111/2013/TT-BTC về thuế TNCN và Chế độ chứng từ kế toán tiền lương Mẫu số 02-LĐTL, NXB Tài Chính, Hà Nội."
Write-Bullet "7. W3Schools & MDN Web Docs (2025), Modern HTML5, CSS3, JavaScript and Responsive Web Design Standards."

# Save Document
$outputPath = "c:\Users\TUF\Desktop\bangChamCong\BAO_CAO_DO_AN_WEB_CUMODI_TranVietManhCuong.docx"
Write-Host "Saving document to $outputPath..."
$doc.SaveAs([ref]$outputPath, [ref]16)
$doc.Close()
$word.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null
[System.GC]::Collect()
[System.GC]::WaitForPendingFinalizers()

$fileSize = (Get-Item $outputPath).Length
Write-Host "SUCCESS! Formal Word Document created: $outputPath ($fileSize bytes)"
