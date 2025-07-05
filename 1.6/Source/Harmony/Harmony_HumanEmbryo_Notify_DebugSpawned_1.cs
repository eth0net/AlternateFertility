//using HarmonyLib;
//using RimWorld;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;

//namespace AlternateFertility.Harmony;

///// <summary>
///// Harmony_HumanEmbryo_Notify_DebugSpawned_1 patches HumanEmbryo to handle our impregnation genes.
///// </summary>
//[HarmonyPatch("HumanEmbryo+<>c.<Notify_DebugSpawned>b__21_1")]
//static class Harmony_HumanEmbryo_Notify_DebugSpawned_1
//{
//    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//    {
//        var codes = new List<CodeInstruction>(instructions);

//        var index = codes.FindIndex(PatcherUtility.LoadsPawnGender);

//        codes[index] = new CodeInstruction(OpCodes.Call, PatcherUtility.m_CanGetPregnant);
//        codes[index + 1] = new CodeInstruction(OpCodes.Nop);
//        codes[index + 2].opcode = OpCodes.Brfalse_S;

//        return codes.AsEnumerable();
//    }
//}
