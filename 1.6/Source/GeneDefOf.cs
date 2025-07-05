using RimWorld;
using Verse;

namespace AlternateFertility;

[DefOf]
public static class GeneDefOf
{
    public static GeneDef AlternateFertility_Androdite;
    public static GeneDef AlternateFertility_Gynodite;
    public static GeneDef AlternateFertility_Hermaphrodite;

    static GeneDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
    }
}
