using UnityEngine;

public class MovimientoJugadorConCamara : MonoBehaviour
{
    public float velocidad = 4f;
    public Transform camara;
    public LayerMask whatIsGround;
    public float spriteScale = 1.82f;

    private Rigidbody rb;
    private Vector3 direccion;
    private float inputMagnitude;
    private int lastDirectionRow = 0; // Default: Down (Row 0)
    private float animationTimer = 0f;
    private const int ColumnasAnimacion = 3;
    private const int FilasAnimacion = 8;
    private const float PixelsPorUnidadSprite = 100f;

    private Transform spriteChild;
    private SpriteRenderer spriteRenderer;
    private Transform groundPointChild;
    private Transform shadowChild;
    private SpriteRenderer shadowRenderer;
    private Sprite[] sprites;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configure Rigidbody constraints for 2.5D stable physics
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ;
        }

        // Auto-configure ground layer mask if not set
        if (whatIsGround.value == 0)
        {
            int layerIdx = LayerMask.NameToLayer("Ground");
            if (layerIdx == -1) layerIdx = 6;
            whatIsGround = 1 << layerIdx;
        }

        // Set up hierarchy dynamically
        CrearHijosDinamicos();
        CargarSprites();
        AplicarSpriteInicial();
        ActualizarVisualCapsula();
    }

    void Start()
    {
        AplicarSpriteInicial();
        ActualizarVisualCapsula();
        // Auto-assign Ground layer to floor objects
        ConfigurarCapasSuelo();
    }

    void OnEnable()
    {
        direccion = Vector3.zero;
        inputMagnitude = 0f;
        AplicarSpriteInicial();
        ActualizarVisualCapsula();
    }

    void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
        if (spriteRenderer != null && sprites != null && sprites.Length > lastDirectionRow * 3 + 1)
        {
            spriteRenderer.sprite = sprites[lastDirectionRow * 3 + 1]; // Idle frame of last direction
        }
    }

    void Update()
    {
        ActualizarVisualCapsula();

        // Skip input processing if paused
        if (MenuPausaAccesibilidad.EstaPausado || InteraccionPC.PCAbierta)
        {
            direccion = Vector3.zero;
            inputMagnitude = 0f;
            ActualizarAnimacion();
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (camara == null)
        {
            Camera cam = Camera.main;
            if (cam != null) camara = cam.transform;
        }

        if (camara != null)
        {
            Vector3 adelante = camara.forward;
            adelante.y = 0f;
            adelante.Normalize();

            Vector3 derecha = camara.right;
            derecha.y = 0f;
            derecha.Normalize();

            direccion = (adelante * vertical + derecha * horizontal).normalized;
            inputMagnitude = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);
        }
        else
        {
            direccion = new Vector3(horizontal, 0f, vertical).normalized;
            inputMagnitude = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);
        }

        ActualizarAnimacion();
        ActualizarBillboard();
    }

    void FixedUpdate()
    {
        // Set horizontal velocity while preserving vertical velocity for gravity
        if (rb != null)
        {
            Vector3 velHorizontal = (InteraccionPC.PCAbierta || MenuPausaAccesibilidad.EstaPausado)
                ? Vector3.zero
                : direccion * velocidad;
            rb.linearVelocity = new Vector3(velHorizontal.x, rb.linearVelocity.y, velHorizontal.z);
        }

        ActualizarSombra();
    }

    void CrearHijosDinamicos()
    {
        float localBottom = 0f;
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            localBottom = col.center.y - (col.height / 2f);
        }
        else
        {
            localBottom = -1.0f; // Default fallback
        }

        // 1. Sprite Child
        Transform spriteExistente = transform.Find("Sprite");
        GameObject spriteObj = spriteExistente != null
            ? spriteExistente.gameObject
            : new GameObject("Sprite");
        spriteObj.transform.SetParent(transform, false);
        spriteObj.transform.localPosition = new Vector3(0f, localBottom, 0f);
        spriteObj.transform.localRotation = Quaternion.identity;
        spriteObj.transform.localScale = Vector3.one * spriteScale;
        spriteRenderer = spriteObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        spriteRenderer.receiveShadows = false;
        spriteRenderer.sortingOrder = 10;
        spriteChild = spriteObj.transform;

        // 2. GroundPoint Child
        Transform gpExistente = transform.Find("GroundPoint");
        GameObject gpObj = gpExistente != null
            ? gpExistente.gameObject
            : new GameObject("GroundPoint");
        gpObj.transform.SetParent(transform, false);
        gpObj.transform.localPosition = new Vector3(0f, localBottom + 0.05f, 0f);
        groundPointChild = gpObj.transform;

        // 3. Shadow Child
        Transform shadowExistente = transform.Find("Shadow");
        GameObject shadowObj = shadowExistente != null
            ? shadowExistente.gameObject
            : new GameObject("Shadow");
        shadowObj.transform.SetParent(transform, false);
        shadowObj.transform.localPosition = new Vector3(0f, localBottom, 0f);
        shadowObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shadowObj.transform.localScale = new Vector3(spriteScale * 0.52f, spriteScale * 0.34f, 1f);
        shadowRenderer = shadowObj.GetComponent<SpriteRenderer>();
        if (shadowRenderer == null)
        {
            shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
        }
        shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        shadowRenderer.receiveShadows = false;
        shadowRenderer.sortingOrder = 9;
        shadowChild = shadowObj.transform;

        // Create smooth shadow texture in memory
        if (shadowRenderer.sprite == null)
        {
            Texture2D tex = CreateShadowTexture();
            Sprite shadowSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), PixelsPorUnidadSprite);
            shadowRenderer.sprite = shadowSprite;
        }
    }

    void CargarSprites()
    {
        sprites = Resources.LoadAll<Sprite>("EntryFilter/Spritesheet_Trabajador");
        if (sprites != null && sprites.Length > 0)
        {
            System.Array.Sort(sprites, CompararSpritesPorFilaColumna);
        }

        if (sprites == null || sprites.Length < FilasAnimacion * ColumnasAnimacion)
        {
            Texture2D textura = Resources.Load<Texture2D>("EntryFilter/Spritesheet_Trabajador");
            if (textura != null)
            {
                sprites = CrearSpritesFallbackDesdeTextura(textura);
            }
        }

        if (sprites == null || sprites.Length < FilasAnimacion * ColumnasAnimacion)
        {
            Debug.LogError("No se pudieron cargar los 24 sprites de Assets/Resources/EntryFilter/Spritesheet_Trabajador.png. Se mantiene la cápsula como fallback visible.");
        }
    }

    Sprite[] CrearSpritesFallbackDesdeTextura(Texture2D textura)
    {
        int[,] rects =
        {
            {489, 942, 82, 138}, {685, 942, 81, 138}, {867, 942, 83, 138},
            {487, 808, 88, 134}, {681, 808, 91, 134}, {867, 808, 92, 134},
            {484, 681, 81, 127}, {678, 681, 81, 127}, {865, 681, 82, 127},
            {482, 558, 85, 123}, {676, 558, 86, 123}, {866, 558, 83, 123},
            {487, 432, 78, 126}, {683, 432, 77, 126}, {872, 432, 77, 126},
            {484, 322, 82, 110}, {677, 322, 79, 110}, {864, 322, 81, 110},
            {485, 188, 81, 134}, {678, 188, 81, 134}, {865, 188, 81, 134},
            {484, 52, 86, 136}, {673, 52, 89, 136}, {860, 52, 90, 134},
        };

        Sprite[] resultado = new Sprite[FilasAnimacion * ColumnasAnimacion];
        for (int i = 0; i < resultado.Length; i++)
        {
            Rect rect = new Rect(rects[i, 0], rects[i, 1], rects[i, 2], rects[i, 3]);
            resultado[i] = Sprite.Create(
                textura,
                rect,
                new Vector2(0.5f, 0f),
                PixelsPorUnidadSprite,
                0,
                SpriteMeshType.FullRect
            );
            resultado[i].name = $"Trabajador_Fallback_{i / ColumnasAnimacion:00}_{i % ColumnasAnimacion:00}";
        }

        return resultado;
    }

    void AplicarSpriteInicial()
    {
        if (spriteRenderer == null || sprites == null || sprites.Length == 0)
        {
            return;
        }

        int spriteIndex = Mathf.Clamp(lastDirectionRow * ColumnasAnimacion + 1, 0, sprites.Length - 1);
        spriteRenderer.sprite = sprites[spriteIndex];
        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.white;
        if (spriteChild != null)
        {
            spriteChild.localScale = Vector3.one * spriteScale;
        }
    }

    int CompararSpritesPorFilaColumna(Sprite a, Sprite b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        Rect rectA = a.textureRect;
        Rect rectB = b.textureRect;
        int filaA = Mathf.RoundToInt((a.texture.height - rectA.yMax) / (a.texture.height / (float)FilasAnimacion));
        int filaB = Mathf.RoundToInt((b.texture.height - rectB.yMax) / (b.texture.height / (float)FilasAnimacion));

        if (filaA != filaB)
        {
            return filaA.CompareTo(filaB);
        }

        return rectA.x.CompareTo(rectB.x);
    }

    void ActualizarVisualCapsula()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null)
        {
            return;
        }

        bool haySpriteVisible = spriteRenderer != null && spriteRenderer.sprite != null;
        r.enabled = !haySpriteVisible;
        r.shadowCastingMode = haySpriteVisible
            ? UnityEngine.Rendering.ShadowCastingMode.Off
            : UnityEngine.Rendering.ShadowCastingMode.On;

    }

    void ConfigurarCapasSuelo()
    {
        int layerIdx = LayerMask.NameToLayer("Ground");
        if (layerIdx == -1) layerIdx = 6;

        GameObject[] allObjs = FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var obj in allObjs)
        {
            if (obj != null && (obj.name.ToLower().Contains("piso") || obj.name.ToLower().Contains("floor")))
            {
                obj.layer = layerIdx;
            }
        }
    }

    Texture2D CreateShadowTexture()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        float center = size / 2.0f - 0.5f;
        float maxRadius = size / 2.0f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1.0f - (dist / maxRadius));
                // Smooth ellipse falloff
                alpha = Mathf.SmoothStep(0f, 1f, alpha) * 0.5f; // Max 50% opacity
                colors[y * size + x] = new Color(0f, 0f, 0f, alpha);
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    void ActualizarAnimacion()
    {
        if (sprites == null || sprites.Length == 0) return;

        if (inputMagnitude > 0.05f && camara != null)
        {
            Vector3 camFwd = camara.forward;
            camFwd.y = 0f;
            camFwd.Normalize();

            Vector3 camRight = camara.right;
            camRight.y = 0f;
            camRight.Normalize();

            float localForward = Vector3.Dot(direccion, camFwd);
            float localRight = Vector3.Dot(direccion, camRight);

            lastDirectionRow = ObtenerFilaDireccion8(localRight, localForward);

            // Update walking animation loop (0 -> 1 -> 2 -> 1)
            animationTimer += Time.deltaTime * (velocidad * 2.5f);
            int[] walkSequence = { 0, 1, 2, 1 };
            int frameIndex = walkSequence[Mathf.FloorToInt(animationTimer) % walkSequence.Length];

            int spriteIndex = lastDirectionRow * 3 + frameIndex;
            if (spriteIndex < sprites.Length)
            {
                spriteRenderer.sprite = sprites[spriteIndex];
            }
        }
        else
        {
            // Idle: frame 1 (center) of last direction
            int spriteIndex = lastDirectionRow * 3 + 1;
            if (spriteIndex < sprites.Length)
            {
                spriteRenderer.sprite = sprites[spriteIndex];
            }
            animationTimer = 0f;
        }
    }

    void ActualizarBillboard()
    {
        if (spriteChild == null || camara == null) return;

        Vector3 dirToCam = camara.position - spriteChild.position;
        dirToCam.y = 0f; // Lock Y axis rotation
        if (dirToCam != Vector3.zero)
        {
            spriteChild.rotation = Quaternion.LookRotation(dirToCam.normalized, Vector3.up);
        }
    }

    int ObtenerFilaDireccion8(float localRight, float localForward)
    {
        if (Mathf.Abs(localRight) < 0.05f && Mathf.Abs(localForward) < 0.05f)
        {
            return lastDirectionRow;
        }

        float angle = Mathf.Atan2(localRight, localForward) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }

        int sector = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;

        // El sprite mira como lámina 2.5D hacia cámara: las diagonales se invierten
        // visualmente respecto al eje horizontal de entrada, así que se corrigen aquí.
        switch (sector)
        {
            case 0: return 4; // Up
            case 1: return 3; // UpRight visual
            case 2: return 2; // Right visual
            case 3: return 1; // DownRight visual
            case 4: return 0; // Down
            case 5: return 7; // DownLeft visual
            case 6: return 6; // Left visual
            case 7: return 5; // UpLeft visual
            default: return lastDirectionRow;
        }
    }

    void ActualizarSombra()
    {
        if (shadowChild == null || groundPointChild == null) return;

        RaycastHit hit;
        Vector3 rayStart = groundPointChild.position;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 2.0f, whatIsGround))
        {
            shadowChild.gameObject.SetActive(true);
            shadowChild.position = hit.point + Vector3.up * 0.01f;
            shadowChild.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            shadowChild.gameObject.SetActive(false);
        }
    }
}
