using UnityEngine;

// To switch your project to using the new InputSystem.
// Edit>Project Settings>Player>Active Input Handling change to "Input System Package (New)".
using UnityEngine.InputSystem;
using RPGCharacterAnims.Actions;

namespace RPGCharacterAnims
{
	[HelpURL("https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html")]

	public class RPGCharacterInputSystemController : MonoBehaviour
    {
        RPGCharacterController rpgCharacterController;

		//InputSystem
		public @RPGInputs rpgInputs;

		// Inputs.
		private bool inputJump;
        private bool inputLightHit;
        private bool inputDeath;
        private bool inputAttackL;
        private bool inputAttackR;
        private bool inputCastL;
        private bool inputCastR;
		private bool inputBlock;
		private bool inputRoll;
		private bool inputShield;
		private bool inputRelax;
		private bool inputAim;
		private Vector2 inputMovement;
		private bool inputFace;
		private Vector2 inputFacing;
		private bool inputSwitchUp;
		private bool inputSwitchDown;
		private bool inputSwitchLeft;
		private bool inputSwitchRight;

		// Variables.
		private Vector3 moveInput;
        private bool isJumpHeld;
		private Vector3 currentAim;
		private float bowPull;
		private bool blockToggle;
		private float inputPauseTimeout = 0;
		private bool inputPaused = false;

		private void Awake()
        {
            rpgCharacterController = GetComponent<RPGCharacterController>();
			rpgInputs = new @RPGInputs();
			currentAim = Vector3.zero;
        }

		private void OnEnable()
		{
			rpgInputs.Enable();
		}

		private void OnDisable()
		{
			rpgInputs.Disable();
		}

		private void Update()
		{
			Inputs();
			Blocking();
			Moving();
			Damage();
			SwitchWeapons();

			if (!rpgCharacterController.IsActive("Relax")) {
				Strafing();
				Facing();
				Aiming();
				Rolling();
				Attacking();
			}
		}

		/// <summary>
		/// Pause input for a number of seconds.
		/// </summary>
		/// <param name="timeout">The amount of time in seconds to ignore input</param>
		public void PauseInput(float timeout)
		{
			inputPaused = true;
			inputPauseTimeout = Time.time + timeout;
		}

		/// <summary>
		/// Input abstraction for easier asset updates using outside control schemes.
		/// </summary>
		private void Inputs()
        {
            try {
				inputAttackL = rpgInputs.RPGCharacter.AttackL.WasPressedThisFrame();
				inputAttackR = rpgInputs.RPGCharacter.AttackR.WasPressedThisFrame();
				inputBlock = rpgInputs.RPGCharacter.Block.IsPressed();
				inputCastL = rpgInputs.RPGCharacter.CastL.WasPressedThisFrame();
				inputCastR = rpgInputs.RPGCharacter.CastR.WasPressedThisFrame();
				inputDeath = rpgInputs.RPGCharacter.Death.WasPressedThisFrame();
				inputFace = rpgInputs.RPGCharacter.Face.IsPressed();
				inputFacing = rpgInputs.RPGCharacter.Facing.ReadValue<Vector2>();
				inputJump = rpgInputs.RPGCharacter.Jump.WasPressedThisFrame();
				inputLightHit = rpgInputs.RPGCharacter.LightHit.WasPressedThisFrame();
				inputMovement = rpgInputs.RPGCharacter.Move.ReadValue<Vector2>();
				inputRelax = rpgInputs.RPGCharacter.Relax.WasPressedThisFrame();
				inputRoll = rpgInputs.RPGCharacter.Roll.WasPressedThisFrame();
				inputShield = rpgInputs.RPGCharacter.Shield.WasPressedThisFrame();
				inputAim = rpgInputs.RPGCharacter.Aim.IsPressed();
				inputSwitchDown = rpgInputs.RPGCharacter.WeaponDown.WasPressedThisFrame();
				inputSwitchLeft = rpgInputs.RPGCharacter.WeaponLeft.WasPressedThisFrame();
				inputSwitchRight = rpgInputs.RPGCharacter.WeaponRight.WasPressedThisFrame();
				inputSwitchUp = rpgInputs.RPGCharacter.WeaponUp.WasPressedThisFrame();

				// Injury toggle.
				if (Keyboard.current.iKey.wasPressedThisFrame) {
                    if (rpgCharacterController.CanStartAction("Injure")) {
                        rpgCharacterController.StartAction("Injure");
                    } else if (rpgCharacterController.CanEndAction("Injure")) {
                        rpgCharacterController.EndAction("Injure");
                    }
                }
                // Headlook toggle.
                if (Keyboard.current.lKey.wasPressedThisFrame) {
                    rpgCharacterController.ToggleHeadlook();
                }
                // Slow time toggle.
                if (Keyboard.current.tKey.wasPressedThisFrame) {
                    if (rpgCharacterController.CanStartAction("SlowTime")) {
                        rpgCharacterController.StartAction("SlowTime", 0.125f);
                    } else if (rpgCharacterController.CanEndAction("SlowTime")) {
                        rpgCharacterController.EndAction("SlowTime");
                    }
                }
                // Pause toggle.
                if (Keyboard.current.pKey.wasPressedThisFrame) {
                    if (rpgCharacterController.CanStartAction("SlowTime")) {
                        rpgCharacterController.StartAction("SlowTime", 0f);
                    } else if (rpgCharacterController.CanEndAction("SlowTime")) {
                        rpgCharacterController.EndAction("SlowTime");
                    }
                }
            } catch (System.Exception) { Debug.LogError("Inputs not found!  Character must have Player Input component."); }
        }

