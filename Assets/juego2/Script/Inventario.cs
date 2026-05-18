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
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (abierto)
                CerrarInventario();
            else
                AbrirInventario();
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
        LinkedListNode<Objeto> nodo = objetos.First;
        while (nodo != null)
        {
            if (nodo.Value.nombre == nombre)
            {
                objetos.Remove(nodo);
                return;
            }
            nodo = nodo.Next;
        }
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
            TextMeshProUGUI[] textos = slot.GetComponentsInChildren<TextMeshProUGUI>();
            if (textos.Length >= 2)
            {
                textos[0].text = obj.nombre;
            }
            if (obj.icono != null)
            {
                Image imagen = slot.GetComponentInChildren<Image>();
                if (imagen != null)
                    imagen.sprite = obj.icono;
            }
        }
    }
}