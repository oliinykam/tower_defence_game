using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonSFX : MonoBehaviour
{
    [Header("Global Click Sound")]
    public AudioClip clickSound;

    [Header("Drag All Buttons Here")]
    public List<Button> buttonsToSound = new List<Button>();

    private void Start()
    {
        foreach (Button btn in buttonsToSound)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(PlaySound);
            }
        }
    }

    public void PlaySound()
    {
        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSound);
        }
    }
}
