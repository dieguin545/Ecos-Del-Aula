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
    public float sensibilidadControl = 125f;
    public float limiteArriba = -35f;
    public float limiteAbajo = 60f;

    [Header("Control Xbox")]
    public bool intercambiarEjesControl = false;
    public bool invertirVerticalControl = false;
    public float deadzoneStickCamara = 0.18f;

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

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;

        Vector2 stickCamara = ObtenerStickCamaraNormalizado();
        float stickX = stickCamara.x * sensibilidadControl * Time.unscaledDeltaTime;
        float stickY = stickCamara.y * sensibilidadControl * Time.unscaledDeltaTime;

        rotacionY += mouseX + stickX;
        // Mouse: restar mouseY para que mover arriba suba la cámara (natural)
        rotacionX -= mouseY;
        // Stick: separado para poder invertirlo con invertirVerticalControl
        rotacionX -= stickY;
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

    private Vector2 ObtenerStickCamaraNormalizado()
    {
        float horizontal = GestorEntradaGlobal.ObtenerCamaraHorizontal();
        float vertical = GestorEntradaGlobal.ObtenerCamaraVertical();

        if (intercambiarEjesControl)
        {
            float temporal = horizontal;
            horizontal = vertical;
            vertical = temporal;
        }

        if (invertirVerticalControl)
        {
            vertical *= -1f;
        }

        if (Mathf.Abs(horizontal) < deadzoneStickCamara)
        {
            horizontal = 0f;
        }

        if (Mathf.Abs(vertical) < deadzoneStickCamara)
        {
            vertical = 0f;
        }

        return new Vector2(horizontal, vertical);
    }
}
