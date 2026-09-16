using UnityEngine;

public class GrassSpot : MonoBehaviour
{
    public bool IsAvailable { get; private set; } = true;

    private Renderer[] renderers;
    private Collider[] colliders;

    void Awake()
    {
        renderers =
            GetComponentsInChildren<Renderer>(
                true
            );

        colliders =
            GetComponentsInChildren<Collider>(
                true
            );
    }

    public void SetAvailable(bool available)
    {
        IsAvailable = available;

        foreach (Renderer r in renderers)
        {
            r.enabled = available;
        }

        foreach (Collider c in colliders)
        {
            c.enabled = available;
        }
    }
}