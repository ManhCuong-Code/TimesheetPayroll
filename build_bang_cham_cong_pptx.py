import os
import sys
from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.enum.text import PP_ALIGN
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml import parse_xml
from pptx.oxml.ns import nsdecls

# -------------------------------------------------------------
# COLOR PALETTE (Derived directly from CCong.png & Web UI)
# -------------------------------------------------------------
COLOR_PRIMARY_NAVY = RGBColor(30, 58, 138)    # #1E3A8A - Dominant Blue from CCong.png
COLOR_NAVY_DARK = RGBColor(15, 23, 42)        # #0F172A - Deep Slate/Navy
COLOR_BLUE_ROYAL = RGBColor(37, 99, 235)      # #2563EB - Royal Blue Accent
COLOR_CYAN = RGBColor(14, 165, 233)           # #0EA5E9 - Tech Cyan
COLOR_TEAL = RGBColor(13, 148, 136)           # #0D9488 - Teal Accent
COLOR_EMERALD = RGBColor(16, 185, 129)        # #10B981 - Success Emerald
COLOR_AMBER = RGBColor(245, 158, 11)          # #F59E0B - Amber Accent
COLOR_ROSE = RGBColor(239, 68, 68)            # #EF4444 - Warning Rose

COLOR_BG_LIGHT = RGBColor(248, 250, 252)      # #F8FAFC - Light Background
COLOR_CARD_BG = RGBColor(255, 255, 255)       # #FFFFFF - Card Background
COLOR_BORDER = RGBColor(226, 232, 240)        # #E2E8F0 - Subtle Border
COLOR_TEXT_MAIN = RGBColor(15, 23, 42)        # #0F172A - Main Text
COLOR_TEXT_MUTED = RGBColor(71, 85, 105)      # #475569 - Secondary Text
COLOR_WHITE = RGBColor(255, 255, 255)         # #FFFFFF

FONT_FAMILY = "Segoe UI"

def set_flat_style(shape, fill_color, border_color=None, border_width=1):
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill_color
    if border_color:
        shape.line.color.rgb = border_color
        shape.line.width = Pt(border_width)
    else:
        shape.line.fill.background()

def add_header(slide, tag_text, title_text, subtitle_text=""):
    # Top accent line
    top_line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.08))
    set_flat_style(top_line, COLOR_PRIMARY_NAVY)
    
    # Tag pill
    tag = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(0.35), Inches(4.6), Inches(0.32))
    set_flat_style(tag, RGBColor(238, 242, 255), RGBColor(199, 210, 254), 1)
    tf_tag = tag.text_frame
    tf_tag.word_wrap = True
    tf_tag.margin_left = tf_tag.margin_top = tf_tag.margin_right = tf_tag.margin_bottom = 0
    p_tag = tf_tag.paragraphs[0]
    p_tag.alignment = PP_ALIGN.CENTER
    r_tag = p_tag.add_run()
    r_tag.text = tag_text.upper()
    r_tag.font.name = FONT_FAMILY
    r_tag.font.size = Pt(13.5)
    r_tag.font.bold = True
    r_tag.font.color.rgb = COLOR_PRIMARY_NAVY
    
    # Title box
    tb = slide.shapes.add_textbox(Inches(0.8), Inches(0.72), Inches(11.733), Inches(0.85))
    tf = tb.text_frame
    tf.word_wrap = True
    tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
    p_title = tf.paragraphs[0]
    p_title.space_after = Pt(2)
    r_title = p_title.add_run()
    r_title.text = title_text
    r_title.font.name = FONT_FAMILY
    r_title.font.size = Pt(28)
    r_title.font.bold = True
    r_title.font.color.rgb = COLOR_NAVY_DARK
    
    if subtitle_text:
        p_sub = tf.add_paragraph()
        r_sub = p_sub.add_run()
        r_sub.text = subtitle_text
        r_sub.font.name = FONT_FAMILY
        r_sub.font.size = Pt(15)
        r_sub.font.color.rgb = COLOR_TEXT_MUTED

def add_footer(slide, current_slide, total_slides=12):
    line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.8), Inches(7.0), Inches(11.733), Inches(0.015))
    set_flat_style(line, COLOR_BORDER)
    
    # Left
    tb_l = slide.shapes.add_textbox(Inches(0.8), Inches(7.05), Inches(5.5), Inches(0.35))
    tf_l = tb_l.text_frame
    tf_l.margin_left = tf_l.margin_top = tf_l.margin_right = tf_l.margin_bottom = 0
    p_l = tf_l.paragraphs[0]
    r_l = p_l.add_run()
    r_l.text = "Lập trình ứng dụng Web | Hệ thống Chấm công & Quản lý nhân viên"
    r_l.font.name = FONT_FAMILY
    r_l.font.size = Pt(12.5)
    r_l.font.color.rgb = COLOR_TEXT_MUTED
    
    # Center
    tb_c = slide.shapes.add_textbox(Inches(5.5), Inches(7.05), Inches(5.0), Inches(0.35))
    tf_c = tb_c.text_frame
    tf_c.margin_left = tf_c.margin_top = tf_c.margin_right = tf_c.margin_bottom = 0
    p_c = tf_c.paragraphs[0]
    r_c = p_c.add_run()
    r_c.text = "Trần Viết Mạnh Cường (23K4080003) | ĐH Kinh tế Huế"
    r_c.font.name = FONT_FAMILY
    r_c.font.size = Pt(12.5)
    r_c.font.color.rgb = COLOR_TEXT_MUTED
    
    # Right Badge
    badge = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(11.733), Inches(7.05), Inches(0.8), Inches(0.28))
    set_flat_style(badge, COLOR_PRIMARY_NAVY)
    tf_b = badge.text_frame
    tf_b.margin_left = tf_b.margin_top = tf_b.margin_right = tf_b.margin_bottom = 0
    p_b = tf_b.paragraphs[0]
    p_b.alignment = PP_ALIGN.CENTER
    r_b = p_b.add_run()
    r_b.text = f"{current_slide:02d} / {total_slides:02d}"
    r_b.font.name = FONT_FAMILY
    r_b.font.size = Pt(12.5)
    r_b.font.bold = True
    r_b.font.color.rgb = COLOR_WHITE

def add_card(slide, left, top, width, height, stripe_color=None, bg_color=COLOR_CARD_BG, border_color=COLOR_BORDER):
    card = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
    set_flat_style(card, bg_color, border_color, 1)
    if stripe_color:
        stripe = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, Inches(0.08))
        set_flat_style(stripe, stripe_color)
    return card

def add_speaker_note(slide, note_text):
    notes_slide = slide.notes_slide
    tf = notes_slide.notes_text_frame
    tf.text = note_text

def add_transition_to_slide(slide, trans_type='fade', spd='med', dir_val='r'):
    for child in list(slide._element):
        if child.tag.endswith('transition'):
            slide._element.remove(child)
            
    if trans_type.startswith('<p:'):
        inner = trans_type
    elif trans_type == 'fade':
        inner = '<p:fade/>'
    elif trans_type == 'push':
        inner = f'<p:push dir="{dir_val}"/>'
    elif trans_type == 'wipe':
        inner = f'<p:wipe dir="{dir_val}"/>'
    elif trans_type == 'cover':
        inner = f'<p:cover dir="{dir_val}"/>'
    elif trans_type == 'split':
        inner = '<p:split orient="horz" dir="out"/>'
    elif trans_type == 'comb':
        inner = '<p:comb orient="horz"/>'
    elif trans_type == 'zoom':
        inner = '<p:zoom dir="in"/>'
    elif trans_type == 'circle':
        inner = '<p:circle/>'
    elif trans_type == 'diamond':
        inner = '<p:diamond/>'
    elif trans_type == 'blinds':
        inner = '<p:blinds dir="vert"/>'
    elif trans_type == 'checker':
        inner = '<p:checker dir="horz"/>'
    elif trans_type == 'wheel':
        inner = '<p:wheel spokes="4"/>'
    elif trans_type == 'plus':
        inner = '<p:plus/>'
    elif trans_type == 'wedge':
        inner = '<p:wedge/>'
    elif trans_type == 'dissolve':
        inner = '<p:dissolve/>'
    else:
        inner = '<p:fade/>'
        
    xml_str = f'<p:transition {nsdecls("p")} spd="{spd}" advClick="1">{inner}</p:transition>'
    trans_elem = parse_xml(xml_str)
    
    inserted = False
    for idx, child in enumerate(slide._element):
        if child.tag.endswith('timing') or child.tag.endswith('extLst'):
            slide._element.insert(idx, trans_elem)
            inserted = True
            break
    if not inserted:
        slide._element.append(trans_elem)

