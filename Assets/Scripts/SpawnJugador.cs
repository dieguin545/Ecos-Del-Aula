using UnityEngine;
using Unity.Cinemachine;

public class SpawnJugador : MonoBehaviour
{
    public GameObject[] personajes;
    public Transform puntoSpawn;
    public CinemachineCamera vcamPasillo;

    void Start()
    {
        int personajeSeleccionado = PlayerPrefs.GetInt("PersonajeSeleccionado", 0);

        GameObject jugador = Instantiate(
            personajes[personajeSeleccionado],
            puntoSpawn.position,
            Quaternion.identity
        );

        vcamPasillo.Target.TrackingTarget = jugador.transform;
        // el database sabe el personaje activo
            PersonajeType tipo = (PersonajeType)personajeSeleccionado;
    BullyingDatabase.Instance.SetPersonaje(tipo);
    }
}