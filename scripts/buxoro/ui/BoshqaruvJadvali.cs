using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>Georgia (the body font, styles.css:6) in the weights and slants the menus use, with CSS letter-spacing.</summary>
public static class MenyuShrift
{
    private static readonly Dictionary<(bool Kursiv, int Vazn), SystemFont> Kesh = new();

    /// <summary><paramref name="oraliqPx"/> is CSS letter-spacing in pixels (em × font size).</summary>
    public static Font Georgia(float oraliqPx = 0, bool kursiv = false, int vazn = 400)
    {
        if (!Kesh.TryGetValue((kursiv, vazn), out SystemFont? f))
        {
            f = new SystemFont { FontNames = new[] { "Georgia", "Times New Roman", "serif" }, FontItalic = kursiv, FontWeight = vazn };
            Kesh[(kursiv, vazn)] = f;
        }

        int o = (int)Math.Round(oraliqPx);
        return o == 0 ? f : new FontVariation { BaseFont = f, SpacingGlyph = o };
    }

    /// <summary>A text run: size and letter-spacing in vmin/em, colour, CSS opacity.</summary>
    public static Label Yozuv(string matn, float px, Color rang, float alfa = 1, float oraliqEm = 0, bool kursiv = false, int vazn = 400)
    {
        var l = new Label { Text = matn, MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1, 1, 1, alfa) };
        l.AddThemeFontOverride("font", Georgia(oraliqEm * px, kursiv, vazn));
        l.AddThemeFontSizeOverride("font_size", Math.Max(6, (int)Math.Round(px)));
        l.AddThemeColorOverride("font_color", rang);
        return l;
    }
}

/// <summary>
/// The controls tables, #yordamKlav and #yordamSensor, index.html:103-119 + styles.css:386-392
/// — shown on the menu's BOSHQARUV page and cloned into the pause card (main.js:229-232).
/// Which one shows follows the device (main.js:387-390). Uzbek only, as in the source (it
/// lives in HTML, outside every dictionary).
/// </summary>
public static class BoshqaruvJadvali
{
    private static readonly Color Krem = new("#e8dfc9"), Oltin = new("#f2c14e");

    /// <summary>
    /// key / bare text / description. The bare " (ushlab tur)" sits between the &lt;b&gt;
    /// and the &lt;span&gt; in the HTML, so in the flex row it is an item of its own.
    /// </summary>
    private static readonly (string Kalit, string? Orta, string Izoh)[] Qatorlar =
    {
        ("W A S D", null, "yurish"),
        ("Shift", null, "yugurish"),
        ("Sichqoncha", null, "kamerani burish — tugma shart emas"),
        ("E", null, "BELGILASH — resursni shogirdlarga tayinlash"),
        ("E", "(ushlab tur)", "hunarmand yollash (4 s) / otga minish"),
        ("F", "(ushlab tur)", "devor qurish / qurol yasash"),
        ("Chap tugma", null, "urish"),
        ("O‘ng tugma", null, "g‘alvir-qalqon bilan bloklash"),
        ("Space", null, "kadrni o‘tkazib yuborish"),
    };

    /// <summary>#yordamSensor, index.html:114-119.</summary>
    private static readonly (string Kalit, string? Orta, string Izoh)[] SensorQatorlar =
    {
        ("Chap yarim", null, "yurish (to‘liq bursangiz yuguradi)"),
        ("O‘ng yarim", null, "surib qarash"),
        ("Resursga teging", null, "belgilash"),
        ("◈ ✋ F ⚔ 🛡", null, "tugmalar faqat kerak bo‘lganda chiqadi"),
    };

    /// <summary>
    /// Both tables, the device's one visible — the web's <c>hidden</c> attribute. Re-checked
    /// whenever the page opens, as a Windows touchscreen is known only after its first touch.
    /// </summary>
    public static Control Qur(float v)
    {
        var ust = new VBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        ust.AddThemeConstantOverride("separation", 0);
        Control klav = Jadval(Qatorlar, v), sensor = Jadval(SensorQatorlar, v);
        ust.AddChild(klav);
        ust.AddChild(sensor);
        void Tanla()
        {
            bool sensorli = Touch.Bormi();
            klav.Visible = !sensorli;
            sensor.Visible = sensorli;
        }

        Tanla();
        ust.VisibilityChanged += Tanla;
        return ust;
    }

    /// <summary>.boshqaruvJadval at <paramref name="v"/> px per vmin: max-width 70vmin, rows 1.85vmin.</summary>
    private static Control Jadval((string Kalit, string? Orta, string Izoh)[] satrlar, float v)
    {
        var jadval = new VBoxContainer { CustomMinimumSize = new Vector2(70 * v, 0), MouseFilter = Control.MouseFilterEnum.Ignore };
        jadval.AddThemeConstantOverride("separation", 0);
        float px = 1.85f * v;
        foreach ((string kalit, string? orta, string izoh) in satrlar)
        {
            // padding .85vmin 0, border-bottom 1px rgba(232,223,201,.10)
            var sb = new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = new Color(Krem, 0.10f), BorderWidthBottom = 1 };
            sb.ContentMarginTop = sb.ContentMarginBottom = 0.85f * v;
            var qator = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
            qator.AddThemeStyleboxOverride("panel", sb);

            var h = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
            h.AddThemeConstantOverride("separation", (int)Math.Round(2 * v));
            Label b = MenyuShrift.Yozuv(kalit, px, Oltin, vazn: 600);
            b.CustomMinimumSize = new Vector2(16 * v, 0); // flex: 0 0 16vmin
            h.AddChild(b);
            if (orta != null)
            {
                h.AddChild(MenyuShrift.Yozuv(orta, px, Krem));
            }

            h.AddChild(MenyuShrift.Yozuv(izoh, px, Krem, 0.7f));
            qator.AddChild(h);
            jadval.AddChild(qator);
        }

        return jadval;
    }
}
