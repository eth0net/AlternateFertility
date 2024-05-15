using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace AlternateFertility.Harmony;

/// <summary>
/// Harmony_PregnancyUtility_CanEverProduceChild patches PregnancyUtility to handle our impregnation genes.
/// </summary>
[HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.CanEverProduceChild))]
static class Harmony_PregnancyUtility_CanEverProduceChild
{
    enum ReproductionType
    {
        None,
        Androdite,
        Gynodite,
        Hermaphrodite
    }

    static FieldInfo f_gender = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));

    static MethodInfo m_PairCanImpregnate = AccessTools.Method(typeof(Harmony_PregnancyUtility_CanEverProduceChild), nameof(PairCanImpregnate));

    static bool PairCanImpregnate(Pawn pawn1, Pawn pawn2, out Pawn impregnator, out Pawn impregnatee)
    {
        switch ((GetReproductionType(pawn1), GetReproductionType(pawn2)))
        {
            case (ReproductionType.Hermaphrodite, ReproductionType.Hermaphrodite):
                // both hermaphrodites, pick one to be the impregnator
                var rand = Rand.Bool;
                impregnator = rand ? pawn1 : pawn2;
                impregnatee = rand ? pawn2 : pawn1;
                return true;
            case (ReproductionType.Androdite, ReproductionType.Gynodite):
            case (ReproductionType.Androdite, ReproductionType.Hermaphrodite):
            case (ReproductionType.Hermaphrodite, ReproductionType.Gynodite):
                impregnator = pawn1;
                impregnatee = pawn2;
                return true;
            case (ReproductionType.Gynodite, ReproductionType.Androdite):
            case (ReproductionType.Gynodite, ReproductionType.Hermaphrodite):
            case (ReproductionType.Hermaphrodite, ReproductionType.Androdite):
                impregnator = pawn2;
                impregnatee = pawn1;
                return true;
            case (_, _):
                impregnator = pawn1;
                impregnatee = pawn2;
                return false;
        }
    }

    static ReproductionType GetReproductionType(Pawn pawn)
    {
        if (pawn == null)
            return ReproductionType.None;

        if (pawn.genes.HasGene(GeneDefOf.Gynodite))
            return ReproductionType.Gynodite;

        if (pawn.genes.HasGene(GeneDefOf.Androdite))
            return ReproductionType.Androdite;

        if (pawn.genes.HasGene(GeneDefOf.Hermaphrodite))
            return ReproductionType.Hermaphrodite;

        return pawn.gender switch
        {
            Gender.Male => ReproductionType.Androdite,
            Gender.Female => ReproductionType.Gynodite,
            _ => ReproductionType.None,
        };
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codes = new List<CodeInstruction>(instructions);

        var startIndex = codes.FindIndex(code => code.LoadsField(f_gender));
        var jumpIndex = codes.FindIndex(startIndex, code => code.opcode == OpCodes.Ldloc_0);

        var jumpLabel = generator.DefineLabel();
        codes[jumpIndex] = codes[jumpIndex].WithLabels(jumpLabel);

        codes.RemoveRange(startIndex, jumpIndex - startIndex);
        codes.InsertRange(startIndex,
        [
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Ldloca_S, 0),
            new CodeInstruction(OpCodes.Ldloca_S, 1),
            new CodeInstruction(OpCodes.Call, m_PairCanImpregnate),
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

        //Log.Error(codes.Join(code => code.ToString(), "\n"));

        return codes.AsEnumerable();
    }
}
