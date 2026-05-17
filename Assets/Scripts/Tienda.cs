using UnityEngine;

public class Tienda : MonoBehaviour
{
public GameObject panelTienda;
    void Start()
    {
        panelTienda.SetActive(false);
    }
    public void abrir()
    {
        if (panelTienda.activeSelf)
        {
            panelTienda.SetActive(false);
        }
        else
        {
            panelTienda.SetActive(true);
        }
    }

 
}
