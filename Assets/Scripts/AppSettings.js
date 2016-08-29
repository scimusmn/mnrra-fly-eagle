#pragma strict

function Awake () {

    // Attempt to unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();
	print('AWAKE - Unloading Unused Resources...');

	ForceFrameRate();

}

function ForceFrameRate() {
	
	QualitySettings.vSyncCount = 0;  // VSync must be disabled or disable in quality manually 
    Application.targetFrameRate = 60;

}

function OnDestroy () {

    // Unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();

}
