import re
import os
import pptx
from pptx.oxml import parse_xml
from pptx.oxml.ns import nsdecls

font_mapping = {
    '8.5': '11.5',
    '8.8': '11.5',
    '9': '12',
    '9.2': '12',
    '9.3': '12.5',
    '9.5': '12.5',
    '9.8': '12.5',
    '10': '12.5',
    '10.5': '12.5',
    '11': '13.5',
    '11.5': '13.5',
    '12': '14',
    '13': '15',
    '13.5': '15.5',
    '14': '16.5',
    '15': '17.5',
    '16': '18',
    '17': '20',
    '18': '26',
    '20': '26',
    '24': '28',
    '26': '32',
    '28': '34',
    '32': '36'
}

# 15 distinct, verified transitions for 15-slide version
trans_15 = [
    '<p:fade/>',                           # Slide 1: Fade mờ dần sang trọng
    '<p:push dir="r"/>',                    # Slide 2: Push trượt ngang phải bối cảnh
    '<p:split orient="horz" dir="out"/>',   # Slide 3: Split mở ngang kiến trúc 4 tầng
    '<p:wipe dir="r"/>',                    # Slide 4: Wipe quét theo dòng dữ liệu Backend API
    '<p:comb orient="horz"/>',              # Slide 5: Comb chải lược hiện đại giao diện Blazor
    '<p:checker dir="horz"/>',              # Slide 6: Checker ô bàn cờ hiển thị cấu trúc CSDL
    '<p:circle/>',                          # Slide 7: Circle vòng tròn lan tỏa quy trình Chấm công
    '<p:wheel spokes="4"/>',                # Slide 8: Wheel 4 nan hoa xoay chu kỳ Tính lương
    '<p:blinds dir="vert"/>',               # Slide 9: Blinds rèm bảo mật phân quyền RBAC & JWT
    '<p:diamond/>',                         # Slide 10: Diamond kim cương công nghệ trợ lý AI
    '<p:zoom dir="in"/>',                   # Slide 11: Zoom phóng to trực quan màn hình demo
    '<p:plus/>',                            # Slide 12: Plus dấu cộng mở rộng kết quả kiểm thử
    '<p:cover dir="u"/>',                   # Slide 13: Cover bao phủ trượt lên so sánh vượt trội
    '<p:wedge/>',                           # Slide 14: Wedge quạt mở góc định hướng tương lai
    '<p:dissolve/>'                         # Slide 15: Dissolve hòa tan lắng đọng kết thúc & Q&A
]

# 12 distinct, verified transitions for 12-slide version
trans_12 = [
    '<p:fade/>',                           # Slide 1: Fade
    '<p:push dir="r"/>',                    # Slide 2: Push Right
    '<p:comb orient="horz"/>',              # Slide 3: Comb Horz
    '<p:split orient="horz" dir="out"/>',   # Slide 4: Split Horz Out
    '<p:checker dir="horz"/>',              # Slide 5: Checker Horz
    '<p:wipe dir="r"/>',                    # Slide 6: Wipe Right
    '<p:circle/>',                          # Slide 7: Circle
    '<p:wheel spokes="4"/>',                # Slide 8: Wheel 4 Spokes
    '<p:blinds dir="vert"/>',               # Slide 9: Blinds Vert
    '<p:diamond/>',                         # Slide 10: Diamond
    '<p:zoom dir="in"/>',                   # Slide 11: Zoom In
    '<p:dissolve/>'                         # Slide 12: Dissolve
]

def upgrade_code_file(filename, transitions):
    print(f"--- Upgrading {filename} ---")
    with open(filename, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Update font sizes
    def repl_font(match):
        old_val = match.group(1)
        new_val = font_mapping.get(old_val, old_val)
        return f'font.size = Pt({new_val})'

    content = re.sub(r'font\.size\s*=\s*Pt\((\d+(?:\.\d+)?)\)', repl_font, content)

    # 2. Update add_transition_to_slide to support all keywords and raw XML
    new_add_trans = '''def add_transition_to_slide(slide, trans_type='fade', spd='med', dir_val='r'):
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
        slide._element.append(trans_elem)'''

    content = re.sub(
        r'def add_transition_to_slide\(slide.*?(?=\ndef build_presentation)',
        new_add_trans + '\n',
        content,
        flags=re.DOTALL
    )

    # 3. Update the slide_transitions list in the file
    trans_formatted = ',\n        '.join([f"'{t}'" for t in transitions])
    trans_block = f'''    # -------------------------------------------------------------
    # APPLY 15 DYNAMIC, DIVERSE SLIDE TRANSITIONS (EACH SLIDE A UNIQUE EFFECT)
    # -------------------------------------------------------------
    slide_transitions = [
        {trans_formatted}
    ]
    for idx, slide in enumerate(prs.slides):
        add_transition_to_slide(slide, slide_transitions[idx])'''

    content = re.sub(
        r'# APPLY DIVERSE PROFESSIONAL SLIDE TRANSITIONS.*?(?=# SAVE PPTX FILE)',
        trans_block + '\n\n    ',
        content,
        flags=re.DOTALL
    )

    with open(filename, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"Updated {filename} successfully!")

if __name__ == '__main__':
    upgrade_code_file('generate_presentation.py', trans_15)
    upgrade_code_file('build_bang_cham_cong_pptx.py', trans_12)
