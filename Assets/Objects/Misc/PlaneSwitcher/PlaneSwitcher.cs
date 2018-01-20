using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class PlaneSwitcher : MonoBehaviour{
	public GameObject Left, Right;
	public TextMeshPro LeftText, RightText;
	public string format = "Depth: {0}\nRot: {1}";
	public static int rayLength = 5;

	// Update is called once per frame
	void Update(){
		float z = Left.transform.position.z,
		      rot = Left.transform.rotation.eulerAngles.y;
		LeftText.text = string.Format(format,
		                              z,
		                              rot > 180 ? rot - 360 : rot);
		z = Right.transform.position.z;
		rot = Right.transform.rotation.eulerAngles.y;
		RightText.text = string.Format(format,
		                               z,
		                               rot > 180 ? rot - 360 : rot);
		Color switcherColor = new Color(0xFF, 0xB6, 0x00, 0x48);
		Debug.DrawRay(Left.transform.position, (Left.transform.rotation * Vector3.left)*rayLength, switcherColor);
		Debug.DrawRay(Right.transform.position, (Right.transform.rotation * Vector3.right)*rayLength, switcherColor);
	}
}
