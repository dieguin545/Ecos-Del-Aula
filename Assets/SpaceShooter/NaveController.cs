using UnityEngine;
using UnityEngine.Serialization;
using System;

[RequireComponent(typeof(Rigidbody))]
public class NaveController : MonoBehaviour
{
    [Header("Movimiento 3D")]
    public float velocidad = 12f;
    public float velocidadTurbo = 19f;
    public float velocidadVertical = 12f;
    [FormerlySerializedAs("limiteMapa")] public float limiteX = 68f;
    public float limiteZ = 68f;
    public float alturaMinima = -34f;
    public float alturaMaxima = 34f;

    [Header("Turbo")]
    [SerializeField] private float energiaTurboMaxima = 100f;
    [SerializeField] private float consumoTurboPorSegundo = 25f;
    [SerializeField] private float regeneracionTurboPorSegundo = 15f;
    [SerializeField] private float retrasoRegeneracionTurbo = 0.75f;
    [SerializeField] private float bloqueoTurboAlVaciar = 0.6f;
    [SerializeField] private ParticleSystem humoTurbo;

    [Header("Esquive")]
    public float fuerzaEsquive = 28f;
    public float duracionEsquive = 0.22f;
    public float cooldownEsquive = 0.55f;
    [SerializeField] private int cargasEsquiveMaximas = 3;
    [SerializeField] private float recargaCargaSegundos = 2.8f;
    [SerializeField] private float radioRecoleccionPowerUps = 3.25f;
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
    private float temporizadorRecargaEsquive;
    private float cooldownBase;
    private float velocidadBalaBase;
    private float tiempoRestanteDisparoMejorado;
    private float multiplicadorCooldownDisparo = 1f;
    private float multiplicadorVelocidadBala = 1f;
    private float energiaTurboActual;
    private float siguienteRegeneracionTurbo;
    private float turboBloqueadoHasta;
    private int cargasEsquiveActuales;

    private float entradaHorizontal;
    private float entradaAdelante;
    private float entradaVertical;

    public event Action<int, int, float> AlActualizarDash;
    public event Action<float, float> AlActualizarTurbo;

    public bool EstaEsquivando => tiempoEsquive > 0f;

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

