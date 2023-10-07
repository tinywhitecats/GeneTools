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

Regarding HAR support:
-  For the most part, all GeneTools features are disabled on HAR aliens. The issue with HAR is that almost all the appearance handling is in one function, and it completely bypasses all the vanilla systems. While I'd like to make (e.g) GT bodies override HAR bodies, I can't really do that without completely replacing the entire texture handler for HAR.
- The HAR texture handler crashes the entire game with GT bodies and heads. I can't let it run alongside GT stuff.
