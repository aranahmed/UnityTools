using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.Splines;
using System.Collections.Generic;

public class SplineObjectPlacerEditor : EditorWindow
{
    // GUI Styles to be reused across scripts // Probably put this in a seperate Interface to be used across Editor scri

    private GUIStyle smallFloatFieldStyle;
    
    
    // spline and object data
    [Tooltip("Container for the spline along which objects will be placed.")]
    private SplineContainer splineContainer; 
    [Tooltip("List of object prefabs to be scattered along the spline.")]
    private List<GameObject> objectPrefabs = new List<GameObject>(); 
    [Tooltip("List of secondary object prefabs to be scattered around the primary objects.")]
    private List<GameObject> secondaryObjectPrefabs = new List<GameObject>(); 
    private Terrain terrain; 

    // placement stuff
    private int objectCount = 10; 
    private int secondaryMeshCount = 10; 
    private float secondaryMeshRadius = 3f;
    private float secondaryMeshScaleMin = 3f;
    private float secondaryMeshScaleMax = 5f;
    private float spacing = 1f; 

    // orientation
    public enum UpAxis { X, Y, Z }
    private UpAxis objectUpAxis = UpAxis.Y;

    // randomness
    public enum RandomizeAxis { None, X, Y, Z }
    private RandomizeAxis randomizeAxis = RandomizeAxis.None;
    private RandomizeAxis randomizeAxisSecondaryObject = RandomizeAxis.None;

    // secondary mesh options
    private bool showSecondaryMeshOptions = false;
    private bool livePreview = false;
    private Vector2 scrollPos;
    private Material previewMaterial;
    private int randomSeed = 0;

    // internal lists
    private List<GameObject> placedObjects = new List<GameObject>();
    private List<GameObject> previewObjects = new List<GameObject>();

    // Editor preferences Keys - i think dictionary would work better than this
    private const string SplineContainerKey = "SplineObjectPlacerEditor_SplineContainer";
    
    private const string LivePreviewKey = "SplineObjectPlacer_LivePreview";
    private const string ObjectCountKey = "SplineObjectPlacer_ObjectCount";
    private const string SpacingKey = "SplineObjectPlacer_Spacing";
    private const string UpAxisKey = "SplineObjectPlacer_UpAxis";
    private const string RandomizeAxisKey = "SplineObjectPlacer_RandomizeAxis";
    private const string RandomSeedKey = "SplineObjectPlacer_RandomSeed";
    
    // show window
    [MenuItem("Tools/Spline Tools/Spline Object Placer")]
    public static void ShowWindow()
    {
        GetWindow<SplineObjectPlacerEditor>("Spline Object Placer");
    }

    // when the script "starts"
    private void OnEnable()
    {
        // creating GUI style to be reused across my float fields
        smallFloatFieldStyle = new GUIStyle(EditorStyles.numberField);
        smallFloatFieldStyle.fixedWidth = 500;
        smallFloatFieldStyle.alignment = TextAnchor.MiddleCenter;
        smallFloatFieldStyle.normal.background = EditorStyles.numberField.normal.background;
        smallFloatFieldStyle.hover.background = EditorStyles.numberField.hover.background;
        smallFloatFieldStyle.focused.background = EditorStyles.numberField.focused.background;
        smallFloatFieldStyle.active.background = EditorStyles.numberField.active.background;

       
        
        
        SceneView.duringSceneGui += OnSceneGUI;
        LoadSettings();
    }

