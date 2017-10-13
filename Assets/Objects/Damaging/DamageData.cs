using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SonicEngine;
using UnityEngine;

public class DamageData : MonoBehaviour{
	public DamageElement DamageElement;
	public DamageType damageType;

	public static implicit operator Damage(DamageData data){
		return new Damage{
			DamageElement = data.DamageElement,
			damageType = data.damageType,
			projectile = null
		};
	}
}
