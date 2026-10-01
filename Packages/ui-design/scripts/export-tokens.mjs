import { writeFileSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const outPath = join(__dirname, '..', '..', '..', 'Assets', 'Source', 'UI', 'Toolkit', 'Theme', 'design-tokens.pxlkit-snippet.uss');

const snippet = `:root {
    --retro-green: rgb(92, 214, 122);
    --retro-cyan: rgb(110, 198, 255);
    --retro-gold: rgb(255, 223, 88);
    --retro-red: rgb(255, 108, 108);
    --retro-purple: rgb(58, 42, 96);
    --retro-pink: rgb(255, 130, 178);
    --retro-bg: rgb(34, 20, 72);
    --retro-surface: rgb(58, 42, 96);
    --retro-border-light: rgb(255, 230, 150);
    --retro-border-dark: rgb(35, 22, 55);
}
`;

writeFileSync(outPath, snippet, 'utf8');
console.log(`[export:tokens] Wrote ${outPath}`);
