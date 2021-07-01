using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceGraphicsToolkit;

public class ProceduralPlanet: MonoBehaviour
{
    [Range(2, 256)]
    public int resolution = 10;
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    SphereMesh[] terrainFaces;
    public ColorSettings colorSettings;
    public ShapeSettings shapeSettings;
    public PropsSettings propsSettings;
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColorGenerator colorGenerator = new ColorGenerator();
    PropsGenerator propsGenerator = new PropsGenerator();
    SgtSharedMaterial sharedMaterial;
    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colorSettingsFoldout;
    [HideInInspector]
    public bool propsSettingsFoldout;
    public SgtSharedMaterial atmosphere;
    public Transform props;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back};
    public FaceRenderMask faceRenderMask;


    private void Start()
    {
        shapeGenerator = new ShapeGenerator();
        colorGenerator = new ColorGenerator();
        propsGenerator = new PropsGenerator();
        
        GeneratePlanet();
        propsGenerator.UpdateSettings(propsSettings, transform, props, atmosphere);
        

    }
    void Initialize()
    {
        if (shapeSettings.random == true)
        {
            Invoke("FixCollisions", 1f);
        }
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings);
        propsGenerator.UpdateSettings(propsSettings, transform, props, atmosphere);
        if (shapeSettings.random==true)
        {
            shapeSettings.seed = Random.Range(0, 1000000);
        }
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new SphereMesh[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;

            terrainFaces[i] = new SphereMesh(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
            if (shapeSettings.random == true) { 
                meshFilters[i].gameObject.AddComponent<MeshCollider>();
                meshFilters[i].GetComponent<MeshCollider>().convex = true;
            }
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }
    void FixCollisions()
    {
        for (int i = 0; i < 6; i++)
        {     
            meshFilters[i].GetComponent<MeshCollider>().convex = false;
        }
    }
    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColours();
        if (shapeSettings.random == true)
        {
            Invoke("GenerateProps", 1f);
        }
    }

    public void OnShapeSettingsUpdated()
    {
        Initialize();
        GenerateMesh();
    }

    public void OnColorSettingsUpdated()
    {
    
        Initialize();
        GenerateColours();
    }
    void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }

        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColours()
    {
        colorGenerator.UpdateColours();
    }
    public void GenerateProps()
    {
        propsGenerator = new PropsGenerator();
        propsGenerator.UpdateSettings(propsSettings, transform, props, atmosphere);
        propsGenerator.GenerateProp();
    }
    public float BodyScale
    {
        get
        {
            return transform.localScale.x;
        }
    }

}