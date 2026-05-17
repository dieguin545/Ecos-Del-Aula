using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject meteoritoPrefab;
    [SerializeField] private Transform nave;

    [Header("Spawn 3D")]
    [SerializeField] private float intervaloSpawnInicial = 2f;
    [SerializeField] private float intervaloSpawnMinimo = 1.1f;

    [SerializeField] private float radioMinimo = 18f;

    [SerializeField] private float radioMaximo = 32f;

    [SerializeField] private float alturaMinima = -15f;
    [SerializeField] private float alturaMaxima = 15f;
    [SerializeField] private int maxEnemigosIniciales = 8;
    [SerializeField] private int maxEnemigosAbsoluto = 15;
    [SerializeField] private int intentosPosicionSpawn = 8;

    [Header("Oleadas")]
    [SerializeField] private List<ConfiguracionOleada> oleadasConfiguradas =
        new List<ConfiguracionOleada>();

    private readonly List<Meteorito> enemigosVivos = new List<Meteorito>();
    private readonly Queue<ConfiguracionOleada> oleadasPendientes =
        new Queue<ConfiguracionOleada>();

    private ConfiguracionOleada oleadaActual;
    private ConfiguracionDificultad dificultadActual =
        ConfiguracionDificultad.CrearPredeterminada(DificultadSpaceShooter.Medio);
    private float tiempoRestanteOleada;
    private float temporizadorSpawn;

    public event Action<Meteorito, bool> AlMeteoritoDestruido;

    public int EnemigosVivos => enemigosVivos.Count;

    private void Start()
    {
        BuscarNaveSiHaceFalta();
        ReiniciarProgreso();
    }

    private void Update()
    {
        if (GameManager.instancia == null || !GameManager.instancia.juegoActivo)
        {
            return;
        }

        if (nave == null || meteoritoPrefab == null)
        {
            return;
        }

        ActualizarOleada();
        LimpiarReferenciasNulas();

        temporizadorSpawn += Time.deltaTime;

        if (
            temporizadorSpawn >= ObtenerIntervaloSpawnActual()
            && enemigosVivos.Count < ObtenerMaxEnemigosActual()
        )
        {
            temporizadorSpawn = 0f;
            SpawnMeteorito();
        }
    }

    private void OnDisable()
    {
        DesuscribirMeteoritosVivos();
    }

    public void ReiniciarProgreso()
    {
        DestruirMeteoritosVivos();
        oleadasPendientes.Clear();

        List<ConfiguracionOleada> fuenteOleadas =
            oleadasConfiguradas.Count > 0
                ? oleadasConfiguradas
                : CrearOleadasPredeterminadas();

        for (int i = 0; i < fuenteOleadas.Count; i++)
        {
            oleadasPendientes.Enqueue(fuenteOleadas[i]);
        }

        temporizadorSpawn = 0f;
        AvanzarOleada();
    }

    public void AplicarDificultad(ConfiguracionDificultad configuracion)
    {
        if (configuracion == null)
        {
            return;
        }

        dificultadActual = configuracion;
        intervaloSpawnInicial = configuracion.intervaloSpawnInicial;
        intervaloSpawnMinimo = configuracion.intervaloSpawnMinimo;
        maxEnemigosIniciales = configuracion.maxEnemigosIniciales;
        maxEnemigosAbsoluto = configuracion.maxEnemigosAbsoluto;
        ReiniciarProgreso();
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
        }
    }

    private void ActualizarOleada()
    {
        if (oleadaActual == null)
        {
            return;
        }

        tiempoRestanteOleada -= Time.deltaTime;

        if (tiempoRestanteOleada <= 0f && oleadasPendientes.Count > 0)
        {
            AvanzarOleada();
        }
    }

    private void AvanzarOleada()
    {
        if (oleadasPendientes.Count <= 0)
        {
            return;
        }

        oleadaActual = oleadasPendientes.Dequeue();
        tiempoRestanteOleada = Mathf.Max(1f, oleadaActual.duracion);
    }

    private void SpawnMeteorito()
    {
        Vector3 posicionSpawn = ObtenerPosicionSpawnValida();
        GameObject nuevoMeteorito = Instantiate(
            meteoritoPrefab,
            posicionSpawn,
            UnityEngine.Random.rotation
        );

        Meteorito meteorito = nuevoMeteorito.GetComponent<Meteorito>();

        if (meteorito == null)
        {
            Debug.LogWarning("El prefab de meteorito no tiene componente Meteorito.");
            Destroy(nuevoMeteorito);
            return;
        }

        meteorito.Configurar(ElegirTipoMeteorito(), nave, dificultadActual);
        meteorito.AlMorir += ManejarMeteoritoMuerto;
        enemigosVivos.Add(meteorito);
    }

    private Vector3 ObtenerPosicionSpawnValida()
    {
        Vector3 ultimaPosicion = nave.position + Vector3.forward * radioMaximo;

        for (int i = 0; i < intentosPosicionSpawn; i++)
        {
            Vector2 direccionHorizontal = UnityEngine.Random.insideUnitCircle;

            if (direccionHorizontal.sqrMagnitude < 0.001f)
            {
                direccionHorizontal = Vector2.right;
            }

            direccionHorizontal.Normalize();
            float radio = UnityEngine.Random.Range(radioMinimo, radioMaximo);
            float altura = UnityEngine.Random.Range(alturaMinima, alturaMaxima);
            Vector3 offset = new Vector3(
                direccionHorizontal.x * radio,
                altura,
                direccionHorizontal.y * radio
            );

            ultimaPosicion = nave.position + offset;

            if (Vector3.Distance(ultimaPosicion, nave.position) >= radioMinimo)
            {
                return ultimaPosicion;
            }
        }

        return ultimaPosicion;
    }

    private TipoAmenaza ElegirTipoMeteorito()
    {
        if (oleadaActual == null || oleadaActual.PesoTotal <= 0)
        {
            return TipoAmenaza.MensajeToxico;
        }

        int valor = UnityEngine.Random.Range(0, oleadaActual.PesoTotal);

        if (valor < oleadaActual.pesoNormal)
        {
            return TipoAmenaza.MensajeToxico;
        }

        valor -= oleadaActual.pesoNormal;

        if (valor < oleadaActual.pesoRapido)
        {
            return TipoAmenaza.RumorViral;
        }

        valor -= oleadaActual.pesoRapido;

        if (valor < oleadaActual.pesoPesado)
        {
            return TipoAmenaza.AtaqueCoordinado;
        }

        valor -= oleadaActual.pesoPesado;

        if (valor < oleadaActual.pesoPredictor)
        {
            return TipoAmenaza.NodoCorrupto;
        }

        valor -= oleadaActual.pesoPredictor;

        if (valor < oleadaActual.pesoFlanqueador)
        {
            return TipoAmenaza.NodoCorrupto;
        }

        valor -= oleadaActual.pesoFlanqueador;

        if (valor < oleadaActual.pesoTirador)
        {
            return TipoAmenaza.TiradorDigital;
        }

        return TipoAmenaza.LaserCorrupto;
    }

    private float ObtenerIntervaloSpawnActual()
    {
        float intervalo =
            oleadaActual != null
                ? oleadaActual.intervaloSpawn
                : intervaloSpawnInicial;

        return Mathf.Max(intervaloSpawnMinimo, intervalo);
    }

    private int ObtenerMaxEnemigosActual()
    {
        int maximo =
            oleadaActual != null
                ? oleadaActual.maxEnemigosVivos
                : maxEnemigosIniciales;

        return Mathf.Clamp(maximo, 1, maxEnemigosAbsoluto);
    }

    private void ManejarMeteoritoMuerto(Meteorito meteorito, bool destruidoPorJugador)
    {
        meteorito.AlMorir -= ManejarMeteoritoMuerto;
        enemigosVivos.Remove(meteorito);
        AlMeteoritoDestruido?.Invoke(meteorito, destruidoPorJugador);
    }

    private void LimpiarReferenciasNulas()
    {
        enemigosVivos.RemoveAll(meteorito => meteorito == null);
    }

    private void DesuscribirMeteoritosVivos()
    {
        for (int i = 0; i < enemigosVivos.Count; i++)
        {
            if (enemigosVivos[i] != null)
            {
                enemigosVivos[i].AlMorir -= ManejarMeteoritoMuerto;
            }
        }
    }

    private void DestruirMeteoritosVivos()
    {
        DesuscribirMeteoritosVivos();

        for (int i = 0; i < enemigosVivos.Count; i++)
        {
            if (enemigosVivos[i] != null)
            {
                Destroy(enemigosVivos[i].gameObject);
            }
        }

        enemigosVivos.Clear();
    }

    private List<ConfiguracionOleada> CrearOleadasPredeterminadas()
    {
        if (dificultadActual.dificultad == DificultadSpaceShooter.Facil)
        {
            return new List<ConfiguracionOleada>
            {
                new ConfiguracionOleada
                {
                    nombre = "Inicio",
                    duracion = 30f,
                    intervaloSpawn = dificultadActual.intervaloSpawnInicial,
                    maxEnemigosVivos = dificultadActual.maxEnemigosIniciales,
                    pesoNormal = 70,
                    pesoRapido = 20,
                    pesoPesado = 10
                },
                new ConfiguracionOleada
                {
                    nombre = "Control",
                    duracion = 999f,
                    intervaloSpawn = dificultadActual.intervaloSpawnMinimo,
                    maxEnemigosVivos = dificultadActual.maxEnemigosAbsoluto,
                    pesoNormal = 55,
                    pesoRapido = 20,
                    pesoPesado = 15,
                    pesoPredictor = 5,
                    pesoTirador = 5
                }
            };
        }

        if (dificultadActual.dificultad == DificultadSpaceShooter.Dificil)
        {
            return new List<ConfiguracionOleada>
            {
                new ConfiguracionOleada
                {
                    nombre = "Inicio",
                    duracion = 20f,
                    intervaloSpawn = dificultadActual.intervaloSpawnInicial,
                    maxEnemigosVivos = dificultadActual.maxEnemigosIniciales,
                    pesoNormal = 45,
                    pesoRapido = 20,
                    pesoPesado = 15,
                    pesoPredictor = 10,
                    pesoTirador = 10
                },
                new ConfiguracionOleada
                {
                    nombre = "Presion",
                    duracion = 25f,
                    intervaloSpawn = 1.3f,
                    maxEnemigosVivos = 12,
                    pesoNormal = 30,
                    pesoRapido = 20,
                    pesoPesado = 15,
                    pesoPredictor = 15,
                    pesoFlanqueador = 5,
                    pesoTirador = 10,
                    pesoLaser = 5
                },
                new ConfiguracionOleada
                {
                    nombre = "Ruptura",
                    duracion = 999f,
                    intervaloSpawn = dificultadActual.intervaloSpawnMinimo,
                    maxEnemigosVivos = dificultadActual.maxEnemigosAbsoluto,
                    pesoNormal = 20,
                    pesoRapido = 20,
                    pesoPesado = 15,
                    pesoPredictor = 15,
                    pesoFlanqueador = 10,
                    pesoTirador = 12,
                    pesoLaser = 8
                }
            };
        }

        return new List<ConfiguracionOleada>
        {
            new ConfiguracionOleada
            {
                nombre = "Inicio",
                duracion = 25f,
                intervaloSpawn = intervaloSpawnInicial,
                maxEnemigosVivos = maxEnemigosIniciales,
                pesoNormal = 70,
                pesoRapido = 20,
                pesoPesado = 10
            },
            new ConfiguracionOleada
            {
                nombre = "Presion",
                duracion = 30f,
                intervaloSpawn = 1.6f,
                maxEnemigosVivos = 10,
                pesoNormal = 50,
                pesoRapido = 25,
                pesoPesado = 15,
                pesoPredictor = 5,
                pesoTirador = 5
            },
            new ConfiguracionOleada
            {
                nombre = "Ruptura",
                duracion = 999f,
                intervaloSpawn = intervaloSpawnMinimo,
                maxEnemigosVivos = 14,
                pesoNormal = 35,
                pesoRapido = 25,
                pesoPesado = 20,
                pesoPredictor = 10,
                pesoFlanqueador = 5,
                pesoTirador = 5
            }
        };
    }
}
