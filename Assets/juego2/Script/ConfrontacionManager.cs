using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConfrontacionManager : MonoBehaviour
{
    public static ConfrontacionManager Instance;

    [Header("UI Confrontacion")]
    public GameObject panelConfrontacion;
    public TextMeshProUGUI textoSituacion;
    public Button botonPelear;
    public Button botonReportar;
    public TextMeshProUGUI textoEvidenciaDisponible;

    [Header("UI Resultado")]
    public GameObject panelResultado;
    public TextMeshProUGUI textoResultado;
    public Button botonVolverAJugar;

    [Header("Bully")]
    public GameObject prefabBully;
    public Transform spawnBully;

    private bool confrontacionActiva = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        EstilizarPanel(panelConfrontacion);
        EstilizarPanel(panelResultado);

        if (panelConfrontacion != null)
        {
            panelConfrontacion.SetActive(false);
        }
        if (panelResultado != null)
        {
            panelResultado.SetActive(false);
        }
    }

    public void IniciarConfrontacion()
    {
        if (confrontacionActiva)
        {
            return;
        }

        confrontacionActiva = true;

        MovimientoJugador jugador = FindAnyObjectByType<MovimientoJugador>();
        if (jugador != null)
        {
            jugador.enabled = false;
        }

        Animacion animacion = FindAnyObjectByType<Animacion>();
        if (animacion != null)
        {
            animacion.enabled = false;
        }

        if (prefabBully != null && spawnBully != null)
        {
            Instantiate(prefabBully, spawnBully.position, Quaternion.identity);
        }

        if (panelConfrontacion != null)
        {
            panelConfrontacion.SetActive(true);
        }

        if (textoSituacion != null)
        {
            textoSituacion.text = "Un estudiante te intercepta antes de salir...\n¿Qué vas a hacer?";
        }

        if (textoEvidenciaDisponible != null)
        {
            int evidencias = SistemaEvidencia.Instance != null ? SistemaEvidencia.Instance.GetEvidencias() : 0;
            textoEvidenciaDisponible.text = $"Evidencias recolectadas: {evidencias}";
        }

        ConectarBoton(botonPelear, ElegirPelear);
        ConectarBoton(botonReportar, ElegirReportar);
        ConfigurarNavegacion(botonPelear);
        ConfigurarNavegacion(botonReportar);

        if (UnityEngine.EventSystems.EventSystem.current != null && botonPelear != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(botonPelear.gameObject);
        }

        if (panelConfrontacion != null)
        {
            EcosAulaPromptUI.CrearBarraPrompts(panelConfrontacion.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Seleccionar"));
        }
    }

    private void ElegirPelear()
    {
        if (panelConfrontacion != null)
        {
            panelConfrontacion.SetActive(false);
        }

        MostrarResultado(false);
    }

    private void ElegirReportar()
    {
        if (panelConfrontacion != null)
        {
            panelConfrontacion.SetActive(false);
        }

        if (SistemaEvidencia.Instance != null && SistemaEvidencia.Instance.TieneSuficienteEvidencia())
        {
            MostrarResultado(true);
        }
        else
        {
            MostrarResultadoSinEvidencia();
        }
    }

    public void MostrarResultado(bool gano)
    {
        PrepararResultado();

        if (textoResultado == null)
        {
            return;
        }

        textoResultado.text = gano
            ? "Hiciste lo correcto.\nEl profesor intervino gracias a tu evidencia.\nEl estudiante agresor recibió consecuencias.\n\nGanaste."
            : "Elegiste pelear y ganaste la pelea...\nPero fuiste sancionado por responder con violencia.\n\nLa violencia nunca es la respuesta.\n\nGame Over.";
    }

    private void MostrarResultadoSinEvidencia()
    {
        PrepararResultado();

        if (textoResultado != null)
        {
            textoResultado.text = "Llamaste a un profesor, pero no tenías evidencia suficiente.\nEl estudiante agresor no recibió consecuencias.\n\nLa próxima vez guarda evidencia.\n\nFinal neutral.";
        }
    }

    private void PrepararResultado()
    {
        if (panelResultado != null)
        {
            panelResultado.SetActive(true);
        }

        ConectarBoton(botonVolverAJugar, VolverAJugar);
        ConfigurarNavegacion(botonVolverAJugar);

        if (UnityEngine.EventSystems.EventSystem.current != null && botonVolverAJugar != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(botonVolverAJugar.gameObject);
        }

        if (panelResultado != null)
        {
            EcosAulaPromptUI.CrearBarraPrompts(panelResultado.transform,
                (AccionLogica.Confirmar, "Volver a jugar"));
        }
    }

    private void VolverAJugar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void ConectarBoton(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if (boton == null)
        {
            return;
        }

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(accion);
    }

    private void ConfigurarNavegacion(Button boton)
    {
        if (boton == null)
        {
            return;
        }

        Navigation nav = boton.navigation;
        nav.mode = Navigation.Mode.Automatic;
        boton.navigation = nav;
    }

    private void EstilizarPanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Image fondo = panel.GetComponent<Image>();
        if (fondo == null)
        {
            fondo = panel.AddComponent<Image>();
        }

        fondo.color = new Color(0.025f, 0.018f, 0.055f, 0.84f);

        Outline outline = panel.GetComponent<Outline>();
        if (outline == null)
        {
            outline = panel.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.55f);
        outline.effectDistance = new Vector2(2f, -2f);

        TextMeshProUGUI[] textos = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] != null)
            {
                textos[i].color = new Color(0.92f, 0.98f, 1f, 1f);
                textos[i].fontSize = Mathf.Max(textos[i].fontSize, 18f);
            }
        }
    }
}
