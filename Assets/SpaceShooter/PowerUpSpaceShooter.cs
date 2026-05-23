using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PowerUpSpaceShooter : MonoBehaviour
{
    [Header("Tipo")]
    [SerializeField] private TipoPowerUp tipo = TipoPowerUp.Vida;

    [Header("Movimiento")]
    [SerializeField] protected float velocidad = 2f;
    [SerializeField] protected float tiempoVida = 8f;
    [SerializeField] private float velocidadRotacion = 90f;

    [Header("Efectos")]
    [SerializeField] private float duracionEscudo = 6f;
    [SerializeField] private float duracionDisparoMejorado = 6f;
    [SerializeField] private float radioLimpiezaDigital = 8f;
    [SerializeField] private int puntosExtra = 150;

    private Vector3 direccion;
    private bool recogido;

    public TipoPowerUp Tipo => tipo;

    protected virtual void Awake()
    {
        Collider collider = GetComponent<Collider>();

        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    protected virtual void Start()
    {
        direccion = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.25f, 0.25f),
            Random.Range(-1f, 1f)
        ).normalized;

        FirewallDelAulaVisuales.AplicarPowerUp(gameObject, tipo);
        Destroy(gameObject, tiempoVida);
    }

    protected virtual void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;
        transform.Rotate(0f, velocidadRotacion * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        NaveController nave = other.GetComponentInParent<NaveController>();

        if (nave != null)
        {
            IntentarRecoger(nave);
        }
    }

    public void ConfigurarTipo(TipoPowerUp nuevoTipo)
    {
        tipo = nuevoTipo;
        gameObject.name = "PowerUp_" + nuevoTipo;
        FirewallDelAulaVisuales.AplicarPowerUp(gameObject, tipo);
    }

    public bool IntentarRecoger(NaveController nave)
    {
        if (recogido || nave == null || GameManager.instancia == null)
        {
            return false;
        }

        recogido = true;
        GameManager gameManager = GameManager.instancia;

        switch (tipo)
        {
            case TipoPowerUp.Vida:
                if (!gameManager.RecuperarVida())
                {
                    gameManager.RegistrarPuntosExtra(puntosExtra);
                }
                break;
            case TipoPowerUp.Dash:
                nave.RecuperarCargaDash(1);
                nave.RecuperarEnergiaTurbo(25f);
                break;
            case TipoPowerUp.Escudo:
                gameManager.ActivarEscudo(duracionEscudo);
                break;
            case TipoPowerUp.DisparoMejorado:
                nave.ActivarDisparoMejorado(duracionDisparoMejorado);
                break;
            case TipoPowerUp.LimpiezaDigital:
                EjecutarLimpiezaDigital();
                break;
            case TipoPowerUp.Puntos:
                gameManager.RegistrarPuntosExtra(puntosExtra);
                break;
        }

        gameManager.RegistrarPowerUpRecogido(tipo);
        Destroy(gameObject);
        return true;
    }

    private void EjecutarLimpiezaDigital()
    {
        Collider[] impactos = Physics.OverlapSphere(
            transform.position,
            radioLimpiezaDigital,
            ~0,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < impactos.Length; i++)
        {
            Meteorito amenaza = impactos[i].GetComponentInParent<Meteorito>();

            if (amenaza != null)
            {
                amenaza.RecibirImpacto(999);
                continue;
            }

            ProyectilEnemigo proyectil = impactos[i].GetComponentInParent<ProyectilEnemigo>();

            if (proyectil != null)
            {
                Destroy(proyectil.gameObject);
            }
        }
    }
}
