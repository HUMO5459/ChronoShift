using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronoShift;

/// <summary>
/// Reads back every imported Buxoro model and asserts the step-1 acceptance
/// criteria. Run headless:
/// <code>
/// Godot --headless --path . scenes/buxoro/BuxoroImportCheck.tscn
/// </code>
/// </summary>
/// <remarks>
/// This checks the RESULT of <see cref="BuxoroImport"/>, not its source, so it
/// catches an importer that silently stopped running — the failure mode that
/// would otherwise be discovered weeks later when a character stands 0.97 m tall
/// in a finished scene. Exit code is non-zero when any check fails, so it can
/// gate a build.
/// </remarks>
public partial class BuxoroImportCheck : Node
{
    private const string ModelFolder = "res://assets/models/buxoro";
    private const string TablePath = "res://resources/data/buxoro/moljal.tres";

    /// <summary>Height tolerance in metres. The importer's own deadband is 15%.</summary>
    private const float HeightTolerance = 0.02f;

    /// <summary>A stripped clip must return to where it started, within this.</summary>
    private const float DriftTolerance = 0.03f;

    private readonly List<string> _failures = new();
    private int _checked;

    public override void _Ready()
    {
        Resource? table = ResourceLoader.Load(TablePath, cacheMode: ResourceLoader.CacheMode.Ignore);
        if (table == null)
        {
            GD.PrintErr($"[check] {TablePath} not found.");
            GetTree().Quit(1);
            return;
        }

        GD.Print("BUXORO IMPORT CHECK");
        GD.Print(new string('=', 78));
        GD.Print($"{"model",-16}{"height",9}{"target",9}{"clips",7}  {"cull",-10}notes");
        GD.Print(new string('-', 78));

        foreach (Variant entry in table.Get("Models").AsGodotArray())
        {
            var row = entry.As<Resource>();
            if (row == null)
            {
                continue;
            }

            string name = row.Get("Name").AsString();
            string path = $"{ModelFolder}/{name}.glb";
            if (!ResourceLoader.Exists(path))
            {
                continue;
            }

            CheckModel(path, name, row.Get("TargetHeight").AsSingle(), row.Get("TwoSided").AsBool());
        }

        GD.Print(new string('=', 78));
        if (_failures.Count == 0)
        {
            GD.Print($"PASS — {_checked} models, every check clean.");
            GetTree().Quit(0);
            return;
        }

        GD.PrintErr($"FAIL — {_failures.Count} problem(s) across {_checked} models:");
        foreach (string failure in _failures)
        {
            GD.PrintErr("  " + failure);
        }

        GetTree().Quit(1);
    }

    private void CheckModel(string path, string name, float target, bool twoSided)
    {
        var packed = ResourceLoader.Load<PackedScene>(path);
        if (packed == null)
        {
            _failures.Add($"{name}: failed to load.");
            return;
        }

        Node scene = packed.Instantiate();
        _checked++;

        float height = MeasuredHeight(scene);
        var notes = new List<string>();

        if (target > 0.0f && Mathf.Abs(height - target) > HeightTolerance)
        {
            _failures.Add($"{name}: height {height:F2} m, expected {target:F2} m.");
            notes.Add("HEIGHT");
        }

        string cull = CullState(scene, twoSided, name, notes);
        int clipCount = CheckClips(scene, name, notes);

        string targetText = target > 0.0f ? target.ToString("F2") : "-";
        GD.Print($"{name,-16}{height,9:F2}{targetText,9}{clipCount,7}  " +
                 $"{cull,-10}{string.Join(" ", notes)}");

        scene.QueueFree();
    }

