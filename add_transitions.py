import os
import pptx
from pptx.oxml import parse_xml
from pptx.oxml.ns import nsdecls

def add_transition_to_slide(slide, trans_type='fade', spd='med', dir_val='r'):
    """
    Safely adds OpenXML slide transition to a PowerPoint slide.
    Valid OpenXML PresentationML sequence for CT_Slide:
    (cSld, clrMapOvr?, transition?, timing?, extLst?)
    """
    # Remove existing transition if any
    for child in list(slide._element):
        if child.tag.endswith('transition'):
            slide._element.remove(child)
            
    if trans_type == 'fade':
        inner = '<p:fade/>'
    elif trans_type == 'push':
        # In OpenXML, dir="r" pushes from right to left, dir="l" pushes from left to right,
        # dir="u" pushes up, dir="d" pushes down.
        inner = f'<p:push dir="{dir_val}"/>'
    elif trans_type == 'wipe':
        inner = f'<p:wipe dir="{dir_val}"/>'
    elif trans_type == 'cover':
        inner = f'<p:cover dir="{dir_val}"/>'
    elif trans_type == 'split':
        inner = '<p:split orient="horz"/>'
    else:
        inner = '<p:fade/>'
        
    xml_str = f'<p:transition {nsdecls("p")} spd="{spd}" advClick="1">{inner}</p:transition>'
    trans_elem = parse_xml(xml_str)
    
    # Insert in schema-compliant position
    inserted = False
    for idx, child in enumerate(slide._element):
        if child.tag.endswith('timing') or child.tag.endswith('extLst'):
            slide._element.insert(idx, trans_elem)
            inserted = True
            break
    if not inserted:
        slide._element.append(trans_elem)

def apply_transitions(pptx_path):
    print(f"Loading presentation: {pptx_path}...")
    prs = pptx.Presentation(pptx_path)
    total = len(prs.slides)
    print(f"Total slides found: {total}")
    
    for idx, slide in enumerate(prs.slides):
        slide_num = idx + 1
        if slide_num == 1:
            # Slide 1 (Bìa): Fade mờ dần sang trọng khi bắt đầu
            add_transition_to_slide(slide, trans_type='fade', spd='med')
            print(f"Slide {slide_num:02d}: Applied 'Fade' transition")
        elif slide_num == total:
            # Slide cuối (Kết luận & Q&A): Fade mờ dần êm ái khi kết thúc
            add_transition_to_slide(slide, trans_type='fade', spd='med')
            print(f"Slide {slide_num:02d}: Applied 'Fade' transition")
        else:
            # Các slide nội dung (Slide 2 - 14): Push trượt ngang mượt mà (chuyển tiếp hiện đại)
            add_transition_to_slide(slide, trans_type='push', spd='med', dir_val='r')
            print(f"Slide {slide_num:02d}: Applied 'Push' (from right) transition")
            
    prs.save(pptx_path)
    print(f"Successfully updated transitions for: {pptx_path}")

if __name__ == "__main__":
    target = os.path.abspath("Bai_trinh_bay_Lap_trinh_ung_dung_Web.pptx")
    apply_transitions(target)
    
    # Also apply to the 12-slide version as well for convenience
    target2 = os.path.abspath("Bao_cao_Lap_trinh_Ung_dung_Web_Bang_Cham_Cong.pptx")
    if os.path.exists(target2):
        apply_transitions(target2)
