using UnityEngine;
using UnityEngine.Audio;

public class RingScript : MonoBehaviour{
	public bool SuperRing;
	public bool LostRing;
	public bool collected;
	public ushort rotation;
	public byte rotateAmount = 1;
	public AudioSource AudioSource;
	public AudioClip RingSound;
	public Rigidbody rigidbody;
	public SphereCollider sphereCollider;

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
		var transformRotation = transform.rotation;
		transformRotation.eulerAngles = new Vector3(0, rotation+=rotateAmount);
		transform.rotation = transformRotation;
		rotation %= 360;
	}

	private void OnTriggerEnter(Collider hit){
		if(!collected){
			isTouched(hit);
		}
	}

	/*private void OnCollisionEnter(Collision hit){
		isTouched(hit.collider);
	}*/

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
