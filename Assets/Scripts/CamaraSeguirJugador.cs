using UnityEngine;

public class CamaraSeguirJugador : MonoBehaviour
{
    public Transform jugador;

    public Vector3 offset = new Vector3(0f, 2.2f, -4f);
    public float suavizado = 6f;
    public float alturaMirada = 0.8f;

    void LateUpdate()
    {
        if (jugador == null) return;

        Vector3 posicionDeseada = jugador.position + offset;
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);

        Vector3 puntoMirada = jugador.position + Vector3.up * alturaMirada;
        transform.LookAt(puntoMirada);
    }
}