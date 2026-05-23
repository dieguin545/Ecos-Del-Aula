using UnityEngine;

public class ControlCamara3D : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;

    [Header("Mouse")]
    public float sensibilidadMouse = 2f;
    public float sensibilidadControl = 115f;
    public float limiteArriba = -25f;
    public float limiteAbajo = 60f;

    [Header("Tercera persona")]
    public float distancia = 4f;
    public float alturaMirada = 0.9f;
    public float radioColision = 0.25f;

    [Header("Primera persona")]
    public float alturaOjos = 1.2f;
    public KeyCode teclaCambiarVista = KeyCode.V;

    private float rotacionX = 15f;
    private float rotacionY = 0f;
    private bool primeraPersona = false;
    private Renderer[] renderersJugador;

    void Start()
    {
        if (jugador != null)
        {
            rotacionY = transform.eulerAngles.y;
            renderersJugador = jugador.GetComponentsInChildren<Renderer>();
            AplicarVisibilidadJugador();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (jugador == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;
        float stickX = GestorEntradaGlobal.ObtenerCamaraHorizontal() * sensibilidadControl * Time.unscaledDeltaTime;
        float stickY = GestorEntradaGlobal.ObtenerCamaraVertical() * sensibilidadControl * Time.unscaledDeltaTime;

        rotacionY += mouseX + stickX;
        rotacionX -= mouseY + stickY;
        rotacionX = Mathf.Clamp(rotacionX, limiteArriba, limiteAbajo);

        if (Input.GetKeyDown(teclaCambiarVista))
        {
            primeraPersona = !primeraPersona;
            AplicarVisibilidadJugador();
        }

        
    }

    void LateUpdate()
    {
        if (jugador == null) return;

        Quaternion rotacionCamara = Quaternion.Euler(rotacionX, rotacionY, 0f);

        if (primeraPersona)
        {
            transform.position = jugador.position + Vector3.up * alturaOjos;
            transform.rotation = rotacionCamara;
        }
        else
        {
            Vector3 puntoMirada = jugador.position + Vector3.up * alturaMirada;
            Vector3 direccionCamara = rotacionCamara * Vector3.back;
            Vector3 posicionDeseada = puntoMirada + direccionCamara * distancia;

            Vector3 direccionRaycast = posicionDeseada - puntoMirada;
            float distanciaRaycast = direccionRaycast.magnitude;

            if (Physics.SphereCast(puntoMirada, radioColision, direccionRaycast.normalized, out RaycastHit hit, distanciaRaycast))
            {
                if (hit.collider.transform != jugador && !hit.collider.transform.IsChildOf(jugador))
                {
                    transform.position = puntoMirada + direccionRaycast.normalized * (hit.distance - 0.1f);
                }
                else
                {
                    transform.position = posicionDeseada;
                }
            }
            else
            {
                transform.position = posicionDeseada;
            }

            transform.LookAt(puntoMirada);
        }
    }

    void AplicarVisibilidadJugador()
    {
        if (jugador == null)
        {
            return;
        }

        renderersJugador = jugador.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderersJugador)
        {
            if (r == null)
            {
                continue;
            }

            // El MeshRenderer del objeto Jugador es la cápsula física antigua:
            // siempre queda oculta; solo los hijos visuales 2.5D se alternan.
            if (r.transform == jugador)
            {
                r.enabled = false;
                continue;
            }

            r.enabled = !primeraPersona;
        }
    }
}
