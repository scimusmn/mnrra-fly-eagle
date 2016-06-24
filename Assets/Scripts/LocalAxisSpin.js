var DegreesX : float = 0.0f;
var DegreesY : float = 0.0f;
var DegreesZ : float = 0.0f;

function Update() {
	
    transform.Rotate(DegreesX * Time.deltaTime, DegreesY * Time.deltaTime, DegreesZ * Time.deltaTime);

}