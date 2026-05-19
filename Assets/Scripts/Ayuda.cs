using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelAyuda : MonoBehaviour
{
    public TMP_Text textoAyuda;
    [SerializeField] private Button botonAnterior;
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private TMP_Text textoIndicadorPagina;

    [TextArea(3,10)]
    public string[] mensajes;

    private int paginaActual = 0;
    private GestorVentanasPC gestorVentanas;
    private int ultimoFrameCambioPagina = -1;

    private void OnEnable()
    {
        paginaActual = 0;
        MostrarPagina();
    }

    void Start()
    {
        gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();
        AplicarEstilo();
        MostrarPagina();
    }

    public void SiguientePagina()
    {
        if (!PuedeProcesarClickPagina())
        {
            return;
        }

        if (mensajes == null || mensajes.Length == 0)
        {
            return;
        }

        if(paginaActual < mensajes.Length - 1)
        {
            paginaActual++;
            MostrarPagina();
        }
    }

    public void PaginaAnterior()
    {
        if (!PuedeProcesarClickPagina())
        {
            return;
        }

        if (mensajes == null || mensajes.Length == 0)
        {
            return;
        }

        if(paginaActual > 0)
        {
            paginaActual--;
            MostrarPagina();
        }
    }

    void MostrarPagina()
    {
        if (textoAyuda != null && mensajes != null && mensajes.Length > 0)
        {
            textoAyuda.text = mensajes[paginaActual];
        }

        if (textoIndicadorPagina != null && mensajes != null && mensajes.Length > 0)
        {
            textoIndicadorPagina.text = (paginaActual + 1) + " / " + mensajes.Length;
        }

        if (botonAnterior != null)
        {
            botonAnterior.interactable = paginaActual > 0;
        }

        if (botonSiguiente != null && mensajes != null)
        {
            botonSiguiente.interactable = paginaActual < mensajes.Length - 1;
        }
    }

    private bool PuedeProcesarClickPagina()
    {
        if (Time.frameCount == ultimoFrameCambioPagina)
        {
            return false;
        }

        ultimoFrameCambioPagina = Time.frameCount;
        return true;
    }

    public void Cerrar()
    {
        paginaActual = 0;
        MostrarPagina();

        if (gestorVentanas != null)
        {
            gestorVentanas.CerrarVentana(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void AplicarEstilo()
    {
        Image fondo = GetComponent<Image>();
        EstiloUIJuego.AplicarPanel(fondo, EstiloUIJuego.FondoPrincipal);
        AjustarVentana();
        PrepararTitulo();

        if (textoAyuda != null)
        {
            RectTransform rectTexto = textoAyuda.rectTransform;
            rectTexto.anchorMin = new Vector2(0.08f, 0.2f);
            rectTexto.anchorMax = new Vector2(0.92f, 0.82f);
            rectTexto.offsetMin = Vector2.zero;
            rectTexto.offsetMax = Vector2.zero;
            textoAyuda.margin = new Vector4(20f, 18f, 20f, 18f);
            textoAyuda.alignment = TextAlignmentOptions.TopLeft;
            EstiloUIJuego.AplicarTexto(textoAyuda, 24f, EstiloUIJuego.TextoPrincipal);
        }

        Button[] botones = GetComponentsInChildren<Button>(true);

        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] == null)
            {
                continue;
            }

            EstiloUIJuego.AplicarBoton(
                botones[i],
                EstiloUIJuego.FondoTarjeta,
                new Color(0.14f, 0.38f, 0.58f, 1f)
            );

            TMP_Text texto = botones[i].GetComponentInChildren<TMP_Text>(true);

            if (texto != null)
            {
                EstiloUIJuego.AplicarTexto(texto, 18f, EstiloUIJuego.TextoPrincipal);

                string etiqueta = texto.text.Trim();

                if (etiqueta.Contains("Atras") || etiqueta.Contains("Atr"))
                {
                    botonAnterior = botones[i];
                    AcomodarBotonNavegacion(botonAnterior, -120f);
                }
                else if (etiqueta.Contains("Siguiente"))
                {
                    botonSiguiente = botones[i];
                    AcomodarBotonNavegacion(botonSiguiente, 120f);
                }
            }
        }

        PrepararIndicadorPagina();
        ConectarBotonesNavegacion();
        PrepararBotonCerrar();
    }

    private void ConectarBotonesNavegacion()
    {
        if (botonAnterior != null)
        {
            botonAnterior.onClick.RemoveAllListeners();
            botonAnterior.onClick.AddListener(PaginaAnterior);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveAllListeners();
            botonSiguiente.onClick.AddListener(SiguientePagina);
        }
    }

    private void PrepararBotonCerrar()
    {
        Transform existente = transform.Find("CerrarAyuda");

        if (existente == null)
        {
            GameObject objeto = new GameObject(
                "CerrarAyuda",
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
            boton.onClick.AddListener(Cerrar);
            EstiloUIJuego.AplicarBoton(
                boton,
                new Color(0.62f, 0.16f, 0.22f, 1f),
                new Color(0.82f, 0.22f, 0.28f, 1f)
            );
        }
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
        rect.sizeDelta = new Vector2(720f, 440f);
    }

    private void PrepararTitulo()
    {
        if (transform.Find("TituloAyuda") != null)
        {
            return;
        }

        EstiloUIJuego.CrearTextoTMP(
            transform,
            "TituloAyuda",
            "Ayuda",
            30f,
            new Vector2(0f, 184f),
            new Vector2(240f, 36f),
            TextAlignmentOptions.Center
        );
    }

    private void PrepararIndicadorPagina()
    {
        if (textoIndicadorPagina == null)
        {
            Transform existente = transform.Find("IndicadorPagina");

            if (existente != null)
            {
                textoIndicadorPagina = existente.GetComponent<TMP_Text>();
            }
        }

        if (textoIndicadorPagina == null)
        {
            textoIndicadorPagina = EstiloUIJuego.CrearTextoTMP(
                transform,
                "IndicadorPagina",
                string.Empty,
                18f,
                new Vector2(0f, -184f),
                new Vector2(120f, 28f),
                TextAlignmentOptions.Center
            );
        }
    }

    private void AcomodarBotonNavegacion(Button boton, float posicionX)
    {
        RectTransform rect = boton != null ? boton.GetComponent<RectTransform>() : null;

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(posicionX, 18f);
        rect.sizeDelta = new Vector2(150f, 42f);
    }
}
