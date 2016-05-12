#pragma strict
import System.Collections.Generic;

/*
 *
 * Place this script on a camera to draw 2D lines.
 * for visual debugging. 
 *
 * Example Usage (From external component):
 * 		var myLine : CameraLine = FindObjectOfType(CameraLines).AddLine('myDebugLine', Color.red);
 *		myLine.start = Vector3(0.8, 0.4, 0);
 *		myLine.end = Vector3(0.0, 0.8, 0);
 *
*/

private var cameraLines = new List.<CameraLine>();
private var lineMaterial : Material;

function Start () {

	lineMaterial = new Material(Shader.Find("Particles/Alpha Blended")); 

}

public function AddLine(color : Color, x1 : float, y1 : float, x2 : float, y2 : float) {

	var cl : CameraLine = new CameraLine(color, Vector3(x1, y1, 0.0), Vector3(x2, y2, 0.0));

	cameraLines.Add(cl);

	return cl;

}

public function AddLine(color : Color) {

	var cl : CameraLine = new CameraLine(color, Vector3(0, 0, 0), Vector3(0, 0, 0));

	cameraLines.Add(cl);

	return cl;

}

private class CameraLine {

	public var color: Color;
	public var start : Vector3;
	public var end : Vector3;

	public function CameraLine (_color : Color, _start : Vector3, _end : Vector3) {

		color = _color;
		start = _start;
		end = _end;

	}

	public function Update (x1 : float, y1 : float, x2 : float, y2 : float) {

		start = Vector3(x1, y1, 0.0);
		end = Vector3(x2, y2, 0.0);

	}

}

function OnPostRender() {

	GL.PushMatrix();
	lineMaterial.SetPass(0);
	GL.LoadOrtho();

	var cl:CameraLine;
	for(var i : int = 0; i < cameraLines.Count; i++) {
        cl = cameraLines[i];

        GL.Begin(GL.LINES);
		GL.Color(cl.color);
		GL.Vertex(cl.start);
		GL.Vertex(cl.end);
		GL.End();

    }

	GL.PopMatrix();

}

function OnApplicationQuit(){
	DestroyImmediate(lineMaterial);
}