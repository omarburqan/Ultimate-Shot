using UnityEngine;

// CoverBehaviour inherits from GenericBehaviour. This class corresponds to take cover and covering moves behaviour.
public class CoverBehaviour : GenericBehaviour
{
	public string coverButton = "Fire3";             // Default cover button (take/exit/turn/change/jump over).
	public float camCornerOffset = 1.0f;             // Default camera shift offset to peek corners.
	public float orientationSmooth = 0.3f;           // Default smooth factor to re-orientate player when turning corners or rounded covers.
	public float crouchSmooth = 0.1f;                // Default smooth factor to change from cover standing to crouch.
	public float searchForCoverDist = 7.5f;          // Default cast distance to search for covers.
	public float showPathDelay = 1.5f;               // Default delay before show the path to cover.
	public float castOriginHeight = 0.3f;            // Height of the cast origin for special movements.
	public Color pathColor =
		new Color(0f, 1f, 1f, 0.3f);                 // Default line color for the path to cover.
	public GameObject coverSign;                     // Take cover sign canvas.
	public GameObject turnCoverSign;                 // Turn on cover corner sign canvas.
	public GameObject changeCoverSign;               // Change cover sign canvas.
	public GameObject jumpCoverSign;                 // Jump over cover sign canvas.

	private bool takeCover;                          // Boolean to determine whether or not the player has to take cover.
	private bool isCovering;                         // Boolean to determine whether or not the player is covering.
	private bool possibleCover;                      // Boolean to determine if exists a possible cover to take.
	private bool isAiming;                           // Boolean to get whether or not the player is aiming.
	private int layerMask;                           // Layer mask to cast when searching for covers.
	private RaycastHit coverWall;                    // Current cover wall.
	private RaycastHit projectedCoverWall;           // Possible cover wall to go.
	private int currentCoverID;                      // ID of the current cover.
	private float coverDist = 0.4f;                  // Minimum distance to take cover directly (above this the navigation is used).
	private Vector3 shift;                           // Projected future position when moving on cover (edge of collider).
	private UnityEngine.AI.NavMeshPath path;         // Partial path to the cover resulting from the search method.
	private Vector3[] navPath;                       // Full navigation path to the cover, including shifted and final waypoints.
	private LineRenderer line;                       // Line renderer to show the path to cover.
	private bool navToDest;                          // Boolean to determine if the player has to navigate to cover.
	private int wayPointIndex;                       // Index of waypoint corner in path to cover.
	private int coveringBool, aimBool;               // Animator variables related to covering and aiming.
	private int crouchFloat;                         // Animator variable related to crouching or standing on cover blend tree sate. 
	private int cornerBool;                          // Animator variable related to cover corner.
	private int changeCoverBool;                     // Animator variable related to change cover action.
	private int jumpCoverBool;                       // Animator variable related to jump over cover action.
	private bool isCrouching;                        // Boolean to determine whether or not the player is crouching.
	private float standingHeight;                    // Size of the player collider when standing.
	private bool corner;                             // Boolean to get whether or not the player is on a corner.
	private enum CoverActions                        // All existing cover special actions.
	{
		NONE,
		TURN,
		CHANGE,
		JUMP
	}
	private CoverActions possibleAction;             // Possible cover special action to take.
	private CoverActions currentAction;              // Cover special action being performed.
	private float maxJumpCoverDist;                  // Maximum length of a cover the player can jump. 
	private Vector3 jumpCoverEnd;                    // Realtime adjustable cover end depth for target matching.
	private float horizontalClampAngle = 50f;        // Aiming clamp angle when peeking a corner.
	private float showPathTimer;                     // Timer to determine when path to cover will be shown.
	private CapsuleCollider col;                     // Reference to the player collider.

