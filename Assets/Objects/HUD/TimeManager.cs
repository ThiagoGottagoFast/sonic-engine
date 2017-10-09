using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour {
	public Text text;
	// Use this for initialization
	void Start (){
		text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update (){
		TimeSpan ts = SonicEngine.Base.Time.Elapsed;
		byte mili = (byte)(ts.Milliseconds.ToString().Length > 2 ? ts.Milliseconds/10 : ts.Milliseconds);
		text.text = string.Format("{0}\'{1:00}\"{2:00}", ts.Minutes, ts.Seconds, mili);
	}
}
