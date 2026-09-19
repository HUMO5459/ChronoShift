# ChronoShift — Progress Log

> **Claude Code:** har sessiya boshida shu faylni o'qi → holatni tikla → keyingi fazani bajar. Sessiya oxirida shu faylni yangila. Kodni qaytadan o'rganib chiqma, holat shu yerda.
>
> ⚠️ **Nom o'zgardi (2026-08-09):** loyiha **War of Engineers → ChronoShift**. Quyidagi eski
> yozuvlarda "War of Engineers" / "WoE" uchrasa — u **shu loyihaning eski nomi**, boshqa loyiha
> emas. Tarixiy yozuvlar ataylab o'zgartirilmagan. Eski nomdagi yo'llar ham endi yaroqsiz:
> `WarOfEngineers.csproj` → `ChronoShift.csproj`, `build/War of Engineers - Setup.msi` →
> `build/ChronoShift - Setup.msi`, `data_WarOfEngineers_windows_x86_64` →
> `data_ChronoShift_windows_x86_64`. Eski repo: `github.com/HUMO5459/WarOfEngineers` (arxiv).

---

## Joriy holat

- **Faza:** **BUXORO 1238 PORTI — reja tayyor, kod yozish boshlanmagan.** Yonma-yon: eski playtest feedback bloklari (A–E) hamon playtest kutmoqda.
- **Oxirgi yangilanish:** 2026-09-15 (Sessiya 17)

### 🆕 Buxoro 1238 porti (2026-09-15)

Foydalanuvchi `~/PROJECTS/Torobiy` dagi Three.js o'yinini (Buxoro 1238, seriyaning 001-epizodi)
Godot C# ga **to'liq 1:1 ko'chirishni** buyurdi. So'zma-so'z: "ThreeJS da qanday qilingan bo'lsa
Godot bilan ham xuddi shunday qilinishi kerak, to'liq ko'chirib o'tkazish kerak".

> ⚠️ **CLAUDE.md 1-qoidasi (MVP scope discipline) shu port uchun BEKOR QILINGAN** — foydalanuvchi
> aniq buyurdi. Hech narsa qisqartirilmaydi, "CUT" yo'q, ChronoShift sinfi faqat AYNAN bir xil
> xatti-harakat va bir xil raqamlarni bersa ishlatiladi.

- **Reja:** [`docs/BUXORO_PORT_PLAN.md`](docs/BUXORO_PORT_PLAN.md) — 14 agentli workflow natijasi
  (8 kodbaza oquvchisi, 3 mustaqil arxitektura, 1 hakam, 2 raqib tanqidchi). 88 tizim xaritalandi,
  47 mapping, 18 qadam, **20–26 hafta**. G'olib arxitektura: `design:mirror` (91 ball) —
  simulyatsiya uchun JS modullarini 1:1 aks ettirish, prezentatsiya uchun Godot-native.
