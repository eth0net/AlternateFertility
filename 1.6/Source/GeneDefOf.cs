using RimWorld;
using Verse;

namespace AlternateFertility;

[DefOf]
public static class GeneDefOf
{
    public static GeneDef AlternateFertility_Androdite;
    public static GeneDef AlternateFertility_Gynodite;
    public static GeneDef AlternateFertility_Hermaphrodite;
    public static GeneDef AlternateFertility_Potendite;
    public static GeneDef AlternateFertility_Recepdite;
    public static GeneDef AlternateFertility_Reflectite;
    public static GeneDef AlternateFertility_Solicite;

    static GeneDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
    }
}
