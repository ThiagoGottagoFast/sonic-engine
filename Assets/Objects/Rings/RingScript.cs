using System;
using UnityEngine;
using UnityEngine.Audio;

public class RingScript : MonoBehaviour{
	public bool SuperRing;
	public float rotation;
	public byte rotateAmount = 1;
	public AudioSource AudioSource;
	public AudioClip RingSound;
	public new Rigidbody rigidbody;
	public SphereCollider sphereCollider;
	
#region LostRing
	public bool LostRing;
	public float LifeTime = 4.2666666f;
	public bool collected;
	public bool collectable;
#endregion

	private void Awake(){
		AudioSource = gameObject.AddComponent<AudioSource>();
		AudioSource.outputAudioMixerGroup = Resources.Load<AudioMixerGroup>("Sound/SFX");
		rigidbody = GetComponent<Rigidbody>();
		sphereCollider = GetComponent<SphereCollider>();
		if(LostRing){
			InitLostRing();
		}
	}

	public void InitLostRing(){
		sphereCollider.isTrigger = false;
		rotateAmount = 4;
	}

	// Update is called once per frame
	private void Update (){
		if(LostRing){
			if((LifeTime -= Time.deltaTime) >= 0){
				rotateAmount = (byte)(LifeTime * 2);
			}
			else{
				if(!collected){
					Destroy(gameObject);
				}
			}
		}
		var transformRotation = transform.rotation;
		rotation += rotateAmount * 100 * Time.deltaTime;
		transformRotation.eulerAngles = new Vector3(0, rotation);
		transform.rotation = transformRotation;
		rotation %= 360;
	}

	private void OnTriggerEnter(Collider hit){
		if(!collected && (collectable || !LostRing)){
			isTouched(hit);
		}
	}

	private void OnTriggerExit(Collider hit){
		if(LostRing && hit.CompareTag("Player")){
			collectable = true;
		}
	}

	private void isTouched(Collider hit){
		Debug.Log(hit.gameObject.tag);
		if(hit.CompareTag("Player")){
			collected = true;
			AudioSource.PlayOneShot(RingSound, 0.5f);
			// Spawn sparkle effects
			SonicEngine.Base.Rings += (ushort)(SuperRing ? 10 : 1);
			transform.localScale = Vector3.zero;
			sphereCollider.enabled = false;
			rigidbody.detectCollisions = false;
			Destroy(gameObject, 0.6f);
		}
	}
}
