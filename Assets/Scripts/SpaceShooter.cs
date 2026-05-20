using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceShooter : MonoBehaviour
{
    public void retroceder()
    {
        InteraccionPC[] pcs = FindObjectsByType<InteraccionPC>(FindObjectsInactive.Include);

        for (int i = 0; i < pcs.Length; i++)
        {
            if (pcs[i] != null)
            {
                pcs[i].SalirPCDesdeUI();
            }
        }

        InteraccionPC.ResetearEstadoGlobalPC();
        MenuPausaAccesibilidad.ResetearEstadoGlobalPausa();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("spaceshooter");
    }
}
