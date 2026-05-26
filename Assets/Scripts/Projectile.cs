using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 1; 
    private Transform target;
    public GameObject hitPS;

    public bool isAoE = false;          
    public float explosionRadius = 1.5f; 

    public bool isFreezer = false;      
    public float slowAmount = 0.5f;     
    public float slowDuration = 2f;     

    public AudioClip hitSFX;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 targetCenter = target.position + new Vector3(0, 0.5f, 0);
        transform.position = Vector3.MoveTowards(transform.position, targetCenter, speed * Time.deltaTime);

        Vector3 direction = targetCenter - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (Vector3.Distance(transform.position, targetCenter) < 0.2f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (isAoE)
        {
            Explode();
        }
        else
        {
            Enemy enemyScript = target.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
                
                if (isFreezer)
                {
                    enemyScript.ApplySlow(GetInstanceID(), slowAmount, slowDuration);
                }
            }
        }

        if (hitPS != null)
        {
            Instantiate(hitPS, transform.position, Quaternion.identity);
            
        }
        
        if (hitSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hitSFX);
        }
        
        gameObject.SetActive(false); 
    }

    void Explode()
    {
        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in allEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy <= explosionRadius)
            {
                enemy.TakeDamage(damage);

                if (isFreezer)
                {
                    enemy.ApplySlow(GetInstanceID(), slowAmount, slowDuration);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isAoE)
        {
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}