using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EcosAulaNavegacionPersonajes : MonoBehaviour
{
    private SeleccionPersonaje selector;
    private Button botonSeleccionar;
    private float tiempoSiguienteInput = 0f;
    private const float CooldownInput = 0.25f;

    private void Start()
    {
        selector = FindAnyObjectByType<SeleccionPersonaje>();
        if (selector == null)
        {
            Debug.LogWarning("[EcosAulaNavegacionPersonajes] No se encontró SeleccionPersonaje en la escena.");
            enabled = false;
            return;
        }

        botonSeleccionar = BuscarBotonPorNombre("Seleccionar");
        if (EventSystem.current != null && botonSeleccionar != null)
        {
            EventSystem.current.SetSelectedGameObject(botonSeleccionar.gameObject);
        }
    }

    private void Update()
    {
        if (selector == null) return;
        if (Time.unscaledTime < tiempoSiguienteInput) return;

        // Cambio rápido: solo hombros del control o Q/E. Las flechas y el D-Pad quedan para navegar botones.
        bool lb = Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.Q);
        bool rb = Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.E);

        if (lb)
        {
            selector.Anterior();
            tiempoSiguienteInput = Time.unscaledTime + CooldownInput;
        }
        else if (rb)
        {
            selector.Siguiente();
            tiempoSiguienteInput = Time.unscaledTime + CooldownInput;
        }

        // Confirmar (R / A / Enter / Space). R coincide con el prompt visible de la escena.
        bool confirmarDirecto = Input.GetKeyDown(KeyCode.R);
        if (confirmarDirecto || GestorEntradaGlobal.ConfirmarPresionado())
        {
            GameObject seleccionado = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            bool seleccionarTieneFoco = botonSeleccionar != null && seleccionado == botonSeleccionar.gameObject;

            if (confirmarDirecto || seleccionarTieneFoco || !HayBotonUISeleccionado())
            {
                selector.Seleccionar();
            }
            tiempoSiguienteInput = Time.unscaledTime + CooldownInput;
        }

        // Cancelar / Regresar (B / ESC)
        if (GestorEntradaGlobal.CancelarPresionado())
        {
            SceneManager.LoadScene("inicio");
            tiempoSiguienteInput = Time.unscaledTime + CooldownInput;
        }
    }

    private bool HayBotonUISeleccionado()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
        {
            return false;
        }

        return EventSystem.current.currentSelectedGameObject.GetComponent<Button>() != null;
    }

    private Button BuscarBotonPorNombre(string nombre)
    {
        Button[] botones = FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Button boton in botones)
        {
            if (boton != null && boton.gameObject.name.Contains(nombre))
            {
                return boton;
            }
        }

        return null;
    }
}
