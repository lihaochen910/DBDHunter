using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DBDHunter.Utilities;
using DigitalRune.Collections;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DBDHunter;

public enum GamePlatform : byte {
	Unknown = 0,
	Steam = 1,
	Epic
}

public static class GameProcessName {
	public const string ProcessName_Steam = "DeadByDaylight-Win64-Shipping.exe";
	public const string ProcessName_Epic = "DeadByDaylight-EGS-Shipping.exe";
}

public static class Offsets {

	// UE4
	// public const string Pattern_GWorld = "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 3B 41 B0 01 33 D2 48 8B CB E8";
	// public const string Pattern_FNames = "4C 8D 0D ?? ?? ?? ?? EB 1E 48 8D 0D";
	// public const string Pattern_GObjects = "48 8B 05 ?? ?? ?? ?? 48 8B 0C C8 48 8D 04 D1 EB";
	
	// UE5
	public const string Pattern_GWorld = "48 8B 3D ?? ?? ?? ?? 48 8B C7";
	public const string Pattern_FNames = "4C 8B F8 C6 05 ?? ?? ?? ?? 01"; // 往上看lea指令
	public const string Pattern_GObjects = "48 8D 14 40 48 8B 05 ?? ?? ?? ?? 48 8B 0C C8";
	
	public const string NullAddress = "0x00000000";
	
	// 7.5.2
	// public const string GWorld_Steam = "0x0E2A7E60";
	// public const string FNames_Steam = "0xE08CE40";
	// public const string GWorld_Epic = "0xDB6B410";
	// public const string FNames_Epic = "0xD965400";
	
	// 7.6.0
	// public const string GWorld_Steam = "0xEF98120";
	// public const string FNames_Steam = "0xED7D140";
	// public const string GWorld_Epic = "0xE8466E0";
	// public const string FNames_Epic = "0xE640700";
	
	// 7.6.1
	// public const string GWorld_Steam = "0xEFA6190";
	// public const string FNames_Steam = "0xED8B180";
	// public const string GWorld_Epic = "0xE854860";
	// public const string FNames_Epic = "0xE64E840";
	
	// 7.6.2
	// public const string GWorld_Steam = "0xEF93D30";
	// public const string FNames_Steam = "0xED78D00";
	// public const string GWorld_Epic = "0xE841390";
	// public const string FNames_Epic = "0xE63B380";
    
	// 7.7.0
	// public const ulong GWorld_Steam = 0xEBF55A0;
	// public const ulong FNames_Steam = 0xE98FD00;
	// public const ulong GWorld_Epic = 0xE6755A0;
	// public const ulong FNames_Epic = 0xE3FF765;
	
	// 7.7.1
	// public const ulong GWorld_Steam = 0xEBE0C70;
	// public const ulong FNames_Steam = 0xE97B380;
	// public const ulong GObjects_Steam = 0xEA3DC40;
	// public const ulong GWorld_Epic = 0xE661B60;
	// public const ulong FNames_Epic = 0xE40FB80;
	// public const ulong GObjects_Epic = 0xE4D2400;
	
	// 8.0.0
	// public const ulong GWorld_Steam = 0xF319AA0;
	// public const ulong FNames_Steam = 0xF0B41C0;
	// public const ulong GObjects_Steam = 0xEA3DC40;
	// public const ulong GWorld_Epic = 0xED98AF0;
	// public const ulong FNames_Epic = 0xEB46B00;
	// public const ulong GObjects_Epic = 0xEC09380;
	
	
	// 8.0.1
	// public const ulong GWorld_Steam = 0xF34CE80;
	// public const ulong FNames_Steam = 0xF0E7530;
	// public const ulong GObjects_Steam = 0xF1A9E80;
	// public const ulong GWorld_Epic = 0xEDCCE50;
	// public const ulong FNames_Epic = 0xEB7AE80;
	// public const ulong GObjects_Epic = 0xEC3D700;
	
	
	// 8.0.2
	// public const ulong GWorld_Steam = 0xF34CE80;
	// public const ulong FNames_Steam = 0xF0E7530;
	// public const ulong GObjects_Steam = 0xF1A9E80;
	// public const ulong GWorld_Epic = 0xEDCDF50;
	// public const ulong FNames_Epic = 0xEB7BF80;
	// public const ulong GObjects_Epic = 0xEC3E800;
	
    
    // 8.1.0
    public const ulong GWorld_Steam = 0xF72B2A0;
	public const ulong FNames_Steam = 0xF4C57C0;
	public const ulong GObjects_Steam = 0xF588094;
	public const ulong GWorld_Epic = 0xF2B2CF0;
	public const ulong FNames_Epic = 0xF060AC0;
	public const ulong GObjects_Epic = 0xF123354;
	
	// public static ulong UWorld_Levels = 0x180;
	// public static string UWorld_LevelCollections = "0x0190";
	// public static int SizeOf_FLevelCollection = 0x0078;
	public static ulong UWorld_PersistentLevel = 0x0038;
	public static ulong UWorld_GameState = 0x0168;
	public static ulong UWorld_OwningGameInstance = 0x1C8;
	public static ulong UGameInstance_LocalPlayers = 0x0040;
	public static ulong UPlayer_PlayerController = 0x0038;
	public static ulong APlayerController_AcknowledgedPawn = 0x0350;
	public static ulong APlayerController_PlayerCameraManager = 0x0360;
	public static ulong APlayerCameraManager_CameraCachePrivate = 0x22D0;
	public static ulong ULevel_Actors = 0x00A0;
	public static ulong UNetConnection_OwningActor = 0x00A0;
	public static ulong UNetConnection_MaxPacket = 0x00A8;
	public static ulong AActor_ActorID = 0x0018;
	public static ulong AActor_ActorMesh = 0x298; // ACharacter::Mesh
	public static ulong BoneArray = 0x658;
	public static ulong ComponentToWorld = 0x260; // USkeletalMeshComponent::ComponentToWorld
	
	public static ulong AGameStateBase_PlayerArray = 0x02B8;
	
	public static ulong ADBDPlayerState_PlayerNamePrivate = 0x03A0;
	public static ulong ADBDPlayerState_GameRole = 0x03FA;
	public static ulong ADBDPlayerState_SelectedCamperIndex = 0x0638;
	public static ulong ADBDPlayerState_SelectedSlasherIndex = 0x063C;
	public static ulong ADBDPlayerState_CamperData = 0x0410;
	public static ulong ADBDPlayerState_SlasherData = 0x0430;
	public static ulong ADBDPlayerState_PlayerData = 0x0450;
	public static ulong ADBDPlayerState_Platform = 0x078C;
	
	public static string FCharacterStateDataInfo_PIPs = "0x0000";
	public static string FCharacterStateDataInfo_PowerId = "0x0004";
	public static ulong FCharacterStateDataInfo_AddonIds = 0x0010;
	
	public static string FPlayerStateData_CharacterLevel = "0x0000";
	public static ulong FPlayerStateData_EquipedFavorId = 0x0004;
	public static ulong FPlayerStateData_EquipedPerkIds = 0x0010;
	public static ulong FPlayerStateData_EquipedPerkLevels = 0x0020;
	public static string FPlayerStateData_EquippedBannerId = "0x0030";
	public static string FPlayerStateData_EquippedBadgeId = "0x0040";
	public static ulong FPlayerStateData_PrestigeLevel = 0x0060;

	// public static string FLevelCollection_GameState = "0x0008";
	// public static string FLevelCollection_Levels = "0x0028";
	public static ulong ADBDGameState_BuiltLevelData = 0x05E8;         // 0x05D0(0x00B8)
	public static ulong ADBDGameState_BuiltLevelData_MapName = 0x0080; // 0x05D0(0x00B8)
	// public static ulong ADBDGameState_LevelReadyToPlay = 0x06DB;       // 0x06DB(0x0001)
	// public static ulong ADBDGameState_GameLevelLoaded = 0x0908;        // 0x0908(0x0001)
	// public static ulong ADBDGameState_GameLevelCreated = 0x0909;       // 0x0909(0x0001)
	// public static ulong ADBDGameState_GameLevelEnded = 0x090A;         // 0x090A(0x0001)
	// public static ulong ADBDGameState_GameEndedReason = 0x090C;        // 0x090C(0x0001)
	// public static ulong ADBDGameState_MeatHooks = 0x06E8;
	// public static ulong ADBDGameState_Searchables = 0x06F8;
	// public static ulong ADBDGameState_Generators = 0x0708;
	// public static ulong ADBDGameState_EscapeDoors = 0x0718;
	// public static ulong ADBDGameState_Hatches = 0x0728;
	// public static ulong ADBDGameState_Pallets = 0x0758;
	// public static ulong ADBDGameState_Windows = 0x0768;
	// public static ulong ADBDGameState_Totems = 0x0798;
	// public static string ADBDGameState_CharacterCollection = "0x0888";
	
	// public static string AProceduralLevelBuilder_MapData = "0x0638";
	// public static string UMapData_ThemeName = "0x0038";
	// public static string UMapData_ThemeWeather = "0x0044";
	// public static string UMapData_MapName = "0x0078";

	public static ulong AGenerator_Activated = 0x03B0;
	public static ulong AGenerator_NativePercentComplete = 0x03C0;
	
	public static ulong AHatch_HatchState = 0x03F0;
	
	public static ulong ATotem_TotemState = 0x03F8;
	
	public static ulong ASearchable_SpawnedItem = 0x03D0;
	public static ulong ACollectable_ItemID = 0x0400; // FName
	
	public static ulong APallet_State = 0x03E0;
	
	public static ulong ABaseTrap_IsTrapSet = 0x0538;
	
	public static ulong AActor_RootComponent = 0x01A8;
	public static ulong APawn_PlayerState = 0x02C0;
	public static ulong APawn_AcknowledgedPawn = 0x02F8;
	public static string APawn_Controller = "0x0268";
	public static ulong ACharacter_Mesh = 0x0328;
	public static string ACamperPlayer_TimeforDeathWhileCrawling = "0x12F0";
	public static string ACamperPlayer_TimeforDeathWhileHooked = "0x12F4";
	public static string ACamperPlayer_BloodTrailSettings = "0x1478";
	public static string ASlasherPlayer_CachedBloodlustTier = "0x1554";
	
	public static string UCharacterCollection_Killer = "0x0178";
	public static string UCharacterCollection_Survivors = "0x0180";
	public static string UCharacterCollection_MainDBDPlayers = "0x01D0";
	public static string UCharacterCollection_OtherCharacters = "0x0220";
	public static string UCharacterCollection_NonPlayableCharacters = "0x0270";
	public static string UCharacterCollection_AllCharacters = "0x02C0";
	
