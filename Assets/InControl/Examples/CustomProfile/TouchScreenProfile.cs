using System;
using System.Collections;
using UnityEngine;
using InControl;


namespace CustomProfileExample
{
	// This custom profile is enabled by adding it to the Custom Profiles list
	// on the InControlManager component, or you can attach it yourself like so:
	// InputManager.AttachDevice( new UnityInputDevice( "KeyboardAndMouseProfile" ) );
	//
	public class TouchScreenProfile : UnityInputDeviceProfile
	{
		public TouchScreenProfile()
		{
			Name = "Touchscreen";
			Meta = "A control scheme for CrossPlatformInput's touchscreen controls";

			// This profile only works on desktops.
			SupportedPlatforms = new[]
			{
				"Android",
				"iPhone"
			};

			Sensitivity = 1.0f;
			LowerDeadZone = 0.0f;
			UpperDeadZone = 1.0f;

			ButtonMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Jump",
					Target = InputControlType.Action1,
					Source = new TouchScreenButtonSource("Jump")
				},
				new InputControlMapping
				{
					Handle = "Back",
					Target = InputControlType.Action2,
					Source = new TouchScreenButtonSource("Back")
				},
				new InputControlMapping
				{
					Handle = "Dpad left",
					Target = InputControlType.DPadLeft,
					Source = KeyCodeButton( KeyCode.A, KeyCode.LeftArrow )
				},
				new InputControlMapping
				{
					Handle = "Dpad right",
					Target = InputControlType.DPadRight,
					Source = KeyCodeButton( KeyCode.D, KeyCode.RightArrow )
				},
				new InputControlMapping
				{
					Handle = "Dpad down",
					Target = InputControlType.DPadDown,
					Source = KeyCodeButton( KeyCode.S, KeyCode.DownArrow )
				},
				new InputControlMapping
				{
					Handle = "Dpad up",
					Target = InputControlType.DPadUp,
					Source = KeyCodeButton( KeyCode.W, KeyCode.UpArrow )
				}
			};

			AnalogMappings = new[]
			{
				new InputControlMapping
				{
					Handle = "Look X",
					Target = InputControlType.RightStickX,
					Source = new TouchScreenAxisSource("Horizontal")
				},
				new InputControlMapping
				{
					Handle = "Look Y",
					Target = InputControlType.RightStickY,
					Source = new TouchScreenAxisSource("Vertical")
				}
			};
		}
	}
}

