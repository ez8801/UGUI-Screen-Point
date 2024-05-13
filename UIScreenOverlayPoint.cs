using UnityEngine;

public class UIScreenOverlayPoint : MonoBehaviour
{
    [SerializeField]
    private Transform _followTransform;

    [SerializeField]
    private Camera _worldCamera;

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

    public void SetTarget(Transform t)
    {
        _followTransform = t;
    }

    void LateUpdate()
    {
        if (null != _followTransform)
        {
            var targetPosition = _followTransform.position + _offset;
            var screenPoint = WorldCamera.WorldToScreenPoint(targetPosition);
            transform.position = screenPoint;
        }
    }

    public static UIScreenOverlayPoint Require(GameObject go)
    {
        if (go != null)
        {
            if (!go.TryGetComponent<UIScreenOverlayPoint>(out var component))
                return go.AddComponent<UIScreenOverlayPoint>();
            return component;
        }
        return null;
    }
}