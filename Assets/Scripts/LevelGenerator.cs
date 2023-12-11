using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Segment the terrain space into cells of a fixed size. Every <c>GameObject</c> added will be assigned to a cell
/// and put into a list with the other <c>GameObjects</c> in the cell.
/// Allows constant time access to any list of objects given a point on the plane.
/// </summary>
class HashGrid
{
    private Dictionary<Vector2Int, List<GameObject>> hashGrid = new Dictionary<Vector2Int, List<GameObject>>();
    private int cellSize = 20;

    /// <summary>
    /// Construct hashgrid and initialize lists for every cell based the given plane size
    /// </summary>
    public HashGrid(Vector2 planeSize)
    {
        InitializeCells(planeSize);
    }

    /// <summary>
    /// Initializes the lists that will hold the <c>GameObjects</c>
    /// </summary>
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

    /// <summary>
    /// Computes the key for a given position
    /// </summary>
    internal Vector2Int GetCell(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int y = Mathf.FloorToInt(position.z / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Dictionary method wrapper. Returns a list of <c>GameObjects</c> for the given key
    /// </summary>
    internal bool TryGetValue(Vector2Int key, out List<GameObject> value)
    {
        return hashGrid.TryGetValue(key, out value);
    }

    /// <summary>
    /// Inserts a <c>GameObject</c> into the corresponding list based on its position on the plane
    /// </summary>
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
    [SerializeField] private float noiseScale = 5;
    [SerializeField] private Vector2 noiseOffset = Vector2.zero;

    // Generate new level
    [SerializeField] private bool recompute = false;

    // Clear terrain details
    [SerializeField] private bool clearPrefabs = false;
    [SerializeField] private bool randomizeNoise = true;

    // Safe zone prefab
    [SerializeField] private GameObject safeZone;

    // Assign point of progress prefab.
    [SerializeField] private GameObject pointOfProgress;
    // How many points should be placed
    [SerializeField] private int pointOfProgressCount = Consts.PILLAR_COUNT;
    
    // Prefab lists
    [SerializeField] private List<GameObject> layer1Assets = new List<GameObject>();
    [SerializeField] private List<GameObject> layer2Assets = new List<GameObject>();

    // Terrain color separation threshold
    [SerializeField] private float blendThreshold = 0.5f;

    // Prefab spawn chance
    [SerializeField] private float detailDensity = 0.005f;

    // How many of each object can fit between itself and it's nearest possible neighbor
    [SerializeField] private float safetyRadiusFactor = 2.0f;
    // How far away should POIs spawn from eachother as a factor of their extents
    [SerializeField] private float spreadFactor = 2.0f;

    [SerializeField] private GameObject marker;

    // Texture properties
    private int texWidth = 1024;
    private int texHeight = 1024;
    private Texture2D noiseTexture;

    // Track prefabs
    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    private List<GameObject> spawnedPillars = new List<GameObject>();
    private GameObject spawnedSafeZone;

    // Map prefabs into spatial cells
    private HashGrid hashGrid;

    private Vector2 planeSize;


    void Start()
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        planeSize = new Vector2(mr.bounds.size.x, mr.bounds.size.z);

        hashGrid = new HashGrid(planeSize);
        
        // Generate noise texture
        noiseTexture = GenerateGroundBlendTexture();
        // Pass noise texture to ground mix shader
        groundMaterial.SetTexture("_NoiseTexture", noiseTexture);

        // Spawn prefabs
        SpawnSafeZone();
        SpawnPointsOfProgress();
        SpawnPrefabsOnNoise();

        // DebugPrefabPosition(planeSize);
    }

    private void Update()
    {
        if (recompute)
        {
            Clear();
            noiseTexture = GenerateGroundBlendTexture();
            groundMaterial.SetTexture("_NoiseTexture", noiseTexture);
            SpawnPointsOfProgress();
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
        foreach (var obj in spawnedPillars)
        { 
            DestroyImmediate(obj);
        }
        spawnedPrefabs.Clear();
        spawnedPillars.Clear();
    }

    /// <summary>
    /// Generates a noise texture for the terrain.
    /// </summary>
    /// <remarks>
    /// Will be passed to a shader graph where it will be used as a mask
    /// to lerp two textures for the terrain.
    /// </remarks>
    private Texture2D GenerateGroundBlendTexture()
    {
        Texture2D texture = new Texture2D(texWidth, texHeight);
        
        if (randomizeNoise) RandomizeNoiseParams();

        for (int x = 0; x < texWidth; x++)
        {
            for (int y = 0; y < texHeight; y++)
            {
                Color color = GenerateNoiseColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Generates the color of a given pixel for the noise texture
    /// using the <c>Mathf.PerlinNoise</c> method.
    /// </summary>
    private Color GenerateNoiseColor (int x, int y)
    {
        float xSample = (float) x / texWidth * noiseScale + noiseOffset.x;
        float ySample = (float) y / texHeight * noiseScale + noiseOffset.y;

        float sample  = Mathf.PerlinNoise(xSample, ySample);
        return new Color(sample, sample, sample);
    }
    
    /// <summary>
    /// Sets the scale and offset to a random value
    /// </summary>
    private void RandomizeNoiseParams()
    {
        noiseScale = Random.Range(3.0f, 5.0f);
        noiseOffset = new Vector2(Random.Range(0f, 100.0f), Random.Range(0f, 100.0f));
    }
    
    private void SpawnSafeZone()
    {
        // Place safe zone at world origin
        spawnedSafeZone = Instantiate(safeZone, new Vector3(0, 0, 0), Quaternion.identity);
    }

    /// <summary>
    /// Places the points of progress on the terrain.
    /// </summary>
    private void SpawnPointsOfProgress()
    {
        // Padding (avoid placing points of interest on the map edges)
        Vector2 paddedPlaneSize = planeSize * 0.85f;

        for (int i = 0; i < pointOfProgressCount; i++)
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
                canSpawn = !CollisionTest(position, spawnedPillars, spreadFactor)
                    && !ProximityTest(position, spawnedSafeZone, 1.0f);
                retries--;
            }

            if (canSpawn)
            {
                GameObject newPillar = Instantiate(pointOfProgress, position, Quaternion.identity);
                spawnedPillars.Add(newPillar);
            }
        }

        // Update GameController stone reference list
        GameController.SetPillarList(spawnedPillars);
    }

    /// <summary>
    /// Places the terrain details depending on the texture type at the spawn point.
    /// </summary>
    /// <remarks>
    /// Iterates over every pixel in the noise texture, computes the corresponding world position
    /// and, depending on the color intensity, selects a random prefab from the appropriate list.
    /// Then tests for collisions (close proximity to other prefabs), randomizes its transform and places it.
    /// </remarks>
    private void SpawnPrefabsOnNoise()
    {
        noiseTexture = (Texture2D) groundMaterial.GetTexture("_NoiseTexture");

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                if (ShouldSpawn())
                {
                    // Normalize to UV coordinates
                    Vector2 normalizedPos = new Vector2(1 - (float) x / noiseTexture.width, 1 - (float) y / noiseTexture.height);

                    // Current world position
                    Vector3 position = new Vector3(
                        (normalizedPos.x - 0.5f) * planeSize.x,
                        0,
                        (normalizedPos.y - 0.5f) * planeSize.y
                    );

                    // Test collision with the safe zone
                    if (ProximityTest(position, spawnedSafeZone, 1.0f)) continue;
                    // Test collisions with points of progress
                    if (CollisionTest(position, spawnedPillars, safetyRadiusFactor)) continue;
                    // Test collisions with other detail prefabs
                    if (HashgridNeighborsCollisionTest(position)) continue;

                    // Get pixel color
                    Color color = noiseTexture.GetPixel(x, y);
                
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

    /// <summary>
    /// Tests neighboring hash grid cells to determine whether the current object is too close to
    /// any prefab that was previously placed on the ground.
    /// </summary>
    private bool HashgridNeighborsCollisionTest(Vector3 position)
    {
        // Determine which cell the given position belongs into
        Vector2Int cell = hashGrid.GetCell(position);

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

    /// <summary>
    /// Determines whether <paramref name="position"/> is too close to <paramref name="obj"/>.
    /// Too close meaning within a disc of radius equal to the object's collider bounds 
    /// extents multiplied by the given <paramref name="radiusFactor"/> 
    /// </summary>
    private bool ProximityTest(Vector3 position, GameObject obj, float radiusFactor)
    {
        Collider collider = obj.GetComponent<Collider>();

        // Half of the collider size
        float objExtent = collider.bounds.extents.magnitude;

        // Ignore safety margin if object is not a pillar
        if (!obj.CompareTag("Pillar"))
        {
            radiusFactor = 1;
        }

        if (Vector3.Distance(position, obj.transform.position) < (objExtent * radiusFactor))
            return true;

        return false;
    }

    /// <summary>
    /// Determines whether <paramref name="position"/> is too close to either object in <paramref name="collection"/>.
    /// Too close meaning within a disc of radius equal to the object's collider bounds 
    /// extents multiplied by the given <paramref name="radiusFactor"/>
    /// </summary>
    private bool CollisionTest(Vector3 position, List<GameObject> collection, float radiusFactor)
    {
        foreach (var obj in collection)
        {
            if (ProximityTest(position, obj, radiusFactor))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Randomize transform properties of a given prefab.
    /// Scale uniformly and rotate around the y axis.
    /// </summary>
    /// <param name="prefab"></param>
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

    /// <summary>
    /// Random chance that determines whether a terrain detail should be placed.
    /// </summary>
    private bool ShouldSpawn()
    {
        return Random.value <= detailDensity;
    }
    
    /// <summary>
    /// Select a random prefab from the appropriate list based on the given color 
    /// and the blend threshold.
    /// </summary>
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
