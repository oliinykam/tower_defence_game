using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyData enemyData;

    [Header("UI")]
    public Slider healthBarSlider;
    
    [Header("Audio")]
    public AudioClip deathSFX;

    private float speed;
    private float currentSpeed; 
    private int health;
    private int maxHealthCurrent; 
    private int currentReward;  
    [System.Serializable]
    public class SlowEffect
    {
        public int sourceId;
        public float timer;
        public float slowPct;
    }
    private System.Collections.Generic.List<SlowEffect> activeSlows = new System.Collections.Generic.List<SlowEffect>();
    
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    [HideInInspector]
    public Transform[] waypoints;
    public int currentWayPoint = 0;

    public void InitWaveStats(float healthScale, float speedScale, float rewardScale)
    {
        currentWayPoint = 0;
        activeSlows.Clear();
        if (sr != null) sr.color = Color.white;

        if (enemyData != null)
        {
            maxHealthCurrent = Mathf.RoundToInt(enemyData.maxHealth * (1f + healthScale));
            speed = enemyData.speed * (1f + speedScale);
            currentReward = Mathf.RoundToInt(enemyData.reward * (1f + rewardScale));
        }
        else
        {
            maxHealthCurrent = 1;
            speed = 2f;
            currentReward = 5;
        }

        health = maxHealthCurrent;
        currentSpeed = speed;

        if (healthBarSlider != null)
        {
            healthBarSlider.value = 1f; 
        }
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0 || currentWayPoint >= waypoints.Length) return;

        if (activeSlows.Count > 0)
        {
            for (int i = activeSlows.Count - 1; i >= 0; i--)
            {
                activeSlows[i].timer -= Time.deltaTime;
                if (activeSlows[i].timer <= 0)
                {
                    activeSlows.RemoveAt(i);
                }
            }

            if (activeSlows.Count > 0)
            {
                activeSlows.Sort((a, b) => a.slowPct.CompareTo(b.slowPct));

                float totalReduction = 0f;
                float diminishingFactor = 1f;

                for (int i = 0; i < activeSlows.Count; i++)
                {
                    float reduction = 1f - activeSlows[i].slowPct;
                    totalReduction += reduction * diminishingFactor;
                    diminishingFactor *= 0.5f; 
                }

                totalReduction = Mathf.Min(0.9f, totalReduction);

                currentSpeed = speed * (1f - totalReduction);
                if (sr != null) sr.color = new Color(0.5f, 0.8f, 1f);
            }
            else
            {
                currentSpeed = speed;
                if (sr != null) sr.color = Color.white;
            }
        }
        else
        {
            currentSpeed = speed;
        }

        Transform target = waypoints[currentWayPoint];
        Vector3 dir = (target.position - transform.position).normalized;

        transform.position += dir * currentSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWayPoint++;

            if (currentWayPoint >= waypoints.Length)
            {
                if (HealthManager.Instance != null)
                {
                    HealthManager.Instance.UpdateHealth(-1);
                }
                Die();
                return;
            }
        }
    }

    public void ApplySlow(int sourceId, float slowPct, float duration)
    {
        if (enemyData != null && enemyData.isImmuneToSlow) return;

        foreach (var slow in activeSlows)
        {
            if (slow.sourceId == sourceId)
            {
                slow.timer = duration; 
                slow.slowPct = slowPct;
                return;
            }
        }
        activeSlows.Add(new SlowEffect { sourceId = sourceId, timer = duration, slowPct = slowPct });
    }

    public float GetCurrentProgress()
    {
        if (waypoints == null || waypoints.Length == 0 || currentWayPoint >= waypoints.Length) return 0f;
        
        float distToNext = Vector3.Distance(transform.position, waypoints[currentWayPoint].position);
        return currentWayPoint - (distToNext / 1000f);
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (healthBarSlider != null)
        {
            healthBarSlider.value = (float)health / maxHealthCurrent;
        }

        if (health <= 0)
        {
            CoinManager.instance.UpdateCoins(currentReward);
            
            if (deathSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(deathSFX);
            }
            
            Die();
        }
    }

    void Die()
    {
        gameObject.SetActive(false);
    }
}