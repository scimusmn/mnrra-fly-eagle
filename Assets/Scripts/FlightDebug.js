#pragma strict

private var debugLineAngle : CameraLine;
private var debugLineAngle2 : CameraLine;
private var debugLineHeight : CameraLine;

public var birdFlight : BirdFlight;

function Start () {

	var debugLines = FindObjectOfType(CameraLines);

	var gridColor : Color = Color(0.2, 0.3, 0.4, 0.5);

	debugLines.AddLine(gridColor, 0.85, 0.85, 0.85, 0.95);
	debugLines.AddLine(gridColor, 0.85, 0.85, 0.95, 0.85);

	debugLineAngle = debugLines.AddLine(Color.yellow);
	debugLineAngle2 = debugLines.AddLine(Color.yellow);
	debugLineHeight = debugLines.AddLine(Color.cyan);

	if (!birdFlight) {
		Debug.LogWarning("Please assign bird flight component target.");
	}

}

function Update () {

	var xAngle = Utils.Map(birdFlight.wingSpanAngle, -1, 1, 0.85, 0.95);
	var yHeight = Utils.Map(birdFlight.wingSpanPitch, -1, 1, 0.85, 0.95);

	debugLineHeight.Update(0.85, yHeight, 0.95, yHeight);

	var angle : float = birdFlight.targetRollRotation.eulerAngles.z * Mathf.Deg2Rad;
	angle += Mathf.PI / 2;

	var x : float = Mathf.Sin(angle) * 0.05;
  	var y : float = Mathf.Cos(angle) * 0.05;

	debugLineAngle.Update(0.9, 0.9, 0.9 + x, 0.9 + y);
	debugLineAngle2.Update(0.9, 0.9, 0.9 - x, 0.9 - y);

}
