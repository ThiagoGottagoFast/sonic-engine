using SonicEngine;
using UnityEngine;

public class BubbleShield : Shield {

	// Use this for initialization
	void Start () {
		Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Sphere.GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 1f, 0.75f);
		Sphere.GetComponent<SphereCollider>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
