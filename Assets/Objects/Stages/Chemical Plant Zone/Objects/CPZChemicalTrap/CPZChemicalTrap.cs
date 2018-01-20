using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CPZChemicalTrap : MonoBehaviour{
	public Vector3 LeftPos, RightPos, CenterL, CenterR;
	public float Hight;
	public float Width;
	public float CurrentTime;
	public bool LeftSide = true;
	public bool Enabled;
	public GameObject ChemicalBlob;

	// Use this for initialization
	void Start (){
		LeftPos = transform.position;
		RightPos = transform.GetChild(0).position;
		CenterL = (LeftPos + RightPos) / 2;
		CenterL.y += Hight;	//CPZChemicalHight;
		CenterR = CenterL;
		CenterL.x -= Width;
		CenterR.x += Width;
		ChemicalBlob = Instantiate(ChemicalBlob);
		ChemicalBlob.transform.position = LeftPos;
	}

	// Update is called once per frame
	void Update (){
		CurrentTime += Time.deltaTime/2;
		if(CurrentTime > 1){
			if(Enabled){
				LeftSide = !LeftSide;
			}
			Enabled = !Enabled;
			CurrentTime = 0;
		}

		if(Enabled){
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
		Vector3 ret = uuu * LeftPos;
		ret += 3 * uu * time * CenterL;
		ret += 3 * u * timeS * CenterR;
		ret += timeC * RightPos;
		return ret;
	}
}
