using System;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public enum HerdMode
    {
        Seeking,
        Wandering
    }

    [Header("Modo actual")]
    public HerdMode currentMode = HerdMode.Seeking;

    [Header("Radio del Villager")]
    public float attractionRadius = 12.0f;

    [Header("Circulo visible")]
    public int circleSegments = 80;
    public float circleWidth = 0.08f;
    public float circleHeight = 0.08f;
    public Color circleColor = Color.green;

    public Color fillColor =
        new Color32(161, 217, 155, 128);

    private Move[] sheep;
    private GrassSpot[] grasses;

    private int finishedSheep = 0;
    private int roundNumber = 0;

    private LineRenderer circle;

    private GameObject fillObject;
    private MeshFilter fillMeshFilter;
    private MeshRenderer fillMeshRenderer;

    void Start()
    {
        sheep = FindObjectsByType<Move>(
            FindObjectsSortMode.None
        );

        grasses = FindObjectsByType<GrassSpot>(
            FindObjectsSortMode.None
        );

        Array.Sort(
            sheep,
            (a, b) => string.Compare(
                a.name,
                b.name,
                StringComparison.Ordinal
            )
        );

        Array.Sort(
            grasses,
            (a, b) => string.Compare(
                a.name,
                b.name,
                StringComparison.Ordinal
            )
        );

        SetupCircle();
        SetupFill();

        StartNewGrassRound();
    }

    void Update()
    {
        // SEEKING
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentMode = HerdMode.Seeking;

            foreach (Move s in sheep)
            {
                s.OnSeekingActivated();
            }
        }

        // WANDERING
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentMode = HerdMode.Wandering;
        }
    }

    void LateUpdate()
    {
        DrawCircle();
    }

    // ======================================
    // RADIO DE ATRACCION
    // ======================================

    public bool IsInsideAttractionRadius(
        Vector3 sheepPosition
    )
    {
        Vector3 villagerPosition =
            transform.position;

        sheepPosition.y = 0;
        villagerPosition.y = 0;

        float distance =
            Vector3.Distance(
                sheepPosition,
                villagerPosition
            );

        return distance <= attractionRadius;
    }

    // ======================================
    // RONDAS DE PASTO
    // ======================================

    public void SheepFinishedEating(
        Move sheepThatFinished,
        GrassSpot eatenGrass
    )
    {
        if (sheepThatFinished.HasFinishedGrass)
            return;

        sheepThatFinished.MarkGrassFinished();

        eatenGrass.SetAvailable(false);

        finishedSheep++;

        if (finishedSheep >= sheep.Length)
        {
            RespawnAllGrass();
        }
    }

    void RespawnAllGrass()
    {
        finishedSheep = 0;

        roundNumber++;

        foreach (GrassSpot grass in grasses)
        {
            grass.SetAvailable(true);
        }

        StartNewGrassRound();
    }

    void StartNewGrassRound()
    {
        if (grasses.Length == 0)
            return;

        for (int i = 0; i < sheep.Length; i++)
        {
            // Cambia el pasto de cada oveja
            // en cada nueva ronda.
            int grassIndex =
                (i + roundNumber) %
                grasses.Length;

            sheep[i].SetGrassTarget(
                grasses[grassIndex]
            );

            sheep[i].ResetGrassRound();
        }
    }

    // ======================================
    // CIRCULO VISIBLE
    // ======================================

    void SetupCircle()
    {
        circle =
            GetComponent<LineRenderer>();

        if (circle == null)
        {
            circle =
                gameObject.AddComponent<LineRenderer>();
        }

        circle.loop = true;
        circle.useWorldSpace = true;

        circle.positionCount =
            circleSegments;

        circle.startWidth =
            circleWidth;

        circle.endWidth =
            circleWidth;

        circle.startColor =
            circleColor;

        circle.endColor =
            circleColor;

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
        {
            shader =
                Shader.Find("Sprites/Default");
        }

        if (shader != null)
        {
            circle.material =
                new Material(shader);
        }
    }

    void DrawCircle()
    {
        if (circle == null)
            return;

        for (int i = 0;
             i < circleSegments;
             i++)
        {
            float angle =
                (float)i /
                circleSegments *
                Mathf.PI *
                2.0f;

            Vector3 point =
                new Vector3(
                    Mathf.Cos(angle) *
                    attractionRadius,
                    circleHeight,
                    Mathf.Sin(angle) *
                    attractionRadius
                );

            point += transform.position;

            circle.SetPosition(
                i,
                point
            );
        }
    }

    void SetupFill()
{
    fillObject =
        new GameObject("AttractionRadiusFill");

    fillObject.transform.SetParent(
        transform
    );

    fillObject.transform.localPosition =
        Vector3.zero;

    fillMeshFilter =
        fillObject.AddComponent<MeshFilter>();

    fillMeshRenderer =
        fillObject.AddComponent<MeshRenderer>();

    Shader shader =
        Shader.Find("Universal Render Pipeline/Unlit");

    if (shader == null)
    {
        shader =
            Shader.Find("Sprites/Default");
    }

    Material material =
        new Material(shader);

    material.color = fillColor;

    // Transparencia URP
    if (
        material.HasProperty("_Surface")
    )
    {
        material.SetFloat(
            "_Surface",
            1
        );

        material.SetFloat(
            "_ZWrite",
            0
        );

        material.EnableKeyword(
            "_SURFACE_TYPE_TRANSPARENT"
        );

        material.renderQueue = 3000;
    }

    fillMeshRenderer.material =
        material;

    CreateFillMesh();
}

void CreateFillMesh()
{
    Mesh mesh =
        new Mesh();

    Vector3[] vertices =
        new Vector3[
            circleSegments + 1
        ];

    int[] triangles =
        new int[
            circleSegments * 3
        ];

    // Centro
    vertices[0] =
        new Vector3(
            0,
            circleHeight - 0.01f,
            0
        );

    // Borde
    for (
        int i = 0;
        i < circleSegments;
        i++
    )
    {
        float angle =
            (float)i /
            circleSegments *
            Mathf.PI *
            2.0f;

        vertices[i + 1] =
            new Vector3(
                Mathf.Cos(angle) *
                attractionRadius,

                circleHeight - 0.01f,

                Mathf.Sin(angle) *
                attractionRadius
            );

        int next =
            (i + 1) %
            circleSegments;

        triangles[i * 3] = 0;
        triangles[i * 3 + 1] =
            next + 1;
        triangles[i * 3 + 2] =
            i + 1;
    }

    mesh.vertices =
        vertices;

    mesh.triangles =
        triangles;

    mesh.RecalculateNormals();

    fillMeshFilter.mesh =
        mesh;
}

}