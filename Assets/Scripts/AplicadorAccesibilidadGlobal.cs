using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AplicadorAccesibilidadGlobal : MonoBehaviour
{
    private static AplicadorAccesibilidadGlobal instancia;
    private readonly Dictionary<TextMeshProUGUI, float> tamanosBaseTMP = new Dictionary<TextMeshProUGUI, float>();
    private readonly Dictionary<Text, int> tamanosBaseText = new Dictionary<Text, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
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
                ? baseSize * 1.12f
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
                ? Mathf.RoundToInt(baseSize * 1.12f)
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
            return new Color(0f, 0f, 0f, Mathf.Max(alpha, 0.85f));
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
            return new Color(0.02f, 0.02f, 0.02f, 1f);
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
}
