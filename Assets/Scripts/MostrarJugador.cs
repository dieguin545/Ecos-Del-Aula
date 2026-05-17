using UnityEngine;

public class MostrarJugador : MonoBehaviour
{
    public GameObject[] personajes;

    void Start()
    {
        int id = PlayerPrefs.GetInt("PersonajeSeleccionado", 0);

        for (int i = 0; i < personajes.Length; i++)
        {
            personajes[i].SetActive(false);
        }

        personajes[id].SetActive(true);
        Debug.Log("ID: " + id);
    }
}