using TMPro;
using UnityEngine;

public class LivesManager : MonoBehaviour{
	private TextMeshProUGUI text;
	// Use this for initialization
	void Start (){
		text = GetComponent<TextMeshProUGUI>();
	}
	
	// Update is called once per frame
	void Update (){
		text.text = SonicEngine.Base.Lives.ToString();
	}
}