	// Start is always called after any Awake functions.
	void Start ()
	{
		// Set up the references.
		possibleAction = CoverActions.NONE;
		currentAction = CoverActions.NONE;
		layerMask = 1 << LayerMask.NameToLayer("Cover");
		coveringBool = Animator.StringToHash ("Cover");
		crouchFloat = Animator.StringToHash ("Crouch");
		cornerBool = Animator.StringToHash ("Corner");
		changeCoverBool = Animator.StringToHash ("ChangeCover");
		jumpCoverBool = Animator.StringToHash ("JumpCover");
		aimBool = Animator.StringToHash("Aim");
		shift = Vector3.zero;
		path = new UnityEngine.AI.NavMeshPath ();
		line = this.GetComponent<LineRenderer>();
		wayPointIndex = 0;
		col = GetComponent<CapsuleCollider> ();
		standingHeight = col.height;
		maxJumpCoverDist = col.radius * 5;

		// Subscribe this behaviour on the manager.
		behaviourManager.SubscribeBehaviour (this);
	}

	// Update is used to set features regardless the active behaviour.
	void Update ()
	{
		// Get current aim status.
		isAiming = behaviourManager.GetAnim.GetBool(aimBool);

		// Handle possible cover target features (sign, path).
		possibleCover = HasPossibleCover(GetCamTarget(), out projectedCoverWall);
		if (!possibleCover || navToDest || !CanTakeCover())
		{
			showPathTimer = 0;
			UndrawPath();
			UndrawSign(coverSign);
		}

		// Cover actions, entering/exiting or changing covers.
		if (CanTakeCover() && (possibleCover || isCovering) && !navToDest)
		{
			// Can't navigate yet due to show path delay?
			if (possibleCover && !HandleNavPath())
			{
				possibleCover = false;
				if (!isCovering)
					return;
			}

			// Handle enter/exit/covering special actions when cover button is pressed.
			if (Input.GetButtonUp (coverButton))
			{
				HandleCoverActions();

				takeCover = !takeCover || (currentAction != CoverActions.NONE);

				// Deal with changing covers situation.
				if (isCovering && possibleCover && !takeCover)
				{
					isCovering = false;
					takeCover = true;
				}

				// Player will take cover.
				if (takeCover)
				{
					behaviourManager.RegisterBehaviour(this.behaviourCode);
					
					// Setup navigation to cover.
					if (!isCovering)
					{
						// No overriding while navigating to cover.
						behaviourManager.LockTempBehaviour(this.behaviourCode);
						// Set navigation to take cover parameters.
						navToDest = true;
						behaviourManager.GetCamScript.SetFOV(behaviourManager.sprintFOV);
						coverWall = projectedCoverWall;
					}
				}
				// Exiting cover.
				else
				{
					behaviourManager.GetAnim.SetFloat(speedFloat, 0.5f);
					EndCover();
				}
			}
		}

		// Handle covering parameters once per frame.
		if (takeCover)
		{
			// Limit rotation angle when corner peek aiming.
			if (isAiming && corner)
			{
				behaviourManager.GetCamScript.ToggleClampHorizontal(-horizontalClampAngle, horizontalClampAngle, -coverWall.normal);
			}
			// Clamp off.
			else
			{
				behaviourManager.GetCamScript.ToggleClampHorizontal();
			}

			// Align jump over cover direction (jumo over is a transitional action to default behaviour).
			if (currentAction == CoverActions.JUMP)
			{
				JumpOverMatchTarget();
			}
		}

		// Trigger anim states for covering.
		behaviourManager.GetAnim.SetBool(coveringBool, isCovering);
		
		//Can't sprint when covering
		canSprint = !isCovering;
	}

	// Manage the possible cover path.
	private bool HandleNavPath()
	{
		// Player will navigate to cover?
		if (Vector3.Distance(transform.position, projectedCoverWall.point) > coverDist && (possibleAction != CoverActions.CHANGE))
		{
			showPathTimer += Time.deltaTime;

			// Delay to show path to cover.
			if (showPathTimer < showPathDelay)
			{
				return false;
			}
			//Show path to cover.
			else
			{
				// Calculate a possible path and draw it.
				if(!DrawPath(projectedCoverWall.point + (projectedCoverWall.normal * 0.1f)))
					return false;

				// Put the take cover sign at the possible cover.
				Vector3 signPosition = projectedCoverWall.point + (projectedCoverWall.normal * 0.01f) + (Vector3.up * 0.4f);
				DrawSign(coverSign, signPosition, -projectedCoverWall.normal);
			}
		}
		// Player already in front of cover?
		else
		{
			// Get cover waypoint directly.
			navPath = new Vector3[1];
			navPath[0] = projectedCoverWall.point;
		}
		return true;
	}

