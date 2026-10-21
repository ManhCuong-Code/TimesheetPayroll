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
# CONSTANTS & MODERN COLOR PALETTE
# -------------------------------------------------------------
COLOR_NAVY_DARK = RGBColor(15, 23, 42)      # #0F172A - Deep Slate/Navy
COLOR_PRIMARY_NAVY = RGBColor(30, 58, 138)  # #1E3A8A - Dominant Navy Blue
COLOR_NAVY_HEADER = RGBColor(30, 41, 59)    # #1E293B - Dark Slate
COLOR_BLUE_ROYAL = RGBColor(37, 99, 235)    # #2563EB - Tech Royal Blue
COLOR_CYAN = RGBColor(14, 165, 233)         # #0EA5E9 - Vibrant Cyan
COLOR_TEAL = RGBColor(13, 148, 136)         # #0D9488 - Teal
COLOR_EMERALD = RGBColor(16, 185, 129)      # #10B981 - Emerald Green
COLOR_AMBER = RGBColor(245, 158, 11)        # #F59E0B - Amber
COLOR_ROSE = RGBColor(239, 68, 68)          # #EF4444 - Rose Red
COLOR_PURPLE = RGBColor(147, 51, 234)       # #9333EA - Purple

COLOR_BG_LIGHT = RGBColor(248, 250, 252)    # #F8FAFC - Light Slide Background
COLOR_CARD_BG = RGBColor(255, 255, 255)     # #FFFFFF - Pure White
COLOR_BORDER = RGBColor(203, 213, 225)      # #CBD5E1 - Slightly stronger border for high contrast
COLOR_TEXT_MAIN = RGBColor(15, 23, 42)      # #0F172A - Dark Crisp Text
COLOR_TEXT_MUTED = RGBColor(51, 65, 85)     # #334155 - Slate 700 for high readability
COLOR_WHITE = RGBColor(255, 255, 255)       # #FFFFFF - Crisp White
COLOR_CODE_BG = RGBColor(15, 23, 42)        # #0F172A - High-contrast code dark slate

FONT_TITLE = "Segoe UI"
FONT_BODY = "Segoe UI"
FONT_CODE = "Consolas"

def set_shape_flat(shape, fill_color, border_color=None, border_width=1.5):
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill_color
    if border_color:
        shape.line.color.rgb = border_color
        shape.line.width = Pt(border_width)
    else:
        shape.line.fill.background()

def create_header(slide, breadcrumb_text, title_text, subtitle_text=""):
    # Dual-tone top accent bar (glowing effect)
    top_bar1 = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(8.0), Inches(0.1))
    set_shape_flat(top_bar1, COLOR_BLUE_ROYAL)
    top_bar2 = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(8.0), Inches(0), Inches(5.333), Inches(0.1))
    set_shape_flat(top_bar2, COLOR_CYAN)
    
    # Larger, vibrant breadcrumb tag pill
    tag_bg = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(0.32), Inches(5.2), Inches(0.36))
    set_shape_flat(tag_bg, RGBColor(238, 242, 255), RGBColor(199, 210, 254), 1.5)
    tf_tag = tag_bg.text_frame
    tf_tag.word_wrap = True
    tf_tag.margin_left = tf_tag.margin_top = tf_tag.margin_right = tf_tag.margin_bottom = 0
    p_tag = tf_tag.paragraphs[0]
    p_tag.alignment = PP_ALIGN.CENTER
    run_tag = p_tag.add_run()
    run_tag.text = "✦  " + breadcrumb_text.upper()
    run_tag.font.name = FONT_BODY
    run_tag.font.size = Pt(11) # INCREASED from 9.5pt
    run_tag.font.bold = True
    run_tag.font.color.rgb = COLOR_PRIMARY_NAVY
    
    # Title & Subtitle box with MUCH LARGER text
    tb = slide.shapes.add_textbox(Inches(0.8), Inches(0.72), Inches(11.733), Inches(0.95))
    tf = tb.text_frame
    tf.word_wrap = True
    tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
    
    p_title = tf.paragraphs[0]
    p_title.space_after = Pt(3)
    run_title = p_title.add_run()
    run_title.text = title_text
    run_title.font.name = FONT_TITLE
    run_title.font.size = Pt(24) # INCREASED from 20pt to 24pt
    run_title.font.bold = True
    run_title.font.color.rgb = COLOR_NAVY_DARK
    
    if subtitle_text:
        p_sub = tf.add_paragraph()
        run_sub = p_sub.add_run()
        run_sub.text = subtitle_text
        run_sub.font.name = FONT_BODY
        run_sub.font.size = Pt(13) # INCREASED from 11.5pt to 13pt
        run_sub.font.color.rgb = COLOR_TEXT_MUTED

def create_footer(slide, current_slide, total_slides=15):
    line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.8), Inches(7.0), Inches(11.733), Inches(0.02))
    set_shape_flat(line, COLOR_BORDER)
    
    # Left
    tb_l = slide.shapes.add_textbox(Inches(0.8), Inches(7.04), Inches(5.5), Inches(0.35))
    tf_l = tb_l.text_frame
    tf_l.word_wrap = True
    tf_l.margin_left = tf_l.margin_top = tf_l.margin_right = tf_l.margin_bottom = 0
    p_l = tf_l.paragraphs[0]
    r_l = p_l.add_run()
    r_l.text = "Lập trình ứng dụng Web | Đồ án: Timesheet & Payroll Automation"
    r_l.font.name = FONT_BODY
    r_l.font.size = Pt(10) # INCREASED from 9pt
    r_l.font.bold = True
    r_l.font.color.rgb = COLOR_TEXT_MUTED
    
    # Center
    tb_c = slide.shapes.add_textbox(Inches(5.5), Inches(7.04), Inches(5.0), Inches(0.35))
    tf_c = tb_c.text_frame
    tf_c.word_wrap = True
    tf_c.margin_left = tf_c.margin_top = tf_c.margin_right = tf_c.margin_bottom = 0
    p_c = tf_c.paragraphs[0]
    r_c = p_c.add_run()
    r_c.text = "Trần Viết Mạnh Cường (23K4080003) | ĐH Kinh tế Huế"
    r_c.font.name = FONT_BODY
    r_c.font.size = Pt(10) # INCREASED from 9pt
    r_c.font.color.rgb = COLOR_TEXT_MUTED
    
    # Right Badge
    badge = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(11.633), Inches(7.03), Inches(0.9), Inches(0.32))
    set_shape_flat(badge, COLOR_NAVY_DARK)
    tf_b = badge.text_frame
    tf_b.margin_left = tf_b.margin_top = tf_b.margin_right = tf_b.margin_bottom = 0
    p_b = tf_b.paragraphs[0]
    p_b.alignment = PP_ALIGN.CENTER
    r_b = p_b.add_run()
    r_b.text = f"{current_slide:02d} / {total_slides:02d}"
    r_b.font.name = FONT_BODY
    r_b.font.size = Pt(10) # INCREASED from 9pt
    r_b.font.bold = True
    r_b.font.color.rgb = COLOR_WHITE

def add_card(slide, left, top, width, height, stripe_color=None, bg_color=COLOR_CARD_BG, border_color=COLOR_BORDER):
    card = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
    set_shape_flat(card, bg_color, border_color, 1.5)
    if stripe_color:
        stripe = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, Inches(0.12)) # Thicker 0.12in stripe
        set_shape_flat(stripe, stripe_color)
    return card

def add_speaker_note(slide, note_text):
    notes_slide = slide.notes_slide
    tf = notes_slide.notes_text_frame
    tf.text = note_text

