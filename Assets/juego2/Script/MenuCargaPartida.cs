using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuCargaPartida : MonoBehaviour
{
    public GameObject panelMenu;
    public Button botonNuevaPartida;
    public Button botonCargarPartida;
    public TextMeshProUGUI textoInfo;

    void Start()
    {
        if (SistemaGuardado.Instance.ExistePartida())
        {
            panelMenu.SetActive(true);
            textoInfo.text = "¿Deseas continuar tu partida anterior?";
            botonCargarPartida.gameObject.SetActive(true);

            // Configurar navegación
            Navigation navNueva = botonNuevaPartida.navigation;
            navNueva.mode = Navigation.Mode.Automatic;
            botonNuevaPartida.navigation = navNueva;

            Navigation navCargar = botonCargarPartida.navigation;
            navCargar.mode = Navigation.Mode.Automatic;
            botonCargarPartida.navigation = navCargar;

            // Auto-seleccionar
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(botonCargarPartida.gameObject);
            }

            EcosAulaPromptUI.CrearBarraPrompts(panelMenu.transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Seleccionar"));
        }
        else
        {
            IniciarNuevaPartida();
        }

        botonNuevaPartida.onClick.AddListener(IniciarNuevaPartida);
        botonCargarPartida.onClick.AddListener(CargarPartida);
    }

    private void IniciarNuevaPartida()
    {
        SistemaGuardado.Instance.EliminarPartida();
        panelMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void CargarPartida()
    {
        DatosPartida2 datos = SistemaGuardado.Instance.CargarPartida();
        if (datos != null)
        {
            AnxietySystem.Instance.IncreaseAnxiety(datos.ansiedad);

            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
                jugador.transform.position = new Vector3(datos.posicionX, datos.posicionY, 0);

            panelMenu.SetActive(false);
        }
    }
}