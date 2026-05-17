using UnityEngine;

public class Meteorito : MonoBehaviour
{
    public float velocidad = 4f;
    public float velocidadRotacion = 80f;
    public GameObject corazonPrefab;

    [Range(0f, 1f)]
    public float probabilidadCorazon = 0.25f;

    Transform nave;
    Vector3 direccion;

    void Start()
    {
        // Busca la nave y va directo hacia ella
        GameObject objNave = GameObject.FindWithTag("Nave");
        if (objNave != null)
        {
            nave = objNave.transform;
            direccion = (nave.position - transform.position).normalized;
        }
    }

    void Update()
    {
        // Actualiza direccion gradualmente para seguir un poco a la nave
        if (nave != null)
        {
            Vector3 dirObjetivo = (nave.position - transform.position).normalized;
            direccion = Vector3.Lerp(direccion, dirObjetivo, 0.5f * Time.deltaTime);
        }

        transform.position += direccion * velocidad * Time.deltaTime;

        transform.Rotate(
            velocidadRotacion * Time.deltaTime,
            velocidadRotacion * 0.6f * Time.deltaTime,
            velocidadRotacion * 0.4f * Time.deltaTime
        );

        // Destruir si se aleja demasiado
        if (Vector3.Distance(transform.position, Vector3.zero) > 60f)
            Destroy(gameObject);
    }

    public void Morir()
    {
        if (Random.value <= probabilidadCorazon)
            Instantiate(corazonPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}