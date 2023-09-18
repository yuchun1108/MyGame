using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class GUIWindow
{
	enum RectChangeReason
	{
		none,
		screenSizeChange,
		userResize,
	}

	private const string WINDOW_POS_ = "WINDOW_POS_";
	private const int minRectW = 100;
	private const int minRectH = 50;
	protected static GUIStyle topBtnStyle = GUIStyle.none;
	protected static GUIStyle windowStyle = GUIStyle.none;
	protected static GUIStyle toggleStyle = GUIStyle.none;
	protected static GUIStyle foldOutStyle = GUIStyle.none;

	private int id;
	public string name;
	public bool hasCloseBtn = true;
	public GUIWindowDefaultValues defaultValues;
	public Action onDrawAction;

	protected Rect rect;

	private bool isResizing => resizeCorner.x != 0 && resizeCorner.y != 0;
	private Vector2Int resizeCorner;
	private Vector2 resizeAnchor;
	private Vector2 resizeOffset;

	private Vector2 anchor;
	private Vector2 posOffset;

	private Vector2 lastScreenSize;

	public bool isShow;

	private RectChangeReason rectChangeReason;

	private bool lastIsShow;
	private Vector2 lastAnchor;
	private Vector2 lastPosOffset;
	private Vector2 lastRectPos;
	private Vector2 lastRectSize;
	private bool lastIsMousePressed;

	public void Setup()
	{
		id = name.GetHashCode();

		rect.width = minRectW;
		rect.height = minRectH;

		ResetToDefault();
		LoadPos();
		RefreshRectPos();

		lastIsShow = isShow;
		lastAnchor = anchor;
		lastPosOffset = posOffset;
		lastRectSize.Set(rect.width, rect.height);

		lastScreenSize.Set(Screen.width, Screen.height);
		lastRectPos.Set(rect.x, rect.y);

		if (isShow) OnOpen();
	}

	public void BaseUpdate()
	{
		if (lastIsShow != isShow)
		{
			if (isShow) OnOpen();
			else OnClose();
		}

		if (lastIsShow != isShow ||
			lastAnchor != anchor ||
			lastPosOffset != posOffset ||
			lastRectSize.x != rect.width ||
			lastRectSize.y != rect.height)
		{
			SavePos();
		}

		lastIsShow = isShow;
		lastAnchor = anchor;
		lastPosOffset = posOffset;
		lastRectSize.Set(rect.width, rect.height);

		Update();
	}

	protected virtual void Update() { }

	protected virtual void OnOpen() { }

	protected virtual void OnClose() { }

	private void SetupStyle()
	{
		if (foldOutStyle == GUIStyle.none)
		{
			foldOutStyle = new GUIStyle(GUI.skin.toggle);
			foldOutStyle.normal.background = CreateFoldOutArrow(false, foldOutStyle.normal.textColor);
			foldOutStyle.hover.background = CreateFoldOutArrow(false, foldOutStyle.hover.textColor);
			foldOutStyle.active.background = CreateFoldOutArrow(false, foldOutStyle.active.textColor);
			foldOutStyle.onNormal.background = CreateFoldOutArrow(true, foldOutStyle.onNormal.textColor);
			foldOutStyle.onHover.background = CreateFoldOutArrow(true, foldOutStyle.onHover.textColor);
			foldOutStyle.onActive.background = CreateFoldOutArrow(false, foldOutStyle.onActive.textColor);
		}

		if (toggleStyle == GUIStyle.none)
		{
			toggleStyle = new GUIStyle(GUI.skin.toggle);
			toggleStyle.normal.background = null;
			toggleStyle.hover.background = null;
			toggleStyle.active.background = null;
			toggleStyle.onNormal.background = null;
			toggleStyle.onHover.background = null;
			toggleStyle.onActive.background = null;
		}

		if (windowStyle == GUIStyle.none)
		{
			windowStyle = new GUIStyle(GUI.skin.window);
			windowStyle.alignment = TextAnchor.UpperLeft;
		}

		if (topBtnStyle == GUIStyle.none)
		{
			topBtnStyle = new GUIStyle(GUI.skin.button);
			topBtnStyle.padding = new RectOffset();
		}
	}

	private static Texture2D CreateFoldOutArrow(bool isExpand, Color color)
	{
		const int s = 13;
		const int h = 6;
		var tex = new Texture2D(s, s);
		for (int y = 0; y < s; y++)
		{
			for (int x = 0; x < s; x++)
			{
				bool b;
				if (isExpand)
				{
					int _x = h - Mathf.Abs(h - x);
					b = y < s - 1 && _x > 0 && y > s - _x * 2;
				}
				else
				{
					int _y = h - Mathf.Abs(h - y);
					b = x > 1 && _y > 0 && x < _y * 2;
				}

				Color c = color;
				c.a = b ? 1 : 0;
				tex.SetPixel(x, y, c);
			}
		}

		tex.Apply();
		tex.wrapMode = TextureWrapMode.Clamp;
		return tex;
	}

	public void Draw()
	{
		SetupStyle();

		if (!isShow) return;

		CheckDragEnd();

		if (lastScreenSize.x != Screen.width ||
			lastScreenSize.y != Screen.height)
		{
			RefreshRectPos();

			rectChangeReason = RectChangeReason.screenSizeChange;
		}
		lastScreenSize.Set(Screen.width, Screen.height);

		rect = GUI.Window(id, rect, OnDrawWindow, name, windowStyle);

		Vector2 screenAnchorPos = new Vector2(Screen.width * anchor.x, Screen.height * anchor.y);
		Vector2 rectAnchorPos = new Vector2(rect.x + rect.width * anchor.x, rect.y + rect.height * anchor.y);
		posOffset = rectAnchorPos - screenAnchorPos;
	}

	private void CheckDragEnd()
	{
		bool isMousePressed = Input.GetMouseButton(0);

		if (isMousePressed != lastIsMousePressed)
		{
			if (!isMousePressed)
			{
				if (rectChangeReason == RectChangeReason.none)
				{
					if (rect.x != lastRectPos.x || rect.y != lastRectPos.y)
					{
						OnDragEnd();
					}
				}
				lastRectPos.Set(rect.x, rect.y);

				rectChangeReason = RectChangeReason.none;
			}
		}
		lastIsMousePressed = isMousePressed;
	}

	private void OnDragEnd()
	{
		int x = 0;
		int y = 0;

		if (rect.x < 0)
		{
			rect.x = 0;
			x = -1;
		}
		else if (rect.x + rect.width > Screen.width)
		{
			rect.x = Screen.width - rect.width;
			x = 1;
		}

		if (rect.y < 0)
		{
			rect.y = 0;
			y = -1;
		}
		else if (rect.y + rect.height > Screen.height)
		{
			rect.y = Screen.height - rect.height;
			y = 1;
		}

		if (x != 0 || y != 0)
		{
			if (x < 0) anchor.x = 0;
			else if (x > 0) anchor.x = 1;
			else anchor.x = 0.5f;

			if (y < 0) anchor.y = 0;
			else if (y > 0) anchor.y = 1;
			else anchor.y = 0.5f;
		}
	}

	private void OnDrawWindow(int windowID)
	{
		const int topBtnW = 22;
		const int topBtnH = 19;
		const int topBtnMarginTop = -3;
		const int topBtnMarginRight = 8;
		const int topBtnSpace = 2;

		int marginRight = topBtnMarginRight;

		if (hasCloseBtn)
		{
			if (GUI.Button(new Rect(rect.width - (topBtnW) - marginRight, topBtnMarginTop, topBtnW, topBtnH), "×", topBtnStyle))
			{
				isShow = false;
			}
			marginRight += topBtnW + topBtnSpace;
		}

		if (GUI.Button(new Rect(rect.width - (topBtnW) - marginRight, topBtnMarginTop, topBtnW, topBtnH), AnchorToChar(anchor).ToString(), topBtnStyle))
		{
			anchor.Set(0.5f, 0.5f);
		}

		onDrawAction?.Invoke();
		OnDraw();

		Resize();

		if (!isResizing)
		{
			GUI.DragWindow(new Rect(0, 0, 10000, 20));
		}
	}

	protected virtual void OnDraw()
	{

	}

	private void Resize()
	{
		Vector2 mouseOnScreen = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		Vector2 mouseOnGUI = GUIUtility.ScreenToGUIPoint(mouseOnScreen);

		const int resizeAreaW = 12;
		const int resizeAreaH = 12;

		if (Event.current.type == EventType.MouseDown)
		{
			resizeCorner.Set(0, 0);

			if (mouseOnGUI.x < resizeAreaW)
			{
				resizeCorner.x = -1;
				resizeOffset.x = mouseOnGUI.x;
				resizeAnchor.x = rect.x + rect.width;
			}
			else if (rect.width - mouseOnGUI.x < resizeAreaW)
			{
				resizeCorner.x = 1;
				resizeOffset.x = rect.width - mouseOnGUI.x;
				resizeAnchor.x = rect.x;
			}
			else
			{
				resizeCorner.x = 0;
			}

			if (mouseOnGUI.y < resizeAreaH)
			{
				resizeCorner.y = -1;
				resizeOffset.y = mouseOnGUI.y;
				resizeAnchor.y = rect.y + rect.height;
			}
			else if (rect.height - mouseOnGUI.y < resizeAreaH)
			{
				resizeCorner.y = 1;
				resizeOffset.y = rect.height - mouseOnGUI.y;
				resizeAnchor.y = rect.y;
			}
			else
			{
				resizeCorner.y = 0;
			}
		}

		if (Event.current.type == EventType.MouseUp || !Input.GetMouseButton(0))
		{
			resizeCorner.Set(0, 0);
		}

		if (isResizing)
		{
			if (resizeCorner.x < 0)
			{
				rect.x = Mathf.Min(resizeAnchor.x - minRectW, mouseOnScreen.x - resizeOffset.x);
				rect.width = Mathf.Max(minRectW, resizeAnchor.x - rect.x);
			}
			else if (resizeCorner.x > 0)
			{
				rect.width = Mathf.Max(minRectW, mouseOnGUI.x + resizeOffset.x);
			}

			if (resizeCorner.y < 0)
			{
				rect.y = Mathf.Min(resizeAnchor.y - minRectH, mouseOnScreen.y - resizeOffset.y);
				rect.height = Mathf.Max(minRectH, resizeAnchor.y - rect.y);
			}
			else if (resizeCorner.y > 0)
			{
				rect.height = Mathf.Max(minRectH, mouseOnGUI.y + resizeOffset.y);
			}

			rectChangeReason = RectChangeReason.userResize;
		}
	}

	public void ResetToDefault()
	{
		GUIWindowDefaultValues _defaultValues = defaultValues;
		if (_defaultValues == null) _defaultValues = new GUIWindowDefaultValues();

		isShow = _defaultValues.isShow;
		anchor = _defaultValues.anchor;
		posOffset = _defaultValues.posOffset;
		rect.width = _defaultValues.size.x;
		rect.height = _defaultValues.size.y;
	}


	private void SavePos()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(isShow ? '1' : '0');
		sb.Append(',');
		sb.Append(AnchorToChar(anchor));
		sb.Append(',');
		sb.Append(posOffset.x);
		sb.Append(',');
		sb.Append(posOffset.y);
		sb.Append(',');
		sb.Append(rect.width);
		sb.Append(',');
		sb.Append(rect.height);

		PlayerPrefs.SetString(WINDOW_POS_ + name, sb.ToString());
	}

	private void LoadPos()
	{
		string content = PlayerPrefs.GetString(WINDOW_POS_ + name);
		if (string.IsNullOrEmpty(content)) return;

		string[] args = content.Split(',');
		if (args.Length != 6) return;

		isShow = char.Parse(args[0]) == '1';

		anchor = CharToAnchor(char.Parse(args[1]));

		posOffset.x = float.Parse(args[2]);
		posOffset.y = float.Parse(args[3]);

		rect.width = float.Parse(args[4]);
		rect.height = float.Parse(args[5]);
	}

	public void RefreshRectPos()
	{
		Vector2 screenAnchorPos = new Vector2(Screen.width * anchor.x, Screen.height * anchor.y);
		Vector2 rectAnchorPos = screenAnchorPos + posOffset;
		rect.x = rectAnchorPos.x - rect.width * anchor.x;
		rect.y = rectAnchorPos.y - rect.height * anchor.y;
	}

	private char AnchorToChar(Vector2 anchor)
	{
		if (anchor.x <= 0 && anchor.y <= 0) return '↖';
		else if (anchor.x <= 0 && anchor.y >= 1) return '↙';
		else if (anchor.x >= 1 && anchor.y >= 1) return '↘';
		else if (anchor.x >= 1 && anchor.y <= 0) return '↗';
		else if (anchor.x <= 0) return '←';
		else if (anchor.x >= 1) return '→';
		else if (anchor.y <= 0) return '↑';
		else if (anchor.y >= 1) return '↓';
		else return '⊙';
	}

	private Vector2 CharToAnchor(char c)
	{
		switch (c)
		{
			case '↖': return new Vector2(0, 0);
			case '↙': return new Vector2(0, 1);
			case '↘': return new Vector2(1, 1);
			case '↗': return new Vector2(1, 0);
			case '←': return new Vector2(0, 0.5f);
			case '→': return new Vector2(1, 0.5f);
			case '↑': return new Vector2(0.5f, 0);
			case '↓': return new Vector2(0.5f, 1);
			default: return new Vector2(0.5f, 0.5f);
		}
	}

	public virtual void OnApplicationQuit()
	{

	}

	#region 常用方法

	protected void LabelValue(string label, object value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label);
		GUILayout.FlexibleSpace();
		GUILayout.Label(value.ToString());
		GUILayout.EndHorizontal();
	}

	#endregion
}

public class GUIWindowDefaultValues
{
	public bool isShow = true;
	public Vector2 size = new Vector2(100, 100);
	public Vector2 anchor = new Vector2(0.5f, 0.5f);
	public Vector3 posOffset = Vector2.zero;
}