using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARSupportChecker : MonoBehaviour
{
    [SerializeField] private ARSession _arSession;
    [SerializeField] private ObjectSpawner _objSpawner;
    ARPlane aRPlane;

    void Awake()
    {
            _objSpawner = FindAnyObjectByType<ObjectSpawner>();
            if (_objSpawner != null)
                _objSpawner.objectSpawned += OnObjectSpawned;
    }

    public void OnObjectSpawned(GameObject obj)
    {
        _objSpawner.objectPrefabs.Clear();
        aRPlane = FindAnyObjectByType<ARPlane>();
        aRPlane.gameObject.SetActive(false);
        _objSpawner.enabled = false;
    }

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
