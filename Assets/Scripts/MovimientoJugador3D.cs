using UnityEngine;

public class MoverCapsula3D : MonoBehaviour
{
    public float velocidad = 4f;

    private Rigidbody rb;
    private Vector3 direccion;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // o sea para A y D lets cook bruh!!!!
        float vertical = Input.GetAxisRaw("Vertical");     // este para  W y S

        direccion = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        Vector3 nuevaPosicion = rb.position + direccion * velocidad * Time.fixedDeltaTime;
        rb.MovePosition(nuevaPosicion);
    }
}