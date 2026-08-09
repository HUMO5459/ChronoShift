using Godot;

namespace ChronoShift;

/// <summary>Spins a rotor blade around a local axis, but only while the aircraft is flown.</summary>
public partial class Rotor : Node3D
{
    /// <summary>Local axis the blades turn around (main rotor: up; tail rotor: sideways).</summary>
    [Export] private Vector3 _axis = Vector3.Up;

    /// <summary>Revolutions per second at full spin.</summary>
    [Export] private float _speed = 4.0f;

    /// <summary>Seconds to spin up / wind down when the aircraft is entered or left.</summary>
    [Export] private float _spinResponse = 1.5f;

    /// <summary>
    /// Hide this node unless the aircraft is flown. Used for the fake "spinning blur"
    /// prop that stands in for a static prop baked into the plane's body mesh.
    /// </summary>
    [Export] private bool _hideWhenParked;

    private Vector3 _unitAxis;
    private Aircraft? _aircraft;
    private float _current;

    public override void _Ready()
    {
        _unitAxis = _axis.LengthSquared() > 0.0001f ? _axis.Normalized() : Vector3.Up;
        _aircraft = FindAircraft();
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        bool flown = _aircraft is { IsFlown: true };

        if (_hideWhenParked)
        {
            Visible = flown;
        }

        // Wind up to full speed while flown, wind back down to a stop when parked.
        float target = flown ? _speed : 0.0f;
        _current = Mathf.MoveToward(_current, target, _spinResponse * _speed * dt);

        if (_current > 0.0001f)
        {
            RotateObjectLocal(_unitAxis, _current * Mathf.Tau * dt);
        }
    }

    private Aircraft? FindAircraft()
    {
        Node? node = GetParent();
        while (node != null)
        {
            if (node is Aircraft aircraft)
            {
                return aircraft;
            }

            node = node.GetParent();
        }

        return null;
    }
}
