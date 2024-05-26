using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_JobDriver_Lovin_MakeNewToils patches JobDriver_Lovin to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(JobDriver_Lovin), "<MakeNewToils>b__12_4")]
static class Harmony_JobDriver_Lovin_MakeNewToils
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var firstStartIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);
        var firstEndIndex = codes.FindIndex(firstStartIndex, code => code.opcode == OpCodes.Ldarg_0);
        var secondStartIndex = codes.FindIndex(firstEndIndex, PatcherUtility.LoadsPawnGender);
        var secondEndIndex = codes.FindIndex(secondStartIndex, code => code.opcode == OpCodes.Stloc_3) + 1;

        codes.RemoveRange(secondStartIndex, secondEndIndex - secondStartIndex);
        codes.InsertRange(secondStartIndex, [
            new CodeInstruction(OpCodes.Ldloca_S, 2),
            new CodeInstruction(OpCodes.Ldloca_S, 3),
            new CodeInstruction(OpCodes.Call, PatcherUtility.m_GetImpregnationPair),
        ]);
        codes.RemoveRange(firstStartIndex, firstEndIndex - firstStartIndex);

        return codes.AsEnumerable();
    }
}
