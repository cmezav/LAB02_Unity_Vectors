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

    [Header("Estado")]
    public bool isEating = false;

    private FlockManager manager;
    private GrassSpot grassTarget;

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

        ChooseRandomTarget();
    }

    void LateUpdate()
    {
        if (manager == null)
            return;

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
        // DENTRO del radio visible.
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
            // S esta activo, pero la oveja
            // esta fuera del radio.
            RandomWander();
        }
    }

    // ======================================
    // WANDERING
    // ======================================

    void WanderingMode()
    {
        // Ya comio en esta ronda:
        // ahora puede vagar libremente.
        if (HasFinishedGrass)
        {
            RandomWander();
            return;
        }

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
    // RANDOM WANDER
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

        if (direction.sqrMagnitude > 0.001f)
        {
            MoveInDirection(
                direction.normalized
            );
        }
    }

    void ChooseRandomTarget()
    {
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