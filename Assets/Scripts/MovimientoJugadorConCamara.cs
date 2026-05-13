using UnityEngine;

public class MovimientoJugadorConCamara : MonoBehaviour
{
    public float velocidad = 4f;
    public Transform camara;

    private Rigidbody rb;
    private Vector3 direccion;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 adelante = camara.forward;
        adelante.y = 0f;
        adelante.Normalize();

        Vector3 derecha = camara.right;
        derecha.y = 0f;
        derecha.Normalize();

        direccion = (adelante * vertical + derecha * horizontal).normalized;
    }

    void FixedUpdate()
    {
        Vector3 nuevaPosicion = rb.position + direccion * velocidad * Time.fixedDeltaTime;
        rb.MovePosition(nuevaPosicion);
    }
}