using CommunityToolkit.Mvvm.ComponentModel;
using SdCharacterSheet.Models;

namespace SdCharacterSheet.ViewModels;

/// <summary>
/// Singleton ViewModel shared across all tabs. Phase 2 adds observable properties.
/// CharacterViewModel must NEVER be serialized directly — use CharacterSaveData DTO.
/// </summary>
public partial class CharacterViewModel : ObservableObject
{
    // Backing character — direct access for Phase 1 service integration
    public Character Character { get; private set; } = new();

    /// <summary>Replace the current character (called by file service after load/import).</summary>
    public void LoadCharacter(Character character)
    {
        Character = character;
        // Phase 2: add OnPropertyChanged calls for all bound properties
    }
}
