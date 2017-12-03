using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CPZChemicalTrap : MonoBehaviour{
	public Vector3 leftPos, rightPos, CenterL, CenterR;
	public float Hight;
	public float Width;
	public float CurrentTime;
	public bool LeftSide = true;
	public bool enabled;
	public GameObject ChemicalBlob;

	// Use this for initialization
	void Start (){
		leftPos = transform.position;
		rightPos = transform.GetChild(0).position;
		CenterL = (leftPos + rightPos) / 2;
		CenterL.y += Hight;	//CPZChemicalHight;
		CenterR = CenterL;
		CenterL.x -= Width;
		CenterR.x += Width;
		ChemicalBlob = Instantiate(ChemicalBlob);
		ChemicalBlob.transform.position = leftPos;
	}

	// Update is called once per frame
	void Update (){
		CurrentTime += Time.deltaTime/2;
		if(CurrentTime > 1){
			if(enabled){
				LeftSide = !LeftSide;
			}
			enabled = !enabled;
			CurrentTime = 0;
		}

		if(enabled){
			ChemicalBlob.transform.position = LeftSide ? CalculateBezierCurve(CurrentTime) : CalculateBezierCurve(1 - CurrentTime);
		}
	}

	Vector3 CalculateBezierCurve(float time){
		float u, uu, uuu, timeS, timeC;
		u = 1 - time;
		uu = u * u;
		uuu = uu * u;
		timeS = time * time;
		timeC = timeS * time;
		Vector3 ret = uuu * leftPos;
		ret += 3 * uu * time * CenterL;
		ret += 3 * u * timeS * CenterR;
		ret += timeC * rightPos;
		return ret;
	}
}
