# Guia Unity - Toques finales BRIV

Esta guia cubre lo unico que tienes que hacer en Unity Editor para que el
proyecto se vea 100% terminado. Todo lo demas (codigo, textos, navegacion,
nombres, branding, jerarquia) ya esta listo y funciona automaticamente al
abrir el proyecto.

> **Tiempo estimado: 15-20 minutos en total.**

---

## Antes de empezar

1. Abre Unity Hub.
2. Abre el proyecto `Bullying-and-Dungeons-no-mercy-main`.
3. Cuando termine de cargar, en el panel **Project** (abajo) ya vas a ver:
   - `Assets/Characters/CozyPeople/` (vacio, esperando tus sprites)
   - `Assets/UI/Fonts/` (vacio, esperando la fuente)
   - `Assets/UI/Backgrounds/` (vacio, esperando un fondo)

---

## Paso 1 - Importar la fuente bonita (5 min)

### 1.1 Descarga la fuente

- Ve a https://fonts.google.com
- Busca **Poppins** (recomendada) o **Inter** o **Manrope**.
- Clic en **Download family** (arriba derecha). Te baja un .zip.
- Descomprime el zip. Te quedan archivos `.ttf`.

### 1.2 Mete los .ttf en Unity

- En Unity, panel **Project**, ve a `Assets/UI/Fonts/`.
- Arrastra los archivos `Poppins-Bold.ttf` y `Poppins-Regular.ttf` desde tu Finder a esa carpeta.

### 1.3 Crea el Font Asset SDF (lo que TextMeshPro necesita)

Por cada .ttf:
- Clic derecho sobre `Poppins-Bold.ttf` -> **Create -> TextMeshPro -> Font Asset**.
- Te crea un archivo nuevo `Poppins-Bold SDF`. Ese es el que usa Unity.
- Repite con `Poppins-Regular.ttf`.

### 1.4 Aplica la fuente a los titulos BRIV

Por cada escena (`inicio`, `seleccion`, `SeleccionJuego`):
- Doble clic en la escena (panel Project -> Assets/Scenes/).
- En la Hierarchy busca los textos que dicen "BRIV", "Elige tu personaje", "Entry Filter", etc.
- Selecciona uno. En el Inspector busca el componente **TextMeshPro - Text (UI)**.
- En el campo **Font Asset**, clic en el circulito ⊙ -> elige `Poppins-Bold SDF`.
- Repite para todos los textos. Para los textos de botones usa `Poppins-Regular SDF`.
- Guarda con **Cmd+S**.

---

## Paso 2 - Reemplazar personajes feos por Cozy People (5 min)

### 2.1 Mete los sprites en Unity

- Descomprime el `Cozy People Asset Pack`.
- Selecciona los PNGs que quieras usar (ej. 5 personajes distintos).
- Arrastralos a `Assets/Characters/CozyPeople/` en Unity.

### 2.2 Configura como Sprite

- Selecciona uno de los PNGs en Project.
- En el Inspector verifica que **Texture Type** este en **Sprite (2D and UI)**.
- Si lo cambiaste, clic en **Apply** abajo.
- Repite con todos.

### 2.3 Reemplaza los personajes en los prefabs

Hay 5 prefabs llamados `Personaje1.prefab` ... `Personaje5.prefab` en `Assets/`.

Por cada prefab:
- Doble clic para abrirlo.
- En la Hierarchy del prefab busca el componente **Sprite Renderer** (o **Image** si es UI).
- En el campo **Sprite**, arrastra uno de tus sprites Cozy People.
- Boton **<** arriba a la izquierda de la Hierarchy para salir del prefab.
- Guarda con **Cmd+S**.

Asi cuando el jugador haga "Cambiar Personaje" en el menu BRIV, ya vera los personajes pixel art bonitos en vez de figuras blancas.

---

## Paso 3 - Fondo bonito para el menu BRIV (3 min)

### 3.1 Consigue un fondo

- Recomiendo: https://www.freepik.com/free-photos-vectors/abstract-dark-purple
- Busca algo como "dark gradient background", "neon city", "abstract purple".
- Descarga un PNG/JPG.

### 3.2 Importalo

- Arrastra el archivo a `Assets/UI/Backgrounds/` en Unity.
- Selecciona la imagen. **Texture Type** = **Sprite (2D and UI)**. **Apply**.

