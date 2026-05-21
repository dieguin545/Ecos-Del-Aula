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

    // Lista enlazada de objetos
    private LinkedList<Objeto> objetos = new LinkedList<Objeto>();
    private bool abierto = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        panelInventario.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.JoystickButton3)) // Tecla I o Botón Y de Xbox
        {
            if (abierto)
                CerrarInventario();
            else
                AbrirInventario();
        }
        else if (abierto && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))) // Escape o Botón B de Xbox
        {
            CerrarInventario();
        }
    }

    public void AgregarObjeto(Objeto objeto)
    {
        objetos.AddLast(objeto);
        Debug.Log("Objeto agregado: " + objeto.nombre);
    }

    public bool TieneObjeto(string nombre)
    {
        foreach (Objeto obj in objetos)
        {
            if (obj.nombre == nombre)
                return true;
        }
        return false;
    }

    public void EliminarObjeto(string nombre)
{
    Debug.Log("Intentando eliminar: '" + nombre + "'");
    LinkedListNode<Objeto> nodo = objetos.First;
    while (nodo != null)
    {
        Debug.Log("Comparando con: '" + nodo.Value.nombre + "'");
        if (nodo.Value.nombre == nombre)
        {
            objetos.Remove(nodo);
            Debug.Log("Objeto eliminado: " + nombre);
            return;
        }
        nodo = nodo.Next;
    }
    Debug.Log("Objeto NO encontrado: " + nombre);
}

    public int GetCantidadObjetos()
    {
        return objetos.Count;
    }

    private void AbrirInventario()
    {
        abierto = true;
        Time.timeScale = 0f;
        panelInventario.SetActive(true);
        ActualizarUI();

        EcosAulaPromptUI.CrearBarraPrompts(panelInventario.transform,
            (AccionLogica.Cancelar, "Cerrar"));
    }

    private void CerrarInventario()
    {
        abierto = false;
        Time.timeScale = 1f;
        panelInventario.SetActive(false);
    }

    private void ActualizarUI()
{
    // Limpia slots anteriores
    foreach (Transform hijo in contenedorObjetos)
        Destroy(hijo.gameObject);

    if (objetos.Count == 0)
    {
        textoVacio.gameObject.SetActive(true);
        textoVacio.text = "Tu inventario está vacío";
        return;
    }

    textoVacio.gameObject.SetActive(false);

    foreach (Objeto obj in objetos)
{
    GameObject slot = Instantiate(prefabSlot, contenedorObjetos);
    
    // Asigna el nombre
    TextMeshProUGUI[] textos = slot.GetComponentsInChildren<TextMeshProUGUI>();
    if (textos.Length >= 1)
        textos[0].text = obj.nombre;

    // Asigna el icono buscando por nombre
    Transform iconoTransform = slot.transform.Find("IconoObjeto");
    if (iconoTransform != null && obj.icono != null)
    {
        Image icono = iconoTransform.GetComponent<Image>();
        if (icono != null)
            icono.sprite = obj.icono;
    }
}
}
}