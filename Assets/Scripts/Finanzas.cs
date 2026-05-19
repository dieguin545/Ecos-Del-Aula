using UnityEngine;

public class Finanzas : MonoBehaviour
{
    public GameObject Finanza;
    private GestorVentanasPC gestorVentanas;

    void Start()
    {
        gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (Finanza != null)
        {
            Finanza.SetActive(false);
        }
    }
    public void abrir()
    {
        if (Finanza == null)
        {
            return;
        }

        if (gestorVentanas == null)
        {
            gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();
        }

        if (gestorVentanas != null)
        {
            gestorVentanas.AlternarVentana(Finanza);
            return;
        }

        Finanza.SetActive(!Finanza.activeSelf);
    }
}
