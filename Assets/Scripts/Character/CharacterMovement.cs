using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterMovement : MonoBehaviour {
	public float WalkAcceleration = 20;
	// TODO: implement WalkMaxImpulse. Same thing for climb.
	public float MaxWalkVelocity = 0.5f;
	public float WalkGravityResistance = 0.5f;
	public float MaxWalkAngle = 80;
	public AudioClip [] WalkSounds;
	public float ClimbStepHeight = 0.125f;
	public float ClimbStepDepth = 0.04f;
	public float ClimbAcceleration = 20;
	public float MaxClimbVelocity = 0.5f;
	public float ClimbGravityResistance = 1;
	public float ClimbMovementDirectionAngle = 60;
	public float ForwardJumpStartVelocity = 3.5f;
	public float ForwardJumpDirectionAngle = 60;
	public float BackJumpStartVelocity = 4.5f;
	public float BackJumpDirectionAngle = 93;
	public float BackJumpSideAcceleration = 0.5f;
	public float BackJumpSideAccelerationDuration = 0.5f;
	public float SurfaceReactionFactor = 0.5f;
	public float JumpPreparationDuration = 0.2f;
	public float JumpBouncinessDuration = 0.35f;
	public float JumpCooldown = 0.5f;
	public AudioClip [] JumpSounds;
	public float FootWeldPause = 0.3f;
	public float FootWeldMaxVelocity = 0.05f;
	public float FootWeldMinFriction = 0.1f;
	public float FootWeldBreakForce = 30;
	public float FallVelocity = 3;
	public PhysicsMaterial2D DefaultSurfaceMaterial;
	public float MaxFrictionAccelFactor = 2;
	public float MinFrictionAccelFactor = 0.01f;
	public float MaxControlLossDuration = 1;
	public float ControlRegainDuration = 0.5f;
	public float ControlLostChangeHeadingMinVelocity = 0.5f;
	public StatePhysicMaterials StateMaterials = new StatePhysicMaterials ();
	public CameraEventList CameraEvents = new CameraEventList ();

	// TODO: follow new naming convention: MoveX -> MoveXInput, PerformForwardJump -> ForwardJumpInput.
	[HideInInspector]
	public float MoveX;
	[HideInInspector]
	public float MoveY;	// TODO: leave it for swimming?
	[HideInInspector]
	public bool PerformForwardJump;
	[HideInInspector]
	public bool PerformBackJump;

	private CharController charController;
	private Transform headingTransform;
	private InventoryUser inventoryUser;
	private CollisionContactsTracker collisionTracker;
	private CollisionDamageDispatcher collisionDamageDispatcher;
	private SoundPlayer soundPlayer;

	// TODO: make HeadingTransform Behaviour.
	public int Heading {
		get {
			float scaleX = headingTransform.localScale.x;
			
			if ( scaleX == 0 )
				return	1;
			else
				return	System.Math.Sign ( scaleX );
		}
		set {
			float floatSign = Mathf.Sign ( value );
			
			if ( floatSign == 0 )
				floatSign = 1;

			headingTransform.localScale = new Vector3 ( floatSign, 1, 1 );
		}
	}

	private List <SurfaceContact> frameContacts = new List <SurfaceContact> ();
	private List <SurfaceContact> FloorContacts = new List <SurfaceContact> ();
	public bool IsGrounded { get { return	FloorContacts.Count != 0; } }
	private float lastGroundedTimestamp;

	public bool IsPreparingToJump { get; private set; }
	private float jumpPreparationStartTimestamp;
	private bool jumpForward;
	private float lastJumpTimestamp;
	public bool IsJumpingForward { get; private set; }
	public bool IsJumpingBackward { get; private set; }
	public bool IsJumping { get { return	IsJumpingForward || IsJumpingBackward; } }
	private Vector2 jumpDir;
	private bool stopJumpBounciness;

	private float bumpBetweenFramesCacheTimestamp = float.NegativeInfinity;
	private bool bumpTheFloorBetweenFramesCached = false;
	private bool bumpTheWallOrCeilingBetweenFramesCached = false;
	private bool BumpTheFloorBetweenFrames {
		get {
			UpdateBumpBetweenFramesCache ();

			return	bumpTheFloorBetweenFramesCached;
		}
	}
	private bool BumpTheWallOrCeilingBetweenFrames {
		get {
			UpdateBumpBetweenFramesCache ();

			return	bumpTheWallOrCeilingBetweenFramesCached;
		}
	}
	private bool AnyBumpBetweenFrames { get { return	collisionTracker.Contacts.Count != 0; } }

	private void UpdateBumpBetweenFramesCache () {
		if ( Time.fixedTime != bumpBetweenFramesCacheTimestamp ) {
			bumpTheFloorBetweenFramesCached = false;
			bumpTheWallOrCeilingBetweenFramesCached = false;

			foreach ( var contact in collisionTracker.Contacts ) {
				if ( IsWalkable ( contact.Normal ) )
					bumpTheFloorBetweenFramesCached = true;
				else
					bumpTheWallOrCeilingBetweenFramesCached = true;
			}

			bumpBetweenFramesCacheTimestamp = Time.fixedTime;
		}
	}

	private float controlLossTimer = 0;
	public bool ControlLost { get { return	controlLossTimer > 0; } }
	private bool bumpedWhileControlWasLost = false;
	private float controlRegainStartTimestamp = float.NegativeInfinity;

	private Joint2D footWeldJoint;
	private BreakableJoint2D footWeldBreakableJoint;
	private float footWeldDestroyedTimestamp = float.NegativeInfinity;

	public int contactsLayerMask = Physics2D.DefaultRaycastLayers;

	void Awake () {
		charController = GetComponent <CharController> ();
		// TODO: make component HeadingTransform and put it on child object. Here call GetComponentInChildren <HeadingTransform> ();
		headingTransform = transform.GetChild ( 0 );
		inventoryUser = GetComponentInChildren <InventoryUser> ();
		collisionTracker = GetComponent <CollisionContactsTracker> ();
		collisionDamageDispatcher = GetComponent <CollisionDamageDispatcher> ();
		soundPlayer = GetComponentInChildren <SoundPlayer> ();

		contactsLayerMask = PhysicsHelper.GetLayerCollisionMask ( gameObject.layer );
	}

	void Start () {
		collider2D.sharedMaterial = new PhysicsMaterial2D ( name + "'s PhysicsMaterial2D" );
		EnterStandState ();
	}

	void FixedUpdate () {
		TrackContacts ();

		bool controlWasLost = false;

		if ( controlLossTimer > 0 ) {
			if ( IsGrounded ) {
				controlLossTimer -= Time.fixedDeltaTime;

				if ( controlLossTimer < 0 )
					controlLossTimer = 0;

				if ( controlLossTimer == 0 )
					controlRegainStartTimestamp = Time.fixedTime;
			}

			controlWasLost = true;
		}

		// Allow using item only in certain few states.
		inventoryUser.CarriedItemIsActive = false;

		// Allow performing idle action only in truly idle state.
		bool canPerformIdleAction = false;

		// Some active actions must be viewed by the camera.
		bool performedMoveAction = false;

		bool regainingControl = Time.fixedTime - controlRegainStartTimestamp < ControlRegainDuration;

		/* TODO: move groups of statements inside if/else into their own methods,
		 * make FixedUpdate () method less "heavy" and more readable. */
		if ( !IsGrounded && footWeldJoint != null ) {
			/* Sometimes weld joint can move character into position where
			 * no ground contacts can be found by circle trace method.
			 * Such state is not valid since visually character is grounded,
			 * but it's not able to move or jump. */
			BreakFootWeld ();
		}

		if ( regainingControl ) {
			bumpedWhileControlWasLost = false;
			charController.SetState ( "ControlRegain", StateMaterials.ControlRegain );
		} else if ( controlWasLost ) {
			BreakFootWeld ();
			IsPreparingToJump = false;

			var velocity = rigidbody2D.velocity;
			bool changeHeading = false;

			if ( IsGrounded ) {
				if ( velocity.magnitude > ControlLostChangeHeadingMinVelocity )
					changeHeading = true;
			} else if ( velocity.magnitude > 0 )
				changeHeading = true;

			if ( AnyBumpBetweenFrames )
				bumpedWhileControlWasLost = true;

			if ( changeHeading )
				Heading = System.Math.Sign ( velocity.x );

			if ( IsGrounded || bumpedWhileControlWasLost )
				charController.SetState ( "ControlLostOnGround", StateMaterials.ControlLostOnGround );
			else
				charController.SetState ( "ControlLostMidAir", StateMaterials.ControlLostMidAir );
		} else {
			if ( IsPreparingToJump ) {
				if ( Time.fixedTime - jumpPreparationStartTimestamp >= JumpPreparationDuration ) {
					IsPreparingToJump = false;

					if ( IsGrounded ) {
						BreakFootWeld ();

						float jumpDirAngle;
						float jumpStartVelocity;

						if ( jumpForward ) {
							jumpDirAngle = ForwardJumpDirectionAngle;
							jumpStartVelocity = ForwardJumpStartVelocity;
							IsJumpingForward = true;
						} else {
							jumpDirAngle = BackJumpDirectionAngle;
							jumpStartVelocity = BackJumpStartVelocity;
							IsJumpingBackward = true;
						}

						float jumpDirAngleRad = jumpDirAngle * Mathf.Deg2Rad;
						jumpDir = new Vector2 ( Mathf.Cos ( jumpDirAngleRad ), Mathf.Sin ( jumpDirAngleRad ) );
						jumpDir.x *= Heading;

						var flatestFloorContact = FloorContacts.WithMax ( c => Vector2.Dot ( jumpDir, c.Normal ) );
						float frictionAccelFactor = CalculateFrictionAccelFactor ( flatestFloorContact );

						if ( frictionAccelFactor > 1 )
							frictionAccelFactor = 1;
						
						var velocityChange = jumpDir * jumpStartVelocity;
						velocityChange.x *= frictionAccelFactor;
						AddForceAndReaction ( velocityChange, ForceMode.VelocityChange, flatestFloorContact );
						lastJumpTimestamp = Time.fixedTime;

						stopJumpBounciness = false;
						performedMoveAction = true;

						soundPlayer.PlayVariation ( JumpSounds );
						charController.SetState ( IsJumpingForward ? "JumpingForward" : "JumpingBackward", StateMaterials.JumpBouncy );
					} else {
						// TODO: transfer control to fall-move processing code.
						EnterStandState ();
					}
				} else
					charController.SetState ( "PrepareToJump", StateMaterials.Stand );
			} else if ( IsJumping ) {
				bool isBouncy = Time.fixedTime - lastJumpTimestamp < JumpBouncinessDuration;

				if ( ( BumpTheFloorBetweenFrames || IsGrounded ) && !isBouncy ) {
					IsJumpingForward = false;
					IsJumpingBackward = false;
					EnterStandState ();
				} else {
					// Stop bouncing after first hit.
					if ( BumpTheWallOrCeilingBetweenFrames )
						stopJumpBounciness = true;

					StateMaterial stateMaterial;

					if ( isBouncy && !stopJumpBounciness ) {
						// Enable character bounciness.
						float angle = rigidbody2D.velocity.AngleRad ();
						float t = Mathf.Abs ( Mathf.Cos ( angle ) );
						stateMaterial = StateMaterial.Lerp ( StateMaterials.JumpNormal, StateMaterials.JumpBouncy, t );
					} else {
						// Disable bouncing.
						stateMaterial = StateMaterials.JumpNormal;
					}

					if ( IsJumpingBackward && Time.fixedTime - lastJumpTimestamp < BackJumpSideAccelerationDuration ) {
						/* Apply little amount of side acceleration so that the character
						 * can climb to the edge of a peak rather than bounce backward and fall down. */
						int sign = System.Math.Sign ( jumpDir.x );
						var accel = new Vector2 ( sign * BackJumpSideAcceleration, 0 );
						rigidbody2D.AddForce ( accel, ForceMode.Acceleration );

						DebugHelper.DrawRay ( transform.position, accel, Color.blue, 0, false );
					}

					charController.SetState ( IsJumpingForward ? "JumpingForward" : "JumpingBackward", stateMaterial );
				}
			} else {
				bool movingDown = Vector2.Dot ( rigidbody2D.velocity, -Vector2.up ) > 0;
				float downVelocity = movingDown ? rigidbody2D.velocity.magnitude : 0;
				// TODO: apply "Fall" animation after some time of being in the air. Velocity should not be accounted.

				if ( !IsGrounded && downVelocity >= FallVelocity )
					charController.SetState ( "Fall", StateMaterials.JumpNormal );
				else {
					bool noMovementPerformed = false;
					var move = new Vector2 ( MoveX, MoveY );
					float moveAmount = move.magnitude;

					if ( moveAmount != 0 ) {
						var moveDir = move.normalized;

						if ( moveAmount > 1 )
							moveAmount = 1;

						SurfaceContact surfaceContact = null;
						Vector2 moveDirAlongSurface = Vector2.zero;
						float surfaceAcceleration = 0;
						float gravityResistance = 0;
						float maxVelocity = 0;
						bool wantToWalk = false;
						bool canTurn = false;

						if ( IsGrounded ) {
							canTurn = true;
							var steepestFloorContact = FloorContacts.WithMin ( c => Vector2.Dot ( moveDir, c.Normal ) );
							var tangent = Common.RightOrthogonal ( steepestFloorContact.Normal );
							moveDirAlongSurface = Vector2.Dot ( moveDir, tangent ) > 0 ? tangent : -tangent;
						
							bool noObstaclesOnTheWay = frameContacts
								.All ( c =>
									Vector2.Dot ( c.Normal, moveDirAlongSurface ) >= 0 ||
									// Push dynamic body if it doesn't belong to other character.
									( c.Collider.attachedRigidbody != null && !IsCharacter ( c.Collider ) )
									/* TODO: check mass of the body? Presumably evaluate to false when object can't be moved.
									 * UPD: well, it depends on friction force applied from the floor to an obstacle object,
									 * so everything is much more complicated.. */
								);

							if ( noObstaclesOnTheWay ) {
								wantToWalk = true;
								surfaceContact = steepestFloorContact;
								surfaceAcceleration = WalkAcceleration;
								gravityResistance = WalkGravityResistance;
								maxVelocity = MaxWalkVelocity;
							} else {
								var obstacleContact = frameContacts.First ( c => Vector2.Dot ( c.Normal, moveDirAlongSurface ) < 0 );
								Debug.DrawRay ( obstacleContact.Point, obstacleContact.Normal * 0.1f, Color.magenta );
							}
						}
					
						if ( !wantToWalk ) {
							SurfaceContact staircaseContact;
							bool canClimb = CheckCanClimbStaircase ( moveDir.x, ClimbStepHeight, ClimbStepDepth, out staircaseContact );

							if ( canClimb ) {
								canTurn = true;
								wantToWalk = true;
								surfaceContact = staircaseContact;

								float climbMoveDirAngleRad = ClimbMovementDirectionAngle * Mathf.Deg2Rad;
								var climbDir = new Vector2 ( Mathf.Cos ( climbMoveDirAngleRad ), Mathf.Sin ( climbMoveDirAngleRad ) );
								climbDir.x *= moveDir.x;
								moveDirAlongSurface = climbDir;

								surfaceAcceleration = ClimbAcceleration;
								gravityResistance = ClimbGravityResistance;
								maxVelocity = MaxClimbVelocity;
							}
						}

						if ( wantToWalk ) {
							BreakFootWeld ();
							StateMaterial stateMaterial;

							if ( IsCharacter ( surfaceContact.Collider ) )
								stateMaterial = StateMaterials.StepOverCharacter;
							else
								stateMaterial = StateMaterials.Walk;

							var otherRigidbody = surfaceContact.Collider.attachedRigidbody;
							var platformVelocity = Vector2.zero;

							if ( otherRigidbody != null )
								platformVelocity = otherRigidbody.GetPointVelocity ( surfaceContact.Point );

							var relVelocity = rigidbody2D.velocity - platformVelocity;
							var velocityAlongSurface = Common.Project ( relVelocity, moveDirAlongSurface );

							float frictionAccelFactor = CalculateFrictionAccelFactor ( surfaceContact );
							surfaceAcceleration *= frictionAccelFactor;

							var velocityDelta = moveDirAlongSurface * surfaceAcceleration * Time.fixedDeltaTime;
							var newVelocityAlongSurface = velocityAlongSurface + velocityDelta;
							Vector2 velocityChange;

							// DEV: new approach.
							// TODO: slow down when walking down steep ground.
							if ( newVelocityAlongSurface.magnitude > maxVelocity ) {
								if ( newVelocityAlongSurface.magnitude > velocityAlongSurface.magnitude ) {
									if ( velocityAlongSurface.magnitude < maxVelocity )
										velocityChange = velocityAlongSurface.normalized * maxVelocity - velocityAlongSurface;
									else
										velocityChange = Vector2.zero;
								} else
									velocityChange = newVelocityAlongSurface - velocityAlongSurface;
							} else
								velocityChange = newVelocityAlongSurface - velocityAlongSurface;

							// TODO: old approach.
							//newVelocityAlongSurface = Vector2.ClampMagnitude ( newVelocityAlongSurface, maxVelocity );
							//velocityChange = newVelocityAlongSurface - velocityAlongSurface;

							Debug.DrawRay ( transform.position, velocityChange, Color.red, 0, false );

							AddForceAndReaction ( velocityChange, ForceMode.VelocityChange, surfaceContact );
							AddForceAndReaction ( -Physics2D.gravity * gravityResistance, ForceMode.Acceleration, surfaceContact );

							soundPlayer.PlayVariation ( WalkSounds, restart : false );
							charController.SetState ( "Walk", stateMaterial );
						} else
							noMovementPerformed = true;

						if ( canTurn ) {
							var headingScale = headingTransform.localScale;
							headingScale.x = moveDir.x;
							headingTransform.localScale = headingScale;
						}
					} else
						noMovementPerformed = true;

					bool readyToJump = Time.fixedTime - lastJumpTimestamp >= JumpCooldown;
					bool wantsToPerformJump = PerformForwardJump || PerformBackJump;

					if ( wantsToPerformJump && IsGrounded && readyToJump ) {
						jumpPreparationStartTimestamp = Time.fixedTime;
						IsPreparingToJump = true;
						/* TODO: disable IsPreparingToJump when control lost.
						 * And maybe in some other cases too. */
						jumpForward = PerformForwardJump;
					}

					if ( noMovementPerformed ) {
						canPerformIdleAction = true;
						bool performedIdleAction = false;

						if ( IsStandingStill ( FootWeldMaxVelocity ) )	// TODO: use other value instead of FootWeldMaxVelocity?
							performedIdleAction = charController.PerformIdleAction ();

						if ( !performedIdleAction )
							EnterStandState ();
					} else
						performedMoveAction = true;
				}
			}
		}

		if ( !canPerformIdleAction )
			charController.InterruptIdleAction ();

		if ( performedMoveAction )
			CameraEvents.Moved.Restart ();
	}

	private void EnterStandState () {
		if ( charController.IsControlledByPlayer && !charController.WeaponIsConcealed )
			inventoryUser.CarriedItemIsActive = true;

		TryWeldFeetToTheGround ();
		charController.SetState ( "Stand", IsGrounded ? StateMaterials.Stand : StateMaterials.JumpNormal );
	}

	private void TryWeldFeetToTheGround () {
		if ( footWeldJoint == null &&
			 Time.fixedTime - footWeldDestroyedTimestamp >= FootWeldPause &&
			 IsStandingStill ( FootWeldMaxVelocity )
		) {
			var flatestFloorContact = FloorContacts.WithMax ( c => Vector2.Dot ( Vector2.up, c.Normal ) );
			var surfaceMaterial = GetSurfaceMaterial ( flatestFloorContact );

			if ( surfaceMaterial.friction < FootWeldMinFriction )
				return;

			/* Hinge joint reports more reliable reaction forces than distance joint,
			 * but it fixes angle of connected body because angle of the character is fixed. */
			// TODO: try not-short-distance SpringJoint. Probably it would report more accurate reaction force.
			var weldJoint = gameObject.AddComponent <DistanceJoint2D> ();
			weldJoint.distance = 0;
			weldJoint.connectedBody = flatestFloorContact.Collider.attachedRigidbody;
			weldJoint.collideConnected = true;
			weldJoint.anchor = transform.InverseTransformPoint ( flatestFloorContact.Point );

			if ( weldJoint.connectedBody != null ) {
				weldJoint.connectedAnchor = weldJoint.connectedBody.transform
					.InverseTransformPoint ( flatestFloorContact.Point );
			} else
				weldJoint.connectedAnchor = flatestFloorContact.Point;

			footWeldJoint = weldJoint;

			footWeldBreakableJoint = gameObject.AddComponent <BreakableJoint2D> ();
			footWeldBreakableJoint.Joint = weldJoint;
			footWeldBreakableJoint.BreakForce = FootWeldBreakForce;
		}
	}

	public bool IsStandingStill ( float maxVelocity ) {
		if ( IsGrounded && !IsPreparingToJump ) {
			var flatestFloorContact = FloorContacts.WithMax ( c => Vector2.Dot ( Vector2.up, c.Normal ) );
			var floorBody = flatestFloorContact.Collider.attachedRigidbody;
			Vector2 floorVelocity;

			if ( floorBody != null )
				floorVelocity = floorBody.GetPointVelocity ( flatestFloorContact.Point );
			else
				floorVelocity = Vector2.zero;

			var footVelocity = rigidbody2D.GetPointVelocity ( flatestFloorContact.Point );
			var relativeVelocity = footVelocity - floorVelocity;

			return	relativeVelocity.magnitude <= maxVelocity;
		} else
			return	false;
	}

	private void BreakFootWeld () {
		if ( footWeldJoint != null ) {
			Destroy ( footWeldJoint );
			footWeldJoint = null;
			footWeldDestroyedTimestamp = Time.fixedTime;
		}
	}

	void OnJointBreak2D ( BrokenJointData brokenJointData ) {
		footWeldDestroyedTimestamp = Time.fixedTime;
	}

	private void AddForceAndReaction ( Vector2 amountAndDir, ForceMode mode, SurfaceContact surfaceContact ) {
		rigidbody2D.AddForce ( amountAndDir, mode );

		if ( surfaceContact != null ) {
			var surfaceBody = surfaceContact.Collider.attachedRigidbody;

			if ( surfaceBody != null ) {
				amountAndDir = PhysicsHelper.ConvertToNewtons ( amountAndDir, mode, surfaceBody.mass );
				surfaceBody.AddForce ( -amountAndDir * SurfaceReactionFactor, ForceMode2D.Force );
			}
		}
	}

	void LateUpdate () {
		if ( ControlLost && !IsGrounded && !bumpedWhileControlWasLost ) {
			var rotationDir = rigidbody2D.velocity;

			if ( Heading == -1 )
				rotationDir = -rotationDir;

			headingTransform.localRotation = Common.RotateAlongDirection ( rotationDir );
		} else
			headingTransform.localRotation = Quaternion.identity;
	}

	public void IncreaseControlLossTimer ( float duration ) {
		controlLossTimer += duration;
		controlLossTimer = Mathf.Clamp ( controlLossTimer, 0, MaxControlLossDuration );
	}

	private PhysicsMaterial2D GetSurfaceMaterial ( SurfaceContact surfaceContact ) {
		var surfaceMaterial = surfaceContact.Collider.sharedMaterial;

		if ( surfaceMaterial == null ) {
			if ( DefaultSurfaceMaterial == null )
				DefaultSurfaceMaterial = new PhysicsMaterial2D ( "DefaultSurfaceMaterial for the Character" );

			surfaceMaterial = DefaultSurfaceMaterial;
		}

		return	surfaceMaterial;
	}

	private float CalculateFrictionAccelFactor ( SurfaceContact surfaceContact ) {
		var surfaceMaterial = GetSurfaceMaterial ( surfaceContact );

		float combinedFriction = PhysicsHelper.CombineFriction (
			collider2D.sharedMaterial.friction,
			surfaceMaterial.friction
		);

		float frictionAccelFactor = Mathf.Clamp (
			Mathf.Sqrt ( combinedFriction ),
			MinFrictionAccelFactor,
			MaxFrictionAccelFactor
		);

		return	frictionAccelFactor;
	}

	private bool CheckCanClimbStaircase ( float horzMoveDir, float stepHeight, float stepDepth, out SurfaceContact staircaseContact ) {
		staircaseContact = null;

		if ( frameContacts.Count == 0 )
			return	false;

		var circleCollider = collider2D as CircleCollider2D;
		var center = ( Vector2 ) circleCollider.bounds.center;
		var horzMoveDirVector = new Vector2 ( horzMoveDir, 0 );

		// Seek forward for obstacle to climb on.
		var steepestContact = frameContacts.WithMin ( c => Vector2.Dot ( c.Normal, horzMoveDirVector ) );

		if ( Vector2.Dot ( steepestContact.Normal, horzMoveDirVector ) >= 0 || IsWalkable ( steepestContact.Normal ) )
			return	false;

		staircaseContact = steepestContact;

		// Cast up.
		Debug.DrawRay ( center, Vector2.up * stepHeight, Color.cyan );

		var hit = Spatial.CircleCastFiltered (
			center, circleCollider.radius, Vector2.up, stepHeight,
			h => h.rigidbody == this.rigidbody2D,
			contactsLayerMask
		);
		
		if ( hit.collider != null ) {
			Debug.DrawRay ( hit.point, hit.normal * 0.1f, Color.red );

			return	false;
		}

		center += Vector2.up * stepHeight;

		// Cast forward.
		Debug.DrawRay ( center, horzMoveDirVector * stepDepth, Color.cyan );

		hit = Spatial.CircleCastFiltered (
			center, circleCollider.radius, horzMoveDirVector, stepDepth,
			h => h.rigidbody == this.rigidbody2D,
			contactsLayerMask
		);

		if ( hit.collider != null ) {
			Debug.DrawRay ( hit.point, hit.normal * 0.1f, Color.red );

			return	false;
		}

		return	true;
	}

	private void TrackContacts () {
		var circleCollider = collider2D as CircleCollider2D;
		var center = circleCollider.bounds.center;
		frameContacts = Spatial.GetCircleContacts (
			center,
			circleCollider.radius,
			circleCollider.radius * 0.1f,
			Spatial.IgnoreSelfCollisionPredicate ( circleCollider ),
			16,
			contactsLayerMask
		);
		
		FindFloor ( frameContacts );

		if ( IsGrounded ) {
			lastGroundedTimestamp = Time.fixedTime;

			foreach ( var floorContact in FloorContacts ) {
				Debug.DrawRay ( floorContact.Point, floorContact.Normal * 0.1f, Color.green );
			}
		}
	}

	private void FindFloor ( IEnumerable <SurfaceContact> contacts ) {
		FloorContacts.Clear ();

		foreach ( var contact in contacts ) {
			Debug.DrawRay ( contact.Point, contact.Normal * 0.1f, Color.yellow );

			if ( IsWalkable ( contact.Normal ) )
				FloorContacts.Add ( contact );
		}
	}

	private bool IsWalkable ( Vector2 normal ) {
		float angle = Vector2.Angle ( normal, Vector2.up );

		return	angle <= MaxWalkAngle;
	}

	public static bool IsCharacter ( Component component ) {
		var charController = component.GetComponent <CharacterMovement> ();

		return	charController != null;
	}

	[System.Serializable]
	public class StatePhysicMaterials {
		public const float DefaultStandFriction = 50;
		public StateMaterial Stand = new StateMaterial ( DefaultStandFriction, 0 );
		public StateMaterial Walk = new StateMaterial ( 2, 0 );
		public StateMaterial JumpBouncy = new StateMaterial ( 0.05f, 0.5f );
		public StateMaterial JumpNormal = new StateMaterial ( 0.5f, 0 );
		public StateMaterial StepOverCharacter = new StateMaterial ( 1 / DefaultStandFriction, 0 );
		public StateMaterial ControlLostMidAir = new StateMaterial ( 0.01f, 0.3f );
		public StateMaterial ControlLostOnGround = new StateMaterial ( 2, 0 );
		public StateMaterial ControlRegain = new StateMaterial ( 8, 0 );
	}

	[System.Serializable]
	public class CameraEventList {
		public CameraEvent Moved;
	}
}