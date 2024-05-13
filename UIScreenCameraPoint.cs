using UnityEngine;

public class UIScreenCameraPoint : MonoBehaviour
{
    [SerializeField]
    private Transform _followTransform;

    [SerializeField]
    private Camera _worldCamera;

    [SerializeField]
    private Camera _uiCamera;

    [SerializeField]
    private Vector3 _offset = Vector3.zero;

    public Camera WorldCamera
    {
        get
        {
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
            return _worldCamera;
        }
    }

    public void SetOffset(Vector3 offset)
    {
        _offset = offset;
    }

    public UIScreenCameraPoint SetTarget(Transform t)
    {
        _followTransform = t;
        return this;
    }

    public UIScreenCameraPoint SetUICamera(Camera camera)
    {
        _uiCamera = camera;
        return this;
    }

    void LateUpdate()
    {
        if (null != _followTransform)
        {
            var targetPosition = _followTransform.position + _offset;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(WorldCamera, targetPosition);
            transform.position = _uiCamera.ScreenToWorldPoint(screenPoint);
        }
    }

    public static UIScreenCameraPoint Require(GameObject go)
    {
        if (go != null)
        {
            if (!go.TryGetComponent<UIScreenCameraPoint>(out var component))
                return go.AddComponent<UIScreenCameraPoint>();
            return component;
        }
        return null;
    }
}