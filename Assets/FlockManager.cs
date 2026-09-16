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
    public Color circleColor = Color.yellow;

    private Move[] sheep;
    private GrassSpot[] grasses;

    private int finishedSheep = 0;
    private int roundNumber = 0;

    private LineRenderer circle;

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
}