	public static string UBloodTrailSettings_InitialDelay = "0x0038";
	public static string UBloodTrailSettings_BloodDropsInterval = "0x0040";
	
	public static ulong USkinnedMeshComponent_SkeletalMesh = 0x05F8;               // USkeletalMesh
	public static ulong USkinnedMeshComponent_CachedWorldOrLocalSpaceBounds = 0x08A0; // FBoxSphereBounds
	public static ulong USkeletalMeshComponent_ComponentSpaceTransformsArray = 0x0668; // BoneArray TArray<struct FTransform>[]
	public static ulong USkeletalMesh_Skeleton = 0x00F8;                           // USkeleton*
	public static ulong USkeletalMesh_MorphTargets = 0x02C8;
	public static ulong USkeletalMesh_RefSkeleton = USkeletalMesh_MorphTargets + 0x10 + 0x10; // FReferenceSkeleton Offset = MorphTargets + sizeof( FRenderCommandFence )( 0xC ) 
	public static ulong USkeleton_Sockets = 0x0198;                                // TArray<class USkeletalMeshSocket*>
	public static ulong USkeletalMeshSocket_SocketName = 0x0030;                   // 0x0030(0x000C) FName
	public static ulong USkeletalMeshSocket_BoneName = 0x003C;                     // 0x003C(0x000C) FName
	public static ulong USkeletalMeshSocket_RelativeLocation = 0x0048;             // 0x0048(0x0018) FVector
	public static ulong USkeletalMeshSocket_RelativeRotation = 0x0060;             // 0x0060(0x0018) FRotator
	public static ulong USkeletalMeshSocket_RelativeScale = 0x0078;                // 0x0078(0x0018) FVector
	public static ulong FReferenceSkeleton_RawRefBoneInfo = 0x0000;				// TArray<FMeshBoneInfo>
	public static ulong FReferenceSkeleton_RawNameToIndexMap = 0x0040;			// TMap<FName, int32>
	
	public static ulong USceneComponent_RelativeLocation = 0x0140;
	public static ulong USceneComponent_RelativeRotation = 0x0158;
	public static ulong USceneComponent_RelativeScale3D = 0x0170;
	public static ulong USceneComponent_ComponentToWorld = 0x01AD + 0xAB + 0x8; // 0x1E0
	public static string USceneComponent_ComponentToWorld_Rotation = "0x01E8";
	public static string USceneComponent_ComponentToWorld_Translation = "0x01F8";
	public static string USceneComponent_ComponentToWorld_Scale3D = "0x0208";

	public const int SizeOf_VoidPointer = 0x8;
	public static ulong TArray_Data = 0x0;
	public static ulong TArray_NumElements = 0x8;
	public static string TArray_MaxElements = "0xC";
	public static ulong TSet_Elements = 0x0;
	public static ulong TSet_Hash = 0x0;
	public static ulong TSet_HashSize = 0x0;
	
	public const int SizeOf_FName = 0xC;
	public static string FName_ComparisonIndex = "0x0";
	public static string FName_DisplayIndex = "0x8";
	public static string FName_Number = "0x4";
	
	public static ulong UStruct_Super = 0x0048;
	
	public static ulong TUObjectArray_NumElements = 0x8 + 0x4 + 0x4;
	public static ulong TUObjectArray_NumChunks = 0x8 + 0x4 + 0x4 + 0x4 + 0x4;
	public static int TUObjectArray_ElementsPerChunk = 0x10000;
	
	
	public static ulong UObject_Class = 0x0010;
	public static ulong UObject_Name = 0x0018;

	
	public static string KEYWORD_EscapeDoor = "EscapeDoor";
	public static string KEYWORD_Window = "Window";
	public static string KEYWORD_Locker = "Locker";
	public static string KEYWORD_BreakableBase = "BreakableBase";
	public static string KEYWORD_MeatHook = "MeatHook";
	public static string KEYWORD_Slasher = "Slasher";
	public static string KEYWORD_Camper = "Camper";

	public const string CLASS_Object = "Object";
	public const string CLASS_SkinnedMeshComponent = "SkinnedMeshComponent";
	public const string CLASS_SkeletalMesh = "SkeletalMesh";

	public const string CLASS_BearTrap = "BearTrap";
	public const string CLASS_Pallet = "Pallet";
	public const string CLASS_Hatch = "Hatch";
	public const string CLASS_Generator = "Generator";
	public const string CLASS_Window = "Window";
	public const string CLASS_Locker = "Locker";
	public const string CLASS_Chest = "BP_Chest_C";
	public const string CLASS_Totem = "Totem";
	public const string CLASS_Slasher = "SlasherPlayer";
	public const string CLASS_Camper = "CamperPlayer";
	public const string CLASS_DBDMenuPlayer = "DBDMenuPlayer";
	public const string CLASS_DBDPlayer = "DBDPlayer";


	#region Player Bone
	
	/* Camper:
		joint_Char
		joint_Pelvis_01
		joint_HipMaster_01
		joint_HipRT_01
		joint_KneeRT_01
		joint_FootRT_01
		joint_ToeRT_01
		joint_FootRT_Roll_01
		joint_KneeRT_Roll_01
		joint_HipLT_01
		joint_KneeLT_01
		joint_FootLT_01
		joint_ToeLT_01
		joint_FootLT_Roll_01
		joint_KneeLT_Roll_01
		joint_HipRT_Roll_01
		joint_HipLT_Roll_01
		joint_TorsoA_01
		joint_TorsoB_01
		joint_TorsoC_01
		joint_ClavicleLT_01
		joint_ShoulderLT_01
		joint_ElbowLT_01
		joint_WristLT_Roll_01
		joint_ForArmLT_Roll_01
		joint_HandLT_01
		joint_PinkyALT_01
		joint_PinkyBLT_01
		joint_PinkyCLT_01
		joint_RingALT_01
		joint_RingBLT_01
		joint_RingCLT_01
		joint_FingerALT_01
		joint_FingerBLT_01
		joint_FingerCLT_01
		joint_IndexALT_01
		joint_IndexBLT_01
		joint_IndexCLT_01
		joint_ThumbALT_01
		joint_ThumbBLT_01
		joint_ThumbCLT_01
		joint_CarryLT_01
		joint_WeaponLT_01
		joint_ShoulderLT_Roll_01
		joint_NeckA_01
		joint_Head_01
		joint_EyeLT_01
		joint_EyeRT_01
		joint_ClavicleRT_01
		joint_ShoulderRT_01
		joint_ElbowRT_01
		joint_WristRT_Roll_01
		joint_ForArmRT_Roll_01
		joint_HandRT_01
		joint_PinkyART_01
		joint_PinkyBRT_01
		joint_PinkyCRT_01
		joint_RingART_01
		joint_RingBRT_01
		joint_RingCRT_01
		joint_FingerART_01
		joint_FingerBRT_01
		joint_FingerCRT_01
		joint_IndexART_01
		joint_IndexBRT_01
		joint_IndexCRT_01
		joint_ThumbART_01
		joint_ThumbBRT_01
		joint_ThumbCRT_01
		joint_WeaponRT_01
		joint_ShoulderRT_Roll_01
		joint_CamOff_01
		joint_Cam_01
		joint_Hand_RT_01_IK
		joint_Hand_LT_01_IK
		joint_Hand_LT_02_IK
		joint_Hand_RT_02_IK
		joint_Foot_LT_01_IK
		joint_Foot_RT_01_IK
	 */
	
	/* Slasher:
		joint_Char
		joint_Pelvis_01
		joint_HipMaster_01
		joint_HipRT_01
		joint_HipRollRT_01
		joint_KneeRT_01
		joint_FootRT_01
		joint_ToeRT_01
		joint_KneeRollRT_01
		joint_HipLT_01
		joint_HipRollLT_01
		joint_KneeLT_01
		joint_FootLT_01
		joint_ToeLT_01
		joint_KneeRollLT_01
		joint_TorsoA_01
		joint_TorsoB_01
		joint_TorsoC_01
		joint_ClavicleLT_01
		joint_ShoulderLT_01
		joint_ShoulderRollLT_01
		joint_ElbowLT_01
		joint_HandLT_01
		joint_WeaponLT_01
		joint_ThumbALT_01
		joint_ThumbBLT_01
		joint_ThumbCLT_01
		joint_IndexALT_01
		joint_IndexBLT_01
		joint_IndexCLT_01
		joint_FingerALT_01
		joint_FingerBLT_01
		joint_FingerCLT_01
		joint_RingALT_01
		joint_RingBLT_01
		joint_RingCLT_01
		joint_PinkyALT_01
		joint_PinkyBLT_01
		joint_PinkyCLT_01
		joint_ElbowRollLT_01
		joint_HandRoll01LT_01
		joint_HandRoll02LT_01
		joint_ClavicleRT_01
		joint_ShoulderRT_01
		joint_ShoulderRollRT_01
		joint_ElbowRT_01
		joint_HandRT_01
		joint_WeaponRT_01
		joint_ThumbART_01
		joint_ThumbBRT_01
		joint_ThumbCRT_01
		joint_IndexART_01
		joint_IndexBRT_01
		joint_IndexCRT_01
		joint_FingerART_01
		joint_FingerBRT_01
		joint_FingerCRT_01
		joint_RingART_01
		joint_RingBRT_01
		joint_RingCRT_01
		joint_PinkyART_01
		joint_PinkyBRT_01
		joint_PinkyCRT_01
		joint_ElbowRollRT_01
		joint_HandRoll01RT_01
		joint_HandRoll02RT_01
		joint_NeckA_01
		joint_Head_01
		joint_FacialGroup
		cheek_LT_01
		cheek_RT_01
		cheekbone_LT_01
		cheekbone_LT_02
		cheekbone_LT_03
		cheekbone_LT_04
		cheekbone_RT_01
		cheekbone_RT_02
		cheekbone_RT_03
		cheekbone_RT_04
		eye_LT
		eye_RT
		eyebrows_LT_01
		eyebrows_LT_02
		eyebrows_LT_03
		eyebrows_LT_04
		eyebrows_RT_02
		eyebrows_RT_03
		eyebrows_RT_04
		eyelids_down_LT
		eyelids_down_RT
		eyelids_up_LT
		eyelids_up_RT
		jaw
		chin
		tongue_01
		tongue_02
		tongue_03
		sneer_RT_04
		sneer_LT_04
		lips_down_RT_03
		lips_down_RT_02
		lips_down_RT_01
		lips_down_LT_03
		lips_down_LT_02
		lips_down_LT_01
		lips_down_mid
		nose
		sneer_LT_01
		sneer_LT_02
		sneer_RT_01
		sneer_RT_02
		lips_up_RT_01
		lips_up_RT_02
		lips_up_LT_01
		lips_up_LT_02
		lips_up_LT_03
		lips_up_RT_03
		lips_corner_LT
		lips_corner_RT
		lips_up_mid
		eyebrows_RT_01
		joint_Hand_RT_01_IK
		joint_Hand_LT_01_IK
		joint_Cam_01
		joint_Cam_02
		joint_Cam_03
		joint_CamperAttach_01
		joint_SlasherAttach_01
		joint_Foot_LT_01_IK
		joint_Foot_RT_01_IK
	 */

