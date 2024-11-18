using System;
using Components;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[SerializeField] private GameObject _getInCarButton;
			
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		private void OnTriggerEnter(Collider other)
		{
			if(other.GetComponent<TriggerTransportDoor>() == null) return;
			_getInCarButton.SetActive(true);
		}
		private void OnTriggerExit(Collider other)
		{
			if(other.GetComponent<TriggerTransportDoor>() == null) return;
			_getInCarButton.SetActive(false);
		}

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value) => MoveInput(new Vector2(value.Get<Vector2>().y, value.Get<Vector2>().x));
		public void OnJump(InputValue value) => JumpInput(value.isPressed);
		public void OnSprint(InputValue value) => SprintInput(value.isPressed);
		public void OnLook(InputValue value)
		{
			if(cursorInputForLook) LookInput(value.Get<Vector2>());
		}
#endif

		public void MoveInput(Vector2 newMoveDirection) => move = newMoveDirection;
		public void LookInput(Vector2 newLookDirection) => look = newLookDirection;
		public void JumpInput(bool newJumpState) => jump = newJumpState;
		public void SprintInput(bool newSprintState) => sprint = newSprintState;
		private void SetCursorState(bool newState) => Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		
		private void OnApplicationFocus(bool hasFocus) => SetCursorState(cursorLocked);
	}
}