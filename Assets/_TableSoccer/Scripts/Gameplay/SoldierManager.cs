using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using NUnit.Framework;

namespace YRA
{
    public class SoldierManager : Singleton<SoldierManager>
    {
        public List<Soldier> soldiersCollection = new List<Soldier>();
        public TeamController teamControllerPlayer {get; private set;}
        public TeamController teamControllerEnemy{get; private set;}
        [SerializeField] Transform parent;
        [SerializeField] GameObject soldierPrefab;
        [SerializeField] public Material _matGray;
        [SerializeField] public Material _matPlayer;
        [SerializeField] public Material _matEnemy;
        public void Start()
        {
            GetTeamControllers();
            GetSoldiers();
        }  
        
        public void Reset()
        {
            soldiersCollection.Clear();
        }

        void GetSoldiers()
        {
            soldiersCollection.Clear();
            soldiersCollection = FindObjectsByType<Soldier>(FindObjectsSortMode.None).ToList();
        }

        public void OnGoal()
        {
            teamControllerEnemy.OnGoal();
            teamControllerPlayer.OnGoal();
        }

        public void GetTeamControllers()
        {
            TeamController[] teams = FindObjectsByType<TeamController>(FindObjectsSortMode.None);
            foreach (var team in teams)
            {
                if (team.isPlayerTeam)
                {
                    teamControllerPlayer = team;
                }else
                {
                    teamControllerEnemy = team;
                }
            }
        }
        
        public void SpawnSoldier(Vector3 hitPoint, Vector3 normal, float normalOffset, bool isPlayer)
        {
            if (soldierPrefab == null) 
            {
                Debug.LogError("Soldier Prefab is missing");
                 return;
            }

            //experimental
            Vector3 spawnPosition = hitPoint + (normal * 1);
            Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, normal);
            GameObject soldierGO = Instantiate(soldierPrefab, spawnPosition, spawnRotation, parent);   
            
            // GameObject soldierGO = Instantiate(soldierPrefab, parent, true);   
            // soldierGO.transform.position = new Vector3(spawnPoint.x, 1, spawnPoint.z);
            soldierGO.transform.position = hitPoint;
            soldierGO.transform.localScale = Vector3.one;
            Soldier soldier = soldierGO.GetComponent<Soldier>();
            TeamController team = isPlayer? teamControllerPlayer : teamControllerEnemy;
            AssignSoldierProperties(soldier, team, isPlayer);
            // AddSoldierToCollection(soldier);
            team.AddSoldierToTeam(soldier);
        }
        
        public void AssignSoldierProperties(Soldier soldier, TeamController team, bool isPlayer)
        {
            SoldierRole soldierRoleToAssign = team.currentRole == TeamRole.Attacking? SoldierRole.Attacker:SoldierRole.Defender;
            if (isPlayer)
            {
                soldier.SetTeamController(teamControllerPlayer);
                soldier.SetPlayerRole(soldierRoleToAssign);
            } else
            {
                soldier.SetTeamController(teamControllerEnemy);
                soldier.SetPlayerRole(soldierRoleToAssign);
            }
        }

        // public void ChangeMaterial(Soldier soldier, SoldierState state)
        // {
        //     Renderer mat = soldier._charObj.GetComponent<Renderer>();
        //     switch (state)
        //     {       
        //         case SoldierState.InActive:
        //             mat.material = _matGray;
        //         break;
                
        //         case SoldierState.Active:
        //             mat.material = soldier.curSoldierRole==SoldierRole.Attacker? _matPlayer:_matEnemy;
        //         break;

        //         case SoldierState.Chasing:
        //         break;
        //     }
        // }

        public void AddSoldierToCollection(Soldier newSoldier)
        {
            if (!soldiersCollection.Contains(newSoldier)) 
                soldiersCollection.Add(newSoldier);
        }
        
        public void RemoveSoldierFromCollection(Soldier oldSoldier)
        {
            if (soldiersCollection.Contains(oldSoldier)) 
                soldiersCollection.Remove(oldSoldier);
        }

        public List<Soldier> GetActiveDefenders()
        {
            return soldiersCollection.Where(p => p.curSoldierRole == SoldierRole.Defender && p.curSoldierState!= SoldierState.InActive).ToList();
        }
        
        public List<Soldier> GetActiveAttackers()
        {
            return soldiersCollection.Where(p => p.curSoldierRole == SoldierRole.Attacker && p.curSoldierState!= SoldierState.InActive).ToList();
        }
    }
}
