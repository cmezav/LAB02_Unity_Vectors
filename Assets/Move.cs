using System;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;
    public float speed = 2.0f;

    [Header("Seeking")]
    public float stopDistance = 2.5f;

    [Header("Wandering")]
    public float grassStopDistance = 1.0f;

    [Header("Rotacion")]
    public float rotationSpeed = 5.0f;

    private static bool wanderingMode = false;

    private GrassSpot grassTarget;

    public bool isEating = false;

    void Start()
    {
        AssignGrass();
    }

    void Update()
    {
        // W = ir a comer pasto
        if (Input.GetKeyDown(KeyCode.W))
        {
            wanderingMode = true;
            isEating = false;
        }

        // S = volver al Villager
        if (Input.GetKeyDown(KeyCode.S))
        {
            wanderingMode = false;
            isEating = false;
        }
    }

    void LateUpdate()
    {
        if (wanderingMode)
        {
            Wander();
        }
        else
        {
            Seek();
        }
    }

    // ===============================
    // SEEK ORIGINAL
    // ===============================

    void Seek()
    {
        if (goal == null)
            return;

        isEating = false;

        Vector3 direction =
            goal.transform.position -
            transform.position;

        direction.y = 0;

        float distance =
            direction.magnitude;

        if (distance > stopDistance)
        {
            MoveTowards(
                direction.normalized
            );
        }
    }

    // ===============================
    // WANDER HACIA PASTO
    // ===============================

    void Wander()
    {
        if (grassTarget == null)
            return;

        Vector3 direction =
            grassTarget.transform.position -
            transform.position;

        direction.y = 0;

        float distance =
            direction.magnitude;

        if (distance > grassStopDistance)
        {
            isEating = false;

            MoveTowards(
                direction.normalized
            );
        }
        else
        {
            // Ya llego al pasto
            isEating = true;
        }
    }

    // ===============================
    // MOVIMIENTO
    // ===============================

    void MoveTowards(Vector3 moveDirection)
    {
        transform.position +=
            moveDirection *
            speed *
            Time.deltaTime;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                moveDirection,
                Vector3.up
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    // ===============================
    // UN PASTO POR OVEJA
    // ===============================

    void AssignGrass()
    {
        GrassSpot[] grasses =
            FindObjectsByType<GrassSpot>(
                FindObjectsSortMode.None
            );

        Move[] sheep =
            FindObjectsByType<Move>(
                FindObjectsSortMode.None
            );

        if (grasses.Length == 0)
            return;

        Array.Sort(
            grasses,
            (a, b) => string.Compare(
                a.name,
                b.name,
                StringComparison.Ordinal
            )
        );

        Array.Sort(
            sheep,
            (a, b) => string.Compare(
                a.name,
                b.name,
                StringComparison.Ordinal
            )
        );

        int index =
            Array.IndexOf(sheep, this);

        grassTarget =
            grasses[index % grasses.Length];
    }
}