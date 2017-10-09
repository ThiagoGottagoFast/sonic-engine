using UnityEngine;

public class RingScript : MonoBehaviour{
	public bool SuperRing;
	public bool LostRing;
	public ushort rotationAmount;
	public AudioSource AudioSource;
	public AudioClip RingSound;

	// Update is called once per frame
	private void Update (){
		var transformRotation = transform.rotation;
		transformRotation.eulerAngles = new Vector3(0, rotationAmount++);
		transform.rotation = transformRotation;
		rotationAmount %= 360;
	}

	private void OnTriggerEnter(Collider hit){
		Debug.Log(hit.gameObject.tag);
		if(hit.gameObject.tag.ToLower() == "player"){
			AudioSource.PlayOneShot(RingSound, 0.5f);
			// Spawn sparkle effects
			SonicEngine.Base.Rings += (ushort)(SuperRing ? 10 : 1);
			Destroy(gameObject);
		}
	}
}
