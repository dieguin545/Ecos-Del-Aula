using System.Collections;
using UnityEngine;

/// <summary>
/// Helper MonoBehaviour que permite ejecutar coroutines desde clases estáticas.
/// Se crea automáticamente en runtime. NO contiene lógica de gameplay.
/// Solo sirve como host para el rediseño visual de Ecos del Aula.
/// </summary>
public class EcosAulaUIHelper : MonoBehaviour
{
    private static EcosAulaUIHelper instancia;

    public static EcosAulaUIHelper Instancia
    {
        get
        {
            if (instancia == null)
            {
                GameObject go = new GameObject("[EcosAulaUIHelper]");
                instancia = go.AddComponent<EcosAulaUIHelper>();
                DontDestroyOnLoad(go);
            }

            return instancia;
        }
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EjecutarTrasFrames(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}
