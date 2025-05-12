using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARBackgroundToggle : MonoBehaviour
{
    [SerializeField] Toggle _toggleARBG;
    ARCameraBackground _aRCameraBackground;

    void Start()
    {
        if (_aRCameraBackground== null) _aRCameraBackground = FindAnyObjectByType<ARCameraBackground>();
        _toggleARBG.onValueChanged.AddListener(delegate{ToggleARBackground();});
    }

    void ToggleARBackground()
    {
        _aRCameraBackground.enabled = _toggleARBG.isOn;
    }
}
