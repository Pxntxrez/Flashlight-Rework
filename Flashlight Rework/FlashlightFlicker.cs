using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Linq;

public static class FlashlightFlicker
{
    [HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Start))]
    public static class Patch_FlashlightController_Start
    {
        public static void Postfix(FlashlightController __instance)
        {
            if (__instance.PlayerAvatar.isLocal)
                __instance.StartCoroutine(FlickerCoroutine(__instance));
        }

        private static IEnumerator FlickerCoroutine(FlashlightController flashlight)
        {
            const float scanInterval = 0.2f;
            const float detectionRadius = 12f;

            while (true)
            {
                if (!flashlight.LightActive)
                {
                    yield return new WaitForSeconds(scanInterval);
                    continue;
                }

                Collider[] colliders = Physics.OverlapSphere(flashlight.transform.position, detectionRadius);
                float closest = detectionRadius;
                bool found = false;

                foreach (var col in colliders)
                {
                    var ec = col.GetComponentInParent<EnemyChecklist>();
                    if (ec != null)
                    {
                        float dist = Vector3.Distance(flashlight.transform.position, col.transform.position);
                        if (dist < closest)
                        {
                            closest = dist;
                            found = true;
                        }
                    }
                }

                if (found)
                {
                    // faster blink when enemy is closer
                    float blinkSpeed = Mathf.Lerp(0.5f, 0.1f, (detectionRadius - closest) / detectionRadius);
                    flashlight.spotlight.enabled = !flashlight.spotlight.enabled;
                    yield return new WaitForSeconds(blinkSpeed);
                }
                else
                {
                    if (!flashlight.spotlight.enabled)
                        flashlight.spotlight.enabled = true;
                    yield return new WaitForSeconds(scanInterval);
                }
            }
        }
    }
}
