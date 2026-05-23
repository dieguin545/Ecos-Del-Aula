using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum AccionLogica
{
    Confirmar,
    Cancelar,
    Interactuar,
    Pausa,
    Navegar,
    SiguientePestana,
    AnteriorPestana,
    RevisarContexto,
    InteractuarF
}

[ExecuteAlways]
public class EcosAulaPromptUI : MonoBehaviour
{
    public AccionLogica accion;
    public string textoVerbo = "";

    private Image imagenPrincipal;
    private Image imagenOverlay;
    private TextMeshProUGUI componenteTexto;
    private TextMeshProUGUI textoFallbackIcono;

    private void Awake()
    {
        AsegurarComponentes();
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            GestorEntradaGlobal.AlCambiarDispositivo += ActualizarVisual;
            transform.localScale = Vector3.one;
        }
        ActualizarVisual(GestorEntradaGlobal.DispositivoActual);
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            GestorEntradaGlobal.AlCambiarDispositivo -= ActualizarVisual;
            transform.localScale = Vector3.one;
        }
    }

    private void MarcarSucio(UnityEngine.Object obj)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && obj != null)
        {
            UnityEditor.EditorUtility.SetDirty(obj);
        }
#endif
    }

    public void Configurar(AccionLogica nuevaAccion, string nuevoTexto)
    {
        accion = nuevaAccion;
        textoVerbo = nuevoTexto;
        AsegurarComponentes();
        ActualizarVisual(GestorEntradaGlobal.DispositivoActual);
        MarcarSucio(this);
    }

    private void AsegurarComponentes()
    {
        // 1. Imagen Principal (para el botón de control o fondo de tecla de teclado)
        Transform imgPrincipalTr = transform.Find("_IconoPrincipal");
        if (imgPrincipalTr == null)
        {
            GameObject go = new GameObject("_IconoPrincipal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            imgPrincipalTr = go.transform;
        }
        imagenPrincipal = imgPrincipalTr.GetComponent<Image>();
        imagenPrincipal.preserveAspect = true;
        imagenPrincipal.raycastTarget = false;

        // Configurar RectTransform de la imagen principal (anclada a la izquierda)
        RectTransform rtMain = imagenPrincipal.GetComponent<RectTransform>();
        rtMain.anchorMin = new Vector2(0f, 0.5f);
        rtMain.anchorMax = new Vector2(0f, 0.5f);
        rtMain.pivot = new Vector2(0f, 0.5f);
        rtMain.anchoredPosition = new Vector2(0f, 0f);
        rtMain.sizeDelta = new Vector2(40f, 40f);

        // 2. Imagen Overlay (para glifos de letras en el teclado, ej. 'E')
        Transform imgOverlayTr = imgPrincipalTr.Find("_IconoOverlay");
        if (imgOverlayTr == null)
        {
            GameObject go = new GameObject("_IconoOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(imgPrincipalTr, false);
            imgOverlayTr = go.transform;
        }
        imagenOverlay = imgOverlayTr.GetComponent<Image>();
        imagenOverlay.preserveAspect = true;
        imagenOverlay.raycastTarget = false;

        // Configurar RectTransform de la imagen overlay (centrada dentro de la principal, ligeramente más chica)
        RectTransform rtOverlay = imagenOverlay.GetComponent<RectTransform>();
        rtOverlay.anchorMin = new Vector2(0.5f, 0.5f);
        rtOverlay.anchorMax = new Vector2(0.5f, 0.5f);
        rtOverlay.pivot = new Vector2(0.5f, 0.5f);
        rtOverlay.anchoredPosition = new Vector2(0f, 1f); // Un pixel arriba para centrado visual en el keycap
        rtOverlay.sizeDelta = new Vector2(23f, 23f);

        Transform txtFallbackTr = imgPrincipalTr.Find("_TextoFallbackIcono");
        if (txtFallbackTr == null)
        {
            GameObject go = new GameObject("_TextoFallbackIcono", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(imgPrincipalTr, false);
            txtFallbackTr = go.transform;
        }
        textoFallbackIcono = txtFallbackTr.GetComponent<TextMeshProUGUI>();
        textoFallbackIcono.fontSize = 15f;
        textoFallbackIcono.color = Color.white;
        textoFallbackIcono.alignment = TextAlignmentOptions.Center;
        textoFallbackIcono.fontStyle = FontStyles.Bold;
        textoFallbackIcono.raycastTarget = false;

        RectTransform rtFallback = textoFallbackIcono.GetComponent<RectTransform>();
        rtFallback.anchorMin = Vector2.zero;
        rtFallback.anchorMax = Vector2.one;
        rtFallback.offsetMin = Vector2.zero;
        rtFallback.offsetMax = Vector2.zero;

        // 3. Texto del Verbo
        Transform txtTr = transform.Find("_TextoVerbo");
        if (txtTr == null)
        {
            GameObject go = new GameObject("_TextoVerbo", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            txtTr = go.transform;
        }
        componenteTexto = txtTr.GetComponent<TextMeshProUGUI>();
        componenteTexto.fontSize = 18f;
        componenteTexto.color = Color.white;
        componenteTexto.alignment = TextAlignmentOptions.MidlineLeft;
        componenteTexto.fontStyle = FontStyles.Bold;
        componenteTexto.raycastTarget = false;

        // Configurar RectTransform del texto (anclado a la derecha de la imagen principal, con ancho fijo para evitar squishing)
        RectTransform rtText = componenteTexto.GetComponent<RectTransform>();
        rtText.anchorMin = new Vector2(0f, 0f);
        rtText.anchorMax = new Vector2(0f, 1f);
        rtText.pivot = new Vector2(0f, 0.5f);
        rtText.anchoredPosition = new Vector2(48f, 0f);
        rtText.sizeDelta = new Vector2(158f, 0f);
    }

    private void ActualizarVisual(TipoDispositivoEntrada dispositivo)
    {
        if (imagenPrincipal == null || imagenOverlay == null || componenteTexto == null)
        {
            return;
        }

        componenteTexto.text = textoVerbo;
        imagenOverlay.gameObject.SetActive(false);
        if (textoFallbackIcono != null)
        {
            textoFallbackIcono.gameObject.SetActive(false);
        }

        if (dispositivo == TipoDispositivoEntrada.ControlXbox)
        {
            int idxXbox = ObtenerIndexXbox(accion);
            Sprite sprite = EcosAulaSpriteLoader.ObtenerSpriteXbox(idxXbox);
            if (sprite != null)
            {
                AplicarSprite(sprite);
            }
            else
            {
                AplicarFallback(ObtenerTextoFallback(dispositivo, accion), new Color(0.02f, 0.16f, 0.34f, 0.95f));
            }
        }
        else // TecladoMouse
        {
            int idxTeclado = ObtenerIndexTeclado(accion);
            bool esLetraOverlay = RequiereOverlayTeclado(accion, out int idxOverlay);

            Sprite spritePrincipal = EcosAulaSpriteLoader.ObtenerSpriteTeclado(idxTeclado);
            if (spritePrincipal != null)
            {
                AplicarSprite(spritePrincipal);

                if (esLetraOverlay)
                {
                    Sprite spriteOverlay = EcosAulaSpriteLoader.ObtenerSpriteTeclado(idxOverlay);
                    if (spriteOverlay != null)
                    {
                        imagenOverlay.sprite = spriteOverlay;
                        imagenOverlay.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                AplicarFallback(ObtenerTextoFallback(dispositivo, accion), new Color(0.12f, 0.12f, 0.20f, 0.95f));
            }
        }
    }

    private void AplicarSprite(Sprite sprite)
    {
        imagenPrincipal.sprite = sprite;
        imagenPrincipal.color = Color.white;
        imagenPrincipal.gameObject.SetActive(true);
    }

    private void AplicarFallback(string texto, Color colorFondo)
    {
        imagenPrincipal.sprite = null;
        imagenPrincipal.color = colorFondo;
        imagenPrincipal.gameObject.SetActive(true);
        imagenOverlay.gameObject.SetActive(false);

        if (textoFallbackIcono != null)
        {
            textoFallbackIcono.text = texto;
            textoFallbackIcono.gameObject.SetActive(true);
        }
    }

    private int ObtenerIndexXboxCorregido(AccionLogica act)
    {
        switch (act)
        {
            case AccionLogica.Confirmar: return 130; // A
            case AccionLogica.Cancelar: return 162; // B
            case AccionLogica.Interactuar: return 130; // A
            case AccionLogica.Pausa: return 100; // Menu/Start
            case AccionLogica.Navegar: return 117; // D-Pad
            case AccionLogica.SiguientePestana: return 392; // RB
            case AccionLogica.AnteriorPestana: return 370; // LB
            case AccionLogica.RevisarContexto: return 120; // X
            case AccionLogica.InteractuarF: return 130; // A
            default: return 130;
        }
    }

    private int ObtenerIndexTecladoCorregido(AccionLogica act)
    {
        switch (act)
        {
            case AccionLogica.Confirmar: return 30; // Enter
            case AccionLogica.Cancelar: return 0; // Esc
            case AccionLogica.Interactuar: return 26; // Keycap para E
            case AccionLogica.Pausa: return 0; // Esc
            case AccionLogica.Navegar: return 45; // Flechas
            case AccionLogica.SiguientePestana: return 26; // Keycap para E
            case AccionLogica.AnteriorPestana: return 26; // Keycap para Q
            case AccionLogica.RevisarContexto: return 26; // Keycap para R
            case AccionLogica.InteractuarF: return 26; // Keycap para F
            default: return 26;
        }
    }

    private string ObtenerTextoFallback(TipoDispositivoEntrada dispositivo, AccionLogica act)
    {
        if (dispositivo == TipoDispositivoEntrada.ControlXbox)
        {
            switch (act)
            {
                case AccionLogica.Confirmar: return "A";
                case AccionLogica.Cancelar: return "B";
                case AccionLogica.Interactuar: return "A";
                case AccionLogica.Pausa: return "Menu";
                case AccionLogica.Navegar: return "D";
                case AccionLogica.SiguientePestana: return "RB";
                case AccionLogica.AnteriorPestana: return "LB";
                case AccionLogica.RevisarContexto: return "X";
                case AccionLogica.InteractuarF: return "A";
            }
        }

        switch (act)
        {
            case AccionLogica.Confirmar: return "Ent";
            case AccionLogica.Cancelar: return "Esc";
            case AccionLogica.Interactuar: return "E";
            case AccionLogica.Pausa: return "Esc";
            case AccionLogica.Navegar: return "Nav";
            case AccionLogica.SiguientePestana: return "E";
            case AccionLogica.AnteriorPestana: return "Q";
            case AccionLogica.RevisarContexto: return "R";
            case AccionLogica.InteractuarF: return "F";
            default: return "?";
        }
    }

    private int ObtenerIndexXbox(AccionLogica act)
    {
        int indiceCorregido = ObtenerIndexXboxCorregido(act);
        if (indiceCorregido >= 0) return indiceCorregido;

        return 130;
    }

    private int ObtenerIndexTeclado(AccionLogica act)
    {
        int indiceCorregido = ObtenerIndexTecladoCorregido(act);
        if (indiceCorregido >= 0) return indiceCorregido;

        return 26;
    }

    private bool RequiereOverlayTeclado(AccionLogica act, out int idxOverlay)
    {
        idxOverlay = -1;
        if (act == AccionLogica.InteractuarF)
        {
            idxOverlay = 106; // Letra F
            return true;
        }

        switch (act)
        {
            case AccionLogica.Interactuar:
                idxOverlay = 103; // Letra E
                return true;
            case AccionLogica.SiguientePestana:
                idxOverlay = 103; // Letra E
                return true;
            case AccionLogica.AnteriorPestana:
                idxOverlay = 99;  // Letra Q
                return true;
            case AccionLogica.RevisarContexto:
                idxOverlay = 105; // Letra R
                return true;
            case AccionLogica.InteractuarF:
                idxOverlay = 104; // Letra F
                return true;
            default:
                return false;
        }
    }

    // ─── Métodos Estáticos de Inyección ──────────────────────────────────────────

    public static EcosAulaPromptUI InyectarEn(GameObject destino, AccionLogica accion, string verbo)
    {
        if (destino == null) return null;

        // Desactivar otros componentes de texto para que no se superpongan
        var textos = destino.GetComponents<TextMeshProUGUI>();
        foreach (var t in textos)
        {
            if (t.gameObject.name != "_TextoVerbo") t.enabled = false;
        }
        var textosLegacy = destino.GetComponents<Text>();
        foreach (var t in textosLegacy)
        {
            if (t.gameObject.name != "_TextoVerbo") t.enabled = false;
        }

        // Desactivar textos en hijos que no sean de nuestro prompt
        for (int i = 0; i < destino.transform.childCount; i++)
        {
            Transform hijo = destino.transform.GetChild(i);
            if (hijo.name != "_IconoPrincipal" && hijo.name != "_TextoVerbo")
            {
                var hTxt = hijo.GetComponent<TextMeshProUGUI>();
                if (hTxt != null) hTxt.enabled = false;
                var hTxtLegacy = hijo.GetComponent<Text>();
                if (hTxtLegacy != null) hTxtLegacy.enabled = false;
            }
        }

        EcosAulaPromptUI prompt = destino.GetComponent<EcosAulaPromptUI>();
        if (prompt == null)
        {
            prompt = destino.AddComponent<EcosAulaPromptUI>();
        }
        prompt.Configurar(accion, verbo);
        return prompt;
    }

    public static GameObject CrearBarraPrompts(Transform parent, params (AccionLogica accion, string verbo)[] listado)
    {
        if (parent == null) return null;

        // Si ya existen barras, destruirlas para rehacer una sola fila limpia.
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform hijo = parent.GetChild(i);
            if (hijo != null && hijo.name == "_BarraPrompts")
            {
                if (Application.isPlaying)
                {
                    Destroy(hijo.gameObject);
                }
                else
                {
                    DestroyImmediate(hijo.gameObject);
                }
            }
        }

        GameObject barra = new GameObject("_BarraPrompts", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barra.transform.SetParent(parent, false);

        RectTransform rt = barra.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(24f, 14f);
        rt.sizeDelta = new Vector2(0f, 54f);

        Image fondo = barra.GetComponent<Image>();
        fondo.color = new Color(0.025f, 0.018f, 0.05f, 0.72f);
        fondo.raycastTarget = false;

        // Agregar HorizontalLayoutGroup para alinear los prompts
        HorizontalLayoutGroup layout = barra.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 18f;
        layout.padding = new RectOffset(16, 16, 6, 6);

        ContentSizeFitter fitter = barra.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (var p in listado)
        {
            GameObject promptGo = new GameObject("_PromptItem_" + p.accion, typeof(RectTransform));
            promptGo.transform.SetParent(barra.transform, false);

            RectTransform rtItem = promptGo.GetComponent<RectTransform>();
            rtItem.sizeDelta = new Vector2(214f, 42f);

            EcosAulaPromptUI promptComp = promptGo.AddComponent<EcosAulaPromptUI>();
            promptComp.Configurar(p.accion, p.verbo);
        }

        return barra;
    }
}
