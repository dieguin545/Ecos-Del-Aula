using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfiguradorEscenaJuego : MonoBehaviour
{
    private const string NombreEscenaJuego = "Juego";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegistrarCargaEscenas()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        ConfigurarSiEsEscenaJuego(escena);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        ConfigurarSiEsEscenaJuego(SceneManager.GetActiveScene());
    }

    private static void ConfigurarSiEsEscenaJuego(Scene escena)
    {
        if (!escena.IsValid() || escena.name != NombreEscenaJuego)
        {
            return;
        }

        ConfiguradorEscenaJuego existente = FindAnyObjectByType<ConfiguradorEscenaJuego>();
        if (existente != null)
        {
            existente.ConfigurarEscenaActual();
            return;
        }

        GameObject configurador = new GameObject("ConfiguradorEscenaJuego");
        SceneManager.MoveGameObjectToScene(configurador, escena);
        configurador.AddComponent<ConfiguradorEscenaJuego>().ConfigurarEscenaActual();
    }

    private void Start()
    {
        ConfigurarEscenaActual();
    }

    public void ConfigurarEscenaActual()
    {
        Time.timeScale = 1f;
        AsegurarEntradaGlobal();
        AsegurarCamaraPrincipalActiva();
        EliminarUISeleccionJuegoPersistente();
        AsegurarPCInicialmenteCerrada();
        AsegurarEventSystemUnico();
        AsegurarColisionCama();
        AsegurarMenuPausaGlobal();
        AsegurarSistemaCasosPC();
        AsegurarDecoracionHabitacion();
    }

    private void AsegurarEntradaGlobal()
    {
        if (FindAnyObjectByType<GestorEntradaGlobal>() != null)
        {
            return;
        }

        GameObject entrada = new GameObject("GestorEntradaGlobal");
        DontDestroyOnLoad(entrada);
        entrada.AddComponent<GestorEntradaGlobal>();
    }

    private void AsegurarCamaraPrincipalActiva()
    {
        Camera camara = BuscarCamaraDeEscenaActiva();

        if (camara == null)
        {
            GameObject objetoCamara = new GameObject("Main Camera");
            camara = objetoCamara.AddComponent<Camera>();
            objetoCamara.transform.SetPositionAndRotation(new Vector3(0f, 2.2f, -6f), Quaternion.Euler(15f, 0f, 0f));
        }

        DesactivarCamarasPersistentesQueTapan(camara);

        camara.gameObject.SetActive(true);
        camara.enabled = true;
        camara.targetTexture = null;
        camara.rect = new Rect(0f, 0f, 1f, 1f);
        camara.depth = 0f;
        camara.clearFlags = CameraClearFlags.Skybox;
        camara.fieldOfView = 48f;
        camara.nearClipPlane = Mathf.Min(camara.nearClipPlane, 0.05f);
        camara.farClipPlane = Mathf.Max(camara.farClipPlane, 100f);

        if (camara.cullingMask == 0)
        {
            camara.cullingMask = ~0;
        }

        if (!camara.CompareTag("MainCamera"))
        {
            camara.tag = "MainCamera";
        }

        AudioListener listener = camara.GetComponent<AudioListener>();
        if (listener == null)
        {
            listener = camara.gameObject.AddComponent<AudioListener>();
        }

        listener.enabled = true;

        GameObject jugador = GameObject.Find("Jugador");
        if (jugador == null)
        {
            jugador = GameObject.Find("Player");
        }

        ControlCamara3D controlCamara = camara.GetComponent<ControlCamara3D>();
        if (controlCamara == null)
        {
            controlCamara = camara.gameObject.AddComponent<ControlCamara3D>();
        }

        if (controlCamara != null && controlCamara.jugador == null)
        {
            if (jugador != null)
            {
                controlCamara.jugador = jugador.transform;
            }
        }

        if (controlCamara != null)
        {
            controlCamara.RecentrarCamaraInicial();
        }

        if (jugador != null)
        {
            MovimientoJugadorConCamara movimiento = jugador.GetComponent<MovimientoJugadorConCamara>();
            if (movimiento != null)
            {
                movimiento.camara = camara.transform;
            }
        }

        InteraccionPC interaccionPc = FindAnyObjectByType<InteraccionPC>(FindObjectsInactive.Include);
        if (interaccionPc != null)
        {
            interaccionPc.camaraPrincipal = camara.gameObject;
            if (interaccionPc.scriptMovimientoJugador == null && jugador != null)
            {
                interaccionPc.scriptMovimientoJugador = jugador.GetComponent<MovimientoJugadorConCamara>();
            }
        }
    }

    private Camera BuscarCamaraDeEscenaActiva()
    {
        Scene escenaActiva = SceneManager.GetActiveScene();
        Camera[] camaras = Resources.FindObjectsOfTypeAll<Camera>();

        Camera primeraDeEscena = null;
        for (int i = 0; i < camaras.Length; i++)
        {
            Camera camara = camaras[i];
            if (camara == null || camara.gameObject == null || camara.gameObject.scene != escenaActiva)
            {
                continue;
            }

            if (primeraDeEscena == null)
            {
                primeraDeEscena = camara;
            }

            if (camara.CompareTag("MainCamera") || camara.gameObject.name == "Main Camera")
            {
                return camara;
            }
        }

        return primeraDeEscena;
    }

    private void DesactivarCamarasPersistentesQueTapan(Camera camaraPrincipal)
    {
        if (camaraPrincipal == null)
        {
            return;
        }

        Scene escenaActiva = SceneManager.GetActiveScene();
        Camera[] camaras = Resources.FindObjectsOfTypeAll<Camera>();

        for (int i = 0; i < camaras.Length; i++)
        {
            Camera camara = camaras[i];
            if (camara == null || camara == camaraPrincipal || camara.gameObject == null)
            {
                continue;
            }

            bool renderizaPantallaPrincipal = camara.targetTexture == null && camara.targetDisplay == camaraPrincipal.targetDisplay;
            bool estaFueraDeEscena = camara.gameObject.scene != escenaActiva;

            if (renderizaPantallaPrincipal && estaFueraDeEscena)
            {
                camara.enabled = false;
                AudioListener listener = camara.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }
    }

    private void AsegurarPCInicialmenteCerrada()
    {
        InteraccionPC.ResetearEstadoGlobalPC();

        InteraccionPC interaccionPc = FindAnyObjectByType<InteraccionPC>(FindObjectsInactive.Include);
        if (interaccionPc == null)
        {
            return;
        }

        if (interaccionPc.canvasPC != null)
        {
            interaccionPc.canvasPC.SetActive(false);
        }

        if (interaccionPc.textoInteractuar != null)
        {
            interaccionPc.textoInteractuar.SetActive(false);
        }
    }

    private void EliminarUISeleccionJuegoPersistente()
    {
        Scene escenaActiva = SceneManager.GetActiveScene();
        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();

        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || canvas.gameObject == null || !canvas.gameObject.scene.IsValid())
            {
                continue;
            }

            if (canvas.gameObject.scene == escenaActiva)
            {
                continue;
            }

            if (!ContieneUISeleccionJuego(canvas.transform))
            {
                continue;
            }

            Destroy(canvas.gameObject);
        }
    }

    private bool ContieneUISeleccionJuego(Transform raiz)
    {
        if (raiz == null)
        {
            return false;
        }

        int coincidencias = 0;

        if (raiz.Find("_Sub_seljuego") != null)
        {
            coincidencias += 2;
        }

        TextMeshProUGUI[] textos = raiz.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] == null)
            {
                continue;
            }

            string contenido = textos[i].text;
            if (string.IsNullOrEmpty(contenido))
            {
                continue;
            }

            contenido = contenido.ToLowerInvariant();
            if (contenido.Contains("selecciona el juego"))
            {
                coincidencias += 2;
            }
            else if (contenido.Contains("entry filter") || contenido.Contains("vida escolar"))
            {
                coincidencias++;
            }
        }

        return coincidencias >= 2;
    }

    private bool EsComponenteDeEscena(Component componente, bool preferirEscenaActiva)
    {
        if (componente == null || componente.gameObject == null || !componente.gameObject.scene.IsValid())
        {
            return false;
        }

        Scene escena = componente.gameObject.scene;
        if (preferirEscenaActiva)
        {
            return escena == SceneManager.GetActiveScene();
        }

        return escena.isLoaded;
    }

    private void AsegurarEventSystemUnico()
    {
        EventSystem[] sistemas = FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
        EventSystem conservar = EventSystem.current;

        if (conservar == null)
        {
            for (int i = 0; i < sistemas.Length; i++)
            {
                if (sistemas[i] != null && sistemas[i].gameObject.activeInHierarchy)
                {
                    conservar = sistemas[i];
                    break;
                }
            }
        }

        if (conservar == null && sistemas.Length > 0)
        {
            conservar = sistemas[0];
        }

        for (int i = 0; i < sistemas.Length; i++)
        {
            if (sistemas[i] != null && sistemas[i] != conservar)
            {
                Destroy(sistemas[i].gameObject);
            }
        }
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
        MenuPausaAccesibilidad[] menus = FindObjectsByType<MenuPausaAccesibilidad>(FindObjectsInactive.Include);

        if (menus.Length > 0)
        {
            MenuPausaAccesibilidad menuExistente = menus[0];

            for (int i = 1; i < menus.Length; i++)
            {
                if (menus[i] != null && menus[i] != menuExistente)
                {
                    Destroy(menus[i].gameObject);
                }
            }

            RepararPanelesMenuExistente(menuExistente);
            ActualizarReferenciasMenuPausa(menuExistente);
            menuExistente.ReinicializarTrasCargaEscena();
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
        ActualizarReferenciasMenuPausa(menu);

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
        menu.ReinicializarTrasCargaEscena();
    }

    private void RepararPanelesMenuExistente(MenuPausaAccesibilidad menu)
    {
        if (menu == null)
        {
            return;
        }

        if (menu.panelPausa == null)
        {
            menu.panelPausa = BuscarDescendiente(menu.transform, "PanelPausa");
        }

        if (menu.panelOpciones == null)
        {
            menu.panelOpciones = BuscarDescendiente(menu.transform, "PanelOpciones");
        }

        if (menu.panelDetalleSlot == null)
        {
            menu.panelDetalleSlot = BuscarDescendiente(menu.transform, "PanelDetalleSlot");
        }

        if (menu.panelPausa == null)
        {
            menu.panelPausa = CrearPanel("PanelPausa", menu.transform, new Vector2(520f, 430f));
            PrepararPanelPausa(menu, menu.panelPausa.transform);
        }

        if (menu.panelOpciones == null)
        {
            menu.panelOpciones = CrearPanel("PanelOpciones", menu.transform, new Vector2(720f, 520f));
            PrepararPanelOpciones(menu, menu.panelOpciones.transform);
        }

        if (menu.panelDetalleSlot == null)
        {
            menu.panelDetalleSlot = CrearPanel("PanelDetalleSlot", menu.transform, new Vector2(680f, 440f));
            PrepararPanelDetalleSlot(menu, menu.panelDetalleSlot.transform);
        }
    }

    private void ActualizarReferenciasMenuPausa(MenuPausaAccesibilidad menu)
    {
        if (menu == null)
        {
            return;
        }

        InteraccionPC interaccionPc = FindAnyObjectByType<InteraccionPC>(FindObjectsInactive.Include);

        if (interaccionPc != null)
        {
            menu.scriptMovimientoJugador = interaccionPc.scriptMovimientoJugador;
            menu.camaraPrincipal = interaccionPc.camaraPrincipal;

            if (interaccionPc.canvasPC != null)
            {
                menu.pantallasQueBloqueanPausa = new[] { interaccionPc.canvasPC };
            }
        }

        menu.textosTMP = menu.GetComponentsInChildren<TextMeshProUGUI>(true);
        menu.textosNormales = menu.GetComponentsInChildren<Text>(true);
        menu.imagenesUI = menu.GetComponentsInChildren<Image>(true);
    }

    private void AsegurarSistemaCasosPC()
    {
        InteraccionPC interaccionPc = FindAnyObjectByType<InteraccionPC>(FindObjectsInactive.Include);

        if (interaccionPc == null || interaccionPc.canvasPC == null)
        {
            return;
        }

        GameObject canvasPc = interaccionPc.canvasPC;
        GestorCasos gestorCasos = canvasPc.GetComponent<GestorCasos>();

        if (gestorCasos == null)
        {
            gestorCasos = canvasPc.AddComponent<GestorCasos>();
        }

        gestorCasos.InicializarSiHaceFalta();

        Sprite iconoCasos = RecursosVisualesEntryFilter.CargarSpriteEditor("Casos_Icon.png");
        GameObject ventanaCasos = BuscarDescendiente(canvasPc.transform, "VentanaCasos");

        if (ventanaCasos == null)
        {
            ventanaCasos = CrearObjetoUI("VentanaCasos", canvasPc.transform);
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

        GestorVentanasPC gestorVentanas = canvasPc.GetComponent<GestorVentanasPC>();

        if (gestorVentanas != null)
        {
            gestorVentanas.RegistrarVentana(ventanaCasos);
        }

        CrearOActualizarIconoCasos(canvasPc.transform, ventanaCasos, iconoCasos);
    }

    private void CrearOActualizarIconoCasos(Transform canvasPc, GameObject ventanaCasos, Sprite iconoCasos)
    {
        Transform iconoExistente = BuscarDescendiente(canvasPc, "IconoCasos")?.transform;
        GameObject icono = iconoExistente != null
            ? iconoExistente.gameObject
            : CrearObjetoUI("IconoCasos", canvasPc);

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

        imagen.sprite = iconoCasos;
        imagen.color = iconoCasos != null ? Color.white : EstiloUIJuego.FondoTarjeta;
        imagen.preserveAspect = iconoCasos != null;
        imagen.raycastTarget = true;

        Transform etiquetaExistente = icono.transform.Find("EtiquetaCasos");

        if (etiquetaExistente == null)
        {
            TextMeshProUGUI etiqueta = EstiloUIJuego.CrearTextoTMP(
                icono.transform,
                "EtiquetaCasos",
                "Casos",
                13f,
                new Vector2(0f, -36f),
                new Vector2(92f, 24f),
                TextAlignmentOptions.Center
            );
            etiqueta.color = EstiloUIJuego.TextoPrincipal;
        }

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(
            () =>
            {
                GestorVentanasPC gestor = canvasPc.GetComponent<GestorVentanasPC>();

                if (gestor != null)
                {
                    gestor.AbrirVentana(ventanaCasos);
                }
                else if (ventanaCasos != null)
                {
                    ventanaCasos.SetActive(true);
                    ventanaCasos.transform.SetAsLastSibling();
                }
            }
        );
    }

    private GameObject BuscarDescendiente(Transform raiz, string nombre)
    {
        if (raiz == null)
        {
            return null;
        }

        Transform[] descendientes = raiz.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < descendientes.Length; i++)
        {
            if (descendientes[i] != null && descendientes[i].name == nombre)
            {
                return descendientes[i].gameObject;
            }
        }

        return null;
    }

    private void AsegurarDecoracionHabitacion()
    {
        GameObject piso = GameObject.Find("Piso");
        Renderer pisoRenderer = piso != null ? piso.GetComponentInChildren<Renderer>() : null;

        if (pisoRenderer == null)
        {
            return;
        }

        Bounds bounds = pisoRenderer.bounds;
        GameObject raiz = GameObject.Find("Decoracion_Habitacion");

        if (raiz == null)
        {
            raiz = new GameObject("Decoracion_Habitacion");
        }

        LimpiarDecoracionGeneradaGlobal();
        LimpiarDecoracionGenerada(raiz.transform);
        CrearTecho(bounds, raiz.transform);
        CrearTapete(bounds, raiz.transform);
        CrearCuadro(bounds, raiz.transform);
    }

    private void LimpiarDecoracionGeneradaGlobal()
    {
        string[] nombresGenerados =
        {
            "Techo_Habitacion",
            "Poster_EcosDelAula",
            "Panel_Recordatorio_Ayuda",
            "Alfombra_Estudio",
            "Cuadro_Pared_Decorativo",
            "Tapete_Decorativo",
            "Decoracion_books",
            "Decoracion_ceilingFan"
        };

        Transform[] objetos = FindObjectsByType<Transform>(FindObjectsInactive.Include);

        for (int i = objetos.Length - 1; i >= 0; i--)
        {
            Transform transformObjetivo = objetos[i];

            if (transformObjetivo == null || transformObjetivo.name == "Decoracion_Habitacion")
            {
                continue;
            }

            for (int j = 0; j < nombresGenerados.Length; j++)
            {
                if (transformObjetivo.name.StartsWith(nombresGenerados[j]))
                {
                    DestruirSeguro(transformObjetivo.gameObject);
                    break;
                }
            }
        }
    }

    private void LimpiarDecoracionGenerada(Transform padre)
    {
        if (padre == null)
        {
            return;
        }

        string[] nombresGenerados =
        {
            "Techo_Habitacion",
            "Poster_EcosDelAula",
            "Panel_Recordatorio_Ayuda",
            "Alfombra_Estudio",
            "Cuadro_Pared_Decorativo",
            "Tapete_Decorativo",
            "Decoracion_books",
            "Decoracion_ceilingFan"
        };

        for (int i = padre.childCount - 1; i >= 0; i--)
        {
            GameObject hijo = padre.GetChild(i).gameObject;

            for (int j = 0; j < nombresGenerados.Length; j++)
            {
                if (hijo.name.StartsWith(nombresGenerados[j]))
                {
                    DestruirSeguro(hijo);
                    break;
                }
            }
        }
    }

    private void DestruirSeguro(GameObject objeto)
    {
        if (objeto == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(objeto);
        }
        else
        {
            DestroyImmediate(objeto);
        }
    }

    private void CrearTecho(Bounds bounds, Transform padre)
    {
        GameObject techo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        techo.name = "Techo_Habitacion";
        techo.transform.SetParent(padre, false);
        techo.transform.position = new Vector3(bounds.center.x, bounds.center.y + 2.75f, bounds.center.z);
        techo.transform.localScale = new Vector3(bounds.size.x + 0.9f, 0.18f, bounds.size.z + 0.9f);
        Renderer renderer = techo.GetComponent<Renderer>();

        if (renderer != null)
        {
            AplicarTextura(renderer, "Assets/Texturas/Apartamento/pared_apartamento.png", new Color(0.70f, 0.59f, 0.44f, 1f));
        }

        Collider collider = techo.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private void CrearTapete(Bounds bounds, Transform padre)
    {
        GameObject tapete = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tapete.name = "Tapete_Decorativo";
        tapete.transform.SetParent(padre, false);
        tapete.transform.position = new Vector3(bounds.center.x + 0.55f, bounds.center.y + 0.035f, bounds.center.z - 0.85f);
        tapete.transform.localScale = new Vector3(2.1f, 0.025f, 1.3f);

        Renderer renderer = tapete.GetComponent<Renderer>();

        if (renderer != null)
        {
            AplicarTextura(renderer, "Assets/Texturas/Apartamento/Tapete_Apartamento.png", new Color(0.28f, 0.16f, 0.35f, 1f));
        }

        Collider collider = tapete.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private void CrearCuadro(Bounds bounds, Transform padre)
    {
        GameObject cuadro = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cuadro.name = "Cuadro_Pared_Decorativo";
        cuadro.transform.SetParent(padre, false);
        cuadro.transform.position = new Vector3(bounds.center.x + 0.15f, bounds.center.y + 2.08f, bounds.center.z + bounds.extents.z - 0.08f);
        cuadro.transform.localScale = new Vector3(1.9f, 1.05f, 0.035f);

        Renderer renderer = cuadro.GetComponent<Renderer>();

        if (renderer != null)
        {
            AplicarTextura(renderer, "Assets/Texturas/Apartamento/Cuadro_Pared_Apartamento.png", Color.white);
        }

        Collider collider = cuadro.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private void AplicarTextura(Renderer renderer, string ruta, Color colorFallback)
    {
        if (renderer == null)
        {
            return;
        }

        Texture2D textura = CargarTexturaEditor(ruta);
        Shader shader = Shader.Find("Standard");

        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        Material material = shader != null
            ? new Material(shader)
            : renderer.sharedMaterial != null
                ? new Material(renderer.sharedMaterial)
                : null;

        if (material == null)
        {
            return;
        }

        material.color = colorFallback;

        if (textura != null)
        {
            material.mainTexture = textura;
        }

        renderer.sharedMaterial = material;
    }

    private Material CargarMaterialEditor(string ruta)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(ruta);
#else
        return null;
#endif
    }

    private Texture2D CargarTexturaEditor(string ruta)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
#else
        return null;
#endif
    }

    private void CrearPanelDecorativo(string nombre, Transform padre, Vector3 posicion, Vector3 escala, Color color)
    {
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = nombre;
        panel.transform.SetParent(padre, false);
        panel.transform.position = posicion;
        panel.transform.localScale = escala;

        Renderer renderer = panel.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Collider collider = panel.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }
    }

    private void InstanciarKenneyEditor(string nombreArchivo, Transform padre, Vector3 posicion, Vector3 escala)
    {
#if UNITY_EDITOR
        string ruta =
            "Assets/Modelos/Kenney_Furniture/Models/FBX format/" + nombreArchivo;
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ruta);

        if (prefab == null)
        {
            return;
        }

        GameObject instancia = Instantiate(prefab, posicion, Quaternion.identity, padre);
        instancia.name = "Decoracion_" + nombreArchivo.Replace(".fbx", string.Empty);
        instancia.transform.localScale = escala;
        Collider[] colliders = instancia.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }
#endif
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
