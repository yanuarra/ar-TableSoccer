using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSupportChecker : MonoBehaviour
{
    [SerializeField] private ARSession _arSession;

    public bool isARAvailable()
    {
        if (_arSession == null)
            _arSession = FindAnyObjectByType<ARSession>();

        if (_arSession!=null)
        {
            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            {
                StartCoroutine(ARSession.CheckAvailability());
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                Debug.Log("AR is not supported on this device.");
            }
            else
            {
                Debug.Log("AR is supported.");
                if (_arSession != null)
                    _arSession.enabled = true;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
