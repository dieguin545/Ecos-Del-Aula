using UnityEngine;
public class CerrarCorreo : MonoBehaviour
{
    public GameObject VentanaCorreo;
    void Start()
    {
        VentanaCorreo.SetActive(false);
    }
   public void TogglePanelCorreo()
{
    if(VentanaCorreo.activeSelf)
    {
        VentanaCorreo.SetActive(false);
    }
    else
    {
        VentanaCorreo.SetActive(true);
    }
}
}
