using System;
using System.Collections;
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
        Standby,
        Stunned
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
        
        [Header("Movement Settings")]
        float _movementSpeed;
        float _passSpeed;
        float _reactiveTime;
    
        [Header("Attacker Settings")]
        [SerializeField] GameObject _directionIndicator;
        
        [Header("Defender Settings")]
        float _detectionRadius;
        
        [Header("Timing Settings")]
        float _stunnedDuration;

        [Header("References")]
        [SerializeField] TeamController teamController;
        [SerializeField] SphereCollider detectionCollider;
        [SerializeField] GameObject detectionVisual;
        
        public Ball heldBall {get; private set;}
        private Soldier targetAttacker;
        private Vector3 goalPosition;
        private Coroutine stunnedCoroutine;
        private Rigidbody rb;
         [SerializeField] 
        private Movement _movement;
        [SerializeField] 
        private Goal goal;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (_movement == null) _movement = GetComponent<Movement>();
        }

        public Vector3 CalculateThirdPoint(Vector3 characterPosition, Vector3 targetPosition)
        {
            // Project both points onto the XZ plane if working in 3D space
            // Remove this if you're working in 2D space
            Vector3 characterPosXZ = new Vector3(characterPosition.x, 0, characterPosition.z);
            Vector3 targetPosXZ = new Vector3(targetPosition.x, 0, targetPosition.z);
            
            // Calculate direction vector from character to target
            Vector3 directionToTarget = targetPosXZ - characterPosXZ;
            
            // Create a right angle by projecting the character position onto a line
            // that is parallel to the X-axis and passes through the target position
            
            // The third point will have the same Z coordinate as the target
            // and the same X coordinate as the character
            Vector3 thirdPoint = new Vector3(characterPosXZ.x, 0, targetPosXZ.z);
            
            // Restore the Y value (height) if needed, typically using either the character or target height
            thirdPoint.y = characterPosition.y; // You can choose targetPosition.y instead if appropriate
            
            return thirdPoint;
        }

        void Update()
        {
            // switch (curSoldierState)
            // {
            //     case SoldierState.Active:
            //         HandleActiveState();
            //         break;
            //     case SoldierState.Chasing:
            //         // HandleChasingState();
            //         break;
            //     case SoldierState.HoldingBall:
            //         // HandleHoldingBallState();
            //         break;
            //     case SoldierState.Standby:
            //         // Just waiting for detection trigger
            //         break;
            //     case SoldierState.Stunned:
            //         // Wait until stun is over
            //         break;
            // }
            // Vector3 direction = (goal.transform.position - transform.position).normalized;
            // // Vector3 goalRight = Goal.goalPost.right;
            // Vector3 perpendicular = Vector3.Cross(goal.transform.forward, Vector3.up);
            // transform.Translate(-perpendicular * 10 * Time.deltaTime);
        }

        public void OnSoldierStateChanged()
        {
            IsActive = true;
            switch (curSoldierState)
            {
                case SoldierState.Active:
                    HandleActiveState();
                    break;
                case SoldierState.Chasing:
                    // HandleChasingState();
                    break;
                case SoldierState.HoldingBall:
                    HandleHoldingBallState();
                    break;
                case SoldierState.Standby:
                    // Just waiting for detection trigger
                    break;
                case SoldierState.Stunned:
                    IsActive = false;
                    // Wait until stun is over
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
            // look for the ball
            if (curSoldierRole == SoldierRole.Attacker)
            {
                if (Ball.Instance.curHolder == null)
                {
                    Debug.Log("Chase the ball");
                    // Chase the free ball
                    Vector3 direction = (Ball.Instance.transform.position - this.transform.position).normalized;
                    SetSoldierStat();
                    Debug.Log(direction);
                    _movement.MoveDirection(gameObject, direction, _movementSpeed);
                }
                else
                {
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
            
            if (newRole == SoldierRole.Defender)
            {
                SetState(SoldierState.Standby);
            }
            else
            {
                SetState(SoldierState.Active);
            }
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
            
            if (curSoldierRole == SoldierRole.Defender)
            {
                SetState(SoldierState.Standby);
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
            if (nearestTeammate != null && nearestTeammate != this)
            {
                // Calculate direction to teammate
                Vector3 passDirection = (nearestTeammate.transform.position - transform.position).normalized;
                
                // Release the ball and make it move towards the teammate
                ReleaseBall();
                heldBall.PassBall(passDirection, _passSpeed);
                
                // Set state to stunned temporarily
                SetStunned();
            }
        }
        #endregion

        #region Stun Management
        public void SetStunned()
        {
            SetState(SoldierState.Stunned);
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
                _stunnedDuration = StaticData.REACTIVE_TIME_ATT;
            }
            else
            {
                _stunnedDuration = StaticData.REACTIVE_TIME_DEF;
            }
            yield return new WaitForSeconds(_stunnedDuration);
            SetState(SoldierState.Standby);
            stunnedCoroutine = null;
        }
        #endregion
    }
}