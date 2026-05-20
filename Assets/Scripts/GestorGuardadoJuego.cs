using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class DatosGuardadoJuego
{
    public int diaActual = 1;
    public int ultimoDia;
    public int dineroTotal;
    public int diasTrabajados;
    public int ultimoSueldo;
    public int errores;
    public int correctos;
    public int correosClasificados;
    public bool tieneCafe;
    public bool tieneSeguro;
    public bool seguroUsado;
    public bool tieneFiltroSpam;
    public bool tieneTeclado;
    public bool lecturaFacilActiva;
    public bool accesibilidadTextoGrande;
    public bool accesibilidadAltoContraste;
    public int accesibilidadTipoDaltonismo;
    public bool accesibilidadReducirEfectos;
    public string ultimaPartidaIso;
    public string dificultadEntryFilter = "Normal";
    public int bienestarEstudiantil = 70;
    public int confianzaEscolar = 70;
    public int precision;
    public List<RegistroCasoGuardado> casos = new List<RegistroCasoGuardado>();
}

[Serializable]
public class ResumenSlotGuardadoJuego
{
    public int id;
    public bool tieneDatos;
    public int diaActual;
    public int dineroTotal;
    public string ultimaPartidaIso;
}

[Serializable]
public class IndiceSlotsGuardadoJuego
{
    public int slotActivo = 1;
    public List<ResumenSlotGuardadoJuego> slots = new List<ResumenSlotGuardadoJuego>();
}

public class GestorGuardadoJuego
{
    private const int CantidadSlots = 3;

    private readonly string rutaArchivoLegacy;
    private readonly string rutaDirectorio;
    private readonly string rutaIndice;
    private readonly Dictionary<int, ResumenSlotGuardadoJuego> slotsPorId =
        new Dictionary<int, ResumenSlotGuardadoJuego>();

    private IndiceSlotsGuardadoJuego indice;

    public GestorGuardadoJuego(string rutaArchivoLegacy)
    {
        this.rutaArchivoLegacy = rutaArchivoLegacy;
        rutaDirectorio = Path.GetDirectoryName(rutaArchivoLegacy);
        rutaIndice = Path.Combine(rutaDirectorio, "partidas_indice.json");

        CargarIndice();
        MigrarGuardadoLegacySiHaceFalta();
    }

    public string RutaArchivo => ObtenerRutaSlot(SlotActivo);
    public string RutaIndice => rutaIndice;
    public int SlotActivo => indice.slotActivo;
    public IReadOnlyList<ResumenSlotGuardadoJuego> Slots => indice.slots;

    public string ObtenerRutaSlotPublica(int slot)
    {
        return EsSlotValido(slot) ? ObtenerRutaSlot(slot) : string.Empty;
    }

    public string ObtenerNombreArchivoSlot(int slot)
    {
        string ruta = ObtenerRutaSlotPublica(slot);
        return string.IsNullOrEmpty(ruta) ? string.Empty : Path.GetFileName(ruta);
    }

    public bool ExisteGuardado()
    {
        return File.Exists(RutaArchivo);
    }

    public bool ExisteGuardado(int slot)
    {
        return EsSlotValido(slot) && File.Exists(ObtenerRutaSlot(slot));
    }

    public ResumenSlotGuardadoJuego ObtenerResumenSlotPublico(int slot)
    {
        return ObtenerResumenSlot(slot);
    }

    public bool SeleccionarSlot(int slot)
    {
        if (!EsSlotValido(slot))
        {
            return false;
        }

        indice.slotActivo = slot;
        GuardarIndice();
        return true;
    }

