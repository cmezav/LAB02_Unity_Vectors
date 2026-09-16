using UnityEngine;

public class WorldBounds : MonoBehaviour
{
    public static WorldBounds Instance;

    private Renderer planeRenderer;
    private Bounds bounds;

    [Header("Margen interior")]
    public float margin = 1.5f;

    void Awake()
    {
        Instance = this;

        planeRenderer =
            GetComponent<Renderer>();

        if (planeRenderer != null)
        {
            bounds =
                planeRenderer.bounds;
        }
    }

    public Vector3 ClampPosition(
        Vector3 position
    )
    {
        if (planeRenderer == null)
            return position;

        bounds =
            planeRenderer.bounds;

        position.x =
            Mathf.Clamp(
                position.x,
                bounds.min.x + margin,
                bounds.max.x - margin
            );

        position.z =
            Mathf.Clamp(
                position.z,
                bounds.min.z + margin,
                bounds.max.z - margin
            );

        return position;
    }

    public Vector3 GetRandomPoint()
    {
        if (planeRenderer == null)
            return transform.position;

        bounds =
            planeRenderer.bounds;

        return new Vector3(
            Random.Range(
                bounds.min.x + margin,
                bounds.max.x - margin
            ),
            transform.position.y,
            Random.Range(
                bounds.min.z + margin,
                bounds.max.z - margin
            )
        );
    }
}