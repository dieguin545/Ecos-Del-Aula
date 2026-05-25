using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MenuPausa : MonoBehaviour
{
    public static MenuPausa Instance;

    [Header("UI")]
    public GameObject panelPausa;
    public Button botonReanudar;
    public Button botonGuardar;
    public Button botonReiniciar;
    public Button botonSeleccionJuego;
    public Button botonMenuPrincipal;
    public TextMeshProUGUI textoEstado;

    private bool pausado;
    private CanvasGroup grupoPausa;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ReinicializarTrasCargaEscena();
    }

    private void OnDestroy()
    {
        if (grupoPausa != null)
        {
            grupoPausa.DOKill();
        }
        if (panelPausa != null)
        {
            panelPausa.transform.DOKill();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (pausado && Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            Reanudar();
            return;
        }

        if (!GestorEntradaGlobal.PausaPresionada())
        {
            return;
        }

        if (pausado)
        {
            Reanudar();
        }
        else
        {
            Pausar();
        }
    }

    public void Pausar()
    {
        ResolverReferencias();
        pausado = true;
        Time.timeScale = 0f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
            PrepararAnimacionPanel(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ConectarBotones();
        UIAudioManager.PlayOpen();

        ConfigurarNavegacion(botonReanudar);
        ConfigurarNavegacion(botonReiniciar);
        ConfigurarNavegacion(botonSeleccionJuego);
        ConfigurarNavegacion(botonMenuPrincipal);
        ConfigurarNavegacion(botonGuardar);

        if (UnityEngine.EventSystems.EventSystem.current != null && botonReanudar != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(botonReanudar.gameObject);
        }

        if (panelPausa != null)
        {
            EcosAulaPromptUI.CrearBarraPrompts(panelPausa.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Confirmar"),
                (AccionLogica.Cancelar, "Continuar"));
        }
    }

    public void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;

        if (panelPausa != null)
        {
            PrepararAnimacionPanel(false);
        }
        UIAudioManager.PlayClose();
    }

    private void Guardar()
    {
        if (SistemaGuardado.Instance != null)
        {
            SistemaGuardado.Instance.GuardarPartida();
        }
        UIAudioManager.PlayConfirm();

        if (textoEstado != null)
        {
            textoEstado.text = "Partida guardada.";
            Invoke(nameof(LimpiarTexto), 2f);
        }
    }

    private void LimpiarTexto()
    {
        if (textoEstado != null)
        {
            textoEstado.text = "";
        }
    }

    private void IrAlMenu()
    {
        pausado = false;
        Time.timeScale = 1f;
        UIAudioManager.PlayCancel();
        SceneManager.LoadScene("inicio");
    }

    private void ReiniciarEscena()
    {
        pausado = false;
        Time.timeScale = 1f;
        UIAudioManager.PlayConfirm();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void IrASeleccionJuego()
    {
        pausado = false;
        Time.timeScale = 1f;
        UIAudioManager.PlayCancel();
        SceneManager.LoadScene("SeleccionJuego");
    }

    public void ReinicializarTrasCargaEscena()
    {
        ResolverReferencias();
        pausado = false;
        Time.timeScale = 1f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        ConectarBotones();
        AplicarEstiloVisual();
    }

    private void ResolverReferencias()
    {
        if (panelPausa == null)
        {
            GameObject panel = GameObject.Find("PanelPausa");
            if (panel == null)
            {
                panel = GameObject.Find("PanelPausaJuego2");
            }

            panelPausa = panel;
        }

        if (panelPausa == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                panelPausa = CrearPanelPausaRuntime(canvas.transform);
            }
        }

        if (panelPausa == null)
        {
            return;
        }

        Button[] botones = panelPausa.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];
            string texto = ObtenerTextoBoton(boton).ToLowerInvariant();

            if (botonReanudar == null && (texto.Contains("continuar") || texto.Contains("reanudar")))
            {
                botonReanudar = boton;
            }
            else if (botonGuardar == null && texto.Contains("guardar"))
            {
                botonGuardar = boton;
            }
            else if (botonReiniciar == null && (texto.Contains("reiniciar") || texto.Contains("reintentar")))
            {
                botonReiniciar = boton;
            }
            else if (botonSeleccionJuego == null && texto.Contains("selecci"))
            {
                botonSeleccionJuego = boton;
            }
            else if (botonMenuPrincipal == null && (texto.Contains("menu") || texto.Contains("salir")))
            {
                botonMenuPrincipal = boton;
            }
        }

        if (botonReanudar == null)
        {
            botonReanudar = CrearBotonPausa("BotonContinuarRuntime", "Continuar", new Vector2(0f, 122f));
        }

        if (botonReiniciar == null)
        {
            botonReiniciar = CrearBotonPausa("BotonReintentarRuntime", "Reintentar", new Vector2(0f, 54f));
        }

        if (botonSeleccionJuego == null)
        {
            botonSeleccionJuego = CrearBotonPausa("BotonSeleccionJuegoRuntime", "Volver a selección", new Vector2(0f, -14f));
        }

        if (botonMenuPrincipal == null)
        {
            botonMenuPrincipal = CrearBotonPausa("BotonMenuPrincipalRuntime", "Volver al menú", new Vector2(0f, -82f));
        }
    }

    private void ConectarBotones()
    {
        ConectarBoton(botonReanudar, Reanudar);
        ConectarBoton(botonGuardar, Guardar);
        ConectarBoton(botonReiniciar, ReiniciarEscena);
        ConectarBoton(botonSeleccionJuego, IrASeleccionJuego);
        ConectarBoton(botonMenuPrincipal, IrAlMenu);
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

    private void AplicarEstiloVisual()
    {
        if (panelPausa != null)
        {
            Image fondo = panelPausa.GetComponent<Image>();

            if (fondo == null)
            {
                fondo = panelPausa.AddComponent<Image>();
            }

            fondo.color = new Color(0.06f, 0.03f, 0.10f, 0.82f);
            Outline borde = panelPausa.GetComponent<Outline>();
            if (borde == null)
            {
                borde = panelPausa.AddComponent<Outline>();
            }

            borde.effectColor = new Color(0.34f, 0.92f, 1f, 0.42f);
            borde.effectDistance = new Vector2(2f, -2f);

            Shadow sombra = panelPausa.GetComponent<Shadow>();
            if (sombra == null)
            {
                sombra = panelPausa.AddComponent<Shadow>();
            }

            sombra.effectColor = new Color(0f, 0f, 0f, 0.55f);
            sombra.effectDistance = new Vector2(0f, -8f);

            grupoPausa = panelPausa.GetComponent<CanvasGroup>();
            if (grupoPausa == null)
            {
                grupoPausa = panelPausa.AddComponent<CanvasGroup>();
            }

            RectTransform rect = panelPausa.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(560f, 500f);
            }

            AsegurarTituloPausa();
            PosicionarBoton(botonReanudar, new Vector2(0f, 122f));
            PosicionarBoton(botonReiniciar, new Vector2(0f, 54f));
            PosicionarBoton(botonSeleccionJuego, new Vector2(0f, -14f));
            PosicionarBoton(botonMenuPrincipal, new Vector2(0f, -82f));
            PosicionarBoton(botonGuardar, new Vector2(0f, -150f));
        }

        EstilizarBoton(botonReanudar, "Continuar");
        EstilizarBoton(botonReiniciar, "Reintentar");
        EstilizarBoton(botonSeleccionJuego, "Volver a selección");
        EstilizarBoton(botonGuardar, "Guardar");
        EstilizarBoton(botonMenuPrincipal, "Volver al menú");

        if (panelPausa != null)
        {
            Button[] botones = panelPausa.GetComponentsInChildren<Button>(true);

            for (int i = 0; i < botones.Length; i++)
            {
                Button boton = botones[i];
                string texto = ObtenerTextoBoton(boton).ToLowerInvariant();

                if (texto.Contains("guardar"))
                {
                    EstilizarBoton(boton, "Guardar");
                }
                else if (texto.Contains("reiniciar") || texto.Contains("reintentar"))
                {
                    EstilizarBoton(boton, "Reintentar");
                }
                else if (texto.Contains("selecci"))
                {
                    EstilizarBoton(boton, "Volver a selección");
                }
                else if (texto.Contains("salir") || texto.Contains("menu"))
                {
                    EstilizarBoton(boton, "Volver al menú");
                }
                else if (texto.Contains("continuar") || texto.Contains("reanudar"))
                {
                    EstilizarBoton(boton, "Continuar");
                }
                else
                {
                    EstilizarBoton(boton, ObtenerTextoBoton(boton));
                }
            }

            TextMeshProUGUI[] textos = panelPausa.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < textos.Length; i++)
            {
                if (textos[i] != null)
                {
                    textos[i].color = Color.white;
                }
            }

            Text[] textosLegacy = panelPausa.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < textosLegacy.Length; i++)
            {
                if (textosLegacy[i] != null)
                {
                    textosLegacy[i].color = Color.white;
                }
            }
        }

        if (textoEstado != null)
        {
            textoEstado.color = Color.white;
        }
    }

    private void PrepararAnimacionPanel(bool mostrar)
    {
        if (panelPausa == null)
        {
            return;
        }

        if (grupoPausa == null)
        {
            grupoPausa = panelPausa.GetComponent<CanvasGroup>();
            if (grupoPausa == null)
            {
                grupoPausa = panelPausa.AddComponent<CanvasGroup>();
            }
        }

        grupoPausa.DOKill();
        panelPausa.transform.DOKill();

        if (mostrar)
        {
            grupoPausa.alpha = 0f;
            panelPausa.transform.localScale = Vector3.one * 0.96f;
            grupoPausa.DOFade(1f, 0.15f).SetUpdate(true).SetLink(panelPausa);
            panelPausa.transform.DOScale(1f, 0.18f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(panelPausa);
        }
        else
        {
            grupoPausa.DOFade(0f, 0.12f)
                .SetUpdate(true)
                .SetLink(panelPausa)
                .OnComplete(() =>
                {
                    if (panelPausa != null)
                    {
                        panelPausa.SetActive(false);
                    }
                });
        }
    }

    private void EstilizarBoton(Button boton, string texto)
    {
        if (boton == null)
        {
            return;
        }

        Image imagen = boton.GetComponent<Image>();

        if (imagen != null)
        {
            Sprite spriteBoton = texto.ToLowerInvariant().Contains("salir")
                || texto.ToLowerInvariant().Contains("menú")
                || texto.ToLowerInvariant().Contains("menu")
                ? EcosAulaUIAssets.ObtenerBoton("red")
                : texto.ToLowerInvariant().Contains("guardar") || texto.ToLowerInvariant().Contains("continuar")
                    ? EcosAulaUIAssets.ObtenerBoton("green")
                    : EcosAulaUIAssets.ObtenerBoton("blue");

            if (spriteBoton != null)
            {
                imagen.sprite = spriteBoton;
                imagen.type = Image.Type.Simple;
                imagen.color = Color.white;
            }
            else
            {
                imagen.color = new Color(0.04f, 0.03f, 0.09f, 0.95f);
            }
        }

        TextMeshProUGUI etiquetaTMP = boton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (etiquetaTMP != null)
        {
            etiquetaTMP.text = texto;
            etiquetaTMP.color = Color.white;
            etiquetaTMP.fontSize = Mathf.Max(etiquetaTMP.fontSize, 22f);
            etiquetaTMP.alignment = TextAlignmentOptions.Center;
        }

        Text etiquetaLegacy = boton.GetComponentInChildren<Text>(true);

        if (etiquetaLegacy != null)
        {
            etiquetaLegacy.text = texto;
            etiquetaLegacy.color = Color.white;
            etiquetaLegacy.fontSize = Mathf.Max(etiquetaLegacy.fontSize, 22);
            etiquetaLegacy.alignment = TextAnchor.MiddleCenter;
        }

        RectTransform rect = boton.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(360f, 58f);
        }

        if (boton.GetComponent<EcosAulaBotonAudio>() == null)
        {
            boton.gameObject.AddComponent<EcosAulaBotonAudio>();
        }
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

    private void PosicionarBoton(Button boton, Vector2 posicion)
    {
        if (boton == null)
        {
            return;
        }

        RectTransform rect = boton.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(360f, 58f);
    }

    private GameObject CrearPanelPausaRuntime(Transform padre)
    {
        GameObject panel = new GameObject(
            "PanelPausaJuego2",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup)
        );
        panel.transform.SetParent(padre, false);
        panel.SetActive(false);
        return panel;
    }

    private Button CrearBotonPausa(string nombre, string texto, Vector2 posicion)
    {
        if (panelPausa == null)
        {
            return null;
        }

        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        objeto.transform.SetParent(panelPausa.transform, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(360f, 58f);

        GameObject etiqueta = new GameObject(
            "Texto",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        etiqueta.transform.SetParent(objeto.transform, false);

        RectTransform rectEtiqueta = etiqueta.GetComponent<RectTransform>();
        rectEtiqueta.anchorMin = Vector2.zero;
        rectEtiqueta.anchorMax = Vector2.one;
        rectEtiqueta.offsetMin = new Vector2(16f, 4f);
        rectEtiqueta.offsetMax = new Vector2(-16f, -4f);

        TextMeshProUGUI textoBoton = etiqueta.GetComponent<TextMeshProUGUI>();
        textoBoton.text = texto;
        textoBoton.fontSize = 24f;
        textoBoton.alignment = TextAlignmentOptions.Center;
        textoBoton.color = Color.white;

        return objeto.GetComponent<Button>();
    }

    private void AsegurarTituloPausa()
    {
        if (panelPausa == null || panelPausa.transform.Find("TituloPausaJuego2") != null)
        {
            return;
        }

        GameObject titulo = new GameObject(
            "TituloPausaJuego2",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        titulo.transform.SetParent(panelPausa.transform, false);

        RectTransform rect = titulo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 196f);
        rect.sizeDelta = new Vector2(460f, 70f);

        TextMeshProUGUI textoTitulo = titulo.GetComponent<TextMeshProUGUI>();
        textoTitulo.text = "PAUSA";
        textoTitulo.fontSize = 48f;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        textoTitulo.color = new Color(0.86f, 0.95f, 1f, 1f);

        Shadow sombra = titulo.AddComponent<Shadow>();
        sombra.effectColor = new Color(0.35f, 0.1f, 1f, 0.65f);
        sombra.effectDistance = new Vector2(0f, -3f);
    }

    private string ObtenerTextoBoton(Button boton)
    {
        if (boton == null)
        {
            return string.Empty;
        }

        TextMeshProUGUI etiquetaTMP = boton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (etiquetaTMP != null)
        {
            return etiquetaTMP.text;
        }

        Text etiquetaLegacy = boton.GetComponentInChildren<Text>(true);
        return etiquetaLegacy != null ? etiquetaLegacy.text : boton.name;
    }
}
