#pragma strict

public var variation:Vector3 = new Vector3(0f, 0f, 0f);

public var speed:float = 1;

private var startPos:Vector3;

function Start () {
	startPos = transform.position;
}

function Update () {
    // Set the position to loop between starting position and (starting position + variation)
    if (variation.x != 0f) transform.position.x = Mathf.PingPong(Time.time * speed, variation.x) + startPos.x;
    if (variation.y != 0f) transform.position.y = Mathf.PingPong(Time.time * speed, variation.y) + startPos.y;
    if (variation.z != 0f) transform.position.z = Mathf.PingPong(Time.time * speed, variation.z) + startPos.z;
}