	public const string JOINT_Neck = "joint_Neck";
	public const string JOINT_Head = "joint_Head";
	// public const string JOINT_HipMaster = "joint_HipMaster_01";
	public const string JOINT_ShoulderLT = "joint_ShoulderLT";
	public const string JOINT_ShoulderLT_Roll = "joint_ShoulderLT_Roll_01";
	public const string JOINT_ShoulderRT = "joint_ShoulderRT";
	public const string JOINT_ShoulderRT_Roll = "joint_ShoulderRT_Roll_01";
	public const string JOINT_ElbowLT = "joint_ElbowLT";
	public const string JOINT_ElbowRT = "joint_ElbowRT";
	public const string JOINT_HandLT = "joint_HandLT";
	public const string JOINT_HandRT = "joint_HandRT";
	public const string JOINT_FingerALT = "joint_FingerALT";
	public const string JOINT_FingerBLT = "joint_FingerBLT";
	public const string JOINT_FingerCLT = "joint_FingerCLT";
	public const string JOINT_FingerART = "joint_FingerART";
	public const string JOINT_FingerBRT = "joint_FingerBRT";
	public const string JOINT_FingerCRT = "joint_FingerCRT";
	
	public const string JOINT_TorsoA = "joint_TorsoA";
	public const string JOINT_TorsoB = "joint_TorsoB";
	public const string JOINT_TorsoC = "joint_TorsoC";
	public const string JOINT_Pelvis = "joint_Pelvis";
	public const string JOINT_HipLT = "joint_HipLT";
	public const string JOINT_HipRT = "joint_HipRT";
	public const string JOINT_KneeLT = "joint_KneeLT";
	public const string JOINT_KneeRT = "joint_KneeRT";
	public const string JOINT_FootLT = "joint_FootLT";
	public const string JOINT_FootRT = "joint_FootRT";
	public const string JOINT_ToeLT = "joint_ToeLT";
	public const string JOINT_ToeRT = "joint_ToeRT";

	public static Pair< string >[] DbdCamperJointConnections = [
		new ( JOINT_Head, JOINT_Neck ),
		new ( JOINT_Neck, JOINT_ShoulderLT ),
		new ( JOINT_Neck, JOINT_ShoulderRT ),
		new ( JOINT_Neck, JOINT_TorsoC ),
		new ( JOINT_ShoulderLT, JOINT_ElbowLT ),
		new ( JOINT_ShoulderRT, JOINT_ElbowRT ),
		new ( JOINT_ElbowLT, JOINT_HandLT ),
		new ( JOINT_ElbowRT, JOINT_HandRT ),
		// new ( JOINT_HandLT, JOINT_FingerALT ),
		// new ( JOINT_HandLT, JOINT_FingerBLT ),
		// new ( JOINT_HandLT, JOINT_FingerCLT ),
		// new ( JOINT_HandRT, JOINT_FingerART ),
		// new ( JOINT_HandRT, JOINT_FingerBRT ),
		// new ( JOINT_HandRT, JOINT_FingerCRT ),
		new ( JOINT_TorsoC, JOINT_TorsoB ),
		new ( JOINT_TorsoB, JOINT_TorsoA ),
		new ( JOINT_TorsoA, JOINT_Pelvis ),
		new ( JOINT_Pelvis, JOINT_HipLT ),
		new ( JOINT_Pelvis, JOINT_HipRT ),
		new ( JOINT_HipLT, JOINT_KneeLT ),
		new ( JOINT_HipRT, JOINT_KneeRT ),
		new ( JOINT_KneeLT, JOINT_FootLT ),
		new ( JOINT_KneeRT, JOINT_FootRT ),
		new ( JOINT_FootLT, JOINT_ToeLT ),
		new ( JOINT_FootRT, JOINT_ToeRT ),
	];
	
	#endregion
	
	
	private static string ToCamelCase( this string str ) {
		var words = str.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
		words = words
				.Select(word => char.ToUpper(word[0]) + word.Substring(1))
				.ToArray();
		return string.Join(string.Empty, words);
	}

	
	public static void LoadOffsets( string offsetsConfigFile ) {
		// JObject jObject = JObject.Parse( File.ReadAllText( offsetsConfigFile ) );
		//
		// foreach ( var kv in jObject ) {
		// 	var immeStrs = kv.Key.Replace( "SDK::", String.Empty ).Replace( "_", string.Empty ).Replace( "::", "_" ).Split( '_' );
		// 	var className = immeStrs[ 0 ];
		// 	var classFieldName = immeStrs[ 1 ].ToCamelCase();
		// 	string offsetsFieldName = $"{className}_{classFieldName}";
		// 	var field = typeof( Offsets ).GetField( offsetsFieldName, BindingFlags.Public | BindingFlags.Static );
		// 	if ( field is not null ) {
		// 		field.SetValue( null, kv.Value.ToString() );
		// 		// Logger.Debug( $"update offset: {kv.Key} -> {offsetsFieldName} = {kv.Value}" );
		// 	}
		// 	else {
		// 		Logger.Warning( $"field not found: {kv.Key} -> {offsetsFieldName}" );
		// 	}
		// }

		if ( GWorld_Steam.Equals( NullAddress ) ) {
			Logger.Error( $"{nameof( GWorld_Steam )} is null." );
		}
		if ( FNames_Steam.Equals( NullAddress ) ) {
			Logger.Error( $"{nameof( FNames_Steam )} is null." );
		}
		if ( GWorld_Epic.Equals( NullAddress ) ) {
			Logger.Error( $"{nameof( GWorld_Epic )} is null." );
		}
		if ( FNames_Epic.Equals( NullAddress ) ) {
			Logger.Error( $"{nameof( FNames_Epic )} is null." );
		}
		
		Logger.Debug( "[Offsets] LoadOffsets ok." );
	}
	
}


public static class DBDKillers {

	private static ( int, string )[] Data = [
		( 268435456, "Trapper" ),        // 陷阱杀手
		( 268435457, "Wraith" ),         // 隐鬼
		( 268435458, "Hillbilly" ),      // 农场主
		( 268435459, "Nurse" ),          // 护士
		( 268435460, "Hag" ),            // 李奶奶
		( 268435461, "Shape" ),          // 迈克尔
		( 268435462, "Doctor" ),         // 老杨
		( 268435463, "Huntress" ),       // 兔妈
		( 268435464, "Cannibal" ),       // 巴布
		( 268435465, "Nightmare" ),      // 梦魇
		( 268435466, "Pig" ),            // 猪猪
		( 268435467, "Clown" ),          // 小丑
		( 268435468, "Spirit" ),         // 怨灵
		( 268435469, "Legion" ),         // 军团
		( 268435470, "Plague" ),         // 瘟疫
		( 268435471, "Ghost Face" ),     // 狗面
		( 268435472, "Demogorgon" ),     // 魔王
		( 268435473, "Oni" ),            // 鬼武士
		( 268435474, "Deathslinger" ),   // 枪手
		( 268435475, "Executioner" ),    // 三角头
		( 268435476, "Blight" ),         // 枯萎者
		( 268435477, "Twins" ),          // 连体婴
		( 268435478, "Trickster" ),      // 骗术师
		( 268435479, "Nemesis" ),        // 追击者
		( 268435480, "Cenobite" ),       // 钉子头
		( 268435481, "Artist" ),         // 艺术家
		( 268435482, "Onryō" ),          // 贞子
		( 268435483, "Dredge" ),         // 影魔
		( 268435484, "Mastermind" ),     // 威斯克
		( 268435485, "Knight" ),         // 恶骑士
		( 268435486, "Skull Merchant" ), // 白骨
		( 268435487, "Singularity" ),    // 奇点
		( 268435488, "Xenomorph" ),      // 异形
		( 268435489, "Good Guy" ),       // 好孩子
		( 268435490, "The Unknown" )     // 未知恶物
	];


	internal static ( int, string )[] DataTranslated = [
		( 268435456, "夹哥" ),
		( 268435457, "隐鬼" ),
		( 268435458, "农幻神" ),
		( 268435459, "狗叫" ),
		( 268435460, "李奶奶" ),
		( 268435461, "迈克尔 迈尔斯" ),
		( 268435462, "老杨" ),
		( 268435463, "兔妈" ),
		( 268435464, "巴ber" ),
		( 268435465, "梦境仙女" ),
		( 268435466, "猪猪" ),
		( 268435467, "小丑" ),
		( 268435468, "怨灵" ),
		( 268435469, "团子" ),
		( 268435470, "吐妈" ),
		( 268435471, "狗面" ),
		( 268435472, "魔王" ),
		( 268435473, "狗武士" ),
		( 268435474, "枪手" ),
		( 268435475, "三角头" ),
		( 268435476, "枯魔" ),
		( 268435477, "连体婴" ),
		( 268435478, "骗术师" ),
		( 268435479, "追击者" ),
		( 268435480, "钉子头" ),
		( 268435481, "艺术家" ),
		( 268435482, "贞子" ),
		( 268435483, "影魔" ),
		( 268435484, "威斯克" ),
		( 268435485, "恶骑士" ),
		( 268435486, "白骨" ),
		( 268435487, "奇点" ),
		( 268435488, "异形" ),
		( 268435489, "好孩子" ),
		( 268435490, "未知恶物" ),
		( 268435491, "巫妖" )
	];

	public static string GetKillerName( int killerID ) {
		// foreach ( var data in Data ) {
		foreach ( var data in DataTranslated ) {
			if ( data.Item1 == killerID ) {
				return data.Item2;
			}
		}

		return null;
	}
}


public static class DBDCampers {
	
