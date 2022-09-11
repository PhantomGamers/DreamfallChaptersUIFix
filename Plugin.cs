using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace DreamfallChaptersUIFix;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<int> ResolutionWidth;
    public static ConfigEntry<int> ResolutionHeight;
    public static ConfigEntry<bool> FullscreenMode;
    public static ConfigEntry<bool> LimitUI;
    public static ConfigEntry<bool> FixCutsceneFOV;
    public static ConfigEntry<bool> LimitSubtitles;

    public static int Offset;

    public static ManualLogSource log;

    private void Awake()
    {
        // Plugin startup logic
        log = Logger;
        log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        ResolutionWidth = Config.Bind(
            "General",
            "ResolutionWidth",
            Display.main.systemWidth);

        ResolutionHeight = Config.Bind(
            "General",
            "ResolutionHeight",
            Display.main.systemHeight);

        FullscreenMode = Config.Bind(
            "General",
            "FullScreenMode",
            Screen.fullScreen);

        FixCutsceneFOV = Config.Bind(
            "General",
            "FixCutsceneFOV",
            true,
            "Changes cutscenes to hor+ FOV");

        LimitUI = Config.Bind(
            "General",
            "LimitUI",
            true,
            "Limits problematic UI elements to 16:9");

        LimitSubtitles = Config.Bind(
            "General",
            "LimitSubtitles",
            true,
            "Limits subtitles to 16:9");

        Harmony.CreateAndPatchAll(typeof(Patches));
    }

    private void Start()
    {
        SetResolution();
    }

    public static void SetResolution()
    {
        Screen.SetResolution(ResolutionWidth.Value, ResolutionHeight.Value, FullscreenMode.Value);
    }

    public static float GetAR()
    {
        return (float)Screen.width / Screen.height;
    }

    public static float GetARDifference()
    {
        return GetAR() / (16f / 9f);
    }

    public static int GetWidthOffset()
    {
        return (Screen.width - GetLimitedUIWidth()) / 2;
    }

    public static int GetLimitedUIWidth()
    {
        return (int)(Screen.width / GetARDifference());
    }

    public static void LimitCameraResolution(Camera cam)
    {
        if (!LimitUI.Value)
        {
            return;
        }

        cam.pixelRect = new(GetWidthOffset(),
                            0,
                            GetLimitedUIWidth(),
                            cam.pixelRect.height);
        cam.aspect = 16f / 9f;
    }
}

[HarmonyPatch]
public class Patches
{
    [HarmonyPatch(typeof(CameraManager), nameof(CameraManager.CorrectFOV))]
    [HarmonyPatch(typeof(CinematicCamera), nameof(CinematicCamera.SetFOVFromVertical))]
    [HarmonyPrefix]
    public static bool PreventVertFov()
    {
        return !Plugin.FixCutsceneFOV.Value;
    }

    [HarmonyPatch(typeof(CinematicCamera), nameof(CinematicCamera.Update))]
    [HarmonyPrefix]
    public static bool HorPlusCutscenes(CinematicCamera __instance)
    {
        if (!Plugin.FixCutsceneFOV.Value)
        {
            return true;
        }

        if (__instance._target != __instance.Target)
        {
            __instance._target = __instance.Target;
            Component component = __instance.GetComponent("DepthOfFieldScatter");
            if (component != null)
            {
                FieldInfo field = component.GetType().GetField("focalTransform");
                field.SetValue(component, __instance.Target.GetLookTarget());
            }
        }
        return false;
    }

    [HarmonyPatch(typeof(InventoryGUI), nameof(InventoryGUI.Awake))]
    [HarmonyPostfix]
    public static void LimitInventoryGUI(InventoryGUI __instance)
    {
        Plugin.LimitCameraResolution(__instance.UICamera.cachedCamera);
    }

    [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.Awake))]
    [HarmonyPostfix]
    public static void LimitTitleMenu(TitleMenu __instance)
    {
        Plugin.LimitCameraResolution(__instance.UICamera.cachedCamera);
    }

    [HarmonyPatch(typeof(UICamera), nameof(UICamera.Awake))]
    [HarmonyPostfix]
    public static void LimitSummaryScreen(UICamera __instance)
    {
        if (Plugin.LimitUI.Value
            && SceneManager.GetActiveScene().name.StartsWith("Summary")
            && Object.FindObjectOfType<BookSummary>() != null)
        {
            Plugin.LimitCameraResolution(__instance.cachedCamera);
        }
    }

    [HarmonyPatch(typeof(UIWidget), nameof(UIWidget.width), MethodType.Setter)]
    [HarmonyPostfix]
    public static void LimitSubtitles(UIWidget __instance)
    {
        if (Plugin.LimitSubtitles.Value && __instance.name == "DialogueLabel")
        {
            __instance.mWidth = (int)(Plugin.GetLimitedUIWidth() * .75f);
        }
    }
}
