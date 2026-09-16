using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;

    Vector3 direction;
    Vector3 velocity;

    float speed = 2.0f;

    void Start()
    {

    }

    void LateUpdate()
    {
        direction = goal.transform.position - transform.position;

        velocity = direction.normalized * speed;

        transform.position = transform.position + velocity * Time.deltaTime;
    }
}