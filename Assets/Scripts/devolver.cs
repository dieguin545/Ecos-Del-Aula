using UnityEngine;
using UnityEngine.SceneManagement;
public class devolver : MonoBehaviour
{
 public void retroceder()
    {
        InteraccionPC interaccionPC = FindAnyObjectByType<InteraccionPC>();

        if (interaccionPC != null && InteraccionPC.PCAbierta)
        {
            interaccionPC.SalirPCDesdeUI();
            return;
        }

        SceneManager.LoadScene("SeleccionJuego");
    }
}