        public bool HasMoveInput()
        {
            return moveInput != Vector3.zero;
        }

		public bool HasAimInput()
		{
			return inputAim;
		}

		public bool HasFacingInput()
		{
			return (inputFacing != Vector2.zero || inputFace);
		}

        public bool HasBlockInput()
        {
            return inputBlock;
        }

		public void Blocking()
        {
            bool blocking = HasBlockInput();
            if (blocking && rpgCharacterController.CanStartAction("Block")) {
                rpgCharacterController.StartAction("Block");
				blockToggle = true;
            } else if (!blocking && blockToggle && rpgCharacterController.CanEndAction("Block")) {
                rpgCharacterController.EndAction("Block");
				blockToggle = false;
            }
        }

        public void Moving()
        {
            moveInput = new Vector3(inputMovement.x, inputMovement.y, 0f);
            rpgCharacterController.SetMoveInput(moveInput);

            // Set the input on the jump axis every frame.
            Vector3 jumpInput = isJumpHeld ? Vector3.up : Vector3.zero;
            rpgCharacterController.SetJumpInput(jumpInput);

            // If we pressed jump button this frame, jump.
            if (inputJump && rpgCharacterController.CanStartAction("Jump")) {
                rpgCharacterController.StartAction("Jump");
            } else if (inputJump && rpgCharacterController.CanStartAction("DoubleJump")) {
                rpgCharacterController.StartAction("DoubleJump");
            }
        }

		public void Rolling()
		{
			if (!inputRoll) { return; }
			if (!rpgCharacterController.CanStartAction("DiveRoll")) { return; }

			rpgCharacterController.StartAction("DiveRoll", 1);
		}

		private void Aiming()
		{
			if (rpgCharacterController.hasAimedWeapon) {
				if (HasAimInput()) {
					if (rpgCharacterController.CanStartAction("Aim")) { rpgCharacterController.StartAction("Aim"); }
				} else {
					if (rpgCharacterController.CanEndAction("Aim")) { rpgCharacterController.EndAction("Aim"); }
				}

				if (rpgCharacterController.rightWeapon == ( int )Weapon.TwoHandBow) {

					// If using the bow, we want to pull back slowly on the bow string while the
					// Left Mouse button is down, and shoot when it is released.
					if (Input.GetMouseButton(0)) {
						bowPull += 0.05f;
					} else if (Input.GetMouseButtonUp(0)) {
						if (rpgCharacterController.CanStartAction("Shoot")) { rpgCharacterController.StartAction("Shoot"); }
					} else {
						bowPull = 0f;
					}
					bowPull = Mathf.Clamp(bowPull, 0f, 1f);
				} else {
					// If using a gun or a crossbow, we want to fire when the left mouse button is pressed.
					if (Input.GetMouseButtonDown(0)) {
						if (rpgCharacterController.CanStartAction("Shoot")) { rpgCharacterController.StartAction("Shoot"); }
					}
				}
				// Reload.
				if (Input.GetMouseButtonDown(2)) {
					if (rpgCharacterController.CanStartAction("Reload")) { rpgCharacterController.StartAction("Reload"); }
				}
				// Finally, set aim location and bow pull.
				rpgCharacterController.SetAimInput(rpgCharacterController.target.position);
				rpgCharacterController.SetBowPull(bowPull);
			} else {
				Strafing();
			}
		}

