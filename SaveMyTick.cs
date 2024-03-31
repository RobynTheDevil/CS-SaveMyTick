using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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
    public static PatchTracker constructorPatch {get; private set;}
    public static PatchTracker convertPatch {get; private set;}
    public static PatchTracker acceptPatch {get; private set;}

    //debug
    public static PatchTracker tracePatch {get; private set;}
    public static PatchTracker tracePatch2 {get; private set;}

    public void Start() => SceneManager.sceneLoaded += Load;

    public void OnDestroy() => SceneManager.sceneLoaded -= Load;

    public void Load(Scene scene, LoadSceneMode mode) {
        try
        {
            if (!started) {
                situationPatch   = new PatchTracker("SaveMyTick", new SituationPatch());
                constructorPatch = new PatchTracker("SaveMyTick", new ConstructorPatch());
                convertPatch     = new PatchTracker("SaveMyTick", new ConvertPatch());
                acceptPatch      = new PatchTracker("SaveMyTick", new AcceptPatch());
                //tracePatch       = new PatchTracker("SaveMyTick", new TracePatch());
                //tracePatch2      = new PatchTracker("SaveMyTick", new TracePatch2());
                started = true;
            } else {
                situationPatch.Subscribe();
                constructorPatch.Subscribe();
                convertPatch.Subscribe();
                acceptPatch.Subscribe();
                //tracePatch.Subscribe();
                //tracePatch2.Subscribe();
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

