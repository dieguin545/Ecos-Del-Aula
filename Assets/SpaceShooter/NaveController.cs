using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NaveController : MonoBehaviour
{
    [Header("Movimiento 3D")]
    public float velocidad = 12f;
    public float velocidadTurbo = 24f;
    public float velocidadVertical = 12f;
    public float limiteMapa = 35f;
    public float alturaMinima = -20f;
    public float alturaMaxima = 20f;

    [Header("Esquive")]
    public float fuerzaEsquive = 12f;
    public float duracionEsquive = 0.18f;
    public float cooldownEsquive = 0.45f;
    public KeyCode teclaEsquivarIzquierda = KeyCode.Q;
    public KeyCode teclaEsquivarDerecha = KeyCode.E;

    [Header("Controles")]
    public KeyCode teclaSubir = KeyCode.Space;
    public KeyCode teclaBajar = KeyCode.LeftControl;
    public KeyCode teclaTurbo = KeyCode.LeftShift;

    [Header("Rotacion visual")]
    public float velocidadRotacion = 12f;
    public float inclinacionLateral = 35f;
    public float inclinacionVertical = 20f;
    public Vector3 rotacionExtraModelo = new Vector3(90f, 0f, 0f);

    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public float cooldown = 0.25f;
    public float velocidadBala = 35f;

    [Header("Camara")]
    public Camera camara;

    private Rigidbody rb;

    private Vector3 direccionMovimiento;
    private Vector3 direccionEsquive;

    private float tiempoEsquive;
    private float siguienteEsquive;
    private float siguienteDisparo;

    private float entradaHorizontal;
    private float entradaAdelante;
    private float entradaVertical;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearDamping = 2f;
        rb.angularDamping = 5f;
        rb.freezeRotation = true;

        if (camara == null)
        {
            camara = Camera.main;
        }
    }

    private void Update()
    {
        LeerMovimiento();
        LeerEsquive();
        LeerDisparo();
        RotarNave();
    }

    private void FixedUpdate()
    {
        MoverNave();
    }

    private void LeerMovimiento()
    {
        if (camara == null)
        {
            direccionMovimiento = Vector3.zero;
            return;
        }

        entradaHorizontal = Input.GetAxisRaw("Horizontal");
        entradaAdelante = Input.GetAxisRaw("Vertical");
        entradaVertical = 0f;

        if (Input.GetKey(teclaSubir))
        {
            entradaVertical = 1f;
        }
        else if (Input.GetKey(teclaBajar))
        {
            entradaVertical = -1f;
        }

        Vector3 adelanteCamara = camara.transform.forward;
        Vector3 derechaCamara = camara.transform.right;

        direccionMovimiento =
            adelanteCamara * entradaAdelante +
            derechaCamara * entradaHorizontal +
            Vector3.up * entradaVertical;

        if (direccionMovimiento.magnitude > 1f)
        {
            direccionMovimiento.Normalize();
        }
    }

    private void MoverNave()
    {
        float velocidadActual = Input.GetKey(teclaTurbo) ? velocidadTurbo : velocidad;

        Vector3 movimiento = direccionMovimiento * velocidadActual;

        if (entradaVertical != 0f)
        {
            movimiento += Vector3.up * entradaVertical * velocidadVertical;
        }

        if (tiempoEsquive > 0f)
        {
            movimiento += direccionEsquive * fuerzaEsquive;
            tiempoEsquive -= Time.fixedDeltaTime;
        }

        Vector3 nuevaPosicion = rb.position + movimiento * Time.fixedDeltaTime;

        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x, -limiteMapa, limiteMapa);
        nuevaPosicion.y = Mathf.Clamp(nuevaPosicion.y, alturaMinima, alturaMaxima);
        nuevaPosicion.z = Mathf.Clamp(nuevaPosicion.z, -limiteMapa, limiteMapa);

        rb.MovePosition(nuevaPosicion);
    }

    private void LeerEsquive()
    {
        if (Time.time < siguienteEsquive)
        {
            return;
        }

        if (camara == null)
        {
            return;
        }

        if (Input.GetKeyDown(teclaEsquivarIzquierda))
        {
            direccionEsquive = -camara.transform.right.normalized;
            tiempoEsquive = duracionEsquive;
            siguienteEsquive = Time.time + cooldownEsquive;
        }

        if (Input.GetKeyDown(teclaEsquivarDerecha))
        {
            direccionEsquive = camara.transform.right.normalized;
            tiempoEsquive = duracionEsquive;
            siguienteEsquive = Time.time + cooldownEsquive;
        }
    }

    private void RotarNave()
    {
        if (camara == null)
        {
            return;
        }

        Vector3 direccionMirada = camara.transform.forward;

        if (direccionMirada.sqrMagnitude < 0.01f)
        {
            return;
        }

        float inclinacionZ = -entradaHorizontal * inclinacionLateral;
        float inclinacionX = -entradaVertical * inclinacionVertical;

        if (tiempoEsquive > 0f)
        {
            inclinacionZ *= 2f;
        }

        Quaternion rotacionBase = Quaternion.LookRotation(direccionMirada, Vector3.up);

        Quaternion rotacionModelo = Quaternion.Euler(
            rotacionExtraModelo.x + inclinacionX,
            rotacionExtraModelo.y,
            rotacionExtraModelo.z + inclinacionZ
        );

        Quaternion rotacionFinal = rotacionBase * rotacionModelo;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionFinal,
            velocidadRotacion * Time.deltaTime
        );
    }

    private void LeerDisparo()
    {
        if (Input.GetMouseButton(0) && Time.time >= siguienteDisparo)
        {
            Disparar();
            siguienteDisparo = Time.time + cooldown;
        }
    }

    private void Disparar()
    {
        if (balaPrefab == null || puntoDisparo == null || camara == null)
        {
            return;
        }

        Vector3 puntoObjetivo = ObtenerPuntoDeApunte();
        Vector3 direccionDisparo = (puntoObjetivo - puntoDisparo.position).normalized;

        GameObject bala = Instantiate(
            balaPrefab,
            puntoDisparo.position,
            Quaternion.LookRotation(direccionDisparo)
        );

        Rigidbody rbBala = bala.GetComponent<Rigidbody>();

        if (rbBala != null)
        {
            rbBala.linearVelocity = direccionDisparo * velocidadBala;
        }
    }

    private Vector3 ObtenerPuntoDeApunte()
    {
        Ray rayo = camara.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit[] impactos = Physics.RaycastAll(
            rayo,
            500f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        System.Array.Sort(impactos, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit impacto in impactos)
        {
            if (impacto.collider.transform == transform)
            {
                continue;
            }

            if (impacto.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            return impacto.point;
        }

        return rayo.origin + rayo.direction * 500f;
    }
}