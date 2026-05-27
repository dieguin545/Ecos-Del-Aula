using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Rediseño visual para Ecos del Aula v3.
///
/// FILOSOFIA: Restilar elementos existentes, NO crear capas nuevas.
/// - NO crea fondos full-screen (los personajes SpriteRenderer quedarían tapados).
/// - Usa camera.backgroundColor para fondos.
/// - Modifica in-place textos, colores, posiciones y tamaños de los elementos originales.
/// - Solo crea elementos nuevos cuando es estrictamente necesario (título reemplazado).
/// - NUNCA toca OnClick, lógica de gameplay, ni scripts de selección.
/// </summary>
public static class EcosAulaUIRediseno
{
    // ─── Paleta ────────────────────────────────────────────────────────────────

    private static readonly Color FondoOscuro     = new Color(0.063f, 0.165f, 0.263f, 1f); // #102A43
    private static readonly Color FondoMorado     = new Color(0.106f, 0.043f, 0.180f, 1f); // #1B0B2E
    private static readonly Color PanelColor      = new Color(0.176f, 0.106f, 0.306f, 0.97f); // #2D1B4E
    private static readonly Color PanelClaro      = new Color(0.200f, 0.130f, 0.340f, 0.95f);

    private static readonly Color Lila            = new Color(0.655f, 0.545f, 0.980f, 1f); // #A78BFA
    private static readonly Color Celeste         = new Color(0.749f, 0.875f, 1.000f, 1f); // #BFDFFF
    private static readonly Color Verde           = new Color(0.463f, 0.784f, 0.576f, 1f); // #76C893
    private static readonly Color Amarillo        = new Color(1.000f, 0.820f, 0.400f, 1f); // #FFD166
    private static readonly Color Rojo            = new Color(0.937f, 0.278f, 0.435f, 1f); // #EF476F

    private static readonly Color Blanco          = Color.white;
    private static readonly Color TextoSec        = new Color(0.847f, 0.871f, 0.914f, 1f);
    private static readonly Color TextoApagado    = new Color(0.749f, 0.875f, 1.000f, 0.80f);

    private static readonly Color BtnMorado       = new Color(0.220f, 0.130f, 0.380f, 0.96f);
    private static readonly Color BtnMoradoH      = new Color(0.420f, 0.280f, 0.680f, 1f);
    private static readonly Color BtnVerde        = new Color(0.180f, 0.420f, 0.320f, 0.96f);
    private static readonly Color BtnVerdeH       = new Color(0.280f, 0.600f, 0.450f, 1f);
    private static readonly Color BtnRojo         = new Color(0.400f, 0.080f, 0.180f, 0.96f);
    private static readonly Color BtnRojoH        = new Color(0.600f, 0.130f, 0.270f, 1f);

