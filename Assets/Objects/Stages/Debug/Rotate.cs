using UnityEngine;

public class Rotate : MonoBehaviour{
	private float rot;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update (){
		var transformRotation = transform.rotation.eulerAngles;
		rot += Time.deltaTime * 20;
		transformRotation.z = rot%360;
		transform.rotation = Quaternion.Euler(transformRotation);
	}
}