    /// <summary>World-space height of the instantiated model, scale included.</summary>
    private static float MeasuredHeight(Node scene)
    {
        if (scene is not Node3D root)
        {
            return 0.0f;
        }

        var box = new Aabb();
        bool first = true;
        Accumulate(root, root.Transform, ref box, ref first);
        return first ? 0.0f : box.Size.Y;

        static void Accumulate(Node node, Transform3D transform, ref Aabb box, ref bool first)
        {
            foreach (Node child in node.GetChildren())
            {
                Transform3D childTransform = child is Node3D spatial
                    ? transform * spatial.Transform
                    : transform;

                if (child is MeshInstance3D mesh && mesh.Mesh != null)
                {
                    Aabb meshBox = childTransform * mesh.Mesh.GetAabb();
                    box = first ? meshBox : box.Merge(meshBox);
                    first = false;
                }

                Accumulate(child, childTransform, ref box, ref first);
            }
        }
    }

    /// <summary>Reports whether every surface matches the expected cull mode.</summary>
    private string CullState(Node scene, bool twoSided, string name, List<string> notes)
    {
        int both = 0;
        int single = 0;

        foreach (MeshInstance3D mesh in Collect<MeshInstance3D>(scene))
        {
            if (mesh.Mesh == null)
            {
                continue;
            }

            for (int surface = 0; surface < mesh.Mesh.GetSurfaceCount(); surface++)
            {
                if (mesh.Mesh.SurfaceGetMaterial(surface) is not BaseMaterial3D material)
                {
                    continue;
                }

                if (material.CullMode == BaseMaterial3D.CullModeEnum.Disabled)
                {
                    both++;
                }
                else
                {
                    single++;
                }
            }
        }

        if (twoSided && single > 0)
        {
            _failures.Add($"{name}: {single} surface(s) still single-sided.");
            notes.Add("CULL");
        }

        return twoSided ? $"both({both})" : $"back({single})";
    }

    /// <summary>Checks clip names carry no dot, loop, and no longer drift.</summary>
    private int CheckClips(Node scene, string name, List<string> notes)
    {
        AnimationPlayer? player = Collect<AnimationPlayer>(scene).FirstOrDefault();
        if (player == null)
        {
            return 0;
        }

        int count = 0;

        foreach (StringName libraryName in player.GetAnimationLibraryList())
        {
            AnimationLibrary? library = player.GetAnimationLibrary(libraryName);
            if (library == null)
            {
                continue;
            }

            foreach (StringName animationName in library.GetAnimationList())
            {
                string clip = animationName.ToString();
                count++;

                if (clip.Contains('.'))
                {
                    _failures.Add($"{name}/{clip}: clip name still contains a dot.");
                    notes.Add("NAME");
                }

                Animation? animation = library.GetAnimation(clip);
                if (animation == null)
                {
                    continue;
                }

                if (animation.LoopMode == Animation.LoopModeEnum.None)
                {
                    notes.Add($"oneshot:{clip}");
                }

                float drift = MaxDrift(animation);
                if (drift > DriftTolerance)
                {
                    _failures.Add($"{name}/{clip}: still drifts {drift:F3} units.");
                    notes.Add("DRIFT");
                }
            }
        }

        return count;
    }

    /// <summary>Largest first-to-last displacement left on any position track.</summary>
    private static float MaxDrift(Animation animation)
    {
        float largest = 0.0f;

        for (int track = 0; track < animation.GetTrackCount(); track++)
        {
            if (animation.TrackGetType(track) != Animation.TrackType.Position3D)
            {
                continue;
            }

            int keys = animation.TrackGetKeyCount(track);
            if (keys < 2)
            {
                continue;
            }

            Vector3 first = animation.TrackGetKeyValue(track, 0).AsVector3();
            Vector3 last = animation.TrackGetKeyValue(track, keys - 1).AsVector3();
            float drift = (last - first).Length();

            if (drift > largest)
            {
                largest = drift;
            }
        }

        return largest;
    }

    private static List<T> Collect<T>(Node node) where T : Node
    {
        var found = new List<T>();
        Walk(node, found);
        return found;

        static void Walk(Node current, List<T> into)
        {
            foreach (Node child in current.GetChildren())
            {
                if (child is T match)
                {
                    into.Add(match);
                }

                Walk(child, into);
            }
        }
    }
}
