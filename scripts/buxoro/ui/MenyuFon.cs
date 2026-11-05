using System;
using Godot;

namespace ChronoShift;

/// <summary>
/// The menu background — kirish.mp4 (here kirish.ogv), muted and looping, on
/// both menu levels (menyu.js:8, main.js:392-399). Two looks, styles.css:
///   Buxoro menu  (#menyuVideo, :308-311): opacity .42, blur(3px) saturate(.75), scale 1.06
///   series sheet (:584-587, #menyu:has(#seriyaMenyu.yon)): opacity .95, no filter, no scale
/// Covers its parent (object-fit: cover), clipped.
/// </summary>
public partial class MenyuFon : Control
{
    public const string Video = "res://assets/video/buxoro/kirish.ogv";

    private const string FiltrShader = @"
shader_type canvas_item;
// CSS filter: blur(Npx) saturate(s), then opacity
uniform float sigma = 0.0;      // blur std-dev in TEXTURE pixels; 0 = none
uniform float doygunlik = 1.0;  // saturate()
uniform float alfa = 1.0;       // opacity
void fragment() {
    vec4 c;
    if (sigma < 0.05) {
        c = texture(TEXTURE, UV);
    } else {
        vec4 s = vec4(0.0);
        float w = 0.0;
        for (int i = -3; i <= 3; i++) {
            for (int j = -3; j <= 3; j++) {
                vec2 o = vec2(float(i), float(j)) * sigma * 0.75;
                float k = exp(-dot(o, o) / (2.0 * sigma * sigma));
                s += texture(TEXTURE, UV + o * TEXTURE_PIXEL_SIZE) * k;
                w += k;
            }
        }
        c = s / w;
    }
    // the CSS saturate() matrix: s*c + (1-s)*luma, luma weights .213/.715/.072
    float l = dot(c.rgb, vec3(0.213, 0.715, 0.072));
    c.rgb = mix(vec3(l), c.rgb, doygunlik);
    COLOR = vec4(c.rgb, c.a * alfa);
}";

    /// <summary>
    /// .chizmaTur (styles.css:457-468) without its grid (the sheet draws its own):
    /// dark on the left so the text reads, nearly clear on the right so the video shows.
    /// </summary>
    private const string VaraqShader = @"
shader_type canvas_item;
uniform vec2 olcham = vec2(1920.0, 1080.0);
vec4 pm(vec3 rgb, float a) { return vec4(rgb / 255.0 * a, a); }
void fragment() {
    vec2 p = UV * olcham;
    // linear-gradient(100deg, …) — CSS gradient line through the centre
    float u = radians(100.0);
    vec2 yon = vec2(sin(u), -cos(u));
    float L = abs(olcham.x * sin(u)) + abs(olcham.y * cos(u));
    float t = clamp(dot(p - olcham * 0.5, yon) / L + 0.5, 0.0, 1.0);
    vec4 g1 = t < 0.32 ? mix(pm(vec3(4.0, 17.0, 36.0), 0.92), pm(vec3(5.0, 22.0, 44.0), 0.80), t / 0.32)
            : t < 0.58 ? mix(pm(vec3(5.0, 22.0, 44.0), 0.80), pm(vec3(6.0, 26.0, 50.0), 0.30), (t - 0.32) / 0.26)
            : mix(pm(vec3(6.0, 26.0, 50.0), 0.30), pm(vec3(8.0, 32.0, 62.0), 0.10), (t - 0.58) / 0.42);
    // linear-gradient(180deg, rgba(8,32,62,.24), rgba(4,17,36,.42)) — under the first
    vec4 g2 = mix(pm(vec3(8.0, 32.0, 62.0), 0.24), pm(vec3(4.0, 17.0, 36.0), 0.42), UV.y);
    vec4 r = g1 + g2 * (1.0 - g1.a);
    COLOR = vec4(r.rgb / max(r.a, 1e-4), r.a);
}";

    /// <summary>Turns ChronoShift's opaque menu sheet into the web's translucent series sheet, so the video shows through.</summary>
    public static void VaraqniOch(CanvasItem varaq)
    {
        varaq.Material = new ShaderMaterial { Shader = new Shader { Code = VaraqShader } };
    }

    private VideoStreamPlayer? _video;
    private ShaderMaterial _mat = null!;
    private float _masshtab = 1f, _blurPx;

    /// <summary>
    /// Adds the background to <paramref name="ota"/> at child index <paramref name="indeks"/>,
    /// or returns null when kirish.ogv is missing (the web's &lt;video&gt; then stays empty).
    /// </summary>
    public static MenyuFon? Qosh(Control ota, int indeks, bool seriya)
    {
        if (!ResourceLoader.Exists(Video))
        {
            return null;
        }

        var f = new MenyuFon { Name = "MenyuFon", MouseFilter = MouseFilterEnum.Ignore, ClipContents = true };
        f.SetAnchorsPreset(LayoutPreset.FullRect);
        ota.AddChild(f);
        ota.MoveChild(f, indeks);
        f.Rejim(seriya);
        return f;
    }

    public override void _Ready()
    {
        // #menyu { background: #100d09 } — under the video until its first frame
        var fon = new ColorRect { Color = new Color("#100d09"), MouseFilter = MouseFilterEnum.Ignore };
        fon.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(fon);

        _mat = new ShaderMaterial { Shader = new Shader { Code = FiltrShader } };
        _video = new VideoStreamPlayer
        {
            Stream = ResourceLoader.Load<VideoStream>(Video), Loop = true, Expand = true,
            VolumeDb = -80f, // muted
            MouseFilter = MouseFilterEnum.Ignore, Material = _mat,
        };
        AddChild(_video);
        _video.Play();
        Qayta();
    }

    /// <summary>Switches between the series-sheet look and the Buxoro-menu look.</summary>
    public void Rejim(bool seriya)
    {
        _masshtab = seriya ? 1f : 1.06f;
        _blurPx = seriya ? 0f : 3f;
        if (_mat != null)
        {
            _mat.SetShaderParameter("doygunlik", seriya ? 1f : 0.75f);
            _mat.SetShaderParameter("alfa", seriya ? 0.95f : 0.42f);
        }

        Qayta();
    }

    public override void _Process(double delta) => Qayta();

    /// <summary>object-fit: cover, then transform: scale().</summary>
    private void Qayta()
    {
        if (_video == null)
        {
            return;
        }

        Vector2 t = _video.GetVideoTexture()?.GetSize() ?? Vector2.Zero;
        if (t.X <= 0 || t.Y <= 0)
        {
            t = new Vector2(1280, 720);
        }

        float k = Math.Max(Size.X / t.X, Size.Y / t.Y) * _masshtab;
        _video.Size = t * k;
        _video.Position = (Size - _video.Size) / 2;

        // blur(3px) is in screen pixels; the shader samples texture pixels
        _mat.SetShaderParameter("sigma", _blurPx / k);
        _mat.SetShaderParameter("doygunlik", _blurPx > 0 ? 0.75f : 1f);
        _mat.SetShaderParameter("alfa", _blurPx > 0 ? 0.42f : 0.95f);
    }
}
