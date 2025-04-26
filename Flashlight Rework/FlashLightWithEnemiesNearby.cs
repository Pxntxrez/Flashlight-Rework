using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace FlashlightRework;
[HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Start))]
static class Patch_FlashlightController_Start
{
    static void Postfix(FlashlightController __instance)
    {
        if (__instance.PlayerAvatar.isLocal)
            __instance.StartCoroutine(FlickerCoroutine(__instance));
    }

    static IEnumerator FlickerCoroutine(FlashlightController flashlight)
    {
        float scanInterval = 0.2f;
        float detectionRadius = 15f;

        while (true)
        {
            Collider[] colliders = Physics.OverlapSphere(flashlight.transform.position, detectionRadius);
            float closestDistance = detectionRadius;
            bool enemyFound = false;

            foreach (var col in colliders)
            {
                // just try to find a component that all enemies (and only enemies have)
                if (col.GetComponentInParent<EnemyChecklist>() != null) 
                {
                    
                    float distance = Vector3.Distance(flashlight.transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        enemyFound = true;
                    }
                }
            }

            if (enemyFound)
            {
                float blinkSpeed = Mathf.Lerp(0.1f, 0.5f, closestDistance / detectionRadius);

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
