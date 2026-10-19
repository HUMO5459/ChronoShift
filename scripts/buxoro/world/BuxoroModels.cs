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

    private static PackedScene? Sahna(string nom)
    {
        if (!Scenes.TryGetValue(nom, out PackedScene? s))
        {
            string path = Dir + nom + ".glb";
            s = ResourceLoader.Exists(path) ? ResourceLoader.Load<PackedScene>(path) : null;
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
        foreach (Node n in obj.FindChildren("*", nameof(Skeleton3D), true, false))
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

        foreach (Node child in node.GetChildren())
        {
            Transform3D ct = child is Node3D c3 ? t * c3.Transform : t;
            Accumulate(child, ct, ref box, ref first);
        }
    }
}
