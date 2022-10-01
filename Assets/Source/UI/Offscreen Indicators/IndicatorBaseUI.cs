using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorBaseUI : MonoBehaviour
{
    [SerializeField, Tooltip("Distance scale factor, the higher this number the faster the arrow grows over distance")]
    float scaleFactor = 600f;

    public IndicatorTracker trackedObject { get; set; }    

    [SerializeField, Tooltip("Reference to the image component that draws this indicator")]
    private Image _arrowImage = null;
    public Image image
    {
        get
        {
            if (!_arrowImage)
            {
                _arrowImage = GetComponent<Image>();
            }

            return _arrowImage;
        }
    }

    [SerializeField, Tooltip("How much to inset the indicator from the edge of the screen in pixels")]
    public float padding = 10;

    [SerializeField, Tooltip("Transform to apply position to")]
    private Transform _positionTransform;
    public Transform positionTransform => _positionTransform ? _positionTransform : transform;

    [SerializeField, Tooltip("Transform to apply rotation to")]
    private Transform _rotationTransform;
    public Transform rotationTransform => _rotationTransform ? _rotationTransform : transform;

    [SerializeField, Tooltip("Transform to apply scale to")]
    private Transform _scaleTransform;
    public Transform scaleTransform => _scaleTransform ? _scaleTransform : transform;


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

    public virtual bool isVisible
    {
        get { return trackedObject != null && !isOnScreen; }
    }

    virtual protected void Update()
    {               
        image.enabled = isVisible;

        if (isVisible)
        {
            positionTransform.position = screenPosition;
            
            Vector3 objectDirection = screenTrackPosition - screenPosition;
            float angle = Mathf.Atan2(objectDirection.y, objectDirection.x);
            rotationTransform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

            float distance = objectDirection.magnitude;
            float scale = scaleFactor / distance;
            float clampedScale = Mathf.Clamp(scale, 0.7f, 1.6f);
            scaleTransform.localScale = new Vector3(clampedScale, clampedScale, clampedScale);
        }
    }
}
