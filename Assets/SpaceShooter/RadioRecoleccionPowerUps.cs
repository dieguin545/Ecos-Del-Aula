using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RadioRecoleccionPowerUps : MonoBehaviour
{
    private NaveController nave;

    public void Inicializar(NaveController nave)
    {
        this.nave = nave;
    }

    private void Awake()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (nave == null)
        {
            nave = GetComponentInParent<NaveController>();
        }

        if (nave == null)
        {
            return;
        }

        PowerUpSpaceShooter powerUp = other.GetComponentInParent<PowerUpSpaceShooter>();

        if (powerUp != null)
        {
            powerUp.IntentarRecoger(nave);
        }
    }
}
