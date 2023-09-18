using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.AddressableAssets;
using System;

public class CheatInput : GUIWindow
{

	private InputActionAsset inputAction;
	private InputActionMap inGameActMap;
	private InputActionMap menuActMap;

	private bool actMapInGameExpand = true;

	private ReadOnlyArray<InputAction> inGameActions;
	private ReadOnlyArray<InputAction> menuActions;

	private bool logWasPerformed;
	private InputActionRebindingExtensions.RebindingOperation rebindingOp;

	private readonly string[] ctrlNameDisplayModeStrs = { "name", "displayName", "shortDisplayName", "path" };
	private int ctrlNameDisplayMode;

	public CheatInput()
	{
		name = "Input";

		var ao = Addressables.LoadAssetAsync<InputActionAsset>("InputAction");
		ao.Completed += (ao) =>
		{
			inputAction = ao.Result;


			inGameActMap = inputAction.FindActionMap("InGame");
			menuActMap = inputAction.FindActionMap("Menu");

			inGameActions = inGameActMap.actions;
			menuActions = menuActMap.actions;
		};
	}

	protected override void Update()
	{
		if (logWasPerformed)
		{
			for (int i = 0; i < inGameActions.Count; i++)
			{
				InputAction act = inGameActions[i];
				if (act.WasPerformedThisFrame())
				{
					Debug.Log($"WasPerformedThisFrame: InGame/{act.name}");
				}
			}
		}
	}

	protected override void OnDraw()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Control Name Display Mode");
		ctrlNameDisplayMode = GUILayout.Toolbar(ctrlNameDisplayMode, ctrlNameDisplayModeStrs);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		actMapInGameExpand = GUILayout.Toggle(actMapInGameExpand, "InGame", foldOutStyle);
		GUILayout.FlexibleSpace();
		GUILayout.Label(inGameActMap.enabled ? "enabled" : "disabled");
		GUILayout.EndHorizontal();

		if (actMapInGameExpand)
		{
			for (int i = 0; i < inGameActions.Count; i++)
			{
				InputAction act = inGameActions[i];

				GUILayout.BeginHorizontal();
				GUILayout.Space(20);

				GUILayout.Label(act.name, GUILayout.Width(100));
				string controlType = act.expectedControlType;
				GUILayout.Label(controlType, GUILayout.Width(100));

				if (controlType.Equals("Vector2", StringComparison.Ordinal))
				{
					GUILayout.Label(act.ReadValue<Vector2>().ToString());
				}
				else if (controlType.Equals("Button", StringComparison.Ordinal))
				{
					GUILayout.Label(act.IsPressed().ToString());
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(act.GetBindingDisplayString()))
				{
					act.Disable();

					DisposeRebindingOp();
					rebindingOp = act.PerformInteractiveRebinding();
					rebindingOp.OnComplete((op) =>
					{
						DisposeRebindingOp();
					});
					rebindingOp.OnComplete((op) =>
					{
						DisposeRebindingOp();
					});
					rebindingOp.Start();
				}
				GUILayout.Label(act.enabled ? "enabled" : "disabled");
				GUILayout.EndHorizontal();


				for (int j = 0; j < act.bindings.Count; j++)
				{
					var bind = act.bindings[j];

					GUILayout.BeginHorizontal();
					GUILayout.Label(bind.isComposite.ToString());
					GUILayout.Label(bind.ToDisplayString());
					GUILayout.Label(bind.name);
					GUILayout.EndHorizontal();
				}

				var controls = act.controls;
				for (int j = 0; j < controls.Count; j++)
				{
					var ctrl = controls[j];
					GUILayout.BeginHorizontal();
					GUILayout.Space(40);

					GUILayout.FlexibleSpace();

					string name = string.Empty;

					switch (ctrlNameDisplayMode)
					{
						case 0:
							name = ctrl.name;
							break;
						case 1:
							name = ctrl.displayName;
							break;
						case 2:
							name = ctrl.shortDisplayName;
							break;
						case 3:
							name = ctrl.path;
							break;
					}


					GUILayout.Button(name);
					GUILayout.EndHorizontal();
				}


			}
		}

		logWasPerformed = GUILayout.Toggle(logWasPerformed, "Log WasPerformed", toggleStyle);
		//showPosition = EditorGUILayout.Foldout(showPosition, status);

		if (rebindingOp != null)
		{
			GUILayout.Box($"Rebinding: {rebindingOp.action}");
		}
	}

	private void DisposeRebindingOp()
	{
		if (rebindingOp != null)
		{
			rebindingOp.Dispose();
			rebindingOp = null;
		}
	}

	protected override void OnClose()
	{
		DisposeRebindingOp();
	}

	public override void OnApplicationQuit()
	{
		DisposeRebindingOp();
	}

}
