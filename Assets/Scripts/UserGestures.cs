using UnityEngine;
//using Windows.Kinect;
using System.Collections;
using System;

// NOTE - This is not being used currently,
// instead manually tracking "T Pose" 
// Seems the tPose particularily isn't recognized well.

public class UserGestures : MonoBehaviour, KinectGestures.GestureListenerInterface {

	public void UserDetected(long userId, int userIndex) {
		
		KinectManager manager = KinectManager.Instance;
		manager.DetectGesture(userId, KinectGestures.Gestures.Tpose);
		Debug.Log ("Gestures added ----+");
	}

	public void UserLost(long userId, int userIndex) {
		Debug.Log ("USer lost ----+");
	}

	public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos) {
		Debug.Log ("Gesture progress ----+"+ gesture);
	}

	public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos) {
		
		Debug.Log(gesture + " detected --+--");
		return true;

	}

	public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint) {
		Debug.Log ("Gesture cancelled ----+"+ gesture);
		return true;

	}

}