	private static ( int, string )[] Data = [
		( 0, "Dwight Fairfield" ),
		( 1, "Meg Thomas" ),
		( 2, "Claudette Morel" ),
		( 3, "Jake Park" ),
		( 4, "Nea Karlsson" ),
		( 5, "Laurie Strode" ),
		( 6, "Ace Visconti" ),
		( 7, "William \"Bill\" Overbeck" ),
		( 8, "Feng Min" ),
		( 9, "David King" ),
		( 10, "Kate Denson" ),
		( 11, "Quentin Smith" ),
		( 13, "Adam Francis" ),
		( 14, "Jeffrey \"Jeff\" Johansen" ),
		( 15, "Jane Romero" ),
		( 16, "Ashley J. Williams" ),
		( 17, "Nancy Wheeler" ),
		( 18, "Steve Harrington" ),
		( 19, "Yui Kimura" ),
		( 20, "Zarina Kassir" ),
		( 21, "Cheryl Mason" ),
		( 22, "Felix Richter" ),
		( 23, "Élodie Rakoto" ),
		( 24, "Yun-Jin Lee" ),
		( 25, "Jill Valentine" ),
		( 26, "Leon Scott Kennedy" ),
		( 27, "Mikaela Reid" ),
		( 31, "Ada Wong" ),
		( 32, "Rebecca Chambers" ),
		( 33, "Vittorio Toscano" ),
		( 34, "Thalita Lyra" ),
		( 35, "Renato Lyra" ),
		( 36, "Gabriel Soma" ),
		( 37, "Nicolas Cage" ),
		( 38, "Ellen Ripley" ),
		( 39, "Alan Wake" ),
		( 40, "New" ),
	];
	
	
	public static ( int, string )[] DataTranslated = [
		( 0, "德怀特" ),
		( 1, "梅格 托马斯" ),
		( 2, "克劳黛特 莫莱" ),
		( 3, "杰克 帕克" ),
		( 4, "妮娅 卡尔森" ),
		( 5, "劳莉" ),
		( 6, "艾斯" ),
		( 7, "威廉 比尔" ),
		( 8, "凤敏" ),
		( 9, "大卫 金" ),
		( 10, "凯特 登森" ),
		( 11, "昆丁 史密斯" ),
		( 13, "亚当 弗朗西斯" ),
		( 14, "Jeffrey \"Jeff\" Johansen" ),
		( 15, "Jane Romero" ),
		( 16, "Ashley J. Williams" ),
		( 17, "南希 惠勒" ),
		( 18, "史蒂夫 哈灵顿" ),
		( 19, "木村唯" ),
		( 20, "Zarina Kassir" ),
		( 21, "雪柔 梅森" ),
		( 22, "Felix Richter" ),
		( 23, "埃洛迪 拉科托" ),
		( 24, "李允珍" ),
		( 25, "吉尔 瓦伦丁" ),
		( 26, "里昂 肯尼迪" ),
		( 27, "Mikaela Reid" ),
		( 28, "约拿 法斯克兹" ),
		( 29, "浅川杨一" ),
		( 30, "哈迪 寇尔" ),
		( 31, "艾达 王" ),
		( 32, "蕾贝卡 钱伯斯" ),
		( 33, "Vittorio Toscano" ),
		( 34, "塔莉妲 莱拉" ),
		( 35, "Renato Lyra" ),
		( 36, "Gabriel Soma" ),
		( 37, "尼古拉斯 凯奇" ),
		( 38, "Ellen Ripley" ),
		( 39, "Alan Wake" ),
		( 40, "赛贝尔 瓦德" ),
		( 41, "艾斯翠 亚撒" ),
	];

	public static string GetCampersName( int camperID ) {
		foreach ( var data in DataTranslated ) {
			if ( data.Item1 == camperID ) {
				return data.Item2;
			}
		}

		return null;
	}
}


public static class DBDAddons {
	
	private static ImmutableDictionary< string, string > Data = new Dictionary< string, string > {
		{ "_EMPTY_", "空" },
		
		// 团子
		{ "Addon_Frenzy_FumingMixTape", "冒烟的混音带" },
		{ "Addon_Frenzy_IridescentButton", "荧光纽扣" },
		{ "Addon_Frenzy_StabWoundsStudy", "刺伤研究" },
		{ "Addon_Frenzy_FranksMixTape", "弗兰克的混音带" },
		{ "Addon_Frenzy_ColdDirt", "永远的死党" },
		{ "Addon_Frenzy_FilthyBlade", "污秽之剑" },
		{ "Addon_Frenzy_JoeysMixTape", "乔伊的混音带" },
		{ "Addon_Frenzy_StolenSketchBook", "失窃的素描簿" },
		{ "Addon_Frenzy_NastyBlade", "时髦墨镜" },
		{ "Addon_Frenzy_SusiesMixTape", "苏西的混音带" },
		{ "Addon_Frenzy_TheLegionPin", "军团饰针" },
		{ "Addon_Frenzy_NeverSleepPills", "不眠药片" },
		{ "Addon_Frenzy_DefacedSmileyPin", "涂鸦笑脸饰针" },
		{ "Addon_Frenzy_MuralSketch", "壁画素描" },
		{ "Addon_Frenzy_JuliesMixTape", "朱莉的混音带" },
		{ "Addon_Frenzy_EtchedRuler", "蚀刻标尺" },
		{ "Addon_Frenzy_MischiefList", "伤害列表" },
		{ "Addon_Frenzy_ScratchedRuler", "划痕标尺" },
		{ "Addon_Frenzy_FriendshipBracelet", "友谊手镯" },
		{ "Addon_Frenzy_SmileyFacePin", "笑脸饰针" },

		// 兔妈
		{ "Addon_Hatchet_PungentFiale", "士兵的绑腿" },
		{ "Addon_Hatchet_IridescentHead", "精致斧头" },
		{ "Addon_Hatchet_InfantryBelt", "多功能腰带" },
		{ "Addon_Hatchet_YewSeedBrew", "木狐狸" },
		{ "Addon_Hatchet_BegrimedHead", "污秽斧头" },
		{ "Addon_Hatchet_GlowingConcoction", "发光调和剂" },
		{ "Addon_Hatchet_VenomousConcoction", "剧毒调和物" },
		{ "Addon_Hatchet_YewSeedConcoction", "玫瑰根" },
		{ "Addon_Hatchet_RustyHead", "生锈斧头" },
		{ "Addon_Hatchet_FlowerBabushka", "碎花头巾" },
		{ "Addon_Hatchet_DeerskinGloves", "鹿皮手套" },
		{ "Addon_Hatchet_ShinyPin", "光亮饰针" },
		{ "Addon_Hatchet_OakHaft", "橡木斧柄" },
		{ "Addon_Hatchet_FineStone", "沉重头颅" },
		{ "Addon_Hatchet_LeatherLoop", "皮带圈" },
		{ "Addon_Hatchet_MannaGrassBraid", "草编圈" },
		{ "Addon_Hatchet_BerusToxin", "泛黄布料" },
		{ "Addon_Hatchet_CoarseStone", "糙石" },
		{ "Addon_Hatchet_BandagedHaft", "缠着绷带的斧柄" },
		{ "Addon_Hatchet_AmanitaToxin", "鹅膏菌毒素" },

		// 迈克尔
		{ "Addon_Stalker_JudithsTombstone", "朱蒂斯的墓碑" },
		{ "Addon_Stalker_FragrantTuftOfHair", "一簇芳香的头发" },
		{ "Addon_Stalker_LockOfHair", "一缕头发" },
		{ "Addon_Stalker_VanityMirror", "化妆镜(永二)" },
		{ "Addon_Stalker_TombstonePiece", "墓碑碎块" },
		{ "Addon_Stalker_ScratchedMirror", "有划痕的镜子(永一)" },
		{ "Addon_Stalker_JMyersMemorial", "绿钞票" },
		{ "Addon_Stalker_DeadRabbit", "死兔子" },

		// 隐鬼
		{ "ADDON_Bell_AllseeingSpirit", "洞穿一切-灵魂" },
		{ "ADDON_Bell_CoxcombedClapper", "无声钟" },
		{ "ADDON_Bell_WindstormBlood", "狂风暴雨-鲜血" },
		{ "ADDON_Bell_BlindSpirit", "影舞-鲜血" },
		{ "Addon_Bell_008", "洞穿一切-鲜血" },
		{ "Addon_Bell_006", "迅捷捕猎-鲜血" },
		{ "ADDON_Bell_BlindBlood", "影舞-白" },
		{ "ADDON_Bell_BlindWhite", "盲战士-白" },
		{ "Addon_Bell_012", "瞬移-白" },
		{ "ADDON_Bell_SwiftMud", "迅捷捕猎-白" },
		{ "Addon_Bell_010", "风暴-白" },
		{ "Addon_Bell_004", "盲战士-污泥" },
		{ "Addon_Bell_001", "骨制钟锤" },
		{ "Addon_Bell_002", "鬼魂煤灰" },

		// 夹哥
		{ "ADDON_trap_DiamondStone", "荧光打磨石" },
		{ "ADDON_trap_BloodyCoil", "血腥弹簧圈" },
		{ "Addon_Beartrap_001", "打磨石" },
		{ "Addon_Beartrap_009", "陷阱杀手袋" },
		{ "Addon_Beartrap_011", "副弹簧" },
		{ "Addon_Beartrap_006", "焦油瓶" },

		// 李奶奶
		{ "Addon_PhantomTrap_WaterloggedShoe", "足利健" },
		{ "Addon_PhantomTrap_ScarredHand", "伤痕手掌" },
		{ "Addon_PhantomTrap_RustyShackles", "生锈铁铐" },

		// 凛妹
		{ "Addon_PhaseWalker_MotherDaughterRing", "母女戒" },
		{ "Addon_PhaseWalker_DriedCherryBlossom", "干樱花" },
		{ "Addon_PhaseWalker_RustyFlute", "生锈长笛" },
		{ "Addon_PhaseWalker_PrayerBeadsBracelet", "风铃" },
		{ "Addon_PhaseWalker_WhiteHairRibbon", "白色发带" },
		{ "Addon_PhaseWalker_MuddySportsDayCap", "肮脏的运动帽" },
		{ "Addon_PhaseWalker_RinsBrokenWatch", "凛的手表" },
		{ "Addon_PhaseWalker_KaiunTalisman", "开运御守" },
		{ "Addon_PhaseWalker_Zori", "草鞋" },

		// 枯萎者
		{ "Addon_Rush_CompoundThirtyThree", "化合物三十三" },
		{ "Addon_Rush_IridescentBlightTag", "莹红枯萎标记" },
		{ "Addon_Rush_BlightedCrow", "枯萎乌鸦" },
		{ "Addon_Rush_AdrenalineVial", "肾上腺素瓶" },
		{ "Addon_Rush_BlightedRat", "枯萎老鼠" },
		{ "Addon_Rush_AlchemistsRing", "炼金术师的戒指" },

		// 钉子头
		// { "Addon_K25Power", "工程师的尖牙/莹红哀痛之盒" },
		{ "Addon_K25Power_01", "弯曲钉子" },
		{ "Addon_K25Power_02", "鲜活蟋蟀" },
		{ "Addon_K25Power_03", "燃烧蜡烛" },
		{ "Addon_K25Power_04", "皮带" },
		{ "Addon_K25Power_05", "蠕动蛆虫" },
		{ "Addon_K25Power_06", "徽变饭菜" },
		{ "Addon_K25Power_07", "串烧老鼠" },
		{ "Addon_K25Power_08", "闪烁电视" },
		{ "Addon_K25Power_09", "腐化血块" },
		
		// 威斯克
		{ "Addon_K29_09", "扩音器" },
		{ "Addon_K29_07", "狮子徽章" },
		{ "Addon_K29_06", "皮革手套" },
		{ "Addon_K29_05", "松动的曲柄把手" },
		{ "Addon_K29_08", "黄金杯" },
		{ "Addon_K29_01", "宝石天牛" },
		{ "Addon_K29_04", "浣熊市警局对讲机" },
		{ "Addon_K29_03", "独角兽徽章" },

		// 枪手
		{ "Addon_Harpoon_IridescentCoin", "莹红币" },
		{ "Addon_Harpoon_HellshireIron", "赫尔塞尔钢铁" },
		{ "Addon_Harpoon_ModifiedAmmoBelt", "改装弹药袋" },
		{ "Addon_Harpoon_WardensKeys", "典狱长的钥匙" },
		
		// 瘟疫
		{ "Addon_Plague_IridescentSeal", "莹红封印" },
		{ "Addon_Plague_BlackIncense", "黑色之香" },

		// 护士
		{ "Addon_Blinker_005", "书签碎片" },
		{ "ADDON_Lastbreath_Matchbox", "火柴盒" },
		{ "ADDON_Lastbreath_BadManLastBreath", "恶男的濒死之息" },
		{ "ADDON_Lastbreath_KavanaghLastBreath", "卡瓦纳的濒死之息" },

		// 猪猪
		{ "Addon_ReverseBearTrap_VideoTape", "录影带" },
		{ "Addon_ReverseBearTrap_AmandasLetter", "阿曼达的信" },
		{ "Addon_ReverseBearTrap_CrateofGears", "一箱零件" },
		{ "Addon_ReverseBearTrap_TamperedTimer", "改装计时器" },
		{ "Addon_ReverseBearTrap_JigsawsSketch", "拼图杀人狂的设计图" },
		{ "Addon_ReverseBearTrap_JigsawsAnnotatedPlan", "拼图杀人狂的注释图" },

		// 狗武士
		{ "Addon_Kanobo_YamaokaFamilyCrest", "山岗家族纹章" },
		{ "Addon_Kanobo_RenjirosBloodyGlove", "廉二郎的染血手套" },
		{ "Addon_Kanobo_AkitosCrutch", "明人的拐杖" },
		{ "Addon_Kanobo_SplinteredHull", "破碎船身" },
		{ "Addon_Kanobo_WoodenOniMask", "木制鬼武者面具" },
		{ "Addon_Kanobo_LionFang", "狮牙" },
		
		// 老杨
		{ "Addon_Spark_IridescentGeneral", "多彩国王" },
		{ "Addon_Spark_JadeCharm", "莹红皇后" },
		{ "Addon_Spark_RestraintMuYisNotes", "枷锁" },
		{ "Addon_Spark_OrderMuYisNotes", "秩序" },
		{ "Addon_Spark_DisciplineMuYisNotes", "纪律" },
		{ "Addon_Spark_CalmMuYisNotes", "镇静" },

		

	}.ToImmutableDictionary();


