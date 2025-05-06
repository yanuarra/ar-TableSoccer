using UnityEngine;

namespace YRA{
    public enum SoldierRole
    {
        Attacker,
        Defender
    }

    public enum SoldierState
    {
        Active,
        Chasing,
        HoldingBall,
        Standby
    }

    public class Soldier : MonoBehaviour
    {
        [Header("Player Settings")]
        public string soldierID;
        [SerializeField] bool isPlayerTeam;
        public SoldierRole curSoldierRole { get; private set; }
        public TeamRole currentTeamRole { get; set; }
        public bool IsActive;
    
        [Header("Attacker Settings")]
        float _rotationSpeed = 10f;
        
        [Header("Defender Settings")]
        float _detectionRadius;

        // Private state variables
        private SoldierState currentState = SoldierState.Standby;
        private Ball heldBall;
        private Soldier targetAttacker;
        private Vector3 goalPosition;
        private Coroutine stunnedCoroutine;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _detectionRadius = StaticData.DETECTION_RANGE_DEF * FieldSetup.Instance.fieldWidth;
        }

        #region State Management

        public void SetPlayerRole(SoldierRole newRole)
        {
            curSoldierRole = newRole;
            
            if (newRole == SoldierRole.Defender)
            {
                SetState(SoldierState.Standby);
            }
            else
            {
                SetState(SoldierState.Active);
            }
        }
        
        private void SetState(SoldierState newState)
        {
            currentState = newState;
        }
    }
    #endregion
}