using UnityEngine;
using DG.Tweening;

public class FlechaDireccion : MonoBehaviour
{
    public float distanciaDeteccion = 35f;
    private Transform jugador;
    private SpriteRenderer sr;
    private Tween pulso;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.25f, 0.95f, 1f, 0.9f);
            sr.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MostrarMarcador(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MostrarMarcador(false);
        }
    }

    private void MostrarMarcador(bool visible)
    {
        if (sr == null)
        {
            return;
        }

        pulso?.Kill();
        transform.DOKill();
        sr.DOKill();

        if (visible)
        {
            sr.enabled = true;
            sr.color = new Color(0.25f, 0.95f, 1f, 0f);
            transform.localScale = Vector3.one * 0.92f;
            sr.DOFade(0.92f, 0.16f).SetEase(Ease.OutQuad).SetLink(gameObject);
            pulso = transform
                .DOScale(1.08f, 0.42f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);
            return;
        }

        sr.DOFade(0f, 0.14f)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                if (sr != null)
                {
                    sr.enabled = false;
                }
            });
    }

    private void OnDestroy()
    {
        pulso?.Kill();
        transform.DOKill();
        if (sr != null)
        {
            sr.DOKill();
        }
    }
}
