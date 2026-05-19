using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class EstiloUIJuego
{
    public static readonly Color FondoPrincipal = new Color(0.11f, 0.05f, 0.2f, 0.97f);
    public static readonly Color FondoSecundario = new Color(0.16f, 0.08f, 0.28f, 0.98f);
    public static readonly Color FondoTarjeta = new Color(0.18f, 0.1f, 0.3f, 0.98f);
    public static readonly Color Acento = new Color(0.44f, 0.94f, 0.73f, 1f);
    public static readonly Color AcentoCalido = new Color(1f, 0.72f, 0.25f, 1f);
    public static readonly Color TextoPrincipal = new Color(0.98f, 0.98f, 1f, 1f);
    public static readonly Color TextoSecundario = new Color(0.86f, 0.86f, 0.96f, 1f);
    public static readonly Color Peligro = new Color(0.98f, 0.42f, 0.35f, 1f);

    public static void AplicarPanel(Image imagen, Color color)
    {
        if (imagen == null)
        {
            return;
        }

        imagen.color = color;
    }

    public static void AplicarBoton(Button boton, Color normal, Color resaltado)
    {
        if (boton == null)
        {
            return;
        }

        Image imagen = boton.GetComponent<Image>();

        if (imagen != null)
        {
            imagen.color = normal;
        }

        ColorBlock colores = boton.colors;
        colores.normalColor = normal;
        colores.highlightedColor = resaltado;
        colores.selectedColor = resaltado;
        colores.pressedColor = Color.Lerp(normal, Color.black, 0.25f);
        colores.disabledColor = new Color(normal.r, normal.g, normal.b, 0.35f);
        boton.colors = colores;
    }

    public static void AplicarTexto(Text texto, int tamano, Color color)
    {
        if (texto == null)
        {
            return;
        }

        texto.fontSize = tamano;
        texto.color = color;
    }

    public static void AplicarTexto(TMP_Text texto, float tamano, Color color)
    {
        if (texto == null)
        {
            return;
        }

        texto.fontSize = tamano;
        texto.color = color;
    }

    public static TextMeshProUGUI CrearTextoTMP(
        Transform padre,
        string nombre,
        string contenido,
        float tamano,
        Vector2 posicion,
        Vector2 tamanoRect,
        TextAlignmentOptions alineacion
    )
    {
        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamanoRect;

        TextMeshProUGUI texto = objeto.GetComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = TextoPrincipal;
        texto.raycastTarget = false;
        texto.textWrappingMode = TextWrappingModes.Normal;

        return texto;
    }

    public static Image CrearImagen(
        Transform padre,
        string nombre,
        Vector2 posicion,
        Vector2 tamanoRect,
        Color color
    )
    {
        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamanoRect;

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        return imagen;
    }
}
