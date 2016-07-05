#pragma strict

public var from:Vector3 = new Vector3(0f, 0f, 0f);
public var to:Vector3   = new Vector3(0f, 0f, 0f);
public var speed:float = 1.0f;

function Start () {

}

function Update() {
	//linear
     //var t:float = Mathf.PingPong(Time.time * speed * 2.0f, 1.0f);
     //smoothed sine
     var t:float = (Mathf.Sin (Time.time * speed * Mathf.PI * 2.0f) + 1.0f) / 2.0f;
     transform.localEulerAngles = Vector3.Lerp (from, to, t);
     
 }