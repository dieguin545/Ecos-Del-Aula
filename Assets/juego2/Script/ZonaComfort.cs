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
        movimientoJugador = FindAnyObjectByType<MovimientoJugador>();
        animacionJugador = FindAnyObjectByType<Animacion>();

        if (panelIndicador != null)
        {
            panelIndicador.SetActive(false);
            CanvasGroup cg = panelIndicador.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = panelIndicador.AddComponent<CanvasGroup>();
            }
            cg.alpha = 1f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        if (panelAccion != null)
        {
            panelAccion.SetActive(false);
        }
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

        if (jugadorDentro && GestorEntradaGlobal.InteractuarPresionado(KeyCode.F) && !enCooldown && !realizandoAccion)
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
            if (signoExclamacion != null)
            {
                signoExclamacion.SetActive(false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            if (panelIndicador != null)
            {
                panelIndicador.SetActive(false);
            }
            if (signoExclamacion != null)
            {
                signoExclamacion.SetActive(true);
            }
        }
    }

    private void MostrarIndicador()
    {
        if (panelIndicador == null)
        {
            return;
        }

        CanvasGroup cg = panelIndicador.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        panelIndicador.transform.localScale = Vector3.one;

        panelIndicador.SetActive(true);

        if (textoIndicador != null)
        {
            textoIndicador.enabled = true;
            RectTransform rect = textoIndicador.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(372f, 66f);
            }
        }

        if (personajeActivo == personajeCompatible)
        {
            if (vecesUsada >= vecesMaximas)
            {
                DesactivarPromptYMostrarTexto("Ya estás aburrido de esta actividad");
            }
            else if (textoIndicador != null)
            {
                EcosAulaPromptUI.InyectarEn(textoIndicador.gameObject, AccionLogica.InteractuarF, mensajeConfort);
                AjustarPromptInyectado(textoIndicador.transform);
            }
        }
        else
        {
            DesactivarPromptYMostrarTexto(mensajeIncompatible);
        }
    }

    private void AjustarPromptInyectado(Transform raiz)
    {
        if (raiz == null) return;

        Transform icono = raiz.Find("_IconoPrincipal");
        if (icono != null)
        {
            RectTransform rectIcono = icono.GetComponent<RectTransform>();
            if (rectIcono != null)
            {
                rectIcono.sizeDelta = new Vector2(38f, 38f);
                rectIcono.anchoredPosition = new Vector2(0f, 0f);
            }
        }

        Transform texto = raiz.Find("_TextoVerbo");
        if (texto != null)
        {
            RectTransform rectTexto = texto.GetComponent<RectTransform>();
            if (rectTexto != null)
            {
                rectTexto.anchoredPosition = new Vector2(50f, 0f);
                rectTexto.sizeDelta = new Vector2(288f, 0f);
            }

            TextMeshProUGUI tmp = texto.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 19f;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }
        }
    }

    private void DesactivarPromptYMostrarTexto(string texto)
    {
        if (textoIndicador == null)
        {
            return;
        }

        EcosAulaPromptUI prompt = textoIndicador.GetComponent<EcosAulaPromptUI>();
        if (prompt != null)
        {
            prompt.enabled = false;
            Transform img = textoIndicador.transform.Find("_IconoPrincipal");
            if (img != null) img.gameObject.SetActive(false);
            Transform txt = textoIndicador.transform.Find("_TextoVerbo");
            if (txt != null) txt.gameObject.SetActive(false);
        }

        textoIndicador.enabled = true;
        textoIndicador.text = texto;
    }

    private void UsarZonaConfort()
    {
        if (personajeActivo == personajeCompatible)
        {
            if (vecesUsada >= vecesMaximas)
            {
                DesactivarPromptYMostrarTexto("Te estás aburriendo de esta actividad");
                return;
            }

            vecesUsada++;
            realizandoAccion = true;
            enCooldown = true;

            if (movimientoJugador != null)
            {
                movimientoJugador.enabled = false;
            }
            if (animacionJugador != null)
            {
                animacionJugador.enabled = false;
            }

            if (panelAccion != null)
            {
                panelAccion.SetActive(true);
                if (textoAccion != null)
                {
                    textoAccion.text = accionTexto;
                }
            }

            if (panelIndicador != null)
            {
                panelIndicador.SetActive(false);
            }

            Invoke(nameof(TerminarAccion), 5f);
        }
        else
        {
            DesactivarPromptYMostrarTexto(mensajeIncompatible);
            Invoke(nameof(OcultarIndicador), 2f);
        }
    }

    private void TerminarAccion()
    {
        realizandoAccion = false;

        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = true;
        }
        if (animacionJugador != null)
        {
            animacionJugador.enabled = true;
        }

        if (panelAccion != null)
        {
            panelAccion.SetActive(false);
        }

        if (vecesUsada <= vecesMaximas && AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.DecreaseAnxiety(ansiedadQueReduce);
            VidaEscolarHUD.Ensure().MostrarToast("Zona de confort: ansiedad reducida");
        }
    }

    private void OcultarIndicador()
    {
        if (panelIndicador != null)
        {
            panelIndicador.SetActive(false);
        }
    }
}
