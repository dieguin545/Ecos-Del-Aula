using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteorito : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 4f;
    [SerializeField] private float velocidadRotacion = 80f;
    [SerializeField] private float suavizadoDireccion = 3f;
    [SerializeField] private float distanciaMaximaDesdeNave = 80f;

    [Header("Combate")]
    [SerializeField] private int resistenciaBase = 1;
    [SerializeField] private int danioBase = 1;

    [Header("Comportamientos")]
    [SerializeField] private float tiempoPrediccion = 0.9f;
    [SerializeField] private float distanciaFlanqueo = 8f;
    [SerializeField] private float duracionFlanqueo = 1.25f;
    [SerializeField] private float distanciaDeseadaTirador = 14f;

    [Header("Ataques")]
    [SerializeField] private float intervaloAtaqueBase = 2.4f;
    [SerializeField] private float velocidadProyectil = 16f;
    [SerializeField] private float distanciaLaser = 28f;
    [SerializeField] private float duracionLaserVisible = 0.14f;
    [SerializeField] private float tiempoAdvertenciaAtaque = 0.45f;

    [Header("Recompensas")]
    [SerializeField] private GameObject corazonPrefab;
    [Range(0f, 1f)] [SerializeField] private float probabilidadDropBase = 0.22f;
    [SerializeField] private List<DropPowerUp> tablaDrops = new List<DropPowerUp>();

    private Transform nave;
    private TipoAmenaza tipo = TipoAmenaza.MensajeToxico;
    private Vector3 direccionActual;
    private Vector3 ultimaPosicionNave;
    private Vector3 velocidadNaveEstimada;
    private Vector3 puntoFlanqueo;
    private float tiempoRestanteFlanqueo;
    private float siguienteAtaque;
    private float velocidadActual;
    private float velocidadRotacionActual;
    private int resistenciaActual;
    private int danioActual;
    private Vector3 escalaBase;
    private bool estaMuerto;
    private ConfiguracionDificultad dificultadActual =
        ConfiguracionDificultad.CrearPredeterminada(DificultadSpaceShooter.Medio);
    private LineRenderer lineRendererLaser;
    private Color ColorAtaqueActual
    {
        get
        {
            switch (AccesibilidadSpaceShooter.TipoDaltonismoActual)
            {
                case TipoDaltonismo.Protanopia:
                case TipoDaltonismo.Deuteranopia:
                    return EstiloVisualSpaceShooter.ColorAtaqueEnemigoDaltonico;
                case TipoDaltonismo.Tritanopia:
                    return new Color(1f, 0.18f, 0.55f);
                case TipoDaltonismo.Acromatopsia:
                    return Color.white;
                default:
                    return EstiloVisualSpaceShooter.ColorAtaqueEnemigo;
            }
        }
    }
    private bool ataquePreparado;
    private float ejecutarAtaqueEn;
    private GestorAlertasAtaque gestorAlertas;

    public event Action<Meteorito, bool> AlMorir;

    public TipoAmenaza Tipo => tipo;

    private void Awake()
    {
        escalaBase = transform.localScale;
        velocidadActual = velocidad;
        velocidadRotacionActual = velocidadRotacion;
        resistenciaActual = resistenciaBase;
        danioActual = danioBase;
        PrepararTablaDropsSiHaceFalta();
        gestorAlertas = FindAnyObjectByType<GestorAlertasAtaque>();
    }

    private void Start()
    {
        BuscarNaveSiHaceFalta();

        if (nave != null)
        {
            ultimaPosicionNave = nave.position;
            direccionActual = (nave.position - transform.position).normalized;
        }
    }

    private void Update()
    {
        if (estaMuerto)
        {
            return;
        }

        if (GameManager.instancia != null && !GameManager.instancia.juegoActivo)
        {
            return;
        }

        if (nave == null)
        {
            return;
        }

        ActualizarVelocidadNaveEstimada();
        Mover();
        Rotar();
        AtacarSiCorresponde();

        if (Vector3.Distance(transform.position, nave.position) > distanciaMaximaDesdeNave)
        {
            Morir(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (estaMuerto || !other.CompareTag("Nave"))
        {
            return;
        }

        if (GameManager.instancia != null)
        {
            NaveController naveController = other.GetComponent<NaveController>();

            if (naveController == null || !naveController.EstaEsquivando)
            {
                GameManager.instancia.PerderVida(danioActual);
            }
        }

        Morir(false);
    }

    public void Configurar(
        TipoAmenaza nuevoTipo,
        Transform objetivo,
        ConfiguracionDificultad configuracion
    )
    {
        tipo = nuevoTipo;
        nave = objetivo;
        dificultadActual = configuracion ?? dificultadActual;
        AplicarPerfilPorTipo();
        AplicarAccesibilidadVisual();

        if (nave != null)
        {
            ultimaPosicionNave = nave.position;
            direccionActual = (nave.position - transform.position).normalized;
            PrepararFlanqueo();
        }
    }

    public void RecibirImpacto(int danio = 1)
    {
        if (estaMuerto)
        {
            return;
        }

        resistenciaActual -= Mathf.Max(1, danio);

        if (resistenciaActual <= 0)
        {
            Morir(true);
        }
    }

    public void Morir(bool destruidoPorJugador)
    {
        if (estaMuerto)
        {
            return;
        }

        estaMuerto = true;

        if (destruidoPorJugador)
        {
            IntentarSoltarPowerUp();
        }

        AlMorir?.Invoke(this, destruidoPorJugador);
        Destroy(gameObject);
    }

    private void BuscarNaveSiHaceFalta()
    {
        if (nave != null)
        {
            return;
        }

        GameObject objNave = GameObject.FindWithTag("Nave");

        if (objNave != null)
        {
            nave = objNave.transform;
            ultimaPosicionNave = nave.position;
        }
    }

    private void AplicarPerfilPorTipo()
    {
        velocidadActual = velocidad;
        velocidadRotacionActual = velocidadRotacion;
        resistenciaActual = resistenciaBase;
        danioActual = danioBase;
        transform.localScale = escalaBase;

        switch (tipo)
        {
            case TipoAmenaza.RumorViral:
                velocidadActual *= 1.5f;
                velocidadRotacionActual *= 1.25f;
                transform.localScale = escalaBase * 0.75f;
                break;
            case TipoAmenaza.AtaqueCoordinado:
                velocidadActual *= 0.7f;
                velocidadRotacionActual *= 0.7f;
                resistenciaActual = resistenciaBase + 1;
                danioActual = danioBase + 1;
                transform.localScale = escalaBase * 1.35f;
                break;
            case TipoAmenaza.NodoCorrupto:
                velocidadActual *= 1.1f;
                break;
            case TipoAmenaza.TiradorDigital:
                velocidadActual *= 0.9f;
                transform.localScale = escalaBase * 0.95f;
                break;
            case TipoAmenaza.LaserCorrupto:
                velocidadActual *= 0.85f;
                resistenciaActual = resistenciaBase + 1;
                danioActual = danioBase + 1;
                transform.localScale = escalaBase * 1.1f;
                break;
        }

        velocidadActual *= dificultadActual.multiplicadorVelocidadEnemigos;
        danioActual = Mathf.Max(
            1,
            Mathf.RoundToInt(danioActual * dificultadActual.multiplicadorDanioEnemigos)
        );
    }

    private void ActualizarVelocidadNaveEstimada()
    {
        if (Time.deltaTime <= 0f)
        {
            return;
        }

        velocidadNaveEstimada = (nave.position - ultimaPosicionNave) / Time.deltaTime;
        ultimaPosicionNave = nave.position;
    }

    private void Mover()
    {
        Vector3 puntoObjetivo = ObtenerPuntoObjetivo();
        Vector3 direccionObjetivo = (puntoObjetivo - transform.position).normalized;

        if (direccionObjetivo.sqrMagnitude > 0.001f)
        {
            direccionActual = Vector3.Slerp(
                direccionActual,
                direccionObjetivo,
                suavizadoDireccion * Time.deltaTime
            );
        }

        transform.position += direccionActual.normalized * velocidadActual * Time.deltaTime;
    }

    private Vector3 ObtenerPuntoObjetivo()
    {
        switch (tipo)
        {
            case TipoAmenaza.NodoCorrupto:
                if (tiempoRestanteFlanqueo > 0f)
                {
                    tiempoRestanteFlanqueo -= Time.deltaTime;
                    return puntoFlanqueo;
                }

                return nave.position + velocidadNaveEstimada * tiempoPrediccion;
            case TipoAmenaza.TiradorDigital:
            case TipoAmenaza.LaserCorrupto:
                return ObtenerPuntoObjetivoTirador();
            default:
                return nave.position;
        }
    }

    private void PrepararFlanqueo()
    {
        if (tipo != TipoAmenaza.NodoCorrupto || nave == null)
        {
            return;
        }

        Vector3 direccionAlJugador = (nave.position - transform.position).normalized;
        Vector3 lateral = Vector3.Cross(Vector3.up, direccionAlJugador).normalized;

        if (UnityEngine.Random.value < 0.5f)
        {
            lateral *= -1f;
        }

        puntoFlanqueo = nave.position + lateral * distanciaFlanqueo;
        tiempoRestanteFlanqueo = duracionFlanqueo;
    }

    private void Rotar()
    {
        transform.Rotate(
            velocidadRotacionActual * Time.deltaTime,
            velocidadRotacionActual * 0.6f * Time.deltaTime,
            velocidadRotacionActual * 0.4f * Time.deltaTime
        );
    }

    private Vector3 ObtenerPuntoObjetivoTirador()
    {
        Vector3 haciaNave = nave.position - transform.position;
        float distancia = haciaNave.magnitude;

        if (distancia > distanciaDeseadaTirador + 2f)
        {
            return nave.position;
        }

        if (distancia < distanciaDeseadaTirador - 2f)
        {
            return transform.position - haciaNave.normalized * 3f;
        }

        Vector3 lateral = Vector3.Cross(Vector3.up, haciaNave.normalized);
        return transform.position + lateral * 2f;
    }

    private void AtacarSiCorresponde()
    {
        if (tipo != TipoAmenaza.TiradorDigital && tipo != TipoAmenaza.LaserCorrupto)
        {
            return;
        }

        if (ataquePreparado)
        {
            if (Time.time >= ejecutarAtaqueEn)
            {
                ataquePreparado = false;
                EjecutarAtaque();
            }

            return;
        }

        if (Time.time < siguienteAtaque)
        {
            return;
        }

        float intervalo = intervaloAtaqueBase / Mathf.Max(0.1f, dificultadActual.multiplicadorFrecuenciaAtaque);
        siguienteAtaque = Time.time + intervalo;
        ataquePreparado = true;
        ejecutarAtaqueEn = Time.time + tiempoAdvertenciaAtaque;

        if (gestorAlertas != null && nave != null)
        {
            gestorAlertas.RegistrarAmenaza(this, Vector3.Distance(transform.position, nave.position));
        }
    }

    private void EjecutarAtaque()
    {
        if (tipo == TipoAmenaza.LaserCorrupto)
        {
            LanzarLaser();
        }
        else
        {
            DispararProyectil();
        }
    }

    private void DispararProyectil()
    {
        if (nave == null)
        {
            return;
        }

        GameObject proyectil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        proyectil.name = "ProyectilEnemigo";
        proyectil.transform.position = transform.position;
        proyectil.transform.localScale = Vector3.one * 0.5f;

        Collider collider = proyectil.GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody rigidbody = proyectil.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;

        ProyectilEnemigo ataque = proyectil.AddComponent<ProyectilEnemigo>();
        Vector3 direccion = (nave.position - transform.position).normalized;
        ataque.Inicializar(direccion, velocidadProyectil, danioActual);
    }

    private void LanzarLaser()
    {
        if (nave == null)
        {
            return;
        }

        PrepararLineRendererLaser();

        Vector3 origen = transform.position;
        Vector3 direccion = (nave.position - origen).normalized;
        Vector3 destino = origen + direccion * distanciaLaser;

        RaycastHit[] impactos = Physics.RaycastAll(
            origen,
            direccion,
            distanciaLaser,
            ~0,
            QueryTriggerInteraction.Collide
        );
        Array.Sort(impactos, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < impactos.Length; i++)
        {
            RaycastHit hit = impactos[i];

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            destino = hit.point;

            if (hit.collider.CompareTag("Nave"))
            {
                NaveController naveController = hit.collider.GetComponent<NaveController>();

                if (naveController == null || !naveController.EstaEsquivando)
                {
                    if (GameManager.instancia != null)
                    {
                        GameManager.instancia.PerderVida(danioActual);
                    }
                }
            }

            break;
        }

        lineRendererLaser.SetPosition(0, origen);
        lineRendererLaser.SetPosition(1, destino);
        StopCoroutine(nameof(MostrarLaserTemporal));
        StartCoroutine(MostrarLaserTemporal());
    }

    private void PrepararLineRendererLaser()
    {
        if (lineRendererLaser != null)
        {
            return;
        }

        lineRendererLaser = gameObject.AddComponent<LineRenderer>();
        lineRendererLaser.positionCount = 2;
        lineRendererLaser.startWidth = 0.18f;
        lineRendererLaser.endWidth = 0.08f;
        lineRendererLaser.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererLaser.enabled = false;
    }

    private IEnumerator MostrarLaserTemporal()
    {
        lineRendererLaser.startColor =
            ColorAtaqueActual;
        lineRendererLaser.endColor = lineRendererLaser.startColor;
        lineRendererLaser.enabled = true;
        yield return new WaitForSeconds(duracionLaserVisible);
        lineRendererLaser.enabled = false;
    }

    public void AplicarAccesibilidadVisual()
    {
        EstiloVisualSpaceShooter.AplicarAEnemigo(
            gameObject,
            AccesibilidadSpaceShooter.TipoDaltonismoActual
        );
        FirewallDelAulaVisuales.AplicarAmenaza(gameObject, tipo);
    }

    private void PrepararTablaDropsSiHaceFalta()
    {
        if (tablaDrops.Count > 0)
        {
            return;
        }

        tablaDrops.Add(new DropPowerUp { tipo = TipoPowerUp.Vida, peso = 10f });
        tablaDrops.Add(new DropPowerUp { tipo = TipoPowerUp.Dash, peso = 20f });
        tablaDrops.Add(new DropPowerUp { tipo = TipoPowerUp.Escudo, peso = 12f });
        tablaDrops.Add(new DropPowerUp { tipo = TipoPowerUp.DisparoMejorado, peso = 8f });
        tablaDrops.Add(new DropPowerUp { tipo = TipoPowerUp.LimpiezaDigital, peso = 5f });
        tablaDrops.Add(new DropPowerUp { tipo = TipoPowerUp.Puntos, peso = 15f });
    }

    private void IntentarSoltarPowerUp()
    {
        if (tablaDrops.Count == 0 || corazonPrefab == null)
        {
            return;
        }

        float multiplicadorDificultad = ObtenerMultiplicadorDropPorDificultad();
        float probabilidadFinal = Mathf.Clamp01(probabilidadDropBase * multiplicadorDificultad);

        if (UnityEngine.Random.value > probabilidadFinal)
        {
            return;
        }

        DropPowerUp drop = ElegirDrop();

        if (drop == null)
        {
            return;
        }

        GameObject prefab = drop.prefab != null ? drop.prefab : corazonPrefab;
        GameObject objeto = Instantiate(prefab, transform.position, Quaternion.identity);
        PowerUpSpaceShooter powerUp = objeto.GetComponent<PowerUpSpaceShooter>();

        if (powerUp == null)
        {
            powerUp = objeto.AddComponent<PowerUpSpaceShooter>();
        }

        powerUp.ConfigurarTipo(drop.tipo);
    }

    private DropPowerUp ElegirDrop()
    {
        float pesoTotal = 0f;

        for (int i = 0; i < tablaDrops.Count; i++)
        {
            pesoTotal += Mathf.Max(0f, tablaDrops[i].peso);
        }

        if (pesoTotal <= 0f)
        {
            return null;
        }

        float valor = UnityEngine.Random.Range(0f, pesoTotal);

        for (int i = 0; i < tablaDrops.Count; i++)
        {
            valor -= Mathf.Max(0f, tablaDrops[i].peso);

            if (valor <= 0f)
            {
                return tablaDrops[i];
            }
        }

        return tablaDrops[tablaDrops.Count - 1];
    }

    private float ObtenerMultiplicadorDropPorDificultad()
    {
        switch (dificultadActual.dificultad)
        {
            case DificultadSpaceShooter.Facil:
                return 1.15f;
            case DificultadSpaceShooter.Dificil:
                return 0.85f;
            default:
                return 1f;
        }
    }
}