	public static ImmutableDictionary< string, string > GetData() => Data;
	
	public static string GetAddonName( string addonFName ) {
		if ( string.IsNullOrEmpty( addonFName ) ) {
			return null;
		}
		if ( Data.TryGetValue( addonFName, out var outName ) ) {
			return outName;
		}
		return addonFName;
	}
}


public static class DBDFavors {
	
	private static ImmutableDictionary< string, string > Data = new Dictionary< string, string > {
		{ "_EMPTY_", "无祭品" },
		{ "EbonyMementoMori", "红苹果" },
		{ "CutCoin", "切分的硬币" },
		{ "PutridOak", "朽坏的橡木" },
		{ "MurkyReagent", "紫雾" },
		{ "BlackWard", "黑护符" },
		{ "TheLastMask", "仅存的面具" },
		{ "JapaneseCountrySide", "山冈家族纹章" },
		{ "HeartLocket", "心形项链" },
		{ "EclipseThemeOffering", "浣熊市警局徽章" },
		{ "Jigsawpiece", "拼图块" },
		{ "StrodeRealtyKey", "斯特罗德宅邸钥匙" },
		{ "KenyaThemeOffering", "毁坏的照片" },
		{ "WormholeThemeOffering", "气闸舱舱门" },
		{ "IonThemeOffering", "乌鸦之眼" },
		{ "CharredWeddingPhotograph", "烧焦的婚纱照" },
		{ "QuantumThemeOffering", "牛油混合物=大便广场" },
		{ "WalesThemeOffering", "玛丽的信" },
		{ "UmbraThemeOffering", "异星植物" },
		{ "UkraineThemeOffering", "破碎之瓶" },
		{ "ShatteredGlasses", "破碎眼镜" },
		{ "SacrificalWard", "献祭结界" },
		{ "GranmasCookbook", "奶奶的烹饪书" },
		{ "RottenOak", "绿色橡木" },
		{ "MeteorThemeOffering", "脓液腐土" },
		{ "IvoryMementoMori", "绿苹果" },
		{ "MacMillanPhalanxBone", "迈克米伦趾骨" },
		{ "AzarovKey", "阿扎罗夫钥匙" },
		{ "QatarThemeOffering", "霍金斯国家实验室ID卡" },
		{ "ThePiedPiper", "魔笛手" },
		{ "ScratchedCoin", "划痕硬币" },
		{ "HazyReagent", "黄色试剂" },
		{ "CypressMementoMori", "黄苹果" },
		{ "MoldyOak", "发灰的橡木" },
		{ "ShroudofSeparation", "碎裹尸布" },
		{ "TornBlueprint", "碎蓝图" },
		{ "BloodiedBlueprint", "染血蓝图" },
		{ "ClearReagent", "清雾试剂" },
		{ "FaintReagent", "眩晕试剂" },
		{ "VigosBlueprint", "维戈蓝图" },
		{ "AnnotatedBlueprint", "注释蓝图" },
		{ "ShroudofBinding", "捆绑裹尸布" },
		{ "WhiteWard", "白护符" },
		{ "PetrifiedOak", "石化橡木" },
		{ "VigosJarOfSaltyLips", "盐罐子" },
		{ "BoundEnvelope", "绿信封" },
		{ "FragrantCrispleafAmaranth", "芳香不凋花" },
		{ "FragrantPrimroseBlossom", "芳香报春花" },
		{ "FragrantBogLaurel", "芳香苔地月桂" },
		{ "FragrantSweetWilliam", "芳香西洋石竹" },
		{ "IvoryChalkPouch", "象牙色白垩粉袋" },
		{ "BlackSaltStatuette", "黑色盐雕像" },
		{ "EscapeCake", "逃生！蛋糕" },
		{ "CreamChalkPouch", "奶油色白垩粉袋" },
		{ "VigosShroud", "威戈的裹尸布" },
		{ "SealedEnvelope", "密封的信封" },
		{ "FreshCrispleafAmaranth", "新鲜不凋花" },
		{ "FreshPrimroseBlossom", "新鲜报春花" },
		{ "FreshBogLaurel", "新鲜苔地月桂" },
		{ "FreshSweetWilliam", "新鲜西洋石竹" },
		{ "TarnishedCoin", "晦暗硬币" },
		{ "ShroudofUnion", "联合裹尸布" },
		{ "SaltPouch", "盐袋" },
		{ "CrispleafAmaranthSachet", "不凋花香包" },
		{ "PrimroseBlossomSachet", "报春花香包" },
		{ "BogLaurelSachet", "苔地月桂香包" },
		{ "SweetWilliamSachet", "西洋石竹香包" },
		{ "ChalkPouch", "粉笔灰袋" },
		{ "ArdentTanagerWreath", "赤雀祭环" },
		{ "ArdentRavenWreath", "赤鸦祭环" },
		{ "ArdentShrikeWreath", "赤鹃祭环" },
		{ "ArdentSpottedowlWreath", "赤鹰祭环" },
		{ "DevoutTanagerWreath", "献祭赤雀祭环" },
		{ "DevoutRavenWreath", "献祭赤鸦祭环" },
		{ "DevoutShrikeWreath", "献祭赤鹃祭环" },
		{ "DevoutSpottedowlWreath", "献祭赤鹰祭环" },
		{ "HollowShell", "破裂之茧" },
		{ "TanagerWreath", "雀祭环" },
		{ "RavenWreath", "鸦祭环" },
		{ "ShrikeWreath", "鹃祭环" },
		{ "SpottedOwlWreath", "鹰祭环" },
		{ "BloodyPartyStreamers", "辣条" },
		{ "Anniversary2024Offering", "尖叫馅饼" },
		{ "Anniversary2023Offering", "恐怖提拉米苏" },
		{ "Anniversary2022Offering", "恐怖布丁" },
		{ "Anniversary2021Offering", "祭祀蛋糕" },
		{ "Anniversary2020Offering", "惊骇蛋糕" },
		{ "Anniversary2019Offering", "恐怖蛋糕" },
		{ "SurvivorPudding", "逃生者布丁" },
	}.ToImmutableDictionary();


	public static ImmutableDictionary< string, string > GetData() => Data;
	
	public static string GetFavorName( string favorFName ) {
		if ( string.IsNullOrEmpty( favorFName ) ) {
			return null;
		}
		if ( Data.TryGetValue( favorFName, out var outName ) ) {
			return outName;
		}
		return favorFName;
	}
}


public static class DBDPerks {

