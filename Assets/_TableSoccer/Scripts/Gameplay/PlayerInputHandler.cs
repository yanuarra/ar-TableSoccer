using UnityEngine;
using UnityEngine.Events;

namespace YRA{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        public UnityEvent<Vector2> onTapEvent;

        void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }
        
        void Update()
        {
            if (Application.isMobilePlatform) 
            {
                HandleMobileInput();

            }
            else
            {
                HandlePCInput();
            }
        }
        
        void HandleMobileInput() {
             if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;
                onTapEvent?.Invoke(touchPosition);
            }
        }
        }
      
        private void HandlePCInput()
        {
            // Check for mouse click
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                onTapEvent?.Invoke(mousePosition);
            }
        }
    }
}