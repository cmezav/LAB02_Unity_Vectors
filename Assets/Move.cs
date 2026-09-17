using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;
    public float speed = 2.0f;

    [Header("Seeking")]
    public float stopDistance = 2.5f;

    [Header("Comer")]
    public float grassStopDistance = 1.0f;
    public float eatingTime = 5.0f;

    [Header("Wandering Random")]
    public float wanderDistance = 7.0f;
    public float wanderArrivalDistance = 0.8f;

    [Header("Rotacion")]
    public float rotationSpeed = 5.0f;

    [Header("No atravesar")]
    public float sheepMinDistance = 1.7f;
    public float villagerMinDistance = 2.0f;
    public int separationIterations = 2;

    [Header("Estado")]
    public bool isEating = false;

    private FlockManager manager;
    private GrassSpot grassTarget;

    private Move[] allSheep;

    private float eatingTimer = 0.0f;

    private Vector3 randomTarget;

    public bool HasFinishedGrass
    {
        get;
        private set;
    }

    void Start()
    {
        manager =
            FindFirstObjectByType<FlockManager>();

        // Guardamos todas las ovejas.
        allSheep =
            FindObjectsByType<Move>(
                FindObjectsSortMode.None
            );

        ChooseRandomTarget();
    }

    void LateUpdate()
    {
        if (manager == null)
            return;

        // ==================================
        // COMPORTAMIENTO PRINCIPAL
        // ==================================

        if (
            manager.currentMode ==
            FlockManager.HerdMode.Seeking
        )
        {
            SeekingMode();
        }
        else
        {
            WanderingMode();
        }

        // ==================================
        // EVITAR ATRAVESARSE
        // ==================================

        ResolveOverlaps();
    }

    // ======================================
    // SEEKING
    // ======================================

    void SeekingMode()
    {
        isEating = false;

        if (goal == null)
        {
            RandomWander();
            return;
        }

        // Solo sigue al Villager si esta
        // dentro del radio de atraccion.
        if (
            manager.IsInsideAttractionRadius(
                transform.position
            )
        )
        {
            Vector3 direction =
                goal.transform.position -
                transform.position;

            direction.y = 0;

            float distance =
                direction.magnitude;

            if (distance > stopDistance)
            {
                MoveInDirection(
                    direction.normalized
                );
            }
        }
        else
        {
            // Fuera del radio:
            // sigue vagando aunque S este activo.
            RandomWander();
        }
    }

    // ======================================
    // WANDERING
    // ======================================

    void WanderingMode()
    {
        // Ya comio:
        // puede caminar libremente.
        if (HasFinishedGrass)
        {
            RandomWander();
            return;
        }

        // No hay pasto disponible.
        if (
            grassTarget == null ||
            !grassTarget.IsAvailable
        )
        {
            RandomWander();
            return;
        }

        Vector3 direction =
            grassTarget.transform.position -
            transform.position;

        direction.y = 0;

        float distance =
            direction.magnitude;

        if (distance > grassStopDistance)
        {
            isEating = false;
            eatingTimer = 0;

            MoveInDirection(
                direction.normalized
            );
        }
        else
        {
            EatGrass();
        }
    }

    // ======================================
    // COMER DURANTE 5 SEGUNDOS
    // ======================================

    void EatGrass()
    {
        isEating = true;

        eatingTimer +=
            Time.deltaTime;

        if (eatingTimer >= eatingTime)
        {
            isEating = false;
            eatingTimer = 0;

            manager.SheepFinishedEating(
                this,
                grassTarget
            );

            ChooseRandomTarget();
        }
    }

    // ======================================
    // WANDERING RANDOM
    // ======================================

    void RandomWander()
    {
        Vector3 direction =
            randomTarget -
            transform.position;

        direction.y = 0;

        if (
            direction.magnitude <
            wanderArrivalDistance
        )
        {
            ChooseRandomTarget();

            direction =
                randomTarget -
                transform.position;

            direction.y = 0;
        }

        if (
            direction.sqrMagnitude >
            0.001f
        )
        {
            MoveInDirection(
                direction.normalized
            );
        }
    }

    // ======================================
    // DESTINO RANDOM DENTRO DEL PLANE
    // ======================================

    void ChooseRandomTarget()
    {
        if (
            WorldBounds.Instance != null
        )
        {
            randomTarget =
                WorldBounds.Instance
                    .GetRandomPoint();

            randomTarget.y =
                transform.position.y;

            return;
        }

        Vector2 randomCircle =
            Random.insideUnitCircle *
            wanderDistance;

        randomTarget =
            transform.position +
            new Vector3(
                randomCircle.x,
                0,
                randomCircle.y
            );
    }

    // ======================================
    // MOVIMIENTO
    // ======================================

    void MoveInDirection(
        Vector3 moveDirection
    )
    {
        transform.position +=
            moveDirection *
            speed *
            Time.deltaTime;

        // Mantenerse dentro del Plane.
        if (
            WorldBounds.Instance != null
        )
        {
            transform.position =
                WorldBounds.Instance
                    .ClampPosition(
                        transform.position
                    );
        }

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

    // ======================================
    // NO ATRAVESAR OTRAS OVEJAS
    // NI AL VILLAGER
    // ======================================

    void ResolveOverlaps()
    {
        Vector3 correctedPosition =
            transform.position;

        // Guardamos la altura original.
        float originalY =
            correctedPosition.y;

        // Repetimos un par de veces porque
        // una correccion puede acercarnos
        // a otra oveja.
        for (
            int iteration = 0;
            iteration < separationIterations;
            iteration++
        )
        {
            // ------------------------------
            // OVEJA vs OVEJA
            // ------------------------------

            if (allSheep != null)
            {
                foreach (
                    Move other
                    in allSheep
                )
                {
                    if (
                        other == null ||
                        other == this
                    )
                    {
                        continue;
                    }

                    Vector3 otherPosition =
                        other.transform.position;

                    Vector3 difference =
                        correctedPosition -
                        otherPosition;

                    difference.y = 0;

                    float distance =
                        difference.magnitude;

                    if (
                        distance <
                        sheepMinDistance
                    )
                    {
                        Vector3 away;

                        // Si por alguna razon
                        // estan exactamente
                        // en la misma posicion.
                        if (distance < 0.001f)
{
    // Si dos ovejas quedaron exactamente
    // en el mismo punto, las mandamos
    // en direcciones opuestas segun su nombre.
    int nameOrder =
        string.CompareOrdinal(
            gameObject.name,
            other.gameObject.name
        );

    if (nameOrder < 0)
    {
        away = Vector3.left;
    }
    else if (nameOrder > 0)
    {
        away = Vector3.right;
    }
    else
    {
        away = Vector3.forward;
    }
}
else
{
    away =
        difference /
        distance;
}

                        float penetration =
                            sheepMinDistance -
                            distance;

                        correctedPosition +=
                            away *
                            penetration;
                    }
                }
            }

            // ------------------------------
            // OVEJA vs VILLAGER
            // ------------------------------

            if (goal != null)
            {
                Vector3 difference =
                    correctedPosition -
                    goal.transform.position;

                difference.y = 0;

                float distance =
                    difference.magnitude;

                if (
                    distance <
                    villagerMinDistance
                )
                {
                    Vector3 away;

                    if (
                        distance <
                        0.001f
                    )
                    {
                        away =
                            -goal.transform.forward;

                        away.y = 0;

                        if (
                            away.sqrMagnitude <
                            0.001f
                        )
                        {
                            away =
                                Vector3.forward;
                        }

                        away.Normalize();
                    }
                    else
                    {
                        away =
                            difference /
                            distance;
                    }

                    float penetration =
                        villagerMinDistance -
                        distance;

                    correctedPosition +=
                        away *
                        penetration;
                }
            }
        }

        // Mantener la altura.
        correctedPosition.y =
            originalY;

        // La correccion tampoco puede
        // sacar la oveja del Plane.
        if (
            WorldBounds.Instance != null
        )
        {
            correctedPosition =
                WorldBounds.Instance
                    .ClampPosition(
                        correctedPosition
                    );

            correctedPosition.y =
                originalY;
        }

        transform.position =
            correctedPosition;
    }

    // ======================================
    // CONTROL DESDE FLOCK MANAGER
    // ======================================

    public void SetGrassTarget(
        GrassSpot target
    )
    {
        grassTarget = target;
    }

    public void MarkGrassFinished()
    {
        HasFinishedGrass = true;
    }

    public void ResetGrassRound()
    {
        HasFinishedGrass = false;
        isEating = false;
        eatingTimer = 0;
    }

    public void OnSeekingActivated()
    {
        isEating = false;
        eatingTimer = 0;

        ChooseRandomTarget();
    }
}