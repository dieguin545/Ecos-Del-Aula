using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RegistroLeaderboard
{
    public string alias;
    public int puntaje;
    public int enemigosDestruidos;
    public float tiempoSobrevivido;
    public string dificultad;
    public string nave;
    public string fechaIso;
    public bool victoria;
}

[Serializable]
public class ArchivoLeaderboard
{
    public List<RegistroLeaderboard> registros = new List<RegistroLeaderboard>();
}

public class LeaderboardSpaceShooter
{
    private readonly string rutaArchivo;
    private readonly List<RegistroLeaderboard> registros = new List<RegistroLeaderboard>();
    private readonly Dictionary<DificultadSpaceShooter, List<RegistroLeaderboard>> registrosPorDificultad =
        new Dictionary<DificultadSpaceShooter, List<RegistroLeaderboard>>();

    public LeaderboardSpaceShooter(string rutaArchivo)
    {
        this.rutaArchivo = rutaArchivo;
        Cargar();
    }

    public string RutaArchivo => rutaArchivo;

    public IReadOnlyList<RegistroLeaderboard> ObtenerTop(int cantidad)
    {
        OrdenarRegistros();
        int limite = Mathf.Min(Mathf.Max(0, cantidad), registros.Count);
        return registros.GetRange(0, limite);
    }

    public IReadOnlyList<RegistroLeaderboard> ObtenerTopPorDificultad(
        DificultadSpaceShooter dificultad,
        int cantidad
    )
    {
        ReconstruirIndicePorDificultad();

        if (!registrosPorDificultad.TryGetValue(dificultad, out List<RegistroLeaderboard> filtrados))
        {
            return new List<RegistroLeaderboard>();
        }

        filtrados.Sort(CompararRegistros);
        int limite = Mathf.Min(Mathf.Max(0, cantidad), filtrados.Count);
        return filtrados.GetRange(0, limite);
    }

    public void RegistrarResultado(string alias, ResultadoMinijuego resultado)
    {
        if (resultado == null)
        {
            return;
        }

        RegistroLeaderboard registro = new RegistroLeaderboard
        {
            alias = string.IsNullOrWhiteSpace(alias) ? "Jugador" : alias.Trim(),
            puntaje = resultado.Puntaje,
            enemigosDestruidos = resultado.EnemigosDestruidos,
            tiempoSobrevivido = resultado.TiempoSobrevivido,
            dificultad = resultado.Dificultad.ToString(),
            nave = string.IsNullOrWhiteSpace(resultado.NaveUsada) ? "Nave base" : resultado.NaveUsada,
            fechaIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            victoria = resultado.Victoria
        };

        registros.Add(registro);
        OrdenarRegistros();
        Guardar();
    }

    private void Cargar()
    {
        registros.Clear();

        try
        {
            if (!File.Exists(rutaArchivo))
            {
                return;
            }

            string json = File.ReadAllText(rutaArchivo);

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            ArchivoLeaderboard archivo = JsonUtility.FromJson<ArchivoLeaderboard>(json);

            if (archivo != null && archivo.registros != null)
            {
                registros.AddRange(archivo.registros);
                OrdenarRegistros();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("No se pudo leer el leaderboard: " + ex.Message);
        }
    }

    private void Guardar()
    {
        try
        {
            ArchivoLeaderboard archivo = new ArchivoLeaderboard
            {
                registros = new List<RegistroLeaderboard>(registros)
            };

            string json = JsonUtility.ToJson(archivo, true);
            File.WriteAllText(rutaArchivo, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("No se pudo guardar el leaderboard: " + ex.Message);
        }
    }

    private void OrdenarRegistros()
    {
        registros.Sort(CompararRegistros);
        ReconstruirIndicePorDificultad();
    }

    private void ReconstruirIndicePorDificultad()
    {
        registrosPorDificultad.Clear();

        foreach (DificultadSpaceShooter dificultad in Enum.GetValues(typeof(DificultadSpaceShooter)))
        {
            registrosPorDificultad[dificultad] = new List<RegistroLeaderboard>();
        }

        for (int i = 0; i < registros.Count; i++)
        {
            if (Enum.TryParse(registros[i].dificultad, out DificultadSpaceShooter dificultad))
            {
                registrosPorDificultad[dificultad].Add(registros[i]);
            }
        }
    }

    private int CompararRegistros(RegistroLeaderboard a, RegistroLeaderboard b)
    {
        int comparacionPuntaje = b.puntaje.CompareTo(a.puntaje);

        if (comparacionPuntaje != 0)
        {
            return comparacionPuntaje;
        }

        return b.tiempoSobrevivido.CompareTo(a.tiempoSobrevivido);
    }
}
