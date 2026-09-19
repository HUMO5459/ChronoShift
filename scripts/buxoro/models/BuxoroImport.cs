using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace ChronoShift;

/// <summary>
/// Post-import pass for the Buxoro 1238 models. Assign this script in the Import
/// dock of each res://assets/models/buxoro/*.glb.
/// </summary>
/// <remarks>
/// A literal port of three functions the web build runs at load time, in the same
/// order, because the third depends on the first:
///
///   1. tayyorla        (models.js:210-286) — materials, uniform scale, foot drop
///   2. klipNomlariniQoy(models.js:46-55)   — exporter clip names to game names
///   3. ildizSiljishi   (models.js:78-137)  — strip baked root motion
///
/// Doing it at import rather than at runtime is the one deliberate structural
/// difference: the result is identical and it costs nothing per instance.
///
/// Why root motion has to go. The exporters bake step displacement into the hip
/// bone, so a walk cycle drags the body forward and snaps back at the loop point.
/// The game drives position in code, so the baked motion produces three visible
/// defects: the body slides then jumps back, turning sweeps an arc because the
/// body sits far from its pivot, and the feet skate because clip speed is
/// unrelated to travel speed. Removing only the LINEAR component keeps the sway,
/// closes the loop, and yields the clip's natural speed in metres per second,
/// which the runtime uses to match step frequency to actual movement.
///
/// MEASURED GOTCHA (Godot 4.7.1, 2026-09-15). During an import pass, a .tres whose
/// script is a C# class loads as a plain Godot.Resource — the C# wrapper is never
/// constructed. This is not specific to the Buxoro resources: the project's own
/// character_roster.tres behaves identically here. So this script reads its two
/// tables through Get() rather than through BuxoroModelTable / BuxoroClipMap,
/// which stay the authoring schema and work normally at runtime.
/// </remarks>
[Tool]
public partial class BuxoroImport : EditorScenePostImport
{
    private const string TablePath = "res://resources/data/buxoro/moljal.tres";
    private const string ClipMapPath = "res://resources/data/buxoro/klip_xaritasi.tres";

    /// <summary>Rescale only when the model is more than 15% off target (models.js:236).</summary>
    private const float ScaleDeadband = 0.15f;

    /// <summary>Below this, a displacement is export noise, not motion (models.js:76).</summary>
    private const float DriftEpsilon = 0.02f;

    /// <summary>Clips whose displacement is deliberate and must survive (models.js:75).</summary>
    private static readonly Regex KeepDrift =
        new("(die|death|olim|yiqil|fall|land|takeoff)", RegexOptions.IgnoreCase);

    /// <summary>
    /// Skinned meshes are culled against a bind-pose box that an animation can
    /// leave, which makes limbs blink out at the screen edge. The web build set
    /// frustumCulled = false (models.js:216); Godot has no such switch, so the
    /// box is padded instead.
    /// </summary>
    private const float SkinnedCullMargin = 4.0f;

    private string _modelName = "";

    /// <summary>One row of moljal.tres, read without the C# wrapper.</summary>
    private readonly record struct ModelRow(float TargetHeight, bool TwoSided);

    /// <summary>One row of klip_xaritasi.tres, read without the C# wrapper.</summary>
    private readonly record struct ClipRow(string GameClip, bool Loop);

    public override GodotObject _PostImport(Node scene)
    {
        _modelName = GetSourceFile().GetFile().GetBaseName();

        Resource? table = LoadFresh(TablePath);
        Resource? clipMap = LoadFresh(ClipMapPath);

        if (table == null)
        {
            GD.PushWarning($"[buxoro] {_modelName}: {TablePath} missing, import left untouched.");
            return scene;
        }

        ModelRow? row = FindModel(table, _modelName);
        if (row == null)
        {
            GD.PushWarning($"[buxoro] {_modelName}: no row in moljal.tres, import left untouched.");
            return scene;
        }

        PrepareMaterials(scene, row.Value.TwoSided);

        float scale = 1.0f;
        if (scene is Node3D root)
        {
            scale = ApplyTargetHeight(root, row.Value.TargetHeight);
            DropToGround(root);
        }

        AnimationPlayer? player = FindNode<AnimationPlayer>(scene);
        if (player != null)
        {
            RenameClips(player, clipMap);
            StripRootMotion(player, FindNode<Skeleton3D>(scene), scale);
        }

        return scene;
    }

    // ------------------------------------------------------------------- tables

    /// <summary>
    /// Loads bypassing the resource cache. During an import pass the cache can
    /// already hold a half-initialised copy, and reusing it silently yields empty
    /// tables.
    /// </summary>
    private static Resource? LoadFresh(string path)
    {
        return ResourceLoader.Exists(path)
            ? ResourceLoader.Load(path, cacheMode: ResourceLoader.CacheMode.Ignore)
            : null;
    }

