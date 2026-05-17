using UnityEngine;
using System.IO;

[System.Serializable]
public class DatosPartida2
{
    public float ansiedad;
    public int evidencias;
    public string zonaActual;
    public float posicionX;
    public float posicionY;
    public int personajeSeleccionado;
    public int bloqueActual;
}

public class SistemaGuardado : MonoBehaviour
{
    public static SistemaGuardado Instance;
    private string rutaArchivo;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        rutaArchivo = Application.persistentDataPath + "/partida.json";
    }

    public void GuardarPartida()
    {
        DatosPartida2 datos = new DatosPartida2();

        // Recoge todos los datos actuales
        datos.ansiedad = AnxietySystem.Instance.GetCurrentAnxiety();
        datos.evidencias = SistemaEvidencia.Instance.GetEvidencias();
        datos.personajeSeleccionado = PlayerPrefs.GetInt("PersonajeSeleccionado", 0);

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            datos.posicionX = jugador.transform.position.x;
            datos.posicionY = jugador.transform.position.y;
        }

        // Serializa a JSON y guarda
        string json = JsonUtility.ToJson(datos, true);
        File.WriteAllText(rutaArchivo, json);

        Debug.Log("Partida guardada en: " + rutaArchivo);
    }

    public DatosPartida2 CargarPartida()
    {
        if (!ExistePartida()) return null;

        string json = File.ReadAllText(rutaArchivo);
        return JsonUtility.FromJson<DatosPartida2>(json);
    }

    public bool ExistePartida()
    {
        return File.Exists(rutaArchivo);
    }

    public void EliminarPartida()
    {
        if (ExistePartida())
            File.Delete(rutaArchivo);
    }
}