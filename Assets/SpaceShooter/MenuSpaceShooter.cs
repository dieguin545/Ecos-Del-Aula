using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSpaceShooter : MonoBehaviour
{
    private readonly string[] tutorialPaginas =
    {
        "OBJETIVO\nProtege la convivencia digital. Intercepta rumores, burlas y mensajes dañinos antes de que afecten el aula.",
        "REGLA EDUCATIVA\nNo se ataca a personas: se detienen conductas dañinas con reportes y evidencia.",
        "CONTROLES\nWASD: mover | Espacio/Ctrl: subir y bajar | Mouse: apuntar | Click: enviar reporte.",
        "DEFENSA\nQ y E consumen cargas de protocolo. Úsalas para esquivar alertas peligrosas."
    };

    private GameManager gameManager;
    private SelectorNave selectorNave;
    private AccesibilidadSpaceShooter accesibilidad;
    private LeaderboardSpaceShooter leaderboard;
    private Canvas canvas;

    private GameObject panelMenu;
    private GameObject panelTutorial;
    private GameObject panelLeaderboard;
    private GameObject panelOpciones;

    private TMP_InputField inputAlias;
    private TextMeshProUGUI textoDificultad;
    private TextMeshProUGUI textoNave;
    private TextMeshProUGUI textoTutorial;
    private TextMeshProUGUI textoLeaderboard;
    private TextMeshProUGUI textoTipoDaltonismo;
    private Toggle toggleTextoGrande;
    private Toggle toggleAltoContraste;

    private DificultadSpaceShooter dificultadSeleccionada = DificultadSpaceShooter.Medio;
    private int paginaTutorialActual;

    public bool MenuVisible => panelMenu != null && panelMenu.activeSelf;

    private void OnEnable()
    {
        GestorEntradaGlobal.AlCambiarDispositivo += AlCambiarDispositivo;
    }

    private void OnDisable()
    {
        GestorEntradaGlobal.AlCambiarDispositivo -= AlCambiarDispositivo;
    }

    public void Inicializar(
        GameManager gameManager,
        SelectorNave selectorNave,
        AccesibilidadSpaceShooter accesibilidad,
        LeaderboardSpaceShooter leaderboard,
        Canvas canvas
    )
    {
        this.gameManager = gameManager;
        this.selectorNave = selectorNave;
        this.accesibilidad = accesibilidad;
        this.leaderboard = leaderboard;
        this.canvas = canvas;

        ConstruirUiSiHaceFalta();
        ActualizarTextoDificultad();
        ActualizarTextoNave();
        ActualizarTutorial();
        ActualizarLeaderboard();

        if (toggleTextoGrande != null)
        {
            toggleTextoGrande.isOn = AccesibilidadSpaceShooter.TextoGrandeActivo;
        }

        if (toggleAltoContraste != null)
        {
            toggleAltoContraste.isOn = AccesibilidadSpaceShooter.AltoContrasteActivo;
        }

    }

    public void MostrarMenuInicial()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ActivarSolo(panelMenu);
    }

    public void OcultarTodo()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelTutorial != null) panelTutorial.SetActive(false);
        if (panelLeaderboard != null) panelLeaderboard.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void MostrarTutorial()
    {
        paginaTutorialActual = 0;
        ActualizarTutorial();
        ActivarSolo(panelTutorial);
    }

    public void SiguienteTutorial()
    {
        if (paginaTutorialActual < tutorialPaginas.Length - 1)
        {
            paginaTutorialActual++;
            ActualizarTutorial();
            return;
        }

        VolverAlMenu();
    }

    public void TutorialAnterior()
    {
        if (paginaTutorialActual > 0)
        {
            paginaTutorialActual--;
            ActualizarTutorial();
        }
    }

    public void MostrarLeaderboard()
    {
        ActualizarLeaderboard();
        ActivarSolo(panelLeaderboard);
    }

    public void MostrarOpciones()
    {
        ActivarSolo(panelOpciones);
    }

    public void VolverAlMenu()
    {
        ActivarSolo(panelMenu);
    }

    public void SalirAlJuegoPrincipal()
    {
        SceneManager.LoadScene("Juego");
    }

    public void SeleccionarFacil()
    {
        dificultadSeleccionada = DificultadSpaceShooter.Facil;
        ActualizarTextoDificultad();
    }

    public void SeleccionarMedio()
    {
        dificultadSeleccionada = DificultadSpaceShooter.Medio;
        ActualizarTextoDificultad();
    }

    public void SeleccionarDificil()
    {
        dificultadSeleccionada = DificultadSpaceShooter.Dificil;
        ActualizarTextoDificultad();
    }

    public void SeleccionarNaveSiguiente()
    {
        if (selectorNave != null)
        {
            selectorNave.SeleccionarSiguiente();
        }

        ActualizarTextoNave();
    }

    public void SeleccionarNaveAnterior()
    {
        if (selectorNave != null)
        {
            selectorNave.SeleccionarAnterior();
        }

        ActualizarTextoNave();
    }

    public void Empezar()
    {
        if (gameManager == null)
        {
            return;
        }

        string alias = inputAlias != null ? inputAlias.text : "Jugador";
        ModeloNaveDisponible nave = selectorNave != null ? selectorNave.SeleccionActual : null;
        gameManager.IniciarPartida(dificultadSeleccionada, alias, nave);
        OcultarTodo();
    }

    public void AlternarModoDaltonico(bool activo)
    {
        if (accesibilidad != null)
        {
            accesibilidad.EstablecerModoDaltonico(activo);
        }
    }

    public void AlternarTextoGrande(bool activo)
    {
        if (accesibilidad != null)
        {
            accesibilidad.EstablecerTextoGrande(activo);
        }
    }

    public void AlternarAltoContraste(bool activo)
    {
        if (accesibilidad != null)
        {
            accesibilidad.EstablecerAltoContraste(activo);
        }
    }

    public void AlternarReducirEfectos(bool activo)
    {
        // Opcion retirada de la UI visible; se conserva el metodo por compatibilidad.
    }

    public void SeleccionarTipoDaltonismoAnterior()
    {
        CambiarTipoDaltonismo(-1);
    }

    public void SeleccionarTipoDaltonismoSiguiente()
    {
        CambiarTipoDaltonismo(1);
    }

    public static GameObject CrearPanelBase(Transform padre, string nombre, Color color)
    {
        GameObject panel = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        panel.transform.SetParent(padre, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image imagen = panel.GetComponent<Image>();
        imagen.color = color;

        return panel;
    }

    public static TextMeshProUGUI CrearTexto(
        Transform padre,
        string nombre,
        string contenido,
        float tamano,
        Vector2 posicion,
        Vector2 tamanoRect,
        TextAlignmentOptions alineacion
    )
    {
        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamanoRect;

        TextMeshProUGUI texto = objeto.GetComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = Color.white;
        texto.raycastTarget = false;

        return texto;
    }

    public static Button CrearBoton(
        Transform padre,
        string nombre,
        string texto,
        Vector2 posicion,
        UnityEngine.Events.UnityAction accion
    )
    {
        return CrearBoton(padre, nombre, texto, posicion, accion, new Vector2(260f, 58f), 24f);
    }

    public static Button CrearBoton(
        Transform padre,
        string nombre,
        string texto,
        Vector2 posicion,
        UnityEngine.Events.UnityAction accion,
        Vector2 tamano,
        float tamanoTexto
    )
    {
        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;

        Image imagen = objeto.GetComponent<Image>();
        imagen.sprite = FirewallDelAulaVisuales.CargarSprite(
            "UI/PNG/Blue/Default/button_square_header_large_rectangle"
        );
        imagen.color = new Color(0.35f, 0.9f, 1f, imagen.sprite != null ? 0.95f : 1f);

        Button boton = objeto.GetComponent<Button>();
        boton.onClick.AddListener(accion);

        Navigation nav = boton.navigation;
        nav.mode = Navigation.Mode.Automatic;
        boton.navigation = nav;

        TextMeshProUGUI textoBoton = CrearTexto(
            objeto.transform,
            "Texto",
            texto,
            tamanoTexto,
            Vector2.zero,
            new Vector2(Mathf.Max(10f, tamano.x - 10f), Mathf.Max(10f, tamano.y - 8f)),
            TextAlignmentOptions.Center
        );
        textoBoton.raycastTarget = false;

        return boton;
    }

    private void ConstruirUiSiHaceFalta()
    {
        if (canvas == null || panelMenu != null)
        {
            return;
        }

        panelMenu = CrearPanelBase(canvas.transform, "PanelMenuSpaceShooter", new Color(0.01f, 0.03f, 0.07f, 0.94f));
        CrearTexto(
            panelMenu.transform,
            "TituloMenu",
            "FIREWALL DEL AULA",
            42f,
            new Vector2(0f, 300f),
            new Vector2(1100f, 70f),
            TextAlignmentOptions.Center
        );
        CrearTexto(
            panelMenu.transform,
            "SubtituloMenu",
            "Protege la convivencia digital. Intercepta rumores, burlas y mensajes dañinos.",
            24f,
            new Vector2(0f, 250f),
            new Vector2(800f, 40f),
            TextAlignmentOptions.Center
        );

        inputAlias = CrearInputAlias(panelMenu.transform);

        textoDificultad = CrearTexto(
            panelMenu.transform,
            "TextoDificultad",
            "",
            24f,
            new Vector2(0f, 145f),
            new Vector2(650f, 40f),
            TextAlignmentOptions.Center
        );
        CrearBoton(panelMenu.transform, "BotonFacil", "Facil", new Vector2(-280f, 95f), SeleccionarFacil);
        CrearBoton(panelMenu.transform, "BotonMedio", "Medio", new Vector2(0f, 95f), SeleccionarMedio);
        CrearBoton(panelMenu.transform, "BotonDificil", "Dificil", new Vector2(280f, 95f), SeleccionarDificil);

        textoNave = CrearTexto(
            panelMenu.transform,
            "TextoNave",
            "",
            24f,
            new Vector2(0f, 15f),
            new Vector2(700f, 40f),
            TextAlignmentOptions.Center
        );

        CrearBoton(panelMenu.transform, "BotonNaveAnterior", "<", new Vector2(-180f, -40f), SeleccionarNaveAnterior);
        CrearBoton(panelMenu.transform, "BotonNaveSiguiente", ">", new Vector2(180f, -40f), SeleccionarNaveSiguiente);

        CrearBoton(panelMenu.transform, "BotonEmpezar", "Iniciar defensa", new Vector2(0f, -130f), Empezar);
        CrearBoton(panelMenu.transform, "BotonTutorial", "Tutorial", new Vector2(-280f, -215f), MostrarTutorial);
        CrearBoton(panelMenu.transform, "BotonLeaderboard", "Leaderboard", new Vector2(0f, -215f), MostrarLeaderboard);
        CrearBoton(panelMenu.transform, "BotonOpciones", "Accesibilidad", new Vector2(280f, -215f), MostrarOpciones);
        CrearBoton(panelMenu.transform, "BotonSalir", "Volver", new Vector2(0f, -300f), SalirAlJuegoPrincipal);

        ConstruirTutorial();
        ConstruirLeaderboard();
        ConstruirOpciones();
    }

    private TMP_InputField CrearInputAlias(Transform padre)
    {
        GameObject objeto = new GameObject(
            "InputAlias",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(TMP_InputField)
        );

        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 205f);
        rect.sizeDelta = new Vector2(420f, 52f);

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = new Color(0.05f, 0.09f, 0.15f, 0.95f);

        TextMeshProUGUI texto = CrearTexto(
            objeto.transform,
            "Texto",
            "",
            24f,
            Vector2.zero,
            new Vector2(390f, 42f),
            TextAlignmentOptions.Left
        );
        texto.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        texto.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        texto.rectTransform.offsetMin = new Vector2(14f, -21f);
        texto.rectTransform.offsetMax = new Vector2(-14f, 21f);

        TextMeshProUGUI placeholder = CrearTexto(
            objeto.transform,
            "Placeholder",
            "Alias del moderador",
            22f,
            Vector2.zero,
            new Vector2(390f, 42f),
            TextAlignmentOptions.Left
        );
        placeholder.color = new Color(1f, 1f, 1f, 0.45f);
        placeholder.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        placeholder.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        placeholder.rectTransform.offsetMin = new Vector2(14f, -21f);
        placeholder.rectTransform.offsetMax = new Vector2(-14f, 21f);

        TMP_InputField input = objeto.GetComponent<TMP_InputField>();
        input.textComponent = texto;
        input.placeholder = placeholder;
        input.characterLimit = 16;
        input.text = "Jugador";

        return input;
    }

    private void ConstruirTutorial()
    {
        panelTutorial = CrearPanelBase(canvas.transform, "PanelTutorialSpaceShooter", new Color(0.01f, 0.03f, 0.07f, 0.96f));
        CrearTexto(
            panelTutorial.transform,
            "TituloTutorial",
            "TUTORIAL",
            42f,
            new Vector2(0f, 250f),
            new Vector2(700f, 60f),
            TextAlignmentOptions.Center
        );
        textoTutorial = CrearTexto(
            panelTutorial.transform,
            "TextoTutorial",
            "",
            30f,
            Vector2.zero,
            new Vector2(1000f, 220f),
            TextAlignmentOptions.Center
        );
        CrearBoton(panelTutorial.transform, "BotonTutorialAnterior", "Anterior", new Vector2(-180f, -210f), TutorialAnterior);
        CrearBoton(panelTutorial.transform, "BotonTutorialSiguiente", "Siguiente", new Vector2(180f, -210f), SiguienteTutorial);
        CrearBoton(panelTutorial.transform, "BotonTutorialVolver", "Volver", new Vector2(0f, -290f), VolverAlMenu);
        panelTutorial.SetActive(false);
    }

    private void ConstruirLeaderboard()
    {
        panelLeaderboard = CrearPanelBase(canvas.transform, "PanelLeaderboardSpaceShooter", new Color(0.01f, 0.03f, 0.07f, 0.96f));
        CrearTexto(
            panelLeaderboard.transform,
            "TituloLeaderboard",
            "LEADERBOARD",
            42f,
            new Vector2(0f, 275f),
            new Vector2(700f, 60f),
            TextAlignmentOptions.Center
        );
        textoLeaderboard = CrearTexto(
            panelLeaderboard.transform,
            "TextoLeaderboard",
            "",
            24f,
            new Vector2(0f, 5f),
            new Vector2(1200f, 420f),
            TextAlignmentOptions.TopLeft
        );
        CrearBoton(panelLeaderboard.transform, "BotonLeaderboardVolver", "Volver", new Vector2(0f, -290f), VolverAlMenu);
        panelLeaderboard.SetActive(false);
    }

    private void ConstruirOpciones()
    {
        panelOpciones = CrearPanelBase(canvas.transform, "PanelOpcionesSpaceShooter", new Color(0.01f, 0.03f, 0.07f, 0.96f));
        CrearTexto(
            panelOpciones.transform,
            "TituloOpciones",
            "ACCESIBILIDAD",
            42f,
            new Vector2(0f, 250f),
            new Vector2(700f, 60f),
            TextAlignmentOptions.Center
        );
        CrearTexto(
            panelOpciones.transform,
            "DescripcionOpciones",
            "Ajusta lectura, contraste y paleta de amenazas.",
            26f,
            new Vector2(0f, 185f),
            new Vector2(900f, 60f),
            TextAlignmentOptions.Center
        );

        textoTipoDaltonismo = CrearTexto(
            panelOpciones.transform,
            "TextoTipoDaltonismo",
            "",
            26f,
            new Vector2(0f, 110f),
            new Vector2(700f, 40f),
            TextAlignmentOptions.Center
        );
        CrearBoton(panelOpciones.transform, "BotonDaltonismoAnterior", "<", new Vector2(-255f, 110f), SeleccionarTipoDaltonismoAnterior, new Vector2(58f, 42f), 22f);
        CrearBoton(panelOpciones.transform, "BotonDaltonismoSiguiente", ">", new Vector2(255f, 110f), SeleccionarTipoDaltonismoSiguiente, new Vector2(58f, 42f), 22f);

        toggleTextoGrande = CrearToggle(
            panelOpciones.transform,
            "ToggleTextoGrande",
            "Texto grande",
            new Vector2(0f, 30f)
        );
        toggleTextoGrande.onValueChanged.AddListener(AlternarTextoGrande);

        toggleAltoContraste = CrearToggle(
            panelOpciones.transform,
            "ToggleAltoContraste",
            "Alto contraste",
            new Vector2(0f, -32f)
        );
        toggleAltoContraste.onValueChanged.AddListener(AlternarAltoContraste);

        CrearBoton(panelOpciones.transform, "BotonOpcionesVolver", "Volver", new Vector2(0f, -160f), VolverAlMenu, new Vector2(180f, 44f), 20f);
        panelOpciones.SetActive(false);
        ActualizarTextoTipoDaltonismo();
    }

    private Toggle CrearToggle(Transform padre, string nombre, string texto, Vector2 posicion)
    {
        GameObject raiz = new GameObject(nombre, typeof(RectTransform), typeof(Toggle));
        raiz.transform.SetParent(padre, false);

        RectTransform rectRaiz = raiz.GetComponent<RectTransform>();
        rectRaiz.anchorMin = new Vector2(0.5f, 0.5f);
        rectRaiz.anchorMax = new Vector2(0.5f, 0.5f);
        rectRaiz.pivot = new Vector2(0.5f, 0.5f);
        rectRaiz.anchoredPosition = posicion;
        rectRaiz.sizeDelta = new Vector2(420f, 60f);

        GameObject fondo = new GameObject(
            "Fondo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        fondo.transform.SetParent(raiz.transform, false);
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = new Vector2(0f, 0.5f);
        rectFondo.anchorMax = new Vector2(0f, 0.5f);
        rectFondo.pivot = new Vector2(0f, 0.5f);
        rectFondo.anchoredPosition = new Vector2(0f, 0f);
        rectFondo.sizeDelta = new Vector2(42f, 42f);
        Image imagenFondo = fondo.GetComponent<Image>();
        imagenFondo.color = new Color(0.08f, 0.28f, 0.42f, 1f);

        GameObject marca = new GameObject(
            "Marca",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        marca.transform.SetParent(fondo.transform, false);
        RectTransform rectMarca = marca.GetComponent<RectTransform>();
        rectMarca.anchorMin = new Vector2(0.5f, 0.5f);
        rectMarca.anchorMax = new Vector2(0.5f, 0.5f);
        rectMarca.pivot = new Vector2(0.5f, 0.5f);
        rectMarca.sizeDelta = new Vector2(28f, 28f);
        Image imagenMarca = marca.GetComponent<Image>();
        imagenMarca.color = new Color(1f, 0.84f, 0.12f, 1f);

        TextMeshProUGUI etiqueta = CrearTexto(
            raiz.transform,
            "Etiqueta",
            texto,
            28f,
            new Vector2(70f, 0f),
            new Vector2(320f, 50f),
            TextAlignmentOptions.Left
        );
        etiqueta.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        etiqueta.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        etiqueta.rectTransform.pivot = new Vector2(0f, 0.5f);

        Toggle toggle = raiz.GetComponent<Toggle>();
        toggle.targetGraphic = imagenFondo;
        toggle.graphic = imagenMarca;

        return toggle;
    }

    private void ActualizarTextoDificultad()
    {
        if (textoDificultad == null)
        {
            return;
        }

        ConfiguracionDificultad config = ConfiguracionDificultad.CrearPredeterminada(dificultadSeleccionada);
        textoDificultad.text =
            "Dificultad: "
            + config.nombreVisible
            + " | objetivo "
            + config.objetivoEnemigos
            + " | protocolo "
            + config.cargasDash;
    }

    private void ActualizarTextoNave()
    {
        if (textoNave == null)
        {
            return;
        }

        string nombreNave =
            selectorNave != null && selectorNave.SeleccionActual != null
                ? selectorNave.SeleccionActual.nombre
                : "Escudo digital";

        int total = selectorNave != null ? selectorNave.CantidadModelos : 0;
        textoNave.text =
            total > 0
                ? "Firewall seleccionado: " + nombreNave + " (" + total + " diseños)"
                : "Firewall seleccionado: Escudo digital";
    }

    private void ActualizarTutorial()
    {
        if (textoTutorial != null)
        {
            textoTutorial.text =
                ObtenerPaginaTutorial(paginaTutorialActual)
                + "\n\n"
                + (paginaTutorialActual + 1)
                + "/"
                + tutorialPaginas.Length;
        }
    }

    private string ObtenerPaginaTutorial(int indice)
    {
        if (indice == 2)
        {
            return GestorEntradaGlobal.UsandoControl
                ? "CONTROLES XBOX\nStick izquierdo: mover | Stick derecho: apuntar | A: enviar reporte | Y/X: subir/bajar | RB: turbo."
                : "CONTROLES TECLADO\nWASD: mover | Mouse: apuntar | Click: enviar reporte | Shift: turbo.";
        }

        if (indice == 3)
        {
            return GestorEntradaGlobal.UsandoControl
                ? "PROTOCOLO\nLB y B consumen cargas de protocolo. Usalas para esquivar alertas peligrosas."
                : "PROTOCOLO\nQ y E consumen cargas de protocolo. Usalas para esquivar alertas peligrosas.";
        }

        return tutorialPaginas[Mathf.Clamp(indice, 0, tutorialPaginas.Length - 1)];
    }

    private void AlCambiarDispositivo(TipoDispositivoEntrada _)
    {
        ActualizarTutorial();
    }

    private void ActualizarLeaderboard()
    {
        if (textoLeaderboard == null || leaderboard == null)
        {
            return;
        }

        IReadOnlyList<RegistroLeaderboard> top = leaderboard.ObtenerTop(10);

        if (top.Count == 0)
        {
            textoLeaderboard.text = "Aun no hay partidas registradas.";
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("TOP 10");
        builder.AppendLine("Alias        Convivencia   Neutral. Tiempo   Dificultad   Firewall");

        for (int i = 0; i < top.Count; i++)
        {
            RegistroLeaderboard r = top[i];
            builder.AppendLine(
                (i + 1).ToString().PadLeft(2)
                + ". "
                + r.alias.PadRight(12)
                + r.puntaje.ToString().PadRight(10)
                + r.enemigosDestruidos.ToString().PadRight(8)
                + Mathf.RoundToInt(r.tiempoSobrevivido).ToString().PadRight(9)
                + r.dificultad.PadRight(13)
                + r.nave
            );
        }

        textoLeaderboard.text = builder.ToString();
    }

    private void ActivarSolo(GameObject objetivo)
    {
        if (panelMenu != null) panelMenu.SetActive(objetivo == panelMenu);
        if (panelTutorial != null) panelTutorial.SetActive(objetivo == panelTutorial);
        if (panelLeaderboard != null) panelLeaderboard.SetActive(objetivo == panelLeaderboard);
        if (panelOpciones != null) panelOpciones.SetActive(objetivo == panelOpciones);

        if (objetivo != null)
        {
            if (objetivo == panelMenu)
            {
                EcosAulaPromptUI.CrearBarraPrompts(panelMenu.transform,
                    (AccionLogica.Navegar, "Navegar"),
                    (AccionLogica.Confirmar, "Confirmar"),
                    (AccionLogica.Cancelar, "Volver"));
            }
            else if (objetivo == panelTutorial)
            {
                EcosAulaPromptUI.CrearBarraPrompts(panelTutorial.transform,
                    (AccionLogica.AnteriorPestana, "Anterior"),
                    (AccionLogica.SiguientePestana, "Siguiente"),
                    (AccionLogica.Cancelar, "Volver"));
            }
            else if (objetivo == panelLeaderboard)
            {
                EcosAulaPromptUI.CrearBarraPrompts(panelLeaderboard.transform,
                    (AccionLogica.Cancelar, "Volver"));
            }
            else if (objetivo == panelOpciones)
            {
                EcosAulaPromptUI.CrearBarraPrompts(panelOpciones.transform,
                    (AccionLogica.Navegar, "Navegar"),
                    (AccionLogica.Confirmar, "Confirmar / Cambiar"),
                    (AccionLogica.Cancelar, "Volver"));
            }

            Selectable first = EncontrarPrimerSelectableEnPanel(objetivo);
            if (first != null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(first.gameObject);
            }
        }
    }

    private Selectable EncontrarPrimerSelectableEnPanel(GameObject panel)
    {
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

    private void CambiarTipoDaltonismo(int delta)
    {
        if (accesibilidad == null)
        {
            return;
        }

        int cantidad = System.Enum.GetValues(typeof(TipoDaltonismo)).Length;
        int indiceActual = (int)AccesibilidadSpaceShooter.TipoDaltonismoActual;
        int nuevoIndice = (indiceActual + delta + cantidad) % cantidad;
        accesibilidad.EstablecerTipoDaltonismo((TipoDaltonismo)nuevoIndice);
        ActualizarTextoTipoDaltonismo();
    }

    private void ActualizarTextoTipoDaltonismo()
    {
        if (textoTipoDaltonismo != null)
        {
            textoTipoDaltonismo.text =
                "Modos de daltonismo: " + AccesibilidadSpaceShooter.TipoDaltonismoActual;
        }
    }

}
