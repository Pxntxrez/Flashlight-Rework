using HarmonyLib;

[HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Hidden))]
static class Patch_FlashlightController_Hidden
{
    static bool Prefix(FlashlightController __instance)
        => !__instance.PlayerAvatar.isLocal;
}
