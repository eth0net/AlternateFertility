using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace AlternateFertility;

static class PatcherUtility
{
    internal static readonly FieldInfo f_gender = AccessTools.Field(typeof(Pawn), nameof(Pawn.gender));

    internal static readonly MethodInfo m_GetFirstSpouseOfOppositeGender = AccessTools.Method(
        typeof(SpouseRelationUtility), nameof(SpouseRelationUtility.GetFirstSpouseOfOppositeGender)
    );

    internal static readonly MethodInfo m_CanGetPregnant = AccessTools.Method(
        typeof(PatcherUtility), nameof(CanGetPregnant)
    );

    internal static readonly MethodInfo m_CanImpregnate = AccessTools.Method(
        typeof(PatcherUtility), nameof(CanImpregnate)
    );

    internal static readonly MethodInfo m_GetImpregnationPair = AccessTools.Method(
        typeof(PatcherUtility), nameof(GetImpregnationPair)
    );

    internal static readonly MethodInfo m_TryGetImpregnationPair =
        AccessTools.Method(typeof(PatcherUtility), nameof(TryGetImpregnationPair));

    internal static readonly MethodInfo m_GetImpregnationPossible =
        AccessTools.Method(typeof(PatcherUtility), nameof(GetImpregnationPossible));

    internal static readonly MethodInfo m_GetFirstImpregnationPairSpouse = AccessTools.Method(
        typeof(PatcherUtility), nameof(GetFirstImpregnationPairSpouse)
    );

    internal static readonly MethodInfo m_GetHumanEmbryoImpregnator =
        AccessTools.Method(typeof(PatcherUtility), nameof(GetHumanEmbryoImpregnator));

    internal static readonly MethodInfo m_GetHumanEmbryoImpregnatee =
        AccessTools.Method(typeof(PatcherUtility), nameof(GetHumanEmbryoImpregnatee));

    internal static ReproductionType GetReproductionType(this Pawn pawn)
    {
        if (pawn == null)
            return ReproductionType.None;

        if (pawn.genes != null)
        {
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Potendite))
                return ReproductionType.Potendite;
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Recepdite))
                return ReproductionType.Recepdite;
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Gynodite))
                return ReproductionType.Gynodite;
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Androdite))
                return ReproductionType.Androdite;
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Hermaphrodite))
                return ReproductionType.Hermaphrodite;
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Reflectite))
                return ReproductionType.Reflectite;
            if (pawn.genes.HasActiveGene(GeneDefOf.AlternateFertility_Absorbite))
                return ReproductionType.Absorbite;
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

    internal static bool IsHermaphrodite(this Pawn pawn) =>
        pawn.GetReproductionType() == ReproductionType.Hermaphrodite;

    internal static bool IsPotendite(this Pawn pawn) => pawn.GetReproductionType() == ReproductionType.Potendite;

    internal static bool IsRecepdite(this Pawn pawn) => pawn.GetReproductionType() == ReproductionType.Recepdite;

    internal static bool IsReflectite(this Pawn pawn) => pawn.GetReproductionType() == ReproductionType.Reflectite;
    internal static bool IsAbsorbite(this Pawn pawn) => pawn.GetReproductionType() == ReproductionType.Absorbite;

    internal static bool CanGetPregnant(this Pawn pawn) => pawn.IsGynodite() || pawn.IsHermaphrodite() || pawn.IsRecepdite() || pawn.IsAbsorbite();

    internal static bool CanImpregnate(this Pawn pawn) =>
        pawn.IsAndrodite() || pawn.IsHermaphrodite() || pawn.IsPotendite() || pawn.IsReflectite();

    internal static void GetImpregnationPair(Pawn pawn1, Pawn pawn2, out Pawn impregnator, out Pawn impregnatee)
    {
        _ = TryGetImpregnationPair(pawn1, pawn2, out impregnator, out impregnatee);
    }

    internal static bool TryGetImpregnationPair(Pawn pawn1, Pawn pawn2, out Pawn impregnator, out Pawn impregnatee)
    {
        // Prevent self-impregnation
        if (pawn1 == pawn2)
        {
            impregnator = null;
            impregnatee = null;
            return false;
        }

        if (pawn1.IsPotendite() && pawn2.IsPotendite())
        {
            var rand = Rand.Bool;
            impregnator = rand ? pawn1 : pawn2;
            impregnatee = rand ? pawn2 : pawn1;
            return true;
        }

        if (pawn1.IsPotendite())
        {
            impregnator = pawn1;
            impregnatee = pawn2;
            return true;
        }

        if (pawn2.IsPotendite())
        {
            impregnator = pawn2;
            impregnatee = pawn1;
            return true;
        }

        // Recepdite logic: Recepdite can be impregnated by anyone, but cannot impregnate
        if (pawn1.IsRecepdite() && pawn2.IsRecepdite())
        {
            var rand = Rand.Bool;
            impregnator = rand ? pawn2 : pawn1;
            impregnatee = rand ? pawn1 : pawn2;
            return true;
        }
        if (pawn1.IsRecepdite())
        {
            impregnator = pawn2;
            impregnatee = pawn1;
            return true;
        }
        if (pawn2.IsRecepdite())
        {
            impregnator = pawn1;
            impregnatee = pawn2;
            return true;
        }

        if (pawn1.IsHermaphrodite() && pawn2.IsHermaphrodite())
        {
            var rand = Rand.Bool;
            impregnator = rand ? pawn1 : pawn2;
            impregnatee = rand ? pawn2 : pawn1;
            return true;
        }

        if (pawn1.CanImpregnate() && pawn2.CanGetPregnant())
        {
            impregnator = pawn1;
            impregnatee = pawn2;
            return true;
        }

        if (pawn1.CanGetPregnant() && pawn2.CanImpregnate())
        {
            impregnator = pawn2;
            impregnatee = pawn1;
            return true;
        }

        // Reflecdite logic: cannot be impregnated, can only impregnate those who could normally impregnate
        if (pawn1.IsReflectite() && pawn2.IsReflectite())
        {
            impregnator = null;
            impregnatee = null;
            return false;
        }
        if (pawn1.IsReflectite() && pawn2.CanImpregnate())
        {
            impregnator = pawn1;
            impregnatee = pawn2;
            return true;
        }
        if (pawn2.IsReflectite() && pawn1.CanImpregnate())
        {
            impregnator = pawn2;
            impregnatee = pawn1;
            return true;
        }
        // Absorbite logic: cannot impregnate, can only be impregnated by those who could normally be impregnated
        if (pawn1.IsAbsorbite() && pawn2.IsAbsorbite())
        {
            impregnator = null;
            impregnatee = null;
            return false;
        }
        if (pawn1.IsAbsorbite() && pawn2.CanGetPregnant())
        {
            impregnator = pawn2;
            impregnatee = pawn1;
            return true;
        }
        if (pawn2.IsAbsorbite() && pawn1.CanGetPregnant())
        {
            impregnator = pawn1;
            impregnatee = pawn2;
            return true;
        }

        impregnator = null;
        impregnatee = null;
        return false;
    }

    internal static bool GetImpregnationPossible(Pawn pawn1, Pawn pawn2) =>
        TryGetImpregnationPair(pawn1, pawn2, out _, out _);

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
                if (TryGetImpregnationPair(pawns[i], pawns[j], out impregnator, out impregnatee))
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
