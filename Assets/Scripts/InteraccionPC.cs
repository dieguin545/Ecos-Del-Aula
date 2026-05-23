using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    [Header("Fondo de escritorio")]
    [SerializeField] private Image imagenFondoEscritorio;
    [SerializeField] private Sprite fondoEscritorioPC;

    [Header("Iconos futuros")]
    [SerializeField] private Sprite iconoCorreo;
    [SerializeField] private Sprite iconoTienda;
    [SerializeField] private Sprite iconoFinanzas;
    [SerializeField] private Sprite iconoBlocNotas;
    [SerializeField] private Sprite iconoAyuda;
    [SerializeField] private Sprite iconoLecturaFacil;
    [SerializeField] private Sprite iconoSpaceShooter;
    [SerializeField] private Sprite iconoCasos;

    private bool jugadorDentro;
    private bool usandoPC;
    private Behaviour scriptCamara;
    private GestorVentanasPC gestorVentanas;

    private void Start()
    {
        Time.timeScale = 1f;
        ResetearEstadoGlobalPC();

        if (canvasPC != null)
        {
            CargarSpritesSiHaceFalta();
            AsegurarAppCasos();
            PrepararGestorVentanas();
            AplicarFondoEscritorio();
            AplicarIconosEscritorio();
            if (canvasPC.GetComponent<EcosAulaNavegacionEscritorioPC>() == null)
            {
                canvasPC.AddComponent<EcosAulaNavegacionEscritorioPC>();
            }
            canvasPC.SetActive(false);
        }

        if (textoInteractuar != null)
        {
            ActualizarTextoInteractuar();
            textoInteractuar.SetActive(false);
        }

        if (camaraPrincipal != null)
        {
            scriptCamara = camaraPrincipal.GetComponent("ControlCamara3D") as Behaviour;
        }

        usandoPC = false;
        PCAbierta = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (MenuPausaAccesibilidad.EstaPausado)
        {
            return;
        }

        bool hayCampoTextoActivo = usandoPC && HayCampoTextoActivo();

        ActualizarTextoInteractuar();

        if (jugadorDentro && GestorEntradaGlobal.InteractuarPresionado(teclaInteractuar))
        {
            if (!usandoPC)
            {
                LimpiarFocoUI();
                EntrarPC();
                return;
            }

            if (!hayCampoTextoActivo && (gestorVentanas == null || !gestorVentanas.HayVentanaAbierta))
            {
                SalirPC();
                return;
            }
        }

        if (usandoPC && GestorEntradaGlobal.CancelarPresionado() && !hayCampoTextoActivo)
        {
            if (gestorVentanas != null && gestorVentanas.CerrarVentanaActiva())
            {
                return;
            }

            SalirPC();
        }
    }

    private void PrepararGestorVentanas()
    {
        if (canvasPC == null)
        {
            return;
        }

        gestorVentanas = canvasPC.GetComponent<GestorVentanasPC>();

        if (gestorVentanas == null)
        {
            gestorVentanas = canvasPC.AddComponent<GestorVentanasPC>();
        }

        List<GameObject> ventanas = new List<GameObject>();

        AgregarVentanaSiExiste(ventanas, "VentanaCorreo");
        AgregarVentanaSiExiste(ventanas, "Tienda");
        AgregarVentanaSiExiste(ventanas, "VentanaTienda");
        AgregarVentanaSiExiste(ventanas, "PanelTienda");
        AgregarVentanaSiExiste(ventanas, "VentanaFinanzas");
        AgregarVentanaSiExiste(ventanas, "PanelFinanzas");
        AgregarVentanaSiExiste(ventanas, "PanelBlocNotas");
        AgregarVentanaSiExiste(ventanas, "BlocNotas");
        AgregarVentanaSiExiste(ventanas, "ayuda");
        AgregarVentanaSiExiste(ventanas, "PanelAyuda");
        AgregarVentanaSiExiste(ventanas, "VentanaCasos");
        AgregarVentanaSiExiste(ventanas, "PanelCasos");

        gestorVentanas.ConfigurarVentanas(ventanas);
    }

    private void AgregarVentanaSiExiste(List<GameObject> ventanas, string nombre)
    {
        GameObject ventana = BuscarDescendiente(nombre);

        if (ventana == null)
        {
            return;
        }

        if (!ventanas.Contains(ventana))
        {
            ventanas.Add(ventana);
        }
    }

    private GameObject BuscarDescendiente(string nombre)
    {
        if (canvasPC == null)
        {
            return null;
        }

        Transform[] descendientes = canvasPC.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < descendientes.Length; i++)
        {
            if (descendientes[i] != null && descendientes[i].name == nombre)
            {
                return descendientes[i].gameObject;
            }
        }

        return null;
    }

    private void CargarSpritesSiHaceFalta()
    {
        if (iconoAyuda == null)
        {
            iconoAyuda = RecursosVisualesEntryFilter.CargarSpriteEditor("boton_ayuda.png");
        }

        if (iconoFinanzas == null)
        {
            iconoFinanzas = RecursosVisualesEntryFilter.CargarSpriteEditor("logo_finanzas.png");
        }

        if (iconoCasos == null)
        {
            iconoCasos = RecursosVisualesEntryFilter.CargarSpriteEditor("Casos_Icon.png");
        }
    }

    private void AplicarFondoEscritorio()
    {
        if (imagenFondoEscritorio == null)
        {
            GameObject fondo = BuscarDescendiente("FondoEscritorioPC");

            if (fondo == null)
            {
                fondo = BuscarDescendiente("FondoPC");
            }

            if (fondo != null)
            {
                imagenFondoEscritorio = fondo.GetComponent<Image>();
            }
        }

        if (imagenFondoEscritorio == null)
        {
            return;
        }

        imagenFondoEscritorio.sprite = fondoEscritorioPC;
        imagenFondoEscritorio.color = fondoEscritorioPC != null
            ? Color.white
            : new Color(0.08f, 0.03f, 0.15f, 1f);

        imagenFondoEscritorio.raycastTarget = false;
    }

    private void AplicarIconosEscritorio()
    {
        AplicarIconoEscritorio("ayuda", iconoAyuda);
        AplicarIconoEscritorio("IconoAyuda", iconoAyuda);
        AplicarIconoEscritorio("Finanzas", iconoFinanzas);
        AplicarIconoEscritorio("IconoFinanzas", iconoFinanzas);
        AplicarIconoEscritorio("Tienda Virtual", iconoTienda);
        AplicarIconoEscritorio("IconoTienda", iconoTienda);
        AplicarIconoEscritorio("Correo", iconoCorreo);
        AplicarIconoEscritorio("IconoCorreo", iconoCorreo);
        AplicarIconoEscritorio("Bloc de notas", iconoBlocNotas);
        AplicarIconoEscritorio("IconoBlocNotas", iconoBlocNotas);
        AplicarIconoEscritorio("Space Shooter", iconoSpaceShooter);
        AplicarIconoEscritorio("IconoSpaceShooter", iconoSpaceShooter);
        AplicarIconoEscritorio("Casos", iconoCasos);
        AplicarIconoEscritorio("IconoCasos", iconoCasos);
        RenombrarSpaceShooterEnEscritorio();
    }

    private void RenombrarSpaceShooterEnEscritorio()
    {
        if (canvasPC == null)
        {
            return;
        }

        TextMeshProUGUI[] textosTmp = canvasPC.GetComponentsInChildren<TextMeshProUGUI>(true);

        for (int i = 0; i < textosTmp.Length; i++)
        {
            if (textosTmp[i] != null && textosTmp[i].text.Trim() == "Space Shooter")
            {
                textosTmp[i].text = "Firewall del Aula";
            }
        }

        Text[] textosLegacy = canvasPC.GetComponentsInChildren<Text>(true);

        for (int i = 0; i < textosLegacy.Length; i++)
        {
            if (textosLegacy[i] != null && textosLegacy[i].text.Trim() == "Space Shooter")
            {
                textosLegacy[i].text = "Firewall del Aula";
            }
        }
    }

    private void AplicarIconoEscritorio(string nombreObjeto, Sprite sprite)
    {
        GameObject objeto = BuscarDescendiente(nombreObjeto);

        if (objeto == null)
        {
            return;
        }

        Image imagen = objeto.GetComponent<Image>();

        if (imagen == null)
        {
            imagen = objeto.GetComponentInChildren<Image>(true);
        }

        if (imagen == null)
        {
            return;
        }

        if (sprite != null)
        {
            imagen.sprite = sprite;
            imagen.color = Color.white;
            imagen.preserveAspect = true;
            DesactivarPlaceholdersIcono(objeto.transform);
        }
        else if (imagen.sprite != null)
        {
            imagen.color = Color.white;
            imagen.preserveAspect = true;
        }
        else
        {
            imagen.color = new Color(0.08f, 0.28f, 0.42f, 1f);
        }
    }

    private void DesactivarPlaceholdersIcono(Transform raiz)
    {
        if (raiz == null)
        {
            return;
        }

        TextMeshProUGUI[] textos = raiz.GetComponentsInChildren<TextMeshProUGUI>(true);

        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] == null)
            {
                continue;
            }

            string contenido = textos[i].text.Trim();

            if (
                contenido == "?"
                || contenido == "F"
                || contenido == "T"
                || contenido == "C"
                || contenido == "B"
                || contenido == "S"
            )
            {
                textos[i].gameObject.SetActive(false);
            }
        }

        Text[] textosLegacy = raiz.GetComponentsInChildren<Text>(true);

        for (int i = 0; i < textosLegacy.Length; i++)
        {
            if (textosLegacy[i] == null)
            {
                continue;
            }

            string contenido = textosLegacy[i].text.Trim();

            if (
                contenido == "?"
                || contenido == "F"
                || contenido == "T"
                || contenido == "C"
                || contenido == "B"
                || contenido == "S"
            )
            {
                textosLegacy[i].gameObject.SetActive(false);
            }
        }
    }

    private bool HayCampoTextoActivo()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        GameObject seleccionado = EventSystem.current.currentSelectedGameObject;

        if (seleccionado == null || !seleccionado.activeInHierarchy)
        {
            return false;
        }

        if (seleccionado.GetComponentInParent<TMP_InputField>() != null)
        {
            return true;
        }

        if (seleccionado.GetComponentInParent<InputField>() != null)
        {
            return true;
        }

        return false;
    }

    private void LimpiarFocoUI()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void AsegurarAppCasos()
    {
        if (canvasPC == null)
        {
            return;
        }

        GestorCasos gestorCasos = canvasPC.GetComponent<GestorCasos>();

        if (gestorCasos == null)
        {
            gestorCasos = canvasPC.AddComponent<GestorCasos>();
        }

        gestorCasos.InicializarSiHaceFalta();

        GameObject ventanaCasos = BuscarDescendiente("VentanaCasos");

        if (ventanaCasos == null)
        {
            ventanaCasos = CrearObjetoUI("VentanaCasos", canvasPC.transform);
            ventanaCasos.AddComponent<Image>();
            AppCasos appCasosNueva = ventanaCasos.AddComponent<AppCasos>();
            appCasosNueva.Inicializar(gestorCasos, iconoCasos);
            ventanaCasos.SetActive(false);
        }
        else
        {
            AppCasos appCasos = ventanaCasos.GetComponent<AppCasos>();

            if (appCasos == null)
            {
                appCasos = ventanaCasos.AddComponent<AppCasos>();
            }

            appCasos.Inicializar(gestorCasos, iconoCasos);
        }

        CrearOActualizarIconoCasos(ventanaCasos);
    }

    private void CrearOActualizarIconoCasos(GameObject ventanaCasos)
    {
        GameObject icono = BuscarDescendiente("IconoCasos");

        if (icono == null)
        {
            icono = CrearObjetoUI("IconoCasos", canvasPC.transform);
        }

        Image imagen = icono.GetComponent<Image>();

        if (imagen == null)
        {
            imagen = icono.AddComponent<Image>();
        }

        Button boton = icono.GetComponent<Button>();

        if (boton == null)
        {
            boton = icono.AddComponent<Button>();
        }

        RectTransform rect = icono.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-345f, -40f);
        rect.sizeDelta = new Vector2(44f, 44f);

        icono.SetActive(true);
        icono.transform.SetAsLastSibling();

        imagen.sprite = iconoCasos;
        imagen.color = iconoCasos != null ? Color.white : EstiloUIJuego.FondoTarjeta;
        imagen.preserveAspect = iconoCasos != null;
        imagen.raycastTarget = true;

        TextMeshProUGUI etiqueta = icono.transform.Find("EtiquetaCasos") != null
            ? icono.transform.Find("EtiquetaCasos").GetComponent<TextMeshProUGUI>()
            : null;

        if (etiqueta == null)
        {
            etiqueta = EstiloUIJuego.CrearTextoTMP(
                icono.transform,
                "EtiquetaCasos",
                "Casos",
                13f,
                new Vector2(0f, -36f),
                new Vector2(92f, 24f),
                TextAlignmentOptions.Center
            );
        }

        etiqueta.text = "Casos";
        etiqueta.color = EstiloUIJuego.TextoPrincipal;
        etiqueta.raycastTarget = false;

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(
            () =>
            {
                if (gestorVentanas != null)
                {
                    gestorVentanas.AbrirVentana(ventanaCasos);
                }
                else if (ventanaCasos != null)
                {
                    ventanaCasos.SetActive(true);
                    ventanaCasos.transform.SetAsLastSibling();
                }
            }
        );
    }

    private GameObject CrearObjetoUI(string nombre, Transform padre)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer));
        objeto.transform.SetParent(padre, false);
        return objeto;
    }

    private void ActualizarTextoInteractuar()
    {
        if (textoInteractuar == null)
        {
            return;
        }

        if (textoInteractuar.GetComponent<EcosAulaPromptUI>() == null)
        {
            EcosAulaPromptUI.InyectarEn(textoInteractuar, AccionLogica.Interactuar, "para usar PC");
        }
    }

    private void EntrarPC()
    {
        usandoPC = true;
        PCAbierta = true;

        LimpiarFocoUI();

        if (canvasPC != null)
        {
            canvasPC.SetActive(true);
            CargarSpritesSiHaceFalta();
            AsegurarAppCasos();
            PrepararGestorVentanas();
            AplicarIconosEscritorio();
        }

        if (gestorVentanas != null)
        {
            gestorVentanas.CerrarTodas();
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

    private void SalirPC()
    {
        usandoPC = false;
        PCAbierta = false;
        FrameCierrePC = Time.frameCount;

        if (canvasPC != null)
        {
            if (gestorVentanas != null)
            {
                gestorVentanas.CerrarTodas();
            }

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

        LimpiarFocoUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (jugadorDentro && textoInteractuar != null)
        {
            textoInteractuar.SetActive(true);
        }
    }

    public void SalirPCDesdeUI()
    {
        if (usandoPC)
        {
            SalirPC();
        }
    }

    public static void ResetearEstadoGlobalPC()
    {
        PCAbierta = false;
        FrameCierrePC = Time.frameCount - 10;
    }

    private void OnTriggerEnter(Collider other)
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

    private void OnTriggerExit(Collider other)
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
