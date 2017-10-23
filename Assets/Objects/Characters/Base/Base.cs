using System;
using System.Diagnostics;
using UnityEngine;
using InControl;

namespace SonicEngine{
	[RequireComponent(typeof(AudioSource))] 
	public class Base : MonoBehaviour{
		public const string Name = "Base test character";
		
		public Transform Transform;
		public Transform ModelTransform;
		public Rigidbody Rigidbody;
		public AudioSource AudioSource;
		public Animator Animator;
		public CapsuleCollider NormalHitbox;
		public SphereCollider BallHitbox;
		public Vector3 SpawnPos;
		public bool InAir;
		public bool LookingUp;
		public bool LookingDown;

		public byte JumpsToResetTo = 1;
		public byte JumpsLeft = 1;
		public bool Jumping;
		public ushort XForceMult = 65;
		public ushort YForceMult = 45;

		#region Status
		
		public State State;
		//public short animation;
		//public int animationFrame;
		public float InvulnerableTime;
		public float InvincibilityTime;
		public float SpeedshoesTime;
		public bool IsFlipped;
		public bool IsHurt;
		public bool IsDead;
		public bool CanMove = true;
		
		#endregion

		public DateTime LastInput = DateTime.Now;
		public float WaitTime;
		public Vector3 GroundAngle;
		
		public static ulong Score;
		public static Stopwatch Time;

		public static ushort Rings;
		//public static ushort Rings_2P;

		public static byte Lives = 3;
		//public static byte Lives_2P;

		public Shield Shield;

		public InputDevice InputDevice;

#if UNITY_EDITOR
		public TextMesh DebugText;
#endif

		private float angle{
			get{
				if (!InAir){
					RaycastHit hitA;
					const int div = 3;
					if (Physics.Raycast(Transform.position + (Transform.forward / div), -Transform.up, out hitA)){
						GroundAngle = Vector3.RotateTowards(transform.up, hitA.normal, 1, 1);
						return GroundAngle.x * Mathf.Rad2Deg;
					}
				}
				return 0;
			}
		}

		private float y{
			set{
				if (InAir){
					Rigidbody.AddForce(0, value * YForceMult, 0, ForceMode.Acceleration);
				} else{
					Rigidbody.AddRelativeForce(0, value * YForceMult, 0, ForceMode.Acceleration);
				}
			}
			get{ return Rigidbody.velocity.y; }
		}

		private float x{
			set{
				if (InAir){
					Rigidbody.AddForce(value * XForceMult, 0, 0, ForceMode.Acceleration);
				} else{
					Rigidbody.AddRelativeForce(value * XForceMult, 0, 0, ForceMode.Acceleration);
				}
			}
			get{ return Rigidbody.velocity.x; }
		}

		public void setY(float y){
			if(InAir || y == 0){
				setYAbs(y);
			}
			else{
				setYRel(y);
			}
		}

		public void setX(float x){
			if(InAir || x == 0){
				setXAbs(x);
			}
			else{
				setXRel(x);
			}
		}

		public void setYAbs(float y){
			var vel = Rigidbody.velocity;
			vel.y = y;
			Rigidbody.velocity = vel;
		}

		public void setXAbs(float x){
			var vel = Rigidbody.velocity;
			vel.x = x;
			Rigidbody.velocity = vel;
		}

		public void setYRel(float y){
			var locVel = transform.InverseTransformDirection(Rigidbody.velocity);
			locVel.y = y;
			Rigidbody.velocity = transform.TransformDirection(locVel);
		}

		public void setXRel(float x){
			var locVel = transform.InverseTransformDirection(Rigidbody.velocity);
			locVel.x = x;
			Rigidbody.velocity = transform.TransformDirection(locVel);
		}

		private const float Acceleration = 0.046875f;
		private const float Decceleration = 0.5f;
		private const float RollDeccel = 0.1484375f;
		private const float Friction = Acceleration;
		private const float TopSpeed = 6;
		private const float JumpSpeed = 6.5f;

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
		
