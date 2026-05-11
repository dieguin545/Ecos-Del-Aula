using UnityEngine;
using Unity.Cinemachine;

public class ZonaTrigger : MonoBehaviour
{
    public CinemachineCamera vcamEstaZona;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entró al trigger: " + other.gameObject.name + " Tag: " + other.tag);
        if (other.CompareTag("Player"))
        {
            vcamEstaZona.Priority = 10;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            vcamEstaZona.Priority = 0;
        }
    }
}