using HarmonyLib;

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
