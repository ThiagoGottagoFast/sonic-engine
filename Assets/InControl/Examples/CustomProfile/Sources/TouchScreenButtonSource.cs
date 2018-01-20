using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class TouchScreenButtonSource : InputControlSource {
	string touchButtonQuery;


	public TouchScreenButtonSource( string axis )
	{
		touchButtonQuery = axis;
	}


	public float GetValue( InputDevice inputDevice )
	{
		return GetState(inputDevice) ? 1f : 0f;
	}


	public bool GetState( InputDevice inputDevice )
	{
		return CrossPlatformInputManager.GetButton(touchButtonQuery);
	}
}
