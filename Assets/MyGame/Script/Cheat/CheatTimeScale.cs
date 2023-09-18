using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatTimeScale : GUIWindow
{

	public CheatTimeScale()
	{
		name = "TimeScale";
		defaultValues = new GUIWindowDefaultValues()
		{
			size = new Vector2(400, 400)
		};
	}

	private bool ctrl;
	private bool dot;

	protected override void Update()
	{
		ctrl = Input.GetKey(KeyCode.RightControl);
		dot = Input.GetKey(KeyCode.KeypadPeriod);

		if (Input.GetKey(KeyCode.RightControl))
		{

		}
	}

	protected override void OnDraw()
	{
		LabelValue("TimeScale", Time.timeScale);
		LabelValue("ctrl", ctrl);
		LabelValue("dot", dot);
	}
}
