using System;
using System.Text;
using Godot;

namespace ChronoShift;

/// <summary>One production order on the workshop's board.</summary>
public partial class RecipeRow : PanelContainer
{
    /// <summary>C# event rather than a Godot signal: RecipeData is passed straight through.</summary>
    public event Action<RecipeData>? ProducePressed;

    [Export] private NodePath _namePath = "Row/Text/Name";
    [Export] private NodePath _outputPath = "Row/Text/Output";
    [Export] private NodePath _descriptionPath = "Row/Text/Description";
    [Export] private NodePath _costPath = "Row/Cost";
    [Export] private NodePath _producePath = "Row/Produce";

    private RecipeData _recipe = null!;
    private Button _produce = null!;
    private Label _cost = null!;

    public void Bind(RecipeData recipe)
    {
        _recipe = recipe;
        _produce = GetNode<Button>(_producePath);
        _cost = GetNode<Label>(_costPath);

        GetNode<Label>(_namePath).Text = recipe.DisplayName;
        GetNode<Label>(_descriptionPath).Text = recipe.Description;

        string output = recipe.Output != null
            ? $"→ {recipe.Output.DisplayName} ×{recipe.OutputAmount}"
            : "→ —";
        GetNode<Label>(_outputPath).Text = output;

        _produce.Pressed += () => ProducePressed?.Invoke(_recipe);
        Refresh(busy: false);
    }

    /// <summary>Repaints the cost list and the button against what the player is holding.</summary>
    public void Refresh(bool busy)
    {
        InventoryManager? inventory = InventoryManager.Instance;
        bool affordable = inventory != null && inventory.CanCraft(_recipe);

        var text = new StringBuilder();
        for (int i = 0; i < _recipe.Inputs.Length; i++)
        {
            ItemData? input = _recipe.Inputs[i];
            if (input == null)
            {
                continue;
            }

            int need = _recipe.AmountFor(i);
            int held = inventory?.Count(input) ?? 0;
            text.Append($"{input.DisplayName} {held}/{need}\n");
        }

        _cost.Text = text.ToString().TrimEnd();
        _cost.AddThemeColorOverride("font_color", affordable ? UiPalette.PaperInk : UiPalette.StampRed);

        _produce.Disabled = busy || !affordable;
        _produce.Text = busy ? "SEX BAND" : affordable ? "ISHLAB CHIQARISH" : "MATERIAL YETMAYDI";
    }
}
