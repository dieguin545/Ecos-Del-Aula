using UnityEngine;

public class SpawnJugador : MonoBehaviour
{
    public GameObject[] personajes; // prefabs o personajes

    void Start()
    {
        int id = PlayerPrefs.GetInt("Personaje");

        Instantiate(personajes[id], transform.position, Quaternion.identity);
    }
}