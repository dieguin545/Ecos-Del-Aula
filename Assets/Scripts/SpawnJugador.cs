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
        if (personajes == null || personajes.Length == 0 || puntoSpawn == null)
        {
            Debug.LogWarning("SpawnJugador no tiene personajes o punto de spawn configurado.");
            return;
        }

        int personajeSeleccionado = Mathf.Clamp(PlayerPrefs.GetInt("PersonajeSeleccionado", 0), 0, personajes.Length - 1);
        GameObject prefab = personajes[personajeSeleccionado];
        if (prefab == null)
        {
            Debug.LogWarning("SpawnJugador encontró un prefab de personaje vacío.");
            return;
        }

        GameObject jugador = Instantiate(prefab, puntoSpawn.position, Quaternion.identity);

        if (vcamPasillo != null)
        {
            vcamPasillo.Target.TrackingTarget = jugador.transform;
        }

        if (BullyingDatabase.Instance != null)
        {
            BullyingDatabase.Instance.SetPersonaje((PersonajeType)personajeSeleccionado);
        }

        ZonaConfort[] zonas = FindObjectsByType<ZonaConfort>();
        Transform panelAccionTransform = jugador.transform.Find("PanelAccion");
        GameObject panelAccion = panelAccionTransform != null ? panelAccionTransform.gameObject : null;

        foreach (ZonaConfort zona in zonas)
        {
            if (zona == null || panelAccion == null)
            {
                continue;
            }

            zona.panelAccion = panelAccion;
            zona.textoAccion = panelAccion.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}
