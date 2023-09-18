using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatStage : GUIWindow
{
	public CheatStage()
	{
		name = "Stage";
	}

	protected override void OnDraw()
	{
		if (GUILayout.Button("Quick Start"))
		{

		}

		if (GUILayout.Button("Win"))
		{

		}

		if (GUILayout.Button("Lose"))
		{

		}
	}
}
