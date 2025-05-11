using UnityEditor.Build;
using UnityEngine;

namespace YRA
{
    public class ObjectSpawner : MonoBehaviour
    {
        // [SerializeField] private GameObject objectToSpawn;
        Camera cam;
        public LayerMask planeLayerMask;
        public EnergySystem[] energySystems;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            cam = Camera.main;
            energySystems = FindObjectsByType<EnergySystem>(FindObjectsSortMode.None);
        }

        EnergySystem GetEnerySystem(bool isPlayerSide){
            foreach (var item in energySystems)
            {
                if (item.isPlayer==isPlayerSide)
                    return item;
            }
            return null;
        }

        public void SpawnObjectAtPosition(Vector2 screenPosition)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, planeLayerMask))
            {
                // Spawn the object at hit position
                bool isPlayerSide = FieldSetup.Instance.GetSideForPoint(hit.point) == FieldSetup.PlaneSide.Player;
                EnergySystem e = GetEnerySystem(isPlayerSide);
                Debug.Log(isPlayerSide + " " +e.isPlayer);
                if (!e.CanAffordSpawn()) return;
                SoldierManager.Instance.SpawnSoldier(hit.point, isPlayerSide);
                e.UseEnergy();
            }
        }
    }
}
