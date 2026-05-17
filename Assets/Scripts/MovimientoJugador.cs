using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float velocidad = 5f;

    void Update()
    {
        float movimientoX = Input.GetAxisRaw("Horizontal");
        float movimientoY = Input.GetAxisRaw("Vertical");

        Vector3 movimiento = new Vector3(movimientoX, movimientoY, 0);

        transform.position += movimiento.normalized * velocidad * Time.deltaTime;
    }
}
