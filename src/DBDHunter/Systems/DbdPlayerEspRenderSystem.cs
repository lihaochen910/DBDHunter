using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Services;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Murder.Core.Graphics;
using Murder.Editor.Services;
using Murder.Services;
using Murder.Utilities;
using Color = Murder.Core.Graphics.Color;
using Vector2 = System.Numerics.Vector2;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( DbdActorComponent ) )]
public class DbdPlayerEspRenderSystem : IMurderRenderSystem {

	// private Vector2I _1080p = new ( 1920, 1080 );
	// private Vector2I _2k = new ( 2560, 1440 );
	// private Vector2I _4k = new ( 3840, 2160 );
	
	private const float CamperHeight = 75f;
	private const float SlasherHeight = 150f;
	
	private static double Distance( in Vector3D a, in Vector3D b ) {
		var diff = a - b;
		return Math.Sqrt( Math.Pow( diff.X, 2.0 ) + Math.Pow( diff.Y, 2.0 ) + Math.Pow( diff.Z, 2.0 ) );
	}

	
	public void Draw( RenderContext render, Context context ) {
		const float fontSize = 12f;
		const float fontScale = 1f;
		var drawInfo = new DrawInfo { Scale = Vector2.One * fontScale };

		var resolution = DBDHunterMurderGame.Profile.EspViewportSize;
		
		if ( context.World.TryGetUnique< DbdPlayerCameraManagerComponent >() is not {} playerCameraManagerComponent ) {
			render.GameUiBatch.DrawText( 103, "PlayerCameraManager not found!", new Vector2( 2, fontSize ), drawInfo );
			return;
		}
		else {
			render.GameUiBatch.DrawText( 103, $"PlayerCameraMgr: 0x{playerCameraManagerComponent.Addr:X8}", new Vector2( 2, fontSize ), drawInfo );
		}
		
		foreach ( var entity in context.Entities ) {
			var relativeLocation = entity.GetDbdActor().RelativeLocation;
			
			var cameraPos = new Vector3D(
				playerCameraManagerComponent.CameraEntry.POV.Location.X,
				playerCameraManagerComponent.CameraEntry.POV.Location.Y,
				playerCameraManagerComponent.CameraEntry.POV.Location.Z
			);
			
			var distance = Distance( in cameraPos, in relativeLocation ) / 39.62d - 6d;
			if ( distance < 0 ) {
				continue;
			}
			
			if ( entity.HasDbdPlayerRole() ) {
				DrawPlayerEsp( render, context, entity, in relativeLocation, distance, in playerCameraManagerComponent, resolution );
			}

			if ( entity.HasDbdGenerator() ) {
				DrawGeneratorEsp( render, entity, in relativeLocation, distance, in playerCameraManagerComponent, resolution );
			}
			
			if ( entity.HasDbdHatch() ) {
				DrawHatchEsp( render, entity, in relativeLocation, distance, in playerCameraManagerComponent, resolution );
			}

			if ( entity.HasDbdTotem() ) {
				DrawTotemEsp( render, entity, in relativeLocation, distance, in playerCameraManagerComponent, resolution );
			}
			
			if ( entity.HasDbdSearchable() ) {
				DrawSearchableEsp( render, entity, in relativeLocation, distance, in playerCameraManagerComponent, resolution );
			}
			
			if ( entity.HasDbdPallet() ) {
				DrawPalletEsp( render, entity, in relativeLocation, distance, in playerCameraManagerComponent, resolution );
			}
		}
	}
	
	private static Vector2 ProjectWorldToScreen( Vector3F world, in DbdPlayerCameraManagerComponent playerCameraManagerComponent ) {
		var screen = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			world,
			DBDHunterMurderGame.Profile.EspViewportSize
		);

