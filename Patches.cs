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
using SecretHistories.States;
using SecretHistories.Enums;
using HarmonyLib;

public class TracePatch : Patch
{
    public TracePatch() {
        this.original = AccessTools.Method(typeof(Situation), "TransitionToState");
        this.patch = AccessTools.Method(typeof(TracePatch), "Postfix");
    }

    // Trace
    public static void Postfix(SituationState newState){
        NoonUtility.Log(string.Format("SaveMyTick: Trace TransitionToState, {0}", newState.Identifier));
    }

}

public class TracePatch2 : Patch
{
    public TracePatch2() {
        this.original = AccessTools.Method(typeof(Sphere), "AcceptToken", new Type[] {typeof(Token), typeof(Context)});
        this.patch = AccessTools.Method(typeof(TracePatch2), "Postfix");
    }

    public static void Postfix(ref Sphere __instance, Token token, Context context) {
        Traverse lifetime = Traverse.Create(token.Payload.GetTimeshadow()).Field("_lifetimeAccurate");
        NoonUtility.Log(string.Format("SaveMyTick: Sphere AcceptToken (Post): {0}, {1}, {2}, {3}",
                    lifetime.GetValue<int>(), context.actionSource, token.Payload, __instance));
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
        bool isSituation = __result.Payload.GetType() == typeof(Situation);
        bool isDebug = context.actionSource == Context.ActionSource.Debug;
        if (ticks <= 0 || !isSituation || isDebug)
            return;
        lifetime.SetValue(ticks + 1);
        NoonUtility.Log(string.Format("SaveMyTick: Saved Tick (Create Situation): {0} -> {1}, {2}", ticks, lifetime.GetValue<int>(), context.actionSource));
        //NoonUtility.Log(string.Format("Action Source {0}", context.actionSource));
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
        NoonUtility.Log(string.Format("SaveMyTick: Saved Tick (Construct): {0} -> {1}", lifetimeRemaining, result));
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
        //NoonUtility.Log(string.Format("SaveMyTick: Saved Tick (Convert): {0} -> {1}", time, __result));
    }

}

public class AcceptPatch : Patch
{
    public AcceptPatch() {
        this.original = AccessTools.Method(typeof(Sphere), "AcceptToken", new Type[] {typeof(Token), typeof(Context)});
        this.patch = AccessTools.Method(typeof(AcceptPatch), "Postfix");
    }

    public static void Postfix(ref Sphere __instance, Token token, Context context)
    {
        if (Watchman.Get<Heart>().Metapaused)
            return;
        Type type = __instance.GetType();
        bool isStorage = type == typeof(SituationStorageSphere);
        bool isOutput = type == typeof(OutputSphere);
        bool isFlush = context.actionSource == Context.ActionSource.FlushingTokens;
        bool isEnRoute = type == typeof(EnRouteSphere);
        bool isGreedy = context.actionSource == Context.ActionSource.GreedyGrab;
        if (! ((isOutput && isFlush) || (isEnRoute && isGreedy)))
            return;
        Traverse lifetime = Traverse.Create(token.Payload).Field("_timeshadow").Field("_lifetimeAccurate");
        int ticks = lifetime.GetValue<int>();
        if (ticks <= 0)
            return;
        lifetime.SetValue(ticks + 1);
        NoonUtility.Log(string.Format("SaveMyTick: Saved Tick (Accept): {0} -> {1}, {2}, {3}, {4}",
            ticks, lifetime.GetValue<int>(), context.actionSource, token.Payload, __instance));
    }

}

