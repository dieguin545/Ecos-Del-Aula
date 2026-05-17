using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProyectilEnemigo : MonoBehaviour
{
    [SerializeField] private float tiempoVida = 5f;

    private int danio = 1;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, tiempoVida);
        AplicarAccesibilidadVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Nave"))
        {
            return;
        }

        NaveController nave = other.GetComponent<NaveController>();

        if (nave == null || !nave.EstaEsquivando)
        {
            if (GameManager.instancia != null)
            {
                GameManager.instancia.PerderVida(danio);
            }
        }

        Destroy(gameObject);
    }

    public void Inicializar(Vector3 direccion, float velocidad, int danio)
    {
        this.danio = Mathf.Max(1, danio);

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = direccion.normalized * velocidad;
        }

    }

    public void AplicarAccesibilidadVisual()
    {
        EstiloVisualSpaceShooter.AplicarAProyectilEnemigo(
            gameObject,
            AccesibilidadSpaceShooter.ModoDaltonicoActivo
        );
    }
}
