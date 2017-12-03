using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MaterialScroller : MonoBehaviour{
	public Vector2 speed;
	public Material material;

	// Update is called once per frame
	void Update () {
		material.mainTextureOffset += speed * Time.deltaTime;
	}
}