    public DatosGuardadoJuego CargarProgreso()
    {
        if (!ExisteGuardado())
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(RutaArchivo);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            DatosGuardadoJuego datos = JsonUtility.FromJson<DatosGuardadoJuego>(json);
            Debug.Log("Progreso cargado desde: " + RutaArchivo);
            return datos;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("No se pudo cargar el progreso: " + ex.Message);
            return null;
        }
    }

    public void GuardarProgreso(DatosGuardadoJuego datos)
    {
        if (datos == null)
        {
            return;
        }

        try
        {
            datos.ultimaPartidaIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            string json = JsonUtility.ToJson(datos, true);
            File.WriteAllText(RutaArchivo, json);
            ActualizarResumenSlot(datos);
            GuardarIndice();
            Debug.Log("Progreso guardado en: " + RutaArchivo);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("No se pudo guardar el progreso: " + ex.Message);
        }
    }

    public void GuardarAlFinalDelDia(DatosGuardadoJuego datos)
    {
        GuardarProgreso(datos);
    }

    public void ReiniciarProgreso()
    {
        BorrarSlot(SlotActivo);
    }

    public bool BorrarSlot(int slot)
    {
        if (!EsSlotValido(slot))
        {
            return false;
        }

        string ruta = ObtenerRutaSlot(slot);

        if (File.Exists(ruta))
        {
            File.Delete(ruta);
        }

        ResumenSlotGuardadoJuego resumen = ObtenerResumenSlot(slot);

        if (resumen != null)
        {
            resumen.tieneDatos = false;
            resumen.diaActual = 0;
            resumen.dineroTotal = 0;
            resumen.ultimaPartidaIso = string.Empty;
            GuardarIndice();
        }

        return true;
    }

    private void CargarIndice()
    {
        if (File.Exists(rutaIndice))
        {
            try
            {
                string json = File.ReadAllText(rutaIndice);
                indice = JsonUtility.FromJson<IndiceSlotsGuardadoJuego>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("No se pudo cargar el indice de partidas: " + ex.Message);
            }
        }

        if (indice == null)
        {
            indice = new IndiceSlotsGuardadoJuego();
        }

        NormalizarIndice();
    }

    private void NormalizarIndice()
    {
        if (indice.slots == null)
        {
            indice.slots = new List<ResumenSlotGuardadoJuego>();
        }

        slotsPorId.Clear();

        for (int i = 0; i < indice.slots.Count; i++)
        {
            ResumenSlotGuardadoJuego slot = indice.slots[i];

            if (slot != null && EsSlotValido(slot.id))
            {
                slotsPorId[slot.id] = slot;
            }
        }

        for (int id = 1; id <= CantidadSlots; id++)
        {
            if (slotsPorId.ContainsKey(id))
            {
                continue;
            }

            ResumenSlotGuardadoJuego nuevo = new ResumenSlotGuardadoJuego { id = id };
            indice.slots.Add(nuevo);
            slotsPorId[id] = nuevo;
        }

        indice.slots.Sort((a, b) => a.id.CompareTo(b.id));

        if (!EsSlotValido(indice.slotActivo))
        {
            indice.slotActivo = 1;
        }
    }

    private void MigrarGuardadoLegacySiHaceFalta()
    {
        string rutaSlot1 = ObtenerRutaSlot(1);

        if (!File.Exists(rutaArchivoLegacy) || File.Exists(rutaSlot1))
        {
            return;
        }

        try
        {
            File.Copy(rutaArchivoLegacy, rutaSlot1);
            DatosGuardadoJuego datos = JsonUtility.FromJson<DatosGuardadoJuego>(
                File.ReadAllText(rutaSlot1)
            );

            if (datos != null)
            {
                ActualizarResumenSlot(datos, 1);
                indice.slotActivo = 1;
                GuardarIndice();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("No se pudo migrar el guardado anterior: " + ex.Message);
        }
    }

    private void ActualizarResumenSlot(DatosGuardadoJuego datos)
    {
        ActualizarResumenSlot(datos, SlotActivo);
    }

    private void ActualizarResumenSlot(DatosGuardadoJuego datos, int slot)
    {
        ResumenSlotGuardadoJuego resumen = ObtenerResumenSlot(slot);

        if (resumen == null || datos == null)
        {
            return;
        }

        resumen.tieneDatos = true;
        resumen.diaActual = Mathf.Max(datos.diaActual, datos.ultimoDia);
        resumen.dineroTotal = datos.dineroTotal;
        resumen.ultimaPartidaIso = datos.ultimaPartidaIso;
    }

    private ResumenSlotGuardadoJuego ObtenerResumenSlot(int slot)
    {
        slotsPorId.TryGetValue(slot, out ResumenSlotGuardadoJuego resumen);
        return resumen;
    }

    private string ObtenerRutaSlot(int slot)
    {
        return Path.Combine(rutaDirectorio, "partida_slot_" + slot + ".json");
    }

    private bool EsSlotValido(int slot)
    {
        return slot >= 1 && slot <= CantidadSlots;
    }

    private void GuardarIndice()
    {
        try
        {
            string json = JsonUtility.ToJson(indice, true);
            File.WriteAllText(rutaIndice, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("No se pudo guardar el indice de partidas: " + ex.Message);
        }
    }
}
