# Gene Tools - Forked

See the original wiki for most info: https://github.com/prkrdp/GeneTools/wiki

Differences here from the experimental branch:
-  Apparel texture patch is now done per-apparel instead of the entire update function (better compatibility).
-  Head to helmet checking is off by default.
-  New headtype extension as GeneToolsHeadTypeDef.enforceHelmets extension. Enables the head to helmet checking.
    Vanilla head types are no longer patched to support all helmets! No reason for that now.
-  HAR compatibility improved and integrated - always on if HAR is installed.
-  Refactored source code.
-  New hair extension as GeneToolsHairDef.useSkinColor. Enabling this passes a pawn's skin color as a second color to the hair shader. This takes priority over HairModdingPlus when enabled.
-  New body and head extensions as GeneToolsBodyTypeDef.enforceOnInvisibleApparel and GeneToolsHeadTypeDef.enforceOnInvisibleApparel. By default, invisible apparel is always acceptable on any body/head, since it will never be missing textures. Enabling this makes the head/body allowed check active on invisible apparel.

Notes:
-  Substitute bodies don't work on fur graphics (e.g. furskin), don't let a pawn have both at once! Looking in to this.
-  Vanilla bodytype genes take precedent over forced bodytypes on adults. I've looked into this, but haven't figured out why.
-  Changing pawn age via character editor doesn't always update body type. I should be able to fix this.

Regarding HAR support:
-  For the most part, all GeneTools features are disabled on HAR aliens. The issue with HAR is that almost all the appearance handling is in one function, and it completely bypasses all the vanilla systems. While I'd like to make (e.g) GT bodies override HAR bodies, I can't really do that without completely replacing the entire texture handler for HAR.
-  The HAR texture handler crashes the entire game with GT bodies and heads. I can't let it run alongside GT stuff.
