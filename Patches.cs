using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using SecretHistories.UI;
using SecretHistories.Entities;
using SecretHistories.Commands;
using SecretHistories.Logic;
using SecretHistories.Abstract;
using HarmonyLib;

public class MainPatch : Patch
{
    public MainPatch() {
        this.original = AccessTools.Method(typeof(ElementStack), "FirstHeartbeat");
        this.patch = AccessTools.Method(typeof(MainPatch), "Postfix");
    }

    //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //{
    //    var codes = new List<CodeInstruction>(instructions);
        //int ind = PatchHelper.FindLdstrOperand(codes, "SituationTokenSpawn");
        //codes.Insert(ind - 7, new CodeInstruction(OpCodes.Ldloc_1)); //token
        //codes.Insert(ind - 6, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Token), "get_Payload")));
        //codes.Insert(ind - 5, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(IManifestable), "GetTimeshadow")));
        //codes.Insert(ind - 4, new CodeInstruction(OpCodes.Dup));
        //codes.Insert(ind - 3, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
        //codes.Insert(ind - 2, new CodeInstruction(OpCodes.Ldc_I4_1));
        //codes.Insert(ind - 1, new CodeInstruction(OpCodes.Add));
        //codes.Insert(ind - 0, new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
        
        //int ind = PatchHelper.FindLdstrOperand(codes, "SituationTokenSpawn", 1);
        //codes.Insert(ind - 4, new CodeInstruction(OpCodes.Dup)); //token
        //codes.Insert(ind - 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Token), "get_Payload")));
        //codes.Insert(ind - 1, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(IManifestable), "GetTimeshadow")));
        //codes.Insert(ind - 0, new CodeInstruction(OpCodes.Dup));
        //codes.Insert(ind + 1, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
        //codes.Insert(ind + 2, new CodeInstruction(OpCodes.Ldc_I4_1));
        //codes.Insert(ind + 3, new CodeInstruction(OpCodes.Add));
        //codes.Insert(ind + 4, new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
    //    return codes.AsEnumerable();
    //}

    public static void Postfix(ref Timeshadow ____timeshadow) {
        Travese lifetime = Traverse.Create(____timeshadow).Field("_lifetimeAccurate");
        lifetime.SetValue( lifetime.GetValue<int>() + 1 );
    }

}

public class PostPatch : Patch
{
    public PostPatch() {
        this.original = AccessTools.Method(typeof(TokenCreationCommand), "Execute");
        this.patch = AccessTools.Method(typeof(PostPatch), "Postfix");
    }

    public static void Postfix(ref Token __result)
    {
        try {
            NoonUtility.Log(string.Format("RegainTickOnCreation: Token {0}", __result.Payload.GetTimeshadow().LifetimeRemaining));
        } catch {}
    }

}

public class TracePatch : Patch
{
    public TracePatch() {
        this.original = AccessTools.Method(typeof(Timeshadow), "SpendTime");
        this.patch = AccessTools.Method(typeof(TracePatch), "Postfix");
    }

    public static void Postfix(object[] __args, int ____lifetimeAccurate) {
        try {
            NoonUtility.Log(string.Format("RegainTickOnCreation: Timeshadow Value {0}, current {1}, Trace {2}", __args[0], ____lifetimeAccurate, new System.Diagnostics.StackTrace()));
        } catch {}
    }
    
}

