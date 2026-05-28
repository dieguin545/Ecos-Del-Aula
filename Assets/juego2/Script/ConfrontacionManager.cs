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
        AjustarDiseñoUI();

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

        // Failsafe: Asegurar que el tiempo corre normalmente al iniciar la confrontación del jefe
        Time.timeScale = 1f;

        confrontacionActiva = true;
        AjustarDiseñoUI();

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
            panelConfrontacion.transform.SetAsLastSibling();
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

        // Si existe el sistema de combate físico del jefe, iniciarlo!
        if (SistemaCombate.Instance != null)
        {
            SistemaCombate.Instance.IniciarCombate();
        }
        else
        {
            MostrarResultado(false);
        }
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
        AsegurarTituloResultado(gano, false);
        PrepararResultado();
        AjustarGlowResultado(gano, false);

        if (textoResultado == null)
        {
            return;
        }

        textoResultado.text = gano
            ? "Hiciste lo correcto.\nEl profesor intervino gracias a tu evidencia.\nEl estudiante agresor recibió consecuencias."
            : "Elegiste pelear y ganaste la pelea...\nPero fuiste sancionado por responder con violencia.\n\nLa violencia nunca es la respuesta.";
    }

    public void MostrarDerrotaCombate()
    {
        AsegurarTituloResultado(false, true);
        PrepararResultado();
        AjustarGlowResultado(false, true);

        if (textoResultado != null)
        {
            textoResultado.text = "Fuiste derrotado en la confrontación...\nTu nivel de ansiedad llegó al límite y no pudiste resistir más.";
        }
    }

    private void MostrarResultadoSinEvidencia()
    {
        AsegurarTituloResultado(false, true);
        PrepararResultado();
        AjustarGlowResultado(false, true);

        if (textoResultado != null)
        {
            textoResultado.text = "Llamaste a un profesor, pero no tenías evidencia suficiente.\nEl estudiante agresor no recibió consecuencias y el acoso continuó.\n\nLa próxima vez guarda evidencias para poder probarlo.";
        }
    }

    private void PrepararResultado()
    {
        if (panelResultado != null)
        {
            panelResultado.transform.SetAsLastSibling();
            panelResultado.SetActive(true);
        }
        AjustarDiseñoUI();

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
                // Solo cambiar el color a blanco/celeste si NO es hijo de un Botón
                if (textos[i].GetComponentInParent<Button>() == null)
                {
                    textos[i].color = new Color(0.92f, 0.98f, 1f, 1f);
                    textos[i].fontSize = Mathf.Max(textos[i].fontSize, 18f);
                }
            }
        }
    }

    private void AjustarRectTransform(RectTransform rect, Vector2 posicion, Vector2 tamaño)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
    }

    private void AjustarDiseñoUI()
    {
        // 1. Panel de Confrontación
        if (panelConfrontacion != null)
        {
            if (textoSituacion != null)
            {
                AjustarRectTransform(textoSituacion.GetComponent<RectTransform>(), new Vector2(0f, 90f), new Vector2(850f, 130f));
                textoSituacion.fontSize = 24f;
                textoSituacion.alignment = TextAlignmentOptions.Center;
            }

            if (botonPelear != null)
            {
                AjustarRectTransform(botonPelear.GetComponent<RectTransform>(), new Vector2(-160f, -40f), new Vector2(240f, 60f));
                EstilizarBotonConTextoNegro(botonPelear, "Pelear");
            }

            if (botonReportar != null)
            {
                AjustarRectTransform(botonReportar.GetComponent<RectTransform>(), new Vector2(160f, -40f), new Vector2(240f, 60f));
                EstilizarBotonConTextoNegro(botonReportar, "Reportar");
            }

            if (textoEvidenciaDisponible != null)
            {
                AjustarRectTransform(textoEvidenciaDisponible.GetComponent<RectTransform>(), new Vector2(0f, -130f), new Vector2(600f, 50f));
                textoEvidenciaDisponible.fontSize = 18f;
                textoEvidenciaDisponible.alignment = TextAlignmentOptions.Center;
            }
        }

        // 2. Panel de Resultado
        if (panelResultado != null)
        {
            if (textoResultado != null)
            {
                AjustarRectTransform(textoResultado.GetComponent<RectTransform>(), new Vector2(0f, -10f), new Vector2(850f, 180f));
                textoResultado.fontSize = 22f;
                textoResultado.alignment = TextAlignmentOptions.Center;
            }

            if (botonVolverAJugar != null)
            {
                AjustarRectTransform(botonVolverAJugar.GetComponent<RectTransform>(), new Vector2(0f, -150f), new Vector2(260f, 60f));
                EstilizarBotonConTextoNegro(botonVolverAJugar, "Volver a jugar");
            }
        }
    }

    private void AsegurarTituloResultado(bool gano, bool esDerrotaReal)
    {
        if (panelResultado == null) return;

        Transform tituloTrans = panelResultado.transform.Find("TituloResultadoJuego2");
        GameObject tituloGo;
        if (tituloTrans != null)
        {
            tituloGo = tituloTrans.gameObject;
        }
        else
        {
            tituloGo = new GameObject("TituloResultadoJuego2", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            tituloGo.transform.SetParent(panelResultado.transform, false);
        }

        tituloGo.transform.localScale = Vector3.one;

        RectTransform rect = tituloGo.GetComponent<RectTransform>();
        AjustarRectTransform(rect, new Vector2(0f, 150f), new Vector2(800f, 80f));

        TextMeshProUGUI textoTitulo = tituloGo.GetComponent<TextMeshProUGUI>();
        
        if (textoResultado != null && textoTitulo != null)
        {
            textoTitulo.font = textoResultado.font;
            textoTitulo.fontSharedMaterial = textoResultado.fontSharedMaterial;
        }

        if (textoTitulo != null)
        {
            if (gano)
            {
                textoTitulo.text = "¡VICTORIA!";
                textoTitulo.color = new Color(0.3f, 1f, 0.4f, 1f); // Verde brillante
                
                Outline outline = tituloGo.GetComponent<Outline>();
                if (outline == null) outline = tituloGo.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0.5f, 0.2f, 0.75f);
                outline.effectDistance = new Vector2(2f, -2f);
            }
            else if (esDerrotaReal)
            {
                textoTitulo.text = "GAME OVER";
                textoTitulo.color = new Color(1f, 0.2f, 0.2f, 1f); // Rojo dramático
                
                Outline outline = tituloGo.GetComponent<Outline>();
                if (outline == null) outline = tituloGo.AddComponent<Outline>();
                outline.effectColor = new Color(0.5f, 0f, 0f, 0.75f);
                outline.effectDistance = new Vector2(2f, -2f);
            }
            else
            {
                textoTitulo.text = "SANCIONADO";
                textoTitulo.color = new Color(1f, 0.6f, 0f, 1f); // Naranja de advertencia
                
                Outline outline = tituloGo.GetComponent<Outline>();
                if (outline == null) outline = tituloGo.AddComponent<Outline>();
                outline.effectColor = new Color(0.5f, 0.2f, 0f, 0.75f);
                outline.effectDistance = new Vector2(2f, -2f);
            }

            textoTitulo.fontSize = 46f;
            textoTitulo.fontStyle = FontStyles.Bold;
            textoTitulo.alignment = TextAlignmentOptions.Center;
        }
    }

    private void AjustarGlowResultado(bool gano, bool esDerrotaReal)
    {
        if (panelResultado == null) return;
        
        Outline outline = panelResultado.GetComponent<Outline>();
        if (outline != null)
        {
            if (gano)
            {
                outline.effectColor = new Color(0.3f, 1f, 0.4f, 0.65f); // Verde
            }
            else if (esDerrotaReal)
            {
                outline.effectColor = new Color(1f, 0.2f, 0.2f, 0.65f); // Rojo
            }
            else
            {
                outline.effectColor = new Color(1f, 0.6f, 0f, 0.65f); // Naranja
            }
        }
    }

    private void EstilizarBotonConTextoNegro(Button boton, string texto)
    {
        if (boton == null) return;

        Image imagen = boton.GetComponent<Image>();
        if (imagen != null)
        {
            // Fondo azul grisáceo claro muy premium
            imagen.color = new Color(0.90f, 0.93f, 0.98f, 1f);
        }

        Outline outline = boton.GetComponent<Outline>();
        if (outline == null)
        {
            outline = boton.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        TextMeshProUGUI txtTMP = boton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (txtTMP != null)
        {
            txtTMP.text = texto;
            txtTMP.color = Color.black;
            txtTMP.fontSize = 20f;
            txtTMP.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            Text txtNormal = boton.GetComponentInChildren<Text>(true);
            if (txtNormal != null)
            {
                txtNormal.text = texto;
                txtNormal.color = Color.black;
                txtNormal.fontSize = 20;
                txtNormal.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}
