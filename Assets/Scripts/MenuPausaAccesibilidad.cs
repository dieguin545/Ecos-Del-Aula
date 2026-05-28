using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuPausaAccesibilidad : MonoBehaviour
{
    public static bool EstaPausado { get; private set; }

    public static void ResetearEstadoGlobalPausa()
    {
        EstaPausado = false;
        Time.timeScale = 1f;
    }

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelOpciones;

    [Header("Botones")]
    public Button botonContinuar;
    public Button botonOpciones;
    public Button botonReiniciar;
    public Button botonSalir;
    public Button botonVolver;

    [Header("Accesibilidad")]
    public Toggle toggleTextoGrande;
    public Toggle toggleAltoContraste;
    public Toggle toggleModoDaltonico;
    public Toggle toggleReducirEfectos;

    [Header("Tipos de daltonismo")]
    public Button botonDaltonismoNinguno;
    public Button botonDaltonismoProtanopia;
    public Button botonDaltonismoDeuteranopia;
    public Button botonDaltonismoTritanopia;
    public Button botonDaltonismoAcromatopsia;

    [Header("Partidas")]
    public Button botonSlot1;
    public Button botonSlot2;
    public Button botonSlot3;
    public TextMeshProUGUI textoSlotActivo;

    [Header("Detalle de slot")]
    public GameObject panelDetalleSlot;
    public TextMeshProUGUI textoDetalleSlot;
    public Button botonEntrarSlot;
    public Button botonBorrarSlot;
    public Button botonConfirmarBorrarSlot;
    public Button botonCancelarSlot;

    [Header("Jugador y camara")]
    public MonoBehaviour scriptMovimientoJugador;
    public GameObject camaraPrincipal;
    public MonoBehaviour[] scriptsExtraBloquear;

    [Header("Ventanas que evitan pausar")]
    public GameObject[] pantallasQueBloqueanPausa;

    [Header("Textos TMP")]
    public TextMeshProUGUI[] textosTMP;

    [Header("Textos normales")]
    public Text[] textosNormales;

    [Header("Fondos UI")]
    public Image[] imagenesUI;

    [Header("Escena menu")]
    public string nombreEscenaMenu = "inicio";

    private Behaviour scriptCamara;

    private float[] tamanosOriginalesTMP;
    private int[] tamanosOriginalesNormales;
    private Color[] coloresOriginalesTMP;
    private Color[] coloresOriginalesNormales;
    private Color[] coloresOriginalesImagenes;
    private int slotSeleccionado = 1;
    private bool confirmandoBorradoSlot;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        ReinicializarTrasCargaEscena();
    }

    private void Start()
    {
        ReinicializarTrasCargaEscena();
    }

    public void ReinicializarTrasCargaEscena()
    {
        ResetearEstadoGlobalPausa();
        NormalizarArreglosSerializados();
        ResolverReferenciasEscena();

        scriptCamara = camaraPrincipal != null
            ? camaraPrincipal.GetComponent("ControlCamara3D") as Behaviour
            : null;

        GuardarValoresOriginales();
        CargarOpciones();
        ConectarBotones();
        CerrarPaneles();
        BloquearJugadorYCamara(false);

        if (SceneManager.GetActiveScene().name == "Juego" && !PCRealmenteAbierta())
        {
            OcultarCursorGameplay();
        }

        AplicarAccesibilidad();
    }

    private void Update()
    {
        if (EstaPausado && !HayPanelDePausaVisible())
        {
            ResetearEstadoGlobalPausa();
        }

        if (MenuAbierto() && Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (panelDetalleSlot != null && panelDetalleSlot.activeSelf)
            {
                CerrarDetalleSlot();
                return;
            }

            if (panelOpciones != null && panelOpciones.activeSelf)
            {
                VolverAPausa();
                return;
            }

            Continuar();
            return;
        }

        if (!GestorEntradaGlobal.PausaPresionada())
        {
            return;
        }

        if (MenuAbierto())
        {
            if (panelDetalleSlot != null && panelDetalleSlot.activeSelf)
            {
                CerrarDetalleSlot();
                return;
            }

            Continuar();
            return;
        }

        if (PCRealmenteAbierta())
        {
            return;
        }

        if (Time.frameCount <= InteraccionPC.FrameCierrePC + 1)
        {
            return;
        }

        Pausar();
    }

    private void LateUpdate()
    {
        if (MenuAbierto())
        {
            MostrarCursorMenu();
        }
    }

    private void ConectarBotones()
    {
        if (botonContinuar != null)
        {
            botonContinuar.onClick.RemoveAllListeners();
            botonContinuar.onClick.AddListener(Continuar);
        }

        if (botonOpciones != null)
        {
            botonOpciones.onClick.RemoveAllListeners();
            botonOpciones.onClick.AddListener(AbrirOpciones);
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveAllListeners();
            botonReiniciar.onClick.AddListener(ReiniciarEscena);
        }

        if (botonSalir != null)
        {
            botonSalir.onClick.RemoveAllListeners();
            botonSalir.onClick.AddListener(SalirAlMenu);
        }

        if (botonVolver != null)
        {
            botonVolver.onClick.RemoveAllListeners();
            botonVolver.onClick.AddListener(VolverAPausa);
        }

        if (botonEntrarSlot != null)
        {
            botonEntrarSlot.onClick.RemoveAllListeners();
            botonEntrarSlot.onClick.AddListener(EntrarSlotSeleccionado);
        }

        if (botonBorrarSlot != null)
        {
            botonBorrarSlot.onClick.RemoveAllListeners();
            botonBorrarSlot.onClick.AddListener(SolicitarBorrarSlot);
        }

        if (botonConfirmarBorrarSlot != null)
        {
            botonConfirmarBorrarSlot.onClick.RemoveAllListeners();
            botonConfirmarBorrarSlot.onClick.AddListener(ConfirmarBorrarSlot);
        }

        if (botonCancelarSlot != null)
        {
            botonCancelarSlot.onClick.RemoveAllListeners();
            botonCancelarSlot.onClick.AddListener(CerrarDetalleSlot);
        }

        if (toggleTextoGrande != null)
        {
            toggleTextoGrande.onValueChanged.RemoveAllListeners();
            toggleTextoGrande.onValueChanged.AddListener(delegate
            {
                GuardarOpciones();
                AplicarAccesibilidad();
            });
        }

        if (toggleAltoContraste != null)
        {
            toggleAltoContraste.onValueChanged.RemoveAllListeners();
            toggleAltoContraste.onValueChanged.AddListener(delegate
            {
                GuardarOpciones();
                AplicarAccesibilidad();
            });
        }

        if (toggleModoDaltonico != null)
        {
            toggleModoDaltonico.onValueChanged.RemoveAllListeners();
            toggleModoDaltonico.onValueChanged.AddListener(delegate
            {
                GuardarOpciones();
                AplicarAccesibilidad();
            });
        }

        if (toggleReducirEfectos != null)
        {
            toggleReducirEfectos.onValueChanged.RemoveAllListeners();
            toggleReducirEfectos.gameObject.SetActive(false);
        }

        ConectarBotonDaltonismo(botonDaltonismoNinguno, TipoDaltonismo.Ninguno);
        ConectarBotonDaltonismo(botonDaltonismoProtanopia, TipoDaltonismo.Protanopia);
        ConectarBotonDaltonismo(botonDaltonismoDeuteranopia, TipoDaltonismo.Deuteranopia);
        ConectarBotonDaltonismo(botonDaltonismoTritanopia, TipoDaltonismo.Tritanopia);
        ConectarBotonDaltonismo(botonDaltonismoAcromatopsia, TipoDaltonismo.Acromatopsia);

        ConectarBotonSlot(botonSlot1, 1);
        ConectarBotonSlot(botonSlot2, 2);
        ConectarBotonSlot(botonSlot3, 3);
    }

    public void Pausar()
    {
        if (PCRealmenteAbierta())
        {
            return;
        }

        EstaPausado = true;

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelDetalleSlot != null)
        {
            panelDetalleSlot.SetActive(false);
        }

        Time.timeScale = 0f;

        BloquearJugadorYCamara(true);
        MostrarCursorMenu();
        FocalizarPanel(panelPausa);
    }

    public void Continuar()
    {
        EstaPausado = false;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        Time.timeScale = 1f;

        if (!PCRealmenteAbierta())
        {
            BloquearJugadorYCamara(false);
            OcultarCursorGameplay();
        }
    }

    public void AbrirOpciones()
    {
        EstaPausado = true;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }

        if (panelDetalleSlot != null)
        {
            panelDetalleSlot.SetActive(false);
        }

        Time.timeScale = 0f;

        BloquearJugadorYCamara(true);
        MostrarCursorMenu();
        FocalizarPanel(panelOpciones);
    }

    public void VolverAPausa()
    {
        EstaPausado = true;

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        if (panelDetalleSlot != null)
        {
            panelDetalleSlot.SetActive(false);
        }

        Time.timeScale = 0f;

        BloquearJugadorYCamara(true);
        MostrarCursorMenu();
        FocalizarPanel(panelPausa);
    }

    private void FocalizarPanel(GameObject panel)
    {
        if (panel == null || UnityEngine.EventSystems.EventSystem.current == null) return;
        Selectable[] selectables = panel.GetComponentsInChildren<Selectable>(true);
        foreach (var s in selectables)
        {
            if (s != null)
            {
                Navigation nav = s.navigation;
                nav.mode = Navigation.Mode.Automatic;
                s.navigation = nav;
            }
        }
        foreach (var s in selectables)
        {
            if (s != null && s.gameObject.activeInHierarchy && s.interactable && s.navigation.mode != Navigation.Mode.None)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(s.gameObject);
                break;
            }
        }
    }

    public void ReiniciarEscena()
    {
        CancelInvoke();
        CerrarPaneles();
        InteraccionPC.ResetearEstadoGlobalPC();
        ResetearEstadoGlobalPausa();
        BloquearJugadorYCamara(false);
        OcultarCursorGameplay();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SalirAlMenu()
    {
        EstaPausado = false;
        Time.timeScale = 1f;

        BloquearJugadorYCamara(false);
        MostrarCursorMenu();

        string escenaMenu = string.IsNullOrWhiteSpace(nombreEscenaMenu) || nombreEscenaMenu == "Inicio"
            ? "inicio"
            : nombreEscenaMenu;
        SceneManager.LoadScene(escenaMenu);
    }

    private bool MenuAbierto()
    {
        bool pausaAbierta = panelPausa != null && panelPausa.activeSelf;
        bool opcionesAbiertas = panelOpciones != null && panelOpciones.activeSelf;
        bool detalleSlotAbierto = panelDetalleSlot != null && panelDetalleSlot.activeSelf;

        return EstaPausado || pausaAbierta || opcionesAbiertas || detalleSlotAbierto;
    }

    private bool HayPanelDePausaVisible()
    {
        return PanelActivo(panelPausa)
            || PanelActivo(panelOpciones)
            || PanelActivo(panelDetalleSlot);
    }

    private bool PanelActivo(GameObject panel)
    {
        return panel != null && panel.activeInHierarchy;
    }

    private void CerrarPaneles()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelDetalleSlot != null)
        {
            panelDetalleSlot.SetActive(false);
        }

        confirmandoBorradoSlot = false;
    }

    private void ResolverReferenciasEscena()
    {
        InteraccionPC interaccionPC = FindAnyObjectByType<InteraccionPC>(FindObjectsInactive.Include);

        if (interaccionPC != null)
        {
            if (interaccionPC.scriptMovimientoJugador != null)
            {
                scriptMovimientoJugador = interaccionPC.scriptMovimientoJugador;
            }

            if (interaccionPC.camaraPrincipal != null)
            {
                camaraPrincipal = interaccionPC.camaraPrincipal;
            }

            if (interaccionPC.canvasPC != null)
            {
                pantallasQueBloqueanPausa = new[] { interaccionPC.canvasPC };
            }
        }

        if (scriptMovimientoJugador == null)
        {
            MovimientoJugador movimientoBasico = FindAnyObjectByType<MovimientoJugador>(FindObjectsInactive.Include);
            if (movimientoBasico != null)
            {
                scriptMovimientoJugador = movimientoBasico;
            }
        }

        if (scriptMovimientoJugador == null)
        {
            MovimientoJugadorConCamara movimientoConCamara = FindAnyObjectByType<MovimientoJugadorConCamara>(FindObjectsInactive.Include);
            if (movimientoConCamara != null)
            {
                scriptMovimientoJugador = movimientoConCamara;
            }
        }

        if (camaraPrincipal == null)
        {
            Camera camara = Camera.main;
            if (camara != null)
            {
                camaraPrincipal = camara.gameObject;
            }
        }

        if (camaraPrincipal == null)
        {
            GameObject camara = GameObject.Find("Main Camera");
            if (camara != null)
            {
                camaraPrincipal = camara;
            }
        }
    }

    private bool PCRealmenteAbierta()
    {
        if (!InteraccionPC.PCAbierta)
        {
            return false;
        }

        InteraccionPC[] pcs = FindObjectsByType<InteraccionPC>(FindObjectsInactive.Include);

        for (int i = 0; i < pcs.Length; i++)
        {
            if (pcs[i] != null && pcs[i].canvasPC != null && pcs[i].canvasPC.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private void BloquearJugadorYCamara(bool bloquear)
    {
        if (scriptMovimientoJugador != null)
        {
            scriptMovimientoJugador.enabled = !bloquear;
        }

        if (scriptCamara != null)
        {
            scriptCamara.enabled = !bloquear;
        }

        HabilitarScriptsExtra(!bloquear);
    }

    private void MostrarCursorMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OcultarCursorGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void GuardarOpciones()
    {
        ConfiguracionAccesibilidadJuego.Guardar(
            EstaTextoGrandeActivo(),
            EstaAltoContrasteActivo(),
            ObtenerTipoDaltonismoSeleccionado(),
            false
        );
    }

    private void CargarOpciones()
    {
        if (toggleTextoGrande != null)
        {
            toggleTextoGrande.isOn = ConfiguracionAccesibilidadJuego.TextoGrandeActivo;
        }

        if (toggleAltoContraste != null)
        {
            toggleAltoContraste.isOn = ConfiguracionAccesibilidadJuego.AltoContrasteActivo;
        }

        if (toggleModoDaltonico != null)
        {
            toggleModoDaltonico.isOn =
                ConfiguracionAccesibilidadJuego.TipoDaltonismoActual != TipoDaltonismo.Ninguno;
        }

        if (toggleReducirEfectos != null)
        {
            toggleReducirEfectos.isOn = false;
            toggleReducirEfectos.gameObject.SetActive(false);
        }

        ActualizarTextoSlotActivo();
        ActualizarBotonesDaltonismo();
    }

    private void GuardarValoresOriginales()
    {
        NormalizarArreglosSerializados();

        tamanosOriginalesTMP = new float[textosTMP.Length];
        coloresOriginalesTMP = new Color[textosTMP.Length];

        for (int i = 0; i < textosTMP.Length; i++)
        {
            if (textosTMP[i] != null)
            {
                tamanosOriginalesTMP[i] = textosTMP[i].fontSize;
                coloresOriginalesTMP[i] = textosTMP[i].color;
            }
        }

        tamanosOriginalesNormales = new int[textosNormales.Length];
        coloresOriginalesNormales = new Color[textosNormales.Length];

        for (int i = 0; i < textosNormales.Length; i++)
        {
            if (textosNormales[i] != null)
            {
                tamanosOriginalesNormales[i] = textosNormales[i].fontSize;
                coloresOriginalesNormales[i] = textosNormales[i].color;
            }
        }

        coloresOriginalesImagenes = new Color[imagenesUI.Length];

        for (int i = 0; i < imagenesUI.Length; i++)
        {
            if (imagenesUI[i] != null)
            {
                coloresOriginalesImagenes[i] = imagenesUI[i].color;
            }
        }
    }

    private void AplicarAccesibilidad()
    {
        AplicarTextosTMP();
        AplicarTextosNormales();
        AplicarFondosUI();
    }

    private void AplicarTextosTMP()
    {
        if (textosTMP == null || tamanosOriginalesTMP == null || coloresOriginalesTMP == null)
        {
            return;
        }

        for (int i = 0; i < textosTMP.Length; i++)
        {
            if (textosTMP[i] == null)
            {
                continue;
            }

            float tamanoBase = tamanosOriginalesTMP[i] <= 0f
                ? textosTMP[i].fontSize
                : tamanosOriginalesTMP[i];

            textosTMP[i].fontSize = EstaTextoGrandeActivo()
                ? tamanoBase * 1.10f
                : tamanoBase;

            textosTMP[i].color = EstaModoDaltonicoActivo()
                ? new Color(0.2f, 0.75f, 1f)
                : coloresOriginalesTMP[i];
        }
    }

    private void AplicarTextosNormales()
    {
        if (textosNormales == null || tamanosOriginalesNormales == null || coloresOriginalesNormales == null)
        {
            return;
        }

        for (int i = 0; i < textosNormales.Length; i++)
        {
            if (textosNormales[i] == null)
            {
                continue;
            }

            int tamanoBase = tamanosOriginalesNormales[i] <= 0
                ? textosNormales[i].fontSize
                : tamanosOriginalesNormales[i];

            textosNormales[i].fontSize = EstaTextoGrandeActivo()
                ? Mathf.RoundToInt(tamanoBase * 1.10f)
                : tamanoBase;

            textosNormales[i].color = EstaModoDaltonicoActivo()
                ? new Color(0.2f, 0.75f, 1f)
                : coloresOriginalesNormales[i];
        }
    }

    private void AplicarFondosUI()
    {
        if (imagenesUI == null || coloresOriginalesImagenes == null)
        {
            return;
        }

        for (int i = 0; i < imagenesUI.Length; i++)
        {
            if (imagenesUI[i] == null)
            {
                continue;
            }

            if (imagenesUI[i].sprite != null)
            {
                imagenesUI[i].color = Color.white;
            }
            else if (EstaAltoContrasteActivo())
            {
                // Only apply dark background to large panels to preserve outlines and detail scales
                RectTransform rect = imagenesUI[i].GetComponent<RectTransform>();
                if (rect != null && (rect.rect.width < 100f || rect.rect.height < 100f))
                {
                    imagenesUI[i].color = coloresOriginalesImagenes[i];
                }
                else
                {
                    imagenesUI[i].color = new Color(0.03f, 0.03f, 0.06f, 0.95f);
                }
            }
            else if (EstaModoDaltonicoActivo())
            {
                imagenesUI[i].color = new Color(0.05f, 0.12f, 0.22f, 0.8f);
            }
            else
            {
                imagenesUI[i].color = coloresOriginalesImagenes[i];
            }
        }
    }

    private bool EstaTextoGrandeActivo()
    {
        return toggleTextoGrande != null && toggleTextoGrande.isOn;
    }

    private bool EstaAltoContrasteActivo()
    {
        return toggleAltoContraste != null && toggleAltoContraste.isOn;
    }

    private bool EstaModoDaltonicoActivo()
    {
        return ConfiguracionAccesibilidadJuego.TipoDaltonismoActual != TipoDaltonismo.Ninguno;
    }

    private bool EstaReducirEfectosActivo()
    {
        return false;
    }

    private TipoDaltonismo ObtenerTipoDaltonismoSeleccionado()
    {
        if (toggleModoDaltonico != null && !toggleModoDaltonico.isOn)
        {
            return TipoDaltonismo.Ninguno;
        }

        TipoDaltonismo tipoGuardado = ConfiguracionAccesibilidadJuego.TipoDaltonismoActual;

        return tipoGuardado == TipoDaltonismo.Ninguno && toggleModoDaltonico != null
            ? TipoDaltonismo.Deuteranopia
            : tipoGuardado;
    }

    private void ConectarBotonDaltonismo(Button boton, TipoDaltonismo tipo)
    {
        if (boton == null)
        {
            return;
        }

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() => SeleccionarTipoDaltonismo(tipo));
    }

    private void ConectarBotonSlot(Button boton, int slot)
    {
        if (boton == null)
        {
            return;
        }

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() => AbrirDetalleSlot(slot));
    }

    public void SeleccionarTipoDaltonismo(TipoDaltonismo tipo)
    {
        if (toggleModoDaltonico != null)
        {
            toggleModoDaltonico.isOn = tipo != TipoDaltonismo.Ninguno;
        }

        ConfiguracionAccesibilidadJuego.Guardar(
            EstaTextoGrandeActivo(),
            EstaAltoContrasteActivo(),
            tipo,
            false
        );

        ActualizarBotonesDaltonismo();
        AplicarAccesibilidad();
        AplicadorAccesibilidadGlobal.AplicarEscenaActual();
    }

    public void AbrirDetalleSlot(int slot)
    {
        slotSeleccionado = slot;
        confirmandoBorradoSlot = false;

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelDetalleSlot != null)
        {
            panelDetalleSlot.SetActive(true);
        }

        ActualizarDetalleSlot();
        FocalizarPanel(panelDetalleSlot);
    }

    public void EntrarSlotSeleccionado()
    {
        ControlCorreo controlCorreo = FindAnyObjectByType<ControlCorreo>();

        if (controlCorreo != null)
        {
            controlCorreo.GuardarProgresoActual();
        }

        GestorGuardadoJuego gestor = new GestorGuardadoJuego(
            Path.Combine(Application.persistentDataPath, "partida_bullying.json")
        );

        if (!gestor.SeleccionarSlot(slotSeleccionado))
        {
            return;
        }

        EstaPausado = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SolicitarBorrarSlot()
    {
        confirmandoBorradoSlot = true;
        ActualizarDetalleSlot();
        FocalizarPanel(panelDetalleSlot);
    }

    public void ConfirmarBorrarSlot()
    {
        GestorGuardadoJuego gestor = new GestorGuardadoJuego(
            Path.Combine(Application.persistentDataPath, "partida_bullying.json")
        );

        gestor.BorrarSlot(slotSeleccionado);
        confirmandoBorradoSlot = false;
        ActualizarTextoSlotActivo();
        ActualizarDetalleSlot();
        FocalizarPanel(panelDetalleSlot);
    }

    public void CerrarDetalleSlot()
    {
        confirmandoBorradoSlot = false;

        if (panelDetalleSlot != null)
        {
            panelDetalleSlot.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }

        ActualizarTextoSlotActivo();
        FocalizarPanel(panelOpciones);
    }

    private void ActualizarDetalleSlot()
    {
        GestorGuardadoJuego gestor = new GestorGuardadoJuego(
            Path.Combine(Application.persistentDataPath, "partida_bullying.json")
        );

        ResumenSlotGuardadoJuego resumen = gestor.ObtenerResumenSlotPublico(slotSeleccionado);
        bool tieneDatos = resumen != null && resumen.tieneDatos && gestor.ExisteGuardado(slotSeleccionado);
        string rutaSlot = gestor.ObtenerRutaSlotPublica(slotSeleccionado);
        string archivoSlot = gestor.ObtenerNombreArchivoSlot(slotSeleccionado);

        if (textoDetalleSlot != null)
        {
            if (confirmandoBorradoSlot)
            {
                textoDetalleSlot.text =
                    "Slot " + slotSeleccionado +
                    "\nSeguro que quieres borrar este slot?\nEsta accion no borra los otros slots.";
            }
            else if (tieneDatos)
            {
                textoDetalleSlot.text =
                    "Slot " + slotSeleccionado +
                    "\nEstado: partida guardada" +
                    "\nDia: " + resumen.diaActual +
                    "\nDinero: $" + resumen.dineroTotal +
                    "\nUltimo guardado: " + (string.IsNullOrWhiteSpace(resumen.ultimaPartidaIso) ? "sin fecha" : resumen.ultimaPartidaIso) +
                    "\nArchivo: " + archivoSlot +
                    "\nRuta: " + rutaSlot;
            }
            else
            {
                textoDetalleSlot.text =
                    "Slot " + slotSeleccionado +
                    "\nEstado: Slot vacio" +
                    "\nArchivo: aun no creado" +
                    "\nRuta: " + rutaSlot +
                    "\nPuedes crear una partida nueva en este slot.";
            }
        }

        if (botonEntrarSlot != null)
        {
            botonEntrarSlot.gameObject.SetActive(!confirmandoBorradoSlot);
            TextMeshProUGUI textoBoton = botonEntrarSlot.GetComponentInChildren<TextMeshProUGUI>(true);

            if (textoBoton != null)
            {
                textoBoton.text = tieneDatos ? "Entrar" : "Crear / Entrar";
            }
        }

        if (botonBorrarSlot != null)
        {
            botonBorrarSlot.gameObject.SetActive(!confirmandoBorradoSlot);
            botonBorrarSlot.interactable = tieneDatos;
        }

        if (botonConfirmarBorrarSlot != null)
        {
            botonConfirmarBorrarSlot.gameObject.SetActive(confirmandoBorradoSlot);
        }
    }

    private void ActualizarTextoSlotActivo()
    {
        if (textoSlotActivo == null)
        {
            return;
        }

        GestorGuardadoJuego gestor = new GestorGuardadoJuego(
            Path.Combine(Application.persistentDataPath, "partida_bullying.json")
        );

        textoSlotActivo.text = "Slot activo: " + gestor.SlotActivo;
    }

    private void ActualizarBotonesDaltonismo()
    {
        TipoDaltonismo actual = ConfiguracionAccesibilidadJuego.TipoDaltonismoActual;

        MarcarBotonDaltonismo(botonDaltonismoNinguno, actual == TipoDaltonismo.Ninguno);
        MarcarBotonDaltonismo(botonDaltonismoProtanopia, actual == TipoDaltonismo.Protanopia);
        MarcarBotonDaltonismo(botonDaltonismoDeuteranopia, actual == TipoDaltonismo.Deuteranopia);
        MarcarBotonDaltonismo(botonDaltonismoTritanopia, actual == TipoDaltonismo.Tritanopia);
        MarcarBotonDaltonismo(botonDaltonismoAcromatopsia, actual == TipoDaltonismo.Acromatopsia);
    }

    private void MarcarBotonDaltonismo(Button boton, bool seleccionado)
    {
        if (boton == null)
        {
            return;
        }

        EstiloUIJuego.AplicarBoton(
            boton,
            seleccionado ? EstiloUIJuego.Acento : EstiloUIJuego.FondoTarjeta,
            seleccionado ? EstiloUIJuego.AcentoCalido : new Color(0.14f, 0.38f, 0.58f, 1f)
        );
    }

    private void NormalizarArreglosSerializados()
    {
        if (scriptsExtraBloquear == null)
        {
            scriptsExtraBloquear = new MonoBehaviour[0];
        }

        if (pantallasQueBloqueanPausa == null)
        {
            pantallasQueBloqueanPausa = new GameObject[0];
        }

        if (textosTMP == null)
        {
            textosTMP = new TextMeshProUGUI[0];
        }

        if (textosNormales == null)
        {
            textosNormales = new Text[0];
        }

        if (imagenesUI == null)
        {
            imagenesUI = new Image[0];
        }
    }

    private void HabilitarScriptsExtra(bool habilitar)
    {
        if (scriptsExtraBloquear == null)
        {
            return;
        }

        for (int i = 0; i < scriptsExtraBloquear.Length; i++)
        {
            if (scriptsExtraBloquear[i] != null)
            {
                scriptsExtraBloquear[i].enabled = habilitar;
            }
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;

        if (EstaPausado)
        {
            EstaPausado = false;
            Time.timeScale = 1f;
        }
    }
}
