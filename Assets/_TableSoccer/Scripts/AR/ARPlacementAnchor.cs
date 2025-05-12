using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlacementAnchor : MonoBehaviour
{
    private ARAnchorManager anchorManager;
    private ARAnchor anchor;
    
    void Start()
    {
        // Get the anchor manager
        anchorManager = FindAnyObjectByType<ARAnchorManager>();
        
        if (anchorManager != null)
        {
            // Create an anchor at this position
            // anchor = anchorManager.AttachAnchor(new Pose(transform.position, transform.rotation));
            
            if (anchor != null)
            {
                // Make this object a child of the anchor to maintain its position
                transform.parent = anchor.transform;
                
                Debug.Log("Object anchored successfully");
            }
            else
            {
                Debug.LogError("Failed to create anchor");
            }
        }
        else
        {
            Debug.LogError("No ARAnchorManager found in the scene");
        }
    }
}
