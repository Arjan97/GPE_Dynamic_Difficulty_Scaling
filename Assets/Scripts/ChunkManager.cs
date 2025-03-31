using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableObjectSettings
{
    [Tooltip("Prefab to spawn.")]
    public GameObject prefab;
    [Tooltip("Minimum number of objects to spawn.")]
    public int minNumber = 0;
    [Tooltip("Maximum number of objects to spawn.")]
    public int maxNumber = 5;
    [Range(0f, 1f)]
    [Tooltip("Chance to spawn objects of this type.")]
    public float spawnChance = 1f;

    [Tooltip("Vertical offset (local Y) for spawned objects.")]
    public float yOffset = 0f;

    [Tooltip("If true, use the plane's renderer bounds as the spawn area; otherwise, use the custom size below.")]
    public bool useParentBounds = true;

    [Tooltip("Custom spawn area size (width, length) in local space if not using parent's bounds.")]
    public Vector2 customSpawnAreaSize = new Vector2(10f, 5f);
}

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private int initialChunks = 3;

    [Header("Player Settings")]
    [SerializeField] private Transform player;

    [Header("Recycle Settings")]
    [Tooltip("Extra distance beyond the chunk’s right edge before recycling.")]
    [SerializeField] private float recycleThreshold = 5f;

    [Header("Child Randomization")]
    [SerializeField] private int minDisabledChildren = 2;
    [SerializeField] private int maxDisabledChildren = 4;
    [Range(0f, 1f)]
    [SerializeField] private float disableChance = 0.5f;

    [Header("Spawnable Objects")]
    [Tooltip("All the different object types to spawn on chunk planes.")]
    [SerializeField] private List<SpawnableObjectSettings> spawnableObjects = new List<SpawnableObjectSettings>();
    [Header("Plane Tag")]
    [Tooltip("Tag assigned to each plane child in the chunk prefab.")]
    [SerializeField] private string chunkPlanesTag = "SpawnPlane";

    // Active chunks in order along the X-axis.
    private List<GameObject> activeChunks = new List<GameObject>();

    // Measured bounding box data for the chunk prefab.
    private float chunkWidth;  // Total width in X.
    private float chunkMinX;   // Offset from pivot to left edge (bounds.min.x).

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // Measure the prefab's bounding box to know how wide each chunk is.
        if (chunkPrefab != null)
        {
            
        Bounds bounds = CalculatePrefabBounds(chunkPrefab);

        chunkWidth = bounds.size.x;
        chunkMinX = bounds.min.x;
        }

        // Spawn initial chunks with children fully enabled.
        for (int i = 0; i < initialChunks; i++)
        {
            SpawnChunk(randomiseChildren: false);
        }
    }

    void Update()
    {
        if (activeChunks.Count == 0 || player == null)
            return;

        // The first chunk is the one to recycle once the player passes it.
        GameObject firstChunk = activeChunks[0];

        // Calculate its left and right edges in world space.
        float firstChunkLeftEdge = firstChunk.transform.position.x + chunkMinX;
        float firstChunkRightEdge = firstChunkLeftEdge + chunkWidth;

        // Recycle if player has passed the chunk's right edge + threshold.
        if (player.position.x > firstChunkRightEdge + recycleThreshold)
        {
            RecycleChunk(firstChunk);
        }
    }

    /// <summary>
    /// Spawns a new chunk at the end of the current sequence.
    /// </summary>
    /// <param name="randomiseChildren">Whether to randomize child objects for the chunk.</param>
    private void SpawnChunk(bool randomiseChildren)
    {
        // Create the chunk.
        GameObject chunk = Instantiate(chunkPrefab);
        float posX = 0f;

        if (activeChunks.Count == 0)
        {
            // First chunk: position so its left edge is at x=0.
            posX = -chunkMinX;
        }
        else
        {
            // Subsequent chunks: position immediately after the last chunk.
            GameObject lastChunk = activeChunks[activeChunks.Count - 1];
            float lastChunkRightEdge = lastChunk.transform.position.x + chunkMinX + chunkWidth;
            posX = lastChunkRightEdge - chunkMinX;
        }

        chunk.transform.position = new Vector3(posX, 0f, 0f);
        activeChunks.Add(chunk);

        // Always re-enable all children before randomizing.
        EnableAllChunkChildren(chunk);

        if (randomiseChildren)
        {
            RandomizeChunkChildren(chunk);
        }

        // Now spawn objects on each plane in this chunk.
        SpawnObjectsOnPlanes(chunk);
    }

    /// <summary>
    /// Recycles the given chunk by moving it to the end and randomizing/spawning new objects.
    /// </summary>
    private void RecycleChunk(GameObject chunk)
    {
        activeChunks.RemoveAt(0);

        GameObject lastChunk = activeChunks[activeChunks.Count - 1];
        float lastChunkRightEdge = lastChunk.transform.position.x + chunkMinX + chunkWidth;
        float newPosX = lastChunkRightEdge - chunkMinX;
        chunk.transform.position = new Vector3(newPosX, 0f, 0f);

        EnableAllChunkChildren(chunk);
        RandomizeChunkChildren(chunk);

        activeChunks.Add(chunk);

        // Re-spawn objects on planes.
        SpawnObjectsOnPlanes(chunk);
    }

    /// <summary>
    /// Finds each plane in the chunk (tagged 'chunkPlanesTag'), clears old objects, and spawns new ones
    /// based on the 'spawnableObjects' settings.
    /// </summary>
    private void SpawnObjectsOnPlanes(GameObject chunk)
    {
        // Find all plane children by tag.
        Transform[] allChildren = chunk.GetComponentsInChildren<Transform>(true);
        List<Transform> planeTransforms = new List<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag(chunkPlanesTag))
            {
                planeTransforms.Add(child);
            }
        }

        // Ensure we have a global container called "OBJECTS"
        Transform objectsContainer = GameObject.Find("OBJECTS")?.transform;
        if (objectsContainer == null)
        {
            GameObject newContainer = new GameObject("OBJECTS");
            objectsContainer = newContainer.transform;
        }

        // For each plane, clear old spawned objects (from the container) and spawn new ones.
        foreach (Transform plane in planeTransforms)
        {
            // Optionally: You might want to clear only objects that were previously spawned for this plane.
            // For simplicity, we assume that any object with a FollowPlane component referencing this plane is removed.
            ClearSpawnedObjectsForPlane(plane, objectsContainer);

            // For each spawnable object setting, check spawn chance, then spawn.
            foreach (var settings in spawnableObjects)
            {
                if (Random.value <= settings.spawnChance)
                {
                    int count = Random.Range(settings.minNumber, settings.maxNumber + 1);
                    // Determine spawn area: either use the plane's renderer bounds or a custom size.
                    Vector2 spawnArea = settings.useParentBounds
                        ? CalculatePlaneBounds(plane)
                        : settings.customSpawnAreaSize;
                    // Cache the prefab's original local scale and local rotation.
                    Vector3 originalPrefabScale = settings.prefab.transform.localScale;
                    Quaternion originalPrefabLocalRot = settings.prefab.transform.localRotation;

                    for (int i = 0; i < count; i++)
                    {
                        // Randomize X and Z within the spawn area; Y is fixed to settings.yOffset.
                        float randomX = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
                        float randomZ = Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f);
                        Vector3 localOffset = new Vector3(randomX, settings.yOffset, randomZ);
                        // Calculate world position using the plane's transform.
                        Vector3 worldPos = plane.TransformPoint(localOffset);

                        // Instantiate the object as a child of the global container.
                        GameObject spawned = Instantiate(settings.prefab, worldPos, settings.prefab.transform.rotation, objectsContainer);
                        // Reset the object's local scale to the original prefab scale.
                        spawned.transform.localScale = originalPrefabScale;

                        // Add a FollowPlane component so the object will update its position relative to the plane.
                        FollowPlane follower = spawned.AddComponent<FollowPlane>();
                        follower.Initialize(plane, localOffset, originalPrefabLocalRot);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Clears previously spawned objects from the plane by destroying all child objects.
    /// </summary>
    private void ClearSpawnedObjectsForPlane(Transform plane, Transform objectsContainer)
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in objectsContainer)
        {
            FollowPlane follower = child.GetComponent<FollowPlane>();
            if (follower != null && follower.plane == plane)
            {
                toDestroy.Add(child.gameObject);
            }
        }
        foreach (GameObject go in toDestroy)
        {
            Destroy(go);
        }
    }

    /// <summary>
    /// Calculates the plane's renderer bounds in local space (width, height).
    /// Interpreted as (width = X, height = Z) for spawning.
    /// </summary>
    private Vector2 CalculatePlaneBounds(Transform plane)
    {
        Renderer[] renderers = plane.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return Vector2.zero;
        }

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        // Convert the world-space size to the plane's local space.
        Vector3 localSize = plane.InverseTransformVector(combined.size);
        return new Vector2(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y));
    }

    /// <summary>
    /// Randomly disables a subset of the chunk's children.
    /// </summary>
    private void RandomizeChunkChildren(GameObject chunk)
    {
        // Get all children (excluding the chunk root) and ensure they're enabled.
        Transform[] children = chunk.GetComponentsInChildren<Transform>(true);
        List<Transform> selectableChildren = new List<Transform>();
        foreach (Transform child in children)
        {
            if (child == chunk.transform)
                continue;

            child.gameObject.SetActive(true);  // Re-enable everything first
            selectableChildren.Add(child);
        }

        // Decide how many children to disable: a random number between minDisabledChildren and maxDisabledChildren.
        int total = selectableChildren.Count;
        int numToDisable = Random.Range(minDisabledChildren, maxDisabledChildren + 1);
        numToDisable = Mathf.Min(numToDisable, total);

        // Shuffle the list to randomize the order.
        for (int i = 0; i < selectableChildren.Count; i++)
        {
            int rnd = Random.Range(i, selectableChildren.Count);
            Transform temp = selectableChildren[i];
            selectableChildren[i] = selectableChildren[rnd];
            selectableChildren[rnd] = temp;
        }

        // Disable the first numToDisable items from the shuffled list.
        for (int i = 0; i < numToDisable; i++)
        {
            selectableChildren[i].gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// Re-enables all child objects within the chunk.
    /// </summary>
    private void EnableAllChunkChildren(GameObject chunk)
    {
        Transform[] children = chunk.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child != chunk.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Calculates the combined bounding box of all Renderer components in the chunk prefab.
    /// Used to determine chunkWidth for side-by-side placement.
    /// </summary>
    private Bounds CalculatePrefabBounds(GameObject prefab)
    {
        GameObject temp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        temp.SetActive(false);

        Renderer[] renderers = temp.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Destroy(temp);
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        Destroy(temp);
        return combined;
    }
}