		return new Vector2( ( float )screen.X, ( float )screen.Y );
	}
	
	private static float _arrowHeadSize = 0.2f;
	private static void DrawArrow( RenderContext render, Vector3F start, Vector3F end, Color color, float sort, in DbdPlayerCameraManagerComponent playerCameraManagerComponent ) {
		// TODO: Arrow could also be drawn as cylinder + cone, or with a preprepared model.
		Vector3F shaft = end - start;
		float length = shaft.Length;
		if ( Numeric.IsZero( length ) )
			return;
		
		render.GameplayBatch.DrawLine(
			ProjectWorldToScreen( start, in playerCameraManagerComponent ),
			ProjectWorldToScreen( end, in playerCameraManagerComponent ),
			color, sort );

		Vector3F shaftDirection = shaft / length;
		Vector3F normal = shaftDirection.Orthonormal1;
		render.GameplayBatch.DrawLine(
			ProjectWorldToScreen( end, in playerCameraManagerComponent ),
			ProjectWorldToScreen( end - _arrowHeadSize * shaft + normal * length * 0.05f, in playerCameraManagerComponent ),
			color, sort );
		render.GameplayBatch.DrawLine(
			ProjectWorldToScreen( end, in playerCameraManagerComponent ),
			ProjectWorldToScreen( end - _arrowHeadSize * shaft - normal * length * 0.05f, in playerCameraManagerComponent ),
			color, sort );
	}

	
	private static void DrawAxes( RenderContext render, Pose pose, float size, float sort, in DbdPlayerCameraManagerComponent playerCameraManagerComponent ) {
		DrawArrow( render, pose.Position, pose.Position + pose.ToWorldDirection( Vector3F.UnitX ) * size, Color.Red,
			sort, in playerCameraManagerComponent );
		DrawArrow( render, pose.Position, pose.Position + pose.ToWorldDirection( Vector3F.UnitY ) * size, Color.Green,
			sort, in playerCameraManagerComponent );
		DrawArrow( render, pose.Position, pose.Position + pose.ToWorldDirection( Vector3F.UnitZ ) * size, Color.Blue,
			sort, in playerCameraManagerComponent );
	}


	private static void DrawBoxShape( RenderContext render, BoxShape boxShape, Pose pose, Color color, float sort, in DbdPlayerCameraManagerComponent playerCameraManagerComponent ) {
		var aabb = boxShape.GetAabb( pose );
		// aabb.Scale( ( Vector3F )actorComponent.RelativeScale3D );
		
		var corner0 = ProjectWorldToScreen( new Vector3F( aabb.Minimum.X, aabb.Minimum.Y, aabb.Maximum.Z ), in playerCameraManagerComponent );
		var corner1 = ProjectWorldToScreen( new Vector3F( aabb.Maximum.X, aabb.Minimum.Y, aabb.Maximum.Z ), in playerCameraManagerComponent );
		var corner2 = ProjectWorldToScreen( aabb.Maximum, in playerCameraManagerComponent );
		var corner3 = ProjectWorldToScreen( new Vector3F( aabb.Minimum.X, aabb.Maximum.Y, aabb.Maximum.Z ), in playerCameraManagerComponent );
		var corner4 = ProjectWorldToScreen( aabb.Minimum, in playerCameraManagerComponent );
		var corner5 = ProjectWorldToScreen( new Vector3F( aabb.Maximum.X, aabb.Minimum.Y, aabb.Minimum.Z ), in playerCameraManagerComponent );
		var corner6 = ProjectWorldToScreen( new Vector3F( aabb.Maximum.X, aabb.Maximum.Y, aabb.Minimum.Z ), in playerCameraManagerComponent );
		var corner7 = ProjectWorldToScreen( new Vector3F( aabb.Minimum.X, aabb.Maximum.Y, aabb.Minimum.Z ), in playerCameraManagerComponent );
		
		render.GameplayBatch.DrawLine( corner0, corner1, color, sort );
		render.GameplayBatch.DrawLine( corner1, corner2, color, sort );
		render.GameplayBatch.DrawLine( corner2, corner3, color, sort );
		render.GameplayBatch.DrawLine( corner0, corner3, color, sort );
		render.GameplayBatch.DrawLine( corner4, corner5, color, sort );
		render.GameplayBatch.DrawLine( corner5, corner6, color, sort );
		render.GameplayBatch.DrawLine( corner6, corner7, color, sort );
		render.GameplayBatch.DrawLine( corner7, corner4, color, sort );
		render.GameplayBatch.DrawLine( corner0, corner4, color, sort );
		render.GameplayBatch.DrawLine( corner1, corner5, color, sort );
		render.GameplayBatch.DrawLine( corner2, corner6, color, sort );
		render.GameplayBatch.DrawLine( corner3, corner7, color, sort );
	}
	
	private BoxShape _camperBoxShape = new ( new Vector3F( 20f, 30f, 80f ) );
	private BoxShape _slasherBoxShape = new ( new Vector3F( 50f, 50f, 180f ) );
	private void DrawPlayerEsp( RenderContext render, Context context, Entity entity, in Vector3D relativeLocation, double distance, in DbdPlayerCameraManagerComponent playerCameraManagerComponent, Vector2I resolution ) {
		
		var uiSkinAsset = LibraryServices.GetUiSkin();
		
		var playerRole = entity.GetDbdPlayerRole().Role;
		var playerHeadRelativeLocation = relativeLocation + new Vector3D( 0, 0, playerRole is EPlayerRole.VE_Camper ? CamperHeight : SlasherHeight );
		
		DrawInfo playerEspDrawInfo = default;
		switch ( playerRole ) {
			case EPlayerRole.VE_Slasher:
				playerEspDrawInfo = uiSkinAsset.SlasherNameDrawInfo;
				break;
			case EPlayerRole.VE_Camper:
				playerEspDrawInfo = uiSkinAsset.CamperNameDrawInfo;
				break;
		}
			
		var playerScreenPos = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			relativeLocation,
			resolution
		);
			
		var playerHeadScreenPos = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			playerHeadRelativeLocation,
			resolution
		);
		
		 // Draw Border
		/*
			struct FBoxSphereBounds
			{
				struct FVector Origin;
				struct FVector BoxExtent;
				float SphereRadius;
			};
			 
			FVector2D Origin2D, BoxMin2D, BoxMax2D;
			auto Bounds = *(FBoxSphereBounds*)(pMesh + 0x654); // CachedWorldSpaceBounds
			auto BoxMin = Bounds.Origin - (Bounds.BoxExtent / 3); // bottom
			auto BoxMax = Bounds.Origin + (Bounds.BoxExtent / 3); // top
			 
			if (WorldToScreen(PlayerController, Bounds.Origin, Origin2D) &&
			    WorldToScreen(PlayerController, BoxMin, BoxMin2D) &&
			    WorldToScreen(PlayerController, BoxMax, BoxMax2D))
			{
				auto Width = fabs((BoxMax2D.Y - BoxMin2D.Y) / 4);
				BoxMin2D.X = Origin2D.X - Width;
				BoxMax2D.X = Origin2D.X + Width;
			 
				DrawBorder(BoxMax2D.X, BoxMax2D.Y, BoxMin2D.X - BoxMax2D.X, BoxMin2D.Y - BoxMax2D.Y, Color);
			}
		 */
		// if ( entity.TryGetDbdActorBoxShape() is {} dbdActorBoxShapeComponent ) {
		// 	var actorComponent = entity.GetDbdActor();
		// 	var pose = new PoseD( actorComponent.RelativeLocation );
		// 	// pose.Orientation = Matrix33D.CreateRotationZ( actorComponent.RelativeRotation.Z ) *
		// 	// 				   Matrix33D.CreateRotationX( actorComponent.RelativeRotation.X ) *
		// 	// 				   Matrix33D.CreateRotationY( actorComponent.RelativeRotation.Y );
		//
		// 	// DrawAxes( render, ( Pose )pose, 10f, 1f, in playerCameraManagerComponent );
		// 	
		// 	var color = playerRole is EPlayerRole.VE_Slasher
		// 		? uiSkinAsset.SlasherBoneColor
		// 		: uiSkinAsset.CamperBoneColor;
		// 	// var aabb = dbdActorBoxShapeComponent.BoxShape.GetAabb( ( Pose )pose );
		// 	DrawBoxShape( render, playerRole is EPlayerRole.VE_Slasher ? _slasherBoxShape : _camperBoxShape, ( Pose )pose, color, 1f, in playerCameraManagerComponent );
		// }

		// TODO:
		/*
			Matrix4x4 ComponentMatrix = ToMatrixWithScale(WorldToComponent.Translation, WorldToComponent.Rotation, WorldToComponent.Scale3D);
			Matrix4x4 BoneMatrix = ToMatrixWithScale(BoneTrans, BoneRot, BoneScale);
			Matrix4x4 FinalPos = BoneMatrix * ComponentMatrix;
			 
			Vector3 BoneLocation = new Vector3
			{
			   X = FinalPos.Translation.X,
			   Y = FinalPos.Translation.Y,
			   Z = FinalPos.Translation.Z
			};
		*/
		if ( entity.TryGetDbdActorMesh() is {} dbdActorMeshComponent &&
			 // entity.TryGetDbdActorMeshSockets() is {} dbdActorMeshSocketsComponent &&
			 entity.TryGetDbdActorMeshCachedBoneSpaceTransforms() is {} dbdActorMeshCachedBoneSpaceTransforms &&
			 entity.TryGetDbdActorMeshRefSkeleton() is {} dbdActorMeshRefSkeleton &&
			 entity.TryGetDbdActorSkeletonJointConnectionDefine() is {} dbdActorSkeletonJointConnectionDefine ) {
			
			bool MaybeThisBoneICare( FName boneName ) {
				var name = boneName.ToString();
				// return name.Contains( "HipMaster" ) ||
				// 	   // name.Contains( "HipRT" ) ||
				// 	   // name.Contains( "Torso" ) ||
				// 	   // name.Contains( "Neck" ) ||
				// 	   // name.Contains( "Elbow" ) ||
				// 	   // name.Contains( "Foot" ) ||
				// 	   // name.Contains( "Head" ) ||
				// 	   // name.Contains( "Hand" ) ||
				// 	   // name.Contains( "Knee" ) ||
				// 	   // name.Contains( "Shoulder" ) ||
				// 	   // name.Contains( "Pelvis" ) ||
				// 	   name.Contains( "Char" );
				// return !name.Contains( "HipMaster" ) &&
				// 	   !name.Contains( "HipRT" ) &&
				// 	   !name.Contains( "Torso" ) &&
				// 	   !name.Contains( "Neck" ) &&
				// 	   !name.Contains( "Elbow" ) &&
				// 	   // name.Contains( "Foot" ) &&
				// 	   !name.Contains( "Head" ) &&
				// 	   name.Contains( "Hand" ) &&
				// 	   !name.Contains( "Knee" ) &&
				// 	   !name.Contains( "Shoulder" ) &&
				// 	   !name.Contains( "Pelvis" ) &&
				// 	   !name.Contains( "Char" );
				return true;
			}

			var componentToWorldMatrix = dbdActorMeshComponent.ActorMeshComponentToWorld.ToMatrixWithScale();
			
			foreach ( var jointConnection in dbdActorSkeletonJointConnectionDefine.Connections ) {
				// var meshBoneInfoA = dbdActorMeshRefSkeleton.RawRefBoneInfo[ jointConnection.BoneA ];
				if ( jointConnection.BoneA < 0 || jointConnection.BoneA >=
					 dbdActorMeshCachedBoneSpaceTransforms.CachedBoneSpaceTransformsData.Length ) {
					return;
				}
				var boneATransform = dbdActorMeshCachedBoneSpaceTransforms.CachedBoneSpaceTransformsData[ jointConnection.BoneA ];
				var boneAMatrix = boneATransform.ToMatrixWithScale();
				var boneAInWorld = FMatrix.D3DXMatrixMultiply( in boneAMatrix, in componentToWorldMatrix );
				
				var boneAInScreen = UEViewMatrix.WorldToScreen(
					playerCameraManagerComponent.CameraEntry.POV,
					boneAInWorld.Translation,
					resolution
				);
				
				// var meshBoneInfoB = dbdActorMeshRefSkeleton.RawRefBoneInfo[ jointConnection.BoneB ];
				if ( jointConnection.BoneB < 0 ||  jointConnection.BoneB >=
					dbdActorMeshCachedBoneSpaceTransforms.CachedBoneSpaceTransformsData.Length ) {
					return;
				}
				var boneBTransform = dbdActorMeshCachedBoneSpaceTransforms.CachedBoneSpaceTransformsData[ jointConnection.BoneB ];
				var boneBMatrix = boneBTransform.ToMatrixWithScale();
				var boneBInWorld = FMatrix.D3DXMatrixMultiply( in boneBMatrix, in componentToWorldMatrix );
				
				var boneBInScreen = UEViewMatrix.WorldToScreen(
					playerCameraManagerComponent.CameraEntry.POV,
					boneBInWorld.Translation,
					resolution
				);
				
				render.GameplayBatch.DrawLine( 
					new Vector2( ( float )boneAInScreen.X, ( float )boneAInScreen.Y ).ToPoint(),
					new Vector2( ( float )boneBInScreen.X, ( float )boneBInScreen.Y ).ToPoint(),
					playerRole is EPlayerRole.VE_Slasher ? uiSkinAsset.SlasherBoneColor : uiSkinAsset.CamperBoneColor,
					1f
				);
			}

			// debug: boneName
			// for ( var boneIndex = 0; boneIndex < dbdActorMeshRefSkeleton.RawRefBoneInfo.Length; boneIndex++ ) {
			// 	var meshBoneInfo = dbdActorMeshRefSkeleton.RawRefBoneInfo[ boneIndex ];
			//
			// 	if ( !MaybeThisBoneICare( meshBoneInfo.Name ) ) {
			// 		continue;
			// 	}
			// 	
			// 	var boneFTransform = dbdActorMeshCachedBoneSpaceTransforms.CachedBoneSpaceTransformsData[ boneIndex ];
			// 	var boneMatrix = boneFTransform.ToMatrixWithScale();
			// 	var final = FMatrix.D3DXMatrixMultiply( in boneMatrix, in componentToWorldMatrix );
			// 	
			// 	var boneInWorld = final.Translation;
			// 		
			// 	var boneInScreen = UEViewMatrix.WorldToScreen(
			// 		playerCameraManagerComponent.CameraEntry.POV,
			// 		boneInWorld,
			// 		resolution
			// 	);
			//
			// 	var bonePoint = new Vector2( ( float )boneInScreen.X, ( float )boneInScreen.Y );
			// 	var cursorPoint = new Vector2( Murder.Game.Input.CursorPosition.X, Murder.Game.Input.CursorPosition.Y );
			//
			// 	var cursorToBoneDistance = Vector2.Distance( bonePoint, cursorPoint );
			// 	if ( Math.Abs( cursorToBoneDistance ) < 10f ) {
			// 		render.GameUiBatch.DrawText( 103, $"{meshBoneInfo.Name.ToString()}", new Vector2( ( float )boneInScreen.X, ( float )boneInScreen.Y ),
			// 			new DrawInfo( Color.White, 1f ) {
			// 				Scale = Vector2.One * Calculator.Remap( ( float )distance, 0f, 200f, 2f, 2f )
			// 				// Scale = Vector2.One * 2f
			// 			} );
			// 	}
			// 	
			// 	render.GameplayBatch.DrawPoint( 
			// 		new Vector2( ( float )boneInScreen.X, ( float )boneInScreen.Y ).ToPoint(),
			// 		playerRole is EPlayerRole.VE_Slasher ? uiSkinAsset.SlasherBoneColor : uiSkinAsset.CamperBoneColor,
			// 		1f
			// 	);
			// }
			
		}

		render.GameUiBatch.DrawText( MurderFonts.PixelFont, $"{distance:0.00}m", new Vector2( ( float )playerHeadScreenPos.X, ( float )playerHeadScreenPos.Y ), playerEspDrawInfo );
		// render.GameUiBatch.DrawText( MurderFonts.PixelFont, $"{playerRole}", new Vector2( ( float )playerHeadScreenPos.X, ( float )playerHeadScreenPos.Y ), playerEspDrawInfo );
		
	}


	private BoxShape _generatorBoxShape = new ( new Vector3F( 20f, 30f, 60f ) );
	private void DrawGeneratorEsp( RenderContext render, Entity entity, in Vector3D relativeLocation,
								   double distance, in DbdPlayerCameraManagerComponent playerCameraManagerComponent,
								   Vector2I resolution ) {
		
		var generatorComponent = entity.GetDbdGenerator();
		if ( generatorComponent.IsDone ) {
			return;
		}
		
		var uiSkinAsset = LibraryServices.GetUiSkin();
		
		var generatorScreenPos = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			relativeLocation,
			resolution
		);
		
		render.GameUiBatch.DrawText( MurderFonts.PixelFont, $"generator {generatorComponent.NativePercentComplete:0.00}", new Vector2( ( float )generatorScreenPos.X, ( float )generatorScreenPos.Y ), uiSkinAsset.GeneratorNameDrawInfo );
			
		var actorComponent = entity.GetDbdActor();
		var pose = new PoseD( actorComponent.RelativeLocation + new Vector3D( 0, 0, _generatorBoxShape.WidthZ / 2d ) );
		pose.Orientation = Matrix33D.CreateRotationZ( actorComponent.RelativeRotation.Z ) *
						   Matrix33D.CreateRotationX( actorComponent.RelativeRotation.X ) *
						   Matrix33D.CreateRotationY( actorComponent.RelativeRotation.Y );
		
		DrawBoxShape( render, _generatorBoxShape, ( Pose )pose, uiSkinAsset.GeneratorBoneColor, 1f, in playerCameraManagerComponent );
	}
	
	
	private BoxShape _hatchBoxShape = new ( new Vector3F( 40f, 40f, 10f ) );
	private void DrawHatchEsp( RenderContext render, Entity entity, in Vector3D relativeLocation,
							   double distance, in DbdPlayerCameraManagerComponent playerCameraManagerComponent,
								Vector2I resolution ) {
		
		var hatchComponent = entity.GetDbdHatch();
		
		var uiSkinAsset = LibraryServices.GetUiSkin();
		
		var hatchScreenPos = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			relativeLocation,
			resolution
		);
		
		render.GameUiBatch.DrawText( MurderFonts.PixelFont, $"Hatch - {hatchComponent.State} ({distance:0.0}m)", new Vector2( ( float )hatchScreenPos.X, ( float )hatchScreenPos.Y ), uiSkinAsset.HatchNameDrawInfo );
		
		var actorComponent = entity.GetDbdActor();
		var pose = new PoseD( actorComponent.RelativeLocation );
		// pose.Orientation = Matrix33D.CreateRotationZ( actorComponent.RelativeRotation.Z ) *
		// 				   Matrix33D.CreateRotationX( actorComponent.RelativeRotation.X ) *
		// 				   Matrix33D.CreateRotationY( actorComponent.RelativeRotation.Y );
		
		DrawBoxShape( render, _hatchBoxShape, ( Pose )pose, uiSkinAsset.HatchBoneColor, 1f, in playerCameraManagerComponent );
	}
	
	
	private BoxShape _totemShape = new ( new Vector3F( 15f, 20f, 30f ) );
	private void DrawTotemEsp( RenderContext render, Entity entity, in Vector3D relativeLocation,
							   double distance, in DbdPlayerCameraManagerComponent playerCameraManagerComponent,
							   Vector2I resolution ) {
		
		var totemComponent = entity.GetDbdTotem();
		if ( totemComponent.State is ETotemState.Cleansed ) {
			return;
		}
		
		var uiSkinAsset = LibraryServices.GetUiSkin();
		
		var totemScreenPos = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			relativeLocation,
			resolution
		);
		
		render.GameUiBatch.DrawText( MurderFonts.PixelFont, $"Totem - {totemComponent.State} ({distance:0.0}m)", new Vector2( ( float )totemScreenPos.X, ( float )totemScreenPos.Y ), uiSkinAsset.TotemNameDrawInfo );
		
		var actorComponent = entity.GetDbdActor();
		var pose = new PoseD( actorComponent.RelativeLocation );
		switch ( totemComponent.State ) {
			case ETotemState.Boon:
				DrawBoxShape( render, _totemShape, ( Pose )pose, uiSkinAsset.TotemBoneBoonColor, 1f, in playerCameraManagerComponent );
				break;
			case ETotemState.Hex:
				DrawBoxShape( render, _totemShape, ( Pose )pose, uiSkinAsset.TotemBoneHexColor, 1f, in playerCameraManagerComponent );
				break;
			default:
				DrawBoxShape( render, _totemShape, ( Pose )pose, uiSkinAsset.TotemBoneColor, 1f, in playerCameraManagerComponent );
				break;
		}
		
	}
	
	
	private BoxShape _searchableShape = new ( new Vector3F( 30f, 30f, 10f ) );
	private void DrawSearchableEsp( RenderContext render, Entity entity, in Vector3D relativeLocation,
									double distance, in DbdPlayerCameraManagerComponent playerCameraManagerComponent,
									Vector2I resolution ) {
		
		var searchableComponent = entity.GetDbdSearchable();
		
		var uiSkinAsset = LibraryServices.GetUiSkin();
		
		var searchableScreenPos = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			relativeLocation,
			resolution
		);
		
		render.GameUiBatch.DrawText( MurderFonts.PixelFont, $"Chest ({distance:0.0}m)", new Vector2( ( float )searchableScreenPos.X, ( float )searchableScreenPos.Y ), uiSkinAsset.SearchableNameDrawInfo );
		
		var actorComponent = entity.GetDbdActor();
		var pose = new PoseD( actorComponent.RelativeLocation );
		DrawBoxShape( render, _searchableShape, ( Pose )pose, uiSkinAsset.SearchableBoneColor, 1f, in playerCameraManagerComponent );
	}
	
	
	private void DrawPalletEsp( RenderContext render, Entity entity, in Vector3D relativeLocation,
								double distance, in DbdPlayerCameraManagerComponent playerCameraManagerComponent,
								Vector2I resolution ) {
		
		var palletComponent = entity.GetDbdPallet();
		if ( palletComponent.State != EPalletState.Up &&
			 palletComponent.State is EPalletState.Fallen &&
			 palletComponent.State is EPalletState.Falling ) {
			return;
		}
		
		var uiSkinAsset = LibraryServices.GetUiSkin();
		
		var palletScreenPos = UEViewMatrix.WorldToScreen(
			playerCameraManagerComponent.CameraEntry.POV,
			relativeLocation,
			resolution
		);
		
		render.GameUiBatch.DrawText( MurderFonts.PixelFont, $"Pallet - {palletComponent.State} ({distance:0.0}m)", new Vector2( ( float )palletScreenPos.X, ( float )palletScreenPos.Y ), uiSkinAsset.PalletNameDrawInfo );
	}
}
