using UnityEngine;

public class Corazon : MonoBehaviour
{
    public float velocidad = 2f;
    public float tiempoVida = 8f;
    Vector3 direccion;

    void Start()
    {
        // Cae lentamente en una direccion aleatoria
        direccion = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);
    }
}