using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;
    public float speed = 2.0f;

    [Header("Seeking")]
    public float stopDistance = 2.5f;

    [Header("Rotacion")]
    public float rotationSpeed = 5.0f;

    void LateUpdate()
    {
        if (goal == null)
            return;

        // Solo usamos la POSICION del Villager
        Vector3 direction =
            goal.transform.position - transform.position;

        // Movimiento solo sobre el suelo
        direction.y = 0;

        float distance =
            direction.magnitude;

        // Igual que el tutorial:
        // perseguir mientras estemos lejos
        if (distance > stopDistance)
        {
            Vector3 moveDirection =
                direction.normalized;

            transform.position +=
                moveDirection *
                speed *
                Time.deltaTime;

            // La oveja mira hacia SU movimiento,
            // no hacia la rotacion del Villager
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
    }
}