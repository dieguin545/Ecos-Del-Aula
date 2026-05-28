using System.Collections.Generic;
using ColorblindFilter.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AplicadorAccesibilidadGlobal : MonoBehaviour
{
    private const string NombreOverlayDaltonismo = "OverlayDaltonismoGlobal";
    private static AplicadorAccesibilidadGlobal instancia;
    private readonly Dictionary<TextMeshProUGUI, float> tamanosBaseTMP = new Dictionary<TextMeshProUGUI, float>();
    private readonly Dictionary<Text, int> tamanosBaseText = new Dictionary<Text, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        // Forzar pantalla completa al iniciar el juego
        Screen.fullScreen = true;

        if (instancia != null)
        {
            return;
        }

        AplicadorAccesibilidadGlobal existente = FindAnyObjectByType<AplicadorAccesibilidadGlobal>();

        if (existente != null)
        {
            instancia = existente;
            DontDestroyOnLoad(existente.gameObject);
            instancia.AplicarEscena();
            return;
        }

        GameObject objeto = new GameObject("AplicadorAccesibilidadGlobal");
        instancia = objeto.AddComponent<AplicadorAccesibilidadGlobal>();
        DontDestroyOnLoad(objeto);
        instancia.AplicarEscena();
    }

    public static void AplicarEscenaActual()
    {
        if (instancia == null)
        {
            CrearSiHaceFalta();
        }

        if (instancia != null)
        {
            instancia.AplicarEscena();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        AplicarEscena();
    }

    private void AplicarEscena()
    {
        AplicarTextosTMP();
        AplicarTextosLegacy();
        AplicarImagenes();
        AplicarBotones();
        AplicarFiltroDaltonismoCamara();
        AplicarOverlayDaltonismoUI();
    }

    private void AplicarTextosTMP()
    {
        TextMeshProUGUI[] textos = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);

        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] == null)
            {
                continue;
            }

            textos[i].color = ObtenerColorTexto();

            if (!tamanosBaseTMP.ContainsKey(textos[i]))
            {
                tamanosBaseTMP[textos[i]] = textos[i].fontSize;
            }

            float baseSize = tamanosBaseTMP[textos[i]];
            textos[i].fontSize = ConfiguracionAccesibilidadJuego.TextoGrandeActivo
                ? baseSize * 1.08f
                : baseSize;
        }
    }

    private void AplicarTextosLegacy()
    {
        Text[] textos = FindObjectsByType<Text>(FindObjectsInactive.Include);

        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] == null)
            {
                continue;
            }

            textos[i].color = ObtenerColorTexto();

            if (!tamanosBaseText.ContainsKey(textos[i]))
            {
                tamanosBaseText[textos[i]] = textos[i].fontSize;
            }

            int baseSize = tamanosBaseText[textos[i]];
            textos[i].fontSize = ConfiguracionAccesibilidadJuego.TextoGrandeActivo
                ? Mathf.RoundToInt(baseSize * 1.08f)
                : baseSize;
        }
    }

    private void AplicarImagenes()
    {
        Image[] imagenes = FindObjectsByType<Image>(FindObjectsInactive.Include);

        for (int i = 0; i < imagenes.Length; i++)
        {
            Image imagen = imagenes[i];

            if (imagen == null || imagen.sprite != null)
            {
                continue;
            }

            // Only apply high-contrast panel coloring to actual large containers/panels
            // to avoid turning small outlines, slots, or decorative elements into black blocks.
            RectTransform rect = imagen.GetComponent<RectTransform>();
            if (rect != null && (rect.rect.width < 100f || rect.rect.height < 100f))
            {
                continue;
            }

            float alpha = imagen.color.a;
            imagen.color = ObtenerColorPanel(alpha);
        }
    }

    private void AplicarBotones()
    {
        Button[] botones = FindObjectsByType<Button>(FindObjectsInactive.Include);

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];

            if (boton == null)
            {
                continue;
            }

            Image imagenBoton = boton.GetComponent<Image>();

            if (imagenBoton != null && imagenBoton.sprite != null)
            {
                imagenBoton.color = Color.white;
                imagenBoton.preserveAspect = true;
                continue;
            }

            Color normal = ObtenerColorBoton();
            Color resaltado = ConfiguracionAccesibilidadJuego.AltoContrasteActivo
                ? Color.white
                : EstiloUIJuego.Acento;
            EstiloUIJuego.AplicarBoton(boton, normal, resaltado);
        }
    }

    private Color ObtenerColorTexto()
    {
        if (ConfiguracionAccesibilidadJuego.AltoContrasteActivo ||
            ConfiguracionAccesibilidadJuego.TipoDaltonismoActual == TipoDaltonismo.Acromatopsia)
        {
            return Color.white;
        }

        switch (ConfiguracionAccesibilidadJuego.TipoDaltonismoActual)
        {
            case TipoDaltonismo.Protanopia:
                return new Color(0.95f, 0.95f, 1f, 1f);
            case TipoDaltonismo.Deuteranopia:
                return new Color(1f, 0.95f, 0.78f, 1f);
            case TipoDaltonismo.Tritanopia:
                return new Color(1f, 0.92f, 0.96f, 1f);
            default:
                return EstiloUIJuego.TextoPrincipal;
        }
    }

    private Color ObtenerColorPanel(float alpha)
    {
        if (ConfiguracionAccesibilidadJuego.AltoContrasteActivo ||
            ConfiguracionAccesibilidadJuego.TipoDaltonismoActual == TipoDaltonismo.Acromatopsia)
        {
            return new Color(0.03f, 0.03f, 0.06f, 0.95f); // Charcoal-navy elegante de alto contraste
        }

        switch (ConfiguracionAccesibilidadJuego.TipoDaltonismoActual)
        {
            case TipoDaltonismo.Protanopia:
                return new Color(0.04f, 0.08f, 0.18f, Mathf.Max(alpha, 0.82f));
            case TipoDaltonismo.Deuteranopia:
                return new Color(0.09f, 0.06f, 0.18f, Mathf.Max(alpha, 0.82f));
            case TipoDaltonismo.Tritanopia:
                return new Color(0.14f, 0.04f, 0.14f, Mathf.Max(alpha, 0.82f));
            default:
                return new Color(EstiloUIJuego.FondoPrincipal.r, EstiloUIJuego.FondoPrincipal.g, EstiloUIJuego.FondoPrincipal.b, alpha);
        }
    }

    private Color ObtenerColorBoton()
    {
        if (ConfiguracionAccesibilidadJuego.AltoContrasteActivo ||
            ConfiguracionAccesibilidadJuego.TipoDaltonismoActual == TipoDaltonismo.Acromatopsia)
        {
            return new Color(0.12f, 0.12f, 0.16f, 1f); // Charcoal-grey de alto contraste
        }

        switch (ConfiguracionAccesibilidadJuego.TipoDaltonismoActual)
        {
            case TipoDaltonismo.Protanopia:
                return new Color(0.04f, 0.28f, 0.48f, 1f);
            case TipoDaltonismo.Deuteranopia:
                return new Color(0.32f, 0.22f, 0.08f, 1f);
            case TipoDaltonismo.Tritanopia:
                return new Color(0.38f, 0.08f, 0.32f, 1f);
            default:
                return new Color(0.06f, 0.19f, 0.32f, 1f);
        }
    }

    private void AplicarFiltroDaltonismoCamara()
    {
        TipoDaltonismo tipo = ConfiguracionAccesibilidadJuego.TipoDaltonismoActual;
        Camera[] camaras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude);

        for (int i = 0; i < camaras.Length; i++)
        {
            Camera camara = camaras[i];

            if (camara == null)
            {
                continue;
            }

            ColorblindFilter.Scripts.ColorblindFilter filtro =
                camara.GetComponent<ColorblindFilter.Scripts.ColorblindFilter>();

            if (filtro == null && tipo != TipoDaltonismo.Ninguno)
            {
                filtro = camara.gameObject.AddComponent<ColorblindFilter.Scripts.ColorblindFilter>();
            }

            if (filtro == null)
            {
                continue;
            }

            filtro.SetUseFilter(tipo != TipoDaltonismo.Ninguno);

            switch (tipo)
            {
                case TipoDaltonismo.Protanopia:
                    filtro.ChangeBlindType(BlindnessType.Protanopia);
                    break;
                case TipoDaltonismo.Deuteranopia:
                    filtro.ChangeBlindType(BlindnessType.Deuteranopia);
                    break;
                case TipoDaltonismo.Tritanopia:
                    filtro.ChangeBlindType(BlindnessType.Tritanopia);
                    break;
                case TipoDaltonismo.Acromatopsia:
                    filtro.ChangeBlindType(BlindnessType.Achromatopsia);
                    break;
            }
        }
    }

    private void AplicarOverlayDaltonismoUI()
    {
        Color colorOverlay = ObtenerColorOverlayDaltonismo();
        bool debeMostrar = colorOverlay.a > 0.01f;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);

        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];

            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
            {
                continue;
            }

            Transform existente = canvas.transform.Find(NombreOverlayDaltonismo);
            GameObject overlay = existente != null ? existente.gameObject : CrearOverlayDaltonismo(canvas.transform);

            if (overlay == null)
            {
                continue;
            }

            Image imagen = overlay.GetComponent<Image>();

            if (imagen != null)
            {
                imagen.color = colorOverlay;
                imagen.raycastTarget = false;
            }

            overlay.SetActive(debeMostrar);
            overlay.transform.SetAsLastSibling();
        }
    }

    private GameObject CrearOverlayDaltonismo(Transform padre)
    {
        GameObject overlay = new GameObject(NombreOverlayDaltonismo, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(padre, false);

        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image imagen = overlay.GetComponent<Image>();
        imagen.raycastTarget = false;

        return overlay;
    }

    private Color ObtenerColorOverlayDaltonismo()
    {
        TipoDaltonismo tipo = ConfiguracionAccesibilidadJuego.TipoDaltonismoActual;

        if (ConfiguracionAccesibilidadJuego.AltoContrasteActivo)
        {
            return new Color(0f, 0f, 0f, 0.12f);
        }

        switch (tipo)
        {
            case TipoDaltonismo.Protanopia:
                return new Color(0.02f, 0.12f, 0.34f, 0.16f);
            case TipoDaltonismo.Deuteranopia:
                return new Color(0.34f, 0.22f, 0.04f, 0.15f);
            case TipoDaltonismo.Tritanopia:
                return new Color(0.36f, 0.04f, 0.28f, 0.15f);
            case TipoDaltonismo.Acromatopsia:
                return new Color(0f, 0f, 0f, 0.24f);
            default:
                return new Color(0f, 0f, 0f, 0f);
        }
    }
}