def add_transition_to_slide(slide, trans_xml_tag):
    for child in list(slide._element):
        if child.tag.endswith('transition'):
            slide._element.remove(child)
            
    xml_str = f'<p:transition {nsdecls("p")} spd="med" advClick="1">{trans_xml_tag}</p:transition>'
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
    
    # -------------------------------------------------------------
    # SLIDE 1: TRANG BÌA (COVER SLIDE - DEEP NAVY GLOW)
    # -------------------------------------------------------------
    s1 = prs.slides.add_slide(blank_layout)
    bg1 = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg1, COLOR_NAVY_DARK)
    
    # Accent top glowing stripe (multi-color)
    g1 = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(6.5), Inches(0.14))
    set_shape_flat(g1, COLOR_CYAN)
    g2 = s1.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(6.5), Inches(0), Inches(6.833), Inches(0.14))
    set_shape_flat(g2, COLOR_BLUE_ROYAL)
    
    # University Logo
    hce_logo = "report_assets/hce_logo.png"
    if os.path.exists(hce_logo):
        s1.shapes.add_picture(hce_logo, Inches(0.8), Inches(0.52), width=Inches(1.2))
        
    # University Header Text (LARGER)
    tb_uni = s1.shapes.add_textbox(Inches(2.2), Inches(0.5), Inches(10.3), Inches(0.95))
    tf_uni = tb_uni.text_frame
    tf_uni.word_wrap = True
    tf_uni.margin_left = tf_uni.margin_top = tf_uni.margin_right = tf_uni.margin_bottom = 0
    p1 = tf_uni.paragraphs[0]
    r1 = p1.add_run()
    r1.text = "BỘ GIÁO DỤC VÀ ĐÀO TẠO  •  ĐẠI HỌC HUẾ"
    r1.font.name = FONT_BODY
    r1.font.size = Pt(12) # INCREASED
    r1.font.bold = True
    r1.font.color.rgb = COLOR_CYAN
    
    p2 = tf_uni.add_paragraph()
    p2.space_before = Pt(3)
    r2 = p2.add_run()
    r2.text = "TRƯỜNG ĐẠI HỌC KINH TẾ  —  KHOA HỆ THỐNG THÔNG TIN KINH TẾ"
    r2.font.name = FONT_BODY
    r2.font.size = Pt(14) # INCREASED
    r2.font.bold = True
    r2.font.color.rgb = COLOR_WHITE
    
    # Course Badge Tag
    badge_c = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(1.8), Inches(5.4), Inches(0.42))
    set_shape_flat(badge_c, RGBColor(30, 41, 59), COLOR_CYAN, 1.5)
    tf_bc = badge_c.text_frame
    tf_bc.margin_left = tf_bc.margin_top = tf_bc.margin_right = tf_bc.margin_bottom = 0
    p_bc = tf_bc.paragraphs[0]
    p_bc.alignment = PP_ALIGN.CENTER
    r_bc = p_bc.add_run()
    r_bc.text = "★ BÁO CÁO ĐỒ ÁN MÔN HỌC: LẬP TRÌNH ỨNG DỤNG WEB"
    r_bc.font.name = FONT_BODY
    r_bc.font.size = Pt(11.5) # INCREASED
    r_bc.font.bold = True
    r_bc.font.color.rgb = COLOR_CYAN
    
    # Main Topic Title (MUCH BIGGER: 32pt)
    tb_title = s1.shapes.add_textbox(Inches(0.8), Inches(2.35), Inches(11.733), Inches(1.7))
    tf_title = tb_title.text_frame
    tf_title.word_wrap = True
    tf_title.margin_left = tf_title.margin_top = tf_title.margin_right = tf_title.margin_bottom = 0
    
    p_m1 = tf_title.paragraphs[0]
    r_m1 = p_m1.add_run()
    r_m1.text = "XÂY DỰNG HỆ THỐNG ỨNG DỤNG WEB\nCHẤM CÔNG VÀ TÍNH LƯƠNG TỰ ĐỘNG"
    r_m1.font.name = FONT_TITLE
    r_m1.font.size = Pt(32) # INCREASED from 28pt to 32pt
    r_m1.font.bold = True
    r_m1.font.color.rgb = COLOR_WHITE
    
    p_m2 = tf_title.add_paragraph()
    p_m2.space_before = Pt(8)
    r_m2 = p_m2.add_run()
    r_m2.text = "Timesheet & Payroll Automation System  —  Clean Architecture 4 Tầng & .NET 10 Blazor"
    r_m2.font.name = FONT_BODY
    r_m2.font.size = Pt(16) # INCREASED from 14pt
    r_m2.font.color.rgb = RGBColor(148, 163, 184)
    
    # 4 Tech Pills (LARGER: 11pt)
    pills = [
        ("Clean Architecture 4 Tầng", COLOR_BLUE_ROYAL),
        (".NET 10 Blazor Server", COLOR_CYAN),
        ("SQL Server & Dapper ORM", COLOR_TEAL),
        ("Trợ Lý Lương AI Thông Minh", COLOR_AMBER)
    ]
    pill_w = Inches(2.78)
    for i, (pill_txt, pill_col) in enumerate(pills):
        px = Inches(0.8 + i * 2.98)
        pill_shape = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, px, Inches(4.25), pill_w, Inches(0.45))
        set_shape_flat(pill_shape, RGBColor(30, 41, 59), pill_col, 1.5)
        tf_p = pill_shape.text_frame
        tf_p.margin_left = tf_p.margin_top = tf_p.margin_right = tf_p.margin_bottom = 0
        p_p = tf_p.paragraphs[0]
        p_p.alignment = PP_ALIGN.CENTER
        r_p = p_p.add_run()
        r_p.text = pill_txt
        r_p.font.name = FONT_BODY
        r_p.font.size = Pt(11) # INCREASED
        r_p.font.bold = True
        r_p.font.color.rgb = COLOR_WHITE

    # Bottom Metadata Card Container
    card_meta = s1.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(4.9), Inches(11.733), Inches(1.9))
    set_shape_flat(card_meta, RGBColor(30, 41, 59), RGBColor(71, 85, 105), 1.5)
    
    meta_cols = [
        ("GIẢNG VIÊN HƯỚNG DẪN", "TS. Hà Ngọc Long", "Khoa Hệ thống Thông tin Kinh tế\nTrường Đại học Kinh tế, ĐH Huế"),
        ("SINH VIÊN THỰC HIỆN", "Trần Viết Mạnh Cường", "Mã SV: 23K4080003\nLớp: K57 Tin học Kinh tế"),
        ("NGÀNH & CHUYÊN NGÀNH", "Hệ thống Thông tin Quản lý", "Chuyên ngành: Tin học Kinh tế\nKhóa học: 2023 – 2027"),
        ("THÔNG TIN BÁO CÁO", "Đồ Án Lập Trình Web", "Thời gian: Năm 2026\nĐịa điểm: Thành phố Huế")
    ]
    for idx, (head_t, name_t, sub_t) in enumerate(meta_cols):
        cx = Inches(0.95 + idx * 2.9)
        tb_col = s1.shapes.add_textbox(cx, Inches(5.05), Inches(2.78), Inches(1.6))
        tf_col = tb_col.text_frame
        tf_col.word_wrap = True
        tf_col.margin_left = tf_col.margin_top = tf_col.margin_right = tf_col.margin_bottom = 0
        
        ph = tf_col.paragraphs[0]
        rh = ph.add_run()
        rh.text = head_t
        rh.font.name = FONT_BODY
        rh.font.size = Pt(10.5) # INCREASED
        rh.font.bold = True
        rh.font.color.rgb = COLOR_CYAN
        
        pn = tf_col.add_paragraph()
        pn.space_before = Pt(4)
        rn = pn.add_run()
        rn.text = name_t
        rn.font.name = FONT_TITLE
        rn.font.size = Pt(14) # INCREASED from 13pt
        rn.font.bold = True
        rn.font.color.rgb = COLOR_WHITE
        
        ps = tf_col.add_paragraph()
        ps.space_before = Pt(3)
        rs = ps.add_run()
        rs.text = sub_t
        rs.font.name = FONT_BODY
        rs.font.size = Pt(10.5) # INCREASED from 9.5pt
        rs.font.color.rgb = RGBColor(203, 213, 225)

    add_speaker_note(s1, "Kính thưa Thầy TS. Hà Ngọc Long cùng toàn thể quý Thầy Cô và các bạn sinh viên!\nEm tên là Trần Viết Mạnh Cường, sinh viên chuyên ngành Tin học Kinh tế, ĐH Kinh tế Huế...")

    # -------------------------------------------------------------
    # SLIDE 2: GIỚI THIỆU & BỐI CẢNH ĐỀ TÀI
    # -------------------------------------------------------------
    s2 = prs.slides.add_slide(blank_layout)
    bg2 = s2.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg2, COLOR_BG_LIGHT)
    create_header(s2, "Bối cảnh & Mục tiêu", "01. Giới thiệu tổng quan đề tài & Lý do phát triển", "Giải pháp tự động hóa toàn diện từ theo dõi ngày công đến quyết toán tiền lương")
    create_footer(s2, 2)
    
    # Left Card (Nỗi đau Excel)
    c2_l = add_card(s2, Inches(0.8), Inches(1.75), Inches(5.65), Inches(3.85), COLOR_ROSE)
    tb2_l = s2.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.15), Inches(3.45))
    tf2_l = tb2_l.text_frame
    tf2_l.word_wrap = True
    tf2_l.margin_left = tf2_l.margin_top = tf2_l.margin_right = tf2_l.margin_bottom = 0
    p = tf2_l.paragraphs[0]
    r = p.add_run("Thực Trạng Quản Lý Lương Tại Doanh Nghiệp SME")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15) # INCREASED from 14pt
    r.font.bold = True
    r.font.color.rgb = COLOR_ROSE
    
    bullets_l = [
        ("Nghiệp vụ tài chính cốt lõi: ", "Quản lý nhân sự và tính lương là nghiệp vụ sống còn, ảnh hưởng trực tiếp đến quyền lợi người lao động."),
        ("Lệ thuộc bảng tính Excel: ", "Dễ kéo nhầm ô công thức làm sai lệch lương hàng triệu đồng, phát sinh tranh chấp gay gắt."),
        ("Nguy cơ lộ dữ liệu thu nhập: ", "Chia sẻ chung file tính lương khiến nhân viên dòm ngó mức thu nhập của nhau, vi phạm quyền riêng tư."),
        ("Bất cập khi luật thay đổi: ", "Mức bảo hiểm (10.5%) và thuế TNCN thay đổi liên tục, rất khó đồng bộ trên bảng tính phân tán.")
    ]
    for b_title, b_desc in bullets_l:
        pb = tf2_l.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run("• " + b_title)
        rb1.font.bold = True
        rb1.font.size = Pt(12) # INCREASED from 11pt
        rb1.font.color.rgb = COLOR_NAVY_DARK
        rb2 = pb.add_run(b_desc)
        rb2.font.size = Pt(11) # INCREASED from 10.5pt
        rb2.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card (Mục tiêu 3 Trụ cột)
    c2_r = add_card(s2, Inches(6.85), Inches(1.75), Inches(5.65), Inches(3.85), COLOR_EMERALD)
    tb2_r = s2.shapes.add_textbox(Inches(7.1), Inches(1.95), Inches(5.15), Inches(3.45))
    tf2_r = tb2_r.text_frame
    tf2_r.word_wrap = True
    tf2_r.margin_left = tf2_r.margin_top = tf2_r.margin_right = tf2_r.margin_bottom = 0
    p = tf2_r.paragraphs[0]
    r = p.add_run("3 Trụ Cột Đột Phá Của Hệ Thống Web")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15) # INCREASED from 14pt
    r.font.bold = True
    r.font.color.rgb = COLOR_EMERALD
    
    bullets_r = [
        ("Tự động hóa 1-Chạm (1-Click Run): ", "Khép kín từ điểm danh đến tính lương hoàn tất trong 1.5 giây, xóa bỏ hoàn toàn sai số Excel."),
        ("Chuẩn hóa 100% Luật Lao Động: ", "Động cơ nạp tỷ lệ bảo hiểm động (10.5%) và biểu thuế TNCN 7 bậc, khóa sổ kỳ lương Read-Only."),
        ("Bảo mật đa tầng & Chống IDOR: ", "Phân quyền RBAC, băm BCrypt, chặn vượt quyền URL 403 Forbidden và bảo vệ phiếu lương cá nhân."),
        ("Cổng tự phục vụ & Trợ lý AI: ", "Nhân viên tự tra cứu phiếu lương an toàn và tương tác hỏi đáp lương với Google Gemini AI.")
    ]
    for b_title, b_desc in bullets_r:
        pb = tf2_r.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run("✓ " + b_title)
        rb1.font.bold = True
        rb1.font.size = Pt(12) # INCREASED from 11pt
        rb1.font.color.rgb = COLOR_EMERALD
        rb2 = pb.add_run(b_desc)
        rb2.font.size = Pt(11) # INCREASED from 10.5pt
        rb2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom 3 KPI Metric Cards (MUCH LARGER: 28pt)
    metrics = [
        ("QUY MÔ DOANH NGHIỆP", "20 – 50", "Nhân sự doanh nghiệp SME", COLOR_BLUE_ROYAL),
        ("TÍNH TOÁN BẢNG LƯƠNG", "1-Click Run", "Tự động hoàn toàn trong < 1.5s", COLOR_EMERALD),
        ("ĐỘ CHÍNH XÁC & BẢO MẬT", "100%", "Chuẩn luật & Chống lộ IDOR", COLOR_PURPLE)
    ]
    for i, (m_tag, m_val, m_sub, m_col) in enumerate(metrics):
        mx = Inches(0.8 + i * 4.04)
        add_card(s2, mx, Inches(5.75), Inches(3.64), Inches(1.1), bg_color=COLOR_WHITE)
        tb_m = s2.shapes.add_textbox(mx + Inches(0.2), Inches(5.8), Inches(3.24), Inches(0.98))
        tf_m = tb_m.text_frame
        tf_m.word_wrap = True
        tf_m.margin_left = tf_m.margin_top = tf_m.margin_right = tf_m.margin_bottom = 0
        
        pm1 = tf_m.paragraphs[0]
        rm1 = pm1.add_run(m_tag)
        rm1.font.name = FONT_BODY
        rm1.font.size = Pt(10) # INCREASED
        rm1.font.bold = True
        rm1.font.color.rgb = m_col
        
        pm2 = tf_m.add_paragraph()
        rm2 = pm2.add_run(m_val)
        rm2.font.name = FONT_TITLE
        rm2.font.size = Pt(22) # INCREASED from 18pt
        rm2.font.bold = True
        rm2.font.color.rgb = COLOR_NAVY_DARK
        
        pm3 = tf_m.add_paragraph()
        rm3 = pm3.add_run(m_sub)
        rm3.font.name = FONT_BODY
        rm3.font.size = Pt(10.5) # INCREASED from 9pt
        rm3.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s2, "Về bối cảnh: Excel gây ra 3 nỗi đau: kéo nhầm ô, tốn 2-3 ngày cuối tháng và lộ bí mật thu nhập...")

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
            ("Mô hình 4 Tầng độc lập: ", "Presentation, Use Cases, Core Domain và Infrastructure."),
            ("Nguyên lý Dependency Inversion: ", "Tầng Core Domain nằm ở trung tâm và độc lập hoàn toàn (Zero Dependency)."),
            ("Lợi ích kỹ thuật: ", "Dễ viết Unit Test, độc lập công nghệ UI và CSDL, khả năng mở rộng tối đa.")
        ]),
        ("02. BLAZOR INTERACTIVE SERVER (REAL-TIME)", COLOR_CYAN, [
            ("SignalR WebSocket: ", "Giao tiếp hai chiều thời gian thực giữa Client và Server mà không tải lại trang."),
            ("Trải nghiệm SPA mượt mà: ", "Tốc độ phản hồi dưới 1.2s, cập nhật DOM tức thì khi thao tác."),
            ("Đồng nhất ngôn ngữ C#: ", "Sử dụng C# từ giao diện đến nghiệp vụ, tận dụng kiểu tĩnh an toàn.")
        ]),
        ("03. BẢO MẬT PHÂN QUYỀN RBAC & BCRYPT", COLOR_PURPLE, [
            ("Mật khẩu băm BCrypt: ", "Băm 1 chiều kèm Salt ngẫu nhiên với WorkFactor = 11, chống tấn công dò mật khẩu."),
            ("Phân quyền 3 vai trò (RBAC): ", "Phân định quyền hạn chặt chẽ giữa Employee, HR Accountant và Admin."),
            ("Chống IDOR & URL Bypass: ", "Chặn truy cập trái phép bằng AuthorizeRouteView (HTTP 403 Forbidden).")
        ]),
        ("04. CƠ SỞ DỮ LIỆU 3NF & GIAO DỊCH ACID", COLOR_TEAL, [
            ("Chuẩn hóa Dạng chuẩn 3 (3NF): ", "Loại bỏ hoàn toàn dư thừa dữ liệu trên 11 bảng thực thể quan hệ."),
            ("Dapper Micro-ORM siêu tốc: ", "100% câu lệnh SQL tham số hóa (Parameterized Query) chống SQL Injection."),
            ("Toàn vẹn Giao dịch ACID: ", "Database Transaction bảo vệ cặp Header-Line (payslips - payslip_lines).")
        ])
    ]
    
    for idx, (c_title, c_color, c_items) in enumerate(concepts):
        col, row = idx % 2, idx // 2
        cx, cy = Inches(0.8 + col * 6.05), Inches(1.75 + row * 2.6)
        add_card(s3, cx, cy, Inches(5.65), Inches(2.45), c_color)
        tb_c = s3.shapes.add_textbox(cx + Inches(0.25), cy + Inches(0.18), Inches(5.15), Inches(2.15))
        tf_c = tb_c.text_frame
        tf_c.word_wrap = True
        tf_c.margin_left = tf_c.margin_top = tf_c.margin_right = tf_c.margin_bottom = 0
        
        pc = tf_c.paragraphs[0]
        rc = pc.add_run(c_title)
        rc.font.name = FONT_TITLE
        rc.font.size = Pt(13) # INCREASED from 12pt
        rc.font.bold = True
        rc.font.color.rgb = c_color
        
        for item_h, item_d in c_items:
            pi = tf_c.add_paragraph()
            pi.space_before = Pt(4)
            ri1 = pi.add_run("• " + item_h)
            ri1.font.bold = True
            ri1.font.size = Pt(11.5) # INCREASED from 10pt
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run(item_d)
            ri2.font.size = Pt(11) # INCREASED from 9.5pt
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s3, "Đề tài kết hợp 4 trụ cột công nghệ: Clean Architecture, Blazor Server, RBAC BCrypt và CSDL 3NF ACID...")

    # -------------------------------------------------------------
    # SLIDE 4: PHÂN TÍCH YÊU CẦU & MA TRẬN PHÂN QUYỀN
    # -------------------------------------------------------------
    s4 = prs.slides.add_slide(blank_layout)
    bg4 = s4.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg4, COLOR_BG_LIGHT)
    create_header(s4, "Phân tích yêu cầu", "03. Phân tích yêu cầu bài toán & Ma trận phân quyền (RBAC)", "Xác định rõ vấn đề cần giải quyết, nhóm đối tượng sử dụng và phạm vi chức năng")
    create_footer(s4, 4)
    
    # Left Card
    c4_l = add_card(s4, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_ROSE)
    tb4_l = s4.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.15), Inches(4.65))
    tf4_l = tb4_l.text_frame
    tf4_l.word_wrap = True
    tf4_l.margin_left = tf4_l.margin_top = tf4_l.margin_right = tf4_l.margin_bottom = 0
    p = tf4_l.paragraphs[0]
    r = p.add_run("4 Nỗi Đau Cần Giải Quyết Của Bảng Tính Excel")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_ROSE
    
    pains = [
        ("Dễ kéo nhầm ô công thức: ", "Kế toán chỉ cần copy nhầm hàng là sai lương nhân viên cả triệu đồng, dẫn đến tranh chấp."),
        ("Tiêu tốn nguồn lực và thời gian: ", "Mất từ 2 đến 3 ngày cuối tháng để đối soát từng tờ phiếu chấm công và đơn xin nghỉ phép giấy."),
        ("Lộ dữ liệu thu nhập cá nhân: ", "Chia sẻ chung một file Excel khiến nhân viên dòm ngó lương nhau, gây mất đoàn kết nội bộ."),
        ("Khó cập nhật chính sách pháp luật: ", "Mức bảo hiểm (10.5%) và thuế TNCN thay đổi thường xuyên, bị hardcode nên cực kỳ khó đồng bộ.")
    ]
    for p_title, p_desc in pains:
        pb = tf4_l.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run("⚠ " + p_title)
        rb1.font.bold = True
        rb1.font.size = Pt(12)
        rb1.font.color.rgb = COLOR_NAVY_DARK
        rb2 = pb.add_run(p_desc)
        rb2.font.size = Pt(11)
        rb2.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card
    c4_r = add_card(s4, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_PRIMARY_NAVY)
    tb4_r = s4.shapes.add_textbox(Inches(7.1), Inches(1.95), Inches(5.15), Inches(4.65))
    tf4_r = tb4_r.text_frame
    tf4_r.word_wrap = True
    tf4_r.margin_left = tf4_r.margin_top = tf4_r.margin_right = tf4_r.margin_bottom = 0
    p = tf4_r.paragraphs[0]
    r = p.add_run("Ma Trận Phân Quyền Người Dùng Hệ Thống (RBAC)")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_PRIMARY_NAVY
    
    roles = [
        ("NHÂN VIÊN (ROLE_EMPLOYEE)", COLOR_CYAN, [
            "Điểm danh Check-in / Check-out hàng ngày theo ca.",
            "Nộp đơn xin nghỉ phép trực tuyến có validation ngày tháng.",
            "Xem phiếu lương cá nhân bảo mật (Anti-IDOR - chống xem lén).",
            "Tương tác cùng Trợ lý Lương AI hỏi đáp và mô phỏng What-If."
        ]),
        ("KẾ TOÁN / NHÂN SỰ (ROLE_HR_ACCOUNTANT)", COLOR_EMERALD, [
            "Xét duyệt hoặc từ chối các đơn xin nghỉ phép của nhân viên.",
            "Bấm nút chạy tính toán bảng lương tự động 1-chạm (1-Click Run).",
            "Khóa sổ kỳ lương Read-Only (LOCKED) và xuất Excel Mẫu 02-LĐTL."
        ]),
        ("QUẢN TRỊ VIÊN HỆ THỐNG (ROLE_ADMIN)", COLOR_PURPLE, [
            "Quản lý tài khoản, cấp quyền người dùng và quản lý phòng ban.",
            "Cấu hình các tỷ lệ trích nộp bảo hiểm động (bảng deduction_rates)."
        ])
    ]
    for r_name, r_col, r_tasks in roles:
        pr = tf4_r.add_paragraph()
        pr.space_before = Pt(8)
        rr = pr.add_run("• " + r_name)
        rr.font.bold = True
        rr.font.size = Pt(12)
        rr.font.color.rgb = r_col
        for t in r_tasks:
            pt = tf4_r.add_paragraph()
            pt.space_before = Pt(2)
            rt = pt.add_run("  - " + t)
            rt.font.size = Pt(10.5)
            rt.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s4, "Hệ thống giải quyết 4 nỗi đau của Excel và phân định rạch ròi 3 vai trò RBAC...")

    # -------------------------------------------------------------
    # SLIDE 5: CÔNG NGHỆ SỬ DỤNG
    # -------------------------------------------------------------
    s5 = prs.slides.add_slide(blank_layout)
    bg5 = s5.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg5, COLOR_BG_LIGHT)
    create_header(s5, "Công nghệ sử dụng", "04. Ngăn xếp công nghệ phát triển đạt chuẩn công nghiệp", "Sự kết hợp tối ưu giữa hệ sinh thái Microsoft .NET 10, SQL Server, Dapper và AI")
    create_footer(s5, 5)
    
    stacks = [
        ("GIAO DIỆN & WEB (UI)", COLOR_CYAN, [
            (".NET 10 Blazor Server", "Interactive Components"),
            ("SignalR WebSocket", "Kênh truyền real-time"),
            ("Bootstrap 5.3 & CSS", "Responsive Mobile 390px"),
            ("FontAwesome 6 Icons", "Biểu tượng trực quan")
        ]),
        ("BACKEND & KIẾN TRÚC", COLOR_BLUE_ROYAL, [
            ("Ngôn ngữ C# 13", "Tính năng hiện đại, kiểu tĩnh"),
            ("Clean Architecture", "Tổ chức 4 tầng độc lập"),
            ("Dependency Injection", "Tiêm phụ thuộc lỏng lẻo"),
            ("SalaryFormulaEngine", "Động cơ tính toán độc lập")
        ]),
        ("CSDL & TRUY XUẤT", COLOR_TEAL, [
            ("Microsoft SQL Server", "CSDL BangChamCongDB"),
            ("Dapper Micro-ORM", "Truy xuất siêu tốc mượt mà"),
            ("100% Parameterized", "Triệt tiêu lỗi SQL Injection"),
            ("ACID Transaction", "Toàn vẹn cặp Header-Line")
        ]),
        ("BẢO MẬT & TRÍ TUỆ NHÂN TẠO", COLOR_PURPLE, [
            ("BCrypt.Net-Next", "Băm mật khẩu 1 chiều Salt"),
            ("RBAC Authorization", "Phân quyền vai trò an toàn"),
            ("Google Gemini API", "Trợ lý Lương AI thông minh"),
            ("Local Fallback Engine", "Quy tắc nội bộ khi mất mạng")
        ])
    ]
    for idx, (t_title, t_col, t_items) in enumerate(stacks):
        tx = Inches(0.8 + idx * 2.98)
        add_card(s5, tx, Inches(1.75), Inches(2.78), Inches(4.0), t_col)
        tb = s5.shapes.add_textbox(tx + Inches(0.18), Inches(1.95), Inches(2.42), Inches(3.6))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        p = tf.paragraphs[0]
        r = p.add_run(t_title)
        r.font.name = FONT_TITLE
        r.font.size = Pt(12)
        r.font.bold = True
        r.font.color.rgb = t_col
        for item_name, item_desc in t_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(6)
            ri1 = pi.add_run("• " + item_name + "\n")
            ri1.font.bold = True
            ri1.font.size = Pt(11)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run("   " + item_desc)
            ri2.font.size = Pt(10)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom highlight banner
    add_card(s5, Inches(0.8), Inches(5.95), Inches(11.733), Inches(0.9), bg_color=RGBColor(238, 242, 255), border_color=RGBColor(199, 210, 254))
    tb_b = s5.shapes.add_textbox(Inches(1.05), Inches(6.02), Inches(11.2), Inches(0.75))
    tf_b = tb_b.text_frame
    tf_b.word_wrap = True
    tf_b.margin_left = tf_b.margin_top = tf_b.margin_right = tf_b.margin_bottom = 0
    pb1 = tf_b.paragraphs[0]
    rb1 = pb1.add_run("★ NGUYÊN TẮC THIẾT KẾ CỐT LÕI: ")
    rb1.font.name = FONT_BODY
    rb1.font.size = Pt(11.5)
    rb1.font.bold = True
    rb1.font.color.rgb = COLOR_BLUE_ROYAL
    rb2 = pb1.add_run("Tầng Core Domain được thiết kế cô lập hoàn toàn (Zero Dependency), không phụ thuộc vào bất kỳ framework giao diện hay thư viện truy xuất CSDL nào. Đảm bảo khả năng kiểm thử độc lập 100% và bảo trì lâu dài.")
    rb2.font.name = FONT_BODY
    rb2.font.size = Pt(11)
    rb2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s5, "Ngăn xếp công nghệ: .NET 10 Blazor Server, SQL Server BangChamCongDB, Dapper ORM và Google Gemini AI...")

    # -------------------------------------------------------------
    # SLIDE 6: THIẾT KẾ KIẾN TRÚC & USE CASE
    # -------------------------------------------------------------
    s6 = prs.slides.add_slide(blank_layout)
    bg6 = s6.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg6, COLOR_BG_LIGHT)
    create_header(s6, "Thiết kế kiến trúc", "05. Kiến trúc Clean Architecture 4 tầng & Sơ đồ Use Case", "Tổ chức mã nguồn rành mạch theo trách nhiệm và mô hình hóa chức năng hệ thống")
    create_footer(s6, 6)
    
    # Left Card
    c6_l = add_card(s6, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_BLUE_ROYAL)
    tb6_l = s6.shapes.add_textbox(Inches(1.05), Inches(1.92), Inches(5.15), Inches(4.7))
    tf6_l = tb6_l.text_frame
    tf6_l.word_wrap = True
    tf6_l.margin_left = tf6_l.margin_top = tf6_l.margin_right = tf6_l.margin_bottom = 0
    p = tf6_l.paragraphs[0]
    r = p.add_run("Cấu Trúc Giải Pháp Clean Architecture 4 Tầng")
    r.font.name = FONT_TITLE
    r.font.size = Pt(14)
    r.font.bold = True
    r.font.color.rgb = COLOR_BLUE_ROYAL
    
    layers = [
        ("TẦNG 1: PRESENTATION (GIAO DIỆN WEB)", COLOR_CYAN,
         "TimesheetPayroll.Web (.NET 10 Blazor Server)\nRazor Components, Responsive CSS Mobile 390px"),
        ("TẦNG 2: USE CASES (NGHIỆP VỤ HỆ THỐNG)", COLOR_BLUE_ROYAL,
         "TimesheetPayroll.UseCases (CalculateBatch, ExportExcel...)\nKhai báo các Repository Interfaces trừu tượng"),
        ("TẦNG 3: CORE DOMAIN (TRUNG TÂM MIỀN LÕI)", COLOR_EMERALD,
         "TimesheetPayroll.CoreBusiness (0 Dependency)\nEntities và Động cơ tính lương SalaryFormulaEngine.cs"),
        ("TẦNG 4: INFRASTRUCTURE (HẠ TẦNG DỮ LIỆU)", COLOR_PURPLE,
         "TimesheetPayroll.DataStore.SQL.Dapper\nTriển khai truy vấn SQL Server BangChamCongDB")
    ]
    for l_name, l_col, l_desc in layers:
        pl = tf6_l.add_paragraph()
        pl.space_before = Pt(6)
        rl = pl.add_run("• " + l_name + "\n")
        rl.font.bold = True
        rl.font.size = Pt(11)
        rl.font.color.rgb = l_col
        rd = pl.add_run(l_desc)
        rd.font.size = Pt(10.5)
        rd.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card: Use Case Image
    c6_r = add_card(s6, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_PURPLE)
    tb6_r_title = s6.shapes.add_textbox(Inches(7.1), Inches(1.92), Inches(5.15), Inches(0.4))
    tf6_rt = tb6_r_title.text_frame
    tf6_rt.margin_left = tf6_rt.margin_top = tf6_rt.margin_right = tf6_rt.margin_bottom = 0
    prt = tf6_rt.paragraphs[0]
    rrt = prt.add_run("Sơ Đồ Use Case Tổng Quan (PlantUML Overview)")
    rrt.font.name = FONT_TITLE
    rrt.font.size = Pt(14)
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
    rrn = prn.add_run("★ Phân định 3 Actor người dùng và 1 AI Assistant điều khiển 19 Use Cases trải dài qua 3 phân hệ: Nhân sự, Chấm công và Quyết toán tiền lương.")
    rrn.font.name = FONT_BODY
    rrn.font.size = Pt(10.5)
    rrn.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s6, "Kiến trúc 4 tầng Clean Architecture: chiều phụ thuộc luôn hướng vào Core Domain...")

    # -------------------------------------------------------------
    # SLIDE 7: THIẾT KẾ CƠ SỞ DỮ LIỆU QUAN HỆ (3NF & ACID)
    # -------------------------------------------------------------
    s7 = prs.slides.add_slide(blank_layout)
    bg7 = s7.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg7, COLOR_BG_LIGHT)
    create_header(s7, "Cơ sở dữ liệu", "06. Thiết kế Cơ sở dữ liệu quan hệ chuẩn 3NF & Toàn vẹn ACID", "Cấu trúc 11 bảng thực thể cốt lõi trên SQL Server đảm bảo tính nhất quán tuyệt đối")
    create_footer(s7, 7)
    
    # Left Card
    c7_l = add_card(s7, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_TEAL)
    tb7_l = s7.shapes.add_textbox(Inches(1.05), Inches(1.92), Inches(5.15), Inches(4.7))
    tf7_l = tb7_l.text_frame
    tf7_l.word_wrap = True
    tf7_l.margin_left = tf7_l.margin_top = tf7_l.margin_right = tf7_l.margin_bottom = 0
    p = tf7_l.paragraphs[0]
    r = p.add_run("Danh Mục 11 Bảng Thực Thể Chuẩn Hóa 3NF")
    r.font.name = FONT_TITLE
    r.font.size = Pt(14)
    r.font.bold = True
    r.font.color.rgb = COLOR_TEAL
    
    tbl_grps = [
        ("1. Nhóm Quản Trị & Nhân Sự:", [
            "departments: Danh mục phòng ban, mã code UNIQUE",
            "employees: Hồ sơ nhân sự, CCCD UNIQUE, STK ngân hàng",
            "user_accounts: Tài khoản đăng nhập, password_hash (BCrypt)",
            "contracts: Hợp đồng lao động, mức lương cơ bản (base_salary >= 0)",
            "deduction_rates: Tỷ lệ trích nộp bảo hiểm động (BHXH 8%, BHYT...)"
        ]),
        ("2. Nhóm Chấm Công & Phép:", [
            "work_shifts: Danh mục ca làm việc và hệ số làm thêm",
            "timesheets: Nhật ký điểm danh Check-in/out, ngày công thực tế",
            "leave_requests: Đơn xin nghỉ phép, trạng thái PENDING/APPROVED"
        ]),
        ("3. Nhóm Kỳ Lương & Quyết Toán (Header - Line):", [
            "payroll_periods: Kỳ lương tháng, trạng thái DRAFT -> LOCKED -> PAID",
            "payslips (Header): Phiếu lương tổng hợp từng nhân viên",
            "payslip_lines (Line): Chi tiết từng dòng thu nhập và khấu trừ"
        ])
    ]
    for grp_t, grp_items in tbl_grps:
        pg = tf7_l.add_paragraph()
        pg.space_before = Pt(6)
        rg = pg.add_run("• " + grp_t)
        rg.font.bold = True
        rg.font.size = Pt(11.5)
        rg.font.color.rgb = COLOR_NAVY_DARK
        for tbl in grp_items:
            pt = tf7_l.add_paragraph()
            pt.space_before = Pt(2)
            rt = pt.add_run("  - " + tbl)
            rt.font.size = Pt(10)
            rt.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card
    c7_r = add_card(s7, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_PRIMARY_NAVY)
    tb7_r = s7.shapes.add_textbox(Inches(7.1), Inches(1.92), Inches(5.15), Inches(4.7))
    tf7_r = tb7_r.text_frame
    tf7_r.word_wrap = True
    tf7_r.margin_left = tf7_r.margin_top = tf7_r.margin_right = tf7_r.margin_bottom = 0
    p = tf7_r.paragraphs[0]
    r = p.add_run("Ràng Buộc Toàn Vẹn & Giao Dịch ACID")
    r.font.name = FONT_TITLE
    r.font.size = Pt(14)
    r.font.bold = True
    r.font.color.rgb = COLOR_PRIMARY_NAVY
    
    acids = [
        ("Mô hình Cặp Header - Line (payslips - payslip_lines): ",
         "Khi tính lương, thông tin tổng quát lưu tại payslips, chi tiết lương công, phụ cấp, trừ BHXH 8%, BHYT 1.5%, thuế TNCN lưu tại payslip_lines với khóa ngoại CASCADE."),
        ("Ràng buộc kiểm tra nghiêm ngặt (CHECK Constraints): ",
         "• chk_contract_salary: Lương cơ bản không âm (base_salary >= 0).\n• chk_period_month: Tháng kỳ lương hợp lệ từ 1 đến 12.\n• chk_period_status: Trạng thái thuộc DRAFT, CALCULATED, LOCKED, PAID."),
        ("Toàn vẹn giao dịch Database Transaction (ACID): ",
         "Quá trình ghi nhận bảng lương cho toàn bộ công ty được bọc trọn vẹn trong using var tx = conn.BeginTransaction(). Nếu xảy ra bất kỳ lỗi nào, hệ thống lập tức Rollback 100%."),
        ("Động cơ Tỷ lệ Bảo hiểm Động (Dynamic Rates): ",
         "Tỷ lệ trích nộp được nạp động từ bảng deduction_rates thay vì hardcode. Khi Nhà nước điều chỉnh mức đóng, quản trị viên chỉ cần cập nhật CSDL mà không cần sửa code.")
    ]
    for atitle, adesc in acids:
        pa = tf7_r.add_paragraph()
        pa.space_before = Pt(6)
        ra1 = pa.add_run("✓ " + atitle + "\n")
        ra1.font.bold = True
        ra1.font.size = Pt(11)
        ra1.font.color.rgb = COLOR_NAVY_DARK
        ra2 = pa.add_run(adesc)
        ra2.font.size = Pt(10)
        ra2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s7, "11 bảng thực thể chuẩn 3NF, mô hình Header-Line và giao dịch ACID...")

    # -------------------------------------------------------------
    # SLIDE 8: THIẾT KẾ GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG
    # -------------------------------------------------------------
    s8 = prs.slides.add_slide(blank_layout)
    bg8 = s8.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_flat_style(bg8, COLOR_BG_LIGHT)
    create_header(s8, "Thiết kế giao diện", "07. Trải nghiệm người dùng UI/UX & Responsive Mobile 390px", "Giao diện hiện đại, trực quan, tối ưu cho cả màn hình máy tính và thiết bị di động")
    create_footer(s8, 8)
    
    ui_cols = [
        ("DASHBOARD ĐỐI SOÁT QUỸ LƯƠNG", COLOR_BLUE_ROYAL, [
            ("4 Thẻ Chỉ Số KPI Trực Quan: ", "Hiển thị tổng nhân sự, tổng giờ công làm việc, quỹ lương thực chi và trạng thái khóa sổ."),
            ("Bảng Đối Soát Đa Cột Chi Tiết: ", "Đối soát từng nhân viên: Lương hợp đồng, Công thực tế, Phụ cấp, Giảm trừ BHXH/BHYT, Thuế TNCN và Thực lĩnh."),
            ("Thao Tác 1-Chạm Tiện Lợi: ", "Nút 'Chạy Tính Lương Tự Động', nút 'Khóa Sổ Kỳ Lương' và nút 'Xuất Báo Cáo Excel'.")
        ]),
        ("CỔNG TỰ PHỤC VỤ NHÂN VIÊN (ESS)", COLOR_CYAN, [
            ("Điểm Danh Check-in / Out 1 Chạm: ", "Giao diện điểm danh nhanh chóng, tự động đối chiếu khung giờ ca làm việc."),
            ("Tra Cứu Phiếu Lương An Toàn: ", "Hiển thị chi tiết từng dòng lương cá nhân, bảo vệ chống xem lén tuyệt đối (Anti-IDOR)."),
            ("Nộp Đơn Nghỉ Phép Trực Tuyến: ", "Form nộp đơn có kiểm tra validation ngày bắt đầu/kết thúc thời gian thực.")
        ]),
        ("TỐI ƯU MOBILE RESPONSIVE 390PX", COLOR_EMERALD, [
            ("Kiểm Thử Chuẩn Màn Hình 390px: ", "Tương thích hoàn hảo trên các dòng điện thoại thông minh (iPhone 12/14/15/16)."),
            ("Menu Tự Thu Gọn Hamburger: ", "Thanh điều hướng co giãn linh hoạt khi xem trên màn hình hẹp."),
            ("Bảng Tính Vuốt Chạm Cảm Ứng: ", "Bọc trong container .table-responsive hỗ trợ vuốt cuộn ngang mượt mà.")
        ])
    ]
    for idx, (u_title, u_col, u_items) in enumerate(ui_cols):
        ux = Inches(0.8 + idx * 4.04)
        add_card(s8, ux, Inches(1.75), Inches(3.64), Inches(4.0), u_col)
        tb = s8.shapes.add_textbox(ux + Inches(0.2), Inches(1.95), Inches(3.24), Inches(3.65))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        p = tf.paragraphs[0]
        r = p.add_run(u_title)
        r.font.name = FONT_TITLE
        r.font.size = Pt(12)
        r.font.bold = True
        r.font.color.rgb = u_col
        for item_h, item_d in u_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(7)
            ri1 = pi.add_run("• " + item_h + "\n")
            ri1.font.bold = True
            ri1.font.size = Pt(11)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run(item_d)
            ri2.font.size = Pt(10.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom AI highlight
    add_card(s8, Inches(0.8), Inches(5.95), Inches(11.733), Inches(0.9), bg_color=RGBColor(245, 243, 255), border_color=RGBColor(221, 214, 254))
    tb_ai = s8.shapes.add_textbox(Inches(1.05), Inches(6.02), Inches(11.2), Inches(0.75))
    tf_ai = tb_ai.text_frame
    tf_ai.word_wrap = True
    tf_ai.margin_left = tf_ai.margin_top = tf_ai.margin_right = tf_ai.margin_bottom = 0
    pai = tf_ai.paragraphs[0]
    rai1 = pai.add_run("✦ WIDGET TRỢ LÝ LƯƠNG AI NỔI TOÀN HỆ THỐNG: ")
    rai1.font.name = FONT_BODY
    rai1.font.size = Pt(11.5)
    rai1.font.bold = True
    rai1.font.color.rgb = COLOR_PURPLE
    rai2 = pai.add_run("Nút tròn thông minh nổi cố định ở góc phải dưới màn hình. Bấm vào mở khung chat kính mờ hiện đại, giải đáp thắc mắc lương tức thì và tự động co giãn calc(100vw - 20px) vừa vặn trên điện thoại di động.")
    rai2.font.name = FONT_BODY
    rai2.font.size = Pt(11)
    rai2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s8, "Giao diện UI/UX: Dashboard đối soát quỹ lương, Cổng tự phục vụ ESS và Responsive Mobile 390px...")

    # -------------------------------------------------------------
    # SLIDE 9: LUỒNG QUY TRÌNH NGHIỆP VỤ
    # -------------------------------------------------------------
    s9 = prs.slides.add_slide(blank_layout)
    bg9 = s9.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg9, COLOR_BG_LIGHT)
    create_header(s9, "Quy trình nghiệp vụ", "08. Luồng nghiệp vụ Chấm công, Tính lương & Khóa sổ kỳ lương", "Quy trình khép kín tự động hóa từ điểm danh hàng ngày đến chốt số liệu tài chính")
    create_footer(s9, 9)
    
    flows = [
        ("LUỒNG 1: CHẤM CÔNG & NGHỈ PHÉP", COLOR_CYAN, [
            ("Bước 1: ", "Nhân viên điểm danh Check-in khi đến và Check-out khi về."),
            ("Bước 2: ", "So khớp khung giờ với ca làm việc (work_shifts) để xác định đúng giờ, đi muộn, về sớm."),
            ("Bước 3: ", "Nhân viên nộp đơn xin nghỉ phép trực tuyến (leave_requests)."),
            ("Bước 4: ", "Kế toán/Quản lý xét duyệt đơn phép (APPROVED / REJECTED)."),
            ("Bước 5: ", "Tự động tổng hợp ngày công thực tế và ngày nghỉ có phép.")
        ]),
        ("LUỒNG 2: TÍNH LƯƠNG 1-CHẠM", COLOR_EMERALD, [
            ("Bước 1: ", "Kế toán chọn kỳ lương cần tính toán (tháng/năm)."),
            ("Bước 2: ", "Nạp danh sách nhân sự, hợp đồng và mức lương cơ bản."),
            ("Bước 3: ", "Nạp tổng số ngày công và ngày phép từ timesheets."),
            ("Bước 4: ", "Nạp tỷ lệ trích nộp bảo hiểm động từ deduction_rates."),
            ("Bước 5: ", "Thực thi SalaryFormulaEngine tính Gross, BH 10.5%, thuế TNCN và Net trong 1 Transaction.")
        ]),
        ("LUỒNG 3: KHÓA SỔ READ-ONLY", COLOR_PURPLE, [
            ("1. DRAFT: ", "Kỳ lương nháp, đang thu thập ngày công."),
            ("2. CALCULATED: ", "Đã tính toán xong, kế toán đối soát."),
            ("3. LOCKED: ", "Kế toán bấm 'Khóa Sổ' -> Kỳ lương chuyển Read-Only bất biến, không được tính lại."),
            ("4. PAID: ", "Xuất ủy nhiệm chi ngân hàng và hoàn tất chi trả."),
            ("Bảo mật tài chính: ", "Ngăn chặn gian lận hoặc can thiệp số liệu sau khi đã chốt sổ.")
        ])
    ]
    for idx, (fl_title, fl_col, fl_steps) in enumerate(flows):
        fx = Inches(0.8 + idx * 4.04)
        add_card(s9, fx, Inches(1.75), Inches(3.64), Inches(4.1), fl_col)
        tb = s9.shapes.add_textbox(fx + Inches(0.2), Inches(1.95), Inches(3.24), Inches(3.8))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        p = tf.paragraphs[0]
        r = p.add_run(fl_title)
        r.font.name = FONT_TITLE
        r.font.size = Pt(12)
        r.font.bold = True
        r.font.color.rgb = fl_col
        for st_head, st_desc in fl_steps:
            pi = tf.add_paragraph()
            pi.space_before = Pt(6)
            ri1 = pi.add_run("• " + st_head)
            ri1.font.bold = True
            ri1.font.size = Pt(11)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run(st_desc)
            ri2.font.size = Pt(10.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom State Machine
    state_img = "uml/images/12_state_payroll_period.png"
    if os.path.exists(state_img):
        add_card(s9, Inches(0.8), Inches(6.0), Inches(11.733), Inches(0.95), bg_color=COLOR_WHITE)
        tb_sm = s9.shapes.add_textbox(Inches(1.0), Inches(6.08), Inches(3.5), Inches(0.8))
        tf_sm = tb_sm.text_frame
        tf_sm.word_wrap = True
        tf_sm.margin_left = tf_sm.margin_top = tf_sm.margin_right = tf_sm.margin_bottom = 0
        psm = tf_sm.paragraphs[0]
        rsm = psm.add_run("SƠ ĐỒ TRẠNG THÁI KỲ LƯƠNG:\nDRAFT → CALCULATED → LOCKED (Read-Only) → PAID")
        rsm.font.name = FONT_BODY
        rsm.font.size = Pt(10.5)
        rsm.font.bold = True
        rsm.font.color.rgb = COLOR_BLUE_ROYAL
        s9.shapes.add_picture(state_img, Inches(4.7), Inches(6.05), height=Inches(0.85))

    add_speaker_note(s9, "3 luồng nghiệp vụ: Chấm công phép, Tính lương 1-chạm và Khóa sổ kỳ lương Read-Only...")

    # -------------------------------------------------------------
    # SLIDE 10: MINH HỌA LẬP TRÌNH THEN CHỐT (LARGER CODE: 10.5pt)
    # -------------------------------------------------------------
    s10 = prs.slides.add_slide(blank_layout)
    bg10 = s10.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg10, COLOR_BG_LIGHT)
    create_header(s10, "Minh họa lập trình", "09. Đoạn mã nguồn tiêu biểu & Kỹ thuật đảm bảo toàn vẹn", "Triển khai Database Transaction ACID và Động cơ tính lương theo luật")
    create_footer(s10, 10)
    
    # Left Code Box
    c10_l = add_card(s10, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_BLUE_ROYAL, bg_color=COLOR_CODE_BG)
    tb10_l = s10.shapes.add_textbox(Inches(1.0), Inches(1.92), Inches(5.25), Inches(4.7))
    tf10_l = tb10_l.text_frame
    tf10_l.word_wrap = True
    tf10_l.margin_left = tf10_l.margin_top = tf10_l.margin_right = tf10_l.margin_bottom = 0
    p = tf10_l.paragraphs[0]
    r = p.add_run("1. Quản Trị Giao Dịch ACID (SqlRepositories.cs)")
    r.font.name = FONT_TITLE
    r.font.size = Pt(13)
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
    tx.Commit(); // Cam kết an toàn ACID
} catch {
    tx.Rollback(); // Hoàn nguyên 100% nếu có lỗi
    throw;
}"""
    p_code1 = tf10_l.add_paragraph()
    p_code1.space_before = Pt(8)
    r_code1 = p_code1.add_run(code1)
    r_code1.font.name = FONT_CODE
    r_code1.font.size = Pt(10.5) # INCREASED from 8.5pt to 10.5pt!
    r_code1.font.color.rgb = RGBColor(241, 245, 249)
    
    p_note1 = tf10_l.add_paragraph()
    p_note1.space_before = Pt(8)
    r_note1 = p_note1.add_run("★ Bảo đảm: Không bao giờ xảy ra tình trạng lưu dở dang Header mà thiếu Line chi tiết khi mất điện hoặc lỗi mạng.")
    r_note1.font.name = FONT_BODY
    r_note1.font.size = Pt(10.5)
    r_note1.font.color.rgb = COLOR_AMBER

    # Right Code Box
    c10_r = add_card(s10, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_EMERALD, bg_color=COLOR_CODE_BG)
    tb10_r = s10.shapes.add_textbox(Inches(7.05), Inches(1.92), Inches(5.25), Inches(4.7))
    tf10_r = tb10_r.text_frame
    tf10_r.word_wrap = True
    tf10_r.margin_left = tf10_r.margin_top = tf10_r.margin_right = tf10_r.margin_bottom = 0
    p = tf10_r.paragraphs[0]
    r = p.add_run("2. Động Cơ Tính Lương & Phòng Vệ URL 403")
    r.font.name = FONT_TITLE
    r.font.size = Pt(13)
    r.font.bold = True
    r.font.color.rgb = COLOR_EMERALD
    
    code2 = """// Động cơ tính lương chuẩn luật (SalaryFormulaEngine.cs)