    // window closes
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        livePreview = false;
        RemovePreviewObjects();
        SaveSettings();
    }

    // GUI renderer
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Main Header
        GUILayout.Label("Spline Object Placer", EditorStyles.boldLabel);

        // Spline and Terrain Inputs
        splineContainer = (SplineContainer)EditorGUILayout.ObjectField(new GUIContent("Spline Container","Container for the spline along which objects will be placed."), splineContainer, typeof(SplineContainer), true);
        if (splineContainer != null && GUILayout.Button("Select Spline Container"))
        {
            Selection.activeObject = splineContainer;
        }
        
        
        terrain = (Terrain)EditorGUILayout.ObjectField(new GUIContent("Terrain","Place Terrain Object from scene in here"), terrain, typeof(Terrain), true);

        // Object Prefab List
        DrawObjectPrefabsList();

      

        // Placement Settings
        objectCount = EditorGUILayout.IntSlider(new GUIContent("Object Count","Amount of Primary Objects you will scatter(Save Recommended)"), objectCount, 1, 100);
        
        spacing = EditorGUILayout.FloatField(new GUIContent("Object Spacing","Space between each object. If you cant see as any objects as you expect, you need to lower this value"), spacing, GUILayout.Width(EditorGUIUtility.fieldWidth - 4),  GUILayout.ExpandWidth(true));
        
        
        EditorGUILayout.BeginHorizontal(); 
        
        EditorGUILayout.LabelField(new GUIContent("Object Up Axis", "Specifies the Up Axis of your mesh"), GUILayout.Width(EditorGUIUtility.labelWidth - 4));
        
        objectUpAxis = (UpAxis)EditorGUILayout.EnumPopup( objectUpAxis);
        
        EditorGUILayout.EndHorizontal();
        
        // So I want to make it so the text fits and then the enum popup on the right is sent to stick to the right
        EditorGUILayout.BeginHorizontal();
        
        // Create the label with tooltip on the left
        EditorGUILayout.LabelField(new GUIContent("Primary Random Rotation Axis", "Applies random rotation to the mesh in specified Axis"), GUILayout.Width(EditorGUIUtility.labelWidth - 4));
        
        // Create the enum popup and align it to the right
        randomizeAxis = (RandomizeAxis)EditorGUILayout.EnumPopup(randomizeAxis);

        EditorGUILayout.EndHorizontal();
        
        randomSeed = EditorGUILayout.IntField(new GUIContent("Random Seed","Applies random seed to the mesh. Use Live Preview to check what it'll look like"), randomSeed);

        // Advanced Options
        livePreview = EditorGUILayout.Toggle(new GUIContent("Live Preview","Shows preview of the mesh. You can also move the splines to see the result. (Turn off when not using, as it is performance heavy)"), livePreview);
        showSecondaryMeshOptions = EditorGUILayout.Foldout(showSecondaryMeshOptions, new GUIContent("Secondary Mesh Options","Shows Secondary Mesh Options"));
        
        if (showSecondaryMeshOptions)
        {
            DrawSecondaryMeshOptions();
        }

        // Actions
        DrawActions();

        EditorGUILayout.EndScrollView();
        
        if (secondaryObjectPrefabs == null || secondaryObjectPrefabs.Count == 0 || secondaryObjectPrefabs.TrueForAll(prefab => prefab == null))
        {
            EditorGUILayout.HelpBox("You have no secondary meshes", MessageType.Info);
        }
    }
    
    private void DrawObjectPrefabsList()
    {
        // Field for the object prefabs
        EditorGUILayout.LabelField("Object Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Object Prefab"))
        {
            objectPrefabs.Add(null);
        }
        // for each prefab in the list, it will add a field for a GO type
        for (int i = 0; i < objectPrefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            objectPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(objectPrefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                // removes the object prefab
                objectPrefabs.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSecondaryObjectPrefabsList()
    {
        // pretty much the same as the object prefabs
        EditorGUILayout.LabelField("Secondary Mesh Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Secondary Mesh Prefab"))
        {
            secondaryObjectPrefabs.Add(null);
        }

        for (int i = 0; i < secondaryObjectPrefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            secondaryObjectPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(secondaryObjectPrefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                secondaryObjectPrefabs.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSecondaryMeshOptions()
    {
        // Secondary Object Prefab List
        DrawSecondaryObjectPrefabsList();
        secondaryMeshCount = EditorGUILayout.IntSlider(
            new GUIContent("Secondary Mesh Count","Amount of Secondary Objects you will scatter(Save Recommended)"),
            secondaryMeshCount, 1, 100);
        GUILayout.Label("Secondary Mesh Settings", EditorStyles.boldLabel);
        
       
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Secondary Mesh Radius", "Radius around the primary mesh, the secondary mesh will spawn"));
        secondaryMeshRadius = EditorGUILayout.FloatField(secondaryMeshRadius);
        EditorGUILayout.EndHorizontal();
        
        
        
        using (new GUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(new GUIContent("Secondary Mesh Scale Minimum", "Minimum size of secondary mesh"));
            secondaryMeshScaleMin = EditorGUILayout.FloatField(secondaryMeshScaleMin); 
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Secondary Mesh Scale Maximum","Maximum size of secondary mesh"));
        secondaryMeshScaleMax = EditorGUILayout.FloatField(secondaryMeshScaleMax);
        EditorGUILayout.EndHorizontal();
        
        randomizeAxisSecondaryObject = (RandomizeAxis)EditorGUILayout.EnumPopup("Secondary Rotation Axis", randomizeAxisSecondaryObject);
    }
    
    // Comeback to this ** Learn how to do abstraction properly 

    // private void DrawHorizontalBox(string labelFieldString,string toolTip, float floatField)
    // {
    //     EditorGUILayout.BeginHorizontal();
    //     EditorGUILayout.LabelField( labelFieldString, toolTip);
    //     floatField = EditorGUILayout.FloatField(floatField);
    //     EditorGUILayout.EndHorizontal();
    // }
    //

    private void DrawActions()
    {
        if (GUILayout.Button("Place Objects"))
        {
            if (splineContainer != null && objectPrefabs.Count > 0 && terrain != null)
            {
                RemovePlacedObjects();
                PlaceObjectsAlongSpline(finalPlacement: true);
            }
            else
            {
                Debug.LogWarning("Fill in required fields!");
            }
        }

        if (GUILayout.Button("Clear Placed Objects"))
        {
            RemovePlacedObjects();
        }
    }

    // Scene View Preview
    private void OnSceneGUI(SceneView sceneView)
    {
        if (livePreview && splineContainer != null && objectPrefabs.Count > 0 && terrain != null)
        {
            PlaceObjectsAlongSpline(finalPlacement: false);
            SceneView.RepaintAll();
        }
        else
        {
            RemovePreviewObjects();
        }
    }

    // Object Placement Logic
    private void PlaceObjectsAlongSpline(bool finalPlacement)
    {
        if (finalPlacement)
        {
            RemovePlacedObjects();
        }
        else
        {
            RemovePreviewObjects();
        }

        float splineLength = splineContainer.CalculateLength();
        int numberOfObjects = Mathf.Min(objectCount, Mathf.FloorToInt(splineLength / spacing));
        System.Random rand = new System.Random(randomSeed);

        for (int i = 0; i < numberOfObjects; i++)
        {
            float t = (float)i / (numberOfObjects - 1);
            Vector3 splinePosition = splineContainer.EvaluatePosition(t);

            if (Physics.Raycast(splinePosition + Vector3.up * 100, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 terrainNormal = hit.normal;
                Quaternion rotation = GetRotationForUpAxis(terrainNormal);
                rotation = ApplyRandomRotation(rotation, rand, randomizeAxis);

                GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
                GameObject placedObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                placedObject.transform.position = hit.point;
                placedObject.transform.rotation = rotation;
                
                // marker script for removing the objects when reloading unity or changing scenes etc.
                // Adds the marker to idenify it by the remove script
                PlacedBySplineObjectPlacer marker = placedObject.AddComponent<PlacedBySplineObjectPlacer>();
                marker.toolIdentifier = "SplineObjectPlacerEditor";
                marker.placementIndex = i; // store the placement as its own index

                if (finalPlacement)
                {
                    Undo.RegisterCreatedObjectUndo(placedObject, "Place Object Along Spline");
                    placedObjects.Add(placedObject);
                    PlaceSecondaryObjects(placedObject.transform.position, rotation, finalPlacement, rand);
                }
                else
                {
                    ApplyPreviewMaterial(placedObject);
                    previewObjects.Add(placedObject);
                }

                
            }
        }
    }

    private void PlaceSecondaryObjects(Vector3 primaryPosition, Quaternion primaryRotation, bool finalPlacement, System.Random rand)
    {
        // Secondary object validation
        if (secondaryObjectPrefabs == null || secondaryObjectPrefabs.Count == 0 || secondaryObjectPrefabs.TrueForAll(prefab => prefab == null))
        {
            return; // if no secondary object in prefab space, it will just do the priamry objects
        }
        for (int j = 0; j < secondaryMeshCount; j++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * secondaryMeshRadius;
            Vector3 secondaryPosition = primaryPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

            if (Physics.Raycast(secondaryPosition + Vector3.up * 100, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 terrainNormal = hit.normal;
                Quaternion rotation = GetRotationForUpAxis(terrainNormal);
                rotation = ApplyRandomRotation(rotation, rand, randomizeAxisSecondaryObject);

                GameObject secondaryPrefab = secondaryObjectPrefabs[Random.Range(0, secondaryObjectPrefabs.Count)];
                GameObject placedSecondaryObject = (GameObject)PrefabUtility.InstantiatePrefab(secondaryPrefab);
                placedSecondaryObject.transform.position = hit.point;
                placedSecondaryObject.transform.rotation = rotation;
                float randomScale = Random.Range(secondaryMeshScaleMin, secondaryMeshScaleMax);
                placedSecondaryObject.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                
                // marker script for removing the objects when reloading unity or changing scenes etc.
                // Adds the marker to idenify it by the remove script
                PlacedBySplineObjectPlacer marker = placedSecondaryObject.AddComponent<PlacedBySplineObjectPlacer>();
                marker.toolIdentifier = "SplineObjectPlacerEditor";
                marker.placementIndex = j; // store the placement as its own index

                if (finalPlacement)
                {
                    Undo.RegisterCreatedObjectUndo(placedSecondaryObject, "Place Secondary Object");
                    placedObjects.Add(placedSecondaryObject);
                }
                else
                {
                    //ApplyPreviewMaterial(placedSecondaryObject);
                    previewObjects.Add(placedSecondaryObject);
                }
            }
        }
    }

    // Helper Functions
    private Quaternion GetRotationForUpAxis(Vector3 terrainNormal)
    {
        switch (objectUpAxis)
        {
            case UpAxis.X:
                return Quaternion.LookRotation(Vector3.forward, terrainNormal);
            case UpAxis.Z:
                return Quaternion.LookRotation(Vector3.right, terrainNormal);
            case UpAxis.Y:
            default:
                return Quaternion.LookRotation(Vector3.forward, terrainNormal);
        }
    }

    private Quaternion ApplyRandomRotation(Quaternion rotation, System.Random rand, RandomizeAxis randomAxis)
    {
        if (randomAxis == RandomizeAxis.None)
            return rotation;
        
        float randomAngle = (float)(rand.NextDouble() * 360);
        Vector3 axis = Vector3.up;
        
        switch (randomAxis)
        {
            case RandomizeAxis.X:
                axis =  Vector3.right;
                break;
            case RandomizeAxis.Y:
                axis =  Vector3.up;
                break;
            case RandomizeAxis.Z:
                axis =  Vector3.forward;
                break;
                
        }

        return rotation * Quaternion.AngleAxis(randomAngle, axis);
    }

    private void ApplyPreviewMaterial(GameObject obj)
    {
        if (previewMaterial == null)
        {
            previewMaterial = new Material(Shader.Find("Custom/WireframeShader")) { color = new Color(1f, 1f, 1f, 1f) };
        }

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = previewMaterial;
        }
    }

    private void RemovePreviewObjects()
    {
        foreach (GameObject previewObject in previewObjects)
        {
            DestroyImmediate(previewObject);
        }
        previewObjects.Clear();
    }

    // private void RemovePlacedObjects()
    // {
    //     foreach (GameObject placedObject in placedObjects)
    //     {
    //         Undo.DestroyObjectImmediate(placedObject);
    //     }
    //     placedObjects.Clear();
    // }
    
    private void RemovePlacedObjects()
    {
        PlacedBySplineObjectPlacer[] placedObjects = FindObjectsOfType<PlacedBySplineObjectPlacer>();

        foreach (PlacedBySplineObjectPlacer placedObject in placedObjects)
        {
            if (placedObject.toolIdentifier == "SplineObjectPlacerEditor")
            {
                Undo.DestroyObjectImmediate(placedObject.gameObject);
            }
        }
    }


    // Editor Preferences loading and saving
    private void LoadSettings()
    {
        string splineContainerPath = EditorPrefs.GetString("SplineObjectPlacer_SplineContainer", "");
        if (!string.IsNullOrEmpty(splineContainerPath))
        {
            splineContainer = AssetDatabase.LoadAssetAtPath<SplineContainer>(splineContainerPath);
        }
        
        
        string terrainPath = EditorPrefs.GetString("SplineObjectPlacerEditor_Terrain", "");
        if (!string.IsNullOrEmpty(terrainPath))
        {
            terrain = AssetDatabase.LoadAssetAtPath<Terrain>(terrainPath);
        }
        
        objectPrefabs.Clear();
        for (int i = 0; ; i++)
        {
            string prefabPath = EditorPrefs.GetString($"SplineObjectPlacerEditor_ObjectPrefab_{i}", "");
            if (string.IsNullOrEmpty(prefabPath)) break;
            objectPrefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        }

        secondaryObjectPrefabs.Clear();
        for (int i = 0; ; i++)
        {
            string prefabPath = EditorPrefs.GetString($"SplineObjectPlacerEditor_SecondaryObjectPrefab_{i}", "");
            if (string.IsNullOrEmpty(prefabPath)) break;
            secondaryObjectPrefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        }
        
        livePreview = EditorPrefs.GetBool(LivePreviewKey, false);
        objectCount = EditorPrefs.GetInt(ObjectCountKey, 10);
        spacing = EditorPrefs.GetFloat(SpacingKey, 1f);
        objectUpAxis = (UpAxis)EditorPrefs.GetInt(UpAxisKey, 1);
        randomizeAxis = (RandomizeAxis)EditorPrefs.GetInt(RandomizeAxisKey, 0);
        randomSeed = EditorPrefs.GetInt(RandomSeedKey, 0);
        
        secondaryMeshCount = EditorPrefs.GetInt("SplineObjectPlacerEditor_SecondaryMeshCount", 10);
        secondaryMeshRadius = EditorPrefs.GetFloat("SplineObjectPlacerEditor_SecondaryMeshRadius", 10f);
        secondaryMeshScaleMin = EditorPrefs.GetFloat("SplineObjectPlacerEditor_SecondaryMeshScaleMin", 10f);
        secondaryMeshScaleMax = EditorPrefs.GetFloat("SplineObjectPlacerEditor_SecondaryMeshScaleMax", 10f);
    }

    void SaveSettings()
    {
        EditorPrefs.SetString("SplineObjectPlacerEditor_SplineContainer", AssetDatabase.GetAssetPath(splineContainer));
        EditorPrefs.SetString("SplineObjectPlacerEditor_Terrain", AssetDatabase.GetAssetPath(terrain));

        for (int i = 0; i < objectPrefabs.Count; i++)
        {
            EditorPrefs.SetString($"SplineObjectPlacerEditor_ObjectPrefab_{i}", AssetDatabase.GetAssetPath(objectPrefabs[i]));
        }

        for (int i = 0; i < secondaryObjectPrefabs.Count; i++)
        {
            EditorPrefs.SetString($"SplineObjectPlacerEditor_SecondaryObjectPrefab_{i}", AssetDatabase.GetAssetPath(secondaryObjectPrefabs[i]));
        }

        EditorPrefs.SetInt("SplineObjectPlacerEditor_ObjectCount", objectCount);
        EditorPrefs.SetInt("SplineObjectPlacerEditor_SecondaryMeshCount", secondaryMeshCount);
        EditorPrefs.SetFloat("SplineObjectPlacerEditor_SecondaryMeshRadius", secondaryMeshRadius);
        EditorPrefs.SetFloat("SplineObjectPlacerEditor_SecondaryMeshScaleMin", secondaryMeshScaleMin);
        EditorPrefs.SetFloat("SplineObjectPlacerEditor_SecondaryMeshScaleMax", secondaryMeshScaleMax);
        EditorPrefs.SetFloat("SplineObjectPlacerEditor_Spacing", spacing);
        EditorPrefs.SetInt("SplineObjectPlacerEditor_ObjectUpAxis", (int)objectUpAxis);
        EditorPrefs.SetInt("SplineObjectPlacerEditor_RandomizeAxis", (int)randomizeAxis);
        EditorPrefs.SetInt("SplineObjectPlacerEditor_RandomSeed", randomSeed);
        EditorPrefs.SetBool("SplineObjectPlacerEditor_LivePreview", livePreview);
    }
}
