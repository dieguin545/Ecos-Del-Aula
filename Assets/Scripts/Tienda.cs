using UnityEngine;

public class Tienda : MonoBehaviour
{
    public GameObject panelTienda;
    private GestorVentanasPC gestorVentanas;

    void Start()
    {
        gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (panelTienda != null)
        {
            panelTienda.SetActive(false);
        }
    }

    public void abrir()
    {
        if (panelTienda == null)
        {
            return;
        }

        if (gestorVentanas == null)
        {
            gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();
        }

        if (gestorVentanas != null)
        {
            gestorVentanas.AlternarVentana(panelTienda);
            return;
        }

        panelTienda.SetActive(!panelTienda.activeSelf);
    }
}
