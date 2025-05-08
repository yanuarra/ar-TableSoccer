using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YRA{
    public class InputTapAndHold : MonoBehaviour
    {
        GameObject spawnedObject;
        Vector2 touchPosition;
        private event Action TrackInput = delegate { };
        private event Action TapEvent = delegate { };
        private event Action HoldEvent = delegate { };
        private event Action ReleaseEvent = delegate { };

        void Start()
        {
        }
        
        void Update()
        {
            if (Application.isMobilePlatform) 
            {
                TrackInput = MobileDefaultInput;
            }
            else
            {
                // TrackInput = EditorDefaultInput;
                EditorDefaultInput();
            }
        }
        
        void MobileDefaultInput() {
            Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    touchPosition = touch.position;
        }

        void EditorDefaultInput() {
            if (Input.GetKeyDown(KeyCode.Space) ) {
                Debug.Log("Place");
        }

    }
    }
}