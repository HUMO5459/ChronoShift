using System.Collections.Generic;
using Godot;

namespace ChronoShift;

/// <summary>
/// The deterministic draw sequence of <c>WORLD.qur</c>, replayed exactly.
/// </summary>
/// <remarks>
/// THE CONTRACT. The Buxoro world is not authored, it is generated from one seed,
/// and the generator's random stream is consumed by the PROCEDURAL PLACEHOLDER
/// GEOMETRY as much as by the positions. Placeholder box counts are load-bearing:
/// shipping tugun_yogoch.glb, or changing how many rocks a stone node scatters,
/// moves all 66 resource nodes and re-rolls the entire Act I economy.
///
/// The stream is conditioned on which GLBs exist. Today exactly one resource-node
/// model is present (tugun_tosh), so stone takes the GLB path and consumes only
/// its two position draws, while wood, hide, iron and coal take the procedural
/// path and consume 16, 1, 35 and 24 extra draws respectively. terak.glb exists,
/// so each poplar costs 2 draws rather than 1.
///
/// The per-type draw counts live in balance_buxoro.tres, not here, so the table
/// and the code cannot disagree.
///
/// Ordered consumers of the stream (world.js), with cumulative index:
///   1. resource nodes   1080   (this file)
///   2. poplars           179 -> 1259  (this file)
///   3. houses              8 -> 1267  (this file)
///   4. workshop            0 -> 1267  (no draws on either branch)
///   5. bazaar              0 -> 1267
///   6. gate ring         207 -> 1474  (step 3)
///   7. Ark                 0 -> 1474
///   8. grass             separate stream, seed 4711
///   9. edge walls         80 -> 1554  (step 3)
/// </remarks>
public static class BuxoroWorldStream
{
    /// <summary>One generated resource node, with the draw index it started at.</summary>
    public readonly record struct NodeEntry(
        int Index, string Tur, int TypeIndex, double X, double Z, int FirstDraw);

    /// <summary>One generated poplar.</summary>
    public readonly record struct TreeEntry(double X, double Z);

    /// <summary>Cumulative draw index after each consumer, for the parity gate.</summary>
    public readonly record struct DrawCounts(int Nodes, int Poplars, int Houses);

    /// <summary>Poplar clusters, as (x, z). Source: world.js:114.</summary>
    private static readonly (double X, double Z)[] Clusters =
    {
        (112, -46), (104, 40), (120, 34), (96, -44), (88, 42),
    };

    /// <summary>
    /// The eight house sites. Source: world.js:248-254. Positions are fixed; only
    /// the yaw is drawn, one draw each, in this order.
    /// </summary>
    private static readonly (string Model, double X, double Z)[] Houses =
    {
        ("uy_katta", 140, -40), ("uy_kichik", 150, -30), ("uy_kichik", 134, -46),
        ("uy_katta", 146, 36), ("uy_kichik", 156, 26),
        ("uy_vayrona", 98, -30), ("uy_vayrona", 108, -22), ("uy_vayrona", 116, -34),
    };

    /// <summary>
    /// Replays the resource-node loop: 1080 draws, 66 nodes, in the order
    /// tosh, yogoch, teri, temir, komir. This is the FIRST consumer of the stream,
    /// so it is self-contained and can be verified on its own.
    /// </summary>
    public static List<NodeEntry> Nodes(Lcg lcg, BuxoroBalance balance)
    {
        var nodes = new List<NodeEntry>();
        int index = 0;

        foreach (BuxoroNodeCountRow row in balance.TugunSoni)
        {
            if (row == null)
            {
                continue;
            }

            for (int i = 0; i < row.Soni; i++)
            {
                int firstDraw = lcg.DrawCount;

                // world.js:83-88 — the array literal draws X then Z, left to right.
                double x = lcg.Between(row.Zona.X, row.Zona.Y);
                double z = lcg.Between(row.Zona.Z, row.Zona.W);

                // TugunYarat's procedural branch, skipped when the GLB loaded.
                for (int d = 0; d < row.ProtseduralDraw; d++)
                {
                    lcg.Next();
                }

                nodes.Add(new NodeEntry(++index, row.Tur, i, x, z, firstDraw));
            }
        }

        return nodes;
    }

