using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class MyUI
{
	public ButtonGroup buttonCopy;
	public ButtonGroup buttonPaste;
	public ButtonGroup buttonClear;
	public ButtonGroup buttonSave;
	public ButtonGroup buttonStop;
	public ButtonGroup buttonLocal;
	public ButtonGroup buttonClose;
	public ButtonGroup buttonRItem;
	public InputField SaveName;
	public GameObject instance;
	public GameObject Res;
	public ButtonGroup ControlButton;
	public GameObject ControlPanel;
	public RectTransform ControlPanelRect;
	public GameObject MainPanel;
	public Text TaskInfo;
	public RectTransform TaskInfoRect;
	public ButtonGroup[]  ButtonDataFile;
	public ButtonGroup[]  ButtonDataPage;
	public bool isLoad = false;
	public bool isShow = false;
	public Vector3 firstPos1;
	public Vector3 firstPos2;

	public MyUI(GameObject objectResourse)
    {
		Res = objectResourse;
    }

	public void LoadUI(Transform tr)
	{

		instance = GameObject.Instantiate(Res, tr);

		ControlPanel = instance.transform.Find("CopyPlanetPanel").gameObject;
		ControlPanel.SetActive(false);
		//ControlPanel.AddComponent<Drag>();
		ControlPanelRect = ControlPanel.GetComponent<RectTransform>();
		firstPos1 = ControlPanelRect.position;
		ControlPanel.GetComponent<Image>().color = new Color(1, 1, 1, 1);
		var title = ControlPanel.transform.Find("title").GetComponent<Text>();
		title.text = "星球蓝图 " + CopyPlanetFactory.Version;
		title.gameObject.AddComponent<DragParent>();

		MainPanel = instance.transform.Find("MainPanel").gameObject;
		MainPanel.SetActive(false);

		ControlButton = new ButtonGroup(GetButton(instance.transform, "ButtonUIControl"));
		var rect = ControlPanel.transform.GetComponent<RectTransform>();
		ControlButton.button.onClick.AddListener(delegate
		{
			TaskInfo.gameObject.SetActive(!ControlPanel.activeSelf);
			ControlPanel.SetActive(!ControlPanel.activeSelf);
			
		});
		ControlButton.SetActive(true);
		var filePanel = ControlPanel.transform.Find("FilesPanel").gameObject;
		ButtonDataFile = new ButtonGroup[7];
		ButtonDataPage = new ButtonGroup[7];
		for (int i = 1; i < 8; i++)
		{
			ButtonDataFile[i - 1] = new ButtonGroup(GetButton(filePanel.transform, "B" + i));
			ButtonDataPage[i - 1] = new ButtonGroup(GetButton(filePanel.transform, "C" + i));
			ButtonDataFile[i - 1].SetActive(false);
			ButtonDataPage[i - 1].SetActive(false);
			ButtonDataFile[i - 1].text.fontSize = 10;
		}
		var cpf = ControlPanel.transform;
		TaskInfo = instance.transform.Find("TaskInfo").GetComponent<Text>();
		TaskInfo.gameObject.AddComponent<Drag>();
		TaskInfoRect = TaskInfo.GetComponent<RectTransform>();
		firstPos2 = TaskInfoRect.position;
		buttonCopy = new ButtonGroup(GetButton(cpf, "ButtonCopy"));
		buttonPaste = new ButtonGroup(GetButton(cpf, "ButtonPaste"));
		buttonClear = new ButtonGroup(GetButton(cpf, "ButtonClear"));
		buttonSave = new ButtonGroup(GetButton(cpf, "ButtonSave"));
		buttonStop = new ButtonGroup(GetButton(cpf, "ButtonStop"));
		buttonLocal = new ButtonGroup(GetButton(cpf, "ButtonLocal"));
		buttonClose = new ButtonGroup(GetButton(cpf, "ButtonClose"));
		buttonRItem = new ButtonGroup(GetButton(cpf, "ButtonRItem"));
		buttonClose.button.onClick.AddListener(delegate
		{
			ControlPanel.SetActive(false);
		});
		buttonCopy.text.text = ST.复制;
		buttonPaste.text.text = ST.粘贴;
		buttonClear.text.text = ST.清空;
		buttonSave.text.text = ST.保存;
		buttonLocal.text.text = ST.当前星球;
		buttonRItem.text.text = ST.补充物品;
		buttonStop.text.text = ST.强制停止;

		SaveName = ControlPanel.transform.Find("SaveText").GetComponent<InputField>();
		SaveName.text = string.Empty;
		SaveName.textComponent.color = Color.black;
		SaveName.textComponent.fontSize = 14;
		isLoad = true;

	}

	public void UIPostionReast() {
		ControlPanelRect.position = firstPos1;
		TaskInfoRect.position = firstPos2;
	}

	public void SetActive(bool active)
    {
		instance.SetActive(active);
	}

	public void OpenOrClose()
    {
		isShow = !isShow;
		SetActive(isShow);
    }

	public Button GetButton(Transform tf,string name)
    {
		return tf.Find(name).GetComponent<Button>();
    }

	public class ButtonGroup
    {
		public Button button;
		public Text text;
		public Image img;
		public GameObject gameObject;
		public ButtonGroup(Button button)
        {
			this.button = button;
			gameObject = button.gameObject;
			var bt = button.GetComponentInChildren<Text>();
			if (bt != null)
				text = bt;
			img= button.GetComponent<Image>();
        }

		public void SetActive(bool falg)
        {
			gameObject.SetActive(falg);
        }

		public void SetOnclik(UnityAction call)
        {
			button.onClick.AddListener( call);
        }
    }

	class DragParent : MonoBehaviour, IDragHandler
	{
		private RectTransform rectTransform;
		private RectTransform parent;
		// Start is called before the first frame update
		void Start()
		{
			rectTransform = GetComponent<RectTransform>();
			parent = rectTransform.parent.GetComponent<RectTransform>();
		}
		public void OnDrag(PointerEventData eventData)
		{
			Vector3 pos;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.enterEventCamera, out pos);
			parent.position = pos+new Vector3(0.7f,-2.5f,0);
		}
    }

	class Drag : MonoBehaviour, IDragHandler
	{
		private RectTransform rectTransform;
		// Start is called before the first frame update
		void Start()
		{
			rectTransform = GetComponent<RectTransform>();
		}
		public void OnDrag(PointerEventData eventData)
		{
			Vector3 pos;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.enterEventCamera, out pos);
			rectTransform.position = pos;
		}
	}
	//class Drag : MonoBehaviour
	//{
	//	RectTransform rt;
	//	RectTransform parent;
	//	RectTransform canvas;
	//	Vector3 lastPosition;
	//	bool drag = false;
	//	long c = 0;
	//	void Start()
	//	{
	//		rt = rt.GetComponent<RectTransform>();//标题栏的rt
	//		parent = rt.parent.GetComponent<RectTransform>();//BasePanel的rt
	//		canvas = parent.parent.GetComponent<RectTransform>();//canvas的rt
	//	}
	//	void Update()
	//	{
	//		//获取鼠标在游戏窗口的unity坐标
	//		var m = Input.mousePosition - Vector3.right * Screen.width / 2 - Vector3.up * Screen.height / 2;
	//		m.x *= canvas.sizeDelta.x / Screen.width;
	//		m.y *= canvas.sizeDelta.y / Screen.height;
	//		//获取标题在游戏窗口内的坐标

	//		var rp = parent.localPosition + rt.localPosition;
	//		//获取标题的rect
	//		var re = rt.rect;
	//		//判断鼠标是否在标题的范围内按下
	//		if (m.x >= rp.x - re.width / 2 && m.x <= rp.x + re.width / 2 && m.y >= rp.y - re.height / 2 && m.y <= rp.y + re.height / 2 && Input.GetMouseButtonDown(0))
	//		{
	//			Debug.Log("true");
	//			drag = true;
	//			lastPosition = m;
	//		}
	//		//获取鼠标是否松开
	//		else if (drag && Input.GetMouseButtonUp(0))
	//		{
	//			drag = false;
	//		}
	//		//根据鼠标的拖动更新窗口位置
	//		if (drag)
	//		{
	//			rt.localPosition += m - lastPosition;
	//			lastPosition = m;
	//		}
	//	}

	//}
}

