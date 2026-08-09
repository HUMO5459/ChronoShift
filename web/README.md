# Landing page

`web/index.html` — o'yin haqidagi bir sahifalik sayt. O'zbek + English (yuqori o'ngdagi `UZ / EN`),
yorug' va qorong'i mavzu (ko'ruvchining tizim sozlamasi bo'yicha), telefonda ham ishlaydi.

Fayl **butunlay o'zini o'zi ta'minlaydi** — shriftlar va skrinshotlar ichiga `data:` URI sifatida
joylashtirilgan. Tashqi CDN, tashqi rasm, tracker yo'q. Hajmi ~600 KB.

Dizayn tili o'yinning o'zinikidan olingan: `scripts/ui/UiPalette.cs` ranglari va
`assets/fonts/` shriftlari (Oswald · Archivo · IBM Plex Mono). Yorug' mavzu — ko'k chizmaning
teskarisi (whiteprint).

---

## Yuklab olish havolalarini ulash

Hozir uchala tugma **placeholder** holatda: "HAVOLA TAYYORLANMOQDA", bosilganda hech qayerga
o'tmaydi. Fayllar (306–371 MB) repo'ga sig'maydi — ularni GitHub Releases'ga qo'yish kerak.

1. Release yarating va uchala faylni yuklang:

   ```bash
   gh auth login                       # bir marta
   gh release create v1.0.0 \
     "build/macos/ChronoShift.dmg" \
     "build/ChronoShift - Setup.msi" \
     "build/ChronoShift - Windows.zip" \
     --title "ChronoShift v1.0.0" --notes-file build/README.md
   ```

2. `web/src/template.html` da uchta `<a class="dl-btn is-pending" href="#" aria-disabled="true">`
   qatorini toping (ular yonida `<!-- TODO: replace href -->` izohi bor) va har birini shunday
   qiling — `is-pending` klassini va `aria-disabled` ni **olib tashlang**, `href` ga real havolani
   qo'ying, matnni "Yuklab olish" ga o'zgartiring:

   ```html
   <a class="dl-btn" href="https://github.com/HUMO5459/ChronoShift/releases/download/v1.0.0/War.of.Engineers.dmg">
     <span><span class="uz">Yuklab olish</span><span class="en">Download</span></span>
     <span class="sz">371 MB</span>
   </a>
   ```

   Tagidagi `<span class="pending-note">…</span>` qatorini ham o'chiring.

3. Sahifani qayta yig'ing:

   ```bash
   python3 web/src/build.py
   ```

---

## GitHub Pages'da chiqarish

GitHub Pages repo ildizidan yoki `/docs` papkasidan xizmat qiladi — `/web` dan emas.
Ikki yo'ldan biri:

- **Oson yo'l:** `web/index.html` ni `docs/index.html` ga nusxalang
  (`cp web/index.html docs/index.html`), so'ng repo Settings → Pages → Source: `main` / `/docs`.
- **Yoki** `gh-pages` nomli branch yarating, unda `index.html` ildizda tursin.

Manzil: `https://humo5459.github.io/ChronoShift/`

---

## Sahifani tahrirlash

Matnni, bo'limlarni, jadvalni `web/src/template.html` da o'zgartiring, so'ng `build.py` ni qayta
ishga tushiring. `web/index.html` ni **qo'lda tahrirlamang** — u har safar qayta yoziladi.

Har bir tarjima juftligi shunday yoziladi:

```html
<span class="uz">O'zbekcha matn</span><span class="en">English text</span>
```

`web/src/assets/` — sahifadagi shriftlar (o'yin shriftlarining lotin qismiga qisqartirilgan
woff2 nusxalari) va skrinshotlar. Yangi skrinshot qo'shsangiz, `build.py` dagi `ASSETS`
lug'atiga token qo'shing va uni `template.html` da ishlating.

Skrinshotlar o'yindan shu tarzda olingan (macOS'da ekran ruxsati kerak emas):

```bash
/Applications/Godot_mono.app/Contents/MacOS/Godot --path . res://scenes/main/Main.tscn \
  --resolution 1920x1080 --write-movie /tmp/shot/frame.png --fixed-fps 1 --quit-after 8
```