### 3.3 Ponlo de fondo en inicio.unity

- Abre `Assets/Scenes/inicio.unity`.
- En la Hierarchy busca el **Canvas**. Expandelo.
- Clic derecho sobre Canvas -> **UI -> Image**. Aparece un Image nuevo.
- Renombralo a `Fondo`.
- Importante: arrastra `Fondo` hasta que quede como **PRIMER hijo** del Canvas (asi queda detras de todo).
- Con `Fondo` seleccionado, en el componente Image:
  - **Source Image**: arrastra tu fondo desde Project.
- En el componente Rect Transform de `Fondo`:
  - Clic en el rectangulo de anclajes (arriba izquierda).
  - Manten **Alt** y elige el rectangulo grande del fondo (el que estira a todo el canvas).
  - Esto hace que el fondo cubra toda la pantalla.

Repite para `seleccion.unity` y `SeleccionJuego.unity` si quieres consistencia.

---

## Paso 4 - Llenar el expediente del caso en Entry Filter (5 min)

El sistema de Entry Filter ahora soporta mostrar el caso como un expediente
visual con nombre de estudiante, categoria y senales detectadas. Para llenarlo:

### 4.1 Abre la escena Juego

- Doble clic en `Assets/Scenes/Juego.unity`.

### 4.2 Encuentra el ControlCorreo

- En la Hierarchy busca el objeto que tiene el componente `ControlCorreo`.
- Selecciona el objeto. En el Inspector veras el componente con tres listas:
  - **Correos Faciles**
  - **Correos Medios**
  - **Correos Dificiles**

### 4.3 Expande un correo y llena los nuevos campos opcionales

- Expande **Correos Faciles** y luego **Element 0**.
- Ahora veras dos secciones nuevas:
  - **Datos del correo (original)**: remitente, asunto, texto, esBullying (no toques esto si ya estaba lleno).
  - **Datos del expediente (opcionales)**:
    - `Nombre Estudiante`: por ejemplo "Maria Gonzalez"
    - `Id Caso`: por ejemplo "042"
    - `Categoria`: dropdown -> elige AcosoDirecto, ExclusionDigital, etc.
    - `Senales`: clic en + para anadir lineas. Ejemplo:
      - "Tono agresivo en el saludo"
      - "Amenaza velada al final"
      - "Remitente fuera del dominio escolar"

### 4.4 Repite para los correos que mas te interesen

No tienes que llenar TODOS, solo los que quieras que se vean como expediente.
Los que dejes vacios se muestran como antes (texto simple).

### 4.5 Guarda con Cmd+S

Cuando juegues Entry Filter ahora vas a ver:

```
CASO #042   Acoso directo
Estudiante: Maria Gonzalez

[el texto original del correo]

Senales detectadas:
  - Tono agresivo en el saludo
  - Amenaza velada al final
  - Remitente fuera del dominio escolar
```

Cuando te equivoques en una decision, ahora dice exactamente por que:
- "ERROR - Era un caso de bullying y lo dejaste pasar."
- "ERROR - El correo era legitimo y lo reportaste sin razon."

---

## Paso 5 - Limpieza opcional de assets feos

En `Assets/` hay estos archivos que no aportan al proyecto y se pueden borrar:

- `question-mark-icon-free-vector.jpg`
- `png-transparent-logo-finance-brand-business-...removebg-preview.png`
- `images.jpg`
- `apgado.png`, `apgado (1).png`, `apgado (2).png` (si no se usan)

Para borrar uno: clic derecho en Project -> **Delete** -> confirma.

> **Cuidado:** antes de borrar, ten Unity abierto y revisa que el asset no este referenciado en ninguna escena. Si lo borras y se referenciaba, Unity muestra un cuadro rosa en lugar de la imagen.

---

## Checklist final

- [ ] Paso 1 - Fuente Poppins aplicada a titulos
- [ ] Paso 2 - Personajes Cozy People en los 5 prefabs
- [ ] Paso 3 - Fondo bonito en menu BRIV
- [ ] Paso 4 - Al menos 3 correos con expediente completo
- [ ] Paso 5 - Assets feos borrados

Al terminar, pulsa Play en la escena `inicio.unity`. Deberias ver el menu BRIV
con tu fuente nueva, los personajes nuevos al cambiar personaje, y el sistema
de casos funcionando en Entry Filter.