	// Manage cover action when cover button is pressed.
	private void HandleCoverActions()
	{
		switch (possibleAction)
		{
			// Change covers situation.
			case CoverActions.CHANGE:
				currentAction = CoverActions.CHANGE;
				UndrawSign(changeCoverSign);
				behaviourManager.GetAnim.SetBool(changeCoverBool, true);
				behaviourManager.LockTempBehaviour(this.behaviourCode);
				break;
			// Turn cover corner situation.
			case CoverActions.TURN:
				currentAction = CoverActions.TURN;
				break;
			// Jump over cover situation.
			case CoverActions.JUMP:
				currentAction = CoverActions.JUMP;
				behaviourManager.SetLastDirection(-coverWall.normal);
				behaviourManager.GetAnim.SetBool(jumpCoverBool, true);
				behaviourManager.LockTempBehaviour(this.behaviourCode);
				break;
			default:
				currentAction = CoverActions.NONE;
				break;
		}
	}

	// Exit covering mode.
	private void EndCover()
	{
		isCovering = false;
		SetCrouchStatus(false);

		behaviourManager.UnregisterBehaviour(this.behaviourCode);
		behaviourManager.UnlockTempBehaviour(this.behaviourCode);

		currentCoverID = 0;
		shift = Vector3.zero;
		behaviourManager.GetAnim.speed = 1f;
	}

	// Handles the case where other behaviour overrides this (ex.: aim behaviour).
	public override void OnOverride()
	{
		UndrawPath();
		UndrawSign(coverSign);
		UndrawSign(turnCoverSign);
		UndrawSign(changeCoverSign);
		UndrawSign(jumpCoverSign);
		SetCrouchStatus(false);
	}

	// Align the jump over cover direction and ensure hand will touch middle of cover.
	void JumpOverMatchTarget()
	{
		float targetHeight = coverWall.collider.bounds.center.y + coverWall.collider.bounds.extents.y;
		Vector3 targetPoint = coverWall.point;
		targetPoint.y = targetHeight;
		jumpCoverEnd.y = targetHeight;

		Vector3 middlePoint = (targetPoint + jumpCoverEnd) / 2;
		middlePoint.y = targetHeight;

		if (behaviourManager.GetAnim.GetCurrentAnimatorStateInfo (0).IsName ("JumpOverCover"))
		{
			behaviourManager.GetAnim.MatchTarget (middlePoint, Quaternion.LookRotation (-coverWall.normal), AvatarTarget.Root, 
				new MatchTargetWeightMask (Vector3.one, 1f), 0.3f, 0.5f); 
		}
	}

	// End the change cover action.
	public void EndChangeCover()
	{
		currentAction = CoverActions.NONE;
		behaviourManager.GetAnim.SetBool (changeCoverBool, false);
		behaviourManager.UnlockTempBehaviour(this.behaviourCode);
	}

	// Calculate and draw the path to the cover.
	bool DrawPath(Vector3 destination)
	{
		// Call pathfind method for a possible path to the cover.
		if (!UnityEngine.AI.NavMesh.CalculatePath(transform.position + 2 * shift, destination, UnityEngine.AI.NavMesh.AllAreas, path))
			return false;

		navPath = new Vector3[path.corners.Length + 1];

		// Create line renderer for the first time.
		if (line == null)
		{
			line = this.gameObject.AddComponent<LineRenderer> ();
			line.material = new Material (Shader.Find ("Sprites/Default")) { color = pathColor };
			line.startWidth = 0.01f;
			line.endWidth = 0.02f;
		}
		line.enabled = true;
		line.positionCount = path.corners.Length + 1;
		line.SetPosition (0, transform.position);

		// Draw the path line.
		for (int i = 0; i < path.corners.Length; i++)
		{
			navPath[i] = path.corners[i];
			line.SetPosition(i + 1, path.corners[i]);
		}
		// Use cover normal to put the last waypoint slightly before the cover wall.
		if (path.corners.Length > 0)
		{
			Vector3 lastPos = destination - 0.1f * (destination - navPath[path.corners.Length - 1]);
			line.SetPosition(path.corners.Length, destination);
			navPath[path.corners.Length] = lastPos;
		}
		return true;
	}

