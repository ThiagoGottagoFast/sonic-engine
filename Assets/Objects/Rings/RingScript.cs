using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[ExecuteInEditMode]
public class RingScript : MonoBehaviour{
	public bool SuperRing;
	public float LostRotation;
	public static float Rotation;
	public byte RotateAmount = 1;
	public AudioSource AudioSource;
	public AudioClip RingSound;
	public Rigidbody Rigidbody;
	public SphereCollider SphereCollider;
	public static Coroutine RingRotateCoroutine;

#region LostRing
	public bool LostRing;
	public float LifeTime = 4.2666666f;
	public bool Collected;
	public bool Collectable;
#endregion

	private void Awake(){
		if(!AudioSource){
			AudioSource = gameObject.AddComponent<AudioSource>();
		}
		else{
			long count = 0;
			foreach(AudioSource source in GetComponents<AudioSource>()){
				if(source == AudioSource){
					continue;
				}
				DestroyImmediate(source);
				count++;
			}
			Debug.Log(string.Format("Removed {0} AudioSources", count));
		}

		AudioSource.outputAudioMixerGroup = Resources.Load<AudioMixerGroup>("Sound/SFX");
		Rigidbody = GetComponent<Rigidbody>();
		SphereCollider = GetComponent<SphereCollider>();
		transform.rotation = new Quaternion(0, 0, 0, 0);
		transform.localRotation = new Quaternion(0, 0, 0, 0);
		if(LostRing){
			initLostRing();
		}
		else{
			if(RingRotateCoroutine == null){
				RingRotateCoroutine = StartCoroutine(ringRotate());
			}
		}
	}

	public static IEnumerator ringRotate(){
		for(;;){
			yield return new WaitForEndOfFrame();
			Rotation += Time.deltaTime * 100;
		}
	}

	public void initLostRing(){
		SphereCollider.isTrigger = false;
		RotateAmount = 4;
	}

	// Update is called once per frame
	private void Update (){
		if(LostRing){
			if((LifeTime -= Time.deltaTime) >= 0){
				RotateAmount = (byte)(LifeTime * 2);
				LostRotation += RotateAmount * Time.deltaTime * 100;
				LostRotation %= 360;
			}
			else{
				if(!Collected){
					Destroy(gameObject);
				}
			}
		}
		var transformRotation = transform.localRotation;
		transformRotation.eulerAngles = new Vector3(0, LostRing ? LostRotation : Rotation);
		transform.localRotation = transformRotation;
	}

	private void OnTriggerEnter(Collider hit){
		if(!Collected && (Collectable || !LostRing)){
			isTouched(hit);
		}
	}

	private void OnTriggerExit(Collider hit){
		if(LostRing && hit.CompareTag("Player")){
			Collectable = true;
		}
	}

	private void isTouched(Collider hit){
		Debug.Log(hit.gameObject.tag);
		if(hit.CompareTag("Player")){
			Collected = true;
			AudioSource.PlayOneShot(RingSound, 0.5f);
			// Spawn sparkle effects
			SonicEngine.Base.Rings += (ushort)(SuperRing ? 10 : 1);
			transform.localScale = Vector3.zero;
			SphereCollider.enabled = false;
			Rigidbody.detectCollisions = false;
			Destroy(gameObject, 0.6f);
		}
	}
}
