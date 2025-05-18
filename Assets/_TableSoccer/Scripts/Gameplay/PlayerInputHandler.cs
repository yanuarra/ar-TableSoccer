using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.XR.ARFoundation;

namespace YRA{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [Header("Input Settings")]
        [SerializeField] private NewInputAction _inputActions;
        [SerializeField] private PlayerInput playerInput;
        // Input actions reference
        private InputAction tapPositionAction;
        private InputAction tapPressedAction;
        private InputAction clickAction;
        private InputActionMap actionMap; // Store the Action Map
        private InputAction spawnAction; // Store the Action
        [SerializeField] private string _actionMapName = "SpawnObject";
        [SerializeField] private string _tapPositionName = "TapPosition";
        [SerializeField] private string _tapPressedName = "TapPress";
        [SerializeField] private string _clickName = "Click";
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask _raycastLayerMask = ~0; 
        [SerializeField] private TMP_Text Debugtext; 
        [SerializeField] private float _maxRaycastDistance = 100f;
        public UnityEvent<Vector2> onTapEvent;
        public Vector2 _tapPosition;
        
        void Awake()
        {
            _inputActions = new NewInputAction();
            _inputActions.Enable();
            _inputActions.SpawnObject.Enable();
            clickAction         = playerInput.actions[_clickName];
            tapPositionAction   = playerInput.actions[_tapPositionName];
            tapPressedAction    = playerInput.actions[_tapPressedName];
        }

        private void OnTapPressedPerformed(InputAction.CallbackContext context)
        {
            Vector2 _tapPosition = tapPositionAction.ReadValue<Vector2>();
            Debugtext.text = $"Tapped at position {_tapPosition}!";
            onTapEvent?.Invoke(_tapPosition);
        }
        
        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Debug.Log( $"Click at position {mousePosition}!");
            onTapEvent?.Invoke(mousePosition);
        }

        private void OnTapPositionPerformed(InputAction.CallbackContext context)
        {
            // _tapPosition = context.ReadValue<Vector2>();
        }

        private void OnEnable() {
            tapPositionAction?.Enable();
            tapPressedAction?.Enable();
            clickAction?.Enable();
            
            if (tapPressedAction != null)
                tapPressedAction.performed += OnTapPressedPerformed;
            if (tapPositionAction != null)
                tapPositionAction.performed += OnTapPositionPerformed;
            if (clickAction != null)
                clickAction.performed += OnClickPerformed;
            // _inputActions.SpawnObject.Tap.performed += OnTapPerformed;
        }

        private void OnDisable()
        {
            tapPositionAction.performed -= OnTapPositionPerformed;
            tapPressedAction.performed -= OnTapPressedPerformed;
            clickAction.performed -= OnClickPerformed;
            tapPositionAction?.Disable();
            tapPressedAction?.Disable();
            clickAction?.Disable();
            _inputActions.SpawnObject.Disable();
        }

        void Start()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }

        private void HandlePCInput()
        {
            // Check for mouse click
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                onTapEvent?.Invoke(mousePosition);
            }
        }
    }
}