        cooldownBase = cooldown;
        velocidadBalaBase = velocidadBala;
        energiaTurboActual = energiaTurboMaxima;
        cargasEsquiveActuales = cargasEsquiveMaximas;
        PrepararRadioRecoleccionPowerUps();
        PrepararHumoTurbo();
        EstiloVisualSpaceShooter.AplicarANave(gameObject);
    }

    private void Update()
    {
        if (!EstaPartidaActiva())
        {
            direccionMovimiento = Vector3.zero;
            return;
        }

        RecargarEsquive();
        ActualizarTurbo();
        ActualizarDisparoMejorado();
        LeerMovimiento();
        LeerEsquive();
        LeerDisparo();
        RotarNave();
    }

    private void FixedUpdate()
    {
        if (!EstaPartidaActiva())
        {
            return;
        }

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
        else if (Input.GetKey(KeyCode.JoystickButton3))
        {
            entradaVertical = 1f;
        }
        else if (Input.GetKey(teclaBajar))
        {
            entradaVertical = -1f;
        }
        else if (Input.GetKey(KeyCode.JoystickButton2))
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
        float velocidadActual = EstaUsandoTurbo() ? velocidadTurbo : velocidad;

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

        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x, -limiteX, limiteX);
        nuevaPosicion.y = Mathf.Clamp(nuevaPosicion.y, alturaMinima, alturaMaxima);
        nuevaPosicion.z = Mathf.Clamp(nuevaPosicion.z, -limiteZ, limiteZ);

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

        if (cargasEsquiveActuales <= 0)
        {
            return;
        }

        if (Input.GetKeyDown(teclaEsquivarIzquierda) || GestorEntradaGlobal.DashIzquierdaPresionado())
        {
            EjecutarEsquive(ObtenerDireccionDash(-camara.transform.right.normalized));
        }

        if (Input.GetKeyDown(teclaEsquivarDerecha) || GestorEntradaGlobal.DashDerechaPresionado())
        {
            EjecutarEsquive(ObtenerDireccionDash(camara.transform.right.normalized));
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
        if (GestorEntradaGlobal.DisparoActivo() && Time.time >= siguienteDisparo)
        {
            Disparar();
            siguienteDisparo = Time.time + ObtenerCooldownActual();
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
            rbBala.linearVelocity = direccionDisparo * ObtenerVelocidadBalaActual();
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

    private bool EstaPartidaActiva()
    {
        return GameManager.instancia == null || GameManager.instancia.PuedeControlarGameplay;
    }

    public void ConfigurarDash(int cargasMaximas, float recargaSegundos)
    {
        cargasEsquiveMaximas = Mathf.Max(1, cargasMaximas);
        recargaCargaSegundos = Mathf.Max(0.1f, recargaSegundos);
        cargasEsquiveActuales = cargasEsquiveMaximas;
        temporizadorRecargaEsquive = 0f;
        NotificarDash();
    }

    public void RecuperarCargaDash(int cantidad)
    {
        if (cantidad <= 0 || cargasEsquiveActuales >= cargasEsquiveMaximas)
        {
            return;
        }

        cargasEsquiveActuales = Mathf.Min(
            cargasEsquiveMaximas,
            cargasEsquiveActuales + cantidad
        );

        if (cargasEsquiveActuales >= cargasEsquiveMaximas)
        {
            temporizadorRecargaEsquive = 0f;
        }

        NotificarDash();
    }

    public void RecuperarEnergiaTurbo(float cantidad)
    {
        energiaTurboActual = Mathf.Min(
            energiaTurboMaxima,
            energiaTurboActual + Mathf.Max(0f, cantidad)
        );
        NotificarTurbo();
    }

    public void ActivarDisparoMejorado(float duracion)
    {
        tiempoRestanteDisparoMejorado = Mathf.Max(tiempoRestanteDisparoMejorado, duracion);
        multiplicadorCooldownDisparo = 0.6f;
        multiplicadorVelocidadBala = 1.25f;
    }

    private void EjecutarEsquive(Vector3 direccion)
    {
        direccionEsquive = direccion;
        tiempoEsquive = duracionEsquive;
        siguienteEsquive = Time.time + cooldownEsquive;
        cargasEsquiveActuales--;

        if (cargasEsquiveActuales < cargasEsquiveMaximas && temporizadorRecargaEsquive <= 0f)
        {
            temporizadorRecargaEsquive = recargaCargaSegundos;
        }

        NotificarDash();
    }

    private Vector3 ObtenerDireccionDash(Vector3 respaldo)
    {
        if (direccionMovimiento.sqrMagnitude > 0.01f)
        {
            return direccionMovimiento.normalized;
        }

        return respaldo.sqrMagnitude > 0.01f ? respaldo.normalized : camara.transform.forward.normalized;
    }

    private void RecargarEsquive()
    {
        if (cargasEsquiveActuales >= cargasEsquiveMaximas)
        {
            return;
        }

        temporizadorRecargaEsquive -= Time.deltaTime;

        if (temporizadorRecargaEsquive > 0f)
        {
            NotificarDash();
            return;
        }

        cargasEsquiveActuales++;

        if (cargasEsquiveActuales < cargasEsquiveMaximas)
        {
            temporizadorRecargaEsquive = recargaCargaSegundos;
        }
        else
        {
            temporizadorRecargaEsquive = 0f;
        }

        NotificarDash();
    }

    private void ActualizarDisparoMejorado()
    {
        if (tiempoRestanteDisparoMejorado <= 0f)
        {
            return;
        }

        tiempoRestanteDisparoMejorado -= Time.deltaTime;

        if (tiempoRestanteDisparoMejorado <= 0f)
        {
            multiplicadorCooldownDisparo = 1f;
            multiplicadorVelocidadBala = 1f;
        }
    }

    private void ActualizarTurbo()
    {
        bool usandoTurbo = EstaUsandoTurbo();

        if (usandoTurbo)
        {
            energiaTurboActual = Mathf.Max(
                0f,
                energiaTurboActual - consumoTurboPorSegundo * Time.deltaTime
            );
            siguienteRegeneracionTurbo = Time.time + retrasoRegeneracionTurbo;

            if (energiaTurboActual <= 0f)
            {
                turboBloqueadoHasta = Time.time + bloqueoTurboAlVaciar;
            }
        }
        else if (
            Time.time >= siguienteRegeneracionTurbo
            && energiaTurboActual < energiaTurboMaxima
        )
        {
            energiaTurboActual = Mathf.Min(
                energiaTurboMaxima,
                energiaTurboActual + regeneracionTurboPorSegundo * Time.deltaTime
            );
        }

        ActualizarHumoTurbo(usandoTurbo && energiaTurboActual > 0f);
        NotificarTurbo();
    }

    private bool EstaUsandoTurbo()
    {
        return (Input.GetKey(teclaTurbo) || GestorEntradaGlobal.TurboActivo())
            && energiaTurboActual > 0f
            && Time.time >= turboBloqueadoHasta
            && direccionMovimiento.sqrMagnitude > 0.01f;
    }

    private float ObtenerCooldownActual()
    {
        return Mathf.Max(0.05f, cooldownBase * multiplicadorCooldownDisparo);
    }

    private float ObtenerVelocidadBalaActual()
    {
        return velocidadBalaBase * multiplicadorVelocidadBala;
    }

    private void PrepararRadioRecoleccionPowerUps()
    {
        Transform existente = transform.Find("RadioRecoleccion");
        GameObject objetoRadio;

        if (existente != null)
        {
            objetoRadio = existente.gameObject;
        }
        else
        {
            objetoRadio = new GameObject("RadioRecoleccion");
            objetoRadio.transform.SetParent(transform, false);
        }

        SphereCollider sphereCollider = objetoRadio.GetComponent<SphereCollider>();

        if (sphereCollider == null)
        {
            sphereCollider = objetoRadio.AddComponent<SphereCollider>();
        }

        sphereCollider.isTrigger = true;
        sphereCollider.radius = Mathf.Max(0.1f, radioRecoleccionPowerUps);

        RadioRecoleccionPowerUps radio =
            objetoRadio.GetComponent<RadioRecoleccionPowerUps>();

        if (radio == null)
        {
            radio = objetoRadio.AddComponent<RadioRecoleccionPowerUps>();
        }

        radio.Inicializar(this);
    }

    private void PrepararHumoTurbo()
    {
        if (humoTurbo == null)
        {
            Transform existente = transform.Find("HumoTurbo");

            if (existente != null)
            {
                humoTurbo = existente.GetComponent<ParticleSystem>();
            }
        }

        if (humoTurbo == null)
        {
            GameObject objeto = new GameObject("HumoTurbo");
            objeto.transform.SetParent(transform, false);
            objeto.transform.localPosition = new Vector3(0f, -0.2f, -1.6f);
            objeto.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            humoTurbo = objeto.AddComponent<ParticleSystem>();
        }

        ParticleSystem.MainModule main = humoTurbo.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = 0.45f;
        main.startSpeed = 3.2f;
        main.startSize = 0.22f;
        main.startColor = new Color(0.45f, 0.9f, 1f, 0.45f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = humoTurbo.emission;
        emission.rateOverTime = 32f;

        ParticleSystem.ShapeModule shape = humoTurbo.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 16f;
        shape.radius = 0.22f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = humoTurbo.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.45f, 0.9f, 1f), 0f),
                new GradientColorKey(new Color(0.1f, 0.35f, 0.55f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradiente;

        ActualizarHumoTurbo(false);
    }

    private void ActualizarHumoTurbo(bool activo)
    {
        if (humoTurbo == null)
        {
            return;
        }

        if (activo)
        {
            if (!humoTurbo.isPlaying)
            {
                humoTurbo.Play();
            }
        }
        else if (humoTurbo.isPlaying)
        {
            humoTurbo.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void NotificarDash()
    {
        AlActualizarDash?.Invoke(
            cargasEsquiveActuales,
            cargasEsquiveMaximas,
            Mathf.Max(0f, temporizadorRecargaEsquive)
        );
    }

    private void NotificarTurbo()
    {
        AlActualizarTurbo?.Invoke(energiaTurboActual, energiaTurboMaxima);
    }
}
