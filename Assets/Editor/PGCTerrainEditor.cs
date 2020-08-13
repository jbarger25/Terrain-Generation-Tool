using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PGCTerrain))]
[CanEditMultipleObjects]
public class PGCTerrainEditor : Editor
{
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty PerlinXScale;
    SerializedProperty PerlinYScale;
    SerializedProperty PerlinSeedX;
    SerializedProperty PerlinSeedY;
    SerializedProperty PerlinOctaves;
    SerializedProperty PerlinPersistance;
    SerializedProperty PerlinHeightScale;
    SerializedProperty PerlinLacunarity;

    SerializedProperty voronoiHeight;
    SerializedProperty voronoiDist;
    SerializedProperty voronoiAmount;

    SerializedProperty sinePeriod;
    SerializedProperty sineAmplitude;
    SerializedProperty sineAlignment;

    SerializedProperty midpointRange;
    SerializedProperty midpointGrain;
    [Range(0f, 1f)]
    SerializedProperty midpointSmoothness;

    bool showRandom = false;
    bool showVoronoi = false;
    bool showSine = false;
    bool showPerlin = false;
    bool showMultiPerlin = false;
    bool showMidpoint = false;

    void Start()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }
    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        PerlinXScale = serializedObject.FindProperty("perlinXScale");
        PerlinYScale = serializedObject.FindProperty("perlinYScale");
        PerlinSeedX = serializedObject.FindProperty("perlinSeedX");
        PerlinSeedY = serializedObject.FindProperty("perlinSeedY");
        PerlinOctaves = serializedObject.FindProperty("perlinOctaves");
        PerlinPersistance = serializedObject.FindProperty("perlinPersistance");
        PerlinHeightScale = serializedObject.FindProperty("perlinHeightScale");
        PerlinLacunarity = serializedObject.FindProperty("perlinLacunarity");

        voronoiHeight = serializedObject.FindProperty("voronoiHeight");
        voronoiDist = serializedObject.FindProperty("voronoiDist");
        voronoiAmount = serializedObject.FindProperty("voronoiAmount");

        sinePeriod = serializedObject.FindProperty("sinePeriod");
        sineAmplitude = serializedObject.FindProperty("sineAmplitude");
        sineAlignment = serializedObject.FindProperty("sineAlignment");

        midpointRange = serializedObject.FindProperty("midpointHeightRange");
        midpointGrain = serializedObject.FindProperty("midpointGrain");
        midpointSmoothness = serializedObject.FindProperty("midpointSmoothness");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PGCTerrain terrain = (PGCTerrain)target;

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        showSine = EditorGUILayout.Foldout(showSine, "Sine");
        showPerlin = EditorGUILayout.Foldout(showPerlin, "Perlin");
        showMultiPerlin = EditorGUILayout.Foldout(showMultiPerlin, "Multiple Perlin");
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        showMidpoint = EditorGUILayout.Foldout(showMidpoint, "Midpoint Displacement Soothing");

        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(randomHeightRange);

            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }
        serializedObject.ApplyModifiedProperties();
        if (showSine)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Set Heights By Sine Values", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(sinePeriod);
            EditorGUILayout.PropertyField(sineAmplitude);
            EditorGUILayout.PropertyField(sineAlignment);

            if (GUILayout.Button("Sine Heights"))
            {
                terrain.SineTerrain();
            }
        }
        serializedObject.ApplyModifiedProperties();
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Set Heights By Single Perlin", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(PerlinXScale);
            EditorGUILayout.PropertyField(PerlinYScale);
            EditorGUILayout.PropertyField(PerlinHeightScale);
            EditorGUILayout.PropertyField(PerlinSeedX);
            EditorGUILayout.PropertyField(PerlinSeedY);


            if (GUILayout.Button("Single Perlin"))
            {
                terrain.SinglePerlinTerrain();
            }
        }
        serializedObject.ApplyModifiedProperties();
        if (showMultiPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Set Heights By Multiple Perlin", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(PerlinXScale);
            EditorGUILayout.PropertyField(PerlinYScale);
            EditorGUILayout.PropertyField(PerlinHeightScale);
            EditorGUILayout.PropertyField(PerlinSeedX);
            EditorGUILayout.PropertyField(PerlinSeedY);
            EditorGUILayout.PropertyField(PerlinOctaves);
            EditorGUILayout.PropertyField(PerlinPersistance);
            EditorGUILayout.PropertyField(PerlinLacunarity); 

            if (GUILayout.Button("Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }
        }
        serializedObject.ApplyModifiedProperties();
        if (showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Set Voronoi Heights", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(voronoiAmount);
            EditorGUILayout.PropertyField(voronoiDist);
            EditorGUILayout.PropertyField(voronoiHeight);

            if (GUILayout.Button("Voronoi Heights"))
            {
                terrain.VoronoiTerrain();
            }
        }
        serializedObject.ApplyModifiedProperties();

        if (showMidpoint)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Set Midpoint Heights", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(midpointRange);
            EditorGUILayout.PropertyField(midpointSmoothness);
            EditorGUILayout.PropertyField(midpointGrain);

            if (GUILayout.Button("Midpoint Heights"))
            {
                terrain.MidpointTerrain();
            }
        }
        serializedObject.ApplyModifiedProperties();
        GUILayout.Label("Reset Terrain Heights", EditorStyles.boldLabel);
        if (GUILayout.Button("Reset"))
        {
            terrain.ResetTerrain();
        }
        GUILayout.Label("Normalize Terrain Heights", EditorStyles.boldLabel);
        if (GUILayout.Button("Normalize"))
        {
            terrain.NormalizeTerrain();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
