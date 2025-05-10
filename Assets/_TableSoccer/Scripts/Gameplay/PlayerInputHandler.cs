using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YRA{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private GameObject objectToSpawn;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask planeLayerMask;

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
                SpawnObjectAtPosition(touchPosition);
            }
        }
        }
      
        private void HandlePCInput()
        {
            // Check for mouse click
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                SpawnObjectAtPosition(mousePosition);
            }
        }

        private void SpawnObjectAtPosition(Vector2 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, planeLayerMask))
            {
                // Spawn the object at hit position
                Debug.Log("Object spawned at: " + hit.point);
                if (objectToSpawn!= null)
                    Instantiate(objectToSpawn, hit.point, Quaternion.identity);
            }
        }
    }
}