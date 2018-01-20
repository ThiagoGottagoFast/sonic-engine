using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class TouchScreenAxisSource : InputControlSource {
	string touchAxisQuery;


	public TouchScreenAxisSource( string axis )
	{
		touchAxisQuery = axis;
	}


	public float GetValue( InputDevice inputDevice )
	{
		return CrossPlatformInputManager.GetAxis( touchAxisQuery );
	}


	public bool GetState( InputDevice inputDevice )
	{
		return !Mathf.Approximately( GetValue( inputDevice ), 0.0f );
	}
}
