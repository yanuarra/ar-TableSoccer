using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Analytics;

namespace YRA {
    public enum TeamRole
    {
        Attacking,
        Defending
    }

    public class TeamController : MonoBehaviour
    {        
        [Header("UI References")]
        [SerializeField] Slider _energyBar;

        [Header("Team Settings")]
        [SerializeField] string teamID;
        public bool isPlayerTeam; //{get; private set;}
        // [SerializeField] TeamRole currentRole;
        public TeamRole currentRole { get; private set; }
        
        [Header("Player References")]
        [SerializeField]  List<Soldier> soldiers = new List<Soldier>();
        Transform[] startingPositionsAttack;
        Transform[] startingPositionsDefense;

        void Start()
        {
            // soldiers = FindObjectsByType<Soldier>(FindObjectsSortMode.None).Where(x => x.isPlayerTeam).ToList();
        }

        public void Reset()
        {
            
        }

        public void UpdateSoldiersState(Soldier except, SoldierState soldierState)
        {
            foreach (var soldier in soldiers)
            {
                if (soldier == except) continue;
                soldier.SetState(soldierState);
            }
        }

        public Soldier GetNearestActiveAttacker(Vector3 position)
        {
            Soldier nearest = null;
            float minDistance = float.MaxValue;
            foreach (var soldier in SoldierManager.Instance.GetActiveAttackers())
            {
                float distance = Vector3.Distance(position, soldier.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = soldier;
                }
            }
            return nearest;
        }
        
        public void SetTeamRole(TeamRole role)
        {
            currentRole = role;
            foreach (var soldier in soldiers)
            {
                Debug.Log(string.Format("{0} cur {1} set {2}", soldier, soldier.curSoldierRole, role));
                soldier.currentTeamRole = role;
                if (role == TeamRole.Attacking)
                {
                    soldier.SetPlayerRole(SoldierRole.Attacker);
                }
                else
                {
                    soldier.SetPlayerRole(SoldierRole.Defender);
                }
            }
        }
    }
}