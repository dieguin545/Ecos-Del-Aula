using UnityEngine;
using UnityEngine.SceneManagement;

public class salida : MonoBehaviour
{
    public void devolver()
    {
        InteraccionPC.ResetearEstadoGlobalPC();
        MenuPausaAccesibilidad.ResetearEstadoGlobalPausa();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Juego");
    }
}