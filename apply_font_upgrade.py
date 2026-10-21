import re
import os

font_mapping = {
    '8.5': '10.5',
    '8.8': '10.5',
    '9': '10.5',
    '9.2': '10.5',
    '9.5': '11.5',
    '9.8': '11.5',
    '10': '12',
    '10.5': '12',
    '11': '13',
    '11.5': '13',
    '12': '14',
    '13': '15',
    '13.5': '15',
    '14': '16',
    '15': '17',
    '18': '24',
    '20': '24',
    '24': '26',
    '28': '32'
}

def upgrade_file(filename, transitions_list):
    with open(filename, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replace font sizes
    def repl_font(match):
        old_val = match.group(1)
        new_val = font_mapping.get(old_val, old_val)
        return f'font.size = Pt({new_val})'

    content_up = re.sub(r'font\.size\s*=\s*Pt\((\d+(?:\.\d+)?)\)', repl_font, content)

    # Replace transitions block with rich transitions
    trans_code = f"""    # -------------------------------------------------------------
    # APPLY DIVERSE PROFESSIONAL SLIDE TRANSITIONS
    # -------------------------------------------------------------
    slide_transitions = {transitions_list}
    for idx, slide in enumerate(prs.slides):
        add_transition_to_slide(slide, slide_transitions[idx])"""

    content_up = re.sub(
        r'# APPLY SMOOTH PROFESSIONAL SLIDE TRANSITIONS.*?(?=# SAVE PPTX FILE)',
        trans_code + '\n\n    ',
        content_up,
        flags=re.DOTALL
    )

    with open(filename, 'w', encoding='utf-8') as f:
        f.write(content_up)
    print(f"Upgraded {filename} successfully!")

# Transitions for 15 slides
trans_15 = [
    '<p:fade/>',               # Slide 1: Fade
    '<p:push dir="r"/>',        # Slide 2: Push
    '<p:wipe dir="r"/>',        # Slide 3: Wipe
    '<p:split orient="horz"/>', # Slide 4: Split
    '<p:comb orient="horz"/>',  # Slide 5: Comb
    '<p:cover dir="r"/>',       # Slide 6: Cover
    '<p:wipe dir="r"/>',        # Slide 7: Wipe
    '<p:push dir="r"/>',        # Slide 8: Push
    '<p:cover dir="r"/>',       # Slide 9: Cover
    '<p:fade/>',               # Slide 10: Fade
    '<p:zoom dir="in"/>',       # Slide 11: Zoom
    '<p:push dir="r"/>',        # Slide 12: Push
    '<p:split orient="vert"/>', # Slide 13: Split
    '<p:wipe dir="r"/>',        # Slide 14: Wipe
    '<p:fade/>'                # Slide 15: Fade
]

# Transitions for 12 slides
trans_12 = [
    '<p:fade/>',               # Slide 1: Fade
    '<p:push dir="r"/>',        # Slide 2: Push
    '<p:wipe dir="r"/>',        # Slide 3: Wipe
    '<p:comb orient="horz"/>',  # Slide 4: Comb
    '<p:split orient="horz"/>', # Slide 5: Split
    '<p:cover dir="r"/>',       # Slide 6: Cover
    '<p:push dir="r"/>',        # Slide 7: Push
    '<p:cover dir="r"/>',       # Slide 8: Cover
    '<p:wipe dir="r"/>',        # Slide 9: Wipe
    '<p:push dir="r"/>',        # Slide 10: Push
    '<p:split orient="vert"/>', # Slide 11: Split
    '<p:fade/>'                # Slide 12: Fade
]

upgrade_file('generate_presentation.py', trans_15)
upgrade_file('build_bang_cham_cong_pptx.py', trans_12)
