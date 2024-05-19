using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_ChildRelationUtility_ChanceOfBecomingChildOf patches ChildRelationUtility to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(ChildRelationUtility), nameof(ChildRelationUtility.ChanceOfBecomingChildOf))]
static class Harmony_ChildRelationUtility_ChanceOfBecomingChildOf
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var maleGenderIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);
        var maleReturnIndex = codes.FindIndex(maleGenderIndex, code => code.opcode == OpCodes.Ret);
        var femaleGenderIndex = codes.FindIndex(maleReturnIndex, PatcherUtility.LoadsPawnGender);

        codes[maleGenderIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanImpregnate);
        codes[maleGenderIndex + 1] = new CodeInstruction(OpCodes.Nop);
        codes[maleGenderIndex + 2].opcode = OpCodes.Brfalse_S;

        codes[femaleGenderIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanGetPregnant);
        codes[femaleGenderIndex + 1] = new CodeInstruction(OpCodes.Nop);
        codes[femaleGenderIndex + 2].opcode = OpCodes.Brfalse_S;

        return codes.AsEnumerable();
    }
}
