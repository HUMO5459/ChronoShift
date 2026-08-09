using Godot;

namespace ChronoShift;

/// <summary>The set of playable characters, shared by the hero-select menu and gameplay.</summary>
[GlobalClass]
public partial class CharacterRosterData : Resource
{
    [Export] public Godot.Collections.Array<CharacterData> Characters = new();
}
