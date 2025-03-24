using UnityEngine;
using System.Collections.Generic;

public class ChunkObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab of the object to spawn.")]
    [SerializeField] private GameObject objectPrefab;
    [Tooltip("Number of objects to spawn on this chunk.")]
    [SerializeField] private int numberOfObjects = 5;

    [Header("Spawn Area Settings")]
    [Tooltip("Area size (width, height) in local space within which objects are randomized on the X and Y axes.")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 5f);

    [Header("Z Offset")]
    [Tooltip("Fixed local Z offset for spawned objects relative to the chunk.")]
    [SerializeField] private float zOffset = 0f;

    // List to keep track of spawned objects (for cleanup when respawning)
    private List<GameObject> spawnedObjects = new List<GameObject>();

    /// <summary>
    /// Clears any previously spawned objects and spawns new ones.
    /// Called from the ChunkManager when the chunk is first spawned or recycled.
    /// </summary>
    public void SpawnObjects()
    {
        ClearObjects();

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Calculate a random local position within the defined spawn area.
            // Only X and Y are randomized; Z is fixed.
            float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomY = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            Vector3 localPos = new Vector3(randomX, randomY, zOffset);

            // Instantiate the object as a child of this chunk.
            GameObject spawned = Instantiate(objectPrefab, transform);
            spawned.transform.localPosition = localPos;

            // Ensure the spawned object's rotation matches the parent's rotation.
            // This makes it parallel to the chunk.
            spawned.transform.rotation = transform.rotation;

            // Optionally, if you need only to match the X rotation, you could do:
            // Vector3 euler = spawned.transform.rotation.eulerAngles;
            // euler.x = transform.rotation.eulerAngles.x;
            // spawned.transform.rotation = Quaternion.Euler(euler);

            spawnedObjects.Add(spawned);
        }
    }

    /// <summary>
    /// Destroys any previously spawned objects.
    /// Called before spawning new ones, e.g., when the chunk is recycled.
    /// </summary>
    public void ClearObjects()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }
        spawnedObjects.Clear();
    }
}
