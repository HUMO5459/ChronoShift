using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Procedural sound — audio.js, sample for sample. Not one audio file: every
/// effect, the wind and the melody are synthesised live into one
/// AudioStreamGenerator, the way the web build builds WebAudio graphs.
/// </summary>
/// <remarks>
/// The WebAudio nodes are reproduced as the maths they compute: oscillators
/// (sine/triangle/square/sawtooth, the last two PolyBLEP band-limited like the
/// browser's), a 2 s white-noise buffer, RBJ biquads with WebAudio's own Q
/// conventions (bandpass: linear Q, constant 0 dB peak; lowpass: Q in dB), gain
/// envelopes with exponential ramps to 0.0001, and linear parameter ramps for the
/// master / music / wind levels. setTimeout offsets become sample offsets.
///
/// Gains: master 0.55 (audio.js:18), music 0 → 0.30 over 2.5 s, wind from shamol().
/// The node runs while the tree is paused, as the web's AudioContext does.
/// </remarks>
public partial class BuxoroAudio : Node
{
    public static BuxoroAudio? Instance { get; private set; }

    private const int Rate = 44100;

    /// <summary>
    /// The one generator of the process, deliberately never freed. Godot's
    /// AudioStreamGeneratorPlayback keeps a RAW pointer to its generator, and the mixer
    /// thread mixes a stopped playback once more to fade it out — so a generator freed
    /// with its player is a use-after-free on that thread, at exit and on a scene change.
    /// Measured: 3 of 33 --test-oyin exits died with 0xC0000005 in
    /// _get_stream_sampling_rate (audio_stream_generator.cpp), no managed frames.
    /// </summary>
    private static AudioStreamGenerator? _generator;

    private AudioStreamPlayer _player = null!;
    private AudioStreamGeneratorPlayback? _pb;
    private long _n; // samples written so far — ctx.currentTime * Rate

    private readonly Param _master = new(0.55);
    private readonly Param _musiqaGain = new(0.0);
    private readonly Param _shamolGain = new(0.0);

    private bool _ovozYoniq = true;
    private bool _musiqaYoniq;
    private double _musiqaTezlik = 1;
    private double _keyingiDavr; // seconds: when the next melody cycle starts

    private readonly List<Ovoz> _ovozlar = new();
    private static readonly float[] Shovqin = ShovqinYarat();

    // wind: noise → bandpass(420 ± 190 Hz at 0.09 Hz, Q 0.6)
    private Biquad _shamolFiltr;
    private long _shamolIdx;

    /// <summary>audio.js:11-30. Called when the episode starts.</summary>
    public static BuxoroAudio Boshla(Node ota)
    {
        if (Instance != null && IsInstanceValid(Instance))
        {
            return Instance;
        }

        var a = new BuxoroAudio { Name = "BuxoroAudio", ProcessMode = ProcessModeEnum.Always };
        ota.AddChild(a);
        return a;
    }

