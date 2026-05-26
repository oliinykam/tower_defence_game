using UnityEngine;

public class TowerSelectionUI : MonoBehaviour
{
    public static GameObject SelectedTowerPrefab;

    public void SelectTower(GameObject towerPrefab){
        if(towerPrefab == SelectedTowerPrefab)
        {
            SelectedTowerPrefab = null;
            return;
        }
        
        Tower towerComponent = towerPrefab.GetComponent<Tower>();
        if(towerComponent != null && towerComponent.towerData != null && towerComponent.towerData.towerPrice <= CoinManager.instance.coins) {
            SelectedTowerPrefab = towerPrefab;
        }
        
    }   
}