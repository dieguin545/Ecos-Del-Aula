using UnityEngine;
using TMPro;

public class ZonaConfort : MonoBehaviour
{
    [Header("Configuracion")]
    public PersonajeType personajeCompatible;
    public float ansiedadQueReduce = 20f;
    public float cooldown = 10f;
    public string mensajeConfort;
    public string mensajeIncompatible;
    public string accionTexto; // ej: "Boxeando", "Dibujando"
    public int vecesMaximas = 5;
    public GameObject signoExclamacion;
    [Header("UI")]
    public GameObject panelIndicador;
    public TextMeshProUGUI textoIndicador;

    [Header("Texto Accion sobre personaje")]
    public GameObject panelAccion;
    public TextMeshProUGUI textoAccion;

    private float timerCooldown = 0f;
    private bool enCooldown = false;
    private bool jugadorDentro = false;
    private bool realizandoAccion = false;
    private PersonajeType personajeActivo;
    private int vecesUsada = 0;
    private MovimientoJugador movimientoJugador;
    private Animacion animacionJugador;

    void Start()
    {
        personajeActivo = (PersonajeType)PlayerPrefs.GetInt("PersonajeSeleccionado", 0);
        movimientoJugador = FindObjectOfType<MovimientoJugador>();
        animacionJugador = FindObjectOfType<Animacion>();

        if (panelIndicador != null)
            panelIndicador.SetActive(false);
        if (panelAccion != null)
            panelAccion.SetActive(false);
    }

    void Update()
    {
        if (enCooldown)
        {
            timerCooldown += Time.deltaTime;
            if (timerCooldown >= cooldown)
            {
                enCooldown = false;
                timerCooldown = 0f;
            }
        }

        if (jugadorDentro && Input.GetKeyDown(KeyCode.F) && !enCooldown && !realizandoAccion)
        {
            UsarZonaConfort();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;
            MostrarIndicador();
            signoExclamacion.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            if (panelIndicador != null)
                panelIndicador.SetActive(false);
            signoExclamacion.SetActive(true);
        }
    }

    private void MostrarIndicador()
    {
        if (panelIndicador == null) return;
        panelIndicador.SetActive(true);

        if (personajeActivo == personajeCompatible)
        {
            if (vecesUsada >= vecesMaximas)
                textoIndicador.text = "Ya estás aburrido de esta actividad";
            else
                textoIndicador.text = "Presiona F para " + mensajeConfort;
        }
        else
        {
            textoIndicador.text = mensajeIncompatible;
        }
    }

    private void UsarZonaConfort()
    {
        if (personajeActivo == personajeCompatible)
        {
            if (vecesUsada >= vecesMaximas)
            {
                if (panelIndicador != null)
                    textoIndicador.text = "Te estás aburriendo de esta actividad";
                return;
            }

            vecesUsada++;
            realizandoAccion = true;
            enCooldown = true;

            // Desactiva movimiento
            if (movimientoJugador != null)
                movimientoJugador.enabled = false;
            if (animacionJugador != null)
                animacionJugador.enabled = false;

            // Muestra texto de accion sobre el personaje
            if (panelAccion != null)
            {
                panelAccion.SetActive(true);
                textoAccion.text = accionTexto;
            }

            if (panelIndicador != null)
                panelIndicador.SetActive(false);

            Invoke("TerminarAccion", 5f);
        }
        else
        {
            if (panelIndicador != null)
                textoIndicador.text = mensajeIncompatible;
            Invoke("OcultarIndicador", 2f);
        }
    }

    private void TerminarAccion()
    {
        realizandoAccion = false;

        // Reactiva movimiento
        if (movimientoJugador != null)
            movimientoJugador.enabled = true;
        if (animacionJugador != null)
            animacionJugador.enabled = true;

        // Oculta texto de accion
        if (panelAccion != null)
            panelAccion.SetActive(false);

        // Reduce ansiedad solo si no esta aburrido
        if (vecesUsada <= vecesMaximas)
            AnxietySystem.Instance.DecreaseAnxiety(ansiedadQueReduce);
    }

    private void OcultarIndicador()
    {
        if (panelIndicador != null)
            panelIndicador.SetActive(false);
    }
}