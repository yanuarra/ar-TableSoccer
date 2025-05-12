using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementWithShadows : MonoBehaviour
{
    [SerializeField]
    private GameObject placedPrefab; // The prefab to place on detected planes

    [SerializeField]
    private Camera arCamera; // AR Camera reference

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placedObject; // Reference to the placed object

    // Indicator for plane detection
    [SerializeField]
    private GameObject planeDetectionIndicator;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        // Ensure we have the AR camera reference
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }

        void Start()
    {
        // Initially, make sure the indicator is active
        if (planeDetectionIndicator != null)
        {
            planeDetectionIndicator.SetActive(true);
        }
    }

    void Update()
    {
        // If we haven't placed an object yet, respond to touch
        if (placedObject == null)
        {
            UpdatePlaneDetectionIndicator();
            HandlePlacement();
        }
    }

    
    private void UpdatePlaneDetectionIndicator()
    {
        if (planeDetectionIndicator != null)
        {
            // Cast ray from center of screen
            Ray ray = arCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            
            if (raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                // Update position of indicator to match raycast hit
                planeDetectionIndicator.transform.position = hits[0].pose.position;
                planeDetectionIndicator.transform.rotation = hits[0].pose.rotation;
                planeDetectionIndicator.SetActive(true);
            }
            else
            {
                planeDetectionIndicator.SetActive(false);
            }
        }
    }

    private void HandlePlacement()
    {
        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Raycast from touch position
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    // Get the hit pose
                    Pose hitPose = hits[0].pose;

                    // Instantiate the object at hit position and rotation
                    placedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
                    
                    // Add persistent tracking script to the placed object
                    placedObject.AddComponent<ARPlacementAnchor>();
                    
                    // Configure shadow casting
                    ConfigureShadows(placedObject);
                    
                    // Hide the plane visualizers after placement for a cleaner look
                    foreach (var plane in planeManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }
                    
                    // Hide the plane detection indicator
                    if (planeDetectionIndicator != null)
                    {
                        planeDetectionIndicator.SetActive(false);
                    }
                }
            }
        }
    }

    private void ConfigureShadows(GameObject obj)
    {
        // Get all renderers in the object
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            // Enable shadow casting for all renderers
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            
            // Enable shadow receiving
            renderer.receiveShadows = true;
        }
    }
}
