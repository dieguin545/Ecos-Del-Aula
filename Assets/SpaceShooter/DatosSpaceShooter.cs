using System;
using System.Collections.Generic;
using UnityEngine;

public enum DificultadSpaceShooter
{
    Facil,
    Medio,
    Dificil
}

public enum EstadoPartidaSpaceShooter
{
    Menu,
    Jugando,
    Pausa,
    Victoria,
    GameOver
}

public enum TipoAmenaza
{
    MensajeToxico,
    RumorViral,
    AtaqueCoordinado,
    NodoCorrupto,
    TiradorDigital,
    LaserCorrupto
}

public enum TipoPowerUp
{
    Vida,
    Escudo,
    Dash,
    DisparoMejorado,
    LimpiezaDigital,
    Puntos
}

public enum TipoDaltonismo
{
    Ninguno,
    Protanopia,
    Deuteranopia,
    Tritanopia,
    Acromatopsia
}

[Serializable]
public class ConfiguracionDificultad
{
    public DificultadSpaceShooter dificultad;
    public string nombreVisible;
    public int objetivoEnemigos;
    public int maxEnemigosIniciales;
    public int maxEnemigosAbsoluto;
    public float intervaloSpawnInicial;
    public float intervaloSpawnMinimo;
    public float multiplicadorVelocidadEnemigos;
    public float multiplicadorDanioEnemigos;
    public float multiplicadorFrecuenciaAtaque;
    public int cargasDash;
    public float recargaDashSegundos;
    public int multiplicadorPuntaje;

    public static ConfiguracionDificultad CrearPredeterminada(DificultadSpaceShooter dificultad)
    {
        switch (dificultad)
        {
            case DificultadSpaceShooter.Facil:
                return new ConfiguracionDificultad
                {
                    dificultad = DificultadSpaceShooter.Facil,
                    nombreVisible = "Fácil",
                    objetivoEnemigos = 20,
                    maxEnemigosIniciales = 5,
                    maxEnemigosAbsoluto = 7,
                    intervaloSpawnInicial = 2.2f,
                    intervaloSpawnMinimo = 1.45f,
                    multiplicadorVelocidadEnemigos = 0.85f,
                    multiplicadorDanioEnemigos = 0.75f,
                    multiplicadorFrecuenciaAtaque = 0.8f,
                    cargasDash = 3,
                    recargaDashSegundos = 2.3f,
                    multiplicadorPuntaje = 1
                };
            case DificultadSpaceShooter.Dificil:
                return new ConfiguracionDificultad
                {
                    dificultad = DificultadSpaceShooter.Dificil,
                    nombreVisible = "Difícil",
                    objetivoEnemigos = 30,
                    maxEnemigosIniciales = 10,
                    maxEnemigosAbsoluto = 15,
                    intervaloSpawnInicial = 1.7f,
                    intervaloSpawnMinimo = 1.0f,
                    multiplicadorVelocidadEnemigos = 1.2f,
                    multiplicadorDanioEnemigos = 1.35f,
                    multiplicadorFrecuenciaAtaque = 1.35f,
                    cargasDash = 2,
                    recargaDashSegundos = 3.2f,
                    multiplicadorPuntaje = 3
                };
            default:
                return new ConfiguracionDificultad
                {
                    dificultad = DificultadSpaceShooter.Medio,
                    nombreVisible = "Medio",
                    objetivoEnemigos = 25,
                    maxEnemigosIniciales = 8,
                    maxEnemigosAbsoluto = 10,
                    intervaloSpawnInicial = 2f,
                    intervaloSpawnMinimo = 1.1f,
                    multiplicadorVelocidadEnemigos = 1f,
                    multiplicadorDanioEnemigos = 1f,
                    multiplicadorFrecuenciaAtaque = 1f,
                    cargasDash = 3,
                    recargaDashSegundos = 2.8f,
                    multiplicadorPuntaje = 2
                };
        }
    }
}

[Serializable]
public class ConfiguracionOleada
{
    public string nombre = "Oleada";
    [Min(1f)] public float duracion = 25f;
    [Min(0.1f)] public float intervaloSpawn = 2f;
    [Min(1)] public int maxEnemigosVivos = 8;
    [Min(0)] public int pesoNormal = 70;
    [Min(0)] public int pesoRapido = 20;
    [Min(0)] public int pesoPesado = 10;
    [Min(0)] public int pesoPredictor = 0;
    [Min(0)] public int pesoFlanqueador = 0;
    [Min(0)] public int pesoTirador = 0;
    [Min(0)] public int pesoLaser = 0;

    public int PesoTotal =>
        pesoNormal +
        pesoRapido +
        pesoPesado +
        pesoPredictor +
        pesoFlanqueador +
        pesoTirador +
        pesoLaser;
}

