using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class CheatPlayerPref : GUIWindow
{
	public const string CHEAT_PLAYER_PREF_LIST = "CHEAT_PLAYER_PREF_LIST";
	private static GUIStyle toolbarStyle = GUIStyle.none;

	private enum Type { Int, Float, Str }
	private readonly string[] types = { "i", "f", "s" };


	private List<KeyValuePair> list = new List<KeyValuePair>();
	private Vector2 scrollPos;

	public CheatPlayerPref()
	{
		name = "PlayerPref";
		AddPermanentList();
		LoadList();
	}

	private void AddPermanentList()
	{
		AddPermanentItem(Type.Int, "test_i");
		AddPermanentItem(Type.Float, "test_f");
		AddPermanentItem(Type.Str, "test_s");
	}

	private void AddPermanentItem(Type type, string key)
	{
		KeyValuePair pair = new KeyValuePair();
		pair.type = (int)type;
		pair.key = key;
		pair.isPermanent = true;
		pair.needRefresh = true;
		list.Add(pair);
	}

	protected override void Update()
	{
		bool hasChange = false;

		for (int i = list.Count - 1; i >= 0; i--)
		{
			KeyValuePair pair = list[i];

			if (pair.needRemove)
			{
				list.RemoveAt(i);
				hasChange = true;
				continue;
			}

			if (pair.needClear)
			{
				if (!string.IsNullOrEmpty(pair.key))
				{
					PlayerPrefs.DeleteKey(pair.key);
				}
				pair.needClear = false;
				pair.needRefresh = true;
			}

			if (pair.needRefresh)
			{
				if (string.IsNullOrEmpty(pair.key))
				{
					pair.value = string.Empty;
				}
				else
				{
					switch (pair.type)
					{
						case 0:
							pair.value = PlayerPrefs.GetInt(pair.key).ToString();
							break;
						case 1:
							pair.value = PlayerPrefs.GetFloat(pair.key).ToString();
							break;
						case 2:
							pair.value = PlayerPrefs.GetString(pair.key);
							break;
					}
				}
				pair.needRefresh = false;
			}

			if (pair.hasChange)
			{
				hasChange = true;
				pair.hasChange = false;
			}
		}

		if (hasChange)
		{
			SaveList();
		}
	}

	protected override void OnOpen()
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i].needRefresh = true;
		}
	}

	private void LoadList()
	{
		string content = PlayerPrefs.GetString(CHEAT_PLAYER_PREF_LIST);
		if (string.IsNullOrEmpty(content)) return;

		string[] spArr = content.Split(',');
		for (int i = 0; i < spArr.Length; i++)
		{
			string sp = spArr[i];
			if (string.IsNullOrEmpty(sp)) continue;
			KeyValuePair pair = new KeyValuePair();
			if (int.TryParse(sp.Substring(0, 1), out int type))
			{
				pair.type = type;
			}
			int keyLen = sp.Length - 1;
			if (keyLen <= 0) continue;
			pair.key = sp.Substring(1, keyLen);
			pair.needRefresh = true;

			list.Add(pair);
		}
	}

	private void SaveList()
	{
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < list.Count; i++)
		{
			KeyValuePair pair = list[i];
			if (pair.isPermanent) continue;
			sb.Append(pair.type);
			sb.Append(pair.key);
			if (i < list.Count - 1)
				sb.Append(',');
		}

		PlayerPrefs.SetString(CHEAT_PLAYER_PREF_LIST, sb.ToString());
	}

	protected override void OnDraw()
	{
		if (toolbarStyle == GUIStyle.none)
		{
			toolbarStyle = new GUIStyle(GUI.skin.button);
			toolbarStyle.padding.left = 0;
			toolbarStyle.padding.right = 0;
		}

		scrollPos = GUILayout.BeginScrollView(scrollPos);

		for (int i = 0; i < list.Count; i++)
		{
			GUILayout.BeginHorizontal();

			KeyValuePair pair = list[i];

			float fieldWidth = Mathf.Max(100, (this.rect.width - 266) / 2);



			if (pair.isPermanent)
			{
				GUILayout.Toolbar(pair.type, types, toolbarStyle, GUILayout.Width(60));
				GUILayout.TextField(pair.key, GUILayout.Width(fieldWidth));
			}
			else
			{
				int _type = GUILayout.Toolbar(pair.type, types, toolbarStyle, GUILayout.Width(60));
				if (_type != pair.type)
				{
					pair.type = _type;
					pair.needRefresh = true;
					pair.hasChange = true;
				}

				string _key = GUILayout.TextField(pair.key, GUILayout.Width(fieldWidth));
				if (!_key.Equals(pair.key, StringComparison.Ordinal))
				{
					pair.key = _key;
					pair.needRefresh = true;
					pair.hasChange = true;
				}
			}

			GUILayout.TextField(pair.value, GUILayout.Width(fieldWidth));

			if (GUILayout.Button("Refresh", GUILayout.Width(60)))
			{
				pair.needRefresh = true;
			}

			if (GUILayout.Button("Clear", GUILayout.Width(50)))
			{
				pair.needClear = true;
			}

			if (!pair.isPermanent)
			{
				if (GUILayout.Button("×", GUILayout.Width(30)))
				{
					pair.needRemove = true;
				}
			}

			GUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();



		if (GUILayout.Button("Add"))
		{
			KeyValuePair pair = new KeyValuePair();
			pair.hasChange = true;
			list.Add(pair);
		}

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Refresh All"))
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].needRefresh = true;
			}
		}

		if (GUILayout.Button("Clear All"))
		{
			PlayerPrefs.DeleteAll();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].needRefresh = true;
			}
		}

		GUILayout.EndHorizontal();
	}

	public class KeyValuePair
	{
		public int type;
		public string key = string.Empty;
		public string value = string.Empty;
		public bool needRefresh;
		public bool needClear;
		public bool needRemove;
		public bool hasChange;
		public bool isPermanent;
	}
}
