using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
	public InputActionAsset inputAction;

	private void Awake()
	{
		Cheat.Setup();

		inputAction.Enable();
	}

	private void Update()
	{
		Cheat.Watch("mouse", Input.mousePosition);
	}
}
