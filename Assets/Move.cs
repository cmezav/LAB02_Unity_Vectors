using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;
    public float speed = 2.0f;

    [Header("Distancias")]
    public float stopDistance = 2.0f;
    public float separationRadius = 2.2f;

    [Header("Separacion")]
    public float separationWeight = 1.5f;
    public float villagerAvoidWeight = 2.0f;

    [Header("Rotacion")]
    public float rotationSpeed = 4.0f;

    Vector3 direction;
    Vector3 velocity;

    Move[] sheep;

    void Start()
    {
        // Busca todas las ovejas que usan este mismo script
        sheep = FindObjectsOfType<Move>();
    }

    void LateUpdate()
    {
        if (goal == null)
            return;

        direction = goal.transform.position - transform.position;

        // Ignoramos diferencias de altura
        direction.y = 0;

        float goalDistance = direction.magnitude;

        // SEEK: seguir al Villager
        Vector3 seekDirection = Vector3.zero;

        if (goalDistance > stopDistance)
        {
            seekDirection = direction.normalized;
        }

        // SEPARATION: evitar que las ovejas se atraviesen
        Vector3 separation = CalculateSeparation();

        // Evitar atravesar al Villager
        Vector3 avoidVillager = Vector3.zero;

        if (goalDistance < stopDistance && goalDistance > 0.01f)
        {
            float strength =
                (stopDistance - goalDistance) / stopDistance;

            avoidVillager =
                -direction.normalized *
                strength *
                villagerAvoidWeight;
        }

        // Combinar SEEK + SEPARATION + evitar Villager
        Vector3 moveDirection =
            seekDirection +
            separation * separationWeight +
            avoidVillager;

        moveDirection.y = 0;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            moveDirection.Normalize();

            // Movimiento
            velocity = moveDirection * speed;

            transform.position +=
                velocity * Time.deltaTime;

            // Rotacion suave hacia la direccion propia
            // de movimiento de esta oveja
            Vector3 flatDirection = new Vector3(
                moveDirection.x,
                0,
                moveDirection.z
            );

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(flatDirection);

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }
    }

    Vector3 CalculateSeparation()
    {
        Vector3 separation = Vector3.zero;

        foreach (Move other in sheep)
        {
            if (other == null || other == this)
                continue;

            Vector3 difference =
                transform.position -
                other.transform.position;

            // Trabajamos sobre el suelo
            difference.y = 0;

            float distance = difference.magnitude;

            if (distance > 0.01f &&
                distance < separationRadius)
            {
                // Mientras mas cerca este otra oveja,
                // mas fuerte es la separacion
                float strength =
                    (separationRadius - distance) /
                    separationRadius;

                separation +=
                    difference.normalized * strength;
            }
        }

        return separation;
    }
}