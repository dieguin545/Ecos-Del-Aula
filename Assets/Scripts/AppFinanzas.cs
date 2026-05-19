using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AppFinanzas : MonoBehaviour
{
    public ControlCorreo controlCorreo;

    public Text textoDineroTotal;
    public Text textoPromedio;
    public Text textoUltimoPago;
    public Text textoDiasTrabajados;
    [SerializeField] private Image imagenIconoFinanzas;
    [SerializeField] private Sprite spriteIconoFinanzas;
    [SerializeField] private Image imagenDecorativaFinanzas;
    [SerializeField] private Sprite spriteImagenDecorativaFinanzas;
    private bool uiPreparada;

    void OnEnable()
    {
        CargarSpritesSiHaceFalta();
        PrepararUiSiHaceFalta();
        ActualizarFinanzas();
    }

    public void ActualizarFinanzas()
    {
        if (controlCorreo == null)
        {
            return;
        }

        if (textoDineroTotal != null) textoDineroTotal.text = "Saldo total: $" + controlCorreo.dineroTotal;
        if (textoUltimoPago != null) textoUltimoPago.text = "Último pago: $" + controlCorreo.ultimoSueldo;
        if (textoDiasTrabajados != null) textoDiasTrabajados.text = "Días trabajados: " + controlCorreo.diasTrabajados;

        if (textoUltimoPago != null) textoUltimoPago.text = "Ultimo pago: $" + controlCorreo.ultimoSueldo;
        if (textoDiasTrabajados != null) textoDiasTrabajados.text = "Dias trabajados: " + controlCorreo.diasTrabajados;

        float promedio = 0;

        if (controlCorreo.diasTrabajados > 0)
        {
            promedio = (float)controlCorreo.dineroTotal / controlCorreo.diasTrabajados;
        }

        if (textoPromedio != null) textoPromedio.text = "Promedio diario: $" + promedio.ToString("0.0");
    }

    private void AplicarEstilo()
    {
        Image fondo = GetComponent<Image>();
        EstiloUIJuego.AplicarPanel(fondo, EstiloUIJuego.FondoPrincipal);

        EstiloUIJuego.AplicarTexto(textoDineroTotal, 22, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoPromedio, 22, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoUltimoPago, 22, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoDiasTrabajados, 22, EstiloUIJuego.TextoPrincipal);
    }

    private void CargarSpritesSiHaceFalta()
    {
        if (spriteIconoFinanzas == null)
        {
            spriteIconoFinanzas = RecursosVisualesEntryFilter.CargarSpriteEditor("logo_finanzas.png");
        }

        if (spriteImagenDecorativaFinanzas == null)
        {
            spriteImagenDecorativaFinanzas = RecursosVisualesEntryFilter.CargarSpriteEditor("finanzas_fondo.png");
        }
    }

    private void PrepararUiSiHaceFalta()
    {
        if (uiPreparada)
        {
            return;
        }

        AjustarVentana();
        AplicarEstilo();
        PrepararTitulo();
        PrepararIcono();
        PrepararImagenDecorativa();
        NeutralizarFondosGrisesLegacy();
        PrepararBotonCerrar();
        PrepararTarjeta("TarjetaSaldo", textoDineroTotal, new Vector2(-155f, 72f));
        PrepararTarjeta("TarjetaDias", textoDiasTrabajados, new Vector2(155f, 72f));
        PrepararTarjeta("TarjetaPago", textoUltimoPago, new Vector2(-155f, -58f));
        PrepararTarjeta("TarjetaPromedio", textoPromedio, new Vector2(155f, -58f));
        uiPreparada = true;
    }

    private void AjustarVentana()
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 18f);
        rect.sizeDelta = new Vector2(700f, 390f);
    }

    private void PrepararTitulo()
    {
        TMP_Text titulo = transform.Find("TituloFinanzas")?.GetComponent<TMP_Text>();

        if (titulo != null)
        {
            titulo.text = "Finanzas";
            EstiloUIJuego.AplicarTexto(titulo, 30f, EstiloUIJuego.TextoPrincipal);
        }
    }

    private void PrepararIcono()
    {
        if (imagenIconoFinanzas == null)
        {
            Transform existente = transform.Find("IconoFinanzasApp");

            if (existente != null)
            {
                imagenIconoFinanzas = existente.GetComponent<Image>();
            }
        }

        if (imagenIconoFinanzas == null)
        {
            imagenIconoFinanzas = EstiloUIJuego.CrearImagen(
                transform,
                "IconoFinanzasApp",
                new Vector2(-292f, 148f),
                new Vector2(46f, 46f),
                EstiloUIJuego.FondoTarjeta
            );
        }

        imagenIconoFinanzas.sprite = spriteIconoFinanzas;
        imagenIconoFinanzas.preserveAspect = spriteIconoFinanzas != null;
        imagenIconoFinanzas.color = spriteIconoFinanzas != null
            ? Color.white
            : EstiloUIJuego.FondoTarjeta;
        imagenIconoFinanzas.raycastTarget = false;

        Transform placeholder = imagenIconoFinanzas.transform.Find("Placeholder");

        if (spriteIconoFinanzas != null)
        {
            if (placeholder != null)
            {
                placeholder.gameObject.SetActive(false);
            }

            return;
        }

        if (placeholder == null)
        {
            EstiloUIJuego.CrearTextoTMP(
                imagenIconoFinanzas.transform,
                "Placeholder",
                "F",
                24f,
                Vector2.zero,
                new Vector2(46f, 46f),
                TextAlignmentOptions.Center
            );
        }
    }

    private void PrepararImagenDecorativa()
    {
        if (imagenDecorativaFinanzas == null)
        {
            Transform existente = transform.Find("ImagenDecorativaFinanzas");

            if (existente != null)
            {
                imagenDecorativaFinanzas = existente.GetComponent<Image>();
            }
        }

        if (imagenDecorativaFinanzas == null)
        {
            imagenDecorativaFinanzas = EstiloUIJuego.CrearImagen(
                transform,
                "ImagenDecorativaFinanzas",
                new Vector2(0f, -8f),
                new Vector2(420f, 280f),
                EstiloUIJuego.FondoSecundario
            );
        }

        imagenDecorativaFinanzas.sprite = spriteImagenDecorativaFinanzas;
        imagenDecorativaFinanzas.color = spriteImagenDecorativaFinanzas != null
            ? new Color(1f, 1f, 1f, 0.38f)
            : EstiloUIJuego.FondoSecundario;
        imagenDecorativaFinanzas.preserveAspect = spriteImagenDecorativaFinanzas != null;
        imagenDecorativaFinanzas.raycastTarget = false;
        imagenDecorativaFinanzas.transform.SetAsFirstSibling();
    }

    private void NeutralizarFondosGrisesLegacy()
    {
        Image[] imagenes = GetComponentsInChildren<Image>(true);

        for (int i = 0; i < imagenes.Length; i++)
        {
            Image imagen = imagenes[i];

            if (imagen == null || imagen == GetComponent<Image>() || imagen == imagenDecorativaFinanzas)
            {
                continue;
            }

            if (imagen.transform.name.StartsWith("Tarjeta") || imagen.transform.name.Contains("Cerrar") || imagen == imagenIconoFinanzas)
            {
                continue;
            }

            Color color = imagen.color;
            float brillo = (color.r + color.g + color.b) / 3f;

            if (brillo > 0.55f)
            {
                imagen.color = EstiloUIJuego.FondoSecundario;
            }
        }
    }

    private void PrepararBotonCerrar()
    {
        Transform existente = transform.Find("CerrarFinanzas");

        if (existente == null)
        {
            GameObject objeto = new GameObject(
                "CerrarFinanzas",
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

        Button botonCerrar = existente.GetComponent<Button>();
        RectTransform rect = existente.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(38f, 38f);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(CerrarVentanaActual);
            EstiloUIJuego.AplicarBoton(
                botonCerrar,
                new Color(0.62f, 0.16f, 0.22f, 1f),
                new Color(0.82f, 0.22f, 0.28f, 1f)
            );
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

    private void PrepararTarjeta(string nombre, Text texto, Vector2 posicion)
    {
        if (texto == null)
        {
            return;
        }

        Transform existente = transform.Find(nombre);
        Image tarjeta = existente != null
            ? existente.GetComponent<Image>()
            : EstiloUIJuego.CrearImagen(
                transform,
                nombre,
                posicion,
                new Vector2(260f, 102f),
                EstiloUIJuego.FondoTarjeta
            );

        if (tarjeta == null)
        {
            return;
        }

        RectTransform rectTarjeta = tarjeta.rectTransform;
        rectTarjeta.anchoredPosition = posicion;
        rectTarjeta.sizeDelta = new Vector2(260f, 102f);
        texto.transform.SetParent(tarjeta.transform, false);

        RectTransform rectTexto = texto.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = new Vector2(18f, 12f);
        rectTexto.offsetMax = new Vector2(-18f, -12f);
        texto.alignment = TextAnchor.MiddleLeft;
    }
}
