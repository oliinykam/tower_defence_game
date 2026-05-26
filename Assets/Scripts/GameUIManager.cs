using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("Speed Up Settings")]
    public Image speedBtnImage;
    public Sprite speed1xSprite;
    public Sprite speed2xSprite;
    public Sprite speed4xSprite;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    
    [Header("End Game Menu")]
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameTitle;

    public static GameUIManager Instance;

    private float savedTimeScale = 1f; 

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Paused)
        {
            if (Time.timeScale > 0f) savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }
        else if (state == GameState.Playing)
        {
            Time.timeScale = savedTimeScale > 0f ? savedTimeScale : 1f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            UpdateSpeedUI();
        }
        else if (state == GameState.GameOver)
        {
            Time.timeScale = 0f;
            if (endGamePanel != null) endGamePanel.SetActive(true);
        }
    }

    void Start()
    {
        if (Camera.main != null)
        {
            Camera.main.transparencySortMode = TransparencySortMode.CustomAxis;
            Camera.main.transparencySortAxis = new Vector3(0, 1, 0);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (endGamePanel != null)
        {
            endGamePanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
        UpdateSpeedUI();
    }

    public void ToggleSpeedUp()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        if (Time.timeScale == 1f)
        {
            Time.timeScale = 2f;
        }
        else if (Time.timeScale == 2f)
        {
            Time.timeScale = 4f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        UpdateSpeedUI();
    }

    private void UpdateSpeedUI()
    {
        if (speedBtnImage == null) return;

        if (Time.timeScale == 1f)
        {
            if (speed1xSprite != null) speedBtnImage.sprite = speed1xSprite;
        }
        else if (Time.timeScale == 2f)
        {
            if (speed2xSprite != null) speedBtnImage.sprite = speed2xSprite;
        }
        else if (Time.timeScale >= 4f)
        {
            if (speed4xSprite != null) speedBtnImage.sprite = speed4xSprite;
            else if (speed2xSprite != null) speedBtnImage.sprite = speed2xSprite;
        }
    }

    public void SetVolume(float volume)
    {
        if (volume > 1f)
        {
            volume = volume / 100f;
        }
        
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    public void TogglePause()
    {
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.State == GameState.Paused)
        {
            ContinueGame();
        }
        else if (GameManager.Instance.State == GameState.Playing)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Paused);
    }

    public void ShowEndGame(bool isVictory)
    {
        if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.GameOver);

        if (endGameTitle != null)
        {
            endGameTitle.text = isVictory ? "VICTORY!" : "GAME OVER!";
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); 
    }

    public void ContinueGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        SceneManager.LoadScene(0);
        
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
