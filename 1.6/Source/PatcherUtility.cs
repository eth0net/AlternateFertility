using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace AlternateFertility;

static class PatcherUtility
{
    internal static readonly FieldInfo f_gender = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));

    internal static readonly MethodInfo m_GetFirstSpouseOfOppositeGender = AccessTools.Method(typeof(SpouseRelationUtility), nameof(SpouseRelationUtility.GetFirstSpouseOfOppositeGender));

    internal static readonly MethodInfo m_CanGetPregnant = AccessTools.Method(typeof(PatcherUtility), nameof(CanGetPregnant));

    internal static readonly MethodInfo m_CanImpregnate = AccessTools.Method(typeof(PatcherUtility), nameof(CanImpregnate));

    internal static readonly MethodInfo m_GetImpregnationPair = AccessTools.Method(typeof(PatcherUtility), nameof(GetImpregnationPair));

    internal static readonly MethodInfo m_GetImpregnationPairPossible = AccessTools.Method(typeof(PatcherUtility), nameof(GetImpregnationPairPossible));

    internal static readonly MethodInfo m_GetImpregnationPossible = AccessTools.Method(typeof(PatcherUtility), nameof(GetImpregnationPossible));

    internal static readonly MethodInfo m_GetFirstImpregnationPairSpouse = AccessTools.Method(typeof(PatcherUtility), nameof(GetFirstImpregnationPairSpouse));

    internal static readonly MethodInfo m_GetHumanEmbryoParents = AccessTools.Method(typeof(PatcherUtility), nameof(GetHumanEmbryoParents));

    internal static readonly MethodInfo m_GetHumanEmbryoImpregnator = AccessTools.Method(typeof(PatcherUtility), nameof(GetHumanEmbryoImpregnator));

    internal static readonly MethodInfo m_GetHumanEmbryoImpregnatee = AccessTools.Method(typeof(PatcherUtility), nameof(GetHumanEmbryoImpregnatee));

    internal static ReproductionType GetReproductionType(this Pawn pawn)
    {
        if (pawn == null)
            return ReproductionType.None;

        if (pawn.genes != null)
        {
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Gynodite))
                return ReproductionType.Gynodite;

            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Androdite))
                return ReproductionType.Androdite;

            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Hermaphrodite))
                return ReproductionType.Hermaphrodite;
        }

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
        // Prevent self-impregnation
        if (pawn1 == pawn2)
        {
            impregnator = null;
            impregnatee = null;
            return false;
        }
        
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

    internal static Pawn GetFirstImpregnationPairSpouse(this Pawn pawn)
    {
        foreach (Pawn spouse in pawn.GetSpouses(includeDead: true))
        {
            if (GetImpregnationPossible(pawn, spouse))
                return spouse;
        }
        return null;
    }

    internal static void GetHumanEmbryoParents(this HumanEmbryo embryo, out Pawn impregnator, out Pawn impregnatee)
    {
        var pawns = embryo.TryGetComp<CompHasPawnSources>().pawnSources;
        var count = pawns.Count;
        for (int i = 0; i < count - 1; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                if (GetImpregnationPairPossible(pawns[i], pawns[j], out impregnator, out impregnatee))
                    return;
            }
        }
        impregnator = null;
        impregnatee = null;
    }

    internal static Pawn GetHumanEmbryoImpregnator(this HumanEmbryo embryo)
    {
        GetHumanEmbryoParents(embryo, out Pawn impregnator, out _);
        return impregnator;
    }

    internal static Pawn GetHumanEmbryoImpregnatee(this HumanEmbryo embryo)
    {
        GetHumanEmbryoParents(embryo, out _, out Pawn impregnatee);
        return impregnatee;
    }
}
