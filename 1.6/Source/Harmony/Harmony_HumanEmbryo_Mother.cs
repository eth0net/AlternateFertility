using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_HumanEmbryo_Mother patches HumanEmbryo to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(HumanEmbryo), "get_Mother")]
static class Harmony_HumanEmbryo_Mother
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Call, PatcherUtility.m_GetHumanEmbryoImpregnatee);
        yield return new CodeInstruction(OpCodes.Ret);
    }
}
