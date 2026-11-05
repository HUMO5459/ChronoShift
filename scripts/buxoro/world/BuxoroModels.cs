using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The minimal <c>MODELS.ol</c> (models.js:290) World.cs needs: a fresh instance
/// of an imported Buxoro GLB, or null when the file does not exist. Step 6 grows
/// this into Models.cs (material cloning, skeleton clones, animation hookup).
/// </summary>
/// <remarks>
/// THE NULL TEST IS THE DETERMINISM GATE. World.cs branches on it exactly where
/// world.js branches on <c>MODELS.ol(...)</c>, and the procedural branch draws
/// from the world stream. Adding tugun_yogoch.glb re-rolls all 66 nodes; that must
/// happen loudly (GameDirector checks the draw total), never silently.
///
/// Scale and foot-drop are already baked into each imported scene's root by
/// BuxoroImport, mirroring what models.js:210-286 does to the clone.
/// </remarks>
public static class BuxoroModels
{
    private const string Dir = "res://assets/models/buxoro/";

    private static readonly Dictionary<string, PackedScene?> Scenes = new();

    /// <summary>DEBUG: <c>-- --glbsiz uy_katta,ark_qalasi</c> or <c>hammasi</c> — plays as if these GLBs were not there.</summary>
    private static readonly string[] Glbsiz = GlbsizArg();

    /// <summary>The asset set is not the shipped one, so the draw checkpoints do not apply.</summary>
    public static bool GlbsizMi => Glbsiz.Length > 0;

    private static string[] GlbsizArg()
    {
        string[] a = OS.GetCmdlineUserArgs();
        int i = Array.IndexOf(a, "--glbsiz");
        return i >= 0 && i + 1 < a.Length ? a[i + 1].Split(',') : Array.Empty<string>();
    }

    private static bool Fayl(string nom, string path) =>
        ResourceLoader.Exists(path) && !(GlbsizMi && (Glbsiz[0] == "hammasi" || Array.IndexOf(Glbsiz, nom) >= 0));

    /// <summary>True when <c>{nom}.glb</c> is present.</summary>
    public static bool Bor(string nom) => Sahna(nom) != null;

    /// <summary>A new instance, or null. models.js:290.</summary>
    public static Node3D? Ol(string nom)
    {
        PackedScene? s = Sahna(nom);
        if (s == null)
        {
            return null;
        }

        var n = s.Instantiate<Node3D>();
        n.Name = nom;
        return n;
    }

    /// <summary>
    /// <c>FindChildren("*", T, true, false)</c> — depth-first pre-order, internal
    /// children included — as a C# list. A Godot.Collections.Array result is freed
    /// only by the .NET finalizer, which can run after the engine has shut down
    /// (0xC0000005 in godotsharp_array_destroy on exit), so the episode makes none.
    /// </summary>
    public static List<T> Avlodlar<T>(Node ota)
        where T : class
    {
        var r = new List<T>();
        Yig(ota, r);
        return r;
    }

    private static void Yig<T>(Node n, List<T> r)
        where T : class
    {
        int soni = n.GetChildCount(true);
        for (int i = 0; i < soni; i++)
        {
            Node b = n.GetChild(i, true);
            if (b is T t)
            {
                r.Add(t);
            }

            Yig(b, r);
        }
    }

    /// <summary><c>GetChildren()</c> as a C# list (see <see cref="Avlodlar{T}"/>).</summary>
    public static List<Node> Bolalar(Node ota)
    {
        int soni = ota.GetChildCount();
        var r = new List<Node>(soni);
        for (int i = 0; i < soni; i++)
        {
            r.Add(ota.GetChild(i));
        }

        return r;
    }

    /// <summary>models.js:8-26 KUTILGAN — every model the game asks for; 18 exist as GLB today, the rest are procedural.</summary>
    public static readonly string[] Kutilgan =
    {
        // characters
        "shomurod", "tarobiy", "shogird", "askar",
        "mogul_piyoda", "mogul_otliq", "noyon", "hunarmand", "ot", "humo",
        // weapons
        "kaltak", "galvir_qalqon", "kamon", "qilich",
        // the wall the player builds
        "devor_seksiya", "devor_buzilgan",
        // architecture — fired brick
        "qala_darvoza", "shahar_devori", "ark_qalasi",
        // architecture — mud plaster
        "uy_kichik", "uy_katta", "uy_vayrona", "paxsa_devor",
        // surroundings
        "ustaxona", "bozor_rastasi", "terak",
        // grass (hundreds of instances)
        "ot_tutam", "ot_past", "gul_sariq", "butta",
        // resource nodes
        "tugun_tosh", "tugun_yogoch", "tugun_teri", "tugun_temir", "tugun_komir",
    };

    private static readonly List<string> Yuklanmoqda = new();

