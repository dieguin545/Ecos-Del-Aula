using UnityEngine;
using UnityEngine.SceneManagement;

// Lanzador del flujo principal desde la pantalla de inicio.
// En inicio.unity el OnClick del boton JUGAR referencia "Juego.Inicio" como
// m_TargetAssemblyTypeName. Esta clase actua como red de seguridad: si Unity
// resuelve por nombre en vez de por GUID, este metodo carga directamente el
// hub BRIV (mismo comportamiento que Seleccionar.Inicio).
public class Juego : MonoBehaviour
{
    public void Inicio()
    {
        SceneManager.LoadScene("SeleccionJuego");
    }
}
