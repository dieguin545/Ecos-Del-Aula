using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EcosAulaNavegacionEscritorioPC : MonoBehaviour
{
    private GestorVentanasPC gestorVentanas;
    private List<Button> botonesEscritorio = new List<Button>();
    private GameObject ultimaVentanaActiva;

    private void Awake()
    {
        gestorVentanas = GetComponent<GestorVentanasPC>();
    }

    private void OnEnable()
    {
        StartCoroutine(InicializarNavegacionRetardada());
    }

    private IEnumerator InicializarNavegacionRetardada()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        ConfigurarBotonesEscritorio();
        FocalizarEscritorio();
        ActualizarPromptsEscritorio();
    }

    private void Update()
    {
        if (EventSystem.current == null) return;

        // Comprobar si hay una ventana activa abierta
        GameObject ventanaActiva = EncontrarVentanaActiva();

        if (ventanaActiva != ultimaVentanaActiva)
        {
            // Cambio de estado (se abrió o cerró una ventana)
            if (ventanaActiva != null)
            {
                // Se abrió una ventana, focalizar primer elemento dentro de ella
                FocalizarElementoEnVentana(ventanaActiva);
                ActualizarPromptsVentana(ventanaActiva);
            }
            else
            {
                // Se cerró la ventana, volver a focalizar el escritorio
                FocalizarEscritorio();
                ActualizarPromptsEscritorio();
            }
            ultimaVentanaActiva = ventanaActiva;
        }

        // Si no hay ventana abierta y no hay nada seleccionado, o lo seleccionado no es del escritorio
        GameObject seleccionado = EventSystem.current.currentSelectedGameObject;
        if (ventanaActiva == null)
        {
            if (seleccionado == null || PerteneceAVentana(seleccionado.transform))
            {
                if (botonesEscritorio.Count > 0)
                {
                    // Asegurar que al menos el primer botón esté activo antes de seleccionar
                    foreach (var btn in botonesEscritorio)
                    {
                        if (btn != null && btn.gameObject.activeInHierarchy)
                        {
                            EventSystem.current.SetSelectedGameObject(btn.gameObject);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            // Hay una ventana abierta. No re-focalizar cada frame si la seleccion quedo en null:
            // eso roba el scroll de ventanas como Casos y lo devuelve arriba.
            if (seleccionado != null && !seleccionado.transform.IsChildOf(ventanaActiva.transform))
            {
                FocalizarElementoEnVentana(ventanaActiva);
            }
        }
    }

    private void ActualizarPromptsEscritorio()
    {
        EcosAulaPromptUI.CrearBarraPrompts(transform,
            (AccionLogica.Navegar, "Cambiar app"),
            (AccionLogica.Confirmar, "Abrir"),
            (AccionLogica.Cancelar, "Apagar PC"));
    }

    private void ActualizarPromptsVentana(GameObject ventana)
    {
        string nombre = ventana.name.ToLowerInvariant();
        if (nombre.Contains("tienda"))
        {
            EcosAulaPromptUI.CrearBarraPrompts(transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Comprar"),
                (AccionLogica.Cancelar, "Volver"));
        }
        else if (nombre.Contains("casos"))
        {
            EcosAulaPromptUI.CrearBarraPrompts(transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Seleccionar / Abrir"),
                (AccionLogica.RevisarContexto, "Revisar contexto"),
                (AccionLogica.Cancelar, "Volver"));
        }
        else
        {
            EcosAulaPromptUI.CrearBarraPrompts(transform,
                (AccionLogica.Navegar, "Navegar"),
                (AccionLogica.Confirmar, "Seleccionar"),
                (AccionLogica.Cancelar, "Volver"));
        }
    }

    private void ConfigurarBotonesEscritorio()
    {
        botonesEscritorio.Clear();
        Button[] todos = GetComponentsInChildren<Button>(true);
        foreach (Button b in todos)
        {
            if (b.gameObject.activeInHierarchy && !PerteneceAVentana(b.transform))
            {
                botonesEscritorio.Add(b);

                // Forzar navegación automática para los iconos
                Navigation nav = b.navigation;
                nav.mode = Navigation.Mode.Automatic;
                b.navigation = nav;
            }
        }
    }

    private void FocalizarEscritorio()
    {
        ConfigurarBotonesEscritorio();
        if (botonesEscritorio.Count > 0 && EventSystem.current != null)
        {
            foreach (var btn in botonesEscritorio)
            {
                if (btn != null && btn.gameObject.activeInHierarchy)
                {
                    EventSystem.current.SetSelectedGameObject(btn.gameObject);
                    break;
                }
            }
        }
    }

    private void FocalizarElementoEnVentana(GameObject ventana)
    {
        if (EventSystem.current == null) return;

        // Buscar el primer Selectable activo y habilitado en la ventana
        Selectable[] components = ventana.GetComponentsInChildren<Selectable>(true);
        foreach (var c in components)
        {
            if (c != null && c.gameObject.activeInHierarchy && c.interactable)
            {
                Navigation nav = c.navigation;
                nav.mode = Navigation.Mode.Automatic;
                c.navigation = nav;
            }
        }

        foreach (var c in components)
        {
            if (c != null && c.gameObject.activeInHierarchy && c.interactable)
            {
                EventSystem.current.SetSelectedGameObject(c.gameObject);
                return;
            }
        }
    }

    private GameObject EncontrarVentanaActiva()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform hijo = transform.GetChild(i);
            if (hijo.gameObject.activeSelf && (hijo.name.StartsWith("Ventana") || hijo.name == "Tienda" || 
                hijo.name == "ayuda" || hijo.name.StartsWith("Panel") || hijo.name == "BlocNotas"))
            {
                return hijo.gameObject;
            }
        }
        return null;
    }

    private bool PerteneceAVentana(Transform t)
    {
        if (t == null)
        {
            return false;
        }

        Transform p = t.parent;
        while (p != null && p != transform)
        {
            if (p.name.StartsWith("Ventana") || p.name == "Tienda" || 
                p.name == "ayuda" || p.name.StartsWith("Panel") || p.name == "BlocNotas")
            {
                return true;
            }
            p = p.parent;
        }
        return false;
    }
}
