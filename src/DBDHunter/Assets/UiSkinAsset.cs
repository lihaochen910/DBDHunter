using Murder.Assets;
using Murder.Assets.Graphics;
using Murder.Attributes;
using System.Numerics;
using Murder.Core.Graphics;


namespace DBDHunter.Assets;

public class UiSkinAsset : GameAsset {

	public override string EditorFolder => "#\uf86dUi";

	public override char Icon => '\uf86d';

	public override Vector4 EditorColor => new ( 1f, .8f, .25f, 1f );
	
	public Color CamperNameColor = Color.White;
	public DrawInfo CamperNameDrawInfo = DrawInfo.Default;
	public Color CamperBoneColor = Color.Green;
	public bool DrawCamperBoundingBox = false;
	public Color CamperBoundingBoxColor = Color.Green;
	
	public Color SlasherNameColor = Color.Red;
	public DrawInfo SlasherNameDrawInfo = DrawInfo.Default;
	public Color SlasherBoneColor = Color.Red;
	public bool DrawSlasherBoundingBox = true;
	public Color SlasherBoundingBoxColor = Color.Red;
	
	public Color GeneratorNameColor = Color.Yellow;
	public DrawInfo GeneratorNameDrawInfo = DrawInfo.Default;
	public Color GeneratorBoneColor = Color.Yellow;

	public DrawInfo HatchNameDrawInfo = DrawInfo.Default;
	public Color HatchBoneColor = Color.Cyan;
	
	public DrawInfo TotemNameDrawInfo = DrawInfo.Default;
	public Color TotemBoneColor = Color.Gray;
	public Color TotemBoneHexColor = Color.Red;
	public Color TotemBoneBoonColor = Color.Red;

	public DrawInfo SearchableNameDrawInfo = DrawInfo.Default;
	public Color SearchableBoneColor = Color.Gray;

	public DrawInfo PalletNameDrawInfo = DrawInfo.Default;
	public Color PalletBoneColor = Color.Orange;

	public DrawInfo BearTrapNameDrawInfo = DrawInfo.Default;
	public Color BearTrapBoneColor = Color.Yellow;
}
