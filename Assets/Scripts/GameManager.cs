using UnityEngine;
using System;

public enum GameState
{
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameState State { get; private set; }
    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ChangeState(GameState.Playing);
    }

    public void ChangeState(GameState newState)
    {
        if (State == newState) return;

        State = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
