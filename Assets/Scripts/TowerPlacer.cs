using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    public Tilemap placementMap;
    public Tilemap nonPlaceableMap;

    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();
    private GameObject ghostInstance;
    private Vector3Int lastCellPos;

    public Vector2 cursorOffset;

    [Header("Audio")]
    public AudioClip placementSFX;
    public AudioClip removalSFX;

    void Update()
    {
        HandlePlacementHover();
        HandlePlacementClick();
        HandleRemoveClick();
    }

    void HandlePlacementHover()
    {
        if (TowerSelectionUI.SelectedTowerPrefab == null)
        {
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            return;
        }

        if (ghostInstance == null)
        {
            ghostInstance = Instantiate(TowerSelectionUI.SelectedTowerPrefab);
            foreach (var col in ghostInstance.GetComponentsInChildren<Collider2D>())
                col.enabled = false;
            foreach (var mono in ghostInstance.GetComponentsInChildren<MonoBehaviour>())
                mono.enabled = false;
                
            CreateRangeCircle(ghostInstance);
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Tower towerScript = ghostInstance != null ? ghostInstance.GetComponent<Tower>() : null;
        if (towerScript != null && towerScript.towerData != null)
        {
            mouseWorldPos.x -= towerScript.towerData.visualOffset.x;
            mouseWorldPos.y -= towerScript.towerData.visualOffset.y;
        }
        
        mouseWorldPos.x += cursorOffset.x;
        mouseWorldPos.y += cursorOffset.y;

        Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);
        
        Vector3 worldCenter = placementMap.GetCellCenterWorld(cellPos);
        worldCenter.z = 0;

        if (towerScript != null && towerScript.towerData != null)
        {
            worldCenter += new Vector3(towerScript.towerData.visualOffset.x, towerScript.towerData.visualOffset.y, 0);
        }

        ghostInstance.transform.position = worldCenter;

        bool valid = IsCellValid(cellPos);

        Color ghostColor = valid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        foreach (var sr in ghostInstance.GetComponentsInChildren<SpriteRenderer>())
            sr.color = ghostColor;
            
        foreach (var lr in ghostInstance.GetComponentsInChildren<LineRenderer>())
        {
            lr.startColor = ghostColor;
            lr.endColor = ghostColor;
        }

        lastCellPos = cellPos;
    }

    private bool IsCellValid(Vector3Int cellPos)
    {
        return !nonPlaceableMap.HasTile(cellPos + new Vector3Int(-1, -1, 0)) 
            && !placedTowers.ContainsKey(cellPos);
    }

    private void CreateRangeCircle(GameObject ghost)
    {
        Tower tower = ghost.GetComponent<Tower>();
        if (tower == null) return;
        
        float range = 0f;
        if (tower.towerData != null)
        {
            range = tower.towerData.range;
        }

        GameObject rangeObj = new GameObject("RangeIndicator");
        rangeObj.transform.SetParent(ghost.transform);
        
        if (tower != null && tower.towerData != null)
        {
            rangeObj.transform.localPosition = new Vector3(tower.towerData.circleOffset.x, tower.towerData.circleOffset.y, 0);
        }
        else
        {
            rangeObj.transform.localPosition = Vector3.zero;
        }

        LineRenderer lr = rangeObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 51;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 2;

        float angle = 0f;
        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * range;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle) * range * 0.6f;
            lr.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / 50f);
        }
    }

    void HandlePlacementClick()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (TowerSelectionUI.SelectedTowerPrefab == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (!IsCellValid(lastCellPos)) return;

        int price = 0;
        Tower towerComp = TowerSelectionUI.SelectedTowerPrefab.GetComponent<Tower>();
        if (towerComp != null && towerComp.towerData != null)
        {
            price = towerComp.towerData.towerPrice;
        }
        
        if (CoinManager.instance.coins < price)
        {
            TowerSelectionUI.SelectedTowerPrefab = null;
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            return;
        }

        GameObject newTower = Instantiate(TowerSelectionUI.SelectedTowerPrefab, ghostInstance.transform.position, Quaternion.identity);
        
        CoinManager.instance.UpdateCoins(-price);

        if (placementSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(placementSFX);
        }
        
        placedTowers.Add(lastCellPos, newTower);

        if (CoinManager.instance.coins < price)
        {
            TowerSelectionUI.SelectedTowerPrefab = null;
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
        }
    }

    void HandleRemoveClick()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;
        if (!Input.GetMouseButtonDown(1)) return;

        if (TowerSelectionUI.SelectedTowerPrefab != null)
        {
            TowerSelectionUI.SelectedTowerPrefab = null;
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPosToDestroy = new Vector3Int(-9999, -9999, 0);
        float minDistance = 1.0f;

        foreach (var kvp in placedTowers)
        {
            Vector3 towerPos = kvp.Value.transform.position;
            Tower t = kvp.Value.GetComponent<Tower>();
            if (t != null && t.towerData != null)
            {
                towerPos.x += t.towerData.visualOffset.x;
                towerPos.y += t.towerData.visualOffset.y;
            }

            float dist = Vector2.Distance(mouseWorldPos, towerPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                cellPosToDestroy = kvp.Key;
            }
        }

        if (placedTowers.ContainsKey(cellPosToDestroy))
        {
            Vector3Int cellPos = cellPosToDestroy;
            GameObject towerToDestroy = placedTowers[cellPos];
            if (towerToDestroy != null)
            {
                int price = 0;
                if (towerToDestroy.GetComponent<Tower>().towerData != null)
                {
                    price = towerToDestroy.GetComponent<Tower>().towerData.towerPrice;
                }
                int cashback = Mathf.RoundToInt(price * 0.5f);
                CoinManager.instance.UpdateCoins(cashback);

                if (removalSFX != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(removalSFX);
                }

                Destroy(towerToDestroy);
            }
            placedTowers.Remove(cellPos);
        }
    }
}