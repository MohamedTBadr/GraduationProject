import os
import re

def css_to_tailwind(css_str):
    classes = []
    styles = [s.strip() for s in css_str.split(';') if s.strip()]
    
    unhandled = []
    
    for style in styles:
        if ':' not in style:
            continue
        key, val = [p.strip() for p in style.split(':', 1)]
        
        if key == 'display' and val == 'flex': classes.append('flex')
        elif key == 'display' and val == 'grid': classes.append('grid')
        elif key == 'display' and val == 'none': classes.append('hidden')
        elif key == 'display' and val == 'block': classes.append('block')
        elif key == 'display' and val == 'inline-block': classes.append('inline-block')
        elif key == 'align-items' and val == 'center': classes.append('items-center')
        elif key == 'align-items' and val == 'flex-start': classes.append('items-start')
        elif key == 'align-items' and val == 'flex-end': classes.append('items-end')
        elif key == 'justify-content' and val == 'center': classes.append('justify-center')
        elif key == 'justify-content' and val == 'space-between': classes.append('justify-between')
        elif key == 'justify-content' and val == 'flex-start': classes.append('justify-start')
        elif key == 'justify-content' and val == 'flex-end': classes.append('justify-end')
        elif key == 'flex-direction' and val == 'column': classes.append('flex-col')
        elif key == 'flex-shrink' and val == '0': classes.append('shrink-0')
        elif key == 'flex' and val == '1': classes.append('flex-1')
        elif key == 'cursor' and val == 'pointer': classes.append('cursor-pointer')
        elif key == 'font-weight' and val == 'bold': classes.append('font-bold')
        elif key == 'font-weight' and val == '700': classes.append('font-bold')
        elif key == 'font-weight' and val == '600': classes.append('font-semibold')
        elif key == 'font-weight' and val == '500': classes.append('font-medium')
        elif key == 'text-align' and val == 'center': classes.append('text-center')
        elif key == 'text-align' and val == 'left': classes.append('text-left')
        elif key == 'text-align' and val == 'right': classes.append('text-right')
        elif key == 'object-fit' and val == 'cover': classes.append('object-cover')
        elif key == 'overflow' and val == 'hidden': classes.append('overflow-hidden')
        elif key == 'overflow-y' and val == 'auto': classes.append('overflow-y-auto')
        elif key == 'position' and val == 'relative': classes.append('relative')
        elif key == 'position' and val == 'absolute': classes.append('absolute')
        elif key == 'position' and val == 'sticky': classes.append('sticky')
        
        elif key == 'gap':
            if val.endswith('px') or val.endswith('rem') or val.endswith('em'):
                classes.append(f'gap-[{val}]')
            else:
                unhandled.append(style)
        elif key == 'width': classes.append(f'w-[{val}]')
        elif key == 'height': classes.append(f'h-[{val}]')
        elif key == 'min-height': classes.append(f'min-h-[{val}]')
        elif key == 'max-width': classes.append(f'max-w-[{val}]')
        elif key == 'margin-top': classes.append(f'mt-[{val}]')
        elif key == 'margin-bottom': classes.append(f'mb-[{val}]')
        elif key == 'margin-left': classes.append(f'ml-[{val}]')
        elif key == 'margin-right': classes.append(f'mr-[{val}]')
        elif key == 'margin' and len(val.split()) == 1: classes.append(f'm-[{val}]')
        elif key == 'margin':
            parts = val.split()
            if len(parts) == 2:
                classes.append(f'my-[{parts[0]}] mx-[{parts[1]}]')
            else:
                unhandled.append(style)
        elif key == 'padding-top': classes.append(f'pt-[{val}]')
        elif key == 'padding-bottom': classes.append(f'pb-[{val}]')
        elif key == 'padding-left': classes.append(f'pl-[{val}]')
        elif key == 'padding-right': classes.append(f'pr-[{val}]')
        elif key == 'padding' and len(val.split()) == 1: classes.append(f'p-[{val}]')
        elif key == 'padding':
            parts = val.split()
            if len(parts) == 2:
                classes.append(f'py-[{parts[0]}] px-[{parts[1]}]')
            else:
                unhandled.append(style)
        elif key == 'font-size': classes.append(f'text-[{val}]')
        elif key == 'color': classes.append(f'text-[{val.replace(" ", "")}]')
        elif key == 'background': classes.append(f'bg-[{val.replace(" ", "")}]')
        elif key == 'background-color': classes.append(f'bg-[{val.replace(" ", "")}]')
        elif key == 'border-radius': classes.append(f'rounded-[{val}]')
        elif key == 'border':
            classes.append(f'[border:{val.replace(" ", "_")}]')
        elif key == 'border-bottom':
            classes.append(f'[border-bottom:{val.replace(" ", "_")}]')
        elif key == 'box-shadow':
            classes.append(f'[box-shadow:{val.replace(" ", "_")}]')
        elif key == 'transition':
            classes.append(f'[transition:{val.replace(" ", "_")}]')
        elif key == 'grid-template-columns':
            classes.append(f'[grid-template-columns:{val.replace(" ", "_")}]')
        else:
            unhandled.append(style)
            
    return classes, unhandled

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    def replacer(match):
        prefix = match.group(1)
        style_content = match.group(2)
        suffix = match.group(3)
        
        tw_classes, unhandled = css_to_tailwind(style_content)
        
        result = prefix
        if tw_classes:
            result += f' __TW_INJECT__="{" ".join(tw_classes)}"'
            
        if unhandled:
            result += f' style="{"; ".join(unhandled)}"'
            
        result += suffix
        return result

    new_content = re.sub(r'(<[^>]+?)\bstyle="([^"]+)"([^>]*>)', replacer, content)
    
    def merge_class(match):
        tag = match.group(0)
        tw_match = re.search(r'__TW_INJECT__="([^"]+)"', tag)
        if not tw_match: return tag
        
        tw_classes = tw_match.group(1)
        tag = tag.replace(tw_match.group(0), '')
        
        class_match = re.search(r'class="([^"]+)"', tag)
        if class_match:
            old_classes = class_match.group(1)
            tag = tag.replace(class_match.group(0), f'class="{old_classes} {tw_classes}"')
        else:
            parts = tag.split(' ', 1)
            if len(parts) == 2:
                tag = f'{parts[0]} class="{tw_classes}" {parts[1]}'
            else:
                tag = f'{tag[:-1]} class="{tw_classes}">'
                
        tag = re.sub(r'\s+', ' ', tag).replace(' >', '>')
        return tag
        
    new_content = re.sub(r'<[^>]+__TW_INJECT__[^>]+>', merge_class, new_content)
    
    if new_content != content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
            
    return True

for root, _, files in os.walk('src'):
    for file in files:
        if file.endswith('.html'):
            filepath = os.path.join(root, file)
            process_file(filepath)
