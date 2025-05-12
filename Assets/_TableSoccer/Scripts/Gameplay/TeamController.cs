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

        [Header("Team Settings")]
        [SerializeField] string teamID;
        [field:SerializeField]
        public bool isPlayerTeam{get; private set;}
        [SerializeField] EnergySystem energySystem;
        public TeamRole currentRole { get; private set; }
        public Goal goal { get; private set; }
        
        [Header("Player References")]
        [SerializeField] List<Soldier> soldiers = new List<Soldier>();

        void Start()
        {
            goal = GetGoal();
            if (energySystem == null) energySystem = GetComponentInChildren<EnergySystem>();
            energySystem.isPlayer = isPlayerTeam;
        }

        public void Reset()
        {
            foreach (var item in soldiers)
            {
                Destroy(item.gameObject);
            }
            soldiers.Clear();
        }
        
        public void AddSoldierToTeam(Soldier newSoldier)
        {
            if (!soldiers.Contains(newSoldier)) 
            {       
                soldiers.Add(newSoldier);
                SoldierManager.Instance.AddSoldierToCollection(newSoldier);
            }
        }

        public void RemoveSoldierFromTeam(Soldier oldSoldier)
        {
            if (soldiers.Contains(oldSoldier)) 
            {
                soldiers.Remove(oldSoldier);
                SoldierManager.Instance.RemoveSoldierFromCollection(oldSoldier);
            }
        }

        public void OnGoal()
        {
            foreach (var soldier in soldiers)
            {
                soldier.StopMoving();
                if (currentRole == TeamRole.Attacking)
                {
                    soldier.PlayVictoryAnimation();
                }else
                {
                    soldier.PlayIdleAnimation();
                }
            }
        }
        public Goal GetGoal()
        {
            return FindObjectsByType<Goal>(FindObjectsSortMode.None).Where(x => x.isPlayerGoal != isPlayerTeam).FirstOrDefault();
        }

        public void UpdateSoldiersState(Soldier except, SoldierState soldierState)
        {
            foreach (var soldier in soldiers)
            {
                if (soldier == except || 
                    soldier.curSoldierState == SoldierState.InActive) 
                    continue;
                soldier.SetState(soldierState);
            }
        }

        public Soldier GetNearestActiveAttacker(Soldier soldier)
        {
            List<Soldier> activeSoldiers = SoldierManager.Instance.GetActiveAttackers();
            activeSoldiers.Remove(soldier);
            return activeSoldiers.OrderBy(x => Vector3.Distance(soldier.transform.position, x.transform.position)).FirstOrDefault();
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