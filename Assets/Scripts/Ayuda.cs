using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelAyuda : MonoBehaviour
{
    public TMP_Text textoAyuda;

    [TextArea(3,10)]
    public string[] mensajes;

    private int paginaActual = 0;

    void Start()
    {
        MostrarPagina();
    }

    public void SiguientePagina()
    {
        if(paginaActual < mensajes.Length - 1)
        {
            paginaActual++;
            MostrarPagina();
        }
    }

    public void PaginaAnterior()
    {
        if(paginaActual > 0)
        {
            paginaActual--;
            MostrarPagina();
        }
    }

    void MostrarPagina()
    {
        textoAyuda.text = mensajes[paginaActual];
    }
}
