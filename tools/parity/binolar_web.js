/* Runs the REAL web sources (three.js included, no browser needed) and writes what
 * the Godot port must reproduce when a GLB is missing:
 *
 *   M,gen,i,x0,y0,z0,x1,y1,z1,hex,rough   every mesh of each binolar.js generator, in
 *                                         traversal order: world AABB and material
 *   T,gen,i,shakl,x,z,r,r1,r2,w,d,bur     its userData.tosiqlar footprint
 *   D,gen,draws                           8317 stream position after the generator
 *   C,nom,draws                           world stream checkpoints, no GLB at all
 *   B,draws                               8317 stream after that whole world
 *   W,i,shakl,x,z,r,r1,r2,w,d,bur         that world's dunyo.tosiqlar
 *
 * Usage: node tools/parity/binolar_web.js <web src dir> <out.csv>
 * Godot side: -- --glbsiz hammasi --binolar-compare <out.csv> (GameDirector.Tekshiruv.cs)
 */
'use strict';
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const [src, chiqish] = process.argv.slice(2);
if (!src || !chiqish) {
  console.error('usage: node binolar_web.js <web src dir> <out.csv>');
  process.exit(2);
}

globalThis.window = globalThis;
globalThis.location = { search: '' };

function yukla(nom, yamoq) {
  let s = fs.readFileSync(path.join(src, nom), 'utf8');
  if (yamoq) s = yamoq(s);
  vm.runInThisContext(s, { filename: nom });
}

function almash(s, a, b) {
  if (!s.includes(a)) throw new Error('patch target not found: ' + a);
  return s.replace(a, b);
}

// Only counters are added: rnd() still returns exactly what it returned.
function binolarYamoq(s) {
  return almash(s, 'function rnd() {', 'function rnd() { window.__b++;');
}

const satrlar = [];
const son = x => (Number.isFinite(x) ? String(x) : '0');

function tosiqSatri(t) {
  if (t.r1 !== undefined) return ['Halqa'].concat([t.x, t.z, 0, t.r1, t.r2, 0, 0, 0].map(son));
  if (t.r) return ['Aylana'].concat([t.x || 0, t.z || 0, t.r, 0, 0, 0, 0, 0].map(son));
  return ['Tortburchak'].concat([t.x || 0, t.z || 0, 0, 0, 0, t.w, t.d, t.burilish || 0].map(son));
}

// ---- part 1: the generators, one after the other on one stream ----
yukla('vendor/three.min.js');
yukla('js/balance.js');
yukla('js/models.js');
yukla('js/actors.js');
yukla('js/anim.js');
window.__b = 0;
yukla('js/binolar.js', binolarYamoq);

const generatorlar = [
  ['uy_kichik', () => window.BINOLAR.uy('kichik')],
  ['uy_katta', () => window.BINOLAR.uy('katta')],
  ['uy_vayrona', () => window.BINOLAR.uyVayrona()],
  ['bozor_rastasi', () => window.BINOLAR.bozorRastasi()],
  ['qala_darvoza', () => window.BINOLAR.qalaDarvoza()],
  ['shahar_devori', () => window.BINOLAR.shaharDevori()],
  ['ark_qalasi', () => window.BINOLAR.arkQalasi()],
  ['paxsa_devor', () => window.BINOLAR.paxsaDevor()],
];

for (const [nom, yasa] of generatorlar) {
  const g = yasa();
  g.updateMatrixWorld(true);
  let i = 0;
  g.traverse(o => {
    if (!o.isMesh) return;
    const q = new THREE.Box3().setFromObject(o);
    const c = o.material.color; // rounded: getHex() truncates r * 255
    const hex = (Math.round(c.r * 255) << 16 | Math.round(c.g * 255) << 8 | Math.round(c.b * 255)).toString(16);
    satrlar.push(['M', nom, i++, q.min.x, q.min.y, q.min.z, q.max.x, q.max.y, q.max.z, hex, o.material.roughness].join(','));
  });
  // bozorRastasi's footprint is never read (bozorYarat registers the platform itself)
  if (nom !== 'bozor_rastasi') {
    (g.userData.tosiqlar || []).forEach((t, k) =>
      satrlar.push(['T', nom, k].concat(tosiqSatri(t)).join(',')));
  }
  satrlar.push(['D', nom, window.__b].join(','));
}

// ---- part 2: the whole field with no GLB at all, on fresh streams ----
window.__b = 0;
yukla('js/binolar.js', binolarYamoq);
yukla('js/otlar.js');

window.__w = 0;
window.__qayd = nom => satrlar.push(['C', nom, window.__w].join(','));
yukla('js/world.js', s => {
  s = almash(s, 'function rnd() {', 'function rnd() { window.__w++;');
  s = almash(s, '/* ---- Teraklar ----', "window.__qayd('nodes');\n/* ---- Teraklar ----");
  s = almash(s, "// Terak tanalari", "window.__qayd('poplars');\n// Terak tanalari");
  s = almash(s, 'uylarQoy(scene, rnd, dunyo);', "uylarQoy(scene, rnd, dunyo); window.__qayd('houses');");
  s = almash(s, 'dunyo.darvoza = darvozaYarat(scene, B.DARVOZA_X, 0, dunyo);',
    "dunyo.darvoza = darvozaYarat(scene, B.DARVOZA_X, 0, dunyo); window.__qayd('gate');");
  s = almash(s, 'chekkalar(scene);', "chekkalar(scene); window.__qayd('edges');");
  return s;
});

const dunyo = window.WORLD.qur(new THREE.Scene());
satrlar.push(['B', window.__b].join(','));
dunyo.tosiqlar.forEach((t, i) => satrlar.push(['W', i].concat(tosiqSatri(t)).join(',')));

fs.writeFileSync(chiqish, satrlar.join('\n') + '\n');
console.log(`${satrlar.length} rows -> ${chiqish}`);
console.log(satrlar.filter(r => /^[DCB],/.test(r)).join('  '));
