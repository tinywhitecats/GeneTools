﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GeneTools
{
    public class GeneToolsGeneDef : DefModExtension
    {
        public List<BodyTypeDef> forcedBodyTypes;
        public List<BodyTypeDef> forcedBodyTypesFemale;
        public List<BodyTypeDef> forcedBodyTypesMale;
        public List<BodyTypeDef> forcedBodyTypesBaby;
        public List<BodyTypeDef> forcedBodyTypesChild;

        public List<HeadTypeDef> forcedHeadTypes;
        public List<HeadTypeDef> forcedHeadTypesFemale;
        public List<HeadTypeDef> forcedHeadTypesMale;
        public List<HeadTypeDef> forcedHeadTypesBaby;
        public List<HeadTypeDef> forcedHeadTypesChild;
    }

    public class GeneToolsApparelDef : DefModExtension
    {
        public List<BodyTypeDef> forcedBodyTypes;
        public List<BodyTypeDef> allowedBodyTypes;
        public List<HeadTypeDef> forcedHeadTypes;
        public List<HeadTypeDef> allowedHeadTypes;
    }

    public class GeneToolsBodyTypeDef : DefModExtension
    {
        public bool colorBody = true;
        public BodyTypeDef substituteBody;
        public bool useShader = false;
        public bool enforceOnInvisibleApparel = false;
    }
    public class GeneToolsHeadTypeDef : DefModExtension
    {
        public bool colorHead = true;
        public bool useShader = false;
        public bool enforceHelmets = false;
        public bool enforceOnInvisibleApparel = false;
    }
    public class GeneToolsHairDef : DefModExtension
    {
        public bool useSkinColor = false;
    }
}
