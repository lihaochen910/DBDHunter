using System.Runtime.InteropServices;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DBDHunter.Components;
using DBDHunter.Utilities;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Murder;
using vmmsharp;


namespace DBDHunter.Systems; 

[Filter( ContextAccessorFilter.AnyOf, typeof( DbdActorComponent ) )]
internal class DbdActorRefreshSystem : IStartupSystem, IExitSystem, IUpdateSystem {

	private const float RefreshTime = 10f / 1000f;
	private float _timer;
	
	public void Start( Context context ) {
		
	}

	public void Exit( Context context ) {
		
	}
	
	public void Update( Context context ) {
		if ( context.Entities.Length < 1 ) {
			return;
		}

		_timer -= Game.DeltaTime;
		if ( _timer < 0f ) {
			DoRefresh( context );
			_timer = RefreshTime;
		}
	}

	private void DoRefresh( Context context ) {
		var handle = Driver.CreateScatterHandle();
		var secondPassHandle = Driver.CreateScatterHandle();
		var thirdPassHandle = Driver.CreateScatterHandle();
		
		// prepare RootComponent props / MeshComponent props
		foreach ( var entity in context.Entities ) {
			handle.Prepare< UEVector >( entity.GetDbdActor().RootComponent + Offsets.USceneComponent_RelativeLocation );
			handle.Prepare< UERotator >( entity.GetDbdActor().RootComponent + Offsets.USceneComponent_RelativeRotation );
			handle.Prepare< UEVector >( entity.GetDbdActor().RootComponent + Offsets.USceneComponent_RelativeScale3D );

			if ( entity.TryGetDbdActorMesh() is {} dbdActorMeshComponent ) {
				handle.Prepare< ulong >( dbdActorMeshComponent.ActorAddr + Offsets.ACharacter_Mesh );
				handle.Prepare< ulong >( dbdActorMeshComponent.ActorMeshAddr + Offsets.USkinnedMeshComponent_SkeletalMesh );
				handle.Prepare< FBoxSphereBounds >( dbdActorMeshComponent.ActorMeshAddr + Offsets.USkinnedMeshComponent_CachedWorldOrLocalSpaceBounds );
				handle.Prepare< ulong >( dbdActorMeshComponent.ActorMeshAddr + Offsets.USkeletalMeshComponent_ComponentSpaceTransformsArray );
				handle.Prepare< int >( dbdActorMeshComponent.ActorMeshAddr + Offsets.USkeletalMeshComponent_ComponentSpaceTransformsArray + Offsets.TArray_NumElements );
			}
		}

		handle.Execute();
		
		// read RootComponent props / MeshComponent 
		foreach ( var entity in context.Entities ) {
			var relativeLocation = handle.Read< UEVector >( entity.GetDbdActor().RootComponent + Offsets.USceneComponent_RelativeLocation );
			var relativeRotation = handle.Read< UERotator >( entity.GetDbdActor().RootComponent + Offsets.USceneComponent_RelativeRotation );
			var relativeScale3d = handle.Read< UEVector >( entity.GetDbdActor().RootComponent + Offsets.USceneComponent_RelativeScale3D );
			entity.SetDbdActor(
				entity.GetDbdActor().ActorAddr,
				entity.GetDbdActor().RootComponent,
				new Vector3D( relativeLocation.X, relativeLocation.Y, relativeLocation.Z ),
				new Vector3D( relativeRotation.Pitch, relativeRotation.Yaw, relativeRotation.Roll ),
				new Vector3D( relativeScale3d.X, relativeScale3d.Y, relativeScale3d.Z )
			);
			
			if ( entity.TryGetDbdActorMesh() is {} dbdActorMeshComponent ) {

				// Sockets数据很大, 如果已经初始化则跳过, 节省读取
				var actorMeshSocketsDataAlreadyCreated = false;
				if ( entity.TryGetDbdActorMeshSockets() is {} dbdActorMeshSocketsComponent ) {
					if ( !dbdActorMeshSocketsComponent.Sockets.IsDefaultOrEmpty &&
						 !dbdActorMeshSocketsComponent.SocketsData.IsDefaultOrEmpty ) {
						actorMeshSocketsDataAlreadyCreated = true;
					}
				}
				
				var actorMeshRefSkeletonAlreadyCreated = entity.HasDbdActorMeshRefSkeleton();

				FTransform meshComponentToWorld = default;
				var meshComponent = handle.Read< ulong >( dbdActorMeshComponent.ActorAddr + Offsets.ACharacter_Mesh );
				if ( meshComponent != 0 ) {
					// debug:
					// Memory.PrintByteArray( Memory.ReadTArray< byte >( meshComponent + 0x01AD, 0x0113 ), addressHint: 0x01AD );
					// Memory.PrintStructureByteArray(Vector3D.One);
					// Marshal.SizeOf< FTransform >(); // 0x60
					// secondPassHandle.Prepare< FTransform >( meshComponent + Offsets.USceneComponent_ComponentToWorld );
					meshComponentToWorld = Memory.Read< FTransform >( meshComponent + Offsets.USceneComponent_ComponentToWorld );
				}
				
				var skeletalMesh = handle.Read< ulong >( meshComponent + Offsets.USkinnedMeshComponent_SkeletalMesh );
				var cachedBoneSpaceTransforms = handle.Read< ulong >( meshComponent + Offsets.USkeletalMeshComponent_ComponentSpaceTransformsArray );
				var cachedBoneSpaceTransformsNum = handle.Read< int >( meshComponent + Offsets.USkeletalMeshComponent_ComponentSpaceTransformsArray + Offsets.TArray_NumElements );
				if ( cachedBoneSpaceTransformsNum > 0xFF ) {
					cachedBoneSpaceTransformsNum = 0;
				}
				
				if ( context.World.TryGetUnique< DbdGNamesComponent >() is {} dbdGNamesComponent ) {
					meshComponent.IsA( Offsets.CLASS_SkinnedMeshComponent, dbdGNamesComponent.Addr );
					skeletalMesh.IsA( Offsets.CLASS_SkeletalMesh, dbdGNamesComponent.Addr );
				}

				if ( skeletalMesh != 0 ) {
					if ( !actorMeshSocketsDataAlreadyCreated ) {
						var skeleton = Memory.Read< ulong >( skeletalMesh + Offsets.USkeletalMesh_Skeleton );
						if ( skeleton != 0 ) {
							secondPassHandle.Prepare< ulong >( skeleton + Offsets.USkeleton_Sockets );
							secondPassHandle.Prepare< int >( skeleton + Offsets.USkeleton_Sockets + Offsets.TArray_NumElements );
						}
					}

					if ( !actorMeshRefSkeletonAlreadyCreated ) {
						// debug: Memory.PrintByteArray(Memory.ReadTArray<byte>(skeletalMesh+Offsets.USkeletalMesh_RefSkeleton, 0x178));
						var rawRefBoneInfo = Memory.ReadUETArray< FMeshBoneInfo >( skeletalMesh + Offsets.USkeletalMesh_RefSkeleton + Offsets.FReferenceSkeleton_RawRefBoneInfo );
						if ( rawRefBoneInfo is { Length: > 0 } ) {
							entity.SetDbdActorMeshRefSkeleton( [..rawRefBoneInfo] );
						}
					}
				}
				
				if (cachedBoneSpaceTransforms != 0 && cachedBoneSpaceTransformsNum is > 0 and < 0xFF ) {
					secondPassHandle.Prepare( cachedBoneSpaceTransforms, ( uint )( Marshal.SizeOf< FTransform >() * cachedBoneSpaceTransformsNum ) );
				}
				
				entity.SetDbdActorMesh(
					dbdActorMeshComponent.ActorAddr,
					meshComponent,
					meshComponentToWorld,
					skeletalMesh,
					cachedBoneSpaceTransforms,
					cachedBoneSpaceTransformsNum
				);
				
				var cachedWorldOrLocalSpaceBounds = handle.Read< FBoxSphereBounds >( dbdActorMeshComponent.ActorMeshAddr + Offsets.USkinnedMeshComponent_CachedWorldOrLocalSpaceBounds );
				var boxExtent = cachedWorldOrLocalSpaceBounds.BoxExtent;
				var origin = cachedWorldOrLocalSpaceBounds.Origin;

				if ( entity.TryGetDbdActorBoxShape() is {} dbdActorBoxShapeComponent ) {
					var boxShape = dbdActorBoxShapeComponent.BoxShape;
					try {
						boxShape.Extent = new Vector3F( ( float )boxExtent.X, ( float )boxExtent.Y, ( float )boxExtent.Z );
					}
					catch ( ArgumentOutOfRangeException ) {}
					entity.SetDbdActorBoxShape(
						boxShape,
						new Vector3F( ( float )origin.X, ( float )origin.Y, ( float )origin.Z ),
						( float )cachedWorldOrLocalSpaceBounds.SphereRadius
					);
				}
				else {
					var boxShape = new BoxShape();
					try {
						boxShape.Extent = new Vector3F( ( float )boxExtent.X, ( float )boxExtent.Y, ( float )boxExtent.Z );
					}
					catch ( ArgumentOutOfRangeException ) {}
					entity.SetDbdActorBoxShape(
						boxShape,
						new Vector3F( ( float )origin.X, ( float )origin.Y, ( float )origin.Z ),
						( float )cachedWorldOrLocalSpaceBounds.SphereRadius
					);
				}
			}
		}

		secondPassHandle.Execute();

		// read CachedBoneSpaceTransforms
		foreach ( var entity in context.Entities ) {
			if ( entity.TryGetDbdActorMesh() is {} dbdActorMeshComponent ) {
				
				if (  dbdActorMeshComponent.SkeletalMesh != 0 ) {
					var skeleton = Memory.Read< ulong >( dbdActorMeshComponent.SkeletalMesh + Offsets.USkeletalMesh_Skeleton );
					if ( skeleton != 0 ) {
						var sockets = secondPassHandle.Read< ulong >( skeleton + Offsets.USkeleton_Sockets );
						var socketsNum = secondPassHandle.Read< int >( skeleton + Offsets.USkeleton_Sockets + Offsets.TArray_NumElements );
						
						// Sockets数据很大, 如果已经初始化则跳过, 节省读取
						var actorMeshSocketsDataAlreadyCreated = false;
						if ( entity.TryGetDbdActorMeshSockets() is {} dbdActorMeshSocketsComponent ) {
							if ( !dbdActorMeshSocketsComponent.Sockets.IsDefaultOrEmpty &&
								 !dbdActorMeshSocketsComponent.SocketsData.IsDefaultOrEmpty ) {
								actorMeshSocketsDataAlreadyCreated = true;
							}
						}
						
						if ( !actorMeshSocketsDataAlreadyCreated && sockets != 0 && socketsNum is > 0 and < 1000 ) {
							// thirdPassHandle.Prepare( Memory.Read< ulong >( sockets ),
							// 	( uint )( sizeof( ulong ) * socketsNum ) );
							thirdPassHandle.Prepare( sockets, ( uint )( sizeof( ulong ) * socketsNum ) );
						}
						
					}
					
					// var refSkeleton = secondPassHandle.Read< ulong >( dbdActorMeshComponent.SkeletalMesh + Offsets.USkeletalMesh_RefSkeleton );
					// if ( refSkeleton != 0 ) {
					// 	
					// }
				}

				if ( dbdActorMeshComponent.CachedBoneSpaceTransforms != 0 &&
					 dbdActorMeshComponent.CachedBoneSpaceTransformsNumElements is > 0 and < 0xFF ) {
					
					FTransform[] cachedBoneSpaceTransforms = default;
					cachedBoneSpaceTransforms = secondPassHandle.ReadTArray< FTransform >(
						dbdActorMeshComponent.CachedBoneSpaceTransforms,
						dbdActorMeshComponent.CachedBoneSpaceTransformsNumElements );
					
					if ( cachedBoneSpaceTransforms != null ) {
						entity.SetDbdActorMeshCachedBoneSpaceTransforms( [..cachedBoneSpaceTransforms] );
					}
				}
			}
		}

		thirdPassHandle.Execute();
		
		// read Sockets
		foreach ( var entity in context.Entities ) {
			if ( entity.TryGetDbdActorMesh() is {} dbdActorMeshComponent ) {
				
				if ( entity.TryGetDbdActorMeshSockets() is {} dbdActorMeshSocketsComponent ) {
					if ( !dbdActorMeshSocketsComponent.Sockets.IsDefaultOrEmpty &&
						 !dbdActorMeshSocketsComponent.SocketsData.IsDefaultOrEmpty ) {
						continue;
					}
				}
				
				var skeleton = Memory.Read< ulong >( dbdActorMeshComponent.SkeletalMesh + Offsets.USkeletalMesh_Skeleton );
				if ( skeleton != 0 ) {
					var sockets = secondPassHandle.Read< ulong >( skeleton + Offsets.USkeleton_Sockets );
					var socketsNum = secondPassHandle.Read< int >( skeleton + Offsets.USkeleton_Sockets + Offsets.TArray_NumElements );
			
					ulong[] socketsArray = default;
					if ( sockets != 0 && socketsNum is > 0 and < 1000 ) {
						socketsArray = thirdPassHandle.ReadTArray< ulong >( sockets, socketsNum );
					}
					
					if ( socketsArray != null ) {
						entity.SetDbdActorMeshSockets( [..socketsArray], ImmutableArray< USkeletalMeshSocket >.Empty );
					}
				}
			}
		}

		var lastPassHandle = Driver.CreateScatterHandle();
		
		// for DbdActorMeshSocketsComponent
		foreach ( var entity in context.Entities ) {
			if ( entity.TryGetDbdActorMeshSockets() is {} dbdActorMeshSocketsComponent &&
				 !dbdActorMeshSocketsComponent.Sockets.IsDefaultOrEmpty &&
				 dbdActorMeshSocketsComponent.SocketsData.IsDefaultOrEmpty ) {
				foreach ( var socket in dbdActorMeshSocketsComponent.Sockets ) {
					lastPassHandle.Prepare< FName >( socket + Offsets.USkeletalMeshSocket_SocketName );
					lastPassHandle.Prepare< FName >( socket + Offsets.USkeletalMeshSocket_BoneName );
					lastPassHandle.Prepare< UEVector >( socket + Offsets.USkeletalMeshSocket_RelativeLocation );
					lastPassHandle.Prepare< UERotator >( socket + Offsets.USkeletalMeshSocket_RelativeRotation );
					lastPassHandle.Prepare< UEVector >( socket + Offsets.USkeletalMeshSocket_RelativeScale );
				}
			}
		}
		
		lastPassHandle.Execute();
		
		foreach ( var entity in context.Entities ) {
			if ( entity.TryGetDbdActorMeshSockets() is {} dbdActorMeshSocketsComponent &&
				 !dbdActorMeshSocketsComponent.Sockets.IsDefaultOrEmpty &&
				 dbdActorMeshSocketsComponent.SocketsData.IsDefaultOrEmpty ) {
				var socketDataList = new List< USkeletalMeshSocket >( dbdActorMeshSocketsComponent.Sockets.Length );
				foreach ( var socket in dbdActorMeshSocketsComponent.Sockets ) {
					socketDataList.Add( new USkeletalMeshSocket {
						SocketName = lastPassHandle.Read< FName >( socket + Offsets.USkeletalMeshSocket_SocketName ),
						BoneName = lastPassHandle.Read< FName >( socket + Offsets.USkeletalMeshSocket_BoneName ),
						RelativeLocation = lastPassHandle.Read< UEVector >( socket + Offsets.USkeletalMeshSocket_RelativeLocation ),
						RelativeRotation = lastPassHandle.Read< UERotator >( socket + Offsets.USkeletalMeshSocket_RelativeRotation ),
						RelativeScale = lastPassHandle.Read< UEVector >( socket + Offsets.USkeletalMeshSocket_RelativeScale )
					} );
				}
				entity.SetDbdActorMeshSockets( dbdActorMeshSocketsComponent.Sockets, [..socketDataList] );
			}
		}
		
		lastPassHandle.SafeClose();
		
		thirdPassHandle.SafeClose();
		secondPassHandle.SafeClose();
		handle.SafeClose();
	}
	
}
