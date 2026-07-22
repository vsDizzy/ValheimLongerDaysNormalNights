// SPDX-License-Identifier: MIT
// Copyright (c) 2026 vsDizzy

using HarmonyLib;
using UnityEngine;
using BepInEx;

namespace Valheim
{
    [BepInPlugin("com.vortex.valheim.longerdaysnormalnights", "Longer Days Normal Nights", "${VERSION}")]
    public class LongerDaysNormalNightsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            LongerDaysNormalNights.Main();
        }

        private void OnDestroy()
        {
            LongerDaysNormalNights.Unload();
        }
    }

    public static class LongerDaysNormalNights
    {
        private static Harmony harmony;

        // Precise phase durations in seconds for a flat 60-minute cycle
        public static float NightPart1Sec = 270f;  // 4.5 min (0.00 - 0.15)
        public static float DawnSec       = 180f;  // 3.0 min (0.15 - 0.25)
        public static float DaySec        = 2700f; // 45.0 min (0.25 - 0.75) -- EXTENDED DAY
        public static float DuskSec       = 180f;  // 3.0 min (0.75 - 0.85)
        public static float NightPart2Sec = 270f;  // 4.5 min (0.85 - 1.00)

        public static void Main()
        {
            if (harmony != null) return; 
            
            harmony = new Harmony("com.vortex.valheim.longerdaysnormalnights");
            harmony.PatchAll(typeof(LongerDaysNormalNights));
        }

        public static void Unload()
        {
            harmony?.UnpatchSelf();
            harmony = null;
        }

        [HarmonyPatch(typeof(EnvMan), "Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(EnvMan __instance)
        {
            float totalCycleTime = NightPart1Sec + DawnSec + DaySec + DuskSec + NightPart2Sec;
            __instance.m_dayLengthSec = (long)totalCycleTime;
        }

        [HarmonyPatch(typeof(EnvMan), "GetDayFraction")]
        [HarmonyPrefix]
        public static bool GetDayFractionPrefix(EnvMan __instance, ref float __result)
        {
            if (ZNet.instance == null) return true;

            float t1 = NightPart1Sec;
            float t2 = t1 + DawnSec;
            float t3 = t2 + DaySec;
            float t4 = t3 + DuskSec;
            float totalCycleTime = t4 + NightPart2Sec;
            
            double timeSeconds = ZNet.instance.GetTimeSeconds();
            float t_sec = (float)(timeSeconds % totalCycleTime);

            float v = 0f;

            if (t_sec <= t1)
            {
                v = Mathf.Lerp(0.0f, 0.15f, t_sec / t1);
            }
            else if (t_sec <= t2)
            {
                v = Mathf.Lerp(0.15f, 0.25f, (t_sec - t1) / DawnSec);
            }
            else if (t_sec <= t3)
            {
                v = Mathf.Lerp(0.25f, 0.75f, (t_sec - t2) / DaySec);
            }
            else if (t_sec <= t4)
            {
                v = Mathf.Lerp(0.75f, 0.85f, (t_sec - t3) / DuskSec);
            }
            else
            {
                v = Mathf.Lerp(0.85f, 1.0f, (t_sec - t4) / NightPart2Sec);
            }

            __result = v;
            return false; 
        }
        
        [HarmonyPatch(typeof(EnvMan), "Update")]
        [HarmonyPostfix]
        public static void UpdatePostfix(EnvMan __instance, ref float ___m_smoothDayFraction)
        {
            if (__instance == null || ZNet.instance == null) return;
            ___m_smoothDayFraction = __instance.GetDayFraction();
        }
    }
}