    /// <summary>
    /// models.js yukla() — starts a threaded load of every expected GLB while the
    /// menu is up, so starting the game does not stall on disk. A missing file
    /// counts as done at once (the web's 404 → placeholder).
    /// </summary>
    public static void YuklashniBoshla()
    {
        foreach (string nom in Kutilgan)
        {
            if (Scenes.ContainsKey(nom) || Yuklanmoqda.Contains(nom))
            {
                continue;
            }

            string path = Dir + nom + ".glb";
            if (!Fayl(nom, path) ||
                ResourceLoader.LoadThreadedRequest(path, "PackedScene", true) != Error.Ok)
            {
                Scenes[nom] = null;
                continue;
            }

            Yuklanmoqda.Add(nom);
        }
    }

    /// <summary>The loaded share, 0..1 (models.js onProgress: done / KUTILGAN.length). Moves finished loads into the cache.</summary>
    public static double Yuklanish()
    {
        for (int i = Yuklanmoqda.Count - 1; i >= 0; i--)
        {
            string path = Dir + Yuklanmoqda[i] + ".glb";
            ResourceLoader.ThreadLoadStatus st = ResourceLoader.LoadThreadedGetStatus(path);
            if (st == ResourceLoader.ThreadLoadStatus.InProgress)
            {
                continue;
            }

            Scenes[Yuklanmoqda[i]] = st == ResourceLoader.ThreadLoadStatus.Loaded ? ResourceLoader.LoadThreadedGet(path) as PackedScene : null;
            Yuklanmoqda.RemoveAt(i);
        }

        int tayyor = 0;
        foreach (string nom in Kutilgan)
        {
            if (Scenes.ContainsKey(nom))
            {
                tayyor++;
            }
        }

        return tayyor / (double)Kutilgan.Length;
    }

    /// <summary>models.js hisobot() — how many of the expected models are real GLBs.</summary>
    public static (int Glb, int Jami) Hisobot()
    {
        int glb = 0;
        foreach (string nom in Kutilgan)
        {
            if (Scenes.TryGetValue(nom, out PackedScene? s) && s != null)
            {
                glb++;
            }
        }

        return (glb, Kutilgan.Length);
    }

    private static PackedScene? Sahna(string nom)
    {
        if (!Scenes.TryGetValue(nom, out PackedScene? s))
        {
            string path = Dir + nom + ".glb";
            s = Fayl(nom, path) ? ResourceLoader.Load<PackedScene>(path) : null;
            Scenes[nom] = s;
        }

        return s;
    }

    /// <summary>A bone: the skeleton that owns it and its index.</summary>
    public readonly record struct Suyak(Skeleton3D Skelet, int Idx);

