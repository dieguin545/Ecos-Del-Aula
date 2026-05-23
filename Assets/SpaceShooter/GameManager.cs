using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Vidas")]
    [SerializeField] private int vidas = 3;
    [SerializeField] private int vidasMaximas = 3;
    [SerializeField] private Image[] iconosVida;

    [Header("Objetivo")]
    [SerializeField] private int objetivoEnemigos = 25;
    [SerializeField] private TextMeshProUGUI textoObjetivo;
    [SerializeField] private TextMeshProUGUI textoEstado;
    [SerializeField] private TextMeshProUGUI textoVidas;
    [SerializeField] private TextMeshProUGUI textoDash;
    [SerializeField] private TextMeshProUGUI textoTurbo;
    [SerializeField] private TextMeshProUGUI textoAlerta;

    [Header("Sprites HUD futuros")]
    [SerializeField] private Sprite spriteVida;
    [SerializeField] private Sprite spriteEscudo;
    [SerializeField] private Sprite spriteTurbo;
    [SerializeField] private Sprite spriteDash;
    [SerializeField] private Sprite spriteReparacion;
    [SerializeField] private Sprite spriteAmenaza;

    [Header("Pantallas")]
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject panelVictoria;

    [Header("Dependencias")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private NaveController naveController;
    [SerializeField] private SelectorNave selectorNave;
    [SerializeField] private MenuSpaceShooter menuSpaceShooter;
    [SerializeField] private ControladorPausaSpaceShooter controladorPausa;
    [SerializeField] private AccesibilidadSpaceShooter accesibilidadSpaceShooter;
    [SerializeField] private CrosshairApuntado crosshairApuntado;

    private readonly DatosPartidaSpaceShooter datosPartida = new DatosPartidaSpaceShooter();
    private readonly System.Collections.Generic.List<Image> iconosDash =
        new System.Collections.Generic.List<Image>();

    private ResultadoMinijuego ultimoResultado;
    private ConfiguracionDificultad dificultadActual =
        ConfiguracionDificultad.CrearPredeterminada(DificultadSpaceShooter.Medio);
    private LeaderboardSpaceShooter leaderboard;
    private string aliasJugador = "Jugador";
    private string nombreNaveSeleccionada = "Escudo digital";
    private bool resultadoRegistrado;
    private Image panelHudVisual;
    private Image barraTurboFondo;
    private Image barraTurboRelleno;
    private TextMeshProUGUI textoGameOverResumen;
    private TextMeshProUGUI textoVictoriaResumen;
    private bool escudoActivo;
    private float tiempoRestanteEscudo;
    private EstadoPartidaSpaceShooter estadoActual = EstadoPartidaSpaceShooter.Menu;
    private EstadoPartidaSpaceShooter estadoAntesDePausa = EstadoPartidaSpaceShooter.Menu;

    public bool juegoActivo;
    public bool victoria { get; private set; }
    public int enemigosDestruidos => datosPartida.EnemigosDestruidos;
    public int Vidas => vidas;
    public ResultadoMinijuego UltimoResultado => ultimoResultado;
    public EstadoPartidaSpaceShooter EstadoActual => estadoActual;
    public bool TieneEscudo => escudoActivo;
    public bool PuedePausar =>
        estadoActual == EstadoPartidaSpaceShooter.Jugando
        || estadoActual == EstadoPartidaSpaceShooter.Menu
        || estadoActual == EstadoPartidaSpaceShooter.Pausa;
    public bool PuedeControlarGameplay => estadoActual == EstadoPartidaSpaceShooter.Jugando;

    private void Awake()
    {
        instancia = this;
        leaderboard = new LeaderboardSpaceShooter(
            Path.Combine(Application.persistentDataPath, "leaderboard_spaceshooter.json")
        );
    }

    private void Start()
    {
        PrepararDependencias();
        PrepararUiObjetivoSiHaceFalta();
        ConectarEventos();
        EntrarAMenuInicial();
    }

    private void Update()
    {
        if (estadoActual == EstadoPartidaSpaceShooter.Jugando)
        {
            datosPartida.ActualizarTiempo(Time.deltaTime);
        }

        ActualizarEscudo();
    }

    private void OnDisable()
    {
        if (spawnManager != null)
        {
            spawnManager.AlMeteoritoDestruido -= ManejarMeteoritoDestruido;
        }

        if (naveController != null)
        {
            naveController.AlActualizarDash -= ActualizarDash;
            naveController.AlActualizarTurbo -= ActualizarTurbo;
        }
    }

    public void IniciarPartida(
        DificultadSpaceShooter dificultad,
        string alias,
        ModeloNaveDisponible naveSeleccionada
    )
    {
        dificultadActual = ConfiguracionDificultad.CrearPredeterminada(dificultad);
        aliasJugador = string.IsNullOrWhiteSpace(alias) ? "Jugador" : alias.Trim();
        nombreNaveSeleccionada =
            naveSeleccionada != null && !string.IsNullOrWhiteSpace(naveSeleccionada.nombre)
                ? naveSeleccionada.nombre
                : "Escudo digital";

        objetivoEnemigos = dificultadActual.objetivoEnemigos;
        vidas = vidasMaximas;
        EstablecerEstado(EstadoPartidaSpaceShooter.Jugando);
        resultadoRegistrado = false;
        ultimoResultado = null;
        escudoActivo = false;
        tiempoRestanteEscudo = 0f;
        datosPartida.Reiniciar();

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }

        if (textoEstado != null)
        {
            textoEstado.text = "Protege la convivencia digital";
        }

        if (spawnManager != null)
        {
            spawnManager.AplicarDificultad(dificultadActual);
        }

        if (naveController != null)
        {
            naveController.ConfigurarDash(
                dificultadActual.cargasDash,
                dificultadActual.recargaDashSegundos
            );
            FirewallDelAulaVisuales.AplicarANave(naveController.gameObject);
        }

        ActualizarUI();
        ActualizarTextoObjetivo();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PerderVida(int cantidad = 1)
    {
        if (estadoActual != EstadoPartidaSpaceShooter.Jugando)
        {
            return;
        }

        if (escudoActivo)
        {
            escudoActivo = false;
            tiempoRestanteEscudo = 0f;
            return;
        }

        cantidad = Mathf.Max(1, cantidad);
        vidas = Mathf.Max(0, vidas - cantidad);
        datosPartida.RegistrarDanio(cantidad);
        ActualizarUI();

        if (vidas <= 0)
        {
            GameOver();
        }
    }

    public bool RecuperarVida()
    {
        if (vidas < vidasMaximas)
        {
            vidas++;
            ActualizarUI();
            return true;
        }

        return false;
    }

    public void Reiniciar()
    {
        IniciarPartida(dificultadActual.dificultad, aliasJugador, selectorNave != null ? selectorNave.SeleccionActual : null);
    }

    public void VolverAlMenuInicial()
    {
        LimpiarObjetosDePartida();
        datosPartida.Reiniciar();
        ultimoResultado = null;
        EntrarAMenuInicial();
    }

    private void EntrarAMenuInicial()
    {
        EstablecerEstado(EstadoPartidaSpaceShooter.Menu);
        Time.timeScale = 1f;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }

        ActualizarUI();
        ActualizarTextoObjetivo();

        if (menuSpaceShooter != null)
        {
            menuSpaceShooter.MostrarMenuInicial();
        }
    }

    private void PrepararDependencias()
    {
        if (spawnManager == null)
        {
            spawnManager = FindAnyObjectByType<SpawnManager>();
        }

        if (naveController == null)
        {
            naveController = FindAnyObjectByType<NaveController>();
        }

        if (crosshairApuntado == null)
        {
            crosshairApuntado = FindAnyObjectByType<CrosshairApuntado>();
        }

        if (selectorNave == null && naveController != null)
        {
            selectorNave = naveController.GetComponent<SelectorNave>();

            if (selectorNave == null)
            {
                selectorNave = naveController.gameObject.AddComponent<SelectorNave>();
            }

            selectorNave.ConfigurarDestino(
                naveController.transform.Find("ModeloSeleccionado"),
                naveController.GetComponentsInChildren<Renderer>()
            );
        }

        if (accesibilidadSpaceShooter == null)
        {
            accesibilidadSpaceShooter = GetComponent<AccesibilidadSpaceShooter>();

            if (accesibilidadSpaceShooter == null)
            {
                accesibilidadSpaceShooter = gameObject.AddComponent<AccesibilidadSpaceShooter>();
            }
        }

        if (menuSpaceShooter == null)
        {
            menuSpaceShooter = GetComponent<MenuSpaceShooter>();

            if (menuSpaceShooter == null)
            {
                menuSpaceShooter = gameObject.AddComponent<MenuSpaceShooter>();
            }
        }

        if (controladorPausa == null)
        {
            controladorPausa = GetComponent<ControladorPausaSpaceShooter>();

            if (controladorPausa == null)
            {
                controladorPausa = gameObject.AddComponent<ControladorPausaSpaceShooter>();
            }
        }

        if (GetComponent<GestorAlertasAtaque>() == null)
        {
            gameObject.AddComponent<GestorAlertasAtaque>();
        }
    }

    private void ConectarEventos()
    {
        if (spawnManager != null)
        {
            spawnManager.AlMeteoritoDestruido -= ManejarMeteoritoDestruido;
            spawnManager.AlMeteoritoDestruido += ManejarMeteoritoDestruido;
        }

        if (naveController != null)
        {
            naveController.AlActualizarDash -= ActualizarDash;
            naveController.AlActualizarDash += ActualizarDash;
            naveController.AlActualizarTurbo -= ActualizarTurbo;
            naveController.AlActualizarTurbo += ActualizarTurbo;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (menuSpaceShooter != null)
        {
            menuSpaceShooter.Inicializar(
                this,
                selectorNave,
                accesibilidadSpaceShooter,
                leaderboard,
                canvas
            );
        }

        if (controladorPausa != null)
        {
            controladorPausa.Inicializar(this, menuSpaceShooter, canvas);
        }

        if (accesibilidadSpaceShooter != null)
        {
            accesibilidadSpaceShooter.AplicarATodos();
        }
    }

    private void ManejarMeteoritoDestruido(Meteorito meteorito, bool destruidoPorJugador)
    {
        if (!juegoActivo || !destruidoPorJugador || meteorito == null)
        {
            return;
        }

        datosPartida.RegistrarAmenazaDestruida(meteorito.Tipo);
        ActualizarTextoObjetivo();

        if (datosPartida.EnemigosDestruidos >= objetivoEnemigos)
        {
            Victoria();
        }
    }

    private void ActualizarUI()
    {
        for (int i = 0; i < iconosVida.Length; i++)
        {
            if (iconosVida[i] != null)
            {
                iconosVida[i].enabled = i < vidas;
            }
        }

        AplicarVidasSimples();
    }

    private void ActualizarTextoObjetivo()
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text =
                "Neutralizados: "
                + datosPartida.EnemigosDestruidos
                + "/"
                + objetivoEnemigos;
        }
    }

    private void CargarSpritesHudSiHaceFalta()
    {
        if (spriteVida == null)
        {
            spriteVida = FirewallDelAulaVisuales.CargarSprite(
                "Icons/ffffff/transparent/1x1/lorc/riot-shield"
            );
        }

        if (spriteDash == null)
        {
            spriteDash = FirewallDelAulaVisuales.CargarSprite(
                "Icons/ffffff/transparent/1x1/delapouite/rule-book"
            );
        }

        if (spriteTurbo == null)
        {
            spriteTurbo = FirewallDelAulaVisuales.CargarSprite(
                "UI/PNG/Blue/Default/bar_round_gloss_large"
            );
        }
    }

    private void ActualizarDash(int cargasActuales, int cargasMaximas, float recargaRestante)
    {
        if (textoDash == null)
        {
            return;
        }

        textoDash.text =
            "PROTOCOLO "
            + cargasActuales
            + "/"
            + cargasMaximas;

        AsegurarIconosDash(cargasMaximas);

        for (int i = 0; i < iconosDash.Count; i++)
        {
            Image icono = iconosDash[i];

            if (icono == null)
            {
                continue;
            }

            bool disponible = i < cargasActuales;
            icono.sprite = spriteDash;
            icono.color =
                disponible
                    ? (spriteDash != null ? Color.white : new Color(0.25f, 0.9f, 1f, 1f))
                    : (spriteDash != null ? new Color(1f, 1f, 1f, 0.22f) : new Color(0.25f, 0.9f, 1f, 0.22f));
            icono.preserveAspect = spriteDash != null;
        }
    }

    private void GameOver()
    {
        EstablecerEstado(EstadoPartidaSpaceShooter.GameOver);
        ultimoResultado = datosPartida.CrearResultado(false, dificultadActual, nombreNaveSeleccionada);
        RegistrarResultadoSiHaceFalta();

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            EcosAulaPromptUI.CrearBarraPrompts(panelGameOver.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Confirmar"));
            
            Selectable first = EncontrarPrimerSelectableEnPanel(panelGameOver);
            if (first != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(first.gameObject);
            }
        }

        ActualizarResumenFinal(textoGameOverResumen);

        if (textoEstado != null)
        {
            textoEstado.text = "La convivencia digital fue saturada";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Victoria()
    {
        EstablecerEstado(EstadoPartidaSpaceShooter.Victoria);
        ultimoResultado = datosPartida.CrearResultado(true, dificultadActual, nombreNaveSeleccionada);
        RegistrarResultadoSiHaceFalta();

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            EcosAulaPromptUI.CrearBarraPrompts(panelVictoria.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Confirmar"));
            
            Selectable first = EncontrarPrimerSelectableEnPanel(panelVictoria);
            if (first != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(first.gameObject);
            }
        }

        ActualizarResumenFinal(textoVictoriaResumen);

        if (textoEstado != null)
        {
            textoEstado.text = "Mensajes dañinos neutralizados";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RegistrarResultadoSiHaceFalta()
    {
        if (resultadoRegistrado || ultimoResultado == null || leaderboard == null)
        {
            return;
        }

        leaderboard.RegistrarResultado(aliasJugador, ultimoResultado);
        resultadoRegistrado = true;
    }

    private void PrepararUiObjetivoSiHaceFalta()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            return;
        }

        CargarSpritesHudSiHaceFalta();
        PrepararFondoEstelarSiHaceFalta();
        PrepararPanelHudSiHaceFalta(canvas.transform);

        if (textoObjetivo == null)
        {
            textoObjetivo = CrearTextoHud(
                canvas.transform,
                "TextoObjetivo",
                new Vector2(24f, -22f),
                24f
            );
        }

        if (textoEstado == null)
        {
            textoEstado = CrearTextoHud(
                canvas.transform,
                "TextoEstado",
                new Vector2(24f, -52f),
                16f
            );
            textoEstado.text = "Protege la convivencia digital";
        }

        if (textoVidas == null)
        {
            textoVidas = CrearTextoHud(
                canvas.transform,
                "TextoVidas",
                new Vector2(24f, -86f),
                18f
            );
            textoVidas.text = "BIENESTAR";
        }

        if (textoDash == null)
        {
            textoDash = CrearTextoHud(
                canvas.transform,
                "TextoDash",
                new Vector2(24f, -124f),
                18f
            );
            textoDash.text = "PROTOCOLO 0/0";
        }

        if (textoTurbo == null)
        {
            textoTurbo = CrearTextoHud(
                canvas.transform,
                "TextoTurbo",
                new Vector2(330f, -86f),
                18f
            );
            textoTurbo.text = "TURBO 100%";
        }

        if (textoAlerta == null)
        {
            textoAlerta = CrearTextoHud(
                canvas.transform,
                "TextoAlerta",
                new Vector2(24f, -168f),
                18f
            );
            textoAlerta.text = string.Empty;
        }

        ConfigurarTextoHud(textoObjetivo, new Vector2(24f, -20f), 22f, new Vector2(590f, 30f));
        ConfigurarTextoHud(textoEstado, new Vector2(24f, -48f), 15f, new Vector2(560f, 26f));
        ConfigurarTextoHud(textoVidas, new Vector2(24f, -82f), 17f, new Vector2(210f, 28f));
        ConfigurarTextoHud(textoDash, new Vector2(24f, -122f), 17f, new Vector2(170f, 28f));
        ConfigurarTextoHud(textoTurbo, new Vector2(410f, -82f), 17f, new Vector2(190f, 28f));
        ConfigurarTextoHud(textoAlerta, new Vector2(24f, -168f), 18f, new Vector2(590f, 30f));

        if (textoVidas != null)
        {
            textoVidas.text = "BIENESTAR DIGITAL";
        }

        PrepararBarraTurboSiHaceFalta(canvas.transform);
        PrepararPanelGameOverSiHaceFalta();

        if (panelVictoria == null)
        {
            panelVictoria = CrearPanelVictoria(canvas.transform);
        }

        AplicarVidasSimples();
    }

    private TextMeshProUGUI CrearTextoHud(
        Transform padre,
        string nombre,
        Vector2 posicion,
        float tamanoFuente
    )
    {
        GameObject objetoTexto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        objetoTexto.transform.SetParent(padre, false);

        RectTransform rect = objetoTexto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(520f, 34f);

        TextMeshProUGUI texto = objetoTexto.GetComponent<TextMeshProUGUI>();
        texto.fontSize = tamanoFuente;
        texto.alignment = TextAlignmentOptions.TopLeft;
        texto.color = Color.white;
        texto.raycastTarget = false;

        return texto;
    }

    private void ConfigurarTextoHud(
        TextMeshProUGUI texto,
        Vector2 posicion,
        float tamanoFuente,
        Vector2 tamanoRect
    )
    {
        if (texto == null)
        {
            return;
        }

        RectTransform rect = texto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamanoRect;

        texto.fontSize = tamanoFuente;
        texto.alignment = TextAlignmentOptions.TopLeft;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.overflowMode = TextOverflowModes.Overflow;
        texto.raycastTarget = false;
    }

    private GameObject CrearPanelVictoria(Transform padre)
    {
        GameObject panel = MenuSpaceShooter.CrearPanelBase(
            padre,
            "Victoria",
            new Color(0f, 0f, 0f, 0.82f)
        );

        MenuSpaceShooter.CrearTexto(
            panel.transform,
            "TextoVictoria",
            "MENSAJES DAÑINOS NEUTRALIZADOS",
            44f,
            new Vector2(0f, 130f),
            new Vector2(900f, 100f),
            TextAlignmentOptions.Center
        );
        MenuSpaceShooter.CrearTexto(
            panel.transform,
            "TextoVictoriaSub",
            "La convivencia digital quedó protegida",
            28f,
            new Vector2(0f, 60f),
            new Vector2(700f, 50f),
            TextAlignmentOptions.Center
        );
        textoVictoriaResumen = MenuSpaceShooter.CrearTexto(
            panel.transform,
            "TextoVictoriaResumen",
            string.Empty,
            22f,
            new Vector2(0f, -12f),
            new Vector2(760f, 112f),
            TextAlignmentOptions.Center
        );
        MenuSpaceShooter.CrearBoton(panel.transform, "BotonVictoriaReiniciar", "Reiniciar", new Vector2(0f, -126f), Reiniciar);
        MenuSpaceShooter.CrearBoton(panel.transform, "BotonVictoriaMenu", "Volver al menu", new Vector2(0f, -206f), VolverAlMenuInicial);

        panel.SetActive(false);
        return panel;
    }

    private void LimpiarObjetosDePartida()
    {
        if (spawnManager != null)
        {
            spawnManager.ReiniciarProgreso();
        }

        foreach (PowerUpSpaceShooter powerUp in FindObjectsByType<PowerUpSpaceShooter>())
        {
            Destroy(powerUp.gameObject);
        }

        foreach (Bala bala in FindObjectsByType<Bala>())
        {
            Destroy(bala.gameObject);
        }

        foreach (ProyectilEnemigo proyectil in FindObjectsByType<ProyectilEnemigo>())
        {
            Destroy(proyectil.gameObject);
        }
    }

    private void PrepararFondoEstelarSiHaceFalta()
    {
        if (FindAnyObjectByType<FondoEstelarSpaceShooter>() != null)
        {
            return;
        }

        gameObject.AddComponent<FondoEstelarSpaceShooter>();
    }

    private void PrepararPanelHudSiHaceFalta(Transform padre)
    {
        if (panelHudVisual != null)
        {
            return;
        }

        GameObject panel = new GameObject(
            "PanelHudVisual",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        panel.transform.SetParent(padre, false);
        panel.transform.SetAsFirstSibling();

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(14f, -14f);
        rect.sizeDelta = new Vector2(650f, 190f);

        panelHudVisual = panel.GetComponent<Image>();
        panelHudVisual.color = new Color(0.01f, 0.04f, 0.09f, 0.78f);
        panelHudVisual.raycastTarget = false;

        Outline borde = panel.AddComponent<Outline>();
        borde.effectColor = new Color(0.25f, 0.9f, 1f, 0.42f);
        borde.effectDistance = new Vector2(1f, -1f);
    }

    private void ActualizarTurbo(float energiaActual, float energiaMaxima)
    {
        if (textoTurbo == null || energiaMaxima <= 0f)
        {
            return;
        }

        float porcentaje = Mathf.Clamp01(energiaActual / energiaMaxima);
        textoTurbo.text = "TURBO " + Mathf.RoundToInt(porcentaje * 100f) + "%";

        if (barraTurboRelleno != null)
        {
            barraTurboRelleno.fillAmount = porcentaje;
        }
    }

    private void AplicarVidasSimples()
    {
        if (iconosVida.Length > 0 && iconosVida[0] != null)
        {
            Image fondoVidas = iconosVida[0].transform.parent.GetComponent<Image>();

            if (fondoVidas != null)
            {
                fondoVidas.color = Color.clear;
                fondoVidas.raycastTarget = false;
            }
        }

        for (int i = 0; i < iconosVida.Length; i++)
        {
            if (iconosVida[i] == null)
            {
                continue;
            }

            iconosVida[i].sprite = spriteVida;
            iconosVida[i].color = spriteVida != null ? Color.white : new Color(0.25f, 0.9f, 1f, 1f);
            iconosVida[i].preserveAspect = spriteVida != null;

            RectTransform rect = iconosVida[i].rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(22f, 22f);
            rect.anchoredPosition = new Vector2(218f + i * 30f, -82f);
        }
    }

    private void AsegurarIconosDash(int cantidad)
    {
        if (
            cantidad <= 0
            || textoDash == null
        )
        {
            return;
        }

        Transform padre = textoDash.transform.parent;

        while (iconosDash.Count < cantidad)
        {
            int indice = iconosDash.Count;
            GameObject objeto = new GameObject(
                "IconoDash_" + indice,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            objeto.transform.SetParent(padre, false);

            RectTransform rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(186f + indice * 30f, -122f);
            rect.sizeDelta = new Vector2(22f, 22f);

            Image imagen = objeto.GetComponent<Image>();
            imagen.sprite = spriteDash;
            imagen.color = new Color(0.25f, 0.9f, 1f, 1f);
            imagen.preserveAspect = spriteDash != null;
            imagen.raycastTarget = false;
            iconosDash.Add(imagen);
        }

        for (int i = 0; i < iconosDash.Count; i++)
        {
            if (iconosDash[i] != null)
            {
                iconosDash[i].gameObject.SetActive(i < cantidad);
            }
        }
    }

    public void ActivarEscudo(float duracion)
    {
        escudoActivo = true;
        tiempoRestanteEscudo = Mathf.Max(tiempoRestanteEscudo, duracion);
    }

    private void PrepararBarraTurboSiHaceFalta(Transform padre)
    {
        if (barraTurboFondo != null || padre == null)
        {
            return;
        }

        GameObject fondo = new GameObject(
            "BarraTurboFondo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        fondo.transform.SetParent(padre, false);

        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = new Vector2(0f, 1f);
        rectFondo.anchorMax = new Vector2(0f, 1f);
        rectFondo.pivot = new Vector2(0f, 1f);
        rectFondo.anchoredPosition = new Vector2(410f, -116f);
        rectFondo.sizeDelta = new Vector2(190f, 20f);

        barraTurboFondo = fondo.GetComponent<Image>();
        barraTurboFondo.sprite = spriteTurbo;
        barraTurboFondo.color = new Color(1f, 1f, 1f, 0.16f);
        barraTurboFondo.preserveAspect = false;
        barraTurboFondo.raycastTarget = false;

        GameObject relleno = new GameObject(
            "BarraTurboRelleno",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        relleno.transform.SetParent(fondo.transform, false);

        RectTransform rectRelleno = relleno.GetComponent<RectTransform>();
        rectRelleno.anchorMin = Vector2.zero;
        rectRelleno.anchorMax = Vector2.one;
        rectRelleno.offsetMin = Vector2.zero;
        rectRelleno.offsetMax = Vector2.zero;

        barraTurboRelleno = relleno.GetComponent<Image>();
        barraTurboRelleno.sprite = FirewallDelAulaVisuales.CargarSprite(
            "UI/PNG/Green/Default/bar_round_gloss_large"
        );
        barraTurboRelleno.color = new Color(0.25f, 0.9f, 1f, 0.95f);
        barraTurboRelleno.type = Image.Type.Filled;
        barraTurboRelleno.fillMethod = Image.FillMethod.Horizontal;
        barraTurboRelleno.fillOrigin = 0;
        barraTurboRelleno.fillAmount = 1f;
        barraTurboRelleno.raycastTarget = false;
    }

    private void PrepararPanelGameOverSiHaceFalta()
    {
        if (panelGameOver == null)
        {
            return;
        }

        Image fondo = panelGameOver.GetComponent<Image>();

        if (fondo != null)
        {
            fondo.color = new Color(0f, 0f, 0f, 0.84f);
        }

        if (panelGameOver.transform.Find("TarjetaGameOver") == null)
        {
            CrearTarjetaFinal(panelGameOver.transform, "TarjetaGameOver");
        }

        TextMeshProUGUI titulo = BuscarTextoDirecto(panelGameOver.transform);

        if (titulo != null)
        {
            titulo.text = "La convivencia digital fue saturada";
            titulo.fontSize = 48f;
            RectTransform rectTitulo = titulo.rectTransform;
            rectTitulo.anchoredPosition = new Vector2(0f, 180f);
            rectTitulo.sizeDelta = new Vector2(760f, 72f);
        }

        Transform subtituloExistente = panelGameOver.transform.Find("TextoGameOverSubtitulo");

        if (subtituloExistente == null)
        {
            MenuSpaceShooter.CrearTexto(
                panelGameOver.transform,
                "TextoGameOverSubtitulo",
                "Refuerza el firewall y vuelve a proteger el aula.",
                24f,
                new Vector2(0f, 124f),
                new Vector2(760f, 42f),
                TextAlignmentOptions.Center
            );
        }

        if (textoGameOverResumen == null)
        {
            textoGameOverResumen = MenuSpaceShooter.CrearTexto(
                panelGameOver.transform,
                "TextoGameOverResumen",
                string.Empty,
                22f,
                new Vector2(0f, 28f),
                new Vector2(760f, 128f),
                TextAlignmentOptions.Center
            );
        }

        Button[] botones = panelGameOver.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null)
            {
                continue;
            }

            if (botones[i].name == "Reintentar")
            {
                ConfigurarBotonFinal(botones[i], "Reintentar", new Vector2(-180f, -138f));
            }
            else
            {
                ConfigurarBotonFinal(botones[i], "Volver a la PC", new Vector2(180f, -138f));
            }
        }

        if (panelGameOver.transform.Find("BotonGameOverMenu") == null)
        {
            MenuSpaceShooter.CrearBoton(
                panelGameOver.transform,
                "BotonGameOverMenu",
                "Volver al menu",
                new Vector2(0f, -214f),
                VolverAlMenuInicial
            );
        }
    }

    private void CrearTarjetaFinal(Transform padre, string nombre)
    {
        GameObject tarjeta = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        tarjeta.transform.SetParent(padre, false);
        tarjeta.transform.SetAsFirstSibling();

        RectTransform rect = tarjeta.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(780f, 500f);

        Image imagen = tarjeta.GetComponent<Image>();
        imagen.color = new Color(0.02f, 0.05f, 0.1f, 0.94f);
        imagen.raycastTarget = false;

        Outline borde = tarjeta.AddComponent<Outline>();
        borde.effectColor = new Color(0.25f, 0.9f, 1f, 0.4f);
        borde.effectDistance = new Vector2(1f, -1f);
    }

    private TextMeshProUGUI BuscarTextoDirecto(Transform padre)
    {
        TextMeshProUGUI[] textos = padre.GetComponentsInChildren<TextMeshProUGUI>(true);

        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] != null && textos[i].transform.parent == padre)
            {
                return textos[i];
            }
        }

        return null;
    }

    private void ConfigurarBotonFinal(Button boton, string etiqueta, Vector2 posicion)
    {
        RectTransform rect = boton.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = new Vector2(260f, 58f);
        }

        Image imagen = boton.GetComponent<Image>();

        if (imagen != null)
        {
            imagen.color = new Color(0.08f, 0.28f, 0.42f, 0.95f);
        }

        TextMeshProUGUI texto = boton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (texto != null)
        {
            texto.text = etiqueta;
            texto.fontSize = 24f;
            texto.color = Color.white;
        }

        if (boton != null)
        {
            Navigation nav = boton.navigation;
            nav.mode = Navigation.Mode.Automatic;
            boton.navigation = nav;
        }
    }

    private Selectable EncontrarPrimerSelectableEnPanel(GameObject panel)
    {
        if (panel == null) return null;
        Selectable[] components = panel.GetComponentsInChildren<Selectable>(true);
        foreach (var c in components)
        {
            if (c.gameObject.activeInHierarchy && c.interactable && c.navigation.mode != Navigation.Mode.None)
            {
                return c;
            }
        }
        return null;
    }

    private void ActualizarResumenFinal(TextMeshProUGUI destino)
    {
        if (destino == null || ultimoResultado == null)
        {
            return;
        }

        int mejorPuntaje = ultimoResultado.Puntaje;

        if (leaderboard != null)
        {
            System.Collections.Generic.IReadOnlyList<RegistroLeaderboard> top = leaderboard.ObtenerTop(1);

            if (top.Count > 0)
            {
                mejorPuntaje = Mathf.Max(mejorPuntaje, top[0].puntaje);
            }
        }

        destino.text =
            "Mensajes neutralizados: "
            + ultimoResultado.EnemigosDestruidos
            + "\nDificultad: "
            + ultimoResultado.Dificultad
            + "  |  Tiempo: "
            + ultimoResultado.TiempoSobrevivido.ToString("0.0")
            + " s"
            + "\nFirewall: "
            + ultimoResultado.NaveUsada
            + "  |  Convivencia protegida: "
            + ultimoResultado.Puntaje
            + "\nMejor registro: "
            + mejorPuntaje;
    }

    public void RegistrarPuntosExtra(int cantidad)
    {
        datosPartida.RegistrarPuntosExtra(cantidad);
    }

    public void RegistrarPowerUpRecogido(TipoPowerUp tipo)
    {
        datosPartida.RegistrarPowerUpRecogido(tipo);
    }

    public void MostrarAlertaAmenaza(int cantidad)
    {
        if (textoAlerta == null || estadoActual != EstadoPartidaSpaceShooter.Jugando)
        {
            return;
        }

        textoAlerta.text =
            cantidad <= 1 ? "Amenaza entrante" : "Amenazas entrantes: " + cantidad;
        CancelInvoke(nameof(OcultarAlertaAmenaza));
        Invoke(nameof(OcultarAlertaAmenaza), 1.1f);
    }

    public void NotificarPausa(bool pausado)
    {
        if (
            pausado
            && estadoActual != EstadoPartidaSpaceShooter.Pausa
            && estadoActual != EstadoPartidaSpaceShooter.GameOver
            && estadoActual != EstadoPartidaSpaceShooter.Victoria
        )
        {
            estadoAntesDePausa = estadoActual;
            EstablecerEstado(EstadoPartidaSpaceShooter.Pausa);
        }
        else if (!pausado && estadoActual == EstadoPartidaSpaceShooter.Pausa)
        {
            EstadoPartidaSpaceShooter estadoDestino =
                estadoAntesDePausa == EstadoPartidaSpaceShooter.Pausa
                    ? EstadoPartidaSpaceShooter.Menu
                    : estadoAntesDePausa;

            EstablecerEstado(estadoDestino);

            if (estadoDestino == EstadoPartidaSpaceShooter.Menu && menuSpaceShooter != null)
            {
                menuSpaceShooter.MostrarMenuInicial();
            }
        }
    }

    private void ActualizarEscudo()
    {
        if (!escudoActivo || estadoActual != EstadoPartidaSpaceShooter.Jugando)
        {
            return;
        }

        tiempoRestanteEscudo -= Time.deltaTime;

        if (tiempoRestanteEscudo <= 0f)
        {
            escudoActivo = false;
            tiempoRestanteEscudo = 0f;
        }
    }

    private void EstablecerEstado(EstadoPartidaSpaceShooter nuevoEstado)
    {
        estadoActual = nuevoEstado;
        juegoActivo =
            nuevoEstado == EstadoPartidaSpaceShooter.Jugando
            || nuevoEstado == EstadoPartidaSpaceShooter.Pausa;
        victoria = nuevoEstado == EstadoPartidaSpaceShooter.Victoria;
        ActualizarVisibilidadCrosshair();
    }

    private void ActualizarVisibilidadCrosshair()
    {
        if (crosshairApuntado != null)
        {
            crosshairApuntado.EstablecerVisible(estadoActual == EstadoPartidaSpaceShooter.Jugando);
        }
    }

    public void AplicarAccesibilidadVisual()
    {
        float escalaTexto = AccesibilidadSpaceShooter.TextoGrandeActivo ? 1.18f : 1f;
        Color colorTexto =
            AccesibilidadSpaceShooter.AltoContrasteActivo ? Color.white : new Color(0.9f, 0.98f, 1f);

        AplicarTextoHud(textoObjetivo, 22f * escalaTexto, colorTexto);
        AplicarTextoHud(textoEstado, 15f * escalaTexto, colorTexto);
        AplicarTextoHud(textoVidas, 17f * escalaTexto, colorTexto);
        AplicarTextoHud(textoDash, 17f * escalaTexto, colorTexto);
        AplicarTextoHud(textoTurbo, 17f * escalaTexto, colorTexto);
        AplicarTextoHud(textoAlerta, 18f * escalaTexto, colorTexto);

        if (panelHudVisual != null)
        {
            panelHudVisual.color =
                AccesibilidadSpaceShooter.AltoContrasteActivo
                    ? new Color(0f, 0f, 0f, 0.94f)
                    : new Color(0.01f, 0.04f, 0.09f, 0.78f);
        }
    }

    private void AplicarTextoHud(TextMeshProUGUI texto, float tamano, Color color)
    {
        if (texto == null)
        {
            return;
        }

        texto.fontSize = tamano;
        texto.color = color;
    }

    private void OcultarAlertaAmenaza()
    {
        if (textoAlerta != null)
        {
            textoAlerta.text = string.Empty;
        }
    }
}
