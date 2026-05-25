using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class UIAudioManager : MonoBehaviour
{
    private static UIAudioManager instancia;

    private AudioSource fuente;
    private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, float> ultimosSonidos = new Dictionary<string, float>();

    [SerializeField] private float volumen = 0.45f;
    [SerializeField] private float cooldown = 0.045f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        if (instancia != null || FindAnyObjectByType<UIAudioManager>() != null)
        {
            return;
        }

        GameObject go = new GameObject("UIAudioManager");
        DontDestroyOnLoad(go);
        go.AddComponent<UIAudioManager>();
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

        fuente = GetComponent<AudioSource>();
        if (fuente == null)
        {
            fuente = gameObject.AddComponent<AudioSource>();
        }

        fuente.playOnAwake = false;
        fuente.loop = false;
        fuente.spatialBlend = 0f;
        SceneManager.sceneLoaded += AlCargarEscena;
        RegistrarBotonesEnEscena();
    }

    private void OnDestroy()
    {
        if (instancia == this)
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            instancia = null;
        }
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        RegistrarBotonesEnEscena();
    }

    private void RegistrarBotonesEnEscena()
    {
        Button[] botones = FindObjectsByType<Button>(FindObjectsInactive.Include);
        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];
            if (boton != null && boton.GetComponent<EcosAulaBotonAudio>() == null)
            {
                boton.gameObject.AddComponent<EcosAulaBotonAudio>();
            }
        }
    }

    public static void PlayHover() => Reproducir("switch-a");
    public static void PlaySelect() => Reproducir("switch-b");
    public static void PlayConfirm() => Reproducir("click-a");
    public static void PlayCancel() => Reproducir("click-b");
    public static void PlayOpen() => Reproducir("tap-a");
    public static void PlayClose() => Reproducir("tap-b");
    public static void PlayError() => Reproducir("switch-b");
    public static void PlayNotification() => Reproducir("tap-a");
    public static void PlayEvidenceSaved() => Reproducir("click-a");
    public static void PlayMissionComplete() => Reproducir("tap-b");
    public static void PlayAnxietyUp() => Reproducir("switch-b", 0.18f);
    public static void PlayAnxietyDown() => Reproducir("switch-a", 0.18f);

    private static void Reproducir(string nombre, float cooldownPersonalizado = -1f)
    {
        if (instancia == null)
        {
            CrearSiHaceFalta();
        }

        if (instancia == null)
        {
            return;
        }

        instancia.ReproducirInterno(nombre, cooldownPersonalizado);
    }

    private void ReproducirInterno(string nombre, float cooldownPersonalizado)
    {
        if (fuente == null || string.IsNullOrWhiteSpace(nombre))
        {
            return;
        }

        float cooldownActivo = cooldownPersonalizado >= 0f ? cooldownPersonalizado : cooldown;
        if (ultimosSonidos.TryGetValue(nombre, out float ultimo) && Time.unscaledTime - ultimo < cooldownActivo)
        {
            return;
        }

        AudioClip clip = ObtenerClip(nombre);
        if (clip == null)
        {
            return;
        }

        ultimosSonidos[nombre] = Time.unscaledTime;
        fuente.PlayOneShot(clip, volumen);
    }

    private AudioClip ObtenerClip(string nombre)
    {
        if (clips.TryGetValue(nombre, out AudioClip clip))
        {
            return clip;
        }

        clip = Resources.Load<AudioClip>("Audio/UI/Kenney/" + nombre);
        if (clip != null)
        {
            clips[nombre] = clip;
        }

        return clip;
    }
}

public class EcosAulaBotonAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    private Vector3 escalaBase = Vector3.one;
    private Outline outlineSeleccion;

    private void Awake()
    {
        escalaBase = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;

        Image imagen = GetComponent<Image>();
        if (imagen != null)
        {
            outlineSeleccion = imagen.GetComponent<Outline>();
            if (outlineSeleccion == null)
            {
                outlineSeleccion = imagen.gameObject.AddComponent<Outline>();
            }

            outlineSeleccion.effectColor = new Color(0.32f, 0.92f, 1f, 0f);
            outlineSeleccion.effectDistance = new Vector2(2f, -2f);
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (outlineSeleccion != null)
        {
            outlineSeleccion.DOKill();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIAudioManager.PlayHover();
        AnimarSeleccion(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimarSeleccion(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        UIAudioManager.PlaySelect();
        AnimarSeleccion(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        AnimarSeleccion(false);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        UIAudioManager.PlayConfirm();
        AnimarConfirmacion();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIAudioManager.PlayConfirm();
        AnimarConfirmacion();
    }

    private void AnimarSeleccion(bool activo)
    {
        transform.DOKill();
        transform.DOScale(activo ? escalaBase * 1.055f : escalaBase, 0.14f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetLink(gameObject);

        if (outlineSeleccion != null)
        {
            outlineSeleccion.DOKill();
            Color destino = new Color(0.32f, 0.92f, 1f, activo ? 0.85f : 0f);
            DOTween.To(
                    () => outlineSeleccion.effectColor,
                    c => outlineSeleccion.effectColor = c,
                    destino,
                    0.12f
                )
                .SetUpdate(true)
                .SetLink(gameObject);
        }
    }

    private void AnimarConfirmacion()
    {
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.065f, 0.16f, 6, 0.65f)
            .SetUpdate(true)
            .SetLink(gameObject);
    }
}
