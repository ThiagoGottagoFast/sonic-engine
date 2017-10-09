using SonicEngine;
using UnityEngine;

public class ElectricShield : Shield {

	// Use this for initialization
	void Start () {
		Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Sphere.GetComponent<Renderer>().material.color = new Color(1f, 0.9215686f, 0.01568628f, 0.75f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
