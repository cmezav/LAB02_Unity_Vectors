using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;

    Vector3 direction;

    void Start()
    {
        direction = goal.transform.position - transform.position;
    }

    void LateUpdate()
    {
        transform.position = transform.position + direction * 0.01f;
    }
}