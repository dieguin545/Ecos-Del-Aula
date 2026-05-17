using UnityEngine;

public class Bala : MonoBehaviour
{
    public float tiempoVida = 3f;

    private void Start()
    {
        Destroy(gameObject, tiempoVida);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemigo"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}