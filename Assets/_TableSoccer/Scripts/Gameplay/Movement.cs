using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace YRA
{
    public class Movement : MonoBehaviour
    {
        public enum MovementMode
        {
            Idle,
            MoveToPosition,
            MoveInDirection,
            MoveFollowTarget,
        }
        public MovementMode currentMode = MovementMode.Idle;
        private  bool _isMoving = false;
        // private  GameObject _origin;
        private  GameObject _target;
        [SerializeField] 
        private  GameObject _moveIndicator;
        // private  Vector3 _direction;
        private  Vector3 _rotationMask = new Vector3(0, 1, 1);
        private  float _moveSpeed;
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
        private Rigidbody rb;
        private bool useRigidbody = false;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }        
        
        void Update()
        {
            switch (currentMode)
            {
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
        
            if (!useRigidbody)
            {
                ApplyMovement();
            }
            
        }

        #region Public Control Methods
        public void MoveToPosition(Vector3 position, float speed = -1)
        {
            currentMode = MovementMode.MoveToPosition;
            _targetPosition = position;
            _moveSpeed = speed;
            ToggleMoveIndicator(true);
        }
        
        public void MoveInDirection(Vector3 direction, float speed = -1)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                currentMode = MovementMode.MoveInDirection;
                _moveDirection = direction.normalized;
                _moveSpeed = speed;
            }
            ToggleMoveIndicator(true);
        }

        public void MoveFollowObject(Transform target, float speed = -1)
        {
            if (target != null)
            {
                currentMode = MovementMode.MoveFollowTarget;
                _targetObject = target;
                _moveSpeed = speed;
            }
            ToggleMoveIndicator(true);
        }
        #endregion

        #region Movement Mode Handlers
        private void HandleIdle()
        {
            _currentSpeed = 0;
            _moveSpeed = 0;
            _moveDirection = Vector3.zero;
            ToggleMoveIndicator(false);
        }
        
        private void HandleMoveToPosition()
        {
            Vector3 directionToTarget = _targetPosition - transform.position;
            _moveDirection = directionToTarget.normalized;
            float distanceToTarget = directionToTarget.magnitude;
            _currentSpeed = _moveSpeed;
            if (distanceToTarget <= 0.01f)
            {
                _currentSpeed = 0;
                currentMode = MovementMode.Idle;
                return;
            }
            if (_faceMovementDirection)
            {
                RotateTowards(_targetPosition);
            }
            
            UpdateSpeed();
        }
        
        private void HandleMoveInDirection()
        {
            _currentSpeed = _moveSpeed;
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
                _currentSpeed = _moveSpeed;
                if (_faceMovementDirection)
                {
                    RotateTowards(_targetObject.position);
                }
            }
            
            UpdateSpeed();
        }
        #endregion

        #region Movement Application
        private void ApplyMovement()
        {
            if (_moveDirection.sqrMagnitude > 0.01f && _currentSpeed > 0.01f)
                transform.position += _moveDirection * _currentSpeed * Time.deltaTime;
        
            // transform.position = Vector3.MoveTowards(transform.position, _moveDirection, _moveSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, 1, transform.position.z);
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
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    _rotationSpeed * Time.deltaTime
                );
            }
        }
        
        private void UpdateSpeed()
        {
        }
        #endregion

        public void ToggleMoveIndicator(bool state) {
            if (_moveIndicator==null) return;
            _moveIndicator.transform.localPosition = new Vector3 (0,-0.75f,1.75f);
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
