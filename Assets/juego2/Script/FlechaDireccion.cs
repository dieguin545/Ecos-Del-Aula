using UnityEngine;

public class FlechaDireccion : MonoBehaviour
{
    public float distanciaDeteccion = 35f;
    private Transform jugador;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            sr.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            sr.enabled = false;
    }
}