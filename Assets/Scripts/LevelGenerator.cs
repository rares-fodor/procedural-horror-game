using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

class HashGrid
{
    private Dictionary<Vector2Int, List<GameObject>> hashGrid = new Dictionary<Vector2Int, List<GameObject>>();
    private int cellSize = 20;

    public HashGrid(Vector2 planeSize)
    {
        InitializeCells(planeSize);
    }

    private void InitializeCells(Vector2 planeSize)
    {
        int xCells = Mathf.FloorToInt(planeSize.x / cellSize) / 2;
        int yCells = Mathf.FloorToInt(planeSize.y / cellSize) / 2;

        for (int x = -xCells; x <= xCells; x++)
        {
            for (int y = -yCells; y <= yCells; y++)
            {
                hashGrid[new Vector2Int(x, y)] = new List<GameObject>();
            }
        }
    }

    /**
     * Get the key for the given position
     */
    internal Vector2Int GetCell(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int y = Mathf.FloorToInt(position.z / cellSize);
        return new Vector2Int(x, y);
    }

    internal bool TryGetValue(Vector2Int key, out List<GameObject> value)
    {
        return hashGrid.TryGetValue(key, out value);
    }

    internal void Add(Vector3 position, GameObject prefab)
    {
        Vector2Int designatedCell = GetCell(position);
        if (!hashGrid.ContainsKey(designatedCell))
        {
            hashGrid[designatedCell] = new List<GameObject>();
        }
        hashGrid[designatedCell].Add(prefab);
    }
}


public class LevelGenerator : MonoBehaviour
{
    // Material into which to feed the noise texture
    [SerializeField] Material groundMaterial;
    
    // Noise parameters
    [SerializeField] private float NoiseScale = 5;
    [SerializeField] private Vector2 NoiseOffset = Vector2.zero;

    // Generate new level
    [SerializeField] private bool recompute = false;
    // Clear terrain details
    [SerializeField] private bool clearPrefabs = false;
    [SerializeField] private bool randomizeNoise = true;

    // Prefab lists
    [SerializeField] private List<GameObject> pointsOfInterest = new List<GameObject>();
    [SerializeField] private List<GameObject> layer1Assets = new List<GameObject>();
    [SerializeField] private List<GameObject> layer2Assets = new List<GameObject>();

    [SerializeField] private float blendThreshold = 0.5f;
    [SerializeField] private float detailDensity = 0.005f;

    // How many of each object can fit between itself and it's nearest possible neighbor
    [SerializeField] private float safetyRadiusFactor = 2.0f;
    // How far away should POIs spawn from eachother as a factor of their extents
    [SerializeField] private float spreadFactor = 2.0f;

    [SerializeField] private GameObject marker;

    // Texture properties
    private int width = 1024;
    private int height = 1024;
    private Texture2D noise_texture;

    // Track prefabs
    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    private List<GameObject> spawnedPointsOfInterest = new List<GameObject>();

    // Map prefabs into spatial cells
    private HashGrid hashGrid;

    private Vector2 planeSize;

    void Start()
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        planeSize = new Vector2(mr.bounds.size.x, mr.bounds.size.z);

        hashGrid = new HashGrid(planeSize);
        
        // Generate noise texture
        noise_texture = GenerateGroundBlendTexture();
        // Pass noise texture to ground mix shader
        groundMaterial.SetTexture("_NoiseTexture", noise_texture);

        // Spawn prefabs
        SpawnPointsOfInterest();
        SpawnPrefabsOnNoise();

