using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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

    private bool jugadorDentro;
    private bool usandoPC;
    private Behaviour scriptCamara;
    private GestorVentanasPC gestorVentanas;

    private void Start()
    {
        Time.timeScale = 1f;

        if (canvasPC != null)
        {
            CargarSpritesSiHaceFalta();
            PrepararGestorVentanas();
            AplicarFondoEscritorio();
            AplicarIconosEscritorio();
            canvasPC.SetActive(false);
        }

        if (textoInteractuar != null)
        {
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

        if (jugadorDentro && Input.GetKeyDown(teclaInteractuar))
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

        if (usandoPC && Input.GetKeyDown(KeyCode.Escape) && !hayCampoTextoActivo)
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

    private void EntrarPC()
    {
        usandoPC = true;
        PCAbierta = true;

        LimpiarFocoUI();

        if (canvasPC != null)
        {
            canvasPC.SetActive(true);
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
