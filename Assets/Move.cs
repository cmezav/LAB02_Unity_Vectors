using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;

    void Start()
    {
        Vector3 direction = goal.transform.position - transform.position;
        transform.Translate(direction);
    }

    void Update()
    {

    }
}