	private static ImmutableDictionary< string, string > Data = new Dictionary< string, string > {
		{ "_EMPTY_", string.Empty },
		{ "_LOCKED_", "锁定" },
		{ "Unrelenting", "不屈不挠" },
		{ "Play_With_Your_Food", "戏耍食物" },
		{ "Surveillance", "了如指掌" },
		{ "BBQAndChilli", "BBQ" },
		{ "MakeYourChoice", "做出你的选择" },
		{ "Lightborn", "光明之眼" },
		{ "K34P03", "飞机杯" },
		{ "HexUndying", "恶咒: 不死" },
		{ "K34P01", "恶咒: 以眼还眼" },
		{ "K26P03", "恶咒: 余痕" },
		{ "Hex_Devour_Hope", "恶咒: 吞噬希望" },
		{ "HexRetribution", "恶咒: 惩戒" },
		{ "Hex_Ruin", "恶咒: 毁灭" },
		{ "Hex_HuntressLullaby", "恶咒: 猎手摇篮曲" },
		{ "Hex_Thrill_Of_The_Hunt", "恶咒: 猎杀戾气" },
		{ "K25P02", "恶咒: 玩物" },
		{ "Hex_The_Third_Seal", "恶咒: 第三封印" },
		{ "K23P02", "恶咒: 群体控制" },
		{ "No_One_Escapes_Death", "恶咒: 难逃一死" },
		{ "K30P02", "恶咒: 直面黑暗" },
		{ "Hex_HauntedGround", "恶咒: 闹鬼之地" },
		{ "HexBloodFavor", "恶咒: 鲜血恩惠" },
		{ "BeastOfPrey", "嗜血凶兽" },
		{ "K22P01", "囤积者" },
		{ "K32P01", "基因限制" },
		{ "OverwhelmingPresence", "压迫性气场" },
		{ "K22P02", "切肤之痛" },
		{ "Nemesis", "宿敌" },
		{ "FranklinsLoss", "富兰克林之死" },
		{ "Coulrophobia", "小丑恐惧症" },
		{ "Tinkerer", "工匠" },
		{ "BoonDestroyer", "破碎希望" },
		{ "ForcedPenance", "强制忏悔" },
		{ "K32P02", "强制减速" },
		{ "MindBreaker", "心神破碎" },
		{ "pop_goes_the_weasel", "心惊肉跳" },
		{ "FireUp", "怒火中烧" },
		{ "Rancor", "怨气冲天" },
		{ "Unnerving_Presence", "恐慌降临" },
		{ "InfectiousFright", "恐惧传染" },
		{ "Distressing", "大心脏" },
		{ "K29P02", "感知觉醒" },
		{ "K28P03", "感染之触" },
		{ "Brutal_Strength", "所向无敌" },
		{ "Save_The_Best_For_Last", "把最好留到最后" },
		{ "TrailOfTorment", "折磨路径" },
		{ "K28P02", "开柜透视" },
		{ "InTheDark", "击倒出局" },
		{ "Shadowborn", "暗夜之睛" },
		{ "SpiritFury", "暴怒怨灵" },
		{ "K32P03", "机器学习" },
		{ "Gearhead", "设备发烧友" },
		{ "Agitation", "大嘴" },
		{ "K24P02", "歇斯底里" },
		{ "Thanatophobia", "死亡恐惧" },
		{ "Deathbound", "死亡枷锁" },
		{ "DeadMansSwitch", "失效开关" },
		{ "K25P01", "死锁" },
		{ "ZanshinTactics", "残心" },
		{ "CruelConfinement", "幽闭恐惧症" },
		{ "ImAllEars", "洗耳恭听" },
		{ "K27P02", "盐水召唤" },
		{ "K28P01", "烟消云散" },
		{ "Dying_Light", "油灯" },
		{ "K31P03", "渐入佳境" },
		{ "K30P01", "无处可藏" },
		{ "K23P03", "无路可逃" },
		{ "K24P03", "爆发" },
		{ "Iron_Grasp", "铁手" },
		{ "K31P02", "物破人惊" },
		{ "Deerstalker", "猎鹿者" },
		{ "K33P03", "异形本能" },
		{ "K31P01", "迅捷狩猎" },
		{ "Madgrit", "疯狂勇气" },
		{ "MonitorAndAbuse", "监控与打击" },
		{ "K27P01", "天灾钩: 狂潮之怒" },
		{ "K26P02", "天灾钩: 痛苦回响" },
		{ "K25P03", "天灾钩: 痛苦礼物" },
		{ "HangmansTrick", "刽子手的技巧" },
		{ "Monstrous_Shrine", "天灾钩: 鬼魔神龛" },
		{ "Surge", "电能震荡" },
		{ "Predator", "穷追不舍" },
		{ "Whispers", "窃窃私语" },
		{ "K33P01", "终极武器" },
		{ "Enduring", "丑脸" },
		{ "CorruptIntervention", "腐烂干预" },
		{ "K22P03", "恩赐解脱" },
		{ "K24P01", "致命追踪者" },
		{ "Bamboozle", "花言巧语" },
		{ "Bitter_Murmur", "低涩苦语" },
		{ "K27P03", "无情风暴" },
		{ "Bloodhound", "血气追踪" },
		{ "BloodWarden", "血腥狱长" },
		{ "Discordance", "冲突" },
		{ "RememberMe", "记忆犹新" },
		{ "NurseCalling", "护士的呼唤" },
		{ "K34P02", "超吉死党" },
		{ "K29P03", "赶尽杀绝" },
		{ "Sloppy_Butcher", "猪头" },
		{ "K33P02", "为杀而杀" },
		{ "K23P01", "追星狂" },
		{ "Ironmaiden", "铁处女" },
		{ "K26P01", "冷酷之拥" },
		{ "GeneratorOvercharge", "电量超载" },
		{ "Insidious", "静止隐身" },
		{ "TerritorialImperative", "领地意识" },
		{ "K30P03", "骄傲自大" },
		{ "ThrillingTremors", "惊悚战栗" },
		{ "FurtiveChase", "鬼祟追杀" },
		{ "Spies_From_The_Shadows", "鬼鸦谍影" },
		{ "BloodEcho", "鲜血回响" },
		{ "Stridor", "鸣喘" },
		{ "DarkDevotion", "黑暗奉献" },
		{ "DragonsGrip", "龙爪" },
		{ "K29P01", "超人类体能" },
		{ "K36P01", "魔网同调" },
		{ "K36P02", "致倦惊鸦" },
		{ "K36P03", "黑暗狂傲" },
		
		// 人类技能
		{ "BetterTogether", "一同离去" },
		{ "OffTheRecord", "非正式调查" },
		{ "AnyMeansNecessary", "不择手段" },
		{ "NoMither", "不痛不痒" },
		{ "Premonition", "不祥预感" },
		{ "TheMettleOfMan", "人之勇气" },
		{ "Deja_Vu", "似曾相识" },
		{ "S32P03", "来去无踪" },
		{ "Babysitter", "保姆" },
		{ "DetectivesHunch", "警探直觉" },
		{ "S40P01", "光明使者" },
		{ "S29P01", "克服难关" },
		{ "InnerStrength", "内心治疗" },
		{ "S33P03", "聚精会神" },
		{ "Breakdown", "分崩离析" },
		{ "S34P03", "加速险招" },
		{ "AfterCare", "劫后余生" },
		{ "HeadOn", "勇往直前" },
		{ "S39P02", "化学陷阱" },
		{ "S36P03", "协力: 来去无踪" },
		{ "S35P03", "协力: 步履如飞" },
		{ "Camaraderie", "同志情谊" },
		{ "FlipFlop", "反复无常" },
		{ "Kindred", "同族" },
		{ "No_One_Left_Behind", "同生共死" },
		{ "S27P01", "咬紧牙关" },
		{ "Deliverance", "善有善报" },
		{ "Solidarity", "团结一致" },
		{ "SelfSufficient", "坚不可摧" },
		{ "BuiltToLast", "经久耐用" },
		{ "RepressedAlliance", "压抑同盟" },
		{ "Visionary", "梦想家" },
		{ "S38P03", "大逆转" },
		{ "S37P02", "为生而生" },
		{ "Up_The_Ante", "好运会传染" },
		{ "LuckyBreak", "幸运喘息" },
		{ "Vigil", "守夜人" },
		{ "Calm_Spirit", "安抚生灵" },
		{ "Fixated", "走路" },
		{ "Hope", "小翅膀" },
		{ "Balanced_Landing", "平稳着陆" },
		{ "S39P01", "幸运星" },
		{ "S24P03", "强力挣扎" },
		{ "S25P02", "轰动演出" },
		{ "Plunderers_Instinct", "强盗直觉" },
		{ "S30P02", "心电感应" },
		{ "Empathy", "心灵共鸣" },
		{ "S31P01", "内在专注" },
		{ "S25P01", "成功捷径" },
		{ "QuickQuiet", "急速静谧" },
		{ "S40P02", "恩赐: 抽丝剥茧" },
		{ "S29P03", "恩赐: 指数增长" },
		{ "S28P02", "恩赐: 治疗之环" },
		{ "S28P03", "恩赐: 暗影步" },
		{ "S30P03", "恩赐: 黑暗理论" },
		{ "Distortion", "扭曲" },
		{ "Bond", "携手合作" },
		{ "BorrowedTime", "怀表" },
		{ "Dark_Sense", "暗黑感知" },
		{ "Urban_Evasion", "暴走族" },
		{ "SoleSurvivor", "最后生还者" },
		{ "LeftBehind", "末日残兵" },
		{ "S26P02", "东山再起" },
		{ "S29P02", "矫正措施" },
		{ "Technician", "技术员" },
		{ "WindowsOfOpportunity", "机遇之窗" },
		{ "S37P01", "检修除错" },
		{ "S31P02", "余光长存" },
		{ "DecisiveStrike", "毁灭打击" },
		{ "Spine_Chill", "毛骨悚然" },
		{ "Tenacity", "永不言弃" },
		{ "S28P01", "千里眼" },
		{ "BuckleUp", "准备好了" },
		{ "S34P01", "潜在能量" },
		{ "ForThePeople", "为了人民" },
		{ "S37P03", "无中生有" },
		{ "S34P02", "迷雾慧眼" },
		{ "Self_Care", "自摸" },
		{ "S36P02", "热血激昂" },
		{ "S26P03", "爆炸地雷" },
		{ "S31P03", "狂热" },
		{ "S38P02", "狂飙戏" },
		{ "Small_Game", "狩猎经验" },
		{ "WereGonnaLiveForever", "生死与共" },
		{ "S35P01", "当机立断" },
		{ "Resilience", "百折不挠" },
		{ "Saboteur", "破坏手" },
		{ "StakeOut", "破案心切" },
		{ "S33P02", "定心丸" },
		{ "Open_Handed", "稳操胜券" },
		{ "Breakout", "摩托车" },
		{ "S32P01", "窃听器" },
		{ "Botany_Knowledge", "百草" },
		{ "S26P01", "反制之力" },
		{ "DesperateMeasures", "绝境措施" },
		{ "Diversion", "声东击西" },
		{ "BoilOver", "摇摇马" },
		{ "Adrenaline", "肾上腺素" },
		{ "S39P03", "猫步" },
		{ "S36P01", "幕后玩家" },
		{ "Autodidact", "自学成才" },
		{ "S25P03", "自我保护" },
		{ "S30P01", "亡夫警言" },
		{ "Dance_with_me", "与我共舞" },
		{ "S35P02", "友谊赛" },
		{ "S27P03", "菜鸟精神" },
		{ "Poised", "蓄势待发" },
		{ "Pharmacy", "药到病除" },
		{ "BloodPact", "鲜血契约" },
		{ "Streetwise", "街头生存" },
		{ "Sprint_Burst", "冲刺爆发" },
		{ "S24P01", "评价" },
		{ "S24P02", "诡计" },
		{ "Prove_Thyself", "证明自己" },
		{ "S38P01", "超展开" },
		{ "S32P02", "越挫越勇" },
		{ "Lightweight", "羽毛" },
		{ "RedHerring", "转移焦点" },
		{ "S40P03", "迫在眉睫" },
		{ "SecondWind", "复苏之风" },
		{ "Slippery_Meat", "逃之夭夭" },
		{ "S33P01", "遇强则强" },
		{ "Wakeup", "醒醒" },
		{ "DeadHard", "DeadHard" },
		{ "Iron_Will", "钢铁意志" },
		{ "S27P02", "闪光弹" },
		{ "This_Is_Not_Happening", "难以置信" },
		{ "Lithe", "灵巧身段" },
		{ "SoulGuard", "灵魂守卫" },
		{ "Leader", "领袖群雄" },
		{ "WellMakeIt", "马到功成" },
		{ "Ace_In_The_Hole", "锦囊妙计" },
		{ "Alert", "高度警觉" },
		{ "ObjectOfObsession", "魂牵梦绕" },
		{ "S41P01", "祈愿: 结网蜘蛛" },
		{ "S41P02", "暗影之力" },
		{ "S41P03", "劣势顽抗" },
		{ "K35P03", "再接再厉" },
		{ "K35P01", "挣脱绑定" },
		{ "K35P02", "出乎意料" },
		{ "S42P01", "镜像幻术" },
		{ "S42P02", "吟诗助阵" },
		{ "S42P03", "静观其气" },
		
	}.ToImmutableDictionary();


