using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_PawnUtility_FertileMateTarget patches PawnUtility to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(PawnUtility), nameof(PawnUtility.FertileMateTarget))]
static class Harmony_PawnUtility_FertileMateTarget
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var startIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);

        codes[startIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanGetPregnant);
        codes[startIndex + 1] = new CodeInstruction(OpCodes.Nop);
        codes[startIndex + 2].opcode = OpCodes.Brfalse_S;

        return codes.AsEnumerable();
    }
}
