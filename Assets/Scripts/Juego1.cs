using UnityEngine;
using UnityEngine.SceneManagement;

public class Juego1 : MonoBehaviour
{
    private const string EscenaEntryFilter = "Juego";

    public void Inicio()
    {
        Time.timeScale = 1f;

        if (!Application.CanStreamedLevelBeLoaded(EscenaEntryFilter))
        {
            Debug.LogError($"No se puede cargar la escena '{EscenaEntryFilter}'. Revisa Build Settings.");
            return;
        }

        SceneManager.LoadScene(EscenaEntryFilter, LoadSceneMode.Single);
    }
}
