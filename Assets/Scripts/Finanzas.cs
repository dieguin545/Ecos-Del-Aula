using UnityEngine;

public class Finanzas : MonoBehaviour
{
    public GameObject Finanza;
    void Start()
    {
        Finanza.SetActive(false);
    }
    public void abrir()
    {
        if (Finanza.activeSelf)
        {
            Finanza.SetActive(false);
        }
        else
        {
            Finanza.SetActive(true);
        }
    }
}
