using UnityEngine;

public class WorldObjectFollowUI : MonoBehaviour
{
    [Tooltip("UI element to follow")]
    [SerializeField] private RectTransform uiTarget;

    [Tooltip("Camera used by the Screen Space - Camera canvas")]
    [SerializeField] private Camera uiCamera;

    [Tooltip("Optional offset in world space")]
    [SerializeField] private Vector3 worldOffset;

    void Update()
    {
        if (uiTarget == null || uiCamera == null)
            return;

        // Convert UI position to screen point
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, uiTarget.position);

        // Convert screen point to world position in front of camera
        Vector3 worldPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(uiTarget, screenPos, uiCamera, out worldPos))
        {
            transform.position = worldPos + worldOffset;
        }
    }
}
