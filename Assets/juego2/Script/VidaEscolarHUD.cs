using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VidaEscolarHUD : MonoBehaviour, IAnxietyObserver
{
    private static VidaEscolarHUD instancia;

    private Image ansiedadFill;
    private TextMeshProUGUI textoAnsiedad;
    private TextMeshProUGUI textoZona;
    private TextMeshProUGUI textoTiempo;
    private TextMeshProUGUI textoTareas;
    private TextMeshProUGUI textoEvidencias;
    private TextMeshProUGUI textoToast;
    private CanvasGroup toastGroup;
    private Coroutine toastRoutine;
    private Tween ansiedadTween;

    public static VidaEscolarHUD Ensure()
    {
        if (instancia != null)
        {
            return instancia;
        }

        VidaEscolarHUD existente = FindAnyObjectByType<VidaEscolarHUD>();
        if (existente != null)
        {
            instancia = existente;
            return instancia;
        }

        GameObject canvasGo = GameObject.Find("Canvas_VidaEscolarHUD");
        Canvas canvas = canvasGo != null ? canvasGo.GetComponent<Canvas>() : null;
        if (canvas == null)
        {
            canvasGo = new GameObject("Canvas_VidaEscolarHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        Transform raizExistente = canvas.transform.Find("VidaEscolarHUD");
        GameObject raiz = raizExistente != null
            ? raizExistente.gameObject
            : new GameObject("VidaEscolarHUD", typeof(RectTransform));
        raiz.transform.SetParent(canvas.transform, false);
        instancia = raiz.GetComponent<VidaEscolarHUD>();
        if (instancia == null)
        {
            instancia = raiz.AddComponent<VidaEscolarHUD>();
        }

        instancia.Construir();
        return instancia;
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        Construir();
        RegistrarAnsiedad();
    }

    private void Start()
    {
        RegistrarAnsiedad();
    }

    private void OnEnable()
    {
        RegistrarAnsiedad();
    }

    private void OnDisable()
    {
        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.RemoveObserver(this);
        }
    }

    public void OnAnxietyChanged(float currentAnxiety, float maxAnxiety)
    {
        ActualizarAnsiedad(currentAnxiety, maxAnxiety);
    }

    private void RegistrarAnsiedad()
{
    if (AnxietySystem.Instance == null)
    {
        StartCoroutine(EsperarAnxietySystem());
        return;
    }

    AnxietySystem.Instance.RemoveObserver(this);
    AnxietySystem.Instance.AddObserver(this);
    ActualizarAnsiedad(
        AnxietySystem.Instance.GetCurrentAnxiety(), 
        AnxietySystem.Instance.maxAnxiety);
}

private IEnumerator EsperarAnxietySystem()
{
    while (AnxietySystem.Instance == null)
        yield return null;

    AnxietySystem.Instance.RemoveObserver(this);
    AnxietySystem.Instance.AddObserver(this);
    ActualizarAnsiedad(
        AnxietySystem.Instance.GetCurrentAnxiety(), 
        AnxietySystem.Instance.maxAnxiety);
}

    private void Construir()
    {
        RectTransform root = GetComponent<RectTransform>();
        if (root == null)
        {
            root = gameObject.AddComponent<RectTransform>();
        }

        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        transform.SetAsLastSibling();

        DesactivarHudAnsiedadViejo();

        if (transform.Find("PanelAnsiedad") == null)
        {
            CrearPanelAnsiedad(root);
        }

        if (transform.Find("PanelTiempo") == null)
        {
            CrearPanelTiempo(root);
        }

        if (transform.Find("PanelTareas") == null)
        {
            CrearPanelTareas(root);
        }

        if (transform.Find("Toast") == null)
        {
            CrearToast(root);
        }

        if (transform.Find("_BarraPrompts") == null)
        {
            GameObject barra = EcosAulaPromptUI.CrearBarraPrompts(root,
                (AccionLogica.Navegar, "Mover"),
                (AccionLogica.Interactuar, "Interactuar"),
                (AccionLogica.Pausa, "Pausa"));
            RectTransform barraRect = barra != null ? barra.GetComponent<RectTransform>() : null;
            if (barraRect != null)
            {
                barraRect.anchorMin = new Vector2(0.5f, 0f);
                barraRect.anchorMax = new Vector2(0.5f, 0f);
                barraRect.pivot = new Vector2(0.5f, 0f);
                barraRect.anchoredPosition = new Vector2(0f, 18f);
            }
        }

        ReajustarLayoutHUD();
    }

    public void ActualizarAnsiedad(float actual, float maxima)
    {
        if (maxima <= 0f)
        {
            maxima = 100f;
        }

        float porcentaje = Mathf.Clamp01(actual / maxima);
        if (ansiedadFill != null)
        {
            ansiedadTween?.Kill();
            if (Application.isPlaying)
            {
                ansiedadTween = ansiedadFill
                    .DOFillAmount(porcentaje, 0.18f)
                    .SetEase(Ease.OutQuad)
                    .SetLink(gameObject);
            }
            else
            {
                ansiedadFill.fillAmount = porcentaje;
            }

            ansiedadFill.color = Color.Lerp(new Color(0.22f, 0.95f, 0.55f), new Color(1f, 0.18f, 0.22f), porcentaje);
        }

        if (textoAnsiedad != null)
        {
            textoAnsiedad.text = $"{Mathf.RoundToInt(porcentaje * 100f)}%";
        }
    }

    public void ActualizarTiempo(BloqueTiempo bloque, float tiempoRestante)
    {
        int minutos = Mathf.FloorToInt(Mathf.Max(0f, tiempoRestante) / 60f);
        int segundos = Mathf.FloorToInt(Mathf.Max(0f, tiempoRestante) % 60f);

        if (textoZona != null)
        {
            textoZona.text = bloque.ToString();
        }

        if (textoTiempo != null)
        {
            textoTiempo.text = $"{minutos:00}:{segundos:00}";
        }
    }

    public void ActualizarEvidencias(int actual, int necesarias)
    {
        if (textoEvidencias != null)
        {
            textoEvidencias.text = $"Evidencias {actual}/{Mathf.Max(1, necesarias)}";
        }
    }

    public void ActualizarMisiones(IEnumerable<Mision> activas, IEnumerable<Mision> completadas)
    {
        if (textoTareas == null)
        {
            return;
        }

        int activasCount = 0;
        string contenido = "";
        if (activas != null)
        {
            foreach (Mision mision in activas)
            {
                if (mision == null)
                {
                    continue;
                }

                activasCount++;
                contenido += $"- {Recortar(mision.dialogoInicio, 68)}\n";
                if (activasCount >= 3)
                {
                    break;
                }
            }
        }

        if (activasCount == 0)
        {
            contenido = "Sin tareas activas\nExplora la zona y habla con personajes.";
        }

        textoTareas.text = contenido;
    }

    public void MostrarToast(string mensaje)
    {
        if (textoToast == null || toastGroup == null)
        {
            return;
        }

        textoToast.text = mensaje;
        UIAudioManager.PlayNotification();
        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
        }

        toastRoutine = StartCoroutine(MostrarToastRutina());
    }

    private IEnumerator MostrarToastRutina()
    {
        toastGroup.gameObject.SetActive(true);
        toastGroup.DOKill();
        toastGroup.alpha = 0f;
        toastGroup.transform.localScale = Vector3.one * 0.96f;
        toastGroup.DOFade(1f, 0.14f).SetUpdate(true).SetLink(toastGroup.gameObject);
        toastGroup.transform.DOScale(1f, 0.16f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(toastGroup.gameObject);
        yield return new WaitForSecondsRealtime(2.2f);

        toastGroup.DOFade(0f, 0.22f).SetUpdate(true).SetLink(toastGroup.gameObject);
        yield return new WaitForSecondsRealtime(0.24f);
        if (toastGroup != null)
        {
            toastGroup.gameObject.SetActive(false);
        }
    }

    private void CrearPanelAnsiedad(RectTransform root)
    {
        GameObject panel = CrearPanel("PanelAnsiedad", root, new Vector2(28f, -24f), new Vector2(340f, 98f), new Vector2(0f, 1f));
        CrearTexto(panel.transform, "Titulo", "ANSIEDAD", 22f, FontStyles.Bold, new Vector2(18f, -12f), new Vector2(184f, 30f), TextAlignmentOptions.Left);
        textoAnsiedad = CrearTexto(panel.transform, "Valor", "0%", 24f, FontStyles.Bold, new Vector2(224f, -10f), new Vector2(94f, 32f), TextAlignmentOptions.Right);

        GameObject barraBg = new GameObject("BarraFondo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barraBg.transform.SetParent(panel.transform, false);
        RectTransform bgRt = barraBg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0f);
        bgRt.anchorMax = new Vector2(1f, 0f);
        bgRt.pivot = new Vector2(0.5f, 0f);
        bgRt.offsetMin = new Vector2(18f, 18f);
        bgRt.offsetMax = new Vector2(-18f, 36f);
        Image bg = barraBg.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.08f, 0.13f, 0.95f);
        bg.raycastTarget = false;

        GameObject fillGo = new GameObject("BarraRelleno", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillGo.transform.SetParent(barraBg.transform, false);
        RectTransform fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        ansiedadFill = fillGo.GetComponent<Image>();
        ansiedadFill.type = Image.Type.Filled;
        ansiedadFill.fillMethod = Image.FillMethod.Horizontal;
        ansiedadFill.color = new Color(0.22f, 0.95f, 0.55f);
        ansiedadFill.raycastTarget = false;
    }

    private void CrearPanelTiempo(RectTransform root)
    {
        GameObject panel = CrearPanel("PanelTiempo", root, new Vector2(-28f, -24f), new Vector2(250f, 92f), new Vector2(1f, 1f));
        textoZona = CrearTexto(panel.transform, "Zona", "Entrada", 24f, FontStyles.Bold, new Vector2(16f, -10f), new Vector2(218f, 34f), TextAlignmentOptions.Center);
        textoTiempo = CrearTexto(panel.transform, "Tiempo", "00:00", 24f, FontStyles.Bold, new Vector2(16f, -50f), new Vector2(218f, 32f), TextAlignmentOptions.Center);
    }

    private void CrearPanelTareas(RectTransform root)
    {
        GameObject panel = CrearPanel("PanelTareas", root, new Vector2(-28f, -134f), new Vector2(390f, 218f), new Vector2(1f, 1f));
        CrearTexto(panel.transform, "Titulo", "TAREAS", 24f, FontStyles.Bold, new Vector2(20f, -14f), new Vector2(174f, 32f), TextAlignmentOptions.Left);
        textoEvidencias = CrearTexto(panel.transform, "Evidencias", "Evidencias 0/3", 18f, FontStyles.Bold, new Vector2(198f, -16f), new Vector2(172f, 28f), TextAlignmentOptions.Right);
        textoTareas = CrearTexto(panel.transform, "Lista", "Sin tareas activas", 18f, FontStyles.Normal, new Vector2(20f, -58f), new Vector2(350f, 140f), TextAlignmentOptions.TopLeft);
    }

    private void ReajustarLayoutHUD()
    {
        RectTransform panelAnsiedad = transform.Find("PanelAnsiedad") as RectTransform;
        ConfigurarRect(panelAnsiedad, new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(340f, 98f), new Vector2(0f, 1f));
        AjustarTexto(panelAnsiedad, "Titulo", 22f, new Vector2(18f, -12f), new Vector2(184f, 30f), TextAlignmentOptions.Left);
        AjustarTexto(panelAnsiedad, "Valor", 24f, new Vector2(224f, -10f), new Vector2(94f, 32f), TextAlignmentOptions.Right);

        RectTransform barraFondo = panelAnsiedad != null ? panelAnsiedad.Find("BarraFondo") as RectTransform : null;
        if (barraFondo != null)
        {
            barraFondo.anchorMin = new Vector2(0f, 0f);
            barraFondo.anchorMax = new Vector2(1f, 0f);
            barraFondo.pivot = new Vector2(0.5f, 0f);
            barraFondo.offsetMin = new Vector2(18f, 18f);
            barraFondo.offsetMax = new Vector2(-18f, 38f);
        }

        RectTransform panelTiempo = transform.Find("PanelTiempo") as RectTransform;
        ConfigurarRect(panelTiempo, new Vector2(1f, 1f), new Vector2(-28f, -24f), new Vector2(250f, 92f), new Vector2(1f, 1f));
        AjustarTexto(panelTiempo, "Zona", 24f, new Vector2(16f, -10f), new Vector2(218f, 34f), TextAlignmentOptions.Center);
        AjustarTexto(panelTiempo, "Tiempo", 24f, new Vector2(16f, -50f), new Vector2(218f, 32f), TextAlignmentOptions.Center);

        RectTransform panelTareas = transform.Find("PanelTareas") as RectTransform;
        ConfigurarRect(panelTareas, new Vector2(1f, 1f), new Vector2(-28f, -134f), new Vector2(390f, 218f), new Vector2(1f, 1f));
        AjustarTexto(panelTareas, "Titulo", 24f, new Vector2(20f, -14f), new Vector2(174f, 32f), TextAlignmentOptions.Left);
        AjustarTexto(panelTareas, "Evidencias", 18f, new Vector2(198f, -16f), new Vector2(172f, 28f), TextAlignmentOptions.Right);
        AjustarTexto(panelTareas, "Lista", 18f, new Vector2(20f, -58f), new Vector2(350f, 140f), TextAlignmentOptions.TopLeft);

        ansiedadFill = transform.Find("PanelAnsiedad/BarraFondo/BarraRelleno")?.GetComponent<Image>();
        textoAnsiedad = transform.Find("PanelAnsiedad/Valor")?.GetComponent<TextMeshProUGUI>();
        textoZona = transform.Find("PanelTiempo/Zona")?.GetComponent<TextMeshProUGUI>();
        textoTiempo = transform.Find("PanelTiempo/Tiempo")?.GetComponent<TextMeshProUGUI>();
        textoEvidencias = transform.Find("PanelTareas/Evidencias")?.GetComponent<TextMeshProUGUI>();
        textoTareas = transform.Find("PanelTareas/Lista")?.GetComponent<TextMeshProUGUI>();

        RectTransform barraPrompts = transform.Find("_BarraPrompts") as RectTransform;
        if (barraPrompts != null)
        {
            barraPrompts.anchorMin = new Vector2(0.5f, 0f);
            barraPrompts.anchorMax = new Vector2(0.5f, 0f);
            barraPrompts.pivot = new Vector2(0.5f, 0f);
            barraPrompts.anchoredPosition = new Vector2(0f, 20f);
        }
    }

    private void ConfigurarRect(RectTransform rect, Vector2 anchor, Vector2 posicion, Vector2 size, Vector2 pivot)
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

    private void AjustarTexto(RectTransform parent, string nombre, float fontSize, Vector2 posicion, Vector2 size, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI texto = parent != null ? parent.Find(nombre)?.GetComponent<TextMeshProUGUI>() : null;
        RectTransform rect = texto != null ? texto.GetComponent<RectTransform>() : null;
        if (texto == null || rect == null)
        {
            return;
        }

        texto.fontSize = fontSize;
        texto.enableAutoSizing = false;
        texto.alignment = alignment;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = size;
    }

    private void CrearToast(RectTransform root)
    {
        if (root == null)
        {
            Debug.LogError("VidaEscolarHUD no pudo crear Toast: root nulo.");
            return;
        }

        GameObject toast = new GameObject(
            "Toast",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline),
            typeof(Shadow),
            typeof(CanvasGroup)
        );
        toast.transform.SetParent(root, false);

        RectTransform rt = toast.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 96f);
        rt.sizeDelta = new Vector2(500f, 64f);

        Image imagen = toast.GetComponent<Image>();
        imagen.color = new Color(0.018f, 0.014f, 0.05f, 0.90f);
        imagen.raycastTarget = false;

        Outline outline = toast.GetComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.55f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow sombra = toast.GetComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.42f);
        sombra.effectDistance = new Vector2(4f, -4f);

        toastGroup = toast.GetComponent<CanvasGroup>();
        if (toastGroup == null)
        {
            toastGroup = toast.AddComponent<CanvasGroup>();
        }

        if (toastGroup == null)
        {
            Debug.LogError("VidaEscolarHUD no pudo crear CanvasGroup para Toast.");
            return;
        }

        toastGroup.alpha = 0f;
        toastGroup.blocksRaycasts = false;
        textoToast = CrearTexto(toast.transform, "Texto", "", 20f, FontStyles.Bold, new Vector2(20f, -12f), new Vector2(460f, 40f), TextAlignmentOptions.Center);
        toast.SetActive(false);
    }

    private GameObject CrearPanel(string nombre, RectTransform parent, Vector2 posicion, Vector2 size, Vector2 anchor)
    {
        GameObject panel = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(Shadow), typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(anchor.x, anchor.y);
        rt.anchoredPosition = posicion;
        rt.sizeDelta = size;

        Image imagen = panel.GetComponent<Image>();
        imagen.color = new Color(0.018f, 0.014f, 0.05f, 0.86f);
        imagen.raycastTarget = false;

        Outline outline = panel.GetComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.48f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow sombra = panel.GetComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.42f);
        sombra.effectDistance = new Vector2(4f, -4f);

        CanvasGroup grupo = panel.GetComponent<CanvasGroup>();
        grupo.blocksRaycasts = false;
        if (Application.isPlaying && nombre != "Toast")
        {
            grupo.alpha = 0f;
            panel.transform.localScale = Vector3.one * 0.985f;
            grupo.DOFade(1f, 0.22f).SetEase(Ease.OutQuad).SetLink(panel);
            panel.transform.DOScale(1f, 0.22f).SetEase(Ease.OutBack).SetLink(panel);
        }
        return panel;
    }

    private void OnDestroy()
    {
        ansiedadTween?.Kill();
        if (toastGroup != null)
        {
            toastGroup.DOKill();
            toastGroup.transform.DOKill();
        }
    }

    private void DesactivarHudAnsiedadViejo()
    {
        AnxietyBarUI[] barras = FindObjectsByType<AnxietyBarUI>(FindObjectsInactive.Include);
        foreach (AnxietyBarUI barra in barras)
        {
            if (barra == null || barra.GetComponentInParent<VidaEscolarHUD>() != null)
            {
                continue;
            }

            if (barra.slider != null)
            {
                barra.slider.gameObject.SetActive(false);
            }

            barra.enabled = false;
            barra.gameObject.SetActive(false);
        }

        AnxietyBarStyler[] stylers = FindObjectsByType<AnxietyBarStyler>(FindObjectsInactive.Include);
        foreach (AnxietyBarStyler styler in stylers)
        {
            if (styler == null || styler.GetComponentInParent<VidaEscolarHUD>() != null)
            {
                continue;
            }

            if (styler.slider != null)
            {
                styler.slider.gameObject.SetActive(false);
            }
            if (styler.textoAnsiedad != null)
            {
                styler.textoAnsiedad.gameObject.SetActive(false);
            }
            if (styler.backgroundImage != null)
            {
                styler.backgroundImage.enabled = false;
            }

            styler.enabled = false;
            styler.gameObject.SetActive(false);
        }

        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include);
        foreach (Slider slider in sliders)
        {
            if (slider == null || slider.GetComponentInParent<VidaEscolarHUD>() != null)
            {
                continue;
            }

            string nombre = slider.gameObject.name.ToLowerInvariant();
            string padre = slider.transform.parent != null ? slider.transform.parent.name.ToLowerInvariant() : string.Empty;
            if (nombre.Contains("ansiedad") || padre.Contains("ansiedad") || slider.GetComponentInParent<AnxietyBarUI>() != null || slider.GetComponentInParent<AnxietyBarStyler>() != null)
            {
                slider.gameObject.SetActive(false);
            }
        }

        TextMeshProUGUI[] textos = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        foreach (TextMeshProUGUI texto in textos)
        {
            if (texto == null || texto.GetComponentInParent<VidaEscolarHUD>() != null)
            {
                continue;
            }

            string contenido = (texto.text + " " + texto.gameObject.name).ToLowerInvariant();
            if (contenido.Contains("ansiedad"))
            {
                texto.gameObject.SetActive(false);
            }
        }

        Text[] textosLegacy = FindObjectsByType<Text>(FindObjectsInactive.Include);
        foreach (Text texto in textosLegacy)
        {
            if (texto == null || texto.GetComponentInParent<VidaEscolarHUD>() != null)
            {
                continue;
            }

            string contenido = (texto.text + " " + texto.gameObject.name).ToLowerInvariant();
            if (contenido.Contains("ansiedad"))
            {
                texto.gameObject.SetActive(false);
            }
        }
    }

    private TextMeshProUGUI CrearTexto(Transform parent, string nombre, string texto, float size, FontStyles estilo, Vector2 posicion, Vector2 rectSize, TextAlignmentOptions alineacion)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = posicion;
        rt.sizeDelta = rectSize;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = size;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.fontStyle = estilo;
        tmp.color = new Color(0.92f, 0.98f, 1f);
        tmp.alignment = alineacion;
        tmp.raycastTarget = false;

        Shadow sombra = go.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.55f);
        sombra.effectDistance = new Vector2(2f, -2f);
        return tmp;
    }

    private string Recortar(string texto, int max)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return "Nueva tarea";
        }

        texto = texto.Replace("\n", " ").Trim();
        return texto.Length <= max ? texto : texto.Substring(0, max - 3) + "...";
    }
}
