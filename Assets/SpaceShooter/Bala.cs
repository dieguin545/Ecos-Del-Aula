using UnityEngine;

public class Bala : MonoBehaviour
{
    public float velocidad = 20f;
    Vector3 direccion;

    public void SetDireccion(Vector3 dir)
    {
        direccion = dir;
    }

    void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;

        if (Vector3.Distance(transform.position, Vector3.zero) > 60f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Meteorito"))
        {
            Meteorito m = otro.GetComponent<Meteorito>();
            if (m != null) m.Morir();
            Destroy(gameObject);
        }
    }
}