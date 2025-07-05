using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_Recipe_ExtractOvum_AvailableReport patches Recipe_ExtractOvum to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(Recipe_ExtractOvum), nameof(Recipe_ExtractOvum.AvailableReport))]
static class Harmony_Recipe_ExtractOvum_AvailableReport
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var startIndex = codes.FindIndex(code => code.opcode == OpCodes.Ldarg_0) + 1;
        var endIndex = codes.FindIndex(startIndex, code => code.opcode == OpCodes.Beq_S);

        codes[startIndex - 1].opcode = OpCodes.Nop;
        codes[endIndex].opcode = OpCodes.Brtrue_S;

        codes.RemoveRange(startIndex, endIndex - startIndex);
        codes.InsertRange(startIndex, [
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanGetPregnant),
        ]);

        return codes.AsEnumerable();
    }
}
