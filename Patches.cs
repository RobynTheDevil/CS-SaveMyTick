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
using SecretHistories.Spheres;
using HarmonyLib;

public class TracePatch : Patch
{
    public TracePatch() {
        this.original = AccessTools.Method(typeof(TokenCreationCommand), "Execute");
        this.patch = AccessTools.Method(typeof(TracePatch), "Prefix");
    }

    // Trace
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

}

public class SituationPatch : Patch
{
    public SituationPatch() {
        this.original = AccessTools.Method(typeof(TokenCreationCommand), "Execute");
        this.patch = AccessTools.Method(typeof(SituationPatch), "Postfix");
    }

    public static void Postfix(ref Token __result, Context context)
    {
        // avoid adding to tokens on game start/load
        if (Watchman.Get<Heart>().Metapaused)
            return;
        Traverse lifetime = Traverse.Create(__result.Payload.GetTimeshadow()).Field("_lifetimeAccurate");
        int ticks = lifetime.GetValue<int>();
        if (ticks <= 0)
            return;
        if (__result.Payload.GetType() != typeof(Situation))
            return;
        lifetime.SetValue(ticks + 1);
        NoonUtility.Log(string.Format("RegainTickOnCreation: Saved Tick (Create Situation): {0} -> {1}", ticks, lifetime.GetValue<int>()));
        //NoonUtility.Log(string.Format("Action Source {0}", context.actionSource));
    }

}

public class ChangePatch : Patch
{
    public ChangePatch(string type) {
        this.original = AccessTools.Method(typeof(ElementStack), "ChangeTo", new Type[] {typeof(string)});
        this.patch = AccessTools.Method(typeof(ChangePatch), type);
    }

    public class State {
        public int prev;
        public bool decays;
    }

    public static void Prefix(ref ElementStack __instance, ref State __state) {
        __state = new State();
        __state.prev = Traverse.Create(__instance).Field("_timeshadow").Field("_lifetimeAccurate").GetValue<int>();
        __state.decays = __instance.Decays;
    }

    public static void Postfix(ref ElementStack __instance, ref State __state) {
        if (Watchman.Get<Heart>().Metapaused || (__state.decays && __state.prev <= 0))
            return;
        Traverse lifetime = Traverse.Create(__instance).Field("_timeshadow").Field("_lifetimeAccurate");
        int ticks = lifetime.GetValue<int>();
        if (ticks <= 0)
            return;
        lifetime.SetValue(ticks + 1);
        NoonUtility.Log(string.Format("RegainTickOnCreation: Saved Tick (Card Change): {0} -> {1}", ticks, lifetime.GetValue<int>()));
    }
    
}

public class ConstructorPatch : Patch
{
    public ConstructorPatch() {
        this.original = AccessTools.Constructor(typeof(Timeshadow), new Type[] {typeof(float), typeof(float), typeof(bool)});
        this.patch = AccessTools.Method(typeof(ConstructorPatch), "Postfix");
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

// Method is sometimes inlined. ConstructorPatch fixes
public class ConvertPatch : Patch
{
    public ConvertPatch() {
        this.original = AccessTools.Method(typeof(Timeshadow), "ConvertToAccurate");
        this.patch = AccessTools.Method(typeof(ConvertPatch), "Postfix");
    }

    public static void Postfix(float time, ref int __result)
    {
        if (time <= 0.0f)
            return;
        __result = (int) (((double)time * 100.0) + 0.1);
        //NoonUtility.Log(string.Format("RegainTickOnCreation: Saved Tick (Convert): {0} -> {1}", time, __result));
    }

}

public class AcceptPatch : Patch
{
    public AcceptPatch() {
        this.original = AccessTools.Method(typeof(SituationStorageSphere), "AcceptToken", new Type[] {typeof(Token), typeof(Context)});
        this.patch = AccessTools.Method(typeof(AcceptPatch), "Prefix");
    }

    public static void Prefix(Token token, Context context)
    {
        if (Watchman.Get<Heart>().Metapaused)
            return;
        Traverse lifetime = Traverse.Create(token.Payload).Field("_timeshadow").Field("_lifetimeAccurate");
        int ticks = lifetime.GetValue<int>();
        if (ticks <= 0)
            return;
        lifetime.SetValue(ticks + 1);
        NoonUtility.Log(string.Format("RegainTickOnCreation: Saved Tick (Accept): {0} -> {1}, {2}, {3}", ticks, lifetime.GetValue<int>(), context.actionSource, token.Payload));
    }

}

