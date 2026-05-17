using UnityEngine;

public class CamariOrbit : MonoBehaviour
{
    public Transform objetivo;
    public float distancia = 15f;
    public float sensibilidad = 3f;
    public float limiteVerticalMin = 10f;
    public float limiteVerticalMax = 80f;

    float rotX = 45f;
    float rotY = 0f;

    void LateUpdate()
    {
        if (Input.GetMouseButton(1)) // click derecho para rotar camara
        {
            rotY += Input.GetAxis("Mouse X") * sensibilidad;
            rotX -= Input.GetAxis("Mouse Y") * sensibilidad;
            rotX = Mathf.Clamp(rotX, limiteVerticalMin, limiteVerticalMax);
        }

        Quaternion rotacion = Quaternion.Euler(rotX, rotY, 0f);
        Vector3 offset = rotacion * new Vector3(0f, 0f, -distancia);
        transform.position = objetivo.position + offset;
        transform.LookAt(objetivo);
    }
}