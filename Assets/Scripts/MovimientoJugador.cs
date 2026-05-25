using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float velocidad = 5f;
    public float escalaVisualMinima = 1.15f;

    void Start()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null && transform.localScale.x < escalaVisualMinima)
        {
            transform.localScale = Vector3.one * escalaVisualMinima;
        }
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        float movimientoX = Input.GetAxisRaw("Horizontal");
        float movimientoY = Input.GetAxisRaw("Vertical");

        Vector3 movimiento = new Vector3(movimientoX, movimientoY, 0);

        transform.position += movimiento.normalized * velocidad * Time.deltaTime;
    }
}
