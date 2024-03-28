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

    public void Start() {
        try
        {
            //new TracePatch().DoPatch();
            new SituationPatch().DoPatch();
            new ChangePatch("Prefix").DoPatch();
            new ChangePatch("Postfix").DoPatch();
            new ConstructorPatch().DoPatch();
            new ConvertPatch().DoPatch();
            new AcceptPatch().DoPatch();
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

}

