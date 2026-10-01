import { copyFileSync, mkdirSync, readdirSync, readFileSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';
import { spawnSync } from 'child_process';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const specDir = join(root, 'spec');
const repoRoot = join(root, '..', '..');
const unitySpecsDir = join(repoRoot, 'Assets', 'Source', 'UI', 'Toolkit', '_specs');

const validate = spawnSync('node', ['scripts/validate-spec.mjs'], { cwd: root, stdio: 'inherit', shell: true });
if (validate.status !== 0) {
  console.error('[export] Validation failed.');
  process.exit(validate.status ?? 1);
}

mkdirSync(unitySpecsDir, { recursive: true });

const files = readdirSync(specDir).filter(f => f.endsWith('.screen.json'));
for (const file of files) {
  const src = join(specDir, file);
  const dest = join(unitySpecsDir, file);
  copyFileSync(src, dest);
  console.log(`[export] ${file} -> ${dest.replace(repoRoot, '')}`);
}

console.log('[export] Done.');
