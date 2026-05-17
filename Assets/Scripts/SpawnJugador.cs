using UnityEngine;
using Unity.Cinemachine;
using TMPro;
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
    // Busca todas las zonas de confort y les asigna el panel de accion del jugador
ZonaConfort[] zonas = FindObjectsOfType<ZonaConfort>();
GameObject panelAccion = jugador.transform.Find("PanelAccion").gameObject;
foreach (ZonaConfort zona in zonas)
{
    zona.panelAccion = panelAccion;
    zona.textoAccion = panelAccion.GetComponentInChildren<TextMeshProUGUI>();
}
    }
}