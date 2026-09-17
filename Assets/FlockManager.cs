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
    public Color circleColor = Color.green;

    [Header("Altura visual del radio")]
    public float groundVisualOffset = 0.02f;

    [Header("Relleno")]
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
        sheep =
            FindObjectsByType<Move>(
                FindObjectsSortMode.None
            );

        grasses =
            FindObjectsByType<GrassSpot>(
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

        UpdateRadiusVisibility();
    }

    void Update()
    {
        // ===============================
        // S = SEEKING
        // ===============================
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentMode =
                HerdMode.Seeking;

            foreach (Move s in sheep)
            {
                s.OnSeekingActivated();
            }

            UpdateRadiusVisibility();
        }

        // ===============================
        // W = WANDERING
        // ===============================
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentMode =
                HerdMode.Wandering;

            UpdateRadiusVisibility();
        }
    }

    void LateUpdate()
    {
        if (
            currentMode ==
            HerdMode.Seeking
        )
        {
            // Mantener el relleno siguiendo
            // X y Z del Villager,
            // pero siempre pegado al suelo.
            if (fillObject != null)
            {
                fillObject.transform.position =
                    new Vector3(
                        transform.position.x,
                        GetGroundVisualY(),
                        transform.position.z
                    );
            }

            DrawCircle();
        }
    }

    // ======================================
    // ALTURA REAL DEL SUELO
    // ======================================

    float GetGroundVisualY()
    {
        if (WorldBounds.Instance != null)
        {
            return
                WorldBounds.Instance
                    .transform.position.y
                +
                groundVisualOffset;
        }

        return groundVisualOffset;
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

        return
            distance <= attractionRadius;
    }

    // ======================================
    // VISIBILIDAD DEL RADIO
    // ======================================

    void UpdateRadiusVisibility()
    {
        bool visible =
            currentMode ==
            HerdMode.Seeking;

        // Contorno
        if (circle != null)
        {
            circle.enabled =
                visible;
        }

        // Relleno
        if (fillObject != null)
        {
            fillObject.SetActive(
                visible
            );
        }
    }

    // ======================================
    // RONDAS DE PASTO
    // ======================================

    public void SheepFinishedEating(
        Move sheepThatFinished,
        GrassSpot eatenGrass
    )
    {
        if (
            sheepThatFinished
                .HasFinishedGrass
        )
        {
            return;
        }

        sheepThatFinished
            .MarkGrassFinished();

        eatenGrass.SetAvailable(false);

        finishedSheep++;

        // Solo reaparecen cuando
        // TODAS terminaron.
        if (
            finishedSheep >=
            sheep.Length
        )
        {
            RespawnAllGrass();
        }
    }

    void RespawnAllGrass()
    {
        finishedSheep = 0;

        roundNumber++;

        foreach (
            GrassSpot grass
            in grasses
        )
        {
            grass.SetAvailable(true);
        }

        StartNewGrassRound();
    }

    void StartNewGrassRound()
    {
        if (grasses.Length == 0)
            return;

        for (
            int i = 0;
            i < sheep.Length;
            i++
        )
        {
            int grassIndex =
                (i + roundNumber) %
                grasses.Length;

            sheep[i].SetGrassTarget(
                grasses[grassIndex]
            );

            sheep[i]
                .ResetGrassRound();
        }
    }

    // ======================================
    // CONTORNO DEL CIRCULO
    // ======================================

    void SetupCircle()
    {
        circle =
            GetComponent<LineRenderer>();

        if (circle == null)
        {
            circle =
                gameObject.AddComponent<
                    LineRenderer
                >();
        }

        circle.loop = true;

        // Las posiciones que damos
        // son coordenadas globales.
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
                Shader.Find(
                    "Sprites/Default"
                );
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

        float groundY =
            GetGroundVisualY();

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

            Vector3 point =
                new Vector3(
                    transform.position.x +
                    Mathf.Cos(angle) *
                    attractionRadius,

                    groundY,

                    transform.position.z +
                    Mathf.Sin(angle) *
                    attractionRadius
                );

            circle.SetPosition(
                i,
                point
            );
        }
    }

    // ======================================
    // RELLENO DEL CIRCULO
    // ======================================

    void SetupFill()
    {
        fillObject =
            new GameObject(
                "AttractionRadiusFill"
            );

        // NO hacemos hijo del Villager,
        // porque heredaria su altura.
        fillObject.transform.position =
            new Vector3(
                transform.position.x,
                GetGroundVisualY(),
                transform.position.z
            );

        fillMeshFilter =
            fillObject.AddComponent<
                MeshFilter
            >();

        fillMeshRenderer =
            fillObject.AddComponent<
                MeshRenderer
            >();

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Sprites/Default"
                );
        }

        if (shader != null)
        {
            Material material =
                new Material(shader);

            material.color =
                fillColor;

            // Transparencia URP
            if (
                material.HasProperty(
                    "_Surface"
                )
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

                material.renderQueue =
                    3000;
            }

            fillMeshRenderer.material =
                material;
        }

        CreateFillMesh();
    }

    // ======================================
    // MALLA DEL RELLENO
    // ======================================

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

        // Punto central del disco.
        // Y = 0 porque la altura
        // la controla fillObject.
        vertices[0] =
            new Vector3(
                0,
                0,
                0
            );

        // Crear puntos alrededor
        // de toda la circunferencia.
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

            // ESTE ERA EL CAMBIO QUE
            // TE FALTABA:
            vertices[i + 1] =
                new Vector3(
                    Mathf.Cos(angle) *
                    attractionRadius,

                    0,

                    Mathf.Sin(angle) *
                    attractionRadius
                );

            int next =
                (i + 1) %
                circleSegments;

            triangles[i * 3] =
                0;

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
        mesh.RecalculateBounds();

        fillMeshFilter.mesh =
            mesh;
    }
}