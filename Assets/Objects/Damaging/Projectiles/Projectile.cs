using UnityEngine;

public class Projectile : MonoBehaviour{
	public GameObject Sphere;
	public Rigidbody Rigidbody;

	// Use this for initialization
	void Start () {
		Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Sphere.GetComponent<Renderer>().material.color = new Color(0xFF, 0xFF, 0);
		Rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
