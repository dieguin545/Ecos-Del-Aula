using UnityEngine;
public class Ayudas : MonoBehaviour
{
    public GameObject ayuda;
    private GestorVentanasPC gestorVentanas;

    void Start()
    {
        gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (ayuda != null)
        {
            ayuda.SetActive(false);
        }
    }

    public void abrir()
    {
        if (ayuda == null)
        {
            return;
        }

        if (gestorVentanas == null)
        {
            gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();
        }

        if (gestorVentanas != null)
        {
            gestorVentanas.AlternarVentana(ayuda);
            return;
        }

        ayuda.SetActive(!ayuda.activeSelf);
    }
}
