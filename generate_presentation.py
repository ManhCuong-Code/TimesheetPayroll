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
# CONSTANTS & COLOR PALETTE
# -------------------------------------------------------------
COLOR_NAVY_DARK = RGBColor(15, 23, 42)      # #0F172A - Deep Navy Background for Cover & Closing
COLOR_NAVY_HEADER = RGBColor(30, 41, 59)    # #1E293B - Dark Slate for header/cards
COLOR_BLUE_ROYAL = RGBColor(37, 99, 235)    # #2563EB - Tech Royal Blue
COLOR_CYAN = RGBColor(14, 165, 233)         # #0EA5E9 - Vibrant Cyan
COLOR_TEAL = RGBColor(13, 148, 136)         # #0D9488 - Teal
COLOR_EMERALD = RGBColor(16, 185, 129)      # #10B981 - Emerald Green
COLOR_AMBER = RGBColor(245, 158, 11)        # #F59E0B - Amber
COLOR_ROSE = RGBColor(239, 68, 68)          # #EF4444 - Rose Red
COLOR_PURPLE = RGBColor(147, 51, 234)       # #9333EA - Purple

COLOR_BG_LIGHT = RGBColor(248, 250, 252)    # #F8FAFC - Light Slide Background
COLOR_CARD_BG = RGBColor(255, 255, 255)     # #FFFFFF - Pure White
COLOR_BORDER = RGBColor(226, 232, 240)      # #E2E8F0 - Light Gray Border
COLOR_TEXT_MAIN = RGBColor(15, 23, 42)      # #0F172A - Dark Text
COLOR_TEXT_MUTED = RGBColor(100, 116, 139)  # #64748B - Muted Gray Text
COLOR_WHITE = RGBColor(255, 255, 255)       # #FFFFFF - Crisp White
COLOR_CODE_BG = RGBColor(24, 32, 47)        # Dark code background

FONT_TITLE = "Segoe UI"
FONT_BODY = "Segoe UI"
FONT_CODE = "Consolas"

def set_shape_flat(shape, fill_color, border_color=None, border_width=1):
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill_color
    if border_color:
        shape.line.color.rgb = border_color
        shape.line.width = Pt(border_width)
    else:
        shape.line.fill.background()

def create_header(slide, breadcrumb_text, title_text, subtitle_text=""):
    # Top blue accent bar
    top_bar = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.08))
    set_shape_flat(top_bar, COLOR_BLUE_ROYAL)
    
    # Breadcrumb tag pill
    tag_bg = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(0.35), Inches(4.8), Inches(0.32))
    set_shape_flat(tag_bg, RGBColor(239, 246, 255), RGBColor(191, 219, 254), 1)
    tf_tag = tag_bg.text_frame
    tf_tag.word_wrap = True
    tf_tag.margin_left = tf_tag.margin_top = tf_tag.margin_right = tf_tag.margin_bottom = 0
    p_tag = tf_tag.paragraphs[0]
    p_tag.alignment = PP_ALIGN.CENTER
    run_tag = p_tag.add_run()
    run_tag.text = breadcrumb_text.upper()
    run_tag.font.name = FONT_BODY
    run_tag.font.size = Pt(13.5)
    run_tag.font.bold = True
    run_tag.font.color.rgb = COLOR_BLUE_ROYAL
    
    # Title & Subtitle box
    tb = slide.shapes.add_textbox(Inches(0.8), Inches(0.72), Inches(11.733), Inches(0.85))
    tf = tb.text_frame
    tf.word_wrap = True
    tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
    
    p_title = tf.paragraphs[0]
    p_title.space_after = Pt(2)
    run_title = p_title.add_run()
    run_title.text = title_text
    run_title.font.name = FONT_TITLE
    run_title.font.size = Pt(28)
    run_title.font.bold = True
    run_title.font.color.rgb = COLOR_NAVY_DARK
    
    if subtitle_text:
        p_sub = tf.add_paragraph()
        run_sub = p_sub.add_run()
        run_sub.text = subtitle_text
        run_sub.font.name = FONT_BODY
        run_sub.font.size = Pt(15)
        run_sub.font.color.rgb = COLOR_TEXT_MUTED

def create_footer(slide, current_slide, total_slides=15):
    line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.8), Inches(7.0), Inches(11.733), Inches(0.015))
    set_shape_flat(line, COLOR_BORDER)
    
    tb_l = slide.shapes.add_textbox(Inches(0.8), Inches(7.05), Inches(5.5), Inches(0.35))
    tf_l = tb_l.text_frame
    tf_l.word_wrap = True
    tf_l.margin_left = tf_l.margin_top = tf_l.margin_right = tf_l.margin_bottom = 0
    p_l = tf_l.paragraphs[0]
    r_l = p_l.add_run()
    r_l.text = "Lập trình ứng dụng Web | Đồ án: Timesheet & Payroll Automation"
    r_l.font.name = FONT_BODY
    r_l.font.size = Pt(12.5)
    r_l.font.color.rgb = COLOR_TEXT_MUTED
    
    tb_c = slide.shapes.add_textbox(Inches(5.5), Inches(7.05), Inches(5.0), Inches(0.35))
    tf_c = tb_c.text_frame
    tf_c.word_wrap = True
    tf_c.margin_left = tf_c.margin_top = tf_c.margin_right = tf_c.margin_bottom = 0
    p_c = tf_c.paragraphs[0]
    r_c = p_c.add_run()
    r_c.text = "Trần Viết Mạnh Cường (23K4080003) | ĐH Kinh tế Huế"
    r_c.font.name = FONT_BODY
    r_c.font.size = Pt(12.5)
    r_c.font.color.rgb = COLOR_TEXT_MUTED
    
    badge = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(11.633), Inches(7.05), Inches(0.9), Inches(0.28))
    set_shape_flat(badge, COLOR_NAVY_DARK)
    tf_b = badge.text_frame
    tf_b.margin_left = tf_b.margin_top = tf_b.margin_right = tf_b.margin_bottom = 0
    p_b = tf_b.paragraphs[0]
    p_b.alignment = PP_ALIGN.CENTER
    r_b = p_b.add_run()
    r_b.text = f"{current_slide:02d} / {total_slides:02d}"
    r_b.font.name = FONT_BODY
    r_b.font.size = Pt(12.5)
    r_b.font.bold = True
    r_b.font.color.rgb = COLOR_WHITE

