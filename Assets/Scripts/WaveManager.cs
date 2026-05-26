using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum GameWaveMode { Finite, Endless, PvPHotSeat }

[System.Serializable]
public class EnemyUnlock
{
    public GameObject enemyPrefab;
    public int unlockWave = 1;
}

[System.Serializable]
public class EnemyPath
{
    public Transform[] wayPoints;
}

public class WaveManager : MonoBehaviour
{
    [Header("Game Mode Settings")]
    public GameWaveMode gameMode;
    public int maxWaves = 10;

    [Header("Difficulty Settings")]
    public int startBudget = 200;
    public AnimationCurve budgetCurve;
    public int pvpMaxBudget = 900;

    public float healthScalePerWave = 0.08f;
    public float speedScalePerWave  = 0.04f;
    public float rewardScalePerWave = 0.08f;

    [Header("Enemies")]
    public EnemyUnlock[] enemyUnlocks;

    [Header("Spawn Group Settings")]
    public int   minGroupSize        = 2;
    public int   maxGroupSize        = 4;
    public float minInnerGroupDelay  = 0.15f;
    public float maxInnerGroupDelay  = 0.35f;
    public float minInterGroupDelay  = 2.0f;
    public float maxInterGroupDelay  = 4.0f;

    [Header("Spawn Group Scaling (Per Wave)")]
    public float groupSizeAddPerWave        =  0.40f;
    public float innerGroupDelayAddPerWave  = -0.008f;
    public float interGroupDelayAddPerWave  = -0.12f;

    [Header("References")]
    public Button            startWaveButton;
    public EnemyPath[]       enemyPaths;
    public TextMeshProUGUI   waveTxt;

    private int  currentWaveIndex = 0;
    private bool waveRunning      = false;

    private List<GameObject> enemyPool = new List<GameObject>();

    [HideInInspector] public int              currentPvPBudget;
    [HideInInspector] public List<GameObject> pvpWaveQueue = new List<GameObject>();

    public static WaveManager Instance;

    private void Awake() => Instance = this;

    void Start()
    {
        int modeInt = PlayerPrefs.GetInt("GameMode", 0);
        gameMode = (GameWaveMode)modeInt;

        if (PlayerPrefs.HasKey("MaxWaves"))
            maxWaves = PlayerPrefs.GetInt("MaxWaves", maxWaves);

        UpdateWaveText();
        startWaveButton.onClick.AddListener(StartWave);

        if (gameMode == GameWaveMode.PvPHotSeat)
            CalculatePvPBudget();
    }

    public void CalculatePvPBudget()
    {
        int scaledBudget = startBudget + (int)budgetCurve.Evaluate(currentWaveIndex);
        currentPvPBudget = Mathf.Min(scaledBudget, pvpMaxBudget);
        pvpWaveQueue.Clear();

        if (AttackerManager.Instance != null)
            AttackerManager.Instance.OpenAttackerUI(currentPvPBudget);
    }

    public void StartWave()
    {
        if (waveRunning) return;

        if ((gameMode == GameWaveMode.Finite || gameMode == GameWaveMode.PvPHotSeat)
            && currentWaveIndex >= maxWaves)
            return;

        if (gameMode == GameWaveMode.PvPHotSeat)
        {
            if (pvpWaveQueue.Count == 0)
            {
                Debug.LogWarning("PvP Wave Queue is empty! Add enemies first.");
                return;
            }
            StartCoroutine(RunPvPWave());
        }
        else
        {
            StartCoroutine(RunWave());
        }
    }

