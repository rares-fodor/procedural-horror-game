using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class DemonstrativeLevelGenerator : MonoBehaviour
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
    [SerializeField] private GameObject originPoint;

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
    private GameObject spawnedOriginPoint;

    // Map prefabs into spatial cells
    private HashGrid hashGrid;

    private Vector2 planeSize;

    public static Vector3 planeExtents;

    private void Start()
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        planeSize = new Vector2(mr.bounds.size.x, mr.bounds.size.z);

        hashGrid = new HashGrid(planeSize);

        // Generate noise texture
        noiseTexture = GenerateGroundBlendTexture();
        // Pass noise texture to ground mix shader
        groundMaterial.SetTexture("_NoiseTexture", noiseTexture);

        // Spawn prefabs
        SpawnOriginPoint();
        SpawnPointsOfProgress();
        SpawnPrefabsOnNoise();
    }

    void Awake()
    {
        planeExtents = GetComponent<MeshRenderer>().bounds.extents;
    }

    private void Update()
    {
        if (recompute || Input.GetKeyDown(KeyCode.Space))
        {
            Clear();
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            planeSize = new Vector2(mr.bounds.size.x, mr.bounds.size.z);

            hashGrid = new HashGrid(planeSize);
            noiseTexture = GenerateGroundBlendTexture();
            groundMaterial.SetTexture("_NoiseTexture", noiseTexture);
            SpawnOriginPoint();
            SpawnPointsOfProgress();
            SpawnPrefabsOnNoise();
            recompute = false;
        }
        if (clearPrefabs)
        {
            Clear();
            clearPrefabs = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
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
        DestroyImmediate(spawnedOriginPoint);
        
        spawnedPrefabs.Clear();
        spawnedPillars.Clear();
    }


    // Generates a noise texture for the terrain.
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

    // Generates the color of a given pixel for the noise texture
    private Color GenerateNoiseColor(int x, int y)
    {
        float xSample = (float)x / texWidth * noiseScale + noiseOffset.x;
        float ySample = (float)y / texHeight * noiseScale + noiseOffset.y;

        float sample = Mathf.PerlinNoise(xSample, ySample);
        return new Color(sample, sample, sample);
    }

    private void RandomizeNoiseParams()
    {
        noiseScale = Random.Range(3.0f, 5.0f);
        noiseOffset = new Vector2(Random.Range(0f, 100.0f), Random.Range(0f, 100.0f));
    }

    private void SpawnOriginPoint()
    {
        // Place safe zone at world origin
        spawnedOriginPoint = Instantiate(originPoint, new Vector3(0, 0, 0), Quaternion.identity);
    }

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
                // Test proximity to the other pillars and to the center zone.
                canSpawn = !ProximityTest(position, spawnedPillars, spreadFactor)
                    && !ProximityTest(position, spawnedOriginPoint, 1.0f);
                retries--;
            }

            if (canSpawn)
            {
                GameObject pillarObject = Instantiate(pointOfProgress, position, Quaternion.identity);
                spawnedPillars.Add(pillarObject);
            }
        }
    }

    private void SpawnPrefabsOnNoise()
    {
        noiseTexture = (Texture2D)groundMaterial.GetTexture("_NoiseTexture");

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                if (ShouldSpawn())
                {
                    // Normalize to UV coordinates
                    Vector2 normalizedPos = new Vector2(1 - (float)x / noiseTexture.width, 1 - (float)y / noiseTexture.height);

                    // Current world position
                    Vector3 position = new Vector3(
                        (normalizedPos.x - 0.5f) * planeSize.x,
                        0,
                        (normalizedPos.y - 0.5f) * planeSize.y
                    );

                    // Test collision with the safe zone
                    if (ProximityTest(position, spawnedOriginPoint, 1.0f)) continue;
                    // Test collisions with points of progress
                    if (ProximityTest(position, spawnedPillars, safetyRadiusFactor)) continue;
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
                    if (ProximityTest(position, prefabsInCell, 1.0f))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool ProximityTest(Vector3 position, GameObject obj, float radiusFactor)
    {
        Collider collider = obj.GetComponent<Collider>();

        // Half of the collider size
        float objExtent = collider.bounds.extents.magnitude;

        if (Vector3.Distance(position, obj.transform.position) < (objExtent * radiusFactor))
            return true;

        return false;
    }

    private bool ProximityTest(Vector3 position, List<GameObject> collection, float radiusFactor)
    {
        foreach (var obj in collection)
        {
            if (ProximityTest(position, obj, radiusFactor))
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


    private GameObject SelectPrefab(Color color)
    {
        if (color.grayscale > blendThreshold)
        {
            return layer2Assets.ElementAt(Random.Range(0, layer2Assets.Count));
        }
        else
        {
            return layer1Assets.ElementAt(Random.Range(0, layer1Assets.Count));
        }
    }

}