def build_presentation():
    prs = Presentation()
    prs.slide_width = Inches(13.333)
    prs.slide_height = Inches(7.5)
    blank_layout = prs.slide_layouts[6]
    
    # =============================================================
    # SLIDE 1: TRANG BÌA (COVER SLIDE - THEMED AFTER CCONG.PNG)
    # =============================================================
    s1 = prs.slides.add_slide(blank_layout)
    bg1 = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg1, COLOR_PRIMARY_NAVY) # Exact Dominant Blue (#1E3A8A) from CCong.png
    
    # Top subtle glowing cyan stripe
    top_glow = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.1))
    set_flat_style(top_glow, COLOR_CYAN)
    
    # University Header Text & HCE Logo
    hce_logo = "report_assets/hce_logo.png"
    if os.path.exists(hce_logo):
        s1.shapes.add_picture(hce_logo, Inches(0.8), Inches(0.55), width=Inches(1.05))
        
    tb_uni = s1.shapes.add_textbox(Inches(2.05), Inches(0.55), Inches(10.4), Inches(0.9))
    tf_uni = tb_uni.text_frame
    tf_uni.word_wrap = True
    tf_uni.margin_left = tf_uni.margin_top = tf_uni.margin_right = tf_uni.margin_bottom = 0
    p1 = tf_uni.paragraphs[0]
    r1 = p1.add_run()
    r1.text = "TRƯỜNG ĐẠI HỌC KINH TẾ, ĐẠI HỌC HUẾ"
    r1.font.name = FONT_FAMILY
    r1.font.size = Pt(17.5)
    r1.font.bold = True
    r1.font.color.rgb = COLOR_WHITE
    
    p2 = tf_uni.add_paragraph()
    r2 = p2.add_run()
    r2.text = "KHOA HỆ THỐNG THÔNG TIN KINH TẾ"
    r2.font.name = FONT_FAMILY
    r2.font.size = Pt(15)
    r2.font.bold = True
    r2.font.color.rgb = RGBColor(199, 210, 254)
    
    # Course Badge Tag
    badge_c = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(1.85), Inches(4.3), Inches(0.38))
    set_flat_style(badge_c, RGBColor(23, 37, 84), COLOR_CYAN, 1.2)
    tf_bc = badge_c.text_frame
    tf_bc.margin_left = tf_bc.margin_top = tf_bc.margin_right = tf_bc.margin_bottom = 0
    p_bc = tf_bc.paragraphs[0]
    p_bc.alignment = PP_ALIGN.CENTER
    r_bc = p_bc.add_run()
    r_bc.text = "BÁO CÁO HỌC PHẦN: LẬP TRÌNH ỨNG DỤNG WEB"
    r_bc.font.name = FONT_FAMILY
    r_bc.font.size = Pt(14)
    r_bc.font.bold = True
    r_bc.font.color.rgb = COLOR_CYAN
    
    # Main Topic Title
    tb_title = s1.shapes.add_textbox(Inches(0.8), Inches(2.4), Inches(8.5), Inches(1.6))
    tf_title = tb_title.text_frame
    tf_title.word_wrap = True
    tf_title.margin_left = tf_title.margin_top = tf_title.margin_right = tf_title.margin_bottom = 0
    
    p_t = tf_title.paragraphs[0]
    r_t = p_t.add_run()
    r_t.text = "XÂY DỰNG HỆ THỐNG CHẤM CÔNG\nVÀ QUẢN LÝ NHÂN VIÊN\nTRÊN NỀN TẢNG WEB"
    r_t.font.name = FONT_FAMILY
    r_t.font.size = Pt(32)
    r_t.font.bold = True
    r_t.font.color.rgb = COLOR_WHITE
    
    # Subtitle English
    p_sub = tf_title.add_paragraph()
    p_sub.space_before = Pt(8)
    r_sub = p_sub.add_run()
    r_sub.text = "Web-based Timesheet & Employee Management System"
    r_sub.font.name = FONT_FAMILY
    r_sub.font.size = Pt(17.5)
    r_sub.font.color.rgb = RGBColor(199, 210, 254)
    
    # Project Logo / Illustration Card on the Right (CCong.png)
    ccong_path = "CCong.png"
    if os.path.exists(ccong_path):
        logo_card = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(9.5), Inches(1.9), Inches(3.033), Inches(2.75))
        set_flat_style(logo_card, COLOR_WHITE, COLOR_CYAN, 1.5)
        s1.shapes.add_picture(ccong_path, Inches(9.6), Inches(2.1), width=Inches(2.833))
        
        # Caption below logo
        tb_cap = s1.shapes.add_textbox(Inches(9.5), Inches(4.35), Inches(3.033), Inches(0.3))
        tf_cap = tb_cap.text_frame
        tf_cap.margin_left = tf_cap.margin_top = tf_cap.margin_right = tf_cap.margin_bottom = 0
        p_cap = tf_cap.paragraphs[0]
        p_cap.alignment = PP_ALIGN.CENTER
        r_cap = p_cap.add_run()
        r_cap.text = "Nhận diện Hệ thống Chấm công"
        r_cap.font.name = FONT_FAMILY
        r_cap.font.size = Pt(12.5)
        r_cap.font.bold = True
        r_cap.font.color.rgb = COLOR_PRIMARY_NAVY
    
    # Bottom Student & Teacher Metadata Box
    meta_box = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(4.9), Inches(11.733), Inches(1.9))
    set_flat_style(meta_box, RGBColor(23, 37, 84), RGBColor(59, 130, 246), 1.2)
    
    metadata_cols = [
        ("SINH VIÊN THỰC HIỆN", "Trần Viết Mạnh Cường", "Mã SV: 23K4080003\nLớp: K57 Tin học Kinh tế"),
        ("NGÀNH & CHUYÊN NGÀNH", "Hệ thống Thông tin Quản lý", "Chuyên ngành: Tin học Kinh tế\nKhóa học: 2023 – 2027"),
        ("GIẢNG VIÊN HƯỚNG DẪN", "[Để trống để bổ sung]", "Khoa Hệ thống Thông tin Kinh tế\nTrường Đại học Kinh tế, ĐH Huế"),
        ("THÔNG TIN BÁO CÁO", "Học kỳ I (2025 – 2026)", "Học phần: Lập trình ứng dụng Web\nThành phố Huế, Năm 2026")
    ]
    col_w = Inches(2.78)
    for idx, (m_tag, m_name, m_desc) in enumerate(metadata_cols):
        mx = Inches(0.95 + idx * 2.9)
        tb_m = s1.shapes.add_textbox(mx, Inches(5.05), col_w, Inches(1.6))
        tf_m = tb_m.text_frame
        tf_m.word_wrap = True
        tf_m.margin_left = tf_m.margin_top = tf_m.margin_right = tf_m.margin_bottom = 0
        
        pm1 = tf_m.paragraphs[0]
        rm1 = pm1.add_run()
        rm1.text = m_tag
        rm1.font.name = FONT_FAMILY
        rm1.font.size = Pt(12.5)
        rm1.font.bold = True
        rm1.font.color.rgb = COLOR_CYAN
        
        pm2 = tf_m.add_paragraph()
        pm2.space_before = Pt(4)
        rm2 = pm2.add_run()
        rm2.text = m_name
        rm2.font.name = FONT_FAMILY
        rm2.font.size = Pt(16.5)
        rm2.font.bold = True
        rm2.font.color.rgb = COLOR_WHITE
        
        pm3 = tf_m.add_paragraph()
        pm3.space_before = Pt(3)
        rm3 = pm3.add_run()
        rm3.text = m_desc
        rm3.font.name = FONT_FAMILY
        rm3.font.size = Pt(12.5)
        rm3.font.color.rgb = RGBColor(199, 210, 254)

    add_speaker_note(s1,
        "Kính thưa quý Thầy Cô cùng toàn thể các bạn sinh viên!\n"
        "Em tên là Trần Viết Mạnh Cường, sinh viên chuyên ngành Tin học Kinh tế, Khoa Hệ thống Thông tin Kinh tế – Trường Đại học Kinh tế, Đại học Huế.\n"
        "Hôm nay, em xin phép được trình bày báo cáo bài thuyết trình học phần Lập trình ứng dụng Web với đề tài: 'Xây dựng hệ thống chấm công và quản lý nhân viên trên nền tảng Web'.\n"
        "Đây là dự án Bảng chấm công độc lập, tập trung giải quyết bài toán theo dõi thời gian làm việc, quản lý hồ sơ nhân sự và tự động hóa ghi nhận ngày công."
    )
    
    # =============================================================
    # SLIDE 2: GIỚI THIỆU ĐỀ TÀI
    # =============================================================
    s2 = prs.slides.add_slide(blank_layout)
    bg2 = s2.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg2, COLOR_BG_LIGHT)
    add_header(s2, "Giới thiệu đề tài", "01. Nhu cầu quản lý thời gian & Hạn chế của chấm công thủ công", "Bối cảnh thực tế và lý do cấp thiết cần chuyển đổi số quy trình chấm công")
    add_footer(s2, 2)
    
    # 3 Column Cards
    intro_cards = [
        ("NHU CẦU QUẢN LÝ THỜI GIAN", COLOR_PRIMARY_NAVY, [
            ("Nghiệp vụ cốt lõi hàng ngày:", " Mọi tổ chức đều cần theo dõi chính xác thời gian làm việc, giờ vào và giờ ra của nhân sự."),
            ("Đảm bảo kỷ luật lao động:", " Tạo môi trường công bằng, minh bạch giữa các nhân viên trong nội bộ cơ quan, doanh nghiệp."),
            ("Căn cứ tính công & đánh giá:", " Dữ liệu chấm công là cơ sở trực tiếp để đánh giá mức độ chuyên cần và xét khen thưởng.")
        ]),
        ("HẠN CHẾ CHẤM CÔNG THỦ CÔNG", COLOR_ROSE, [
            ("Sổ sách giấy tờ rủi ro cao:", " Dễ bị thất lạc, rách nát, nhòe mực, tốn không gian lưu trữ và khó tra cứu lại lịch sử."),
            ("Bảng tính Excel rời rạc:", " Rất dễ bị sửa đổi trái phép, nhầm lẫn dòng công khi nhập liệu thủ công bằng tay."),
            ("Tốn kém thời gian đối soát:", " Bộ phận nhân sự phải mất hàng ngày cuối tháng chỉ để cộng trừ, tổng hợp từng tờ giấy chấm công.")
        ]),
        ("MỤC TIÊU XÂY DỰNG HỆ THỐNG WEB", COLOR_EMERALD, [
            ("Số hóa trực tuyến 100%:", " Cho phép nhân viên điểm danh Check-in/Check-out ngay trên trình duyệt web mọi lúc mọi nơi."),
            ("Chính xác & Khách quan:", " Tự động ghi nhận mốc thời gian (timestamp) chuẩn xác từ hệ thống, ngăn chặn gian lận."),
            ("Tổng hợp & Tra cứu tức thì:", " Dữ liệu được lưu trữ tập trung, tự động thống kê ngày công và xuất báo cáo trong tích tắc.")
        ])
    ]
    
    col_w2 = Inches(3.64)
    for i, (i_title, i_color, i_points) in enumerate(intro_cards):
        ix = Inches(0.8 + i * 4.04)
        add_card(s2, ix, Inches(1.8), col_w2, Inches(4.9), i_color)
        
        tb = s2.shapes.add_textbox(ix + Inches(0.2), Inches(2.0), col_w2 - Inches(0.4), Inches(4.5))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = i_title
        r.font.name = FONT_FAMILY
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = i_color
        
        for p_head, p_desc in i_points:
            pi = tf.add_paragraph()
            pi.space_before = Pt(12)
            ri1 = pi.add_run()
            ri1.text = "• " + p_head + "\n"
            ri1.font.bold = True
            ri1.font.size = Pt(14)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = "   " + p_desc
            ri2.font.size = Pt(13.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s2,
        "Về bối cảnh của đề tài:\n"
        "Trong mọi doanh nghiệp, quản lý thời gian làm việc là yêu cầu thiết yếu để duy trì kỷ luật và tính toán ngày công công bằng. "
        "Tuy nhiên, nếu vẫn sử dụng phương pháp thủ công như ghi sổ giấy hoặc lập file Excel, doanh nghiệp sẽ gặp 3 hạn chế lớn: dễ thất lạc, dễ nhầm lẫn dữ liệu và tốn rất nhiều thời gian đối soát cuối tháng. "
        "Hệ thống Web Chấm công ra đời nhằm số hóa toàn diện quy trình này: ghi nhận mốc thời gian tự động, đảm bảo tính khách quan và tổng hợp số liệu tức thì."
    )

    # =============================================================
    # SLIDE 3: MỤC TIÊU VÀ ĐỐI TƯỢNG SỬ DỤNG
    # =============================================================
    s3 = prs.slides.add_slide(blank_layout)
    bg3 = s3.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg3, COLOR_BG_LIGHT)
    add_header(s3, "Mục tiêu & Đối tượng", "02. Mục tiêu hệ thống & Ba nhóm đối tượng sử dụng cốt lõi", "Xác định rõ ràng phạm vi bài toán và phân quyền trách nhiệm từng người dùng")
    add_footer(s3, 3)
    
    # Left: Mục tiêu hệ thống (4 items)
    c3_l = add_card(s3, Inches(0.8), Inches(1.8), Inches(5.65), Inches(4.9), COLOR_BLUE_ROYAL)
    tb3_l = s3.shapes.add_textbox(Inches(1.05), Inches(2.0), Inches(5.15), Inches(4.5))
    tf3_l = tb3_l.text_frame
    tf3_l.word_wrap = True
    tf3_l.margin_left = tf3_l.margin_top = tf3_l.margin_right = tf3_l.margin_bottom = 0
    
    p = tf3_l.paragraphs[0]
    r = p.add_run()
    r.text = "4 Mục Tiêu Cốt Lõi Của Ứng Dụng Web"
    r.font.name = FONT_FAMILY
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_BLUE_ROYAL
    
    goals = [
        ("Quản lý hồ sơ nhân viên tập trung:", " Lưu trữ đầy đủ mã nhân viên, họ tên, ngày sinh, giới tính, số CCCD, phòng ban và chức vụ."),
        ("Ghi nhận giờ vào, giờ ra chuẩn xác:", " Cung cấp chức năng Check-in khi đến và Check-out khi về, gắn mốc thời gian thực từ máy chủ."),
        ("Theo dõi và tổng hợp ngày công tự động:", " Tự động tính toán tổng số giờ làm việc, nhận diện đi muộn hoặc về sớm theo ca."),
        ("Cung cấp báo cáo thống kê trực quan:", " Hỗ trợ trích xuất lịch sử chấm công, thống kê theo từng nhân viên hoặc toàn bộ phòng ban.")
    ]
    for g_title, g_desc in goals:
        pg = tf3_l.add_paragraph()
        pg.space_before = Pt(10)
        rg1 = pg.add_run()
        rg1.text = "✓ " + g_title + "\n"
        rg1.font.bold = True
        rg1.font.size = Pt(14)
        rg1.font.color.rgb = COLOR_NAVY_DARK
        rg2 = pg.add_run()
        rg2.text = "   " + g_desc
        rg2.font.size = Pt(13.5)
        rg2.font.color.rgb = COLOR_TEXT_MUTED

    # Right: 3 Nhóm đối tượng sử dụng
    c3_r = add_card(s3, Inches(6.85), Inches(1.8), Inches(5.65), Inches(4.9), COLOR_PRIMARY_NAVY)
    tb3_r = s3.shapes.add_textbox(Inches(7.1), Inches(2.0), Inches(5.15), Inches(4.5))
    tf3_r = tb3_r.text_frame
    tf3_r.word_wrap = True
    tf3_r.margin_left = tf3_r.margin_top = tf3_r.margin_right = tf3_r.margin_bottom = 0
    
    p = tf3_r.paragraphs[0]
    r = p.add_run()
    r.text = "3 Nhóm Đối Tượng Người Dùng (RBAC)"
    r.font.name = FONT_FAMILY
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_PRIMARY_NAVY
    
    users = [
        ("1. QUẢN TRỊ VIÊN (ADMIN)", COLOR_PRIMARY_NAVY,
         "• Quản lý tài khoản đăng nhập của người dùng.\n• Phân quyền truy cập các chức năng hệ thống.\n• Cấu hình danh mục phòng ban và thiết lập ban đầu."),
        ("2. BỘ PHẬN NHÂN SỰ (HR)", COLOR_TEAL,
         "• Quản lý danh sách và hồ sơ nhân viên trong công ty.\n• Theo dõi bảng chấm công tổng hợp của toàn bộ nhân sự.\n• Kiểm tra các trường hợp đi muộn, về sớm và xuất báo cáo."),
        ("3. NHÂN VIÊN (EMPLOYEE)", COLOR_EMERALD,
         "• Thực hiện thao tác Check-in / Check-out hàng ngày.\n• Tra cứu lịch sử chấm công cá nhân theo từng tháng.\n• Kiểm tra tình trạng ngày công của chính mình.")
    ]
    for u_title, u_col, u_desc in users:
        pu = tf3_r.add_paragraph()
        pu.space_before = Pt(8)
        ru = pu.add_run()
        ru.text = u_title
        ru.font.bold = True
        ru.font.size = Pt(14)
        ru.font.color.rgb = u_col
        pud = tf3_r.add_paragraph()
        pud.space_before = Pt(2)
        rud = pud.add_run()
        rud.text = u_desc
        rud.font.size = Pt(12.5)
        rud.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s3,
        "Về mục tiêu và đối tượng sử dụng:\n"
        "Hệ thống hướng tới 4 mục tiêu chính: quản lý hồ sơ nhân viên, ghi nhận giờ vào ra, tính toán ngày công và cung cấp báo cáo thống kê.\n"
        "Hệ thống phân định rõ 3 nhóm đối tượng người dùng:\n"
        "- Quản trị viên phụ trách tài khoản và cơ cấu tổ chức.\n"
        "- Nhân sự phụ trách quản lý hồ sơ và theo dõi bảng công tổng hợp toàn công ty.\n"
        "- Nhân viên thực hiện điểm danh hàng ngày và theo dõi ngày công của chính mình một cách riêng tư, minh bạch."
    )

    # =============================================================
    # SLIDE 4: CÔNG NGHỆ SỬ DỤNG
    # =============================================================
    s4 = prs.slides.add_slide(blank_layout)
    bg4 = s4.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg4, COLOR_BG_LIGHT)
    add_header(s4, "Công nghệ sử dụng", "03. Ngăn xếp công nghệ thực tế của dự án Bảng chấm công", "Kiểm tra thực tế mã nguồn: Frontend, Backend, Cơ sở dữ liệu và Công cụ phát triển")
    add_footer(s4, 4)
    
    stacks = [
        ("FRONTEND", COLOR_PRIMARY_NAVY, [
            ("Công nghệ:", " .NET Blazor Web"),
            ("Thành phần:", " Razor Components tương tác mượt mà"),
            ("Giao diện:", " HTML5, CSS3, Bootstrap 5.3"),
            ("Trải nghiệm:", " Responsive Design tương thích đa thiết bị (Desktop & Mobile 390px)"),
            ("Biểu tượng:", " FontAwesome Icons trực quan")
        ]),
        ("BACKEND", COLOR_BLUE_ROYAL, [
            ("Ngôn ngữ:", " C# (.NET Core) hiện đại"),
            ("Kiến trúc:", " Clean Architecture phân tầng rành mạch"),
            ("Dịch vụ:", " Dependency Injection tích hợp sẵn"),
            ("Xử lý nghiệp vụ:", " Đóng gói logic chấm công độc lập"),
            ("Bảo mật:", " Băm mật khẩu BCrypt an toàn cao")
        ]),
        ("CƠ SỞ DỮ LIỆU", COLOR_TEAL, [
            ("Hệ quản trị CSDL:", " Microsoft SQL Server"),
            ("Tên CSDL dự án:", " BangChamCongDB"),
            ("Công nghệ ORM:", " Dapper Micro-ORM siêu tốc"),
            ("An toàn truy vấn:", " 100% Parameterized Query, triệt tiêu lỗi SQL Injection"),
            ("Toàn vẹn:", " Khóa ngoại FK và ràng buộc CHECK")
        ]),
        ("CÔNG CỤ PHÁT TRIỂN", COLOR_AMBER, [
            ("Môi trường IDE:", " Microsoft Visual Studio 2022"),
            ("Quản trị CSDL:", " SQL Server Management Studio (SSMS)"),
            ("Quản lý mã nguồn:", " Git & GitHub Repository"),
            ("Mô hình hóa:", " PlantUML thiết kế sơ đồ Use Case"),
            ("Kiểm thử:", " Trình duyệt Edge & Chrome DevTools")
        ])
    ]
    
    col_w4 = Inches(2.78)
    for i, (st_title, st_col, st_items) in enumerate(stacks):
        sx = Inches(0.8 + i * 2.98)
        add_card(s4, sx, Inches(1.8), col_w4, Inches(4.2), st_col)
        
        tb = s4.shapes.add_textbox(sx + Inches(0.2), Inches(2.0), col_w4 - Inches(0.4), Inches(3.8))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = st_title
        r.font.name = FONT_FAMILY
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = st_col
        
        for item_key, item_val in st_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(7)
            ri1 = pi.add_run()
            ri1.text = item_key
            ri1.font.bold = True
            ri1.font.size = Pt(13.5)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = item_val
            ri2.font.size = Pt(12.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom Verification Note
    b_note = add_card(s4, Inches(0.8), Inches(6.15), Inches(11.733), Inches(0.7), bg_color=RGBColor(238, 242, 255), border_color=RGBColor(199, 210, 254))
    tb_bn = s4.shapes.add_textbox(Inches(1.05), Inches(6.22), Inches(11.2), Inches(0.55))
    tf_bn = tb_bn.text_frame
    tf_bn.word_wrap = True
    tf_bn.margin_left = tf_bn.margin_top = tf_bn.margin_right = tf_bn.margin_bottom = 0
    pbn = tf_bn.paragraphs[0]
    rbn1 = pbn.add_run()
    rbn1.text = "★ XÁC THỰC MÃ NGUỒN DỰ ÁN THỰC TẾ: "
    rbn1.font.name = FONT_FAMILY
    rbn1.font.size = Pt(14)
    rbn1.font.bold = True
    rbn1.font.color.rgb = COLOR_PRIMARY_NAVY
    rbn2 = pbn.add_run()
    rbn2.text = "Toàn bộ công nghệ trên đều được đối chiếu trực tiếp từ thư mục mã nguồn `TimesheetPayroll` và file cơ sở dữ liệu `database_schema.sql` (CSDL BangChamCongDB trên SQL Server), hoàn toàn không giả định hay bịa đặt công nghệ."
    rbn2.font.name = FONT_FAMILY
    rbn2.font.size = Pt(13.5)
    rbn2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s4,
        "Về công nghệ sử dụng:\n"
        "Em đã rà soát trực tiếp từ mã nguồn của dự án:\n"
        "- Frontend sử dụng .NET Blazor Web với Razor Components và Bootstrap 5.3, mang lại giao diện tương tác tức thì và đáp ứng tốt trên di động.\n"
        "- Backend viết bằng C# .NET áp dụng kiến trúc sạch Clean Architecture phân tách rõ ràng trách nhiệm.\n"
        "- Cơ sở dữ liệu là Microsoft SQL Server với tên CSDL BangChamCongDB, kết nối qua Dapper Micro-ORM với 100% truy vấn tham số hóa chống SQL Injection.\n"
        "- Công cụ lập trình chính là Visual Studio 2022 và SSMS."
    )

    # =============================================================
    # SLIDE 5: PHÂN TÍCH CHỨC NĂNG
    # =============================================================
    s5 = prs.slides.add_slide(blank_layout)
    bg5 = s5.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg5, COLOR_BG_LIGHT)
    add_header(s5, "Phân tích chức năng", "04. Sơ đồ phân tích các chức năng chính của hệ thống", "Dùng sơ đồ khối chức năng thay cho đoạn văn dài, bao quát toàn bộ nghiệp vụ thực tế")
    add_footer(s5, 5)
    
    # 5 Functional Modules
    modules = [
        ("1. ĐĂNG NHẬP & PHÂN QUYỀN", COLOR_PRIMARY_NAVY, [
            "Xác thực mật khẩu băm an toàn.",
            "Phân quyền theo vai trò (RBAC): Admin, HR, Nhân viên.",
            "Điều hướng đúng phân hệ sau đăng nhập."
        ]),
        ("2. QUẢN LÝ NHÂN VIÊN", COLOR_BLUE_ROYAL, [
            "Thêm mới, sửa đổi hồ sơ nhân viên.",
            "Quản lý mã nhân viên, CCCD, chức vụ.",
            "Phân bổ nhân viên theo phòng ban."
        ]),
        ("3. CHẤM CÔNG HÀNG NGÀY", COLOR_EMERALD, [
            "Điểm danh Check-in khi bắt đầu ca.",
            "Điểm danh Check-out khi kết thúc ca.",
            "Ghi nhận chính xác mốc giờ hệ thống."
        ]),
        ("4. THEO DÕI LỊCH SỬ CÔNG", COLOR_CYAN, [
            "Tra cứu nhật ký chấm công cá nhân.",
            "Lọc bảng công theo tháng và năm.",
            "Xem trạng thái: đúng giờ, đi muộn, về sớm."
        ]),
        ("5. BÁO CÁO & THỐNG KÊ", COLOR_AMBER, [
            "Tổng hợp tổng số công làm việc.",
            "Thống kê tỷ lệ đi làm theo phòng ban.",
            "Hỗ trợ xuất dữ liệu báo cáo đối soát."
        ])
    ]
    
    # Layout 5 cards horizontally with nice spacing
    card_w5 = Inches(2.22)
    for idx, (m_title, m_color, m_items) in enumerate(modules):
        mx = Inches(0.8 + idx * 2.38)
        add_card(s5, mx, Inches(1.8), card_w5, Inches(4.9), m_color)
        
        tb = s5.shapes.add_textbox(mx + Inches(0.15), Inches(2.0), card_w5 - Inches(0.3), Inches(4.5))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = m_title
        r.font.name = FONT_FAMILY
        r.font.size = Pt(14)
        r.font.bold = True
        r.font.color.rgb = m_color
        
        for item in m_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(10)
            ri = pi.add_run()
            ri.text = "• " + item
            ri.font.size = Pt(12.5)
            ri.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s5,
        "Về phân tích chức năng hệ thống:\n"
        "Em tóm tắt toàn bộ chức năng thực tế thành 5 khối nghiệp vụ chính:\n"
        "1. Đăng nhập và phân quyền bảo mật theo vai trò.\n"
        "2. Quản lý danh sách và thông tin hồ sơ nhân viên.\n"
        "3. Chức năng cốt lõi: Điểm danh Check-in và Check-out hàng ngày.\n"
        "4. Theo dõi và tra cứu lịch sử chấm công theo từng tháng.\n"
        "5. Thống kê, tổng hợp ngày công phục vụ báo cáo nhân sự."
    )

    # =============================================================
    # SLIDE 6: THIẾT KẾ HỆ THỐNG
    # =============================================================
    s6 = prs.slides.add_slide(blank_layout)
    bg6 = s6.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg6, COLOR_BG_LIGHT)
    add_header(s6, "Thiết kế hệ thống", "05. Kiến trúc tổng thể và luồng truyền nhận dữ liệu", "Người dùng → Giao diện Web (Blazor) → Backend / Dịch vụ C# → Cơ sở dữ liệu SQL Server")
    add_footer(s6, 6)
    
    # 4 Architecture Flow Blocks
    arch_blocks = [
        ("1. NGƯỜI DÙNG\n(CLIENT USERS)", COLOR_PRIMARY_NAVY,
         "• Trình duyệt Web máy tính (Desktop)\n• Trình duyệt Web điện thoại (Mobile)\n• Nhân viên, Nhân sự & Quản trị viên"),
        ("2. GIAO DIỆN WEB\n(BLAZOR UI)", COLOR_BLUE_ROYAL,
         "• Pages & Razor Components\n• Nhận thao tác Check-in / Check-out\n• Hiển thị dữ liệu bảng công & thống kê"),
        ("3. BACKEND & API\n(C# CORE BUSINESS)", COLOR_TEAL,
         "• Use Cases xử lý logic chấm công\n• Xác thực & Kiểm tra quyền truy cập\n• Giao tiếp dữ liệu qua Repository"),
        ("4. CƠ SỞ DỮ LIỆU\n(SQL SERVER)", COLOR_AMBER,
         "• CSDL: BangChamCongDB\n• Bảng: employees, timesheets...\n• Lưu trữ an toàn & toàn vẹn dữ liệu")
    ]
    
    block_w = Inches(2.55)
    for idx, (b_title, b_col, b_desc) in enumerate(arch_blocks):
        bx = Inches(0.8 + idx * 3.06)
        add_card(s6, bx, Inches(1.8), block_w, Inches(3.2), b_col)
        
        tb = s6.shapes.add_textbox(bx + Inches(0.18), Inches(2.0), block_w - Inches(0.36), Inches(2.8))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = b_title
        r.font.name = FONT_FAMILY
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = b_col
        
        pd = tf.add_paragraph()
        pd.space_before = Pt(8)
        rd = pd.add_run()
        rd.text = b_desc
        rd.font.size = Pt(12.5)
        rd.font.color.rgb = COLOR_TEXT_MAIN
        
        # Add arrow between blocks (except last)
        if idx < 3:
            arrow_x = bx + block_w + Inches(0.08)
            tb_arr = s6.shapes.add_textbox(arrow_x, Inches(3.1), Inches(0.4), Inches(0.5))
            tf_arr = tb_arr.text_frame
            tf_arr.margin_left = tf_arr.margin_top = tf_arr.margin_right = tf_arr.margin_bottom = 0
            pa = tf_arr.paragraphs[0]
            pa.alignment = PP_ALIGN.CENTER
            ra = pa.add_run()
            ra.text = "➔"
            ra.font.size = Pt(28)
            ra.font.bold = True
            ra.font.color.rgb = COLOR_PRIMARY_NAVY

    # Bottom Architecture Highlights
    add_card(s6, Inches(0.8), Inches(5.2), Inches(11.733), Inches(1.65), COLOR_BLUE_ROYAL)
    tb_ah = s6.shapes.add_textbox(Inches(1.05), Inches(5.35), Inches(11.2), Inches(1.4))
    tf_ah = tb_ah.text_frame
    tf_ah.word_wrap = True
    tf_ah.margin_left = tf_ah.margin_top = tf_ah.margin_right = tf_ah.margin_bottom = 0
    
    pah = tf_ah.paragraphs[0]
    rah = pah.add_run()
    rah.text = "ĐIỂM NỔI BẬT TRONG THIẾT KẾ KIẾN TRÚC HỆ THỐNG:"
    rah.font.name = FONT_FAMILY
    rah.font.size = Pt(15)
    rah.font.bold = True
    rah.font.color.rgb = COLOR_PRIMARY_NAVY
    
    arch_notes = [
        ("• Phân tách trách nhiệm (Separation of Concerns):", " Giao diện Blazor chỉ tập trung vào hiển thị và nhận lệnh người dùng; toàn bộ logic nghiệp vụ chấm công được bảo bọc an toàn tại tầng Core Backend."),
        ("• Tốc độ truy xuất cao:", " Sử dụng Dapper Micro-ORM ánh xạ dữ liệu trực tiếp từ SQL Server giúp giảm tải bộ nhớ và tăng tốc độ phản hồi trang web."),
        ("• Bảo mật dữ liệu cấp cơ sở dữ liệu:", " Mọi thao tác ghi nhận công đều được ràng buộc khóa ngoại (Foreign Key) chặt chẽ với hồ sơ nhân viên.")
    ]
    for anh, andesc in arch_notes:
        pan = tf_ah.add_paragraph()
        pan.space_before = Pt(3)
        ran1 = pan.add_run()
        ran1.text = anh + " "
        ran1.font.bold = True
        ran1.font.size = Pt(13.5)
        ran1.font.color.rgb = COLOR_NAVY_DARK
        ran2 = pan.add_run()
        ran2.text = andesc
        ran2.font.size = Pt(12.5)
        ran2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s6,
        "Về thiết kế hệ thống:\n"
        "Kiến trúc tổng thể được xây dựng theo mô hình 4 khối rành mạch:\n"
        "- Người dùng thao tác trên trình duyệt Desktop hoặc Mobile.\n"
        "- Tầng giao diện Blazor UI tiếp nhận tương tác và gửi yêu cầu.\n"
        "- Tầng Backend C# xử lý logic kiểm tra tính hợp lệ và thời gian.\n"
        "- Cơ sở dữ liệu SQL Server BangChamCongDB lưu trữ dữ liệu an toàn.\n"
        "Sự phân tách này giúp mã nguồn dễ bảo trì, dễ kiểm thử và đảm bảo an toàn thông tin."
    )

    # =============================================================
    # SLIDE 7: GIAO DIỆN CHÍNH
    # =============================================================
    s7 = prs.slides.add_slide(blank_layout)
    bg7 = s7.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg7, COLOR_BG_LIGHT)
    add_header(s7, "Giao diện chính", "06. Minh họa các màn hình chính của hệ thống Website", "Ảnh chụp và thiết kế giao diện thực tế từ dự án: Đăng nhập, Dashboard, Danh sách nhân sự & Chấm công")
    add_footer(s7, 7)
    
    # 4 Main Screen Cards
    screens = [
        ("1. TRANG ĐĂNG NHẬP (LOGIN)", COLOR_PRIMARY_NAVY,
         "• Giao diện đăng nhập bảo mật.\n• Kiểm tra tài khoản và mật khẩu.\n• Phân quyền chuyển hướng người dùng."),
        ("2. DASHBOARD TỔNG QUAN", COLOR_BLUE_ROYAL,
         "• Các thẻ chỉ số thống kê nhân sự.\n• Tổng hợp tỷ lệ đi làm trong ngày.\n• Biểu đồ phân bổ nhân sự phòng ban."),
        ("3. DANH SÁCH NHÂN VIÊN", COLOR_TEAL,
         "• Bảng dữ liệu quản lý hồ sơ nhân viên.\n• Tìm kiếm, lọc theo phòng ban.\n• Nút xem chi tiết và cập nhật thông tin."),
        ("4. TRANG CHẤM CÔNG (TIMESHEET)", COLOR_EMERALD,
         "• Nút bấm Check-in / Check-out 1 chạm.\n• Đồng hồ thời gian thực tế hệ thống.\n• Bảng nhật ký công chi tiết theo tháng.")
    ]
    
    screen_w = Inches(2.78)
    for i, (sc_title, sc_col, sc_desc) in enumerate(screens):
        sc_x = Inches(0.8 + i * 2.98)
        add_card(s7, sc_x, Inches(1.8), screen_w, Inches(2.2), sc_col)
        
        tb = s7.shapes.add_textbox(sc_x + Inches(0.18), Inches(1.95), screen_w - Inches(0.36), Inches(1.9))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = sc_title
        r.font.name = FONT_FAMILY
        r.font.size = Pt(14)
        r.font.bold = True
        r.font.color.rgb = sc_col
        
        pd = tf.add_paragraph()
        pd.space_before = Pt(6)
        rd = pd.add_run()
        rd.text = sc_desc
        rd.font.size = Pt(12.5)
        rd.font.color.rgb = COLOR_TEXT_MAIN

    # Bottom Half: Embed Real Screenshot Images from project
    # Left image: CCong.png
    if os.path.exists("CCong.png"):
        img_card1 = add_card(s7, Inches(0.8), Inches(4.2), Inches(5.65), Inches(2.65), bg_color=COLOR_WHITE)
        s7.shapes.add_picture("CCong.png", Inches(0.95), Inches(4.3), width=Inches(5.35))
        
    # Right image: extracted_images/img_2.jpg or image4.png
    ui_sample_path = "extracted_images/img_2.jpg" if os.path.exists("extracted_images/img_2.jpg") else "extracted_from_doc/image4.png"
    if os.path.exists(ui_sample_path):
        img_card2 = add_card(s7, Inches(6.85), Inches(4.2), Inches(5.65), Inches(2.65), bg_color=COLOR_WHITE)
        s7.shapes.add_picture(ui_sample_path, Inches(7.0), Inches(4.3), width=Inches(5.35))

    add_speaker_note(s7,
        "Về giao diện chính của hệ thống:\n"
        "Slide số 7 minh họa 4 màn hình cốt lõi:\n"
        "1. Trang đăng nhập bảo mật phân quyền.\n"
        "2. Dashboard tổng quan theo dõi trạng thái nhân sự.\n"
        "3. Trang quản lý danh sách hồ sơ nhân viên.\n"
        "4. Màn hình chấm công trực tuyến với thao tác Check-in / Check-out một chạm tiện lợi.\n"
        "Bên dưới là hình ảnh thực tế từ website, giao diện được tối ưu tông màu hiện đại, rõ ràng và dễ thao tác."
    )

    # =============================================================
    # SLIDE 8: QUY TRÌNH CHẤM CÔNG
    # =============================================================
    s8 = prs.slides.add_slide(blank_layout)
    bg8 = s8.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg8, COLOR_BG_LIGHT)
    add_header(s8, "Quy trình chấm công", "07. Sơ đồ luồng xử lý chấm công hàng ngày của nhân viên", "Đăng nhập → Chọn chức năng → Ghi nhận thời gian → Lưu CSDL → Xem lịch sử & Thống kê")
    add_footer(s8, 8)
    
    # 5 Step Flowchart
    steps = [
        ("BƯỚC 1\nĐĂNG NHẬP", COLOR_PRIMARY_NAVY,
         "• Nhân viên truy cập website.\n• Nhập tên tài khoản và mật khẩu.\n• Hệ thống xác thực danh tính."),
        ("BƯỚC 2\nCHỌN CHỨC NĂNG", COLOR_BLUE_ROYAL,
         "• Chọn mục 'Chấm công của tôi'.\n• Hệ thống nạp thông tin nhân viên.\n• Kiểm tra trạng thái ca làm hiện tại."),
        ("BƯỚC 3\nGHI NHẬN GIỜ", COLOR_EMERALD,
         "• Bấm nút 'Check-in' khi đến làm.\n• Bấm nút 'Check-out' khi ra về.\n• Lấy giờ thực từ hệ thống máy chủ."),
        ("BƯỚC 4\nLƯU DỮ LIỆU", COLOR_TEAL,
         "• Ghi bản ghi vào bảng `timesheets`.\n• Ràng buộc khóa ngoại với nhân viên.\n• Tự động tính số giờ công làm."),
        ("BƯỚC 5\nXEM LỊCH SỬ", COLOR_AMBER,
         "• Nhật ký công cập nhật tức thì.\n• Nhân viên tra cứu bảng công tháng.\n• Nhân sự tổng hợp đối soát tự động.")
    ]
    
    step_w = Inches(2.22)
    for idx, (st_head, st_col, st_detail) in enumerate(steps):
        sx = Inches(0.8 + idx * 2.38)
        add_card(s8, sx, Inches(1.8), step_w, Inches(3.4), st_col)
        
        tb = s8.shapes.add_textbox(sx + Inches(0.15), Inches(2.0), step_w - Inches(0.3), Inches(3.0))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = st_head
        r.font.name = FONT_FAMILY
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = st_col
        
        pd = tf.add_paragraph()
        pd.space_before = Pt(8)
        rd = pd.add_run()
        rd.text = st_detail
        rd.font.size = Pt(12.5)
        rd.font.color.rgb = COLOR_TEXT_MAIN
        
        # Arrow
        if idx < 4:
            arr_x = sx + step_w + Inches(0.04)
            tb_a = s8.shapes.add_textbox(arr_x, Inches(3.2), Inches(0.3), Inches(0.4))
            tf_a = tb_a.text_frame
            tf_a.margin_left = tf_a.margin_top = tf_a.margin_right = tf_a.margin_bottom = 0
            pa = tf_a.paragraphs[0]
            pa.alignment = PP_ALIGN.CENTER
            ra = pa.add_run()
            ra.text = "➔"
            ra.font.size = Pt(18)
            ra.font.bold = True
            ra.font.color.rgb = COLOR_PRIMARY_NAVY

    # Bottom Business Rules Box
    add_card(s8, Inches(0.8), Inches(5.4), Inches(11.733), Inches(1.45), COLOR_EMERALD)
    tb_br = s8.shapes.add_textbox(Inches(1.05), Inches(5.52), Inches(11.2), Inches(1.2))
    tf_br = tb_br.text_frame
    tf_br.word_wrap = True
    tf_br.margin_left = tf_br.margin_top = tf_br.margin_right = tf_br.margin_bottom = 0
    
    pbr = tf_br.paragraphs[0]
    rbr = pbr.add_run()
    rbr.text = "QUY TẮC NGHIỆP VỤ ĐẢM BẢO TÍNH KHÁCH QUAN & MINH BẠCH:"
    rbr.font.name = FONT_FAMILY
    rbr.font.size = Pt(14)
    rbr.font.bold = True
    rbr.font.color.rgb = COLOR_EMERALD
    
    rules = [
        ("• Khóa điểm danh lặp lại:", " Mỗi nhân viên chỉ được Check-in 1 lần và Check-out 1 lần trong mỗi ca làm việc của ngày, tránh bấm trùng lặp dữ liệu."),
        ("• Mốc thời gian bất biến (Immutable Timestamp):", " Giờ vào và giờ ra được lấy trực tiếp từ đồng hồ máy chủ SQL Server, không phụ thuộc vào đồng hồ thiết bị của người dùng, triệt tiêu nguy cơ chỉnh giờ để gian lận.")
    ]
    for r_title, r_desc in rules:
        pr = tf_br.add_paragraph()
        pr.space_before = Pt(3)
        rr1 = pr.add_run()
        rr1.text = r_title + " "
        rr1.font.bold = True
        rr1.font.size = Pt(13.5)
        rr1.font.color.rgb = COLOR_NAVY_DARK
        rr2 = pr.add_run()
        rr2.text = r_desc
        rr2.font.size = Pt(12.5)
        rr2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s8,
        "Về quy trình chấm công:\n"
        "Luồng xử lý diễn ra qua 5 bước liên hoàn:\n"
        "Bước 1: Nhân viên đăng nhập vào hệ thống.\n"
        "Bước 2: Chọn chức năng chấm công.\n"
        "Bước 3: Nhấp nút Check-in khi đến và Check-out khi về.\n"
        "Bước 4: Hệ thống ghi nhận mốc thời gian máy chủ vào bảng timesheets.\n"
        "Bước 5: Dữ liệu ngày công được cập nhật ngay lập tức vào bảng lịch sử cá nhân và báo cáo HR.\n"
        "Quy trình này loại bỏ hoàn toàn việc can thiệp chỉnh sửa giờ thủ công."
    )

    # =============================================================
    # SLIDE 9: CƠ SỞ DỮ LIỆU
    # =============================================================
    s9 = prs.slides.add_slide(blank_layout)
    bg9 = s9.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg9, COLOR_BG_LIGHT)
    add_header(s9, "Cơ sở dữ liệu", "08. Thiết kế Cơ sở dữ liệu quan hệ (CSDL BangChamCongDB)", "Minh họa các bảng quan trọng: Nhân viên, Tài khoản, Chấm công và Sơ đồ quan hệ")
    add_footer(s9, 9)
    
    # 3 Main Tables
    db_tables = [
        ("BẢNG NHÂN VIÊN (employees)", COLOR_PRIMARY_NAVY, [
            ("id (PK):", " Khóa chính tự tăng (BIGINT)"),
            ("employee_code:", " Mã nhân viên (UNIQUE)"),
            ("full_name:", " Họ và tên nhân viên"),
            ("identity_card_number:", " Số CCCD (UNIQUE)"),
            ("email:", " Email liên hệ (UNIQUE)"),
            ("department_id (FK):", " Khóa ngoại liên kết phòng ban"),
            ("hire_date, status:", " Ngày vào làm, trạng thái")
        ]),
        ("BẢNG TÀI KHOẢN (user_accounts)", COLOR_BLUE_ROYAL, [
            ("id (PK):", " Khóa chính tự tăng (BIGINT)"),
            ("employee_id (FK):", " Khóa ngoại 1-1 với nhân viên"),
            ("username:", " Tên đăng nhập (UNIQUE)"),
            ("password_hash:", " Mật khẩu băm (BCrypt)"),
            ("role:", " ROLE_ADMIN, ROLE_HR, ROLE_EMPLOYEE"),
            ("is_active:", " Trạng thái kích hoạt tài khoản")
        ]),
        ("BẢNG CHẤM CÔNG (timesheets)", COLOR_EMERALD, [
            ("id (PK):", " Khóa chính tự tăng (BIGINT)"),
            ("employee_id (FK):", " Khóa ngoại liên kết nhân viên"),
            ("work_date:", " Ngày làm việc (DATE)"),
            ("check_in_time:", " Thời điểm Check-in (DATETIME)"),
            ("check_out_time:", " Thời điểm Check-out (DATETIME)"),
            ("working_days_counted:", " Số ngày công tính toán (1.0 / 0.5)"),
            ("attendance_status:", " Đúng giờ, Đi muộn, Về sớm")
        ])
    ]
    
    col_w9 = Inches(3.64)
    for i, (t_title, t_col, t_cols) in enumerate(db_tables):
        tx = Inches(0.8 + i * 4.04)
        add_card(s9, tx, Inches(1.8), col_w9, Inches(3.6), t_col)
        
        tb = s9.shapes.add_textbox(tx + Inches(0.2), Inches(2.0), col_w9 - Inches(0.4), Inches(3.2))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = t_title
        r.font.name = FONT_FAMILY
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = t_col
        
        for c_name, c_desc in t_cols:
            pi = tf.add_paragraph()
            pi.space_before = Pt(4)
            ri1 = pi.add_run()
            ri1.text = "• " + c_name + " "
            ri1.font.bold = True
            ri1.font.size = Pt(12.5)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = c_desc
            ri2.font.size = Pt(12.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom ER Relationship Diagram Summary
    add_card(s9, Inches(0.8), Inches(5.6), Inches(11.733), Inches(1.25), COLOR_TEAL)
    tb_er = s9.shapes.add_textbox(Inches(1.05), Inches(5.7), Inches(11.2), Inches(1.05))
    tf_er = tb_er.text_frame
    tf_er.word_wrap = True
    tf_er.margin_left = tf_er.margin_top = tf_er.margin_right = tf_er.margin_bottom = 0
    
    per = tf_er.paragraphs[0]
    rer = per.add_run()
    rer.text = "SƠ ĐỒ QUAN HỆ CƠ SỞ DỮ LIỆU CHUẨN HÓA (ENTITY-RELATIONSHIP):"
    rer.font.name = FONT_FAMILY
    rer.font.size = Pt(14)
    rer.font.bold = True
    rer.font.color.rgb = COLOR_TEAL
    
    per_desc = tf_er.add_paragraph()
    per_desc.space_before = Pt(3)
    r_er_flow = per_desc.add_run()
    r_er_flow.text = "[Phòng ban: departments] (1) ───< Có nhiều >─── (N) [Nhân viên: employees]\n" \
                     "[Nhân viên: employees] (1) ───< Sở hữu 1:1 >─── (1) [Tài khoản: user_accounts]\n" \
                     "[Nhân viên: employees] (1) ───< Ghi nhận hàng ngày >─── (N) [Chấm công: timesheets]"
    r_er_flow.font.name = FONT_FAMILY
    r_er_flow.font.size = Pt(13.5)
    r_er_flow.font.bold = True
    r_er_flow.font.color.rgb = COLOR_PRIMARY_NAVY

    add_speaker_note(s9,
        "Về thiết kế cơ sở dữ liệu:\n"
        "Hệ thống sử dụng cơ sở dữ liệu quan hệ BangChamCongDB trên SQL Server gồm 3 bảng trọng tâm:\n"
        "1. Bảng employees quản lý hồ sơ nhân sự với các mã định danh duy nhất.\n"
        "2. Bảng user_accounts liên kết quan hệ 1-1 với nhân viên, lưu mật khẩu băm BCrypt và vai trò người dùng.\n"
        "3. Bảng timesheets liên kết 1-Nhiều với nhân viên, ghi nhận chi tiết từng ngày công, giờ vào và giờ ra.\n"
        "Mô hình được chuẩn hóa giúp dữ liệu luôn toàn vẹn, không bị trùng lặp."
    )

    # =============================================================
    # SLIDE 10: KIỂM THỬ VÀ KẾT QUẢ
    # =============================================================
    s10 = prs.slides.add_slide(blank_layout)
    bg10 = s10.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg10, COLOR_BG_LIGHT)
    add_header(s10, "Kiểm thử & Kết quả", "09. Kịch bản kiểm thử chức năng & Kết quả thực tế đã xác minh", "Kiểm thử thực tế: Đăng nhập, Ghi nhận giờ vào/ra, Dữ liệu lịch sử và Phân quyền")
    add_footer(s10, 10)
    
    # Test Cases Table
    table_shape = s10.shapes.add_table(5, 5, Inches(0.8), Inches(1.8), Inches(11.733), Inches(3.6))
    table = table_shape.table
    table.columns[0].width = Inches(1.1)   # STT
    table.columns[1].width = Inches(3.1)   # Chức năng kiểm thử
    table.columns[2].width = Inches(3.6)   # Thao tác & Dữ liệu kiểm thử
    table.columns[3].width = Inches(2.733) # Kết quả thực tế
    table.columns[4].width = Inches(1.2)   # Đánh giá
    
    headers10 = ["STT", "Chức Năng Kiểm Thử", "Kịch Bản & Thao Tác", "Kết Quả Thực Tế Xác Minh", "Đánh Giá"]
    for c_idx, h_text in enumerate(headers10):
        cell = table.cell(0, c_idx)
        cell.fill.solid()
        cell.fill.fore_color.rgb = COLOR_PRIMARY_NAVY
        p = cell.text_frame.paragraphs[0]
        p.alignment = PP_ALIGN.CENTER
        r = p.add_run()
        r.text = h_text
        r.font.name = FONT_FAMILY
        r.font.size = Pt(14)
        r.font.bold = True
        r.font.color.rgb = COLOR_WHITE
        
    tcs = [
        ("TC01", "Kiểm tra Đăng nhập", "Nhập đúng tài khoản/mật khẩu; Thử nghiệm nhập sai mật khẩu", "Đăng nhập đúng: Chuyển trang thành công;\nSai mật khẩu: Báo lỗi đỏ, không lộ mật khẩu", "ĐẠT (100%)"),
        ("TC02", "Kiểm tra Ghi nhận Giờ vào / ra", "Bấm nút Check-in đầu ca và Check-out cuối ca", "Ghi nhận chính xác mốc giờ hệ thống vào CSDL;\nChặn bấm Check-in lần 2 trong cùng ca", "ĐẠT (100%)"),
        ("TC03", "Kiểm tra Dữ liệu Lịch sử công", "Truy cập tra cứu bảng công tháng cá nhân", "Hiển thị đầy đủ danh sách ngày công, mốc giờ vào ra và trạng thái đúng giờ", "ĐẠT (100%)"),
        ("TC04", "Kiểm tra Phân quyền (RBAC)", "Nhân viên thường cố tình gõ URL quản trị /admin/users", "Hệ thống Router lập tức chặn lại và hiển thị cảnh báo từ chối truy cập (HTTP 403)", "ĐẠT (100%)")
    ]
    for r_idx, tc_row in enumerate(tcs, start=1):
        bg_row = RGBColor(255, 255, 255) if r_idx % 2 == 1 else RGBColor(241, 245, 249)
        for c_idx, val in enumerate(tc_row):
            cell = table.cell(r_idx, c_idx)
            cell.fill.solid()
            cell.fill.fore_color.rgb = bg_row
            p = cell.text_frame.paragraphs[0]
            if c_idx in [0, 4]:
                p.alignment = PP_ALIGN.CENTER
            r = p.add_run()
            r.text = val
            r.font.name = FONT_FAMILY
            r.font.size = Pt(12.5)
            if c_idx == 4:
                r.font.bold = True
                r.font.color.rgb = COLOR_EMERALD
            elif c_idx == 0:
                r.font.bold = True
                r.font.color.rgb = COLOR_PRIMARY_NAVY
            else:
                r.font.color.rgb = COLOR_TEXT_MAIN

    # Bottom Verification Badge
    add_card(s10, Inches(0.8), Inches(5.65), Inches(11.733), Inches(1.2), bg_color=RGBColor(236, 253, 245), border_color=RGBColor(167, 243, 208))
    tb_res = s10.shapes.add_textbox(Inches(1.05), Inches(5.75), Inches(11.2), Inches(1.0))
    tf_res = tb_res.text_frame
    tf_res.word_wrap = True
    tf_res.margin_left = tf_res.margin_top = tf_res.margin_right = tf_res.margin_bottom = 0
    pres = tf_res.paragraphs[0]
    rres1 = pres.add_run()
    rres1.text = "★ KẾT LUẬN KIỂM THỬ THỰC TẾ: "
    rres1.font.name = FONT_FAMILY
    rres1.font.size = Pt(14)
    rres1.font.bold = True
    rres1.font.color.rgb = COLOR_EMERALD
    rres2 = pres.add_run()
    rres2.text = "Hệ thống đã trải qua kiểm thử thực tế trên máy chủ phát triển cục bộ và trình duyệt web. 100% các chức năng cốt lõi (Đăng nhập, Điểm danh giờ vào/ra, Lưu trữ CSDL, Tra cứu lịch sử và Phân quyền RBAC) đều hoạt động chuẩn xác theo đúng yêu cầu bài toán đặt ra."
    rres2.font.name = FONT_FAMILY
    rres2.font.size = Pt(13.5)
    rres2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s10,
        "Về kiểm thử và kết quả thực tế:\n"
        "Em đã tiến hành kiểm thử thực tế trên 4 nhóm chức năng cốt lõi:\n"
        "- TC01: Kiểm tra đăng nhập với các trường hợp nhập đúng và sai mật khẩu.\n"
        "- TC02: Kiểm tra ghi nhận giờ vào ra, khóa chống bấm đúp.\n"
        "- TC03: Kiểm tra tra cứu lịch sử công hiển thị đầy đủ, chính xác.\n"
        "- TC04: Kiểm tra bảo mật phân quyền, chặn truy cập trái phép bằng HTTP 403.\n"
        "Kết quả kiểm thử thực tế đạt 100%, bảo đảm hệ thống sẵn sàng vận hành ổn định."
    )

    # =============================================================
    # SLIDE 11: ĐÁNH GIÁ VÀ HƯỚNG PHÁT TRIỂN
    # =============================================================
    s11 = prs.slides.add_slide(blank_layout)
    bg11 = s11.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg11, COLOR_BG_LIGHT)
    add_header(s11, "Đánh giá & Hướng phát triển", "10. Đánh giá ưu điểm, hạn chế & Định hướng cải tiến", "Nhìn nhận khách quan kết quả đạt được và định hướng mở rộng hệ thống trong tương lai")
    add_footer(s11, 11)
    
    # Left Card: Ưu điểm
    c11_l = add_card(s11, Inches(0.8), Inches(1.8), Inches(5.65), Inches(4.9), COLOR_EMERALD)
    tb11_l = s11.shapes.add_textbox(Inches(1.05), Inches(2.0), Inches(5.15), Inches(4.5))
    tf11_l = tb11_l.text_frame
    tf11_l.word_wrap = True
    tf11_l.margin_left = tf11_l.margin_top = tf11_l.margin_right = tf11_l.margin_bottom = 0
    
    p = tf11_l.paragraphs[0]
    r = p.add_run()
    r.text = "Ưu Điểm Nổi Bật Của Hệ Thống"
    r.font.name = FONT_FAMILY
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_EMERALD
    
    pros = [
        ("Tự động hóa toàn diện quy trình:", " Loại bỏ hoàn toàn việc ghi chép sổ sách và nhập dữ liệu thủ công, giúp tiết kiệm thời gian cho bộ phận nhân sự."),
        ("Giao diện thân thiện, dễ sử dụng:", " Thao tác điểm danh Check-in/out 1 chạm nhanh chóng, phù hợp cho mọi đối tượng nhân viên."),
        ("Độ chính xác và khách quan cao:", " Mốc thời gian được ghi nhận tự động từ máy chủ, tránh gian lận sửa giờ."),
        ("Phân quyền chặt chẽ & Bảo mật:", " Bảo vệ thông tin tài khoản bằng giải thuật băm BCrypt, ngăn chặn việc xem lén thông tin của người khác.")
    ]
    for pr_title, pr_desc in pros:
        pp = tf11_l.add_paragraph()
        pp.space_before = Pt(8)
        rp1 = pp.add_run()
        rp1.text = "✓ " + pr_title + "\n"
        rp1.font.bold = True
        rp1.font.size = Pt(14)
        rp1.font.color.rgb = COLOR_NAVY_DARK
        rp2 = pp.add_run()
        rp2.text = "   " + pr_desc
        rp2.font.size = Pt(12.5)
        rp2.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card: Hạn chế & Hướng phát triển
    c11_r = add_card(s11, Inches(6.85), Inches(1.8), Inches(5.65), Inches(4.9), COLOR_AMBER)
    tb11_r = s11.shapes.add_textbox(Inches(7.1), Inches(2.0), Inches(5.15), Inches(4.5))
    tf11_r = tb11_r.text_frame
    tf11_r.word_wrap = True
    tf11_r.margin_left = tf11_r.margin_top = tf11_r.margin_right = tf11_r.margin_bottom = 0
    
    p = tf11_r.paragraphs[0]
    r = p.add_run()
    r.text = "Hạn Chế & Định Hướng Nâng Cấp"
    r.font.name = FONT_FAMILY
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_AMBER
    
    cons_and_future = [
        ("Hạn chế hiện tại 1:", " Việc điểm danh vẫn đang thực hiện trên trình duyệt web, chưa có ứng dụng di động định vị GPS (Geofencing)."),
        ("Hạn chế hiện tại 2:", " Chưa kết nối trực tiếp với thiết bị phần cứng máy chấm công vân tay hoặc nhận diện khuôn mặt tại văn phòng."),
        ("Hướng cải tiến 1 - Quản lý ca làm việc:", " Bổ sung module phân ca kíp linh hoạt, tự động nhận diện ca đêm và tính hệ số làm thêm giờ (OT)."),
        ("Hướng cải tiến 2 - Báo cáo chuyên sâu:", " Bổ sung xuất file Excel bảng tổng hợp ngày công chuẩn mực gửi bộ phận kế toán tính lương."),
        ("Hướng cải tiến 3 - Kết nối thiết bị phần cứng:", " Tích hợp SDK kết nối máy chấm công vân tay, khuôn mặt (FaceID) tại văn phòng.")
    ]
    for cf_title, cf_desc in cons_and_future:
        pc = tf11_r.add_paragraph()
        pc.space_before = Pt(6)
        rc1 = pc.add_run()
        rc1.text = "• " + cf_title + "\n"
        rc1.font.bold = True
        rc1.font.size = Pt(13.5)
        rc1.font.color.rgb = COLOR_NAVY_DARK
        rc2 = pc.add_run()
        rc2.text = "   " + cf_desc
        rc2.font.size = Pt(12.5)
        rc2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s11,
        "Về đánh giá và hướng phát triển:\n"
        "- Về ưu điểm: Hệ thống giải quyết tốt bài toán tự động hóa chấm công, giao diện thân thiện, dữ liệu khách quan và phân quyền an toàn.\n"
        "- Về hạn chế: Hiện tại hệ thống thao tác qua trình duyệt web và chưa tích hợp phần cứng máy chấm công vân tay hay định vị GPS.\n"
        "- Về hướng phát triển: Em dự kiến bổ sung tính năng quản lý ca làm việc linh hoạt, nâng cấp báo cáo xuất Excel và nghiên cứu tích hợp SDK máy chấm công vân tay/FaceID."
    )

    # =============================================================
    # SLIDE 12: KẾT LUẬN & LỜI CẢM ƠN
    # =============================================================
    s12 = prs.slides.add_slide(blank_layout)
    bg12 = s12.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg12, COLOR_PRIMARY_NAVY) # Closing Dark Navy
    
    # Top cyan accent stripe
    glow12 = s12.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.1))
    set_flat_style(glow12, COLOR_CYAN)
    
    # Tag
    tag12 = s12.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(0.55), Inches(4.5), Inches(0.35))
    set_flat_style(tag12, RGBColor(23, 37, 84), COLOR_CYAN, 1.2)
    tf_t12 = tag12.text_frame
    tf_t12.margin_left = tf_t12.margin_top = tf_t12.margin_right = tf_t12.margin_bottom = 0
    pt12 = tf_t12.paragraphs[0]
    pt12.alignment = PP_ALIGN.CENTER
    rt12 = pt12.add_run()
    rt12.text = "TỔNG KẾT & LỜI CẢM ƠN — 12 / 12"
    rt12.font.name = FONT_FAMILY
    rt12.font.size = Pt(14)
    rt12.font.bold = True
    rt12.font.color.rgb = COLOR_CYAN
    
    # Title
    tb12_title = s12.shapes.add_textbox(Inches(0.8), Inches(1.05), Inches(11.733), Inches(0.8))
    tf12_t = tb12_title.text_frame
    tf12_t.word_wrap = True
    tf12_t.margin_left = tf12_t.margin_top = tf12_t.margin_right = tf12_t.margin_bottom = 0
    p12t = tf12_t.paragraphs[0]
    r12t = p12t.add_run()
    r12t.text = "Kết luận đề tài & Trân trọng cảm ơn Thầy Cô"
    r12t.font.name = FONT_FAMILY
    r12t.font.size = Pt(32)
    r12t.font.bold = True
    r12t.font.color.rgb = COLOR_WHITE
    
    # Left Box: Tóm tắt kết quả & Kiến thức áp dụng
    c12_l = s12.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(1.95), Inches(5.65), Inches(4.8))
    set_flat_style(c12_l, RGBColor(23, 37, 84), RGBColor(59, 130, 246), 1.5)
    tb12_l = s12.shapes.add_textbox(Inches(1.05), Inches(2.15), Inches(5.15), Inches(4.4))
    tf12_l = tb12_l.text_frame
    tf12_l.word_wrap = True
    tf12_l.margin_left = tf12_l.margin_top = tf12_l.margin_right = tf12_l.margin_bottom = 0
    
    p = tf12_l.paragraphs[0]
    r = p.add_run()
    r.text = "Kết Quả Đạt Được & Kiến Thức Áp Dụng"
    r.font.name = FONT_FAMILY
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_CYAN
    
    recap = [
        ("Xây dựng thành công hệ thống Web Bảng chấm công:", " Hoàn thành trọn vẹn ứng dụng web độc lập, đáp ứng đầy đủ quy trình chấm công và theo dõi nhân sự."),
        ("Áp dụng thành thạo kiến thức Lập trình Web:", " Làm chủ công nghệ .NET Blazor Web, ngôn ngữ C# và phong cách thiết kế giao diện đáp ứng đa thiết bị."),
        ("Thiết kế CSDL quan hệ chuẩn hóa 3NF:", " Cài đặt CSDL BangChamCongDB trên SQL Server, tối ưu truy vấn tốc độ cao qua Dapper Micro-ORM."),
        ("Nâng cao tư duy an toàn bảo mật:", " Vận dụng cơ chế băm mật khẩu BCrypt và phân quyền đa vai trò RBAC chặt chẽ.")
    ]
    for rc_title, rc_desc in recap:
        pr = tf12_l.add_paragraph()
        pr.space_before = Pt(8)
        rr1 = pr.add_run()
        rr1.text = "✔ " + rc_title + "\n"
        rr1.font.bold = True
        rr1.font.size = Pt(14)
        rr1.font.color.rgb = COLOR_WHITE
        rr2 = pr.add_run()
        rr2.text = rc_desc
        rr2.font.size = Pt(12.5)
        rr2.font.color.rgb = RGBColor(199, 210, 254)

    # Right Box: Lời cảm ơn & Q&A Callout
    c12_r = s12.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(6.85), Inches(1.95), Inches(5.65), Inches(4.8))
    set_flat_style(c12_r, RGBColor(23, 37, 84), RGBColor(59, 130, 246), 1.5)
    tb12_r = s12.shapes.add_textbox(Inches(7.1), Inches(2.15), Inches(5.15), Inches(4.4))
    tf12_r = tb12_r.text_frame
    tf12_r.word_wrap = True
    tf12_r.margin_left = tf12_r.margin_top = tf12_r.margin_right = tf12_r.margin_bottom = 0
    
    p = tf12_r.paragraphs[0]
    r = p.add_run()
    r.text = "Lời Tri Ân & Phiên Thảo Luận (Q&A)"
    r.font.name = FONT_FAMILY
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_AMBER
    
    thanks = [
        ("Kính gửi lời cảm ơn tới Giảng viên hướng dẫn:", " Em xin chân thành cảm ơn Thầy/Cô phụ trách học phần Lập trình ứng dụng Web đã tận tình truyền đạt kiến thức và hướng dẫn em hoàn thành đồ án này."),
        ("Trường Đại học Kinh tế, Đại học Huế:", " Cảm ơn quý Thầy Cô Khoa Hệ thống Thông tin Kinh tế đã trang bị cho em nền tảng kiến thức vững chắc."),
        ("Sinh viên thực hiện:", " Trần Viết Mạnh Cường — MSV: 23K4080003\nLớp: K57 Tin học Kinh tế — Khoa HTTT Kinh tế")
    ]
    for th_title, th_desc in thanks:
        pt = tf12_r.add_paragraph()
        pt.space_before = Pt(8)
        rt1 = pt.add_run()
        rt1.text = th_title + "\n"
        rt1.font.bold = True
        rt1.font.size = Pt(14)
        rt1.font.color.rgb = COLOR_WHITE
        rt2 = pt.add_run()
        rt2.text = th_desc
        rt2.font.size = Pt(12.5)
        rt2.font.color.rgb = RGBColor(199, 210, 254)
        
    # Big Q&A Badge Callout
    qa_badge = s12.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(7.35), Inches(5.35), Inches(4.65), Inches(1.1))
    set_flat_style(qa_badge, COLOR_BLUE_ROYAL)
    tf_qa = qa_badge.text_frame
    tf_qa.word_wrap = True
    tf_qa.margin_left = tf_qa.margin_top = tf_qa.margin_right = tf_qa.margin_bottom = 0
    pqa1 = tf_qa.paragraphs[0]
    pqa1.alignment = PP_ALIGN.CENTER
    rqa1 = pqa1.add_run()
    rqa1.text = "XIN TRÂN TRỌNG CẢM ƠN!"
    rqa1.font.name = FONT_FAMILY
    rqa1.font.size = Pt(20)
    rqa1.font.bold = True
    rqa1.font.color.rgb = COLOR_WHITE
    pqa2 = tf_qa.add_paragraph()
    pqa2.alignment = PP_ALIGN.CENTER
    rqa2 = pqa2.add_run()
    rqa2.text = "Kính mong nhận được ý kiến đóng góp từ Thầy Cô & Hội đồng\n— Q&A SESSION —"
    rqa2.font.name = FONT_FAMILY
    rqa2.font.size = Pt(13.5)
    rqa2.font.color.rgb = COLOR_CYAN

    add_speaker_note(s12,
        "Kính thưa quý Thầy Cô cùng toàn thể các bạn sinh viên!\n"
        "Đề tài 'Xây dựng hệ thống chấm công và quản lý nhân viên trên nền tảng Web' đã hoàn thành xuất sắc các mục tiêu đặt ra, giúp em củng cố sâu sắc các kiến thức về kiến trúc web, C# .NET, Blazor và quản trị cơ sở dữ liệu quan hệ.\n"
        "Em xin trân trọng cảm ơn Giảng viên hướng dẫn và các Thầy Cô giáo đã tận tâm chỉ bảo em trong suốt học phần.\n"
        "Em xin trân trọng cảm ơn và kính mời Thầy Cô cùng các bạn sinh viên đặt câu hỏi để em được làm rõ hơn về dự án!"
    )
    
    # -------------------------------------------------------------
        # -------------------------------------------------------------
        # -------------------------------------------------------------
    # APPLY 15 DYNAMIC, DIVERSE SLIDE TRANSITIONS (EACH SLIDE A UNIQUE EFFECT)
    # -------------------------------------------------------------
    slide_transitions = [
        '<p:fade/>',
        '<p:push dir="r"/>',
        '<p:comb orient="horz"/>',
        '<p:split orient="horz" dir="out"/>',
        '<p:checker dir="horz"/>',
        '<p:wipe dir="r"/>',
        '<p:circle/>',
        '<p:wheel spokes="4"/>',
        '<p:blinds dir="vert"/>',
        '<p:diamond/>',
        '<p:zoom dir="in"/>',
        '<p:dissolve/>'
    ]
    for idx, slide in enumerate(prs.slides):
        add_transition_to_slide(slide, slide_transitions[idx])

    # SAVE PPTX FILE
    # -------------------------------------------------------------
    output_filename = "Bao_cao_Lap_trinh_Ung_dung_Web_Bang_Cham_Cong.pptx"
    prs.save(output_filename)
    print(f"Presentation saved successfully with slide transitions as: {output_filename}")

if __name__ == "__main__":
    build_presentation()
