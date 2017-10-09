using System;
using System.Diagnostics;
using UnityEngine;

namespace SonicEngine{
	[RequireComponent(typeof(AudioSource))] public class Base : MonoBehaviour{
		public const string Name = "Base test character";
		
		public Transform Transform;
		public Transform ModelTransform;
		public Rigidbody Rigidbody;
		public AudioSource AudioSource;
		public Animator animator;
		public bool InAir;
		public bool LookingUp;
		public bool LookingDown;

		public byte JumpsToResetTo = 1;
		public byte JumpsLeft = 1;
		public bool Jumping;
		public ushort ForceMult = 60;
		public Status Status;

		public DateTime lastInput = DateTime.Now;
		public float waitTime;
		
		public static ulong Score;
		public static Stopwatch Time;

		public static ushort Rings;
		//public static ushort Rings_2P;

		public Shield Shield;

#if UNITY_EDITOR
		public TextMesh DebugText;
		public Vector3 groundAngle;
#endif

		private float Angle{
			get{
				if (!InAir){
					RaycastHit hitA;
					const int div = 3;
					if (Physics.Raycast(Transform.position + Transform.forward / div, -Transform.up, out hitA)){
						groundAngle = Vector3.RotateTowards(transform.up, hitA.normal, 1, 1);
						return groundAngle.x * Mathf.Rad2Deg;
					}
				}
				return 0;
			}
		}

		private float Y{
			set{
				if (InAir){
					Rigidbody.AddForce(0, value * ForceMult, 0, ForceMode.Acceleration);
				} else{
					Rigidbody.AddRelativeForce(0, value * ForceMult, 0, ForceMode.Acceleration);
				}
			}
			get{ return Rigidbody.velocity.y; }
		}

		public void SetY(float y){
			var vel = Rigidbody.velocity;
			vel.y = y;
			Rigidbody.velocity = vel;
		}

		private float X{
			set{
				if (InAir){
					Rigidbody.AddForce(value * ForceMult, 0, 0, ForceMode.Acceleration);
				} else{
					Rigidbody.AddRelativeForce(value * ForceMult, 0, 0, ForceMode.Acceleration);
				}
			}
			get{ return Rigidbody.velocity.x; }
		}

		public void SetX(float x){
			var vel = Rigidbody.velocity;
			vel.x = x;
			Rigidbody.velocity = vel;
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
		
		public Transform mouthCenterBone;
		public Transform mouthDead2Bone;
		public Transform mouthSideBone;
		public Transform mouthSide2Bone;

		// Use this for initialization
		private void Start(){
			Transform = GetComponent<Transform>();
			Rigidbody = GetComponent<Rigidbody>();
			AudioSource = GetComponent<AudioSource>();
			animator = GetComponent<Animator>();
			ModelTransform = Transform.GetChild(0).transform;
			Time = new Stopwatch();
			Time.Start(); // Move to when act begins
			mouthCenterBone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Center");
			mouthDead2Bone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/mesh_mouth_dead2");
			mouthSideBone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Side");
			mouthSide2Bone = ModelTransform.Find("ClassicSonicRoot/Reference/Hips/Neck/Head/Mouth_Side_02");
		}

		// Update is called once per frame
		private void Update(){
			ClearOnFrame();
			ControlMove();
			ControlJump();
			Debug();
			if(Input.anyKey){
				lastInput = DateTime.Now;
				waitTime = 0;
			}
			else{
				waitTime = (float)DateTime.Now.Subtract(lastInput).TotalSeconds;
			}
		}
		
		private void LateUpdate(){
			ControlAnimation();
		}

		[Conditional("UNITY_EDITOR")] private void Debug(){
			DebugText.text = string.Format("Angle: {0}, Velocity: {1}, State: {2}, Floor Angle: {3}",
			                               Angle,
			                               Rigidbody.velocity,
			                               Status.state,
			                               groundAngle
			                               );
			if (Input.GetKeyDown(KeyCode.Alpha1)){
				Destroy(Shield.gameObject);
				Shield = null;
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

		protected virtual void ClearOnFrame(){ Jumping = false; }

		protected virtual void ControlMove(){
			var right = Input.GetKey(KeyCode.RightArrow);
			var left = Input.GetKey(KeyCode.LeftArrow);
			if (right ^ left){
				if (right){
					if (Status.state == State.Roll){
						if (X < 0){
							X = RollDeccel;
						}
					} else{
						if (X < 0 &&
						    !InAir){
							X = Decceleration;
						} else{
							if (X < TopSpeed){
								X = Acceleration * (InAir ? 2 : 1);
								if (X > TopSpeed){
									SetX(TopSpeed);
								}
							}
						}
					}
				} else{
					if (Status.state == State.Roll){
						if (X > 0){
							X = -RollDeccel;
						}
					} else{
						if (X > 0 &&
						    !InAir){
							X = -Decceleration;
						} else{
							if (X > -TopSpeed){
								X = -Acceleration * (InAir ? 2 : 1);
								if (X < -TopSpeed){
									SetX(-TopSpeed);
								}
							}
						}
					}
				}
			} else{
				if (X > 0){
					X = -Friction * (Status.state == State.Roll ? 0.5f : 1f);
					if (X < 0){
						SetX(0);
					}
				} else if (X < 0){
					X = Friction * (Status.state == State.Roll ? 0.5f : 1f);
					if (X > 0){
						SetX(0);
					}
				}
			}
			if (Status.state != State.Jump){
				if (Math.Abs(X) < 0.1){
					SetX(0);
				} else{
					if (X > 0.1){
						Status.isFlipped = false;
					} else if (X < -0.1){
						Status.isFlipped = true;
					}
				}
			}
			
			var up = Input.GetKey(KeyCode.UpArrow);
			var down = Input.GetKey(KeyCode.DownArrow);
			if(up ^ down){
				if(up){
					LookingUp = true;
				}
				if(down){
					LookingDown = true;
				}
			}
		}

		protected virtual void ControlJump(){
			if (Input.GetKeyDown(KeyCode.Space)){
				if (JumpsLeft != 0){
					Y = JumpSpeed;
					JumpsLeft--;
					InAir = true;
					Status.state = State.Jump;
					Jumping = true;
					AudioSource.PlayOneShot(JumpSound, 0.5f);
					if (onJump != null){
						onJump(this);
					}
				} else{
					if (onJumpAction != null){
						onJumpAction(this);
					}
				}
			} else if (Input.GetKeyUp(KeyCode.Space) &&
			           Status.state == State.Jump){
				if (Rigidbody.velocity.y > 4){
					SetY(4);
				}
			} /*else if(Input.GetKey(KeyCode.Space)){
			}*/
		}

		protected virtual void ControlAnimation(){
			var rot = Transform.eulerAngles;
			var modelRot = ModelTransform.localEulerAngles;
			var flip = Status.isFlipped ? -1 : 1;
			rot.z = -Angle;
			modelRot.y = 90 * flip;
			transform.eulerAngles = rot;
			ModelTransform.localEulerAngles = modelRot;
			
			animator.SetFloat(AnimatorParameter(AnimatorParamtersValues.WaitTime), 
			                  waitTime
			                 );
			animator.SetFloat(AnimatorParameter(AnimatorParamtersValues.Speed), 
			                  Mathf.Abs(Rigidbody.velocity.x)
			                  );
			animator.SetBool(AnimatorParameter(AnimatorParamtersValues.Jumping), 
			                  Status.state == State.Jump
			                 );
			animator.SetBool(AnimatorParameter(AnimatorParamtersValues.LookingUp), 
			                 LookingUp
			                );
			LookingUp = false;
			animator.SetBool(AnimatorParameter(AnimatorParamtersValues.LookingDown), 
			                 LookingDown
			                );
			LookingDown = false;
			animator.SetBool(AnimatorParameter(AnimatorParamtersValues.SpinDashing), 
			                 false
			                );
			animator.SetBool(AnimatorParameter(AnimatorParamtersValues.Rolling), 
			                 false
			                );
			animator.SetBool(AnimatorParameter(AnimatorParamtersValues.Hurt), 
			                 false
			                );
			animator.SetBool(AnimatorParameter(AnimatorParamtersValues.Die), 
			                 false
			                );
			mouthCenterBone.localScale = Vector3.zero;
			mouthDead2Bone.localScale = Vector3.zero;
			if(Status.isFlipped){
				mouthSideBone.localScale = Vector3.zero;
				mouthSide2Bone.localScale = Vector3.one;
			} else{
				mouthSide2Bone.localScale = Vector3.zero;
				mouthSideBone.localScale = Vector3.one;
			}
		}

		private string AnimatorParameter(AnimatorParamtersValues value){
			return AnimatorParamterStrings[(int)value];
		}

		private enum AnimatorParamtersValues{
			WaitTime, Speed, Jumping, LookingUp, LookingDown, SpinDashing, Rolling, Hurt, Die
		}

		private static readonly string[] AnimatorParamterStrings = {
			"Wait Time", "Speed", "Jumping", "LookingUp", "LookingDown", "SpinDashing", "Rolling", "Hurt", "Die"
		};

		/*protected virtual void OnCollisionEnter(Collision hit){
			if(hit.gameObject.CompareTag("Ground")){
				Status.state = State.Normal;
			}
		}*/

		protected virtual void OnTriggerStay(Collider hit){
			if (hit.gameObject.CompareTag("Ground") && !Jumping){
				JumpsLeft = JumpsToResetTo;
				InAir = false;
				Status.state = State.Normal;
			}
		}

		protected virtual void OnTriggerExit(Collider hit){
			if (hit.gameObject.CompareTag("Ground")){
				if (JumpsLeft != 0 && Status.state != State.Jump){
					JumpsLeft--;
					Status.state = State.Air;
					InAir = true;
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

		public event OnJumpAction onJumpAction;

		public delegate void OnJumpAction(Base character);

		#endregion
	}
	
	
	public struct Status{
		public State state;
		public short animation;
		//public int animationFrame;
		public short invulnerable_time;
		public short invincibility_time;
		public short speedshoes_time;
		public bool isFlipped;
	}

	public enum State{
		Normal, Air, Roll, Jump
	}

	public struct Damage{
		public Projectile projectile;
		public DamageType DamageType;
	}

	public enum DamageType{
		Normal, Fire, Electric, Water
	}
}
