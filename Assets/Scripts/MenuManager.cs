using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OpenPanel(GameObject panel)
    {
        Debug.Log("Кнопка натиснута! OpenPanel викликано для: " + (panel != null ? panel.name : "null"));
        if (panel != null) 
        {
            panel.SetActive(true);
            Debug.Log("Панель " + panel.name + " увімкнена (SetActive(true)). Якщо ви її не бачите, проблема у її координатах або кольорі/прозорості.");
        }
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(false);
    }

    public void SetFiniteMode(int maxWaves)
    {
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.SetInt("MaxWaves", maxWaves);
        PlayerPrefs.Save();
    }

    public void SetEndlessMode()
    {
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerPrefs.Save();
    }

    public void SetPvPHotSeatMode(int maxWaves)
    {
        PlayerPrefs.SetInt("GameMode", 2);
        PlayerPrefs.SetInt("MaxWaves", maxWaves);
        PlayerPrefs.Save();
    }
}