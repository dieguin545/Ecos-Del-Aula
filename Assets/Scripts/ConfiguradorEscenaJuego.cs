using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfiguradorEscenaJuego : MonoBehaviour
{
    private const string NombreEscenaJuego = "Juego";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        if (SceneManager.GetActiveScene().name != NombreEscenaJuego)
        {
            return;
        }

        if (FindAnyObjectByType<ConfiguradorEscenaJuego>() != null)
        {
            return;
        }

        GameObject configurador = new GameObject("ConfiguradorEscenaJuego");
        configurador.AddComponent<ConfiguradorEscenaJuego>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        AsegurarColisionCama();
        AsegurarMenuPausaGlobal();
    }

    private void AsegurarColisionCama()
    {
        GameObject cama = GameObject.Find("Cama");

        if (cama == null || cama.GetComponentInChildren<Collider>() != null)
        {
            return;
        }

        Renderer[] renderers = cama.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        BoxCollider collider = cama.AddComponent<BoxCollider>();
        collider.center = cama.transform.InverseTransformPoint(bounds.center);
        collider.size = new Vector3(
            Mathf.Max(0.1f, bounds.size.x),
            Mathf.Max(0.1f, bounds.size.y),
            Mathf.Max(0.1f, bounds.size.z)
        );
    }

    private void AsegurarMenuPausaGlobal()
    {
        if (FindAnyObjectByType<MenuPausaAccesibilidad>() != null)
        {
            return;
        }

        GameObject canvasHud = GameObject.Find("Canvas_HUD");

        if (canvasHud == null)
        {
            return;
        }

        GameObject raiz = CrearObjetoUI("MenuPausaGlobal", canvasHud.transform);
        RectTransform rectRaiz = raiz.GetComponent<RectTransform>();
        rectRaiz.anchorMin = Vector2.zero;
        rectRaiz.anchorMax = Vector2.one;
        rectRaiz.offsetMin = Vector2.zero;
        rectRaiz.offsetMax = Vector2.zero;

        MenuPausaAccesibilidad menu = raiz.AddComponent<MenuPausaAccesibilidad>();
        InteraccionPC interaccionPc = FindAnyObjectByType<InteraccionPC>();

        if (interaccionPc != null)
        {
            menu.scriptMovimientoJugador = interaccionPc.scriptMovimientoJugador;
            menu.camaraPrincipal = interaccionPc.camaraPrincipal;

            if (interaccionPc.canvasPC != null)
            {
                menu.pantallasQueBloqueanPausa = new[] { interaccionPc.canvasPC };
            }
        }

        GameObject panelPausa = CrearPanel("PanelPausa", raiz.transform, new Vector2(520f, 430f));
        GameObject panelOpciones = CrearPanel("PanelOpciones", raiz.transform, new Vector2(720f, 520f));
        GameObject panelDetalleSlot = CrearPanel("PanelDetalleSlot", raiz.transform, new Vector2(680f, 440f));

        menu.panelPausa = panelPausa;
        menu.panelOpciones = panelOpciones;
        menu.panelDetalleSlot = panelDetalleSlot;

        PrepararPanelPausa(menu, panelPausa.transform);
        PrepararPanelOpciones(menu, panelOpciones.transform);
        PrepararPanelDetalleSlot(menu, panelDetalleSlot.transform);

        menu.textosTMP = raiz.GetComponentsInChildren<TextMeshProUGUI>(true);
        menu.imagenesUI = raiz.GetComponentsInChildren<Image>(true);

        panelPausa.SetActive(false);
        panelOpciones.SetActive(false);
        panelDetalleSlot.SetActive(false);
    }

    private void PrepararPanelPausa(MenuPausaAccesibilidad menu, Transform padre)
    {
        CrearTexto(padre, "TituloPausa", "PAUSA", 40f, new Vector2(0f, 158f), new Vector2(360f, 60f));

        menu.botonContinuar = CrearBoton(padre, "BotonContinuar", "Continuar", new Vector2(0f, 78f));
        menu.botonOpciones = CrearBoton(padre, "BotonOpciones", "Opciones", new Vector2(0f, 18f));
        menu.botonReiniciar = CrearBoton(padre, "BotonReiniciar", "Reiniciar", new Vector2(0f, -42f));
        menu.botonSalir = CrearBoton(padre, "BotonSalirMenu", "Salir al menu", new Vector2(0f, -102f));
    }

    private void PrepararPanelOpciones(MenuPausaAccesibilidad menu, Transform padre)
    {
        CrearTexto(padre, "TituloOpciones", "Opciones", 34f, new Vector2(0f, 208f), new Vector2(420f, 54f));

        menu.toggleTextoGrande = CrearToggle(padre, "ToggleTextoGrande", "Texto grande", new Vector2(-210f, 132f));
        menu.toggleAltoContraste = CrearToggle(padre, "ToggleAltoContraste", "Alto contraste", new Vector2(-210f, 84f));

        CrearTexto(
            padre,
            "TituloDaltonismo",
            "Modos de daltonismo",
            21f,
            new Vector2(192f, 138f),
            new Vector2(260f, 36f)
        );

        menu.botonDaltonismoNinguno = CrearBoton(padre, "BotonDaltonismoNinguno", "Ninguno", new Vector2(192f, 94f), new Vector2(250f, 38f));
        menu.botonDaltonismoProtanopia = CrearBoton(padre, "BotonDaltonismoProtanopia", "Protanopia", new Vector2(192f, 48f), new Vector2(250f, 38f));
        menu.botonDaltonismoDeuteranopia = CrearBoton(padre, "BotonDaltonismoDeuteranopia", "Deuteranopia", new Vector2(192f, 2f), new Vector2(250f, 38f));
        menu.botonDaltonismoTritanopia = CrearBoton(padre, "BotonDaltonismoTritanopia", "Tritanopia", new Vector2(192f, -44f), new Vector2(250f, 38f));
        menu.botonDaltonismoAcromatopsia = CrearBoton(padre, "BotonDaltonismoAcromatopsia", "Acromatopsia", new Vector2(192f, -90f), new Vector2(250f, 38f));

        CrearTexto(
            padre,
            "TituloSlots",
            "Partida activa",
            21f,
            new Vector2(-210f, 18f),
            new Vector2(250f, 36f)
        );

        menu.botonSlot1 = CrearBoton(padre, "BotonSlot1", "Slot 1", new Vector2(-288f, -34f), new Vector2(110f, 40f));
        menu.botonSlot2 = CrearBoton(padre, "BotonSlot2", "Slot 2", new Vector2(-166f, -34f), new Vector2(110f, 40f));
        menu.botonSlot3 = CrearBoton(padre, "BotonSlot3", "Slot 3", new Vector2(-44f, -34f), new Vector2(110f, 40f));

        menu.textoSlotActivo = CrearTexto(
            padre,
            "TextoSlotActivo",
            "Slot activo: 1",
            18f,
            new Vector2(-166f, -82f),
            new Vector2(330f, 34f)
        );

        menu.botonVolver = CrearBoton(padre, "BotonVolverPausa", "Volver", new Vector2(0f, -204f), new Vector2(210f, 44f));
    }

    private void PrepararPanelDetalleSlot(MenuPausaAccesibilidad menu, Transform padre)
    {
        CrearTexto(
            padre,
            "TituloDetalleSlot",
            "Partida",
            32f,
            new Vector2(0f, 176f),
            new Vector2(360f, 46f)
        );

        menu.textoDetalleSlot = CrearTexto(
            padre,
            "TextoDetalleSlot",
            "Slot",
            16f,
            new Vector2(0f, 50f),
            new Vector2(610f, 220f)
        );
        menu.textoDetalleSlot.alignment = TextAlignmentOptions.Center;

        menu.botonEntrarSlot = CrearBoton(
            padre,
            "BotonEntrarSlot",
            "Entrar",
            new Vector2(-150f, -152f),
            new Vector2(150f, 42f)
        );
        menu.botonBorrarSlot = CrearBoton(
            padre,
            "BotonBorrarSlot",
            "Borrar",
            new Vector2(0f, -152f),
            new Vector2(150f, 42f)
        );
        menu.botonConfirmarBorrarSlot = CrearBoton(
            padre,
            "BotonConfirmarBorrarSlot",
            "Confirmar",
            new Vector2(0f, -152f),
            new Vector2(150f, 42f)
        );
        menu.botonCancelarSlot = CrearBoton(
            padre,
            "BotonCancelarSlot",
            "Cancelar",
            new Vector2(150f, -152f),
            new Vector2(150f, 42f)
        );

        menu.botonConfirmarBorrarSlot.gameObject.SetActive(false);
    }

    private GameObject CrearPanel(string nombre, Transform padre, Vector2 tamano)
    {
        GameObject panel = CrearObjetoUI(nombre, padre);
        Image fondo = panel.AddComponent<Image>();
        fondo.color = EstiloUIJuego.FondoPrincipal;
        fondo.raycastTarget = true;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = tamano;

        return panel;
    }

    private Button CrearBoton(Transform padre, string nombre, string texto, Vector2 posicion)
    {
        return CrearBoton(padre, nombre, texto, posicion, new Vector2(260f, 46f));
    }

    private Button CrearBoton(Transform padre, string nombre, string texto, Vector2 posicion, Vector2 tamano)
    {
        GameObject objeto = CrearObjetoUI(nombre, padre);
        Image imagen = objeto.AddComponent<Image>();
        Button boton = objeto.AddComponent<Button>();

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;

        imagen.color = new Color(0.06f, 0.19f, 0.32f, 1f);
        EstiloUIJuego.AplicarBoton(boton, imagen.color, EstiloUIJuego.Acento);

        TextMeshProUGUI etiqueta = CrearTexto(
            objeto.transform,
            "Texto",
            texto,
            20f,
            Vector2.zero,
            tamano
        );
        etiqueta.raycastTarget = false;

        return boton;
    }

    private Toggle CrearToggle(Transform padre, string nombre, string texto, Vector2 posicion)
    {
        GameObject objeto = CrearObjetoUI(nombre, padre);
        Toggle toggle = objeto.AddComponent<Toggle>();

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(270f, 36f);

        GameObject fondo = CrearObjetoUI("Fondo", objeto.transform);
        Image imagenFondo = fondo.AddComponent<Image>();
        imagenFondo.color = EstiloUIJuego.FondoTarjeta;

        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = new Vector2(0f, 0.5f);
        rectFondo.anchorMax = new Vector2(0f, 0.5f);
        rectFondo.pivot = new Vector2(0f, 0.5f);
        rectFondo.anchoredPosition = new Vector2(0f, 0f);
        rectFondo.sizeDelta = new Vector2(30f, 30f);

        GameObject check = CrearObjetoUI("Checkmark", fondo.transform);
        Image imagenCheck = check.AddComponent<Image>();
        imagenCheck.color = EstiloUIJuego.Acento;

        RectTransform rectCheck = check.GetComponent<RectTransform>();
        rectCheck.anchorMin = new Vector2(0.5f, 0.5f);
        rectCheck.anchorMax = new Vector2(0.5f, 0.5f);
        rectCheck.pivot = new Vector2(0.5f, 0.5f);
        rectCheck.anchoredPosition = Vector2.zero;
        rectCheck.sizeDelta = new Vector2(18f, 18f);

        TextMeshProUGUI etiqueta = CrearTexto(
            objeto.transform,
            "Texto",
            texto,
            18f,
            new Vector2(32f, 0f),
            new Vector2(230f, 34f)
        );
        etiqueta.alignment = TextAlignmentOptions.Left;
        etiqueta.raycastTarget = false;

        toggle.targetGraphic = imagenFondo;
        toggle.graphic = imagenCheck;
        return toggle;
    }

    private TextMeshProUGUI CrearTexto(
        Transform padre,
        string nombre,
        string texto,
        float tamano,
        Vector2 posicion,
        Vector2 tamanoRect
    )
    {
        TextMeshProUGUI etiqueta = EstiloUIJuego.CrearTextoTMP(
            padre,
            nombre,
            texto,
            tamano,
            posicion,
            tamanoRect,
            TextAlignmentOptions.Center
        );
        etiqueta.color = EstiloUIJuego.TextoPrincipal;
        return etiqueta;
    }

    private GameObject CrearObjetoUI(string nombre, Transform padre)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer));
        objeto.transform.SetParent(padre, false);
        return objeto;
    }
}
