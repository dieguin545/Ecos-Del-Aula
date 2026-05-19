using UnityEngine;

public class CamaraNave3D : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform objetivo;

    [Header("Posicion de camara")]
    public float distancia = 10f;
    public float altura = 2.5f;
    public float suavizado = 12f;

    [Header("Mouse")]
    public float sensibilidadMouse = 3f;
    public float limiteArriba = -35f;
    public float limiteAbajo = 60f;

    private float rotacionX = 10f;
    private float rotacionY = 0f;

    private void Start()
    {
        if (objetivo != null)
        {
            rotacionY = objetivo.eulerAngles.y;
        }

        BloquearCursor();
    }

    private void LateUpdate()
    {
        if (objetivo == null)
        {
            return;
        }

        if (GameManager.instancia != null && !GameManager.instancia.PuedeControlarGameplay)
        {
            LiberarCursor();
            return;
        }

        BloquearCursor();

        rotacionY += Input.GetAxis("Mouse X") * sensibilidadMouse;
        rotacionX -= Input.GetAxis("Mouse Y") * sensibilidadMouse;
        rotacionX = Mathf.Clamp(rotacionX, limiteArriba, limiteAbajo);

        Quaternion rotacionCamara = Quaternion.Euler(rotacionX, rotacionY, 0f);

        Vector3 posicionDeseada =
            objetivo.position
            - rotacionCamara * Vector3.forward * distancia
            + Vector3.up * altura;

        transform.position = Vector3.Lerp(
            transform.position,
            posicionDeseada,
            suavizado * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionCamara,
            suavizado * Time.deltaTime
        );
    }

    private void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LiberarCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