		public Transform MouthCenterBone;
		public Transform MouthDead2Bone;
		public Transform MouthSideBone;
		public Transform MouthSide2Bone;

		// Use this for initialization
		private void Awake(){
			Transform = GetComponent<Transform>();
			Rigidbody = GetComponent<Rigidbody>();
			AudioSource = GetComponent<AudioSource>();
			Animator = GetComponent<Animator>();
			NormalHitbox = GetComponent<CapsuleCollider>();
			BallHitbox = GetComponent<SphereCollider>();
			ModelTransform = Transform.GetChild(0).transform;
			Time = new Stopwatch();
			Time.Start(); // Move to when act begins
			MouthCenterBone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Center");
			MouthDead2Bone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/mesh_mouth_dead2");
			MouthSideBone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Side");
			MouthSide2Bone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Side_02");
			SpawnPos = transform.position;
		}

		// Update is called once per frame
		private void Update(){
			InputDevice = InputManager.ActiveDevice;
			clearOnFrame();
			if(!IsHurt && CanMove){
				controlMove();
			}
			debug();
			if(InputDevice.AnyButton || !(InputDevice.LeftStick.Vector == Vector2.zero || InputDevice.DPad.Vector == Vector2.zero || CanMove)){
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

		[Conditional("UNITY_EDITOR")] private void debug(){
#if UNITY_EDITOR
			DebugText.text = string.Format("Angle: {0}, Velocity: {1}, State: {2}, Floor Angle: {3}",
			                               angle,
			                               Rigidbody.velocity,
			                               State,
			                               GroundAngle
			                               );
#endif
			if (Input.GetKeyDown(KeyCode.Alpha1)){
				if(Shield != null){
					Destroy(Shield.Sphere.gameObject);
				}
			} else if (Input.GetKeyDown(KeyCode.Alpha2)){
				Shield.createShield<Shield>(this);
			} else if (Input.GetKeyDown(KeyCode.Alpha3)){
				Shield.createShield<ElectricShield>(this);
			} else if (Input.GetKeyDown(KeyCode.Alpha4)){
				Shield.createShield<BubbleShield>(this);
			} else if (Input.GetKeyDown(KeyCode.Alpha5)){
				Shield.createShield<FireShield>(this);
			}
		}

		protected virtual void clearOnFrame(){
			Jumping = false;
			if(!IsHurt &&
			   InvulnerableTime >= 0){
				InvulnerableTime -= UnityEngine.Time.deltaTime;
			}
		}

		protected virtual void controlMove(){
			bool right = InputDevice.DPadRight;
			bool left = InputDevice.DPadLeft;
			if (right ^ left){
				if (right){
					if (State == State.Roll){
						if (x < 0){
							x = RollDeccel * UnityEngine.Time.deltaTime;
						}
					} else{
						if (x < 0 &&
						    !InAir){
							x = Decceleration * UnityEngine.Time.deltaTime;
						} else{
							if (x < TopSpeed){
								x = Acceleration * (InAir ? 2 : 1) * UnityEngine.Time.deltaTime;
								if (x > TopSpeed){
									setX(TopSpeed);
								}
							}
						}
					}
				} else{
					if (State == State.Roll){
						if (x > 0){
							x = -RollDeccel * UnityEngine.Time.deltaTime;
						}
					} else{
						if (x > 0 &&
						    !InAir){
							x = -Decceleration * UnityEngine.Time.deltaTime;
						} else{
							if (x > -TopSpeed){
								x = -Acceleration * (InAir ? 2 : 1) * UnityEngine.Time.deltaTime;
								if (x < -TopSpeed){
									setX(-TopSpeed);
								}
							}
						}
					}
				}
			} else{
				if (x > 0){
					x = -Friction * (State == State.Roll ? 0.5f : 1f) * UnityEngine.Time.deltaTime;
					if (x < 0){
						setX(0);
					}
				} else if (x < 0){
					x = Friction * (State == State.Roll ? 0.5f : 1f) * UnityEngine.Time.deltaTime;
					if (x > 0){
						setX(0);
					}
				}
			}
			if (State != State.Jump){
				const float minSpeed = 0.15f;
				if (Math.Abs(x) < minSpeed){
					setX(0);
				} else{
					if (x > minSpeed){
						IsFlipped = false;
					} else if (x < -minSpeed){
						IsFlipped = true;
					}
				}
			}
			
			var up = InputDevice.DPadUp;
			var down = InputDevice.DPadDown;
			if(up ^ down){
				if(up){
					LookingUp = true;
				}
				if(down){
					LookingDown = true;
				}
			}
			if (LookingDown){
				NormalHitbox.height = 1.5f;
				NormalHitbox.center = new Vector3(0, -0.25f);
			} else{
				NormalHitbox.height = 2;
				NormalHitbox.center = Vector3.zero;
			}
			controlJump();
		}

		protected virtual void controlJump(){
			if (InputDevice.Action1.WasPressed || InputDevice.Action2.WasPressed || InputDevice.Action3.WasPressed || InputDevice.Action4.WasPressed){
				if (JumpsLeft != 0){
					y = JumpSpeed;
					JumpsLeft--;
					InAir = true;
					State = State.Jump;
					Jumping = true;
					AudioSource.PlayOneShot(JumpSound, 0.5f);
					if (onJump != null){
						onJump(this);
					}
				} else{
					onJumpAction();
				}
			} else if ((InputDevice.Action1.WasReleased || InputDevice.Action2.WasReleased || InputDevice.Action3.WasReleased || InputDevice.Action4.WasReleased) && State == State.Jump && Rigidbody.velocity.y > 4){
				setY(4);
			}
		}

		protected virtual void controlAnimation(){
			var rot = Transform.eulerAngles;
			var modelRot = ModelTransform.localEulerAngles;
			var flip = IsFlipped ? -1 : 1;
			rot.z = -angle;
			modelRot.y = 90 * flip;
			transform.eulerAngles = rot;
			ModelTransform.localEulerAngles = modelRot;
			if(((int)InvulnerableTime / 4) % 2 == 1){
				ModelTransform.localScale = Vector3.zero;
			}
			else{
				ModelTransform.localScale = Vector3.one * 2;
			}
			
			Animator.SetFloat(animatorParameter(AnimatorParamtersValues.WaitTime), 
			                  WaitTime
			                 );
			float speed = Mathf.Abs(Rigidbody.velocity.x);
			if(State != State.Air){
				Animator.SetFloat(animatorParameter(AnimatorParamtersValues.Speed),
				                  speed
				                 );
			}
			if(State != State.Jump){
				float percent = Mathf.Clamp01(speed / 6f) * 2;
				Animator.SetFloat(animatorParameter(AnimatorParamtersValues.SpinSpeed),
				                  percent + 1
				                 );
			}
			Animator.SetBool(animatorParameter(AnimatorParamtersValues.Jumping), 
			                  State == State.Jump
			                 );
			Animator.SetBool(animatorParameter(AnimatorParamtersValues.LookingUp), 
			                 LookingUp
			                );
			LookingUp = false;
			Animator.SetBool(animatorParameter(AnimatorParamtersValues.LookingDown), 
			                 LookingDown
			                );
			LookingDown = false;
			Animator.SetBool(animatorParameter(AnimatorParamtersValues.SpinDashing), 
			                 false
			                );
			Animator.SetBool(animatorParameter(AnimatorParamtersValues.Rolling), 
			                 false
			                );
			Animator.SetBool(animatorParameter(AnimatorParamtersValues.Hurt), 
			                 IsHurt
			                );
			if(IsDead) Animator.SetTrigger(animatorParameter(AnimatorParamtersValues.Die));
			MouthCenterBone.localScale = Vector3.zero;
			MouthDead2Bone.localScale = Vector3.zero;
			if(IsFlipped){
				MouthSideBone.localScale = Vector3.zero;
				MouthSide2Bone.localScale = Vector3.one;
			} else{
				MouthSide2Bone.localScale = Vector3.zero;
				MouthSideBone.localScale = Vector3.one;
			}
		}

		public virtual void onJumpAction(){
			if(Shield != null){
				Shield.onJumpAction();
			}
		}
		
		public float Speed = 4.5f;

		public virtual void damage(Damage damage, Vector3 damagePosition){
			if(InvincibilityTime > 0 || InvulnerableTime > 0) return;
			if(Shield != null){
				if (Shield.damage(damage)){
					AudioSource.PlayOneShot(DieSound, 0.5f);
				} else{
					return;
				}
			}
			if(Rings != 0){
				AudioSource.PlayOneShot(RingLossSound, 0.5f);
				float Speed = this.Speed;
				float Angle = 0;
				for(int count = 0; Rings > 0 && count < 64; Rings--){
					count++;
					var ring = Instantiate(Resources.Load<GameObject>("Prefabs/LostRing")).GetComponent<RingScript>();
					Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), ring.GetComponents<SphereCollider>()[0]);
					Physics.IgnoreCollision(GetComponent<SphereCollider>(), ring.GetComponents<SphereCollider>()[0]);
					
					Vector3 result = new Vector3();
					result.y = -Mathf.Sin(Angle) * Speed;
					result.x = Mathf.Cos(Angle) * Speed;
					if(count % 2 == 1){
						Angle -= 22.5f * Mathf.Deg2Rad;
						result.x *= -1;
					}
					ring.transform.position = Transform.position;
					ring.rigidbody.velocity = result;
					if(count % 16 == 0 && count != 0){
						Angle = 0;
						Speed /= 2;
					}
					//Rings--; return;
				}
				Rings = 0;
			}
			else{
				// die
				AudioSource.PlayOneShot(DieSound, 0.5f);
				return;
			}
			IsHurt = true;
			InvulnerableTime = 2;
			// fly back
			Vector3 velocity = new Vector3(2, 4);
			if(damagePosition.x > transform.position.x){
				velocity.x *= -1;
			}
			Rigidbody.velocity = velocity;
		}

