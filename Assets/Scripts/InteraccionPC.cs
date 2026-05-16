using UnityEngine;

public class InteraccionPC : MonoBehaviour
{
    public static bool PCAbierta { get; private set; }
    public static int FrameCierrePC { get; private set; } = -100;

    [Header("UI de la PC")]
    public GameObject canvasPC;
    public GameObject textoInteractuar;

    [Header("Jugador y camara")]
    public MonoBehaviour scriptMovimientoJugador;
    public GameObject camaraPrincipal;

    [Header("Configuracion")]
    public KeyCode teclaInteractuar = KeyCode.E;

    private bool jugadorDentro = false;
    private bool usandoPC = false;
    private Behaviour scriptCamara;

    void Start()
    {
        if (canvasPC != null)
        {
            canvasPC.SetActive(false);
        }

        if (textoInteractuar != null)
        {
            textoInteractuar.SetActive(false);
        }

        if (camaraPrincipal != null)
        {
            scriptCamara = camaraPrincipal.GetComponent("ControlCamara3D") as Behaviour;
        }

        usandoPC = false;
        PCAbierta = false;
    }

    void Update()
    {
        if (MenuPausaAccesibilidad.EstaPausado)
        {
            return;
        }

        if (jugadorDentro && Input.GetKeyDown(teclaInteractuar))
        {
            if (usandoPC)
            {
                SalirPC();
            }
            else
            {
                EntrarPC();
            }
        }

        if (usandoPC && Input.GetKeyDown(KeyCode.Escape))
        {
            SalirPC();
        }
    }

    void EntrarPC()
    {
        usandoPC = true;
        PCAbierta = true;

        if (canvasPC != null)
        {
            canvasPC.SetActive(true);
        }

        if (textoInteractuar != null)
        {
            textoInteractuar.SetActive(false);
        }

        if (scriptMovimientoJugador != null)
        {
            scriptMovimientoJugador.enabled = false;
        }

        if (scriptCamara != null)
        {
            scriptCamara.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void SalirPC()
    {
        usandoPC = false;
        PCAbierta = false;
        FrameCierrePC = Time.frameCount;

        if (canvasPC != null)
        {
            canvasPC.SetActive(false);
        }

        if (scriptMovimientoJugador != null)
        {
            scriptMovimientoJugador.enabled = true;
        }

        if (scriptCamara != null)
        {
            scriptCamara.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (jugadorDentro && textoInteractuar != null)
        {
            textoInteractuar.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;

            if (textoInteractuar != null && !usandoPC)
            {
                textoInteractuar.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;

            if (textoInteractuar != null)
            {
                textoInteractuar.SetActive(false);
            }

            if (usandoPC)
            {
                SalirPC();
            }
        }
    }
}