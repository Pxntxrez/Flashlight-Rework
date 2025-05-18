using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PxntxrezStudio.RepoFlashlightRework;
using UnityEngine;

[HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Update))]
static class Patch_FlashlightController_Update
{
    static readonly FieldInfo fiCurrentState = AccessTools.Field(
        typeof(FlashlightController),
        "currentState"
    );
    static readonly FieldInfo fiStateTimer = AccessTools.Field(
        typeof(FlashlightController),
        "stateTimer"
    );
    static readonly FieldInfo fiIntroRotLerp = AccessTools.Field(
        typeof(FlashlightController),
        "introRotLerp"
    );
    static readonly FieldInfo fiIntroYLerp = AccessTools.Field(
        typeof(FlashlightController),
        "introYLerp"
    );
    static readonly FieldInfo fiClick = AccessTools.Field(typeof(FlashlightController), "click");

    static readonly Dictionary<FlashlightController, float> lastToggle = new();
    const float toggleCooldown = 1.3f;

    static bool Prefix(FlashlightController __instance)
    {
        if (!__instance.PlayerAvatar.isLocal)
            return true;

        bool canToggle =
            !lastToggle.TryGetValue(__instance, out float lastTime)
            || Time.time - lastTime >= toggleCooldown;

        if (Input.GetKeyDown(FlashlightConfig.GetKeyCode()) && canToggle)
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
                fiCurrentState.SetValue(
                    __instance,
                    Enum.Parse(fiCurrentState.FieldType, "LightOff")
                );
                fiStateTimer.SetValue(__instance, 0.25f);
                fiClick.SetValue(__instance, true);
                __instance.lightOffAudio.Play(__instance.transform.position, 1f, 1f, 1f, 1f);
            }
        }

        return true;
    }

    static void Postfix(FlashlightController __instance)
    {
        if (!__instance.PlayerAvatar.isLocal)
            return;

        AccessTools
            .Field(typeof(FlashlightController), "active")
            .SetValue(__instance, __instance.LightActive);

        if (!__instance.LightActive)
            return;

        if (FlashlightConfig.UseCustomRGB.Value)
        {
            Color color = FlashlightConfig.GetColor();
            __instance.spotlight.color = color;

            Light haloLight = __instance.halo.GetComponent<Light>();
            if (haloLight != null)
                haloLight.color = color;
        }
        else if (
            Patch_FlashlightController_Start.originalColors.TryGetValue(__instance, out Color orig)
        )
        {
            __instance.spotlight.color = orig;

            Light haloLight = __instance.halo.GetComponent<Light>();
            if (haloLight != null)
                haloLight.color = orig;
        }
    }
}
