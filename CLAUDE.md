# ChronoShift — Claude Code Instructions

## Loyiha

3D third-person engineering game. Engineer 1900-yildan kelajakgacha harbiy-muhandislik vazifalarini bajaradi → pul + XP to'playdi → tech tree ochadi → vaqt tezlashadi → yangi texnika chiqadi.

**Har sessiya boshida o'qi:**
- `docs/ChronoShift_MVP_TZ.md` — mahsulot TZ'si (investorlar uchun vertical slice, 18 bo'lim)
- `PROGRESS.md` — joriy holat, faza checklist

**Har sessiya oxirida:** `PROGRESS.md` ni yangila (nima bajarildi, muammo, keyingi qadam).

---

## Oltin qoidalar (buzilmaydi)

1. **Scope discipline.** Faqat MVP — spec'ning 1–6 bo'limlari. Spec'ning **7-bo'limi (Backlog) IMPLEMENT QILINMAYDI** — u north-star, hozirgi ish emas. So'ralmagan feature qo'shma.
2. **Bitta fazada ishla.** Faza tartibi `PROGRESS.md` da. Keyingi fazaga o'zing sakrama — foydalanuvchi aytadi.
3. **Platform-agnostic kod.** Fayl yo'llari **faqat** `res://` (asset) va `user://` (save). Hech qachon OS path, `\`, yoki absolute path. Sabab: bitta loyihadan Windows + macOS + Linux chiqadi.
4. **Input faqat Input Map orqali.** Hardcoded key/scancode yozma.
5. **Data-driven.** Kontent (weapon, mission, tech node, vehicle) — `Resource` (`.tres`) fayllarda. Kod ichida hardcode qiymat yo'q. Sabab: kelajakdagi era'lar = yangi data fayl, kod refactor emas.
6. **Placeholder-first.** 3D model hali yo'q. `CapsuleMesh` / `BoxMesh` ishlat. Model yo'qligi hech narsani bloklamaydi — real model'lar Faza 8'da ulanadi.

---

## Stack

- **Godot 4.x — .NET/C# versiyasi** (oddiy Godot emas)
- **.NET 8 SDK**
- **C# faqat. GDScript yozma.**

---

## Vazifa taqsimoti

**Sen (Claude Code):**
- C# skriptlar yozasan / tahrirlaysan (asosiy ish)
- `dotnet build` bilan compile tekshirasan
- Foydalanuvchiga **aniq node-tree ko'rsatmasi** berasan: qaysi node, qaysi bola, script qayerga biriktiriladi, `[Export]` qiymatlari nima
- `.tscn` / `.tres` ni matn sifatida yoza olasan — lekin foydalanuvchi editor'da ochib tekshirishi shart

**Foydalanuvchi (Humoyun):**
- Editor'da scene quradi / tekshiradi
- Autoload'ni Project Settings → Autoload'ga ro'yxatdan o'tkazadi
- Input Map sozlaydi
- **Playtest qiladi** (sen qila olmaysan) va feedback beradi

Har faza faqat foydalanuvchi playtest qilib tasdiqlagach "DONE" bo'ladi.

---

## Godot 4 C# konventsiyalari

- Har bir Godot sinf **`partial`**: `public partial class PlayerController : CharacterBody3D`
- Editor'ga chiqarish: `[Export] private float _moveSpeed = 5.0f;`
- Resource sinflar: `[GlobalClass] public partial class WeaponData : Resource` (editor'da ko'rinishi uchun `[GlobalClass]` shart)
- Signal: `[Signal] public delegate void MissionCompletedEventHandler(string missionId);` → `EmitSignal(SignalName.MissionCompleted, id);` (nom `EventHandler` bilan tugashi majburiy)
- Lifecycle: `_Ready()`, `_Process(double delta)`, `_PhysicsProcess(double delta)` — harakat/fizika `_PhysicsProcess` da
- Harakat: `CharacterBody3D` → `Velocity` + `MoveAndSlide()`
- Autoload singleton: `public static GameManager Instance { get; private set; }` → `_Ready()` da set qilinadi
- Nomlash: private field `_camelCase`, public `PascalCase`, fayl nomi = sinf nomi (`PlayerController.cs`)
- Kod va comment — **English**. O'yin ichidagi matn keyin lokalizatsiya qilinadi (uz/ru/en).

---

## Buyruqlar

```bash
dotnet build          # compile tekshirish (sen bajarasan)
```
O'yinni ishga tushirish — Godot editor'da F5 (foydalanuvchi bajaradi).

---

## `.gitignore`

```
.godot/
bin/
obj/
*.user
/android/
export/
```

---

## Muhim eslatma

Bu **o'yin loyihasi**, backend emas. Mening odatiy .NET backend konventsiyalarim (EF Core, CQRS/MediatR, Postgres, DTO mapping) bu yerda **ishlatilmaydi** — ular bu kontekstda ahamiyatsiz. Bu yerda amal qiladigan yagona umumiy prinsip: **data-driven, hardcode yo'q, modular**.
