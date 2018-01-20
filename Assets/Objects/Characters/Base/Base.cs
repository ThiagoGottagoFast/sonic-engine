using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using InControl;
using TMPro;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace SonicEngine{
	[RequireComponent(typeof(AudioSource))]
	public class Base : MonoBehaviour{
		public const string Name = "Base test character";

		public Transform Transform;
		public Transform NormalModelTransform;
		public Transform SuperModelTransform;
		public Transform modelTransform{
			get{return Super ? SuperModelTransform : NormalModelTransform;}
		}
		public Rigidbody Rigidbody;
		public AudioSource AudioSource;
		public Animator NormalAnimator;
		public Animator SuperAnimator;
		public Animator animator{
			get{return Super ? SuperAnimator : NormalAnimator;}
		}
		public CapsuleCollider NormalHitbox;
		public SphereCollider BallHitbox;
		public PhysicMaterial PhysicMaterial;
		public Vector3 CheckPointPos;
		public Vector3 LastVelocity;
		//public ulong CheckPointTime;
		public bool Invisible;
		public bool InAir;
		public bool LookingUp;
		public bool LookingDown;

		public byte JumpsToResetTo = 1;
		public byte JumpsLeft = 1;
		public bool Jumping;
		public uint XForceMult = 65;
		public uint YForceMult = 45;

		public float MovingFriction, Coasting, StopingFriction;

		#region Status

		public State State;
		//public short animation;
		//public int animationFrame;
		public float InvulnerableTime;
		public float InvincibilityTime;
		public float SpeedshoesTime;
		public float SpinDashTime;
		public float SuperPeelOutTime;
		public bool Super;
		public bool IsFlipped;
		public bool IsHurt;
		public bool IsDead;
		public bool CanMove = true;

		#endregion

		public DateTime LastInput = DateTime.Now;
		public float WaitTime;
		//public Vector3 GroundAngle;

		public static ulong Score;
		public static Stopwatch Time;

		public static ushort Rings;
		//public static ushort Rings_2P;

		public static byte Lives = 3;
		//public static byte Lives_2P;

		public Shield Shield;

		public InputDevice InputDevice;

#if UNITY_EDITOR
		public TextMeshProUGUI DebugText;
#endif

		private float angle{
			get{
				if(!InAir){
					//GroundAngle = Vector3.RotateTowards(transform.up, GroundNormal, 1, 1);
					return GroundNormal.x * Mathf.Rad2Deg;
				}

				return 0;
			}
		}

		private Vector3 GroundNormal{
			get{
				if(!InAir){
					RaycastHit hitA, hitB;
					const int div = 3;
					Vector3 startLeft = Transform.position,
					        startRight = startLeft,
					        direction = -Transform.up;
					startLeft += -Transform.right / div;
					startRight += Transform.right / div;
					Debug.DrawRay(startLeft, direction, Color.red);
					Debug.DrawRay(startRight, direction, Color.blue);
					bool left, right;
					left = Physics.Raycast(startLeft, direction, out hitA);
					right = Physics.Raycast(startRight, direction, out hitB);
					if(left && right){
						return (hitA.normal + hitB.normal) / 2;
					}

					if(left){
						return hitA.normal;
					}

					if(right){
						return hitB.normal;
					}
				}

				return Vector3.zero;
			}
		}

		#region Velocity

		private Vector3 relativeVel{
			get{return transform.InverseTransformDirection(vel);}
			set{vel = transform.TransformDirection(value);}
		}

		private Vector3 vel{
			get{return Rigidbody.velocity;}
			set{Rigidbody.velocity = value;}
		}

		#region YVeclocity

		private float y{
			set{yRel = value;}
			get{return yRel;}
		}

		private float yRel{
			set{Rigidbody.AddRelativeForce(0, value * YForceMult, 0, ForceMode.Force);}
			get{return relativeVel.y;}
		}

		private float yAbs{
			set{Rigidbody.AddForce(0, value * YForceMult, 0, ForceMode.Force);}
			get{return vel.y;}
		}

		public void setY(float y){
			if(InAir || y == 0){
				setYAbs(y);
			}
			else{
				setYRel(y);
			}
		}

		public void setYAbs(float y){
			var vel = this.vel;
			vel.y = y;
			this.vel = vel;
		}

		public void setYRel(float y){
			var locVel = transform.InverseTransformDirection(vel);
			locVel.y = y;
			vel = transform.TransformDirection(locVel);
		}

		#endregion

		#region XVelocity

		private float x{
			set{xRel = value;}
			get{return xRel;}
		}

		private float xRel{
			set{
				Vector3 force = new Vector3(value * XForceMult, 0, 0);
				/*Debug.Log("Force: " + force);
				force = Vector3.ProjectOnPlane(force, GroundNormal).normalized;
				Debug.Log("Projected Force: " + force);*/
				Rigidbody.AddRelativeForce(force, ForceMode.Force);
			}
			get{return relativeVel.x;}
		}

		private float xAbs{
			set{Rigidbody.AddForce(value * XForceMult, 0, 0, ForceMode.Force);}
			get{return vel.x;}
		}

		public void setX(float x){
			if(InAir || x == 0){
				setXAbs(x);
			}
			else{
				setXRel(x);
			}
		}

		public void setXAbs(float x){
			var vel = this.vel;
			vel.x = x;
			this.vel = vel;
		}

		public void setXRel(float x){
			var locVel = transform.InverseTransformDirection(vel);
			locVel.x = x;
			vel = transform.TransformDirection(locVel);
		}

		#endregion

		#endregion

		private float acceleration{
			get{return 0.046875f * (Super ? 2 : 1);}
		}
		private float decceleration{
			get{return -0.5f * (Super ? 2 : 1);}
		}
		private float rollDeccel{
			get{return 0.1484375f * (Super ? 2 : 1);}
		}
		private float friction{
			get{return acceleration;}
		}
		private float topSpeed{
			get{return 6 * (Super ? 2 : 1);}
		}
		private float jumpSpeed{
			get{return Super ? 7 : 6.5f;}
		}

		#region sounds

		public AudioClip JumpSound;
		public AudioClip JumpDashSound;
		public AudioClip RollSound;
		public AudioClip SpinChargeSound;
		public AudioClip SpinReleaseSound;
		public AudioClip InstaShieldSound;
		public AudioClip RingLossSound;
		public AudioClip DieSound;

		#endregion

		public Transform NormalMouthCenterBone;
		public Transform SuperMouthCenterBone;
		public Transform mouthCenterBone{
			get{return Super ? SuperMouthCenterBone : NormalMouthCenterBone;}
		}

		public Transform NormalMouthDead2Bone;
		public Transform SuperMouthDead2Bone;
		public Transform mouthDead2Bone{
			get{return Super ? SuperMouthDead2Bone : NormalMouthDead2Bone;}
		}

		public Transform NormalMouthSideBone;
		public Transform SuperMouthSideBone;
		public Transform mouthSideBone{
			get{return Super ? SuperMouthSideBone : NormalMouthSideBone;}
		}

		public Transform NormalMouthSide2Bone;
		public Transform SuperMouthSide2Bone;
		public Transform mouthSide2Bone{
			get{return Super ? SuperMouthSide2Bone : NormalMouthSide2Bone;}
		}

		public float Scale = 2;

		// Use this for initialization
		private void Awake(){
			Transform = GetComponent<Transform>();
			Rigidbody = GetComponent<Rigidbody>();
			AudioSource = GetComponent<AudioSource>();
			NormalHitbox = GetComponent<CapsuleCollider>();
			BallHitbox = GetComponent<SphereCollider>();
			NormalModelTransform = Transform.GetChild(0);
			NormalAnimator = NormalModelTransform.GetComponent<Animator>();
			SuperModelTransform = Transform.GetChild(1);
			SuperAnimator = SuperModelTransform.GetComponent<Animator>();
			SuperAnimator.SetLayerWeight(1, 1f);
			Time = new Stopwatch();
			Time.Start(); // Move to when act begins
			NormalMouthCenterBone = NormalModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Center");
			NormalMouthDead2Bone = NormalModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/mesh_mouth_dead2");
			NormalMouthSideBone = NormalModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Side");
			NormalMouthSide2Bone = NormalModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Side_02");
			SuperMouthCenterBone = SuperModelTransform.Find("chr_classic_SuperSonic_HD/Reference/Hips/Neck/Head/Mouth_Center");
			SuperMouthDead2Bone =
				SuperModelTransform.Find("chr_classic_SuperSonic_HD/Reference/Hips/Neck/Head/mesh_mouth_dead2");
			SuperMouthSideBone = SuperModelTransform.Find("chr_classic_SuperSonic_HD/Reference/Hips/Neck/Head/Mouth_Side");
			SuperMouthSide2Bone = SuperModelTransform.Find("chr_classic_SuperSonic_HD/Reference/Hips/Neck/Head/Mouth_Side_02");
			GameObject startPosition = GameObject.FindWithTag("StartPosition");
			if(startPosition == null)
				throw new Exception(
				                    "Missing Start Position object. Make sure you add a object tagged with \"StartPosition\" to signify where to start sonic at."
				                   );
			transform.position = startPosition.transform.position;
			CheckPointPos = transform.position;
			superTransform(false);
		}

		// Update is called once per frame
		private void Update(){
			InputDevice = InputManager.ActiveDevice;
			clearOnFrame();
			if(!IsHurt && CanMove){
				controlMove();
			}

			debug();
			bool result = InputDevice.AnyButton ||
			              InputDevice.LeftStick.Vector != Vector2.zero ||
			              InputDevice.DPad.Vector != Vector2.zero;
			if(result || Super){
				LastInput = DateTime.Now;
				WaitTime = 0;
			}
			else{
				WaitTime = (float)DateTime.Now.Subtract(LastInput).TotalSeconds;
			}
		}

		private void LateUpdate(){
			controlAnimation();
		}

		[Conditional("UNITY_EDITOR")]
		private void debug(){
#if UNITY_EDITOR
			DebugText.text =
				string.Format("Depth: {0}\nRot: {1}\nVelocity: {2}\nRelative Velocity: {3}\nState: {4}\nAngle: {5}\nFloor Angle: {6}\nSuper Peel-Out time: {7}",
				              transform.position.z,
				              transform.rotation.eulerAngles.y,
				              vel,
				              relativeVel,
				              State,
				              angle,
				              GroundNormal,
				              (int)SuperPeelOutTime
				             );
#endif
			if(Input.GetKeyDown(KeyCode.Alpha1)){
				if(Shield != null){
					Destroy(Shield.Sphere.gameObject);
				}
			}
			else if(Input.GetKeyDown(KeyCode.Alpha2)){
				Shield.createShield<Shield>(this);
			}
			else if(Input.GetKeyDown(KeyCode.Alpha3)){
				Shield.createShield<ElectricShield>(this);
			}
			else if(Input.GetKeyDown(KeyCode.Alpha4)){
				Shield.createShield<BubbleShield>(this);
			}
			else if(Input.GetKeyDown(KeyCode.Alpha5)){
				Shield.createShield<FireShield>(this);
			}
		}

		protected virtual void clearOnFrame(){
			/*if(Super){
				SuperModelTransform.gameObject.SetActive(true);
				NormalModelTransform.gameObject.SetActive(false);
			}
			else{
				SuperModelTransform.gameObject.SetActive(false);
				NormalModelTransform.gameObject.SetActive(true);
			}*/
			Jumping = false;
			Rigidbody.useGravity = true;
			if(!IsHurt &&
			   InvulnerableTime >= 0){
				InvulnerableTime -= UnityEngine.Time.deltaTime;
			}

			if(SpinDashTime > 0){
				SpinDashTime -= UnityEngine.Time.deltaTime;
				if(SpinDashTime < 0){
					SpinDashTime = 0;
				}
			}

			Vector3 vel = relativeVel;
			vel.z = 0;
			relativeVel = vel;
		}

		protected virtual void controlMove(){
			const float minSpeed = 0.1f;
			var up = InputDevice.DPadUp;
			var down = InputDevice.DPadDown;
			if((up ^ down) &&
			   (Mathf.Abs(x) < minSpeed)){
				if(up){
					LookingUp = true;
				}

				if(down){
					LookingDown = true;
				}
			}
			else{
				bool right = InputDevice.DPadRight;
				bool left = InputDevice.DPadLeft;
				if(right ^ left){
					if(right){
						if(State == State.Roll){
							if(x < 0){
								//x = RollDeccel * UnityEngine.Time.deltaTime;
								PhysicMaterial.dynamicFriction = Coasting;
							}
							else{
								PhysicMaterial.dynamicFriction = MovingFriction;
							}
						}
						else{
							if(x < -minSpeed &&
							   !InAir){
								x = -acceleration * UnityEngine.Time.deltaTime;
								PhysicMaterial.dynamicFriction = StopingFriction;
							}
							else{
								PhysicMaterial.dynamicFriction = MovingFriction;
								if(x <= topSpeed){
									x = acceleration * (InAir ? 2 : 1) * UnityEngine.Time.deltaTime;
									if(x > topSpeed){
										setX(topSpeed);
									}
								}
							}
						}
					}
					else{
						if(State == State.Roll){
							if(x > 0){
								//x = -RollDeccel * UnityEngine.Time.deltaTime;
								PhysicMaterial.dynamicFriction = Coasting;
							}
							else{
								PhysicMaterial.dynamicFriction = MovingFriction;
							}
						}
						else{
							if(x > minSpeed &&
							   !InAir){
								x = acceleration * UnityEngine.Time.deltaTime;
								PhysicMaterial.dynamicFriction = StopingFriction;
							}
							else{
								PhysicMaterial.dynamicFriction = MovingFriction;
								if(x >= -topSpeed){
									x = -acceleration * (InAir ? 2 : 1) * UnityEngine.Time.deltaTime;
									if(x < -topSpeed){
										setX(-topSpeed);
									}
								}
							}
						}
					}
				}
				else{
					PhysicMaterial.dynamicFriction = Coasting;
				}
			}

			if(LookingDown){
				NormalHitbox.height = 1.5f;
				NormalHitbox.center = new Vector3(0, -0.25f);
			}
			else{
				NormalHitbox.height = 2;
				NormalHitbox.center = Vector3.zero;
			}

			if(State != State.Jump){
				if(Math.Abs(x) < minSpeed){
					//setX(0);
				}
				else{
					if(x > minSpeed){
						IsFlipped = false;
					}
					else if(x < -minSpeed){
						IsFlipped = true;
					}
				}
			}

			//var tempLastVelocity = LastVelocity;
			LastVelocity = relativeVel;
			controlJump();
			if(!InAir){
				float absX = Mathf.Abs(xRel);
				if(absX > 2){
					//setYRel(-2 * (Super ? 2 : 1));
					setYRel(-(absX / (6 * (Super ? 0.5f : 1.5f))));
					yAbs = (-0.25f + (1 / xRel)) / 4;
					xRel = (Mathf.Abs(angle)/50000) * (xRel > 0 ? 1 : -1);
					Rigidbody.useGravity = false;
					Debug.Log("Sticking to surface");
				}
			}
		}

		public Coroutine SuperPeelOutCoroutine;

		public IEnumerator SuperPeelOut(){
			SuperPeelOutTime = 1;
			for(;;){
				yield return new WaitForEndOfFrame();
				SuperPeelOutTime += UnityEngine.Time.deltaTime * 4;
			}
		}

		protected virtual void controlJump(){
			/*if(InputDevice.Action1 ||
			   InputDevice.Action2 ||
			   InputDevice.Action3 ||
			   InputDevice.Action4){
			}*/
			if(!LookingUp){
				if(SuperPeelOutTime > 0){
					if(SuperPeelOutTime > 1){
						setXRel(topSpeed * (IsFlipped ? -1 : 1));
					}
					SuperPeelOutTime = 0;
					StopCoroutine(SuperPeelOutCoroutine);
				}
			}
			if(InputDevice.Action1.WasPressed ||
			        InputDevice.Action2.WasPressed ||
			        InputDevice.Action3.WasPressed ||
			        InputDevice.Action4.WasPressed){
				if(LookingUp){ // Super Peel-Out
					LookingUp = false;
					if(SuperPeelOutTime <= 0){
						SuperPeelOutCoroutine = StartCoroutine(SuperPeelOut());
					}
				}
				if(LookingDown){
					// Spindash
				}
				else{
					if(JumpsLeft != 0){
						if(SuperPeelOutTime <= 0){
							y = jumpSpeed;
							JumpsLeft--;
							InAir = true;
							State = State.Jump;
							Jumping = true;
							AudioSource.PlayOneShot(JumpSound, 0.5f);
							if(onJump != null){
								onJump(this);
							}
						}
					}
					else{
						onJumpAction();
					}
				}
			}
			else if(InputDevice.Action1.WasReleased ||
			        InputDevice.Action2.WasReleased ||
			        InputDevice.Action3.WasReleased ||
			        InputDevice.Action4.WasReleased){
				if(State == State.Jump &&
				   vel.y > 4){
					setY(4);
				}
			}
		}

		protected virtual void controlAnimation(){
			var rot = Transform.eulerAngles;
			var modelRot = modelTransform.localEulerAngles;
			var flip = IsFlipped ? -1 : 1;
			rot.z = -angle;
			if(!IsDead){
				if(!InAir){
					modelRot.y = 90 * flip;
				}
			}
			else{
				modelRot.y = 180;
			}

			transform.eulerAngles = rot;
			modelTransform.localEulerAngles = modelRot;
			modelTransform.gameObject.SetActive(!Invisible);
			animator.SetFloat(animatorParameter(AnimatorParamtersValues.WaitTime),
			                  WaitTime
			                 );
			float speed = Mathf.Abs(x);
			if(State != State.Air){
				animator.SetFloat(animatorParameter(AnimatorParamtersValues.Speed),
				                  SuperPeelOutTime > 0
					                  ? Mathf.Clamp(SuperPeelOutTime, 0f, 12f)
					                  : speed
				                 );
			}

			if(State != State.Jump){
				float percent = Mathf.Clamp01(speed / topSpeed);
				animator.SetLayerWeight(2, percent);
				animator.SetFloat(animatorParameter(AnimatorParamtersValues.SpinSpeed),
				                  ((percent * 2) + 1) * (Super ? 1 : -1)
				                 );
			}

			animator.SetFloat(animatorParameter(AnimatorParamtersValues.InvulnTimer),
			                  InvulnerableTime
			                 );
			animator.SetBool(animatorParameter(AnimatorParamtersValues.Jumping),
			                 State == State.Jump
			                );
			animator.SetBool(animatorParameter(AnimatorParamtersValues.LookingUp),
			                 SuperPeelOutTime <= 0 && LookingUp
			                );
			LookingUp = false;
			animator.SetBool(animatorParameter(AnimatorParamtersValues.LookingDown),
			                 LookingDown
			                );
			LookingDown = false;
			animator.SetBool(animatorParameter(AnimatorParamtersValues.SpinDashing),
			                 false
			                );
			animator.SetBool(animatorParameter(AnimatorParamtersValues.SuperPeelOut),
			                 false
			                );
			animator.SetBool(animatorParameter(AnimatorParamtersValues.Rolling),
			                 false
			                );
			mouthCenterBone.localScale = Vector3.zero;
			mouthDead2Bone.localScale = Vector3.zero;
			if(IsFlipped){
				mouthSideBone.localScale = Vector3.zero;
				mouthSide2Bone.localScale = Vector3.one;
			}
			else{
				mouthSide2Bone.localScale = Vector3.zero;
				mouthSideBone.localScale = Vector3.one;
			}
		}

		public virtual void onJumpAction(){
			if(Super){
				superTransform(false);
				return;
			}

			if(Rings >= 50){
				superTransform(true);
				return;
			}

			if(Shield != null){
				Shield.onJumpAction();
			}
		}

		public void superTransform(){
			superTransform(!Super);
		}

		private Coroutine _ringDrain;

		public void superTransform(bool toSuper){
			if(toSuper){
				Super = true;
				SuperModelTransform.gameObject.SetActive(true);
				NormalModelTransform.gameObject.SetActive(false);
				_ringDrain = StartCoroutine(superRingDrain());
			}
			else{
				Super = false;
				SuperModelTransform.gameObject.SetActive(false);
				NormalModelTransform.gameObject.SetActive(true);
				if(_ringDrain != null){
					StopCoroutine(_ringDrain);
				}
			}
		}

		private IEnumerator superRingDrain(){
			while(Rings > 0){
				yield return new WaitForSeconds(1.01f);
				Rings--;
			}

			_ringDrain = null;
			superTransform(false);
		}

		public float Speed = 4.5f;

		public virtual void damage(Damage damage, Vector3 damagePosition){
			if(InvincibilityTime > 0 || // Don't take damage if you have invincibility
			   InvulnerableTime > 0 || // Don't take damage if we just recently took damage
			   Super) // Don't take damage because super sonic doesn't care about damage
				return;
			if(Shield != null){
				if(Shield.damage(damage)){
					AudioSource.PlayOneShot(DieSound, 1f);
					knockBack(damagePosition.x);
				}

				return;
			}

			if(Rings != 0){
				AudioSource.PlayOneShot(RingLossSound, 0.5f);
				float speed = Speed;
				float angle = 0;
				for(int count = 0; Rings > 0 && count < 64; Rings--){
					count++;
					var ring = Instantiate(Resources.Load<GameObject>("Prefabs/LostRing")).GetComponent<RingScript>();
					Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), ring.GetComponents<SphereCollider>()[0]);
					Physics.IgnoreCollision(GetComponent<SphereCollider>(), ring.GetComponents<SphereCollider>()[0]);
					Vector3 result = new Vector3();
					result.y = -Mathf.Sin(angle) * speed;
					result.x = Mathf.Cos(angle) * speed;
					if(count % 2 == 1){
						angle -= 22.5f * Mathf.Deg2Rad;
						result.x *= -1;
					}

					ring.transform.position = Transform.position;
					ring.Rigidbody.velocity = result;
					if(count % 16 == 0 &&
					   count != 0){
						angle = 0;
						speed /= 2;
					}

					//Rings--; return;
				}

				Rings = 0;
			}
			else{
				die();
				return;
			}

			knockBack(damagePosition.x);
		}

		private void die(){
			AudioSource.PlayOneShot(DieSound, 1f);
			State = State.Normal;
			InAir = true;
			IsDead = true;
			CanMove = false;
			vel = new Vector3(0, 7);
			Vector3 pos = transform.position;
			pos.z -= 2;
			transform.position = pos;
			SmoothCamera.enabled = false;
			NormalHitbox.enabled = false;
			BallHitbox.enabled = false;
			if(Lives != 0){
				Lives--;
				StartCoroutine(respawn());
			}

			animator.SetTrigger(animatorParameter(AnimatorParamtersValues.Die));
		}

		private IEnumerator respawn(){
			yield return new WaitForSeconds(2);
			transform.position = CheckPointPos;
			CanMove = true;
			IsDead = false;
			InAir = false;
			SmoothCamera.enabled = true;
			NormalHitbox.enabled = true;
			BallHitbox.enabled = false;
		}

		private IEnumerator blinkAnim(){
			while(InvulnerableTime > 0){
				Invisible = !Invisible;
				yield return new WaitForSeconds(0.066666599999999993f);
			}

			Invisible = false;
		}

		public void knockBack(float damagePosition){
			IsHurt = true;
			InvulnerableTime = 2;
			// fly back
			Vector3 velocity = new Vector3(2, 4);
			if(damagePosition > transform.position.x){
				velocity.x *= -1;
			}

			vel = velocity;
			animator.SetTrigger(animatorParameter(AnimatorParamtersValues.Hurt));
		}

		private static string animatorParameter(AnimatorParamtersValues value){
			return AnimatorParamterStrings[(int)value];
		}

		private enum AnimatorParamtersValues{
			WaitTime,
			Speed,
			SpinSpeed,
			InvulnTimer,
			Jumping,
			LookingUp,
			LookingDown,
			SpinDashing,
			SuperPeelOut,
			Rolling,
			Hurt,
			Die
		}

		private static readonly string[] AnimatorParamterStrings = {
			"Wait Time",
			"Speed",
			"SpinSpeed",
			"InvulnTimer",
			"Jumping",
			"LookingUp",
			"LookingDown",
			"SpinDashing",
			"Super Peel-out",
			"Rolling",
			"Hurt",
			"Die"
		};

		/*protected virtual void OnCollisionEnter(Collision hit){
			if(hit.gameObject.CompareTag("Ground")){
				state = State.Normal;
			}
		}*/

		protected virtual void OnTriggerEnter(Collider hit){
			if(IsDead)
				return;
			Debug.Log(hit.tag);
			if(hit.CompareTag("Damage") &&
			   InvulnerableTime <= 0){
				damage(hit.GetComponent<DamageData>(), hit.transform.position);
			}
			else if(hit.tag.StartsWith("Ground")){
				FloorType floorType;
				switch(hit.tag.Substring(hit.tag.IndexOf("/", StringComparison.Ordinal))){
				case "Concrete":
					floorType = FloorType.Concrete;
					break;
				case "Metal":
					floorType = FloorType.Metal;
					break;
				case "Grass":
					floorType = FloorType.Grass;
					break;
				default:
					floorType = FloorType.Concrete;
					break;
				}

				playFootStep(floorType);
				Debug.Log(floorType);
				if(IsHurt){
					IsHurt = false;
					setX(0);
					StartCoroutine(blinkAnim());
				}
			}
		}

		protected virtual void OnTriggerStay(Collider hit){
			if(IsDead)
				return;
			if(hit.tag.StartsWith("Ground") &&
			   !Jumping){
				JumpsLeft = JumpsToResetTo;
				InAir = false;
				State = State.Normal;
			}
		}

		protected virtual void OnTriggerExit(Collider hit){
			if(IsDead)
				return;
			if(hit.tag.StartsWith("Ground")){
				if(JumpsLeft != 0 &&
				   State != State.Jump){
					JumpsLeft--;
					State = State.Air;
					InAir = true;
					if(onAir != null)
						onAir(this);
				}
			}
		}

		public virtual void playFootStep(FloorType floorType){
			switch(floorType){
			case FloorType.Concrete: break;
			case FloorType.Grass: break;
			case FloorType.Metal: break;
			default: throw new ArgumentOutOfRangeException("floorType", floorType, null);
			}
		}

		#region Events

		//public event OnNormal onNormal;

		//public delegate void OnNormal(Base character);

		public event OnAir onAir;

		public delegate void OnAir(Base character);

		public event OnRoll onRoll;

		public delegate void OnRoll(Base character);

		public event OnJump onJump;

		public delegate void OnJump(Base character);

		#endregion
	}

	public enum State{
		Normal,
		Air,
		Roll,
		Jump
	}

	public struct Damage{
		public Projectile Projectile;
		public DamageElement DamageElement;
		public DamageType DamageType;
	}

	public enum DamageElement{
		Normal,
		Fire,
		Electric,
		Water
	}

	public enum DamageType{
		Enviroment,
		Badnik,
		Projectile
	}
}
