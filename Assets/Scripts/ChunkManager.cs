using System.Collections.Generic;
using UnityEngine;

// A simple helper component to mark initial chunks.
public class ChunkInfo : MonoBehaviour
{
    public bool isInitialChunk = false;
}

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    public GameObject[] chunkPrefabs;    // Array of different chunk prefabs for variety
    public float chunkLength = 20f;        // Length of each chunk segment (along Z-axis)

    [Header("Player Settings")]
    public Transform player;               // Reference to the player's transform
    public float spawnDistance = 50f;      // (Not used with recycling but kept for configuration)

    [Header("Initial Setup")]
    public int initialChunks = 6;          // Total number of chunks to be active (e.g., 6)
    public int minDisabledChildren = 2;    // Minimum number of disabled children per chunk
    public int maxDisabledChildren = 4;    // Maximum number of disabled children per chunk
    [Range(0f, 1f)]
    public float disableChance = 0.5f;     // Probability of disabling each child

    [Header("Spawning Threshold")]
    public float spawnDespawnThreshold = 20f; // Distance beyond a chunk at which it should be recycled

    // Pools for object pooling
    private Queue<GameObject> chunkPool = new Queue<GameObject>();

    // Tracking active chunks and their spawned objects
    private float nextSpawnZ = 0f;
    private List<GameObject> activeChunks = new List<GameObject>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // Spawn the initial chunks. These chunks will always have all children enabled.
        for (int i = 0; i < initialChunks; i++)
        {
            GameObject chunk = SpawnChunk(false);
            activeChunks.Add(chunk);
        }
        LevelController.OnGameOverEvent += EndGame;
    }

    void Update()
    {
        if (player == null)
            return;
        // When the player has passed the first chunk by the threshold, recycle it.
        if (activeChunks.Count > 0 && player.position.z - activeChunks[0].transform.position.z > spawnDespawnThreshold)
        {
            RecycleChunk(activeChunks[0]);
        }
    }

    // Spawns a new chunk. 'randomise' controls whether child objects should be randomized.
    GameObject SpawnChunk(bool randomise)
    {
        GameObject newChunk = GetChunkFromPool();
        newChunk.transform.position = new Vector3(0f, 0f, nextSpawnZ);
        newChunk.SetActive(true);

        // Add or get the ChunkInfo component to mark whether this is an initial chunk.
        ChunkInfo chunkInfo = newChunk.GetComponent<ChunkInfo>();
        if (chunkInfo == null)
            chunkInfo = newChunk.AddComponent<ChunkInfo>();

        // If not randomising, mark this as an initial chunk.
        chunkInfo.isInitialChunk = !randomise;

        nextSpawnZ += chunkLength;
        if (randomise)
            RandomizeChunkChildren(newChunk);

        ChunkObjectSpawner[] spawners = newChunk.GetComponentsInChildren<ChunkObjectSpawner>();
        foreach (ChunkObjectSpawner spawner in spawners)
        {
            spawner.SpawnObjects();
        }

        return newChunk;
    }
    void EndGame()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void ResetGame()
    {
        nextSpawnZ = 0f;
        foreach (GameObject chunk in activeChunks)
        {
            chunk.SetActive(false);
            chunkPool.Enqueue(chunk);
        }
        activeChunks.Clear();

        for (int i = 0; i < initialChunks; i++)
        {
            GameObject chunk = SpawnChunk(false);
            activeChunks.Add(chunk);
        }
    }
    // Retrieves a chunk from the pool or instantiates one if the pool is empty.
    GameObject GetChunkFromPool()
    {
        if (chunkPool.Count > 0)
        {
            return chunkPool.Dequeue();
        }
        else
        {
            int index = Random.Range(0, chunkPrefabs.Length);
            return Instantiate(chunkPrefabs[index]);
        }
    }

    void RandomizeChunkChildren(GameObject chunk)
    {
        Transform[] children = chunk.GetComponentsInChildren<Transform>(true);
        List<Transform> selectableChildren = new List<Transform>();

        foreach (Transform child in children)
        {
            if (child != chunk.transform && child.name != "GroundCheck")
            {
                selectableChildren.Add(child);
            }
        }

        int numToDisable = Random.Range(minDisabledChildren, maxDisabledChildren + 1);
        numToDisable = Mathf.Min(numToDisable, selectableChildren.Count); 

        int disabledCount = 0;
        foreach (Transform child in selectableChildren)
        {
            if (disabledCount >= numToDisable)
                break;

            if (Random.value < disableChance && child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                disabledCount++;
            }
        }
    }


    void RecycleChunk(GameObject chunk)
    {
        chunk.transform.position = new Vector3(0f, 0f, nextSpawnZ);
        chunk.SetActive(true); // Ensure the chunk is reactivated before modifications

        EnableAllChunkChildren(chunk); // Ensure all children are enabled first

        ChunkInfo chunkInfo = chunk.GetComponent<ChunkInfo>();
        if (chunkInfo != null && chunkInfo.isInitialChunk)
        {
            chunkInfo.isInitialChunk = false;
        }
        else
        {
            RandomizeChunkChildren(chunk); // Only randomize after enabling
        }

        activeChunks.RemoveAt(0);
        activeChunks.Add(chunk);
        nextSpawnZ += chunkLength;
    }

    // Enables all children (excluding the chunk root, "GroundCheck", and objects tagged "ChunkObject").
    void EnableAllChunkChildren(GameObject chunk)
    {
        Transform[] children = chunk.GetComponentsInChildren<Transform>(true); 
        foreach (Transform child in children)
        {
            if (child != chunk.transform && child.name != "GroundCheck")
            {
                child.gameObject.SetActive(true);
            }
        }
    }

}
