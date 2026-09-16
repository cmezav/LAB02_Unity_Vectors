using System;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;
    public float speed = 2.0f;

    [Header("Formacion")]
    public float formationRadius = 4.5f;
    public float arriveDistance = 0.35f;
    public float slowRadius = 3.5f;

    [Header("Modelo visual")]
    public Transform visualModel;

    // IMPORTANTE:
    // -90 porque con +90 la oveja quedaba mirando hacia atras.
    public float modelYawOffset = -90.0f;

    [Header("Rotacion visual")]
    public float rotationSpeed = 360.0f;

    private Vector3 formationOffset;

    void Start()
    {
        Move[] sheep = FindObjectsByType<Move>(
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

        int index = Array.IndexOf(sheep, this);

        float angle =
            (Mathf.PI * 2.0f * index) /
            Mathf.Max(1, sheep.Length);

        // Posicion propia alrededor del Villager.
        // Este vector NO depende de su rotacion.
        formationOffset = new Vector3(
            Mathf.Cos(angle),
            0,
            Mathf.Sin(angle)
        ) * formationRadius;

        // Buscar automaticamente Low Poly Sheep
        if (visualModel == null &&
            transform.childCount > 0)
        {
            visualModel = transform.GetChild(0);
        }
    }

    void LateUpdate()
    {
        if (goal == null)
            return;

        // SOLO utilizamos la POSICION del Villager.
        Vector3 targetPosition =
            goal.transform.position +
            formationOffset;

        targetPosition.y =
            transform.position.y;

        Vector3 direction =
            targetPosition -
            transform.position;

        direction.y = 0;

        float distance =
            direction.magnitude;

        // Ya esta en su lugar.
        if (distance <= arriveDistance)
        {
            return;
        }

        Vector3 moveDirection =
            direction.normalized;

        float currentSpeed =
            speed;

        // Frenar cuando se acerca a su punto
        if (distance < slowRadius)
        {
            float factor =
                Mathf.InverseLerp(
                    arriveDistance,
                    slowRadius,
                    distance
                );

            currentSpeed =
                speed * factor;
        }

        // ========================================
        // MOVER SOLO EL PADRE
        // NO LO ROTAMOS
        // ========================================

        transform.position +=
            moveDirection *
            currentSpeed *
            Time.deltaTime;

        // ========================================
        // ROTAR SOLO EL MODELO VISUAL
        // ========================================

        if (visualModel != null)
        {
            Quaternion movementRotation =
                Quaternion.LookRotation(
                    moveDirection,
                    Vector3.up
                );

            Quaternion modelCorrection =
                Quaternion.Euler(
                    0,
                    modelYawOffset,
                    0
                );

            Quaternion targetVisualRotation =
                movementRotation *
                modelCorrection;

            visualModel.rotation =
                Quaternion.RotateTowards(
                    visualModel.rotation,
                    targetVisualRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }
    }
}