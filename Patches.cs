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
        this.original = AccessTools.Method(typeof(NullPayload), "FirstHeartbeat");
        this.patch = AccessTools.Method(typeof(MainPatch), "Postfix");
    }

    public static void Postfix(ref Timeshadow ____timeshadow) {
        Traverse lifetime = Traverse.Create(____timeshadow).Field("_lifetimeAccurate");
        try {
            NoonUtility.Log(string.Format("RegainTickOnCreation: First Heartbeat current {0}", lifetime.GetValue<int>()));
        } catch {}
        lifetime.SetValue( lifetime.GetValue<int>() + 1 );
        try {
            NoonUtility.Log(string.Format("RegainTickOnCreation: First Heartbeat adjustment {0}, Trace {1}", lifetime.GetValue<int>(), new System.Diagnostics.StackTrace()));
        } catch {}

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