	// Draw the cover signs.
	void DrawSign(GameObject sign, Vector3 destination, Vector3 normal, float xRot = 0f, int direction = 0, bool reScale = false)
	{
		if (sign != null)
		{
			sign.SetActive (true);
			sign.transform.position = destination;
			sign.transform.rotation = Quaternion.LookRotation (normal);
			// Re-scale the sign?
			if(reScale)
				sign.transform.localScale = new Vector3 (direction, 1f, 1f) * sign.transform.localScale.z;
			// Rotation on X axis.
			if (xRot > 0)
				sign.transform.Rotate (Vector3.right * xRot);
		}
	}

	// Hide the cover signs.
	void UndrawSign(GameObject sign)
	{
			sign.SetActive (false);
	}

	// Hide the path to cover.
	void UndrawPath()
	{
		if (line != null)
		{
			line.enabled = false;
		}
	}

	// Handle standing or crouching cover position.
	void SetCrouchStatus(bool crouch)
	{
		if(crouch)
		{
			behaviourManager.GetCamScript.SetYCamOffset(0f);
			col.height = 0.65f * standingHeight;
		}

		else
		{
			behaviourManager.GetCamScript.ResetTargetOffsets();
			col.height = standingHeight;
		}
		col.center = Vector3.up * (col.height / 2 - 0.01f);
	}

	// LocalFixedUpdate overrides the virtual function of the base class.
	public override void LocalFixedUpdate()
	{
		// Navigate to cover?
		if (navToDest)
		{
			NavigateToCover ();
		}
		// Possible states to handle cover.
		else if (currentAction != CoverActions.CHANGE && currentAction != CoverActions.JUMP && takeCover && !isAiming)
		{
			CoverManagement ();
		}

		// Jump cover is a transitional action that exits cover behaviour.
		JumpCoverManagement();

		// Ensure player will exit cover when falling.
		if (!behaviourManager.IsGrounded())
		{
			EndCover();
		}
	}

	// Follow the path to cover.
	// This code turn/stop immediately on waypoints, differing from Unity default navmesh agent.
	void NavigateToCover()
	{
		// Already at destination?
		if (navPath.Length == 0)
		{
			StartTakeCover();
			return;
		}

		// Change waypoint.
		if (Vector3.Distance (transform.position, navPath [wayPointIndex]) < coverDist)
		{
			wayPointIndex++;

			// Arrived at destination?
			if (wayPointIndex == navPath.Length)
			{
				path.ClearCorners();
				StartTakeCover();
				return;
			}
		}

		// Re-orientate the player to the next waypoint.
		behaviourManager.GetAnim.SetFloat(speedFloat, 2.0f);
		Vector3 direction = navPath [wayPointIndex] - transform.position;
		direction.y = 0;
		Rotating (direction.normalized);
	}

	// Start taking cover initial parameters.
	private void StartTakeCover()
	{
		navToDest = false;
		isCovering = true;
		behaviourManager.GetAnim.SetFloat(speedFloat, 0f);
		currentCoverID = coverWall.collider.gameObject.GetInstanceID();
		wayPointIndex = 0;
		behaviourManager.UnlockTempBehaviour(this.behaviourCode);
		behaviourManager.GetCamScript.ResetFOV();
	}

