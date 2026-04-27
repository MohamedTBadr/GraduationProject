const fs = require('fs');
const path = require('path');

function cssToTailwind(cssStr) {
    const classes = [];
    const styles = cssStr.split(';').map(s => s.trim()).filter(s => s);
    const unhandled = [];
    
    for (const style of styles) {
        if (!style.includes(':')) continue;
        const [key, valRaw] = style.split(/:(.+)/);
        const val = valRaw.trim();
        const k = key.trim();
        
        if (k === 'display' && val === 'flex') classes.push('flex');
        else if (k === 'display' && val === 'grid') classes.push('grid');
        else if (k === 'display' && val === 'none') classes.push('hidden');
        else if (k === 'display' && val === 'block') classes.push('block');
        else if (k === 'display' && val === 'inline-block') classes.push('inline-block');
        else if (k === 'align-items' && val === 'center') classes.push('items-center');
        else if (k === 'align-items' && val === 'flex-start') classes.push('items-start');
        else if (k === 'align-items' && val === 'flex-end') classes.push('items-end');
        else if (k === 'justify-content' && val === 'center') classes.push('justify-center');
        else if (k === 'justify-content' && val === 'space-between') classes.push('justify-between');
        else if (k === 'justify-content' && val === 'flex-start') classes.push('justify-start');
        else if (k === 'justify-content' && val === 'flex-end') classes.push('justify-end');
        else if (k === 'flex-direction' && val === 'column') classes.push('flex-col');
        else if (k === 'flex-shrink' && val === '0') classes.push('shrink-0');
        else if (k === 'flex' && val === '1') classes.push('flex-1');
        else if (k === 'cursor' && val === 'pointer') classes.push('cursor-pointer');
        else if (k === 'font-weight' && val === 'bold') classes.push('font-bold');
        else if (k === 'font-weight' && val === '700') classes.push('font-bold');
        else if (k === 'font-weight' && val === '600') classes.push('font-semibold');
        else if (k === 'font-weight' && val === '500') classes.push('font-medium');
        else if (k === 'text-align' && val === 'center') classes.push('text-center');
        else if (k === 'text-align' && val === 'left') classes.push('text-left');
        else if (k === 'text-align' && val === 'right') classes.push('text-right');
        else if (k === 'object-fit' && val === 'cover') classes.push('object-cover');
        else if (k === 'overflow' && val === 'hidden') classes.push('overflow-hidden');
        else if (k === 'overflow-y' && val === 'auto') classes.push('overflow-y-auto');
        else if (k === 'position' && val === 'relative') classes.push('relative');
        else if (k === 'position' && val === 'absolute') classes.push('absolute');
        else if (k === 'position' && val === 'sticky') classes.push('sticky');
        
        else if (k === 'gap') {
            if (val.endsWith('px') || val.endsWith('rem') || val.endsWith('em')) {
                classes.push(`gap-[${val}]`);
            } else {
                unhandled.push(style);
            }
        }
        else if (k === 'width') classes.push(`w-[${val}]`);
        else if (k === 'height') classes.push(`h-[${val}]`);
        else if (k === 'min-height') classes.push(`min-h-[${val}]`);
        else if (k === 'max-width') classes.push(`max-w-[${val}]`);
        else if (k === 'margin-top') classes.push(`mt-[${val}]`);
        else if (k === 'margin-bottom') classes.push(`mb-[${val}]`);
        else if (k === 'margin-left') classes.push(`ml-[${val}]`);
        else if (k === 'margin-right') classes.push(`mr-[${val}]`);
        else if (k === 'margin' && val.split(' ').length === 1) classes.push(`m-[${val}]`);
        else if (k === 'margin') {
            const parts = val.split(' ');
            if (parts.length === 2) classes.push(`my-[${parts[0]}] mx-[${parts[1]}]`);
            else unhandled.push(style);
        }
        else if (k === 'padding-top') classes.push(`pt-[${val}]`);
        else if (k === 'padding-bottom') classes.push(`pb-[${val}]`);
        else if (k === 'padding-left') classes.push(`pl-[${val}]`);
        else if (k === 'padding-right') classes.push(`pr-[${val}]`);
        else if (k === 'padding' && val.split(' ').length === 1) classes.push(`p-[${val}]`);
        else if (k === 'padding') {
            const parts = val.split(' ');
            if (parts.length === 2) classes.push(`py-[${parts[0]}] px-[${parts[1]}]`);
            else unhandled.push(style);
        }
        else if (k === 'font-size') classes.push(`text-[${val}]`);
        else if (k === 'color') classes.push(`text-[${val.replace(/ /g, '')}]`);
        else if (k === 'background' || k === 'background-color') classes.push(`bg-[${val.replace(/ /g, '_')}]`);
        else if (k === 'border-radius') classes.push(`rounded-[${val}]`);
        else if (k === 'border') classes.push(`[border:${val.replace(/ /g, '_')}]`);
        else if (k === 'border-bottom') classes.push(`[border-bottom:${val.replace(/ /g, '_')}]`);
        else if (k === 'box-shadow') classes.push(`[box-shadow:${val.replace(/ /g, '_')}]`);
        else if (k === 'transition') classes.push(`[transition:${val.replace(/ /g, '_')}]`);
        else if (k === 'grid-template-columns') classes.push(`[grid-template-columns:${val.replace(/ /g, '_')}]`);
        else unhandled.push(style);
    }
    return { classes, unhandled };
}

function processFile(filepath) {
    let content = fs.readFileSync(filepath, 'utf-8');
    let modified = false;

    // Replace style="..."
    const newContent = content.replace(/(<[^>]+?)\bstyle="([^"]+)"([^>]*>)/g, (match, prefix, styleContent, suffix) => {
        const { classes, unhandled } = cssToTailwind(styleContent);
        
        let result = prefix;
        if (classes.length > 0) {
            result += ` __TW_INJECT__="${classes.join(' ')}"`;
        }
        if (unhandled.length > 0) {
            result += ` style="${unhandled.join('; ')}"`;
        }
        result += suffix;
        modified = true;
        return result;
    });

    // Merge __TW_INJECT__ into class="..."
    const finalContent = newContent.replace(/<[^>]+__TW_INJECT__[^>]+>/g, (tag) => {
        const twMatch = tag.match(/__TW_INJECT__="([^"]+)"/);
        if (!twMatch) return tag;
        
        const twClasses = twMatch[1];
        tag = tag.replace(twMatch[0], '');
        
        const classMatch = tag.match(/class="([^"]+)"/);
        if (classMatch) {
            const oldClasses = classMatch[1];
            tag = tag.replace(classMatch[0], `class="${oldClasses} ${twClasses}"`);
        } else {
            const parts = tag.split(' ');
            if (parts.length >= 2) {
                tag = `${parts[0]} class="${twClasses}" ${parts.slice(1).join(' ')}`;
            } else {
                tag = `${tag.slice(0, -1)} class="${twClasses}">`;
            }
        }
        return tag.replace(/\s+/g, ' ').replace(' >', '>');
    });

    if (finalContent !== content) {
        fs.writeFileSync(filepath, finalContent, 'utf-8');
        console.log(`Updated ${filepath}`);
    }
}

function walkDir(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            walkDir(fullPath);
        } else if (fullPath.endsWith('.html')) {
            processFile(fullPath);
        }
    }
}

walkDir('src');
