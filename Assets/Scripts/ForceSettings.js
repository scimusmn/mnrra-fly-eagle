#pragma strict

function Awake () {

    // Attempt to unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();
	print('AWAKE - Unloading Unused Resources...');

	ForceFrameRate();

}

function ForceFrameRate() {
	print('ForceFrameRate - 60fps');
	QualitySettings.vSyncCount = 0;  // VSync must be disabled or disable in quality manually 
    Application.targetFrameRate = 60;

}

function OnDestroy () {

    // Attempt to unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();
	print('ONDESTROY - Unloading Unused Resources...');

}
