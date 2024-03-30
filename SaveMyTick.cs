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

public class SaveMyTick : MonoBehaviour
{
    public static bool started = false;
    public static PatchTracker situationPatch {get; private set;}
    public static PatchTracker changePatchPre {get; private set;}
    public static PatchTracker changePatchPost {get; private set;}
    public static PatchTracker constructorPatch {get; private set;}
    public static PatchTracker convertPatch {get; private set;}
    public static PatchTracker acceptPatch {get; private set;}

    public void Start() => SceneManager.sceneLoaded += Load;

    public void OnDestroy() => SceneManager.sceneLoaded -= Load;

    public void Load() {
        try
        {
            if (!started) {
                situationPatch   = new PatchTracker("SaveMyTick", new SituationPatch());
                changePatchPre   = new PatchTracker("SaveMyTick", new ChangePatch("Prefix"));
                changePatchPost  = new PatchTracker("SaveMyTick", new ChangePatch("Postfix"));
                constructorPatch = new PatchTracker("SaveMyTick", new ConstructorPatch());
                convertPatch     = new PatchTracker("SaveMyTick", new ConvertPatch());
                acceptPatch      = new PatchTracker("SaveMyTick", new AcceptPatch());
                started = true;
            } else {
                situationPatch.Subscribe();
                changePatchPre.Subscribe();
                changePatchPost.Subscribe();
                constructorPatch.Subscribe();
                convertPatch.Subscribe();
                acceptPatch.Subscribe();
            }
        }
        catch (Exception ex)
        {
          NoonUtility.LogException(ex);
        }
        NoonUtility.Log("SaveMyTick: Trackers Started");
    }

    public static void Initialise() {
        //Harmony.DEBUG = true;
        Patch.harmony = new Harmony("robynthedevil.savemytick");
		new GameObject().AddComponent<SaveMyTick>();
        NoonUtility.Log("SaveMyTick: Initialised");
	}

}

