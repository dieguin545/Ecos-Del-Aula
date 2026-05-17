using UnityEngine;
using System.Collections.Generic;

public enum ZonaTipo
{
    Pasillo,
    Salon,
    Biblioteca,
    Cafeteria,
    Gimnasio,
    Patio
}

// TAD - Base de datos de mensajes de bullying
public class BullyingDatabase : MonoBehaviour
{
    public static BullyingDatabase Instance;

    // Diccionario anidado: Zona -> Personaje -> Lista circular
    private Dictionary<ZonaTipo, Dictionary<PersonajeType, CircularMessageList>> mensajesEspecificos;
    
    // Lista circular de mensajes neutros por zona
    private Dictionary<ZonaTipo, CircularMessageList> mensajesNeutros;

    private PersonajeType personajeActivo;
    private ZonaTipo zonaActual = ZonaTipo.Pasillo;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        InicializarMensajes();
    }

    public void SetPersonaje(PersonajeType tipo)
    {
        personajeActivo = tipo;
    }

    public void SetZona(ZonaTipo zona)
    {
        zonaActual = zona;
    }

    public BullyingMessage GetMensaje(bool neutro = false)
    {
        if (neutro)
        {
            if (mensajesNeutros.ContainsKey(zonaActual))
                return mensajesNeutros[zonaActual].GetRandom();
        }
        else
        {
            if (mensajesEspecificos.ContainsKey(zonaActual) &&
                mensajesEspecificos[zonaActual].ContainsKey(personajeActivo))
                return mensajesEspecificos[zonaActual][personajeActivo].GetNext();
        }
        return null;
    }

    private void InicializarMensajes()
    {
        mensajesEspecificos = new Dictionary<ZonaTipo, Dictionary<PersonajeType, CircularMessageList>>();
        mensajesNeutros = new Dictionary<ZonaTipo, CircularMessageList>();

        // ========== PASILLO ==========
        var pasillo = new Dictionary<PersonajeType, CircularMessageList>();

        var pasilloNegro = new CircularMessageList();
        pasilloNegro.AddMessage(new BullyingMessage("¿A dónde crees que vas?", 8f));
        pasilloNegro.AddMessage(new BullyingMessage("Este pasillo no es para ti.", 10f));
        pasilloNegro.AddMessage(new BullyingMessage("Sigues por aquí, qué sorpresa.", 7f));
        pasillo[PersonajeType.Negro] = pasilloNegro;

        var pasilloEmo = new CircularMessageList();
        pasilloEmo.AddMessage(new BullyingMessage("Mira quién sale de su cueva.", 8f));
        pasilloEmo.AddMessage(new BullyingMessage("¿Te perdiste el funeral?", 10f));
        pasilloEmo.AddMessage(new BullyingMessage("Qué oscuro te ves hoy, como siempre.", 7f));
        pasillo[PersonajeType.Emo] = pasilloEmo;

        var pasilloOtaku = new CircularMessageList();
        pasilloOtaku.AddMessage(new BullyingMessage("¿Buscando tu anime favorito en el pasillo?", 7f));
        pasilloOtaku.AddMessage(new BullyingMessage("Deberías estar en Japón, no aquí.", 9f));
        pasilloOtaku.AddMessage(new BullyingMessage("¿Ese es un pin de anime en tu mochila?", 6f));
        pasillo[PersonajeType.Otaku] = pasilloOtaku;

        var pasilloFurro = new CircularMessageList();
        pasilloFurro.AddMessage(new BullyingMessage("¿Vas a ladrar o a caminar normal?", 10f));
        pasilloFurro.AddMessage(new BullyingMessage("El zoológico está por allá.", 9f));
        pasilloFurro.AddMessage(new BullyingMessage("Qué raro verte sin orejas puestas.", 7f));
        pasillo[PersonajeType.Furro] = pasilloFurro;

        var pasilloNerd = new CircularMessageList();
        pasilloNerd.AddMessage(new BullyingMessage("¿Memorizaste el pasillo también?", 6f));
        pasilloNerd.AddMessage(new BullyingMessage("Cuidado no te tropieces con tus libros.", 7f));
        pasilloNerd.AddMessage(new BullyingMessage("El sabelotodo ya llegó.", 8f));
        pasillo[PersonajeType.Nerd] = pasilloNerd;

        mensajesEspecificos[ZonaTipo.Pasillo] = pasillo;

        var pasilloNeutros = new CircularMessageList();
        pasilloNeutros.AddMessage(new BullyingMessage("Este pasillo huele raro hoy.", 0f));
        pasilloNeutros.AddMessage(new BullyingMessage("¿Alguien vio mis audífonos?", 0f));
        pasilloNeutros.AddMessage(new BullyingMessage("El profe de mate llegó temprano hoy.", 0f));
        mensajesNeutros[ZonaTipo.Pasillo] = pasilloNeutros;

        // ========== CAFETERIA ==========
        var cafeteria = new Dictionary<PersonajeType, CircularMessageList>();

        var cafNegro = new CircularMessageList();
        cafNegro.AddMessage(new BullyingMessage("¿Eso es lo que comes? Qué típico.", 8f));
        cafNegro.AddMessage(new BullyingMessage("Siéntate en otro lado.", 10f));
        cafNegro.AddMessage(new BullyingMessage("Este puesto está ocupado.", 9f));
        cafeteria[PersonajeType.Negro] = cafNegro;

        var cafEmo = new CircularMessageList();
        cafEmo.AddMessage(new BullyingMessage("¿Comes o solo sufres?", 9f));
        cafEmo.AddMessage(new BullyingMessage("No manches el ambiente con tu tristeza.", 8f));
        cafEmo.AddMessage(new BullyingMessage("¿Por qué comes solo otra vez?", 7f));
        cafeteria[PersonajeType.Emo] = cafEmo;

        var cafOtaku = new CircularMessageList();
        cafOtaku.AddMessage(new BullyingMessage("¿Esperabas comida japonesa?", 7f));
        cafOtaku.AddMessage(new BullyingMessage("Tus personajes de anime no te van a dar de comer.", 8f));
        cafOtaku.AddMessage(new BullyingMessage("¿Comes viendo anime en el teléfono?", 6f));
        cafeteria[PersonajeType.Otaku] = cafOtaku;

        var cafFurro = new CircularMessageList();
        cafFurro.AddMessage(new BullyingMessage("¿Comes con las manos o con las patas?", 10f));
        cafFurro.AddMessage(new BullyingMessage("El comedero está por allá.", 9f));
        cafFurro.AddMessage(new BullyingMessage("No te comas a los demás estudiantes.", 8f));
        cafeteria[PersonajeType.Furro] = cafFurro;

        var cafNerd = new CircularMessageList();
        cafNerd.AddMessage(new BullyingMessage("¿Estudiando mientras comes? Qué triste.", 7f));
        cafNerd.AddMessage(new BullyingMessage("Hasta aquí traes libros.", 6f));
        cafNerd.AddMessage(new BullyingMessage("¿Le pediste permiso al profe para comer?", 8f));
        cafeteria[PersonajeType.Nerd] = cafNerd;

        mensajesEspecificos[ZonaTipo.Cafeteria] = cafeteria;

        var cafNeutros = new CircularMessageList();
        cafNeutros.AddMessage(new BullyingMessage("Esta comida apesta hoy.", 0f));
        cafNeutros.AddMessage(new BullyingMessage("¿Alguien quiere el postre?", 0f));
        cafNeutros.AddMessage(new BullyingMessage("La fila está larguísima hoy.", 0f));
        mensajesNeutros[ZonaTipo.Cafeteria] = cafNeutros;

        // ========== BIBLIOTECA ==========
        var biblioteca = new Dictionary<PersonajeType, CircularMessageList>();

        var bibNegro = new CircularMessageList();
        bibNegro.AddMessage(new BullyingMessage("¿Sabes leer eso?", 10f));
        bibNegro.AddMessage(new BullyingMessage("Este no es tu lugar.", 9f));
        bibNegro.AddMessage(new BullyingMessage("Qué raro verte con un libro.", 8f));
        biblioteca[PersonajeType.Negro] = bibNegro;

        var bibEmo = new CircularMessageList();
        bibEmo.AddMessage(new BullyingMessage("¿Buscando poemas de tristeza?", 8f));
        bibEmo.AddMessage(new BullyingMessage("¿Otro libro oscuro?", 7f));
        bibEmo.AddMessage(new BullyingMessage("La sección de terror es para ti.", 9f));
        biblioteca[PersonajeType.Emo] = bibEmo;

        var bibOtaku = new CircularMessageList();
        bibOtaku.AddMessage(new BullyingMessage("Los manga no son libros de verdad.", 9f));
        bibOtaku.AddMessage(new BullyingMessage("¿Buscando tu próximo waifu en los libros?", 8f));
        bibOtaku.AddMessage(new BullyingMessage("Esto no es una tienda de anime.", 7f));
        biblioteca[PersonajeType.Otaku] = bibOtaku;

        var bibFurro = new CircularMessageList();
        bibFurro.AddMessage(new BullyingMessage("¿Los animales leen ahora?", 10f));
        bibFurro.AddMessage(new BullyingMessage("No babees los libros.", 9f));
        bibFurro.AddMessage(new BullyingMessage("¿Buscando un libro sobre tu especie?", 8f));
        biblioteca[PersonajeType.Furro] = bibFurro;

        var bibNerd = new CircularMessageList();
        bibNerd.AddMessage(new BullyingMessage("Ya leíste todos estos, ¿verdad?", 6f));
        bibNerd.AddMessage(new BullyingMessage("¿Viniste a memorizar la biblioteca entera?", 7f));
        bibNerd.AddMessage(new BullyingMessage("Qué sorpresa verte aquí, no.", 6f));
        biblioteca[PersonajeType.Nerd] = bibNerd;

        mensajesEspecificos[ZonaTipo.Biblioteca] = biblioteca;

        var bibNeutros = new CircularMessageList();
        bibNeutros.AddMessage(new BullyingMessage("Este libro lleva meses aquí.", 0f));
        bibNeutros.AddMessage(new BullyingMessage("Hace frío aquí dentro.", 0f));
        bibNeutros.AddMessage(new BullyingMessage("La bibliotecaria está de mal humor hoy.", 0f));
        mensajesNeutros[ZonaTipo.Biblioteca] = bibNeutros;

        // ========== GIMNASIO ==========
        var gimnasio = new Dictionary<PersonajeType, CircularMessageList>();

        var gimNegro = new CircularMessageList();
        gimNegro.AddMessage(new BullyingMessage("¿Vienes a demostrar algo?", 8f));
        gimNegro.AddMessage(new BullyingMessage("Todos saben que eres bueno en esto.", 7f));
        gimNegro.AddMessage(new BullyingMessage("Qué predecible.", 9f));
        gimnasio[PersonajeType.Negro] = gimNegro;

        var gimEmo = new CircularMessageList();
        gimEmo.AddMessage(new BullyingMessage("¿El gimnasio? Esto no es para ti.", 9f));
        gimEmo.AddMessage(new BullyingMessage("¿Vas a llorar si pierdes?", 10f));
        gimEmo.AddMessage(new BullyingMessage("Deberías estar escribiendo poemas.", 8f));
        gimnasio[PersonajeType.Emo] = gimEmo;

        var gimOtaku = new CircularMessageList();
        gimOtaku.AddMessage(new BullyingMessage("¿Crees que eres un personaje de anime aquí?", 8f));
        gimOtaku.AddMessage(new BullyingMessage("Esto no es un torneo de manga.", 9f));
        gimOtaku.AddMessage(new BullyingMessage("¿Entrenando para ser un héroe de anime?", 7f));
        gimnasio[PersonajeType.Otaku] = gimOtaku;

        var gimFurro = new CircularMessageList();
        gimFurro.AddMessage(new BullyingMessage("¿Vas a correr en cuatro patas?", 10f));
        gimFurro.AddMessage(new BullyingMessage("Las mascotas no entran al gimnasio.", 9f));
        gimFurro.AddMessage(new BullyingMessage("¿Juegas o ladras?", 8f));
        gimnasio[PersonajeType.Furro] = gimFurro;

        var gimNerd = new CircularMessageList();
        gimNerd.AddMessage(new BullyingMessage("¿Trajiste un libro al gimnasio?", 7f));
        gimNerd.AddMessage(new BullyingMessage("Esto requiere músculos, no cerebro.", 8f));
        gimNerd.AddMessage(new BullyingMessage("Primera vez que te veo aquí.", 6f));
        gimnasio[PersonajeType.Nerd] = gimNerd;

        mensajesEspecificos[ZonaTipo.Gimnasio] = gimnasio;

        var gimNeutros = new CircularMessageList();
        gimNeutros.AddMessage(new BullyingMessage("Alguien dejó sudor en las pesas.", 0f));
        gimNeutros.AddMessage(new BullyingMessage("El partido de hoy va a estar bueno.", 0f));
        gimNeutros.AddMessage(new BullyingMessage("¿Alguien vio mi botella de agua?", 0f));
        mensajesNeutros[ZonaTipo.Gimnasio] = gimNeutros;

        // ========== PATIO ==========
        var patio = new Dictionary<PersonajeType, CircularMessageList>();

        var patioNegro = new CircularMessageList();
        patioNegro.AddMessage(new BullyingMessage("¿A quién esperas aquí?", 8f));
        patioNegro.AddMessage(new BullyingMessage("Nadie te llamó.", 9f));
        patioNegro.AddMessage(new BullyingMessage("Este grupo no es el tuyo.", 10f));
        patio[PersonajeType.Negro] = patioNegro;

        var patioEmo = new CircularMessageList();
        patioEmo.AddMessage(new BullyingMessage("¿Saliste a ver el sol? Qué raro.", 8f));
        patioEmo.AddMessage(new BullyingMessage("¿No te quema la luz?", 7f));
        patioEmo.AddMessage(new BullyingMessage("Pensé que evitabas los espacios sociales.", 9f));
        patio[PersonajeType.Emo] = patioEmo;

        var patioOtaku = new CircularMessageList();
        patioOtaku.AddMessage(new BullyingMessage("¿Recreando una escena de anime aquí?", 7f));
        patioOtaku.AddMessage(new BullyingMessage("Sal del mundo ficticio por un momento.", 8f));
        patioOtaku.AddMessage(new BullyingMessage("¿Buscando tu grupo de nakamas?", 9f));
        patio[PersonajeType.Otaku] = patioOtaku;

        var patioFurro = new CircularMessageList();
        patioFurro.AddMessage(new BullyingMessage("¿Saliste a correr como perrito?", 9f));
        patioFurro.AddMessage(new BullyingMessage("Cuidado con los árboles.", 8f));
        patioFurro.AddMessage(new BullyingMessage("¿Marcando territorio?", 10f));
        patio[PersonajeType.Furro] = patioFurro;

        var patioNerd = new CircularMessageList();
        patioNerd.AddMessage(new BullyingMessage("¿El nerd salió al recreo?", 7f));
        patioNerd.AddMessage(new BullyingMessage("¿No tienes tarea que hacer?", 6f));
        patioNerd.AddMessage(new BullyingMessage("Esto no es una clase.", 7f));
        patio[PersonajeType.Nerd] = patioNerd;

        mensajesEspecificos[ZonaTipo.Patio] = patio;

        var patioNeutros = new CircularMessageList();
        patioNeutros.AddMessage(new BullyingMessage("Hace un calor horrible hoy.", 0f));
        patioNeutros.AddMessage(new BullyingMessage("¿Alguien tiene una pelota?", 0f));
        patioNeutros.AddMessage(new BullyingMessage("El timbre ya casi suena.", 0f));
        mensajesNeutros[ZonaTipo.Patio] = patioNeutros;

        // ========== SALON ==========
        var salon = new Dictionary<PersonajeType, CircularMessageList>();

        var salNegro = new CircularMessageList();
        salNegro.AddMessage(new BullyingMessage("¿Entendiste algo de la clase?", 9f));
        salNegro.AddMessage(new BullyingMessage("Ese puesto no es tuyo.", 10f));
        salNegro.AddMessage(new BullyingMessage("No copies mi tarea.", 8f));
        salon[PersonajeType.Negro] = salNegro;

        var salEmo = new CircularMessageList();
        salEmo.AddMessage(new BullyingMessage("¿Vas a llorar en clase otra vez?", 9f));
        salEmo.AddMessage(new BullyingMessage("Siéntate atrás donde no molestes.", 8f));
        salEmo.AddMessage(new BullyingMessage("Ni el profe te pela.", 7f));
        salon[PersonajeType.Emo] = salEmo;

        var salOtaku = new CircularMessageList();
        salOtaku.AddMessage(new BullyingMessage("¿Dibujando anime en clase?", 8f));
        salOtaku.AddMessage(new BullyingMessage("Esto no es un club de manga.", 7f));
        salOtaku.AddMessage(new BullyingMessage("Deja de ver subtítulos en clase.", 9f));
        salon[PersonajeType.Otaku] = salOtaku;

        var salFurro = new CircularMessageList();
        salFurro.AddMessage(new BullyingMessage("¿Escribes con patas?", 9f));
        salFurro.AddMessage(new BullyingMessage("El profe no acepta ladridos como respuesta.", 10f));
        salFurro.AddMessage(new BullyingMessage("Siéntate como persona.", 8f));
        salon[PersonajeType.Furro] = salFurro;

        var salNerd = new CircularMessageList();
        salNerd.AddMessage(new BullyingMessage("Ya sé que sabes la respuesta, cállate.", 7f));
        salNerd.AddMessage(new BullyingMessage("Deja de levantar la mano.", 6f));
        salNerd.AddMessage(new BullyingMessage("¿Le vas a sacar 10 otra vez? Qué asco.", 8f));
        salon[PersonajeType.Nerd] = salNerd;

        mensajesEspecificos[ZonaTipo.Salon] = salon;

        var salNeutros = new CircularMessageList();
        salNeutros.AddMessage(new BullyingMessage("El profe llegó tarde otra vez.", 0f));
        salNeutros.AddMessage(new BullyingMessage("¿Alguien hizo la tarea?", 0f));
        salNeutros.AddMessage(new BullyingMessage("Este tablero está sucio.", 0f));
        mensajesNeutros[ZonaTipo.Salon] = salNeutros;
    }
}
