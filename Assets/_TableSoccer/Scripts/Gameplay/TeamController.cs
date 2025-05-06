using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace YRA {
    public enum TeamRole
    {
        Attacking,
        Defending
    }

    public class TeamController : MonoBehaviour
    {        
        [Header("UI References")]
        Slider _energyBar;

        [Header("Team Settings")]
        [SerializeField] string teamID;
        public bool isPlayerTeam {get; private set;}
        // [SerializeField] TeamRole currentRole;
        public TeamRole currentRole { get; private set; }
        
        [Header("Player References")]
        [SerializeField]  List<Soldier> soldiers = new List<Soldier>();
        Transform[] startingPositionsAttack;
        Transform[] startingPositionsDefense;

        void UpdateEnergy()
        {
            
        }
        
        public void SetTeamRole(TeamRole role)
        {
            currentRole = role;
            
            foreach (var player in soldiers)
            {
                player.currentTeamRole = role;
                
                // Set individual player roles based on team role
                if (role == TeamRole.Attacking)
                {
                    player.SetPlayerRole(SoldierRole.Attacker);
                }
                else
                {
                    player.SetPlayerRole(SoldierRole.Defender);
                }
            }
        }
    }
}