	public static ImmutableDictionary< string, string > GetData() => Data;
	
	public static string GetPerkName( string perkFName ) {
		if ( string.IsNullOrEmpty( perkFName ) ) {
			return null;
		}
		if ( Data.TryGetValue( perkFName, out var outName ) ) {
			return outName;
		}
		return perkFName;
	}
}


public static class DBDThemes {

	private static ImmutableDictionary< string, string > Data = new Dictionary< string, string > {
		{ "_EMPTY_", string.Empty },
		{ "None", "无" },
		{ "Applepie", "荒芜之岛-格林威尔广场" },
		{ "Wormhole", "德瓦卡星密林-诺史莫号残骸" },
		{ "Quantum", "博尔戈废墟-残破广场" },
		{ "Umbra", "德瓦卡星密林-托巴登陆舰" },
		{ "Meteor", "荒芜之岛-极乐园" },
		{ "Ion", "遗弃墓地-乌鸦高塔" },
		{ "Eclipse", "浣熊市警察局" },
		{ "Wales", "寂静岭-米德维奇小学" },
		{ "Ukraine", "格伦威尔之墓-死狗酒馆" },
		{ "Qatar", "霍金斯国家实验室-地下设施" },
		{ "Kenya", "奥蒙德-奥蒙德山度假村" },
		{ "Haiti", "山岗家族地产" },
		{ "Finland", "吉迪恩肉品加工厂-游戏" },
		{ "England", "春木镇-巴德姆幼儿园" },
		{ "Swamp", "黑水沼泽" },
		{ "Boreal", "红树林" },
		{ "Hospital", "莱里疗养中心-疗程教室" },
		{ "Suburbs", "哈登菲尔德-普兰金街" },
		{ "Asylum", "克洛普瑞恩疯人院" },
		{ "Junkyard", "废旧车库" },
		{ "Farm", "寒风农场" },
		{ "Industrial", "迈克米伦庄园" },
		{ "Clear", "晴" },
		{ "Rain", "雨" },
	}.ToImmutableDictionary();
	
	
	public static ImmutableDictionary< string, string > GetData() => Data;
	
	
	public static string GetThemeName( string themeName ) {
		if ( string.IsNullOrEmpty( themeName ) ) {
			return null;
		}
		if ( Data.TryGetValue( themeName, out var outName ) ) {
			return outName;
		}
		return themeName;
	}
}


public enum EPlayerRole : byte {
	VE_None                                  = 0,
	VE_Slasher                               = 1,
	VE_Camper                                = 2,
	VE_Observer                              = 3,
	Max                                      = 4,
	EPlayerRole_MAX                          = 5
}


public enum EPlatformFlag : uint {
	None                                     = 0,
	Android                                  = 1,
	DMM                                      = 2,
	IOS                                      = 4,
	Switch                                   = 8,
	PS4                                      = 16,
	Steam                                    = 32,
	WinGDK                                   = 64,
	Xbox                                     = 128,
	PS5                                      = 1024,
	XSX                                      = 2048,
	Epic                                     = 4096,
	EPlatformFlag_MAX                        = 4097
}


public enum ECurrencyType : byte {
	None                                     = 0,
	BloodPoints                              = 1,
	FearTokens                               = 2,
	Cells                                    = 3,
	HalloweenCoins                           = 4,
	LunarNewYearCoins                        = 5,
	HalloweenEventCurrency                   = 6,
	WinterEventCurrency                      = 7,
	ECurrencyType_MAX                        = 8
}


public enum EPalletState : byte {
	Up                                       = 0,
	Falling                                  = 1,
	Fallen                                   = 2,
	Destroyed                                = 3,
	EPalletState_MAX                         = 4
}


public enum EHatchState : byte {
	Hidden                                   = 0,
	DefaultClose                             = 1,
	Opened                                   = 2,
	ForcedClose                              = 3,
	EHatchState_MAX                          = 4
}


public enum ETotemState : byte {
	Cleansed                                 = 0,
	Dull                                     = 1,
	Hex                                      = 2,
	Boon                                     = 3,
	ETotemState_MAX                          = 4
}


public enum EGender : byte {
	VE_Male                                  = 0,
	VE_Female                                = 1,
	VE_Multiple                              = 2,
	VE_NotHuman                              = 3,
	VE_Undefined                              = 4,
	VE_MAX                                   = 5
}


public enum EEndGameReason : byte {
	None                                     = 0,
	Normal                                   = 1,
	KillerLeft                               = 2,
	PlayerLeftDuringLoading                  = 3,
	KillerLeftEarly                          = 4,
	InvalidPlayerRoles                       = 5,
	LoadingTimeout                           = 6,
	EEndGameReason_MAX                       = 7
}


[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}")]
[StructLayout( LayoutKind.Sequential )]
public struct UEVector {
	public double X; // 0x0(0x8)
	public double Y; // 0x8(0x8)
	public double Z; // 0x10(0x8)
	
	public UEVector( double x, double y, double z ) {
		X = x;
	}
}


[DebuggerDisplay("Pitch: {Pitch}, Yaw: {Yaw}, Roll: {Roll}")]
[StructLayout( LayoutKind.Sequential )]
public struct UERotator {
	public double Pitch; // 0x0(0x8)
	public double Yaw;   // 0x8(0x8)
	public double Roll; // 0x10(0x8)
}


[StructLayout( LayoutKind.Sequential )]
public readonly struct FUObjectItem {
	public readonly ulong Object;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0010)] 
	public readonly byte[] Pad_0; // 0x0008(0x0010)
}


[DebuggerDisplay("Name: {ToString()} ComparisonIndex: {ComparisonIndex}")]
[StructLayout( LayoutKind.Sequential )]
public readonly struct FName {
	public readonly int ComparisonIndex;
	public readonly int Number;
	public readonly int DisplayIndex;

	public override string ToString() {
		return UEHelper.GetFNameByComparisonIndex( ComparisonIndex );
	}
}


[DebuggerDisplay("x: {X}, y: {Y}, z: {Z}, w: {W}")]
[StructLayout( LayoutKind.Sequential )]
public struct FQuat {
	public double X; // 0x0(0x8)
	public double Y;   // 0x8(0x8)
	public double Z;  // 0x10(0x8)
	public double W;  // 0x18(0x8)
}


[DebuggerDisplay("T: {Translation}, R: {Rotation}, S: {Scale3D}")]
[StructLayout( LayoutKind.Sequential )]
public struct FTransform { // 0x0060
	public FQuat Rotation;       // 0x0000(0x0020)
	public UEVector Translation; // 0x0020(0x0018)
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0008)] 
	public readonly byte[] Pad_30; // 0x0038(0x0008)
	public UEVector Scale3D;       // 0x0040(0x0018)
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0008)] 
	public readonly byte[] Pad_31; // 0x0058(0x0008)
	
	public static readonly FTransform Identity = new () {
		Rotation = new FQuat { X = 0, Y = 0, Z = 0, W = 0 },
		Translation = new UEVector { X = 0, Y = 0, Z = 0 },
		Scale3D = new UEVector { X = 1, Y = 1, Z = 1 }
	};


	public FMatrix ToMatrixWithScale() {
		FMatrix mtx;
 
		mtx.M41 = Translation.X;
		mtx.M42 = Translation.Y;
		mtx.M43 = Translation.Z;
 
		var x2 = Rotation.X + Rotation.X;
		var y2 = Rotation.Y + Rotation.Y;
		var z2 = Rotation.Z + Rotation.Z;
 
		var xx2 = Rotation.X * x2;
		var yy2 = Rotation.Y * y2;
		var zz2 = Rotation.Z * z2;
		mtx.M11 = (1.0d - (yy2 + zz2)) * Scale3D.X;
		mtx.M22 = (1.0d - (xx2 + zz2)) * Scale3D.Y;
		mtx.M33 = (1.0d - (xx2 + yy2)) * Scale3D.Z;
 
 
		var yz2 = Rotation.Y * z2;
		var wx2 = Rotation.W * x2;
		mtx.M32 = (yz2 - wx2) * Scale3D.Z;
		mtx.M23 = (yz2 + wx2) * Scale3D.Y;
 
 
		var xy2 = Rotation.X * y2;
		var wz2 = Rotation.W * z2;
		mtx.M21 = (xy2 - wz2) * Scale3D.Y;
		mtx.M12 = (xy2 + wz2) * Scale3D.X;
 
 
		var xz2 = Rotation.X * z2;
		var wy2 = Rotation.W * y2;
		mtx.M31 = (xz2 + wy2) * Scale3D.Z;
		mtx.M13 = (xz2 - wy2) * Scale3D.X;
 
		mtx.M14 = 0.0d;
		mtx.M24 = 0.0d;
		mtx.M34 = 0.0d;
		mtx.M44 = 1.0d;
 
		return mtx;
	}
}


