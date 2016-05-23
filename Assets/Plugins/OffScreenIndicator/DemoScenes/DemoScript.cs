using UnityEngine;
using System.Collections;
using Greyman;

public class DemoScript : MonoBehaviour {

	public OffScreenIndicator offScreenArrow;

	public void AddTarget(){
		GameObject cubeToInstantiate = GameObject.Find("Cube");
		GameObject newCube = GameObject.Instantiate(cubeToInstantiate);
		//position it at random place
		newCube.transform.localPosition = new Vector3(Random.Range(-50, 50), 1, Random.Range(-50, 50));
		//add the ArrowIndicatorStuff
		offScreenArrow.AddIndicator(newCube.transform, Random.Range(0, offScreenArrow.indicators.Length));
	}

	void Update(){
		if (Input.GetKeyUp(KeyCode.Q)){
			AddTarget();
		}
	}
}
