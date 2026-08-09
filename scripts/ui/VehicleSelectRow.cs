using System;
using Godot;

namespace ChronoShift;

/// <summary>One vehicle on the depot select panel.</summary>
public partial class VehicleSelectRow : PanelContainer
{
    public event Action<VehicleData>? Picked;

    [Export] private NodePath _kindPath = "Row/Text/Kind";
    [Export] private NodePath _namePath = "Row/Text/Name";
    [Export] private NodePath _descriptionPath = "Row/Text/Description";
    [Export] private NodePath _specsPath = "Row/Text/Specs";
    [Export] private NodePath _selectButtonPath = "Row/Select";
    [Export] private NodePath _currentLabelPath = "Row/Current";

    private VehicleData _data = null!;

    public void Bind(VehicleData data, bool isCurrent)
    {
        _data = data;

        GetNode<Label>(_kindPath).Text = data.Kind;
        GetNode<Label>(_namePath).Text = data.DisplayName;
        GetNode<Label>(_descriptionPath).Text = data.Description;

        Label specs = GetNode<Label>(_specsPath);
        specs.Text = Specs(data);
        specs.Visible = !string.IsNullOrEmpty(specs.Text);

        GetNode<Label>(_currentLabelPath).Visible = isCurrent;
        GetNode<Button>(_selectButtonPath).Visible = !isCurrent;
        GetNode<Button>(_selectButtonPath).Pressed += () => Picked?.Invoke(_data);
    }

    /// <summary>
    /// The card's one-line spec strip. Only figures the resource actually carries
    /// are listed, so an on-foot entry or a half-filled one shows nothing rather
    /// than a row of zeroes.
    /// </summary>
    private static string Specs(VehicleData data)
    {
        var parts = new System.Collections.Generic.List<string>();

        if (data.MaxHealth > 0.0f)
        {
            parts.Add($"KORPUS {Mathf.RoundToInt(data.MaxHealth)}");
        }

        if (data.MoveSpeed > 0.0f)
        {
            parts.Add($"TEZLIK {data.MoveSpeed:0.0} m/s");
        }

        if (data.Armor > 0.0f)
        {
            parts.Add($"BRONYA {data.Armor * 100.0f:0}%");
        }

        if (data.IsArmed)
        {
            parts.Add($"URON {data.ArmamentDamage:0}");
            parts.Add($"DALLIK {data.ArmamentRange:0} m");
        }

        if (data.ProductionCost > 0)
        {
            parts.Add($"NARX ₳{data.ProductionCost}");
        }

        return string.Join("   ·   ", parts);
    }
}
