#pragma strict

import System;
import System.IO;
 
var logPath:String = "Assets/Resources/Logger/";

private var logFileName:String = '';
private var writeStream:StreamWriter;

function Awake() {

	var nowTime:String = System.DateTime.Now.ToString("hh-mm-ss"); 
    var nowDate:String = System.DateTime.Now.ToString("yyyy-MM-dd");

	logFileName = 'LOG-'+ nowDate + '-' + nowTime + '.txt';

	writeStream = File.CreateText(logPath + logFileName); // Returns Stream Writer

}
 
function Start() {

	
}

function Log(logMsg:String) {

	var nowTime:String = System.DateTime.Now.ToString("hh:mm:ss");

	logMsg = '['+nowTime+']:' + logMsg;

	if ( writeStream != null ) {
		print('logging: '+ logMsg);
		writeStream.WriteLine( logMsg );
		writeStream.Flush();
	}

}

function OnDestroy() {
	print('OnDestroy');
	if ( writeStream != null ) {
		writeStream.Close();
		writeStream = null;
	}
}