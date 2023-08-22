using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

    // Texture properties
    private int width = 1024;
    private int height = 1024;
    private Texture2D noise_texture;

    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    private List<GameObject> spawnedPointsOfInterest = new List<GameObject>();

    void Start()
    {
        noise_texture = GenerateGroundBlendTexture();
        groundMaterial.SetTexture("_NoiseTexture", noise_texture);
        SpawnPointsOfInterest();
        SpawnPrefabsOnNoise();
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
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        Vector2 planeSize = new Vector2(mr.bounds.size.x, mr.bounds.size.z);

        // Padding (avoid placing points of interest on the map edges)
        planeSize *= 0.85f;

        foreach (var obj in pointsOfInterest)
        {
            bool canSpawn = false;
            int retries = 100;
            Vector3 position = Vector3.zero;

            while (!canSpawn && retries != 0)
            {
                position = new Vector3(
                    Random.Range(-planeSize.x / 2, planeSize.x / 2),
                    0,
                    Random.Range(-planeSize.y / 2, planeSize.y / 2)
                );
                canSpawn = !CollisionTest(position, spawnedPointsOfInterest, spreadFactor);
                retries--;
            }

            if (canSpawn)
                spawnedPointsOfInterest.Add(Instantiate(obj, position, Quaternion.identity));
        }
    }

    private bool CollisionTest(Vector3 position, List<GameObject> collection, float radiusFactor)
    {
        foreach (var obj in collection)
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider == null) continue;

            // Add the safety radius to the objects extent
            // i.e. half of it's collider's size + safety radius
            float objExtent = collider.bounds.extents.magnitude;

            if (Vector3.Distance(position, obj.transform.position) < (objExtent * radiusFactor))
                return true;
        }
        return false;
    }

    private void SpawnPrefabsOnNoise()
    {
        noise_texture = (Texture2D) groundMaterial.GetTexture("_NoiseTexture");

        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        Vector2 planeSize = new Vector2(mr.bounds.size.x, mr.bounds.size.z);

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

                    if (CollisionTest(position, spawnedPointsOfInterest, safetyRadiusFactor)) continue;

                    // Spawn logic
                    Color color = noise_texture.GetPixel(x, y);
                
                    // Choose prefab to spawn
                    GameObject prefab = SelectPrefab(color);

                    // Spawn
                    spawnedPrefabs.Add(Instantiate(prefab, position, Quaternion.identity));
                }
            }
        }
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
}
