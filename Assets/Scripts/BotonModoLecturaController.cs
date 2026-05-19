using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotonModoLecturaController : MonoBehaviour
{
    [SerializeField] private ControlCorreo controlCorreo;
    [SerializeField] private Image imagenBoton;
    [SerializeField] private Image imagenIconoLecturaFacil;
    [SerializeField] private Sprite spriteIconoLecturaFacil;
    [SerializeField] private TextMeshProUGUI textoBoton;

    private void Awake()
    {
        gameObject.name = "IconoLecturaFacil";
        CargarSpriteSiHaceFalta();

        if (controlCorreo == null)
        {
            controlCorreo = FindAnyObjectByType<ControlCorreo>();
        }

        if (imagenBoton == null)
        {
            imagenBoton = GetComponent<Image>();
        }

        if (textoBoton == null)
        {
            textoBoton = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (imagenBoton != null)
        {
            imagenBoton.color = EstiloUIJuego.FondoTarjeta;
        }

        PrepararIcono();

        if (textoBoton != null)
        {
            textoBoton.text = "Lectura facil";
            EstiloUIJuego.AplicarTexto(textoBoton, 8f, EstiloUIJuego.TextoPrincipal);

            RectTransform rectTexto = textoBoton.rectTransform;
            rectTexto.anchorMin = new Vector2(0f, 0f);
            rectTexto.anchorMax = new Vector2(1f, 0f);
            rectTexto.pivot = new Vector2(0.5f, 0f);
            rectTexto.anchoredPosition = new Vector2(0f, 2f);
            rectTexto.sizeDelta = new Vector2(-4f, 14f);
            textoBoton.gameObject.SetActive(spriteIconoLecturaFacil == null);
        }

        RectTransform rect = GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-82f, -18f);
            rect.sizeDelta = new Vector2(52f, 52f);
        }
    }

    private void CargarSpriteSiHaceFalta()
    {
        if (spriteIconoLecturaFacil == null)
        {
            spriteIconoLecturaFacil = RecursosVisualesEntryFilter.CargarSpriteEditor("modo_lectura.png");
        }
    }

    private void PrepararIcono()
    {
        if (imagenIconoLecturaFacil == null)
        {
            Transform existente = transform.Find("ImagenLecturaFacil");

            if (existente != null)
            {
                imagenIconoLecturaFacil = existente.GetComponent<Image>();
            }
        }

        if (imagenIconoLecturaFacil == null)
        {
            imagenIconoLecturaFacil = EstiloUIJuego.CrearImagen(
                transform,
                "ImagenLecturaFacil",
                Vector2.zero,
                new Vector2(40f, 40f),
                EstiloUIJuego.FondoSecundario
            );
        }

        imagenIconoLecturaFacil.sprite = spriteIconoLecturaFacil;
        imagenIconoLecturaFacil.preserveAspect = spriteIconoLecturaFacil != null;
        imagenIconoLecturaFacil.color = spriteIconoLecturaFacil != null
            ? Color.white
            : EstiloUIJuego.FondoSecundario;
        imagenIconoLecturaFacil.raycastTarget = false;

        RectTransform rectIcono = imagenIconoLecturaFacil.rectTransform;
        rectIcono.anchorMin = new Vector2(0.5f, 0.5f);
        rectIcono.anchorMax = new Vector2(0.5f, 0.5f);
        rectIcono.pivot = new Vector2(0.5f, 0.5f);
        rectIcono.anchoredPosition = Vector2.zero;
        rectIcono.sizeDelta = new Vector2(40f, 40f);

        Transform placeholder = imagenIconoLecturaFacil.transform.Find("Placeholder");

        if (spriteIconoLecturaFacil != null)
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
                imagenIconoLecturaFacil.transform,
                "Placeholder",
                "L",
                22f,
                Vector2.zero,
                new Vector2(34f, 34f),
                TextAlignmentOptions.Center
            );
        }
    }

    public void AlternarModoLectura()
    {
        if (controlCorreo != null)
        {
            controlCorreo.AlternarModoLectura();
        }
        else
        {
            Debug.Log("No se encontro ControlCorreo en la escena");
        }
    }
}
