using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppCasos : MonoBehaviour
{
    [SerializeField] private GestorCasos gestorCasos;
    [SerializeField] private Sprite spriteIconoCasos;

    private RectTransform listaCasos;
    private RectTransform contenidoDetalle;
    private ScrollRect scrollDetalle;
    private TextMeshProUGUI textoDetalle;
    private TextMeshProUGUI textoTituloDetalle;
    private readonly List<Button> botonesCasos = new List<Button>();
    private bool uiPreparada;
    private CasoBullying casoSeleccionado;
    private const float VelocidadScrollControl = 0.72f;

    private void Awake()
    {
        CargarSpriteSiHaceFalta();
        PrepararUiSiHaceFalta();
    }

    private void OnEnable()
    {
        if (gestorCasos == null)
        {
            gestorCasos = FindAnyObjectByType<GestorCasos>();
        }

        if (gestorCasos != null)
        {
            gestorCasos.InicializarSiHaceFalta();
        }

        PrepararUiSiHaceFalta();
        ReconstruirLista();
        SeleccionarPrimerCasoVisible();
    }

    private void Update()
    {
        if (!isActiveAndEnabled || scrollDetalle == null || !scrollDetalle.gameObject.activeInHierarchy)
        {
            return;
        }

        float delta = 0f;
        float rueda = Input.mouseScrollDelta.y;

        if (Mathf.Abs(rueda) > 0.01f)
        {
            delta += rueda * 0.08f;
        }

        if (Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.JoystickButton5))
        {
            delta -= VelocidadScrollControl * Time.unscaledDeltaTime;
        }

        if (Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.JoystickButton4))
        {
            delta += VelocidadScrollControl * Time.unscaledDeltaTime;
        }

        if (Mathf.Abs(delta) > 0.001f)
        {
            scrollDetalle.verticalNormalizedPosition = Mathf.Clamp01(scrollDetalle.verticalNormalizedPosition + delta);
        }
    }

    public void Inicializar(GestorCasos gestor, Sprite icono)
    {
        gestorCasos = gestor;

        if (spriteIconoCasos == null)
        {
            spriteIconoCasos = icono;
        }

        PrepararUiSiHaceFalta();
    }

    private void CargarSpriteSiHaceFalta()
    {
        if (spriteIconoCasos == null)
        {
            spriteIconoCasos = RecursosVisualesEntryFilter.CargarSpriteEditor("Casos_Icon.png");
        }
    }

    private void PrepararUiSiHaceFalta()
    {
        if (uiPreparada)
        {
            return;
        }

        Image fondo = GetComponent<Image>();

        if (fondo == null)
        {
            fondo = gameObject.AddComponent<Image>();
        }

        EstiloUIJuego.AplicarPanel(fondo, EstiloUIJuego.FondoPrincipal);

        RectTransform rect = GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 10f);
            rect.sizeDelta = new Vector2(820f, 460f);
        }

        CrearHeader();
        CrearColumnas();
        PrepararBotonCerrar();
        uiPreparada = true;
    }

    private void CrearHeader()
    {
        if (transform.Find("HeaderCasos") == null)
        {
            EstiloUIJuego.CrearImagen(
                transform,
                "HeaderCasos",
                new Vector2(0f, 202f),
                new Vector2(820f, 56f),
                EstiloUIJuego.FondoSecundario
            ).transform.SetAsFirstSibling();
        }

        Transform iconoExistente = transform.Find("IconoCasosHeader");
        Image icono = iconoExistente != null
            ? iconoExistente.GetComponent<Image>()
            : EstiloUIJuego.CrearImagen(
                transform,
                "IconoCasosHeader",
                new Vector2(-365f, 202f),
                new Vector2(42f, 42f),
                Color.white
            );

        icono.sprite = spriteIconoCasos;
        icono.color = spriteIconoCasos != null ? Color.white : EstiloUIJuego.FondoTarjeta;
        icono.preserveAspect = spriteIconoCasos != null;
        icono.raycastTarget = false;

        if (transform.Find("TituloCasos") == null)
        {
            EstiloUIJuego.CrearTextoTMP(
                transform,
                "TituloCasos",
                "Casos",
                30f,
                new Vector2(0f, 202f),
                new Vector2(220f, 42f),
                TextAlignmentOptions.Center
            );
        }

        if (transform.Find("AyudaCasos") == null)
        {
            EstiloUIJuego.CrearTextoTMP(
                transform,
                "AyudaCasos",
                "Revisa contexto antes de decidir correos ambiguos. Cada evidencia ayuda a tomar una decisión justa.",
                14f,
                new Vector2(0f, 160f),
                new Vector2(700f, 34f),
                TextAlignmentOptions.Center
            );
        }
    }

    private void CrearColumnas()
    {
        Transform listaExistente = transform.Find("ListaCasos");

        if (listaExistente == null)
        {
            Image listaImagen = EstiloUIJuego.CrearImagen(
                transform,
                "ListaCasos",
                new Vector2(-270f, -28f),
                new Vector2(250f, 320f),
                EstiloUIJuego.FondoSecundario
            );
            listaCasos = listaImagen.rectTransform;
        }
        else
        {
            listaCasos = listaExistente.GetComponent<RectTransform>();
        }

        Transform detalleExistente = transform.Find("PanelDetalleCaso");

        if (detalleExistente == null)
        {
            EstiloUIJuego.CrearImagen(
                transform,
                "PanelDetalleCaso",
                new Vector2(145f, -28f),
                new Vector2(542f, 320f),
                EstiloUIJuego.FondoTarjeta
            );
        }

        if (textoTituloDetalle == null)
        {
            textoTituloDetalle = EstiloUIJuego.CrearTextoTMP(
                transform,
                "TituloDetalleCaso",
                "Selecciona un caso",
                24f,
                new Vector2(145f, 118f),
                new Vector2(500f, 34f),
                TextAlignmentOptions.Left
            );
        }

        PrepararScrollDetalle();

        if (textoDetalle == null)
        {
            Transform textoExistente = transform.Find("TextoDetalleCaso");

            if (textoExistente != null)
            {
                textoDetalle = textoExistente.GetComponent<TextMeshProUGUI>();

                if (textoDetalle != null && contenidoDetalle != null)
                {
                    textoDetalle.transform.SetParent(contenidoDetalle, false);
                }
            }
        }

        if (textoDetalle == null && contenidoDetalle != null)
        {
            textoDetalle = EstiloUIJuego.CrearTextoTMP(
                contenidoDetalle,
                "TextoDetalleCaso",
                "Los expedientes aparecen cuando usas Revisar contexto en Correo.",
                17f,
                new Vector2(0f, 0f),
                new Vector2(492f, 270f),
                TextAlignmentOptions.TopLeft
            );
        }

        if (textoDetalle != null)
        {
            RectTransform rectTexto = textoDetalle.GetComponent<RectTransform>();
            rectTexto.anchorMin = new Vector2(0f, 1f);
            rectTexto.anchorMax = new Vector2(1f, 1f);
            rectTexto.pivot = new Vector2(0f, 1f);
            rectTexto.anchoredPosition = new Vector2(10f, -8f);
            rectTexto.sizeDelta = new Vector2(-20f, 270f);
            textoDetalle.textWrappingMode = TextWrappingModes.Normal;
            textoDetalle.overflowMode = TextOverflowModes.Overflow;
        }

        CrearBotonAccionCaso("BotonObservarMas", "Observar", new Vector2(-20f, -178f), TipoResolucionCaso.ObservarMas);
        CrearBotonAccionCaso("BotonMediar", "Mediar", new Vector2(105f, -178f), TipoResolucionCaso.MediarConversacion);
        CrearBotonAccionCaso("BotonReportarOrientacion", "Reportar", new Vector2(230f, -178f), TipoResolucionCaso.ReportarOrientacion);
        CrearBotonAccionCaso("BotonProtocoloGrave", "Protocolo", new Vector2(355f, -178f), TipoResolucionCaso.ActivarProtocoloGrave);
    }

    private void PrepararScrollDetalle()
    {
        Transform scrollExistente = transform.Find("ScrollDetalleCaso");
        GameObject scrollObjeto;

        if (scrollExistente == null)
        {
            scrollObjeto = new GameObject(
                "ScrollDetalleCaso",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Mask),
                typeof(ScrollRect)
            );
            scrollObjeto.transform.SetParent(transform, false);
        }
        else
        {
            scrollObjeto = scrollExistente.gameObject;
        }

        RectTransform rectScroll = scrollObjeto.GetComponent<RectTransform>();
        rectScroll.anchorMin = new Vector2(0.5f, 0.5f);
        rectScroll.anchorMax = new Vector2(0.5f, 0.5f);
        rectScroll.pivot = new Vector2(0.5f, 0.5f);
        rectScroll.anchoredPosition = new Vector2(145f, -32f);
        rectScroll.sizeDelta = new Vector2(500f, 238f);

        Image fondoScroll = scrollObjeto.GetComponent<Image>();
        fondoScroll.color = new Color(0.04f, 0.06f, 0.12f, 0.72f);
        fondoScroll.raycastTarget = true;

        Mask mascara = scrollObjeto.GetComponent<Mask>();
        mascara.showMaskGraphic = true;

        scrollDetalle = scrollObjeto.GetComponent<ScrollRect>();
        scrollDetalle.horizontal = false;
        scrollDetalle.vertical = true;
        scrollDetalle.scrollSensitivity = 24f;

        Transform contenidoExistente = scrollObjeto.transform.Find("ContenidoDetalleCaso");
        GameObject contenidoObjeto;

        if (contenidoExistente == null)
        {
            contenidoObjeto = new GameObject(
                "ContenidoDetalleCaso",
                typeof(RectTransform)
            );
            contenidoObjeto.transform.SetParent(scrollObjeto.transform, false);
        }
        else
        {
            contenidoObjeto = contenidoExistente.gameObject;
        }

        contenidoDetalle = contenidoObjeto.GetComponent<RectTransform>();
        contenidoDetalle.anchorMin = new Vector2(0f, 1f);
        contenidoDetalle.anchorMax = new Vector2(1f, 1f);
        contenidoDetalle.pivot = new Vector2(0.5f, 1f);
        contenidoDetalle.anchoredPosition = Vector2.zero;
        contenidoDetalle.sizeDelta = new Vector2(0f, 270f);
        scrollDetalle.content = contenidoDetalle;
    }

    private void CrearBotonAccionCaso(
        string nombre,
        string texto,
        Vector2 posicion,
        TipoResolucionCaso decision
    )
    {
        if (transform.Find(nombre) != null)
        {
            return;
        }

        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        objeto.transform.SetParent(transform, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(110f, 32f);

        Button boton = objeto.GetComponent<Button>();
        boton.onClick.AddListener(() => ResolverCasoSeleccionado(decision));
        EstiloUIJuego.AplicarBoton(
            boton,
            new Color(0.08f, 0.24f, 0.34f, 1f),
            EstiloUIJuego.Acento
        );

        EstiloUIJuego.CrearTextoTMP(
            objeto.transform,
            "Texto",
            texto,
            14f,
            Vector2.zero,
            new Vector2(104f, 28f),
            TextAlignmentOptions.Center
        );
    }

    private void PrepararBotonCerrar()
    {
        Transform existente = transform.Find("CerrarCasos");

        if (existente == null)
        {
            GameObject objeto = new GameObject(
                "CerrarCasos",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            objeto.transform.SetParent(transform, false);
            existente = objeto.transform;

            EstiloUIJuego.CrearTextoTMP(
                existente,
                "Texto",
                "X",
                18f,
                Vector2.zero,
                new Vector2(38f, 38f),
                TextAlignmentOptions.Center
            );
        }

        RectTransform rect = existente.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(38f, 38f);
        }

        Button boton = existente.GetComponent<Button>();

        if (boton != null)
        {
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(CerrarVentanaActual);
            EstiloUIJuego.AplicarBoton(
                boton,
                new Color(0.62f, 0.16f, 0.22f, 1f),
                new Color(0.82f, 0.22f, 0.28f, 1f)
            );
        }
    }

    private void ReconstruirLista()
    {
        if (listaCasos == null)
        {
            return;
        }

        for (int i = listaCasos.childCount - 1; i >= 0; i--)
        {
            Destroy(listaCasos.GetChild(i).gameObject);
        }

        botonesCasos.Clear();

        if (gestorCasos == null)
        {
            return;
        }

        IReadOnlyList<CasoBullying> casos = gestorCasos.Casos;
        int visibles = 0;

        for (int i = 0; i < casos.Count; i++)
        {
            CasoBullying caso = casos[i];

            if (caso == null || !caso.desbloqueado)
            {
                continue;
            }

            CrearBotonCaso(caso, visibles);
            visibles++;
        }

        if (visibles == 0)
        {
            EstiloUIJuego.CrearTextoTMP(
                listaCasos,
                "SinCasos",
                "Sin expedientes activos.\nUsa Revisar contexto desde Correo.",
                16f,
                Vector2.zero,
                new Vector2(220f, 120f),
                TextAlignmentOptions.Center
            );
        }
    }

    private void CrearBotonCaso(CasoBullying caso, int indice)
    {
        GameObject objeto = new GameObject(
            "BotonCaso_" + caso.idCaso,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        objeto.transform.SetParent(listaCasos, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -18f - indice * 54f);
        rect.sizeDelta = new Vector2(220f, 44f);

        Button boton = objeto.GetComponent<Button>();
        boton.onClick.AddListener(() => SeleccionarCaso(caso));
        EstiloUIJuego.AplicarBoton(
            boton,
            new Color(0.08f, 0.24f, 0.34f, 1f),
            EstiloUIJuego.Acento
        );

        EstiloUIJuego.CrearTextoTMP(
            objeto.transform,
            "Texto",
            caso.titulo,
            15f,
            Vector2.zero,
            new Vector2(208f, 38f),
            TextAlignmentOptions.Center
        );

        botonesCasos.Add(boton);
    }

    private void SeleccionarPrimerCasoVisible()
    {
        if (gestorCasos == null)
        {
            MostrarDetalle(null);
            return;
        }

        IReadOnlyList<CasoBullying> casos = gestorCasos.Casos;

        for (int i = 0; i < casos.Count; i++)
        {
            if (casos[i] != null && casos[i].desbloqueado)
            {
                SeleccionarCaso(casos[i]);
                return;
            }
        }

        MostrarDetalle(null);
    }

    private void SeleccionarCaso(CasoBullying caso)
    {
        casoSeleccionado = caso;
        MostrarDetalle(casoSeleccionado);
    }

    private void MostrarDetalle(CasoBullying caso)
    {
        if (textoTituloDetalle == null || textoDetalle == null)
        {
            return;
        }

        if (caso == null)
        {
            textoTituloDetalle.text = "Sin casos activos";
            textoDetalle.text =
                "Algunos correos necesitan contexto.\n\n"
                + "Usa Revisar contexto cuando veas una situación ambigua. "
                + "Aqui se guardan evidencias, personas involucradas y estado del expediente.";
            AjustarScrollDetalle();
            return;
        }

        textoTituloDetalle.text = caso.titulo;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Riesgo: " + caso.nivelRiesgo + " | Estado: " + caso.estado);
        builder.AppendLine("Dificultad: " + caso.dificultad);
        builder.AppendLine();
        builder.AppendLine(caso.descripcion);
        builder.AppendLine();
        builder.AppendLine("Personajes:");

        for (int i = 0; i < caso.personajesInvolucrados.Count; i++)
        {
            PersonajeCaso personaje = gestorCasos != null
                ? gestorCasos.ObtenerPersonaje(caso.personajesInvolucrados[i])
                : null;
            builder.AppendLine(
                "- "
                + (personaje != null ? personaje.nombre : caso.personajesInvolucrados[i])
                + (personaje != null ? " (" + personaje.rolActual + ")" : "")
            );
        }

        builder.AppendLine();
        builder.AppendLine("Evidencias:");

        for (int i = 0; i < caso.evidencias.Count; i++)
        {
            EvidenciaCaso evidencia = caso.evidencias[i];

            if (evidencia == null)
            {
                continue;
            }

            builder.AppendLine(
                evidencia.descubierta
                    ? "- " + evidencia.descripcion
                    : "- Evidencia pendiente"
            );
        }

        builder.AppendLine();
        builder.AppendLine(
            caso.TieneEvidenciaSuficiente
                ? "Hay evidencia suficiente para decidir con cuidado."
                : "Falta contexto. Resolver ahora puede afectar la confianza."
        );

        textoDetalle.text = builder.ToString();
        AjustarScrollDetalle();
    }

    private void AjustarScrollDetalle()
    {
        if (textoDetalle == null || contenidoDetalle == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        float alto = Mathf.Max(270f, textoDetalle.preferredHeight + 28f);
        contenidoDetalle.sizeDelta = new Vector2(contenidoDetalle.sizeDelta.x, alto);

        RectTransform rectTexto = textoDetalle.GetComponent<RectTransform>();

        if (rectTexto != null)
        {
            rectTexto.sizeDelta = new Vector2(rectTexto.sizeDelta.x, alto - 16f);
        }

        if (scrollDetalle != null)
        {
            scrollDetalle.verticalNormalizedPosition = 1f;
        }
    }

    private void ResolverCasoSeleccionado(TipoResolucionCaso decision)
    {
        if (gestorCasos == null || casoSeleccionado == null)
        {
            return;
        }

        DecisionCaso resultado = gestorCasos.ResolverCaso(casoSeleccionado.idCaso, decision);

        if (resultado != null)
        {
            ControlCorreo controlCorreo = FindAnyObjectByType<ControlCorreo>();

            if (controlCorreo != null)
            {
                controlCorreo.AplicarImpactoDecisionCaso(resultado);
            }

            MostrarDetalle(casoSeleccionado);
            textoDetalle.text +=
                "\n\nResultado de decisión:\n"
                + resultado.resultado
                + "\nImpacto bienestar: "
                + resultado.impactoBienestar
                + " | confianza: "
                + resultado.impactoConfianza
                + " | precisión: "
                + resultado.impactoPrecision;
        }
    }

    private void CerrarVentanaActual()
    {
        GestorVentanasPC gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (gestorVentanas != null)
        {
            gestorVentanas.CerrarVentana(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