    private static ModelRow? FindModel(Resource table, string name)
    {
        foreach (Variant entry in table.Get("Models").AsGodotArray())
        {
            var row = entry.As<Resource>();
            if (row == null || row.Get("Name").AsString() != name)
            {
                continue;
            }

            return new ModelRow(row.Get("TargetHeight").AsSingle(), row.Get("TwoSided").AsBool());
        }

        return null;
    }

    private static ClipRow? FindClip(Resource? clipMap, string model, string glbClip)
    {
        if (clipMap == null)
        {
            return null;
        }

        foreach (Variant entry in clipMap.Get("Clips").AsGodotArray())
        {
            var row = entry.As<Resource>();
            if (row == null ||
                row.Get("Model").AsString() != model ||
                row.Get("GlbClip").AsString() != glbClip)
            {
                continue;
            }

            return new ClipRow(row.Get("GameClip").AsString(), row.Get("Loop").AsBool());
        }

        return null;
    }

    // ---------------------------------------------------------------- materials

    /// <summary>Shadows on every surface, and both faces drawn for the hollow models.</summary>
    private static void PrepareMaterials(Node node, bool twoSided)
    {
        foreach (MeshInstance3D mesh in CollectNodes<MeshInstance3D>(node))
        {
            mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;

            if (mesh.Skin != null)
            {
                mesh.ExtraCullMargin = SkinnedCullMargin;
            }

            Mesh? geometry = mesh.Mesh;
            if (geometry == null || !twoSided)
            {
                continue;
            }

            for (int surface = 0; surface < geometry.GetSurfaceCount(); surface++)
            {
                if (geometry.SurfaceGetMaterial(surface) is BaseMaterial3D material)
                {
                    material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
                }
            }
        }
    }

    // ------------------------------------------------------------------- scale

    /// <summary>
    /// Scales the model so its bounding box stands <paramref name="targetHeight"/>
    /// tall, and returns the factor applied. Mirrors models.js:226-240 including
    /// the 15% deadband, which exists so a model authored at the right size is not
    /// nudged by float noise.
    /// </summary>
    private float ApplyTargetHeight(Node3D root, float targetHeight)
    {
        if (targetHeight <= 0.0f)
        {
            return 1.0f;
        }

        Aabb box = LocalAabb(root);
        if (box.Size.Y <= 0.001f)
        {
            return 1.0f;
        }

        float factor = targetHeight / box.Size.Y;
        if (Mathf.Abs(factor - 1.0f) <= ScaleDeadband)
        {
            return 1.0f;
        }

        root.Scale *= factor;
        GD.Print($"[buxoro] {_modelName}: {box.Size.Y:F2} m -> {targetHeight:F2} m (x{factor:F2})");
        return factor;
    }

    /// <summary>
    /// Sinks or lifts the model so its lowest point sits at y = 0, so a model whose
    /// origin is at its centre does not stand buried. Mirrors models.js:242-244.
    /// </summary>
    private static void DropToGround(Node3D root)
    {
        Aabb box = LocalAabb(root);
        float bottom = box.Position.Y * root.Scale.Y;

        if (Mathf.Abs(bottom) > DriftEpsilon)
        {
            root.Position = new Vector3(root.Position.X, root.Position.Y - bottom, root.Position.Z);
        }
    }

