using UnityEngine;

namespace SonicEngine{
	public class Shield : MonoBehaviour{

		public Base Character;
		public GameObject Sphere;
		public short Rotate;

		
		public static void createShield<T>(Base character) where T : Shield{
			if(character.Shield != null){
				Destroy(character.Shield.Sphere.gameObject);
			}
			T shield = character.gameObject.AddComponent<T>();
			character.Shield = shield;
			shield.Character = character;
		}

		private void Start(){
			Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Sphere.GetComponent<Renderer>().material.color = new Color(8, 109, 195);
			Sphere.GetComponent<SphereCollider>().enabled = false;
			AudioClip shieldSound = Resources.Load<AudioClip>("BlueShield");
			Character.AudioSource.PlayOneShot(shieldSound);
			transform.SetParent(Character.Transform, false);
			Sphere.transform.SetParent(transform.parent, false);
		}

		private void Update(){
			var rot = Sphere.transform.eulerAngles;
			rot.y = Rotate++;
			Sphere.transform.eulerAngles = rot;
			Rotate %= 360;
		}

		private void OnDestroy(){
			Character.AudioSource.PlayOneShot(Character.DieSound);
		}

		public virtual void onJumpAction(){}

		public virtual bool damage(Damage damage){
			if(damage.DamageType != DamageType.Projectile){
				return true;
			}
			bounceProjectile(ref damage.Projectile);
			return false;
		}

		// ReSharper disable once VirtualMemberNeverOverridden.Global
		public virtual void bounceProjectile(ref Projectile projectile){
			var vel = projectile.Rigidbody.velocity;
			vel.x *= -1;
			vel.y *= -1;
			projectile.Rigidbody.velocity = vel;
		}
	}
}
