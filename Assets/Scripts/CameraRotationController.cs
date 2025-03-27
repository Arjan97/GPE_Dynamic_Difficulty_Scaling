using UnityEngine;

public class CameraCounterRotation : MonoBehaviour
{
    private Transform referenceSpawnPlane;

    void Start()
    {
        // Find all transforms (including inactive) and pick the first with the "SpawnPlane" tag.
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.CompareTag("SpawnPlane"))
            {
                referenceSpawnPlane = t;
                break;
            }
        }

        if (referenceSpawnPlane == null)
        {
            Debug.LogWarning("CameraCounterRotation: No object with tag 'SpawnPlane' was found.");
        }
    }

    void LateUpdate()
    {
        if (referenceSpawnPlane == null)
            return;

        // Read the reference object's x-axis rotation (in world space)
        float tunnelXRotation = referenceSpawnPlane.eulerAngles.x;

        // Set the camera's z-axis rotation to the negative of that value.
        // This counteracts the tunnel's rotation, making it appear static.
        Vector3 currentEuler = transform.eulerAngles;
        transform.eulerAngles = new Vector3(currentEuler.x, currentEuler.y, -tunnelXRotation);
    }
}
