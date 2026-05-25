using UnityEngine;

public class NPCPerseguidor : NPCBase
{
    public GameObject burbujaDialogo;
    public TMPro.TextMeshProUGUI textoBurbuja;
    public float velocidad = 2f;
    private Transform jugador;
    private bool persiguiendo = false;
    private float distanciaMaxima = 25f;
    private Vector3 posicionOriginal;
    private Rigidbody2D rb;

    void Start()
    {
        posicionOriginal = transform.position;
        burbujaDialogo.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();

        if (persiguiendo && jugador != null)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);

            if (distancia > distanciaMaxima)
            {
                persiguiendo = false;
                Vector3 direccionRegreso = (posicionOriginal - transform.position).normalized;
                rb.MovePosition(transform.position + direccionRegreso * velocidad * Time.deltaTime);
            }
            else
            {
                Vector3 direccion = (jugador.position - transform.position).normalized;
                rb.MovePosition(transform.position + direccion * velocidad * Time.deltaTime);
            }
        }
        else if (!persiguiendo && Vector3.Distance(transform.position, posicionOriginal) > 0.1f)
        {
            Vector3 direccionRegreso = (posicionOriginal - transform.position).normalized;
            rb.MovePosition(transform.position + direccionRegreso * velocidad * Time.deltaTime);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugador = other.transform;
            persiguiendo = true;
            base.OnTriggerEnter2D(other);
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugador = null;
            persiguiendo = false;
            base.OnTriggerExit2D(other);
        }
    }

    protected override void Interactuar()
    {
        bool esNeutro = Random.Range(0f, 1f) < 0.2f;
        BullyingMessage mensaje = BullyingDatabase.Instance.GetMensaje(esNeutro);

        if (mensaje != null)
        {
            MostrarBurbuja(mensaje.mensaje);
            if (!esNeutro)
            SistemaEvidencia.Instance.SetUltimoMensaje(mensaje.mensaje, true);

            if (!esNeutro && mensaje.ansiedadQueGenera > 0)
                AnxietySystem.Instance.IncreaseAnxiety(mensaje.ansiedadQueGenera);
        }
    }

    private void MostrarBurbuja(string mensaje)
    {
        textoBurbuja.text = mensaje;
        burbujaDialogo.SetActive(true);
        Invoke("OcultarBurbuja", 3f);
    }

    private void OcultarBurbuja()
    {
        burbujaDialogo.SetActive(false);
    }
}
