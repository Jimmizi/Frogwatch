using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorBaseUI : MonoBehaviour
{
    public IndicatorTracker trackedObject { get; set; }    

    [SerializeField, Tooltip("Reference to the image component that draws this indicator")]
    private Image _image = null;
    public Image image
    {
        get
        {
            if (!_image)
            {
                _image = GetComponent<Image>();
            }

            return _image;
        }
    }

    [SerializeField, Tooltip("How much to inset the indicator from the edge of the screen in pixels")]
    public float padding = 10;


    /// <summary> World position of the tracked object. </summary>
    public Vector3 worldTrackPosition
    {
        get { return trackedObject?.transform.position ?? Vector3.zero; }
    }

    /// <summary> Screen position of the tracked object. </summary>
    public Vector3 screenTrackPosition
    {
        get { return Camera.main.WorldToScreenPoint(worldTrackPosition); }
    }

    /// <summary> Screen position of the indicator. </summary>
    public Vector3 screenPosition
    {
        get {
            return new Vector3(
                Mathf.Clamp(screenTrackPosition.x, 0 + padding, Screen.width - padding),
                Mathf.Clamp(screenTrackPosition.y, 0 + padding, Screen.height - padding));
        }
    }

    /// <summary> True if the tracked object is currently on screen, false otherwise. </summary>
    public bool isOnScreen
    {
        get
        {
            if (trackedObject == null)
            {
                return false;
            }
            else
            {
                Vector3 screenPos = screenTrackPosition;
                return screenPos.x >= 0 && screenPos.y >= 0
                    && screenPos.x <= Screen.width && screenPos.y <= Screen.height;
            }
        }
    }

    public bool isVisible
    {
        get { return trackedObject != null && !isOnScreen; }
    }

    void Update()
    {               
        image.enabled = isVisible;

        if (isVisible)
        {
            transform.position = screenPosition;

            Vector3 objectDirection = screenTrackPosition - screenPosition;
            float angle = Mathf.Atan2(objectDirection.y, objectDirection.x);
            transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }
    }
}
