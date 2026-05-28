using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class Objeto
{
    public string nombre;
    public Sprite icono;
    public int dano;
    public int durabilidad;
    public int durabilidadMaxima;
    public bool esAtaque;
    public bool esCuracion;
    public float valorCuracion;

    public Objeto(string nombre, Sprite icono = null,
        int dano = 0, int durabilidad = 0,
        bool esAtaque = false, bool esCuracion = false,
        float valorCuracion = 0f)
    {
        this.nombre = nombre;
        this.icono = icono;

        // Dynamic stats configuration based on item name to bypass unconfigured Inspector/Quest values
        string nombreNormalizado = string.IsNullOrEmpty(nombre) ? "" : nombre.Trim().ToLower();
        if (nombreNormalizado.Contains("regla"))
        {
            this.esAtaque = true;
            this.esCuracion = false;
            this.dano = 5;
            this.durabilidad = 6;
            this.durabilidadMaxima = 6;
            this.valorCuracion = 0f;
        }
        else if (nombreNormalizado.Contains("audifonos") || nombreNormalizado.Contains("audífonos"))
        {
            this.esAtaque = false;
            this.esCuracion = true;
            this.dano = 0;
            this.durabilidad = 6;
            this.durabilidadMaxima = 6;
            this.valorCuracion = 25f;
        }
        else if (nombreNormalizado.Contains("chocolate"))
        {
            this.esAtaque = false;
            this.esCuracion = true;
            this.dano = 0;
            this.durabilidad = 6;
            this.durabilidadMaxima = 6;
            this.valorCuracion = 35f;
        }
        else if (nombreNormalizado.Contains("termo"))
        {
            this.esAtaque = true;
            this.esCuracion = false;
            this.dano = 4;
            this.durabilidad = 6;
            this.durabilidadMaxima = 6;
            this.valorCuracion = 0f;
        }
        else if (nombreNormalizado.Contains("escudo"))
        {
            this.esAtaque = false;
            this.esCuracion = true;
            this.dano = 0;
            this.durabilidad = 6;
            this.durabilidadMaxima = 6;
            this.valorCuracion = 40f;
        }
        else if (nombreNormalizado.Contains("rosa"))
        {
            this.esAtaque = false;
            this.esCuracion = true;
            this.dano = 0;
            this.durabilidad = 6;
            this.durabilidadMaxima = 6;
            this.valorCuracion = 20f;
        }
        else if (nombreNormalizado.Contains("puños") || nombreNormalizado.Contains("puño"))
        {
            this.esAtaque = true;
            this.esCuracion = false;
            this.dano = 2;
            this.durabilidad = 999;
            this.durabilidadMaxima = 999;
            this.valorCuracion = 0f;
        }
        else
        {
            // Default fallback for any other custom items (keep parameters but apply average durability of 6 if 0 or 1 is specified)
            this.esAtaque = esAtaque;
            this.esCuracion = esCuracion;
            this.dano = dano;
            this.durabilidad = (durabilidad <= 1) ? 6 : durabilidad;
            this.durabilidadMaxima = this.durabilidad;
            this.valorCuracion = valorCuracion;
        }
    }
}

public class Inventario : MonoBehaviour
{
    public static Inventario Instance;

    [Header("UI")]
    public GameObject panelInventario;
    public Transform contenedorObjetos;
    public GameObject prefabSlot;
    public TextMeshProUGUI textoVacio;

    private readonly LinkedList<Objeto> objetos = new LinkedList<Objeto>();
    private bool abierto;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        EstilizarPanel(panelInventario);
        AjustarDiseñoUI();

