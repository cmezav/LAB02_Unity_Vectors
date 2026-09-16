using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;

    Vector3 direction;
    Vector3 velocity;

    float speed = 0.05f;

    void Start()
    {
        direction = goal.transform.position - transform.position;
    }

    void LateUpdate()
    {
        velocity = direction.normalized * speed;

        transform.position = transform.position + velocity;
    }
}