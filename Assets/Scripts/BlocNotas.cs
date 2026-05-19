using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlocNotas : MonoBehaviour
{
    public GameObject panel;
    public InputField campoNotas;

    private const string SAVE_KEY = "briv_notas";
    private GestorVentanasPC gestorVentanas;

    void Start()
    {
        gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (panel != null)
        {
            AplicarEstilo();
            panel.SetActive(false);
        }
    }

    public void Abrir()
    {
        if (panel == null) return;

        if (gestorVentanas == null)
        {
            gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();
        }

        if (gestorVentanas != null)
        {
            gestorVentanas.AlternarVentana(panel);
        }
        else
        {
            panel.SetActive(!panel.activeSelf);
        }

        if (panel.activeSelf && campoNotas != null)
            campoNotas.text = PlayerPrefs.GetString(SAVE_KEY, "");
    }

    public void Cerrar()
    {
        Guardar();

        if (gestorVentanas != null)
        {
            gestorVentanas.CerrarVentana(panel);
        }
        else if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Guardar()
    {
        if (campoNotas != null)
        {
            PlayerPrefs.SetString(SAVE_KEY, campoNotas.text);
            PlayerPrefs.Save();
        }
    }

    private void AplicarEstilo()
    {
        Image fondo = panel.GetComponent<Image>();
        EstiloUIJuego.AplicarPanel(fondo, EstiloUIJuego.FondoPrincipal);
        AjustarVentana();

        TMP_Text titulo = panel.GetComponentInChildren<TMP_Text>(true);
        EstiloUIJuego.AplicarTexto(titulo, 28f, EstiloUIJuego.TextoPrincipal);
        PrepararBotonCerrar();

        if (campoNotas != null)
        {
            Image fondoCampo = campoNotas.GetComponent<Image>();
            EstiloUIJuego.AplicarPanel(fondoCampo, new Color(0.94f, 0.96f, 1f, 1f));
            EstiloUIJuego.AplicarTexto(campoNotas.textComponent, 18, new Color(0.12f, 0.08f, 0.2f, 1f));

            if (campoNotas.placeholder is Text textoPlaceholder)
            {
                textoPlaceholder.text = "Escribe tus notas aqui...";
                EstiloUIJuego.AplicarTexto(textoPlaceholder, 18, new Color(0.42f, 0.38f, 0.48f, 1f));
            }

            RectTransform rect = campoNotas.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.08f, 0.12f);
                rect.anchorMax = new Vector2(0.92f, 0.82f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }

    private void PrepararBotonCerrar()
    {
        if (panel == null)
        {
            return;
        }

        Transform existente = panel.transform.Find("CerrarNotas");

        if (existente == null)
        {
            GameObject objeto = new GameObject(
                "CerrarNotas",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            objeto.transform.SetParent(panel.transform, false);
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

        Button botonCerrar = existente.GetComponent<Button>();

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(Cerrar);
            EstiloUIJuego.AplicarBoton(
                botonCerrar,
                new Color(0.62f, 0.16f, 0.22f, 1f),
                new Color(0.82f, 0.22f, 0.28f, 1f)
            );
        }
    }

    private void AjustarVentana()
    {
        RectTransform rect = panel != null ? panel.GetComponent<RectTransform>() : null;

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
}
