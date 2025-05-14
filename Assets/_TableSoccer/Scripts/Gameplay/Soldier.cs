using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField]  
        Outline _outline;
        private float _spawnTime;
        
        [Header("Movement Settings")]
        private float _movementSpeed;
        private float _passSpeed;
        [Header("Timing Settings")]
        private float _reactiveTime;
    
        [Header("Character Settings")]
        [SerializeField] Animator _charAnimator;
        [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] GameObject _detectionVisual;
        [SerializeField] GameObject _fenceVisual;
        [SerializeField] GameObject _inactiveVisual;


        [Header("Defender Settings")]
        private float _detectionRadius;

        [Header("References")]
        public TeamController teamController{ get; private set; }
        [SerializeField] SphereCollider detectionCollider;
        public Ball heldBall {get; private set;}
        private Vector3 _originPosition;
        private Coroutine _stunnedCoroutine;
        [SerializeField] private Movement _movement;
        [SerializeField] private Goal _goal;
        private float _distanceToBall;

        [Header("Audio SFX")]
        [SerializeField] AudioClip _getBallSFX;
        [SerializeField] AudioClip _getBallHolderSFX;
        [SerializeField] AudioClip _passBallSFX;
        [SerializeField] AudioClip _portalSFX;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_movement == null) _movement = GetComponent<Movement>();
            if (_outline == null) _outline = GetComponent<Outline>();
            if (_charAnimator == null) _charAnimator = GetComponent<Animator>();
            if (_skinnedMeshRenderer == null) _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            // keep position
            _originPosition = transform.position;   
            _fenceVisual.SetActive(false);
            _inactiveVisual.SetActive(false);
            // ToggleOutline(false);
            // ToggleDetectionVisual(false);
        }

        public void StopMoving() => _movement.StopMoving();
        public void PlayVictoryAnimation() => _charAnimator.SetTrigger("Victory");
        public void PlayIdleAnimation() => _charAnimator.SetTrigger("Idle");

        void ToggleOutline(bool state)
        {
            if (_outline != null) _outline.enabled = state;
        }

        void ToggleDetectionVisual(bool state)
        {
            if (_detectionVisual != null) _detectionVisual.SetActive ( state);
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
            _distanceToBall = Vector3.Distance(transform.position, Ball.Instance.curHolder.transform.position);
            if ( _distanceToBall < _detectionRadius) 
                SetState(SoldierState.Chasing);
        }

        void OnDrawGizmos()
        {
            if (curSoldierRole != SoldierRole.Defender && curSoldierState != SoldierState.Active ) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
        
        public void ChangeMaterial()
        {
            // Material[] mat = _skinnedMeshRenderer.sharedMaterials;
            Material matBody = _skinnedMeshRenderer.sharedMaterials[0]; 
            Material matHead = _skinnedMeshRenderer.sharedMaterials[1]; 
            Material[] mats = new Material[]{matBody, matHead};; 
            switch (curSoldierState)
            {       
                case SoldierState.InActive:
                    // mat[0].SetColor("_Color", Color.gray);
                    mats = new Material[]{SoldierManager.Instance._matGray, matHead};
                break;
                case SoldierState.Active:
                    // mat[0].SetColor("_Color", curSoldierRole == SoldierRole.Attacker? Color.cyan:Color.red);
                    // matsmat[0] = curSoldierRole == SoldierRole.Attacker? SoldierManager.Instance._matEnemy:SoldierManager.Instance._matPlayer ;
                    mats = new Material[]{curSoldierRole == SoldierRole.Attacker? SoldierManager.Instance._matEnemy:SoldierManager.Instance._matPlayer, matHead};
                break;
                case SoldierState.Chasing:
                break;
            }
            _skinnedMeshRenderer.materials = mats; 
        }

        IEnumerator WaitForSeconds (Action doneEvent, float duration)
        {
            yield return new WaitForSeconds(duration);
            doneEvent?.Invoke();
        }

        #region Collision Handlers

        void RemoveSoldierFromField()
        {
            _movement.StopMoving();
            _charAnimator.SetTrigger("Victory");
            _fenceVisual.SetActive(true);
            if (_portalSFX!=null)
                    AudioHandler.Instance.PlayAudioSfx(_portalSFX);
             StartCoroutine(WaitForSeconds
                (delegate {
                    teamController.RemoveSoldierFromTeam(this);
                    Destroy(gameObject);
                    _fenceVisual.SetActive(false);
                },
                1.5f)
            );
        }

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
            if (other.TryGetComponent(out Goal goal)){
                if (curSoldierRole == SoldierRole.Attacker && heldBall == null)
                    RemoveSoldierFromField();
            }
            
            //Check Attacker and Fence Collision 
            if (other.TryGetComponent(out Fence fence))
            {
                RemoveSoldierFromField();
            }

            //Check Defender and Attacker Collision 
            if (other.TryGetComponent(out Soldier otherSoldier))
            {
                if (curSoldierRole == SoldierRole.Defender && 
                    otherSoldier.heldBall != null)
                {
                    if (_getBallHolderSFX!=null)
                        AudioHandler.Instance.PlayAudioSfx(_getBallHolderSFX);
                    otherSoldier.PassBallToTeammate();
                    SetState(SoldierState.InActive); 
                    teamController.UpdateSoldiersState(this, SoldierState.Active);
                }
            }

            //Check Ball Collision
            if (other.TryGetComponent(out Ball ball))
            {
                // if (ball.curHolder!=null) return;
                if (curSoldierRole == SoldierRole.Attacker && curSoldierState == SoldierState.Chasing)
                {
                    _charAnimator.SetTrigger("Pickup");
                    HoldBall(ball);
                }
            }
        }
        #endregion

        public void OnSoldierStateChanged()
        {
            IsActive = true;
            ToggleOutline(false);
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
            // SoldierManager.Instance.ChangeMaterial(this, curSoldierState);
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
            ChangeMaterial();
            if (curSoldierRole == SoldierRole.Attacker)
            {
                if (Ball.Instance.curHolder == null)
                {
                    SetState(SoldierState.Chasing);
                }
                else
                {
                    _charAnimator.SetTrigger("Running");
                    Debug.Log(gameObject.name + " State:Straight");
                    // If no Ball to chase or hold, go straight into the opponent Land Field
                    // Vector3 originPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 originPosXZ = transform.position;
                    // Vector3 targetPosXZ = new Vector3(_goal.transform.position.x, 0, _goal.transform.position.z);
                    Vector3 targetPosXZ = _goal.transform.position;
                    Vector3 thirdPoint = new Vector3(originPosXZ.x, targetPosXZ.y, targetPosXZ.z);
                    thirdPoint.y = originPosXZ.y;
                    Vector3 direction = thirdPoint - transform.position;
                    SetSoldierStat();
                    _movement.MoveInDirection(direction, _movementSpeed);
                }
            }
            else
            {
                _charAnimator.SetTrigger("Idle");
            }
            // Defender Standby
        }
        
        private void HandleChasingState()
        {
            SetSoldierStat();
            _charAnimator.SetTrigger("Running");
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
            ChangeMaterial();
            if (_goal == null)
                _goal = teamController.goal;
            _charAnimator.SetTrigger("Waving");
            ToggleDetectionVisual(curSoldierRole == SoldierRole.Defender);
            StartCoroutine(StunnedRoutine(_spawnTime));
        }

        private void HandleInactiveState()
        {
            SetSoldierStat();
            ChangeMaterial();
            _charAnimator.SetTrigger("Idle");
            _inactiveVisual.gameObject.SetActive(true);
            if (curSoldierRole == SoldierRole.Defender)
            {
                StartCoroutine(GetBackToOriginPos());
            }else
            {
                StartCoroutine(StunnedRoutine(_reactiveTime));
            }
        }
        //Defender Get Back To Origin Pos
        IEnumerator GetBackToOriginPos()
        {
            if (curSoldierRole == SoldierRole.Defender)
            {
                _charAnimator.SetTrigger("Running");
                _movement.MoveToPosition(_originPosition, _movementSpeed);
            }
            yield return new WaitForSeconds(_reactiveTime);
            SetState(SoldierState.Active);
        }

        IEnumerator StunnedRoutine(float duration)
        {
            Debug.Log($"{this.name} is stunned for {duration}" );
            _movement.StopMoving();
            yield return new WaitForSeconds(duration);
            SetState(SoldierState.Active);
        }

        private void HandleHoldingBallState()
        {
            Debug.Log("Chase the goal");
            ToggleOutline(true);
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
            if (_getBallSFX!=null)
                AudioHandler.Instance.PlayAudioSfx(_getBallSFX);
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
                // Vector3 passDirection = (nearestTeammate.transform.position - transform.position).normalized;
                // Release the ball and make it move towards the teammate
                nearestTeammate.SetState(SoldierState.Chasing);
                heldBall.PassBall(nearestTeammate.transform, _passSpeed);
                ReleaseBall();
                if (_passBallSFX!=null)
                    AudioHandler.Instance.PlayAudioSfx(_passBallSFX);
            }
            else
            {
                //Defender WIN
                GameManager.Instance.ScorePoint(teamController);
            }
            SetState(SoldierState.InActive);
        }
        #endregion
    }
}