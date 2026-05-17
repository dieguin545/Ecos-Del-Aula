using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConfrontacionManager : MonoBehaviour
{
    public static ConfrontacionManager Instance;

    [Header("UI Confrontacion")]
    public GameObject panelConfrontacion;
    public TextMeshProUGUI textoSituacion;
    public Button botonPelear;
    public Button botonReportar;
    public TextMeshProUGUI textoEvidenciaDisponible;

    [Header("UI Resultado")]
    public GameObject panelResultado;
    public TextMeshProUGUI textoResultado;
    public Button botonVolverAJugar;

    [Header("Bully")]
    public GameObject prefabBully;
    public Transform spawnBully;

    private bool confrontacionActiva = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        panelConfrontacion.SetActive(false);
        panelResultado.SetActive(false);
    }

    public void IniciarConfrontacion()
    {
        if (confrontacionActiva) return;
        confrontacionActiva = true;

        // Desactiva movimiento del jugador
        FindObjectOfType<MovimientoJugador>().enabled = false;
        FindObjectOfType<Animacion>().enabled = false;

        // Spawna el bully en el pasillo
        if (prefabBully != null)
            Instantiate(prefabBully, spawnBully.position, Quaternion.identity);

        // Muestra el panel
        panelConfrontacion.SetActive(true);
        textoSituacion.text = "Un estudiante te intercepta antes de salir...\n¿Qué vas a hacer?";

        // Muestra evidencia disponible
        int evidencias = SistemaEvidencia.Instance.GetEvidencias();
        textoEvidenciaDisponible.text = $"Evidencias recolectadas: {evidencias}";

        // Configura botones
        botonPelear.onClick.AddListener(ElegirPelear);
        botonReportar.onClick.AddListener(ElegirReportar);
    }

    private void ElegirPelear()
    {
        panelConfrontacion.SetActive(false);
        MostrarResultado(false);
    }

    private void ElegirReportar()
    {
        panelConfrontacion.SetActive(false);

        if (SistemaEvidencia.Instance.TieneSuficienteEvidencia())
        {
            MostrarResultado(true);
        }
        else
        {
            MostrarResultadoSinEvidencia();
        }
    }

    private void MostrarResultado(bool gano)
    {
        panelResultado.SetActive(true);

        if (gano)
        {
            textoResultado.text = "¡Hiciste lo correcto!\nEl profesor intervino gracias a tu evidencia.\nEl acosador recibió consecuencias.\n\n🏆 ¡Ganaste!";
        }
        else
        {
            textoResultado.text = "Elegiste pelear y ganaste la pelea...\nPero fuiste expulsado del colegio.\n\nLa violencia nunca es la respuesta.\n\n💀 Game Over";
        }

        botonVolverAJugar.onClick.AddListener(VolverAJugar);
    }

    private void MostrarResultadoSinEvidencia()
    {
        panelResultado.SetActive(true);
        textoResultado.text = "Llamaste a un profesor pero no tenías evidencia suficiente.\nEl acosador no recibió consecuencias.\n\nLa próxima vez guarda evidencia.\n\n⚠️ Final Neutro";
        botonVolverAJugar.onClick.AddListener(VolverAJugar);
    }

    private void VolverAJugar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}