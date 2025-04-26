using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[BepInPlugin("PxntxrezStudio.RepoFlashlightRework", "REPO Flashlight Rework", "1.1.0")]
public class FlashlightReworkPlugin : BaseUnityPlugin
{
    private Harmony _harmony = null!;

    void Awake()
    {
        _harmony = new Harmony("PxntxrezStudio.RepoFlashlightRework");
        _harmony.PatchAll();
    }

    void OnDestroy()
        => _harmony.UnpatchSelf();
}

[HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Update))]
static class Patch_FlashlightController_Update
{
    static readonly FieldInfo fiCurrentState = AccessTools.Field(typeof(FlashlightController), "currentState");
    static readonly FieldInfo fiStateTimer    = AccessTools.Field(typeof(FlashlightController), "stateTimer");
    static readonly FieldInfo fiIntroRotLerp  = AccessTools.Field(typeof(FlashlightController), "introRotLerp");
    static readonly FieldInfo fiIntroYLerp    = AccessTools.Field(typeof(FlashlightController), "introYLerp");
    static readonly FieldInfo fiClick         = AccessTools.Field(typeof(FlashlightController), "click");

    static readonly Dictionary<FlashlightController, float> lastToggle = new();
    const float toggleCooldown = 1.1f;

    static bool Prefix(FlashlightController __instance)
    {
        if (!__instance.PlayerAvatar.isLocal)
            return true;

        float last;
        bool canToggle = !lastToggle.TryGetValue(__instance, out last)
                         || Time.time - last >= toggleCooldown;

        if (Input.GetKeyDown(KeyCode.F) && canToggle)
        {
            lastToggle[__instance] = Time.time;

            if (!__instance.LightActive)
            {
                fiCurrentState.SetValue(__instance, Enum.Parse(fiCurrentState.FieldType, "Intro"));
                fiStateTimer.SetValue(__instance, 1f);
                fiIntroRotLerp.SetValue(__instance, 0f);
                fiIntroYLerp.SetValue(__instance, 0f);
                fiClick.SetValue(__instance, true);
            }
            else
            {
                fiCurrentState.SetValue(__instance, Enum.Parse(fiCurrentState.FieldType, "LightOff"));
                fiStateTimer.SetValue(__instance, 0.25f);
                fiClick.SetValue(__instance, true);
                __instance.lightOffAudio.Play(__instance.transform.position, 1f, 1f, 1f, 1f);
            }
        }

        return true;
    }

    static void Postfix(FlashlightController __instance)
    {
        if (__instance.PlayerAvatar.isLocal && __instance.LightActive)
        {
            AccessTools.Field(typeof(FlashlightController), "active")
                       .SetValue(__instance, true);
        }
    }
}

[HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Hidden))]
static class Patch_FlashlightController_Hidden
{
    static bool Prefix(FlashlightController __instance)
        => !__instance.PlayerAvatar.isLocal;
}

[HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Idle))]
static class Patch_FlashlightController_Idle
{
    static bool Prefix(FlashlightController __instance)
    {
        if (__instance.PlayerAvatar.isLocal 
            && __instance.LightActive 
            && (__instance.PlayerAvatar.isCrouching
                || __instance.PlayerAvatar.isTumbling
                || __instance.PlayerAvatar.isSliding))
            return false;
        return true;
    }
}
