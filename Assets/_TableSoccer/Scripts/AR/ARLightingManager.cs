using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class ARLightingManager : MonoBehaviour
{
    [SerializeField]
    private ARCameraManager cameraManager;
    
    private Light mainLight;
    
    void Awake()
    {
        mainLight = GetComponent<Light>();
        
        if (cameraManager == null)
        {
            cameraManager = FindAnyObjectByType<ARCameraManager>();
        }
    }
    
    void OnEnable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived += FrameUpdated;
        }
    }
    
    void OnDisable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived -= FrameUpdated;
        }
    }
    
    private void FrameUpdated(ARCameraFrameEventArgs args)
    {
        // Update light intensity based on camera's estimated brightness
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            mainLight.intensity = args.lightEstimation.averageBrightness.Value * 2;
        }
        
        // Update light color based on camera's white balance
        if (args.lightEstimation.colorCorrection.HasValue)
        {
            mainLight.color = args.lightEstimation.colorCorrection.Value;
        }
        
        // Update light direction if available
        if (args.lightEstimation.mainLightDirection.HasValue && 
            args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
            transform.rotation = Quaternion.LookRotation(args.lightEstimation.mainLightDirection.Value);
            mainLight.intensity = args.lightEstimation.mainLightIntensityLumens.Value;
        }
    }
}