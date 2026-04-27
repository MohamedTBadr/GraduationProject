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
        
        if (k === 'cursor' && val === 'not-allowed') classes.push('cursor-not-allowed');
        else if (k === 'text-decoration' && val === 'none') classes.push('no-underline');
        else if (k === 'text-decoration' && val === 'underline') classes.push('underline');
        else if (k === 'appearance' && val === 'auto') classes.push('appearance-auto');
        else if (k === 'appearance' && val === 'none') classes.push('appearance-none');
        else if (k === 'text-transform' && val === 'capitalize') classes.push('capitalize');
        else if (k === 'text-transform' && val === 'uppercase') classes.push('uppercase');
        else if (k === 'text-transform' && val === 'lowercase') classes.push('lowercase');
        else if (k === 'white-space' && val === 'nowrap') classes.push('whitespace-nowrap');
        else if (k === 'text-overflow' && val === 'ellipsis') classes.push('text-ellipsis');
        else if (k === 'flex-wrap' && val === 'wrap') classes.push('flex-wrap');
        else if (k === 'flex-wrap' && val === 'nowrap') classes.push('flex-nowrap');
        else if (k === 'user-select' && val === 'none') classes.push('select-none');
        else if (k === 'line-height') classes.push(`leading-[${val.replace(/ /g, '_')}]`);
        else if (k === 'letter-spacing') classes.push(`tracking-[${val.replace(/ /g, '_')}]`);
        else if (k === 'font-style' && val === 'italic') classes.push('italic');
        else if (k === 'font-family') classes.push(`font-[${val.replace(/ /g, '_')}]`);
        else if (k === 'top') classes.push(`top-[${val}]`);
        else if (k === 'bottom') classes.push(`bottom-[${val}]`);
        else if (k === 'left') classes.push(`left-[${val}]`);
        else if (k === 'right') classes.push(`right-[${val}]`);
        else if (k === 'flex') classes.push(`[flex:${val.replace(/ /g, '_')}]`);
        else if (k === 'border-color') classes.push(`border-[${val.replace(/ /g, '_')}]`);
        else if (k === 'border-left') classes.push(`[border-left:${val.replace(/ /g, '_')}]`);
        else if (k === 'border-right') classes.push(`[border-right:${val.replace(/ /g, '_')}]`);
        else if (k === 'border-top') classes.push(`[border-top:${val.replace(/ /g, '_')}]`);
        else if (k === 'z-index') classes.push(`z-[${val}]`);
        else if (k === 'grid-column') classes.push(`[grid-column:${val.replace(/ /g, '_')}]`);
        else if (k === 'min-width') classes.push(`min-w-[${val}]`);
        else if (k === 'max-height') classes.push(`max-h-[${val}]`);
        else unhandled.push(style);
    }
    return { classes, unhandled };
}

function processFile(filepath) {
    let content = fs.readFileSync(filepath, 'utf-8');
    let modified = false;

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