    // ─── Inicialización ────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        Aplicar(SceneManager.GetActiveScene().name);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Aplicar(escena.name);
    }

    private static void Aplicar(string nombreEscena)
    {
        string nombre = nombreEscena.ToLowerInvariant();
        EcosAulaUIHelper.Instancia.EjecutarTrasFrames(AplicarConDelay(nombre));
    }

    public static void AplicarDirecto(string nombreEscena)
    {
        string nombre = nombreEscena.ToLowerInvariant();
        switch (nombre)
        {
            case "inicio":
                RedisenarInicio();
                break;
            case "seleccionjuego":
                RedisenarSeleccionJuego();
                break;
            case "seleccion":
                RedisenarSeleccionPersonaje();
                break;
            case "juego":
                RedisenarPausa();
                break;
        }
    }

    private static IEnumerator AplicarConDelay(string nombreEscena)
    {
        yield return null; // 1 frame de espera

        switch (nombreEscena)
        {
            case "inicio":
                RedisenarInicio();
                break;
            case "seleccionjuego":
                RedisenarSeleccionJuego();
                break;
            case "seleccion":
                RedisenarSeleccionPersonaje();
                break;
            case "juego":
                RedisenarPausa();
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INICIO — Menú Principal
    // ═══════════════════════════════════════════════════════════════════════════

    private static void RedisenarInicio()
    {
        // Fondo: solo cámara, NO crear overlay (hay SpriteRenderers de personajes)
        CambiarFondoCamara(FondoOscuro);

        Canvas canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) return;
        AsegurarCanvasOverlay(canvas);

        // ── Crear o buscar panel lateral izquierdo ──
        Transform leftPanelTr = canvas.transform.Find("_LeftMenuPanel");
        GameObject leftPanelGo;
        if (leftPanelTr == null)
        {
            leftPanelGo = new GameObject("_LeftMenuPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            leftPanelGo.transform.SetParent(canvas.transform, false);
            leftPanelTr = leftPanelGo.transform;
        }
        else
        {
            leftPanelGo = leftPanelTr.gameObject;
        }

        RectTransform panelRect = leftPanelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(420f, 0f);

        Image panelImg = leftPanelGo.GetComponent<Image>();
        panelImg.color = PanelColor;

        // ── Crear o buscar línea divisoria ──
        Transform dividerTr = leftPanelTr.Find("_Divider");
        if (dividerTr == null)
        {
            GameObject dividerGo = new GameObject("_Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dividerGo.transform.SetParent(leftPanelTr, false);
            dividerTr = dividerGo.transform;
        }
        RectTransform divRect = dividerTr.GetComponent<RectTransform>();
        divRect.anchorMin = new Vector2(1f, 0f);
        divRect.anchorMax = new Vector2(1f, 1f);
        divRect.pivot = new Vector2(1f, 0.5f);
        divRect.anchoredPosition = Vector2.zero;
        divRect.sizeDelta = new Vector2(3f, 0f);
        dividerTr.GetComponent<Image>().color = Lila;

        // ── Crear o buscar sombra de panel ──
        Transform shadowTr = leftPanelTr.Find("_Shadow");
        if (shadowTr == null)
        {
            GameObject shadowGo = new GameObject("_Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadowGo.transform.SetParent(leftPanelTr, false);
            shadowTr = shadowGo.transform;
        }
        RectTransform shadRect = shadowTr.GetComponent<RectTransform>();
        shadRect.anchorMin = new Vector2(1f, 0f);
        shadRect.anchorMax = new Vector2(1f, 1f);
        shadRect.pivot = new Vector2(0f, 0.5f);
        shadRect.anchoredPosition = new Vector2(3f, 0f);
        shadRect.sizeDelta = new Vector2(6f, 0f);
        shadowTr.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        // ── Buscar y restilar el título original ──
        // Texto original: "Bullying and dungeons: no mercy" → Reemplazar contenido
        TextMeshProUGUI[] textos = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI t in textos)
        {
            if (t.gameObject.name.StartsWith("_")) continue;

            // ¿Es hijo de un botón? → no tocar aquí, se toca abajo
            if (t.GetComponentInParent<Button>() != null) continue;

            // Es el título suelto de la escena
            string contenido = t.text.ToLowerInvariant();
            if (contenido.Contains("bullying") || contenido.Contains("ecos"))
            {
                // Reparentar al panel izquierdo
                t.transform.SetParent(leftPanelTr, false);

                // Convertirlo en nuestro título
                t.text = "ECOS DEL AULA";
                t.fontSize = 30f;
                t.color = Lila;
                t.fontStyle = FontStyles.Bold;
                t.alignment = TextAlignmentOptions.Center;

                // Reposicionar en el panel (anclado arriba-centro)
                RectTransform rect = t.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -40f);
                    rect.sizeDelta = new Vector2(-40f, 70f);
                }
            }
        }

        // ── Crear o reparentar subtítulo ──
        Transform subTr = leftPanelTr.Find("_Sub_inicio");
        if (subTr == null)
        {
            subTr = canvas.transform.Find("_Sub_inicio");
            if (subTr != null)
            {
                subTr.SetParent(leftPanelTr, false);
            }
            else
            {
                CrearTextoSimple(leftPanelTr, "_Sub_inicio",
                    "Un viaje educativo sobre convivencia digital",
                    15f, TextoApagado,
                    new Vector2(0f, -115f), new Vector2(-40f, 50f));
                subTr = leftPanelTr.Find("_Sub_inicio");
            }
        }

        if (subTr != null)
        {
            TextMeshProUGUI subTmp = subTr.GetComponent<TextMeshProUGUI>();
            if (subTmp != null)
            {
                subTmp.fontSize = 15f;
                subTmp.color = TextoApagado;
                subTmp.alignment = TextAlignmentOptions.Center;
                subTmp.fontStyle = FontStyles.Italic;
            }
            RectTransform subRect = subTr.GetComponent<RectTransform>();
            if (subRect != null)
            {
                subRect.anchorMin = new Vector2(0f, 1f);
                subRect.anchorMax = new Vector2(1f, 1f);
                subRect.pivot = new Vector2(0.5f, 1f);
                subRect.anchoredPosition = new Vector2(0f, -115f);
                subRect.sizeDelta = new Vector2(-40f, 50f);
            }
        }

        // ── Restilar y reposicionar botones ──
        Button[] botones = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button b in botones)
        {
            if (b.gameObject.name.StartsWith("_")) continue;

            string nombre = b.gameObject.name.ToLowerInvariant();
            RectTransform rect = b.GetComponent<RectTransform>();

            if (nombre == "jugar")
            {
                b.transform.SetParent(leftPanelTr, false);
                EstilizarBoton(b, BtnVerde, BtnVerdeH, "JUGAR", 28f, Blanco);
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -200f);
                    rect.sizeDelta = new Vector2(300f, 55f);
                }
            }
            else if (nombre == "cambiar")
            {
                b.transform.SetParent(leftPanelTr, false);
                EstilizarBoton(b, BtnMorado, BtnMoradoH, "CAMBIAR PERSONAJE", 22f, Blanco);
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -275f);
                    rect.sizeDelta = new Vector2(300f, 48f);
                }
            }
            else if (nombre == "salir")
            {
                b.transform.SetParent(leftPanelTr, false);
                EstilizarBoton(b, BtnRojo, BtnRojoH, "SALIR", 22f, Blanco);
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -340f);
                    rect.sizeDelta = new Vector2(300f, 48f);
                }
            }
        }

        // ── Reposicionar todos los personajes en el espacio 3D/Mundo a la derecha ──
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Include);
        }

        foreach (GameObject rootGo in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            MoverPersonajesRecursivo(rootGo.transform, mainCam, canvas);
        }

        // Prompts en el menú lateral izquierdo
        EcosAulaPromptUI.CrearBarraPrompts(leftPanelTr, 
            (AccionLogica.Confirmar, "Seleccionar"), 
            (AccionLogica.Navegar, "Navegar"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SELECCIÓN DE JUEGO
    // ═══════════════════════════════════════════════════════════════════════════

    private static void RedisenarSeleccionJuego()
    {
        CambiarFondoCamara(FondoMorado);

        Canvas canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) return;
        AsegurarCanvasOverlay(canvas);

        Transform raiz = canvas.transform;

        // ── Restilar textos sueltos ──
        TextMeshProUGUI[] textos = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI t in textos)
        {
            if (t.gameObject.name.StartsWith("_")) continue;
            if (t.GetComponentInParent<Button>() != null) continue;

            string contenido = t.text.ToLowerInvariant();

            if (contenido.Contains("selecciona"))
            {
                // Título principal - restilar in-place
                t.text = "SELECCIONA EL JUEGO";
                t.fontSize = 30f;
                t.color = Lila;
                t.fontStyle = FontStyles.Bold;
                t.alignment = TextAlignmentOptions.Center;

                RectTransform rect = t.GetComponent<RectTransform>();
                if (rect != null)
                {
                    AsegurarHijoDirecto(rect, raiz);
                    PosicionarSeleccionJuego(rect, new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(760f, 46f), new Vector2(0.5f, 1f));
                }
            }
            else if (contenido.Contains("filtro") || contenido.Contains("entry"))
            {
                // Etiqueta juego 1
                t.text = "ENTRY FILTER";
                t.fontSize = 20f;
                t.color = Verde;
                t.fontStyle = FontStyles.Bold;
                t.alignment = TextAlignmentOptions.Center;
                RectTransform rect = t.GetComponent<RectTransform>();
                AsegurarHijoDirecto(rect, raiz);
                PosicionarSeleccionJuego(rect, new Vector2(0.28f, 0.66f), Vector2.zero, new Vector2(300f, 34f), new Vector2(0.5f, 0.5f));
            }
            else if (contenido.Contains("minijuego") || contenido.Contains("vida"))
            {
                // Etiqueta juego 2
                t.text = "VIDA ESCOLAR";
                t.fontSize = 20f;
                t.color = Amarillo;
                t.fontStyle = FontStyles.Bold;
                t.alignment = TextAlignmentOptions.Center;
                RectTransform rect = t.GetComponent<RectTransform>();
                AsegurarHijoDirecto(rect, raiz);
                PosicionarSeleccionJuego(rect, new Vector2(0.72f, 0.66f), Vector2.zero, new Vector2(300f, 34f), new Vector2(0.5f, 0.5f));
            }
        }

        // ── Subtítulo ──
        Transform sub = raiz.Find("_Sub_seljuego");
        if (sub == null)
        {
            CrearTextoSimple(raiz, "_Sub_seljuego",
                "Elige una experiencia para comenzar",
                16f, TextoApagado,
                new Vector2(0f, -65f), new Vector2(0f, 25f));
        }
        else
        {
            TextMeshProUGUI subTmp = sub.GetComponent<TextMeshProUGUI>();
            if (subTmp != null)
            {
                subTmp.text = "Elige una experiencia para comenzar";
                subTmp.fontSize = 16f;
                subTmp.alignment = TextAlignmentOptions.Center;
                subTmp.color = TextoApagado;
            }

            PosicionarSeleccionJuego(sub.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(620f, 28f), new Vector2(0.5f, 1f));
        }

        // ── Restilar botones ──
        Button[] botones = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button b in botones)
        {
            if (b.gameObject.name.StartsWith("_")) continue;

            string nombre = b.gameObject.name.ToLowerInvariant();

            if (nombre == "jugar")
            {
                // Botón Jugar del Entry Filter
                EstilizarBoton(b, new Color(0.12f, 0.42f, 0.32f, 0.96f), BtnVerdeH, "JUGAR", 20f, Blanco);
                RectTransform rect = b.GetComponent<RectTransform>();
                AsegurarHijoDirecto(rect, raiz);
                PosicionarSeleccionJuego(rect, new Vector2(0.28f, 0.20f), Vector2.zero, new Vector2(190f, 40f), new Vector2(0.5f, 0.5f));
                b.transform.SetAsLastSibling();
            }
            else if (nombre == "minijuego")
            {
                // Botón Jugar del Minijuego/Vida Escolar
                EstilizarBoton(b, new Color(0.22f, 0.13f, 0.04f, 0.96f), new Color(0.42f, 0.27f, 0.08f, 1f), "JUGAR", 20f, Blanco);
                RectTransform rect = b.GetComponent<RectTransform>();
                AsegurarHijoDirecto(rect, raiz);
                PosicionarSeleccionJuego(rect, new Vector2(0.72f, 0.20f), Vector2.zero, new Vector2(190f, 40f), new Vector2(0.5f, 0.5f));
                b.transform.SetAsLastSibling();
            }
            else if (nombre.Contains("atras") || nombre == "button")
            {
                EstilizarBoton(b, BtnMorado, BtnMoradoH, "ATRÁS", 20f, Blanco);
                RectTransform rect = b.GetComponent<RectTransform>();
                AsegurarHijoDirecto(rect, raiz);
                PosicionarSeleccionJuego(rect, new Vector2(0.50f, 0.12f), Vector2.zero, new Vector2(184f, 36f), new Vector2(0.5f, 0.5f));
                b.transform.SetAsLastSibling();
            }
        }

        // ── Estilizar las imágenes de preview ──
        Image[] imagenes = canvas.GetComponentsInChildren<Image>(true);
        foreach (Image img in imagenes)
        {
            if (img.sprite != null && img.GetComponent<Button>() == null && img.GetComponentInParent<Button>() == null)
            {
                // Es una imagen de preview (no un botón)
                img.preserveAspect = true;
                string nombreImagen = img.gameObject.name.ToLowerInvariant();
                if (nombreImagen.Contains("entry"))
                {
                    RectTransform rect = img.GetComponent<RectTransform>();
                    AsegurarHijoDirecto(rect, raiz);
                    PosicionarSeleccionJuego(rect, new Vector2(0.28f, 0.43f), Vector2.zero, new Vector2(266f, 150f), new Vector2(0.5f, 0.5f));
                }
                else if (nombreImagen.Contains("minijuego") || nombreImagen.Contains("vida"))
                {
                    RectTransform rect = img.GetComponent<RectTransform>();
                    AsegurarHijoDirecto(rect, raiz);
                    PosicionarSeleccionJuego(rect, new Vector2(0.72f, 0.43f), Vector2.zero, new Vector2(266f, 150f), new Vector2(0.5f, 0.5f));
                }
            }
        }

        foreach (Button b in botones)
        {
            if (b == null || b.gameObject.name.StartsWith("_"))
            {
                continue;
            }

            string nombre = b.gameObject.name.ToLowerInvariant();
            if (nombre == "jugar" || nombre == "minijuego" || nombre.Contains("atras") || nombre == "button")
            {
                b.transform.SetAsLastSibling();
            }
        }

        // Prompts para seleccionar tarjeta de juego
        GameObject barra = EcosAulaPromptUI.CrearBarraPrompts(canvas.transform,
            (AccionLogica.Navegar, "Cambiar juego"),
            (AccionLogica.Confirmar, "Jugar"),
            (AccionLogica.Cancelar, "Atrás"));
        PosicionarSeleccionJuego(barra != null ? barra.GetComponent<RectTransform>() : null, new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(0f, 42f), new Vector2(0.5f, 0f));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SELECCIÓN DE PERSONAJE
    // ═══════════════════════════════════════════════════════════════════════════

    private static void RedisenarSeleccionPersonaje()
    {
        // NO crear fondo — los personajes son SpriteRenderers en world space
        CambiarFondoCamara(FondoMorado);

        Canvas canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null) return;
        AsegurarCanvasOverlay(canvas);

        // ── Restilar título existente ──
        TextMeshProUGUI[] textos = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI t in textos)
        {
            if (t.gameObject.name.StartsWith("_")) continue;
            if (t.GetComponentInParent<Button>() != null) continue;

            string contenido = t.text.ToLowerInvariant();
            if (contenido.Contains("selecciona") || contenido.Contains("personaje")
                || contenido.Contains("elige"))
            {
                t.text = "ELIGE TU PERSONAJE";
                t.fontSize = 40f;
                t.color = Lila;
                t.fontStyle = FontStyles.Bold;
                t.alignment = TextAlignmentOptions.Center;

                RectTransform rect = t.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -15f);
                    rect.sizeDelta = new Vector2(0f, 55f);
                }
            }
        }

        // ── Restilar botones ──
        Button[] botones = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button b in botones)
        {
            if (b.gameObject.name.StartsWith("_")) continue;

            string nombre = b.gameObject.name.ToLowerInvariant();

            if (nombre.Contains("anterior") || nombre.Contains("prev"))
            {
                EstilizarBoton(b, BtnMorado, BtnMoradoH, "ANTERIOR", 22f, Blanco);
            }
            else if (nombre.Contains("siguiente") || nombre.Contains("next"))
            {
                EstilizarBoton(b, BtnMorado, BtnMoradoH, "SIGUIENTE", 22f, Blanco);
            }
            else if (nombre.Contains("seleccionar") || nombre.Contains("select"))
            {
                EstilizarBoton(b, BtnVerde, BtnVerdeH, "SELECCIONAR", 24f, Blanco);
            }
            else if (nombre.Contains("atras") || nombre.Contains("back"))
            {
                EstilizarBoton(b, BtnMorado, BtnMoradoH, "ATRÁS", 20f, Blanco);
            }
            else
            {
                EstilizarBoton(b, BtnMorado, BtnMoradoH, null, 20f, Blanco);
            }
        }

        // Prompts para selección de personaje
        EcosAulaPromptUI.CrearBarraPrompts(canvas.transform,
            (AccionLogica.AnteriorPestana, "Anterior"),
            (AccionLogica.SiguientePestana, "Siguiente"),
            (AccionLogica.Confirmar, "Seleccionar"),
            (AccionLogica.Cancelar, "Atrás"));

        if (canvas.gameObject.GetComponent<EcosAulaNavegacionPersonajes>() == null)
        {
            canvas.gameObject.AddComponent<EcosAulaNavegacionPersonajes>();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MENÚ DE PAUSA
    // ═══════════════════════════════════════════════════════════════════════════

    private static void RedisenarPausa()
    {
        CambiarFondoCamara(FondoOscuro);

        MenuPausaAccesibilidad pausa = Object.FindAnyObjectByType<MenuPausaAccesibilidad>(FindObjectsInactive.Include);
        if (pausa == null) return;

        // ── Panel principal ──
        if (pausa.panelPausa != null)
        {
            AplicarFondoPanel(pausa.panelPausa, PanelColor);
        }

        // ── Panel opciones ──
        if (pausa.panelOpciones != null)
        {
            AplicarFondoPanel(pausa.panelOpciones, PanelClaro);
        }

        // ── Panel detalle slot ──
        if (pausa.panelDetalleSlot != null)
        {
            AplicarFondoPanel(pausa.panelDetalleSlot, PanelClaro);
        }

        // ── Textos especiales ──
        if (pausa.textoSlotActivo != null)
            pausa.textoSlotActivo.color = Lila;
        if (pausa.textoDetalleSlot != null)
            pausa.textoDetalleSlot.color = TextoSec;

        // ── Botones principales ──
        EstilizarBoton(pausa.botonContinuar,  BtnVerde,  BtnVerdeH,  "CONTINUAR",       22f, Blanco);
        EstilizarBoton(pausa.botonOpciones,   BtnMorado, BtnMoradoH, "OPCIONES",        22f, Blanco);
        EstilizarBoton(pausa.botonReiniciar,  BtnMorado, BtnMoradoH, "REINICIAR",       22f, Blanco);
        EstilizarBoton(pausa.botonSalir,      BtnRojo,   BtnRojoH,   "SALIR AL MENU",   22f, Blanco);
        EstilizarBoton(pausa.botonVolver,     BtnMorado, BtnMoradoH, "VOLVER",          22f, Blanco);

        // ── Botones daltonismo ──
        Color cDalt  = new Color(0.18f, 0.12f, 0.32f, 0.95f);
        Color cDaltH = new Color(0.32f, 0.22f, 0.52f, 1f);
        EstilizarBotonColor(pausa.botonDaltonismoNinguno,      cDalt, cDaltH);
        EstilizarBotonColor(pausa.botonDaltonismoProtanopia,   cDalt, cDaltH);
        EstilizarBotonColor(pausa.botonDaltonismoDeuteranopia, cDalt, cDaltH);
        EstilizarBotonColor(pausa.botonDaltonismoTritanopia,   cDalt, cDaltH);
        EstilizarBotonColor(pausa.botonDaltonismoAcromatopsia, cDalt, cDaltH);

        // ── Botones guardado ──
        Color cSlot  = new Color(0.14f, 0.10f, 0.28f, 0.95f);
        Color cSlotH = new Color(0.28f, 0.20f, 0.50f, 1f);
        EstilizarBotonColor(pausa.botonSlot1, cSlot, cSlotH);
        EstilizarBotonColor(pausa.botonSlot2, cSlot, cSlotH);
        EstilizarBotonColor(pausa.botonSlot3, cSlot, cSlotH);
        EstilizarBoton(pausa.botonEntrarSlot,          BtnVerde, BtnVerdeH, "CARGAR",           20f, Blanco);
        EstilizarBoton(pausa.botonBorrarSlot,          BtnRojo,  BtnRojoH,  "BORRAR",           20f, Blanco);
        EstilizarBoton(pausa.botonConfirmarBorrarSlot, BtnRojo,  BtnRojoH,  "CONFIRMAR BORRAR", 18f, Blanco);
        EstilizarBoton(pausa.botonCancelarSlot,        BtnMorado, BtnMoradoH, "CANCELAR",       20f, Blanco);

        // ── Todos los textos del panel de pausa en blanco ──
        if (pausa.panelPausa != null)
        {
            TextMeshProUGUI[] textosPausa = pausa.panelPausa.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI t in textosPausa)
            {
                if (t.gameObject.name.StartsWith("_")) continue;
                t.color = Blanco;
            }

            EcosAulaPromptUI.CrearBarraPrompts(pausa.panelPausa.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Confirmar"),
                (AccionLogica.Cancelar, "Continuar"));
        }

        if (pausa.panelOpciones != null)
        {
            EcosAulaPromptUI.CrearBarraPrompts(pausa.panelOpciones.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Confirmar / Cambiar"),
                (AccionLogica.Cancelar, "Volver"));
        }

        if (pausa.panelDetalleSlot != null)
        {
            EcosAulaPromptUI.CrearBarraPrompts(pausa.panelDetalleSlot.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Confirmar"),
                (AccionLogica.Cancelar, "Cancelar"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // UTILIDADES
    // ═══════════════════════════════════════════════════════════════════════════

    private static void PosicionarSeleccionJuego(RectTransform rect, Vector2 anchor, Vector2 posicion, Vector2 size, Vector2 pivot)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = size;
    }

    /// <summary>Estiliza un botón: colores + texto (si se proporciona).</summary>
    private static void AsegurarHijoDirecto(RectTransform rect, Transform parent)
    {
        if (rect == null || parent == null)
        {
            return;
        }

        if (rect.parent != parent)
        {
            rect.SetParent(parent, false);
        }

        rect.localScale = Vector3.one;
    }

    private static void EstilizarBoton(Button boton, Color normal, Color hover,
        string texto, float fontSize, Color colorTexto)
    {
        if (boton == null) return;

        EstilizarBotonColor(boton, normal, hover);

        if (texto != null)
        {
            TextMeshProUGUI tmp = boton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = texto;
                tmp.color = colorTexto;
                tmp.fontSize = fontSize;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold;
            }
        }
    }

    /// <summary>Solo aplica colores a un botón, sin cambiar texto.</summary>
    private static void EstilizarBotonColor(Button boton, Color normal, Color hover)
    {
        if (boton == null) return;

        Image img = boton.GetComponent<Image>();
        if (img != null)
        {
            img.color = normal;
        }

        ColorBlock cb = boton.colors;
        cb.normalColor      = normal;
        cb.highlightedColor = hover;
        cb.selectedColor    = hover;
        cb.pressedColor     = Color.Lerp(normal, Color.black, 0.3f);
        cb.disabledColor    = new Color(normal.r, normal.g, normal.b, 0.35f);
        cb.colorMultiplier  = 1f;
        boton.colors = cb;

        Navigation nav = boton.navigation;
        nav.mode = Navigation.Mode.Automatic;
        boton.navigation = nav;

        MarcarSucio(boton);
    }

    /// <summary>Aplica fondo a un panel existente.</summary>
    private static void AplicarFondoPanel(GameObject panel, Color color)
    {
        if (panel == null) return;
        Image img = panel.GetComponent<Image>();
        if (img != null)
        {
            img.color = color;
        }
        else
        {
            img = panel.AddComponent<Image>();
            img.color = color;
        }
        MarcarSucio(panel);
    }

    /// <summary>Cambia el color de fondo de la cámara (sin crear UI overlay).</summary>
    private static void CambiarFondoCamara(Color color)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Include);
        }
        if (cam != null)
        {
            cam.backgroundColor = color;
            cam.clearFlags = CameraClearFlags.SolidColor;
            MarcarSucio(cam);
        }
    }

    private static void AsegurarCanvasOverlay(Canvas canvas)
    {
        if (canvas == null) return;

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            MarcarSucio(canvas);
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            MarcarSucio(scaler);
        }
    }

    /// <summary>Crea un texto TMP simple anclado arriba-centro.</summary>
    private static void CrearTextoSimple(Transform padre, string nombre,
        string texto, float fontSize, Color color,
        Vector2 posOffset, Vector2 size)
    {
        GameObject go = new GameObject(nombre,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = posOffset;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Italic;
        tmp.raycastTarget = false;

        MarcarSucio(go);
    }

    /// <summary>Mueve recursivamente cualquier objeto que empiece por 'Personaje' al centro del espacio derecho de la pantalla.</summary>
    private static void MoverPersonajesRecursivo(Transform t, Camera mainCam, Canvas canvas)
    {
        if (t.gameObject.name.StartsWith("Personaje", System.StringComparison.OrdinalIgnoreCase))
        {
            Vector3 pos = t.position;
            if (mainCam != null)
            {
                // Profundidad del personaje en el espacio de la cámara
                Vector3 localPos = mainCam.transform.InverseTransformPoint(pos);
                
                // Ancho y alto del renderizado de la cámara
                float W = mainCam.pixelWidth;
                float H = mainCam.pixelHeight;
                
                // Factor de escala del canvas
                float scaleFactor = (canvas != null) ? canvas.scaleFactor : 1.0f;
                if (scaleFactor <= 0f) scaleFactor = 1.0f;
                
                // Ancho del panel izquierdo (420px de referencia) escalado a píxeles de pantalla
                float panelWidthScreen = 420f * scaleFactor;
                
                // Centro del espacio restante a la derecha
                float targetScreenX = (panelWidthScreen + W) / 2f;
                float targetScreenY = H / 2f;
                
                // Convertir la coordenada de pantalla a mundo con la profundidad original
                Vector3 worldPoint = mainCam.ScreenToWorldPoint(new Vector3(targetScreenX, targetScreenY, localPos.z));
                pos.x = worldPoint.x;
            }
            t.position = pos;
            MarcarSucio(t);
        }

        for (int i = 0; i < t.childCount; i++)
        {
            MoverPersonajesRecursivo(t.GetChild(i), mainCam, canvas);
        }
    }

    private static void MarcarSucio(UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && obj != null)
        {
            UnityEditor.EditorUtility.SetDirty(obj);
        }
#endif
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
public static class EcosAulaUIEditorHook
{
    static EcosAulaUIEditorHook()
    {
        // Registrar callbacks en el editor para rediseñar en tiempo de diseño
        EditorApplication.delayCall += AplicarEnEditor;
        
        EditorApplication.hierarchyChanged -= AlCambiarJerarquia;
        EditorApplication.hierarchyChanged += AlCambiarJerarquia;

        EditorSceneManager.sceneOpened -= AlAbrirEscena;
        EditorSceneManager.sceneOpened += AlAbrirEscena;
    }

    private static void AlCambiarJerarquia()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.delayCall -= AplicarEnEditor;
        EditorApplication.delayCall += AplicarEnEditor;
    }

    private static void AlAbrirEscena(Scene escena, OpenSceneMode modo)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.delayCall -= AplicarEnEditor;
        EditorApplication.delayCall += AplicarEnEditor;
    }

    private static void AplicarEnEditor()
    {
        string nombreEscena = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(nombreEscena)) return;
        EcosAulaUIRediseno.AplicarDirecto(nombreEscena);

        if (!Application.isPlaying)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
            SceneView.RepaintAll();
        }
    }
}
#endif
