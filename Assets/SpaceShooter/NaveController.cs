using UnityEngine;

public class NaveController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 6f;
    public float limiteX = 300f;
    public float limiteZ = 300f;

    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public float cooldown = 0.25f;
    public Camera camara;

    Rigidbody rb;
    float tiempoUltimoDisparo;
    bool invencible = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (camara == null)
            camara = Camera.main;
    }

    void Update()
    {
        if (!GameManager.instancia.juegoActivo) return;
        Disparar();

    }

    void FixedUpdate()
    {
        if (!GameManager.instancia.juegoActivo) return;
        Mover();
    }

    void Mover()
{
    float inputX = Input.GetAxis("Horizontal");
    float inputZ = Input.GetAxis("Vertical");

    // Toma los ejes de la camara pero ignora Y
    Vector3 adelante = camara.transform.forward;
    Vector3 derecha = camara.transform.right;

    adelante.y = 0f;
    derecha.y = 0f;
    adelante.Normalize();
    derecha.Normalize();

    Vector3 direccion = (adelante * inputZ + derecha * inputX);

    Vector3 pos = rb.position + direccion * velocidad * Time.fixedDeltaTime;
    rb.MovePosition(pos);

    // Rotar la nave hacia donde se mueve
    if (direccion != Vector3.zero)
    {
        Quaternion rotObjetivo = Quaternion.LookRotation(direccion, Vector3.up);
        Quaternion rotFinal = rotObjetivo * Quaternion.Euler(90f, 0f, 0f);
        rb.rotation = Quaternion.Slerp(rb.rotation, rotFinal, 10f * Time.fixedDeltaTime);
    }
}

    void Disparar()
{
    if (Input.GetMouseButtonDown(0) &&
        Time.time > tiempoUltimoDisparo + cooldown)
    {
        // Toma la direccion de la camara pero aplana en Y
        Vector3 direccionBala = camara.transform.forward;
        direccionBala.y = 0f;
        direccionBala.Normalize();

        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);
        bala.GetComponent<Bala>().SetDireccion(direccionBala);
        tiempoUltimoDisparo = Time.time;
    }
}


    void OnTriggerEnter(Collider otro)
    {
        if (invencible) return;

        if (otro.CompareTag("Meteorito"))
        {
            Destroy(otro.gameObject);
            GameManager.instancia.PerderVida();
            StartCoroutine(InvencibilidadTemporal());
        }

        if (otro.CompareTag("Corazon"))
        {
            Destroy(otro.gameObject);
            GameManager.instancia.GanarVida();
        }
    }

    System.Collections.IEnumerator InvencibilidadTemporal()
    {
        invencible = true;
        yield return new WaitForSeconds(1.5f);
        invencible = false;
    }
}