        // DebugPrefabPosition(planeSize);
    }

    private void Update()
    {
        if (recompute)
        {
            Clear();
            noise_texture = GenerateGroundBlendTexture();
            groundMaterial.SetTexture("_NoiseTexture", noise_texture);
            SpawnPointsOfInterest();
            SpawnPrefabsOnNoise();
            recompute = false;
        }
        if (clearPrefabs)
        {
            Clear();
            clearPrefabs = false;
        }
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void Clear()
    {
        foreach (var obj in spawnedPrefabs)
        {
            DestroyImmediate(obj);
        }
        foreach (var obj in spawnedPointsOfInterest)
        { 
            DestroyImmediate(obj);
        }
        spawnedPrefabs.Clear();
        spawnedPointsOfInterest.Clear();
    }

    private Texture2D GenerateGroundBlendTexture()
    {
        Texture2D texture = new Texture2D(width, height);
        
        if (randomizeNoise) RandomizeNoiseParams();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = GenerateNoiseColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    private Color GenerateNoiseColor (int x, int y)
    {
        float xSample = (float) x / width * NoiseScale + NoiseOffset.x;
        float ySample = (float) y / height * NoiseScale + NoiseOffset.y;

        float sample  = Mathf.PerlinNoise(xSample, ySample);
        return new Color(sample, sample, sample);
    }
    
    private void RandomizeNoiseParams()
    {
        NoiseScale = Random.Range(3.0f, 5.0f);
        NoiseOffset = new Vector2(Random.Range(0f, 100.0f), Random.Range(0f, 100.0f));
    }

    private void SpawnPointsOfInterest()
    {
        // Padding (avoid placing points of interest on the map edges)
        Vector2 paddedPlaneSize = planeSize * 0.85f;

        foreach (var obj in pointsOfInterest)
        {
            bool canSpawn = false;
            int retries = 100;
            Vector3 position = Vector3.zero;

            while (!canSpawn && retries != 0)
            {
                position = new Vector3(
                    Random.Range(-paddedPlaneSize.x / 2, paddedPlaneSize.x / 2),
                    0,
                    Random.Range(-paddedPlaneSize.y / 2, paddedPlaneSize.y / 2)
                );
                canSpawn = !CollisionTest(position, spawnedPointsOfInterest, spreadFactor);
                retries--;
            }

            if (canSpawn)
                spawnedPointsOfInterest.Add(Instantiate(obj, position, Quaternion.identity));
        }

        // Notify gameController of stone positions
        GameController.StoneLocationChangedEvent.Invoke(spawnedPointsOfInterest);
    }

    private void SpawnPrefabsOnNoise()
    {
        noise_texture = (Texture2D) groundMaterial.GetTexture("_NoiseTexture");

        for (int x = 0; x < noise_texture.width; x++)
        {
            for (int y = 0; y < noise_texture.height; y++)
            {
                if (ShouldSpawn())
                {
                    // Normalize to UV coordinates
                    Vector2 normalizedPos = new Vector2(1 - (float) x / noise_texture.width, 1 - (float) y / noise_texture.height);

                    // Current world position
                    Vector3 position = new Vector3(
                        (normalizedPos.x - 0.5f) * planeSize.x,
                        0,
                        (normalizedPos.y - 0.5f) * planeSize.y
                    );

                    // Test collisions with POIs
                    if (CollisionTest(position, spawnedPointsOfInterest, safetyRadiusFactor)) continue;
                    // Test collisions with other detail prefabs
                    if (CollisionTest(position)) continue;

                    // Get pixel color
                    Color color = noise_texture.GetPixel(x, y);
                
                    // Choose prefab to spawn based on terrain type (color intensity as determined by blend threshold)
                    GameObject prefab = SelectPrefab(color);

                    GameObject spawnedPrefab = Instantiate(prefab, position, Quaternion.identity);
                    
                    // Randomize prefab transform properties
                    RandomizeScaleAndRotation(spawnedPrefab);
                    
                    spawnedPrefabs.Add(spawnedPrefab);

                    // Add to hashgrid
                    hashGrid.Add(position, spawnedPrefab);
                }
            }
        }
    }

    private bool CollisionTest(Vector3 position)
    {
        // Determine which cell the given position belongs into
        Vector2Int cell = hashGrid.GetCell(position);

        // Look for collisions in all neighboring cells
        // Particularly useful for objects that are located on the edges of a cell
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector2Int neighborCell = cell + new Vector2Int(i, j);
                if (hashGrid.TryGetValue(neighborCell, out List<GameObject> prefabsInCell))
                {
                    if (CollisionTest(position, prefabsInCell, safetyRadiusFactor))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool CollisionTest(Vector3 position, List<GameObject> collection, float radiusFactor)
    {
        foreach (var obj in collection)
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider == null)
                continue;

            // Add the safety radius to the objects extent
            // i.e. half of it's collider's size + safety radius
            float objExtent = collider.bounds.extents.magnitude;

            // Ignore safety margin if object is small
            if (objExtent < 10.0f)
            {
                radiusFactor = 1;
            }

            if (Vector3.Distance(position, obj.transform.position) < (objExtent * radiusFactor))
                return true;
        }
        return false;
    }

    private void RandomizeScaleAndRotation(GameObject prefab)
    {
        float scaleFactor = Random.Range(0.75f, 1.3f);
        float rotationScale = Random.Range(0f, 360f);

        float xScale = prefab.transform.localScale.x * scaleFactor;
        float yScale = prefab.transform.localScale.y * scaleFactor;
        float zScale = prefab.transform.localScale.z * scaleFactor;

        prefab.transform.localScale = new Vector3(xScale, yScale, zScale);
        prefab.transform.rotation = Quaternion.Euler(0, rotationScale, 0);
    }

    private bool ShouldSpawn()
    {
        return Random.value <= detailDensity;
    }
    
    // Select a random prefab from the appropriate list
    private GameObject SelectPrefab (Color color)
    {
        if (color.grayscale > blendThreshold)
        {
            return layer2Assets.ElementAt(Random.Range(0, layer2Assets.Count));
        } else
        {
            return layer1Assets.ElementAt(Random.Range(0, layer1Assets.Count));
        }
    }
    private void DebugPrefabPosition(Vector2 planeSize)
    {
        int xCells = Mathf.FloorToInt(planeSize.x / 20) / 2;
        int yCells = Mathf.FloorToInt(planeSize.y / 20) / 2;

        for (int x = -xCells; x <= xCells; x++)
        {
            for (int y = -yCells; y <= yCells; y++)
            {
                hashGrid.TryGetValue(new Vector2Int(x, y), out List<GameObject> list);
                foreach (GameObject obj in list)
                {
                    Instantiate(marker, obj.transform.position, obj.transform.rotation);
                }
            }
        }
    }

}