    List<GameObject> GenerateWaveEnemies(int totalBudget)
    {
        List<GameObject> result = new List<GameObject>();

        GameObject goblinPrefab = GetUnlockedPrefabByName("Goblin");
        GameObject orcPrefab    = GetUnlockedPrefabByName("Orc");
        GameObject ghostPrefab  = GetUnlockedPrefabByName("Ghost");

        int remainingBudget = totalBudget;

        float waveProgress = Mathf.Clamp01((float)currentWaveIndex / 9f);
        float orcBudgetRatio = Mathf.Lerp(0f, 0.5f, waveProgress);
        int   orcBudget      = Mathf.RoundToInt(totalBudget * orcBudgetRatio);

        if (orcPrefab != null)
        {
            int orcCost    = GetCost(orcPrefab);
            int goblinCost = goblinPrefab != null ? GetCost(goblinPrefab) : int.MaxValue;
            int ghostCost  = ghostPrefab  != null ? GetCost(ghostPrefab)  : int.MaxValue;

            while (remainingBudget >= orcCost && orcBudget >= orcCost)
            {
                result.Add(orcPrefab);
                remainingBudget -= orcCost;
                orcBudget       -= orcCost;
                int followersCount = Random.Range(2, 5);
                for (int f = 0; f < followersCount; f++)
                {
                    bool canSpawnGhost = ghostPrefab  != null
                                      && remainingBudget >= ghostCost
                                      && (currentWaveIndex + 1) >= GetUnlockWave("Ghost")
                                      && Random.value < 0.3f;

                    bool canSpawnGoblin = goblinPrefab != null
                                       && remainingBudget >= goblinCost;

                    if (canSpawnGhost)
                    {
                        result.Add(ghostPrefab);
                        remainingBudget -= ghostCost;
                    }
                    else if (canSpawnGoblin)
                    {
                        result.Add(goblinPrefab);
                        remainingBudget -= goblinCost;
                    }
                    else break;
                }
            }
        }

        int maxAttempts = 500;
        while (remainingBudget > 0 && maxAttempts-- > 0)
        {
            List<GameObject> affordable = GetAffordablePrefabs(remainingBudget);
            if (affordable.Count == 0) break;

            float totalWeight = 0f;
            var weighted = new List<(GameObject prefab, float weight)>();

            foreach (var prefab in affordable)
            {
                float w = Mathf.Lerp(1f / GetCost(prefab), GetCost(prefab), waveProgress);
                weighted.Add((prefab, w));
                totalWeight += w;
            }

            float roll       = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            GameObject chosen = weighted[0].prefab;

            foreach (var (prefab, weight) in weighted)
            {
                cumulative += weight;
                if (roll <= cumulative) { chosen = prefab; break; }
            }

            result.Add(chosen);
            remainingBudget -= GetCost(chosen);
        }

        Debug.Log($"Хвиля {currentWaveIndex + 1}: {result.Count} ворогів, бюджет витрачено.");
        return result;
    }

    GameObject GetUnlockedPrefabByName(string enemyName)
    {
        foreach (var unlock in enemyUnlocks)
        {
            if (unlock.enemyPrefab != null
                && unlock.enemyPrefab.name == enemyName
                && (currentWaveIndex + 1) >= unlock.unlockWave)
                return unlock.enemyPrefab;
        }
        return null;
    }

    int GetUnlockWave(string enemyName)
    {
        foreach (var unlock in enemyUnlocks)
            if (unlock.enemyPrefab != null && unlock.enemyPrefab.name == enemyName)
                return unlock.unlockWave;
        return 999;
    }

    int GetCost(GameObject prefab)
    {
        var e = prefab.GetComponent<Enemy>();
        return (e != null && e.enemyData != null) ? e.enemyData.attackCost : 0;
    }

    List<GameObject> GetAffordablePrefabs(int budget)
    {
        var list = new List<GameObject>();
        foreach (var unlock in enemyUnlocks)
        {
            if (unlock.enemyPrefab == null) continue;
            if ((currentWaveIndex + 1) < unlock.unlockWave) continue;
            if (GetCost(unlock.enemyPrefab) <= budget) list.Add(unlock.enemyPrefab);
        }
        return list;
    }

    IEnumerator RunWave()
    {
        waveRunning = true;
        startWaveButton.interactable = false;

        int totalBudget = startBudget + (int)budgetCurve.Evaluate(currentWaveIndex);
        List<GameObject> enemiesToSpawn = GenerateWaveEnemies(totalBudget);

        yield return StartCoroutine(SpawnEnemiesInGroups(enemiesToSpawn));
        yield return StartCoroutine(WaitForAllEnemiesDead());

        waveRunning = false;
        currentWaveIndex++;

        if (gameMode == GameWaveMode.Finite && currentWaveIndex >= maxWaves)
        {
            OnGameVictory();
        }
        else
        {
            startWaveButton.interactable = true;
            UpdateWaveText();
        }
    }