def add_card(slide, left, top, width, height, stripe_color=None, bg_color=COLOR_CARD_BG, border_color=COLOR_BORDER):
    card = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
    set_shape_flat(card, bg_color, border_color, 1)
    if stripe_color:
        stripe = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, Inches(0.08))
        set_shape_flat(stripe, stripe_color)
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
    blank_layout = prs.slide_layouts[6] # blank layout
    
    # -------------------------------------------------------------
    # SLIDE 1: TRANG BÌA (COVER SLIDE - DARK NAVY THEME)
    # -------------------------------------------------------------
    s1 = prs.slides.add_slide(blank_layout)
    bg1 = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg1, COLOR_NAVY_DARK)
    
    # Accent top glowing stripe
    stripe1 = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.12))
    set_shape_flat(stripe1, COLOR_CYAN)
    
    # Logo HCE
    logo_path = "report_assets/hce_logo.png"
    if os.path.exists(logo_path):
        s1.shapes.add_picture(logo_path, Inches(0.8), Inches(0.55), width=Inches(1.1))
        
    # University Header Text
    tb_uni = s1.shapes.add_textbox(Inches(2.1), Inches(0.52), Inches(10.2), Inches(0.9))
    tf_uni = tb_uni.text_frame
    tf_uni.word_wrap = True
    tf_uni.margin_left = tf_uni.margin_top = tf_uni.margin_right = tf_uni.margin_bottom = 0
    p1 = tf_uni.paragraphs[0]
    r1 = p1.add_run()
    r1.text = "BỘ GIÁO DỤC VÀ ĐÀO TẠO  •  ĐẠI HỌC HUẾ"
    r1.font.name = FONT_BODY
    r1.font.size = Pt(15)
    r1.font.bold = True
    r1.font.color.rgb = COLOR_CYAN
    
    p2 = tf_uni.add_paragraph()
    r2 = p2.add_run()
    r2.text = "TRƯỜNG ĐẠI HỌC KINH TẾ  —  KHOA HỆ THỐNG THÔNG TIN KINH TẾ"
    r2.font.name = FONT_BODY
    r2.font.size = Pt(17.5)
    r2.font.bold = True
    r2.font.color.rgb = COLOR_WHITE
    
    # Course Badge Tag
    badge_course = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(1.8), Inches(4.8), Inches(0.38))
    set_shape_flat(badge_course, RGBColor(30, 41, 59), COLOR_BLUE_ROYAL, 1.5)
    tf_bc = badge_course.text_frame
    tf_bc.margin_left = tf_bc.margin_top = tf_bc.margin_right = tf_bc.margin_bottom = 0
    p_bc = tf_bc.paragraphs[0]
    p_bc.alignment = PP_ALIGN.CENTER
    r_bc = p_bc.add_run()
    r_bc.text = "BÁO CÁO ĐỒ ÁN MÔN HỌC: LẬP TRÌNH ỨNG DỤNG WEB"
    r_bc.font.name = FONT_BODY
    r_bc.font.size = Pt(14)
    r_bc.font.bold = True
    r_bc.font.color.rgb = COLOR_CYAN
    
    # Main Title
    tb_title = s1.shapes.add_textbox(Inches(0.8), Inches(2.35), Inches(11.733), Inches(1.6))
    tf_title = tb_title.text_frame
    tf_title.word_wrap = True
    tf_title.margin_left = tf_title.margin_top = tf_title.margin_right = tf_title.margin_bottom = 0
    
    p_m1 = tf_title.paragraphs[0]
    r_m1 = p_m1.add_run()
    r_m1.text = "XÂY DỰNG HỆ THỐNG ỨNG DỤNG WEB\nCHẤM CÔNG VÀ TÍNH LƯƠNG TỰ ĐỘNG"
    r_m1.font.name = FONT_TITLE
    r_m1.font.size = Pt(36)
    r_m1.font.bold = True
    r_m1.font.color.rgb = COLOR_WHITE
    
    p_m2 = tf_title.add_paragraph()
    p_m2.space_before = Pt(8)
    r_m2 = p_m2.add_run()
    r_m2.text = "Timesheet & Payroll Automation System — Clean Architecture 4 Tầng & .NET 10 Blazor"
    r_m2.font.name = FONT_BODY
    r_m2.font.size = Pt(18)
    r_m2.font.color.rgb = RGBColor(148, 163, 184)
    
    # 4 Tech Pills
    pills = [
        ("Clean Architecture 4 Tầng", COLOR_BLUE_ROYAL),
        (".NET 10 Blazor Server", COLOR_CYAN),
        ("SQL Server & Dapper ORM", COLOR_TEAL),
        ("Tích hợp Trợ lý Lương AI", COLOR_AMBER)
    ]
    pill_w = Inches(2.78)
    for i, (pill_txt, pill_col) in enumerate(pills):
        px = Inches(0.8 + i * 2.98)
        pill_shape = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, px, Inches(4.2), pill_w, Inches(0.4))
        set_shape_flat(pill_shape, RGBColor(30, 41, 59), pill_col, 1.2)
        tf_p = pill_shape.text_frame
        tf_p.margin_left = tf_p.margin_top = tf_p.margin_right = tf_p.margin_bottom = 0
        p_p = tf_p.paragraphs[0]
        p_p.alignment = PP_ALIGN.CENTER
        r_p = p_p.add_run()
        r_p.text = pill_txt
        r_p.font.name = FONT_BODY
        r_p.font.size = Pt(14)
        r_p.font.bold = True
        r_p.font.color.rgb = COLOR_WHITE

    # Bottom Metadata Card Container
    card_meta = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(4.85), Inches(11.733), Inches(1.95))
    set_shape_flat(card_meta, RGBColor(30, 41, 59), RGBColor(51, 65, 85), 1.5)
    
    # 4 Columns of metadata
    meta_cols = [
        ("GIẢNG VIÊN HƯỚNG DẪN", "TS. Hà Ngọc Long", "Khoa Hệ thống Thông tin Kinh tế\nTrường Đại học Kinh tế, ĐH Huế"),
        ("SINH VIÊN THỰC HIỆN", "Trần Viết Mạnh Cường", "Mã SV: 23K4080003\nLớp: K57 Tin học Kinh tế"),
        ("NGÀNH & CHUYÊN NGÀNH", "Hệ thống Thông tin Quản lý", "Chuyên ngành: Tin học Kinh tế\nKhóa học: 2023 – 2027"),
        ("THÔNG TIN BÁO CÁO", "Đồ Án Lập Trình Web", "Thời gian: Năm 2026\nĐịa điểm: TP. Huế")
    ]
    col_w = Inches(2.78)
    for idx, (head_t, name_t, sub_t) in enumerate(meta_cols):
        cx = Inches(0.95 + idx * 2.9)
        tb_col = s1.shapes.add_textbox(cx, Inches(5.0), col_w, Inches(1.65))
        tf_col = tb_col.text_frame
        tf_col.word_wrap = True
        tf_col.margin_left = tf_col.margin_top = tf_col.margin_right = tf_col.margin_bottom = 0
        
        ph = tf_col.paragraphs[0]
        rh = ph.add_run()
        rh.text = head_t
        rh.font.name = FONT_BODY
        rh.font.size = Pt(13.5)
        rh.font.bold = True
        rh.font.color.rgb = COLOR_CYAN
        
        pn = tf_col.add_paragraph()
        pn.space_before = Pt(4)
        rn = pn.add_run()
        rn.text = name_t
        rn.font.name = FONT_TITLE
        rn.font.size = Pt(17.5)
        rn.font.bold = True
        rn.font.color.rgb = COLOR_WHITE
        
        ps = tf_col.add_paragraph()
        ps.space_before = Pt(3)
        rs = ps.add_run()
        rs.text = sub_t
        rs.font.name = FONT_BODY
        rs.font.size = Pt(13.5)
        rs.font.color.rgb = RGBColor(148, 163, 184)

    add_speaker_note(s1, 
        "Kính thưa Thầy TS. Hà Ngọc Long cùng toàn thể các bạn sinh viên!\n"
        "Em tên là Trần Viết Mạnh Cường, sinh viên lớp K57 Tin học Kinh tế, ngành Hệ thống Thông tin Quản lý - Trường Đại học Kinh tế, Đại học Huế.\n"
        "Hôm nay, em xin phép được trình bày báo cáo đồ án học phần Lập trình ứng dụng Web với đề tài: 'Xây dựng hệ thống ứng dụng Web Chấm công và Tính lương tự động (Timesheet & Payroll Automation System)'.\n"
        "Đề tài được xây dựng trên nền tảng kiến trúc sạch Clean Architecture, công nghệ .NET 10 Blazor Server, kết nối Microsoft SQL Server qua Dapper Micro-ORM và tích hợp Trợ lý Lương AI thông minh."
    )
    
    # -------------------------------------------------------------
    # SLIDE 2: GIỚI THIỆU & BỐI CẢNH ĐỀ TÀI
    # -------------------------------------------------------------
    s2 = prs.slides.add_slide(blank_layout)
    bg2 = s2.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg2, COLOR_BG_LIGHT)
    create_header(s2, "Bối cảnh & Mục tiêu", "01. Giới thiệu tổng quan đề tài & Lý do phát triển", "Giải pháp tự động hóa toàn diện từ theo dõi ngày công đến quyết toán tiền lương doanh nghiệp")
    create_footer(s2, 2)
    
    # Left Card: Bối cảnh & Thực trạng
    c2_l = add_card(s2, Inches(0.8), Inches(1.75), Inches(5.65), Inches(3.85), COLOR_ROSE)
    tb2_l = s2.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.15), Inches(3.45))
    tf2_l = tb2_l.text_frame
    tf2_l.word_wrap = True
    tf2_l.margin_left = tf2_l.margin_top = tf2_l.margin_right = tf2_l.margin_bottom = 0
    
    p = tf2_l.paragraphs[0]
    r = p.add_run()
    r.text = "Thực trạng Quản lý Tiền lương tại Doanh nghiệp SME"
    r.font.name = FONT_TITLE
    r.font.size = Pt(18)
    r.font.bold = True
    r.font.color.rgb = COLOR_NAVY_DARK
    
    bullets_l = [
        ("Nghiệp vụ tài chính cốt lõi:", " Đi đôi với quản lý nhân sự, tính lương là công tác trọng yếu tác động trực tiếp đến quyền lợi người lao động và nghĩa vụ pháp lý của doanh nghiệp."),
        ("Sự lệ thuộc vào bảng tính Excel:", " Hầu hết doanh nghiệp vừa và nhỏ (20-50 nhân sự) vẫn dùng Excel thủ công, tiềm ẩn rủi ro kéo nhầm công thức gây sai lệch tiền lương."),
        ("Nguy cơ mất an toàn thông tin:", " Chia sẻ chung file tính lương dẫn đến lộ mức thu nhập cá nhân, gây mất đoàn kết nội bộ và vi phạm quyền riêng tư."),
        ("Khó khăn khi cập nhật chính sách:", " Các quy định về BHXH (8%), BHYT (1.5%), BHTN (1%) và biểu thuế TNCN thường xuyên thay đổi, rất khó đồng bộ trên bảng tính phân tán.")
    ]
    for b_title, b_desc in bullets_l:
        pb = tf2_l.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run()
        rb1.text = "• " + b_title
        rb1.font.bold = True
        rb1.font.size = Pt(15)
        rb1.font.color.rgb = COLOR_ROSE
        rb2 = pb.add_run()
        rb2.text = b_desc
        rb2.font.size = Pt(14)
        rb2.font.color.rgb = COLOR_TEXT_MAIN

    # Right Card: Mục tiêu giải pháp
    c2_r = add_card(s2, Inches(6.85), Inches(1.75), Inches(5.65), Inches(3.85), COLOR_EMERALD)
    tb2_r = s2.shapes.add_textbox(Inches(7.1), Inches(1.95), Inches(5.15), Inches(3.45))
    tf2_r = tb2_r.text_frame
    tf2_r.word_wrap = True
    tf2_r.margin_left = tf2_r.margin_top = tf2_r.margin_right = tf2_r.margin_bottom = 0
    
    p = tf2_r.paragraphs[0]
    r = p.add_run()
    r.text = "3 Trụ cột Mục tiêu Đột phá của Ứng dụng Web"
    r.font.name = FONT_TITLE
    r.font.size = Pt(18)
    r.font.bold = True
    r.font.color.rgb = COLOR_NAVY_DARK
    
    bullets_r = [
        ("Tự động hóa 1-Chạm (1-Click Run):", " Khép kín quy trình từ điểm danh, tổng hợp ngày công thực tế đến tính toán bảng lương hoàn tất chỉ trong 1.5 giây."),
        ("Chuẩn hóa 100% theo Luật Lao Động:", " Động cơ nạp tỷ lệ trích nộp bảo hiểm động (10.5%) và biểu thuế lũy tiến từng phần, khóa sổ kỳ lương bất biến (Read-Only)."),
        ("Bảo mật đa tầng & Quyền riêng tư:", " Phân quyền RBAC, mật khẩu băm BCrypt, chống lộ lương IDOR và chặn truy cập trái phép bằng HTTP 403."),
        ("Cổng tự phục vụ & Trợ lý AI:", " Nhân viên chủ động tra cứu phiếu lương cá nhân an toàn và tương tác hỏi đáp với Trợ lý Lương AI (Google Gemini API).")
    ]
    for b_title, b_desc in bullets_r:
        pb = tf2_r.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run()
        rb1.text = "✓ " + b_title
        rb1.font.bold = True
        rb1.font.size = Pt(15)
        rb1.font.color.rgb = COLOR_EMERALD
        rb2 = pb.add_run()
        rb2.text = b_desc
        rb2.font.size = Pt(14)
        rb2.font.color.rgb = COLOR_TEXT_MAIN

    # Bottom 3 KPI Metric Cards
    metrics = [
        ("QUY MÔ NHÂN SỰ", "20 – 50", "Nhân sự doanh nghiệp SME", COLOR_BLUE_ROYAL),
        ("TÍNH TOÁN BẢNG LƯƠNG", "1-Click Run", "Tự động hoàn toàn trong < 1.5s", COLOR_EMERALD),
        ("ĐỘ CHÍNH XÁC & BẢO MẬT", "100%", "Chuẩn luật & Chống lộ IDOR", COLOR_PURPLE)
    ]
    m_w = Inches(3.64)
    for i, (m_tag, m_val, m_sub, m_col) in enumerate(metrics):
        mx = Inches(0.8 + i * 4.04)
        m_card = add_card(s2, mx, Inches(5.75), m_w, Inches(1.1), bg_color=COLOR_WHITE)
        tb_m = s2.shapes.add_textbox(mx + Inches(0.2), Inches(5.82), m_w - Inches(0.4), Inches(0.95))
        tf_m = tb_m.text_frame
        tf_m.word_wrap = True
        tf_m.margin_left = tf_m.margin_top = tf_m.margin_right = tf_m.margin_bottom = 0
        
        pm1 = tf_m.paragraphs[0]
        rm1 = pm1.add_run()
        rm1.text = m_tag
        rm1.font.name = FONT_BODY
        rm1.font.size = Pt(12.5)
        rm1.font.bold = True
        rm1.font.color.rgb = m_col
        
        pm2 = tf_m.add_paragraph()
        rm2 = pm2.add_run()
        rm2.text = m_val
        rm2.font.name = FONT_TITLE
        rm2.font.size = Pt(28)
        rm2.font.bold = True
        rm2.font.color.rgb = COLOR_NAVY_DARK
        
        pm3 = tf_m.add_paragraph()
        rm3 = pm3.add_run()
        rm3.text = m_sub
        rm3.font.name = FONT_BODY
        rm3.font.size = Pt(12.5)
        rm3.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s2, 
        "Về bối cảnh của đề tài:\n"
        "Trong mọi doanh nghiệp, tiền lương là huyết mạch duy trì hoạt động. Tuy nhiên, tại các doanh nghiệp quy mô 20-50 nhân sự, việc tính lương thủ công bằng Excel thường gây ra 3 vấn đề nhức nhối: thứ nhất là dễ kéo nhầm ô công thức; thứ hai là mất từ 2-3 ngày cuối tháng để đối soát phiếu chấm công; và thứ ba là nguy cơ lộ bí mật thu nhập khi chia sẻ file tính.\n"
        "Mục tiêu cốt lõi của đề tài là xây dựng một ứng dụng web hoàn chỉnh, giải phóng kế toán khỏi các thao tác thủ công với tính năng tính lương 1-chạm (1-Click Run), đảm bảo chuẩn xác 100% theo Luật Lao Động và bảo mật thu nhập tuyệt đối."
    )

    # -------------------------------------------------------------
    # SLIDE 3: TỔNG QUAN KIẾN THỨC CỐT LÕI
    # -------------------------------------------------------------
    s3 = prs.slides.add_slide(blank_layout)
    bg3 = s3.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg3, COLOR_BG_LIGHT)
    create_header(s3, "Tổng quan kiến thức", "02. Các khái niệm cốt lõi trong lập trình ứng dụng Web", "Nền tảng kiến trúc phần mềm, bảo mật đa tầng và cơ sở dữ liệu quan hệ doanh nghiệp")
    create_footer(s3, 3)
    
    concepts = [
        ("01. KIẾN TRÚC SẠCH (CLEAN ARCHITECTURE)", COLOR_BLUE_ROYAL, [
            ("Mô hình 4 Tầng Phân Tách:", " Presentation (UI), Use Cases (Nghiệp vụ), Core Domain (Miền cốt lõi) và Infrastructure (Hạ tầng DB)."),
            ("Quy tắc Phụ thuộc (Dependency Rule):", " Chiều phụ thuộc luôn hướng vào trung tâm (Domain). Miền nghiệp vụ không phụ thuộc bất kỳ thư viện hay framework bên ngoài nào."),
            ("Lợi ích kỹ thuật:", " Mã nguồn độc lập công nghệ UI và CSDL, khả năng viết Unit Test cao, bảo trì và mở rộng linh hoạt.")
        ]),
        ("02. MÔ HÌNH ỨNG DỤNG WEB INTERACTIVE (BLAZOR SERVER)", COLOR_CYAN, [
            ("Giao tiếp Real-time SignalR:", " Sử dụng kênh WebSocket duy trì trạng thái phiên hai chiều giữa Client và Server, phản hồi DOM tức thì mà không cần nạp lại trang."),
            ("Mô hình Single Page Application (SPA):", " Mang lại trải nghiệm người dùng mượt mà, tốc độ chuyển trang dưới 1.2 giây."),
            ("Lập trình đồng nhất (Unified C#):", " Sử dụng hoàn toàn ngôn ngữ C# cả giao diện lẫn xử lý nghiệp vụ, tận dụng tối đa hệ thống kiểm tra kiểu tĩnh (Strongly Typed).")
        ]),
        ("03. BẢO MẬT PHÂN QUYỀN RBAC & BCRYPT", COLOR_PURPLE, [
            ("Xác thực Mật khẩu BCrypt:", " Băm một chiều kèm Salt ngẫu nhiên với hệ số WorkFactor = 11, chống tấn công dò từ điển (Dictionary Attack) và Rainbow Table."),
            ("Kiểm soát truy cập dựa trên vai trò (RBAC):", " Phân định rạch ròi 3 vai trò `ROLE_EMPLOYEE`, `ROLE_HR_ACCOUNTANT`, `ROLE_ADMIN`."),
            ("Phòng vệ chống IDOR & URL Bypass:", " Chặn truy cập trái phép bằng `AuthorizeRouteView` (HTTP 403 Forbidden) và bảo vệ dữ liệu cấp bản ghi.")
        ]),
        ("04. CƠ SỞ DỮ LIỆU CHUẨN HÓA 3NF & GIAO DỊCH ACID", COLOR_TEAL, [
            ("Chuẩn hóa Dạng chuẩn 3 (3NF):", " Loại bỏ hoàn toàn dư thừa dữ liệu và dị thường cập nhật trên 11 bảng thực thể quan hệ."),
            ("Truy xuất dữ liệu siêu tốc Dapper Micro-ORM:", " 100% câu lệnh SQL tham số hóa (Parameterized Query), triệt tiêu nguy cơ SQL Injection."),
            ("Toàn vẹn Giao dịch ACID:", " Áp dụng cơ chế Database Transaction cho cặp Header-Line (`payslips` - `payslip_lines`), đảm bảo tính nguyên tử (Atomicity).")
        ])
    ]
    
    card_w = Inches(5.65)
    card_h = Inches(2.45)
    for idx, (c_title, c_color, c_items) in enumerate(concepts):
        col = idx % 2
        row = idx // 2
        cx = Inches(0.8 + col * 6.05)
        cy = Inches(1.75 + row * 2.6)
        
        add_card(s3, cx, cy, card_w, card_h, c_color)
        tb_c = s3.shapes.add_textbox(cx + Inches(0.25), cy + Inches(0.18), card_w - Inches(0.5), card_h - Inches(0.3))
        tf_c = tb_c.text_frame
        tf_c.word_wrap = True
        tf_c.margin_left = tf_c.margin_top = tf_c.margin_right = tf_c.margin_bottom = 0
        
        pc = tf_c.paragraphs[0]
        rc = pc.add_run()
        rc.text = c_title
        rc.font.name = FONT_TITLE
        rc.font.size = Pt(16.5)
        rc.font.bold = True
        rc.font.color.rgb = c_color
        
        for item_h, item_d in c_items:
            pi = tf_c.add_paragraph()
            pi.space_before = Pt(4)
            ri1 = pi.add_run()
            ri1.text = "• " + item_h
            ri1.font.bold = True
            ri1.font.size = Pt(14)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = item_d
            ri2.font.size = Pt(13.5)
            ri2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s3, 
        "Về nền tảng kiến thức lý thuyết:\n"
        "Đề tài kết hợp 4 trụ cột công nghệ then chốt của kỹ thuật lập trình web hiện đại:\n"
        "1. Kiến trúc Clean Architecture của Robert C. Martin giúp tách biệt rõ ràng các tầng trách nhiệm, bảo đảm tầng Core Domain hoàn toàn độc lập với UI hay Database.\n"
        "2. Nền tảng Blazor Server sử dụng SignalR WebSocket cho tương tác hai chiều thời gian thực, đem lại trải nghiệm ứng dụng trang đơn SPA mượt mà.\n"
        "3. Bảo mật đa tầng kết hợp thuật toán băm BCrypt, cơ chế phân quyền RBAC và phòng vệ chống lỗi IDOR.\n"
        "4. Cơ sở dữ liệu chuẩn hóa 3NF trên SQL Server kết hợp Dapper Micro-ORM và giao dịch ACID đảm bảo toàn vẹn dữ liệu kế toán tuyệt đối."
    )

    # -------------------------------------------------------------
    # SLIDE 4: PHÂN TÍCH YÊU CẦU HỆ THỐNG
    # -------------------------------------------------------------
    s4 = prs.slides.add_slide(blank_layout)
    bg4 = s4.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg4, COLOR_BG_LIGHT)
    create_header(s4, "Phân tích yêu cầu", "03. Phân tích yêu cầu bài toán & Ma trận phân quyền (RBAC)", "Xác định rõ vấn đề cần giải quyết, nhóm đối tượng sử dụng và phạm vi chức năng")
    create_footer(s4, 4)
    
    # Left Card: Nỗi đau Excel cần giải quyết
    c4_l = add_card(s4, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_ROSE)
    tb4_l = s4.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.15), Inches(4.65))
    tf4_l = tb4_l.text_frame
    tf4_l.word_wrap = True
    tf4_l.margin_left = tf4_l.margin_top = tf4_l.margin_right = tf4_l.margin_bottom = 0
    
    p = tf4_l.paragraphs[0]
    r = p.add_run()
    r.text = "4 Nỗi Đau Lớn Của Phương Pháp Quản Lý Thủ Công"
    r.font.name = FONT_TITLE
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_ROSE
    
    pain_points = [
        ("Rất dễ kéo lệch ô công thức Excel:", " Kế toán chỉ cần copy nhầm hàng hoặc sai dấu ngoặc đơn là lương nhân viên bị tính thiếu hoặc thừa hàng triệu đồng, dẫn đến tranh chấp gay gắt."),
        ("Tiêu tốn nguồn lực và thời gian:", " Cuối tháng kế toán mất từ 2 đến 3 ngày làm việc chỉ để đối soát từng tờ phiếu chấm công, kiểm tra đơn phép giấy và dò tìm ngày nghỉ."),
        ("Lộ bí mật thu nhập nội bộ (Privacy):", " Chia sẻ chung một file Excel bảng lương rất dễ khiến nhân viên dòm ngó mức thu nhập của đồng nghiệp, gây mất đoàn kết và vi phạm bảo mật."),
        ("Bất cập khi thay đổi chính sách pháp luật:", " Mức lương cơ sở, tỷ lệ bảo hiểm (10.5%) hay mức giảm trừ gia cảnh thuế TNCN thay đổi thường xuyên. Nếu bị hardcode trong Excel thì cực kỳ khó cập nhật.")
    ]
    for p_title, p_desc in pain_points:
        pb = tf4_l.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run()
        rb1.text = "⚠ " + p_title
        rb1.font.bold = True
        rb1.font.size = Pt(14)
        rb1.font.color.rgb = COLOR_NAVY_DARK
        rb2 = pb.add_run()
        rb2.text = p_desc
        rb2.font.size = Pt(14)
        rb2.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card: Ma trận phân quyền 3 Roles
    c4_r = add_card(s4, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_BLUE_ROYAL)
    tb4_r = s4.shapes.add_textbox(Inches(7.1), Inches(1.95), Inches(5.15), Inches(4.65))
    tf4_r = tb4_r.text_frame
    tf4_r.word_wrap = True
    tf4_r.margin_left = tf4_r.margin_top = tf4_r.margin_right = tf4_r.margin_bottom = 0
    
    p = tf4_r.paragraphs[0]
    r = p.add_run()
    r.text = "Ma Trận Phân Quyền Người Dùng Hệ Thống (RBAC)"
    r.font.name = FONT_TITLE
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_BLUE_ROYAL
    
    roles = [
        ("VAI TRÒ 1: NHÂN VIÊN (ROLE_EMPLOYEE)", COLOR_CYAN, [
            "Điểm danh Check-in / Check-out hàng ngày theo ca làm việc.",
            "Nộp đơn xin nghỉ phép trực tuyến có xác thực validation ngày tháng.",
            "Xem phiếu lương cá nhân bảo mật (Anti-IDOR - không xem lén người khác).",
            "Tương tác cùng Trợ lý Lương AI hỏi đáp và mô phỏng What-If thu nhập."
        ]),
        ("VAI TRÒ 2: KẾ TOÁN / NHÂN SỰ (ROLE_HR_ACCOUNTANT)", COLOR_EMERALD, [
            "Xét duyệt hoặc từ chối các đơn xin nghỉ phép của nhân viên.",
            "Thực thi tính toán bảng lương tự động hàng loạt 1-Chạm (1-Click Run).",
            "Xem bảng lương tổng hợp đối soát và Chốt khóa sổ kỳ lương (LOCKED).",
            "Xuất báo cáo bảng lương Excel Mẫu số 02-LĐTL và file CSV ngân hàng."
        ]),
        ("VAI TRÒ 3: QUẢN TRỊ VIÊN HỆ THỐNG (ROLE_ADMIN)", COLOR_PURPLE, [
            "Quản lý tài khoản người dùng, cấp quyền và quản lý phòng ban/hợp đồng.",
            "Cấu hình các tỷ lệ trích nộp bảo hiểm động (CSDL deduction_rates).",
            "Giám sát toàn bộ tiến trình chốt sổ và bảo toàn dữ liệu tài chính."
        ])
    ]
    for r_name, r_col, r_tasks in roles:
        pr = tf4_r.add_paragraph()
        pr.space_before = Pt(8)
        rr = pr.add_run()
        rr.text = r_name
        rr.font.bold = True
        rr.font.size = Pt(14)
        rr.font.color.rgb = r_col
        for t in r_tasks:
            pt = tf4_r.add_paragraph()
            pt.space_before = Pt(2)
            rt = pt.add_run()
            rt.text = "  • " + t
            rt.font.size = Pt(13.5)
            rt.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s4, 
        "Về phân tích yêu cầu:\n"
        "Hệ thống tập trung giải quyết 4 nỗi đau lớn nhất của bảng tính Excel: công thức dễ sai lệch, tốn thời gian đối soát thủ công, rủi ro lộ thu nhập và khó cập nhật khi luật thay đổi.\n"
        "Về người dùng, hệ thống áp dụng ma trận phân quyền RBAC phân định rạch ròi 3 vai trò:\n"
        "- Nhân viên chỉ có quyền thao tác chấm công, nộp phép và xem phiếu lương của chính mình, đảm bảo tính riêng tư tuyệt đối.\n"
        "- Kế toán thực hiện duyệt phép, chạy tính lương tự động, khóa sổ kỳ lương và xuất báo cáo gửi ngân hàng.\n"
        "- Quản trị viên quản lý danh mục tài khoản và cấu hình tỷ lệ bảo hiểm động."
    )

    # -------------------------------------------------------------
    # SLIDE 5: CÔNG NGHỆ SỬ DỤNG
    # -------------------------------------------------------------
    s5 = prs.slides.add_slide(blank_layout)
    bg5 = s5.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg5, COLOR_BG_LIGHT)
    create_header(s5, "Công nghệ sử dụng", "04. Ngăn xếp công nghệ phát triển đạt chuẩn công nghiệp", "Sự kết hợp tối ưu giữa hệ sinh thái Microsoft .NET 10, SQL Server, Dapper và AI")
    create_footer(s5, 5)
    
    tech_stacks = [
        ("GIAO DIỆN & WEB (UI/UX)", COLOR_CYAN, [
            (".NET 10 Blazor Server", "Interactive Server Components"),
            ("SignalR WebSocket", "Kênh truyền dữ liệu thời gian thực"),
            ("Bootstrap 5.3 & CSS", "Responsive Layout tối ưu mobile 390px"),
            ("FontAwesome 6 Pro", "Bộ biểu tượng giao diện hiện đại"),
            ("Vanilla JS Interop", "Hỗ trợ xuất tệp Base64 tải trực tiếp")
        ]),
        ("BACKEND & KIẾN TRÚC", COLOR_BLUE_ROYAL, [
            ("Ngôn ngữ C# 13", "Tính năng hiện đại, kiểu tĩnh an toàn"),
            ("Clean Architecture", "Tổ chức 4 tầng theo chuẩn Robert Martin"),
            ("Dependency Injection", "Tiêm phụ thuộc lỏng (Loose Coupling)"),
            ("Use Cases Pattern", "Đóng gói ca sử dụng nghiệp vụ độc lập"),
            ("SalaryFormulaEngine", "Động cơ tính toán độc lập 0 dependency")
        ]),
        ("CƠ SỞ DỮ LIỆU & ORM", COLOR_TEAL, [
            ("Microsoft SQL Server", "CSDL quan hệ doanh nghiệp BangChamCongDB"),
            ("Dapper Micro-ORM", "Truy xuất siêu tốc, ánh xạ đối tượng mượt"),
            ("100% Parameterized", "Triệt tiêu hoàn toàn lỗi SQL Injection"),
            ("ACID Transactions", "Toàn vẹn Header-Line khi tính lương"),
            ("CHECK Constraints", "Ràng buộc toàn vẹn lương >= 0 và tháng 1-12")
        ]),
        ("BẢO MẬT & TRÍ TUỆ NHÂN TẠO", COLOR_PURPLE, [
            ("BCrypt.Net-Next", "Băm mật khẩu 1 chiều, Salt ngẫu nhiên"),
            ("RBAC Authorization", "Phân quyền theo vai trò và Claims an toàn"),
            ("AuthorizeRouteView", "Phòng vệ URL, chặn HTTP 403 Forbidden"),
            ("Google Gemini API", "Trợ lý Lương AI giải đáp thắc mắc lương"),
            ("Local Fallback Engine", "Động cơ quy tắc nội bộ khi mất mạng")
        ])
    ]
    
    col_w5 = Inches(2.78)
    col_h5 = Inches(4.0)
    for idx, (t_title, t_col, t_items) in enumerate(tech_stacks):
        tx = Inches(0.8 + idx * 2.98)
        ty = Inches(1.75)
        add_card(s5, tx, ty, col_w5, col_h5, t_col)
        
        tb = s5.shapes.add_textbox(tx + Inches(0.2), ty + Inches(0.18), col_w5 - Inches(0.4), col_h5 - Inches(0.3))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = t_title
        r.font.name = FONT_TITLE
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = t_col
        
        for item_name, item_desc in t_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(7)
            ri1 = pi.add_run()
            ri1.text = "• " + item_name + "\n"
            ri1.font.bold = True
            ri1.font.size = Pt(14)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = "   " + item_desc
            ri2.font.size = Pt(12.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom Architecture principle highlight banner
    banner = add_card(s5, Inches(0.8), Inches(5.95), Inches(11.733), Inches(0.9), bg_color=RGBColor(239, 246, 255), border_color=RGBColor(191, 219, 254))
    tb_b = s5.shapes.add_textbox(Inches(1.05), Inches(6.02), Inches(11.2), Inches(0.75))
    tf_b = tb_b.text_frame
    tf_b.word_wrap = True
    tf_b.margin_left = tf_b.margin_top = tf_b.margin_right = tf_b.margin_bottom = 0
    pb1 = tf_b.paragraphs[0]
    rb1 = pb1.add_run()
    rb1.text = "★ NGUYÊN TẮC CỐT LÕI TRONG LỰA CHỌN CÔNG NGHỆ: "
    rb1.font.name = FONT_BODY
    rb1.font.size = Pt(14)
    rb1.font.bold = True
    rb1.font.color.rgb = COLOR_BLUE_ROYAL
    rb2 = pb1.add_run()
    rb2.text = "Tầng Core Domain được thiết kế cô lập hoàn toàn (Zero Reference), không phụ thuộc vào bất kỳ framework giao diện hay thư viện truy xuất CSDL nào. Điều này bảo đảm khả năng kiểm thử độc lập 100% và dễ dàng bảo trì nâng cấp trong tương lai."
    rb2.font.name = FONT_BODY
    rb2.font.size = Pt(14)
    rb2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s5, 
        "Về ngăn xếp công nghệ được lựa chọn:\n"
        "Em sử dụng công nghệ .NET 10 mới nhất kết hợp Blazor Server cho giao diện tương tác real-time. Phía cơ sở dữ liệu là Microsoft SQL Server được truy xuất qua Dapper Micro-ORM giúp tốc độ thực thi truy vấn đạt mức tối đa.\n"
        "Điểm đặc biệt trong kiến trúc là tầng Core Business hoàn toàn không có tham chiếu bên ngoài (Zero Dependency), tuân thủ nghiêm ngặt nguyên lý Dependency Inversion.\n"
        "Hệ thống cũng tích hợp Google Gemini API kết hợp bộ Fallback Engine đảm bảo Trợ lý Lương AI luôn hoạt động ổn định kể cả khi mất kết nối mạng."
    )

    # -------------------------------------------------------------
    # SLIDE 6: PHÂN TÍCH & THIẾT KẾ KIẾN TRÚC HỆ THỐNG
    # -------------------------------------------------------------
    s6 = prs.slides.add_slide(blank_layout)
    bg6 = s6.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg6, COLOR_BG_LIGHT)
    create_header(s6, "Thiết kế kiến trúc", "05. Kiến trúc Clean Architecture 4 tầng & Sơ đồ Use Case", "Tổ chức mã nguồn rành mạch theo trách nhiệm và mô hình hóa chức năng hệ thống")
    create_footer(s6, 6)
    
    # Left: 4 Tầng Clean Architecture
    c6_l = add_card(s6, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_BLUE_ROYAL)
    tb6_l = s6.shapes.add_textbox(Inches(1.05), Inches(1.9), Inches(5.15), Inches(4.75))
    tf6_l = tb6_l.text_frame
    tf6_l.word_wrap = True
    tf6_l.margin_left = tf6_l.margin_top = tf6_l.margin_right = tf6_l.margin_bottom = 0
    
    p = tf6_l.paragraphs[0]
    r = p.add_run()
    r.text = "Cấu Trúc Giải Pháp Clean Architecture 4 Tầng"
    r.font.name = FONT_TITLE
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_BLUE_ROYAL
    
    layers = [
        ("TẦNG 1: PRESENTATION LAYER (GIAO DIỆN & WEB)", COLOR_CYAN,
         "• TimesheetPayroll.Web (.NET 10 Blazor Interactive Server)\n• AdminPortal & EmployeePortal Pages, Components\n• Responsive CSS 390px Mobile, SignalR WebSocket"),
        ("TẦNG 2: APPLICATION & USE CASES LAYER (NGHIỆP VỤ)", COLOR_BLUE_ROYAL,
         "• TimesheetPayroll.UseCases\n• CalculatePayrollBatchUseCase, ExportExcelUseCase, SalaryAi...\n• Định nghĩa Repository Interfaces (IEmployeeRepository...)"),
        ("TẦNG 3: CORE DOMAIN LAYER (TRUNG TÂM MIỀN NGHIỆP VỤ)", COLOR_EMERALD,
         "• TimesheetPayroll.CoreBusiness (0 External Reference)\n• Entities: Employee, Department, Contract, Timesheet, Payslip...\n• Động cơ tính lương: SalaryFormulaEngine.cs độc lập"),
        ("TẦNG 4: INFRASTRUCTURE & PLUGINS LAYER (HẠ TẦNG CSDL)", COLOR_PURPLE,
         "• TimesheetPayroll.DataStore.SQL.Dapper\n• Triển khai các Interface bằng Dapper Micro-ORM\n• Cơ sở dữ liệu quan hệ Microsoft SQL Server (BangChamCongDB)")
    ]
    for l_name, l_col, l_desc in layers:
        pl = tf6_l.add_paragraph()
        pl.space_before = Pt(6)
        rl = pl.add_run()
        rl.text = l_name
        rl.font.bold = True
        rl.font.size = Pt(13.5)
        rl.font.color.rgb = l_col
        pd = tf6_l.add_paragraph()
        pd.space_before = Pt(1)
        rd = pd.add_run()
        rd.text = l_desc
        rd.font.size = Pt(12.5)
        rd.font.color.rgb = COLOR_TEXT_MAIN

    # Right: Embedded Use Case Diagram
    c6_r = add_card(s6, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_PURPLE)
    
    tb6_r_title = s6.shapes.add_textbox(Inches(7.1), Inches(1.9), Inches(5.15), Inches(0.4))
    tf6_rt = tb6_r_title.text_frame
    tf6_rt.margin_left = tf6_rt.margin_top = tf6_rt.margin_right = tf6_rt.margin_bottom = 0
    prt = tf6_rt.paragraphs[0]
    rrt = prt.add_run()
    rrt.text = "Sơ Đồ Use Case Tổng Quan (PlantUML Overview)"
    rrt.font.name = FONT_TITLE
    rrt.font.size = Pt(17.5)
    rrt.font.bold = True
    rrt.font.color.rgb = COLOR_PURPLE
    
    uc_img_path = "uml/images/01_usecase_overview.png"
    if os.path.exists(uc_img_path):
        s6.shapes.add_picture(uc_img_path, Inches(7.05), Inches(2.35), width=Inches(5.25))
        
    tb6_r_note = s6.shapes.add_textbox(Inches(7.1), Inches(6.15), Inches(5.15), Inches(0.55))
    tf6_rn = tb6_r_note.text_frame
    tf6_rn.word_wrap = True
    tf6_rn.margin_left = tf6_rn.margin_top = tf6_rn.margin_right = tf6_rn.margin_bottom = 0
    prn = tf6_rn.paragraphs[0]
    rrn = prn.add_run()
    rrn.text = "★ Phân định 3 Actor người dùng và 1 AI Assistant điều khiển 19 Use Cases trải dài qua 3 phân hệ cốt lõi: Nhân sự, Chấm công và Quyết toán tiền lương."
    rrn.font.name = FONT_BODY
    rrn.font.size = Pt(12.5)
    rrn.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s6, 
        "Về thiết kế kiến trúc hệ thống:\n"
        "Em áp dụng triệt để kiến trúc Clean Architecture 4 tầng. Phía bên trái là cấu trúc phân tầng: tầng Web chỉ phụ thuộc vào Use Cases, tầng Use Cases chỉ phụ thuộc vào Core Domain, và tầng Hạ tầng DataStore triển khai các interface được định nghĩa ở Use Cases.\n"
        "Phía bên phải là sơ đồ Use Case tổng quan được sinh tự động từ PlantUML, gồm 19 Use Cases phân định chặt chẽ quyền hạn của Nhân viên, Kế toán và Admin."
    )

    # -------------------------------------------------------------
    # SLIDE 7: THIẾT KẾ CƠ SỞ DỮ LIỆU QUAN HỆ (3NF & ACID)
    # -------------------------------------------------------------
    s7 = prs.slides.add_slide(blank_layout)
    bg7 = s7.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg7, COLOR_BG_LIGHT)
    create_header(s7, "Cơ sở dữ liệu", "06. Thiết kế Cơ sở dữ liệu quan hệ chuẩn 3NF & Toàn vẹn ACID", "Cấu trúc 11 bảng thực thể cốt lõi trên SQL Server đảm bảo tính nhất quán tuyệt đối")
    create_footer(s7, 7)
    
    # Left Card: 11 Thực thể chuẩn 3NF
    c7_l = add_card(s7, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_TEAL)
    tb7_l = s7.shapes.add_textbox(Inches(1.05), Inches(1.9), Inches(5.15), Inches(4.75))
    tf7_l = tb7_l.text_frame
    tf7_l.word_wrap = True
    tf7_l.margin_left = tf7_l.margin_top = tf7_l.margin_right = tf7_l.margin_bottom = 0
    
    p = tf7_l.paragraphs[0]
    r = p.add_run()
    r.text = "Danh Mục 11 Bảng Thực Thể Chuẩn Hóa 3NF"
    r.font.name = FONT_TITLE
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_TEAL
    
    table_groups = [
        ("1. Nhóm Quản Trị & Nhân Sự:", [
            "`departments`: Danh mục phòng ban, mã code UNIQUE",
            "`employees`: Hồ sơ nhân sự, số CCCD UNIQUE, STK ngân hàng",
            "`user_accounts`: Tài khoản đăng nhập, password_hash (BCrypt), role",
            "`contracts`: Hợp đồng lao động, mức lương cơ bản (base_salary >= 0)",
            "`deduction_rates`: Tỷ lệ trích nộp bảo hiểm động (BHXH 8%, BHYT 1.5%...)"
        ]),
        ("2. Nhóm Chấm Công & Phép:", [
            "`work_shifts`: Danh mục ca làm việc và hệ số làm thêm",
            "`timesheets`: Nhật ký điểm danh Check-in/out, ngày công thực tế",
            "`leave_requests`: Đơn xin nghỉ phép, trạng thái PENDING/APPROVED"
        ]),
        ("3. Nhóm Kỳ Lương & Quyết Toán (Header - Line Pattern):", [
            "`payroll_periods`: Kỳ lương tháng, trạng thái DRAFT -> LOCKED -> PAID",
            "`payslips` (Header): Phiếu lương tổng hợp từng nhân viên",
            "`payslip_lines` (Line): Chi tiết từng dòng thu nhập và khấu trừ thuế/BH"
        ])
    ]
    for grp_title, grp_tables in table_groups:
        pg = tf7_l.add_paragraph()
        pg.space_before = Pt(6)
        rg = pg.add_run()
        rg.text = grp_title
        rg.font.bold = True
        rg.font.size = Pt(13.5)
        rg.font.color.rgb = COLOR_NAVY_DARK
        for tbl in grp_tables:
            pt = tf7_l.add_paragraph()
            pt.space_before = Pt(2)
            rt = pt.add_run()
            rt.text = "  • " + tbl
            rt.font.size = Pt(12.5)
            rt.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card: Ràng buộc toàn vẹn & ACID Transaction
    c7_r = add_card(s7, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_BLUE_ROYAL)
    tb7_r = s7.shapes.add_textbox(Inches(7.1), Inches(1.9), Inches(5.15), Inches(4.75))
    tf7_r = tb7_r.text_frame
    tf7_r.word_wrap = True
    tf7_r.margin_left = tf7_r.margin_top = tf7_r.margin_right = tf7_r.margin_bottom = 0
    
    p = tf7_r.paragraphs[0]
    r = p.add_run()
    r.text = "Ràng Buộc Toàn Vẹn Dữ Liệu & Giao Dịch ACID"
    r.font.name = FONT_TITLE
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_BLUE_ROYAL
    
    acid_points = [
        ("Mô hình Cặp Header - Line (`payslips` - `payslip_lines`):", 
         "Khi tính lương, thông tin tổng quát lưu tại `payslips`, các khoản chi tiết (lương ngày công, phụ cấp ăn trưa, trừ BHXH 8%, BHYT 1.5%, thuế TNCN) được lưu tại `payslip_lines` với khóa ngoại ON DELETE CASCADE."),
        ("Ràng buộc kiểm tra nghiêm ngặt (CHECK Constraints):", 
         "• `chk_contract_salary`: Lương cơ bản không âm: `CHECK (base_salary >= 0)`.\n• `chk_period_month`: Tháng kỳ lương hợp lệ: `CHECK (period_month BETWEEN 1 AND 12)`.\n• `chk_period_status`: Trạng thái: `CHECK (status IN ('DRAFT', 'CALCULATED', 'LOCKED', 'PAID'))`."),
        ("Toàn vẹn giao dịch Database Transaction (ACID):", 
         "Quá trình ghi nhận bảng lương cho toàn bộ công ty được bọc trọn vẹn trong một giao dịch `using var tx = conn.BeginTransaction()`. Nếu xảy ra bất kỳ lỗi tính toán nào, hệ thống lập tức Rollback 100%, bảo vệ tuyệt đối tính nhất quán kế toán."),
        ("Động cơ Tỷ lệ Bảo hiểm Động (Dynamic Rates):", 
         "Tỷ lệ trích nộp được nạp động từ bảng `deduction_rates` thay vì hardcode. Khi Nhà nước điều chỉnh mức đóng, quản trị viên chỉ cần cập nhật CSDL mà không cần sửa code.")
    ]
    for a_title, a_desc in acid_points:
        pa = tf7_r.add_paragraph()
        pa.space_before = Pt(6)
        ra1 = pa.add_run()
        ra1.text = "✓ " + a_title + "\n"
        ra1.font.bold = True
        ra1.font.size = Pt(13.5)
        ra1.font.color.rgb = COLOR_NAVY_DARK
        ra2 = pa.add_run()
        ra2.text = a_desc
        ra2.font.size = Pt(12.5)
        ra2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s7, 
        "Về thiết kế cơ sở dữ liệu quan hệ:\n"
        "Hệ thống bao gồm 11 bảng thực thể được chuẩn hóa đạt chuẩn 3NF. Đặc biệt, để xử lý bài toán kế toán, em sử dụng mô hình Header-Line giữa bảng payslips và payslip_lines.\n"
        "Toàn bộ quá trình tính lương được thực thi trong một Database Transaction duy nhất, bảo đảm tính toàn vẹn ACID: hoặc thành công 100%, hoặc Rollback toàn bộ nếu có lỗi.\n"
        "Các tỷ lệ trích nộp bảo hiểm 10.5% được lưu động trong bảng deduction_rates giúp hệ thống thích ứng ngay lập tức khi chính sách nhà nước thay đổi."
    )

    # -------------------------------------------------------------
    # SLIDE 8: THIẾT KẾ GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (UI/UX)
    # -------------------------------------------------------------
    s8 = prs.slides.add_slide(blank_layout)
    bg8 = s8.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg8, COLOR_BG_LIGHT)
    create_header(s8, "Thiết kế giao diện", "07. Trải nghiệm người dùng UI/UX & Responsive Mobile 390px", "Giao diện hiện đại, trực quan, tối ưu cho cả màn hình máy tính và thiết bị di động")
    create_footer(s8, 8)
    
    ui_cols = [
        ("DASHBOARD & ĐỐI SOÁT QUỸ LƯƠNG", COLOR_BLUE_ROYAL, [
            ("4 Thẻ Chỉ Số KPI Trực Quan:", " Hiển thị tổng nhân sự, tổng giờ công làm việc, tổng quỹ lương thực chi và trạng thái khóa sổ kỳ lương."),
            ("Bảng Tổng Hợp Đối Soát Đa Cột:", " Đối soát chi tiết từng nhân viên: Lương hợp đồng, Ngày công thực tế, Phụ cấp, Giảm trừ BHXH/BHYT/BHTN, Thuế TNCN và Thực lĩnh (Net)."),
            ("Thao Tác 1-Chạm Tiện Lợi:", " Tích hợp nút 'Chạy Tính Lương Tự Động', nút 'Khóa Sổ Kỳ Lương' và nút 'Xuất Báo Cáo Excel'.")
        ]),
        ("CỔNG TỰ PHỤC VỤ NHÂN VIÊN (ESS)", COLOR_CYAN, [
            ("Điểm Danh Check-in / Out 1 Chạm:", " Giao diện điểm danh nhanh chóng, tự động đối chiếu khung giờ ca làm việc (`work_shifts`)."),
            ("Tra Cứu Phiếu Lương An Toàn:", " Hiển thị chi tiết từng dòng lương cá nhân, bảo vệ chống xem lén tuyệt đối (Anti-IDOR)."),
            ("Nộp Đơn Xin Nghỉ Phép Trực Tuyến:", " Form nhập liệu có validation kiểm tra ngày bắt đầu/kết thúc, cảnh báo lỗi thời gian thực.")
        ]),
        ("TỐI ƯU MOBILE RESPONSIVE 390PX", COLOR_EMERALD, [
            ("Kiểm Thử Chuẩn Màn Hình 390px:", " Tương thích hoàn hảo trên các dòng điện thoại thông minh (iPhone 12/14/15/16)."),
            ("Menu Tự Động Thu Gọn:", " Thanh điều hướng co giãn thành nút Hamburger mượt mà khi màn hình hẹp."),
            ("Bảng Tính Vuốt Chạm Cảm Ứng:", " Bọc trong container `.table-responsive` hỗ trợ vuốt cuộn ngang mượt mà, không vỡ layout.")
        ])
    ]
    
    col_w8 = Inches(3.64)
    col_h8 = Inches(4.0)
    for idx, (u_title, u_col, u_items) in enumerate(ui_cols):
        ux = Inches(0.8 + idx * 4.04)
        uy = Inches(1.75)
        add_card(s8, ux, uy, col_w8, col_h8, u_col)
        
        tb = s8.shapes.add_textbox(ux + Inches(0.2), uy + Inches(0.18), col_w8 - Inches(0.4), col_h8 - Inches(0.3))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = u_title
        r.font.name = FONT_TITLE
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = u_col
        
        for item_h, item_d in u_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(7)
            ri1 = pi.add_run()
            ri1.text = "• " + item_h + "\n"
            ri1.font.bold = True
            ri1.font.size = Pt(14)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = "   " + item_d
            ri2.font.size = Pt(12.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom AI Floating Widget Highlight
    banner_ai = add_card(s8, Inches(0.8), Inches(5.95), Inches(11.733), Inches(0.9), bg_color=RGBColor(245, 243, 255), border_color=RGBColor(221, 214, 254))
    tb_ai = s8.shapes.add_textbox(Inches(1.05), Inches(6.02), Inches(11.2), Inches(0.75))
    tf_ai = tb_ai.text_frame
    tf_ai.word_wrap = True
    tf_ai.margin_left = tf_ai.margin_top = tf_ai.margin_right = tf_ai.margin_bottom = 0
    pai = tf_ai.paragraphs[0]
    rai1 = pai.add_run()
    rai1.text = "✦ WIDGET TRỢ LÝ LƯƠNG AI NỔI TOÀN HỆ THỐNG (`FloatingAiAssistant.razor`): "
    rai1.font.name = FONT_BODY
    rai1.font.size = Pt(14)
    rai1.font.bold = True
    rai1.font.color.rgb = COLOR_PURPLE
    rai2 = pai.add_run()
    rai2.text = "Nút tròn thông minh nổi cố định ở góc phải dưới màn hình trên mọi trang. Bấm vào mở khung chat kính mờ hiện đại, hỗ trợ nhân viên hỏi đáp lương tức thì và tự động co giãn kích thước `calc(100vw - 20px)` khi xem trên di động."
    rai2.font.name = FONT_BODY
    rai2.font.size = Pt(14)
    rai2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s8, 
        "Về thiết kế giao diện UI/UX:\n"
        "Giao diện được thiết kế theo phong cách hiện đại với 3 phân hệ chính:\n"
        "- Dashboard kế toán với bảng tổng hợp đối soát đa cột và các nút hành động 1-chạm.\n"
        "- Cổng tự phục vụ nhân viên cho phép điểm danh nhanh và tra cứu phiếu lương cá nhân an toàn.\n"
        "- Đạt chuẩn thiết kế đáp ứng Responsive Mobile (kiểm thử chuẩn 390px trên iPhone), bảng tính vuốt ngang mượt mà.\n"
        "Đặc biệt, hệ thống có Widget Trợ lý AI nổi toàn màn hình cho phép người dùng hỏi đáp ở bất kỳ đâu."
    )

    # -------------------------------------------------------------
    # SLIDE 9: LUỒNG CHỨC NĂNG & QUY TRÌNH NGHIỆP VỤ
    # -------------------------------------------------------------
    s9 = prs.slides.add_slide(blank_layout)
    bg9 = s9.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg9, COLOR_BG_LIGHT)
    create_header(s9, "Quy trình nghiệp vụ", "08. Luồng nghiệp vụ Chấm công, Tính lương & Khóa sổ kỳ lương", "Quy trình khép kín tự động hóa từ điểm danh hàng ngày đến chốt số liệu tài chính")
    create_footer(s9, 9)
    
    # 3 Flow Cards
    flows = [
        ("LUỒNG 1: CHẤM CÔNG & NGHỈ PHÉP", COLOR_CYAN, [
            ("Bước 1:", " Nhân viên điểm danh Check-in khi đến và Check-out khi về."),
            ("Bước 2:", " Hệ thống so khớp khung giờ với danh mục ca làm (`work_shifts`) để tính số giờ và xác định trạng thái (Đúng giờ, Đi muộn, Về sớm)."),
            ("Bước 3:", " Nhân viên nộp đơn xin nghỉ phép trực tuyến (`leave_requests`)."),
            ("Bước 4:", " Kế toán/Quản lý xét duyệt đơn phép (`APPROVED`/`REJECTED`)."),
            ("Bước 5:", " Tổng hợp ngày công thực tế và ngày nghỉ có phép cho kỳ lương.")
        ]),
        ("LUỒNG 2: ĐỘNG CƠ TÍNH LƯƠNG 1-CHẠM", COLOR_EMERALD, [
            ("Bước 1:", " Kế toán chọn kỳ lương cần tính toán (tháng/năm)."),
            ("Bước 2:", " Nạp danh sách nhân viên, hợp đồng và mức lương cơ bản."),
            ("Bước 3:", " Nạp tổng số ngày công và ngày phép từ bảng `timesheets`."),
            ("Bước 4:", " Nạp tỷ lệ trích nộp bảo hiểm động từ bảng `deduction_rates`."),
            ("Bước 5:", " Thực thi động cơ `SalaryFormulaEngine` tính Gross, BH 10.5%, thuế TNCN lũy tiến và Net Salary trong 1 Transaction.")
        ]),
        ("LUỒNG 3: KHÓA SỔ KỲ LƯƠNG READ-ONLY", COLOR_PURPLE, [
            ("Trạng thái 1: DRAFT", " Kỳ lương nháp, đang thu thập ngày công."),
            ("Trạng thái 2: CALCULATED", " Đã tính toán xong, kế toán đối soát quỹ lương."),
            ("Trạng thái 3: LOCKED", " Kế toán bấm 'Khóa Sổ' -> Kỳ lương chuyển sang trạng thái Read-Only bất biến, tuyệt đối không được phép tính lại."),
            ("Trạng thái 4: PAID", " Xuất ủy nhiệm chi ngân hàng và hoàn tất chi trả."),
            ("Bảo mật tài chính:", " Ngăn chặn mọi hành vi gian lận hoặc can thiệp số liệu sau khi đã chốt sổ.")
        ])
    ]
    
    col_w9 = Inches(3.64)
    col_h9 = Inches(4.1)
    for idx, (fl_title, fl_col, fl_steps) in enumerate(flows):
        fx = Inches(0.8 + idx * 4.04)
        fy = Inches(1.75)
        add_card(s9, fx, fy, col_w9, col_h9, fl_col)
        
        tb = s9.shapes.add_textbox(fx + Inches(0.2), fy + Inches(0.18), col_w9 - Inches(0.4), col_h9 - Inches(0.3))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = fl_title
        r.font.name = FONT_TITLE
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = fl_col
        
        for st_head, st_desc in fl_steps:
            pi = tf.add_paragraph()
            pi.space_before = Pt(6)
            ri1 = pi.add_run()
            ri1.text = st_head + " "
            ri1.font.bold = True
            ri1.font.size = Pt(13.5)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = st_desc
            ri2.font.size = Pt(12.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom State Machine Image embedding
    state_img = "uml/images/12_state_payroll_period.png"
    if os.path.exists(state_img):
        # Add a framed container for the diagram
        add_card(s9, Inches(0.8), Inches(6.0), Inches(11.733), Inches(0.95), bg_color=COLOR_WHITE)
        tb_sm = s9.shapes.add_textbox(Inches(1.0), Inches(6.05), Inches(3.2), Inches(0.8))
        tf_sm = tb_sm.text_frame
        tf_sm.word_wrap = True
        tf_sm.margin_left = tf_sm.margin_top = tf_sm.margin_right = tf_sm.margin_bottom = 0
        psm = tf_sm.paragraphs[0]
        rsm = psm.add_run()
        rsm.text = "SƠ ĐỒ TRẠNG THÁI KỲ LƯƠNG:\nDRAFT → CALCULATED → LOCKED (Read-Only) → PAID"
        rsm.font.name = FONT_BODY
        rsm.font.size = Pt(13.5)
        rsm.font.bold = True
        rsm.font.color.rgb = COLOR_BLUE_ROYAL
        
        s9.shapes.add_picture(state_img, Inches(4.3), Inches(6.05), height=Inches(0.85))

    add_speaker_note(s9, 
        "Về quy trình nghiệp vụ:\n"
        "Hệ thống khép kín qua 3 luồng xử lý chính:\n"
        "Luồng 1: Chấm công và quản lý đơn nghỉ phép trực tuyến, tự động tính công thực tế.\n"
        "Luồng 2: Động cơ tính lương 1-chạm nạp đồng thời nhân viên, công thực tế và tỷ lệ trích nộp bảo hiểm động để xác định số tiền thực lĩnh.\n"
        "Luồng 3: Quản lý vòng đời kỳ lương theo máy trạng thái (State Machine): từ DRAFT sang CALCULATED, và quan trọng nhất là trạng thái LOCKED (Khóa sổ) biến kỳ lương thành Read-Only, không ai được phép sửa đổi số liệu kế toán đã chốt."
    )

    # -------------------------------------------------------------
    # SLIDE 10: MINH HỌA LẬP TRÌNH THEN CHỐT (CODE HIGHLIGHTS)
    # -------------------------------------------------------------
    s10 = prs.slides.add_slide(blank_layout)
    bg10 = s10.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg10, COLOR_BG_LIGHT)
    create_header(s10, "Minh họa lập trình", "09. Đoạn mã nguồn tiêu biểu & Kỹ thuật đảm bảo toàn vẹn", "Triển khai Database Transaction ACID và Động cơ tính lương theo luật")
    create_footer(s10, 10)
    
    # Left Code Box: ACID Transaction
    c10_l = add_card(s10, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_BLUE_ROYAL, bg_color=COLOR_CODE_BG)
    tb10_l = s10.shapes.add_textbox(Inches(1.0), Inches(1.9), Inches(5.25), Inches(4.75))
    tf10_l = tb10_l.text_frame
    tf10_l.word_wrap = True
    tf10_l.margin_left = tf10_l.margin_top = tf10_l.margin_right = tf10_l.margin_bottom = 0
    
    p = tf10_l.paragraphs[0]
    r = p.add_run()
    r.text = "1. Quản Trị Giao Dịch ACID (SqlRepositories.cs)"
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_CYAN
    
    code1 = """// Mở giao dịch CSDL an toàn ACID
using var tx = conn.BeginTransaction();
try {
    foreach (var slip in payslips) {
        // 1. Lưu Header phiếu lương & lấy ID
        var slipId = await conn.ExecuteScalarAsync<long>(
            sqlHeader, slip, tx);
        
        // 2. Lưu toàn bộ các dòng chi tiết Line
        foreach (var line in slip.Lines) {
            line.PayslipId = slipId;
            await conn.ExecuteAsync(sqlLine, line, tx);
        }
    }
    tx.Commit(); // Hoàn tất an toàn
} catch {
    tx.Rollback(); // Hoàn nguyên 100% nếu lỗi
    throw;
}"""
    p_code1 = tf10_l.add_paragraph()
    p_code1.space_before = Pt(6)
    r_code1 = p_code1.add_run()
    r_code1.text = code1
    r_code1.font.name = FONT_CODE
    r_code1.font.size = Pt(12.5)
    r_code1.font.color.rgb = RGBColor(226, 232, 240)
    
    p_note1 = tf10_l.add_paragraph()
    p_note1.space_before = Pt(6)
    r_note1 = p_note1.add_run()
    r_note1.text = "★ Bảo đảm: Nếu mất điện hoặc lỗi dữ liệu giữa chừng, không bao giờ xảy ra tình trạng lưu dở dang Header mà thiếu Line."
    r_note1.font.name = FONT_BODY
    r_note1.font.size = Pt(12.5)
    r_note1.font.color.rgb = COLOR_AMBER

    # Right Code Box: Động cơ tính lương & Phòng vệ URL
    c10_r = add_card(s10, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_EMERALD, bg_color=COLOR_CODE_BG)
    tb10_r = s10.shapes.add_textbox(Inches(7.05), Inches(1.9), Inches(5.25), Inches(4.75))
    tf10_r = tb10_r.text_frame
    tf10_r.word_wrap = True
    tf10_r.margin_left = tf10_r.margin_top = tf10_r.margin_right = tf10_r.margin_bottom = 0
    
    p = tf10_r.paragraphs[0]
    r = p.add_run()
    r.text = "2. Động Cơ Tính Lương & Phòng Vệ URL 403"
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_EMERALD
    
    code2 = """// Động cơ tính lương theo luật (SalaryFormulaEngine.cs)
decimal dayRate = contract.BaseSalary / standardDays;
decimal proratedBase = dayRate * actualWorkingDays;

// Khấu trừ BHXH 8%, BHYT 1.5%, BHTN 1% từ CSDL động
decimal bhxh = contract.BaseSalary * rates["BHXH_EMP"];
decimal totalDeductions = bhxh + bhyt + bhtn;

// Thuế TNCN lũy tiến 7 bậc sau giảm trừ gia cảnh 11tr
decimal pit = CalculatePersonalIncomeTax(taxableIncome);
decimal netSalary = grossSalary - totalDeductions - pit;

// Phòng vệ URL Router (Routes.razor)
<AuthorizeRouteView RouteData="@routeData">
    <NotAuthorized>
        <AccessDenied403 /> <!-- Chặn người thường -->
    </NotAuthorized>
</AuthorizeRouteView>"""
    p_code2 = tf10_r.add_paragraph()
    p_code2.space_before = Pt(6)
    r_code2 = p_code2.add_run()
    r_code2.text = code2
    r_code2.font.name = FONT_CODE
    r_code2.font.size = Pt(12.5)
    r_code2.font.color.rgb = RGBColor(226, 232, 240)
    
    p_note2 = tf10_r.add_paragraph()
    p_note2.space_before = Pt(6)
    r_note2 = p_note2.add_run()
    r_note2.text = "★ Bảo vệ đa tầng: Tỷ lệ bảo hiểm nạp động từ CSDL; Router chặn ngay lập tức nếu người dùng cố gõ URL quản trị trên thanh địa chỉ."
    r_note2.font.name = FONT_BODY
    r_note2.font.size = Pt(12.5)
    r_note2.font.color.rgb = COLOR_CYAN

    add_speaker_note(s10, 
        "Về minh họa mã nguồn lập trình:\n"
        "Em xin làm nổi bật 2 đoạn code trọng yếu nhất của dự án:\n"
        "1. Quản trị giao dịch Database Transaction trong SqlRepositories.cs: Toàn bộ quá trình ghi nhận phiếu lương Header và các dòng chi tiết Line đều được gắn chặt với biến tx. Nếu có lỗi, lệnh tx.Rollback() sẽ phục hồi nguyên trạng cơ sở dữ liệu.\n"
        "2. Động cơ tính lương SalaryFormulaEngine.cs: Áp dụng công thức lương ngày công theo tỷ lệ, khấu trừ bảo hiểm 10.5% nạp động từ bảng deduction_rates và tính thuế TNCN theo biểu thuế lũy tiến từng phần.\n"
        "Đồng thời, tại Routes.razor, em sử dụng AuthorizeRouteView để chặn đứng nguy cơ Bypass URL."
    )

    # -------------------------------------------------------------
    # SLIDE 11: KẾT QUẢ THỰC HIỆN & TÍNH NĂNG NỔI BẬT
    # -------------------------------------------------------------
    s11 = prs.slides.add_slide(blank_layout)
    bg11 = s11.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg11, COLOR_BG_LIGHT)
    create_header(s11, "Kết quả thực hiện", "10. Hệ thống vận hành thực tế & Các tính năng đột phá", "Sản phẩm phần mềm hoàn chỉnh với các năng lực tự động hóa và tích hợp AI")
    create_footer(s11, 11)
    
    results = [
        ("TỰ ĐỘNG HÓA 1-CHẠM (1-CLICK RUN)", COLOR_EMERALD, [
            ("Tốc Độ Tính Toán Vượt Trội:", " Xử lý toàn bộ bảng lương công ty 20-50 nhân sự chỉ mất dưới 1.5 giây."),
            ("Độ Chính Xác Tuyệt Đối:", " Loại bỏ 100% rủi ro kéo nhầm dòng, lệch ô công thức thường gặp trên Excel."),
            ("Bảo Toàn Số Liệu Read-Only:", " Cơ chế khóa sổ kỳ lương (`LOCKED`) ngăn chặn tuyệt đối can thiệp sửa đổi trái phép sau khi duyệt.")
        ]),
        ("TRỢ LÝ LƯƠNG AI THÔNG MINH", COLOR_PURPLE, [
            ("Tích Hợp Google Gemini API:", " Giải đáp ngôn ngữ tự nhiên câu hỏi của nhân viên: 'Tại sao lương tháng này bị trừ?'."),
            ("Mô Phỏng Thu Nhập What-If:", " Cho phép nhân viên giả định: 'Nếu tháng này làm thêm 10 giờ OT thì thực lĩnh tăng bao nhiêu?'."),
            ("Local Fallback Engine:", " Động cơ quy tắc nội bộ tự động kích hoạt khi mất kết nối mạng, đảm bảo không bị gián đoạn.")
        ]),
        ("XUẤT BÁO CÁO TÀI CHÍNH CHUẨN MỰC", COLOR_BLUE_ROYAL, [
            ("Excel Mẫu Số 02-LĐTL:", " Xuất bảng thanh toán tiền lương chuẩn theo Thông tư của Bộ Tài chính đầy đủ cột mục kế toán."),
            ("Định Dạng Tiền Tệ Kế Toán:", " Sử dụng định dạng tiền tệ chuyên nghiệp `mso-number-format: '#,##0' VNĐ`."),
            ("Tệp CSV Chuyển Khoản Ngân Hàng:", " Xuất file ủy nhiệm chi gửi trực tiếp ngân hàng (Vietcombank, MB Bank) để chi trả tự động.")
        ])
    ]
    
    col_w11 = Inches(3.64)
    col_h11 = Inches(4.0)
    for idx, (res_title, res_col, res_items) in enumerate(results):
        rx = Inches(0.8 + idx * 4.04)
        ry = Inches(1.75)
        add_card(s11, rx, ry, col_w11, col_h11, res_col)
        
        tb = s11.shapes.add_textbox(rx + Inches(0.2), ry + Inches(0.18), col_w11 - Inches(0.4), col_h11 - Inches(0.3))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        
        p = tf.paragraphs[0]
        r = p.add_run()
        r.text = res_title
        r.font.name = FONT_TITLE
        r.font.size = Pt(15)
        r.font.bold = True
        r.font.color.rgb = res_col
        
        for item_h, item_d in res_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(7)
            ri1 = pi.add_run()
            ri1.text = "✓ " + item_h + "\n"
            ri1.font.bold = True
            ri1.font.size = Pt(14)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = "   " + item_d
            ri2.font.size = Pt(12.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom Implementation summary card
    banner_res = add_card(s11, Inches(0.8), Inches(5.95), Inches(11.733), Inches(0.9), bg_color=RGBColor(240, 253, 244), border_color=RGBColor(187, 247, 208))
    tb_bres = s11.shapes.add_textbox(Inches(1.05), Inches(6.02), Inches(11.2), Inches(0.75))
    tf_bres = tb_bres.text_frame
    tf_bres.word_wrap = True
    tf_bres.margin_left = tf_bres.margin_top = tf_bres.margin_right = tf_bres.margin_bottom = 0
    pbr = tf_bres.paragraphs[0]
    rbr1 = pbr.add_run()
    rbr1.text = "★ MINH CHỨNG VẬN HÀNH THỰC TẾ: "
    rbr1.font.name = FONT_BODY
    rbr1.font.size = Pt(14)
    rbr1.font.bold = True
    rbr1.font.color.rgb = COLOR_EMERALD
    rbr2 = pbr.add_run()
    rbr2.text = "Ứng dụng đã được kiểm thử chạy thực tế trên môi trường .NET 10 Blazor Server và SQL Server 2022. Toàn bộ 5 phân hệ từ Đăng nhập, Điểm danh, Duyệt phép, Tính lương đến Xuất file Excel đều hoạt động trơn tru với độ phản hồi dưới 1.2s."
    rbr2.font.name = FONT_BODY
    rbr2.font.size = Pt(14)
    rbr2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s11, 
        "Về kết quả thực hiện đồ án:\n"
        "Hệ thống đã được hiện thực hóa 100% với 3 điểm sáng công nghệ nổi bật:\n"
        "Thứ nhất: Tính lương 1-chạm tự động xử lý toàn bộ bảng lương trong 1.5 giây, số liệu khớp hoàn toàn với các quy định pháp luật.\n"
        "Thứ hai: Trợ lý Lương AI thông minh sử dụng Google Gemini API giải đáp tự nhiên thắc mắc thu nhập và hỗ trợ mô phỏng What-If.\n"
        "Thứ ba: Tính năng xuất báo cáo Excel chuẩn Mẫu 02-LĐTL của Bộ Tài chính và xuất tệp CSV chuyển khoản ngân hàng trực tiếp."
    )

    # -------------------------------------------------------------
    # SLIDE 12: KIỂM THỬ HỆ THỐNG & BẢO MẬT
    # -------------------------------------------------------------
    s12 = prs.slides.add_slide(blank_layout)
    bg12 = s12.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg12, COLOR_BG_LIGHT)
    create_header(s12, "Kiểm thử hệ thống", "11. Kịch bản kiểm thử then chốt & Đánh giá an toàn bảo mật", "Thực thi 10 Test Cases (TC01 – TC10) bao phủ chức năng, toàn vẹn dữ liệu và bảo mật")
    create_footer(s12, 12)
    
    # Table of Test Cases
    rows = 8
    cols = 5
    table_shape = s12.shapes.add_table(rows, cols, Inches(0.8), Inches(1.75), Inches(11.733), Inches(4.1))
    table = table_shape.table
    table.columns[0].width = Inches(1.1)   # Mã TC
    table.columns[1].width = Inches(3.2)   # Chức năng kiểm thử
    table.columns[2].width = Inches(3.5)   # Kết quả kỳ vọng
    table.columns[3].width = Inches(2.733) # Kết quả thực tế
    table.columns[4].width = Inches(1.2)   # Đánh giá
    
    headers = ["Mã TC", "Chức Năng Kiểm Thử", "Kết Quả Kỳ Vọng", "Kết Quả Thực Tế", "Đánh Giá"]
    for c_idx, h_text in enumerate(headers):
        cell = table.cell(0, c_idx)
        cell.fill.solid()
        cell.fill.fore_color.rgb = COLOR_NAVY_DARK
        p = cell.text_frame.paragraphs[0]
        p.alignment = PP_ALIGN.CENTER
        r = p.add_run()
        r.text = h_text
        r.font.name = FONT_BODY
        r.font.size = Pt(14)
        r.font.bold = True
        r.font.color.rgb = COLOR_WHITE
        
    test_cases = [
        ("TC01", "Đăng nhập xác thực BCrypt", "Đăng nhập thành công, cấp Claims ROLE_ADMIN", "Đăng nhập mượt mà, chuyển trang đúng vai trò", "ĐẠT (100%)"),
        ("TC03", "Ràng buộc kiểm tra Lương âm", "CSDL chk_contract_salary từ chối base_salary < 0", "CSDL từ chối, ứng dụng báo alert đỏ thân thiện", "ĐẠT (100%)"),
        ("TC05", "Tính lương 1-chạm trong Transaction", "Mở Transaction, lưu đồng thời payslips & lines", "Ghi nhận CSDL an toàn, trạng thái CALCULATED", "ĐẠT (100%)"),
        ("TC06", "Demo Khấu trừ Bảo hiểm 10.5%", "Lương 10tr -> Trừ BHXH 800k, BHYT 150k, BHTN 100k", "Số liệu tính toán chuẩn xác 100% khớp bảng rates", "ĐẠT (100%)"),
        ("TC07", "Demo Khóa sổ kỳ lương Read-Only", "Chặn tính lại khi kỳ lương đã ở trạng thái LOCKED", "Ứng dụng chặn, báo lỗi thân thiện không văng app", "ĐẠT (100%)"),
        ("TC08", "Chặn vượt quyền URL trực tiếp", "Nhân viên thường gõ URL quản trị /admin/...", "Router AuthorizeRouteView chặn, hiện 403 Forbidden", "ĐẠT (100%)"),
        ("TC10", "Tương thích Responsive Mobile 390px", "Menu thu gọn hamburger, bảng cuộn mượt mà", "Giao diện hiển thị chuẩn responsive trên di động", "ĐẠT (100%)")
    ]
    for r_idx, tc_data in enumerate(test_cases, start=1):
        bg_row = RGBColor(255, 255, 255) if r_idx % 2 == 1 else RGBColor(241, 245, 249)
        for c_idx, val in enumerate(tc_data):
            cell = table.cell(r_idx, c_idx)
            cell.fill.solid()
            cell.fill.fore_color.rgb = bg_row
            p = cell.text_frame.paragraphs[0]
            if c_idx in [0, 4]:
                p.alignment = PP_ALIGN.CENTER
            r = p.add_run()
            r.text = val
            r.font.name = FONT_BODY
            r.font.size = Pt(12.5)
            if c_idx == 4:
                r.font.bold = True
                r.font.color.rgb = COLOR_EMERALD
            elif c_idx == 0:
                r.font.bold = True
                r.font.color.rgb = COLOR_BLUE_ROYAL
            else:
                r.font.color.rgb = COLOR_TEXT_MAIN

    # Bottom Summary Badge
    b_test = add_card(s12, Inches(0.8), Inches(6.0), Inches(11.733), Inches(0.85), bg_color=RGBColor(236, 253, 245), border_color=RGBColor(167, 243, 208))
    tb_bt = s12.shapes.add_textbox(Inches(1.05), Inches(6.07), Inches(11.2), Inches(0.7))
    tf_bt = tb_bt.text_frame
    tf_bt.word_wrap = True
    tf_bt.margin_left = tf_bt.margin_top = tf_bt.margin_right = tf_bt.margin_bottom = 0
    pbt = tf_bt.paragraphs[0]
    rbt1 = pbt.add_run()
    rbt1.text = "★ TỔNG KẾT KẾT QUẢ KIỂM THỬ: "
    rbt1.font.name = FONT_BODY
    rbt1.font.size = Pt(14)
    rbt1.font.bold = True
    rbt1.font.color.rgb = COLOR_EMERALD
    rbt2 = pbt.add_run()
    rbt2.text = "10/10 Kịch bản kiểm thử (TC01 – TC10) đều đạt kết quả kỳ vọng 100%. Hệ thống chứng minh khả năng phòng vệ đa tầng: triệt tiêu SQL Injection bằng Dapper, chống Bypass URL bằng HTTP 403 Forbidden và ngăn ngừa lỗ hổng IDOR khi tra cứu phiếu lương."
    rbt2.font.name = FONT_BODY
    rbt2.font.size = Pt(13.5)
    rbt2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s12, 
        "Về công tác kiểm thử hệ thống:\n"
        "Em đã thiết lập 10 kịch bản kiểm thử bao quát từ giao diện đến nghiệp vụ lõi và an toàn bảo mật. Bảng trên slide tổng hợp 7 Test Cases tiêu biểu nhất:\n"
        "- TC01 kiểm thử đăng nhập BCrypt thành công.\n"
        "- TC03 kiểm thử ràng buộc CHECK lương âm tại CSDL.\n"
        "- TC05 và TC06 kiểm thử tính lương tự động trong Transaction và khấu trừ bảo hiểm 10.5% chuẩn xác.\n"
        "- TC07 kiểm thử tính bất biến của kỳ lương đã LOCKED.\n"
        "- TC08 kiểm thử bảo mật chặn vượt quyền URL 403 Forbidden.\n"
        "- TC10 kiểm thử giao diện co giãn Responsive 390px trên điện thoại.\n"
        "Tất cả 10 Test Cases đều ĐẠT 100%."
    )

    # -------------------------------------------------------------
    # SLIDE 13: ĐÁNH GIÁ HỆ THỐNG (ƯU ĐIỂM & HẠN CHẾ)
    # -------------------------------------------------------------
    s13 = prs.slides.add_slide(blank_layout)
    bg13 = s13.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg13, COLOR_BG_LIGHT)
    create_header(s13, "Đánh giá hệ thống", "12. Đánh giá ưu điểm vượt trội & Các hạn chế cần hoàn thiện", "Cái nhìn khách quan về kết quả đạt được và các thách thức trong triển khai thực tế")
    create_footer(s13, 13)
    
    # Left Card: Ưu điểm vượt trội
    c13_l = add_card(s13, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_EMERALD)
    tb13_l = s13.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.15), Inches(4.65))
    tf13_l = tb13_l.text_frame
    tf13_l.word_wrap = True
    tf13_l.margin_left = tf13_l.margin_top = tf13_l.margin_right = tf13_l.margin_bottom = 0
    
    p = tf13_l.paragraphs[0]
    r = p.add_run()
    r.text = "Ưu Điểm Vượt Trội Của Hệ Thống (4 Điểm Sáng)"
    r.font.name = FONT_TITLE
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_EMERALD
    
    pros = [
        ("Kiến trúc phần mềm chuẩn mực:", " Áp dụng Clean Architecture 4 tầng phân tách độc lập, tuân thủ Dependency Inversion. Tầng Core Business không phụ thuộc framework ngoài, cực kỳ dễ bảo trì và mở rộng."),
        ("Tự động hóa toàn diện 100% theo Luật:", " Rút ngắn thời gian tính lương từ 2-3 ngày xuống còn 1.5 giây. Tỷ lệ trích nộp bảo hiểm động và thuế TNCN lũy tiến đảm bảo tính chuẩn xác tuyệt đối."),
        ("Bảo mật đa tầng & Quyền riêng tư:", " Mã hóa BCrypt, phân quyền RBAC, Router chặn truy cập trái phép bằng HTTP 403 Forbidden và phòng vệ chống lộ dữ liệu IDOR."),
        ("Trải nghiệm người dùng hiện đại:", " Tích hợp Trợ lý Lương AI hỏi đáp tức thì và giao diện tương thích đa thiết bị Responsive Mobile 390px.")
    ]
    for p_title, p_desc in pros:
        pb = tf13_l.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run()
        rb1.text = "✓ " + p_title
        rb1.font.bold = True
        rb1.font.size = Pt(14)
        rb1.font.color.rgb = COLOR_EMERALD
        rb2 = pb.add_run()
        rb2.text = p_desc
        rb2.font.size = Pt(13.5)
        rb2.font.color.rgb = COLOR_TEXT_MAIN

    # Right Card: Hạn chế & Thách thức
    c13_r = add_card(s13, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_AMBER)
    tb13_r = s13.shapes.add_textbox(Inches(7.1), Inches(1.95), Inches(5.15), Inches(4.65))
    tf13_r = tb13_r.text_frame
    tf13_r.word_wrap = True
    tf13_r.margin_left = tf13_r.margin_top = tf13_r.margin_right = tf13_r.margin_bottom = 0
    
    p = tf13_r.paragraphs[0]
    r = p.add_run()
    r.text = "Hạn Chế Hiện Tại & Vấn Đề Cần Hoàn Thiện"
    r.font.name = FONT_TITLE
    r.font.size = Pt(17.5)
    r.font.bold = True
    r.font.color.rgb = COLOR_AMBER
    
    cons = [
        ("Chưa kết nối trực tiếp phần cứng máy chấm công:", " Hiện tại việc điểm danh đang thực hiện qua giao diện web hoặc nạp dữ liệu. Chưa tích hợp SDK kết nối trực tiếp với máy chấm công vân tay, khuôn mặt (FaceID) tại văn phòng."),
        ("Chưa liên kết cổng thanh toán ngân hàng trực tuyến:", " Hệ thống mới dừng lại ở bước xuất file CSV chuẩn ngân hàng để kế toán tải lên Internet Banking, chưa gọi trực tiếp Open Banking API để tự động chuyển khoản tiền lương."),
        ("Phụ thuộc đường truyền Internet khi gọi Gemini API:", " Mặc dù đã có bộ Fallback Engine quy tắc nội bộ, nhưng tính năng giải thích ngôn ngữ tự nhiên nâng cao vẫn cần kết nối Internet ổn định."),
        ("Chưa hỗ trợ đa ngôn ngữ:", " Hệ thống hiện chỉ hỗ trợ giao diện tiếng Việt, cần bổ sung tiếng Anh nếu triển khai cho doanh nghiệp FDI.")
    ]
    for c_title, c_desc in cons:
        pc = tf13_r.add_paragraph()
        pc.space_before = Pt(8)
        rc1 = pc.add_run()
        rc1.text = "⚠ " + c_title
        rc1.font.bold = True
        rc1.font.size = Pt(14)
        rc1.font.color.rgb = COLOR_AMBER
        rc2 = pc.add_run()
        rc2.text = c_desc
        rc2.font.size = Pt(13.5)
        rc2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s13, 
        "Về đánh giá hệ thống một cách khách quan:\n"
        "Về ưu điểm: Đề tài đã hoàn thành xuất sắc mục tiêu học thuật và thực tiễn: kiến trúc Clean Architecture chuẩn mực, tự động hóa tính lương chuẩn xác tuyệt đối theo Luật Lao Động, bảo mật đa tầng và tích hợp Trợ lý AI hiện đại.\n"
        "Về hạn chế: Hệ thống hiện chưa kết nối trực tiếp với phần cứng máy chấm công vân tay/FaceID, và chưa tích hợp cổng Open Banking API để chi trả lương tự động qua tài khoản ngân hàng. Đây là những điểm em đã ghi nhận và lên kế hoạch hoàn thiện."
    )

    # -------------------------------------------------------------
    # SLIDE 14: ĐỊNH HƯỚNG PHÁT TRIỂN TƯƠNG LAI
    # -------------------------------------------------------------
    s14 = prs.slides.add_slide(blank_layout)
    bg14 = s14.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg14, COLOR_BG_LIGHT)
    create_header(s14, "Hướng phát triển", "13. Định hướng nghiên cứu & Nâng cấp hệ thống trong tương lai", "Kế hoạch mở rộng khả năng kết nối thiết bị IoT, Fintech và Trí tuệ Nhân tạo")
    create_footer(s14, 14)
    
    future_plans = [
        ("01. KẾT NỐI MÁY CHẤM CÔNG PHẦN CỨNG (IOT)", COLOR_CYAN, [
            ("Tích hợp SDK Thiết Bị:", " Kết nối trực tiếp máy chấm công vân tay, nhận diện khuôn mặt (FaceID) và thẻ từ thông minh (ZKTeco, Ronald Jack)."),
            ("Đồng Bộ Thời Gian Thực:", " Sử dụng Webhook hoặc Background Worker tự động đồng bộ dữ liệu điểm danh về CSDL ngay khi nhân viên bấm vân tay tại cửa văn phòng.")
        ]),
        ("02. TÍCH HỢP CỔNG OPEN BANKING API", COLOR_EMERALD, [
            ("Chi Trả Lương Tự Động 1-Chạm:", " Kết nối API ngân hàng điện tử (Vietcombank, MB Bank, Techcombank) qua kênh bảo mật OAuth2 & mTLS."),
            ("Tự Động Chuyển Khoản Hàng Loạt:", " Ngay khi kỳ lương chuyển trạng thái `PAID`, hệ thống tự động phát lệnh chuyển khoản tiền lương vào STK từng nhân viên mà không cần tải file thủ công.")
        ]),
        ("03. PHÁT TRIỂN MOBILE APP / PWA VỚI ĐỊNH VỊ GPS", COLOR_PURPLE, [
            ("Ứng Dụng Di Động Chuyên Nghiệp:", " Xây dựng ứng dụng di động đa nền tảng (Flutter / Blazor Hybrid) cho hệ điều hành iOS và Android."),
            ("Chấm Công GPS Geofencing:", " Hỗ trợ nhân viên thị trường, nhân viên làm việc từ xa (Work From Home) điểm danh bằng định vị GPS và chụp ảnh khuôn mặt."),
            ("Thông Báo Đẩy (Push Notifications):", " Báo tin tức thì về điện thoại ngay khi phiếu lương tháng mới được phê duyệt.")
        ]),
        ("04. DỰ BÁO QUỸ LƯƠNG BẰNG MACHINE LEARNING", COLOR_BLUE_ROYAL, [
            ("Phân Tích Xu Hướng Nhân Sự:", " Ứng dụng mô hình học máy phân tích biến động chi phí làm thêm giờ (OT), tỷ lệ nghỉ việc và nhu cầu tuyển dụng."),
            ("Tối Ưu Hóa Ngân Sách:", " Đưa ra các khuyến nghị phân bổ ngân sách tiền lương và khen thưởng tối ưu cho ban giám đốc doanh nghiệp.")
        ])
    ]
    
    card_w14 = Inches(5.65)
    card_h14 = Inches(2.45)
    for idx, (f_title, f_col, f_items) in enumerate(future_plans):
        col = idx % 2
        row = idx // 2
        fx = Inches(0.8 + col * 6.05)
        fy = Inches(1.75 + row * 2.6)
        
        add_card(s14, fx, fy, card_w14, card_h14, f_col)
        tb_f = s14.shapes.add_textbox(fx + Inches(0.25), fy + Inches(0.18), card_w14 - Inches(0.5), card_h14 - Inches(0.3))
        tf_f = tb_f.text_frame
        tf_f.word_wrap = True
        tf_f.margin_left = tf_f.margin_top = tf_f.margin_right = tf_f.margin_bottom = 0
        
        pf = tf_f.paragraphs[0]
        rf = pf.add_run()
        rf.text = f_title
        rf.font.name = FONT_TITLE
        rf.font.size = Pt(16.5)
        rf.font.bold = True
        rf.font.color.rgb = f_col
        
        for item_h, item_d in f_items:
            pi = tf_f.add_paragraph()
            pi.space_before = Pt(5)
            ri1 = pi.add_run()
            ri1.text = "• " + item_h + "\n"
            ri1.font.bold = True
            ri1.font.size = Pt(14)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run()
            ri2.text = "   " + item_d
            ri2.font.size = Pt(13.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s14, 
        "Về định hướng phát triển tương lai:\n"
        "Em định hướng 4 mục tiêu nâng cấp trọng tâm cho hệ thống:\n"
        "1. Tích hợp SDK máy chấm công vân tay, khuôn mặt FaceID tại văn phòng.\n"
        "2. Liên kết Open Banking API chuyển khoản tiền lương tự động vào tài khoản ngân hàng nhân viên.\n"
        "3. Xây dựng ứng dụng di động chấm công định vị GPS Geofencing cho nhân viên làm việc linh hoạt.\n"
        "4. Ứng dụng Machine Learning phân tích xu hướng và dự báo quỹ lương doanh nghiệp."
    )

    # -------------------------------------------------------------
    # SLIDE 15: KẾT LUẬN & LỜI CẢM ƠN (CLOSING & Q&A - DARK THEME)
    # -------------------------------------------------------------
    s15 = prs.slides.add_slide(blank_layout)
    bg15 = s15.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg15, COLOR_NAVY_DARK)
    
    # Accent top stripe
    stripe15 = s15.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.12))
    set_shape_flat(stripe15, COLOR_EMERALD)
    
    # Header tag
    tag_close = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(0.55), Inches(4.5), Inches(0.35))
    set_shape_flat(tag_close, RGBColor(30, 41, 59), COLOR_EMERALD, 1.2)
    tf_tc = tag_close.text_frame
    tf_tc.margin_left = tf_tc.margin_top = tf_tc.margin_right = tf_tc.margin_bottom = 0
    ptc = tf_tc.paragraphs[0]
    ptc.alignment = PP_ALIGN.CENTER
    rtc = ptc.add_run()
    rtc.text = "KẾT LUẬN & LỜI CẢM ƠN — 14 / 15"
    rtc.font.name = FONT_BODY
    rtc.font.size = Pt(14)
    rtc.font.bold = True
    rtc.font.color.rgb = COLOR_EMERALD
    
    # Title
    tb_c_title = s15.shapes.add_textbox(Inches(0.8), Inches(1.05), Inches(11.733), Inches(0.8))
    tf_ct = tb_c_title.text_frame
    tf_ct.word_wrap = True
    tf_ct.margin_left = tf_ct.margin_top = tf_ct.margin_right = tf_ct.margin_bottom = 0
    pct = tf_ct.paragraphs[0]
    rct = pct.add_run()
    rct.text = "Tổng kết đồ án & Trân trọng cảm ơn Thầy Cô"
    rct.font.name = FONT_TITLE
    rct.font.size = Pt(32)
    rct.font.bold = True
    rct.font.color.rgb = COLOR_WHITE
    
    # Left Box: Tóm tắt thành quả
    c15_l = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(1.95), Inches(5.65), Inches(4.8))
    set_shape_flat(c15_l, RGBColor(30, 41, 59), RGBColor(51, 65, 85), 1.5)
    tb15_l = s15.shapes.add_textbox(Inches(1.05), Inches(2.15), Inches(5.15), Inches(4.4))
    tf15_l = tb15_l.text_frame
    tf15_l.word_wrap = True
    tf15_l.margin_left = tf15_l.margin_top = tf15_l.margin_right = tf15_l.margin_bottom = 0
    
    p = tf15_l.paragraphs[0]
    r = p.add_run()
    r.text = "Thành Quả Đạt Được Của Đồ Án"
    r.font.name = FONT_TITLE
    r.font.size = Pt(18)
    r.font.bold = True
    r.font.color.rgb = COLOR_CYAN
    
    achievements = [
        ("Hoàn thành xuất sắc mục tiêu đề tài:", " Xây dựng một ứng dụng web hoàn chỉnh, giải quyết triệt để các hạn chế của phương pháp tính lương bằng Excel."),
        ("Làm chủ kiến trúc Clean Architecture 4 tầng:", " Mã nguồn được tổ chức chuyên nghiệp, tầng Core Domain độc lập 0 dependency, dễ kiểm thử và bảo trì lâu dài."),
        ("Toàn vẹn cơ sở dữ liệu quan hệ 3NF:", " 11 bảng thực thể, giao dịch Database Transaction bảo toàn tính nguyên tử ACID, ràng buộc CHECK constraints chặt chẽ."),
        ("Bảo mật đa tầng theo tiêu chuẩn công nghiệp:", " BCrypt hashing, RBAC 3 vai trò, phòng vệ chống lỗ hổng IDOR và chặn vượt quyền URL bằng HTTP 403."),
        ("Tích hợp AI & Xuất báo cáo tài chính:", " Trợ lý Lương AI thông minh giải đáp thắc mắc và xuất file Excel Mẫu số 02-LĐTL của Bộ Tài chính.")
    ]
    for a_title, a_desc in achievements:
        pa = tf15_l.add_paragraph()
        pa.space_before = Pt(8)
        ra1 = pa.add_run()
        ra1.text = "✔ " + a_title + "\n"
        ra1.font.bold = True
        ra1.font.size = Pt(14)
        ra1.font.color.rgb = COLOR_WHITE
        ra2 = pa.add_run()
        ra2.text = a_desc
        ra2.font.size = Pt(13.5)
        ra2.font.color.rgb = RGBColor(148, 163, 184)

    # Right Box: Lời cảm ơn & Q&A
    c15_r = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(6.85), Inches(1.95), Inches(5.65), Inches(4.8))
    set_shape_flat(c15_r, RGBColor(30, 41, 59), RGBColor(51, 65, 85), 1.5)
    tb15_r = s15.shapes.add_textbox(Inches(7.1), Inches(2.15), Inches(5.15), Inches(4.4))
    tf15_r = tb15_r.text_frame
    tf15_r.word_wrap = True
    tf15_r.margin_left = tf15_r.margin_top = tf15_r.margin_right = tf15_r.margin_bottom = 0
    
    p = tf15_r.paragraphs[0]
    r = p.add_run()
    r.text = "Lời Tri Ân & Phiên Hỏi Đáp (Q&A)"
    r.font.name = FONT_TITLE
    r.font.size = Pt(18)
    r.font.bold = True
    r.font.color.rgb = COLOR_AMBER
    
    thanks_text = [
        ("Lời cảm ơn chân thành:", " Em xin bày tỏ lòng biết ơn sâu sắc tới Thầy TS. Hà Ngọc Long đã tận tâm hướng dẫn, định hướng kiến trúc sạch và phương pháp luận giải quyết bài toán nghiệp vụ trong suốt học phần."),
        ("Khoa Hệ thống Thông tin Kinh tế:", " Xin trân trọng cảm ơn quý Thầy, Cô giáo trong Khoa HTTT Kinh tế – Trường Đại học Kinh tế, Đại học Huế đã trang bị cho em nền tảng kiến thức vững chắc."),
        ("Sinh viên thực hiện:", " Trần Viết Mạnh Cường — MSV: 23K4080003\nLớp: K57 Tin học Kinh tế — Trường ĐH Kinh tế Huế\nKho lưu trữ: github.com/ManhCuong-Code/TimesheetPayroll")
    ]
    for t_head, t_body in thanks_text:
        pt = tf15_r.add_paragraph()
        pt.space_before = Pt(8)
        rt1 = pt.add_run()
        rt1.text = t_head + "\n"
        rt1.font.bold = True
        rt1.font.size = Pt(14)
        rt1.font.color.rgb = COLOR_WHITE
        rt2 = pt.add_run()
        rt2.text = t_body
        rt2.font.size = Pt(13.5)
        rt2.font.color.rgb = RGBColor(148, 163, 184)
        
    # Big Q&A Callout Badge
    badge_qa = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(7.35), Inches(5.35), Inches(4.65), Inches(1.1))
    set_shape_flat(badge_qa, COLOR_BLUE_ROYAL)
    tf_qa = badge_qa.text_frame
    tf_qa.word_wrap = True
    tf_qa.margin_left = tf_qa.margin_top = tf_qa.margin_right = tf_qa.margin_bottom = 0
    pqa1 = tf_qa.paragraphs[0]
    pqa1.alignment = PP_ALIGN.CENTER
    rqa1 = pqa1.add_run()
    rqa1.text = "XIN TRÂN TRỌNG CẢM ƠN!"
    rqa1.font.name = FONT_TITLE
    rqa1.font.size = Pt(20)
    rqa1.font.bold = True
    rqa1.font.color.rgb = COLOR_WHITE
    pqa2 = tf_qa.add_paragraph()
    pqa2.alignment = PP_ALIGN.CENTER
    rqa2 = pqa2.add_run()
    rqa2.text = "Kính mong nhận được ý kiến đóng góp từ Thầy Cô & Hội đồng\n— Q&A SESSION —"
    rqa2.font.name = FONT_BODY
    rqa2.font.size = Pt(13.5)
    rqa2.font.color.rgb = COLOR_CYAN

    add_speaker_note(s15, 
        "Kính thưa Thầy TS. Hà Ngọc Long cùng toàn thể quý Thầy Cô và các bạn sinh viên!\n"
        "Đồ án 'Xây dựng hệ thống ứng dụng Web Chấm công và Tính lương tự động' không chỉ giúp em hoàn thành xuất sắc mục tiêu học phần Lập trình ứng dụng Web, mà còn là cơ hội quý báu để em vận dụng thành thạo kiến trúc Clean Architecture, công nghệ .NET 10, CSDL SQL Server và bảo mật ứng dụng vào một bài toán thực tế doanh nghiệp.\n"
        "Em xin gửi lời cảm ơn chân thành và sâu sắc nhất tới Thầy TS. Hà Ngọc Long đã luôn tận tình chỉ dẫn và định hướng học thuật cho em trong suốt thời gian qua.\n"
        "Em xin trân trọng cảm ơn và kính mời Thầy cùng các bạn sinh viên đặt câu hỏi thảo luận!"
    )
    
    # -------------------------------------------------------------
        # -------------------------------------------------------------
        # -------------------------------------------------------------
    # APPLY 15 DYNAMIC, DIVERSE SLIDE TRANSITIONS (EACH SLIDE A UNIQUE EFFECT)
    # -------------------------------------------------------------
    slide_transitions = [
        '<p:fade/>',
        '<p:push dir="r"/>',
        '<p:split orient="horz" dir="out"/>',
        '<p:wipe dir="r"/>',
        '<p:comb orient="horz"/>',
        '<p:checker dir="horz"/>',
        '<p:circle/>',
        '<p:wheel spokes="4"/>',
        '<p:blinds dir="vert"/>',
        '<p:diamond/>',
        '<p:zoom dir="in"/>',
        '<p:plus/>',
        '<p:cover dir="u"/>',
        '<p:wedge/>',
        '<p:dissolve/>'
    ]
    for idx, slide in enumerate(prs.slides):
        add_transition_to_slide(slide, slide_transitions[idx])

    # SAVE PPTX FILE
    # -------------------------------------------------------------
    output_filename = "Bai_trinh_bay_Lap_trinh_ung_dung_Web.pptx"
    updated_filename = "Bai_trinh_bay_Lap_trinh_ung_dung_Web_CapNhat.pptx"
    try:
        prs.save(output_filename)
        print(f"Presentation saved successfully as: {output_filename}")
    except PermissionError:
        print(f"[NOTE] '{output_filename}' is currently OPEN in PowerPoint (file locked by Windows).")
        prs.save(updated_filename)
        print(f"Saved the updated version with LARGE FONTS and DIVERSE TRANSITIONS as: {updated_filename}")

if __name__ == "__main__":
    build_presentation()
