using Godot;

namespace ChronoShift;

/// <summary>
/// Action NAMES for the Buxoro episode. Bindings live in project.godot [input]
/// (BUXORO_PORT_PLAN.md correction 2). Nothing under scripts/buxoro may read a
/// ChronoShift action: Buxoro declares its own complete set.
/// </summary>
public static class BuxoroActions
{
    public const string Oldinga = "buxoro_oldinga";             // W + Up
    public const string Orqaga = "buxoro_orqaga";               // S + Down
    public const string Chapga = "buxoro_chapga";               // A + Left
    public const string Ongga = "buxoro_ongga";                 // D + Right
    public const string Sakra = "buxoro_sakra";                 // Space
    public const string Yugur = "buxoro_yugur";                 // Shift (either)
    public const string Belgila = "buxoro_belgila";             // E
    public const string Yasa = "buxoro_yasa";                   // F
    public const string Urish = "buxoro_urish";                 // LMB
    public const string Blok = "buxoro_blok";                   // RMB
    public const string Qurol1 = "buxoro_qurol_1";              // 1
    public const string Qurol2 = "buxoro_qurol_2";              // 2
    public const string Qurol3 = "buxoro_qurol_3";              // 3
    public const string Pauza = "buxoro_pauza";                 // Escape
    public const string VideoKeyingi = "buxoro_video_keyingi";  // Space / Enter
}

/// <summary>
/// The keyboard/mouse half of player.js <c>kirish()</c> (player.js:106-203).
/// Writes the same fields the web's listeners write, so the player code reads
/// one shape whatever the source (<see cref="Touch"/> writes them too).
/// </summary>
/// <remarks>
/// Pointer lock maps to Input.MouseMode.Captured: a left click captures (and
/// still attacks, player.js:169). The web's no-lock fallback (cursor delta +
/// screen-edge turning, player.js:183-195) exists for iframes that refuse the
/// lock; a desktop build always gets the capture, so only the locked path is
/// ported — the deltas and the pitch clamp are identical.
/// </remarks>
public partial class BuxoroInput : Node
{
    /// <summary>The player this input writes to (player.js keeps them on <c>p</c>).</summary>
    public Oyinchi? P;

    /// <summary>Set by the director: no camera turning while paused or in a cutscene.</summary>
    public bool Faol = true;

    /// <summary>
    /// Touch buttons held down (touch.js:164-169). <see cref="Oqi"/> rewrites p.tugmalar
    /// and p.bloklaymi every frame, so a held button joins that poll instead of writing
    /// them once, as the web's event does.
    /// </summary>
    public bool SensorE, SensorF, SensorBlok;

    public override void _UnhandledInput(InputEvent e)
    {
        if (P == null || !Faol)
        {
            return;
        }

        // The mouse Godot synthesises from a touch: touch.js's e.preventDefault() keeps the
        // browser from doing the same, or every tap would swing and grab the cursor.
        // Under --sensorli the real mouse IS the finger.
        if (e is InputEventMouse && (e.Device == InputEvent.DeviceIdEmulation || Input.EmulateTouchFromMouse))
        {
            return;
        }

        if (e is InputEventMouseMotion m && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // player.js:177-180 — the pointer-lock path
            P.Yonalish -= m.Relative.X * 0.0022;
            P.Pitch = Mathf.Clamp(P.Pitch - (m.Relative.Y * 0.0018), -0.85, 0.25);
            return;
        }

        if (e.IsActionPressed(BuxoroActions.Urish))
        {
            // player.js:169 — the click asks for the lock AND swings
            if (Input.MouseMode != Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }

            P.Urmoqchi = true;
        }

        if (e.IsActionPressed(BuxoroActions.Belgila) && !e.IsEcho())
        {
            P.EBosildi = true; // player.js:121
        }

        if (e.IsActionPressed(BuxoroActions.Sakra) && !e.IsEcho())
        {
            P.SpaceBosildi = true; // player.js:122
        }

        if (e.IsActionPressed(BuxoroActions.Qurol1) && !e.IsEcho())
        {
            P.Tugmalar.Digit1 = true;
        }

        if (e.IsActionPressed(BuxoroActions.Qurol2) && !e.IsEcho())
        {
            P.Tugmalar.Digit2 = true;
        }

        if (e.IsActionPressed(BuxoroActions.Qurol3) && !e.IsEcho())
        {
            P.Tugmalar.Digit3 = true;
        }
    }

    /// <summary>
    /// Per-frame polling of the held keys into <c>p.tugmalar</c>. Held state is
    /// polled (not event-driven) so a right-button release outside the window
    /// still drops the shield — player.js:172 binds mouseup on document.
    /// </summary>
    public void Oqi()
    {
        if (P == null)
        {
            return;
        }

        var t = P.Tugmalar;
        t.Oldinga = Input.IsActionPressed(BuxoroActions.Oldinga);
        t.Orqaga = Input.IsActionPressed(BuxoroActions.Orqaga);
        t.Chapga = Input.IsActionPressed(BuxoroActions.Chapga);
        t.Ongga = Input.IsActionPressed(BuxoroActions.Ongga);
        t.Yugur = Input.IsActionPressed(BuxoroActions.Yugur);
        t.E = Input.IsActionPressed(BuxoroActions.Belgila) || SensorE;
        t.F = Input.IsActionPressed(BuxoroActions.Yasa) || SensorF;

        // player.js:275-278: Space jumps once per press — JustPressed has no echo.
        t.Space = Input.IsActionJustPressed(BuxoroActions.Sakra);

        // player.js:170/173 — RMB down/up toggles bloklaymi
        P.Bloklaymi = Input.IsActionPressed(BuxoroActions.Blok) || SensorBlok;

        // player.js:128-129 — releasing E / F resets their hold timers
        if (!t.E)
        {
            P.EUshlandi = 0;
        }

        if (!t.F)
        {
            P.FUshlandi = 0;
        }
    }

    /// <summary>
    /// player.js:202 and main.js:192 — focus loss and pause clear every held key
    /// and the shield. Godot does not guarantee key-ups on focus loss.
    /// </summary>
    public void Tozala()
    {
        if (P == null)
        {
            return;
        }

        P.Tugmalar = new Tugmalar();
        P.Bloklaymi = false;
        SensorE = SensorF = SensorBlok = false;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut || what == NotificationWMWindowFocusOut)
        {
            Tozala();
        }
    }
}

/// <summary>The keys player.js reads from <c>p.tugmalar</c>.</summary>
public sealed class Tugmalar
{
    public bool Oldinga, Orqaga, Chapga, Ongga, Yugur, Space, E, F;

    // game.js:307-316 — set on press, consumed (cleared) by the game
    public bool Digit1, Digit2, Digit3;
}
