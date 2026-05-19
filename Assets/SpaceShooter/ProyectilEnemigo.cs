using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProyectilEnemigo : MonoBehaviour
{
    [SerializeField] private float tiempoVida = 5f;

    private int danio = 1;
    private Rigidbody rb;
    private TrailRenderer trail;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, tiempoVida);
        PrepararTrail();
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
            AccesibilidadSpaceShooter.TipoDaltonismoActual
        );

        if (trail != null)
        {
            Color color;

            switch (AccesibilidadSpaceShooter.TipoDaltonismoActual)
            {
                case TipoDaltonismo.Protanopia:
                case TipoDaltonismo.Deuteranopia:
                    color = EstiloVisualSpaceShooter.ColorAtaqueEnemigoDaltonico;
                    break;
                case TipoDaltonismo.Tritanopia:
                    color = new Color(1f, 0.18f, 0.55f);
                    break;
                case TipoDaltonismo.Acromatopsia:
                    color = Color.white;
                    break;
                default:
                    color = EstiloVisualSpaceShooter.ColorAtaqueEnemigo;
                    break;
            }
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
        }
    }

    private void PrepararTrail()
    {
        if (AccesibilidadSpaceShooter.ReducirEfectosActivo)
        {
            return;
        }

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.18f;
        trail.endWidth = 0.03f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
    }
}
