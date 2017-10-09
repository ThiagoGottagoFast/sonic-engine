using UnityEngine;

public class Rotate : MonoBehaviour{
	private int rot;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update (){
		var transformRotation = transform.rotation.eulerAngles;
		transformRotation.z = rot++%360;
		transform.rotation = Quaternion.Euler(transformRotation);
	}
}
