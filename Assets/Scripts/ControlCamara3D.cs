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

    [Header("Control Xbox")]
    public bool intercambiarEjesControl = false;
    public bool invertirVerticalControl = false;
    public float deadzoneStickCamara = 0.18f;

    [Header("Tercera persona")]
    public float distancia = 3.1f;
    public float alturaMirada = 0.85f;
    public float radioColision = 0.18f;
    public float distanciaMinimaColision = 1.35f;

    [Header("Primera persona")]
    public float alturaOjos = 1.2f;
    public KeyCode teclaCambiarVista = KeyCode.V;

    private float rotacionX = 18f;
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

    public void RecentrarCamaraInicial()
    {
        if (jugador == null)
        {
            return;
        }

        primeraPersona = false;
        distancia = Mathf.Clamp(distancia, 2.9f, 3.35f);
        alturaMirada = Mathf.Clamp(alturaMirada, 0.75f, 0.95f);
        rotacionX = 18f;
        rotacionY = 0f;
        AplicarVisibilidadJugador();
        AplicarTransformacionCamara();
    }

    void Update()
    {
        if (jugador == null) return;
        if (InteraccionPC.PCAbierta || MenuPausaAccesibilidad.EstaPausado) return;

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;

        Vector2 stickCamara = ObtenerStickCamaraNormalizado();
        float stickX = stickCamara.x * sensibilidadControl * Time.unscaledDeltaTime;
        float stickY = stickCamara.y * sensibilidadControl * Time.unscaledDeltaTime;

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
        if (InteraccionPC.PCAbierta || MenuPausaAccesibilidad.EstaPausado) return;

        AplicarTransformacionCamara();
    }

    private void AplicarTransformacionCamara()
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
                    float distanciaSegura = Mathf.Max(hit.distance - 0.1f, distanciaMinimaColision);
                    transform.position = puntoMirada + direccionRaycast.normalized * distanciaSegura;
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
