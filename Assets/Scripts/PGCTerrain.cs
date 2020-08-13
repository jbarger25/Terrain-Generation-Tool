using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PGCTerrain : MonoBehaviour
{
    public Terrain terrain;
    [SerializeField]
    public TerrainData myTerrainData;
    [SerializeField]
    public float randomHeightRange;
    public Vector3 heightMapScale;
    public Texture2D heightMapImage;
    float[,] heightMap;

    private static readonly int[] permutation = { 151,160,137,91,90,15,					// Hash lookup table as defined by Ken Perlin.  This is a randomly
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,	// arranged array of all numbers from 0-255 inclusive.
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180 };
    private static int[] p;
    //Perlin Variables
    public float perlinXScale = 1f;
    public float perlinYScale = 1f;
    public int perlinOctaves = 3;
    public float perlinPersistance = 0.5f;
    public float perlinHeightScale = 1f;
    public float perlinLacunarity = 3.0f;
    public int perlinSeedX = 0;
    public int perlinSeedY = 0;
    public bool remove = false;

    public int width = 128;
    public int height = 128;
    public int depth = 128;
    //Voronoi Varriables
    public float voronoiHeight;
    public float voronoiDist;
    public int voronoiAmount;
    //Sine Variables
    public float sinePeriod;
    public float sineAmplitude;
    public float sineAlignment = 1f;
    //Midpoint Variables
    public float midpointHeightRange = 1.0f;
    public float midpointSmoothness = 0.05f;
    public int midpointGrain = 5;

    void Start()
    {
        randomHeightRange = 0.0f;
        
    }

    void OnEnable()
    {
        terrain = this.GetComponent<Terrain>();
        myTerrainData = Terrain.activeTerrain.terrainData;
        myTerrainData.size = new Vector3(width, depth, height);
        myTerrainData.baseMapResolution = depth;
        Permutation();
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);
    }

    public void LoadTexture()
    {
        float[,] heightMap;
        heightMap = new float[myTerrainData.heightmapWidth, myTerrainData.heightmapHeight];
        for (int x = 0; x < myTerrainData.heightmapWidth; x++)
        {
            for(int z = 0; z < myTerrainData.heightmapHeight; z++)
            {
                heightMap[x, z] = heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    //Randomly set the terrain heights based on the range assigned in the editor
    public void RandomTerrain()
    {
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);

        for (int x = 0; x < myTerrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += Random.Range(-randomHeightRange, randomHeightRange);
            }
        }
        NormalizeHeightMap(heightMap);
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    //Set terrain heights in a sine wave based on Period and Amplitude. The wave's orientation can be changed with Alignment
    public void SineTerrain()
    {
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);    

        for (int x = 0; x < myTerrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapHeight; z++)
            {
                heightMap[x, z] += Mathf.Sin(((x*sineAlignment)+z)*(sinePeriod/10f))*sineAmplitude;
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    //Set Terrrain heights with a single pass of Ken Perlin's algorithm for Perlin Noise
    public void SinglePerlinTerrain()
    {
        float z = depth;
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);

        for (int x = 0; x < myTerrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < myTerrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += (MySinglePerlin(x, y, z) * (perlinHeightScale));
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    //Perlin function based on Ken Perlin's improved perlin noise algorithm
    float MySinglePerlin(float x, float y, float z)
    {
        float xCord = (float)x / width * perlinXScale + perlinSeedX;
        float yCord = (float)y / height * perlinYScale + perlinSeedY;
        float zCord = (float)z / depth * perlinHeightScale;
        x = xCord;
        y = yCord;

        int X = (int)x & 255;
        int Y = (int)y & 255;
        int Z = (int)z & 255;
        x -= (int)x;
        y -= (int)y;
        z -= (int)z;
        float u = x * x * x * (x * (x * 6 - 15) + 10);
        float v = y * y * y * (y * (y * 6 - 15) + 10);
        float w = z * z * z * (z * (z * 6 - 15) + 10);

        int A = p[X] + Y, AA = p[A] + Z, AB = p[A + 1] + Z, B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;
        

        return MyLerp(w, MyLerp(v, MyLerp(u, Grad(p[AA], x, y, z),  
                                     Grad(p[BA], x - 1, y, z)), 
                               MyLerp(u, Grad(p[AB], x, y - 1, z),  
                                     Grad(p[BB], x - 1, y - 1, z))),
                               MyLerp(v, MyLerp(u, Grad(p[AA + 1], x, y, z - 1),  
                                     Grad(p[BA + 1], x - 1, y, z - 1)), 
                               MyLerp(u, Grad(p[AB + 1], x, y - 1, z - 1),
                                     Grad(p[BB + 1], x - 1, y - 1, z - 1))));
       
    }
    //Helper function for Perlin Noise. Utilizes the permutation lookup array
    static void Permutation()
    {
        p = new int[512];
        for (int x = 0; x < p.Length; x++)
        {
            p[x] = permutation[x % 256];
        }
    }
    //Gradient helper function for Perrlin Noise
    static float Grad(int hash, float x, float y, float z)
    {
        int h = hash & 15;                      
        float u = h < 8 ? x : y;                 
        float v;

        if( h < 4)
        {
            v = y;
        }
        else if (h == 12 || h == 14)
        {
            v = x;
        }
        else
        {
            v = z;
        }

        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
    //Lerp Helper function for Perlin Noise
    static float MyLerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }
    //Sets the terrain heights based on multiple passes of Perlin Noise
    public void MultiplePerlinTerrain()
    {
        float z = depth;
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);
        for (int x = 0; x < myTerrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < myTerrainData.heightmapHeight; y++)
            {
                heightMap[x, y] += MultiplePerlin(x, y, z, perlinOctaves, perlinPersistance) * perlinHeightScale;
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
        
    }
    //Runs multiple passes of Perlin Noise for more realistically random values
    float MultiplePerlin(float x, float y, float z, int octaves, float persistance)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        for(int i = 0; i < octaves; i++)
        {
            total += MySinglePerlin(x * frequency, y * frequency, z * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= perlinLacunarity;
        }

        return total / maxValue;
    }
    //Attempts to set terrain heights to make Voronoi mountains. Randomly picks a coordinate and sets the value to the specified height
    public void VoronoiTerrain()
    {
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);
        int xCord, yCord;
        for(int i = 0; i < voronoiAmount; i++)
        {
            xCord = Random.Range(0, myTerrainData.heightmapWidth-1);
            yCord = Random.Range(0, myTerrainData.heightmapWidth-1);

            heightMap[xCord, yCord] += voronoiHeight;
            VoronoiSlope(xCord, yCord, heightMap);
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    //Sets the rest of the heights for Voronoi mountains by decreasing the specified height, setting the values in a circle around the previous value, and advancing to the specified radius
    void VoronoiSlope(int x, int y, float[,] heightMap)
    {
        float height = voronoiHeight;
        for (int i = 0; i < voronoiDist; i++)
        {
            height *= 0.9f;
            for (int j = 0; j <= 360; j++)
            {
                int xdisp = (Mathf.FloorToInt(i * Mathf.Cos(j*Mathf.Deg2Rad)));
                int xdisp2 = (Mathf.CeilToInt(i * Mathf.Cos(j * Mathf.Deg2Rad)));
                int ydisp = (Mathf.FloorToInt(i * Mathf.Sin(j*Mathf.Deg2Rad)));
                int ydisp2 = (Mathf.CeilToInt(i * Mathf.Sin(j * Mathf.Deg2Rad)));
                if (((x + xdisp) < myTerrainData.heightmapWidth) && ((y + ydisp) < myTerrainData.heightmapHeight))
                {
                    
                    heightMap[x + xdisp, y + ydisp] = NormalizeValue(height);

                }
                
            }
        }
    }
    //Reset the Terrain back to the base height
    public void ResetTerrain()
    {
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);

        for (int x = 0; x < myTerrainData.heightmapWidth; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapHeight; z++)
            {
                heightMap[x, z] = 0.5f;
            }
        }
        
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    //Squeeze values obtained by the various functions into the bounds of the terrain objects max an min height
    public void NormalizeHeightMap(float[,] heightMap)
    {
        float max = 20;
        float min = -20;

        for(int x = 0; x < myTerrainData.heightmapWidth; x++)
        {
            for (int y = 0; y < myTerrainData.heightmapHeight; y++) {
                float temp = (heightMap[x, y] - min) / (max - min);
                heightMap[x, y] = temp;
            }
        }
    }
    //Same as above but for a single value 
    public float NormalizeValue(float value)
    {
        float max = 20;
        float min = -20;

        
        float temp = (value - min) / (max - min);
        value = temp;
        return value;
    }

    //Set heights of terrain according to Midpoint Displacement/Diamond Square. Randonly sets the corners, then passes the heights to a recursive algorithm for the center, edge, and next square set calculations
    public void MidpointTerrain()
    {
        heightMap = myTerrainData.GetHeights(0, 0, myTerrainData.heightmapWidth, myTerrainData.heightmapHeight);
        float width = myTerrainData.heightmapWidth;
        float height = myTerrainData.heightmapHeight;
        float vertical = midpointHeightRange;

        float corner1 = Random.Range(-vertical, vertical);
        float corner2 = Random.Range(-vertical, vertical);
        float corner3 = Random.Range(-vertical, vertical);
        float corner4 = Random.Range(-vertical, vertical);

        MidpointRecursive(heightMap, 0, 0, width, height, corner1, corner2, corner3, corner4);
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    //Recursive step if Midpoint Displacement. Takes the corner values to find the center and edges, then passes new values to next smaller set of squares
    public void MidpointRecursive(float[,] map, float x, float y, float width, float height, float c1, float c2, float c3, float c4)
    {
        float newWidth = width * 0.5f;
        float newHeight = height * 0.5f;

        if(width < 1.0f || height < 1.0f)
        {
            float center = (c1 + c2 + c3 + c4) * 0.25f;
            map[(int)x, (int)y] = center;
        }
        else
        {
            float middle = (c1 + c2 + c3 + c4) * 0.25f + (Random.Range(-midpointSmoothness, midpointSmoothness) * ((newWidth*newHeight)/myTerrainData.heightmapWidth*midpointGrain)); 
            float edge1 = (c1 + c2) * 0.5f; 
            float edge2 = (c2 + c3) * 0.5f;
            float edge3 = (c3 + c4) * 0.5f;
            float edge4 = (c4 + c1) * 0.5f;

            if (middle <= 0)
            {
                middle = 0;
            }
            else if (middle > 1.0f)
            {
                middle = 1.0f;
            }

            MidpointRecursive(map, x, y, newWidth, newHeight, c1, edge1, middle, edge4);
            MidpointRecursive(map, x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
            MidpointRecursive(map, x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
            MidpointRecursive(map, x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
        }


    }
    //Function for Normalizing Terrain via editor ui if values get set too high
    public void NormalizeTerrain()
    {  
        NormalizeHeightMap(heightMap);
        myTerrainData.SetHeights(0, 0, heightMap);

    }
}
