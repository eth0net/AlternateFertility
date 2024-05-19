using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_LovePartnerRelationUtility_TryToShareChildrenForGeneratedLovePartner patches LovePartnerRelationUtility to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.TryToShareChildrenForGeneratedLovePartner))]
static class Harmony_LovePartnerRelationUtility_TryToShareChildrenForGeneratedLovePartner
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        var firstIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);
        var secondIndex = codes.FindIndex(firstIndex + 1, PatcherUtility.LoadsPawnGender);

        codes[firstIndex] = new CodeInstruction(OpCodes.Nop);
        codes[secondIndex] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_GetImpregnationPossible);
        codes[secondIndex + 1].opcode = OpCodes.Brtrue_S;

        var thirdIndex = codes.FindIndex(secondIndex + 1, code => code.opcode == OpCodes.Ldc_R4);
        var fourthIndex = codes.FindIndex(thirdIndex + 1, code => code.opcode == OpCodes.Ldloc_1);

        codes.RemoveRange(thirdIndex, fourthIndex - thirdIndex);
        codes.InsertRange(thirdIndex, [
            new CodeInstruction(OpCodes.Ldloc_2),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Ldarg_2),
            new CodeInstruction(OpCodes.Ldarg_3),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Harmony_LovePartnerRelationUtility_TryToShareChildrenForGeneratedLovePartner), nameof(DoParentThing))),
        ]);

        return codes.AsEnumerable();
    }

    static void DoParentThing(Pawn child, Pawn generated, Pawn other, PawnGenerationRequest request, float extraChanceFactor)
    {
        var chance = 1f;
        PatcherUtility.GetImpregnationPair(generated, other, out Pawn impregnator, out Pawn impregnatee);
        if (impregnator == generated)
        {
            chance = ChildRelationUtility.ChanceOfBecomingChildOf(child, generated, other, null, request, null);
        } else if (impregnatee == generated)
        {
            chance = ChildRelationUtility.ChanceOfBecomingChildOf(child, other, generated, null, null, request);
        }
        chance *= extraChanceFactor;
        if (Rand.Value < chance)
        {
            if (impregnator == generated)
            {
                child.SetFather(generated);
            } else if (impregnatee == generated)
            {
                child.SetMother(generated);
            }
        }
    }
}
