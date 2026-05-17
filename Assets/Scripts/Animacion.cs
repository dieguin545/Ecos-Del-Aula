using UnityEngine;

public class Animacion : MonoBehaviour
{
    public float velocidad = 5f;
    private Animator animator;
    private Transform panelAccion;
    private Vector3 escalaOriginalPanel;

    void Start()
    {
        animator = GetComponent<Animator>();
        panelAccion = transform.Find("PanelAccion");
        if (panelAccion != null)
            escalaOriginalPanel = panelAccion.localScale;
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
            transform.localScale = new Vector3(27, 27, 27);
            if (panelAccion != null)
                panelAccion.localScale = escalaOriginalPanel;
        }
        else if (movimientoX < 0)
        {
            transform.localScale = new Vector3(-27, 27, 27);
            if (panelAccion != null)
                panelAccion.localScale = new Vector3(
                    -escalaOriginalPanel.x,
                    escalaOriginalPanel.y,
                    escalaOriginalPanel.z
                );
        }
    }
}
