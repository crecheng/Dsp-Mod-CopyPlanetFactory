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
	/// <summary>
	/// 复制按钮组
	/// </summary>
	public ButtonGroup buttonCopy;
	/// <summary>
	/// 粘贴按钮组
	/// </summary>
	public ButtonGroup buttonPaste;
	/// <summary>
	/// 清空按钮组
	/// </summary>
	public ButtonGroup buttonClear;
	/// <summary>
	/// 保存按钮组
	/// </summary>
	public ButtonGroup buttonSave;
	/// <summary>
	/// 当前星球按钮组
	/// </summary>
	public ButtonGroup buttonLocal;
	/// <summary>
	/// 关闭按钮组
	/// </summary>
	public ButtonGroup buttonClose;
	/// <summary>
	/// 撤销任务按钮组
	/// </summary>
	public ButtonGroup buttonZ;
	/// <summary>
	/// 保存名字输入框
	/// </summary>
	public InputField SaveName;
	/// <summary>
	/// 实例对象
	/// </summary>
	public GameObject instance;
	/// <summary>
	/// 资源包实例
	/// </summary>
	public GameObject Res;
	/// <summary>
	/// 控制按钮
	/// </summary>
	public ButtonGroup ControlButton;
	/// <summary>
	/// 控制面板
	/// </summary>
	public GameObject ControlPanel;
	/// <summary>
	/// 控制面板的方位控制
	/// </summary>
	public RectTransform ControlPanelRect;
	/// <summary>
	/// 预留面板
	/// </summary>
	public GameObject MainPanel;
	/// <summary>
	/// 任务信息
	/// </summary>
	public Text TaskInfo;

	public Text Info;
	/// <summary>
	/// 任务信息矩阵
	/// </summary>
	public RectTransform TaskInfoRect;
	/// <summary>
	/// 文件按钮组
	/// </summary>
	public ButtonGroup[]  ButtonDataFile;
	/// <summary>
	/// 文件上一页按钮
	/// </summary>
	public ButtonGroup  ButtonDataUp;
	/// <summary>
	/// 文件页数
	/// </summary>
	public Text  ButtonDataPage;
	/// <summary>
	/// 文件下一页
	/// </summary>
	public ButtonGroup  ButtonDataDown;
	/// <summary>
	/// 是否加载成功
	/// </summary>
	public bool isLoad = false;
	/// <summary>
	/// 是否显示
	/// </summary>
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
			//TaskInfo.gameObject.SetActive(!ControlPanel.activeSelf);
			ControlPanel.SetActive(!ControlPanel.activeSelf);
			
		});
		ControlButton.SetActive(true);

		Info = ControlPanel.transform.Find("Info").GetComponent<Text>();
		Info.color = Color.white;
		var filePanel = ControlPanel.transform.Find("FilesPanel").gameObject;
		ButtonDataFile = new ButtonGroup[7];
		for (int i = 1; i < 8; i++)
		{
			ButtonDataFile[i - 1] = new ButtonGroup(GetButton(filePanel.transform, "B" + i));
			ButtonDataFile[i - 1].SetActive(false);
			ButtonDataFile[i - 1].text.fontSize = 10;
		}
		ButtonDataPage = filePanel.transform.Find("Page").gameObject.transform.GetComponentInChildren<Text>();
		ButtonDataUp = new ButtonGroup(GetButton(filePanel.transform, "ButtonUp"));
		ButtonDataDown = new ButtonGroup(GetButton(filePanel.transform, "ButtonDown"));
		var cpf = ControlPanel.transform;
		//设置信息面板实例
		TaskInfo = instance.transform.Find("TaskInfo").GetComponent<Text>();
		//加载拖动组件
		TaskInfo.gameObject.AddComponent<Drag>();
		//获取任务面板关闭按钮
		var closeTask= TaskInfo.GetComponentInChildren<Button>();

		closeTask.onClick.AddListener(delegate
		{
			//关闭任务面板
			TaskInfo.gameObject.SetActive(false);
		});
		TaskInfoRect = TaskInfo.GetComponent<RectTransform>();
		TaskInfoRect.sizeDelta = new Vector2(TaskInfoRect.sizeDelta.x, Screen.height *0.7f);
		firstPos2 = TaskInfoRect.position;
		//暂时关闭，只显示错误信息
		TaskInfo.gameObject.SetActive(false);
		//设置按钮实例
		buttonCopy = new ButtonGroup(GetButton(cpf, "ButtonCopy"));
		buttonPaste = new ButtonGroup(GetButton(cpf, "ButtonPaste"));
		buttonClear = new ButtonGroup(GetButton(cpf, "ButtonClear"));
		buttonSave = new ButtonGroup(GetButton(cpf, "ButtonSave"));
		buttonLocal = new ButtonGroup(GetButton(cpf, "ButtonLocal"));
		buttonClose = new ButtonGroup(GetButton(cpf, "ButtonClose"));
		buttonZ = new ButtonGroup(GetButton(cpf, "ButtonZ"));
		buttonClose.button.onClick.AddListener(delegate
		{
			ControlPanel.SetActive(false);
		});



		var buttonHelp = ControlPanel.transform.Find("Help").GetComponent<Button>();
		buttonHelp.onClick.AddListener(delegate
		{
			System.Diagnostics.Process.Start("https://www.bilibili.com/video/BV1gZ4y1w7RY");
			buttonHelp.gameObject.SetActive(false);
		});
		if(Localization.language != Language.zhCN)
        {
			buttonHelp.gameObject.SetActive(false);
        }

		//加载翻译
		buttonCopy.text.text = ST.复制;
		buttonPaste.text.text = ST.粘贴;
		buttonClear.text.text = ST.清空;
		buttonSave.text.text = ST.保存;
		buttonLocal.text.text = ST.当前星球;
		buttonZ.text.text = ST.撤销任务;

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
		void Start()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		public void OnDrag(PointerEventData eventData)
		{
			Vector3 pos;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.enterEventCamera, out pos);
			rectTransform.position = pos ;
		}
	}
}