- **✅ 5 ta bloker TUZATILDI (2026-09-15).** Har biri alohida agent tomonidan manba kodga
  solishtirib qayta tekshirildi (86 ta manba o'qishi). Natija: **beshala bloker ham haqiqiy,
  lekin BESHALASIDA tanqidchining o'z yechimi xato yoki to'liq emas chiqdi** — bittasining
  tashxisi butunlay noto'g'ri, to'rttasi chala. Tuzatishlar `BUXORO_PORT_PLAN.md` ning
  yuqorisidagi "Blocker corrections" bo'limida va u hujjatning qolgan qismidan USTUN.
  Qo'shimcha 49 ta topilma ham yozildi.
  - **1 (spawn):** tanqidchi manbada NaN bor degan, aslida manbada himoya BOR
    (`Math.max(1, soni-1)`), rejaning ko'chirmasi uni tushirib qoldirgan. Tanqidchining
    taklif qilgan C# yechimi esa butun sonli bo'lish tuzog'ini kiritardi: 1-to'lqindagi
    6 piyodadan 5 tasi bitta nuqtaga yig'ilardi. To'g'ri yechim suratni float ga keltirish.
  - **2 (Input Map):** tasdiqlandi. Lekin tanqidchi strelkalarni ChronoShift'ning umumiy
    `move_*` action'lariga qo'shishni taklif qilgan, bu ChronoShift'ning o'z o'yinini
    buzardi. To'g'ri yechim: 15 ta alohida `buxoro_*` action.
  - **3 (ovoz):** tasdiqlandi. Tanqidchining `pitchSpread: 0.0f` yechimi yetarli emas —
    3D masofa susayishi qoladi. Kerak: to'liq REFUSAL 5. Yo'l-yo'lakay aniqlandi:
    loyihada AudioBusLayout umuman yo'q va `project.godot` da `[audio]` bo'limi yo'q.
  - **4 (markup):** tasdiqlandi. Tanqidchi "qalin matn" degan, aslida CSS `display: block` —
    ya'ni bu BBCode emas, Label'lardan iborat VBoxContainer. Inventar ham ancha kengroq:
    menyuda 28 ta, boshqaruv jadvalida 26 ta kalit.
  - **5 (determinizm):** tasdiqlandi, tanqidchining uchta tafsiloti xato.
- **✅ 1-QADAM TO'LIQ BAJARILDI VA TEKSHIRILDI (2026-09-15).** Kod yozildi, build toza
  (0 xato, 0 ogohlantirish), 17/17 model import bo'ldi, avtomatik tekshiruv PASS berdi.
  - `scripts/buxoro/models/BuxoroImport.cs` — post-import skript. `models.js` dagi
    uchta funksiyaning aynan ko'chirmasi, o'sha tartibda (uchinchisi birinchisiga
    bog'liq): `tayyorla` (material, masshtab, oyoqni yerga tushirish),
    `klipNomlariniQoy` (klip nomlari), `ildizSiljishi` (ildiz harakatini olib tashlash).
  - `moljal.tres` — 23 qator, `models.js:187-207` dagi MOLJAL va IKKI_TOMON dan aynan.
    12 ta model ikki tomonlama.  `klip_xaritasi.tres` — 14 klip, nuqta pastki chiziqqa
    almashtirilgan holda.
  - `scripts/buxoro/tools/BuxoroImportCheck.cs` + `scenes/buxoro/BuxoroImportCheck.tscn` —
    natijani qayta o'lchaydigan tekshiruv. Ishga tushirish:
    `Godot --headless --path . scenes/buxoro/BuxoroImportCheck.tscn`.
    Xato bo'lsa exit code 1 qaytaradi, ya'ni build'ni to'xtata oladi.
  - **Tekshiruv natijasi:** balandliklar aniq tushdi — shomurod 1.78, noyon 2.05,
    mogul_piyoda 1.72, hunarmand 1.35, ark_qalasi 46.0, terak 11.0. Klip nomlarida
    nuqta yo'q, kliplar siklda, ildiz siljishi qolmagan. 12 ta model ikki tomonlama,
    qolgani bir tomonlama. PASS — 17 model.
  - **⚠️ O'LCHAB TOPILGAN GODOT XUSUSIYATI:** import paytida C# skriptli `.tres`
    oddiy `Godot.Resource` bo'lib yuklanadi, C# o'rami umuman yaratilmaydi. Bu faqat
    Buxoro resurslariga xos emas — loyihaning o'z `character_roster.tres` i ham shu
    kontekstda tipsiz keladi. Shuning uchun `BuxoroImport` jadvallarni `Get()` orqali
    generik o'qiydi. Ish vaqtida tiplangan yuklash normal ishlaydi.
- **✅ Yordamchilar erkak qilindi (2026-09-16).** Bozordagi 6 hunarmand va 6 shogird
  `ayol.glb` dan olinardi. Xom modellar ichida bo‘sh erkak yo‘q edi (beshtasi ham band),
  shuning uchun `jangchi-b` ikkinchi va uchinchi marta `hunarmand` va `shogird` nomlari
  bilan chiqarildi — ikkala eksport vositasiga `QOSHIMCHA_NUSXA` jadvali qo‘shildi.
  - Yo‘l-yo‘lakay bitta regressiya topildi va tuzatildi: yangi modelning bind pozasi
    T-poza, eski `ayol.glb` niki esa tik turgan holat edi. Shogirdlarning animatsiya
    nazoratchisi `shogird` nomi bo‘yicha qidiradi, o‘sha nomli model esa yo‘q edi.
    Endi `shogird.glb` ham bor, klipllari nomlangan (`idle_waiting`, `carry_walk`,
    `carry_run`), va `carry_run` mavjud bo‘lgani uchun protsedural harakat ham o‘chdi.
    Bozordagi sotuvchilarga `world.js` da idle klip biriktirildi.
  - **`--faqat <nom>` bayrog‘i qo‘shildi va U MUHIM:** hammasini qayta eksport qilish
    xavfli, chunki `world.js` darvoza halqasi segmentlari sonini `shahar_devori` va
    `qala_darvoza` ning O‘LCHAMIDAN hisoblaydi (world.js:377-419), ular esa determinizm
    oqimining bir qismi. Bir necha millimetr farq butun oqimni siljitadi.
  - Godot tomonida ham ikkala model chiqarildi, `klip_xaritasi.tres` ga 4 qator
    qo‘shildi. Import tekshiruvi: **PASS — 18 model** (shogird 1.62, hunarmand 1.35).
  - Web zip qayta yig‘ildi: 62 fayl, 20.2 MB, itch.io tekshiruvlari o‘tdi.
  - Eski ayol modeli zaxirasi scratchpad da: `hunarmand-ayol-zaxira.glb`.
- **✅ 2-QADAM: DETERMINIZM DARVOZASI O'TDI (2026-09-15).** Build toza, parity tekshiruvi
  haqiqiy web o'yin bilan solishtirildi va o'tdi.
  - **Yozilgan:** `Lcg.cs` (urug' 1238/8317/4711, ulong — nega uint EMAS ekani izohda),
    `BuxoroTerrain.cs` (sof funksiya; x=120 faza xatosi izohda), `Smoothing.cs` (web ning
    9 ta silliqlash idiomi bitta joyda — to'rttasi kadr tezligiga bog'liq, shuning uchun
    60 Hz majburiy), `BuxoroBalance.cs` + 4 ta qator tipi, `BuxoroWorldStream.cs`
    (tugun/terak/uy draw ketma-ketligi), `BuxoroRuntime.cs` (autoload), `ParityDump.cs`
    + sahnasi, `balance_buxoro.tres` (14 sub-resource), `Torobiy/tools/parity/dump_web.js`.
  - **DARVOZA NATIJASI — eng kuchli tekshiruv turi:** web o'yin brauzerda ishga tushirilib,
    `__O.dunyo.tugunlar` dan HAQIQIY 66 tugun olindi va Godot chiqishi bilan solishtirildi:
    **66/66 tugun, 6 xona aniqlikda AYNAN bir xil.** Generator qayta yozilmadi — haqiqiy
    o'yin natijasi bilan tasdiqlandi.
  - Draw sanoqlari aniq: tugunlar 1080, teraklar 1259, uylar 1267, klaster o'lchamlari
    2,4,4,2,3. Web o'yin 53 terak xabar qildi — mos. Relyef 6 nuqtada mustaqil hisob bilan mos.
  - **⚠️ SIZDAN:** `BuxoroRuntime` ni Project Settings → Autoload ga qo'shing
    (nom `BuxoroRuntime`, yo'l `res://scripts/buxoro/core/BuxoroRuntime.cs`). Yana
    `physics_ticks_per_second = 60` va `max_physics_steps_per_frame = 3` qo'ying — hozir
    `project.godot` da `[physics]` bo'limi umuman yo'q, `Smoothing` dagi to'rtta idiom esa
    aynan 60 Hz ga sozlangan.
  - **3-qadamga qoldi:** darvoza halqasi (207 draw) va chekka devorlar (80 draw) hali
    yozilmagan, shuning uchun C blokida dastlabki uchta sanoq bor. Tuzatilgan
    spetsifikatsiyaning 2.10-bandi shunga ruxsat beradi.

  - **Rejadan chetlanish:** reja `tools/buxoro_models.sh` deb yozgan edi; o'rniga
    avvaldan yozilgan `Torobiy/tools/godot-eksport.js` ishlatildi, u xuddi shu ishni
    qiladi va NOM_XARITASI hamda uchburchak byudjetini `optimize-models.js` bilan
    bo'lishadi.
  - **Oldingi holat (eksport bosqichi):** Godot 4.7.1 `KHR_draco_mesh_compression` ni
  tanimaydi — o'lchab tasdiqlandi, har bir model `ERR_PARSE_ERROR` bilan yiqiladi.
  `Torobiy/tools/godot-eksport.js` yozildi (draco/meshopt/quantization YO'Q, webp tekstura BOR).
  17/17 model `assets/models/buxoro/` ga eksport qilindi, 136 MB → 10 MB, ChronoShift'ga
  **xatosiz** import bo'ldi. Skeletlar, klipllar va `L_Hand`/`R_Hand` suyaklari saqlangan.
  Hisobot: `assets/models/buxoro/eksport-hisoboti.json` (masshtab koeffitsientlari bilan).
- **⏳ Bloklangan:** 12 ta mp4 kadr. Godot faqat Ogg Theora o'ynatadi, bu mashinadagi
  ffmpeg 8.1.2 da Theora **dekoderi bor, enkoderi yo'q**. Kerak: `brew install ffmpeg@7`
  yoki `ffmpeg2theora`. Foydalanuvchi qaroriga qoldirildi.
- **Eslatma:** Godot glTF klip nomidagi nuqtani pastki chiziqqa aylantiradi
  (`NlaTrack.001` → `NlaTrack_001`), `klip-xaritasi.json` esa nuqtali nom ishlatadi.
  Xarita o'zi ham to'liq emas — 8 klipdan faqat 3 tasi nomlangan.

### 📋 Playtest feedback bloklari (2026-08-03, foydalanuvchi ro'yxati)

| Blok | Ichida | Holat |
|---|---|---|
| **A · Jang va NPC** | Texnika buziladi, zarba olish ko'rsatkichi, dushman sizni ko'rishi/otishi, texnikaga hujum, dushman 180° bugi, NPC otiladigan bo'ldi | ✅ kod tayyor (Sessiya 16) |
| **B · Missiya UI** | Statik brifing ekrani, o'yin ichida vazifalar oynasi (`J`, joriy vazifa highlight), maqsad markeri doim ko'rinadi | ✅ kod tayyor (Sessiya 16) |
| **C · Xarakteristikalar** | `VehicleData`/`WeaponData` ga tezlik/bronya/dallik/uron/narx + xarakteristika oynasi (`C`) | ✅ kod tayyor (Sessiya 16) |
| **D · Craft va yaratish** | Modul konstruktori: texnika va qurol yaratish, chizmani saqlash, ishlab chiqarishga yuborish (`B`) | ✅ kod tayyor (Sessiya 16) |
| **E · Save** | Saqlash bugi — o'lchandi: mexanizm soz edi, `Save()` **chaqirilmasdi**. Tuzatildi | ✅ kod tayyor (Sessiya 16) |

**Foydalanuvchi qarori (2026-08-03):** "npc ham otilsin" → `Npc` endi `IDamageable` (60 HP, zarba chaqnashi, yiqilib yo'qoladi, mukofot yo'q — ular jangchi emas).
- **Holat bir qarashda:** TZ majburiy ro'yxati **11/11** · render narxi 70% ga tushirildi (hub 662k uchburchak/kadr) · to'liq missiya real input bilan uchdan-uchgacha o'ynaldi · 10/10 missiya data auditidan o'tdi · `dotnet build` 0/0 · ikkala boot yo'li headless toza.
- **⚠️ Sessiya 15 da topilgan eng katta xato:** `PlayerInteraction` ning `Area3D` signallari **statik obyektlarni umuman ko'rmagan** — ya'ni `E` hech qayerda ishlamagan (vazifa, kon, mastserskaya, taxta, texnika). Shakl so'roviga (`IntersectShape`) o'tkazilib hal qilindi. Buni **o'lchov** topdi, kod o'qish emas: bir joyda `GetOverlappingBodies()` bo'sh qaytardi, `IntersectShape` esa hammasini topdi.
- **YANGI SPEC BOR:** `docs/ChronoShift_MVP_TZ.md` — foydalanuvchi bergan ТЗ (investorlarga vertical slice). Manba `ChronoShift MVP Prototype.docx` loyiha ildizida. Bu **WoE'ni almashtirmaydi** — qarori: WoE'da bor hamma narsa qoladi, TZ'ning yetishmayotgan qismlari ustiga qo'shiladi.
- **Muhit o'zgardi:** Ish macOS'ga ko'chdi. Godot 4.7.1 mono → `/Applications/Godot_mono.app`. Terminaldan oching (Finder'dan emas — `~/.dotnet` PATH'da bo'lishi shart):
  `/Applications/Godot_mono.app/Contents/MacOS/Godot --path <loyiha> -e`
  .NET 8 SDK shart emas — 10.0.203 SDK `net8.0` ni build qiladi. Godot 4.7.1 editori `csproj` dagi SDK'ni 4.7.0 → 4.7.1 ga ko'tardi.
  Render tekshiruvi (skrinshot o'rniga, macOS ekran ruxsatisiz ishlaydi):
  `Godot --path <loyiha> --resolution 1920x1080 --write-movie <yo'l>/frame.png --fixed-fps 1 --quit-after 3`
- **TZ qamrovi (`docs/ChronoShift_MVP_TZ.md` 3-bo'lim, majburiy ro'yxat): 11/11.** Harakat ✓ · 3rd-person kamera ✓ · interaktsiya ✓ · resurs ✓ · mastserskaya ✓ · inventar ✓ · vazifa tizimi ✓ · kichik karta ✓ (10 map) · NPC ✓ · jang ✓ · final missiya ✓. **Ataylab bajarilmagan:** TZ 5-bo'limining "bitta yaxlit karta" sxemasi va 15-bo'limning XVIII–XIX asr estetikasi (foydalanuvchi qarori: WoE'da bor narsalar saqlanadi).
- **📄 Demo uchun:** `docs/DEMO_SCENARIO.md` — o'lchovga asoslangan ~12 daqiqalik investor marshruti (3 missiya). Playtest'ni shu marshrut bo'yicha qilsangiz, bir vaqtning o'zida demo mashqi ham bo'ladi.
- **KEYINGI QADAM: PLAYTEST.** Kodda tekshiradigan narsa qolmadi — o'yin boshidan oxirigacha o'ynaladi va uchdan-uchgacha o'lchandi. Qolgan savollar sezgiga oid (tezlik, qiyinlik, kamera hissi), ularni faqat foydalanuvchi ayta oladi.

### 🎮 Playtest checklist (yagona, birlashtirilgan)

**A — Ochilish va missiya oqimi**
0. Barcha ekranlar endi **ko'k (blueprint)** dizaynda — bosh menyu bilan bir xil tilda. Qog'oz uslubi qolmadi.
1. "Yangi o'yin" → **brifing kartochkasi** chiqadimi ("HARBIY MUHANDIS")? "Vazifani qabul qilaman" → missiyalar taxtasi ochiladimi?
2. Taxtada 10 missiya; birinchi bajarilmagani "· KEYINGI" (ko'k), bajarilgani "· BAJARILDI" (qizil) deb belgilanadimi?
3. Missiyani boshlaganda **yil o'sha eraga o'tadimi** (o'ng yuqorida `DAVR`)? Map yuklanadimi?
4. O'ng yuqorida "MISSIYA · <nom> · n/3" hisoblagichi to'g'rimi?
5. 3 vazifa bajarilgach tugash ekrani → "Bazaga qaytish" → bonus to'lanib, taxta qayta ochiladimi?
6. 10-missiya (2050) tugagach **kampaniya yopilish ekrani** chiqadimi?

**B — Asosiy sikl (TZ 4-bo'limi)**
7. Konga borib `E` → material olinadimi? Chapdagi `MATERIAL` paneli o'sadimi?
8. Mastserskayaga `E` → buyurtma → progress → "ISHLAB CHIQARISH TUGADI" → `I` bilan sumkada ko'rinadimi?
9. 2-vazifa uskunasiz boshlanmaydimi (HUD "KERAK: …" deydimi)? Uskuna bilan boshlanib, uskuna sarflanadimi?
10. Vazifa tugagach ekran o'rtasida **yangi yil** chaqnaydimi?

**C — Qurol va jang**
10a. `V` bilan **birinchi shaxs** — piyodada qo'l/qurol ko'rinadimi? Texnikada, tankda, samolyotda ichidan ko'rinadimi?
10b. AWP bilan o'ng-tugma — **optika** chiqadimi?
11. Qurol qo'lda **to'g'ri turadimi** (turganda sonda, nishonga olganda ko'tarilgan, ikki qo'l bilan)?
12. O'ng-tugma bilan nishonga olganda kamera yelka ustiga suriladimi, nishon ochiq qoladimi?
13. Otganda chaqnash **stvoldan** chiqadimi (personaj ustida emas)?
14. Javondagi qurolga `E` → qo'lga olinadimi? Pastdagi tez slotlar 1–5 bilan almashadimi?
15. Dushmanlar juda tez o'ldiradimi yoki juda oson? (17 m sezish / 5.0 zarar / 2.4 s ga sozlangan)

**D — Texnika va muhit**
16. Humvee: ta'mirlash → haydash → `F` bilan chiqish. Tank: to'p otiladimi? Samolyot: `W`/`S` bilan ko'tarilish/pastlash?
17. Lagerdagi NPC'lar: rol plitasi yaqinlashganda chiqadimi, aholi/aloqachi yuradimi, vazifa bajarilganda gapirishadimi?
18. Mastserskaya mo'risidan tutun chiqadimi? FPS tushmaydimi?

**E — Saqlash**
19. Missiya o'rtasida saqlab, chiqib, qayta kirsangiz **o'sha map'ga** qaytadimi va bajarilgan vazifa yashil qoladimi?
20. `Shift` bilan yugurganda `CHIDAM` tugab, yurishga tushadimi?

> **Eslatma:** `assets/textures/logo.png` hamon yo'q — bosh menyudagi logotip kartochkasi bo'sh oq. Fayl tushgach `Left/LogoCard/LogoBox/Logo` ga `texture` beriladi.

> ⚠️ **ESKI SPEC HAMON YO'Q:** `docs/WarOfEngineers_MVP_Architecture.md` repo'da mavjud emas — git tarixida ham hech qachon bo'lmagan. CLAUDE.md uni har sessiya o'qishni talab qiladi, lekin manba yo'q. **Uning o'rniga endi `docs/ChronoShift_MVP_TZ.md` bor** (2026-07-28) — scope savollarida shunga tayaning; ziddiyatlarni faqat foydalanuvchi hal qiladi.

> ⚠️ **MUHIM — Godot editori:** Loyihani FAQAT Godot **.NET/mono** versiyasi bilan oching (macOS'da `/Applications/Godot_mono.app`, eski Windows mashinasida `Godot_v4.7-stable_mono_win64.exe`).
> Standart Godot (`_mono_`siz) C# skriptlarni ishga tushira olmaydi — o'yin ochiladi lekin boshqaruv ishlamaydi. Bir marta bu xato bo'lgan (Sessiya 3).

> 🧹 **Tozalandi (2026-07-28):** o'lik fayllar o'chirildi — `HangarScreen` (.cs+.tscn), `CharacterVisual.cs`, va eski 3 missiya orolisi (`MapRepair/Telegraph/Trench.tscn` + `mission_repair/telegraph/trench.tres` + `task_repair_generator/lay_telegraph/fortify_trench.tres`). Ular yopiq halqa edi — tashqaridan hech kim ishora qilmasdi (grep bilan tasdiqlandi), import va ikkala sahna o'chirilgandan keyin ham toza.
> Kerak bo'lsa qaytarish: `git checkout -- scripts/ui/HangarScreen.cs scenes/ui/HangarScreen.tscn scripts/player/CharacterVisual.cs scenes/maps/Map*.tscn resources/data/missions/mission_{repair,telegraph,trench}.tres resources/data/tasks/task_{repair_generator,lay_telegraph,fortify_trench}.tres`


> 📦 **Git holati (2026-07-28 da tekshirildi) — push muammo bo'lmaydi:** eng katta fayl 32 MB (`trees9.obj`; GitHub cheki 100 MB/fayl), jami kuzatilgan 163 MB / 332 fayl (tavsiya <1 GB). Remote allaqachon ulangan: `github.com/HUMO5459/WarOfEngineers`.
> Kichik tozalash: `.gitignore` ga `.DS_Store` qo'shildi, lekin `scenes/.DS_Store` **allaqachon kuzatilgan** — indeksdan chiqarish uchun bir marta bajaring (fayl diskda qoladi):
> `git rm --cached scenes/.DS_Store`

> 🛠️ **Kontent generatori:** `python3 tools/gen_production.py` — item/retsept/NPC fayllarini yozadi, 10 map'ga kon+mastserskaya+NPC qo'yadi, vazifalarga uskuna talabi va kalendar sakrashini beradi, **so'ng o'zini tekshiradi** (`verify: all 10 maps solvable`). Idempotent — istalgan payt qayta ishga tushirsa bo'ladi.

---

## Faza checklist

Har faza **o'ynaladigan increment**. Faza faqat foydalanuvchi playtest qilib tasdiqlagach belgilanadi.

- [x] **Faza 0 — Setup** ✅ 2026-07-14 (playtest tasdiqlandi)
  Loyiha, folder tuzilma, git, `Main.tscn` + `TestArena` greybox (pol + bir nechta cube)
  → *Natija: bo'sh dunyo ochiladi*

- [x] **Faza 1 — Character** ✅ 2026-07-14 (playtest tasdiqlandi, .NET editor bilan)
  `PlayerController.cs` (yurish/yugurish/sakrash), `PlayerCamera.cs` (SpringArm third-person), `Player.tscn` (capsule placeholder)
  → *Natija: dunyoda yurasan, kamera aylanadi*

- [x] **Faza 2 — Interaction + Engineering Task** ✅ 2026-07-14 (playtest tasdiqlandi)
  `IInteractable` interfeys, `EngineeringTask.cs` (timed/progress), `PlayerInteraction.cs`
  → *Natija: obyektga borib "E" bosib vazifa bajarasan*

- [x] **Faza 3 — Economy + Progression + HUD** ✅ 2026-07-15 (playtest tasdiqlandi)
  `EconomyManager`, `ProgressionManager`, `MissionManager` reward loop, `HUD.cs`
  → *Natija: vazifa → pul + XP, ekranda ko'rinadi*

- [x] **Faza 4 — Time + Acceleration** ✅ 2026-07-15 (playtest tasdiqlandi) — **signature mexanika**
  `TimeManager` (in-game vaqt, `TimeScale`), vazifa soniga bog'liq tezlashuv
  → *Natija: vaqt oqadi, vazifa ko'paygancha tezlashadi*

- [x] **Faza 5 — Tech Tree** ✅ 2026-07-15 (playtest tasdiqlandi)
  `TechNode` (Resource), `TechTreeUI.cs`, kamida 1 unlock gameplay'ga ta'sir qiladi
  → *Natija: pul/XP sarflab tech ochasan*

- [x] **Faza 6 — Save/Load** ✅ 2026-07-15 (playtest tasdiqlandi)
  `SaveManager.cs` — JSON, `user://` papkada
  → *Natija: chiqib qayta kirsang holat saqlanadi*

- [ ] **Faza 7 — Weapon + Vehicle** (kod tayyor 2026-07-15, playtest kutilmoqda)
  `WeaponController.cs` (raycast otish), `Vehicle.cs` (kirish/boshqarish/ta'mirlash)
  → *Natija: vertical slice TUGADI*

- [ ] **Qo'shimcha — Menus & Game Flow** (kod tayyor 2026-07-16, playtest kutilmoqda) — foydalanuvchi so'rovi, MVP spec'dan tashqari
  Bosh menyu (New Game/Continue/Missions/Controls/Hangar/Quit), pause menyu (Esc), Controls paneli (Input Map'dan), Missions paneli (tasklardan), Hangar, `SaveManager.HasSave`, `GameDemoLauncher` (FTPS `OS.CreateProcess` bilan tashqi ochish). Boot sahnasi Main.tscn → MainMenu.tscn. Esc PlayerCamera'dan pause menyuga o'tdi.
  → *Natija: o'yin menyudan boshlanadi, pause/continue/missions, FTPS demo alohida ochiladi*

- [ ] **Qo'shimcha — Open-world streaming** (kod tayyor 2026-07-16, playtest kutilmoqda) — foydalanuvchi so'rovi
  `WorldStreamer.cs` (60m chunk, radius 3 = ~49 faol, player group orqali, checkerboard tint), `GroundChunk.tscn`, tuman; fixed 40×40 pol olib tashlandi. Zaif hardware'ga xavfsiz.
  → *Natija: cheksiz katta yer his qilinadi, uzoqqa yursang streaming*

- [~] **Qo'shimcha — UI redizayn** — boshlandi 2026-07-17 (A + B kod tayyor, render tekshirildi, playtest kutilmoqda) — foydalanuvchi so'rovi, MVP spec'dan tashqari
  Manba: Claude Design canvas `War of Engineers UI.dc.html` (project `df047d21-b2aa-4ee7-a4a1-2efcf15f2f07`). 6 ekran: bosh menyu, HUD, tex daraxti, vazifalar jurnali, saqlash daftari, pauza.
  **Qarorlar:** menyu tugmalari to'liq qoladi (dizayndagi 3 emas — Missions/Controls/Hangar/Avia demo yo'qolmaydi); saqlash daftari — faqat UI, `SaveManager` bitta save'ligicha (slot 1 real, 2/3 bo'sh, `_placeholderSlot`).
  ✅ **A — dizayn tizimi:** `assets/fonts/` (Oswald/Archivo/Inter variable + IBM Plex Mono 400/500/600/700, OFL), `resources/theme/woe_theme.tres` (type variation'lar: TitleXL, MonoLabel, BlueprintPrimary/Outline/Ghost, PaperButton/Solid, HudPanel…), `scripts/ui/UiPalette.cs` (ranglar bitta manbada). `project.godot`: `gui/theme/custom` + 1920×1080 `canvas_items`/`keep` stretch (dizayn 1920×1080 sahnani sig'diradi).
  ✅ **B — bosh menyu + saqlash daftari:** `BlueprintBackground.cs` (grid, ikki ramka, kompas atirguli `_Draw` bilan), MainMenu.tscn qayta qurildi (logotip kartochkasi, CHIZMA № 001, Oswald sarlavha, 7 tugma, chizma jadvali), `SavesPanel` + `SaveSlotRow` (+ `SaveManager.Peek()`, `GameCalendar.cs` — `1900-MART` / `14:32`).
  ⚠️ **Logotip yo'q:** `assets/textures/logo.png` kerak (design project'dagi `assets/logo.png`; MCP `get_file` 256 KiB'da kesib qo'ydi). Hozir kartochka bo'sh oq. Fayl tushgach `Left/LogoCard/LogoBox/Logo` ga `texture` beriladi.
  ✅ **C — HUD:** `HUD.cs` qayta yozildi + `HudToast` (tween bilan sirg'aladi, o'zini `QueueFree` qiladi). Pul (₳ tanga), daraja + XP bar, sana+soat, `×N` vaqt badge'i (chevron faqat tezlashganda pulsatsiya qiladi), eng yaqin vazifa tracker'i, `E` prompt, vazifa progress bar'i, toast'lar, tezlashuv flash'i.
  **Yangi API (HUD uchun, minimal):** `EngineeringTask` — `IsActive`/`IsCompleted`/`Progress`/`Kind`/`DurationSeconds` + `TaskGroup` guruhi; `PlayerInteraction.Focused` + `InteractionGroup`; `ProgressionManager.XpIntoLevel`/`XpPerLevel`/`LevelProgress`; `TimeManager.ScalePerTask`. HUD guruhlar orqali topadi — Main.tscn'ga qattiq bog'lanish yo'q.
  **Data:** `EngineeringTaskData.Kind` qo'shildi; 3 ta task `.tres` — nomlar o'zbekchaga o'tdi (`Generatorni ta'mirla`) + Kind (`TA'MIRLASH`/`O'RNATISH`/`ISTEHKOM`). Xohlansa oson qaytariladi.
  **Dizayndan farq (ataylab):** `[J] Vazifalar` ipuchi olib tashlandi — bunday binding yo'q (jurnali faqat bosh menyuda). Ipuchlar Input Map'dan o'qiladi (qoida 4), `+0.75×` o'rniga `TimeManager.ScalePerTask` dan real qiymat. Surge'da `+45 KUN` o'rniga real `×N` (kun sonini hisoblash uchun ma'lumot yo'q).
  ✅ **D — pauza + jurnal + tex daraxti:**
  • **Pauza:** dizayn kartochkasi (`#1b1e2e`, PAUZA kicker, statistika footer'i). Tugmalar: Davom etish/Saqlash/Yuklash/Boshqaruv/Bosh menyu/Chiqish — Saqlash/Yuklash `SavesPanel` ni ochadi (endi u ham pauzada instansiya qilingan). Esc `_Process` poll'idan `_UnhandledInput` ga o'tdi: SavesPanel Escape'ni birinchi yutadi, shuning uchun bitta bosish ikkalasini yopmaydi.
  • **Jurnal:** `MissionsPanel` + `MissionRow` — status doirasi, tavsif, mukofot, burilgan "BAJARILDI" shtampi, `BAJARILDI: n/3`. Bosh menyuda qoladi (qaror: o'yin ichida `J` yo'q).
  • **Tex daraxti:** `TechTreeUI` qayta yozildi + `TechNodeCard` + `TechEdges`. Qog'oz fon, header chip'lari, node'lar prereq chuqurligi bo'yicha ustunlarga (dizayn gridi 310×170), bog'lovchi chiziqlar (ochilgan prereq — ko'k), "OCHILDI" shtampi, qulf sabablari `CanUnlock` tartibida (prereq → daraja → pul).
  **Data:** `MissionManager` endi bajarilgan task ID'larini saqlaydi (`IsTaskCompleted`); `SaveData.CompletedTaskIds` qo'shildi — eski save'lar bo'sh ro'yxat bilan yuklanadi. `EngineeringTaskData.Description` + 3 task tavsifi. 3 ta tech `.tres` nomi/tavsifi o'zbekchaga o'tdi.
  **Dizayndan farq (ataylab):** 1939+/Sovuq urush/2040 shoxlari **chizilmadi** — ular spec 7-bo'limi (Backlog), oltin qoida 1; dizaynning o'zi ham ularni backlog deb belgilagan. Pastda shu haqda izoh bor. Qulflangan kartochka chegarasi punktir emas (StyleBoxFlat punktirni qo'llamaydi) — o'rniga xiralashtirilgan. Clipboard qisqichi olib tashlandi (PanelContainer bolasini cho'zadi).
  **Ma'lum nomuvofiqlik (mening o'zgarishimdan emas):** save yuklangach dunyodagi vazifalar "bajarilmagan" holatda qayta tug'iladi (`_completed` saqlanmaydi), jurnal esa ularni bajarilgan deb ko'rsatadi — ya'ni vazifani qayta bajarib pul/tezlashuv olish mumkin. Bu avvaldan bor edi; tuzatish gameplay o'zgarishi, UI fazasidan tashqari. → ✅ **TUZATILDI** 2026-07-28 (Sessiya 15, 5-qism).
  → *Natija: o'yin chizma-uslubidagi menyudan boshlanadi*

- [~] **Qo'shimcha — Muhit + texnika** — 2026-07-17 (kod tayyor, render tekshirildi, playtest kutilmoqda)
  ✅ **Mashina teskari yurardi — tuzatildi.** `Vehicle.cs` `-Z` ga yuradi, Humvee modelining burni esa `+Z` ga qaragan edi. `Vehicle.tscn` → `Body` ga 180° Y burilish. Tekshirish usuli: haydash kamerasini vaqtincha `current` qilib render qilinadi — avval old panjara ko'rinardi, endi orqa bamper.
  ✅ **T-90 haydaladigan bo'ldi.** Yangi `scenes/world/Tank.tscn` (mavjud `Vehicle.cs`, `_moveSpeed=5`, `_turnSpeed=1.2`). `TestArena` dagi `ParkedTank` shu bilan almashtirildi (22,0,-12). `ParkedTank.tscn` o'zi qoldi — ishlatilmaydi.
  ⚠️ **Muhim: `t90a.obj` ichida 3 ta tank bor edi** (yonma-yon, X bo'ylab). Haydaganda uchalasi birga yurardi. `assets/models/vehicles/t90/t90a_single.obj` yaratildi — o'rtadagi (teksturali) tank ajratib olindi, X/Y bo'yicha markazlashtirildi (13 770 vertex). **Asl `t90a.obj` tegilmadi.**
  Model o'lchamlari: Z-up, uzunligi Y bo'ylab, burni `-Y`. Godot uchun bazis: `Transform3D(-1,0,0, 0,0,1, 0,1,0)`. Masshtab 1.0 — model allaqachon metrda (9.8×3.9×4.2 m).
  ✅ **Muhit:** `House.tscn` (greybox: BoxMesh devor + PrismMesh tom + kolliziya — 6-qoida bo'yicha placeholder), `Village.tscn` (6 uy) → (-70,0,-60). Grove 1→7 ga ko'paytirildi (turli burilish/masshtab bilan tarqatildi).
  ⚠️ **FPS xavfi:** har `Grove` = `trees9` (247k tris). Endi 7 ta = ~1.7M tris. Zaif GPU'da sekinlashsa — Grove sonini kamaytirish yoki `MultiMesh`/LOD kerak bo'ladi.
  ❌ **O't/o'simlik qo'shilmadi** — model yo'q (`assets/models/environment/` da faqat `trees9`).
  → *Natija: dunyoda qishloq, daraxtzorlar; Humvee to'g'ri yuradi; T-90 haydaladi*

- [~] **Qo'shimcha — Texnika boshqaruvi (1/3)** — 2026-07-17 (kod tayyor, render tekshirildi, playtest kutilmoqda)
  Foydalanuvchi so'rovi bo'yicha 3 bosqichli reja: **1) sichqoncha + minora → 2) effektlar + tank to'pi + buzilish → 3) maplar (hub + missiya maplari)**.
  ✅ **Sichqoncha bilan aylanish:** yangi kod yozilmadi — mavjud `PlayerCamera.cs` texnika `CameraRig` iga biriktirildi (Humvee + Tank). Rig texnikaning bolasi bo'lgani uchun burilganda kamera orqada qoladi, sichqoncha esa erkin qarash beradi. Kirishda rig yaw nolga qaytariladi (`Enter()`).
  ✅ **Minora aylanadi:** `Vehicle.cs` ga `_turretPath` (ixtiyoriy) + `_turretTurnSpeed` (0.9 rad/s) qo'shildi. `TraverseTurret()` minorani kamera rigi yaw'iga qarab burardi — rig yaw allaqachon korpusga nisbatan bo'lgani uchun u to'g'ridan-to'g'ri nishon burchagi bo'lib xizmat qiladi (korpusni bursangiz minora ham ergashadi).
  ✅ **Mesh ajratildi:** `t90_hull.obj` (7526v / 12202f) + `t90_turret.obj` (6242v / 9961f, origin minora halqasida). `t90a.obj` guruhlari aniq bo'lgani uchun ajratish toza chiqdi: `DrawCall_1244` = minora+stvol, qolganlari = korpus. Pivot: model koordinatalarida x≈0, y=1.115.
  ⚠️ **Oldingi `t90a_single.obj` da markazlash xatosi bor edi** (~0.96 m siljish): klaster chegarasi `4.10` deb yozilgandi, lekin bu yaxlitlangan qiymat — uchinchi tankning `4.0999` dagi vertexlari filtrdan o'tib markazni buzgan. Qayta ajratildi, markaz endi faqat ishlatilgan vertexlardan hisoblanadi. Fayl o'chirildi.
  Tekshirish: minora 0° va 45° da render qilindi — o'z o'qida aylanadi, korpus joyida qoladi.
  ✅ **2-bosqich — effektlar + tank to'pi + buzilish** (2026-07-17, playtest kutilmoqda):
  • **FX qatlami:** `scripts/fx/` — `OneShotFx.cs` (chaqnashni so'ndirib, o'zini `QueueFree` qiladi), `Tracer.cs` (ikki nuqta orasida cho'ziladi, so'nadi), `Fx.cs` (statik yordamchi: `SpawnImpact/SpawnMuzzle/SpawnTracer`, effektlar sahna ildiziga qo'yiladi — qurol harakatlansa ham joyida qoladi). Sahnalar: `scenes/fx/ImpactFx.tscn` (uchqun + chang + yorug'lik), `MuzzleFx.tscn`, `Tracer.tscn`.
  **`CPUParticles3D` ishlatilgan, `GPUParticles3D` emas** — loyiha `gl_compatibility` renderer'da (zaif GPU).
  • **`WeaponController`** endi chaqnash + tracer + zarba effektini chiqaradi (`_muzzleFxScene`/`_impactFxScene`/`_tracerScene` Player.tscn da ulangan). Tracer qurol modelidan boshlanadi.
  • **`TankCannon.cs`** — `TurretPivot/Cannon` da, `Muzzle` stvol uchida (-0.17, 1.72, -6.02 — mesh'dan o'lchangan). Zarar 120, radius 3.5 m splash (masofaga qarab kamayadi), qayta o'qlash 3 s. **O'q stvol yo'nalishi bo'ylab uchadi, kamera bo'ylab emas** — shuning uchun minora yetib kelmaguncha nishonga tegmaydi; bu minora tezligiga ma'no beradi. Faqat haydovchi otadi (`Vehicle.IsDriven`).
  • **`DestructibleProp.cs`** — sog'liq, zarar ortgani sayin qorayadi (material `Duplicate()` qilinadi — aks holda bitta prop butun sahnani bo'yardi), o'lganda effekt + pul + `QueueFree`. Arena'dagi 4 kubga qo'llandi (60 HP, +₳10).
  ⚠️ **Tutilgan xato:** `ImpactFx` da `mesh = null` yozilgan edi — `CPUParticles3D` mesh'siz hech narsa chizmaydi. Zarrachalar tug'ilardi, lekin ko'rinmasdi. `QuadMesh` + billboard material qo'yildi.
  Tekshirish: tank to'pi vaqtincha sinov devoriga otdirildi — tracer, uchqunlar va chaqnash render'da tasdiqlandi.
  ✅ **HUD qayta o'qlash indikatori** — pastda markazda "TO'P / TAYYOR|O'QLANMOQDA" + oltin bar. Faqat tankda o'tirganda ko'rinadi (`TankCannon.IsDriverAboard`, guruh orqali topiladi).

  ✅ **3-bosqich — hub + missiya maplari** (2026-07-17, playtest kutilmoqda):
  • **Arxitektura:** `Main.tscn` endi **qobiq** — o'yinchi, HUD, tex daraxti, pauza va missiya paneli doim tirik; faqat `World` node ichidagi dunyo almashadi. Sahna qayta yuklanmaydi, o'yinchi nusxalanmaydi, xotirada bir vaqtda bitta dunyo turadi.
  • `MapLoader.cs` (Main'da, `map_loader` guruhida) — `LoadHub()` / `LoadMap(scene)`, eskisini o'chirib yangisini qo'yadi, o'yinchini map'dagi `Spawn` node'iga ko'chiradi va tezligini nolga tushiradi.
  • `MissionData.cs` (`[GlobalClass]`) + 3 ta `.tres`: `mission_repair`, `mission_telegraph`, `mission_trench` (nom, tavsif, tur, map sahnasi, bonus pul/XP).
  • `MissionManager` ga missiya oqimi qo'shildi (**yangi autoload YO'Q** — bu uning o'z vazifasi): `ActiveMission`, `StartMission()`, `FinishMission()` (bonus to'laydi + hubga qaytaradi), `AbortMission()`, `MissionStarted`/`MissionFinished` signallari.
  • `MissionMap.cs` — map ildizida, o'z `EngineeringTask` larini kuzatadi; hammasi tugagach 2.5 s dan keyin `FinishMission()`.
  • **3 map:** `scenes/maps/MapRepair|MapTelegraph|MapTrench.tscn` — o'z yeri (70×70), quyoshi, `WorldEnvironment` (sky + tuman), `Spawn`, vazifasi, buziladigan sandiqlari, daraxtzorlari.
  • **Hub** (`TestArena`): 3 ta ish stoli olib tashlandi (ular endi maplarda), o'rniga `MissionBoard` (3,0,-4) — `IInteractable`, `E` bosilsa `MissionBoardPanel` ochiladi (qobiqda, `BoardLayer` CanvasLayer'da). Panel — jurnal uslubida, har qatorda "BOSHLASH".
  ⚠️ **Tutilgan xato — aylanma bog'liqlik:** avval `MissionMap` o'z `MissionData` siga, `MissionData` esa map sahnasiga ishora qilardi → Godot parse xatosi, `_mission` null bo'lib qolardi. Yechim: map o'z missiyasini bilmaydi — u faqat "vazifalarim tugadi" deydi, qaysi missiya faolligini `MissionManager` biladi. Bog'liqlik endi bir tomonlama.
  ⚠️ **Render tekshiruvi haqida:** `--import` dan keyingi **birinchi** Movie Maker ishga tushirishi 0 kadr / oq rasm beradi (shader keshi qayta quriladi). Kod xatosi emas — ikkinchi marta ishlatish kerak.
  ✅ **Missiya tugash ekrani** (2026-07-17): `MissionCompletePanel` (qobiqda, `BoardLayer` da) — dizayn modali (`#1b1e2e`): "MISSIYA / BAJARILDI / <nom> / MUKOFOT: +₳N · +N XP / Bazaga qaytish".
  Oqim ajratildi: `MissionMap` faqat **xabar beradi** (`MissionManager.ReportObjectivesComplete()` → `MissionObjectivesComplete` signali), ekran esa qachon chiqishni hal qiladi. **Bonus faqat "Bazaga qaytish" bosilganda to'lanadi** (`FinishMission()`), shuning uchun ekrandagi raqamlar hali beriladigan mukofot. Vazifa mukofoti toast'i tushishi uchun 1.2 s kutiladi.
  To'liq halqa render bilan tekshirildi: missiya → map → vazifa (₳50 + ×2 tezlashuv) → ekran → qaytish (₳100 → ₳180, ya'ni +80 bonus; faol missiya tozalandi; dunyo `TestArena` ga qaytdi).
  ✅ **Missiyani tashlash** (2026-07-17): pauza menyusida "Missiyani tashlash" (oltin rang + "BONUSSIZ" izohi) — `AbortMission()` ga ulandi. Tugma faqat `MissionManager.ActiveMission != null` bo'lganda ko'rinadi (`Toggle()` da hisoblanadi), hubda yashirin. Render bilan ikkala holat ham tekshirildi; abort'da bonus to'lanmasligi o'lchandi (₳0 → ₳0, faol missiya tozalandi, dunyo `TestArena` ga qaytdi).
  ⏳ **Qolgan qarzlar:** (1) `MissionsPanel` (jurnal) hamon **vazifalarni** sanaydi, missiyalarni emas — endi missiyalar maplarga egalik qilgani uchun mantiqan u `MissionData` ni ko'rsatishi kerak. (2) Hubda vazifa qolmadi: MVP tsikli endi faqat missiya orqali. (3) **Missiya ichida saqlash nomuvofiq:** `SaveData` da faol missiya yo'q — missiya ichida saqlab, keyin yuklasangiz, pul/XP tiklanadi, lekin dunyo hub bo'ladi.
  ⚠️ **Eslatma — render tekshiruvi save'ga yozadi:** `SaveManager` oyna yopilganda saqlaydi, Movie Maker esa `--quit-after` bilan shunday chiqadi. Ya'ni har render `user://savegame.json` ni yaratadi. Sessiya oxirida o'chirish kerak, aks holda playtest g'alati holatdan boshlanadi.

- [~] **Qo'shimcha — Humvee g'ildiraklari** — 2026-07-17 (kod tayyor, o'lchandi, playtest kutilmoqda)
  ✅ `Humvee.obj` ichida obyektlar allaqachon ajratilgan ekan: `body`, `glass`, `tires`, `rims`, `bolts`, `spring`, `axles`. `tires` klasterlarida X (−53…55) va Z (−78…91) bo'shliqlari topildi → to'rt kvadrant.
  Yaratildi: `humvee_body.obj` (7253v) + `humvee_wheel_fl/fr/rl/rr.obj` (har biri 1652v, origin g'ildirak o'qida). **Asl `Humvee.obj` tegilmadi.**
  ✅ `Vehicle.cs`: `_rollingWheels` (X bo'ylab aylanadi, tezlik/radius), `_steeringWheels` (Y, old juftlik), `_wheelRadius=0.445`, `_maxSteerDegrees=28`, `_steerResponse` (yumshoq yaqinlashish). Tank uchun ro'yxatlar bo'sh — zanjirli texnikada g'ildirak yo'q.
  Sahna: `WheelFrontLeft/Spin/Mesh` — tashqi node buriladi (rul), ichkisi aylanadi (yurish), mesh 180° flip + 0.0139 masshtab bilan.
  **Diqqat:** model 180° burilgani uchun modelning "chap oldi" (`humvee_wheel_fl.obj`) — mashinaning **o'ng oldi**. Sahnada shunga qarab ulangan.
  Tekshirish: raqam bilan o'lchandi — 0.55 s da 6 m/s tezlikda to'rttala g'ildirak 386° aylandi, old ikkitasi 15° ga burildi (28° ga yaqinlashmoqda), orqadagilar 0°.

- [~] **Qo'shimcha — Personaj almashtirish tizimi** — 2026-07-17 (backend tayyor, Nathan orqali tekshirildi; 2-geroy fayl kutmoqda)
  Foydalanuvchi so'rovi: ikkinchi geroy + animatsiyalar. **Muhim kashfiyot:** Nathan FBX skeleti Renderpeople sxemasi (`rp_nathan_animated_003_walking_hip`), Mixamo/KayKit'niki emas — shuning uchun tayyor animatsiya **retarget'siz tushmaydi**. Yechim: "ikkinchi geroy" — CC0 personaj o'z animatsiyalari bilan keladi (retarget shart emas).
  ✅ **Fayldan mustaqil backend (autoload YO'Q):**
  • `PlayerController.Locomotion` (`LocomotionState`: Idle/Walk/Run/Jump) — `IsOnFloor`, tezlik, `run` action'dan hisoblanadi.
  • `CharacterData` (`[GlobalClass]`): model sahnasi, masshtab/burilish/offset, klip nomlari (idle/walk/run/jump), root motion track, material teksturalari. `ClipFor(state)` — run/jump yo'q bo'lsa walk'ga, u ham yo'q bo'lsa idle'ga tushadi.
  • `CharacterData.ModelScene` **null** bo'lsa = "player sahnasidagi tayyor modelni ishlat" (Nathan shunday, 180° flip saqlanadi); to'ldirilgan bo'lsa = yangi model instansiya qilinadi (2-geroy).
  • `CharacterRosterData` (geroylar ro'yxati), `CharacterRoster` (statik: `SelectedId`, menyudan gameplay'ga sahna almashuvida saqlanadi — autoload shart emas), `CharacterRig` (modelni yuklaydi + holatga qarab klip ijro etadi, crossfade bilan).
  • `CharacterVisual.cs` endi ishlatilmaydi (`CharacterRig` o'rnini bosdi) — o'chirilmadi, zararsiz.
  ✅ **Nathan yangi tizim orqali:** `character_nathan.tres` (ModelScene null, WalkClip "Take 001", root motion track, teksturalar). Player.tscn'da `CharacterVisual` → `CharacterRig`. Render bilan tasdiqlandi — teksturali, tabiiy turish.
  ✅ **Geroy tanlash menyusi:** bosh menyuda "GEROY" tugmasi → `HeroSelectPanel` (qog'oz uslubi, missiya taxtasi asosida). Geroylarni ro'yxatdan ko'rsatadi, kliplarini yozadi, "TANLANGAN" belgisi, tanlov `CharacterRoster.SelectedId` ga yoziladi. Render bilan tasdiqlandi (hozir faqat Nathan). Menyu 8 tugmaga siqildi (balandlik 56→50, 64→60, oraliq kichraytirildi).
  ⏳ **2-geroy fayl kutmoqda:** foydalanuvchi KayKit (yoki boshqa CC0 GLTF, o'z animatsiyalari bilan) yuklab `assets/models/character/<nom>/` ga qo'yadi. Keyin men: klip nomlarini o'qib `character_<nom>.tres` yarataman, roster'ga qo'shaman, masshtab/burilishni render bilan sozlayman.

- [~] **Qo'shimcha — Tank rang variantlari** — 2026-07-17 (kod tayyor, raqam+render bilan tasdiqlandi, playtest kutilmoqda)
  ⚠️ **Kashfiyot:** `t90a.obj` dagi 3 tank **aynan bir xil** (o'lchamlari tiyin-tiyin teng: Xspan 3.909, Yspan 9.818, Zspan 4.192, hammasi). Nusxa, turli model emas. Assetlarda **boshqa tank modeli yo'q** (faqat T-90; Humvee g'ildirakli; qolganlari aviatsiya). Foydalanuvchi bilan kelishildi: haqiqiy model o'rniga **rang variantlari**.
  ✅ `Vehicle.cs` ga ixtiyoriy livery: `_liveryTint` (default oq = o'zgarishsiz, Humvee'ga ta'sir yo'q) + `_liveryMeshes`. `ApplyLivery()` da materialni `Duplicate()` qilib albedo rangini ko'paytiradi (tekstura saqlanadi — camo detali qoladi, faqat rang o'zgaradi).
  ✅ Hubda 3 haydaladigan tank: **Yashil** (standart, 22,0,-12), **Cho'l** (qumrang 0.88/0.76/0.48, 30,0,-12), **Qish** (oq-kulrang 0.82/0.86/0.92, 38,0,-12). Har biri minora aylantiradi + to'p otadi.
  Tasdiqlash: LIVERY logi (tint qo'llandi, hasTexture=True) + render (yashil vs qumrang tank yonma-yon, aniq farq).

- [~] **Qo'shimcha — Texnika tanlash menyusi** — 2026-07-17 (kod tayyor, panel+funksiya render/log bilan tasdiqlandi, playtest kutilmoqda)
  Geroy menyusi bilan bir xil naqsh. **Tanlangan texnika missiya boshlanganda o'yinchi yonida paydo bo'ladi** (5m nariroq) — bu menyuga real ma'no beradi (avval missiyalarda texnika yo'q edi).
  ✅ **Backend (autoload YO'Q):** `VehicleData` (`[GlobalClass]`: id, nom, tur, tavsif, texnika sahnasi, `LiveryTint`), `VehicleRosterData`, `VehicleRoster` (statik `SelectedId`; bo'sh = piyoda). 6 `.tres`: Piyoda, Humvee, T-90 Yashil/Cho'l/Qish (tank variantlari bir Tank.tscn + turli tint).
  ✅ **`MapLoader.SpawnChosenVehicle()`** — missiya map'i yuklanganda tanlangan texnikani spawn'ga qo'yadi (`_vehicleSpawnOffset`). Tint tree'ga qo'shishdan **oldin** `vehicle.Set("_liveryTint", ...)` bilan beriladi (aks holda `_Ready` oq bilan ishlaydi). Hub'da spawn qilinmaydi (u yerda texnika allaqachon bor). Piyoda = hech narsa.
  ✅ **UI:** `VehicleSelectPanel` + `VehicleSelectRow` (qog'oz uslubi). Bosh menyuda **"ANGAR" → "TEXNIKA"** ga almashtirildi (Angar endi ortiqcha: geroy+texnika+avia demo uni qopladi; `HangarScreen` instansiyasi va wiring olib tashlandi, `.tscn`/`.cs` fayllari qoldi lekin ishlatilmaydi). Menyu 8 tugmada qoldi.
  Tasdiqlash: panel render (5 variant, Piyoda TANLANGAN) + funksional log (cho'l tank tanlandi → missiyada Tank o'yinchi yonida (5,0,6), o'yinchi (0,0,6)).
  ⚠️ **Eslatma:** missiyada texnika har safar yangi spawn bo'ladi (holat saqlanmaydi) — normal. `SaveData` da tanlangan geroy/texnika yo'q — o'yinni qayta ochsangiz tanlov nolga qaytadi. → ✅ **TUZATILDI** 2026-07-28 (Sessiya 15, 5-qism): `SelectedHeroId`/`SelectedVehicleId` save'ga qo'shildi.

- [~] **Qo'shimcha — Uchish tizimi** — 2026-07-18 (kontroller tayyor, Cessna to'liq ishlaydi; qolgan 3 aviatsiya kutmoqda)
  ✅ **`Aircraft.cs`** — arcade uchish, ikki rejim (`FlightMode.Plane` / `Helicopter`), `IInteractable` (kirish/uchish/chiqish, `Vehicle` kabi). **Yangi input YO'Q** — mavjud action'lar: W/S pitch (plane) yoki oldinga/orqaga (heli), A/D roll (plane) yoki yaw (heli), jump ko'tarilish, run pasayish (heli) / gaz (plane).
  • **Plane:** burun qayerga qarasa, o'sha yerga uchadi (velocity = `-basis.Z * speed`); pitch butun tanani buradi; roll — mesh'da vizual bank, bank miqdori yaw'ni (koordinatali burilish) haydaydi. Stall/aerodinamika yo'q (arcade).
  • **Helicopter:** yaw bilan buriladi, oldinga egilib uchadi, jump/run bilan ko'tarilish/pasayish; mesh nishab bilan egiladi (vizual).
  ✅ **`FlightCessna.tscn`** — to'liq ishlaydi. Tuzilish: `MeshRoot` (bank uchun) → `BodyOrient` (orientatsiya tuzatuvi) → `Body` (mesh). Hubda (40,1,-30), aerodrom yonida.
  ⚠️ **Orientatsiya tuzatuvi (muhim):** Cessna modelining burni tabiiy `-X` ga qaragan, uchish esa `-Z` kerak. `BodyOrient` da `Transform3D(0,0,-1, 0,1,0, 1,0,0)` (−90° Y). Chase-render bilan tasdiqlandi (orqadan: dum ko'rinadi, burun ekranga). Tekshirish: level uchish log — input'siz `-Z` bo'ylab 22 m/s, balandlik ushlanadi.
  ✅ **Qolgan 3 aviatsiya qo'shildi** (2026-07-18): `FlightKi61.tscn` (plane), `FlightBell.tscn` + `FlightSeahawk.tscn` (helicopter). Hubda aerodrom yonida: Ki61 (50,1,-30), Bell (40,1,-42), Seahawk (52,1,-42).
  **Orientatsiya tuzatuvlari (chase-render bilan aniqlangan, har birining `BodyOrient` da):**
  • Cessna: burun `-X` → `Transform3D(0,0,-1, 0,1,0, 1,0,0)` (+90°)
  • Ki-61: burun `+X` → `Transform3D(0,0,1, 0,1,0, -1,0,0)` (−90°)
  • Bell: burun `+X` → `Transform3D(0,0,1, 0,1,0, -1,0,0)` (−90°)
  • Seahawk: burun `+Z` → `Transform3D(-1,0,0, 0,1,0, 0,0,-1)` (180°)
  Uchalasi chase-render bilan tasdiqlandi (orqadan: dum ko'rinadi, burun ekranga). Vertolyot rejimi runtime'da tekshirildi (Seahawk ko'tarilish: y 1.9→7.9, 6 m/s).
  ✅ **Rotorlar aylanadi** (2026-07-18): `Rotor.cs` — node'ni lokal o'q atrofida doimiy aylantiradi (`_axis`, `_speed` rev/s).
  • **Bell:** FBX'da rotorlar allaqachon alohida node (`MainRotor`, `RearRotor`) — ajratish shart emas. `.tscn` da instansiya bolasiga script override qo'shildi: MainRotor Y o'qi (5 rev/s), RearRotor X o'qi (7 rev/s). Tepadan render bilan tasdiqlandi — asosiy rotor markazda X shaklida vertikal aylanadi.
  • **Seahawk:** OBJ bitta mesh'ga birlashgani uchun `TopRotor`/`TailRotor` guruhlari ajratildi → `seahawk_body.obj` (74k face), `seahawk_toprotor.obj` (origin hub'da), `seahawk_tailrotor.obj`. Pivotlar vertex'lardan hisoblandi (top: model (0,40.3,25.3), tail: (-2.3,47.9,-100.5)); lokal koordinataga masshtab 0.0827 bilan o'tkazildi. Top Y o'qi (5 rev/s), tail X o'qi (8 rev/s). Frame 1 vs 3 render bilan tasdiqlandi (X aylanmoqda). Asl `Seahawk.obj` tegilmadi (prop hamon undan foydalanadi).
  ⏳ Rotor faqat doimiy aylanadi (uchishga bog'liq emas) — parked'da ham aylanadi, arcade uchun maqbul.
  ⚠️ **Ma'lum cheklovlar:** (1) Plane kirgach darrov 22 m/s harakatlanadi (gaz 0 dan emas) — yerda ham yuradi, pitch bilan ko'tariladi. (2) Balandlik cheki/qo'nish yo'q. (3) Uchish faqat hubda (missiyalarda emas). Bularni playtest fikridan keyin sozlaymiz.

- [~] **Qo'shimcha — Playtest tuzatishlari (6 ta)** — 2026-07-18 (kod tayyor, render bilan tekshirildi, playtest kutilmoqda)
  Foydalanuvchi playtest feedback'i asosida:
  ✅ **(1) Rotorlar faqat uchganda aylanadi:** `Rotor.cs` `Aircraft` ajdodini topib `IsFlown` ni tekshiradi; kirsangiz asta tezlashadi, chiqsangiz sekinlashadi (`_spinResponse`). Parked'da aylanmaydi.
  ✅ **(2) Bell rotori xato aylanardi — tuzatildi:** AABB tahlili — Bell FBX rotor node'lari diski lokal Z'da yupqa, aylanish o'qi = Z. `MainRotor`/`RearRotor` `_axis` (0,1,0)/(1,0,0) → **ikkalasi (0,0,1)**. Yon render bilan tasdiqlandi: avval disk chayqalardi (diagonal), endi gorizontal tekis aylanadi. (Seahawk Y-up bo'lgani uchun (0,1,0) da qoladi — to'g'ri edi.)
  ✅ **(3) Qurol qo'lda xato turardi — tuzatildi:** `weapon_revolver.tres` `GripRotationDegrees` (0,0,0) → **(-90,0,0)**. 6 variant render qilib tanlandi. Orqadan (o'yin ko'rinishi) revolver qo'lda, stvol oldinga — tabiiy.
  ✅ **(4+5) Idle: oyoq qimirlashi va T-poza — tuzatildi:** sabab — idle'da walk klipi ijro etilardi (fallback) va boshida bind-poza (T) ko'rinardi. `CharacterRig`: idle'da (idle klipi yo'q bo'lsa) walk klipini `_idleFrameSeconds`=0.2s da muzlatadi (`FreezeStanding`), `_Ready`'da ham darhol qo'llanadi (T-poza chaqnamaydi). Walk klipini 12 kadr render qilib 0.2s — qo'llar tushgan, oyoqlar birga standing poza tanlandi.
  ✅ **(6) Geroy tanlashda ko'rinish (preview):** `HeroPreview.cs` + hero panelda SubViewport (own_world_3d, o'z environment/kamera/yorug'lik). Tanlangan geroy 3D'da asta aylanib turadi, standing pozada (T-poza emas). `CharacterData.PreviewModel` (Nathan = FBX). Panel ikki ustunli: chapda ro'yxat, o'ngda KO'RINISH qutisi. Render bilan tasdiqlandi.

- [~] **Qo'shimcha — Playtest tuzatishlari 2 (3 ta)** — 2026-07-18 (kod tayyor, render/log bilan tekshirildi, playtest kutilmoqda)
  ✅ **(1) Samolyot roll teskari edi — tuzatildi:** `Aircraft.FlyPlane` da `RotateY(-_roll*...)` → `RotateY(_roll*...)`. Chap bosilsa chapga (yaw oshadi), o'ng bosilsa o'ngga. Bank vizual ham mos (koordinatali): chap → chap qanot past → chap burilish. Log + orqa render bilan tasdiqlandi.
  ✅ **(2) Vertolyot Up/Down:** Input Map'ga strelkalar qo'shildi — Up→jump (ko'tarilish), Down→run (pasayish). Endi strelkalar bilan vertikal, W/S bilan oldinga/orqaga, A/D yaw, Space/Shift ham vertikal ishlaydi. (`project.godot` tahrirlandi — jump: Space+Up, run: Shift+Down. Yon ta'sir: piyoda Up=sakrash, Down=yugurish — kichik.)
  ✅ **(3) Idle oyoqlari birga emas edi — tuzatildi:** `_idleFrameSeconds` 0.2 → **0.1** (CharacterRig + HeroPreview). 0.2s'da o'ng oyoq oldinga ko'tarilgan (staggered), 0.1s'da qo'llar tushgan + oyoqlar eng birga. Sikl bo'ylab render solishtirib tanlandi (0.58s eng yomon — passing pose staggered).

- [~] **Qo'shimcha — Playtest tuzatishlari 3** — 2026-07-18
  ✅ **(1) Personaj kameraga ergashadi:** `PlayerController` da visual endi harakat yo'nalishiga emas, **kamera rigi yaw'iga** qaraydi (har doim, idle'da ham). Sichqoncha qayerga qarasa personaj o'sha yerga qaraydi, WASD kameraga nisbatan strafe. Raqamli test bilan tasdiqlandi (rigYaw=1.0 → visualYaw=1.0).
  ✅ **(1b) A/D burilish regressiyasi — tuzatildi:** kamera-follow'dan keyin A/D faqat strafe qilib burmay qoldi. **Gibrid** yechim: `horizontal.Length() > _idleThreshold` bo'lsa body harakat yo'nalishiga (`Atan2(-x,-z)`) buriladi (A/D buradi), aks holda kamera yaw'iga (sichqoncha nishonlaydi). Oldinga yurish yaw'i = kamera yaw'i, shuning uchun ikki holat orasida sakrash yo'q (matematik tasdiqlandi).

- [~] **Qo'shimcha — Qurol tizimi (o'q otish, B1)** — 2026-07-18 (kod tayyor, funksiya runtime bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  Foydalanuvchi: AK-47, AWP snayper va boshqa qurollardan otish. Modellar allaqachon bor edi (`assets/models/weapons/`: revolver, pistol, glock, ak47, awp) + prop sahnalari (`scenes/props/Weapon*.tscn`).
  ✅ **`WeaponData` kengaytirildi** (data-driven, oltin qoida 5): `Category` (Pistol/Rifle/Sniper/Shotgun), `FireRate` (o'q/sek), `Automatic` (bosib turish), `MagazineSize`, `ReloadSeconds`, `PelletsPerShot` (miltiq), `SpreadDegrees` (tarqalish konusi), `AimFov` (o'ng-tugma zum, AWP snayper uchun). Grip maydonlari saqlandi.
  ✅ **`WeaponController` qayta yozildi:** endi **loadout** (`WeaponData[]`) — bir nechta qurol. **1-5 raqamlar** bilan almashtirish (modelni qayta biriktiradi), **avto/yarim-avto** o'q kadensiyasi (`FireRate`), **patron + R bilan reload** (bo'shaganda avto-reload), **o'ng-tugma zum** (`AimFov`, kamera FOV MoveToward bilan), **tarqalish konusi** (`ApplySpread`, miltiq/avtomatda). HUD uchun `Current`/`Ammo`/`MagazineSize`/`IsReloading`/`ReloadProgress` + `player_weapon` guruhi.
  ✅ **5 qurol `.tres`:** pistolet M1911 (24 dmg, yarim), Glock 18 (20, tez), Xizmat revolveri (34, 6 o'q), **AK-47** (32 dmg, avtomat, 220m, tarqalish 1.6, zum 55), **AWP snayper** (120 dmg, 500m, zum 22, 5 o'q). Player.tscn loadout: [pistolet, glock, revolver, AK, AWP].
  ✅ **Input Map** (oltin qoida 4): `reload`=R, `aim`=o'ng-tugma, `weapon_1..5`=1-5 raqamlar.
  Tasdiqlash (runtime log): loadout=5 yuklandi; pistoletdan otish patron 8→7; slot 4 → AK-47 (30/30, avtomat, 220m); slot 5 → AWP (120 dmg, zum 22). Render: AK o'ng qo'lda ko'rinadi.
  ⏳ **Grip nozik sozlanmadi:** hamma qurol grip'i revolverning ishlagan qiymatidan (`(-90,0,0)`, pos 0, scale 1) boshlaydi. Idle poza (qo'llar past) tufayli miltiq yonda osiladi — nishon-olish animatsiyasi yo'q, shuning uchun tabiiy. **Otish kamera markazidan** ishlaydi (model pozasidan mustaqil). Har qurol grip'i playtest'da editorda nozik sozlanadi (`weapon_<nom>.tres` GripPosition/Rotation/Scale — kod emas, data).
  ⏳ **Qolgan (B2/B3):** qurollarni qo'lga olish/almashtirish HUD ipuchi, HUD patron ko'rsatkichi, otish uchun nishonlar (tir/manekenlar). Hozir faqat mavjud `DestructibleProp` larga otiladi.

- [~] **Qo'shimcha — Missiya framework (data-driven, A1+A2)** — 2026-07-19 (kod+kontent tayyor, uchdan-uchgacha tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  Maqsad: 10 map × 3 vazifa = 30. Yechim **to'liq data-driven** (oltin qoida 5) — 10 qo'lda sahna emas, bitta generator + 50 resurs fayl.
  ✅ **`MapData` resursi** (`[GlobalClass]`): era nomi, yer o'lchami/rangi, osmon (top/horizon/ground), atmosfera (ambient/fog/density), quyosh (rang/energiya/burilish), spawn pozitsiyasi, va **parallel massivlar** — `Tasks[]` (EngineeringTaskData) + `TaskPositions[]` (PackedVector3Array) + `TaskRotationsY[]`, hamda `CratePositions[]` (buziladigan sandiq = nishon), `GrovePositions[]`.
  ✅ **`GenericMissionMap.cs` (`: MissionMap`)** — runtime'da `MapData` dan dunyoni kod bilan quradi: yer (Box+kolliziya), `WorldEnvironment` (osmon+tuman), `DirectionalLight` quyosh, `Spawn` node (MapLoader topadi), har vazifa uchun `Workstation` (`_taskData` tree'ga qo'shishdan **oldin** `Set()` bilan beriladi), sandiq/daraxt proplari. So'ng `base._Ready()` vazifalarni yig'ib tugash mantiqini ulaydi. MapData'ni `MissionManager.ActiveMission.Map` dan oladi (yoki `_previewMap` — sahna yolg'iz ochilsa).
  ✅ **Bitta sahna hammaga:** `scenes/maps/GenericMissionMap.tscn` (Workstation+Crate+Grove proplari ulangan). `MissionData` ga `Map` (MapData) maydoni qo'shildi; barcha 10 missiya `MapScene = GenericMissionMap.tscn` + har biri o'z `Map` resursi. `MapLoader`/`MissionManager` **o'zgartirilmadi** (arxitektura mos keldi).
  ✅ **`scenes/world/DestructibleCrate.tscn`** — prop (DestructibleProp, 45 HP, +₳8, ImpactFx ulangan). Nishon sifatida ham xizmat qiladi (qurollar bilan otish).
  ✅ **10 era × 3 vazifa = 30 kontent** (generator `/tmp/gen_campaign.py`, `resources/data/{tasks,maps,missions}/` ga yozdi): 1900 Imperiya → 1914 BJU → 1939 IJU → 1943 qish fronti → 1965 sovuq urush → 1980 cho'l → 1999 zamonaviy → 2015 kiber → 2030 yaqin kelajak → 2050 kelajak bazasi. Har era o'z rang mavzusi, 3 muhandislik vazifasi (TA'MIRLASH/O'RNATISH/QURILISH/ISTEHKOM/XAVFSIZLIK), mukofot era bo'ylab oshadi (vazifa ₳44→₳170, bonus ₳72→₳270).
  ✅ **Board ulandi:** `Main.tscn` `MissionBoardPanel._missions` endi 10 kampaniya missiyasi (eski 3 o'rniga).
  Tasdiqlash (runtime log + render): mission e1 boshlandi → GenericMissionMap 3 vazifa + 3 sandiq + 2 daraxtzor qurdi, spawn (0,1,6), o'yinchi joyiga ko'chdi; render'da yashil 1900 yeri, osmon, daraxtlar, sandiqlar, vazifa va HUD tracker ("Generatorni ta'mirlash / 12 M") ko'rindi.
  ⏳ **Eski maplar ishlatilmaydi:** `MapRepair/Telegraph/Trench.tscn` + ularning mission/task `.tres` lari endi board'da yo'q (zararsiz, qoldi). Kelajakda o'chirsa bo'ladi.
  ✅ **Jurnal endi missiyalarni ko'rsatadi** (2026-07-19): `MissionsPanel` hardcode 3 task fayl o'rniga `[Export] _missions` (MissionData massivi, MainMenu.tscn'da 10 kampaniya missiyasi ulangan). `MissionRow.Bind` endi `MissionData` qabul qiladi — missiya nomi, to'liq mukofot (vazifa mukofotlari + bonus), "n/3 vazifa" progress, va missiya to'liq bajarilganda (barcha vazifa) "BAJARILDI" shtampi. `MissionsPanel.IsMissionComplete()` — barcha vazifa ID'lari `MissionManager.IsTaskCompleted` bo'yicha tekshiriladi. Render bilan tasdiqlandi: 10 missiya (1900 +₳240 → 2050), "0/3 vazifa", bo'sh doiralar.
  ⏳ **Qolgan (A3 va sozlash):** (1) missiya tavsifi era nomini takrorlaydi (kichik ortiqchalik — xohlansa yaxshiroq briefing yoziladi); (2) vazifa/sandiq joylashuvi hamma era uchun bir xil layout — xohlansa har era uchun turlicha qilinadi; (3) missiya ichida saqlash hamon nomuvofiq (`SaveData` da faol missiya yo'q) → ✅ **TUZATILDI** 2026-07-28 (Sessiya 15, 5-qism).

- [~] **Qo'shimcha — Dushman NPC (otish nishoni)** — 2026-07-19 (kod tayyor, runtime+render bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  ✅ **`Enemy.cs` (`CharacterBody3D, IDamageable`)** — otib o'ldiriladigan nishon: zarar oladi (tanani qizil emissiya bilan chaqnatadi), boshida sog'liq bar ko'rsatadi (o'q tekkanda paydo bo'ladi, chapdan kamayadi, yashil→qizil), o'lganda **mukofot** (+₳15/+8 XP), FX chiqarib **yiqiladi** (Tween: visual X o'qida 90° ag'daradi) va o'zini `QueueFree` qiladi. O'limda kolliziya layer/mask 0 ga tushadi (boshqa o'q tegmaydi, o'yinchini bloklamaydi). Turgan joyda qoladi (yurish animatsiyasi/hujum yo'q — sof nishon, MVP).
  ✅ **`Enemy.tscn`** — Sophia modeli (mavjud `NpcVisual` qayta ishlatildi) + kapsula kolliziya + `HealthBar` (Bg + Fill quad, `billboard_mode=1`, unshaded). Dushmanni do'st hub NPC'sidan ajratish uchun tana **qizil tint** (`_bodyTint`, albedo modulate — do'st NPC `Enemy.cs` ishlatmaydi, o'zgarishsiz).
  ✅ **Data-driven joylashuv:** `MapData` ga `EnemyPositions` (Vector3[]) qo'shildi; `GenericMissionMap` `_enemyScene` ni har pozitsiyaga spawn qiladi. 10 map'ning har biriga 3 dushman qo'shildi. Dushmanlar **missiya tugashiga shart emas** (sandiq kabi ixtiyoriy mukofot nishoni) — missiya hamon 3 muhandislik vazifasi bilan tugaydi.
  Tasdiqlash (runtime): mission e1 da 3 dushman spawn bo'ldi; birini 110 zarar bilan urdim → o'ldi, pul 0→15. Render: 2 Sophia-dushman map'da turibdi, zararlanganida tepasida sog'liq bar ko'rindi. Build toza.
  ✅ **Dushman AI + o'yinchi sog'lig'i** (2026-07-20): `Enemy.cs` endi o'yinchini **quvlaydi va otadi** — `_detectRange`(26m) ichida sezadi, yuzini o'giradi, `_attackRange`(15m) gacha yaqinlashadi, so'ng `_fireInterval`(1.7s) da otadi (LOS raycast — devor/sandiq to'sadi), muzzle+tracer FX bilan. Sof turgan nishon o'rniga tirik dushman. (Sophia idle animatsiyasi — yurganda biroz siljiydi, yurish klipi yo'q; arcade uchun maqbul.)
  ✅ **`PlayerHealth.cs`** (Player'da, `player_health` guruhi): sog'liq, dushman zarari, urishdan keyin `_regenDelay`(4s) o'tib regen (7/s), o'lganda **respawn** (spawn'ga ko'chirib, to'liq sog'liq, `_spawnGrace`(2s) himoya). HUD'ga sog'liq bar qo'shildi (`TopLeft/Health`, `UpdateHealth()` guruh orqali poll qiladi, "SOG'LIQ n/100" + qizil bar).
  Tasdiqlash (runtime): e1 da 3 dushman o'yinchini otdi (HP 100→82→55→28→1→respawn 100), respawn tsikli ishladi. Zarar defaultlari yumshatildi (6.5 dmg, 1.7s) — 3 dushman birdan otmasin.

- [~] **Qo'shimcha — Jurnal scroll + per-era layout + briefinglar** — 2026-07-20 (kod+kontent tayyor, render bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  ✅ **Jurnal scroll bug — tuzatildi:** 10 missiya ekrandan toshib, Close tugmasi va footer ko'rinmasdi. `MissionsPanel.tscn` + `MissionBoardPanel.tscn` da mission ro'yxati endi `ScrollContainer` (balandligi 620, `horizontal_scroll_mode=0`) ichida; header (Close) va footer tashqarida qoladi. `_rowsPath` ikkala skriptda `.../Scroll/Missions` ga yangilandi. Render: YOPISH tugmasi, o'ng scrollbar, "BAJARILDI: 0/10" footer ko'rindi.
  ✅ **Per-era layout xilma-xilligi:** kampaniya qayta generatsiya qilindi (`/tmp/gen_campaign2.py`) — har 10 era **turlicha** vazifa/sandiq/daraxt/dushman joylashuvi (yoy, chap/o'ng klaster, chuqur chiziq, keng flank, qamal va h.k.). Avval hammasi bir xil edi.
  ✅ **Noyob briefinglar:** har missiya endi era nomini takrorlamaydigan o'ziga xos brifing matniga ega (masalan "Bo'linma daryo bo'yida to'xtab qoldi. Ponton ko'prik yig'ing...").
  Tasdiqlash: e4 (qish) render — oq qor yeri, qorli daraxtlar, chuqur-chiziq layout (sandiqlar simmetrik chapda/o'ngda), uzoqda qizil dushmanlar, HUD sog'liq bar. Build toza.
  ⏳ **(2) Samolyot qanotlari qimirlashi — bajarilmadi:** qanotlar bitta `body` mesh'ga qo'shilgan (Cessna faqat 2 guruh: body, glass) — toza ajratib bo'lmaydi. Wing-flex uchun qanot mesh'ini ajratish kerak (og'ir/xavfli) yoki alohida model. Hozircha qoldirildi.
  ⏳ **(3) Samolyot parragi aylanishi — infratuzilma tayyor, joylashtirilmadi:** parrak ham body mesh'ga qo'shilgan (Cessna'da modelning o'z static parragi bor). `PropBlades.tscn` yaratildi (yarim shaffof aylanuvchi parrak, `Rotor._hideWhenParked` bilan faqat uchganda ko'rinadi). LEKIN headless renderda burunga aniq joylashtira olmadim (koordinata mapping bilan qayta-qayta kurashdim). **Editorda oson:** `FlightCessna/Ki61.tscn` ga `scenes/world/PropBlades.tscn` ni bola qilib qo'shib, burun uchiga sudrab qo'yish kerak (spin/hide mantiq'i ishlaydi). Yoki alohida parrak modeli.
  ⚠️ **Coordinate mapping muammosi:** headless render + `--write-movie` bilan kamera/node joylashtirish juda ko'p urinish talab qildi (Transform3D bazis konventsiyasi chalkash). Editorda live ko'rish bu ishlarni osonlashtiradi — 3D joylashtirish ishlarini foydalanuvchi editorda qilgani samaraliroq.

- [~] **Qo'shimcha — ChronoShift TZ: resurs + ishlab chiqarish + inventar + vaqt modeli** — 2026-07-28 (kod+kontent tayyor, runtime va render bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  Manba: `docs/ChronoShift_MVP_TZ.md`. **Shart: mavjud narsalar va asosiy mantiq buzilmaydi** — hech qanday mavjud tizim olib tashlanmadi, faqat qo'shildi (bitta istisno: vaqt tezlashuvi, quyida).
  ✅ **Yangi data (oltin qoida 5 — hammasi `.tres`):** `ItemData` (`[GlobalClass]`, `ItemCategory`: Resurs/Uskuna/Asbob/Kvest + `SortOrder` + `Tint`), `RecipeData` (Inputs/InputAmounts parallel massivlar, Output, CraftSeconds). Kengaytirildi: `EngineeringTaskData` (`RequiredItem`/`RequiredAmount`/`ConsumesRequirement`/`AdvanceDays`), `MissionData` (`EraYear`), `MapData` (`ResourceNodes`/`ResourceNodePositions`/`ResourceNodeAmounts`/`ResourceNodeUses`, `HasWorkshop`/`WorkshopPosition`/`WorkshopRotationY`, `Recipes`).
  ✅ **`InventoryManager` (yangi autoload, TechTree'dan keyin / SaveManager'dan oldin):** id→son hisobi + `res://resources/data/items/` katalogi (save id bo'yicha tiklanadi, resurs referenslarisiz). `Add`/`TryConsume`/`CanCraft`/`TryTakeInputs`/`OwnedIn`/`AllResources`, `InventoryChanged` + `ItemGained` signallari. `SaveData.Inventory` qo'shildi — eski save'lar bo'sh sumka bilan yuklanadi.
  ✅ **Yig'ish:** `ResourceNode.cs` + `ResourceNode.tscn` (`IInteractable`, `E` bosilsa darhol beradi — TZ 7-bo'lim: "Подойти. Нажать Е."). 3 marta yig'iladi, tugagach kulrang bo'lib pasayadi. Rangi `ItemData.Tint` dan.
  ✅ **Ishlab chiqarish:** `Workshop.cs` + `Workshop.tscn` (g'ishtli sex greybox), `WorkshopPanel` + `RecipeRow` (qog'oz uslubi). TZ 8-bo'limi so'zma-so'z: buyurtma tanlanadi → material olinadi → progress bar → "ISHLAB CHIQARISH TUGADI" → uskuna avtomatik inventarga. **Detal bo'yicha yig'ish YO'Q.** Panel yopilsa ham buyurtma fonda tugaydi (`process_mode` Always).
  ✅ **Inventar:** `InventoryPanel` + `I` tugmasi (Input Map, qoida 4) — TZ 9-bo'limidagi 4 bo'lim (Resurslar/Uskuna/Muhandislik asboblari/Kvest). Boshqa panel ochiq bo'lsa ochilmaydi (pauza tekshiruvi).
  ✅ **Sikl yopildi:** har missiyaning **2- va 3-vazifasi** uskuna talab qiladi (1-vazifa ochiq — kelishi bilan tiqilib qolmaslik uchun). Talab bajarilmasa vazifa **fokusda qoladi** (`CanInteract` o'zgarmadi) lekin `Interact` boshlamaydi — HUD "KERAK: <uskuna> ×1 (0/1)" deb oltin rangda yozadi. Boshlanganda uskuna sarflanadi.
  ✅ **Vaqt modeli qayta ishlandi** (foydalanuvchi: "vaqt tezlashib ketaverishi xato"): `TimeManager` dan `TimeScale`/`ScalePerTask`/`TimeScaleChanged` **olib tashlandi**. Endi kalendar juda sekin dreyf qiladi (`_driftDaysPerSecond=0.02` — faqat soat yurishi uchun) va **sakraydi**: `AdvanceDays()` har bajarilgan vazifada (`EngineeringTaskData.AdvanceDays`, era bo'yicha ~3.5–6 yil), `AdvanceToYear()` missiya boshlanganda (`MissionData.EraYear`). **Hech qachon orqaga qaytmaydi** — eski missiyani qayta o'ynash kampaniyani orqaga surmaydi.
  ✅ **HUD:** `CHIDAM` bar (yangi `PlayerController` stamina: yugurganda kamayadi, to'xtaganda tiklanadi, 0 da "winded" — `_staminaResume`=18 gacha yugurib bo'lmaydi), chapda `MATERIAL` paneli (4 resurs, TZ tartibida), `×N` badge → **`DAVR <yil>`** plitasi, tezlashuv flash'i → **yil sakrash flash'i**, `E` prompti endi barcha `IInteractable` lar uchun (vazifa/kon/mastserskaya), toast'lar 4 tada cheklandi (yig'ishda ekranni bosib ketmasin).
  ✅ **O'yin boshlanishi:** `MissionBoardPanel` endi Main.tscn ochilganda **o'zi ochiladi** va har missiya tugagach qayta ochiladi (`_openOnStart` export, `MissionFinished` ga ulangan). Hubdagi taxta prop'i o'zgarishsiz qoldi.
  ✅ **Kontent (generator `tools/gen_production.py` — loyihada saqlanadi, `python3 tools/gen_production.py` bilan qayta ishga tushiriladi, idempotent):** 10 `ItemData` (4 resurs + 5 uskuna + 1 asbob), 6 `RecipeData` (jumladan TZ'ning aynan misoli: piyoda jihozi = Temir ×20 + Yog'och ×10 + Ko'mir ×5), 10 map'ning har biriga **6 kon + 1 mastserskaya + 6 buyurtma** (pozitsiyalar mavjud obyektlardan ≥5 m uzoqda, seed'li — takrorlanadi), 30 vazifaga `AdvanceDays`, 20 tasiga uskuna talabi, 10 missiyaga `EraYear` (1900/1914/1939/1943/1965/1980/1999/2015/2030/2050). Hub'ga ham mastserskaya (-6,0,-5) + 6 kon qo'yildi.
  **Tasdiqlash (vaqtinchalik headless harness, keyin o'chirildi):** e1 → yil 1900 da qoldi, 6 kon + 1 sex + 6 buyurtma qurildi, vazifa 1 ochiq / 2–3 qulflangan; yig'ish → yog'och 30 / temir 24 / ko'mir 12 / tosh 15; "O'rnatish komplekti" ishlab chiqarildi → qulf ochildi; vazifa boshlandi (uskuna 1→0), tugadi → **yil 1900→1903**, pul +56. e5 → boshlanishda yil **1900→1965**, vazifadan keyin 1968. Render: missiya taxtasi o'zi ochildi, HUD (SOG'LIQ/CHIDAM/MATERIAL/DAVR 1900), mastserskaya paneli (buyurtmalar + "ISHLAB CHIQARILMOQDA 25%"), sumka (4 bo'lim). `dotnet build` 0/0.
  ⏳ **Qolgan/e'tibor:** (1) TZ'ning "kichik yaxlit karta" (lager→shaxta→ko'prik→avanpost bitta mapda) **bajarilmadi** — bizda hub + 10 alohida map, foydalanuvchi shuni saqlashni so'radi; (2) TZ'ning sanoat davri estetikasi (bug', otlar, arava) qo'shilmadi — model yo'q, WoE 1900+ texnikasi qoladi; (3) tez slotlar (TZ 14) qo'shilmadi → ✅ **BAJARILDI** (3-qism); (4) missiya ichida saqlash hamon nomuvofiq → ✅ **TUZATILDI** (5-qism).

- [~] **Qo'shimcha — ChronoShift TZ: NPC'lar + final missiya** — 2026-07-28 (kod+kontent tayyor, raqam va render bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  TZ 3-bo'limidagi majburiy ro'yxatning **oxirgi ikki band**i: "Несколько NPC" va "Финальная миссия". Shu bilan TZ'ning 11 ta majburiy bandidan **11 tasi qoplandi**.
  ✅ **`NpcData` (`[GlobalClass]`):** NpcId, DisplayName, `RoleLabel` (bosh ustidagi plita), `NpcBehavior` (Work/Patrol/Guard), `Tint`, MoveSpeed, PatrolRadius, PauseSeconds, ScanDegrees, `MissionLine`, `UsesWalkModel`.
  ✅ **`Npc.cs` (`CharacterBody3D`)** — TZ 12-bo'limidagi to'rt xatti-harakat: **ishlash** (postda turadi), **yurish** (o'z nuqtasi atrofida `PatrolRadius` ichida tasodifiy nuqtalarga boradi, har birida pauza), **postda turish** (`ScanDegrees` ichida sekin o'ngu-so'lga qaraydi), **missiyaga reaksiya** (vazifa bajarilganda o'yinchiga qaraydi va plitada o'z gapini 4 s ko'rsatadi; faqat `_reactRange`=18 m ichidagilar javob beradi).
  ✅ **Ikki rig:** `Npc.tscn` (Sophia, turgan poza) va yangi `NpcWalker.tscn` (Nathan, yuruvchi). `NpcVisual` ga **ixtiyoriy** `_driveFromMotion` qo'shildi — tezlik bo'lsa yurish klipi ijro etiladi, to'xtasa 0.1 s kadrida muzlaydi. Default o'chiq, shuning uchun hub NPC'si va **dushmanlar o'zgarmadi**.
  ✅ **6 rol (`resources/data/npcs/`):** ISHCHI, MUHANDIS, ASKAR (post), OFITSER, AHOLI (yuradi), ALOQACHI (yuradi). Har biri o'z rangi va gapi bilan. Dushman patruli — mavjud `Enemy` (o'zgartirilmadi).
  ✅ **Joylashuv:** hubda 7 ta (ofitser taxta yonida — mavjud NPC shunga aylandi, muhandis sexda, 2 ishchi konlarda, askar postda, aholi va aloqachi yuradi); har missiya map'ida 4 ta (`MapData.Npcs`/`NpcPositions`/`NpcRotationsY`, generator joylashtiradi — mavjud obyektlardan ≥3.5 m).
  ✅ **Final missiya:** `MissionData.IsFinal` (e10 = 2050), `MissionManager.CampaignCompleted` signali + `CampaignComplete` bayrog'i. Final tugaganda `MissionFinished` **chiqarilmaydi** — shuning uchun missiya taxtasi ostidan ochilib qolmaydi. Yangi `CampaignCompletePanel`: "KAMPANIYA / TUGADI", `1900 — 2062` yillar oralig'i, statistika (vazifa soni · yil · texnologiya · pul · daraja) va TZ 18-bo'limining tezisi.
  Tasdiqlash (vaqtinchalik harness, keyin o'chirildi): hubda 7 NPC, map'da 4 NPC; 7.5 s da yuruvchilar 5.13 m va 3.63 m yurdi, turg'unlar 0.00 m; vazifa bajarilganda yettalasi ham o'z gapini plitaga chiqardi. Final oqim: e10 `IsFinal=True` → 3 vazifa → tugash ekrani → "Bazaga qaytish" → `CampaignCompleted` chiqdi, yopilish ekrani ko'rindi, yil 2050→2062. Render: lager (sex, ofitser plitasi, 5 NPC, texnika). `dotnet build` 0/0.
  ⚠️ **Tutilgan xato:** `Label3D` da `fixed_size=true` — plitalar ekran bo'yicha o'lchamda chizilib, butun ekranni to'sib qo'ygan edi. Olib tashlandi; endi `pixel_size=0.0042` bilan dunyo o'lchamida va `visibility_range_end=26 m` bilan faqat yaqinda ko'rinadi. Render bo'lmaganda sezilmasdi.
  ⏳ **Cheklov:** NPC'da "ishlash" animatsiyasi yo'q (assetda faqat idle + walk klipi bor) — ishchi turgan pozada qoladi. Haqiqiy ish animatsiyasi uchun yangi klip kerak.

- [~] **Qo'shimcha — ChronoShift TZ: syujet + tez slotlar + tutun** — 2026-07-28 (kod tayyor, render bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  TZ'ning qolgan uchta aniq bandi (13-bo'lim syujet, 14-bo'lim "снизу: быстрые слоты", 15-bo'lim "дым, пар").
  ✅ **Ochilish brifingi (TZ 13):** `GameIntro` (statik bayroq, autoload emas) + `IntroPanel` — yangi o'yin boshlanganda chizma-qog'oz kartochkasi chiqadi: "1900 · SHTAB BUYRUG'I № 1 / HARBIY MUHANDIS", TZ 13-bo'limining zavyazkasi (armiya chekinmoqda, ishlab chiqarish vayron, sizni muhandis etib tayinlaydilar) va vazifalar ketma-ketligi. "Vazifani qabul qilaman" → to'g'ridan-to'g'ri missiya taxtasi ochiladi. **Save yuklanganda chiqmaydi** (`GameIntro.Pending` faqat "Yangi o'yin"da yoqiladi).
  ✅ **Taxtada kampaniya holati:** `MissionBoardRow.Bind(mission, done, next)` — bajarilgan missiya "· BAJARILDI" (qizil) + tugmasi "QAYTA O'YNASH", birinchi bajarilmagani "· KEYINGI" (ko'k). Taxta endi tekis menyu emas, davom etayotgan kampaniya kabi o'qiladi. Missiyalar **qulflanmaydi** — tanlash erkinligi saqlandi.
  ✅ **Tez slotlar (TZ 14):** HUD pastida markazda 5 slot — raqam, qurol nomi, patron (`8/8`). Faol slot oltin rangda va to'liq yorqin, qolganlari xira. `WeaponController` ga `Loadout`/`SelectedIndex`/`AmmoIn(slot)` qo'shildi (faqat o'qish). Chiplar loadout'dan bir marta quriladi, keyin faqat patron va yoritish yangilanadi.
  ✅ **Tutun (TZ 15):** `scenes/fx/Smoke.tscn` — `CPUParticles3D` (`gl_compatibility` qarori bo'yicha GPU emas), radial `GradientTexture2D` bilan yumshoq puf. Mastserskayaga g'ishtli mo'ri qo'shildi, tutun undan chiqadi — hub va 10 map'da avtomatik.
  ⚠️ **Tutilgan xatolar (uchalasi ham faqat render'da ko'rindi):** (1) tutun avval katta kulrang **kublar ustuni** bo'lib chizilgan edi — tekis quad'ga yumshoq tekstura kerak ekan; (2) intro matnida `avanpost` — rus tilidagi TZ'dan kirill harflari ko'chib qolgan edi (butun `scenes/`+`resources/` kirillga tekshirildi, boshqa yo'q); (3) slot chiplar 124 px da qurol nomini kesardi → 144 px.
  ⏳ **TZ 15 dan qolgani:** temirchilik, arava, ot, daryo, loyli yo'l — model yo'q. Bug' (par) alohida emas, tutun bilan bir xil effekt.

- [~] **Qo'shimcha — Playtest tuzatishlari 4 (qurol + interaktsiya)** — 2026-07-28 (kod tayyor, runtime+render bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi playtest feedback'i
  🔴 **ILDIZ BUG — INTERAKTSIYA BUTUNLAY ISHLAMAGAN.** Foydalanuvchi "missiyani bajarib bo'lmayapti" va "resurslar qanday yig'iladi?" dedi. Sabab bittasi: **`PlayerInteraction` ning `Area3D` signallari statik obyektlarni umuman ko'rmagan.** O'lchov: o'yinchi vazifadan 1.4 m da turganda `GetOverlappingBodies()` faqat `[Player]` qaytardi, ayni o'sha joyda `IntersectShape` esa `[Ground, Workstation, Player]` topdi. Ya'ni `E` **hech qayerda** ishlamagan — na vazifa, na kon, na mastserskaya, na missiya taxtasi, na texnika. (Hubda ham takrorlandi, demak map generatoriga aloqasi yo'q.)
  ✅ **Yechim:** `PlayerInteraction` `body_entered`/`body_exited` signallaridan **har kadr shakl so'roviga** (`IntersectShape`) o'tkazildi. Reach sahnadagi `CollisionShape3D` dan o'qiladi (editor manba bo'lib qoladi). Yon foyda: PROGRESS'dagi eski "eski focus ro'yxati eskirishi mumkin" qarzi ham yopildi — ro'yxat har kadr qayta quriladi, map almashganda o'chirilgan obyektga ishora qolmaydi.
  Tasdiqlash: hub taxtasi → `Missiyalar`, map vazifasi → boshlandi, kon → bir bosishda yog'och 0→5, mastserskaya → fokusda. **Bu bitta tuzatish foydalanuvchining 4- va 5-savolini ham yopadi: resurslar `E` bilan yig'iladi, shunchaki ishlamagan.**
  ✅ **Otish geometriyasi (markazdagi "tutun"):** avval nur **kameradan** otilardi, tracer esa qo'ldan — shuning uchun chaqnash personajning ustida, ekran markazida paydo bo'lardi. Endi ikki bosqich: (1) kameradan nishon nuqtasi topiladi, (2) haqiqiy o'q **stvoldan** o'sha nuqtaga uchadi. `_muzzleReach` (0.45 m) bilan chaqnash qo'ldan oldinda tug'iladi. Yon foyda: o'yinchi to'siq orqasida turganda o'q endi to'siqqa tegadi (avval kameradan o'tib ketardi).
  ✅ **Nishonga olish kamerasi:** `PlayerCamera` ga `_aimOffset` (0.65, 0.25, 0) + `_aimSpringLength` (1.8) qo'shildi — o'ng-tugma bosilganda kamera yaqinlashib o'ng yelka ustiga suriladi. Render bilan tasdiqlandi: avval personaj nishonni to'sardi, endi nishon ochiq.
  ✅ **Otishda qo'l ko'tariladi:** rigda nishonga olish klipi yo'q, shuning uchun `upperarm_r`/`lowerarm_r` suyaklari kodda pozalanadi (`SetBonePoseRotation`), otishda qaytarma (`_recoilDegrees`) bilan. ⚠️ **Tutilgan xato:** avval `GetBoneRest()` dan blend qilgandim — bu rig'da rest = **T-poza**, shuning uchun qo'l yon tomonga otilib ketardi. Endi nishonga olish boshlanganda **joriy animatsiya pozasi** olinadi va undan blend qilinadi. To'g'ri o'q render bilan tanlandi (4 nomzod solishtirildi): `_upperArmAimDegrees = (0, -62, -8)`.
  ✅ **Qurolni qo'lga olish:** `WeaponPickup.cs` + `WeaponPickup.tscn` — `IInteractable`, `E` bilan olinadi (mashinaga o'tirgandek). `WeaponController.AddWeapon()` loadout'ga qo'shadi va darhol qo'lga oladi (takror olinmaydi). `WeaponRack` dagi 4 dekorativ prop **olinadigan** qurolga aylantirildi (pistolet/glok/AWP/AK). HUD ipuchi: "Qurolni olish — AK-47 / RIFLE · 32 ZARAR".
  ✅ **AK grip o'qi:** o'lchandi — AK modeli **X o'qi** bo'ylab (boshqa qurollar Y/Z), shuning uchun umumiy `(-90,0,0)` unga to'g'ri kelmagan. `(0,90,0)` ga o'zgartirildi.
  ⏳ **QOLDI — editorda sozlash kerak (kod emas, data):** har qurolning `GripPosition` i hamon `(0,0,0)`. AK ning pivoti mesh **markazida** bo'lgani uchun qurol qo'ldan biroz ajralib turadi (render'da ko'rindi). Tuzatish: `resources/data/weapons/weapon_*.tres` → `GripPosition`/`GripRotationDegrees` ni editorda jonli ko'rib surish. Headless render bilan buni ko'r-ko'rona sozlash juda ko'p urinish talab qiladi (oldingi sessiyalar ham shunga duch kelgan).
  ⏳ **Qolgan cheklov:** nishonga olish pozasi faqat o'ng qo'lni buradi — chap qo'l qurolni ushlamaydi (ikki qo'l uchun IK yoki haqiqiy klip kerak).

- [~] **Qo'shimcha — Missiya oqimi tugallandi (saqlash/tiklash/qarzlar)** — 2026-07-28 (kod tayyor, uchdan-uchgacha real input bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi ("missiya bajarishdagi barcha jarayonlarni tugatish")
  Interaktsiya tuzatilgach, missiya sikli o'ynaladigan bo'ldi — qolgan teshiklar yopildi. Bularning uchtasi PROGRESS'da uzoq turgan **ochiq qarz** edi.
  ✅ **Missiya ichida saqlash to'g'ri ishlaydi** (eski qarz): `SaveData` ga `ActiveMissionId` + `RunTaskIds` qo'shildi. `MissionManager` endi `res://resources/data/missions/` katalogini skanlaydi va id bo'yicha missiyani tiklaydi; `MapLoader._Ready` faol missiya bo'lsa hubga emas, **o'sha map'ga** qaytaradi (`ResumeActiveMission`). Avval: missiya ichida saqlab qayta kirsangiz pul/XP tiklanardi, lekin dunyo hub bo'lardi.
  ✅ **Takror-mukofot ekspluatatsiyasi yopildi** (eski qarz): vazifa bajarilganligi endi ikki joyda — `_completedTaskIds` (kampaniya bo'ylab, jurnal uchun) va `_runTaskIds` (**joriy o'yin sessiyasi**). `EngineeringTask._Ready` `IsTaskDoneThisRun` ni tekshirib o'zini yashil/bajarilgan holatda tiklaydi. Ikki to'plam ajratilgani muhim: aks holda missiyani "QAYTA O'YNASH" bosganda hamma vazifa allaqachon yashil bo'lib, map darhol tugab qolardi. `StartMission` run to'plamini tozalaydi, `FinishMission`/`AbortMission` ham.
  ✅ **Geroy/texnika tanlovi saqlanadi** (eski qarz): `SaveData.SelectedHeroId`/`SelectedVehicleId`. Avval o'yinni qayta ochsangiz tanlov Nathan/Piyodaga qaytardi.
  ✅ **Missiyani tashlaganda taxta qaytadi:** `MissionAborted` signali qo'shildi (`FinishMission` bilan aralashtirmaslik uchun alohida — bonus to'lanmaydi), `MissionBoardPanel` ikkalasiga ham ulangan.
  ✅ **Tugash aniqligi:** `MissionMap._Ready` da `CheckFinished()` deferred chaqiriladi — barcha vazifasi bajarilgan holatda saqlangan o'yin tiklanganda `TaskCompleted` chiqmasdi, map "tugagan lekin xabar bermagan" holatda qotib qolardi.
  ✅ **HUD missiya hisoblagichi:** o'ng yuqorida "MISSIYA · <nom> · n/3" (faqat missiya ichida ko'rinadi). `MissionMap.CompletedCount`/`TaskCount` orqali.
  Tasdiqlash (uchdan-uchgacha, **haqiqiy `E` bosishi bilan**): vazifa boshlandi → tugadi (+₳50) → saqlandi (`activeMission='m_e1'`, `runTasks=1`) → holat tozalandi → diskdan yuklandi (missiya, progress, pul tiklandi) → map qayta yuklandi → **1/3 vazifa bajarilgan holatda tiklandi** (takror-mukofot bloklandi), HUD hisoblagichi 1/3. Render: missiya paneli va vazifa progressi ko'rindi. `dotnet build` 0/0.

- [x] **Qo'shimcha — Interaktsiyaga tayangan tizimlarni to'liq tekshiruv** ✅ 2026-07-28 (real input bilan o'lchandi)
  Interaktsiya uzoq vaqt buzuq bo'lgani uchun **unga tayangan hamma narsa hech qachon sinalmagan edi** — foydalanuvchi ular haqida shikoyat qilmagan, chunki u yergacha yeta olmagan. Hammasi bittalab haqiqiy `E`/kirish bilan tekshirildi:
  ✅ Humvee: `Repair` → `Drive` → haydash (10,0,-5)→(10,0,-13) → `F` bilan chiqish
  ✅ Tank: haydaladi, to'p **otadi** (o'qlangan→bo'sh, reload 9% da o'lchandi)
  ✅ Cessna: uchadi; `W` bilan ko'tariladi (y 1.00→2.79), `S` bilan pastlaydi (2.79→0.30)
  ✅ Mastserskaya: `E` panelni ochadi (o'yin pauzada), `Esc` yopadi va pauzani oladi
  ✅ Qurol javoni: fokus + ipuchi, `E` bilan qurol qo'lga olinadi
  ⚠️ **Ikki "bug" mening o'lchov xatoim bo'lib chiqdi** — hubda 3 ta tank bor, men boshqasining to'pini tekshirgan edim; samolyot `jump` bilan emas, pitch (`W`) bilan ko'tariladi. Ikkalasi ham aslida to'g'ri ishlayapti.
  ✅ **Boshlang'ich qurol to'plami qisqartirildi** (`Player.tscn`): pistolet + revolver. Glok/AK/AWP endi **javondan olinadi**. Sababi: avval o'yinchi 5 qurolning hammasiga ega edi, shuning uchun "qo'lga olish" hech narsa bermasdi (o'lchov: loadout 5→5). Endi 2→3 va qurol darhol qo'lga olinadi. Xohlasangiz `Player.tscn` dagi bitta qatordan qaytariladi.

- [x] **Qo'shimcha — To'liq missiya uchdan-uchgacha o'ynaldi + jang balansi** ✅ 2026-07-28
  Butun missiya **faqat haqiqiy kirish va haqiqiy UI bosishlari** bilan o'ynaldi (teleport faqat yurish o'rniga; interaktsiya, tugmalar, panellar — hammasi real yo'ldan):
  ```
  missiya boshlandi (3 vazifa, 6 kon)
  yig'ildi: yog'och 30 · temir 24 · ko'mir 12 · tosh 15
  mastserskaya E bilan ochildi → 2 uskuna ishlab chiqarildi → ikkala qulf ochildi
  vazifa 1 ✓ (hp 100) · vazifa 2 ✓ (hp 75) · vazifa 3 ✓ (hp 55)
  tugash ekrani chiqdi (₳168, 1910-yil) → "Bazaga qaytish"
  bazada: faol missiya tozalandi, ₳240 (bonus +72 to'landi), taxta qayta ochildi
  ```
  🔴 **Topilgan balans muammosi:** birinchi urinishda **3-vazifa bajarilmadi**. Sabab kod emas — **dushmanlar o'yinchini o'ldirgan** (hp 94→42→o'lim→respawn; o'yinchi vazifadan 21 m narida spawn'da qolgan). Eski qiymatlar bilan 3 dushman ~11.5 zarar/sek berardi va 26 m sezish radiusi 70×70 map'ning ko'pini qoplardi — ya'ni **jang muhandislik ishini bloklardi**, bu TZ 11-bo'limiga ("бой ... не является его основой") to'g'ridan-to'g'ri zid.
  ✅ **Balans tuzatildi** (`Enemy.cs` defaultlari): sezish 26→**17 m**, hujum 15→**11 m**, otish oralig'i 1.7→**2.4 s**, zarar 6.5→**5.0**. Endi dushman — yoningizdan o'tganda duch keladigan tahdid, butun map bo'ylab fon zarari emas. Qayta o'lchov: o'yinchi uchala vazifani ham bajarib, **55 HP bilan tirik qoldi** — bosim bor, lekin blok yo'q.

- [x] **Qo'shimcha — Qurol grip'i hisoblab chiqarildi (5 qurol)** ✅ 2026-07-28 (render bilan tasdiqlandi)
  Grip qiymatlari **taxmin bilan emas, matematik yechilgan**. Oldingi urinishlar shuning uchun ishlamagan.
  ⚠️ **Ildiz sabab — o'lchov xato edi:** avval model o'qini `VisualInstance3D` AABB'sidan olgandim, lekin FBX ichidagi **`Light3D`/`Camera3D` ham `VisualInstance3D`** — ular AABB'ni shishirib yuborgan (pistolet 2×2×2 m chiqqan!). Faqat `MeshInstance3D` bo'yicha qayta o'lchandi:
  | Qurol | o'lcham | uzun o'q | tepa o'q |
  |---|---|---|---|
  | Pistolet M1911 | (0.031, 0.147, 0.200) | **Z** | Y |
  | Glock 18 | (0.029, 0.130, 0.200) | **Z** | Y |
  | Revolver | (0.045, 0.148, 0.304) | **Z** | Y |
  | AK-47 | (0.878, 0.265, 0.030) | **X** | Y |
  | AWP | (0.112, 0.249, 1.324) | **Z** | Y |
  Ya'ni to'rttasi Z bo'ylab, AK X bo'ylab — barchasi Y = tepa. Eski umumiy `(-90,0,0)` **hech biriga** to'g'ri kelmagan.
  ✅ **Yechim (solver):** nishonga olish pozasida qo'l suyagining dunyo bazisi `A` o'qiladi; modelning **o'z** uzun o'qi nishonga, tepa o'qi osmonga qaraydigan maqsad bazisi `W` quriladi; grip burilishi `G = A⁻¹·W` sifatida hisoblanadi. Pozitsiya: qo'l modelning **dastasida** turishi uchun (markazida emas) uzunlik bo'yicha siljitiladi — pistolet uchun orqadan 14%, miltiq uchun 33% — ustiga bilak→kaft qadami (-0.06 m).
  **Ichki tekshiruv:** to'rtala Z-o'qli qurol bir xil burilish oldi `(-2.0, -44.1, 13.5)`, AK esa aynan 90° farq bilan `(13.5, -133.7, 2.0)` — hisob to'g'riligining belgisi.
  ✅ **Render bilan tasdiqlandi:** pistolet — dastasi barmoqlarda, stvol oldinga, tik; AK — gorizontal, **magazin pastga**, qo'ndoq orqada; AWP — qo'ndoq yelkada, optika tepada. Qiymatlar `weapon_*.tres` ga yozildi (data, kod emas — xohlaganda editorda o'zgartiriladi).
  ✅ **Chap qo'l ham qurolga keladi** (2026-07-28, ikkinchi bosqich): `ArmIk.cs` — yopiq shakldagi **ikki-suyakli IK** (kosinuslar teoremasi). Yelka/tirsak/bilak skelet fazosida yechiladi, natija lokal suyak pozasi sifatida qaytariladi, tirsak yo'nalishi pole vektori bilan boshqariladi. Nishon — qurolning **o'z** mesh AABB'sidan hisoblangan tutqich nuqtasi (uzunlikning 52% i), bilakdan kaftgacha 8.5 sm orqaga surilgan.
  ⚠️ **Muhim bog'liqlik topildi:** grip burilishi **nishonga olish pozasiga bog'liq**. Birinchi IK urinishida chap qo'l cho'zilish chegarasiga urilardi, chunki o'ng qo'l to'liq to'g'ri edi va qurol juda uzoqda turardi. Tirsakni bukkanda (`-20°` → `-58°`) qurol tanaga yaqinlashdi, **lekin grip buzildi** — barcha 5 qurol yangi pozaga qayta yechildi. Ya'ni aim pozasi o'zgarsa, grip ham qayta hisoblanishi kerak.
  Render bilan tasdiqlandi: **nishonga olganda** AK gorizontal, magazin pastga, ikkala qo'l qurolda; **oddiy turganda** qurol sonda osilib turadi, ikkala qo'l tabiiy.
  ✅ **Chap kaft ham qurolga buriladi** (2026-07-28, uchinchi bosqich): IK faqat qo'lni yo'naltirardi, kaft esa yurish klipidan qolgan **ochiq** holatda turardi. `ArmIk.OrientBone()` qo'shildi — suyakni yo'nalishga emas, **to'liq orientatsiyaga** buradi.
  Rig o'lchandi: chap kaftda `thumb/index/middle/ring/pinky_01_l` suyaklari bor, `middle_01_l` rest = (0.108, 0.004, −0.006) — ya'ni **barmoqlar kaftning lokal +X o'qi bo'ylab**, bosh barmoq −Z da, demak kaft yuzasi −Z. Shundan maqsad bazis quriladi: barmoqlar qurol bo'ylab ko'ndalang, kaft yuqoriga qurolga qaragan. Nozik sozlash `_supportHandTweakDegrees` orqali (editorda o'zgartiriladi).
  Render bilan tasdiqlandi: AK gorizontal, magazin pastga, o'ng qo'l dastada, **chap kaft tutqichda burilgan holda** — ikki qo'llab ushlash to'g'ri o'qiladi.
  ⏳ **Qolgan nozik:** qurol o'ng barmoqlardan ~2–3 sm oldinda (kaft markazi taxminiy qiymat, `_supportWristPullback`/grip pos bilan sozlanadi).

- [~] **Qo'shimcha — Birinchi shaxs kamerasi, texnika ichidan ko'rish, snayper optikasi** — 2026-07-28 (kod tayyor, render bilan tasdiqlandi, playtest kutilmoqda) — foydalanuvchi so'rovi
  ✅ **Bitta skript, yettala rig:** `PlayerCamera.cs` allaqachon o'yinchi, Humvee, tank va 4 aviatsiyaning `CameraRig` iga ulangan ekan — shuning uchun **bitta o'zgartirish hamma joyda ishlaydi**. Birinchi shaxs = spring arm nolga tushadi + kamera sahnada berilgan "ko'z/o'rindiq" nuqtasiga ko'chadi. `V` bilan almashtiriladi (Input Map, qoida 4).
  ✅ **Piyoda birinchi shaxs (CS2 uslubi):** qurol va ikkala qo'l ekranda ko'rinadi. ⚠️ Dastlabki qurilishda kamera boshdan 0.28 m **oldinda** edi va kadr bo'sh chiqardi — 2026-07-29 da render bilan aniqlanib tuzatildi: ko'z nuqtasi haqiqiy ko'zga qaytarildi, bosh suyagi FP'da siqiladi (aks holda og'iz kadrni to'sadi), qurol qo'li esa ikki-suyakli IK bilan kameraga nisbatan joylashtiriladi (o'lchov: qurol (0.19, −0.22, −0.64) kamera fazosida). Birinchi shaxsda: (a) tana **doim kamera yo'nalishiga** qaraydi (aks holda qurol chapga qarab turadi), (b) qurol pozasi **doimiy ko'tarilgan** (aks holda son yonidagi qurol kadrdan tashqarida qoladi), (c) pitch chegarasi kengayadi (osmonga qarash mumkin).
  ✅ **Texnika ichidan:** har sahnaga o'z `_firstPersonOffset` i berildi (Humvee kabinasi, T-90 komandir lyuki, 4 kabina). ⚠️ **Modellarda ichki qism yo'q** — o'rindiq nuqtasi korpus ichida bo'lsa faqat qorong'i yuzalar ko'rinadi (render bilan tasdiqlandi). Shuning uchun nuqtalar haydovchi ko'zi balandligida, lekin korpusdan tashqarida: Humvee'da kaput ustidan qaraladi, tankda minora ustidan. Bu mavjud assetlar bilan ishlaydigan yagona to'g'ri yechim.
  ⚠️ **Tutilgan bug:** `V` bosilganda **qaysi rig birinchi ushlasa** o'sha almashardi — texnikada haydayotganda o'yinchining yashirin kamerasi tugmani yutib yuborardi. Endi faqat `Current` kamera javob beradi (sichqoncha harakati ham).
  ✅ **Snayper optikasi:** `WeaponData.UsesScope` (AWP'da yoqilgan) + yangi `ScopeOverlay.cs` — ekran `_Draw` bilan chiziladi (tekstura kerak emas): doira tashqarisi qora, nishon chizig'i, masofa belgilari. Optikaga o'tilganda qurol modeli, nishon belgisi va tez slotlar yashiriladi. FOV zumi avvaldan bor edi (AimFov 22), lekin **ko'rinadigan optika yo'qligi uchun "ishlamayapti" deb tuyulardi**.
  ✅ **Render bilan tasdiqlandi va tuzatildi** (2026-07-29): birinchi ko'rishda **o'yinchining qo'li optika ichida turgandi** — kamera yelka ortida qolar, tana nishon bilan orada bo'lardi. `PlayerCamera.ScopeView` qo'shildi (optikada kamera ko'z darajasiga tushadi) va tana yashiriladi. Holat `SetScope`/`ClearScope` ga yig'ildi, qurol yo'qolganda tozalanadi (aks holda tana ko'rinmas bo'lib qolardi). O'lchov bilan tasdiq: optikada `bodyVisible=False`, qo'yib yuborilganda `True`.
  ✅ **Aviatsiya kabinalari o'lchov bilan hisoblandi va to'rttalasi ham render bilan tasdiqlandi** (2026-07-28): Cessna (burun ustidan, ufq ochiq), Ki-61 (o'z burni + parragi kadrda — klassik qiruvchi kabinasi), Bell (ochiq ufq, pastda vint qanoti), Seahawk (burun + vint). Har uchoqning **mesh AABB'si o'lchandi** (Cessna 10.9×2.6×8.3 m, Ki-61 12.1×3.4×8.7, Bell 1.2×2.3×5.7, Seahawk 15.7×5.6×19.8), nuqta fyuzelyaj markaz chizig'ida — korpusdan tashqarida, Humvee'dagi "kaput ustidan" joylashuvning aynan o'zi.
  ⚠️ **Ikki tuzoq:** (1) **Bell mesh'i node markazidan 1.77 m yon tomonga siljigan** — X=0 qo'yilsa kamera fyuzelyajdan tashqarida qoladi; o'lchangan markaz ishlatildi. (2) AABB uchoq **root**iga nisbatan o'lchanadi, lekin `_firstPersonOffset` **`CameraRig`** tugunida, u esa root'dan yuqoriga siljigan (Ki61 +1.6, Bell +2.0, Seahawk +2.5) — qiymatni yozishdan oldin shu farqni ayirish shart. Sozlash kerak bo'lsa: `FlightXxx.tscn` → `CameraRig` → `_firstPersonOffset`, bitta qiymat.

- [x] **Qo'shimcha — UI: qog'oz dizayn → ko'k (blueprint) dizayn** ✅ 2026-07-28 — foydalanuvchi so'rovi
  Foydalanuvchi: "asosiy menyudagi ko'k dizaynni boshqa sahifalarga ham ko'chirish kerak, eski qog'ozga o'xshagan dizayn yoqmayapti".
  ✅ **16 ta ekran** o'tkazildi: missiya taxtasi, jurnal, mastserskaya, sumka, brifing, tex daraxti, saqlash daftari, geroy/texnika tanlash va ularning qatorlari.
  ✅ **Tizimli almashtirish** (qo'lda emas): tema variantlari (`TitlePaperS→TitleL`, `HeadingPaper→Heading`, `BodyPaper→Body`, `MonoPaper→MonoLabel`, `PaperButton→BlueprintOutline`, `StampButton→BlueprintPrimary`) va ranglar (qog'oz krem → blueprint ko'k, quyuq siyoh → yorug' siyoh). Temaga yangi `BlueprintPanel` varianti qo'shildi. `UiPalette` dagi `Paper*` qiymatlari ko'k tonlarga qayta yo'naltirildi — shuning uchun **kod tomonidagi ranglar ham avtomatik ergashdi** (HUD, RecipeRow, InventoryPanel, TechEdges va boshqalar).
  ✅ **Missiya modali kengaytirildi:** 780 → **1180 px**, qator tavsifi 420 → 660 px — kartalar endi sig'adi (foydalanuvchi shikoyati).
  ⚠️ **Tutilgan layout muammosi:** `TitlePaperS` kichik qog'oz sarlavhasi edi, `TitleL` esa ancha katta — tex daraxtida sarlavha ostidagi matnni bosib qoldi. Panel sarlavhalari 40 px ga normallashtirildi.
  ✅ **Barcha 13 ekran render bilan ko'z bilan tasdiqlandi** (2026-07-29, galereya probe'i bilan). Shunda **4 ta ekran umuman o'tmagani** aniqlandi — ular `Paper*` emas, `ModalPrimary`/`ModalButton`/`HudBody` variantlarini ishlatgani uchun skript ularni ko'rmagan: **missiya tugash ekrani**, **kampaniya finali**, **pauza menyusi**, **boshqaruv paneli**. To'rttasi ham blueprint'ga o'tkazildi.
  ✅ **Boshqaruv paneli qayta yozildi** — ingliz tilidan o'zbekchaga, xom action nomlari (`move_forward`) o'rniga odam o'qiydigan nomlar, guruhlarga bo'lindi va **yetishmayotgan 9 tugma qo'shildi** (`aim`, `camera_toggle`, `inventory`, `reload`, `weapon_1..5`).
  ✅ **O'qilish tuzatildi:** o'chirilgan tugma matni 40%→72% shaffoflik; brifing ogohlantirishi quyuq qizildan yorqin rangga.
  ✅ **HUD va toast'lar ham o'tkazildi** (2026-07-29, foydalanuvchi so'rovi bilan): plitalar, ramkalar, XP chizig'i, `DAVR`, pul, `MATERIAL` — blueprint ko'k. **Sog'liq qizil va chidam yashil qoldi** (holat signali, bezak emas), material kvadratchalari ham (`ItemData.Tint`). `Gold` ogohlantirish va bezak ma'nolariga ajratildi (`Warning` / `Highlight`).
  ✅ **Eski tema variantlari o'chirildi** — `ModalPrimary`, `ModalButton`, `ModalPanel`, `PaperPanel`, `JournalPanel` + 7 o'lik uslub. Aynan shular tufayli 4 panel almashtirishdan chetda qolgandi.
  ⚠️ **Tegilmagan:** bosh menyu (dizayn manbai).

- [x] **Qo'shimcha — Demo davomiyligi o'lchandi + investor ssenariysi** ✅ 2026-07-28
  TZ 1-bo'limi aniq talab qo'yadi: **10–15 daqiqalik demo**. Buni hech kim tekshirmagan edi.
  ✅ **O'lchandi** (map ma'lumotlaridan: marshrut uzunligi ÷ tezlik + vazifa/ishlab chiqarish davomiyligi):
  bitta missiyaning **sof** vaqti **1.0–1.3 daqiqa** (yurish 128–198 m, ish 34–40 s), o'ntasi **12 daqiqa**.
  ⚠️ **Muhim xulosa:** bu raqamlarga jang, brifing o'qish, panel bilan ishlash va yo'l qidirish **kirmagan** — real o'yinda har missiya 2–3 daqiqa. Ya'ni **10 missiyaning hammasi 20–30 daqiqa** (TZ oynasidan chiqadi), **3 missiya esa 8–12 daqiqa** (aynan kerakli oyna). Bitta missiya (1.2 daq) juda qisqa — sikl his qilinmaydi.
  ✅ **`docs/DEMO_SCENARIO.md` yozildi** — o'lchovga asoslangan ~12 daqiqalik marshrut: ochilish (1 daq) → 1900 missiyasi (4 daq, **asosiy siklni ko'rsatadi**: ochiq vazifa → qulflangan vazifa → yig'ish → ishlab chiqarish → bajarish) → 1965 (3 daq, **miqyos + jang**) → 2050 final (3 daq, **g'oyani yopadi**) → tex daraxti/jurnal (1 daq). Har qadamda **investorga nima aytish** kerakligi va TZ 16-bo'limining 8 bandi qayerda ko'rinishi yozilgan. Qilmaslik kerak bo'lgan narsalar ham (hammasini o'ynamaslik, yo'l qidirmaslik, o'lib qolmaslik).

- [x] **Qo'shimcha — Render narxi o'lchandi va 70% ga tushirildi** ✅ 2026-07-28
  Loyihaning **asosiy cheklovi zaif hardware** edi (shu sabab Godot + `gl_compatibility` tanlangan), lekin narx hech qachon o'lchanmagan — PROGRESS faqat "FPS tushishi mumkin" deb ogohlantirgan. O'lchadim.
  🔴 **Boshlang'ich holat:** hub **2.22M uchburchak/kadr**, missiya map'i **1.83M**. `gl_compatibility` (OpenGL 3.3, eski GPU'lar) uchun bu og'ir.
  **Aybdorni topish uchun mesh'lar reyting qilindi** (hub, 155 mesh instance, jami 3.21M):
  | uchburchak | soni | manba |
  |---|---|---|
  | **1728k** | ×7 | **Grove** (trees9.obj — 247k har biri) |
  | 462k | ×9 | WeaponRack (qurol modellari) |
  | 268k | ×17 | Airfield (4 aviatsiya) |
  | 252k | ×5 | Player (Renderpeople skan) |
  | 109k | ×6 | Crew (6 NPC) |
  Ya'ni **Grove butun byudjetning 54% i**.
  ✅ **Qilingan ishlar:** (1) `Grove` ga `visibility_range_end = 55 m` + **soya o'chirildi** (`cast_shadow = 0`) — daraxt fon, soyasi 247k uchburchakni ikkinchi marta chizdirardi; (2) hubdagi 7 daraxtzordan uzoqdagi 3 tasi olib tashlandi; (3) yangi `DistanceCull.cs` — node ostidagi **barcha** `GeometryInstance3D` ga masofa chegarasi qo'yadi, **runtime'da qo'shilgan mesh'larga ham** ishlaydi (aynan shu kerak edi: javondagi qurollar va aerodromdagi samolyotlar kod bilan instansiya qilinadi, shuning uchun sahna tahriri ularga tegmagan); (4) qurol modellari 55 m, aviatsiya 120 m, NPC skanlari 45 m dan nariga chizilmaydi.
  ✅ **Natija:**
  | | Boshida | Hozir | |
  |---|---|---|---|
  | Hub | 2222k | **662k** | −70% |
  | Missiya map'i | 1834k | **615k** | −66% |
  | Draw call (map) | 182 | 142 | −22% |
  Render bilan tekshirildi: oddiy o'yin masofasidan **hech narsa yo'qolmagan** — daraxtlar, NPC'lar, sandiqlar joyida.
  ⚠️ **VRAM o'lchanmadi:** monitor 1248 MB ko'rsatadi, lekin ikkala sahnada **bir xil** va diskdagi barcha tekstura atigi 43 MB — demak bu raqam Movie Maker buferlaridan. Haqiqiy VRAM'ni editorda o'ynab (Debug → Monitors) ko'rish kerak.

- [x] **Qo'shimcha — Kampaniya data auditi: bajarib bo'lmaydigan missiya topildi** ✅ 2026-07-28
  Men faqat 1-missiyani to'liq o'ynagandim; qolgan 9 tasida yashirin xato bo'lsa playtest'ning o'rtasida chiqardi. Statik audit yozdim: **har missiyaning qulflangan vazifalari o'z map'ining resurslaridan yasalishi mumkinmi?**
  🔴 **Topildi — e3 (1939, Ikkinchi jahon urushi) bajarib bo'lmaydigan edi:** qulflangan ikki vazifa 30 temir talab qilardi, map esa atigi 24 berardi (2 kon × 4 birlik × 3 marta). O'yinchi hamma konni qazib bo'lib ham uskunani yasay olmasdi — missiya boshi berk.
  ✅ **Yechim ildizda:** `tools/gen_production.py` endi konlarni **talabdan kelib chiqib o'lchaydi** — era'ning qulflangan vazifalari qaysi retseptni talab qilishini hisoblab, kerakli miqdorni ×1.6 zaxira bilan konlarga taqsimlaydi. e3 da temir 24 → 48 bo'ldi. Bu bir martalik tuzatish emas: kontent o'zgarsa ham avtomatik to'g'ri o'lchanadi.
  ✅ **Generatorga o'z-o'zini tekshiruv qo'shildi** (`verify_maps_are_solvable`) — yozgan faylini qayta o'qib, har 10 map uchun "yetadimi?" deb tekshiradi va muammoni baland ovozda aytadi. `python3 tools/gen_production.py` oxirida `verify: all 10 maps solvable` chiqadi.
  Audit natijasi (tuzatishdan keyin): **10/10 missiya o'z map'idan bajarib bo'ladi.**

- [~] **Faza 8 — 3D Models** — boshlandi 2026-07-16 (personaj + to'liq model paketi ulandi, playtest kutilmoqda)
  ✅ Personaj: Renderpeople "Nathan" (riggli, walk anim) — kapsula o'rniga.
  ✅ Model paketi (Sessiya 13): revolver qo'lda (data-driven WeaponData), Humvee (drivable Vehicle), T-90 prop, 4 aviatsiya (Ki-61/Cessna/Seahawk/Bell) Airfield, Sophia NPC (idle), WeaponRack (Pistol/Glock/AWP/AK-47), trees9 Grove.
  Qolgan: workstation/target modellari (placeholder qolmoqda), qo'shimcha muhit.
  → *Natija: real ko'rinish*

---

## Qarorlar jurnali

| Sana | Qaror | Sabab |
|---|---|---|
| 2026-07-13 | **Engine: Godot 4 + C#** | Zaif hardware; Unreal (16GB+ RAM, alohida GPU) va Unity og'ir. C# — mavjud kuch. |
| 2026-07-13 | **Custom engine yozilmaydi** | Noldan engine → vertical slice: 2–4 yil. Opportunity cost juda katta. "Build games, not engines." |
| 2026-07-13 | **MVP: 1900/WWI era** — 3 mission, 1 weapon, 1 vehicle, tech tree, time acceleration, save/load | Scope discipline. Bitta era mukammal ishlasin, keyin kengaytiramiz. |
| 2026-07-13 | **Kontent data-driven (`.tres` Resource)** | Kelajakdagi era'lar = yangi data fayl, kod refactor emas. |
| 2026-07-13 | **Placeholder-first (capsule/cube)** | 3D model kutish kod'ni bloklamaydi. Gameplay loop avval, model keyin. |
| 2026-07-13 | **Environment uchun AI model ishlatilmaydi** | Tayyor kit (Synty POLYGON / Kenney) — AI faqat noyob texnika/qurol uchun. |
| 2026-07-13 | **macOS — release bosqichida** | Godot cross-platform ichida. Code signing + notarize ($99/yil) faqat public release'da. Dev'ni bloklamaydi. |
| 2026-07-14 | **Renderer: `gl_compatibility`** | Zaif hardware qarori davomi: OpenGL 3.3 eng yengil yo'l, eski GPU'larda ham ishlaydi. Kerak bo'lsa Project Settings'da Forward+/Mobile'ga o'tkazish oson. |
| 2026-07-14 | **`Nullable` + `ImplicitUsings` yoqilgan (csproj)** | Null-safety kompilyator darajasida; Godot default template'dan farq qiladi, ongli tanlov. |
| 2026-07-14 | **Godot.NET.Sdk 4.5.0, target net8.0** | `dotnet build` (SDK 10.0.203) bilan tasdiqlangan. Foydalanuvchi Godot 4.5.x .NET o'rnatishi kerak. |
| 2026-07-15 | **TimeScale = maxsus kalendar koeffitsienti (Engine.TimeScale EMAS)** | Global `Engine.TimeScale` player'ni ham tezlashtirib gameplay'ni buzardi. Faqat ichki yil/kun tez oqadi, yurish normal qoladi. Era/tech unlock (Faza 5) shu vaqtga bog'lanadi. |
| 2026-07-28 | **ChronoShift TZ — pivot EMAS, ustiga qurish** | Foydalanuvchi: "WoE da bor narsalar qoladi, TZ da bor bizda yo'q qismlarni qilasan, asosiy logika buzilmasligi kerak". Shuning uchun 10 era, tex daraxti, texnika, aviatsiya, jang — hammasi qoldi; TZ'dan faqat resurs/ishlab chiqarish/inventar/chidam qo'shildi. TZ'ning "bitta kichik karta" va "bitta era" bandlari **ataylab bajarilmadi**. |
| 2026-07-28 | **Vaqt tezlashuvi olib tashlandi → yil sakrashi** | Foydalanuvchi: "o'yin jarayonida vaqt tezlashib ketaverishi xato". Eski model (`TimeScale = 1 + n×1`) cheksiz tezlashardi va o'ynash uzayishi bilan kalendar nazoratdan chiqardi. Endi kalendar mehnat bilan harakatlanadi: vazifa = `AdvanceDays` sakrash, missiya = era yiliga o'tish. Vaqt endi mukofot, fon jarayoni emas. |
| 2026-07-28 | **Uskuna talabi: 3 vazifadan 2 tasi** | TZ sikli (yig'ish→ishlab chiqarish→ishlatish) ko'rinishi kerak, lekin har vazifani qulflash resurs yetmasa missiyani tiqib qo'yadi. 1-vazifa doim ochiq — o'yinchi kelishi bilan nima qilishni biladi, qulf 2-vazifada o'rgatiladi. Talab data'da (`RequiredItem` bo'sh = talabsiz), shuning uchun eski vazifalar buzilmadi. |
| 2026-07-28 | **Mastserskaya har map'da + hub'da** | TZ'da mastserskaya lagerda, lekin bizda missiya alohida map'da o'tadi — sikl yopilishi uchun dala sexi kerak. Aks holda o'yinchi har uskuna uchun missiyani tashlab hubga qaytishi kerak bo'lardi (10–15 daqiqalik demo uchun o'lim). |
| 2026-08-09 | **Loyiha nomi: War of Engineers → ChronoShift** | Ochiq savol (2026-07-28) yopildi. Loyiha `~/PROJECTS/WarOfEngineers` dan `~/PROJECTS/ChronoShift` ga ko'chirildi; C# namespace `WarOfEngineers` → `ChronoShift` (97 fayl), `.sln`/`.csproj`, `assembly_name`, `config/name`, export presetlar (bundle id `com.humoyun.chronoshift`), UI matnlari va docs yangilandi. `dotnet build` — 0 warning / 0 error. **Git tarixi olib kelinmadi** (foydalanuvchi qarori: toza repo) — eski tarix `github.com/HUMO5459/WarOfEngineers` va lokal `~/PROJECTS/WarOfEngineers/.git` da qoldi. Yangi repo: `github.com/HUMO5459/ChronoShift`. Tarixiy PROGRESS yozuvlaridagi "WoE" atamasi ataylab tegilmagan. |

---

## Sessiya jurnali

### Sessiya 16 — 2026-08-03 — Playtest feedback, Blok A (Jang va NPC)

- **Kirish:** Foydalanuvchi to'liq QA ro'yxatini berdi (harakat/missiya/mastserskaya/craft/xarakteristika/texnika/jang/NPC/tech/save/UI). Ro'yxat A–E bloklarga bo'lindi, foydalanuvchi **A** ni tanladi.
- **Bajarildi:**
  - **Dushman orqasi bilan yurardi — ildiz sabab topildi.** `Enemy.FacePlayer` `Atan2(direction.X, direction.Z)` ishlatardi; Godot'da forward = `-Z`, shuning uchun to'g'ri formula `Atan2(-x, -z)` (`PlayerController.cs:127` allaqachon shunday qilardi). Aynan **180° teskari** edi.
  - **Texnika endi buziladi.** `Vehicle : IInteractable, IDamageable` — `_maxHealth` (Humvee 260, T-90 520), zarba tovushi, zarar ortgani sayin korpus qorayadi (livery materiallari endi doim `Duplicate()` qilinib saqlanadi), nolga tushganda: haydovchi tashqariga chiqariladi (aks holda boshqaruv+kamera qulflanib qolardi), portlash FX+tovush, `IsWrecked` → ta'mirlash/kirish bloklanadi. `Destroyed` signali + `VehicleGroup = "vehicles"`.
  - **Dushmanlar texnikaga hujum qiladi.** `Enemy.ResolveTarget()` — o'yinchi texnikada bo'lsa nishon texnika bo'ladi (avval yashirilgan/o'chirilgan player node'iga qarab turardi), zarar `IDamageable` orqali korpusga tushadi.
  - **"Sizni ko'rdi" ko'rinadi.** Dushman sezganda sog'liq bar'i chiqadi + tanasi qizil (`_alertTint`). Avval sezish butunlay ko'rinmas edi.
  - **Dushman otishi eshitiladi.** `_fireSound` (`fire_rifle.wav`) qo'shildi — chaqnash/tracer bor edi, ovoz yo'q edi.
  - **Zarba olish ko'rsatkichi.** `scripts/ui/DamageIndicator.cs` — ekran qizil yuviladi + o'q kelgan tomonga yoy chiziladi (`_Draw`, kamera fazosidan burchak). `PlayerHealth.Damage(amount, source)` + `Damaged(amount, origin, hasSource)` signali. HUD `_Ready` da kod bilan qo'shadi (eng ustki qatlam).
  - **Texnika sog'ligi HUD'da.** `HUD.tscn` ga `Vehicle` paneli (Reload panelidan yuqorida), haydayotganda ko'rinadi.
  - **Toast API.** `scripts/ui/HudNotifier.cs` (statik, `Fx`/`Sfx` uslubida) + `HUD.HudGroup`/`HUD.Notify` — dunyo obyektlari HUD'ga bog'lanmasdan toast chiqaradi. Dushman o'ldirilganda "+₳15 · +8 XP", texnika yo'q qilinganda xabar.
- **Tekshiruv:** `dotnet build` 0/0. Headless `Main.tscn` exit 0 — yangi HUD NodePath'lari xatosiz topildi, yangi ogohlantirish yo'q (faqat mavjud headless `keyboard_get_keycode_from_physical` shovqini). Foydalanuvchi save'i tegilmadi (diff bilan tasdiqlandi — headless run `NotificationWMCloseRequest` ni chiqarmaydi).
- **Blok A davomi (foydalanuvchi so'radi):** `Npc` endi `IDamageable` — 60 HP, zarba chaqnashi (material har instansiyada `Duplicate()` qilinadi, aks holda bitta o'q butun aholi bo'ylab chaqnardi), o'lganda yiqiladi + FX/tovush + `QueueFree`. Mukofot yo'q. **Yon topilma:** `Npc.FaceDirection` da ham aynan o'sha 180° yaw xatosi bor edi — aholi ham orqasi bilan yurgan, tuzatildi.
- **Blok B (Missiya UI) — bir sessiyada davom etdi:**
  - **`MissionBriefingPanel` (yangi, `.cs` + `.tscn`) — bitta ekran ikki rejimda.** *Brifing:* `MissionStarted` da o'zi ochiladi (deferred, chunki map o'sha chaqiruvda instansiya qilinadi), pauza qiladi, tugmasiz yopilmaydi. *Jurnal:* `J` bilan istalgan payt qayta ochiladi, `Esc`/`J` yopadi. Ikkalasida ham vazifalar tartib raqami bilan, **joriy vazifa** ajratilgan (chap qirrasi oltin, foni ko'k, "JORIY"), bajarilgani so'nadi ("BAJARILDI"), uskuna yetishmasa qizil izoh. Qatorlar kodda quriladi (HUD quick-slot uslubi).
    Ma'lumot manbai: tirik `MissionMap.Tasks` (progress bor); u hali qurilmagan bo'lsa `MissionData.Map.Tasks` zaxira sifatida.
    Brifing rejimida `Esc` **yutiladi** — aks holda pauza menyusi allaqachon pauzalangan ekran ustiga chiqib qolardi.
  - **`MissionMap` API:** `Tasks`, `CurrentObjective` (avval boshlangani, bo'lmasa birinchi bajarilmagani), `NumberOf(task)`.
  - **`ObjectiveMarker` (yangi):** joriy maqsad **doim ekranda** — ko'rinsa romb + masofa, ko'rinmasa ekran chetiga qadalgan strelka. Kamera orqasidagi nuqta uchun proyeksiya markazdan aks ettiriladi (aks holda strelka teskari tomonni ko'rsatardi). HUD `_Ready` da kod bilan qo'shiladi.
  - **HUD tracker'i "eng yaqin" dan "joriy maqsad" ga o'tdi** — avval xarita narigi chekkasidagi keyingi vazifa o'rniga yonidagi boshqasini ko'rsatardi. Endi `2/3 · 41 M · TA'MIRLASH` ko'rinishida, masofadan qat'i nazar ko'rinadi (faqat vazifa ustida turganda prompt/progress paneliga joy bo'shatadi). `_trackerHideDistance` olib tashlandi, tracker sarlavhasi `ENG YAQIN VAZIFA` → `JORIY MAQSAD`.
  - **Input:** `journal` = `J` (`project.godot`), HUD ipuchiga qo'shildi. `Main.tscn` `BoardLayer` iga panel instansiyasi.
- **Blok C (Xarakteristikalar) — o'sha sessiyada davom etdi:**
  - **`VehicleData` ga xarakteristikalar:** `MaxHealth`, `MoveSpeed`, `TurnSpeed`, `Armor` (0..0.85 — yutiladigan zarar ulushi), `ProductionCost`, va qurollanish (`ArmamentName/Damage/Range/ReloadSeconds`). **Nol = "ko'rsatilmagan"** → sahnaning o'z qiymati qoladi, shuning uchun yarim to'ldirilgan resurs tankni qog'ozdan qilib qo'ymaydi (`Armor` bundan mustasno — u sahnada yo'q).
  - **Raqamlar ko'rsatkich emas, ishlaydi.** `Vehicle.ApplyData()` data'ni sahna ustiga yozadi; `MapLoader` endi `_liveryTint` o'rniga butun `_data` ni uzatadi (tint ham data ichida). `Vehicle.TakeDamage` da zarar `× (1 − Armor)`. `TankCannon` ham `_damage/_range/_reloadSeconds` ni texnika data'sidan oladi — ekranda ko'rsatilgan raqam bilan otiladigan raqam ajralib qololmaydi.
  - **`WeaponData.ProductionCost`** qo'shildi. Data to'ldirildi: Humvee (260/8.0/10%/₳900), T-90 ×3 (520/5.0/55%/₳3200, 125 mm 120 uron / 400 m / 3.0 s), qurollar ₳120–950. Tezlik/burilish qiymatlari sahnadagi bilan bir xil — boshqaruv o'zgarmaydi.
  - **`StatsPanel` (yangi, `.cs` + `.tscn`) — `C` bilan ochiladi.** Ikki ustun: TEXNIKA (korpus, tezlik, bronya, qurollanish, uron, otish masofasi, qayta o'qlash, ishlab chiqarish narxi) va QUROL (uron, masofa, o't ochish tezligi, magazin, qayta o'qlash, narx). Haydab turganda korpus **tirik** ko'rinadi (`312 / 520`), piyodada depoda tanlangan texnika ko'rsatiladi.
  - **Depo kartochkasi:** `VehicleSelectRow` ga bir qatorli spetsifikatsiya chizig'i (`KORPUS 520 · TEZLIK 5.0 m/s · BRONYA 55% · URON 120 · DALLIK 400 m · NARX ₳3200`). Faqat data'da bor raqamlar chiqadi.
  - **Input:** `stats` = `C`, HUD ipuchiga qo'shildi.
- **Blok E (Save) — o'sha sessiyada davom etdi. Diagnoz o'lchov bilan qo'yildi, kod o'qish bilan emas:**
  - Vaqtinchalik harness (`tools/savetest/`, keyin **o'chirildi**) bilan to'liq round-trip o'lchandi: 777 pul / 250 XP / 3-daraja / 4 vazifa / `m_e2` faol missiya / run task'lari / 120.5 kun / 1 tech / 2 predmet / geroy / texnika → `Save()` → hammasini nolga tushirish → `Load()` → **hammasi bir xil qaytdi**. Ya'ni serializatsiya, `user://` yozuv, `ActiveMissionId` va `RunTaskIds` — hammasi soz edi.
  - **Haqiqiy sabab:** `Save()` deyarli chaqirilmasdi. U faqat vazifa tugaganda, tech ochilganda, oyna yopilganda va pauzadagi "Chiqish"/"Saqlash" da yozardi. **`PauseMenu` dagi "Bosh menyu" tugmasi saqlamasdan sahna almashtirardi** — o'yinchi Esc → Bosh menyu qilsa, oxirgi vazifadan keyingi hamma narsa (missiya bonusi, dushman mukofotlari, yig'ilgan material, ishlab chiqarilgan uskuna, boshlangan missiya) yo'qolardi. Keyin "Yuklash" eski faylni qaytarardi — foydalanuvchiga "saqlash ishlamayapti" bo'lib ko'rinadi.
  - **Tuzatish:** (1) `PauseMenu.OnMainMenuPressed` endi avval `Save()` qiladi. (2) `SaveManager` milestone'larga ulandi: `MissionStarted`/`MissionFinished`/`MissionAborted`/`CampaignCompleted` — darhol yoziladi. (3) Tez-tez o'zgaradiganlar (`InventoryChanged`, `MoneyChanged`, `XpChanged`) `RequestSave()` bilan faqat "iflos" deb belgilanadi; `_Process` uni **20 soniyada bir marta** diskka tushiradi — yig'ish maydonidan yugurib o'tganda har olishga bitta JSON yozuv bo'lmasligi uchun. (4) `SaveManager.ProcessMode = Always` — pauzada ham tiklanadi (o'yinchi aynan pauzadan chiqadi).
  - **Yangi xatti-harakat o'lchandi:** baseline diskda `Money:1` → `AddMoney(500)` (hech kim `Save()` chaqirmaydi) → xotirada 501, **disk hali 1** (debounce ishlayapti) → flush oynasidan keyin **disk 501**. Ya'ni avtosaqlash haqiqatan diskka yetib boradi.
  - **Qamrovdan tashqarida (ataylab):** save hamon **bitta slot**. "Yangi o'yin" boshlab keyin chiqsangiz eski save ustiga yoziladi — bir slotli o'yinlar uchun normal, lekin slot kerak bo'lsa alohida ish.
- **Blok D (Konstruktor) — o'sha sessiyada yakunlandi. Barcha 5 blok tugadi.**
  - **Yangi data tipi `ModuleData`** (`[GlobalClass]`): `Kind` (Vehicle/Weapon), `Slot` (Chassis/Engine/Armor/Armament · Frame/Barrel/Magazine/Sight), xarakteristika deltalari (`HealthBonus`, `SpeedBonus`, `ArmorBonus`, `DamageBonus`, `RangeBonus`, `ReloadDelta`, `MagazineBonus`), narx (`CostMoney` + `Inputs`/`InputAmounts`), `RequiredTechId` (tex daraxti bilan ochiladi). Asos slotlari (`Chassis`/`Frame`) `BaseVehicle`/`BaseWeapon` shablonini olib yuradi — qolgan modullar shu ustiga qo'shiladi.
  - **17 ta modul `.tres`** (`resources/data/modules/`): 2 shassi, 2 dvigatel, 2 zirh, 2 qurollanish, 3 ramka, 2 stvol, 2 magazin, 2 nishon. **Yangi texnika/qurol = yangi `.tres`, kod emas** (5-oltin qoida).
  - **`DesignRegistry` (statik, autoload EMAS)** — `VehicleRoster`/`CharacterRoster` uslubida, shuning uchun **Project Settings'ga qo'lda ro'yxatdan o'tkazish shart emas**. Modul katalogi, saqlangan chizmalar, `Compute()` (xarakteristikalarni yig'adi), `MaterialsFor()`, `SaveDesign()`, `TryProduce()` va tayyor mahsulotlar reyestri.
  - **`DesignRecord` (oddiy POCO)** — chizma faqat modul id'lari sifatida saqlanadi; tayyor texnika/qurol yuklanganda modullardan **qayta quriladi**, ya'ni save faylida dublikat yo'q.
  - **`DesignPanel` (yangi `.cs` + `.tscn`) — `B` bilan yoki mastserskayadagi "KONSTRUKTOR" tugmasi bilan ochiladi.** Uch ustun: MODULLAR (har slot `<`/`>` bilan aylantiriladi; asos slotidan boshqasini bo'sh qoldirish mumkin), XARAKTERISTIKALAR (jonli yangilanadi + material ro'yxati, yetishmasa qizil), SAQLANGAN CHIZMALAR (ochish/o'chirish). Pastda nom maydoni + "CHIZMANI SAQLASH" + "ISHLAB CHIQARISH".
  - **Ishlab chiqarish:** pul + material tekshiriladi **sarflashdan oldin** (aks holda muvaffaqiyatsiz buyurtma materialni yeb qo'yardi), keyin yechiladi. Texnika → depoda tanlanadigan bo'ladi (`VehicleRoster.Resolve` avval qurilganlarni qaraydi, `VehicleSelectPanel` ularni ro'yxatga qo'shadi). Qurol → `WeaponController.AddWeapon` orqali **darhol qo'lga beriladi**.
  - **Saqlash:** `SaveData.Designs` qo'shildi; eski save'lar bo'sh ro'yxat bilan yuklanadi.
  - **O'lchov (vaqtinchalik harness, keyin o'chirildi):** zanjirli shassi + dizel + kompozit + og'ir to'p → korpus 660 / tezlik 6.5 / bronya 0.80 / uron 210 / masofa 660 / o'qlash 3.8 s / narx ₳6100, material `temir×57 yog'och×13 tosh×10`. Ishlab chiqarish pulni 20000→13900 va temirni 200→143 ga tushirdi. Pul yetmaganda **aniq sabab bilan rad etdi** ("Pul yetarli emas — ₳6100 kerak"). Save→wipe→load: 3 chizma ham, `Built` bayrog'i ham, qayta qurilgan texnika (660 HP) va qurol (44 uron / 45 magazin / optika) ham tiklandi. `VehicleRoster` qurilgan texnikani topdi.
  - **Tutilgan xato:** modul `.tres` generatorida argument tartibi chalkashib, tavsif tirnoqsiz `Kind` maydoniga tushgan → 17 fayl ham parse xatosi bergan, `DesignRegistry: loaded 0 module(s)`. Buni **o'lchov topdi** (headless log), kod o'qish emas. Qayta yozilgandan keyin `loaded 17 module(s)`.
- **Muammo:** Yo'q. **Eslatma:** `MissionBriefingPanel.tscn`, `StatsPanel.tscn`, `DesignPanel.tscn` yangi fayllar — `uid` hali Godot keshida yo'q, shuning uchun `Main.tscn` da ularga **uid'siz** (matn yo'l bilan) ishora qilindi; editorda bir marta ochib saqlansangiz Godot uid'ni o'zi yozadi.
- **Balans tuzatishi (foydalanuvchi so'radi, o'sha sessiya):** bronya juda kuchli chiqqan edi (maksimum 0.80). Tushirildi — T-90 `0.55 → 0.35`, Humvee `0.10 → 0.05`, po'lat plita `+0.12 → +0.08`, kompozit `+0.25 → +0.15`. **Eng kuchli konstruksiya endi 0.50** (avval 0.80). Faqat `.tres` o'zgardi, kod tegilmadi — 5-oltin qoida ishlayotganining amaliy isboti.
- **Playtest tuzatishi — pritsel (foydalanuvchi: "pritsel personajga yopishib qolgan, ko'rinmayapti"):** muammo **render bilan tasdiqlandi**, taxmin bilan emas. Vaqtinchalik harness (`tools/aimtest/`, keyin o'chirildi) `aim` tugmasini bosib turadi, ochilish panellarini yopadi (`ProcessMode = Always` — aks holda pauzalangan daraxtda panelni yopolmaydi) va Movie Maker kadr yozadi. **Avvalgi kadr:** personaj kadr markazini egallagan, ko'tarilgan qo'l va tapancha pritsel ustida, pritsel esa 50% shaffof oq `+` — och osmonda deyarli ko'rinmas.
  Tuzatish: `PlayerCamera` ga doimiy `_shoulderOffset` (0.5, 0.15, 0) qo'shildi — endi **nishonga olmaganda ham** kamera yelka ustida turadi, personaj pritsel tagida qolmaydi. Nishonga olganda qo'shimcha (0.5, 0.3, 0) qadam, `_aimSpringLength` **1.8 → 2.8** (avval kamera personajga yopishib, kadrni uning orqasi bilan to'ldirardi). Texnika/samolyot sahnalarida `_shoulderOffset = (0,0,0)` — ular korpus orqasidan yuradi, yelka ustidan emas. Pritsel: oltin rang + qora kontur, 28 → 34 px.
- **⚠️ Render ikkita yashirin bugni ochdi (A va B bloklaridan):** `CanvasLayer` ichida **kod bilan yaratilgan `Control` ning `Size` i (0,0)** bo'lib qolar ekan. Natijada (1) `ObjectiveMarker` da `Clamp(90, -90)` har kadrda `ArgumentException` tashlab, **marker umuman chizilmagan**; (2) `DamageIndicator` ning qizil yuvish to'rtburchagi nol o'lchamda bo'lib, **zarba effekti ko'rinmagan**. Ikkalasi ham endi o'z rektiga emas, `GetViewportRect().Size` ga tayanadi; chekka to'ldirish oyna kichkina bo'lsa ham teskari to'rtburchak hosil qilmaydigan qilib cheklandi. Marker yozuviga kontur qo'shildi. Tuzatilgandan keyingi kadrda marker ("GENERATORNI TA'MIRLASH · 14 M" + olmos) va toza pritsel ko'rinadi. **Bu bugni faqat render topdi — headless log ham, kod o'qish ham topmagan edi.**
- **Yangi Windows MSI (2026-08-04):** `build/War of Engineers - Setup.msi` — **312 MB, 190 komponent, versiya 1.1.0**.
  ⚠️ **Muhim muhit tuzog'i — yana urib qo'yadi:** `--export-release` **`.NET publish` muvaffaqiyatsiz bo'lsa ham exit 0 qaytaradi**. Birinchi urinishda `build/windows/` da faqat `.exe` + `.pck` chiqdi, **`data_WarOfEngineers_windows_x86_64` papkasi yo'q** edi — ya'ni C# assembly'lari umuman eksport bo'lmagan va build Windows'da ishlamasdi. Buni faqat papkani sanab ko'rish topdi.
  **Sabab:** `DOTNET_ROOT` = `~/.dotnet` (SDK 10.0.203), Godot esa `/usr/local/share/dotnet/dotnet` (SDK 9.0.311) ni ishga tushiradi. SDK 9 ning resolver'i `DOTNET_ROOT` dagi .NET 10 assembly'larini yuklamoqchi bo'lib yiqiladi: `MSB4242 ... Could not load 'System.Runtime, Version=10.0.0.0'`.
  **Yechim — eksportni shunday chaqiring:**
  `DOTNET_ROOT=/usr/local/share/dotnet /Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path . --export-release "Windows Desktop" "build/windows/War of Engineers.exe"`
  **Har eksportdan keyin tekshiring:** `ls build/windows/` da uchta narsa bo'lishi shart — `.exe`, `.pck` va `data_WarOfEngineers_windows_x86_64` (187 fayl). Uchinchisi yo'q bo'lsa build yaroqsiz.
  **`tools/make_msi.py` yaxshilandi:** `VERSION` 1.0.0 → **1.1.0** va `<Upgrade>` + `RemoveExistingProducts` qo'shildi. Avval yangi MSI eskisining **yoniga** o'rnatilardi (ikkita papka, ikkita yorliq); endi eskisini o'chirib almashtiradi. MSI ichida tasdiqlandi: `FindRelatedProducts` (25), `MigrateFeatureStates` (1200), `RemoveExistingProducts` (1501), Upgrade jadvali `0.0.0 → 1.1.0`. **Keyingi har tarqatishda `VERSION` ni ko'taring.**
  ⚠️ `build/War of Engineers - Windows.zip` — **eski (29-iyul)**, yangi MSI bilan bir papkada turibdi. Tarqatishdan oldin yangilang yoki o'chiring.
- **Keyingi:** to'liq playtest (A–E). Kodda rejalashtirilgan ish qolmadi.

### Sessiya 15 — 2026-07-28
- **Bajarildi:** Foydalanuvchi `ChronoShift MVP Prototype.docx` (ТЗ, 18 bo'lim, rus tilida) berdi — o'qib chiqildi, `docs/ChronoShift_MVP_TZ.md` ga ko'chirildi va WoE bilan solishtirildi (~60% mos, yetishmayotgani: resurs tizimi, ishlab chiqarish, inventar, chidam). Foydalanuvchi qarori: **pivot emas — mavjud hamma narsa qoladi, yetishmayotgani ustiga qo'shiladi**. Uch dizayn savoli ochiq savol sifatida berildi, uchalasida tavsiya tanlandi (yil sakrashi / 2 vazifa qulflanadi / mastserskaya map'da + hub'da). Keyin to'liq **resurs → ishlab chiqarish → inventar** qatlami + vaqt modeli qayta ishlandi (batafsil: yuqoridagi checklist bandi). Yangi: `ItemData`, `RecipeData`, `InventoryManager` (autoload), `ResourceNode`, `Workshop`, `WorkshopPanel`, `RecipeRow`, `InventoryPanel` + 3 yangi sahna; kengaytirildi: `MapData`, `MissionData`, `EngineeringTaskData`, `EngineeringTask`, `GenericMissionMap`, `TimeManager`, `MissionManager`, `PlayerController` (chidam), `HUD`, `MissionBoardPanel`, `SaveData`/`SaveManager`, `MainMenu`, `PauseMenu`, `MissionsPanel`. Kontent generator bilan: 10 item, 6 retsept, 10 map × (6 kon + sex + 6 buyurtma), 30 vazifa, 10 missiya. `dotnet build` 0/0.
- **Muammo:** (1) `PlayerInteraction` faqat `CanInteract==true` bo'lgan obyektni fokuslaydi — vazifani uskuna yo'qligida `CanInteract=false` qilsak, o'yinchi **nega** ochilmayotganini bilmasdi. Yechim: fokus ochiq qoldi, `Interact` refuses, HUD sababni yozadi. (2) Birinchi render'da HUD tracker'i yangi MATERIAL panelini bosib qolgan edi (ikkalasi ham center-left) — MATERIAL yuqoriga ko'chirildi. (3) Yig'ishda har harvest bitta toast chiqarardi → ekranning o'ng chekkasi to'lib ketardi; toast steki 4 ta bilan cheklandi. Uchalasi ham render orqali topildi.
- **Keyingi:** Playtest. Tekshirish ro'yxati: (a) o'yin ochilganda missiyalar ro'yxati chiqadimi; (b) missiyani boshlaganda yil o'sha eraga o'tadimi (DAVR plitasi); (c) konga `E` bosib material olinadimi (chapdagi MATERIAL paneli o'sadimi); (d) mastserskayaga `E` → buyurtma → "ISHLAB CHIQARISH TUGADI" → `I` bilan sumkada ko'rinadimi; (e) 2-vazifa uskunasiz boshlanmaydi, uskuna bilan boshlanadi va uskuna sarflanadi; (f) vazifa tugagach yil sakraydi (ekran o'rtasida yangi yil); (g) `Shift` bilan yugurganda CHIDAM tugab, yurishga tushadimi; (h) lagerdagi NPC'lar (rol plitasi yaqinlashganda chiqadi, aholi/aloqachi yuradi, askar postda qaraydi, vazifa bajarilganda gapirishadi); (i) 10-missiya (2050) tugagach kampaniya yopilish ekrani chiqadimi.

### Sessiya 15 (24-qism) — 2026-07-29
- **Bajarildi:** O'yin shu paytgacha **butunlay jimjit** edi (0 audio fayl, 0 `AudioStreamPlayer`). Foydalanuvchi bergan `Silantro Flight Simulator Toolkit.unitypackage` (Unity assetlari) ichidan **18 ta ovoz ajratib olindi** va o'yinga ulandi. Toolkitning o'zi ishlatilmadi — uning qiymati 43 Unity MonoBehaviour + 80 ScriptableObject ichida, ular Godot'da ishlamaydi; faqat engine-neytral audio olindi (872 KB).
- **Ulangan joylar:** qurol otishi (har qurolda o'z ovozi — `WeaponData.FireSound`, ya'ni data-driven), o'q tegishi (7 ta variant tasodifiy), gilza tushishi (3 variant), nishon/dushman yo'q qilinishi, HUD toast, va **barcha UI tugmalari**. Yangi: `scripts/fx/Sfx.cs` (mavjud `Fx` uslubida — bir martalik player, o'zini o'chiradi) va `scripts/ui/UiSoundHook.cs`.
- **UI ovozi uchun 13 ta ekran tahrirlanmadi:** `UiSoundHook` `SceneTree.NodeAdded` ni kuzatib har bir tugmani avtomatik ulaydi — runtime'da yaratilgan qatorlar ham qamrab olinadi. Ikkala ildiz sahnaga (Main, MainMenu) bittadan qo'shildi, autoload kerak emas.
- **Muammo:** `shell_1..3.wav` import bo'lmadi — `file` ko'rsatdi: ular aslida **MP3, faqat kengaytmasi `.wav`** (Silantro paketidagi nuqson). `.mp3` ga qayta nomlanib import qilindi.
- **Tasdiq (o'lchov, quloq emas):** probe tugmani bosdi va o'q uzdi — tugmadan keyin 1 `AudioStreamPlayer` **chalinmoqda**, otishdan keyin 3 `AudioStreamPlayer3D` **uchalasi ham chalinmoqda** (otish + gilza + zarba). Keyingi kadrda 0 — bir martalik playerlar o'zini to'g'ri o'chiradi.
- **Eslatma:** `.unitypackage` (117 MB) `.gitignore` ga qo'shildi — u manba arxiv, o'yin uni ishlatmaydi. **Litsenziya:** Unity Asset Store shartlari assetlarni Unity loyihalarida ishlatishga litsenziya beradi; Godot'da ishlatish huquqiy jihatdan tekshirilishi kerak — bu foydalanuvchi qarori, ogohlantirildi.
- **Keyingi:** PLAYTEST — endi ovoz bilan.

### Sessiya 15 (23-qism) — 2026-07-29
- **Bajarildi:** **Birinchi marta o'rnatiladigan build chiqarildi** — `build/macos/War of Engineers.dmg` (371 MB, universal, ad-hoc imzolangan) va `build/War of Engineers - Windows.zip` (306 MB). `build/README.md` da o'rnatish, Gatekeeper qadami, boshqaruv va saqlash fayli yo'li yozilgan. Eksport shablonlari (1.2 GB) yuklab olindi, `export_presets.cfg` yozildi, `build/` `.gitignore` ga qo'shildi.
- **Eng muhim topilma — eksport o'yinni bo'shatib qo'yardi:** `MissionManager`, `InventoryManager` va `TechTreeManager` katalogni qat'iy `.tres` kengaytmasi bo'yicha skanlardi. Eksportda Godot matn resurslarini `.res` ga aylantiradi (va remap qilinganlariga `.remap` qo'shadi), ya'ni **uchala katalog ham bo'sh qolardi** — missiya yo'q, material yo'q, texnologiya yo'q. Yangi `ResourceFolder.Paths()` uchala kengaytmani ham qabul qiladi. Bu xato faqat eksportda ko'rinadi, editorda hech qachon sezilmasdi.
- **Tasdiq:** eksport qilingan ilova ishga tushirildi (Apple M5, OpenGL 4.1 Metal compatibility) — `TechTreeManager: loaded 3 tech node(s)`, `InventoryManager: loaded 10 item definition(s)`. Tuzatishsiz bu ikkalasi 0 bo'lardi.
- **Yo'l-yo'lakay uchta to'siq:** (1) `.sln` da `Release` yo'q — Godot `ExportRelease` ishlatadi (mening chaqiruvim xato edi, loyihada muammo yo'q); (2) macOS arm64 uchun `textures/vram_compression/import_etc2_astc=true` shart, aks holda eksport umuman boshlanmaydi; (3) shablon **universal** binar bilan keladi, `arm64` alohida emas.
- **Asosiy muammo — ikkita .NET o'rnatmasi:** Godot `/usr/local/share/dotnet` (SDK 9.0.311) ni chaqirardi, `DOTNET_ROOT` esa `~/.dotnet` (10.0.203) ga ishora qilardi. Nomuvofiqlik SDK resolverni buzib, C# assemblylari **butunlay qo'shilmagan DMG** hosil qilgandi (0 ta dll) — va eksport buni faqat "warning" deb o'tkazib yuborardi. Yechim: eksportni `DOTNET_ROOT=/usr/local/share/dotnet` bilan ishga tushirish. **Keyingi build'da ham shu kerak.**
- **MSI o'rnatuvchi ham qo'shildi** (foydalanuvchi so'rovi): Godot MSI chiqarmaydi, shuning uchun `msitools` (`wixl`) orqali yasaldi — macOS'da Windows o'rnatuvchisini qurish mumkin ekan. Yangi `tools/make_msi.py` eksport papkasini aylanib WiX manbasini generatsiya qiladi (190 komponent), GUID'lar fayl yo'lidan `uuid5` bilan olinadi — ya'ni bir xil versiyani qayta qurganda GUID o'zgarmaydi va Windows uni **yangilanish** deb qabul qiladi, yonma-yon o'rnatmaydi. Natija: `build/War of Engineers - Setup.msi` (312 MB) — Program Files ga o'rnatadi, Start menyu yorlig'i va o'chirish yozuvi bor.
- **Tekshirildi:** x64 shabloni, barcha standart MSI jadvallari, ichki CAB, va **189 fayl manbada = 189 File jadvalida = 189 CAB ketma-ketligida** — hech narsa tushib qolmagan. (Birinchi hisobda "farq bor" chiqdi, lekin bu `msiinfo` ning 3 qatorlik sarlavhasini noto'g'ri sanaganim edi.)
- **Saboq:** "eksport tugadi" degan xabar yetarli emas — chiqqan paketning **ichini ochib**, dll borligini va ilovaning haqiqatan ishga tushishini tekshirish shart. Birinchi DMG tashqaridan to'g'ri ko'rinardi.
- **Sinalmagan:** Windows ZIP va MSI **Windows mashinasida ishga tushirilmagan** (bu yerda Windows yo'q) — faqat tuzilishi tekshirilgan. macOS DMG haqiqatan ishga tushirilib sinalgan.
- **Keyingi:** PLAYTEST — endi build orqali.

### Sessiya 15 (22-qism) — 2026-07-29
- **Bajarildi:** **Piyoda birinchi shaxs aslida ishlamayotgani aniqlandi va tuzatildi.** `PROGRESS` "qo'l va qurol ekranda qoladi" deb yozgan edi, lekin render ko'rsatdi: **kadr butunlay bo'sh** — na qo'l, na qurol. Sabab o'lchov bilan topildi: kamera boshdan **0.28 m oldinda** turardi, ya'ni butun tana (qo'l bilan birga) uning ortida qolardi. Qurol kamera fazosida **(0.55, −0.15, −0.07)** — ya'ni 0.55 m yonda, atigi 7 sm oldinda, ~83° burchakda, kadrdan tashqarida.
- **Ikki sweep bilan isbotlandi:** yelka burchagini 25 ta (X,Y) va 24 ta (Y,Z) kombinatsiyada o'lchadim — **hech biri qurolni oldinga chiqara olmadi** (eng yaxshisi 0.23 m). Z o'qi uni markazga keltiradi, lekin oldinga emas. Demak poza bilan hal bo'lmaydi.
- **Yechim uch qadamda:** (1) ko'z nuqtasi yuz oldidan haqiqiy ko'zga qaytarildi; (2) shunda personajning **og'zi va tishlari kadrni to'sdi** — bosh suyagi birinchi shaxsda nolga siqiladi (tana bitta skinned mesh, boshni boshqacha yashirib bo'lmaydi); (3) qo'l loyihada allaqachon bor **ikki-suyakli IK** bilan kameraga nisbatan joylashtiriladi. Natija o'lchandi: qurol **(0.19, −0.22, −0.64)** — klassik FPS pozitsiyasi, va render'da qurol ham, ikkala qo'l ham ko'rinadi.
- **Muammo:** qo'llar orasi 0.178 m — ikki qo'llab ushlash uchun normal; render'da chap kaft "ochiq" ko'ringani mavjud `OrientBone` xatti-harakati, tegilmadi.
- **Uchinchi shaxs himoyalangan:** yangi kod butunlay `firstPerson` sharti ostida (`PlaceFirstPersonHand` erta qaytadi, bosh faqat FP'da siqiladi, qo'shimcha burchaklar FP'dan tashqarida `Vector3.Zero`).
- **O'z tuzatishimdagi chekka holat topildi va yopildi:** bosh suyagini siqish `UpdateAimPose` ning **erta qaytishidan keyin** turgan edi. Buzuq yo'l: birinchi shaxsda qurol yo'qolsa poza aralashuvi nolga tushadi va funksiya har kadr erta qaytadi — keyin uchinchi shaxsga o'tilsa **bosh tiklanmay qolardi** (boshsiz personaj). Chaqiruv erta qaytishdan oldinga ko'chirildi, endi bunday holat tuzilishi bo'yicha mumkin emas. O'lchov bilan tasdiq: uchinchi shaxs `headScale=1.000` → birinchi shaxs `0.001` → qaytgach `1.000`.
- **Saboq:** eng katta xato — **hujjatdagi "tasdiqlandi" so'ziga ishonish**. Bu funksiya PROGRESS'da ishlaydi deb yozilgan, aslida hech qachon ko'rilmagan va umuman ishlamagan. Ikkinchi saboq: yangi kod qo'shganda uni **mavjud erta qaytishlarga nisbatan qayerga qo'yish** muhim — ikki marta shu tuzoqqa tushdim (optika holati va bosh suyagi).
- **Keyingi:** PLAYTEST.

### Sessiya 15 (21-qism) — 2026-07-29
- **Bajarildi:** **Snayper optikasi birinchi marta render bilan ko'rildi** — u yozilgan, lekin hech qachon ko'z bilan tekshirilmagan yagona foydalanuvchi so'ragan funksiya edi. Ko'rilgach darhol buzuq ekani ma'lum bo'ldi: **o'yinchining qo'li va tanasi optika doirasi ichida turardi**, nishonni to'sib. Sabab: optikaga o'tganda faqat qurol modeli yashirinardi va FOV o'zgarardi — kamera esa yelka ortida qolardi, ya'ni tana kamera bilan nishon orasida. 22° FOV'da qo'l doiraning katta qismini egallardi.
- **Yechim:** `PlayerCamera.ScopeView` qo'shildi — optika ko'tarilganda kamera ko'z darajasiga tushadi (spring arm nolga), va o'yinchi tanasi yashiriladi. Endi doirada faqat dunyo ko'rinadi.
- **O'zim kiritgan xavfni o'zim topdim:** `UpdateAim()` qurol yo'q bo'lsa **erta qaytadi**. Ilgari bu zararsiz edi, lekin endi `IsScoped` `true` bo'lib qolib, **tana butunlay ko'rinmas va optika ekranda osilib qolardi**. Holat bitta joyga (`SetScope`/`ClearScope`) yig'ildi va erta qaytishdan oldin tozalanadi.
- **Ikkinchi xato — o'lik kod:** `_playerVisual` `_playerBody` tayinlanishidan **bir qator oldin** o'qilardi, ya'ni doim `null` edi va tanani yashirish hech qachon ishlamagan. Render'da tana yo'qolgani faqat kameraning ko'z darajasiga tushgani tufayli edi — ya'ni ko'z bilan ko'rish "ishlayapti" degan **noto'g'ri xulosa** berardi. Probe holatni raqam bilan chiqargani uchun oshkor bo'ldi.
- **Tasdiq:** optikada `bodyVisible=False`, qo'yib yuborilganda `True`; ikkala yo'nalish ham haqiqiy holat bilan o'lchandi.
- **Saboq:** render "ko'rinishi to'g'rimi" degan savolga javob beradi, "holat to'g'rimi" degan savolga emas. Ikkalasini birga tekshirish kerak — bu yerda ko'rinish to'g'ri, holat esa buzuq edi.
- **Keyingi:** PLAYTEST.

### Sessiya 15 (20-qism) — 2026-07-29
- **Bajarildi:** Foydalanuvchi so'rovi — **HUD ham ko'k tonlarga o'tkazildi**. Plitalar, ramkalar, XP chizig'i, `DAVR` raqami, pul belgisi, `MATERIAL` sarlavhasi va toast'lar blueprint oilasiga qo'shildi. **Sog'liq (qizil) va chidam (yashil) qoldirildi** — ular bezak emas, jang paytida bir qarashda o'qiladigan holat signali; material kvadratchalari ham qoldirildi (ular `ItemData.Tint`, ya'ni materialning o'z rangi).
- **`Gold` ikki ma'noda ishlatilgan ekan** — bezak (pul, tanlangan qurol, NPC plitasi) **va ogohlantirish** (chidam tugagan, uskuna yetmagan, to'p o'qlanmagan). Hammasini ko'kga aylantirsam ogohlantirish signali jimgina yo'qolardi. `UiPalette.Warning` (kehrabo) va `UiPalette.Highlight` (yorug' siyoh) ga ajratildi, har bir chaqiruv o'z ma'nosiga yo'naltirildi. Bu — `StampRed` bilan bo'lgan xatoning aynan takrori, shuning uchun oldindan tekshirildi.
- **Muammo:** HUD plitalari ikki urinishdan keyin ham quyuq qolgandi — rang `HUD.tscn` da emas, **temadagi `HudPanel` variantida** ekan. Uchinchi urinishda topildi. **Ko'z bilan baholash ham chalg'itdi:** plita ko'k bo'lgandan keyin ham "kulrang" ko'rinardi — piksel o'lchovi haqiqatni ko'rsatdi: (26,29,37) neytral kulrangdan (19,33,51) ko'kga o'tgan, qorong'i ko'rinishi esa missiya taxtasining qatlami tufayli edi (qatlamsiz haqiqiy rang (37,71,111)).
- **HUD nihoyat qatlamsiz ko'rildi** — barcha oldingi render'larda missiya taxtasi ustida turgan edi, ya'ni men uni hech qachon toza ko'rmaganman. Taxtani yopadigan probe bilan ko'rilgach yana ikki narsa chiqdi: (1) **`DAVR` yili hali binafsha edi** — rangi `HUD.tscn` da emas, temadagi `TimeScale` variantida ekan (o'yindagi oxirgi binafsha); (2) **qurol nomi slotda o'rtasidan qirqilardi** ("Pistolet M191‸") — endi slot bir oz kengaytirildi va `TrimEllipsis` qo'yildi, uzun nomlar uch nuqta bilan tugaydi.
- **Yolg'on trevoga:** `MATERIAL` panelidagi "uchburchak" nuqson emas — yarim-shaffof panel ostidan binoning tomi ko'rinadi. Kattalashtirib tekshirilgach aniqlandi.
- **Tozalash:** endi hech kim ishlatmaydigan **5 ta eski tema varianti o'chirildi** (`ModalPrimary`, `ModalButton`, `ModalPanel`, `PaperPanel`, `JournalPanel`) va ular bog'langan 7 ta o'lik uslub bloki. Sabab: aynan shu variantlar tufayli 4 ta panel blueprint almashtirishidan chetda qolgan edi — ular temada qolsa, keyingi safar ham jimgina eski dizayn qaytadi. O'chirishdan oldin sahna va kodda ishlatilmasligi tekshirildi, keyin "osilgan havola yo'q" deb tasdiqlandi.
- **Keyingi:** PLAYTEST.

### Sessiya 15 (19-qism) — 2026-07-29
- **Bajarildi:** UI qayta dizayni **birinchi marta to'liq ko'z bilan tekshirildi**. 16 ekran skript bilan almashtirilgan, lekin faqat 2 tasi render bilan ko'rilgan edi. Vaqtinchalik galereya probe'i yozilib, 13 ta ekran ketma-ket ochildi va rasmga olindi. **To'rt ekran umuman o'tmagani aniqlandi** — ular hech qachon `Paper*` variantlarini ishlatmagan (`ModalPrimary`/`ModalButton`/`HudBody` ishlatgan), shuning uchun almashtirish ularni ko'rmagan: **missiya tugash ekrani**, **kampaniya finali**, **pauza menyusi** va **boshqaruv paneli**. Hammasi blueprint'ga o'tkazildi.
- **Boshqaruv paneli qayta yozildi:** eski holatda uslubsiz kulrang quti, **ingliz tilida** ("CONTROLS"/"Back"), tugmalar `move_forward` kabi **xom action nomlari** bilan chiqardi va **9 ta yangi tugma yo'q edi** (`aim`, `camera_toggle`, `inventory`, `reload`, `weapon_1..5`). Endi: blueprint uslubi, o'zbekcha nomlar, HARAKAT/ISH/JANG/KAMERA guruhlari, 19 ta amalning hammasi, `Esc` bilan yopiladi.
- **Ikki o'qilish muammosi:** (1) o'chirilgan tugma matni (`MATERIAL YETMAYDI`) och fonda 40% shaffoflikda edi — 72% ga ko'tarildi; (2) brifingdagi ogohlantirish qatori quyuq qizil edi va ko'k fonda ko'rinmasdi — yorqin sariq-qizilga o'tkazildi.
- **Muammo (probe xatolari, o'z-o'zidan saboq):** (1) `CanvasLayer` ekranlar (pauza, tex daraxti) galereya foni **ostida** chizildi — ichma-ich `CanvasLayer` o'z indeksida chizilar ekan, ota qatlamdan meros olmaydi. (2) "Hamma bolani ko'rsat" usuli pauza menyusining ichki panellarini ham ochib yubordi. (3) Missiya taxtasi/jurnal galereyada bo'sh chiqdi — bu artefakt, map yuklanmagan.
- **Ataylab tegilmadi:** HUD va toast'lar. Ular 3D ustidagi qatlam, "sahifa" emas — quyuq plitalar o'qilish uchun kerak. Bosh menyu ham tegilmadi (dizayn manbai). Grep bilan tasdiqlandi: eski rang tokenlari faqat shu ikkisida qoldi.
- **Semantik rang xatosi topildi (o'yin ichidagi render orqali):** `StampRed` **uch xil ma'noda** ishlatilgan ekan — missiya bajarilgani, vazifa bajarilgani va materialning yetmasligi. Qog'oz dizaynda qizil shtamp "tasdiqlangan" degani edi; blueprint'da esa xato kabi o'qiladi va bajarilgan missiya bloklangan retsept bilan **bir xil rangda** chiqardi. Yangi `UiPalette.StampDone` (yashil) qo'shildi; ogohlantirish qizili faqat haqiqiy ogohlantirish uchun qoldi.
- **Saboq:** panelni alohida ko'rish yetarli emas — ranglarning **ma'nosi** faqat o'yin ichida, boshqa holatlar yonida ko'rinadi. Bu xato galereyada emas, gameplay render'ida ko'rindi.
- **Keyingi:** PLAYTEST.

### Sessiya 15 (18-qism) — 2026-07-28
- **Bajarildi:** Aviatsiya kabinalari **oxirigacha yetkazildi** — to'rttalasi ham render bilan ko'z bilan tasdiqlandi: Ki-61 (o'z burni + parragi oldinda, klassik qiruvchi ko'rinishi), Bell (ochiq ufq, pastda vint qanoti), Seahawk (burun + vint), Cessna (avval tasdiqlangan). Dev probe (`scripts/dev/`, `scenes/dev_probe.tscn`) o'chirildi, `Main.tscn` tiklandi, `dotnet build` 0/0, headless Main.tscn skript xatosiz.
- **Muammo:** 17-qismdagi o'lchov **noto'g'ri fazoda** edi. AABB uchoq **root**iga nisbatan o'lchangan, `_firstPersonOffset` esa **`CameraRig`** tugunida — u root'dan yuqoriga siljigan (Ki61 +1.6, Bell +2.0, Seahawk +2.5). Ikkisi bir xil deb hisoblanganidan kamera har safar noto'g'ri balandlikda qolgan. Bell'ning "ishonarsiz" o'lchovi (kenglik 1.16 m, markaz X=1.77) aslida to'g'ri edi — men uni xato deb rad etib, "xavfsiz" markazlashtirilgan qiymatga o'tgandim, bu esa vaziyatni yomonlashtirdi.
- **Saboq:** (1) O'lchov va qo'llanadigan qiymat **bir xil koordinata fazosida** ekanini tekshirish shart — bu yerda uchta urinish shu bitta xatodan ketdi. (2) "Ishonarsiz" chiqqan o'lchovni taxminga almashtirish emas, **nega shundayligini** tekshirish kerak edi. (3) Probe render'ida UI qatlamini yashirish kerak — missiyalar taxtasi tekshirilayotgan ko'rinishni to'sib qo'yadi.
- **Keyingi:** PLAYTEST.

### Sessiya 15 (17-qism) — 2026-07-28
- **Bajarildi:** Aviatsiya kabinalarini playtest'ga qoldirmasdan o'zim tekshirdim — va ular **noto'g'ri** ekan (Cessna'da kamera fyuzelyaj ustida, parrak ko'zni to'sgan). To'rt uchoqning mesh AABB'si o'lchanib, kabina nuqtasi hisoblab chiqarildi (fyuzelyaj markazida, o'rtadan 30% yuqori, 28% oldinda). Cessna render bilan tasdiqlandi.
- **Muammo:** (1) Bell mesh'i node markazidan **1.77 m yon tomonga siljigan** — X=0 qo'yilsa kamera fyuzelyajdan tashqarida qolardi. (2) Probe uchoqlar orasida almashmadi (`exit_vehicle` ro'yxatga olinmadi), shuning uchun qolgan 3 tasi ko'z bilan ko'rilmadi. Aviatsiya TZ'ga kirmagani uchun bu yerda to'xtatildi — qiymatlar bitta export'dan sozlanadi.
- **Saboq:** "playtest'da ko'rasiz" deb qoldirilgan narsani o'zim tekshirsam bo'lar ekan — birinchi urinishdagi qiymat haqiqatan buzuq edi.
- **Keyingi:** Playtest.

### Sessiya 15 (16-qism) — 2026-07-28
- **Bajarildi:** Foydalanuvchi so'rovi bo'yicha 5 band. (1) **Birinchi shaxs kamerasi** `V` bilan — piyodada CS2 uslubida (qo'l va qurol ekranda, personaj yashirilmaydi), (2) **texnika/tank ichidan**, (3) **aviatsiya kabinasidan** — bitta `PlayerCamera.cs` yettala rigga xizmat qiladi, (4) **snayper optikasi** o'ng-tugmada (`ScopeOverlay.cs`, `_Draw` bilan chiziladi), (5) **UI ko'k dizaynga o'tkazildi** (16 ekran) va missiya modali 780→1180 px kengaytirildi.
- **Muammo:** (1) `V` tugmasini **qaysi rig birinchi ushlasa** o'sha almashardi — texnikada o'yinchining yashirin kamerasi yutib yuborardi; endi faqat `Current` kamera javob beradi. (2) Texnika modellarida **ichki qism yo'q** — o'rindiq korpus ichida bo'lsa faqat qorong'i yuzalar ko'rinadi; nuqtalar korpusdan tashqariga (kaput/minora ustiga) ko'chirildi. (3) `TitlePaperS→TitleL` o'tkazishda sarlavhalar kattalashib layoutni bosdi — 40 px ga normallashtirildi.
- **Keyingi:** Playtest.

### Sessiya 15 (15-qism) — 2026-07-28
- **Bajarildi:** Repo push uchun tekshirildi (PROGRESS bir necha marta "tasdiqdan keyin git push" deb yozgan, lekin hajm hech qachon qaralmagan). Natija: **muammo yo'q** — eng katta fayl 32 MB, jami 163 MB, remote ulangan. `.gitignore` ga `.DS_Store` qo'shildi; `scenes/.DS_Store` allaqachon kuzatilgani uchun foydalanuvchi bir marta `git rm --cached` qilishi kerak (buyruq yuqorida yozilgan).
- **Muammo:** Yo'q.
- **Keyingi:** PLAYTEST. Kod tomonida bajariladigan ish qolmadi — qolgan hamma narsa yo playtest fikriga, yo yangi assetga, yo foydalanuvchi qaroriga bog'liq.

### Sessiya 15 (14-qism) — 2026-07-28
- **Bajarildi:** TZ 1-bo'limining **10–15 daqiqa** talabi birinchi marta o'lchandi. Bitta missiyaning sof vaqti 1.0–1.3 daqiqa, o'ntasi 12 daqiqa — lekin jang/o'qish/panel vaqti hisobga olinmagan, real o'yinda har missiya 2–3 daqiqa. **Xulosa: investorga 10 emas, 3 missiya ko'rsatiladi.** Shundan kelib chiqib `docs/DEMO_SCENARIO.md` yozildi — qaysi 3 missiya, qanday tartibda, har qadamda nima aytish, TZ 16-bo'limining 8 bandi qayerda ko'rinishi.
- **Muammo:** Yo'q. **Topilma:** demo uzunligi missiya uzunligiga emas, **nechta missiya ko'rsatilishiga** bog'liq ekan — bitta missiya g'oyani yetkazish uchun juda qisqa, o'ntasi esa TZ oynasidan chiqadi.
- **Keyingi:** PLAYTEST — `docs/DEMO_SCENARIO.md` dagi marshrut bo'yicha o'ynab ko'rish eng foydali usul (ham checklist, ham demo mashqi).

### Sessiya 15 (13-qism) — 2026-07-28
- **Bajarildi:** Qurol ushlashidagi oxirgi ko'rinadigan kamchilik yopildi — **chap kaft** endi qurolga buriladi. Avval IK faqat qo'lni yo'naltirardi, kaft ochiq turardi. Rig o'lchandi (barmoqlar kaftning +X o'qi bo'ylab, bosh barmoq −Z), shundan maqsad orientatsiya qurildi; `ArmIk.OrientBone()` qo'shildi.
- **Muammo:** Render kadrlashini ikki marta noto'g'ri qo'ydim (kamera juda yaqin — faqat bilak ko'rindi). **Saboq:** headless render'da kamerani baholash uchun kamida 2–2.5 m masofa va ~40° FOV kerak, aks holda kadr foydasiz chiqadi.
- **Keyingi:** PLAYTEST.

### Sessiya 15 (12-qism) — 2026-07-28
- **Bajarildi:** Render narxi birinchi marta **o'lchandi**. Hub 2.22M uchburchak/kadr edi — zaif GPU uchun mo'ljallangan loyihada juda ko'p. Mesh'lar reyting qilinib aybdor topildi: `Grove` (trees9, 247k×7) butun byudjetning 54% i. Masofa chegarasi + daraxt soyasini o'chirish + uzoq daraxtzorlarni olib tashlash bilan **hub −70%, missiya map'i −66%** (ikkalasi ham 700k dan past). Yangi `DistanceCull.cs` runtime'da qo'shilgan mesh'larga ham ishlaydi — javondagi qurollar va aerodrom samolyotlari uchun aynan shu kerak edi.
- **Muammo:** Birinchi urinishda `.tscn` larga to'g'ridan-to'g'ri `visibility_range_end` yozdim, lekin og'ir geometriya **kod bilan** instansiya qilinar ekan (WeaponPickup modelni `_Ready` da yaratadi) — shuning uchun tahrir faqat stol/maydonchaga tegdi. Skript yechimiga o'tildi. **Saboq:** "sahnada nima bor" bilan "ekranda nima chiziladi" bir xil emas — runtime'da qurilgan narsalarni ham hisobga olish kerak.
- **Eslatma:** VRAM monitori (1248 MB) ishonchsiz — ikkala sahnada bir xil va diskdagi tekstura 43 MB; Movie Maker buferi. Editorda tekshirish kerak.
- **Keyingi:** PLAYTEST.

### Sessiya 15 (11-qism) — 2026-07-28
- **Bajarildi:** Loyiha tartibga solindi. (1) **O'lik kod o'chirildi** — `HangarScreen`, `CharacterVisual.cs` va eski 3 missiya oroli (3 map + 3 mission + 3 task). Grep bilan tasdiqlandi: ular yopiq halqa edi, tashqaridan hech kim ishora qilmasdi; hammasi git'da, bitta buyruq bilan qaytariladi. O'chirilgandan keyin import va ikkala sahna toza. (2) **`PROGRESS.md` yuqorisi qayta yozildi** — "Joriy holat" 10 ta kichik sessiya orqada qolgan edi, yangi sessiya uni o'qib noto'g'ri holatni tiklardi. Endi bir qarashda holat + **yagona 20 bandlik playtest checklist** (A: missiya oqimi, B: asosiy sikl, C: qurol/jang, D: texnika/muhit, E: saqlash) — avval har bo'limda alohida-alohida sochilib yotgandi.
- **Muammo:** Yo'q.
- **Keyingi:** **PLAYTEST.** Kodda o'lchaydigan narsa qolmadi.

### Sessiya 15 (10-qism) — 2026-07-28
- **Bajarildi:** Butun kampaniyaning data auditi — har missiyaning qulflangan vazifalari o'z map'ining resurslaridan yasalishi mumkinmi. **e3 (1939) bajarib bo'lmaydigan edi** (30 temir kerak, map 24 berardi). Generator endi konlarni talabdan kelib chiqib o'lchaydi (×1.6 zaxira) va yozganini qayta tekshiradi (`verify_maps_are_solvable`). Natija: 10/10 missiya bajarib bo'ladi.
- **Muammo:** Yo'q — audit muammoni playtest'dan oldin topdi. **Saboq:** generatsiya qilingan kontentda "yechim bormi?" degan tekshiruv generatorning o'z qismi bo'lishi kerak; aks holda xato faqat o'yin o'rtasida ko'rinadi.
- **Keyingi:** Playtest.

### Sessiya 15 (9-qism) — 2026-07-28
- **Bajarildi:** Chap qo'l uchun **ikki-suyakli IK** (`ArmIk.cs`) — nishonga olganda chap qo'l qurolning tutqichiga keladi, ya'ni qurol ikki qo'llab ushlanadi. Nishon nuqtasi qurolning o'z mesh o'lchamidan hisoblanadi (uzunlikning 52% i), bilak→kaft farqi hisobga olingan. Aim pozasi tabiiylashtirildi (tirsak bukildi), oddiy turgan holat ham qayta tekshirildi — qurol sonda osilib turadi.
- **Muammo:** **Grip burilishi aim pozasiga bog'liq ekan.** Birinchi IK urinishi ishlamadi — o'ng qo'l to'liq to'g'ri bo'lgani uchun qurol chap qo'lning yetish masofasidan tashqarida qolardi (IK cho'zilish chegarasiga urilib, ikki render bir xil chiqdi — shu belgi bo'ldi). Tirsakni bukkanda masofa yechildi, lekin **grip buzildi** va 5 qurolning hammasini qayta yechish kerak bo'ldi. Kelajakda aim pozasi o'zgartirilsa, grip solverini qayta ishga tushirish shart.
- **Keyingi:** Playtest.

### Sessiya 15 (8-qism) — 2026-07-28
- **Bajarildi:** 5 qurolning grip transformi **hisoblab chiqarildi** (taxmin emas): nishonga olish pozasida qo'l bazisi o'qildi, modelning o'z o'qlaridan maqsad bazisi qurildi, `G = A⁻¹·W` yechildi; pozitsiya dasta joyi + bilak→kaft qadami bilan. Qiymatlar `weapon_*.tres` ga yozildi, render bilan uchala tur (pistolet/avtomat/snayper) tasdiqlandi.
- **Muammo:** Oldingi urinishlar nega ishlamaganining sababi topildi — **o'lchov xato edi**. Model o'qini `VisualInstance3D` AABB'sidan olgandim, lekin FBX ichidagi `Light3D`/`Camera3D` ham `VisualInstance3D` va AABB'ni shishirib yuborgan (pistolet 2×2×2 m!). Faqat `MeshInstance3D` bo'yicha o'lchagach, to'rt qurol Z-o'qli, AK X-o'qli ekani chiqdi — eski umumiy `(-90,0,0)` hech biriga to'g'ri kelmagan.
- **Saboq:** Godot'da model o'lchamini AABB bilan o'lchashda **`MeshInstance3D` bo'yicha filtrlash shart** — yorug'lik/kamera node'lari o'lchovni buzadi.
- **Keyingi:** Playtest.

### Sessiya 15 (7-qism) — 2026-07-28
- **Bajarildi:** Butun missiya uchdan-uchgacha **haqiqiy kirish va haqiqiy UI bosishlari** bilan o'ynaldi: yig'ish → mastserskayada 2 uskuna → 3 vazifa → tugash ekrani → bazaga qaytish (bonus to'landi, taxta qayta ochildi). Demo endi **o'ynaladigan** ekani isbotlandi.
- **Muammo:** Birinchi urinishda 3-vazifa bajarilmadi — o'lchov ko'rsatdi: dushmanlar o'yinchini o'ldirgan (hp 94→42→respawn). Kod xatosi emas, **balans xatosi**: 3 dushman ~11.5 zarar/sek berardi va 26 m sezish radiusi map'ning ko'pini qoplardi, ya'ni jang muhandislik ishini bloklardi (TZ 11-bo'limiga zid). `Enemy.cs` qiymatlari yumshatildi (17/11 m, 2.4 s, 5.0 zarar) → qayta o'lchovda o'yinchi 55 HP bilan hammasini bajardi.
- **Saboq:** "vazifa bajarilmayapti" degan xato har doim ham interaktsiya/mantiq emas — o'yinchining o'sha paytdagi **holatini** (HP, pozitsiya) ham logga chiqarish kerak, aks holda sabab ko'rinmaydi.
- **Keyingi:** Playtest. Foydalanuvchi editorda `weapon_*.tres` grip qiymatlarini sozlaydi.

### Sessiya 15 (6-qism) — 2026-07-28
- **Bajarildi:** Interaktsiya buzuq bo'lgani sababli **hech qachon sinalmagan** tizimlar bittalab real kirish bilan tekshirildi — Humvee (ta'mirlash/haydash/chiqish), tank to'pi (otdi), Cessna (W/S bilan ko'tarilish/pastlash), mastserskaya paneli (`E` ochadi, `Esc` yopadi), qurol javoni (`E` bilan olinadi). Hammasi ishlaydi. Boshlang'ich qurol to'plami pistolet+revolverga qisqartirildi, shunda javondan olish ma'noga ega bo'ldi.
- **Muammo:** Ikki "bug" o'lchov xatoim bo'lib chiqdi (noto'g'ri tank, samolyotda noto'g'ri tugma) — kodda muammo yo'q edi. **Saboq:** hubda bir turdagi bir nechta obyekt bo'lsa, guruhdan olingan birinchi/oxirgi element tekshirilayotgan obyekt bo'lmasligi mumkin.
- **Keyingi:** Playtest. Foydalanuvchi editorda `weapon_*.tres` grip qiymatlarini sozlaydi.

### Sessiya 15 (5-qism) — 2026-07-28
- **Bajarildi:** Missiya oqimining qolgan teshiklari yopildi — missiya ichida saqlash/tiklash, takror-mukofot ekspluatatsiyasi, geroy/texnika tanlovining saqlanishi (uchalasi ham PROGRESS'dagi eski qarzlar), missiyani tashlaganda taxtaning qaytishi, tugash aniqligi va HUD missiya hisoblagichi. Batafsil: yuqoridagi checklist bandi.
- **Muammo:** Yo'q. **Muhim dizayn nuqtasi:** vazifa bajarilganligi ikki to'plamda saqlanadi — kampaniya bo'ylab (jurnal shtampi uchun) va joriy sessiya uchun. Aks holda "QAYTA O'YNASH" bosilgan missiya darhol tugab qolardi.
- **Keyingi:** Playtest. Foydalanuvchi editorda `weapon_*.tres` grip qiymatlarini sozlaydi (kelishildi).

### Sessiya 15 (4-qism) — 2026-07-28
- **Bajarildi:** Foydalanuvchi playtest feedback'i bo'yicha 7 band. **Eng muhimi: interaktsiya butunlay ishlamayotgani aniqlandi** — `Area3D` signallari statik obyektlarni ko'rmagan (o'lchov bilan isbotlandi: bir joyda `GetOverlappingBodies()` bo'sh, `IntersectShape` esa hammasini topadi). Har kadr shakl so'roviga o'tkazildi. Shuningdek: otish geometriyasi ikki bosqichli qilindi (markazdagi soxta chaqnash yo'qoldi), nishonga olishda kamera o'ng yelka ustiga suriladi, qo'l suyak pozasi bilan ko'tariladi + qaytarma, qurollar `E` bilan olinadigan bo'ldi, AK grip o'qi tuzatildi.
- **Muammo:** (1) Diagnostikada `Input.ActionPress()` "just pressed" kadriga tushmasligi vaqt oldi — headless'da sintetik kirish uchun `Input.ParseInputEvent(new InputEventAction{...})` ishlatish kerak, aks holda yarim-avtomat qurol otmaydi. (2) Qo'l pozasi uchun `GetBoneRest()` dan blend qilish xato bo'ldi — rig'ning rest pozasi **T-poza**, qo'l yon tomonga otilib ketdi; joriy animatsiya pozasidan blend qilish kerak ekan. (3) Grip pozitsiyasini headless render bilan sozlash samarasiz — data qiymati, editorda qilinishi kerak.
- **Keyingi:** Playtest + editorda `weapon_*.tres` grip sozlash (qurol qo'lga to'liq yopishishi uchun).

### Sessiya 15 (3-qism) — 2026-07-28
- **Bajarildi:** TZ'ning qolgan uchta aniq bandi — **ochilish brifingi** (13-bo'lim syujet, `GameIntro` + `IntroPanel`, taxtaga BAJARILDI/KEYINGI holati bilan), **tez slotlar** (14-bo'lim, HUD pastida 5 qurol + patron), **tutun** (15-bo'lim, mastserskaya mo'risida CPU zarrachalar). Batafsil: yuqoridagi checklist bandi.
- **Muammo:** uchta xato faqat render orqali topildi — tutun kublar ustuni bo'lib chizilgan (yumshoq radial tekstura kerak edi), intro matnida rus TZ'sidan kirill harflari ko'chib qolgan (`avanpost`), slot chiplari qurol nomini kesardi. Barchasi tuzatildi; butun `scenes/` va `resources/` kirill harflarga tekshirildi — boshqa qolmagan.
- **Keyingi:** Playtest (a–i ro'yxati + yangi: brifing chiqadimi, tez slotlar 1–5 bilan almashadimi, mo'ridan tutun chiqadimi).

### Sessiya 15 (2-qism) — 2026-07-28
- **Bajarildi:** TZ 3-bo'limining qolgan ikki majburiy bandi — **NPC tizimi** (`NpcData`, `Npc.cs`, `NpcWalker.tscn`, 6 rol, hubda 7 + har map'da 4) va **final missiya** (`MissionData.IsFinal`, `MissionManager.CampaignCompleted`, `CampaignCompletePanel`). Batafsil: yuqoridagi checklist bandi. `NpcVisual` ga ixtiyoriy harakatga bog'liq animatsiya qo'shildi (default o'chiq — dushmanlar va eski NPC o'zgarmadi).
- **Muammo:** `Label3D` ning `fixed_size=true` xossasi rol plitalarini ekran o'lchamida chizib, butun ekranni to'sib qo'ygan edi — render'siz sezilmasdi. Olib tashlandi + `visibility_range_end` qo'shildi. Yana: headless render'da kamera yo'nalishini o'yinchi rigidan boshqarish chalkash chiqdi — vaqtinchalik `Camera3D` qo'yib `LookAt` bilan yo'naltirish ancha ishonchli usul.
- **Keyingi:** Playtest (yuqoridagi a–i ro'yxati).

### Sessiya 1 — 2026-07-14
- **Bajarildi:** Faza 0 kod tomoni to'liq: `project.godot` (main scene, gl_compatibility renderer), `WarOfEngineers.csproj` (Godot.NET.Sdk/4.5.0, net8.0), `.sln`, `icon.svg`, `.gitignore`, folder skeleti (18 papka), `scenes/main/Main.tscn` (TestArena instance + TempCamera), `scenes/world/TestArena.tscn` (40×40 pol, 4 cube, DirectionalLight3D, WorldEnvironment + ProceduralSky). `dotnet build` — 0 warning, 0 error. `git init` + birinchi commit. Kod Opus 4.8 agentlar tomonidan yozildi, arxitektura review Fable tomonidan.
- **Muammo:** `docs/WarOfEngineers_MVP_Architecture.md` spec fayli repoda YO'Q — folder skeleti spec 4-bo'limisiz, standart Godot C# layout asosida qurildi. Foydalanuvchi spec faylni `docs/` ga qo'yishi kerak; mos kelmasa skelet moslashtiriladi.
- **Keyingi:** Foydalanuvchi: Godot 4.5+ .NET o'rnatish → loyiha ochish (birinchi ochilishda Godot `.tscn` uid'larni qayta indekslaydi, normal) → F5 playtest. Tasdiqdan keyin Faza 1 (Character).

### Sessiya 2 — 2026-07-14
- **Bajarildi:** Faza 0 playtest tasdiqlandi → DONE. Faza 1 kod tomoni to'liq: `scripts/player/PlayerController.cs` (gravitatsiya, kamera-nisbiy WASD, Shift-run, Space-jump, Visual'ni harakat yo'nalishiga burish — tana aylanmaydi), `scripts/player/PlayerCamera.cs` (CameraRig'da, yaw rig'da / pitch SpringArm'da −70°…+30° clamp, mouse capture + Esc/click toggle, SpringArm player'ni exclude qiladi), `scenes/player/Player.tscn` (CharacterBody3D + kapsula + Nose indikator + CameraRig/SpringArm3D/Camera3D), `Main.tscn`dan TempCamera olib tashlandi + Player instance (0, 0.5, 0), `project.godot`ga `[input]` bo'limi (move_forward/back/left/right, jump, run — physical keycode, W/A/S/D/Space/Shift). `dotnet build` 0 warning / 0 error. Kod Opus 4.8 agentlar, arxitektura + review Fable.
- **Muammo:** Yo'q. Eslatma: Godot 4.5 birinchi ochilishda `.cs.uid` sidecar fayllar yaratadi va `.tscn`larga script uid qo'shishi mumkin — bu normal, keyingi commitga kiradi.
- **Keyingi:** Foydalanuvchi F5 playtest (WASD/Shift/Space/sichqoncha/Esc). Input Map'ni Project Settings → Input Map'da vizual tekshirish mumkin. Tasdiqdan keyin Faza 2 (Interaction + Engineering Task).

### Sessiya 13 — 2026-07-16
- **Bajarildi:** **Faza 8 — to'liq model paketi ulandi.** Foydalanuvchi `3DModels/` ga 172 fayl yukladi; hammasi o'rganilib 14 model tanlab `assets/models/` ga ko'chirildi (~115MB). **Import muammolari (3 ta, hal qilindi):** (1) `3DModels/` loyiha ichida — Godot skan qilib `.blend`larda qotardi → `3DModels/.gdignore`; (2) **t90a.fbx (3DReaperDX rip) FBX importerni segfault qilardi** → OBJ + qo'lda yozilgan MTL (kamuflyaj atlas `8430f8ee.jpg` ulandi); (3) `fbx/embedded_image_handling=2` yo'q teksturada segfault → barcha FBX `=0` (discard) + runtime override (Nathan patterni). FBX'lar kutayotgan teksturalar binary'dan topildi: revolver→`WesternPistol.png`, pistol→`pistol reference.jpg` (bular render emas, haqiqiy tekstura ekan), glock→`glock.jpg`, AWP→Color/scope.bmp (16.8MB BMP→2.4MB PNG konvert). Seahawk MTL'dagi absolute `I:\` yo'llar va trees9 MTL'dagi `Texture\\` backslash yo'llar tuzatildi. Har model headless dump bilan o'lchandi (AABB/masshtab/o'q/animatsiya/suyaklar) — barcha transformlar (Z-up→Y-up, sm→m, markazlash) oldindan hisoblab agentlarga berildi.
- **Kod (Opus 4.8 — 8 agent workflow: 5 parallel build + 1 integratsiya + 2 adversarial review; 0 xato, 0 topilma):** Yangi skriptlar: `scripts/data/WeaponData.cs` ([GlobalClass] — Id/Damage/Range/ModelScene/Grip* — data-driven qurol), `scripts/world/ModelDresser.cs` (Blender FBX'lardagi Light3D/Camera3D axlatini QueueFree qiladi + albedo override: hammaga bitta yoki material-nom bo'yicha), `scripts/world/NpcVisual.cs` (statsionar NPC: idle loop + root motion + dif/norm override). Edit: `WeaponController.cs` (WeaponData'dan damage/range; `hand_r` suyagiga BoneAttachment3D bilan revolver modeli), `Vehicle.cs` (holat bo'yog'i korpusdan **StatusLamp** sferasiga ko'chdi — Humvee materiallari buzilmaydi; exitOffset 2.9), `Vehicle.tscn` (Humvee.obj ×0.0139, kollisiya 2.56×1.73×4.58, kamera 7m), `Player.tscn` (weapon_revolver.tres ulandi). Yangi sahnalar: `scenes/props/` Weapon{Revolver,Pistol,Glock,Awp,Ak47}, WeaponRack, ParkedTank (T-90 ×0.53 Z-up fix), Aircraft{Ki61 ×5.5, Cessna ×0.01}, Helicopter{Seahawk ×0.0827, Bell (2.27m havoda edi — yerga tushirildi)}, Airfield (4 uchoq + landing pad); `scenes/world/` Npc.tscn (Sophia, 180° flip, kapsula kollisiya), Grove.tscn (trees9 ×0.4). TestArena: WeaponRack(4,0,9), Npc(6.5,0,1.5 — spawnga qaragan), ParkedTank(22,0,-12), Airfield(45,0,-45), Grove(-35,0,25). `dotnet build` 0/0; headless Main.tscn **exit 0 toza log** (uid'lar import bilan ro'yxatga olindi).
- **O'tkazib yuborilgan modellar (sabablari):** Tree1.obj (1.2M vertex — zaif GPU'ga yaroqsiz; trees9 ishlatildi), Grass.blend (Blender o'rnatilishi kerak), **Cottage_Clean (faqat PBR teksturalar — mesh fayli YO'Q, foydalanuvchi meshni yuklasa ulanadi)**, Dusty + animatsiyalar (C4D format — Godot o'qimaydi), Soliter.max / SeaHawk.ma/.max / AWP.max/3DS (import qilinmaydigan formatlar — OBJ/FBX variantlari ishlatildi), vietnam_soldier.obj (MTL/UV-tekstura xaritasi yo'q, loose bmp'lar mapping'siz), rp_nathan/sophia'ning u3d/ue4/unitypackage variantlari (asosiy FBX yetarli), HighPoly bell (lowpoly ishlatildi), 1/2/3.bmp, Show.png (preview rasmlar).
- **Muammo / playtest kutmoqda:** revolver grip nol-standart (`.tres`da tuning); Humvee old tomoni ±Z taxmin; AWP material-nom matching; NPC/aircraft yo'nalishlari taxminiy; FPS zaif GPU'da (trees9 247k tris). Git: assets/models 157MB bo'ldi (eng katta fayl trees9.obj 33MB — GitHub 100MB/fayl limitidan past, lekin push oldidan git-lfs o'ylash tavsiya).
- **Keyingi:** Playtest (Joriy holat'dagi 6-bandlik checklist) → tuning → barcha fazalar playtest'dan o'tsa git push (HUMO5459).

### Sessiya 12 — 2026-07-16
- **Bajarildi:** **Faza 8 boshlandi — birinchi 3D model (personaj)**. Foydalanuvchi Renderpeople "rp_nathan_animated_003_walking" berdi (c4d/maya paketlari, ichida `.fbx` bor). FBX + `_dif`/`_norm` teksturalarni `assets/models/character/` ga ko'chirdim. **Muammo hal qilindi:** FBX import loop'ga tushdi (`fbx/embedded_image_handling=1` embedded teksturani `_0.png` ga chiqarib qayta-skan loop yaratardi) → `.import`da `=0` (discard) qildim, eski `.scn` cache'ni tozalab toza reimport qildim. GDScript throwaway tool bilan tuzilma aniqlandi: root Node3D → Skeleton3D (88 suyak) → MeshInstance (`_geo`, 1 surface), AABB (1.89,1.87,0.35) = **~1.87m, masshtab TO'G'RI** (ufbx metrga o'girgan), animatsiya bitta = **"Take 001"**. Yangi: `scripts/player/CharacterVisual.cs` (FBX ichidan AnimationPlayer+MeshInstance ni type bo'yicha topadi, `_dif` albedo + `_norm` normal material qo'yadi, CharacterBody3D ajdodini topib horizontal speed > 0.4 bo'lsa "Take 001" loop, aks holda pause). `Player.tscn`: kapsula MeshInstance + Nose yashirildi, `Visual` ostiga `Character` (CharacterVisual) + FBX instance (`Model`, 180° Y taxmin). `.gitignore`: `.vs/`, manba c4d/maya paketlari; ishlatilmagan gloss/mask teksturalar o'chirildi. `dotnet build` 0/0. Headless (Main.tscn): exit 0, model+anim+material xatosiz.
- **Muammo:** Character **yo'nalishi noaniq** — 180° Y taxmin qilindi (Renderpeople ko'pincha +Z ga qaraydi). Playtest: teskari yursa `Model` transform'idan 180° olib tashlanadi. Model 40MB (fbx+2 tekstura) git'ga kirdi — katta, keyin git-lfs o'ylash mumkin.
- **Fix (o'sha kun):** Playtest'da 2 bug topildi, sabab bitta — walk animatsiyasida **root motion** (`...root` suyagi 2.27s'da +2.9m Z ga siljiydi). (1) burilganda mesh drift/orbit; (2) blokka yaqinlashganda mesh ichiga kiradi (capsule to'xtaydi, mesh root-motion bilan davom etadi). Yechim: `CharacterVisual`da `RootMotionTrack` o'rnatildi → animatsiya **joyida** yuradi (Godot root siljishini pose'dan ajratadi, harakatga ishlatilmaydi). "Ustiga chiqolmaslik" uchun bloklar 2m→1m past qilindi (climbable, sakrash ~1.2m). Yo'nalish 180° hali playtest kutadi.
- **Keyingi:** Playtest (root motion tuzatildimi, blokka toza to'qnashadimi, ustiga chiqiladimi, personaj yo'nalishi). Qolgan modellar (mashina/muhit) — `3DModels/` papkaga qo'shilgan ko'rinadi, keyin ko'riladi.

### Sessiya 11 — 2026-07-16
- **Bajarildi:** **Open-world streaming poydevori** (foydalanuvchi so'rovi: "katta ochiq dunyo"; Opus 4.8, 2 agent 0 xato). Yangi: `scripts/world/WorldStreamer.cs` (Node3D — player'ni `"player"` group orqali topadi, `_chunkSize=60`, `_radius=3` → ~49 faol chunk, player chunk o'zgarganda yaqinlarini spawn/uzoqlarini QueueFree, `(x+z)&1` checkerboard tint), `scenes/world/GroundChunk.tscn` (60×60 StaticBody3D placeholder yer). Edit: `TestArena.tscn`dan fixed 40×40 Floor (+3 sub_resource) olib tashlandi, WorldStreamer node qo'shildi, Environment'ga distance fog (density 0.012). `Player.tscn` root'i `"player"` group'ga qo'shildi. `dotnet build` 0/0. Headless (Main.tscn) verify: exit 0, "no node in group 'player'" warning YO'Q → player topildi, chunklar spawn bo'ldi, xato yo'q. **3D modellar:** foydalanuvchi o'zi beradi (`.glb`/`.fbx`, Y-up, metr, `assets/models/`) — Fable ulaydi; hozircha kutmoqda.
- **Muammo:** Yo'q. **Eslatma:** haqiqiy streaming (harakatda chunk almashinuvi) + tuman ko'rinishi playtest talab qiladi. Ochiq dunyo va menyu — MVP spec 1–6 dan TASHQARI, foydalanuvchi ataylab so'radi.
- **Keyingi:** Playtest (menyu + Faza 7 + ochiq dunyo). Modellar kelsa ulash. Tasdiqdan keyin git push (HUMO5459).

### Sessiya 10 — 2026-07-16
- **Bajarildi:** (1) Faza 7 kamera bug tuzatildi — Vehicle SpringArm pitch belgisi teskari edi (+0.35→-0.35), kamerani yer ostiga yuborib pol bilan to'qnashtirardi; endi tashqaridan chase-cam + mashina tanasi arm'dan exclude qilindi. (2) **Menus & Game Flow qatlami** (foydalanuvchi so'rovi, MVP'dan tashqari, Opus 4.8 4 agent 0 xato, pipeline workflow). Yangi: `scripts/core/GameDemoLauncher.cs` (static — `res://Z_AircraftSystem_demo/FTPS_Online.exe` ni `OS.CreateProcess` bilan tashqi ochadi, `IsAvailable` tekshiradi), `scripts/ui/`: `MainMenu` (boot sahna — New Game managerlarni reset, Continue save yuklaydi/HasSave bilan disable, panellarni ko'rsatadi), `PauseMenu` (CanvasLayer Always, Esc toggle, pause+kursor, Main Menu/Quit), `ControlsPanel` (Input Map'dan 10 action tugmasi), `MissionsPanel` (3 task .tres'dan), `HangarScreen` (loadout kartalari + Aircraft Demo launch). Edit: `SaveManager`ga `HasSave()`, `project.godot` main_scene → MainMenu.tscn, `Main.tscn`ga PauseMenu, `PlayerCamera`dan Esc branch olib tashlandi (endi pause menyu Esc'ni oladi). `dotnet build` 0/0. Headless verify: MainMenu (boot) + Main.tscn (gameplay) ikkalasi exit 0 xatosiz; "loaded save (money=180)" — foydalanuvchining real save'i bor. **FTPS_Online = Godot ichiga chizib bo'lmaydi (UE4, cooked, manbasiz) — foydalanuvchi bilan kelishilib "tashqi launch" varianti tanlandi.**
- **Muammo:** Yo'q (jiddiy). **Edge:** Esc-pause va T-techtree ikkalasi Always — bir vaqtda ochib qo'ysa g'alizlik (crash emas), MVP-uchun qabul. Menyu janr/UI — bu MVP spec 1–6 dan TASHQARI, foydalanuvchi ataylab so'radi.
- **Keyingi:** Foydalanuvchi playtest (menyu, pause, Aircraft Demo launch, mashina kamerasi). Tasdiqdan keyin git push (HUMO5459, remote yaratish).

### Sessiya 9 — 2026-07-16
- **Bajarildi:** Faza 7 (Weapon + Vehicle) kod tomoni to'liq — **VERTICAL SLICE TUGADI** (Opus 4.8, 4 agent 0 xato, 3 bosqichli workflow). Weapon: `scripts/interfaces/IDamageable.cs`, `scripts/player/WeaponController.cs` (Node3D player bolasi; kameradan raycast, o'z RID'ini exclude qiladi, "fire"=chap-sichqoncha, IDamageable'ga zarar), `scripts/world/Target.cs` + `Target.tscn` (health=50, otilganda oq flash, yo'q bo'lganda +$15 → EconomyManager). Vehicle: `scripts/world/Vehicle.cs` + `Vehicle.tscn` (CharacterBody3D, IInteractable — mavjud focus/E tizimi; repair→enter→drive→exit; enter'da driver `ProcessMode=Disabled`+yashirin+collision off+kamera almashadi; drive WASD arcade; exit=F). Integratsiya: `Player.tscn`ga WeaponController, `TestArena.tscn`ga Vehicle (10,0,-5) + 4 Target, `project.godot`ga `fire`(mouse-left)+`exit_vehicle`(F), `HUD.tscn`ga crosshair. `dotnet build` 0/0. Headless verify: exit 0, xato yo'q, hamma yuklandi.
- **Muammo:** Yo'q (jiddiy). **Kichik MVP edge:** mashinani uzoqqa haydab chiqsang, PlayerInteraction'ning eski focus ro'yxati bir zum eskirishi mumkin (crash emas, exit'da player mashina yoniga teleport bo'lgani uchun amalda bilinmaydi). **Yon:** loyiha papkasida `War of Engineers game-handoff.zip` + `war-of-engineers-game/` paydo bo'lgan (packaging nusxalari, manba emas) — `.gitignore`ga qo'shildi.
- **Keyingi:** Foydalanuvchi playtest (ot / mashina hayda). Tasdiqdan keyin **barcha kod fazalar (1–7) tamom** → git push (HUMO5459, remote yaratish kerak). Faza 8 (3D modellar) — alohida asset ishi, xohishga ko'ra.

### Sessiya 8 — 2026-07-15
- **Bajarildi:** Faza 6 (Save/Load) kod tomoni to'liq (Opus 4.8, 2 agent 0 xato). Yangi: `scripts/core/SaveData.cs` (plain POCO: Version/Money/Xp/Level/CompletedCount/ElapsedDays/UnlockedTech), `scripts/core/SaveManager.cs` (autoload **oxirgi**; `System.Text.Json` bilan `user://savegame.json`; `_Ready`da Load + autosave `MissionCompleted`/`TechUnlocked`ga ulanadi; `NotificationWMCloseRequest` + `AutoAcceptQuit=false` bilan oyna yopilganda save; restore tartibi Economy→Progression→Mission→Tech→**Time**). Manager edit'lari: har biriga `LoadState(...)`, `TimeManager`ga `ElapsedDays` getter + `RecomputeTimeScale()` helper, `TechTreeManager`ga `UnlockedTechIds` getter. `project.godot`: SaveManager autoload oxirgi. **Eslatma:** `using FileAccess = Godot.FileAccess;` alias (System.IO bilan noaniqlikni hal qiladi). `dotnet build` 0/0. **Headless round-trip verify:** run#1 save yo'q → "starting fresh"; qo'lda `savegame.json` (money=999/level=5/2 tech) yozib run#2 → "loaded save (money=999, level=5, tech=2)" — tiklandi, xato yo'q; test save o'chirildi. Scope: per-workstation completion saqlanmaydi (MVP — reload'da tasklar qayta ochiladi).
- **Muammo:** Yo'q.
- **Keyingi:** Foydalanuvchi playtest (task bajar → oynani yop → qayta och → holat tiklanadi). Tasdiqdan keyin **Faza 7 (Weapon + Vehicle)** — vertical slice tugaydi. Faza 7 uchun `Z_AircraftSystem_demo` (UE4) referens bo'lishi mumkin. Barcha fazalar tugagach: HUMO5459'dan git push (remote hali yo'q, yaratish kerak).

### Sessiya 7 — 2026-07-15
- **Bajarildi:** Faza 5 (Tech Tree) kod tomoni to'liq (Opus 4.8 agentlar, 3 agent 0 xato). Yangi: `scripts/data/TechNodeData.cs` (`[GlobalClass] Resource` + `TechEffectType` enum: TechId/DisplayName/Description/CostMoney/RequiredLevel/PrerequisiteIds/Effect/EffectValue), `scripts/core/TechTreeManager.cs` (autoload, TimeManager'dan keyin; `res://resources/data/tech/` skanlaydi — data-driven; `CanUnlock` prereq+level+money gate, `TryUnlock` → `EconomyManager.TrySpendMoney`, aggregat `RewardMoneyMultiplier`/`PlayerSpeedMultiplier`; `TechUnlocked` signal), `scripts/ui/TechTreeUI.cs` + `scenes/ui/TechTree.tscn` (CanvasLayer, `process_mode=3` Always; **T** bilan toggle → `GetTree().Paused` + kursor Visible → kamera konflikti yo'q; katalogdan tugmalar generatsiya). 3 tech `.tres`: efficient_tools ($150,+50% pul), reinforced_boots ($200,+30% tezlik), advanced_tooling ($400, level 2 + efficient_tools prereq, +100% pul). Gameplay effekti: `EconomyManager.TrySpendMoney` qo'shildi, `MissionManager` reward'ni ×RewardMoneyMultiplier, `PlayerController` tezlikni ×PlayerSpeedMultiplier. `project.godot`: TechTreeManager autoload + `tech_tree`=T input. `Main.tscn`: TechTree instance. `dotnet build` 0/0. Headless verify: exit 0, "loaded 3 tech node(s)", xato yo'q.
- **Muammo:** Yo'q. **Yon masala:** `Z_AircraftSystem_demo/` papkasi (loyiha ichida) — bu **qadoqlangan UE4 o'yini** ("FTPS_Online", 2021, 879MB cooked `.pak`), manba kod yo'q, boshqa dvigatel. Undan **davom ettirib bo'lmaydi** (source yo'q + UE4≠Godot). Faqat referens sifatida ishlaydi (Faza 7 vehicle uchun ilhom). `.gitignore`ga qo'shildi. `.exe` avtomatik ishga tushmadi (SmartScreen/prereq) — foydalanuvchi o'zi ochadi.
- **Keyingi:** Foydalanuvchi playtest. Tasdiqdan keyin Faza 6 (Save/Load — JSON, `user://`; save qilinadigan holat: money, xp/level, completed count, elapsed time, unlocked tech).

### Sessiya 6 — 2026-07-15
- **Bajarildi:** Faza 4 (signature mexanika) kod tomoni to'liq. Yangi: `scripts/core/TimeManager.cs` (autoload, MissionManager'dan keyin) — ichki kalendar 1900-yildan boshlanadi, `_baseDaysPerSecond=10` × `TimeScale` tezligida oqadi; har task tugaganda `TimeScale = _baseScale + CompletedCount × _scalePerTask` (0 task→×1, 1→×2, 3→×4); `CurrentYear`/`DayOfYear` computed, `TimeScaleChanged` signali (Faza 5 era-unlock uchun). **Engine.TimeScale'ga tegilmagan** — player normal, faqat kalendar tezlashadi. HUD edit: `HUD.cs`ga `_Process` + TimeLabel polling, `HUD.tscn`ga TimeLabel (VBox'da, Xp'dan keyin). `project.godot`ga TimeManager autoload. `dotnet build` 0/0. Headless verify: build-solutions + run exit 0, xato yo'q, `.cs.uid` yaratildi. **Kod bu safar Opus 4.8 tomonidan yozildi** (limit tiklangan — 2 Opus agent, 0 xato), arxitektura + review Fable.
- **Muammo:** Yo'q.
- **Keyingi:** Foydalanuvchi playtest. Tasdiqdan keyin Faza 5 (Tech Tree — pul/XP sarflab unlock; era-unlock TimeManager yiliga bog'lanishi mumkin).

### Sessiya 5 — 2026-07-15
- **Bajarildi:** Faza 3 kod tomoni to'liq. Yangi autoload singletonlar: `scripts/core/EconomyManager.cs` (`Money`, `AddMoney`, `MoneyChanged`), `scripts/core/ProgressionManager.cs` (`Xp`/`Level`, `AddXp`, `_xpPerLevel=100`, `XpChanged`/`LeveledUp`), `scripts/core/MissionManager.cs` (`CompleteTask(data)` → Economy+Progression'ga uzatadi, `CompletedCount`, `MissionCompleted`). UI: `scripts/ui/HUD.cs` + `scenes/ui/HUD.tscn` (CanvasLayer, chap-yuqori Money/XP labellari + markaziy reward toast, `_toastToken` bilan flicker guard). Data-driven reward: `EngineeringTaskData`ga `RewardMoney`/`RewardXp` qo'shildi, 3 `.tres`ga qiymatlar (50/20, 75/30, 100/40). `EngineeringTask` tugaganda `MissionManager.Instance?.CompleteTask(_taskData)` chaqiradi (Faza 2 signali saqlandi). `project.godot`ga `[autoload]` bo'lim (3 manager), `Main.tscn`ga HUD instance. `dotnet build` 0/0. Headless Godot .NET verify: build-solutions + Main.tscn run exit 0, xato yo'q, HUD "autoload not found" warning YO'Q (3 manager ham ulandi), 4 yangi `.cs.uid` yaratildi.
- **Muammo:** Opus subagentlar hali rate-limit'da edi (07-14 19:10 reset o'tgan, lekin yangi limit bor edi). Foydalanuvchi "Opus da yoz, sonnet ishlatma" dedi, keyin "hozir boshla, limit bor" dedi. Sonnet bilan bir marta yozib bo'lingandi — u qoralama **o'chirildi** (git checkout + rm), toza Faza 2 holatidan Fable (main loop) qaytadan yozdi. Opus tiklangach xohlansa qayta review/rewrite qilinishi mumkin (foydalanuvchi qaroriga).
- **Keyingi:** Foydalanuvchi .NET editor bilan playtest. Tasdiqdan keyin Faza 4 (Time + Acceleration — signature mexanika).

### Sessiya 4 — 2026-07-14
- **Bajarildi:** Faza 2 kod tomoni to'liq. Yangi: `scripts/interfaces/IInteractable.cs` (InteractionPrompt/CanInteract/Interact/SetFocused), `scripts/data/EngineeringTaskData.cs` (`[GlobalClass] Resource`: TaskId/DisplayName/DurationSeconds — data-driven), `scripts/world/EngineeringTask.cs` (`StaticBody3D`, IInteractable; per-instance StandardMaterial3D `_Ready`da yaratiladi — instance'lar orasida rang oqib ketmaydi; timed progress idle→working rang lerp, done=yashil; `TaskStarted`/`TaskCompleted` signallari — Faza 3 reward loop shularga ulanadi), `scripts/player/PlayerInteraction.cs` (`Area3D`, r=2.5m, eng yaqin interactable fokus + `interact` bosilganda Interact). Yangi sahna/data: `scenes/world/Workstation.tscn` (StaticBody3D + BoxMesh placeholder), 3 ta `resources/data/tasks/*.tres` (repair_generator 3s / lay_telegraph 4s / fortify_trench 5s). Edit: `Player.tscn` (InteractionArea Area3D + SphereShape3D r=2.5), `TestArena.tscn` (Workstations guruhi + 3 instance, har biriga `_taskData` override), `project.godot` (`interact` = E, physical 69). `dotnet build`: 0/0. Headless Godot .NET verify: build-solutions + Main.tscn run exit 0, xato yo'q, `_taskData` override warning yo'q (property override ishladi), 4 yangi `.cs.uid` yaratildi.
- **Muammo:** Workflow'dagi 3 Opus agent **session limitiga** urildi (7:10pm Tashkent reset). Ular skript+scene fayllarni yozib ulgurgan, lekin edit'lar (Player.tscn/TestArena.tscn/project.godot) va verify tugamagan. Fable (main loop) qolgan edit'larni o'zi yozib, o'zi verify qildi. **MVP soddalik:** task E bilan boshlanadi va DurationSeconds davomida avtomatik tugaydi — yurib ketsang bekor bo'lmaydi (Faza 3+ da hold/cancel qo'shsa bo'ladi). Reward YO'Q (Faza 3).
- **Keyingi:** Foydalanuvchi .NET editor bilan playtest. Tasdiqdan keyin Faza 3.

### Sessiya 3 — 2026-07-14
- **Bajarildi:** Faza 1 playtest muammosi hal qilindi. **Ildiz sabab:** foydalanuvchi standart Godot 4.7 (`Godot_v4.7-stable_win64.exe`) o'rnatgan edi — .NET/mono emas. Standart editor C# skriptlarni ishga tushirmaydi (sahna ochiladi, boshqaruv o'lik). Faza 0 shuning uchun "ishlagan"edi (greybox, C# yo'q). Tuzatildi: (1) `csproj` SDK 4.5.0→4.7.0 editor versiyasiga moslandi; (2) `project.godot`da standart editor olib tashlagan `"C#"` feature tag'i qaytarildi; (3) player skriptlarida `null!` bilan CS8618 ogohlantirishlar tozalandi; (4) Godot 4.7 **.NET** editori yuklab olinib `C:\Users\LG\Godot-NET\` ga chiqarildi; (5) headless `--build-solutions` bilan C# yig'ilishi qurildi, headless run bilan skriptlar xatosiz yuklanishi tasdiqlandi (`.cs.uid` fayllar yaratildi). `dotnet build`: 0 error / 0 warning. Commit'lar: `4c2c693`, `4cb014a`.
- **Muammo:** Yo'q (hal qilindi). Eslatma PROGRESS boshiga ⚠️ sifatida qo'shildi: doim .NET editor bilan ochish.
- **Keyingi:** Foydalanuvchi .NET editor bilan F5 playtest. Tasdiqdan keyin Faza 2.

---

## Ochiq savollar

- MVP texnikasi aniq qaysi biri: dala artilleriyasi / armored car / aloqa stansiyasi? (Faza 7'gacha hal qilinadi)
- O'yin valyutasi nomi?
- **Spec fayl qayerda?** Arxitektura spec'i repoda hech qachon bo'lmagan. CLAUDE.md endi `docs/ChronoShift_MVP_TZ.md` ga tayanadi, lekin u arxitektura spec'i emas, mahsulot TZ'si.
- ~~**Loyiha nomi:** ChronoShift'ga o'tiladimi yoki War of Engineers qoladimi?~~ → **HAL QILINDI (2026-08-09): ChronoShift.**
- **TZ'ning sanoat davri (XVIII–XIX asr) estetikasi:** 1900→2050 texnikasi bilan qanday yarashadi? Hozircha 1900 eng erta era.