	// Handle cover.
	private void CoverManagement()
	{
		UpdateCoverParameters (-coverWall.normal);
		CheckForCorners ();

		// Deal with player distance to the cover wall.
		if ((transform.position - coverWall.point).sqrMagnitude > 0.1)
		{
			// Too far from cover, exit.
			if((transform.position - coverWall.point).sqrMagnitude > 0.8)
			{
				takeCover = false;
				EndCover();
			}
			// Force player against cover (deal with irregular horizontal movements).
			else
				behaviourManager.GetRigidBody.AddForce (-100 * coverWall.normal, ForceMode.Acceleration);
		}

		// Make the camera follows the player when orientation is changing (turning corners or rounded covers).
		if (behaviourManager.IsHorizontalMoving ())
			behaviourManager.GetCamScript.LockOnDirection (-coverWall.normal);
		else
			behaviourManager.GetCamScript.UnlockOnDirection ();

		// Turn player against the cover.
		Rotating(coverWall.normal);
	}

	// Handle the jump over cover action.
	private void JumpCoverManagement()
	{
		// Can only jump over cover if player is crouching, cover depth is within max range, not moving, pressing forward button, and is in FOV.
		Vector3 castOrigin = transform.position - (maxJumpCoverDist + 0.1f + col.radius) * transform.forward + standingHeight * Vector3.up;
		bool canJumpCover = !Physics.Raycast (castOrigin, Vector3.down, 2.0f, layerMask);
		canJumpCover = canJumpCover && !Physics.Raycast(castOrigin, transform.forward, 1.5f);
		canJumpCover = canJumpCover && (Mathf.Abs(behaviourManager.GetH) < 0.2f);
		canJumpCover = canJumpCover && (behaviourManager.GetV > 0.5f) && isCrouching;
		canJumpCover = canJumpCover && InPLayerFOV (GetCamTarget (), true);

		// Draw jump cover cover sign and adjust cover end depth.
		if (canJumpCover && currentAction == CoverActions.NONE)
		{
			possibleAction = CoverActions.JUMP;
			Vector3 signPosition = transform.position + (Vector3.up * 1.5f);
			DrawSign(jumpCoverSign, signPosition, -coverWall.normal, 30f);
			jumpCoverEnd = transform.position - (AdjustCoverEnd(maxJumpCoverDist) + col.radius) * transform.forward + standingHeight * Vector3.up;
			Debug.DrawRay(jumpCoverEnd, Vector3.down, canJumpCover ? Color.green : Color.red);
		}
		else
		{
			UndrawSign(jumpCoverSign);
			if (possibleAction == CoverActions.JUMP && currentAction == CoverActions.NONE)
				possibleAction = CoverActions.NONE;
		}
	}

	// Refine position of the cover end in realtime.
	private float AdjustCoverEnd(float beginDist)
	{
		bool stillJump = true;
		while (stillJump)
		{
			beginDist -= 0.1f;
			stillJump = !Physics.Raycast (transform.position - (beginDist + col.radius) * transform.forward + standingHeight * Vector3.up,
				Vector3.down, 2.0f, layerMask);
		}
		return beginDist;
	}

	// Start jumping over cover (called by animation).
	public void StartJumpOver()
	{
		col.enabled = false;
	}

	// End jumping over cover (called by animation).
	public void EndJumpOver()
	{
		possibleAction = currentAction = CoverActions.NONE;
		col.enabled = true;
		behaviourManager.GetAnim.SetBool (jumpCoverBool, false);
		behaviourManager.GetAnim.SetFloat (speedFloat, 0.1f);
		takeCover = false;
		EndCover();
	}

