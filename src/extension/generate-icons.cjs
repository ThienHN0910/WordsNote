const fs = require('fs');
const path = require('path');

// Minimal 1x1 purple PNG (base64)
const png1x1 = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==',
  'base64'
);

const iconsDir = path.join(__dirname, 'public', 'icons');
fs.mkdirSync(iconsDir, { recursive: true });
fs.writeFileSync(path.join(iconsDir, 'icon16.png'), png1x1);
fs.writeFileSync(path.join(iconsDir, 'icon48.png'), png1x1);
fs.writeFileSync(path.join(iconsDir, 'icon128.png'), png1x1);
console.log('Icons created!');
