﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_JobGiver_Mate_TryGiveJob patches JobGiver_Mate to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(JobGiver_Mate), "TryGiveJob")]
static class Harmony_JobGiver_Mate_TryGiveJob
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var index = codes.FindIndex(PatcherUtility.LoadsPawnGender);

        codes[index] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanImpregnate);
        codes[index + 1] = new CodeInstruction(OpCodes.Nop);
        codes[index + 2].opcode = OpCodes.Brfalse_S;

        return codes.AsEnumerable();
    }
}
