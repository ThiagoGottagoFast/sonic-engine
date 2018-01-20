using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CPZChemicalTrap))]
public class CPZChemicalTrapEditor : Editor {
	private void OnSceneGUI(){
		CPZChemicalTrap trap = target as CPZChemicalTrap;
		if(trap == null) return;
		Handles.color = Color.white;
		Vector3 centerL, centerR;
		centerL = (trap.LeftPos + trap.RightPos) / 2;
		centerL.y += trap.Hight;	//CPZChemicalTrap.Hight;
		centerR = centerL;
		centerL.x -= trap.Width;
		centerR.x += trap.Width;
		Handles.DrawBezier(trap.LeftPos, trap.RightPos, centerL, centerR, Color.blue, null, 5f);
		//Handles.DrawPolyLine(trap.leftPos, centerL, centerR, trap.rightPos);
	}
}
