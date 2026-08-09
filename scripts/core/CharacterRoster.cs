namespace ChronoShift;

/// <summary>
/// Remembers which character the player picked. A plain static so the choice
/// survives the menu-to-gameplay scene change without needing an autoload.
/// </summary>
public static class CharacterRoster
{
    /// <summary>Empty means "use the roster's first entry".</summary>
    public static string SelectedId { get; set; } = "";

    /// <summary>Resolves the selection against a roster, falling back to the first character.</summary>
    public static CharacterData? Resolve(CharacterRosterData? roster)
    {
        if (roster == null || roster.Characters.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(SelectedId))
        {
            foreach (CharacterData character in roster.Characters)
            {
                if (character != null && character.CharacterId == SelectedId)
                {
                    return character;
                }
            }
        }

        return roster.Characters[0];
    }
}