		private void Strafing()
		{
			if (rpgCharacterController.canStrafe) {
				if (!rpgCharacterController.hasAimedWeapon) {
					if (inputAim) {
						if (rpgCharacterController.CanStartAction("Strafe")) { rpgCharacterController.StartAction("Strafe"); }
					} else {
						if (rpgCharacterController.CanEndAction("Strafe")) { rpgCharacterController.EndAction("Strafe"); }
					}
				}
			}
		}

		private void Facing()
		{
			if (rpgCharacterController.canFace) {
				if (HasFacingInput()) {
					if (inputFace) {
						// Get world position from mouse position on screen and convert to direction from character.
						Plane playerPlane = new Plane(Vector3.up, transform.position);
						Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
						float hitdist = 0.0f;
						if (playerPlane.Raycast(ray, out hitdist)) {
							Vector3 targetPoint = ray.GetPoint(hitdist);
							Vector3 lookTarget = new Vector3(targetPoint.x - transform.position.x, transform.position.z - targetPoint.z, 0);
							rpgCharacterController.SetFaceInput(lookTarget);
						}
					} else {
						rpgCharacterController.SetFaceInput(new Vector3(inputFacing.x, inputFacing.y, 0));
					}
					if (rpgCharacterController.CanStartAction("Face")) { rpgCharacterController.StartAction("Face"); }
				} else {
					if (rpgCharacterController.CanEndAction("Face")) { rpgCharacterController.EndAction("Face"); }
				}
			}
		}

		private void Attacking()
		{
			if ((inputCastL || inputCastR) && rpgCharacterController.IsActive("Cast")) { rpgCharacterController.EndAction("Cast"); }
			if (!rpgCharacterController.CanStartAction("Attack")) { return; }
			if (inputAttackL) {
				rpgCharacterController.StartAction("Attack", new Actions.AttackContext("Attack", "Left"));
			} else if (inputAttackR) {
				rpgCharacterController.StartAction("Attack", new Actions.AttackContext("Attack", "Right"));
			} else if (inputCastL) {
				rpgCharacterController.StartAction("Cast", new Actions.CastContext("Cast", "Left"));
			} else if (inputCastR) {
				rpgCharacterController.StartAction("Cast", new Actions.CastContext("Cast", "Right"));
			}
		}

		private void Damage()
		{
			// Hit.
			if (inputLightHit) { rpgCharacterController.StartAction("GetHit", new HitContext()); }

			// Death.
			if (inputDeath) {
				if (rpgCharacterController.CanStartAction("Death")) {
					rpgCharacterController.StartAction("Death");
				} else if (rpgCharacterController.CanEndAction("Death")) {
					rpgCharacterController.EndAction("Death");
				}
			}
		}

		/// <summary>
		/// Cycle weapons using directional pad input. Up and Down cycle forward and backward through
		/// the list of two handed weapons. Left cycles through the left hand weapons. Right cycles through
		/// the right hand weapons.
		/// </summary>
		private void SwitchWeapons()
        {
			// Bail out if we can't switch weapons.
			if (!rpgCharacterController.CanStartAction("SwitchWeapon")) { return; }

			// Switch to Relaxed state.
			if (inputRelax) {
				rpgCharacterController.StartAction("Relax");
				return;
			}

			bool doSwitch = false;
			SwitchWeaponContext context = new SwitchWeaponContext();
			int weaponNumber = 0;

			// Switch to Shield.
			if (inputShield) {
				doSwitch = true;
				context.side = "Left";
				context.type = "Switch";
				context.leftWeapon = 7;
				context.rightWeapon = weaponNumber;
				rpgCharacterController.StartAction("SwitchWeapon", context);
				return;
			}

			// Cycle through 2H weapons any input happens on the up-down axis.
			if (inputSwitchUp || inputSwitchDown) {
                int[] twoHandedWeapons = new int[] {
                    (int) Weapon.TwoHandSword,
                    (int) Weapon.TwoHandSpear,
                    (int) Weapon.TwoHandAxe,
                    (int) Weapon.TwoHandBow,
                    (int) Weapon.TwoHandCrossbow,
                    (int) Weapon.TwoHandStaff,
                    (int) Weapon.Rifle,
                };

                // If we're not wielding 2H weapon already, just switch to the first one in the list.
                if (System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon) == -1) {
                    weaponNumber = twoHandedWeapons[0];
                }
                // Otherwise, we should loop through them.
                else {
                    int index = System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon);
                    if (inputSwitchUp) {
                        index = (index - 1 + twoHandedWeapons.Length) % twoHandedWeapons.Length;
                    } else if (inputSwitchDown) {
                        index = (index + 1) % twoHandedWeapons.Length;
                    }
                    weaponNumber = twoHandedWeapons[index];
                }

