using UnityEngine;

public class BotonModoLecturaController : MonoBehaviour
{
    public void AlternarModoLectura()
    {
        ControlCorreo control = FindObjectOfType<ControlCorreo>();

        if (control != null)
        {
            control.AlternarModoLectura();
        }
        else
        {
            Debug.Log("No se encontro ControlCorreo en la escena");
        }
    }
}