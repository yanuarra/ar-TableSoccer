using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

namespace YRA
{
    public class SoldierManager : Singleton<SoldierManager>
    {
        public List<Soldier> soldiersCollection = new List<Soldier>();
        TeamController teamControllerPlayer;
        TeamController teamControllerEnemy;
        [SerializeField] Transform parent;
        [SerializeField] GameObject soldierPrefab;

        [SerializeField] private Material _matGray;
        [SerializeField] private Material _matPlayer;
        [SerializeField] private Material _matEnemy;
        public void Start()
        {
            GetTeamControllers();
            GetSoldiers();
        }

        void GetSoldiers()
        {
            soldiersCollection.Clear();
            soldiersCollection = FindObjectsByType<Soldier>(FindObjectsSortMode.None).ToList();

            //dummy
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
        
        public void SpawnSoldier(Vector3 spawnPoint, bool isPlayer)
        {
            GameObject soldierGO = Instantiate(soldierPrefab, parent, true);   
            soldierGO.transform.position = new Vector3(spawnPoint.x, 1, spawnPoint.z);
            Soldier soldier = soldierGO.GetComponent<Soldier>();
            AssignSoldierTeam(soldier, isPlayer);
        }
        
        public void AssignSoldierTeam(Soldier soldier, bool isPlayer)
        {
            if (isPlayer)
            {
                soldier.SetTeamController(teamControllerPlayer);
            } else
            {
                soldier.SetTeamController(teamControllerEnemy);
            }
        }

        public void ChangeMaterial(Soldier soldier, SoldierState state)
        {
            Renderer mat = soldier._charObj.GetComponent<Renderer>();
            switch (state)
            {       
                case SoldierState.InActive:
                    mat.material = _matGray;
                break;
                
                case SoldierState.Active:
                    mat.material = soldier.isPlayerTeam? _matPlayer:_matEnemy;
                break;

                case SoldierState.Chasing:
                break;
            }

        }
        

        public void AddSoldier(Soldier newSoldier)
        {
            if (!soldiersCollection.Contains(newSoldier)) 
                soldiersCollection.Add(newSoldier);
        }
        
        public void RemoveSoldier(Soldier oldSoldier)
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
