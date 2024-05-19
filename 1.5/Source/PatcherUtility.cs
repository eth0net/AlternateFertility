using HarmonyLib;
using System.Reflection;
using Verse;

namespace AlternateFertility;

static class PatcherUtility
{
    internal static readonly FieldInfo f_gender = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));

    internal static readonly MethodInfo m_CanGetPregnant = AccessTools.Method(typeof(PatcherUtility), nameof(CanGetPregnant));

    internal static readonly MethodInfo m_CanImpregnate = AccessTools.Method(typeof(PatcherUtility), nameof(CanImpregnate));

    internal static readonly MethodInfo m_GetImpregnationPair = AccessTools.Method(typeof(PatcherUtility), nameof(GetImpregnationPair));

    internal static readonly MethodInfo m_GetImpregnationPairPossible = AccessTools.Method(typeof(PatcherUtility), nameof(GetImpregnationPairPossible));

    internal static readonly MethodInfo m_GetImpregnationPossible = AccessTools.Method(typeof(PatcherUtility), nameof(GetImpregnationPossible));

    internal static ReproductionType GetReproductionType(this Pawn pawn)
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

    internal static bool LoadsPawnGender(this CodeInstruction instruction) => instruction.LoadsField(f_gender);

    internal static bool IsAndrodite(this Pawn pawn) => pawn.GetReproductionType() == ReproductionType.Androdite;

    internal static bool IsGynodite(this Pawn pawn) => pawn.GetReproductionType() == ReproductionType.Gynodite;

    internal static bool IsHermaphrodite(this Pawn pawn) => pawn.GetReproductionType() == ReproductionType.Hermaphrodite;

    internal static bool CanGetPregnant(this Pawn pawn) => pawn.IsGynodite() || pawn.IsHermaphrodite();

    internal static bool CanImpregnate(this Pawn pawn) => pawn.IsAndrodite() || pawn.IsHermaphrodite();

    internal static void GetImpregnationPair(Pawn pawn1, Pawn pawn2, out Pawn impregnator, out Pawn impregnatee)
    {
        _ = GetImpregnationPairPossible(pawn1, pawn2, out impregnator, out impregnatee);
    }

    internal static bool GetImpregnationPairPossible(Pawn pawn1, Pawn pawn2, out Pawn impregnator, out Pawn impregnatee)
    {
        if (pawn1.IsHermaphrodite() && pawn2.IsHermaphrodite())
        {
            var rand = Rand.Bool;
            impregnator = rand ? pawn1 : pawn2;
            impregnatee = rand ? pawn2 : pawn1;
            return true;
        }
        else if (pawn1.CanImpregnate() && pawn2.CanGetPregnant())
        {
            impregnator = pawn1;
            impregnatee = pawn2;
            return true;
        }
        else if (pawn1.CanGetPregnant() && pawn2.CanImpregnate())
        {
            impregnator = pawn2;
            impregnatee = pawn1;
            return true;
        }
        else
        {
            impregnator = null;
            impregnatee = null;
            return false;
        }
    }

    internal static bool GetImpregnationPossible(Pawn pawn1, Pawn pawn2) => GetImpregnationPairPossible(pawn1, pawn2, out _, out _);
}
