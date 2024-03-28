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
    public MainPatch(string type) {
        this.original = AccessTools.Method(typeof(TokenCreationCommand), "Execute");
        this.patch = AccessTools.Method(typeof(MainPatch), type);
    }

    public static void Prefix(TokenCreationCommand __instance){
        float lifetime;
        ITokenPayloadCreationCommand payload = __instance.Payload;
        if (payload is ElementStackCreationCommand) {
            lifetime = ((ElementStackCreationCommand)payload).LifetimeRemaining;
        } else if (payload is SituationCreationCommand) {
            lifetime = ((SituationCreationCommand)payload).TimeRemaining;
        } else {
            return;
        }
        if (lifetime > 0.0f)
        {
            NoonUtility.Log(string.Format("RegainTickOnCreation: Before Create Token: {0}, {1}", __instance, lifetime));
        }
    }

    public static void Postfix(ref Token __result)
    {
        Traverse lifetime = Traverse.Create(__result.Payload.GetTimeshadow()).Field("_lifetimeAccurate");
        int ticks = lifetime.GetValue<int>();
        if (ticks > 0)
        {
            NoonUtility.Log(string.Format("RegainTickOnCreation: After Token Create: {0}, {1}", __result, lifetime.GetValue<int>()));
            Type type = __result.Payload.GetType();
            if (type == typeof(ElementStack)) {
                //lifetime.SetValue(ticks + 1);
            } else if (type == typeof(Situation)) {
                //lifetime.SetValue(ticks + 1);
            }
            //NoonUtility.Log(string.Format("RegainTickOnCreation:                   :      {0}", lifetime.GetValue<int>()));
        }
    }

}

public class SavePatch : Patch
{
    public SavePatch() {
        this.original = AccessTools.Constructor(typeof(Timeshadow), new Type[] {typeof(float), typeof(float), typeof(bool)});
        this.patch = AccessTools.Method(typeof(SavePatch), Postfix);
    }

    public static void Postfix(float lifetimeRemaining, ref Timeshadow __instance)
    {
        if (lifetimeRemaining <= 0.0f)
            return;
        int result = (int) (((double)lifetimeRemaining * 100.0) + 0.1);
        Traverse.Create(__instance).Field("_lifetimeAccurate").SetValue(result);
        NoonUtility.Log(string.Format("RegainTickOnCreation: Saved Tick (Construct): {0} -> {1}", lifetimeRemaining, result));
    }

}

public class ConvertPatch : Patch
{
    public SavePatch() {
        this.original = AccessTools.Constructor(typeof(Timeshadow), new Type[] {typeof(float), typeof(float), typeof(bool)});
        this.patch = AccessTools.Method(typeof(SavePatch), Postfix);
    }

    public static void Postfix(float time, ref int __result)
    {
        if (time <= 0.0f)
            return;
        __result = (int) (((double)time * 100.0) + 0.1);
        NoonUtility.Log(string.Format("RegainTickOnCreation: Saved Tick (Convert): {0} -> {1}", time, __result));
    }
}

public class TracePatch : Patch
{
    public TracePatch() {
        //this.original = AccessTools.Method(typeof(Timeshadow), "SpendTime");
        //this.patch = AccessTools.Method(typeof(TracePatch), "Postfix");
        this.original = AccessTools.Method(typeof(TokenCreationCommand), "Execute");
        this.patch = AccessTools.Method(typeof(TracePatch), "Transpiler");
    }

    //public static void Postfix(object[] __args, int ____lifetimeAccurate) {
    //    try {
    //        NoonUtility.Log(string.Format("RegainTickOnCreation: Timeshadow Value {0}, current {1}, Trace {2}", __args[0], ____lifetimeAccurate, new System.Diagnostics.StackTrace()));
    //    } catch {}
    //}
        
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        int index = PatchHelper.FindOpcode(codes, OpCodes.Ldloc_0, 4);
        codes.Insert(index + 0, new CodeInstruction(OpCodes.Ldstr, "RegainTickOnCreation: Before Heartbeat: {0}"));
        codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldloc_1)); //newToken
        codes.Insert(index + 2, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Token), "Payload")));
        codes.Insert(index + 3, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(IManifestable), "GetTimeshadow")));
        codes.Insert(index + 4, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
        codes.Insert(index + 5, new CodeInstruction(OpCodes.Box, typeof(int)));
        codes.Insert(index + 6, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), "Format", new Type[] {typeof(string), typeof(object)})));
        codes.Insert(index + 7, new CodeInstruction(OpCodes.Ldc_I4_0)); // Message Level
        codes.Insert(index + 8, new CodeInstruction(OpCodes.Ldc_I4_8)); // Verbosity Chatter
        codes.Insert(index + 9, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NoonUtility), "Log", new Type[] {typeof(string), typeof(int), typeof(VerbosityLevel)})));
        //                   10
        //FirstHeartbeat     11
        codes.Insert(index + 12, new CodeInstruction(OpCodes.Ldloc_1)); //newToken
        codes.Insert(index + 13, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Token), "Payload")));
        codes.Insert(index + 14, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(IManifestable), "GetTimeshadow")));
        codes.Insert(index + 15, new CodeInstruction(OpCodes.Dup));
        codes.Insert(index + 16, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
        codes.Insert(index + 17, new CodeInstruction(OpCodes.Ldc_I4_1));
        codes.Insert(index + 18, new CodeInstruction(OpCodes.Add));
        codes.Insert(index + 19, new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
        //--
        codes.Insert(index + 20, new CodeInstruction(OpCodes.Ldstr, "RegainTickOnCreation: After Heartbeat: {0}"));
        codes.Insert(index + 21, new CodeInstruction(OpCodes.Ldloc_1)); //newToken
        codes.Insert(index + 22, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Token), "Payload")));
        codes.Insert(index + 23, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(IManifestable), "GetTimeshadow")));
        codes.Insert(index + 24, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Timeshadow), "_lifetimeAccurate")));
        codes.Insert(index + 25, new CodeInstruction(OpCodes.Box, typeof(int)));
        codes.Insert(index + 26, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), "Format", new Type[] {typeof(string), typeof(object)})));
        codes.Insert(index + 27, new CodeInstruction(OpCodes.Ldc_I4_0)); // Message Level
        codes.Insert(index + 28, new CodeInstruction(OpCodes.Ldc_I4_8)); // Verbosity Chatter
        codes.Insert(index + 29, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NoonUtility), "Log", new Type[] {typeof(string), typeof(int), typeof(VerbosityLevel)})));


        return codes.AsEnumerable();
    }
    
}

