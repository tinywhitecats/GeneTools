# Gene Tools - Forked

See the original wiki for most info: https://github.com/prkrdp/GeneTools/wiki

Differences here from the experimental branch:

-    Apparel texture patch is now done per-apparel instead of the entire update function (better compatibility).
-    Head to helmet checking is off by default.
-    New headtype extension as GeneToolsHeadTypeDef.enforceHelmets extension. Enables the head to helmet checking. Vanilla head types are no longer patched to support all helmets! No reason for that now.
-    HAR compatibility improved and integrated - always on if HAR is installed.
-    Refactored source code.
-    New hair extension as GeneToolsHairDef.useSkinColor. Enabling this passes a pawn's skin color as a second color to the hair shader. This takes priority over HairModdingPlus when enabled.
-    New body and head extensions as GeneToolsBodyTypeDef.enforceOnInvisibleApparel and GeneToolsHeadTypeDef.enforceOnInvisibleApparel. By default, invisible apparel is always acceptable on any body/head, since it will never be missing textures. Enabling this makes the head/body allowed check active on invisible apparel.
-    Added forcedBodyTypesMale and forcedHeadTypesMale. Equivalent to the female-only version, these are only necessary if you want to retain unmodified female heads/bodies while overwriting male ones.

Notes:

-    Vanilla bodytype genes may not cooperate with GT bodytype genes on the same pawn. In theory it's all set up for the vanilla exclusionTags to work, but in testing I couldn't get it to behave consistently. 
-    Changing pawn age via character editor doesn't always update body type. I should be able to fix this. For now there's a debug tool to set bodytype in the base game, so you can fix it with that.

Regarding HAR support:

-    For the most part, all GeneTools features are disabled on HAR aliens. The issue with HAR is that almost all the appearance handling is in one function, and it completely bypasses all the vanilla systems. While I'd like to make (e.g) GT bodies override HAR bodies, I can't really do that without completely replacing the entire texture handler for HAR.
-    The HAR texture handler crashes the entire game with GT bodies and heads. I can't let it run alongside GT stuff.