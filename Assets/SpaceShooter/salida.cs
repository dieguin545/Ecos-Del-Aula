using UnityEngine;
using UnityEngine.SceneManagement;
public class salida : MonoBehaviour
{
    public void devolver()
    {
        SceneManager.LoadScene("Juego");
    }
}
