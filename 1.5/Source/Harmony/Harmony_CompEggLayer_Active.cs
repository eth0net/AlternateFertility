﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_CompEggLayer_Active patches CompEggLayer to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(CompEggLayer), "get_Active")]
static class Harmony_CompEggLayer_Active
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var index = codes.FindIndex(PatcherUtility.LoadsPawnGender);

        codes[index] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanGetPregnant);
        codes[index + 1] = new CodeInstruction(OpCodes.Nop);
        codes[index + 2].opcode = OpCodes.Brtrue_S;

        return codes.AsEnumerable();
    }
}