using BepInEx;
using HarmonyLib;
using UnityEngine;
using PxntxrezStudio.RepoFlashlightRework;

[BepInPlugin("PxntxrezStudio.RepoFlashlightRework", "REPO Flashlight Rework", "1.2.0")]
public class FlashlightReworkPlugin : BaseUnityPlugin
{
    private Harmony _harmony = null!;

    void Awake()
    {
        FlashlightConfig.Init(Config);
        _harmony = new Harmony("PxntxrezStudio.RepoFlashlightRework");
        _harmony.PatchAll();
        Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
    }
}
