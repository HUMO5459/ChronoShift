# AI asset quvuri — 1-davr: 1850–1900

Birinchi davr MVP'si uchun asset ro'yxati, Meshy promtlari, import qoidalari va xarajat hisobi.

---

## 1. Qaysi vosita nimani qiladi

Meshy **faqat 3D model** yasaydi. Rejangizdagi qolgan uchta toifa boshqa vositalarni talab qiladi:

| Toifa | Meshy | To'g'ri vosita |
|---|---|---|
| 3D obyektlar, qurilmalar, qurollar | ✅ asosiy ish | Meshy (text→3D, image→3D) |
| Personajlar (muhandis, askar) | ⚠️ zaif | Character Creator / marketplace / Mixamo rig |
| Ovozlar | ❌ | ElevenLabs SFX, yoki CC0: freesound.org |
| Effektlar (bug', tutun, uchqun) | ❌ | Godot `GPUParticles3D` — qo'lda, AI emas |
| 2D chizmalar (blueprint, ikona) | ❌ | Midjourney / DALL·E / Stable Diffusion |
| Animatsiya | ❌ | Mixamo (bepul), yoki qo'lda |

**Muhim:** o'yinning UI'si allaqachon **blueprint (ko'k chizma)** uslubida — bu 1850–1900 davriga tabiiy mos tushadi. 2D tomonda deyarli ish qolmagan.

---

## 2. Uslub langari (eng muhim qism)

AI asset yasashda eng ko'p uchraydigan xato — **har bir obyekt boshqa uslubda chiqishi**. Buning oldini olish uchun quyidagi matnni **har bir promt oxiriga o'zgartirmasdan** qo'shing:

```
STYLE: game-ready low-poly, stylised realism, matte PBR textures,
muted earth palette (weathered wood, oxidised iron, brass, canvas),
soft even lighting, no text, no logos, single object, centred,
neutral grey background, clean topology
```

Nega: bitta obyekt ajralib turishi butun sahnani buzadi. Loyihada bu allaqachon bo'lgan — Renderpeople personaji (fotorealistik) qolgan past-poli muhitdan ajralib turadi.

**Poli chegarasi:** loyiha `gl_compatibility` renderer'ida, zaif GPU uchun. O'lchangan: hub 662k uchburchak, karta 615k. Har bir asset uchun **8k uchburchakdan oshmasin** — Meshy'da "Low Poly" / target polycount sozlamasidan foydalaning.

---

## 3. Promt paketi — 1850–1900

Har birini alohida generatsiya qiling. Oxiriga §2 dagi STYLE blokini qo'shing.

### 3.1 Resurs konlari (4 ta) — o'yinchi shulardan material yig'adi

```
1. A stack of freshly cut pine logs, bark still on, tied with rope, resting on
   uneven ground, 19th century logging site
2. An exposed iron ore outcrop, rusty red-brown rock with jagged broken faces
   and loose ore chunks at the base
3. A heap of black coal lumps with a wooden shovel leaning against it,
   19th century mining yard
4. A quarried limestone block pile, rough chisel marks on the faces,
   pale grey stone, some blocks cracked
```

### 3.2 Muhandislik vazifalari (12 ta) — bular o'yinning **asosiy mazmuni**

```
5.  A wooden pontoon bridge section, flat plank deck on two canvas-covered
    boats, iron cleats and rope lashings, 1860s military engineering
6.  A gabion: cylindrical wicker basket woven from willow branches, packed
    with earth and stones, 19th century field fortification
7.  A fascine: tight bundle of long straight branches bound with three iron
    wire ties, used to revet trenches
8.  A cheval de frise: heavy timber beam pierced by crossed sharpened wooden
    stakes, anti-cavalry obstacle
9.  A portable field telegraph station: polished wooden box, brass Morse key,
    glass battery jars, coiled copper wire, open lid, 1870s
10. A creosoted wooden telegraph pole with a crossarm carrying four white
    ceramic insulators and taut wire stubs
11. A wooden gunpowder keg, iron hoops, stencil-free, with a coiled fuse cord
    resting on the lid
12. A portable field forge: iron firebox on a wheeled frame, leather bellows,
    small anvil bolted to the side, 19th century army blacksmith
13. A cast iron hand water pump on a timber base, long curved handle,
    weathered green paint, spout with drip stain
14. A section of narrow gauge railway track: two steel rails on eight creosoted
    wooden sleepers, iron spikes and fishplates
15. A railway handcar: wooden platform on four steel wheels with a central
    see-saw pump handle, 1880s
16. A wooden observation tower: four braced legs, ladder, small railed
    platform on top, rough sawn timber
```

### 3.3 Texnika va mashinalar (4 ta)

```
17. A steam traction engine: large rear iron wheels, tall boiler with brass
    fittings, chimney, canopy over the driver platform, 1890s agricultural type
18. A horse-drawn military supply wagon: wooden bed with canvas hoops,
    iron-rimmed spoked wheels, brake lever, no horses
19. A muzzle-loading field cannon on a wooden two-wheeled carriage,
    bronze barrel, iron trail spade, 1860s artillery
20. A hand-cranked Gatling gun on a two-wheeled carriage, six brass barrels,
    top-mounted gravity magazine, 1870s
```

### 3.4 Qurollar (3 ta) — o'yinchi qo'lida

```
21. A percussion cap revolver, blued steel frame, walnut grip, octagonal
    barrel, 1851 pattern, side view, isolated
22. A single-shot military rifle with wooden stock, iron barrel bands,
    ramrod under the barrel, 1860s infantry musket
23. A military engineer's felling axe: broad steel head, straight hickory
    handle, leather sheath on the blade
```

### 3.5 Muhit (8 ta)

```
24. A stack of filled hessian sandbags, three rows, sagging and dusty,
    field fortification
25. A timber revetment wall: vertical rough planks held by horizontal beams
    and iron stakes, holding back earth
26. A wooden military supply crate, rope handles, iron corner brackets,
    lid closed
27. An oak barrel with iron hoops, standing upright, weathered staves
28. A canvas A-frame army tent, guy ropes and wooden pegs, closed flap,
    19th century campaign tent
29. An earthwork redoubt section: sloped rammed-earth parapet with a firing
    step and timber facing
30. A cast iron brazier on three legs with burnt logs inside, no fire
31. A broken wooden cart wheel lying on the ground, several spokes missing,
    iron rim rusted
```

### 3.6 Personajlar (2 ta) — ⚠️ Meshy bilan emas

AI 3D personajlarda eng zaif. Tavsiya: **Mixamo** (bepul rig + animatsiya) yoki marketplace modeli, ustiga davrga mos kiyim. Meshy'ga urinib ko'rmoqchi bo'lsangiz:

```
32. A standing male military engineer in a dark blue 1860s uniform tunic with
    brass buttons, peaked forage cap, leather belt and tool satchel,
    T-pose, arms out, symmetrical, full body
33. A standing infantry soldier in a grey 1870s greatcoat, kepi, ammunition
    pouch, T-pose, arms out, symmetrical, full body
```

---

## 4. Godot'ga import qoidalari

Bu loyihada bu masalada **allaqachon qimmatga tushgan darslar** bor:

1. **Format:** Meshy'dan `.glb` yuklab oling (`.fbx` emas). GLB'da Y-up va material to'g'ri keladi.
2. **O'lcham:** AI modellar ixtiyoriy o'lchamda chiqadi. Godot'ga qo'yishdan oldin **AABB'ni o'lchang**, `MeshInstance3D` bo'yicha filtrlab — model ichidagi `Light3D`/`Camera3D` o'lchovni buzadi (bu xato bu loyihada pistoletni 2×2×2 m qilib ko'rsatgan).
3. **Yo'nalish:** oldinga qaragan tomon **−Z** bo'lishi kerak. Ko'p model 90° burilgan chiqadi — `MeshRoot/BodyOrient` tugunida tuzating, modelning o'ziga tegmang.
4. **Masofa chegarasi:** har bir yangi mesh'ga `DistanceCull.ApplyTo(...)` qo'llang. Loyihada daraxtzor butun byudjetning 54% ini yeb qo'ygan edi.
5. **Nomlash:** `assets/models/<toifa>/<obyekt>/` — mavjud tuzilishga mos.

---

## 5. Xarajat hisobi — 1-davr MVP

**33 asset, 3–4 missiya** uchun.

### 5.1 Pul

| Modda | Hisob | Narx |
|---|---|---|
| Meshy obuna | 33 asset × ~4 urinish = ~130 generatsiya. Pro rejasi bir oyga yetadi, tor bo'lsa 2 oy | **$20–60** |
| Personaj (2 ta) | Mixamo bepul; marketplace kerak bo'lsa | **$0–80** |
| Ovozlar (~20 ta) | freesound CC0 = $0; ElevenLabs SFX oylik | **$0–25** |
| 2D (ixtiyoriy) | UI tayyor; faqat missiya illyustratsiyasi kerak bo'lsa | **$0–30** |
| Animatsiya | Mixamo | **$0** |
| **Jami naqd** | | **$20–195** |

> ⚠️ Narxlar mening ma'lumotim kesilgan sanaga oid — buyurtmadan oldin joriy tarifni tekshiring.

**Pul bu yerda muammo emas.** Asosiy xarajat — vaqt.

### 5.2 Vaqt (haqiqiy raqam)

| Ish | Baho |
|---|---|
| 33 asset: generatsiya + qayta urinish + tanlash | 25–40 soat |
| Har biriga: o'lcham/yo'nalish tuzatish, import, sahnaga qo'yish | 15–25 soat |
| 2 personaj (eng qiyini) | 8–15 soat |
| ~20 ovoz: topish/generatsiya + ulash | 5–8 soat |
| Effektlar (bug', tutun, uchqun) — Godot'da qo'lda | 6–10 soat |
| 3–4 missiya data'si (generator bor, tez) | 3–5 soat |
| Yig'ish, sinov, balans | 10–15 soat |
| **Jami** | **~70–120 soat** |

Ya'ni to'liq kunlik ish bilan **2–3 hafta**, yarim kunlik bilan **4–6 hafta**.

---

## 6. Bu bilan nimaga erishasiz

1. **Vizual yaxlitlik.** Hozir o'yin kulrang qutilar + davrga mos kelmaydigan assetlar aralashmasi: 1900-yilda futbolka-jinsdagi personaj, yonida Humvee va AK-47. Bir davrni to'liq yopish — birinchi marta "haqiqiy o'yin" ko'rinishini beradi.
2. **Takrorlanadigan quvur.** Bir davr = data (.tres) + asset paketi. Buni bir marta qilsangiz, qolgan davrlar **shu qolipdan** chiqadi. "Seriyalar" g'oyangiz aynan shu tuzilishga tayanadi.
3. **Arxitektura tasdiqlanadi.** Loyiha ataylab data-driven qilingan (kodda hardcode yo'q). 1850–1900 ni **koddan bitta qator o'zgartirmasdan** qo'sha olsangiz — bu prinsip ishlayotganining isboti. Investorga aytiladigan kuchli gap.
4. **Mavjud tizimlar tekin keladi.** Resurs → ishlab chiqarish → inventar, tex daraxti, vaqt o'tishi, saqlash — hammasi davrdan mustaqil, qayta yozilmaydi.
5. **Resurs to'plami aynan mos.** Hozirgi 4 material — **yog'och, temir, ko'mir, tosh** — 1850–1900 uchun 2050 dan ham tabiiyroq. Bu tasodif emas, davr tanlovi to'g'ri.

---

## 7. Ochiq risklar

1. **AI 3D mexanizmlarda zaif.** Harakatlanuvchi qismli qurilmalar (Gatling barabani, ponton bo'g'inlari, bug' mashinasi shatuni) — Meshy ularni yaxlit "haykal" qilib beradi. Agar mexanizm **harakatlanishi** kerak bo'lsa, uni qismlarga bo'lib alohida generatsiya qiling.
2. **Ingichka geometriya.** Sim, g'ildirak spitsalari, relslar, arqon — AI bularni yo eritib yuboradi, yo juda ko'p poligon sarflaydi.
3. **Personajlar** — eng zaif nuqta. Bu yerda AI'ga tayanmang.
4. **Uslub tarqoqligi** — §2 dagi langar matnisiz 33 asset 33 xil o'yindan chiqqandek ko'rinadi.
5. **Perf.** `gl_compatibility` + zaif GPU. Har asset 8k uchburchakdan past bo'lsin, aks holda oldingi optimallashtirish ishi bekor bo'ladi.

---

## 8. Davr rejasi bo'yicha tavsiya

Hozir o'yinda **1900–2050** bor: 10 missiya, 10 karta, 30 vazifa, o'lchangan 12 daqiqalik demo marshruti — **hammasi ishlaydi va build qilingan**.

Agar 1850–1900 ni MVP sifatida **almashtirsangiz**, shu tayyor demo bir necha oyga yo'qoladi.

**Tavsiya: almashtirmang — oldiga qo'ying.**

```
1-mavsum  1850–1900   ← yangi, AI assetlar bilan
2-mavsum  1900–1945   ← hozirgi 1900/1914/1939/1943
3-mavsum  1945–2000   ← hozirgi 1965/1980/1999
4-mavsum  2000–2150   ← hozirgi 2015/2030/2050 + kengaytma
```

Shunda: yangi davr qurilayotganda ham ko'rsatadigan ishlaydigan demo qoladi, va "seriyalar" tuzilmasi tabiiy chiqadi.
