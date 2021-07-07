﻿// RimWorld.Gizmo_EnergyShieldStatus
using RimWorld;
using UnityEngine;
using Verse;

/// <summary>
/// <c>Gizmo</c> which displays the current energy state of the <see cref="D9Framework.RangedShieldBelt"/>. Identical to the vanilla one, except references the aforementioned class since it's no longer a subclass.
/// </summary>
[StaticConstructorOnStartup]
public class Gizmo_RangedShieldStatus : Gizmo
{
	public RangedShieldBelt shield;

	private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

	private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

	public Gizmo_RangedShieldStatus()
	{
		order = -100f;
	}

	public override float GetWidth(float maxWidth)
	{
		return 140f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(6f);
		Widgets.DrawWindowBackground(rect);
		Rect rect3 = rect2;
		rect3.height = rect.height / 2f;
		Text.Font = GameFont.Tiny;
		Widgets.Label(rect3, shield.LabelCap);
		Rect rect4 = rect2;
		rect4.yMin = rect2.y + rect2.height / 2f;
		float fillPercent = shield.Energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
		Widgets.FillableBar(rect4, fillPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);
		Text.Font = GameFont.Small;
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect4, (shield.Energy * 100f).ToString("F0") + " / " + (shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f).ToString("F0"));
		Text.Anchor = TextAnchor.UpperLeft;
		return new GizmoResult(GizmoState.Clear);
	}
}