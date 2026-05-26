using UnityEngine;
using UnityEngine.EventSystems;

public class PvPBuyButton : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Префаб ворога, який буде додаватися або забиратися")]
    public GameObject enemyPrefab;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AttackerManager.Instance == null || enemyPrefab == null) return;

        int multiplier = 1;
        
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            multiplier = 10;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            AttackerManager.Instance.BuyEnemyAmount(enemyPrefab, multiplier);
        }
        
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            AttackerManager.Instance.RemoveEnemyAmount(enemyPrefab, multiplier);
        }
    }
}
