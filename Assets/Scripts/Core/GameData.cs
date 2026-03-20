/// <summary>
/// Static data that persists between scenes.
/// Set before loading the game scene, read by SceneSetup.
/// </summary>
public static class GameData
{
    public static int SelectedCharacterIndex = 0;

    public static CharacterDefinition SelectedCharacter =>
        CharacterDefinition.All[SelectedCharacterIndex];
}
