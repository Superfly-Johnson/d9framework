﻿// RimWorld.ShieldBelt
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

/// <summary>
/// <c>ThingClass</c> which acts like the vanilla <c>ShieldBelt</c> class but allows the user to fire ranged weapons while worn.
/// </summary>
/// <remarks>
/// Used to be a pretty simple subclass of <c>ShieldBelt</c>, but that ran into compatibility problems:
/// <list type="bullet">
/// <item>Some mods check whether ranged shots were allowed by seeing if the thingClass <c>is</c> a ShieldBelt, which returned true in this case, 
/// causing errors including them being automatically, erroneously, unequipped on ranged pawns.</item>
/// <item>A growing number of patches were necessary to prevent the vanilla game treating ranged shield belts in the same way.</item>
/// </list>
/// </remarks>
[StaticConstructorOnStartup]
public class RangedShieldBelt : Apparel
{
	private float energy;

	private int ticksToReset = -1;

	private int lastKeepDisplayTick = -9999;

	private Vector3 impactAngleVect;

	private int lastAbsorbDamageTick = -9999;

	private const float MinDrawSize = 1.2f;

	private const float MaxDrawSize = 1.55f;

	private const float MaxDamagedJitterDist = 0.05f;

	private const int JitterDurationTicks = 8;

	private int StartingTicksToReset = 3200;

	private float EnergyOnReset = 0.2f;

	private float EnergyLossPerDamage = 0.033f;

	private int KeepDisplayingTicks = 1000;

	private float ApparelScorePerEnergyMax = 0.25f;

	private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

	private float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

	private float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

	public float Energy => energy;

	public ShieldState ShieldState
	{
		get
		{
			if (ticksToReset > 0)
			{
				return ShieldState.Resetting;
			}
			return ShieldState.Active;
		}
	}

	private bool ShouldDisplay
	{
		get
		{
			Pawn wearer = base.Wearer;
			if (!wearer.Spawned || wearer.Dead || wearer.Downed)
			{
				return false;
			}
			if (wearer.InAggroMentalState)
			{
				return true;
			}
			if (wearer.Drafted)
			{
				return true;
			}
			if (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner)
			{
				return true;
			}
			if (Find.TickManager.TicksGame < lastKeepDisplayTick + KeepDisplayingTicks)
			{
				return true;
			}
			return false;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref energy, "energy", 0f);
		Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
		Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0);
	}

	public override IEnumerable<Gizmo> GetWornGizmos()
	{
		foreach (Gizmo wornGizmo in base.GetWornGizmos())
		{
			yield return wornGizmo;
		}
		if (Find.Selector.SingleSelectedThing == base.Wearer)
		{
			Gizmo_RangedShieldStatus gizmo = new Gizmo_RangedShieldStatus();
			gizmo.shield = this;
			yield return gizmo;
		}
	}

	public override float GetSpecialApparelScoreOffset()
	{
		return EnergyMax * ApparelScorePerEnergyMax;
	}

	public override void Tick()
	{
		base.Tick();
		if (base.Wearer == null)
		{
			energy = 0f;
		}
		else if (ShieldState == ShieldState.Resetting)
		{
			ticksToReset--;
			if (ticksToReset <= 0)
			{
				Reset();
			}
		}
		else if (ShieldState == ShieldState.Active)
		{
			energy += EnergyGainPerTick;
			if (energy > EnergyMax)
			{
				energy = EnergyMax;
			}
		}
	}

	public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
	{
		if (ShieldState != 0)
		{
			return false;
		}
		if (dinfo.Def == DamageDefOf.EMP)
		{
			energy = 0f;
			Break();
			return false;
		}
		if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
		{
			energy -= dinfo.Amount * EnergyLossPerDamage;
			if (energy < 0f)
			{
				Break();
			}
			else
			{
				AbsorbedDamage(dinfo);
			}
			return true;
		}
		return false;
	}

	public void KeepDisplaying()
	{
		lastKeepDisplayTick = Find.TickManager.TicksGame;
	}

	private void AbsorbedDamage(DamageInfo dinfo)
	{
		SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map));
		impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
		Vector3 loc = base.Wearer.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;
		float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
		FleckMaker.Static(loc, base.Wearer.Map, FleckDefOf.ExplosionFlash, num);
		int num2 = (int)num;
		for (int i = 0; i < num2; i++)
		{
			FleckMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
		}
		lastAbsorbDamageTick = Find.TickManager.TicksGame;
		KeepDisplaying();
	}

	private void Break()
	{
		SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map));
		FleckMaker.Static(base.Wearer.TrueCenter(), base.Wearer.Map, FleckDefOf.ExplosionFlash, 12f);
		for (int i = 0; i < 6; i++)
		{
			FleckMaker.ThrowDustPuff(base.Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), base.Wearer.Map, Rand.Range(0.8f, 1.2f));
		}
		energy = 0f;
		ticksToReset = StartingTicksToReset;
	}

	private void Reset()
	{
		if (base.Wearer.Spawned)
		{
			SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map));
			FleckMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 3f);
		}
		ticksToReset = -1;
		energy = EnergyOnReset;
	}

	public override void DrawWornExtras()
	{
		if (ShieldState == ShieldState.Active && ShouldDisplay)
		{
			float num = Mathf.Lerp(1.2f, 1.55f, energy);
			Vector3 drawPos = base.Wearer.Drawer.DrawPos;
			drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
			if (num2 < 8)
			{
				float num3 = (float)(8 - num2) / 8f * 0.05f;
				drawPos += impactAngleVect * num3;
				num -= num3;
			}
			float angle = Rand.Range(0, 360);
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
		}
	}

	public override bool AllowVerbCast(Verb verb)
	{
		return true;
	}
}