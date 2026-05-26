using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    public int health = 100;
    public TextMeshProUGUI healthTxt;

    [Header("Audio")]
    public AudioClip baseDamageSFX;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHealth(int changeAmount)
    {
        if (changeAmount < 0 && baseDamageSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(baseDamageSFX);
        }

        health += changeAmount;

        healthTxt.text = health.ToString();

        if (health <= 0)
        {
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowEndGame(false);
            }
        }
    }
}