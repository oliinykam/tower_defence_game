using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerData towerData;

    public Transform firePoint;

    private float fireCountdown = 0f;
    private Transform currentTarget;

    private SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (towerData != null && towerData.cloudPS != null)
        {
            Instantiate(towerData.cloudPS, transform.position, Quaternion.identity);
        }

        if (towerData != null && towerData.isAura)
        {
            CreateAuraVisual();
        }
    }

    private void CreateAuraVisual()
    {
        GameObject rangeObj = new GameObject("AuraIndicator");
        rangeObj.transform.SetParent(transform);
        
        if (towerData != null)
        {
            rangeObj.transform.localPosition = new Vector3(towerData.circleOffset.x, towerData.circleOffset.y, 0);
        }
        else
        {
            rangeObj.transform.localPosition = Vector3.zero;
        }

        LineRenderer lr = rangeObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.positionCount = 51;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            lr.material = sr.sharedMaterial;
        else
            lr.material = new Material(Shader.Find("Sprites/Default"));
        
        Color auraColor = new Color(0.2f, 0.8f, 1f, 0.4f); 
        lr.startColor = auraColor;
        lr.endColor = auraColor;
        lr.sortingOrder = 2;

        float angle = 0f;
        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * towerData.range;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle) * towerData.range * 0.6f;
            lr.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / 50f);
        }
    }

    void Update()
    {
        if (towerData != null && towerData.isAura)
        {
            if (fireCountdown <= 0f)
            {
                ApplyAura();
                fireCountdown = 1f / towerData.fireRate;
            }
            fireCountdown -= Time.deltaTime;
        }
        else
        {
            FindTarget(); 

            if (currentTarget != null)
            {
                if (fireCountdown <= 0f)
                {
                    Shoot();
                    if (towerData != null)
                    {
                        fireCountdown = 1f / towerData.fireRate;
                    }
                }
                fireCountdown -= Time.deltaTime;
            }
        }
    }

    void FindTarget()
    {
        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        Transform bestTarget = null;
        float maxProgress = -1000f;

        foreach (Enemy enemy in allEnemies)
        {
            Vector2 diff = enemy.transform.position - transform.position;
            diff.y /= 0.6f;
            float distanceToEnemy = diff.magnitude;
            
            if (towerData != null && distanceToEnemy <= towerData.range)
            {
                float enemyProgress = enemy.GetCurrentProgress();

                if (enemyProgress > maxProgress)
                {
                    maxProgress = enemyProgress;
                    bestTarget = enemy.transform;
                }
            }
        }

        currentTarget = bestTarget;
    }

    void ApplyAura()
    {
        if (towerData == null) return;

        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            Vector2 diff = enemy.transform.position - transform.position;
            diff.y /= 0.6f;
            
            if (diff.magnitude <= towerData.range)
            {
                enemy.ApplySlow(GetInstanceID(), towerData.slowAmount, towerData.slowDuration);
            }
        }
    }

    private System.Collections.Generic.List<GameObject> projectilePool = new System.Collections.Generic.List<GameObject>();

    GameObject GetPooledProjectile()
    {
        foreach (GameObject obj in projectilePool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        
        if (towerData == null || towerData.arrowPrefab == null) return null;
        
        GameObject newObj = Instantiate(towerData.arrowPrefab);
        projectilePool.Add(newObj);
        return newObj;
    }

    void Shoot()
    {
        if (firePoint == null || towerData == null || towerData.arrowPrefab == null) return;

        if (towerData.shootSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(towerData.shootSFX, 0.6f); 
        }

        GameObject arrowGO = GetPooledProjectile();
        if (arrowGO == null) return;
        
        arrowGO.transform.position = firePoint.position;
        arrowGO.transform.rotation = Quaternion.identity;
        arrowGO.SetActive(true);
        
        Projectile projectile = arrowGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.SetTarget(currentTarget);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (towerData != null)
        {
            Gizmos.DrawWireSphere(transform.position, towerData.range);
        }
    }
}