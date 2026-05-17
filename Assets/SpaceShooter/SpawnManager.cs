using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject meteoritoPrefab;
    public float tiempoInicial = 2f;
    public float tiempoMinimo = 0.5f;
    public float reduccion = 0.05f;
    public float radioSpawn = 26f;

    float timer;
    float tiempoActual;

    void Start()
    {
        tiempoActual = tiempoInicial;
    }

    void Update()
    {
        if (!GameManager.instancia.juegoActivo) return;

        timer += Time.deltaTime;
        if (timer >= tiempoActual)
        {
            timer = 0f;
            SpawnMeteorito();
            tiempoActual = Mathf.Max(tiempoMinimo, tiempoActual - reduccion);
        }
    }

    void SpawnMeteorito()
    {
        // Spawnea en los bordes del mapa en angulo aleatorio
        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 pos = new Vector3(
            Mathf.Cos(angulo) * radioSpawn,
            Random.Range(0f, 3f),
            Mathf.Sin(angulo) * radioSpawn
        );
        Instantiate(meteoritoPrefab, pos, Random.rotation);
    }
}