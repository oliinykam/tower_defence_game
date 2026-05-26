using UnityEngine;
using TMPro;

public class AttackerManager : MonoBehaviour
{
    public static AttackerManager Instance;

    [System.Serializable]
    public class EnemyTextMapping
    {
        public string enemyName; 
        public TextMeshProUGUI countText;
    }

    [Header("UI Elements")]
    public GameObject attackerUIPanel;
    public TextMeshProUGUI budgetText;
    
    [Header("Enemy Counters (For specific mobs)")]
    public EnemyTextMapping[] enemyCountTexts;

    [Header("Defender UI Elements (Hidden during attack planning)")]
    public GameObject[] defenderUIElements;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (WaveManager.Instance == null || WaveManager.Instance.gameMode != GameWaveMode.PvPHotSeat)
        {
            if (attackerUIPanel != null)
            {
                attackerUIPanel.SetActive(false);
            }
        }
    }

    public void OpenAttackerUI(int budget)
    {
        if (attackerUIPanel != null)
        {
            attackerUIPanel.SetActive(true);
        }

        if (defenderUIElements != null)
        {
            foreach (GameObject ui in defenderUIElements)
            {
                if (ui != null) ui.SetActive(false);
            }
        }

        UpdateUI();
    }

    public void CloseAttackerUI()
    {
        if (attackerUIPanel != null)
        {
            attackerUIPanel.SetActive(false);
        }
    }

    public void AttackerReady()
    {
        CloseAttackerUI();

        if (defenderUIElements != null)
        {
            foreach (GameObject ui in defenderUIElements)
            {
                if (ui != null) ui.SetActive(true);
            }
        }
    }

    public void BuyEnemy(GameObject enemyPrefab)
    {
        BuyEnemyAmount(enemyPrefab, 1);
    }

    public void BuyEnemyAmount(GameObject enemyPrefab, int count)
    {
        if (WaveManager.Instance == null) return;

        Enemy enemyScript = enemyPrefab.GetComponent<Enemy>();
        if (enemyScript == null || enemyScript.enemyData == null) return;

        int cost = enemyScript.enemyData.attackCost;
        bool updated = false;

        for (int i = 0; i < count; i++)
        {
            if (WaveManager.Instance.currentPvPBudget >= cost)
            {
                WaveManager.Instance.currentPvPBudget -= cost;
                WaveManager.Instance.pvpWaveQueue.Add(enemyPrefab);
                updated = true;
            }
            else
            {
                break; 
            }
        }

        if (updated) UpdateUI();
    }

    public void RemoveEnemyAmount(GameObject enemyPrefab, int count)
    {
        if (WaveManager.Instance == null) return;
        
        Enemy enemyScript = enemyPrefab.GetComponent<Enemy>();
        if (enemyScript == null || enemyScript.enemyData == null) return;

        int cost = enemyScript.enemyData.attackCost;
        int removedCount = 0;

        for (int i = WaveManager.Instance.pvpWaveQueue.Count - 1; i >= 0; i--)
        {
            if (WaveManager.Instance.pvpWaveQueue[i].name == enemyPrefab.name)
            {
                WaveManager.Instance.pvpWaveQueue.RemoveAt(i);
                WaveManager.Instance.currentPvPBudget += cost;
                removedCount++;

                if (removedCount >= count)
                {
                    break;
                }
            }
        }

        if (removedCount > 0)
        {
            UpdateUI();
        }
    }

    public void ClearQueue()
    {
        if (WaveManager.Instance == null) return;

        foreach (GameObject enemy in WaveManager.Instance.pvpWaveQueue)
        {
            int cost = enemy.GetComponent<Enemy>().enemyData.attackCost;
            WaveManager.Instance.currentPvPBudget += cost;
        }

        WaveManager.Instance.pvpWaveQueue.Clear();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (WaveManager.Instance == null) return;

        if (budgetText != null)
        {
            budgetText.text = "Budget: " + WaveManager.Instance.currentPvPBudget.ToString();
        }

        if (enemyCountTexts != null)
        {
            System.Collections.Generic.Dictionary<string, int> counts = new System.Collections.Generic.Dictionary<string, int>();
            foreach (GameObject enemy in WaveManager.Instance.pvpWaveQueue)
            {
                string enemyName = enemy.name;
                if (counts.ContainsKey(enemyName))
                    counts[enemyName]++;
                else
                    counts[enemyName] = 1;
            }

            foreach (var mapping in enemyCountTexts)
            {
                if (mapping.countText != null)
                {
                    int count = counts.ContainsKey(mapping.enemyName) ? counts[mapping.enemyName] : 0;
                    mapping.countText.text = count.ToString(); 
                }
            }
        }
    }
}
