using UnityEngine;

public class Bala : MonoBehaviour
{
    [SerializeField] private float tiempoVida = 3f;
    [SerializeField] private int danio = 1;

    private void Start()
    {
        EstiloVisualSpaceShooter.AplicarABala(gameObject);
        Destroy(gameObject, tiempoVida);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemigo"))
        {
            Meteorito meteorito = other.GetComponent<Meteorito>();

            if (meteorito != null)
            {
                meteorito.RecibirImpacto(danio);
            }
            else
            {
                Destroy(other.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
