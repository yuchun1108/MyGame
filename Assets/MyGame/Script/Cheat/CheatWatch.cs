using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatWatch : GUIWindow
{
	private readonly Dictionary<string, object> watchDic;
	private Vector2 scrollPos;

	public CheatWatch(Dictionary<string, object> watchDic)
	{
		name = "Watch";
		defaultValues = new GUIWindowDefaultValues()
		{
			size = new Vector2(400, 400)
		};

		this.watchDic = watchDic;
	}

	protected override void OnDraw()
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		foreach (KeyValuePair<string, object> pair in watchDic)
		{
			LabelValue(pair.Key, pair.Value);
		}
		GUILayout.EndScrollView();
	}
}
