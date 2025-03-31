using UnityEngine;

public class FollowPlane : MonoBehaviour
{
    [Tooltip("Reference to the plane this object should follow.")]
    public Transform plane;
    private Vector3 localOffset;      // The offset from the plane's position
    private Quaternion localRotation; // The rotation offset relative to the plane

    /// <summary>
    /// Initialize the follower with the plane, the local offset, and the local rotation.
    /// </summary>
    public void Initialize(Transform plane, Vector3 offset, Quaternion localRot)
    {
        this.plane = plane;
        localOffset = offset;
        localRotation = localRot;
    }

    void Update()
    {
        if (plane == null)
            return;

        // Update world position and rotation so that the object tracks the plane.
        transform.position = plane.TransformPoint(localOffset);
        transform.rotation = plane.rotation * localRotation;
    }
}