		private string animatorParameter(AnimatorParamtersValues value){
			return AnimatorParamterStrings[(int)value];
		}

		private enum AnimatorParamtersValues{
			WaitTime, Speed, SpinSpeed, Jumping, LookingUp, LookingDown, SpinDashing, Rolling, Hurt, Die
		}

		private static readonly string[] AnimatorParamterStrings = {
			"Wait Time", "Speed", "SpinSpeed", "Jumping", "LookingUp", "LookingDown", "SpinDashing", "Rolling", "Hurt", "Die"
		};

		/*protected virtual void OnCollisionEnter(Collision hit){
			if(hit.gameObject.CompareTag("Ground")){
				state = State.Normal;
			}
		}*/

		protected virtual void OnTriggerEnter(Collider hit){
			if(hit.CompareTag("Damage") && InvulnerableTime <= 0){
				damage(hit.GetComponent<DamageData>(), hit.transform.position);
			} else if (hit.CompareTag("Ground")){
				if(IsHurt){
					IsHurt = false;
					setX(0);
				}
			}
		}

		protected virtual void OnTriggerStay(Collider hit){
			if (hit.CompareTag("Ground") && !Jumping){
				JumpsLeft = JumpsToResetTo;
				InAir = false;
				State = State.Normal;
			}
		}

		protected virtual void OnTriggerExit(Collider hit){
			if (hit.CompareTag("Ground")){
				if (JumpsLeft != 0 && State != State.Jump){
					JumpsLeft--;
					State = State.Air;
					InAir = true;
					
					if (onAir != null) onAir(this);
				}
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
		Normal, Air, Roll, Jump
	}

	public struct Damage{
		public Projectile Projectile;
		public DamageElement DamageElement;
		public DamageType DamageType;
	}

	public enum DamageElement{
		Normal, Fire, Electric, Water
	}

	public enum DamageType{
		Enviroment, Badnik, Projectile
	}
}
