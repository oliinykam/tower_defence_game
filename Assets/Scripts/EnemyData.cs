using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "TowerDefense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float speed = 2f;
    public int maxHealth = 3;
    
    [Header("Special Abilities")]
    public bool isImmuneToSlow = false;

    [Header("Економіка та бюджет")]
    public int attackCost = 10;
    public int reward = 5;      
}