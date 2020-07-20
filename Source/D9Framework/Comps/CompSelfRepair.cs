﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace D9Framework
{
    /// <summary>
    /// Automatically repairs a specified (non-pawn) item. Example implementation of <see cref="D9Framework.CompWithCheapHashInterval"/> and designed for use with <see cref="D9Framework.CompFromStuff"/>.
    /// </summary>
    public class CompSelfRepair : CompWithCheapHashInterval
    {        
        CompProperties_SelfRepair Props => (CompProperties_SelfRepair)props;
        public override void CompTick()
        {
            base.CompTick();
            int hp = base.parent.HitPoints;
            if (IsCheapIntervalTick(Props.tickInterval) && parent.def.useHitPoints && hp < parent.MaxHitPoints) hp++;
        }
        public override string CompInspectStringExtra()
        {
            string ret = base.CompInspectStringExtra();
            if(Prefs.DevMode) ret += "CompSelfRepair with TicksPerRepair " + Props.tickInterval;
            return ret;
        }
    }
    /// <summary>
    /// <c>CompProperties</c> for use with <see cref="D9Framework.CompSelfRepair"/>. Allows specifying the tick interval in XML, and auto-assigns the appropriate class.
    /// </summary>
    class CompProperties_SelfRepair : CompProperties
    {
#pragma warning disable CS0649 //disable the warning that this field is never assigned to, as the game handles that
        public int tickInterval = 250;
#pragma warning restore CS0649

        public CompProperties_SelfRepair()
        {
            base.compClass = typeof(CompSelfRepair);
        }
    }
}