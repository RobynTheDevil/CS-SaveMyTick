using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SecretHistories;
using SecretHistories.UI;
using SecretHistories.Manifestations;
using SecretHistories.Entities;
using SecretHistories.Spheres;
using SecretHistories.Abstract;
using HarmonyLib;

public class RegainTickOnCreation : MonoBehaviour
{
    public static PatchTracker mainPatch {get; private set;}
    public static PatchTracker savePatch {get; private set;}
    public static PatchTracker tracePatch {get; private set;}

    public void Start() {
        try
        {
            mainPatch = new PatchTracker("AlwaysShowTimers", new MainPatch("Postfix"), WhenSettingUpdated);
            savePatch = new PatchTracker("AlwaysShowTimers", new SavePatch(), WhenSettingUpdated);
            tracePatch = new PatchTracker("AlwaysShowTimers", new MainPatch("Prefix"), WhenSettingUpdated);
        }
        catch (Exception ex)
        {
          NoonUtility.LogException(ex);
        }
        NoonUtility.Log("RegainTickOnCreation: Trackers Started");
    }

    public static void Initialise() {
        Harmony.DEBUG = true;
        Patch.harmony = new Harmony("robynthedevil.regaintickoncreation");
		new GameObject().AddComponent<RegainTickOnCreation>();
        NoonUtility.Log("RegainTickOnCreation: Initialised");
	}

    public static void WhenSettingUpdated(SettingTracker<bool> tracker) {
        NoonUtility.Log(string.Format("RegainTickOnCreation: Setting Updated {0}", tracker.current));
        if (tracker.current) {
            RegainTickOnCreation.Enable();
        } else {
            RegainTickOnCreation.Disable();
        }
    }

    public static void Enable() {
    }

    public static void Disable() {
    }
}

