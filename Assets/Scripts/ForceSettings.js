#pragma strict

function Awake () {

	QualitySettings.vSyncCount = 0;  // VSync must be disabled or disable in quality manually 
    Application.targetFrameRate = 60;

}
