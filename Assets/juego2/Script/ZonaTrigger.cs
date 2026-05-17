using UnityEngine;
using Unity.Cinemachine;

public class ZonaTrigger : MonoBehaviour
{
    public CinemachineCamera vcamEstaZona;
    public ZonaTipo zonaTipo;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            vcamEstaZona.Priority = 10;
            BullyingDatabase.Instance.SetZona(zonaTipo);
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