    /// <summary>Combined mesh bounds in the root's own space, ignoring the root's scale.</summary>
    private static Aabb LocalAabb(Node3D root)
    {
        var box = new Aabb();
        bool first = true;
        Accumulate(root, Transform3D.Identity, ref box, ref first);
        return box;

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

    // ------------------------------------------------------------------- clips

    /// <summary>
    /// Applies the clip-name map and the loop flag. An unmapped clip keeps its
    /// imported name, matching models.js:48 ("xaritada bo'lmagan klip o'z nomida
    /// qoladi") so a missing entry degrades to procedural motion rather than a
    /// silent wrong animation.
    /// </summary>
    private void RenameClips(AnimationPlayer player, Resource? clipMap)
    {
        foreach (StringName libraryName in player.GetAnimationLibraryList())
        {
            AnimationLibrary? library = player.GetAnimationLibrary(libraryName);
            if (library == null)
            {
                continue;
            }

            // Collect first: renaming inside the enumeration invalidates it.
            var names = new List<string>();
            foreach (StringName animationName in library.GetAnimationList())
            {
                names.Add(animationName.ToString());
            }

            int renamed = 0;
            foreach (string name in names)
            {
                ClipRow? row = FindClip(clipMap, _modelName, name);

                Animation? animation = library.GetAnimation(name);
                if (animation != null)
                {
                    animation.LoopMode = row == null || row.Value.Loop
                        ? Animation.LoopModeEnum.Linear
                        : Animation.LoopModeEnum.None;
                }

                if (row == null || string.IsNullOrEmpty(row.Value.GameClip) || row.Value.GameClip == name)
                {
                    continue;
                }

                if (library.HasAnimation(row.Value.GameClip))
                {
                    GD.PushWarning(
                        $"[buxoro] {_modelName}: cannot rename '{name}' to '{row.Value.GameClip}', " +
                        "that name is already taken.");
                    continue;
                }

                library.RenameAnimation(name, row.Value.GameClip);
                renamed++;
            }

            if (renamed > 0)
            {
                GD.Print($"[buxoro] {_modelName}: {renamed} clip(s) renamed.");
            }
        }
    }

    // ------------------------------------------------------------- root motion

    /// <summary>
    /// Removes the linear component of every position track, recentres the moved
    /// axes on the bone's rest pose, and records the clip's natural speed as
    /// metadata. Port of models.js:78-137.
    /// </summary>
    private void StripRootMotion(AnimationPlayer player, Skeleton3D? skeleton, float scale)
    {
        if (scale <= 0.0f)
        {
            scale = 1.0f;
        }

        foreach (StringName libraryName in player.GetAnimationLibraryList())
        {
            AnimationLibrary? library = player.GetAnimationLibrary(libraryName);
            if (library == null)
            {
                continue;
            }

            foreach (StringName animationName in library.GetAnimationList())
            {
                string name = animationName.ToString();
                if (KeepDrift.IsMatch(name))
                {
                    continue;
                }

                Animation? animation = library.GetAnimation(name);
                if (animation == null)
                {
                    continue;
                }

                float largest = StripAnimation(animation, skeleton);
                float duration = (float)animation.Length;

                if (largest > DriftEpsilon && duration > 0.05f)
                {
                    float metres = largest * scale;
                    float speed = metres / duration;
                    animation.SetMeta("buxoro_tabiiy_tezlik", speed);
                    GD.Print(
                        $"[buxoro] {_modelName}/{name}: root motion removed — " +
                        $"{metres:F2} m / {duration:F2} s = {speed:F2} m/s");
                }
            }
        }
    }

    /// <summary>Returns the largest displacement removed from any track, in model units.</summary>
    private static float StripAnimation(Animation animation, Skeleton3D? skeleton)
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

            var times = new float[keys];
            var values = new Vector3[keys];
            for (int k = 0; k < keys; k++)
            {
                times[k] = (float)animation.TrackGetKeyTime(track, k);
                values[k] = animation.TrackGetKeyValue(track, k).AsVector3();
            }

            Vector3 drift = values[keys - 1] - values[0];
            float length = drift.Length();
            if (length < DriftEpsilon)
            {
                continue;
            }

            // 1. Subtract the linear ramp so the last key lands back on the first.
            float start = times[0];
            float span = times[keys - 1] - start;
            if (Mathf.IsZeroApprox(span))
            {
                span = 1.0f;
            }

            for (int k = 0; k < keys; k++)
            {
                float f = (times[k] - start) / span;
                values[k] -= drift * f;
            }

            // 2. Recentre the moved axes on the bone's rest pose. A run clip starts
            //    mid-stride, so removing the ramp alone still leaves the body parked
            //    forward of its pivot — and turning would sweep an arc again.
            Vector3 rest = RestPosition(skeleton, animation.TrackGetPath(track));
            for (int axis = 0; axis < 3; axis++)
            {
                if (Mathf.Abs(drift[axis]) < DriftEpsilon)
                {
                    continue;
                }

                float mean = 0.0f;
                for (int k = 0; k < keys; k++)
                {
                    mean += values[k][axis];
                }

                float shift = rest[axis] - (mean / keys);
                for (int k = 0; k < keys; k++)
                {
                    Vector3 v = values[k];
                    v[axis] += shift;
                    values[k] = v;
                }
            }

            for (int k = 0; k < keys; k++)
            {
                animation.TrackSetKeyValue(track, k, values[k]);
            }

            if (length > largest)
            {
                largest = length;
            }
        }

        return largest;
    }

    /// <summary>
    /// Rest position of the bone a track drives. Without a skeleton, or for a bone
    /// the skeleton does not know, recentring falls back to the origin — the same
    /// outcome as the web build, which skipped the shift when it could not resolve
    /// the bone (models.js:117).
    /// </summary>
    private static Vector3 RestPosition(Skeleton3D? skeleton, NodePath path)
    {
        if (skeleton == null || path.GetSubNameCount() == 0)
        {
            return Vector3.Zero;
        }

        string boneName = path.GetSubName(path.GetSubNameCount() - 1);
        int bone = skeleton.FindBone(boneName);
        return bone < 0 ? Vector3.Zero : skeleton.GetBoneRest(bone).Origin;
    }

    // ------------------------------------------------------------------ helpers

    private static T? FindNode<T>(Node node) where T : Node
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T match)
            {
                return match;
            }

            T? nested = FindNode<T>(child);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static List<T> CollectNodes<T>(Node node) where T : Node
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
