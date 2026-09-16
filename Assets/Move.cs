using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;
    public float speed = 2.0f;

    Vector3 direction;
    Vector3 velocity;

    void Start()
    {

    }

    void LateUpdate()
    {
        direction = goal.transform.position - transform.position;

        transform.LookAt(goal.transform.position);

        if (direction.magnitude > 2)
        {
            velocity = direction.normalized * speed;

            transform.position =
                transform.position + velocity * Time.deltaTime;
        }
    }
}