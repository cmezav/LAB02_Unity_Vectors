using UnityEngine;

public class Drive : MonoBehaviour
{
    public float speed = 10.0f;
    public float rotationSpeed = 100.0f;

    void Update()
    {
        float translation = 0.0f;
        float rotation = 0.0f;

        // SOLO flechas para avanzar y retroceder
        if (Input.GetKey(KeyCode.UpArrow))
        {
            translation = 1.0f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            translation = -1.0f;
        }

        // SOLO flechas para girar
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotation = -1.0f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotation = 1.0f;
        }

        translation *= speed * Time.deltaTime;
        rotation *= rotationSpeed * Time.deltaTime;

        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);
    }
}