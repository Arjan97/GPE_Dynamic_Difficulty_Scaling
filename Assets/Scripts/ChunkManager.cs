using System.Collections.Generic;
using UnityEngine;

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

    // Active chunks in order along the X-axis
    private List<GameObject> activeChunks = new List<GameObject>();

    // Measured bounding box data for the chunk prefab
    private float chunkWidth;  // Total width in X
    private float chunkMinX;   // Offset from pivot to left edge (bounds.min.x)

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Measure the prefab's bounding box to get width and left offset.
        Bounds bounds = CalculatePrefabBounds(chunkPrefab);
        chunkWidth = bounds.size.x;
        chunkMinX = bounds.min.x;

        // Spawn initial chunks in a row.
        for (int i = 0; i < initialChunks; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        if (activeChunks.Count == 0 || player == null)
            return;

        // Get the first chunk (the one to be recycled)
        GameObject firstChunk = activeChunks[0];

        // Calculate its left and right edges in world space.
        // Left edge = transform.position.x + chunkMinX
        float firstChunkLeftEdge = firstChunk.transform.position.x + chunkMinX;
        float firstChunkRightEdge = firstChunkLeftEdge + chunkWidth;

        // When the player has passed beyond the first chunk's right edge (plus buffer), recycle it.
        if (player.position.x > firstChunkRightEdge + recycleThreshold)
        {
            RecycleChunk(firstChunk);
        }
    }

    /// <summary>
    /// Spawns a new chunk at the end of the current sequence.
    /// </summary>
    private void SpawnChunk()
    {
        GameObject chunk = Instantiate(chunkPrefab);
        float posX = 0f;

        if (activeChunks.Count == 0)
        {
            // For the first chunk, position it so its left edge is at x = 0.
            posX = -chunkMinX;
        }
        else
        {
            // For subsequent chunks, position it immediately after the last chunk.
            GameObject lastChunk = activeChunks[activeChunks.Count - 1];
            // Calculate the right edge of the last chunk:
            float lastChunkRightEdge = lastChunk.transform.position.x + chunkMinX + chunkWidth;
            // Position the new chunk so that its left edge is exactly at lastChunkRightEdge.
            posX = lastChunkRightEdge - chunkMinX;
        }

        chunk.transform.position = new Vector3(posX, 0f, 0f);
        activeChunks.Add(chunk);
    }

    /// <summary>
    /// Recycles the given chunk by repositioning it immediately after the last chunk.
    /// </summary>
    private void RecycleChunk(GameObject chunk)
    {
        // Remove the chunk from the beginning of the list.
        activeChunks.RemoveAt(0);

        // Get the last chunk in the list.
        GameObject lastChunk = activeChunks[activeChunks.Count - 1];
        float lastChunkRightEdge = lastChunk.transform.position.x + chunkMinX + chunkWidth;

        // Reposition the recycled chunk so its left edge aligns with the last chunk's right edge.
        float newPosX = lastChunkRightEdge - chunkMinX;
        chunk.transform.position = new Vector3(newPosX, 0f, 0f);

        // Add it to the end of the list.
        activeChunks.Add(chunk);
    }

    /// <summary>
    /// Calculates the combined bounding box of all Renderer components in the prefab.
    /// </summary>
    private Bounds CalculatePrefabBounds(GameObject prefab)
    {
        // Instantiate a temporary copy.
        GameObject temp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        temp.SetActive(false);

        Renderer[] renderers = temp.GetComponentsInChildren<Renderer>();
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