	// Manage parameters like cover wall normal, cover wall size, camera FOV.
	private void UpdateCoverParameters(Vector3 direction)
	{
		// Calculate current cover wall normal.
		Physics.SphereCast (transform.position, 0.9f*col.radius, direction, out coverWall, 0.5f, layerMask);

		// Check if player has to crouch or stand.
		isCrouching = !Physics.Raycast(transform.position+(standingHeight * Vector3.up), direction, 0.5f);
		if(currentAction != CoverActions.TURN)
			behaviourManager.GetAnim.SetFloat (crouchFloat, isCrouching?1:0, crouchSmooth, Time.deltaTime);
		SetCrouchStatus(isCrouching);
		Debug.DrawRay(transform.position + (standingHeight * Vector3.up), -coverWall.normal, isCrouching ? Color.red : Color.green);

		// Get the player FOV.
		bool inFOV = InFOV(coverWall.normal, GetCamTarget(), true);

		// Aim mode can override only when crouched or peeking corners.
		if (!((inFOV && corner) || (inFOV && isCrouching)))
		{
			behaviourManager.LockTempBehaviour(this.behaviourCode);
		}
		else
		{
			behaviourManager.UnlockTempBehaviour(this.behaviourCode);
		}
		Debug.DrawRay(transform.position + Vector3.up * col.height, GetCamTarget(), inFOV ? Color.green : Color.red);

		// Handle transition to different cover after special action.
		if (NotSameCover (coverWall))
		{
			currentCoverID = coverWall.collider.gameObject.GetInstanceID();
		}
	}

	// Deal with corner moves, dead ends.
	private void CheckForCorners()
	{
		// Get the direction of cover movement.
		int direction = 0;
		if(behaviourManager.IsHorizontalMoving())
			direction = (int)Mathf.Sign (behaviourManager.GetH);

		// Get projected position when moving (collider edge).
		shift.x = -coverWall.normal.z;
		shift.z = coverWall.normal.x;
		shift.y = 0;
		shift *= direction;
		shift *= col.radius;
		Ray ray = new Ray ((castOriginHeight * Vector3.up) - (col.radius *0.9f* transform.forward) + transform.position + shift, -coverWall.normal);

		// Is player on a corner?
		corner = !Physics.Raycast (ray.origin, ray.direction, 1.0f, layerMask);
		Debug.DrawRay(ray.origin, ray.direction, corner ? Color.red : Color.green);

		// Handle dead ends.
		DeadEndManagement(ray);

		// Handle turning cover action started on previous frames.
		TurnCoverManagement();

		// Check if a special action is possible.
		RaycastHit hit;
		if(CanChangeCover(ray, out hit))
		{
			possibleAction = CoverActions.CHANGE;
		}
		else if(CanTurnCover(ray, out hit))
		{
			possibleAction = CoverActions.TURN;
		}
		else if(possibleAction != CoverActions.JUMP)
		{
			possibleAction = CoverActions.NONE;
		}

		// Draw special action sign.
		Vector3 signPosition = transform.position + (3f * shift) + (Vector3.up * 0.5f);
		switch(possibleAction)
		{
			case CoverActions.CHANGE:
				DrawSign(changeCoverSign, signPosition, -coverWall.normal, 90f, direction, true);
				UndrawSign(turnCoverSign);
				break;
			case CoverActions.TURN:
				DrawSign(turnCoverSign, signPosition, -coverWall.normal, 90f, direction, true);
				UndrawSign(changeCoverSign);
				break;
			default:
				UndrawSign(changeCoverSign);
				UndrawSign(turnCoverSign);
				break;
		}

		// Shift camera position on corners.
		int factor = ((corner && isCovering) && behaviourManager.IsHorizontalMoving()) ? 1 : 0;
		factor *= direction;
		behaviourManager.GetCamScript.SetXCamOffset(factor * camCornerOffset);

		// Set corner parameter on animator controller.
		behaviourManager.GetAnim.SetBool(cornerBool, corner);
	}

	// Deal with dead ends.
	private void DeadEndManagement(Ray ray)
	{
		// End of cover situation.
		if (corner && Physics.Raycast(ray.origin, ray.direction, 1.0f))
		{
			behaviourManager.GetAnim.speed = 0f;
			return;
		}

		// Convex corner situation.
		RaycastHit h;
		if (Physics.Raycast(ray.origin - shift, 2 * shift, out h, 2 * shift.magnitude))
		{
			behaviourManager.GetAnim.speed = 0f;
			return;
		}
		else
		{
			behaviourManager.GetAnim.speed = 1f;
		}
	}

	// Handle the turn cover corner action.
	private void TurnCoverManagement()
	{
		// End turning cover when passed corner.
		if ((currentAction == CoverActions.TURN) && !corner)
		{
			currentAction = CoverActions.NONE;
		}

		// Can turn the cover corner only when no path to other cover is avaliable.
		if (!possibleCover)
		{
			// Ignore corner stop until passed the current corner.
			corner = corner && (currentAction != CoverActions.TURN);
		}
	}