/// <summary>
/// All indices are one-based.
/// </summary>
public struct FMatrix {
	
	public double M11, M12, M13, M14;
	public double M21, M22, M23, M24;
	public double M31, M32, M33, M34;
	public double M41, M42, M43, M44;


	public Vector3D Translation {
		get => new (M41, M42, M43);
		set {
			M41 = value.X;
			M42 = value.Y;
			M43 = value.Z;
		}
	}


	public static FMatrix D3DXMatrixMultiply( in FMatrix pM1, in FMatrix pM2 ) {
		FMatrix pOut;
		pOut.M11 = pM1.M11 * pM2.M11 + pM1.M12 * pM2.M21 + pM1.M13 * pM2.M31 + pM1.M14 * pM2.M41;
		pOut.M12 = pM1.M11 * pM2.M12 + pM1.M12 * pM2.M22 + pM1.M13 * pM2.M32 + pM1.M14 * pM2.M42;
		pOut.M13 = pM1.M11 * pM2.M13 + pM1.M12 * pM2.M23 + pM1.M13 * pM2.M33 + pM1.M14 * pM2.M43;
		pOut.M14 = pM1.M11 * pM2.M14 + pM1.M12 * pM2.M24 + pM1.M13 * pM2.M34 + pM1.M14 * pM2.M44;
		pOut.M21 = pM1.M21 * pM2.M11 + pM1.M22 * pM2.M21 + pM1.M23 * pM2.M31 + pM1.M24 * pM2.M41;
		pOut.M22 = pM1.M21 * pM2.M12 + pM1.M22 * pM2.M22 + pM1.M23 * pM2.M32 + pM1.M24 * pM2.M42;
		pOut.M23 = pM1.M21 * pM2.M13 + pM1.M22 * pM2.M23 + pM1.M23 * pM2.M33 + pM1.M24 * pM2.M43;
		pOut.M24 = pM1.M21 * pM2.M14 + pM1.M22 * pM2.M24 + pM1.M23 * pM2.M34 + pM1.M24 * pM2.M44;
		pOut.M31 = pM1.M31 * pM2.M11 + pM1.M32 * pM2.M21 + pM1.M33 * pM2.M31 + pM1.M34 * pM2.M41;
		pOut.M32 = pM1.M31 * pM2.M12 + pM1.M32 * pM2.M22 + pM1.M33 * pM2.M32 + pM1.M34 * pM2.M42;
		pOut.M33 = pM1.M31 * pM2.M13 + pM1.M32 * pM2.M23 + pM1.M33 * pM2.M33 + pM1.M34 * pM2.M43;
		pOut.M34 = pM1.M31 * pM2.M14 + pM1.M32 * pM2.M24 + pM1.M33 * pM2.M34 + pM1.M34 * pM2.M44;
		pOut.M41 = pM1.M41 * pM2.M11 + pM1.M42 * pM2.M21 + pM1.M43 * pM2.M31 + pM1.M44 * pM2.M41;
		pOut.M42 = pM1.M41 * pM2.M12 + pM1.M42 * pM2.M22 + pM1.M43 * pM2.M32 + pM1.M44 * pM2.M42;
		pOut.M43 = pM1.M41 * pM2.M13 + pM1.M42 * pM2.M23 + pM1.M43 * pM2.M33 + pM1.M44 * pM2.M43;
		pOut.M44 = pM1.M41 * pM2.M14 + pM1.M42 * pM2.M24 + pM1.M43 * pM2.M34 + pM1.M44 * pM2.M44;
		return pOut;
	}
}


[StructLayout( LayoutKind.Sequential )]
public struct FBoxSphereBounds {
	public UEVector Origin;
	public UEVector BoxExtent;
	public double SphereRadius;
}


[StructLayout( LayoutKind.Sequential )]
public record struct FMeshBoneInfo {
	public FName Name;
	public int ParentIndex;
}


[DebuggerDisplay("BoneName: {BoneName}, SocketName: {SocketName}, RelativeLocation: {RelativeLocation}, RelativeRotation: {RelativeRotation}, RelativeScale: {RelativeScale}")]
public record USkeletalMeshSocket {
	public FName SocketName;
	public FName BoneName;
	public UEVector RelativeLocation;
	public UERotator RelativeRotation;
	public UEVector RelativeScale;
	// public bool bForceAlwaysAnimated;
	// [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x7)] 
	// public byte[] Pad_31;

	public FTransform GetFTransform() {
		return new FTransform {
			Translation = RelativeLocation,
			Rotation = new FQuat { X = RelativeRotation.Pitch, Y = RelativeRotation.Roll, Z = RelativeRotation.Yaw },
			Scale3D = RelativeScale
		};
	}
}


public record USkeletalMeshSocketDumped {
    public string SocketName;
	public string BoneName;
	public UEVector RelativeLocation;
	public UERotator RelativeRotation;
	public UEVector RelativeScale;
}


[StructLayout( LayoutKind.Sequential )]
public struct FMinimalViewInfo {
	public UEVector Location;  // 0x0(0x18)
	public UERotator Rotation; // 0x18(0x18)
	public float FOV;           // 0x30(0x4)
}


[StructLayout( LayoutKind.Sequential )]
public readonly struct FCameraCacheEntry {
	// public const uint Size = 0x07D0;
	
	public readonly float Timestamp; // 0x00(0x04)
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0c)] 
	public readonly byte[] pad_4; // 0x04(0x0c)
	public readonly FMinimalViewInfo POV; // 0x10(0x5e0)
}


[StructLayout( LayoutKind.Sequential )]
public readonly struct TArray {
	public readonly ulong Data;
	public readonly int NumElements;
	public readonly int MaxElements;
}


[StructLayout( LayoutKind.Sequential )]
public readonly struct FBuiltLevelData {
	public readonly FName ThemeName;            // 0x00(0x04)
	public readonly FName ThemeWeather;         // 0x00(0x04)
	// public readonly FName AudioStateThemes;     // 0x00(0x04)
	// public readonly FName AudioStateWeather;    // 0x00(0x04)
	// public readonly FName AudioThemeEvent;      // 0x00(0x04)
	// public readonly FName AudioLimitPointEvent; // 0x00(0x04)
	// [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0038)] 
	// public readonly byte[] AudioThemeSoundBank;  // 0x00(0x04)
	// public readonly TArray MapName;
}


public struct UEViewMatrix {
	
	
	public double[,] Matrix = new double[ 4, 4 ];
	
	public UEViewMatrix() {}

	public UEVector Transform( in UEVector vector ) {
		return default;
	}
	
	public static Vector2D WorldToScreen( in FMinimalViewInfo viewInfo, Vector3D world, Vector2I resolution ) {
		var screenLocation = Vector2D.Zero;
		var rot = new Vector3D( viewInfo.Rotation.Pitch, viewInfo.Rotation.Yaw, viewInfo.Rotation.Roll );
		var campos = new Vector3D( viewInfo.Location.X, viewInfo.Location.Y, viewInfo.Location.Z );
		// var tempMatrix = CreateMatrix( new UEVector( rot.X, rot.Y, rot.Z ), new UEVector( 0, 0, 0 ) );

		Matrix44D CreateMatrix( Vector3D rot, Vector3D origin ) {
			double radPitch = Mathf.ToRadians( rot.X );
			double radYaw = Mathf.ToRadians( rot.Y );
			double radRoll = Mathf.ToRadians( rot.Z );

			double SP = Math.Sin( radPitch );
			double CP = Math.Cos( radPitch );
			double SY = Math.Sin( radYaw );
			double CY = Math.Cos( radYaw );
			double SR = Math.Sin( radRoll );
			double CR = Math.Cos( radRoll );

			Matrix44D viewMatrix = new ();
			viewMatrix[0, 0] = CP * CY;
			viewMatrix[0, 1] = CP * SY;
			viewMatrix[0, 2] = SP;
			viewMatrix[0, 3] = 0;

			viewMatrix[1, 0] = SR * SP * CY - CR * SY;
			viewMatrix[1, 1] = SR * SP * SY + CR * CY;
			viewMatrix[1, 2] = -SR * CP;
			viewMatrix[1, 3] = 0;

			viewMatrix[2, 0] = -(CR * SP * CY + SR * SY);
			viewMatrix[2, 1] = CY * SR - CR * SP * SY;
			viewMatrix[2, 2] = CR * CP;
			viewMatrix[2, 3] = 0;

			viewMatrix[3, 0] = origin.X;
			viewMatrix[3, 1] = origin.Y;
			viewMatrix[3, 2] = origin.Z;
			viewMatrix[3, 3] = 1.0;

			return viewMatrix;
		}

		var mtx = CreateMatrix( rot, Vector3D.Zero );

		var vAxisX = new Vector3D( mtx[ 0, 0 ], mtx[ 0, 1 ], mtx[ 0, 2 ] );
		var vAxisY = new Vector3D( mtx[ 1, 0 ], mtx[ 1, 1 ], mtx[ 1, 2 ] );
		var vAxisZ = new Vector3D( mtx[ 2, 0 ], mtx[ 2, 1 ], mtx[ 2, 2 ] );

		var vDelta = world - campos;
		var vTransformed = new Vector3D(
			Vector3D.Dot( vDelta, vAxisY ),
			Vector3D.Dot( vDelta, vAxisZ ),
			Vector3D.Dot( vDelta, vAxisX )
		);

		if ( vTransformed.Z < 1 ) {
			vTransformed.Z = 1;
		}

		const float FOV_DEG_TO_RAD = ConstantsF.Pi / 360f;
		var centrex = resolution.X / 2;
		var centrey = resolution.Y / 2;
		screenLocation.X = centrex + vTransformed.X * ( centrex / Math.Tan(
			viewInfo.FOV * FOV_DEG_TO_RAD ) ) / vTransformed.Z;
		screenLocation.Y = centrey - vTransformed.Y * ( centrex / Math.Tan(
			viewInfo.FOV * FOV_DEG_TO_RAD ) ) / vTransformed.Z;

		return screenLocation;
	}
}
