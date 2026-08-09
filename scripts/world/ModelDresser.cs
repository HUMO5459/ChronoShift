using Godot;

namespace ChronoShift;

/// <summary>
/// Dresses an instanced model at _Ready: removes stray lights/cameras that ship inside
/// exported model scenes and applies runtime albedo textures (FBX imports drop their textures).
/// </summary>
public partial class ModelDresser : Node3D
{
    [Export] private bool _stripLightsAndCameras = true;

    /// <summary>When set, overrides EVERY surface of every MeshInstance3D descendant with this albedo.</summary>
    [Export] private Texture2D? _albedoAll;

    /// <summary>Parallel to <see cref="_textures"/>: surfaces whose active material name matches get that texture.</summary>
    [Export] private string[] _materialNames = System.Array.Empty<string>();

    /// <summary>Parallel to <see cref="_materialNames"/>.</summary>
    [Export] private Texture2D[] _textures = System.Array.Empty<Texture2D>();

    public override void _Ready()
    {
        if (_stripLightsAndCameras)
        {
            StripLightsAndCameras(this);
        }

        DressMeshes(this);
    }

    private void StripLightsAndCameras(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Light3D || child is Camera3D)
            {
                child.QueueFree();
                continue;
            }

            StripLightsAndCameras(child);
        }
    }

    private void DressMeshes(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                DressMesh(mesh);
            }

            DressMeshes(child);
        }
    }

    private void DressMesh(MeshInstance3D mesh)
    {
        if (mesh.Mesh == null)
        {
            return;
        }

        int surfaceCount = mesh.Mesh.GetSurfaceCount();
        for (int i = 0; i < surfaceCount; i++)
        {
            if (_albedoAll != null)
            {
                mesh.SetSurfaceOverrideMaterial(i, new StandardMaterial3D { AlbedoTexture = _albedoAll });
                continue;
            }

            if (_materialNames.Length == 0)
            {
                continue;
            }

            Material? active = mesh.GetActiveMaterial(i);
            string matName = active?.ResourceName ?? "";
            for (int j = 0; j < _materialNames.Length && j < _textures.Length; j++)
            {
                if (matName == _materialNames[j] && _textures[j] != null)
                {
                    mesh.SetSurfaceOverrideMaterial(i, new StandardMaterial3D { AlbedoTexture = _textures[j] });
                    break;
                }
            }
        }
    }
}
