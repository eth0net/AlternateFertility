using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_PawnGenerator_GenerateInitialHediffs patches PawnGenerator to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(PawnGenerator), "GenerateInitialHediffs")]
static class Harmony_PawnGenerator_GenerateInitialHediffs
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var firstIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);
        var secondIndex = codes.FindIndex(firstIndex + 1, PatcherUtility.LoadsPawnGender);

        codes[firstIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanGetPregnant);
        codes[secondIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanImpregnate);

        return codes.AsEnumerable();
    }
}
