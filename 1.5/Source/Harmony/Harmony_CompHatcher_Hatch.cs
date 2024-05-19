using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_CompHatcher_Hatch patches CompHatcher to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(CompHatcher), nameof(CompHatcher.Hatch))]
static class Harmony_CompHatcher_Hatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var firstIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);
        var secondIndex = codes.FindIndex(firstIndex + 1, PatcherUtility.LoadsPawnGender);

        codes[firstIndex] = new CodeInstruction(OpCodes.Nop);
        codes[secondIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_GetImpregnationPossible);
        codes[secondIndex + 1].opcode = OpCodes.Brfalse_S;

        return codes.AsEnumerable();
    }
}
