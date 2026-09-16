using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject goal;

    void Start()
    {
        this.transform.Translate(goal.transform.position);
    }

    void Update()
    {

    }
}