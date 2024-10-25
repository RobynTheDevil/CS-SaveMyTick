
https://steamcommunity.com/sharedfiles/filedetails/?id=3204067131

Observations:
1) On save & load, some tokens (cards and verbs) will lose a tick (0.01 seconds). This is due to inherent floating point errors; the game saves the data in decimal and converts back to float on load, occasionally losing accuracy.
2) On token creation, if the game is paused, the current Heart.Beat() will execute _after_ the token has been created. This leads to a lost tick in the case of tick perfect skipping.
3) On token sphere output the same happens. This is a distinct case from token creation, as situations do not experience this and elementstacks do (when they leave the situation storage sphere).
4) Greedy grab (e.g. season of suspicion, etc.) causes a lost tick.

This mod aims to repair these errors by using the following conversion: ticks = (int) (((double)time_float * 100) + 0.1)
And by adding 1 to the value when a tick would be erroneously consumed.

Caveats: For large timers (>2000 seconds) the issues may still persist for the same reasons, the error gets too large to correct in this way.

This mod patches the following methods:
TokenCreationCommand.Execute
Timeshadow.ctor
Timeshadow.ConvertToAccurate
Sphere.AcceptToken

I have developed and tested the mod with many of the popular workshop mods already enabled. Compatibility should be high.

