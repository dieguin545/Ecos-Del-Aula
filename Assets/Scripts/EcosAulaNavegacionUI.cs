using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EcosAulaNavegacionUI : MonoBehaviour
{
    private static EcosAulaNavegacionUI instancia;
    public static EcosAulaNavegacionUI Instancia => instancia;

    private GameObject ultimoSeleccionado;
    private Outline contornoActual;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        ultimoSeleccionado = null;
        contornoActual = null;
        StartCoroutine(FocoInicialRetardado());
    }

    private IEnumerator FocoInicialRetardado()
    {
        // Espera un momento a que la escena y el rediseño se inicialicen
        yield return new WaitForSecondsRealtime(0.1f);
        IntentarFocalizarPrimero();
    }

    private void Update()
    {
        if (EventSystem.current == null) return;

        GameObject seleccionado = EventSystem.current.currentSelectedGameObject;

        // Auto-Foco si se presiona navegación y no hay nada seleccionado
        if (seleccionado == null)
        {
            if (DetectarIntentoNavegacion())
            {
                IntentarFocalizarPrimero();
            }
        }
        else if (seleccionado != ultimoSeleccionado)
        {
            // Actualizar borde de selección
            RemoverContorno(ultimoSeleccionado);
            AgregarContorno(seleccionado);
            ultimoSeleccionado = seleccionado;
            AsegurarElementoEnVistaScroll(seleccionado);
        }

        // Mantener el foco visible y estable. El pulso rápido dificultaba la lectura.
        if (contornoActual != null)
        {
            Color c = contornoActual.effectColor;
            c.a = 0.9f;
            contornoActual.effectColor = c;
        }
    }

    private bool DetectarIntentoNavegacion()
    {
        // Teclado
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            return true;
        }

        // Gamepad: Sticks
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (Mathf.Abs(horizontal) > 0.3f || Mathf.Abs(vertical) > 0.3f)
        {
            return true;
        }

        // Gamepad: Botones o D-pad virtual
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1) ||
            Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.JoystickButton3) ||
            Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5) ||
            Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            return true;
        }

        return false;
    }

    public void IntentarFocalizarPrimero()
    {
        if (EventSystem.current == null) return;

        // Buscar primero en paneles prioritarios (Pausa, apps abiertas)
        Selectable target = EncontrarPrimerSelectableActivo();
        if (target != null)
        {
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }
    }

    private Selectable EncontrarPrimerSelectableActivo()
    {
        // 1. Buscar en paneles de pausa u opciones si están activos
        MenuPausaAccesibilidad pausa = FindAnyObjectByType<MenuPausaAccesibilidad>(FindObjectsInactive.Include);
        if (pausa != null)
        {
            if (pausa.panelOpciones != null && pausa.panelOpciones.activeInHierarchy)
            {
                Selectable s = BuscarEnJerarquia(pausa.panelOpciones.transform);
                if (s != null) return s;
            }
            if (pausa.panelDetalleSlot != null && pausa.panelDetalleSlot.activeInHierarchy)
            {
                Selectable s = BuscarEnJerarquia(pausa.panelDetalleSlot.transform);
                if (s != null) return s;
            }
            if (pausa.panelPausa != null && pausa.panelPausa.activeInHierarchy)
            {
                Selectable s = BuscarEnJerarquia(pausa.panelPausa.transform);
                if (s != null) return s;
            }
        }

        // 1b. Buscar en pausa de SpaceShooter
        ControladorPausaSpaceShooter pausaSS = FindAnyObjectByType<ControladorPausaSpaceShooter>(FindObjectsInactive.Include);
        if (pausaSS != null && pausaSS.EstaPausado)
        {
            Transform panelTr = null;
            Canvas canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas != null)
            {
                panelTr = canvas.transform.Find("PanelPausaSpaceShooter");
            }
            if (panelTr == null)
            {
                panelTr = pausaSS.transform.Find("PanelPausaSpaceShooter");
            }
            if (panelTr != null && panelTr.gameObject.activeInHierarchy)
            {
                Selectable s = BuscarEnJerarquia(panelTr);
                if (s != null) return s;
            }
        }

        // 2. Buscar en el Canvas general
        var selectables = Selectable.allSelectablesArray;
        
        // Ordenar selectables para priorizar elementos más arriba o activos en jerarquía
        List<Selectable> lista = new List<Selectable>();
        foreach (var s in selectables)
        {
            if (EsSelectableValido(s))
            {
                if (s.navigation.mode == Navigation.Mode.None)
                {
                    Navigation nav = s.navigation;
                    nav.mode = Navigation.Mode.Automatic;
                    s.navigation = nav;
                }
                lista.Add(s);
            }
        }

        if (lista.Count == 0) return null;

        // Priorizar por orden de jerarquía o nombre de panel
        lista.Sort((a, b) =>
        {
            // Si uno es del menú izquierdo de inicio, priorizar
            bool aIsMenu = ObtenerPrioridadSelectable(a) == 0;
            bool bIsMenu = ObtenerPrioridadSelectable(b) == 0;
            if (aIsMenu && !bIsMenu) return -1;
            if (!aIsMenu && bIsMenu) return 1;
            int ordenA = EsSelectableValido(a) ? a.transform.GetSiblingIndex() : int.MaxValue;
            int ordenB = EsSelectableValido(b) ? b.transform.GetSiblingIndex() : int.MaxValue;
            return ordenA.CompareTo(ordenB);
        });

        return lista[0];
    }

    private Selectable BuscarEnJerarquia(Transform padre)
    {
        if (padre == null)
        {
            return null;
        }

        Selectable[] components = padre.GetComponentsInChildren<Selectable>(true);
        foreach (var c in components)
        {
            if (EsSelectableValido(c))
            {
                if (c.navigation.mode == Navigation.Mode.None)
                {
                    Navigation nav = c.navigation;
                    nav.mode = Navigation.Mode.Automatic;
                    c.navigation = nav;
                }
                return c;
            }
        }
        return null;
    }

    private bool EsSelectableValido(Selectable selectable)
    {
        try
        {
            if (selectable == null)
            {
                return false;
            }

            GameObject go = selectable.gameObject;

            if (go == null || !go.activeInHierarchy || !selectable.isActiveAndEnabled || !selectable.interactable)
            {
                return false;
            }

            return selectable.transform != null;
        }
        catch (MissingReferenceException)
        {
            return false;
        }
        catch (System.NullReferenceException)
        {
            return false;
        }
    }

    private int ObtenerPrioridadSelectable(Selectable selectable)
    {
        if (!EsSelectableValido(selectable))
        {
            return int.MaxValue;
        }

        string nombre = selectable.name ?? string.Empty;
        Transform padre = selectable.transform.parent;
        string nombrePadre = padre != null ? padre.name : string.Empty;

        bool esMenu =
            nombre.StartsWith("_")
            || nombrePadre.Contains("Panel")
            || nombrePadre.Contains("Menu")
            || nombrePadre.Contains("Pausa");

        return esMenu ? 0 : 1;
    }

    private void AgregarContorno(GameObject go)
    {
        if (go == null) return;

        // Solo agregar contorno a objetos con un componente gráfico
        Graphic grafico = go.GetComponent<Graphic>();
        if (grafico == null) return;

        Outline outline = go.GetComponent<Outline>();
        if (outline == null)
        {
            outline = go.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0.655f, 0.545f, 0.980f, 1f); // Lila
        outline.effectDistance = new Vector2(4f, 4f);
        outline.useGraphicAlpha = true;
        outline.enabled = true;

        contornoActual = outline;
    }

    private void RemoverContorno(GameObject go)
    {
        if (go == null) return;

        Outline outline = go.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }

        if (contornoActual == outline)
        {
            contornoActual = null;
        }
    }

    private void AsegurarElementoEnVistaScroll(GameObject seleccionado)
    {
        if (seleccionado == null) return;
        ScrollRect scrollRect = seleccionado.GetComponentInParent<ScrollRect>();
        if (scrollRect == null || scrollRect.content == null) return;

        RectTransform viewport = scrollRect.viewport;
        if (viewport == null) viewport = scrollRect.GetComponent<RectTransform>();

        RectTransform item = seleccionado.GetComponent<RectTransform>();
        if (item == null) return;

        // Forzar actualización de layouts para tener posiciones correctas
        Canvas.ForceUpdateCanvases();

        // Convertir esquinas del item a espacio local del viewport
        Vector3[] itemCorners = new Vector3[4];
        item.GetWorldCorners(itemCorners);

        float itemMinY = float.MaxValue;
        float itemMaxY = float.MinValue;
        float itemMinX = float.MaxValue;
        float itemMaxX = float.MinValue;

        for (int i = 0; i < 4; i++)
        {
            Vector3 localPt = viewport.InverseTransformPoint(itemCorners[i]);
            if (localPt.y < itemMinY) itemMinY = localPt.y;
            if (localPt.y > itemMaxY) itemMaxY = localPt.y;
            if (localPt.x < itemMinX) itemMinX = localPt.x;
            if (localPt.x > itemMaxX) itemMaxX = localPt.x;
        }

        float viewportMinY = -viewport.rect.height * viewport.pivot.y;
        float viewportMaxY = viewport.rect.height * (1f - viewport.pivot.y);
        float viewportMinX = -viewport.rect.width * viewport.pivot.x;
        float viewportMaxX = viewport.rect.width * (1f - viewport.pivot.x);

        Vector2 contentPos = scrollRect.content.anchoredPosition;

        if (scrollRect.vertical)
        {
            if (itemMinY < viewportMinY)
            {
                contentPos.y += (viewportMinY - itemMinY) + 10f; // Margen de 10px
            }
            else if (itemMaxY > viewportMaxY)
            {
                contentPos.y -= (itemMaxY - viewportMaxY) + 10f;
            }
        }

        if (scrollRect.horizontal)
        {
            if (itemMinX < viewportMinX)
            {
                contentPos.x += (viewportMinX - itemMinX) + 10f;
            }
            else if (itemMaxX > viewportMaxX)
            {
                contentPos.x -= (itemMaxX - viewportMaxX) + 10f;
            }
        }

        scrollRect.content.anchoredPosition = contentPos;
    }
}
