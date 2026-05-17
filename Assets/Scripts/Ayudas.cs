using UnityEngine;
public class Ayudas : MonoBehaviour
{
    public GameObject ayuda;
    void Start()
    {
        ayuda.SetActive(true);
    }
    public void abrir()
    {
        if(ayuda.activeSelf)
    {
        ayuda.SetActive(false);
    }
    else
    {
        ayuda.SetActive(true);
    }
    }

}