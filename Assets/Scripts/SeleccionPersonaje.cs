using UnityEngine;

public class SeleccionPersonaje : MonoBehaviour
{
    public GameObject[] personajes;
    private int indiceActual = 0;

    void Start()
    {
        ActualizarVisual();
    }

    public void Siguiente()
    {
        indiceActual = (indiceActual + 1) % personajes.Length;
        ActualizarVisual();
    }

    public void Anterior()
    {
        indiceActual--;
        if (indiceActual < 0)
        {
            indiceActual = personajes.Length - 1;
        }
        ActualizarVisual();
    }

    void ActualizarVisual()
    {
        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(false);
        }

        personajes[indiceActual].SetActive(true);
    }

    public void Seleccionar()
    {
        PlayerPrefs.SetInt("PersonajeSeleccionado", indiceActual);
        PlayerPrefs.Save();
        Debug.Log("Personaje seleccionado: " + indiceActual);
    }
}
