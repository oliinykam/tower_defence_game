using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Data", menuName = "TowerDefense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Main Settings")]
    public string towerName = "New Tower";
    public int towerPrice = 10;
    
    [Header("Combat Settings")]
    public float range = 3.5f;
    public float fireRate = 1f;
    
    [Header("Prefabs & Audio")]
    public GameObject arrowPrefab;
    public GameObject cloudPS;
    public AudioClip shootSFX;
    
    [Header("Visual Settings")]
    public Vector2 visualOffset;
    public Vector2 circleOffset;

    [Header("Aura Settings (Freezer)")]
    public bool isAura = false;
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;
}
