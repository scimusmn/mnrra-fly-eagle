﻿#pragma strict

function Awake () {

	QualitySettings.vSyncCount = 0;  // VSync must be disabled or disable in quality manually 
    Application.targetFrameRate = 60;

    // Attempt to unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();
	print('AWAKE - Unloading Unused Resources...');

}

function OnDestroy () {

    // Attempt to unload resources
	Resources.UnloadUnusedAssets();
	System.GC.Collect();
	print('ONDESTROY - Unloading Unused Resources...');

}
