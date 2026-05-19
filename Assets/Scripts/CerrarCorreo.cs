using UnityEngine;
public class CerrarCorreo : MonoBehaviour
{
    public GameObject VentanaCorreo;
    private GestorVentanasPC gestorVentanas;

    void Start()
    {
        gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (VentanaCorreo != null)
        {
            VentanaCorreo.SetActive(false);
        }
    }

    public void TogglePanelCorreo()
    {
        if (VentanaCorreo == null)
        {
            return;
        }

        if (gestorVentanas == null)
        {
            gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();
        }

        if (gestorVentanas != null)
        {
            gestorVentanas.AlternarVentana(VentanaCorreo);
            return;
        }

        VentanaCorreo.SetActive(!VentanaCorreo.activeSelf);
    }
}