    /// <summary>
    /// Replays the four poplar call-site groups: 179 draws, 53 trees.
    /// Source: world.js:107-127.
    /// </summary>
    /// <param name="terakGlb">
    /// True when terak.glb is present. Two draws per tree if so, one if not — a
    /// difference worth 53 draws, which shifts everything after the poplars.
    /// </param>
    public static List<TreeEntry> Poplars(Lcg lcg, bool terakGlb, out int[] clusterSizes)
    {
        var trees = new List<TreeEntry>();

        // 1. Main row, north to south, 16 trees.
        for (int i = 0; i < 16; i++)
        {
            double x = 129.0 + (lcg.Next() * 4.0);
            trees.Add(Tree(lcg, terakGlb, x, -50.0 + (i * 6.4)));
        }

        // 2. Second row, slightly behind, 10 trees — this is what gives depth.
        for (int i = 0; i < 10; i++)
        {
            double x = 137.0 + (lcg.Next() * 3.0);
            trees.Add(Tree(lcg, terakGlb, x, -44.0 + (i * 9.2)));
        }

        // 3. Five clusters beside the channel. The size of each is DRAWN, not
        //    stored: with seed 1238 they come out 2, 4, 4, 2, 3.
        clusterSizes = new int[Clusters.Length];
        for (int c = 0; c < Clusters.Length; c++)
        {
            int n = 2 + (int)Mathf.Floor(lcg.Next() * 3.0);
            clusterSizes[c] = n;

            for (int i = 0; i < n; i++)
            {
                double x = Clusters[c].X + ((lcg.Next() - 0.5) * 7.0);
                double z = Clusters[c].Z + ((lcg.Next() - 0.5) * 7.0);
                trees.Add(Tree(lcg, terakGlb, x, z));
            }
        }

        // 4. Gate road, both verges, 6 pairs.
        for (int i = 0; i < 6; i++)
        {
            double x = 148.0 + (i * 4.5);
            trees.Add(Tree(lcg, terakGlb, x, -14.0 - (lcg.Next() * 2.0)));
            trees.Add(Tree(lcg, terakGlb, x, 14.0 + (lcg.Next() * 2.0)));
        }

        return trees;
    }

    /// <summary>
    /// Replays house placement: one yaw draw per house, 8 total.
    /// Source: world.js:247-271.
    /// </summary>
    /// <param name="uylar">
    /// The <c>?uysiz=1</c> switch (world.js:17, 255). False removes the houses AND
    /// shifts every draw after index 1267, so it is not a cosmetic toggle.
    /// </param>
    public static int HouseYaws(Lcg lcg, bool uylar)
    {
        if (!uylar)
        {
            return 0;
        }

        foreach ((string _, double _, double _) in Houses)
        {
            lcg.Next();
        }

        return Houses.Length;
    }

    /// <summary>Runs the three implemented consumers and reports the cumulative indices.</summary>
    public static DrawCounts Replay(Lcg lcg, BuxoroBalance balance, bool terakGlb, bool uylar)
    {
        Nodes(lcg, balance);
        int afterNodes = lcg.DrawCount;

        Poplars(lcg, terakGlb, out _);
        int afterPoplars = lcg.DrawCount;

        HouseYaws(lcg, uylar);
        int afterHouses = lcg.DrawCount;

        return new DrawCounts(afterNodes, afterPoplars, afterHouses);
    }

    /// <summary>TerakYarat's own draws. GLB path costs 2, procedural costs 1.</summary>
    private static TreeEntry Tree(Lcg lcg, bool terakGlb, double x, double z)
    {
        if (terakGlb)
        {
            lcg.Next(); // rotation.y = rnd() * TAU
            lcg.Next(); // scale *= 0.82 + rnd() * 0.40
        }
        else
        {
            lcg.Next(); // h = 9 + rnd() * 3.5
        }

        return new TreeEntry(x, z);
    }
}
