using UnityEngine;

public static class ConfiguracionAccesibilidadJuego
{
    private const string ClaveTextoGrande = "accesibilidad_texto_grande";
    private const string ClaveAltoContraste = "accesibilidad_alto_contraste";
    private const string ClaveTipoDaltonismo = "accesibilidad_tipo_daltonismo";

    public static bool TextoGrandeActivo =>
        PlayerPrefs.GetInt(ClaveTextoGrande, 0) == 1;

    public static bool AltoContrasteActivo =>
        PlayerPrefs.GetInt(ClaveAltoContraste, 0) == 1;

    public static bool ReducirEfectosActivo => false;

    public static TipoDaltonismo TipoDaltonismoActual =>
        (TipoDaltonismo)PlayerPrefs.GetInt(
            ClaveTipoDaltonismo,
            (int)TipoDaltonismo.Ninguno
        );

    public static void Guardar(
        bool textoGrande,
        bool altoContraste,
        TipoDaltonismo tipoDaltonismo,
        bool reducirEfectos
    )
    {
        PlayerPrefs.SetInt(ClaveTextoGrande, textoGrande ? 1 : 0);
        PlayerPrefs.SetInt(ClaveAltoContraste, altoContraste ? 1 : 0);
        PlayerPrefs.SetInt(ClaveTipoDaltonismo, (int)tipoDaltonismo);
        PlayerPrefs.Save();
        AplicadorAccesibilidadGlobal.AplicarEscenaActual();
    }
}
