#pragma strict

public var warningMessage : String = 'Warning - Stay near the river';

function Start () {

}

function Update () {

}

function OnTriggerEnter(Collider other) {
  print('Object ENTERED Trigger');
}

function OnTriggerStay(Collider other) {
  print('Object staying within Trigger');
}

function OnTriggerExit(Collider other) {
  print('Object EXITED Trigger');
}