    public override void _Ready()
    {
        Instance = this;
        if (_generator == null)
        {
            _generator = new AudioStreamGenerator { MixRate = Rate, BufferLength = 0.12f };
            _generator.Reference(); // a native reference of its own: survives the C# wrapper's dispose at shutdown
        }

        _player = new AudioStreamPlayer { Stream = _generator };
        AddChild(_player);
        _player.Play();
        _pb = (AudioStreamGeneratorPlayback)_player.GetStreamPlayback();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private double Hozir => (double)_n / Rate;

    // ================= public API (window.AUDIO) =================

    /// <summary><c>AUDIO.S[nom]()</c> — a one-shot effect by name (audio.js:92-113).</summary>
    public static void S(string nom) => Instance?.Effekt(nom);

    /// <summary>audio.js:143-166.</summary>
    public static void Musiqa(bool yoq, double tezlik = 1.0) => Instance?.MusiqaQoy(yoq, tezlik);

    /// <summary>audio.js:60-63.</summary>
    public static void Shamol(double daraja, double vaqt = 1.5)
    {
        if (Instance == null)
        {
            return;
        }

        Instance._shamolGain.Ramp(Instance.Hozir, daraja, Instance.Hozir + vaqt);
    }

    /// <summary>audio.js:168-171 — the tab went to the background, or came back.</summary>
    public static void OvozYoq(bool yoq)
    {
        if (Instance == null)
        {
            return;
        }

        Instance._ovozYoniq = yoq;
        Instance._master.Ramp(Instance.Hozir, yoq ? 0.55 : 0.0, Instance.Hozir + 0.2);
    }

    public static bool Tayyor => Instance != null;

    public static bool MusiqaYoniqmi => Instance?._musiqaYoniq ?? false;

    /// <summary>For --test-qayta: samples written so far by the current episode's audio.</summary>
    public static long Yozilgan => Instance?._n ?? 0;

    // ================= effects =================

    private void Effekt(string nom)
    {
        switch (nom)
        {
            case "bolga":
                Zarb(new ZarbOpt { Chastota = 2100, Uzunlik = 0.13, Hajm = 0.28, Shovqin = true, Q = 5 });
                Zarb(new ZarbOpt { Chastota = 780, Oxirgi = 300, Uzunlik = 0.22, Hajm = 0.16, Tur = Shakl.Square });
                break;
            case "qadam":
                Zarb(new ZarbOpt { Chastota = 160, Uzunlik = 0.07, Hajm = 0.09, Shovqin = true, Lowpass = true });
                break;
            case "tuyoq":
                Zarb(new ZarbOpt { Chastota = 110, Uzunlik = 0.10, Hajm = 0.20, Shovqin = true, Lowpass = true });
                break;
            case "kaltak":
                Zarb(new ZarbOpt { Chastota = 210, Oxirgi = 80, Uzunlik = 0.17, Hajm = 0.26, Tur = Shakl.Square });
                break;
            case "qilich":
                Zarb(new ZarbOpt { Chastota = 3000, Uzunlik = 0.10, Hajm = 0.22, Shovqin = true, Q = 8 });
                Zarb(new ZarbOpt { Chastota = 620, Oxirgi = 190, Uzunlik = 0.20, Hajm = 0.20, Tur = Shakl.Sawtooth });
                break;
            case "kamon":
                Zarb(new ZarbOpt { Chastota = 1500, Uzunlik = 0.09, Hajm = 0.15, Shovqin = true, Q = 3 });
                break;
            case "blok":
                Zarb(new ZarbOpt { Chastota = 1100, Uzunlik = 0.14, Hajm = 0.24, Shovqin = true, Q = 6 });
                break;
            case "olim":
                Zarb(new ZarbOpt { Chastota = 220, Oxirgi = 55, Uzunlik = 0.45, Hajm = 0.20, Tur = Shakl.Sawtooth });
                break;
            case "zarba":
                Zarb(new ZarbOpt { Chastota = 90, Oxirgi = 40, Uzunlik = 0.28, Hajm = 0.30, Tur = Shakl.Sine });
                break;
            case "belgi":
                Zarb(new ZarbOpt { Chastota = 880, Oxirgi = 1320, Uzunlik = 0.09, Hajm = 0.10, Tur = Shakl.Sine });
                break;
            case "yigildi":
                Zarb(new ZarbOpt { Chastota = 520, Oxirgi = 700, Uzunlik = 0.11, Hajm = 0.09, Tur = Shakl.Sine });
                break;
            case "rad":
                Zarb(new ZarbOpt { Chastota = 200, Oxirgi = 120, Uzunlik = 0.12, Hajm = 0.14, Tur = Shakl.Square });
                break;
            case "shogird":
                Zarb(new ZarbOpt { Chastota = 440, Oxirgi = 660, Uzunlik = 0.16, Hajm = 0.14, Tur = Shakl.Sine });
                Zarb(new ZarbOpt { Chastota = 660, Oxirgi = 880, Uzunlik = 0.20, Hajm = 0.12, Tur = Shakl.Sine }, 0.110);
                break;
            case "vaqt": // ChronoShift: the clock jumps back
                double[] f = { 523, 659, 784, 1046 };
                for (int i = 0; i < f.Length; i++)
                {
                    Zarb(new ZarbOpt { Chastota = f[i], Uzunlik = 0.55, Hajm = 0.16, Tur = Shakl.Sine }, i * 0.085);
                }

                break;
            case "qushUchdi":
                Zarb(new ZarbOpt { Chastota = 300, Oxirgi = 1200, Uzunlik = 0.55, Hajm = 0.13, Tur = Shakl.Sine });
                break;
            default:
                GD.PushWarning($"[buxoro] unknown sound '{nom}'");
                break;
        }
    }

    /// <summary>audio.js:66-90. <paramref name="kechikish"/> is a setTimeout in seconds.</summary>
    private void Zarb(ZarbOpt o, double kechikish = 0)
    {
        if (!_ovozYoniq)
        {
            return;
        }

        var v = new Ovoz
        {
            Bosh = _n + (long)(kechikish * Rate),
            Uzunlik = o.Uzunlik,
            Hajm = o.Hajm,
            Shovqinli = o.Shovqin,
            Tur = o.Tur,
            F0 = o.Chastota,
            F1 = o.Oxirgi > 0 ? o.Oxirgi : o.Chastota,
            ShovqinIdx = GD.Randi() % (uint)Shovqin.Length,
        };
        if (o.Shovqin)
        {
            v.Filtr = o.Lowpass ? Biquad.Lowpass(o.Chastota, o.Q ?? 1.2) : Biquad.Bandpass(o.Chastota, o.Q ?? 1.2);
        }

        lock (_ovozlar)
        {
            _ovozlar.Add(v);
        }
    }

    // ================= music =================

    // D Phrygian: D Eb F G A Bb C (audio.js:117-122)
    private static readonly (double F, double U)[] Kuy =
    {
        (293.66, 1.2), (311.13, 0.6), (349.23, 1.2), (392.00, 0.6),
        (349.23, 1.2), (311.13, 1.2), (293.66, 2.0),
        (349.23, 0.9), (392.00, 0.9), (440.00, 1.4), (392.00, 0.7),
        (349.23, 1.4), (311.13, 2.2),
    };

    private void MusiqaQoy(bool yoq, double tezlik)
    {
        if (yoq)
        {
            if (_musiqaYoniq)
            {
                return;
            }

            _musiqaYoniq = true;
            _musiqaTezlik = tezlik;
            _musiqaGain.Ramp(Hozir, 0.30, Hozir + 2.5);
            _keyingiDavr = Hozir;
        }
        else
        {
            // §9: when Tarobiy dies the music stops — not abruptly, it fades over 1.2 s.
            _musiqaYoniq = false;
            _musiqaGain.Ramp(Hozir, 0.0001, Hozir + 1.2);
        }
    }

    /// <summary>audio.js:151-156 — schedules one pass of the melody.</summary>
    private void Davr(double boshlanish)
    {
        double k = 0;
        foreach ((double f, double u) in Kuy)
        {
            lock (_ovozlar)
            {
                _ovozlar.Add(new Ovoz
                {
                    Bosh = (long)((boshlanish + k) * Rate),
                    Uzunlik = u * _musiqaTezlik,
                    Nota = true,
                    F0 = f,
                    F1 = f,
                });
            }

            k += u * _musiqaTezlik * 0.92;
        }

        _keyingiDavr = boshlanish + k;
    }

    // ================= mixing =================

    public override void _Process(double delta)
    {
        if (_pb == null)
        {
            return;
        }

        int bor = _pb.GetFramesAvailable();
        if (bor <= 0)
        {
            return;
        }

        var buf = new Vector2[bor];
        for (int i = 0; i < bor; i++)
        {
            double t = (double)_n / Rate;
            if (_musiqaYoniq && t >= _keyingiDavr)
            {
                Davr(_keyingiDavr);
            }

            double effekt = 0, nota = 0;
            lock (_ovozlar)
            {
                for (int j = _ovozlar.Count - 1; j >= 0; j--)
                {
                    Ovoz v = _ovozlar[j];
                    if (_n < v.Bosh)
                    {
                        continue;
                    }

                    double tt = (double)(_n - v.Bosh) / Rate;
                    double oxir = v.Nota ? v.Uzunlik + 0.05 : v.Uzunlik;
                    if (tt >= oxir)
                    {
                        _ovozlar.RemoveAt(j);
                        continue;
                    }

                    if (v.Nota)
                    {
                        nota += NotaNamuna(v, tt);
                    }
                    else
                    {
                        effekt += ZarbNamuna(v, tt);
                    }
                }
            }

            double shamol = ShamolNamuna(t);
            double s = _master.Qiymat(t) * (effekt + (_musiqaGain.Qiymat(t) * nota) + (_shamolGain.Qiymat(t) * shamol));
            float x = (float)Math.Clamp(s, -1, 1);
            buf[i] = new Vector2(x, x);
            _n++;
        }

        _pb.PushBuffer(buf);
    }

    private static double ZarbNamuna(Ovoz v, double tt)
    {
        double g = v.Hajm * Math.Pow(0.0001 / v.Hajm, tt / v.Uzunlik); // exponentialRamp to 0.0001
        double x;
        if (v.Shovqinli)
        {
            x = v.Filtr.Process(Shovqin[v.ShovqinIdx]);
            v.ShovqinIdx = (v.ShovqinIdx + 1) % (uint)Shovqin.Length;
        }
        else
        {
            double f = v.F0 * Math.Pow(v.F1 / v.F0, tt / v.Uzunlik);
            x = Osc(v.Tur, v.Faza, f / Rate);
            v.Faza += f / Rate;
            v.Faza -= Math.Floor(v.Faza);
        }

        return g * x;
    }

    /// <summary>audio.js:124-141 — triangle + a sine 0.5 % sharp at 0.35, both enveloped.</summary>
    private static double NotaNamuna(Ovoz v, double tt)
    {
        double g = tt < 0.06
            ? 0.0001 * Math.Pow(0.20 / 0.0001, tt / 0.06)
            : tt < v.Uzunlik ? 0.20 * Math.Pow(0.0001 / 0.20, (tt - 0.06) / (v.Uzunlik - 0.06)) : 0.0001;
        double a = Osc(Shakl.Triangle, v.Faza, v.F0 / Rate);
        double b = Math.Sin(2 * Math.PI * v.Faza2);
        v.Faza += v.F0 / Rate;
        v.Faza -= Math.Floor(v.Faza);
        v.Faza2 += v.F0 * 1.005 / Rate;
        v.Faza2 -= Math.Floor(v.Faza2);
        return g * (a + (0.35 * b));
    }

    /// <summary>audio.js:44-58. The LFO is added to the filter's 420 Hz base.</summary>
    private double ShamolNamuna(double t)
    {
        if ((_n & 31) == 0)
        {
            double f = 420 + (190 * Math.Sin(2 * Math.PI * 0.09 * t));
            _shamolFiltr.SetBandpass(Math.Max(10, f), 0.6);
        }

        double x = _shamolFiltr.Process(Shovqin[_shamolIdx]);
        _shamolIdx = (_shamolIdx + 1) % Shovqin.Length;
        return x;
    }

    private static double Osc(Shakl tur, double faza, double dfaza)
    {
        switch (tur)
        {
            case Shakl.Sine:
                return Math.Sin(2 * Math.PI * faza);
            case Shakl.Square:
            {
                double y = faza < 0.5 ? 1 : -1;
                y += PolyBlep(faza, dfaza);
                y -= PolyBlep((faza + 0.5) % 1.0, dfaza);
                return y;
            }

            case Shakl.Sawtooth:
                return (2 * faza) - 1 - PolyBlep(faza, dfaza);
            default: // triangle, peak 1 like WebAudio's
                return (4 * Math.Abs(faza - 0.5)) - 1;
        }
    }

    private static double PolyBlep(double t, double dt)
    {
        if (t < dt)
        {
            t /= dt;
            return t + t - (t * t) - 1;
        }

        if (t > 1 - dt)
        {
            t = (t - 1) / dt;
            return (t * t) + t + t + 1;
        }

        return 0;
    }

    private static float[] ShovqinYarat()
    {
        // audio.js:34-41 — two seconds of Math.random()*2-1, built once
        var r = new Random();
        var d = new float[Rate * 2];
        for (int i = 0; i < d.Length; i++)
        {
            d[i] = (float)((r.NextDouble() * 2) - 1);
        }

        return d;
    }

    // ================= small types =================

    private enum Shakl
    {
        Triangle,
        Sine,
        Square,
        Sawtooth,
    }

    private struct ZarbOpt
    {
        public double Chastota, Oxirgi, Uzunlik, Hajm;
        public double? Q;
        public bool Shovqin, Lowpass;
        public Shakl Tur;
    }

    private sealed class Ovoz
    {
        public long Bosh;
        public double Uzunlik, Hajm, F0, F1, Faza, Faza2;
        public bool Shovqinli, Nota;
        public Shakl Tur;
        public Biquad Filtr;
        public uint ShovqinIdx;
    }

    /// <summary>An AudioParam with one pending linear ramp — all audio.js schedules.</summary>
    private sealed class Param
    {
        private double _v0, _v1, _t0, _t1;

        public Param(double v)
        {
            _v0 = _v1 = v;
        }

        public double Qiymat(double t)
        {
            if (t >= _t1)
            {
                return _v1;
            }

            if (t <= _t0)
            {
                return _v0;
            }

            return _v0 + ((_v1 - _v0) * (t - _t0) / (_t1 - _t0));
        }

        /// <summary>cancelScheduledValues + setValueAtTime(current) + linearRampToValueAtTime.</summary>
        public void Ramp(double hozir, double maqsad, double oxir)
        {
            _v0 = Qiymat(hozir);
            _t0 = hozir;
            _v1 = maqsad;
            _t1 = Math.Max(oxir, hozir + 1e-6);
        }
    }

    /// <summary>RBJ biquad with the WebAudio parameter conventions.</summary>
    private struct Biquad
    {
        private double _b0, _b1, _b2, _a1, _a2, _x1, _x2, _y1, _y2;

        public static Biquad Bandpass(double f, double q)
        {
            var b = default(Biquad);
            b.SetBandpass(f, q);
            return b;
        }

        public static Biquad Lowpass(double f, double qDb)
        {
            double w0 = 2 * Math.PI * f / Rate, c = Math.Cos(w0);
            double alpha = Math.Sin(w0) / (2 * Math.Pow(10, qDb / 20));
            double a0 = 1 + alpha;
            return new Biquad
            {
                _b0 = (1 - c) / 2 / a0, _b1 = (1 - c) / a0, _b2 = (1 - c) / 2 / a0,
                _a1 = -2 * c / a0, _a2 = (1 - alpha) / a0,
            };
        }

        public void SetBandpass(double f, double q)
        {
            double w0 = 2 * Math.PI * f / Rate;
            double alpha = Math.Sin(w0) / (2 * q);
            double a0 = 1 + alpha;
            _b0 = alpha / a0;
            _b1 = 0;
            _b2 = -alpha / a0;
            _a1 = -2 * Math.Cos(w0) / a0;
            _a2 = (1 - alpha) / a0;
        }

        public double Process(double x)
        {
            double y = (_b0 * x) + (_b1 * _x1) + (_b2 * _x2) - (_a1 * _y1) - (_a2 * _y2);
            _x2 = _x1;
            _x1 = x;
            _y2 = _y1;
            _y1 = y;
            return y;
        }
    }
}
