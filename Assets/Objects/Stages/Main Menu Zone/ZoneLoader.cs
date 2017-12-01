using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using InControl;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZoneLoader : MonoBehaviour {

	#region Menu
	public TextMeshProUGUI ZoneSelection;
	public Image image;
	public TextMeshProUGUI SonicEngine;
	public TextMeshProUGUI Instructions;
	#endregion

	public TextMeshProUGUI LevelName;
	public Image Backdrop;

	public Transform TitleCard;
	private bool loadScene;


	public InputDevice InputDevice;
	private String[] Levels = {
		"Test",
		"Chemical Plant"
	};

	private int scene;

	private void Start(){
		scene = 0;
		TitleCard.localScale = Vector3.zero;
	}

	// Updates once per frame
	void Update() {
		InputDevice = InputManager.ActiveDevice;

		// If the new scene has started loading...
		if(loadScene){

			ZoneSelection.text = string.Empty;
			image.enabled = false;
			SonicEngine.text = string.Empty;
			Instructions.text = string.Empty;
			FindObjectOfType<Camera>().clearFlags = CameraClearFlags.SolidColor;
			TitleCard.localScale = Vector3.one;

			LevelName.text = Levels[scene];

			Backdrop.rectTransform.sizeDelta = new Vector2(LevelName.preferredWidth, Backdrop.rectTransform.sizeDelta.y);
			// ...then pulse the transparency of the loading text to let the player know that the computer is still working.
			//loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));

		} else{
			// If the player has pressed the space bar and a new scene is not loading yet...
			if((InputDevice.Action1 || InputDevice.Action2 || InputDevice.Action3 || InputDevice.Action4) &&
			   loadScene == false){

				// ...set the loadScene boolean to true to prevent loading a new scene more than once...
				loadScene = true;

				// ...and start a coroutine that will load the desired scene.
				StartCoroutine(LoadNewScene());

			}
			else{
				var up = InputDevice.DPadUp.WasPressed;
				var down = InputDevice.DPadDown.WasPressed;
				if(up ^ down){
					if(up){
						if(++scene >= Levels.Length){
							scene = 0;
						}
					}
					else{
						if(--scene < 0){
							scene = Levels.Length - 1;
						}
					}
				}
			}

			ZoneSelection.text = Levels[scene] + " Zone";
		}

	}


	// The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
	IEnumerator LoadNewScene() {

		// This line waits for 3 seconds before executing the next line in the coroutine.
		// This line is only necessary for this demo. The scenes are so simple that they load too fast to read the "Loading..." text.
		yield return new WaitForSeconds(3);

		// Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
		AsyncOperation async = SceneManager.LoadSceneAsync(scene+1);

		// While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
		while (!async.isDone) {
			yield return null;
		}

	}

}
