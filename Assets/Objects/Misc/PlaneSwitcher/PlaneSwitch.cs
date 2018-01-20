using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlaneSwitch : MonoBehaviour{
	public GameObject LinkedPlane;

	private void Update(){
		if(LinkedPlane != null){
			LinkedPlane.transform.localEulerAngles = transform.localEulerAngles;
		}
	}

	private void OnTriggerEnter(Collider player){
		if(player.CompareTag("Player")){
			var vec3 = player.transform.position;
			vec3.z = transform.position.z;
			player.transform.position = vec3;
			vec3 = player.transform.rotation.eulerAngles;
			vec3.y = transform.rotation.eulerAngles.y;
			player.transform.rotation = Quaternion.Euler(vec3);
		}
	}
}
