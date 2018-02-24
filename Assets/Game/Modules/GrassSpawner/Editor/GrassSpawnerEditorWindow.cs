using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;

class GrassSpawnerEditorWindow : EditorWindow
{
    [MenuItem("Dynamic Grass/Spawner")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GrassSpawnerEditorWindow));
    }

    public GrassSpawnerEditorWindow Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Transform grassStraw;    
    public int straws = 100;
    public float scale = 1;
    public float scaleVariance = .5f;
    public float rotationOffset = 0;
    public float rotationVariation = 360;
    
    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        grassStraw = (Transform)EditorGUILayout.ObjectField("Grass Model", grassStraw, typeof(Transform), true);
        straws = EditorGUILayout.IntSlider("Amount", straws, 1, 1000);
        scale = EditorGUILayout.Slider("Scale", scale, 0, 25);
        scaleVariance = EditorGUILayout.Slider("Scale Variance", scaleVariance, 0, 2);
        rotationOffset = EditorGUILayout.Slider("Rotation Offset", rotationOffset, 0, 360);
        rotationVariation = EditorGUILayout.Slider("Rotation Variation", rotationVariation, 0, 360);
        if (EditorGUI.EndChangeCheck())
        {
            OnValueChange();
        }

        if (Selection.activeTransform)
        {
            EditorGUILayout.LabelField(Selection.activeTransform.gameObject.name);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spawn Grass"))
            {
                SpawnGrassOnObject(Selection.activeTransform);
            }
            if (GUILayout.Button("Clear Grass"))
            {
                ClearGrassObject(Selection.activeTransform);
            }
            GUILayout.EndHorizontal();
        }
    }

    void OnValueChange()
    {
        if (Selection.activeTransform)
        {
            SpawnGrassOnObject(Selection.activeTransform);
        }
    }

    void ClearGrassObject(Transform transform)
    {
        Transform grassTransform = transform.Find("Grass");
        if (grassTransform != null)
        {
            DestroyImmediate(grassTransform.gameObject);
        }
    }

    void SpawnGrassOnObject(Transform transform)
    {
        Mesh mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;
        int numberOfTris = triangles.Length / 3;
        float[] triangleAreas = new float[numberOfTris];
        float[] cumulativeArea = new float[numberOfTris];
        float cumulativeFloat = 0;
        Transform grassTransform = transform.Find("Grass");
        if (grassTransform != null)
            DestroyImmediate(grassTransform.gameObject);
        GameObject grass = new GameObject("Grass");
        grass.transform.parent = transform;

        for (int i = 0; i < numberOfTris; i++)
        {
            float length_a = Vector3.Distance(vertices[triangles[i * 3]], vertices[triangles[i * 3 + 1]]);
            float length_b = Vector3.Distance(vertices[triangles[i * 3]], vertices[triangles[i * 3 + 2]]);
            float length_c = Vector3.Distance(vertices[triangles[i * 3 + 2]], vertices[triangles[i * 3 + 1]]);
            float s = (length_a + length_b + length_c) / 2;
            triangleAreas[i] = Mathf.Sqrt(s * (s - length_a) * (s - length_b) * (s - length_c));
            cumulativeFloat += triangleAreas[i];
            cumulativeArea[i] = cumulativeFloat;
        }

        for (int i = 0; i < straws; i++)
        {
            float rand = UnityEngine.Random.Range(0, cumulativeFloat);
            int index = Array.BinarySearch(cumulativeArea, rand);
            int randomTri = (-index - 1) * 3;
            Vector3 PointA = vertices[triangles[randomTri]];
            Vector3 PointB = vertices[triangles[randomTri + 1]];
            Vector3 PointC = vertices[triangles[randomTri + 2]];

            Vector3 NormA = normals[triangles[randomTri]];
            Vector3 NormB = normals[triangles[randomTri + 1]];
            Vector3 NormC = normals[triangles[randomTri + 2]];

            float randA = UnityEngine.Random.value;
            float randB = UnityEngine.Random.value;
            float randC = UnityEngine.Random.value;

            Vector3 randomPos = new Vector3((PointA.x * randA + PointB.x * randB + PointC.x * randC) / (randA + randB + randC), (PointA.y * randA + PointB.y * randB + PointC.y * randC) / (randA + randB + randC), (PointA.z * randA + PointB.z * randB + PointC.z * randC) / (randA + randB + randC));
            Vector3 randomNorm = new Vector3((NormA.x * randA + NormB.x * randB + NormC.x * randC) / (randA + randB + randC), (NormA.y * randA + NormB.y * randB + NormC.y * randC) / (randA + randB + randC), (NormA.z * randA + NormB.z * randB + NormC.z * randC) / (randA + randB + randC));

            Transform clone = GameObject.Instantiate(grassStraw, randomPos, Quaternion.LookRotation(randomNorm) * Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, rotationOffset + UnityEngine.Random.Range(-rotationVariation, rotationVariation) / 2, 0), grass.transform);
            float randScale = UnityEngine.Random.Range(-scaleVariance, scaleVariance) / 2;
            clone.localScale = new Vector3(scale + randScale, scale + randScale, scale + randScale);
            clone.name = "Grass Straw";
        }
        grass.transform.position = transform.position;
        grass.transform.rotation = transform.rotation;
    }
}