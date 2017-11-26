using SonicEngine;
using UnityEngine;

public class FireShield : Shield {

	// Use this for initialization
	void Start () {
		Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Sphere.GetComponent<Renderer>().material.color = new Color(1f, 0.6f, 0);
		Sphere.GetComponent<SphereCollider>().enabled = false;
		
	}

	public override void onJumpAction(){
		var vel = new Vector3(7, 0);
		if(Character.IsFlipped){
			vel.x = -vel.x;
		}
		Character.Rigidbody.velocity = vel;
	}

	public override bool damage(Damage damage){
		if(damage.Projectile != null){
			return false;
		}
		if(damage.DamageElement == DamageElement.Fire){
			bounceProjectile(ref damage.Projectile);
			return false;
		}
		return true;
	}
}
