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
    [SerializeField] private TextMeshProUGUI textoDash;

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
    private string nombreNaveSeleccionada = "Nave base";
    private bool resultadoRegistrado;
    private Image panelHudVisual;
    private bool escudoActivo;
    private float tiempoRestanteEscudo;
    private EstadoPartidaSpaceShooter estadoActual = EstadoPartidaSpaceShooter.Menu;

    public bool juegoActivo;
    public bool victoria { get; private set; }
    public int enemigosDestruidos => datosPartida.EnemigosDestruidos;
    public int Vidas => vidas;
    public ResultadoMinijuego UltimoResultado => ultimoResultado;
    public EstadoPartidaSpaceShooter EstadoActual => estadoActual;
    public bool TieneEscudo => escudoActivo;
    public bool PuedePausar =>
        estadoActual == EstadoPartidaSpaceShooter.Jugando
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
                : "Nave base";

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
            textoEstado.text = "Defiendete de los ataques digitales";
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
                "Amenazas neutralizadas: "
                + datosPartida.EnemigosDestruidos
                + "/"
                + objetivoEnemigos;
        }
    }

    private void ActualizarDash(int cargasActuales, int cargasMaximas, float recargaRestante)
    {
        if (textoDash == null)
        {
            return;
        }

        textoDash.text =
            "DASH "
            + cargasActuales
            + "/"
            + cargasMaximas
            + (cargasActuales < cargasMaximas ? "  recarga " + recargaRestante.ToString("0.0") + "s" : "");

        AsegurarIconosDash(cargasMaximas);

        for (int i = 0; i < iconosDash.Count; i++)
        {
            Image icono = iconosDash[i];

            if (icono == null)
            {
                continue;
            }

            bool disponible = i < cargasActuales;
            icono.sprite = null;
            icono.color =
                disponible
                    ? new Color(0.25f, 0.9f, 1f, 1f)
                    : new Color(0.25f, 0.9f, 1f, 0.22f);
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
        }

        if (textoEstado != null)
        {
            textoEstado.text = "Vuelve a intentarlo";
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
        }

        if (textoEstado != null)
        {
            textoEstado.text = "Rompe el ciclo";
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

        PrepararFondoEstelarSiHaceFalta();
        PrepararPanelHudSiHaceFalta(canvas.transform);

        if (textoObjetivo == null)
        {
            textoObjetivo = CrearTextoHud(
                canvas.transform,
                "TextoObjetivo",
                new Vector2(24f, -24f),
                28f
            );
        }

        if (textoEstado == null)
        {
            textoEstado = CrearTextoHud(
                canvas.transform,
                "TextoEstado",
                new Vector2(24f, -64f),
                22f
            );
            textoEstado.text = "Defiendete de los ataques digitales";
        }

        if (textoDash == null)
        {
            textoDash = CrearTextoHud(
                canvas.transform,
                "TextoDash",
                new Vector2(24f, -104f),
                22f
            );
            textoDash.text = "DASH 0/0";
        }

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
        rect.sizeDelta = new Vector2(700f, 40f);

        TextMeshProUGUI texto = objetoTexto.GetComponent<TextMeshProUGUI>();
        texto.fontSize = tamanoFuente;
        texto.alignment = TextAlignmentOptions.TopLeft;
        texto.color = Color.white;
        texto.raycastTarget = false;

        return texto;
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
            "ROMPE EL CICLO",
            52f,
            new Vector2(0f, 130f),
            new Vector2(900f, 100f),
            TextAlignmentOptions.Center
        );
        MenuSpaceShooter.CrearTexto(
            panel.transform,
            "TextoVictoriaSub",
            "Objetivo cumplido",
            28f,
            new Vector2(0f, 60f),
            new Vector2(700f, 50f),
            TextAlignmentOptions.Center
        );
        MenuSpaceShooter.CrearBoton(panel.transform, "BotonVictoriaReiniciar", "Reiniciar", new Vector2(0f, -35f), Reiniciar);
        MenuSpaceShooter.CrearBoton(panel.transform, "BotonVictoriaMenu", "Volver al menu", new Vector2(0f, -115f), VolverAlMenuInicial);

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
        rect.sizeDelta = new Vector2(430f, 180f);

        panelHudVisual = panel.GetComponent<Image>();
        panelHudVisual.color = new Color(0.01f, 0.04f, 0.09f, 0.78f);
        panelHudVisual.raycastTarget = false;
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

            iconosVida[i].sprite = null;
            iconosVida[i].color = new Color(0.25f, 0.9f, 1f, 1f);
            iconosVida[i].preserveAspect = false;

            RectTransform rect = iconosVida[i].rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(22f, 22f);
            rect.anchoredPosition = new Vector2(270f + i * 30f, -138f);
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
            rect.anchoredPosition = new Vector2(24f + indice * 28f, -138f);
            rect.sizeDelta = new Vector2(22f, 22f);

            Image imagen = objeto.GetComponent<Image>();
            imagen.color = new Color(0.25f, 0.9f, 1f, 1f);
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

    public void RegistrarPuntosExtra(int cantidad)
    {
        datosPartida.RegistrarPuntosExtra(cantidad);
    }

    public void RegistrarPowerUpRecogido(TipoPowerUp tipo)
    {
        datosPartida.RegistrarPowerUpRecogido(tipo);
    }

    public void NotificarPausa(bool pausado)
    {
        if (pausado && estadoActual == EstadoPartidaSpaceShooter.Jugando)
        {
            EstablecerEstado(EstadoPartidaSpaceShooter.Pausa);
        }
        else if (!pausado && estadoActual == EstadoPartidaSpaceShooter.Pausa)
        {
            EstablecerEstado(EstadoPartidaSpaceShooter.Jugando);
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
}
