using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    public Vector3 offset =
        new Vector3(0f, 5f, -7f);

    public float smoothSpeed = 5.0f;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition =
            target.TransformPoint(offset);

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );

        Vector3 lookPoint =
            target.position +
            Vector3.up * 1.5f;

        transform.LookAt(lookPoint);
    }
}