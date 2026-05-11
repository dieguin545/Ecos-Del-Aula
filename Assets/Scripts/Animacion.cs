using UnityEngine;

public class Animacion : MonoBehaviour
{
public float velocidad = 5f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float movimientoX = Input.GetAxisRaw("Horizontal");
        float movimientoY = Input.GetAxisRaw("Vertical");

        Vector3 movimiento = new Vector3(movimientoX, movimientoY, 0);

        transform.position += movimiento.normalized * velocidad * Time.deltaTime;

        bool caminando = movimiento != Vector3.zero;
        animator.SetBool("Caminando", caminando);

        if (movimientoX > 0)
        {
            transform.localScale = new Vector3(32, 32, 32);
        }
        else if (movimientoX < 0)
        {
            transform.localScale = new Vector3(-32, 32, 32);
        }
    }
}
