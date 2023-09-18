using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 偵錯作弊用
/// </summary>
public class Cheat : MonoBehaviour
{
	private static Cheat instance;
	public static void Setup()
	{
		if (instance != null) return;

		var obj = new GameObject("Cheat");
		instance = obj.AddComponent<Cheat>();
		DontDestroyOnLoad(obj);
	}

	private static Dictionary<string, object> watchDic = new Dictionary<string, object>();
	public static void Watch(string key, object value)
	{
		watchDic[key] = value;
	}

	private const float longPressTime = 1f;
	private bool isShow = true;
	private float pressBeginTime;
	private bool hasResetToDefault;

	private GUIWindow menu;

	private List<GUIWindow> windows = new List<GUIWindow>();

	private void Awake()
	{
		menu = new GUIWindow();
		menu.name = "Cheat";
		menu.hasCloseBtn = false;
		menu.defaultValues = new GUIWindowDefaultValues()
		{
			size = new Vector2(600, 50),
			anchor = new Vector2(0.5f, 0)
		};
		menu.onDrawAction += DoMyWindow;
		windows.Add(menu);

		windows.Add(new CheatWatch(watchDic));
		windows.Add(new CheatTimeScale());
		windows.Add(new CheatStage());
		windows.Add(new CheatPlayerPref());
		windows.Add(new CheatInput());

		SetupWindows();
	}

	private void Start()
	{

		PlayerPrefs.SetFloat("test_f", 123.456f);
		PlayerPrefs.SetInt("test_i", 23);
		PlayerPrefs.SetString("test_s", "abc");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			hasResetToDefault = false;
			pressBeginTime = Time.time;
		}

		if (Input.GetKeyUp(KeyCode.F1))
		{
			if (Time.time - pressBeginTime < longPressTime)
			{
				isShow = !isShow;
			}
		}

		if (Input.GetKey(KeyCode.F1))
		{
			if (!hasResetToDefault)
			{
				if (Time.time - pressBeginTime >= longPressTime)
				{
					ResetWindows();
					hasResetToDefault = true;
				}
			}
		}

		UpdateWindows();
	}

	private void SetupWindows()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].Setup();
		}
	}

	private void ResetWindows()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].ResetToDefault();
			windows[i].RefreshRectPos();
		}
	}

	private void UpdateWindows()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].BaseUpdate();
		}
	}

	void OnGUI()
	{
		if (!isShow) return;

		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].Draw();
		}
	}

	void DoMyWindow()
	{
		GUILayout.BeginHorizontal();

		for (int i = 0; i < windows.Count; i++)
		{
			GUIWindow w = windows[i];
			if (w == menu) continue;

			SetHighlight(w.isShow);
			if (GUILayout.Button(w.name))
			{
				w.isShow = !w.isShow;
			}
		}

		ResetHighlight();

		GUILayout.EndHorizontal();
	}

	private void SetHighlight(bool b)
	{
		GUI.contentColor = b ? Color.yellow : Color.white;
	}

	private void ResetHighlight()
	{
		GUI.contentColor = Color.white;
	}

	private void OnApplicationQuit()
	{
		for (int i = 0; i < windows.Count; i++)
		{
			windows[i].OnApplicationQuit();
		}
	}

}