decimal dayRate = contract.BaseSalary / standardDays;
decimal proratedBase = dayRate * actualWorkingDays;

// Khấu trừ BHXH 8%, BHYT 1.5%, BHTN 1% từ CSDL động
decimal bhxh = contract.BaseSalary * rates["BHXH_EMP"];
decimal totalDeductions = bhxh + bhyt + bhtn;

// Thuế TNCN lũy tiến sau giảm trừ gia cảnh 11tr
decimal pit = CalculatePersonalIncomeTax(taxableIncome);
decimal netSalary = gross - totalDeductions - pit;

// Phòng vệ Router URL (Routes.razor)
<AuthorizeRouteView RouteData="@routeData">
    <NotAuthorized>
        <AccessDenied403 /> <!-- Chặn người thường -->
    </NotAuthorized>
</AuthorizeRouteView>"""
    p_code2 = tf10_r.add_paragraph()
    p_code2.space_before = Pt(8)
    r_code2 = p_code2.add_run(code2)
    r_code2.font.name = FONT_CODE
    r_code2.font.size = Pt(10.5) # INCREASED from 8.5pt to 10.5pt!
    r_code2.font.color.rgb = RGBColor(241, 245, 249)
    
    p_note2 = tf10_r.add_paragraph()
    p_note2.space_before = Pt(8)
    r_note2 = p_note2.add_run("★ Bảo vệ đa tầng: Tỷ lệ bảo hiểm nạp động từ CSDL; Router chặn ngay nếu người dùng cố gõ URL quản trị.")
    r_note2.font.name = FONT_BODY
    r_note2.font.size = Pt(10.5)
    r_note2.font.color.rgb = COLOR_CYAN

    add_speaker_note(s10, "Minh họa 2 đoạn code then chốt: Database Transaction ACID và Động cơ tính lương theo luật...")

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
            ("Tốc Độ Vượt Trội: ", "Xử lý toàn bộ bảng lương công ty 20-50 nhân sự chỉ mất dưới 1.5 giây."),
            ("Độ Chính Xác Tuyệt Đối: ", "Loại bỏ hoàn toàn rủi ro kéo nhầm dòng, lệch ô công thức thường gặp trên Excel."),
            ("Bảo Toàn Số Liệu: ", "Cơ chế khóa sổ (LOCKED) ngăn chặn can thiệp sửa đổi trái phép sau khi duyệt.")
        ]),
        ("TRỢ LÝ LƯƠNG AI THÔNG MINH", COLOR_PURPLE, [
            ("Google Gemini API: ", "Giải đáp ngôn ngữ tự nhiên: 'Tại sao lương tháng này của em bị trừ?'."),
            ("Mô Phỏng What-If: ", "Cho phép giả định: 'Nếu làm thêm 10 giờ OT thì thực lĩnh tăng bao nhiêu?'."),
            ("Local Fallback Engine: ", "Động cơ quy tắc nội bộ tự kích hoạt khi mất mạng, đảm bảo không gián đoạn.")
        ]),
        ("XUẤT BÁO CÁO CHUẨN TÀI CHÍNH", COLOR_BLUE_ROYAL, [
            ("Excel Mẫu 02-LĐTL: ", "Bảng thanh toán tiền lương chuẩn theo Thông tư Bộ Tài chính đầy đủ cột mục."),
            ("Định Dạng Tiền Tệ: ", "Định dạng tiền tệ chuyên nghiệp mso-number-format: '#,##0' VNĐ."),
            ("CSV Chuyển Khoản: ", "Xuất file ủy nhiệm chi gửi trực tiếp ngân hàng (Vietcombank, MB Bank).")
        ])
    ]
    for idx, (res_title, res_col, res_items) in enumerate(results):
        rx = Inches(0.8 + idx * 4.04)
        add_card(s11, rx, Inches(1.75), Inches(3.64), Inches(4.0), res_col)
        tb = s11.shapes.add_textbox(rx + Inches(0.2), Inches(1.95), Inches(3.24), Inches(3.65))
        tf = tb.text_frame
        tf.word_wrap = True
        tf.margin_left = tf.margin_top = tf.margin_right = tf.margin_bottom = 0
        p = tf.paragraphs[0]
        r = p.add_run(res_title)
        r.font.name = FONT_TITLE
        r.font.size = Pt(12)
        r.font.bold = True
        r.font.color.rgb = res_col
        for item_h, item_d in res_items:
            pi = tf.add_paragraph()
            pi.space_before = Pt(7)
            ri1 = pi.add_run("✓ " + item_h + "\n")
            ri1.font.bold = True
            ri1.font.size = Pt(11)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run(item_d)
            ri2.font.size = Pt(10.5)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    # Bottom Banner
    add_card(s11, Inches(0.8), Inches(5.95), Inches(11.733), Inches(0.9), bg_color=RGBColor(240, 253, 244), border_color=RGBColor(187, 247, 208))
    tb_bres = s11.shapes.add_textbox(Inches(1.05), Inches(6.02), Inches(11.2), Inches(0.75))
    tf_bres = tb_bres.text_frame
    tf_bres.word_wrap = True
    tf_bres.margin_left = tf_bres.margin_top = tf_bres.margin_right = tf_bres.margin_bottom = 0
    pbr = tf_bres.paragraphs[0]
    rbr1 = pbr.add_run("★ MINH CHỨNG VẬN HÀNH THỰC TẾ: ")
    rbr1.font.name = FONT_BODY
    rbr1.font.size = Pt(11.5)
    rbr1.font.bold = True
    rbr1.font.color.rgb = COLOR_EMERALD
    rbr2 = pbr.add_run("Ứng dụng đã hoàn thành 100% trên môi trường .NET 10 Blazor Server và SQL Server 2022. Toàn bộ 5 phân hệ từ Đăng nhập, Điểm danh, Duyệt phép, Tính lương đến Xuất file Excel đều vận hành trơn tru với độ phản hồi dưới 1.2s.")
    rbr2.font.name = FONT_BODY
    rbr2.font.size = Pt(11)
    rbr2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s11, "Kết quả thực hiện: Tính lương 1-chạm 1.5s, Trợ lý AI và Xuất Excel Mẫu 02-LĐTL...")

    # -------------------------------------------------------------
    # SLIDE 12: KIỂM THỬ HỆ THỐNG & BẢO MẬT (LARGER TABLE: 11pt)
    # -------------------------------------------------------------
    s12 = prs.slides.add_slide(blank_layout)
    bg12 = s12.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg12, COLOR_BG_LIGHT)
    create_header(s12, "Kiểm thử hệ thống", "11. Kịch bản kiểm thử then chốt & Đánh giá an toàn bảo mật", "Thực thi 10 Test Cases (TC01 – TC10) bao phủ chức năng, toàn vẹn dữ liệu và bảo mật")
    create_footer(s12, 12)
    
    rows, cols = 8, 5
    table_shape = s12.shapes.add_table(rows, cols, Inches(0.8), Inches(1.75), Inches(11.733), Inches(4.15))
    table = table_shape.table
    table.columns[0].width = Inches(1.1)
    table.columns[1].width = Inches(3.2)
    table.columns[2].width = Inches(3.5)
    table.columns[3].width = Inches(2.733)
    table.columns[4].width = Inches(1.2)
    
    headers = ["Mã TC", "Chức Năng Kiểm Thử", "Kết Quả Kỳ Vọng", "Kết Quả Thực Tế", "Đánh Giá"]
    for c_idx, h_text in enumerate(headers):
        cell = table.cell(0, c_idx)
        cell.fill.solid()
        cell.fill.fore_color.rgb = COLOR_NAVY_DARK
        p = cell.text_frame.paragraphs[0]
        p.alignment = PP_ALIGN.CENTER
        r = p.add_run(h_text)
        r.font.name = FONT_BODY
        r.font.size = Pt(11.5) # INCREASED from 10pt
        r.font.bold = True
        r.font.color.rgb = COLOR_WHITE
        
    test_cases = [
        ("TC01", "Đăng nhập xác thực BCrypt", "Đăng nhập thành công, cấp Claims ROLE_ADMIN", "Đăng nhập mượt mà, chuyển trang đúng vai trò", "ĐẠT (100%)"),
        ("TC03", "Ràng buộc kiểm tra Lương âm", "CSDL chk_contract_salary từ chối base_salary < 0", "CSDL từ chối, ứng dụng báo alert đỏ thân thiện", "ĐẠT (100%)"),
        ("TC05", "Tính lương 1-chạm Transaction", "Mở Transaction, lưu đồng thời payslips & lines", "Ghi nhận CSDL an toàn, trạng thái CALCULATED", "ĐẠT (100%)"),
        ("TC06", "Khấu trừ Bảo hiểm 10.5%", "Lương 10tr -> Trừ BHXH 800k, BHYT 150k, BHTN 100k", "Số liệu tính toán chuẩn xác 100% khớp bảng rates", "ĐẠT (100%)"),
        ("TC07", "Khóa sổ kỳ lương Read-Only", "Chặn tính lại khi kỳ lương đã ở trạng thái LOCKED", "Ứng dụng chặn, báo lỗi thân thiện không văng app", "ĐẠT (100%)"),
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
            r = p.add_run(val)
            r.font.name = FONT_BODY
            r.font.size = Pt(10.5) # INCREASED from 9.2pt
            if c_idx == 4:
                r.font.bold = True
                r.font.color.rgb = COLOR_EMERALD
            elif c_idx == 0:
                r.font.bold = True
                r.font.color.rgb = COLOR_BLUE_ROYAL
            else:
                r.font.color.rgb = COLOR_TEXT_MAIN

    # Bottom Summary Badge
    add_card(s12, Inches(0.8), Inches(6.05), Inches(11.733), Inches(0.82), bg_color=RGBColor(236, 253, 245), border_color=RGBColor(167, 243, 208))
    tb_bt = s12.shapes.add_textbox(Inches(1.05), Inches(6.12), Inches(11.2), Inches(0.68))
    tf_bt = tb_bt.text_frame
    tf_bt.word_wrap = True
    tf_bt.margin_left = tf_bt.margin_top = tf_bt.margin_right = tf_bt.margin_bottom = 0
    pbt = tf_bt.paragraphs[0]
    rbt1 = pbt.add_run("★ TỔNG KẾT KẾT QUẢ KIỂM THỬ: ")
    rbt1.font.name = FONT_BODY
    rbt1.font.size = Pt(11.5)
    rbt1.font.bold = True
    rbt1.font.color.rgb = COLOR_EMERALD
    rbt2 = pbt.add_run("10/10 Kịch bản kiểm thử đều đạt kết quả kỳ vọng 100%. Hệ thống chứng minh khả năng phòng vệ đa tầng: triệt tiêu SQL Injection bằng Dapper, chống Bypass URL bằng HTTP 403 Forbidden và ngăn ngừa lỗ hổng IDOR.")
    rbt2.font.name = FONT_BODY
    rbt2.font.size = Pt(11)
    rbt2.font.color.rgb = COLOR_TEXT_MAIN

    add_speaker_note(s12, "10/10 Test Cases kiểm thử thực tế đạt 100% kết quả kỳ vọng...")

    # -------------------------------------------------------------
    # SLIDE 13: ĐÁNH GIÁ HỆ THỐNG (ƯU ĐIỂM & HẠN CHẾ)
    # -------------------------------------------------------------
    s13 = prs.slides.add_slide(blank_layout)
    bg13 = s13.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg13, COLOR_BG_LIGHT)
    create_header(s13, "Đánh giá hệ thống", "12. Đánh giá ưu điểm vượt trội & Các hạn chế cần hoàn thiện", "Cái nhìn khách quan về kết quả đạt được và các thách thức trong triển khai thực tế")
    create_footer(s13, 13)
    
    # Left Card
    c13_l = add_card(s13, Inches(0.8), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_EMERALD)
    tb13_l = s13.shapes.add_textbox(Inches(1.05), Inches(1.95), Inches(5.15), Inches(4.65))
    tf13_l = tb13_l.text_frame
    tf13_l.word_wrap = True
    tf13_l.margin_left = tf13_l.margin_top = tf13_l.margin_right = tf13_l.margin_bottom = 0
    p = tf13_l.paragraphs[0]
    r = p.add_run("Ưu Điểm Vượt Trội Của Hệ Thống (4 Điểm Sáng)")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_EMERALD
    
    pros = [
        ("Kiến trúc phần mềm chuẩn mực: ", "Clean Architecture 4 tầng phân tách độc lập, tuân thủ Dependency Inversion. Tầng Core Business cô lập, dễ bảo trì và kiểm thử."),
        ("Tự động hóa 100% theo Luật: ", "Rút ngắn thời gian tính lương từ 2-3 ngày xuống còn 1.5 giây. Tỷ lệ trích nộp bảo hiểm động và thuế lũy tiến chuẩn xác."),
        ("Bảo mật đa tầng & Quyền riêng tư: ", "Mã hóa BCrypt, phân quyền RBAC, Router chặn truy cập trái phép bằng HTTP 403 Forbidden và phòng vệ chống IDOR."),
        ("Trải nghiệm người dùng hiện đại: ", "Tích hợp Trợ lý Lương AI hỏi đáp tức thì và giao diện tương thích Responsive Mobile 390px.")
    ]
    for p_title, p_desc in pros:
        pb = tf13_l.add_paragraph()
        pb.space_before = Pt(8)
        rb1 = pb.add_run("✓ " + p_title)
        rb1.font.bold = True
        rb1.font.size = Pt(12)
        rb1.font.color.rgb = COLOR_NAVY_DARK
        rb2 = pb.add_run(p_desc)
        rb2.font.size = Pt(11)
        rb2.font.color.rgb = COLOR_TEXT_MUTED

    # Right Card
    c13_r = add_card(s13, Inches(6.85), Inches(1.75), Inches(5.65), Inches(5.05), COLOR_AMBER)
    tb13_r = s13.shapes.add_textbox(Inches(7.1), Inches(1.95), Inches(5.15), Inches(4.65))
    tf13_r = tb13_r.text_frame
    tf13_r.word_wrap = True
    tf13_r.margin_left = tf13_r.margin_top = tf13_r.margin_right = tf13_r.margin_bottom = 0
    p = tf13_r.paragraphs[0]
    r = p.add_run("Hạn Chế Hiện Tại & Vấn Đề Cần Hoàn Thiện")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_AMBER
    
    cons = [
        ("Chưa kết nối phần cứng máy chấm công: ", "Hiện tại điểm danh đang thực hiện qua giao diện web; chưa tích hợp SDK kết nối máy chấm công vân tay/FaceID tại văn phòng."),
        ("Chưa liên kết cổng thanh toán trực tuyến: ", "Mới dừng lại ở xuất file CSV gửi ngân hàng, chưa gọi trực tiếp Open Banking API để tự động chuyển khoản tiền lương."),
        ("Phụ thuộc đường truyền Internet khi gọi AI: ", "Tính năng giải thích ngôn ngữ tự nhiên nâng cao cần Internet ổn định (dù đã có bộ Fallback Engine dự phòng)."),
        ("Chưa hỗ trợ đa ngôn ngữ: ", "Hệ thống hiện chỉ hỗ trợ tiếng Việt, cần bổ sung tiếng Anh nếu triển khai cho doanh nghiệp nước ngoài (FDI).")
    ]
    for c_title, c_desc in cons:
        pc = tf13_r.add_paragraph()
        pc.space_before = Pt(8)
        rc1 = pc.add_run("⚠ " + c_title)
        rc1.font.bold = True
        rc1.font.size = Pt(12)
        rc1.font.color.rgb = COLOR_NAVY_DARK
        rc2 = pc.add_run(c_desc)
        rc2.font.size = Pt(11)
        rc2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s13, "Đánh giá khách quan: 4 ưu điểm vượt trội và 4 hạn chế thực tế cần cải thiện...")

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
            ("Tích hợp SDK Thiết Bị: ", "Kết nối trực tiếp máy chấm công vân tay, nhận diện khuôn mặt (FaceID) tại cửa văn phòng."),
            ("Đồng Bộ Thời Gian Thực: ", "Sử dụng Webhook hoặc Background Worker tự động đồng bộ giờ vào ra về CSDL ngay lập tức.")
        ]),
        ("02. TÍCH HỢP CỔNG OPEN BANKING API", COLOR_EMERALD, [
            ("Chi Trả Lương Tự Động 1-Chạm: ", "Kết nối API ngân hàng điện tử (Vietcombank, MB Bank) qua kênh bảo mật OAuth2."),
            ("Tự Động Chuyển Khoản: ", "Ngay khi kỳ lương chuyển trạng thái PAID, hệ thống tự động phát lệnh chuyển khoản tiền lương.")
        ]),
        ("03. MOBILE APP / PWA VỚI ĐỊNH VỊ GPS", COLOR_PURPLE, [
            ("Ứng Dụng Di Động: ", "Xây dựng ứng dụng di động cho hệ điều hành iOS và Android."),
            ("Chấm Công GPS Geofencing: ", "Hỗ trợ nhân viên làm việc từ xa (WFH) điểm danh bằng định vị GPS và chụp ảnh khuôn mặt."),
            ("Thông Báo Đẩy: ", "Báo tin tức thì về điện thoại ngay khi phiếu lương tháng mới được phê duyệt.")
        ]),
        ("04. DỰ BÁO QUỸ LƯƠNG BẰNG MACHINE LEARNING", COLOR_BLUE_ROYAL, [
            ("Phân Tích Xu Hướng: ", "Ứng dụng mô hình học máy phân tích biến động chi phí làm thêm giờ (OT) và tỷ lệ nghỉ việc."),
            ("Tối Ưu Ngân Sách: ", "Khuyến nghị phân bổ ngân sách tiền lương và khen thưởng tối ưu cho ban giám đốc.")
        ])
    ]
    for idx, (f_title, f_col, f_items) in enumerate(future_plans):
        col, row = idx % 2, idx // 2
        fx, fy = Inches(0.8 + col * 6.05), Inches(1.75 + row * 2.6)
        add_card(s14, fx, fy, Inches(5.65), Inches(2.45), f_col)
        tb_f = s14.shapes.add_textbox(fx + Inches(0.25), fy + Inches(0.18), Inches(5.15), Inches(2.15))
        tf_f = tb_f.text_frame
        tf_f.word_wrap = True
        tf_f.margin_left = tf_f.margin_top = tf_f.margin_right = tf_f.margin_bottom = 0
        pf = tf_f.paragraphs[0]
        rf = pf.add_run(f_title)
        rf.font.name = FONT_TITLE
        rf.font.size = Pt(13)
        rf.font.bold = True
        rf.font.color.rgb = f_col
        for item_h, item_d in f_items:
            pi = tf_f.add_paragraph()
            pi.space_before = Pt(5)
            ri1 = pi.add_run("• " + item_h)
            ri1.font.bold = True
            ri1.font.size = Pt(11.5)
            ri1.font.color.rgb = COLOR_NAVY_DARK
            ri2 = pi.add_run(item_d)
            ri2.font.size = Pt(11)
            ri2.font.color.rgb = COLOR_TEXT_MUTED

    add_speaker_note(s14, "Định hướng 4 mũi nhọn: IoT máy chấm công, Open Banking API, Mobile App GPS và Machine Learning...")

    # -------------------------------------------------------------
    # SLIDE 15: KẾT LUẬN & LỜI CẢM ƠN (CLOSING & Q&A)
    # -------------------------------------------------------------
    s15 = prs.slides.add_slide(blank_layout)
    bg15 = s15.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(7.5))
    set_shape_flat(bg15, COLOR_NAVY_DARK)
    
    # Glowing top stripe
    g15 = s15.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0), Inches(0), Inches(13.333), Inches(0.14))
    set_shape_flat(g15, COLOR_EMERALD)
    
    tag_close = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(0.52), Inches(4.8), Inches(0.4))
    set_shape_flat(tag_close, RGBColor(30, 41, 59), COLOR_EMERALD, 1.5)
    tf_tc = tag_close.text_frame
    tf_tc.margin_left = tf_tc.margin_top = tf_tc.margin_right = tf_tc.margin_bottom = 0
    ptc = tf_tc.paragraphs[0]
    ptc.alignment = PP_ALIGN.CENTER
    rtc = ptc.add_run("✦ KẾT LUẬN & LỜI CẢM ƠN — 15 / 15")
    rtc.font.name = FONT_BODY
    rtc.font.size = Pt(11.5)
    rtc.font.bold = True
    rtc.font.color.rgb = COLOR_EMERALD
    
    tb_c_title = s15.shapes.add_textbox(Inches(0.8), Inches(1.05), Inches(11.733), Inches(0.8))
    tf_ct = tb_c_title.text_frame
    tf_ct.word_wrap = True
    tf_ct.margin_left = tf_ct.margin_top = tf_ct.margin_right = tf_ct.margin_bottom = 0
    pct = tf_ct.paragraphs[0]
    rct = pct.add_run("Tổng kết đồ án & Trân trọng cảm ơn Thầy Cô")
    rct.font.name = FONT_TITLE
    rct.font.size = Pt(26) # INCREASED from 24pt
    rct.font.bold = True
    rct.font.color.rgb = COLOR_WHITE
    
    # Left Box
    c15_l = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.8), Inches(1.95), Inches(5.65), Inches(4.8))
    set_shape_flat(c15_l, RGBColor(30, 41, 59), RGBColor(71, 85, 105), 1.5)
    tb15_l = s15.shapes.add_textbox(Inches(1.05), Inches(2.15), Inches(5.15), Inches(4.4))
    tf15_l = tb15_l.text_frame
    tf15_l.word_wrap = True
    tf15_l.margin_left = tf15_l.margin_top = tf15_l.margin_right = tf15_l.margin_bottom = 0
    p = tf15_l.paragraphs[0]
    r = p.add_run("Thành Quả Đạt Được Của Đồ Án")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_CYAN
    
    achievements = [
        ("Hoàn thành xuất sắc mục tiêu: ", "Xây dựng ứng dụng web hoàn chỉnh, giải quyết triệt để hạn chế của Excel."),
        ("Làm chủ kiến trúc Clean Architecture 4 tầng: ", "Tầng Core Domain độc lập 0 dependency, dễ kiểm thử và bảo trì."),
        ("Toàn vẹn cơ sở dữ liệu quan hệ 3NF: ", "11 bảng thực thể, giao dịch Database Transaction bảo toàn tính nguyên tử ACID."),
        ("Bảo mật đa tầng tiêu chuẩn công nghiệp: ", "BCrypt hashing, RBAC 3 vai trò, phòng vệ chống lỗ hổng IDOR và URL 403."),
        ("Tích hợp AI & Xuất báo cáo tài chính: ", "Trợ lý AI giải đáp thắc mắc lương và xuất file Excel Mẫu số 02-LĐTL.")
    ]
    for a_title, a_desc in achievements:
        pa = tf15_l.add_paragraph()
        pa.space_before = Pt(8)
        ra1 = pa.add_run("✔ " + a_title)
        ra1.font.bold = True
        ra1.font.size = Pt(11.5)
        ra1.font.color.rgb = COLOR_WHITE
        ra2 = pa.add_run(a_desc)
        ra2.font.size = Pt(11)
        ra2.font.color.rgb = RGBColor(203, 213, 225)

    # Right Box
    c15_r = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(6.85), Inches(1.95), Inches(5.65), Inches(4.8))
    set_shape_flat(c15_r, RGBColor(30, 41, 59), RGBColor(71, 85, 105), 1.5)
    tb15_r = s15.shapes.add_textbox(Inches(7.1), Inches(2.15), Inches(5.15), Inches(4.4))
    tf15_r = tb15_r.text_frame
    tf15_r.word_wrap = True
    tf15_r.margin_left = tf15_r.margin_top = tf15_r.margin_right = tf15_r.margin_bottom = 0
    p = tf15_r.paragraphs[0]
    r = p.add_run("Lời Tri Ân & Phiên Hỏi Đáp (Q&A)")
    r.font.name = FONT_TITLE
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_AMBER
    
    thanks_text = [
        ("Lời cảm ơn chân thành: ", "Em xin bày tỏ lòng biết ơn sâu sắc tới Thầy TS. Hà Ngọc Long đã tận tâm hướng dẫn, định hướng kiến trúc sạch và phương pháp luận trong suốt học phần."),
        ("Khoa Hệ thống Thông tin Kinh tế: ", "Xin trân trọng cảm ơn quý Thầy, Cô giáo trong Khoa HTTT Kinh tế – Trường Đại học Kinh tế, Đại học Huế."),
        ("Sinh viên thực hiện: ", "Trần Viết Mạnh Cường — MSV: 23K4080003\nLớp: K57 Tin học Kinh tế — Khoa HTTT Kinh tế, ĐH Kinh tế Huế")
    ]
    for t_head, t_body in thanks_text:
        pt = tf15_r.add_paragraph()
        pt.space_before = Pt(8)
        rt1 = pt.add_run(t_head)
        rt1.font.bold = True
        rt1.font.size = Pt(11.5)
        rt1.font.color.rgb = COLOR_WHITE
        rt2 = pt.add_run(t_body)
        rt2.font.size = Pt(11)
        rt2.font.color.rgb = RGBColor(203, 213, 225)
        
    # Big Q&A Callout Badge (LARGER: 18pt)
    badge_qa = s15.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(7.35), Inches(5.25), Inches(4.65), Inches(1.25))
    set_shape_flat(badge_qa, COLOR_BLUE_ROYAL)
    tf_qa = badge_qa.text_frame
    tf_qa.word_wrap = True
    tf_qa.margin_left = tf_qa.margin_top = tf_qa.margin_right = tf_qa.margin_bottom = 0
    pqa1 = tf_qa.paragraphs[0]
    pqa1.alignment = PP_ALIGN.CENTER
    rqa1 = pqa1.add_run("XIN TRÂN TRỌNG CẢM ƠN!")
    rqa1.font.name = FONT_TITLE
    rqa1.font.size = Pt(17) # INCREASED from 15pt
    rqa1.font.bold = True
    rqa1.font.color.rgb = COLOR_WHITE
    pqa2 = tf_qa.add_paragraph()
    pqa2.alignment = PP_ALIGN.CENTER
    rqa2 = pqa2.add_run("Kính mong nhận được ý kiến đóng góp từ Thầy Cô & Hội đồng\n— Q&A SESSION —")
    rqa2.font.name = FONT_BODY
    rqa2.font.size = Pt(10.5)
    rqa2.font.color.rgb = COLOR_CYAN

    add_speaker_note(s15, "Kính thưa Thầy TS. Hà Ngọc Long cùng toàn thể quý Thầy Cô và các bạn sinh viên! Em xin trân trọng cảm ơn và kính mời Thầy Cô đặt câu hỏi thảo luận!")
    
    # -------------------------------------------------------------
    # APPLY 15 DYNAMIC, DIVERSE SLIDE TRANSITIONS
    # -------------------------------------------------------------
    slide_transitions = [
        '<p:fade/>',               # Slide 1: Fade mở màn êm dịu
        '<p:push dir="r"/>',        # Slide 2: Push từ phải sang
        '<p:wipe dir="r"/>',        # Slide 3: Wipe quét mượt
        '<p:split orient="horz"/>', # Slide 4: Split mở ngang
        '<p:comb orient="horz"/>',  # Slide 5: Comb chải lược công nghệ
        '<p:cover dir="r"/>',       # Slide 6: Cover bao phủ chuyển tầng
        '<p:wipe dir="r"/>',        # Slide 7: Wipe quét CSDL
        '<p:push dir="r"/>',        # Slide 8: Push trượt giao diện
        '<p:cover dir="r"/>',       # Slide 9: Cover chuyển bước quy trình
        '<p:fade/>',               # Slide 10: Fade tập trung đọc code
        '<p:zoom dir="in"/>',       # Slide 11: Zoom đột phá tính năng AI
        '<p:push dir="r"/>',        # Slide 12: Push trượt bảng kiểm thử
        '<p:split orient="vert"/>', # Slide 13: Split tách dọc so sánh
        '<p:wipe dir="r"/>',        # Slide 14: Wipe quét mở tương lai
        '<p:fade/>'                # Slide 15: Fade kết thúc trang trọng
    ]
    for idx, slide in enumerate(prs.slides):
        add_transition_to_slide(slide, slide_transitions[idx])

    # -------------------------------------------------------------
    # SAVE PPTX FILE
    # -------------------------------------------------------------
    output_filename = "Bai_trinh_bay_Lap_trinh_ung_dung_Web.pptx"
    prs.save(output_filename)
    print(f"Presentation saved successfully with LARGE FONTS and DIVERSE TRANSITIONS as: {output_filename}")

if __name__ == "__main__":
    build_presentation()
