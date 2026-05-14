using UnityEngine;
// STRATEGY PATTERN
public abstract class NPCBase : MonoBehaviour
{
    protected bool jugadorCerca = false;
    protected float tiempoEntreInteracciones = 5f;
    protected float timerInteraccion = 0f;

    private float cooldownMensaje = 1.5f;
    private float timerCooldown = 0f;
    private bool enCooldown = false;

    protected virtual void Update()
    {
        if (enCooldown)
        {
            timerCooldown += Time.deltaTime;
            if (timerCooldown >= cooldownMensaje)
            {
                enCooldown = false;
                timerCooldown = 0f;
            }
        }

        if (jugadorCerca && !enCooldown)
        {
            timerInteraccion += Time.deltaTime;
            if (timerInteraccion >= tiempoEntreInteracciones)
            {
                timerInteraccion = 0f;
                enCooldown = true;
                Interactuar();
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !enCooldown)
        {
            jugadorCerca = true;
            timerInteraccion = 0f;
            enCooldown = true;
            Interactuar();
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            timerInteraccion = 0f;
        }
    }

    protected abstract void Interactuar();
}