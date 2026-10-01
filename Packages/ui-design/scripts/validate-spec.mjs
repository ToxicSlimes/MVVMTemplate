import Ajv2020 from 'ajv/dist/2020.js';
import addFormats from 'ajv-formats';
import { readFileSync, readdirSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const specDir = join(__dirname, '..', 'spec');
const schemaPath = join(specDir, 'screen-spec.schema.json');

const schema = JSON.parse(readFileSync(schemaPath, 'utf8'));
const ajv = new Ajv2020({ allErrors: true, strict: false });
addFormats(ajv);
const validate = ajv.compile(schema);

const files = readdirSync(specDir).filter(f => f.endsWith('.screen.json'));
let failed = false;

for (const file of files) {
  const path = join(specDir, file);
  const data = JSON.parse(readFileSync(path, 'utf8'));
  if (!validate(data)) {
    console.error(`[validate] ${file}:`);
    for (const err of validate.errors ?? [])
      console.error(`  ${err.instancePath} ${err.message}`);
    failed = true;
  } else {
    console.log(`[validate] OK ${file}`);
  }
}

if (failed) process.exit(1);
