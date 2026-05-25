using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class Objeto
{
    public string nombre;
    public Sprite icono;

    public Objeto(string nombre, Sprite icono = null)
    {
        this.nombre = nombre;
        this.icono = icono;
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

        if (panelInventario != null)
        {
            panelInventario.SetActive(false);
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
        else if (abierto && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)))
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
        panelInventario.SetActive(true);
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

            TextMeshProUGUI[] textos = slot.GetComponentsInChildren<TextMeshProUGUI>();
            if (textos.Length >= 1)
            {
                textos[0].text = obj.nombre;
            }

            Transform iconoTransform = slot.transform.Find("IconoObjeto");
            if (iconoTransform != null && obj.icono != null)
            {
                Image icono = iconoTransform.GetComponent<Image>();
                if (icono != null)
                {
                    icono.sprite = obj.icono;
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
}
