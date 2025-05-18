using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using PxntxrezStudio.RepoFlashlightRework;
using UnityEngine;

public static class Patch_FlashlightController_Start
{
    public static readonly Dictionary<FlashlightController, Color> originalColors = new();
    private static readonly Dictionary<FlashlightController, Coroutine> activeFlickers = new();

    [HarmonyPatch(typeof(FlashlightController), nameof(FlashlightController.Start))]
    public static class Patch
    {
        public static void Postfix(FlashlightController __instance)
        {
            if (__instance.PlayerAvatar.isLocal)
            {
                if (!originalColors.ContainsKey(__instance))
                    originalColors[__instance] = __instance.spotlight.color;

                TryStartFlicker(__instance);
            }
        }
    }

    public static void TryStartFlicker(FlashlightController flashlight)
    {
        if (!FlashlightConfig.EnableFlicker.Value)
        {
            StopFlicker(flashlight);
            return;
        }

        if (activeFlickers.ContainsKey(flashlight))
            return;

        Coroutine c = flashlight.StartCoroutine(FlickerCoroutine(flashlight));
        activeFlickers[flashlight] = c;
    }

    public static void StopFlicker(FlashlightController flashlight)
    {
        if (activeFlickers.TryGetValue(flashlight, out Coroutine coroutine))
        {
            flashlight.StopCoroutine(coroutine);
            activeFlickers.Remove(flashlight);
        }

        if (!flashlight.LightActive)
        {
            flashlight.spotlight.enabled = false;
            flashlight.halo.enabled = false;
        }
        else
        {
            flashlight.spotlight.enabled = true;
            flashlight.halo.enabled = true;
        }
    }

    public static IEnumerator FlickerCoroutine(FlashlightController flashlight)
    {
        const float detectionRadius = 15f;

        while (true)
        {
            if (!FlashlightConfig.EnableFlicker.Value || !flashlight.LightActive)
            {
                flashlight.spotlight.enabled = flashlight.LightActive;
                flashlight.halo.enabled = flashlight.LightActive;
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            Collider[] colliders = Physics.OverlapSphere(
                flashlight.transform.position,
                detectionRadius
            );
            float closest = detectionRadius;
            bool enemyFound = false;

            foreach (var col in colliders)
            {
                var ec = col.GetComponentInParent<EnemyChecklist>();
                if (ec != null)
                {
                    float dist = Vector3.Distance(
                        flashlight.transform.position,
                        col.transform.position
                    );
                    if (dist < closest)
                    {
                        closest = dist;
                        enemyFound = true;
                    }
                }
            }

            if (!enemyFound)
            {
                if (!flashlight.spotlight.enabled)
                {
                    flashlight.spotlight.enabled = true;
                    flashlight.halo.enabled = true;
                }
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            float panicLevel = Mathf.Clamp01((detectionRadius - closest) / detectionRadius);

            if (panicLevel < 0.3f)
            {
                if (Random.value < panicLevel)
                {
                    flashlight.spotlight.enabled = false;
                    flashlight.halo.enabled = false;
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
                    flashlight.spotlight.enabled = true;
                    flashlight.halo.enabled = true;
                }

                yield return new WaitForSeconds(Random.Range(0.15f, 0.25f));
            }
            else if (panicLevel < 0.7f)
            {
                int flicks = Random.Range(3, 6);
                for (int i = 0; i < flicks; i++)
                {
                    flashlight.spotlight.enabled = false;
                    flashlight.halo.enabled = false;
                    yield return new WaitForSeconds(Random.Range(0.03f, 0.07f));
                    flashlight.spotlight.enabled = true;
                    flashlight.halo.enabled = true;
                    yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
                }

                if (Random.value < 0.3f)
                {
                    flashlight.spotlight.enabled = false;
                    flashlight.halo.enabled = false;
                    yield return new WaitForSeconds(Random.Range(0.8f, 1.4f));
                    flashlight.spotlight.enabled = true;
                    flashlight.halo.enabled = true;
                }

                yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
            }
            else
            {
                if (Random.value < 0.6f)
                {
                    flashlight.spotlight.enabled = false;
                    flashlight.halo.enabled = false;
                    yield return new WaitForSeconds(Random.Range(1.5f, 3.0f));
                    flashlight.spotlight.enabled = true;
                    flashlight.halo.enabled = true;
                }

                float totalTime = Random.Range(1.2f, 2.5f);
                float timer = 0f;

                while (timer < totalTime)
                {
                    bool on = Random.value > 0.2f;
                    flashlight.spotlight.enabled = on;
                    flashlight.halo.enabled = on;

                    float wait = Random.Range(0.015f, 0.07f);
                    yield return new WaitForSeconds(wait);
                    timer += wait;
                }

                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
    }

    public static void OnFlickerSettingChanged()
    {
        foreach (var flashlight in Object.FindObjectsOfType<FlashlightController>())
        {
            if (!flashlight.PlayerAvatar.isLocal)
                continue;

            if (FlashlightConfig.EnableFlicker.Value)
                TryStartFlicker(flashlight);
            else
                StopFlicker(flashlight);
        }
    }
}
