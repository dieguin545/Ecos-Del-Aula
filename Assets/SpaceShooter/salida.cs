using UnityEngine;
using UnityEngine.SceneManagement;
public class salida : MonoBehaviour
{
    public void devolver()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Juego");
    }
}
