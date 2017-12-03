using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SimpleRotate : MonoBehaviour{
	public Vector3 Rotate = Vector3.zero;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		transform.localRotation *= Quaternion.Euler(Rotate*Time.deltaTime);
	}
}
