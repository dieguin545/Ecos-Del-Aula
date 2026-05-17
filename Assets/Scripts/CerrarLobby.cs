using UnityEngine;
public class CerrarLobby : MonoBehaviour
{
    public GameObject Lobby;
    void Start()
    {
        Lobby.SetActive(false);
    } 
   public void TogglePanelCorreo()
{
    if(Lobby.activeSelf)
    {
        Lobby.SetActive(false);
    }
    else
    {
        Lobby.SetActive(true);
    }
}
}
