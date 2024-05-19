using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_PawnRelationWorker_Child_GenerationChance patches PawnRelationWorker_Child to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(PawnRelationWorker_Child), nameof(PawnRelationWorker_Child.GenerationChance))]
static class Harmony_PawnRelationWorker_Child_GenerationChance
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var firstIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);
        var secondIndex = codes.FindIndex(firstIndex + 1, PatcherUtility.LoadsPawnGender);

        codes[firstIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanImpregnate);
        codes[firstIndex + 1] = new CodeInstruction(OpCodes.Nop);
        codes[firstIndex + 2].opcode = OpCodes.Brfalse_S;

        codes[secondIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanGetPregnant);
        codes[secondIndex + 1] = new CodeInstruction(OpCodes.Nop);
        codes[secondIndex + 2].opcode = OpCodes.Brfalse_S;

        return codes.AsEnumerable();
    }
}
