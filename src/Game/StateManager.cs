public enum GameState
{
    PlayerTurn,
    EnemyTurn,
    GameOver,
    Victory
}

public class StateManager
{
    private GameState _currentState;
    private GameManager _game;

    public GameState CurrentState => _currentState;

    public StateManager(GameManager game)
    {
        _game = game;
        _currentState = GameState.PlayerTurn;
    }

    public void ChangeState(GameState newState)
    {
        var oldState = _currentState;
        _currentState = newState;

        // Exit old state
        switch (oldState)
        {
            case GameState.PlayerTurn:
                // Clean up any player turn visuals/highlights
                break;
        }

        // Enter new state
        switch (_currentState)
        {
            case GameState.EnemyTurn:
                // ProcessEnemyTurn();
                break;
            case GameState.GameOver:
                // HandleGameOver();
                break;
            case GameState.Victory:
                // HandleVictory();
                break;
        }
    }

    public bool CanProcessInput()
    {
        return _currentState == GameState.PlayerTurn;
    }
}