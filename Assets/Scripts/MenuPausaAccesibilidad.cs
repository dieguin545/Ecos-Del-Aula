using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuPausaAccesibilidad : MonoBehaviour
{
    public static bool EstaPausado { get; private set; }

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelOpciones;

    [Header("Botones")]
    public Button botonContinuar;
    public Button botonOpciones;
    public Button botonReiniciar;
    public Button botonSalir;
    public Button botonVolver;

    [Header("Accesibilidad")]
    public Toggle toggleTextoGrande;
    public Toggle toggleAltoContraste;
    public Toggle toggleModoDaltonico;

    [Header("Jugador y camara")]
    public MonoBehaviour scriptMovimientoJugador;
    public GameObject camaraPrincipal;
    public MonoBehaviour[] scriptsExtraBloquear;

    [Header("Ventanas que evitan pausar")]
    public GameObject[] pantallasQueBloqueanPausa;

    [Header("Textos TMP")]
    public TextMeshProUGUI[] textosTMP;

    [Header("Textos normales")]
    public Text[] textosNormales;

    [Header("Fondos UI")]
    public Image[] imagenesUI;

    [Header("Escena menu")]
    public string nombreEscenaMenu = "Inicio";

    private Behaviour scriptCamara;

    private float[] tamanosOriginalesTMP;
    private int[] tamanosOriginalesNormales;
    private Color[] coloresOriginalesTMP;
    private Color[] coloresOriginalesNormales;
    private Color[] coloresOriginalesImagenes;

    private Coroutine rutinaCerrarPausa;

    private void Start()
    {
        EstaPausado = false;
        Time.timeScale = 1f;
        NormalizarArreglosSerializados();

        if (camaraPrincipal != null)
        {
            scriptCamara = camaraPrincipal.GetComponent("ControlCamara3D") as Behaviour;
        }

        GuardarValoresOriginales();
        CargarOpciones();
        ConectarBotones();

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        BloquearJugadorYCamara(false);
        OcultarCursorGameplay();

        AplicarAccesibilidad();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (MenuAbierto())
        {
            Continuar();
            return;
        }

        if (InteraccionPC.PCAbierta)
        {
            return;
        }

        if (Time.frameCount <= InteraccionPC.FrameCierrePC + 1)
        {
            return;
        }

        if (HayOtraPantallaAbierta())
        {
            return;
        }

        Pausar();
    }

    private void LateUpdate()
    {
        if (MenuAbierto())
        {
            MostrarCursorMenu();
        }
    }

    private void ConectarBotones()
    {
        if (botonContinuar != null)
        {
            botonContinuar.onClick.RemoveAllListeners();
            botonContinuar.onClick.AddListener(Continuar);
        }

        if (botonOpciones != null)
        {
            botonOpciones.onClick.RemoveAllListeners();
            botonOpciones.onClick.AddListener(AbrirOpciones);
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.RemoveAllListeners();
            botonReiniciar.onClick.AddListener(ReiniciarEscena);
        }

        if (botonSalir != null)
        {
            botonSalir.onClick.RemoveAllListeners();
            botonSalir.onClick.AddListener(SalirAlMenu);
        }

        if (botonVolver != null)
        {
            botonVolver.onClick.RemoveAllListeners();
            botonVolver.onClick.AddListener(VolverAPausa);
        }

        if (toggleTextoGrande != null)
        {
            toggleTextoGrande.onValueChanged.RemoveAllListeners();
            toggleTextoGrande.onValueChanged.AddListener(delegate
            {
                GuardarOpciones();
                AplicarAccesibilidad();
            });
        }

        if (toggleAltoContraste != null)
        {
            toggleAltoContraste.onValueChanged.RemoveAllListeners();
            toggleAltoContraste.onValueChanged.AddListener(delegate
            {
                GuardarOpciones();
                AplicarAccesibilidad();
            });
        }

        if (toggleModoDaltonico != null)
        {
            toggleModoDaltonico.onValueChanged.RemoveAllListeners();
            toggleModoDaltonico.onValueChanged.AddListener(delegate
            {
                GuardarOpciones();
                AplicarAccesibilidad();
            });
        }
    }

    public void Pausar()
    {
        if (rutinaCerrarPausa != null)
        {
            StopCoroutine(rutinaCerrarPausa);
            rutinaCerrarPausa = null;
        }

        EstaPausado = true;

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        Time.timeScale = 0f;

        BloquearJugadorYCamara(true);
        MostrarCursorMenu();
    }

    public void Continuar()
    {
        EstaPausado = false;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        Time.timeScale = 1f;

        OcultarCursorGameplay();

        if (scriptMovimientoJugador != null)
        {
            scriptMovimientoJugador.enabled = true;
        }

        HabilitarScriptsExtra(true);

        if (rutinaCerrarPausa != null)
        {
            StopCoroutine(rutinaCerrarPausa);
        }

        rutinaCerrarPausa = StartCoroutine(ReactivarCamaraDespuesDeUnFrame());
    }

    private IEnumerator ReactivarCamaraDespuesDeUnFrame()
    {
        yield return null;

        if (!EstaPausado && !InteraccionPC.PCAbierta)
        {
            if (scriptCamara != null)
            {
                scriptCamara.enabled = true;
            }

            OcultarCursorGameplay();
        }

        rutinaCerrarPausa = null;
    }

    public void AbrirOpciones()
    {
        EstaPausado = true;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }

        Time.timeScale = 0f;

        BloquearJugadorYCamara(true);
        MostrarCursorMenu();
    }

    public void VolverAPausa()
    {
        EstaPausado = true;

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        Time.timeScale = 0f;

        BloquearJugadorYCamara(true);
        MostrarCursorMenu();
    }

    public void ReiniciarEscena()
    {
        EstaPausado = false;
        Time.timeScale = 1f;

        BloquearJugadorYCamara(false);
        OcultarCursorGameplay();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SalirAlMenu()
    {
        EstaPausado = false;
        Time.timeScale = 1f;

        BloquearJugadorYCamara(false);
        MostrarCursorMenu();

        SceneManager.LoadScene(nombreEscenaMenu);
    }

    private bool MenuAbierto()
    {
        bool pausaAbierta = panelPausa != null && panelPausa.activeSelf;
        bool opcionesAbiertas = panelOpciones != null && panelOpciones.activeSelf;

        return EstaPausado || pausaAbierta || opcionesAbiertas;
    }

    private bool HayOtraPantallaAbierta()
    {
        if (pantallasQueBloqueanPausa == null || pantallasQueBloqueanPausa.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < pantallasQueBloqueanPausa.Length; i++)
        {
            GameObject pantalla = pantallasQueBloqueanPausa[i];

            if (pantalla == null)
            {
                continue;
            }

            Scene escenaPantalla = pantalla.scene;

            if (!escenaPantalla.IsValid() || !escenaPantalla.isLoaded)
            {
                continue;
            }

            if (pantalla.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private void BloquearJugadorYCamara(bool bloquear)
    {
        if (scriptMovimientoJugador != null)
        {
            scriptMovimientoJugador.enabled = !bloquear;
        }

        if (scriptCamara != null)
        {
            scriptCamara.enabled = !bloquear;
        }

        HabilitarScriptsExtra(!bloquear);
    }

    private void MostrarCursorMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OcultarCursorGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void GuardarOpciones()
    {
        if (toggleTextoGrande != null)
        {
            PlayerPrefs.SetInt("accesibilidad_texto_grande", toggleTextoGrande.isOn ? 1 : 0);
        }

        if (toggleAltoContraste != null)
        {
            PlayerPrefs.SetInt("accesibilidad_alto_contraste", toggleAltoContraste.isOn ? 1 : 0);
        }

        if (toggleModoDaltonico != null)
        {
            PlayerPrefs.SetInt(
                "accesibilidad_tipo_daltonismo",
                toggleModoDaltonico.isOn ? (int)TipoDaltonismo.Deuteranopia : (int)TipoDaltonismo.Ninguno
            );
        }

        PlayerPrefs.Save();
    }

    private void CargarOpciones()
    {
        if (toggleTextoGrande != null)
        {
            toggleTextoGrande.isOn = PlayerPrefs.GetInt("accesibilidad_texto_grande", 0) == 1;
        }

        if (toggleAltoContraste != null)
        {
            toggleAltoContraste.isOn = PlayerPrefs.GetInt("accesibilidad_alto_contraste", 0) == 1;
        }

        if (toggleModoDaltonico != null)
        {
            toggleModoDaltonico.isOn =
                PlayerPrefs.GetInt("accesibilidad_tipo_daltonismo", 0)
                != (int)TipoDaltonismo.Ninguno;
        }
    }

    private void GuardarValoresOriginales()
    {
        NormalizarArreglosSerializados();

        tamanosOriginalesTMP = new float[textosTMP.Length];
        coloresOriginalesTMP = new Color[textosTMP.Length];

        for (int i = 0; i < textosTMP.Length; i++)
        {
            if (textosTMP[i] != null)
            {
                tamanosOriginalesTMP[i] = textosTMP[i].fontSize;
                coloresOriginalesTMP[i] = textosTMP[i].color;
            }
        }

        tamanosOriginalesNormales = new int[textosNormales.Length];
        coloresOriginalesNormales = new Color[textosNormales.Length];

        for (int i = 0; i < textosNormales.Length; i++)
        {
            if (textosNormales[i] != null)
            {
                tamanosOriginalesNormales[i] = textosNormales[i].fontSize;
                coloresOriginalesNormales[i] = textosNormales[i].color;
            }
        }

        coloresOriginalesImagenes = new Color[imagenesUI.Length];

        for (int i = 0; i < imagenesUI.Length; i++)
        {
            if (imagenesUI[i] != null)
            {
                coloresOriginalesImagenes[i] = imagenesUI[i].color;
            }
        }
    }

    private void AplicarAccesibilidad()
    {
        AplicarTextosTMP();
        AplicarTextosNormales();
        AplicarFondosUI();
    }

    private void AplicarTextosTMP()
    {
        if (textosTMP == null || tamanosOriginalesTMP == null || coloresOriginalesTMP == null)
        {
            return;
        }

        for (int i = 0; i < textosTMP.Length; i++)
        {
            if (textosTMP[i] == null) continue;

            float tamanoBase = tamanosOriginalesTMP[i] <= 0 ? textosTMP[i].fontSize : tamanosOriginalesTMP[i];

            textosTMP[i].fontSize = EstaTextoGrandeActivo() ? tamanoBase * 1.25f : tamanoBase;

            if (EstaModoDaltonicoActivo())
            {
                textosTMP[i].color = new Color(0.2f, 0.75f, 1f);
            }
            else
            {
                textosTMP[i].color = coloresOriginalesTMP[i];
            }
        }
    }

    private void AplicarTextosNormales()
    {
        if (textosNormales == null || tamanosOriginalesNormales == null || coloresOriginalesNormales == null)
        {
            return;
        }

        for (int i = 0; i < textosNormales.Length; i++)
        {
            if (textosNormales[i] == null) continue;

            int tamanoBase = tamanosOriginalesNormales[i] <= 0 ? textosNormales[i].fontSize : tamanosOriginalesNormales[i];

            textosNormales[i].fontSize = EstaTextoGrandeActivo()
                ? Mathf.RoundToInt(tamanoBase * 1.25f)
                : tamanoBase;

            if (EstaModoDaltonicoActivo())
            {
                textosNormales[i].color = new Color(0.2f, 0.75f, 1f);
            }
            else
            {
                textosNormales[i].color = coloresOriginalesNormales[i];
            }
        }
    }

    private void AplicarFondosUI()
    {
        if (imagenesUI == null || coloresOriginalesImagenes == null)
        {
            return;
        }

        for (int i = 0; i < imagenesUI.Length; i++)
        {
            if (imagenesUI[i] == null) continue;

            if (EstaAltoContrasteActivo())
            {
                imagenesUI[i].color = new Color(0f, 0f, 0f, 0.9f);
            }
            else if (EstaModoDaltonicoActivo())
            {
                imagenesUI[i].color = new Color(0.05f, 0.12f, 0.22f, 0.8f);
            }
            else
            {
                imagenesUI[i].color = coloresOriginalesImagenes[i];
            }
        }
    }

    private bool EstaTextoGrandeActivo()
    {
        return toggleTextoGrande != null && toggleTextoGrande.isOn;
    }

    private bool EstaAltoContrasteActivo()
    {
        return toggleAltoContraste != null && toggleAltoContraste.isOn;
    }

    private bool EstaModoDaltonicoActivo()
    {
        return toggleModoDaltonico != null && toggleModoDaltonico.isOn;
    }

    private void NormalizarArreglosSerializados()
    {
        if (scriptsExtraBloquear == null)
        {
            scriptsExtraBloquear = new MonoBehaviour[0];
        }

        if (pantallasQueBloqueanPausa == null)
        {
            pantallasQueBloqueanPausa = new GameObject[0];
        }

        if (textosTMP == null)
        {
            textosTMP = new TextMeshProUGUI[0];
        }

        if (textosNormales == null)
        {
            textosNormales = new Text[0];
        }

        if (imagenesUI == null)
        {
            imagenesUI = new Image[0];
        }
    }

    private void HabilitarScriptsExtra(bool habilitar)
    {
        if (scriptsExtraBloquear == null)
        {
            return;
        }

        for (int i = 0; i < scriptsExtraBloquear.Length; i++)
        {
            if (scriptsExtraBloquear[i] != null)
            {
                scriptsExtraBloquear[i].enabled = habilitar;
            }
        }
    }
}
