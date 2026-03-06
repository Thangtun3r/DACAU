using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ── State ────────────────────────────────────────────────────────────────
    public enum GameState { Idle, Playing, Paused }

    public static GameState CurrentState { get; private set; } = GameState.Idle;

    // ── Events ───────────────────────────────────────────────────────────────
    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action OnGameStop;

    // ── Public helpers ───────────────────────────────────────────────────────
    public static bool IsPlaying => CurrentState == GameState.Playing;

    // ── Input (placeholder – wire up your own UI buttons as needed) ──────────
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))   StartGame();
        if (Input.GetKeyDown(KeyCode.P))        TogglePause();
        if (Input.GetKeyDown(KeyCode.Escape))   StopGame();
    }

    // ── State transitions ────────────────────────────────────────────────────
    public void StartGame()
    {
        if (CurrentState == GameState.Playing) return;

        SetCursorLocked(true);
        CurrentState = GameState.Playing;
        OnGameStart?.Invoke();
        Debug.Log("[GameManager] Game started.");
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            SetCursorLocked(false);
            CurrentState = GameState.Paused;
            OnGamePause?.Invoke();
            Debug.Log("[GameManager] Game paused.");
        }
        else if (CurrentState == GameState.Paused)
        {
            SetCursorLocked(true);
            CurrentState = GameState.Playing;
            OnGameResume?.Invoke();
            Debug.Log("[GameManager] Game resumed.");
        }
    }

    public void StopGame()
    {
        if (CurrentState == GameState.Idle) return;

        SetCursorLocked(false);
        CurrentState = GameState.Idle;
        OnGameStop?.Invoke();
        Debug.Log("[GameManager] Game stopped.");
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.visible   = !locked;
        Cursor.lockState = CursorLockMode.None;
    }
}