    // models.js:331-334 — every generator names the hand differently
    private static readonly System.Text.RegularExpressions.Regex QolR =
        new("^(hand_?r|r_?hand|righthand|hand\\.r|hand_right|right_hand)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    private static readonly System.Text.RegularExpressions.Regex QolL =
        new("^(hand_?l|l_?hand|lefthand|hand\\.l|hand_left|left_hand)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    /// <summary>models.js:336-346 — the hand bone, or null.</summary>
    public static Suyak? Qol(Node obj, char tomon) => SuyakTop(obj, tomon == 'L' ? QolL : QolR);

    /// <summary>player.js:96-103 — the first bone whose name (prefix stripped) matches.</summary>
    public static Suyak? SuyakTop(Node obj, System.Text.RegularExpressions.Regex naqsh)
    {
        foreach (Node n in BuxoroModels.Avlodlar<Skeleton3D>(obj))
        {
            if (n is not Skeleton3D sk)
            {
                continue;
            }

            for (int i = 0; i < sk.GetBoneCount(); i++)
            {
                string nom = sk.GetBoneName(i);
                int c = nom.LastIndexOf(':');
                if (naqsh.IsMatch(c >= 0 ? nom[(c + 1)..] : nom))
                {
                    return new Suyak(sk, i);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// models.js:386-396 — attach to a bone and CANCEL the bone's scale, or the
    /// weapon inherits the skeleton's import scale and balloons. The offset and
    /// rotation are in the bone's own space, exactly as in three.js.
    /// </summary>
    public static bool SuyakkaUla(Suyak? suyak, Node3D obj, Vector3 joy, Vector3 burilish)
    {
        if (suyak is not { } s)
        {
            return false;
        }

        var ba = new BoneAttachment3D { BoneName = s.Skelet.GetBoneName(s.Idx) };
        s.Skelet.AddChild(ba);
        Vector3 m = (s.Skelet.GlobalTransform * s.Skelet.GetBoneGlobalPose(s.Idx)).Basis.Scale;
        obj.Scale = new Vector3(1 / (m.X == 0 ? 1 : m.X), 1 / (m.Y == 0 ? 1 : m.Y), 1 / (m.Z == 0 ? 1 : m.Z));
        obj.Position = joy;
        Actors.JsRotation(obj, burilish.X, burilish.Y, burilish.Z);
        ba.AddChild(obj);
        return true;
    }

    /// <summary>
    /// models.js otBurilish (web 2026-10-19): ot.glb's head points −X while every
    /// character faces +Z, so a horse turned with its rider needs yaw + π/2. The
    /// procedural horse (Actors.Ot) faces +Z and needs nothing.
    /// </summary>
    public static double OtBurilish() => Bor("ot") ? Math.PI / 2 : 0;

    /// <summary>models.js EGAR — rider root above the ground, and how far behind the horse's centre.</summary>
    public const double EgarY = 0.30, EgarOrqa = 0.25;

    private static readonly System.Text.RegularExpressions.Regex Oyoq =
        new("^(L|R)_(Thigh|Calf)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    private static readonly Dictionary<ulong, List<Suyak>> OyoqKesh = new();

    /// <summary>
    /// models.js otir — the seated pose: thigh rotateX(−1.4), calf rotateX(+1.4) in
    /// the bone's own space. Call AFTER the animation advanced this frame (the clip
    /// rewrites these bones every frame, so nothing accumulates) and only while a
    /// clip is playing.
    /// </summary>
    public static bool Otir(Node3D mesh)
    {
        if (!OyoqKesh.TryGetValue(mesh.GetInstanceId(), out List<Suyak>? s))
        {
            s = new List<Suyak>();
            foreach (Node n in BuxoroModels.Avlodlar<Skeleton3D>(mesh))
            {
                if (n is Skeleton3D sk)
                {
                    for (int i = 0; i < sk.GetBoneCount(); i++)
                    {
                        string nom = sk.GetBoneName(i);
                        int c = nom.LastIndexOf(':');
                        if (Oyoq.IsMatch(c >= 0 ? nom[(c + 1)..] : nom))
                        {
                            s.Add(new Suyak(sk, i));
                        }
                    }
                }
            }

            OyoqKesh[mesh.GetInstanceId()] = s;
        }

        foreach (Suyak b in s)
        {
            float a = b.Skelet.GetBoneName(b.Idx).EndsWith("Thigh", StringComparison.OrdinalIgnoreCase) ? -1.4f : 1.4f;
            b.Skelet.SetBonePoseRotation(b.Idx, b.Skelet.GetBonePoseRotation(b.Idx) * new Quaternion(Vector3.Right, a));
        }

        return s.Count > 0;
    }

    /// <summary>
    /// models.js:352-381 — a stand-in hand anchor from the body's bounding box
    /// when the rig has no hand bone, so a weapon does not stick to the pelvis.
    /// </summary>
    public static Node3D QolTayanch(Node3D mesh, char tomon)
    {
        string nom = "__qol_" + tomon;
        if (mesh.FindChild(nom, true, false) is Node3D bor)
        {
            return bor;
        }

        Olcham(mesh, Transform3D.Identity, out Aabb q);
        Vector3 o = q.Size; // world size
        Vector3 m = mesh.Scale;
        float sx = m.X == 0 ? 1 : m.X, sy = m.Y == 0 ? 1 : m.Y, sz = m.Z == 0 ? 1 : m.Z;
        float balandlik = (o.Y > 0 ? o.Y : 1.75f) / sy;
        float kenglik = (o.X > 0 ? o.X : 0.50f) / sx;
        float chuqurlik = (o.Z > 0 ? o.Z : 0.30f) / sz;

        var t = new Node3D
        {
            Name = nom,
            Position = new Vector3((tomon == 'L' ? -1 : 1) * kenglik * 0.42f, balandlik * 0.52f, chuqurlik * 0.18f),
            Scale = new Vector3(1 / sx, 1 / sy, 1 / sz), // keep the weapon in true metres
        };
        mesh.AddChild(t);
        return t;
    }

    /// <summary>
    /// <c>new THREE.Box3().setFromObject(ob)</c> expressed in the frame of
    /// <paramref name="frame"/> (pass the identity for the object's parent space).
    /// Works on detached nodes: transforms are accumulated by hand rather than read
    /// from GlobalTransform.
    /// </summary>
    public static bool Olcham(Node3D ob, Transform3D frame, out Aabb box)
    {
        box = default;
        bool first = true;
        Accumulate(ob, frame * ob.Transform, ref box, ref first);
        return !first;
    }

    private static void Accumulate(Node node, Transform3D t, ref Aabb box, ref bool first)
    {
        if (node is MeshInstance3D mi && mi.Mesh != null)
        {
            Aabb b = t * mi.Mesh.GetAabb();
            box = first ? b : box.Merge(b);
            first = false;
        }

        foreach (Node child in BuxoroModels.Bolalar(node))
        {
            Transform3D ct = child is Node3D c3 ? t * c3.Transform : t;
            Accumulate(child, ct, ref box, ref first);
        }
    }
}