        if (panelInventario != null)
        {
            panelInventario.SetActive(false);
        }
    }

    private void EstilizarPanel(GameObject panel)
    {
        if (panel == null) return;

        Image fondo = panel.GetComponent<Image>();
        if (fondo == null) fondo = panel.AddComponent<Image>();
        fondo.color = new Color(0.025f, 0.018f, 0.055f, 0.90f); // Midnight-blue premium semi-transparente

        Outline outline = panel.GetComponent<Outline>();
        if (outline == null) outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.55f); // Borde cyan neón elegante
        outline.effectDistance = new Vector2(2f, -2f);

        // Estilizar textos en el panel
        TextMeshProUGUI[] textos = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] != null)
            {
                textos[i].color = new Color(0.92f, 0.98f, 1f, 1f);
                textos[i].fontSize = Mathf.Max(textos[i].fontSize, 18f);
            }
        }
    }

    private void AjustarRectTransform(RectTransform rect, Vector2 posicion, Vector2 tamaño)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
    }

    private void AjustarDiseñoUI()
    {
        if (panelInventario != null)
        {
            // Panel de Inventario: tamaño 750x480 centrado
            AjustarRectTransform(panelInventario.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(750f, 480f));
        }

        if (textoVacio != null)
        {
            // Título o texto vacío
            AjustarRectTransform(textoVacio.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(600f, 100f));
            textoVacio.fontSize = 22f;
            textoVacio.alignment = TextAlignmentOptions.Center;
        }

        // Título del Inventario
        if (panelInventario != null)
        {
            Transform tituloTrans = panelInventario.transform.Find("TituloMochila");
            GameObject tituloGo;
            if (tituloTrans != null)
            {
                tituloGo = tituloTrans.gameObject;
            }
            else
            {
                tituloGo = new GameObject("TituloMochila", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                tituloGo.transform.SetParent(panelInventario.transform, false);
            }
            tituloGo.transform.localScale = Vector3.one;
            AjustarRectTransform(tituloGo.GetComponent<RectTransform>(), new Vector2(0f, 200f), new Vector2(600f, 50f));

            TextMeshProUGUI txtTitulo = tituloGo.GetComponent<TextMeshProUGUI>();
            if (txtTitulo != null)
            {
                if (textoVacio != null)
                {
                    txtTitulo.font = textoVacio.font;
                    txtTitulo.fontSharedMaterial = textoVacio.fontSharedMaterial;
                }
                txtTitulo.text = "MOCHILA DE OBJETOS";
                txtTitulo.color = new Color(0.25f, 0.88f, 1f, 1f); // Cyan brillante
                txtTitulo.fontSize = 28f;
                txtTitulo.fontStyle = FontStyles.Bold;
                txtTitulo.alignment = TextAlignmentOptions.Center;
            }
        }

        if (contenedorObjetos != null)
        {
            // Contenedor de objetos: centrado y bien espaciado
            AjustarRectTransform(contenedorObjetos.GetComponent<RectTransform>(), new Vector2(0f, -20f), new Vector2(660f, 320f));
            
            // Configurar Grid Layout Group para una rejilla perfecta
            UnityEngine.UI.GridLayoutGroup grid = contenedorObjetos.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (grid == null) grid = contenedorObjetos.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize = new Vector2(195f, 85f);
            grid.spacing = new Vector2(25f, 20f);
            grid.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.Flexible;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            if (abierto)
            {
                CerrarInventario();
            }
            else
            {
                AbrirInventario();
            }
        }
        else if (abierto && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1)))
        {
            CerrarInventario();
        }
    }

    public void AgregarObjeto(Objeto objeto)
    {
        if (objeto == null)
        {
            return;
        }

        objetos.AddLast(objeto);
        Debug.Log("Objeto agregado: " + objeto.nombre);
    }

    public bool TieneObjeto(string nombre)
    {
        foreach (Objeto obj in objetos)
        {
            if (obj.nombre == nombre)
            {
                return true;
            }
        }

        return false;
    }

    public void EliminarObjeto(string nombre)
    {
        LinkedListNode<Objeto> nodo = objetos.First;
        while (nodo != null)
        {
            LinkedListNode<Objeto> siguiente = nodo.Next;
            if (nodo.Value.nombre == nombre)
            {
                objetos.Remove(nodo);
                Debug.Log("Objeto eliminado: " + nombre);
                return;
            }

            nodo = siguiente;
        }

        Debug.Log("Objeto no encontrado: " + nombre);
    }

    public int GetCantidadObjetos()
    {
        return objetos.Count;
    }

    private void AbrirInventario()
    {
        if (panelInventario == null)
        {
            Debug.LogWarning("Inventario sin panel asignado. Se ignora apertura para evitar referencia rota.");
            return;
        }

        abierto = true;
        Time.timeScale = 0f;
        panelInventario.transform.SetAsLastSibling();
        panelInventario.SetActive(true);
        AjustarDiseñoUI();
        ActualizarUI();

        EcosAulaPromptUI.CrearBarraPrompts(panelInventario.transform, (AccionLogica.Cancelar, "Cerrar"));
    }

    private void CerrarInventario()
    {
        abierto = false;
        Time.timeScale = 1f;
        if (panelInventario != null)
        {
            panelInventario.SetActive(false);
        }
    }

    private void ActualizarUI()
    {
        if (contenedorObjetos == null)
        {
            return;
        }

        foreach (Transform hijo in contenedorObjetos)
        {
            Destroy(hijo.gameObject);
        }

        if (objetos.Count == 0)
        {
            if (textoVacio != null)
            {
                textoVacio.gameObject.SetActive(true);
                textoVacio.text = "Tu inventario está vacío";
            }
            return;
        }

        if (textoVacio != null)
        {
            textoVacio.gameObject.SetActive(false);
        }

        foreach (Objeto obj in objetos)
        {
            GameObject slot = prefabSlot != null
                ? Instantiate(prefabSlot, contenedorObjetos)
                : CrearSlotFallback(contenedorObjetos);

            slot.transform.localScale = Vector3.one;

            // Estilizar el fondo de la celda de slot
            Image imgSlot = slot.GetComponent<Image>();
            if (imgSlot != null)
            {
                imgSlot.color = new Color(0.08f, 0.06f, 0.16f, 0.9f);
                Outline outSlot = slot.GetComponent<Outline>();
                if (outSlot == null) outSlot = slot.AddComponent<Outline>();
                outSlot.effectColor = new Color(0.25f, 0.88f, 1f, 0.35f); // Borde neón suave
                outSlot.effectDistance = new Vector2(1.5f, -1.5f);
            }

            // Estilizar y posicionar el texto del nombre del objeto
            TextMeshProUGUI txtTMP = slot.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txtTMP != null)
            {
                txtTMP.text = obj.nombre;
                txtTMP.color = new Color(0.92f, 0.98f, 1f, 1f);
                txtTMP.fontSize = 15f;
                txtTMP.fontStyle = FontStyles.Bold;
                
                RectTransform rectText = txtTMP.GetComponent<RectTransform>();
                rectText.anchorMin = new Vector2(0f, 0.5f);
                rectText.anchorMax = new Vector2(1f, 0.5f);
                rectText.pivot = new Vector2(0f, 0.5f);
                rectText.anchoredPosition = new Vector2(65f, 12f); // Desplazado a la derecha del icono
                rectText.sizeDelta = new Vector2(-75f, 30f);
                rectText.localScale = Vector3.one;
            }

            // Estilizar y posicionar el icono del objeto
            Transform iconoTransform = slot.transform.Find("IconoObjeto");
            if (iconoTransform != null)
            {
                RectTransform rectIcono = iconoTransform.GetComponent<RectTransform>();
                rectIcono.anchorMin = new Vector2(0f, 0.5f);
                rectIcono.anchorMax = new Vector2(0f, 0.5f);
                rectIcono.pivot = new Vector2(0f, 0.5f);
                rectIcono.anchoredPosition = new Vector2(10f, 0f); // Alineado a la izquierda
                rectIcono.sizeDelta = new Vector2(45f, 45f);
                rectIcono.localScale = Vector3.one;

                Image icono = iconoTransform.GetComponent<Image>();
                if (icono != null)
                {
                    icono.raycastTarget = false;
                    if (obj.icono != null)
                    {
                        icono.sprite = obj.icono;
                        icono.color = Color.white;
                        icono.gameObject.SetActive(true);
                    }
                    else
                    {
                        // Color por defecto si no hay icono
                        icono.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);
                    }
                }
            }

            // Crear dinámicamente un texto para mostrar las estadísticas del objeto (daño, curación, durabilidad)
            Transform durabilidadTrans = slot.transform.Find("TextoDurabilidadSlot");
            GameObject durabilidadGo;
            if (durabilidadTrans != null)
            {
                durabilidadGo = durabilidadTrans.gameObject;
            }
            else
            {
                durabilidadGo = new GameObject("TextoDurabilidadSlot", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                durabilidadGo.transform.SetParent(slot.transform, false);
            }
            durabilidadGo.transform.localScale = Vector3.one;

            RectTransform rectDur = durabilidadGo.GetComponent<RectTransform>();
            rectDur.anchorMin = new Vector2(0f, 0.5f);
            rectDur.anchorMax = new Vector2(1f, 0.5f);
            rectDur.pivot = new Vector2(0f, 0.5f);
            rectDur.anchoredPosition = new Vector2(65f, -15f); // Posicionado debajo del nombre
            rectDur.sizeDelta = new Vector2(-75f, 25f);

            TextMeshProUGUI txtDur = durabilidadGo.GetComponent<TextMeshProUGUI>();
            if (txtTMP != null && txtDur != null)
            {
                txtDur.font = txtTMP.font;
                txtDur.fontSharedMaterial = txtTMP.fontSharedMaterial;
            }

            if (txtDur != null)
            {
                txtDur.fontSize = 11f;
                txtDur.color = new Color(0.7f, 0.8f, 0.9f, 0.8f);
                txtDur.alignment = TextAlignmentOptions.MidlineLeft;
                if (obj.esAtaque)
                {
                    txtDur.text = $"Dmg: {obj.dano} | Dur: {obj.durabilidad}/{obj.durabilidadMaxima}";
                }
                else if (obj.esCuracion)
                {
                    txtDur.text = $"Curar: {obj.valorCuracion} | Dur: {obj.durabilidad}";
                }
                else
                {
                    txtDur.text = "Objeto clave";
                }
            }
        }
    }

    private GameObject CrearSlotFallback(Transform parent)
    {
        GameObject slot = new GameObject("SlotInventarioFallback", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        slot.transform.SetParent(parent, false);

        Image fondo = slot.GetComponent<Image>();
        fondo.color = new Color(0.04f, 0.03f, 0.10f, 0.88f);
        fondo.raycastTarget = false;

        GameObject texto = new GameObject("Nombre", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        texto.transform.SetParent(slot.transform, false);
        RectTransform rectTexto = texto.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = new Vector2(10f, 4f);
        rectTexto.offsetMax = new Vector2(-10f, -4f);

        TextMeshProUGUI tmp = texto.GetComponent<TextMeshProUGUI>();
        tmp.color = Color.white;
        tmp.fontSize = 18f;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

        return slot;
    }
    public LinkedList<Objeto> GetObjetos()
{
    return objetos;
}
}
