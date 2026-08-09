# ChronoShift — investor demo ssenariysi

> TZ 1-bo'lim: prototip **10–15 daqiqada** asosiy g'oyani ko'rsatishi kerak.
> TZ 16-bo'lim: demo oxirida investor 8 ta narsani tushungan bo'lishi kerak.
>
> Bu hujjat o'lchovga asoslangan — quyidagi vaqtlar map ma'lumotlaridan hisoblangan
> (marshrut uzunligi ÷ yurish tezligi + vazifa/ishlab chiqarish davomiyligi).

---

## O'lchangan vaqtlar

Bitta missiyaning **sof** vaqti (yurish + yig'ish + ishlab chiqarish + 3 vazifa):

| Era | Yurish | Yurish vaqti | Ish vaqti | Jami |
|---|---|---|---|---|
| 1900 | 172 m | 33 s | 34 s | **67 s** |
| 1939 | 198 m | 38 s | 36 s | **74 s** |
| 1965 | 161 m | 31 s | 38 s | **69 s** |
| 2050 | 192 m | 37 s | 40 s | **77 s** |

**O'rtacha 1.2 daqiqa; o'ntasi 12 daqiqa.**

⚠️ Bu raqamlarga **kirmagan**: jang (dushmanlar), menyu/brifing o'qish, panel bilan ishlash,
yo'l qidirish. Real o'yinda har missiya **2–3 daqiqa**. Ya'ni:

- **10 missiyaning hammasi = 20–30 daqiqa** → TZ oynasidan chiqadi
- **3 missiya = 8–12 daqiqa** → ✅ aynan kerakli oyna

**Xulosa: investorga hammasini emas, 3 ta missiyani ko'rsating.**
Bitta missiya (1.2 daq) esa juda qisqa — sikl his qilinmaydi.

---

## Tavsiya etilgan marshrut (~12 daqiqa)

### 0. Ochilish — 1 daqiqa
Bosh menyu → **Yangi o'yin** → brifing kartochkasi chiqadi.

> Ovoz chiqarib o'qing: *"Armiya chekinmoqda, ishlab chiqarish vayron. Sizni harbiy
> muhandis etib tayinlaymiz."* — bu TZ 13-bo'limidagi zavyazka.

Keyin missiyalar taxtasi ochiladi: **10 era, 1900 dan 2050 gacha**.
👉 *Bu yerda ko'rsatiladi: o'yin hajmi va vaqt o'qi.*

### 1. Birinchi missiya (1900) — 4 daqiqa · **asosiy siklni ko'rsatadi**
Taxtadan **"1900 — Imperiya chegarasi"** ni tanlang.

Tartib bilan:
1. **1-vazifani darrov bajaring** (u ochiq) → yil sakraydi, ekranda yangi sana chaqnaydi
   👉 *"Vaqt o'z-o'zidan emas, mehnat bilan o'tadi."*
2. **2-vazifaga boring** → `E` ishlamaydi, HUD "KERAK: O'rnatish komplekti" deydi
   👉 *"Mana shu — o'yinning yuragi. Jangchi emas, muhandis."*
3. **Konlarga boring**, `E` bilan yog'och/temir/ko'mir yig'ing → chapdagi MATERIAL paneli o'sadi
4. **Mastserskayaga boring** → buyurtma → progress → "ISHLAB CHIQARISH TUGADI"
   👉 *"Qurolni detal bo'yicha yasamaymiz — sanoat ishlab chiqarishini ko'rsatamiz."* (TZ 8)
5. `I` → sumkada uskuna → 2- va 3-vazifani bajaring
6. Tugash ekrani → **Bazaga qaytish** → bonus

### 2. O'rta era (1965 yoki 1980) — 3 daqiqa · **miqyosni ko'rsatadi**
Taxtadan sakrab **"1965 — Sovuq urush"** ni tanlang.

👉 *"Bir xil sikl, boshqa davr: boshqa rang, boshqa vazifa, boshqa texnika."*

Bu yerda **jangni** ko'rsating: dushmanlar bor, otishmoq mumkin, lekin missiya
muhandislik vazifalari bilan tugaydi — jang bilan emas. (TZ 11)

Ixtiyoriy: hubda **texnika** — Humvee, T-90 (to'p otadi), samolyot.

### 3. Final (2050) — 3 daqiqa · **g'oyani yopadi**
Taxtadan **"2050 — Kelajak bazasi"**.

Tugatgach **kampaniya yopilish ekrani** chiqadi:
`1900 — 2062`, statistika, va tezis:

> *"Kampaniyani o'q soni emas, ishlab chiqarish, ta'minot va muhandislik
> yechimlari hal qildi."*

👉 *Demoning butun ma'nosi shu ekranda aytiladi.*

### 4. Yakun — 1 daqiqa
`T` bilan **tex daraxti**, menyudan **jurnal** — kengayish yo'nalishini ko'rsatish uchun.

---

## TZ 16-bo'limi bilan solishtirish

Demo oxirida investor quyidagilarni ko'rgan bo'ladi:

| TZ talabi | Qayerda ko'rinadi |
|---|---|
| ochiq dunyoni tekshiradi | hub + har missiya map'i |
| resurs yig'adi | 1-missiya, 3-qadam |
| ishlab chiqarishni rivojlantiradi | mastserskaya paneli |
| uskuna ishlab chiqaradi | 1-missiya, 4-qadam |
| texnologiyalarni ishlatadi | qulflangan vazifa ochilishi |
| muhandislik vazifalarini bajaradi | har uch missiya |
| kichik janglarda qatnashadi | 1965 missiyasi |
| texnologiya bilan tarixni o'zgartiradi | final ekrani |

---

## Demo paytida qilmaslik kerak

- **10 missiyaning hammasini o'ynamang** — 20–30 daqiqa, investor zerikadi
- **Yo'l qidirib yurmang** — marshrutni oldindan bir marta o'ynab chiqing
- **O'lib qolmang** — dushmanlar 17 m dan sezadi; vazifani bajarayotganda ular
  yo'q joydan boshlang yoki avval otib tashlang
- **Aviatsiyani uzoq ko'rsatmang** — u TZ'da yo'q, chalg'itadi

## Texnik eslatma

- Loyihani **faqat Godot .NET/mono** bilan oching
- Demo oldidan `user://savegame.json` ni o'chiring, aks holda brifing chiqmaydi
  va missiyalar "BAJARILDI" deb turadi
