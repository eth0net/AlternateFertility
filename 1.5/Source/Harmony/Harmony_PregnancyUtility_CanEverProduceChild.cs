using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_PregnancyUtility_CanEverProduceChild patches PregnancyUtility to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.CanEverProduceChild))]
static class Harmony_PregnancyUtility_CanEverProduceChild
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codes = new List<CodeInstruction>(instructions);

        var startIndex = codes.FindIndex(PatcherUtility.LoadsPawnGender);
        var jumpIndex = codes.FindIndex(startIndex, code => code.opcode == OpCodes.Ldloc_0);

        var jumpLabel = generator.DefineLabel();
        codes[jumpIndex] = codes[jumpIndex].WithLabels(jumpLabel);

        codes.RemoveRange(startIndex, jumpIndex - startIndex);
        codes.InsertRange(startIndex,
        [
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Ldloca_S, 0),
            new CodeInstruction(OpCodes.Ldloca_S, 1),
            new CodeInstruction(OpCodes.Call, PatcherUtility.m_TryGetImpregnationPair),
            new CodeInstruction(OpCodes.Brtrue_S, jumpLabel),

            new CodeInstruction(OpCodes.Ldc_I4_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AcceptanceReport), "op_Implicit", [typeof(bool)])),

            //new CodeInstruction(OpCodes.Ldstr, "PawnsCannotReproduce"),
            //new CodeInstruction(OpCodes.Ldarg_0),
            //new CodeInstruction(OpCodes.Ldstr, "PAWN1"),
            //new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NamedArgumentUtility), nameof(NamedArgumentUtility.Named))),
            //new CodeInstruction(OpCodes.Ldarg_1),
            //new CodeInstruction(OpCodes.Ldstr, "PAWN2"),
            //new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NamedArgumentUtility), nameof(NamedArgumentUtility.Named))),
            //new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TranslatorFormattedStringExtensions), nameof(TranslatorFormattedStringExtensions.Translate), [typeof(string), typeof(NamedArgument)])),
            //new CodeInstruction(OpCodes.Stloc_S, 8),
            //new CodeInstruction(OpCodes.Ldloc_S, 8),
            //new CodeInstruction(OpCodes.Calli, AccessTools.Method(typeof(string), nameof(TaggedString.Resolve))),
            //new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AcceptanceReport), "op_Implicit", [typeof(string)])),
            new CodeInstruction(OpCodes.Ret)
        ]);

        return codes.AsEnumerable();
    }
}
