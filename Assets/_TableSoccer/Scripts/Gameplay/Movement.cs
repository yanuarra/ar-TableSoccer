using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace YRA
{
    public class Movement : MonoBehaviour
    {
        public enum MovementMode
        {
            InActive,
            Idle,
            MoveToPosition,
            MoveInDirection,
            MoveFollowTarget,
        }
        public MovementMode currentMode = MovementMode.InActive;
        private  bool _isMoving = false;
        // private  GameObject _origin;
        private  GameObject _target;
        [SerializeField] 
        private  GameObject _moveIndicator;
        // private  Vector3 _direction;
        // private  float _moveSpeed;
        private  float _rotationSpeed = 180.0f;
        private float arrivalDistance = 0.01f;
        private  bool _faceMovementDirection = true;
        
        [SerializeField] 
        private bool _lockXRotation = false;
        [SerializeField] 
        private bool _lockYRotation = true;
        [SerializeField] 
        private bool _lockZRotation = false;

        // Target information
        private Vector3 _targetPosition;
        private Vector3 _moveDirection;
        private Transform _targetObject;
        private float _currentSpeed;
        private float _multiplier = 1f;
        private Rigidbody rb;
        [SerializeField] private bool useRigidbody = false;
        /// <summary>
        /// Check if running AR or not
        /// </summary>
        ARSupportChecker ar;
        Action doneEvent;
        
        private void Awake()
        {
        }

        void OnEnable()
        {
            ar = FindAnyObjectByType<ARSupportChecker>();
            if (ar!=null) 
                _multiplier = ar.isARAvailable()? .01f:1f;
        
            rb= GetComponent<Rigidbody>();
        }

        void Update()
        {
            switch (currentMode)
            {
                case MovementMode.InActive:
                    break;

                case MovementMode.Idle:
                    HandleIdle();
                    break;
                    
                case MovementMode.MoveToPosition:
                    HandleMoveToPosition();
                    break;
                    
                case MovementMode.MoveInDirection:
                    HandleMoveInDirection();
                    break;
                    
                case MovementMode.MoveFollowTarget:
                    HandleMoveFollowObject();
                    break;
            }

            if (!useRigidbody && currentMode != MovementMode.InActive)
            {
                ApplyMovement();
             
                if (hasReachedTarget() && doneEvent != null)
                {
                    doneEvent?.Invoke();
                    doneEvent = null;
                }
            }
        }

        private bool hasReachedTarget()
        {
            Vector3 directionToTarget = _targetPosition - transform.position;
            float distanceToTarget = directionToTarget.magnitude;
            Debug.Log($"{this.name} _targetPosition {_targetPosition} distanceToTarget {distanceToTarget}");

            if (distanceToTarget <= arrivalDistance)
            {
                StopMoving();
                return true;
            }
            return false;
        }

        private void FixedUpdate()
        {
            if (useRigidbody)
            {
                ApplyRigidbodyMovement();
            }
        }

        #region Public Control Methods
        public void MoveToPosition(Vector3 position, float speed = -1, Action done = null)
        {
            currentMode = MovementMode.MoveToPosition;
            _targetPosition = position;
            _currentSpeed = speed;
            if (done!=null) doneEvent = done;
            ToggleMoveIndicator(true);
        }

        public void MoveInDirection(Vector3 direction, float speed = -1)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                currentMode = MovementMode.MoveInDirection;
                _moveDirection = direction.normalized;
                _currentSpeed = speed;
            }
            ToggleMoveIndicator(true);
        }

        public void MoveFollowObject(Transform target, float speed = -1)
        {
            if (target != null)
            {
                currentMode = MovementMode.MoveFollowTarget;
                _targetObject = target;
                _currentSpeed = speed;
            }
            ToggleMoveIndicator(true);
        }
        #endregion

        #region Movement Mode Handlers
        private void HandleIdle()
        {
            _currentSpeed = 0;
            // _moveSpeed = 0;
            _moveDirection = Vector3.zero;
            ToggleMoveIndicator(false);
        }
        
        private void HandleMoveToPosition()
        {
            Vector3 directionToTarget = _targetPosition - transform.position;
            _moveDirection = directionToTarget.normalized;
            float distanceToTarget = directionToTarget.magnitude;
            // _currentSpeed = _moveSpeed;
            if (distanceToTarget <= arrivalDistance)
            {
                // _currentSpeed = 0;
                // currentMode = MovementMode.Idle;
                StopMoving();
                return;
            }
            if (_faceMovementDirection)
            {
                RotateTowards(_targetPosition);
            }
            
        }
        
        private void HandleMoveInDirection()
        {
            // _currentSpeed = _moveSpeed;
            // UpdateSpeed();
            if (_faceMovementDirection && _moveDirection.sqrMagnitude > 0.01f)
            {
                RotateTowards(transform.position + _moveDirection);
            }
        }
        
        private void HandleMoveFollowObject()
        {
            if (_targetObject == null)
            {
                currentMode = MovementMode.Idle;
                return;
            }
            Vector3 directionToTarget = _targetObject.position - transform.position;
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget <= arrivalDistance)
            {
                _currentSpeed = 0;
                _moveDirection = Vector3.zero;
                if (_faceMovementDirection)
                {
                    RotateTowards(_targetObject.position);
                }
            }
            else
            {
                _moveDirection = directionToTarget.normalized;
                // _currentSpeed = _moveSpeed;
                if (_faceMovementDirection)
                {
                    RotateTowards(_targetObject.position);
                }
            }
            
        }
        #endregion

        #region Movement Application
        private void ApplyMovement()
        {
            _multiplier = ar.isARAvailable()? .01f:1f;
            if (_moveDirection.sqrMagnitude > 0.01f && _currentSpeed > 0.01f)
                transform.position += _moveDirection * _currentSpeed * Time.deltaTime * _multiplier;
        
            // transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }  
    
        private void ApplyRigidbodyMovement()
        {
            // Physics-based movement
            if (_moveDirection.sqrMagnitude > 0.01f && _currentSpeed > 0.01f)
            {
                Vector3 velocity = _moveDirection * _currentSpeed;
                if (rb.useGravity)
                {
                    velocity.y = rb.linearVelocity.y;
                }
                rb.linearVelocity = velocity;
            }
            else if (_currentSpeed < 0.01f)
            {
                // Preserve y velocity for gravity
                if (rb.useGravity)
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                }
                else
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }
        }
    
        private void RotateTowards(Vector3 targetPos)
        {
            // Vector3 lookatRot = Quaternion.LookRotation(targetPos, Vector3.up).eulerAngles;
            // transform.rotation = Quaternion.Euler(Vector3.Scale(lookatRot, _rotationMask));
   
            Vector3 targetDirection = targetPos - transform.position;
            if (targetDirection.sqrMagnitude < 0.001f)
                return;
            if (_lockXRotation) targetDirection.x = 0;
            if (_lockYRotation) targetDirection.y = 0;
            if (_lockZRotation) targetDirection.z = 0;
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, FieldSetup.Instance.GetFieldsUpwards());
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        public void UpdateSpeed(float newValue)
        {
            _currentSpeed = newValue;
        }
        #endregion

        public void ToggleMoveIndicator(bool state) {
            if (_moveIndicator==null) return;
            _moveIndicator.transform.localPosition = new Vector3 (0,-0.75f,0f);
            _moveIndicator.SetActive(state);
        }     

        public void StopMoving() {
            currentMode = MovementMode.Idle;
            _moveDirection = Vector3.zero;
            _currentSpeed = 0;
            _isMoving = false;
            _target = null;
            ToggleMoveIndicator(false);
        }
    }
}