                // Set up the context and flag that we actually want to perform the switch.
                doSwitch = true;
                context.type = "Switch";
                context.side = "None";
                context.leftWeapon = -1;
                context.rightWeapon = weaponNumber;
            }

            // Cycle through 1H weapons if any input happens on the left-right axis.
            if (inputSwitchLeft || inputSwitchRight) {
                doSwitch = true;
                context.type = "Switch";

                // Left-handed weapons.
                if (inputSwitchLeft) {
                    int[] leftWeapons = new int[] {
                        (int) Weapon.Unarmed,
                        (int) Weapon.Shield,
                        (int) Weapon.LeftSword,
                        (int) Weapon.LeftMace,
                        (int) Weapon.LeftDagger,
                        (int) Weapon.LeftItem,
                        (int) Weapon.LeftPistol,
                    };

                    // If we are not wielding a left-handed weapon, switch to unarmed.
                    if (System.Array.IndexOf(leftWeapons, rpgCharacterController.leftWeapon) == -1) { weaponNumber = leftWeapons[0]; }

                    // Otherwise, cycle through the list.
                    else {
                        int currentIndex = System.Array.IndexOf(leftWeapons, rpgCharacterController.leftWeapon);
                        weaponNumber = leftWeapons[(currentIndex + 1) % leftWeapons.Length];
                    }

                    context.side = "Left";
                    context.leftWeapon = weaponNumber;
                    context.rightWeapon = -1;
                }
                // Right-handed weapons.
                else if (inputSwitchRight) {
                    int[] rightWeapons = new int[] {
                        (int) Weapon.Unarmed,
                        (int) Weapon.RightSword,
                        (int) Weapon.RightMace,
                        (int) Weapon.RightDagger,
                        (int) Weapon.RightItem,
                        (int) Weapon.RightPistol,
                        (int) Weapon.RightSpear,
                    };
                    // If we are not wielding a right-handed weapon, switch to unarmed.
                    if (System.Array.IndexOf(rightWeapons, rpgCharacterController.rightWeapon) == -1) {
                        weaponNumber = rightWeapons[0];
                    }
                    // Otherwise, cycle through the list.
                    else {
                        int currentIndex = System.Array.IndexOf(rightWeapons, rpgCharacterController.rightWeapon);
                        weaponNumber = rightWeapons[(currentIndex + 1) % rightWeapons.Length];
                    }

                    context.side = "Right";
                    context.leftWeapon = -1;
                    context.rightWeapon = weaponNumber;
                }
            }
            // If we've received input, then "doSwitch" is true, and the context is filled out,
            // so start the SwitchWeapon action.
            if (doSwitch) { rpgCharacterController.StartAction("SwitchWeapon", context); }
        }
    }

	/// <summary>
	/// Extension Method to allow checking InputSystem without Action Callbacks.
	/// </summary>
	public static class InputActionExtensions
	{
		public static bool IsPressed(this InputAction inputAction)
		{
			return inputAction.ReadValue<float>() > 0f;
		}

		public static bool WasPressedThisFrame(this InputAction inputAction)
		{
			return inputAction.triggered && inputAction.ReadValue<float>() > 0f;
		}

		public static bool WasReleasedThisFrame(this InputAction inputAction)
		{
			return inputAction.triggered && inputAction.ReadValue<float>() == 0f;
		}
	}
}