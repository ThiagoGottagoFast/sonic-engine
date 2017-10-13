using SonicEngine;
using UnityEngine;

public class FireShield : Shield {

	// Use this for initialization
	void Start () {
		Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Sphere.GetComponent<Renderer>().material.color = new Color(1f, 0.6f, 0, 0.75f);
	}

	public override void onJumpAction(){
		var vel = new Vector3(7, 0);
		if(Character.Status.isFlipped){
			vel.x = -vel.x;
		}
		Character.Rigidbody.velocity = vel;
	}

	public override bool damage(Damage damage){
		if(damage.projectile != null){
			return false;
		}
		if(damage.DamageElement == DamageElement.Fire){
			bounceProjectile(ref damage.projectile);
			return false;
		}
		return true;
	}
}
