// SPDX-License-Identifier: MIT
// Copyright (c) 2026 vsDizzy

using System;
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
            Logger.LogInfo("Longer Days Normal Nights plugin initialized!");
        }

        private void OnDestroy()
        {
            LongerDaysNormalNights.Unload();
        }
    }

    public static class LongerDaysNormalNights
    {
        private static Harmony harmony;

        public static void Main()
        {
            if (harmony != null) 
            { 
                return; 
            }
            
            harmony = new Harmony("com.vortex.valheim.longerdaysnormalnights");
            harmony.PatchAll(typeof(LongerDaysNormalNights));
        }

        public static void Unload()
        {
            harmony?.UnpatchSelf();
            harmony = null;
        }

        [HarmonyPatch(typeof(ZNet), "Update")]
        [HarmonyPrefix]
        public static void PrefixUpdate(ZNet __instance, ref double ___m_netTime)
        {
            if (__instance == null || EnvMan.instance == null)
            {
                return;
            }

            float timeOfDay = EnvMan.instance.GetDayFraction();

            if (timeOfDay < 0.25f || timeOfDay > 0.75f)
            {
                return;
            }

            ___m_netTime -= (double)(Time.deltaTime * (2f / 3f));
        }
    }
}