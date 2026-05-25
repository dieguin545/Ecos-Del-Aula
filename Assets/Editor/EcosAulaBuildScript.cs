using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.IO.Compression;

public static class EcosAulaBuildScript
{
    [MenuItem("Ecos del Aula/Ejecutar Build y Empaquetar")]
    public static void BuildGame()
    {
        Debug.Log("[EcosAulaBuildScript] Iniciando proceso de build...");

        // Asegurar que el contenedor de sprites esté inicializado y guardado
        EcosAulaSpriteLoader.InicializarSiHaceFalta();

        // Obtener escenas desde EditorBuildSettings
        if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
        {
            Debug.LogError("[EcosAulaBuildScript] No hay escenas configuradas en EditorBuildSettings!");
            return;
        }

        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
            Debug.Log($"[EcosAulaBuildScript] Escena en build settings: {scenes[i]}");
        }

        // Obtener target activo
        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log($"[EcosAulaBuildScript] Target activo detectado: {activeTarget}");

        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string buildDir = Path.Combine(Application.dataPath, "../BuildTemp");

        if (Directory.Exists(buildDir))
        {
            try
            {
                Directory.Delete(buildDir, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EcosAulaBuildScript] No se pudo borrar el directorio temporal anterior: {ex.Message}");
            }
        }
        Directory.CreateDirectory(buildDir);

        if (activeTarget == BuildTarget.StandaloneWindows64 || activeTarget == BuildTarget.StandaloneWindows)
        {
            Debug.Log("[EcosAulaBuildScript] Generando build para Windows Standalone...");
            string exePath = Path.Combine(buildDir, "Ecos_Del_Aula.exe");
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = exePath;
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[EcosAulaBuildScript] Build exitosa! Comprimiendo en ZIP...");
                string zipPath = Path.Combine(desktopPath, "Ecos_Del_Aula_Final.zip");
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                ZipFile.CreateFromDirectory(buildDir, zipPath);
                Debug.Log($"[EcosAulaBuildScript] Archivo comprimido creado correctamente en: {zipPath}");
            }
            else
            {
                Debug.LogError($"[EcosAulaBuildScript] Error durante el build: {summary.result}");
            }
        }
        else if (activeTarget == BuildTarget.WebGL)
        {
            Debug.Log("[EcosAulaBuildScript] Configurando opciones WebGL (Gzip, NO Brotli)...");
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = buildDir;
            buildPlayerOptions.target = BuildTarget.WebGL;
            buildPlayerOptions.options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[EcosAulaBuildScript] Build WebGL exitosa! Comprimiendo en ZIP...");
                string zipPath = Path.Combine(desktopPath, "Ecos_Del_Aula_WebGL_Gzip.zip");
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                ZipFile.CreateFromDirectory(buildDir, zipPath);
                Debug.Log($"[EcosAulaBuildScript] Archivo comprimido creado correctamente en: {zipPath}");
            }
            else
            {
                Debug.LogError($"[EcosAulaBuildScript] Error durante el build: {summary.result}");
            }
        }
        else
        {
            Debug.LogWarning($"[EcosAulaBuildScript] El build target actual '{activeTarget}' no está soportado. Forzando compilación Windows Standalone...");
            string exePath = Path.Combine(buildDir, "Ecos_Del_Aula.exe");
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = exePath;
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                string zipPath = Path.Combine(desktopPath, "Ecos_Del_Aula_Final.zip");
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                ZipFile.CreateFromDirectory(buildDir, zipPath);
                Debug.Log($"[EcosAulaBuildScript] Archivo comprimido creado correctamente en: {zipPath}");
            }
            else
            {
                Debug.LogError($"[EcosAulaBuildScript] Error durante el build de fallback: {summary.result}");
            }
        }
    }

    [MenuItem("Ecos del Aula/Limpiar y Guardar Escenas")]
    public static void LimpiarYAplicarEscenas()
    {
        Debug.Log("[EcosAulaBuildScript] Iniciando limpieza de escenas...");
        
        // Obtener escenas desde EditorBuildSettings
        if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
        {
            Debug.LogError("[EcosAulaBuildScript] No hay escenas configuradas en EditorBuildSettings!");
            return;
        }

        foreach (var editorScene in EditorBuildSettings.scenes)
        {
            if (string.IsNullOrEmpty(editorScene.path)) continue;
            
            Debug.Log($"[EcosAulaBuildScript] Abriendo y procesando escena: {editorScene.path}");
            var scene = EditorSceneManager.OpenScene(editorScene.path, OpenSceneMode.Single);
            
            // Forzar el rediseño directo
            EcosAulaUIRediseno.AplicarDirecto(scene.name);
            
            // Marcar escena como sucia y guardarla
            EditorSceneManager.MarkSceneDirty(scene);
            bool guardado = EditorSceneManager.SaveScene(scene);
            Debug.Log($"[EcosAulaBuildScript] Escena {scene.name} guardada: {guardado}");
        }
        
        Debug.Log("[EcosAulaBuildScript] Limpieza de escenas completada.");
    }
}
