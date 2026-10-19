using System;
using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// Grass and flowers — otlar.js. Hundreds of copies in one draw call.
/// </summary>
/// <remarks>
/// The point is not prettiness but A SENSE OF DISTANCE: on an empty field running
/// speed cannot be felt; near tufts passing fast and far ones slowly make motion
/// readable (script §2: "the player must always feel he is running").
///
/// OWN STREAM, seed 4711 (otlar.js:13). It never touches the world stream, so
/// leaving it out or reordering it moves no resource node.
/// </remarks>
public static class Otlar
{
    private static BuxoroBalance B => BuxoroRuntime.B;

    /// <summary>otlar.js:18-23. Dark on purpose: flat blades face up and take full light.</summary>
    private static readonly (string Nom, int Soni, double Min, double Max, uint Rang)[] Tur =
    {
        ("ot_past", 800, 0.8, 1.5, 0x6b6a56),
        ("ot_tutam", 600, 0.7, 1.4, 0x77704f),
        ("butta", 120, 0.8, 1.3, 0x5f5e4c),
        ("gul_sariq", 150, 0.9, 1.2, 0x8f7f3e),
    };

    /// <summary>otlar.js:62-63 — (height, blades) for the procedural fallback.</summary>
    private static readonly Dictionary<string, (double H, int N)> Par = new()
    {
        ["ot_past"] = (0.16, 6),
        ["ot_tutam"] = (0.34, 8),
        ["butta"] = (0.62, 7),
        ["gul_sariq"] = (0.28, 4),
    };

    /// <summary>otlar.js:82-123.</summary>
    public static List<MultiMeshInstance3D> Qur(Node3D scene)
    {
        var lcg = BuxoroRuntime.NewGrassLcg();
        var guruhlar = new List<MultiMeshInstance3D>();

        foreach (var t in Tur)
        {
            (Mesh geo, Material mat) = GeometriyaOl(lcg, t.Nom, t.Rang);

            var mm = new MultiMesh
            {
                TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                Mesh = geo,
                InstanceCount = t.Soni,
            };

            int qoyildi = 0, urinish = 0;
            while (qoyildi < t.Soni && urinish < t.Soni * 6)
            {
                urinish++;

                // across the field, east of the spawn zone
                double x = 14 + (lcg.Next() * (B.MaydonKengligi - 26));
                double z = -46 + (lcg.Next() * 92);
                if (!JoyBoshmi(x, z))
                {
                    continue;
                }

                double y = World.YerBalandligi(x, z);
                double k = t.Min + (lcg.Next() * (t.Max - t.Min));
                double bur = lcg.Next() * Math.PI * 2;
                double sy = k * (0.85 + (lcg.Next() * 0.4));

                // three.js compose(P, Q, S) = T · R · S
                var basis = new Basis(Vector3.Up, (float)bur) * Basis.FromScale(new Vector3((float)k, (float)sy, (float)k));
                mm.SetInstanceTransform(qoyildi, new Transform3D(basis, new Vector3((float)x, (float)y, (float)z)));
                qoyildi++;
            }

            mm.VisibleInstanceCount = qoyildi;
            var im = new MultiMeshInstance3D
            {
                Name = t.Nom,
                Multimesh = mm,
                MaterialOverride = mat,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off, // grass casts no shadow — keep it cheap
            };
            scene.AddChild(im);
            guruhlar.Add(im);
        }

        return guruhlar;
    }

    /// <summary>otlar.js:47-69. The GLB path takes the first mesh; none exists today.</summary>
    private static (Mesh, Material) GeometriyaOl(Lcg lcg, string nom, uint rang)
    {
        Node3D? glb = BuxoroModels.Ol(nom);
        if (glb != null)
        {
            foreach (Node n in glb.FindChildren("*", nameof(MeshInstance3D), true, false))
            {
                if (n is MeshInstance3D mi && mi.Mesh != null)
                {
                    Material m = mi.GetActiveMaterial(0) ?? Actors.Mat(rang, 1f);
                    glb.Free();
                    return (mi.Mesh, m);
                }
            }

            glb.Free();
        }

        (double h, int barglar) = Par.TryGetValue(nom, out var p) ? p : (0.3, 6);
        var mat = new StandardMaterial3D
        {
            AlbedoColor = Actors.Rang(rang),
            Roughness = 1f,
            Metallic = 0f,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled, // side: DoubleSide
        };
        return (ProtseduralTutam(lcg, h, barglar), mat);
    }

    /// <summary>
    /// otlar.js:27-45 — a few flat ribbons, tapering, tips bent. Flat because a
    /// tube wastes polygons. Normals point sideways: the blade is flat, not up.
    /// Draws per blade: angle, bend, height, width — in that order.
    /// </summary>
    private static Mesh ProtseduralTutam(Lcg lcg, double balandlik, int barglar)
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        for (int i = 0; i < barglar; i++)
        {
            double bur = ((double)i / barglar * Math.PI * 2) + (lcg.Next() * 0.7);
            double egilish = 0.25 + (lcg.Next() * 0.45);
            double h = balandlik * (0.55 + (lcg.Next() * 0.6));
            double kx = Math.Cos(bur), kz = Math.Sin(bur);
            double w = 0.022 + (lcg.Next() * 0.016);
            double ux = kx * h * egilish, uz = kz * h * egilish;

            var nrm = new Vector3((float)(kx * 0.7), 0.5f, (float)(kz * 0.7)).Normalized();
            st.SetNormal(nrm);
            st.AddVertex(new Vector3((float)(-kz * w), 0, (float)(kx * w)));
            st.SetNormal(nrm);
            st.AddVertex(new Vector3((float)(kz * w), 0, (float)(-kx * w)));
            st.SetNormal(nrm);
            st.AddVertex(new Vector3((float)ux, (float)h, (float)uz));
        }

        return st.Commit();
    }

    /// <summary>otlar.js:72-80 — not on the road, the bazaar, the forge or the wall line.</summary>
    private static bool JoyBoshmi(double x, double z)
    {
        if (Math.Abs(x - B.DevorChizigiX) < 3.5)
        {
            return false; // wall line
        }

        double dx = x - B.Ustaxona.X, dz = z - B.Ustaxona.Y;
        if ((dx * dx) + (dz * dz) < 60)
        {
            return false; // forge
        }

        double bx = x - B.Bozor.X, bz = z - B.Bozor.Y;
        if ((bx * bx) + (bz * bz) < 90)
        {
            return false; // bazaar
        }

        if (x > B.DarvozaX - 12 && Math.Abs(z) < 12)
        {
            return false; // gate road
        }

        return true;
    }
}