[Serializable]
public class DropPowerUp
{
    public TipoPowerUp tipo = TipoPowerUp.Vida;
    [Min(0f)] public float peso = 1f;
    public GameObject prefab;
}

[Serializable]
public class ModeloNaveDisponible
{
    public string nombre = "Nave";
    public GameObject modelo;
    public Vector3 posicionLocal = Vector3.zero;
    public Vector3 rotacionLocal = Vector3.zero;
    public Vector3 escalaLocal = Vector3.one;
}

public class DatosPartidaSpaceShooter
{
    private readonly Dictionary<TipoAmenaza, int> destruidosPorTipo =
        new Dictionary<TipoAmenaza, int>();
    private readonly Dictionary<TipoPowerUp, int> powerUpsRecogidosPorTipo =
        new Dictionary<TipoPowerUp, int>();

    public int EnemigosDestruidos { get; private set; }
    public int DanioRecibido { get; private set; }
    public int PuntosExtra { get; private set; }
    public float TiempoSobrevivido { get; private set; }

    public DatosPartidaSpaceShooter()
    {
        Reiniciar();
    }

    public void Reiniciar()
    {
        EnemigosDestruidos = 0;
        DanioRecibido = 0;
        PuntosExtra = 0;
        TiempoSobrevivido = 0f;
        destruidosPorTipo.Clear();
        powerUpsRecogidosPorTipo.Clear();

        foreach (TipoAmenaza tipo in Enum.GetValues(typeof(TipoAmenaza)))
        {
            destruidosPorTipo[tipo] = 0;
        }

        foreach (TipoPowerUp tipo in Enum.GetValues(typeof(TipoPowerUp)))
        {
            powerUpsRecogidosPorTipo[tipo] = 0;
        }
    }

    public void ActualizarTiempo(float deltaTime)
    {
        TiempoSobrevivido += deltaTime;
    }

    public void RegistrarAmenazaDestruida(TipoAmenaza tipo)
    {
        EnemigosDestruidos++;
        destruidosPorTipo[tipo]++;
    }

    public void RegistrarDanio(int cantidad)
    {
        DanioRecibido += Mathf.Max(0, cantidad);
    }

    public void RegistrarPuntosExtra(int cantidad)
    {
        PuntosExtra += Mathf.Max(0, cantidad);
    }

    public void RegistrarPowerUpRecogido(TipoPowerUp tipo)
    {
        powerUpsRecogidosPorTipo[tipo]++;
    }

    public int ObtenerDestruidosPorTipo(TipoAmenaza tipo)
    {
        return destruidosPorTipo.TryGetValue(tipo, out int cantidad) ? cantidad : 0;
    }

    public int ObtenerPowerUpsRecogidosPorTipo(TipoPowerUp tipo)
    {
        return powerUpsRecogidosPorTipo.TryGetValue(tipo, out int cantidad) ? cantidad : 0;
    }

    public ResultadoMinijuego CrearResultado(
        bool victoria,
        ConfiguracionDificultad dificultad,
        string naveUsada
    )
    {
        int vidasBonus = Mathf.Max(0, GameManager.instancia != null ? GameManager.instancia.Vidas : 0);
        int puntaje =
            EnemigosDestruidos * 100 +
            Mathf.RoundToInt(TiempoSobrevivido * 3f) +
            vidasBonus * 150 +
            PuntosExtra +
            (victoria ? 500 : 0);

        if (dificultad != null)
        {
            puntaje *= Mathf.Max(1, dificultad.multiplicadorPuntaje);
        }

        return new ResultadoMinijuego(
            victoria,
            EnemigosDestruidos,
            DanioRecibido,
            TiempoSobrevivido,
            puntaje,
            dificultad != null ? dificultad.dificultad : DificultadSpaceShooter.Medio,
            naveUsada
        );
    }
}

public class ResultadoMinijuego
{
    public bool Victoria { get; }
    public int EnemigosDestruidos { get; }
    public int DanioRecibido { get; }
    public float TiempoSobrevivido { get; }
    public int Puntaje { get; }
    public DificultadSpaceShooter Dificultad { get; }
    public string NaveUsada { get; }

    public ResultadoMinijuego(
        bool victoria,
        int enemigosDestruidos,
        int danioRecibido,
        float tiempoSobrevivido,
        int puntaje,
        DificultadSpaceShooter dificultad,
        string naveUsada
    )
    {
        Victoria = victoria;
        EnemigosDestruidos = enemigosDestruidos;
        DanioRecibido = danioRecibido;
        TiempoSobrevivido = tiempoSobrevivido;
        Puntaje = puntaje;
        Dificultad = dificultad;
        NaveUsada = naveUsada;
    }
}
