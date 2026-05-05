using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    public float targetWidth = 16f;
    public float targetHeight = 9f;

    private Camera targetCamera;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        ApplyAspect();
    }

    private void Update()
    {
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
        {
            return;
        }

        ApplyAspect();
    }

    private void OnValidate()
    {
        targetCamera = GetComponent<Camera>();
        ApplyAspect();
    }

    public void ApplyAspect()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera == null || Screen.height <= 0 || targetWidth <= 0f || targetHeight <= 0f)
        {
            return;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        var targetAspect = targetWidth / targetHeight;
        var windowAspect = (float)Screen.width / Screen.height;
        var scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            targetCamera.rect = new Rect(0f, (1f - scaleHeight) * 0.5f, 1f, scaleHeight);
            return;
        }

        var scaleWidth = 1f / scaleHeight;
        targetCamera.rect = new Rect((1f - scaleWidth) * 0.5f, 0f, scaleWidth, 1f);
    }
}
