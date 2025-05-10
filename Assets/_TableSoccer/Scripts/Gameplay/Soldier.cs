using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        Spawning,
        InActive
    }

    public class Soldier : MonoBehaviour
    {
        [Header("Player Settings")]
        public string soldierID;
        public bool isPlayerTeam { get; private set; }
        [field: SerializeField]
        public SoldierRole curSoldierRole { get; private set; }
        public SoldierState curSoldierState { get; private set; }
        public TeamRole currentTeamRole { get; set; }
        public bool IsActive;
        [SerializeField] private Material _matGray;
        [SerializeField] private Material _matPlayer;
        [SerializeField] private Material _matEnemy;
        
        [Header("Movement Settings")]
        float _movementSpeed;
        float _passSpeed;
        float _reactiveTime;
    
        [Header("Attacker Settings")]
        
        [Header("Defender Settings")]
        float _detectionRadius;
        
        [Header("Timing Settings")]
        float _reactiveDuration;

        [Header("References")]
        [SerializeField] TeamController teamController;
        [SerializeField] SphereCollider detectionCollider;
        [SerializeField] GameObject detectionVisual;
        public Ball heldBall {get; private set;}
        private Soldier targetAttacker;
        private Vector3 goalPosition;
        private Coroutine stunnedCoroutine;
        private Rigidbody rb;
        [SerializeField] private Movement _movement;
        [SerializeField] private Goal goal;
        float _distanceToBall;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (_movement == null) _movement = GetComponent<Movement>();
        }

        void Update()
        {
            if (curSoldierRole==SoldierRole.Defender && 
                curSoldierState == SoldierState.Active &&
                Ball.Instance.curHolder != null)
                DoDetection();
        }

        public void SetTeamController(TeamController team)
        {
            teamController = team;
            isPlayerTeam = teamController.isPlayerTeam;
        }

        void DoDetection()
        {
            _distanceToBall = Vector3.Distance(this.transform.position, Ball.Instance.curHolder.transform.position);
            if ( _distanceToBall < _detectionRadius) 
                SetState(SoldierState.Chasing);
        }

        void OnDrawGizmos()
        {
            if (curSoldierRole != SoldierRole.Defender && curSoldierState != SoldierState.Active ) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }

        #region Collision Handlers
        void OnTriggerEnter(Collider other)
        {
            //Check Defender and Attacker Collision 
            if (other.TryGetComponent(out Soldier otherSoldier))
            {
                if (curSoldierRole == SoldierRole.Defender && 
                    otherSoldier.heldBall != null)
                {
                    SetState(SoldierState.InActive);
                    otherSoldier.PassBallToTeammate();
                    // otherSoldier.SetState(SoldierState.InActive);
                }
            }

            //Check Ball Collision
            if (other.TryGetComponent(out Ball ball))
            {
                // if (ball.curHolder!=null) return;
                if (curSoldierRole == SoldierRole.Attacker && curSoldierState == SoldierState.Chasing)
                {

                    HoldBall(ball);
                }
            }
        }
        #endregion

        public void OnSoldierStateChanged()
        {
            IsActive = true;
            switch (curSoldierState)
            {
                case SoldierState.Active:
                    HandleActiveState();
                    break;
                case SoldierState.Chasing:
                    HandleChasingState();
                    break;
                case SoldierState.HoldingBall:
                    HandleHoldingBallState();
                    break;
                case SoldierState.Spawning:
                    // Just waiting for detection trigger
                    break;
                case SoldierState.InActive:
                    IsActive = false;
                    // Wait until stun is over
                    HandleInactiveState();
                    break;
            }
        }
        
        #region State Handlers
        private void HandleActiveState()
        {
            if (Ball.Instance == null)
            {
                Debug.LogError("Ball is missing");
                return;
            }
            // look for the goal
            if (goal == null)
                goal = FindObjectsByType<Goal>(FindObjectsSortMode.None).Where(x => x.isPlayerGoal != teamController.isPlayerTeam).FirstOrDefault();
                    
            // Attacker look for the ball
            SetSoldierStat();
            if (curSoldierRole == SoldierRole.Attacker)
            {
                if (Ball.Instance.curHolder == null)
                {
                    SetState(SoldierState.Chasing);
                }
                else
                {
                    Debug.Log(gameObject.name + " State:Straight");
                    // If no Ball to chase or hold, go straight into the opponent Land Field
                    Vector3 originPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 targetPosXZ = new Vector3(goal.transform.position.x, 0, goal.transform.position.z);
                    Vector3 thirdPoint = new Vector3(originPosXZ.x, 0, targetPosXZ.z);
                    thirdPoint.y = originPosXZ.y;
                    Vector3 direction = (thirdPoint-transform.position).normalized;
                    SetSoldierStat();
                    _movement.MoveDirection(gameObject, direction, _movementSpeed);
                }
            }
            // Defender Standby
        }
        
        private void HandleChasingState()
        {
            SetSoldierStat();
            Debug.Log($"{curSoldierRole} is chasing the ball");
            Vector3 direction = (Ball.Instance.transform.position - transform.position).normalized;
            _movement.MoveDirection(gameObject, direction, _movementSpeed);
            // if (curSoldierRole == SoldierRole.Attacker)
            // {
            //     Vector3 direction = (Ball.Instance.transform.position - transform.position).normalized;
            //     _movement.MoveDirection(gameObject, direction, _movementSpeed);
            // }
            // else
            // {
            //     Debug.Log("Chase the attacker that is holding the ball");
            //     Vector3 direction = (Ball.Instance.transform.position - transform.position).normalized;
            //     _movement.MoveDirection(gameObject, direction, _movementSpeed);
            // }
        }

        private void HandleInactiveState()
        {
            Debug.Log("Stunned");
            SetSoldierStat();
            StartCoroutine(StunnedRoutine(_reactiveTime));
        }

        IEnumerator StunnedRoutine(float duration)
        {
            _movement.StopMoving();
            yield return new WaitForSeconds(duration);
            SetState(SoldierState.Active);
        }

        private void HandleHoldingBallState()
        {
            Debug.Log("Chase the goal");
            SetSoldierStat();
            Vector3 direction = (goal.transform.position - transform.position).normalized;
            _movement.MoveDirection(gameObject, direction, _movementSpeed);
        }
        #endregion

        #region State Management
        void SetSoldierStat()
        {
            if (curSoldierRole == SoldierRole.Attacker)
            {
                _movementSpeed = heldBall == null? StaticData.NORMAL_SPEED_ATT:StaticData.CARRY_SPEED_ATT;
                _passSpeed = StaticData.PASS_SPEED_ATT;
                _reactiveTime = StaticData.REACTIVE_TIME_ATT;
                _detectionRadius = 0;
            }else
            {
                _movementSpeed = StaticData.NORMAL_SPEED_DEF; //Same as return speed
                _passSpeed  = 0;
                _reactiveTime = StaticData.REACTIVE_TIME_DEF;
                _detectionRadius = StaticData.DETECTION_RANGE_DEF * FieldSetup.Instance.fieldWidth;
            }
        }

        public void SetPlayerRole(SoldierRole newRole)
        {
            curSoldierRole = newRole;
            SetState(SoldierState.Active);
        }
        
        public void SetState(SoldierState newState)
        {
            curSoldierState = newState;
            OnSoldierStateChanged();
        }

        public void ResetState()
        {
            if (stunnedCoroutine != null)
            {
                StopCoroutine(stunnedCoroutine);
                stunnedCoroutine = null;
            }
            
            ReleaseBall();
            
            if (curSoldierRole == SoldierRole.Attacker)
            {
                SetState(SoldierState.InActive);
            }
            else
            {
                // SetState(SoldierState.Active);
            }
            // UpdateDetectionColliderState();
        }
        #endregion

        #region Ball Handling
        public void HoldBall(Ball ball)
        {
            heldBall = ball;
            heldBall.SetHolder(this);
            SetState(SoldierState.HoldingBall);
            teamController.UpdateSoldiersState (this, SoldierState.Active);
        }
        
        public void ReleaseBall()
        {
            if (heldBall != null)
            {
                heldBall.Release();
                heldBall = null;
            }
        }
        
        private void PassBallToTeammate()
        {
            if (heldBall == null) return;
            Soldier nearestTeammate = teamController.GetNearestActiveAttacker(transform.position);
            Debug.Log(nearestTeammate);
            if (nearestTeammate != null && nearestTeammate != this)
            {
                // Calculate direction to teammate
                Vector3 passDirection = (nearestTeammate.transform.position - transform.position).normalized;
                // Release the ball and make it move towards the teammate
                heldBall.PassBall(passDirection, _passSpeed);
                ReleaseBall();
                // Set state to inactive temporarily
                SetIncative();
            }
            else
            {
                
            }
        }
        #endregion

        #region Inactive Handling
        public void SetIncative()
        {
            SetState(SoldierState.InActive);
            if (stunnedCoroutine != null)
            {
                StopCoroutine(stunnedCoroutine);
            }
            stunnedCoroutine = StartCoroutine(StunnedCoroutine());
        }
        
        private IEnumerator StunnedCoroutine()
        {
            if (curSoldierRole == SoldierRole.Attacker)
            {
                _reactiveDuration = StaticData.REACTIVE_TIME_ATT;
            }
            else
            {
                _reactiveDuration = StaticData.REACTIVE_TIME_DEF;
            }
            yield return new WaitForSeconds(_reactiveDuration);
            SetState(SoldierState.Active);
            stunnedCoroutine = null;
        }
        #endregion
    }
}