	// Check if can change to a next cover using special action.
	private bool CanChangeCover(Ray ray, out RaycastHit hit)
	{
		hit = default(RaycastHit);
		bool canChangeCover = corner && Physics.Raycast(ray.origin + 5f * shift, ray.direction, out hit, 0.7f, layerMask);
		canChangeCover = canChangeCover && behaviourManager.IsHorizontalMoving() && NotSameCover(hit);
		Debug.DrawRay(ray.origin + 5f * shift, 0.7f * ray.direction, canChangeCover ? Color.green : Color.red);
		return canChangeCover;
	}

	// Check if can turn a cover corner.
	private bool CanTurnCover(Ray ray, out RaycastHit hit)
	{
		hit = default(RaycastHit);
		return (corner && behaviourManager.IsHorizontalMoving() && !possibleCover);
	}

	// Re-orientate the player.
	void Rotating(Vector3 direction)
	{
		Quaternion targetRotation = Quaternion.LookRotation (direction);

		Quaternion newRotation = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, targetRotation, orientationSmooth);
		behaviourManager.GetRigidBody.MoveRotation (newRotation);

		behaviourManager.SetLastDirection(direction);
	}

	// Get where the camera is facing.
	private Vector3 GetCamTarget()
	{
		Vector3 forward = behaviourManager.playerCamera.forward;
		forward.y = 0.0f;
		return forward.normalized;
	}

	// The current player behaviour and other parameters allow to change to cover?
	private bool CanTakeCover()
	{
		return behaviourManager.IsGrounded() &&
			!behaviourManager.GetTempLockStatus(this.behaviourCode) && !behaviourManager.IsOverriding() &&
			(behaviourManager.IsCurrentBehaviour(this.behaviourCode) || behaviourManager.IsCurrentBehaviour(behaviourManager.GetDefaultBehaviour));

	}

	// Check if exists a destination where the player can navigate to take cover.
	private bool HasPossibleCover(Vector3 rayTarget, out RaycastHit hit)
	{
		bool hasCover = Physics.Raycast(transform.position + 2 * shift, rayTarget, out hit, searchForCoverDist) &&
			hit.collider.gameObject.layer == LayerMask.NameToLayer("Cover") &&
		(InPLayerFOV(rayTarget) || projectedCoverWall.distance < coverDist || takeCover) &&
		NotSameCover(hit) && behaviourManager.GetAnim.speed != 0 &&
		currentAction != CoverActions.TURN;

		// Get the cover point on the ground (consider cover up to 3 units height).
		if (hasCover)
		{
			RaycastHit groundHit;
			if (Physics.Raycast(hit.point, Vector3.down, out groundHit, 3.0f))
				hit.point = groundHit.point;
			else
				hasCover = false;
		}
		return hasCover;
	}

	// Check if cast target is the current player cover.
	private bool NotSameCover(RaycastHit hit)
	{
		return hit.collider != null && hit.collider.gameObject.GetInstanceID() != currentCoverID;
	}

	// Check if the target position is within player FOV.
	private bool InPLayerFOV(Vector3 rayTarget, bool reverse = false)
	{
		return InFOV(transform.forward, rayTarget, reverse);
	}

	// Check if the target position is within a custom FOV direction, passed as a parameter.
	private bool InFOV(Vector3 direction, Vector3 rayTarget, bool reverse = false)
	{
		if (reverse)
			direction = -direction;
		return Vector3.Angle(direction, rayTarget) <= horizontalClampAngle;
	}

	// LocalLateUpdate: manage post animation step corrections.
	public override void LocalLateUpdate()
	{
		// Adjust player position when stand up covering to approximate the cover wall.
		if (!isCrouching)
		{
			Transform hips = behaviourManager.GetAnim.GetBoneTransform(HumanBodyBones.Hips);
			hips.position += 0.09f * hips.up;
		}
	}
}
