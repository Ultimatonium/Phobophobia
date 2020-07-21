using Unity.Entities;

[GenerateAuthoringComponent]
public struct GameStateData : IComponentData
{
    public GameState gameState;
    public GameState prevGameState;
}
