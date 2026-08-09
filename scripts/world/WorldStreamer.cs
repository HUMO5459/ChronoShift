using Godot;
using System.Collections.Generic;

namespace ChronoShift;

/// <summary>Streams flat ground chunks in a square radius around the "player" group node,
/// spawning tiles that enter range and freeing those that leave, so the world feels unbounded
/// while only (2*radius+1)^2 chunks stay active.</summary>
public partial class WorldStreamer : Node3D
{
    [Export] private PackedScene _chunkScene = null!;
    [Export] private float _chunkSize = 60.0f;
    [Export] private int _radius = 3;
    [Export] private Color _tintA = new Color(0.42f, 0.45f, 0.40f);
    [Export] private Color _tintB = new Color(0.38f, 0.41f, 0.36f);

    private Node3D _player = null!;
    private readonly Dictionary<Vector2I, Node3D> _active = new();
    private StandardMaterial3D _matA = null!;
    private StandardMaterial3D _matB = null!;
    private Vector2I _lastCoord = new Vector2I(int.MinValue, int.MinValue);

    public override void _Ready()
    {
        _matA = new StandardMaterial3D { AlbedoColor = _tintA };
        _matB = new StandardMaterial3D { AlbedoColor = _tintB };

        var p = GetTree().GetFirstNodeInGroup("player");
        if (p is Node3D node)
        {
            _player = node;
            UpdateChunks(WorldToChunk(_player.GlobalPosition));
        }
        else
        {
            GD.PushWarning("WorldStreamer: no node in group 'player'; streaming disabled.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_player == null)
        {
            return;
        }

        Vector2I coord = WorldToChunk(_player.GlobalPosition);
        if (coord != _lastCoord)
        {
            UpdateChunks(coord);
        }
    }

    private Vector2I WorldToChunk(Vector3 pos) =>
        new Vector2I(Mathf.RoundToInt(pos.X / _chunkSize), Mathf.RoundToInt(pos.Z / _chunkSize));

    private void UpdateChunks(Vector2I center)
    {
        _lastCoord = center;

        var desired = new HashSet<Vector2I>();
        for (int dx = -_radius; dx <= _radius; dx++)
        {
            for (int dz = -_radius; dz <= _radius; dz++)
            {
                desired.Add(new Vector2I(center.X + dx, center.Y + dz));
            }
        }

        foreach (Vector2I c in desired)
        {
            if (_active.ContainsKey(c))
            {
                continue;
            }

            var chunk = _chunkScene.Instantiate<Node3D>();
            chunk.Position = new Vector3(c.X * _chunkSize, 0f, c.Y * _chunkSize);
            AddChild(chunk);

            var mesh = chunk.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
            if (mesh != null)
            {
                mesh.SetSurfaceOverrideMaterial(0, ((c.X + c.Y) & 1) == 0 ? _matA : _matB);
            }

            _active[c] = chunk;
        }

        // Collect stale keys first to avoid modifying the dictionary during iteration.
        var stale = new List<Vector2I>();
        foreach (var kv in _active)
        {
            if (!desired.Contains(kv.Key))
            {
                stale.Add(kv.Key);
            }
        }

        foreach (var key in stale)
        {
            _active[key].QueueFree();
            _active.Remove(key);
        }
    }
}
