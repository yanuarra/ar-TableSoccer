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
        [field: SerializeField]
        public GameObject _charObj  { get; private set; }
        float _spawnTime;
        
        [Header("Movement Settings")]
        float _movementSpeed;
        float _passSpeed;
        [Header("Timing Settings")]
        float _reactiveTime;
    
        [Header("Attacker Settings")]
        
        [Header("Defender Settings")]
        float _detectionRadius;

        [Header("References")]
        public TeamController teamController{ get; private set; }
        [SerializeField] SphereCollider detectionCollider;
        [SerializeField] GameObject detectionVisual;
        public Ball heldBall {get; private set;}
        private Soldier _targetAttacker;
        private Vector3 _goalPosition;
        private Vector3 _originPosition;
        private Coroutine _stunnedCoroutine;
        private Rigidbody _rb;
        [SerializeField] private Movement _movement;
        [SerializeField] private Goal _goal;
        float _distanceToBall;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_movement == null) _movement = GetComponent<Movement>();
            // isPlayerTeam = teamController.isPlayerTeam;
            // keep position
            _originPosition = transform.position;   
        }

        public void SetTeamController(TeamController team)
        {
            teamController = team;
            isPlayerTeam = teamController.isPlayerTeam;
            Debug.Log($"isPlayerTeam {teamController.isPlayerTeam}");
        }

        void Update()
        {
            if (curSoldierRole==SoldierRole.Defender && 
                curSoldierState == SoldierState.Active &&
                Ball.Instance.curHolder != null)
                DoDetection();
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
        void OnTriggerStay(Collider other) {
            if (other.TryGetComponent(out Ball ball))
            {
                if (ball.curHolder == null &&
                    curSoldierRole == SoldierRole.Attacker && 
                    curSoldierState == SoldierState.Chasing)
                {
                    HoldBall(ball);
                }
            }
            
        }

        void OnTriggerEnter(Collider other)
        {
            if (curSoldierState == SoldierState.InActive) return;
            
            //Check Attacker and Fence Collision 
            if (other.TryGetComponent(out Fence fence))
            {
                teamController.RemoveSoldierFromTeam(this);
                Destroy(gameObject);
            }

            //Check Defender and Attacker Collision 
            if (other.TryGetComponent(out Soldier otherSoldier))
            {
                if (curSoldierRole == SoldierRole.Defender && 
                    otherSoldier.heldBall != null)
                {
                    SetState(SoldierState.InActive);
                    teamController.UpdateSoldiersState(this, SoldierState.Active);
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
                    HandleSpawningState();
                    break;
                case SoldierState.InActive:
                    IsActive = false;
                    // Wait until stun is over
                    HandleInactiveState();
                    break;
            }
            SoldierManager.Instance.ChangeMaterial(this, curSoldierState);
        }
        
        #region State Handlers
        private void HandleActiveState()
        {
            if (Ball.Instance == null)
            {
                Debug.LogError("Ball is missing");
                return;
            }
            
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
                    Vector3 targetPosXZ = new Vector3(_goal.transform.position.x, 0, _goal.transform.position.z);
                    Vector3 thirdPoint = new Vector3(originPosXZ.x, 0, targetPosXZ.z);
                    thirdPoint.y = originPosXZ.y;
                    Vector3 direction = thirdPoint - transform.position;
                    SetSoldierStat();
                    _movement.MoveInDirection(direction, _movementSpeed);
                }
            }
            // Defender Standby
        }
        
        private void HandleChasingState()
        {
            SetSoldierStat();
            Debug.Log($"{curSoldierRole} is chasing the ball");
            if (curSoldierRole == SoldierRole.Attacker)
            {
                _movement.MoveToPosition(Ball.Instance.transform.position, _movementSpeed);
            }
            else
            {
                _movement.MoveFollowObject(Ball.Instance.transform, _movementSpeed);
            }
        }

        private void HandleSpawningState()
        {
            SetSoldierStat();
            // if (teamController == null)
            //     teamController = teamController.;
            if (_goal == null)
                _goal = teamController.goal;
            StartCoroutine(StunnedRoutine(_spawnTime));
        }

        private void HandleInactiveState()
        {
            SetSoldierStat();
            StartCoroutine(StunnedRoutine(_reactiveTime));
        }

        IEnumerator StunnedRoutine(float duration)
        {
            Debug.Log($"{this.name} is stunned for {duration}" );
            _movement.StopMoving();
            yield return new WaitForEndOfFrame();
            if (curSoldierRole == SoldierRole.Defender)
                _movement.MoveToPosition(_originPosition, _movementSpeed);
            yield return new WaitForSeconds(duration);
            SetState(SoldierState.Active);
        }

        private void HandleHoldingBallState()
        {
            Debug.Log("Chase the goal");
            SetSoldierStat();
            // Vector3 direction = (_goal.transform.position - transform.position).normalized;
            Vector3 direction = _goal.transform.position - transform.position;
            // _movement.MoveToDirection(gameObject, direction, _movementSpeed);
            _movement.MoveInDirection(direction, _movementSpeed);
        }
        #endregion

        #region State Management
        void SetSoldierStat()
        {
            _spawnTime = StaticData.SPAWN_TIME;
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
            SetState(SoldierState.Spawning);
        }
        
        public void SetState(SoldierState newState)
        {
            curSoldierState = newState;
            OnSoldierStateChanged();
        }

        public void ResetState()
        {
            if (_stunnedCoroutine != null)
            {
                StopCoroutine(_stunnedCoroutine);
                _stunnedCoroutine = null;
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
        
        public void PassBallToTeammate()
        {
            if (heldBall == null) return;
            Soldier nearestTeammate = teamController.GetNearestActiveAttacker(this);
            if (nearestTeammate != null && nearestTeammate != this)
            {
                // Calculate direction to teammate
                // Vector3 passDirection = (nearestTeammate.transform.position - transform.position).normalized;
                // Release the ball and make it move towards the teammate
                nearestTeammate.SetState(SoldierState.Chasing);
                heldBall.PassBall(nearestTeammate.transform, _passSpeed);
                // Set state to inactive temporarily
                ReleaseBall();
                SetState(SoldierState.InActive);
            }
            else
            {
                //Defender WIN
                GameManager.Instance.ScorePoint(teamController);
            }
        }
        #endregion
    }
}