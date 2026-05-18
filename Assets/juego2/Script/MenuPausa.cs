using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public static MenuPausa Instance;

    [Header("UI")]
    public GameObject panelPausa;
    public Button botonReanudar;
    public Button botonGuardar;
    public Button botonMenuPrincipal;
    public TextMeshProUGUI textoEstado;

    private bool pausado = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        panelPausa.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        pausado = true;
        Time.timeScale = 0f;
        panelPausa.SetActive(true);

        botonReanudar.onClick.AddListener(Reanudar);
        botonGuardar.onClick.AddListener(Guardar);
        botonMenuPrincipal.onClick.AddListener(IrAlMenu);
    }

    public void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;
        panelPausa.SetActive(false);
    }

    private void Guardar()
    {
        SistemaGuardado.Instance.GuardarPartida();
        textoEstado.text = "¡Partida guardada!";
        Invoke("LimpiarTexto", 2f);
    }

    private void LimpiarTexto()
    {
        textoEstado.text = "";
    }

    private void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("inicio");
    }
}