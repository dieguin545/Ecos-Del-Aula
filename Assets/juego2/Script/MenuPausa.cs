using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    public static MenuPausa Instance;

    [Header("UI")]
    public GameObject panelPausa;
    public Button botonReanudar;
    public Button botonGuardar;
    public Button botonMenuPrincipal;
    public TextMeshProUGUI textoEstado;

    private bool pausado;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ReinicializarTrasCargaEscena();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!GestorEntradaGlobal.PausaPresionada())
        {
            return;
        }

        if (pausado)
        {
            Reanudar();
        }
        else
        {
            Pausar();
        }
    }

    public void Pausar()
    {
        ResolverReferencias();
        pausado = true;
        Time.timeScale = 0f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ConectarBotones();
    }

    public void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
    }

    private void Guardar()
    {
        if (SistemaGuardado.Instance != null)
        {
            SistemaGuardado.Instance.GuardarPartida();
        }

        if (textoEstado != null)
        {
            textoEstado.text = "Partida guardada.";
            Invoke(nameof(LimpiarTexto), 2f);
        }
    }

    private void LimpiarTexto()
    {
        if (textoEstado != null)
        {
            textoEstado.text = "";
        }
    }

    private void IrAlMenu()
    {
        pausado = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("inicio");
    }

    public void ReinicializarTrasCargaEscena()
    {
        ResolverReferencias();
        pausado = false;
        Time.timeScale = 1f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        ConectarBotones();
        AplicarEstiloVisual();
    }

    private void ResolverReferencias()
    {
        if (panelPausa == null)
        {
            GameObject panel = GameObject.Find("PanelPausa");
            if (panel == null)
            {
                panel = GameObject.Find("PanelPausaJuego2");
            }

            panelPausa = panel;
        }

        if (panelPausa == null)
        {
            return;
        }

        Button[] botones = panelPausa.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];
            string texto = ObtenerTextoBoton(boton).ToLowerInvariant();

            if (botonReanudar == null && (texto.Contains("continuar") || texto.Contains("reanudar")))
            {
                botonReanudar = boton;
            }
            else if (botonGuardar == null && texto.Contains("guardar"))
            {
                botonGuardar = boton;
            }
            else if (botonMenuPrincipal == null && (texto.Contains("menu") || texto.Contains("salir")))
            {
                botonMenuPrincipal = boton;
            }
        }
    }

    private void ConectarBotones()
    {
        ConectarBoton(botonReanudar, Reanudar);
        ConectarBoton(botonGuardar, Guardar);
        ConectarBoton(botonMenuPrincipal, IrAlMenu);
    }

    private void ConectarBoton(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if (boton == null)
        {
            return;
        }

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(accion);
    }

    private void AplicarEstiloVisual()
    {
        if (panelPausa != null)
        {
            Image fondo = panelPausa.GetComponent<Image>();

            if (fondo == null)
            {
                fondo = panelPausa.AddComponent<Image>();
            }

            fondo.color = new Color(0.06f, 0.03f, 0.10f, 0.82f);

            RectTransform rect = panelPausa.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(520f, 430f);
            }
        }

        EstilizarBoton(botonReanudar, "Continuar");
        EstilizarBoton(botonGuardar, "Guardar");
        EstilizarBoton(botonMenuPrincipal, "Salir al menu");

        if (panelPausa != null)
        {
            Button[] botones = panelPausa.GetComponentsInChildren<Button>(true);

            for (int i = 0; i < botones.Length; i++)
            {
                Button boton = botones[i];
                string texto = ObtenerTextoBoton(boton).ToLowerInvariant();

                if (texto.Contains("guardar"))
                {
                    EstilizarBoton(boton, "Guardar");
                }
                else if (texto.Contains("salir") || texto.Contains("menu"))
                {
                    EstilizarBoton(boton, "Salir al menu");
                }
                else if (texto.Contains("continuar") || texto.Contains("reanudar"))
                {
                    EstilizarBoton(boton, "Continuar");
                }
                else
                {
                    EstilizarBoton(boton, ObtenerTextoBoton(boton));
                }
            }

            TextMeshProUGUI[] textos = panelPausa.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < textos.Length; i++)
            {
                if (textos[i] != null)
                {
                    textos[i].color = Color.white;
                }
            }

            Text[] textosLegacy = panelPausa.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < textosLegacy.Length; i++)
            {
                if (textosLegacy[i] != null)
                {
                    textosLegacy[i].color = Color.white;
                }
            }
        }

        if (textoEstado != null)
        {
            textoEstado.color = Color.white;
        }
    }

    private void EstilizarBoton(Button boton, string texto)
    {
        if (boton == null)
        {
            return;
        }

        Image imagen = boton.GetComponent<Image>();

        if (imagen != null)
        {
            imagen.color = new Color(0.04f, 0.03f, 0.09f, 0.95f);
        }

        TextMeshProUGUI etiquetaTMP = boton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (etiquetaTMP != null)
        {
            etiquetaTMP.text = texto;
            etiquetaTMP.color = Color.white;
            etiquetaTMP.fontSize = Mathf.Max(etiquetaTMP.fontSize, 22f);
        }

        Text etiquetaLegacy = boton.GetComponentInChildren<Text>(true);

        if (etiquetaLegacy != null)
        {
            etiquetaLegacy.text = texto;
            etiquetaLegacy.color = Color.white;
            etiquetaLegacy.fontSize = Mathf.Max(etiquetaLegacy.fontSize, 22);
        }
    }

    private string ObtenerTextoBoton(Button boton)
    {
        if (boton == null)
        {
            return string.Empty;
        }

        TextMeshProUGUI etiquetaTMP = boton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (etiquetaTMP != null)
        {
            return etiquetaTMP.text;
        }

        Text etiquetaLegacy = boton.GetComponentInChildren<Text>(true);
        return etiquetaLegacy != null ? etiquetaLegacy.text : boton.name;
    }
}
