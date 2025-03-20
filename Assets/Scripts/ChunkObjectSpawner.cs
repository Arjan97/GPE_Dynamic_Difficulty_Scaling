using System.Collections.Generic;
using UnityEngine;

public class ChunkObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] GameObject[] spawnPrefabs; // Objects to spawn
    [SerializeField] int minSpawnCount = 1;
    [SerializeField] int maxSpawnCount = 5;
    [Range(0f, 1f)][SerializeField] float spawnChance = 0.7f;
    [SerializeField] float yOffset = 1.5f; // Adjusted to avoid ground clipping

    float chunkWidth = 10f;
    float chunkHeight = 5f;
    float chunkLength = 20f;
    List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        CalculateChunkDimensions();
    }

    void CalculateChunkDimensions()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            chunkWidth = renderer.bounds.size.x;
            chunkHeight = renderer.bounds.size.y;
            chunkLength = renderer.bounds.size.z;
        }
        else
        {
            Debug.LogWarning($"ChunkObjectSpawner: No Renderer found. Using defaults (Width={chunkWidth}, Height={chunkHeight}, Length={chunkLength})");
        }
    }

    public void SpawnObjects()
    {
        ClearObjects();
        if (spawnPrefabs.Length == 0 || Random.value > spawnChance) return;

        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            Vector3 spawnPos = GetRandomPosition();

            // Ensure the spawned object inherits the exact Z rotation of the parent chunk
            Quaternion spawnRotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z);

            GameObject obj = Instantiate(prefab, spawnPos, spawnRotation, transform);
            spawnedObjects.Add(obj);
        }

        Debug.Log($"Spawned {spawnCount} objects in chunk {gameObject.name} with rotation Z: {transform.rotation.eulerAngles.z}");
    }

    public void ResetObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            obj.transform.position = GetRandomPosition();
            obj.SetActive(true);
        }
    }

    void ClearObjects()
    {
        Debug.Log($"Clearing {spawnedObjects.Count} objects in chunk {gameObject.name}");
        foreach (var obj in spawnedObjects)
        {
            Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-chunkWidth / 2, chunkWidth / 2);
        float y = Mathf.Max(0.5f, Random.Range(0, chunkHeight / 2)) + yOffset; // Ensures above ground
        float z = Random.Range(-chunkLength / 2, chunkLength / 2);
        Vector3 position = transform.position + new Vector3(x, y, z);

        Debug.Log($"Spawning object at: {position} in chunk {gameObject.name}");
        return position;
    }
}