    IEnumerator RunPvPWave()
    {
        waveRunning = true;
        startWaveButton.interactable = false;

        List<GameObject> enemiesToSpawn = new List<GameObject>(pvpWaveQueue);
        pvpWaveQueue.Clear();

        if (AttackerManager.Instance != null)
            AttackerManager.Instance.CloseAttackerUI();

        Debug.Log($"PvP Хвиля {currentWaveIndex + 1}: {enemiesToSpawn.Count} ворогів.");

        yield return StartCoroutine(SpawnEnemiesInGroups(enemiesToSpawn));
        yield return StartCoroutine(WaitForAllEnemiesDead());

        waveRunning = false;
        currentWaveIndex++;

        if (currentWaveIndex >= maxWaves)
        {
            OnGameVictory();
        }
        else
        {
            startWaveButton.interactable = true;
            UpdateWaveText();
            CalculatePvPBudget();
        }
    }

    IEnumerator SpawnEnemiesInGroups(List<GameObject> enemiesToSpawn)
    {
        int   curMinGroupSize   = Mathf.FloorToInt(minGroupSize  + groupSizeAddPerWave       * currentWaveIndex);
        int   curMaxGroupSize   = Mathf.FloorToInt(maxGroupSize  + groupSizeAddPerWave       * currentWaveIndex);
        float curMinInnerDelay  = Mathf.Max(0.01f, minInnerGroupDelay + innerGroupDelayAddPerWave * currentWaveIndex);
        float curMaxInnerDelay  = Mathf.Max(0.01f, maxInnerGroupDelay + innerGroupDelayAddPerWave * currentWaveIndex);
        float curMinInterDelay  = Mathf.Max(0.10f, minInterGroupDelay + interGroupDelayAddPerWave * currentWaveIndex);
        float curMaxInterDelay  = Mathf.Max(0.10f, maxInterGroupDelay + interGroupDelayAddPerWave * currentWaveIndex);

        int i = 0;
        while (i < enemiesToSpawn.Count)
        {
            int groupSize = Random.Range(curMinGroupSize, curMaxGroupSize + 1);

            for (int j = 0; j < groupSize && i < enemiesToSpawn.Count; j++)
            {
                SpawnEnemy(enemiesToSpawn[i]);
                i++;
                yield return new WaitForSeconds(Random.Range(curMinInnerDelay, curMaxInnerDelay));
            }

            yield return new WaitForSeconds(Random.Range(curMinInterDelay, curMaxInterDelay));
        }
    }

    IEnumerator WaitForAllEnemiesDead()
    {
        bool enemiesAlive = true;
        while (enemiesAlive)
        {
            enemiesAlive = false;
            foreach (GameObject enemyObj in enemyPool)
            {
                if (enemyObj.activeInHierarchy)
                {
                    enemiesAlive = true;
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        GameObject e = GetPooledEnemy(prefab);

        Transform[] chosenPath = enemyPaths[Random.Range(0, enemyPaths.Length)].wayPoints;

        e.transform.position = chosenPath[0].position;
        e.SetActive(true);

        Enemy enemy = e.GetComponent<Enemy>();
        enemy.waypoints = chosenPath;

        float currentHealthScale = healthScalePerWave * currentWaveIndex;
        float currentSpeedScale  = speedScalePerWave  * currentWaveIndex;
        float currentRewardScale = rewardScalePerWave * currentWaveIndex;

        enemy.InitWaveStats(currentHealthScale, currentSpeedScale, currentRewardScale);
    }

    GameObject GetPooledEnemy(GameObject prefab)
    {
        foreach (GameObject obj in enemyPool)
        {
            if (!obj.activeInHierarchy && obj.name == prefab.name)
                return obj;
        }

        GameObject newObj = Instantiate(prefab);
        newObj.name = prefab.name;
        enemyPool.Add(newObj);
        return newObj;
    }

    void OnGameVictory()
    {
        Debug.Log("Перемога! Всі хвилі пройдено.");
        startWaveButton.interactable = false;

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.ShowEndGame(true);
    }

    private void UpdateWaveText()
    {
        if (waveTxt == null) return;

        if (gameMode == GameWaveMode.Finite || gameMode == GameWaveMode.PvPHotSeat)
            waveTxt.text = $"{currentWaveIndex + 1} / {maxWaves}";
        else
            waveTxt.text = $"{currentWaveIndex + 1}";
    }
}