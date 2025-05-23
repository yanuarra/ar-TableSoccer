using TMPro;
using UnityEngine;

namespace YRA
{
    public class SoldierSpawner : MonoBehaviour
    {
        // [SerializeField] private GameObject objectToSpawn;
        Camera cam;
        public LayerMask planeLayerMask;
        public EnergySystem[] energySystems;
        [SerializeField] private TMP_Text Debugtext;
        [Header("Spawning Settings")]
        [SerializeField] private float normalOffset = 0.01f;

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
                // Debugtext.text = hit.transform.gameObject.name;
                // Spawn the object at hit position
                bool isPlayerSide = FieldSetup.Instance.GetSideForPoint(hit.point) == FieldSetup.PlaneSide.Player;
                EnergySystem e = GetEnerySystem(isPlayerSide);
                Debug.Log(isPlayerSide + " " +e.isPlayer);
                if (!e.CanAffordSpawn()) return;
                SoldierManager.Instance.SpawnSoldier(hit.point, hit.normal, normalOffset, isPlayerSide);
                e.UseEnergy();
            }
        }
    }
}
