using System.Globalization;
using System.Text;
using Godot;

namespace ChronoShift;

/// <summary>
/// Writes the determinism fingerprint of the Godot port so it can be diffed
/// against the web original. Run headless:
/// <code>
/// Godot --headless --path . scenes/buxoro/ParityDump.tscn
/// </code>
/// The output lands in user://parity_godot.csv; <c>tools/parity/dump_web.js</c>
/// produces a byte-identical file from the unmodified web build.
/// </summary>
/// <remarks>
/// THIS IS THE GATE FOR EVERYTHING DOWNSTREAM. If the generator diverges, the 66
/// resource nodes move, the economy changes, and every number in balance.js was
/// tuned against a field that no longer exists. Nothing built on top of a failing
/// parity run is worth building.
///
/// Four blocks, in order of how early they fail:
///   A RAW        the first 200 raw generator values. If this differs, stop.
///   B NODES      all 66 nodes with the draw index each started at. The draw index
///                is what catches a wrong placeholder box count: coordinates alone
///                can coincide by luck, the index cannot.
///   C DRAWCOUNTS cumulative draw index after each consumer, plus the five cluster
///                sizes, which are drawn rather than stored.
///   D TERRAIN    ground height at six fixed probes.
///
/// The gate and edge-wall consumers are not replayed yet — they arrive with
/// World.cs in step 3, and block C carries only the three implemented counts
/// until then.
/// </remarks>
public partial class ParityDump : Node
{
    private const string OutputPath = "user://parity_godot.csv";
    private const string BalancePath = "res://resources/data/buxoro/balance_buxoro.tres";
    private const string ModelFolder = "res://assets/models/buxoro";

    /// <summary>Raw draws in block A. 200 is far past the point any error shows.</summary>
    private const int RawDraws = 200;

    /// <summary>Terrain probes. Spread across the field, including both edges.</summary>
    private static readonly (double X, double Z)[] TerrainProbes =
    {
        (96, 6), (108, 1), (152, -22), (148, 20), (60, 0), (172, 0),
    };

    public override void _Ready()
    {
        var balance = ResourceLoader.Load(BalancePath, cacheMode: ResourceLoader.CacheMode.Ignore)
            as BuxoroBalance;

        if (balance == null)
        {
            GD.PrintErr($"[parity] {BalancePath} did not load as BuxoroBalance.");
            GetTree().Quit(1);
            return;
        }

        if (!balance.Tekshir())
        {
            GD.PrintErr("[parity] balance failed its own checks; refusing to dump.");
            GetTree().Quit(1);
            return;
        }

        // The stream is conditioned on which GLBs exist, so report what we assumed.
        bool terakGlb = ResourceLoader.Exists($"{ModelFolder}/terak.glb");
        var text = new StringBuilder();

        text.Append("# BUXORO PARITY DUMP\n");
        text.Append($"# terak_glb,{(terakGlb ? 1 : 0)}\n");
        foreach (BuxoroNodeCountRow row in balance.TugunSoni)
        {
            bool present = ResourceLoader.Exists($"{ModelFolder}/{row.GlbNomi}.glb");
            text.Append($"# glb,{row.GlbNomi},{(present ? 1 : 0)},{row.ProtseduralDraw}\n");
        }

        WriteRaw(text);
        WriteNodes(text, balance);
        WriteDrawCounts(text, balance, terakGlb);
        WriteTerrain(text);

        // Fully qualified: ImplicitUsings pulls in System.IO, which has its own FileAccess.
        using Godot.FileAccess? file =
            Godot.FileAccess.Open(OutputPath, Godot.FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr($"[parity] cannot write {OutputPath}.");
            GetTree().Quit(1);
            return;
        }

        file.StoreString(text.ToString());
        GD.Print($"[parity] wrote {ProjectSettings.GlobalizePath(OutputPath)}");
        GD.Print(text.ToString());
        GetTree().Quit(0);
    }

    /// <summary>Block A — raw generator values, full precision.</summary>
    private static void WriteRaw(StringBuilder text)
    {
        text.Append("BLOCK,A,RAW\n");
        var lcg = new Lcg(Lcg.WorldSeed);

        for (int i = 0; i < RawDraws; i++)
        {
            text.Append(i.ToString(CultureInfo.InvariantCulture));
            text.Append(',');
            text.Append(lcg.Next().ToString("R", CultureInfo.InvariantCulture));
            text.Append('\n');
        }
    }

    /// <summary>Block B — the 66 resource nodes and the draw index each began at.</summary>
    private static void WriteNodes(StringBuilder text, BuxoroBalance balance)
    {
        text.Append("BLOCK,B,NODES\n");
        var lcg = new Lcg(Lcg.WorldSeed);

        foreach (BuxoroWorldStream.NodeEntry node in BuxoroWorldStream.Nodes(lcg, balance))
        {
            text.Append(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3:F6},{4:F6},{5}\n",
                node.Index, node.Tur, node.TypeIndex, node.X, node.Z, node.FirstDraw));
        }
    }

    /// <summary>Block C — cumulative draw index per consumer, and the cluster sizes.</summary>
    private static void WriteDrawCounts(StringBuilder text, BuxoroBalance balance, bool terakGlb)
    {
        text.Append("BLOCK,C,DRAWCOUNTS\n");

        var lcg = new Lcg(Lcg.WorldSeed);
        BuxoroWorldStream.Nodes(lcg, balance);
        text.Append($"nodes,{lcg.DrawCount}\n");

        BuxoroWorldStream.Poplars(lcg, terakGlb, out int[] clusters);
        text.Append($"poplars,{lcg.DrawCount}\n");

        BuxoroWorldStream.HouseYaws(lcg, uylar: true);
        text.Append($"houses,{lcg.DrawCount}\n");

        text.Append("clusters,");
        text.Append(string.Join(",", clusters));
        text.Append('\n');
    }

    /// <summary>Block D — ground height probes.</summary>
    private static void WriteTerrain(StringBuilder text)
    {
        text.Append("BLOCK,D,TERRAIN\n");

        foreach ((double x, double z) in TerrainProbes)
        {
            text.Append(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2:F6}\n", x, z, BuxoroTerrain.HeightAt(x, z)));
        }
    }
}
