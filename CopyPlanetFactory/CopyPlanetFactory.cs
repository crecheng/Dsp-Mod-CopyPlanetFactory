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

[BepInPlugin("crecheng.CopyPlanetFactory", "CopyPlanetFactory",CopyPlanetFactory.Version )]
public class CopyPlanetFactory : BaseUnityPlugin
{
	public const string Version = "2.3.7";
	public const bool isDebug = false;
	public static bool isLoad = false;
	static MyUI ui;
	public long frame = 0;
	public long clickFrame = 0;
	public Texture2D RedRectX;
	public Texture2D RedRectY;
	public GUIStyle rectStyle;
	public static FactoryTask PastIngData = null;
	public static bool isShowItem = false;
	public static bool confirmStop = false;
	private static int atPage = 0;
	private static string info1 = string.Empty;
	private static string info = string.Empty;
	private static FactoryTask FData = new FactoryTask();
	private static List<FactoryData> DataList = new List<FactoryData>();
	private static string Buginfo = string.Empty;
	private static Rect rect = new Rect(30f, 30f, 1000, 600);
	private static GUIStyle haveStyle = new GUIStyle();
	private static GUIStyle noStyle = new GUIStyle();
	private static int haveItemCount = 0;
	private static int noItemCount = 0;
	private static bool[] area = new bool[8];
	private static FactoryData SelectData = null;
	private static string haveItem = string.Empty;
	private static string noItem = string.Empty;
	private static string nameInput = string.Empty;
	void Start()
	{
		Harmony.CreateAndPatchAll(typeof(CopyPlanetFactory), null);
		readFile();
		haveStyle.fontSize = 15;
		noStyle.fontSize = 15;
		haveStyle.normal.textColor = new Color(255f, 255f, 255f);
		noStyle.normal.textColor = new Color(255f/256f, 77f / 256f, 77f / 256f);
		windowsBorder = new Texture2D(826, 525);
		windowsBorder.LoadImage(Convert.FromBase64String(MyResources.WindowsBorder));
		//windowContent.image = windowsBorder;
		windowStyle.fontSize = 15;
		var res = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CopyPlanetFactory.copyplanetallui"));
		ui=new MyUI(res.LoadAsset<GameObject> ("GameObject"));
		rectImg = new RectImg();
		BuildName = new Dictionary<string, int>();
		RecipeName = new Dictionary<string, int>();
	}

	void Update()
	{
		frame++;
		if(ui!=null&&!ui.isLoad && UIRoot.instance.overlayCanvas.transform!=null && GameMain.instance != null)
        {
			var canvas = UIRoot.instance.overlayCanvas;
			ui.LoadUI(canvas.transform);
			SetUIData();
		}
        if (ui != null && ui.isLoad)
		{
			nameInput=ui.SaveName.text;
			nameInput.Replace("\\", "").Replace("/", "").Replace("?", "").Replace("|", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("*", "").Replace("\"", "");
			ui.SaveName.text = nameInput;
			if (PastIngData != null)
			{
				if (PastIngData.error)
				{
					isShowItem = true;
					ui.TaskInfo.color = Color.red;
					info1 = "error!!!\n"+PastIngData.errorMsg;
					ui.TaskInfo.text= PastIngData.errorMsg;
					var tRect = ui.TaskInfo.GetComponent<RectTransform>();
					tRect.sizeDelta = new Vector2(300f, 400f);
					ui.TaskInfo.gameObject.SetActive(true);
				}
			}
			ui.TaskInfo.text = info1;
			ui.Info.text = info+"\n"+(PastIngData!=null?"\n当前任务："+PastIngData.Data.Name:"");
			//Debug.Log($"{ui.ControlPanel.GetComponent<RectTransform>().position}|{Input.mousePosition}");
		}

	}

	void SetUIData()
    {
		ui.buttonCopy.SetOnclik(CopyData);
		ui.buttonPaste.SetOnclik(PasteData);
		ui.buttonSave.SetOnclik(SaveData);
		ui.buttonClear.SetOnclik(ClearData);
		ui.buttonLocal.SetOnclik(LocalImg);
		ui.ControlButton.SetOnclik(delegate
		{
			if (frame - clickFrame < 10)
			{
				ui.UIPostionReast();
			}
			clickFrame = frame;
		});
		SetPageData();
		for (int i = 0; i < 7; i++)
		{
			int index = i;
			ui.ButtonDataFile[i].SetOnclik(delegate
			{
				SelectData = GetData(index);
                if (SelectData != null)
                {
					AddChangeBuild(SelectData);
                }
				isShowImg = true;
				isLookLocal = false;
				isAreaSelect = false;
				AreaTrue();
				ui.ChangeRecipe.SetActive(true);
			});
		}
		ui.ButtonDataUp.SetOnclik(delegate
		{
			if (atPage > 0)
			{
				atPage--;
				PageTo();
			}
		});
		ui.ButtonDataDown.SetOnclik(delegate
		{
			if (atPage < 2100000000)
			{
				atPage++;
				PageTo();
			}
		});
		//点击撤销按钮
		ui.buttonZ.SetOnclik(delegate
		{
			if (GameMain.mainPlayer != null&&PastIngData!=null)
			{
				PastIngData.CancelTask(GameMain.mainPlayer);
				PastIngData.PasteClear();
			}
		});

		ui.SelectBuild.onValueChanged.AddListener(delegate
		{
			var name = ui.SelectBuild.options[ui.SelectBuild.value].text;
            if (BuildName.ContainsKey(name))
            {
				AddChangeRecipe(BuildName[name]);
            }
		});

		ui.ButtonChangeRecipe.SetOnclik(delegate
		{
			var name = ui.SelectBuild.options[ui.SelectBuild.value].text;
			if (BuildName.ContainsKey(name))
			{
				var recipeName = ui.SelectRecipe.options[ui.SelectRecipe.value].text;
                if (RecipeName.ContainsKey(recipeName))
                {
					int build = BuildName[name];
					int recipe = RecipeName[recipeName];
					if (SelectData != null)
					{
						SelectData.ChangeRecipe(build, recipe);
						info = SelectData.Name + "\n" + "修改配方成功";
					}
                }
			}
		});

		ui.ButtonOpneFile.SetOnclik(delegate
		{
			string path = System.Environment.CurrentDirectory + "\\BepInEx\\config\\PlanetFactoryData\\";
			System.Diagnostics.Process.Start(path);
		});

		ui.ButtonReLoadFile.SetOnclik(delegate
		{
			readFile();
			SelectData = null;
			atPage = 0;
			PageTo();
		});
	}
	void CopyData()
	{
		AreaTrue();
		var factory = GetFactory();
		if (factory != null)
		{
			FData.NewData();
			FData.CopyData(factory);
			for (int i = 0; i < 30; i++)
			{
				StartCoroutine(FData.Data.CheckBelt((float)(0.1 + 0.1 * i)));
			}
			SelectData = FData.Data;
			info = ST.复制 + ST.成功 + ":" + SelectData.AllData.Count;
			string temps;
			SelectData.CheckItem(null, out string ts1, out int i1, out temps, out int i2);
			info +="\n"+temps;
		}
	}


	void PasteData()
	{
		var player = GetPlayer();
		if (player != null && PastIngData == null)
		{
			if (FData.CheckCanPaste(player))
			{
				FData.PasteDate(player, area);
				info = ST.粘贴 + ST.成功;
				PastIngData = FData;
			}
			else
			{
				//提示物品不足
				info =  ST.物品 + ST.o + ST.不足 + "\n" + ST.noItemTip;
			}
		}
		

	}
	void ClearData()
	{
		FData.PasteClear();
		FData.NewData();
		SelectData = null;
		PastIngData = null;
		noItem = string.Empty;
		AreaFalse();
	}

	FactoryData GetData(int i)
    {
		int index=atPage*7+i;
        if (index < DataList.Count)
        {
            if (CheckData())
            {
				var player = GameMain.mainPlayer;
				DataList[index].CheckItem(player, out haveItem,out haveItemCount,out noItem,out noItemCount);
			}
            else
            {
				DataList[index].CheckItem(null, out haveItem, out haveItemCount, out noItem, out noItemCount);
            }
			isAreaSelect = false;
			isLookLocal = false;
			SelectData = DataList[index];
			FData.Data = SelectData;
			return DataList[index];
        }
		return null;
    }

	static Dictionary<string, int> BuildName;
	static Dictionary<string, int> RecipeName;

	public void AddChangeBuild(FactoryData data)
    {
		BuildName.Clear();
		ui.SelectBuild.ClearOptions();
		int i = 0;
		BuildName.Add("NULL",0);
		foreach (var d in data.ItemNeed)
        {
			var id = d.Key;
			var item = LDB.items.Select(id);
            if (item != null && item.prefabDesc.isAssembler)
            {
				BuildName.Add(item.name, d.Key);
			}
        }
		var Oldre = data.GetAllRecipe();
        foreach (var d in Oldre)
        {
			var recipe = LDB.recipes.Select(d);
            if (recipe != null)
            {
				BuildName.Add(ST.old+ ":" + recipe.name, -d);
            }
        }
		ui.SelectBuild.AddOptions(BuildName.Keys.ToList());
    }

	public void AddChangeRecipe(int itemId)
	{
		RecipeName.Clear();
		ui.SelectRecipe.ClearOptions();
		RecipeName.Add("NULL", 0);
		if (itemId == 0)
			return;

		int type = -1;
		if (itemId > 0)
		{
			var item = LDB.items.Select(itemId);
            if (item != null)
            {
				type = (int)item.prefabDesc.assemblerRecipeType;
            }
		}
        else if(itemId<0)
        {
			var item = LDB.recipes.Select(-itemId);
			if (item != null)
			{
				type = (int)item.Type;
			}
		}
        if (type > -1)
        {
			foreach (RecipeProto d in LDB.recipes.dataArray)
			{
				if (d.Type != (ERecipeType)type)
					continue;
				var id = d.ID;
				var items = d.Items;
				var res = d.Results;
				string option = "";
                for (int i = 0;;)
                {
					var temp= LDB.items.Select(res[i]);
                    if (temp != null)
                    {
						if (temp != null)
							option += temp.name;
                    }
					i++;
					if ( i< res.Length)
                    {
						option += "+";
                    }
                    else
                    {
						break;
                    }
                }
				option += "\n";
				for (int i = 0; ;)
				{
					var temp = LDB.items.Select(items[i]);
					if (temp != null)
					{
						option += temp.name;
					}
					i++;
					if (i < items.Length)
					{
						if(temp!=null)
							option += "+";
					}
					else
					{
						break;
					}
				}
				RecipeName.Add(option, id);

			}
		}
		ui.SelectRecipe.AddOptions(RecipeName.Keys.ToList());
	}

	FactoryData GetDataInPage(int i)
    {
		int index = atPage * 7 + i;
		if (index < DataList.Count)
		{
			return DataList[index];
		}
		return null;
	}


	void SaveData()
	{
		if (FData.Data.Count > 0 && nameInput.Length > 0)
		{
			FData.Data.Name = nameInput;
			FData.Data.Export();
			DataList.Add(FData.Data);
			info = "保存在BepInEx\\config\\PlanetFactoryData";
			FData.NewData();
			PageTo();
		}
	}

	void LocalImg()
    {
		SelectData = null;
		isShowImg = true;
		isLookLocal = true;
	}


	static void readFile()
    {

		string path = System.Environment.CurrentDirectory + "\\BepInEx\\config\\PlanetFactoryData\\";
		string filename = string.Empty;
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			else
			{
				DataList.Clear();
				var f = Directory.GetFiles(path);
				foreach (var d in f)
				{
					FactoryData data = new FactoryData();
					filename = d.Split('\\').Last() ;
					data.Import(d);
					DataList.Add(data);
				}
			}
			
		}
		catch(Exception e)
        {
			Debug.LogError("readFileError");
			Debug.LogError(e.Message);
			FactoryData data = new FactoryData();
			data.Name = filename + ST.导入错误+"！";
			DataList.Add(data);
		}
	}

	void SetPageData()
	{
		atPage = 0;
		PageTo();
	}

	void PageTo()
	{
		ui.ButtonDataPage.text = "" + (atPage + 1);

		int start = atPage * 7;
		int c = DataList.Count - start;
		if (c > 0)
		{
			for (int i = 0; i < 7; i++)
			{
				ui.ButtonDataFile[i].SetActive(true);
				var temp = GetDataInPage(i);
				if (temp != null)
					ui.ButtonDataFile[i].text.text =temp.Name;
				else
				{
					ui.ButtonDataFile[i].text.text = string.Empty;
					ui.ButtonDataFile[i].SetActive(false);
				}
			}
		}
        else
        {
			for (int i=0; i < 7; i++)
			{
				ui.ButtonDataFile[i].SetActive(false);
			}
		}
	}

	void RemoverAllBuilding()
    {
        if (CheckData())
        {
			var player = GameMain.mainPlayer;
			var factory = player.planetData.factory;
			for(int i = 0; i < factory.entityCursor; i++)
            {
				if (factory.entityPool[i].protoId > 0)
				{
					if (!Common.RemoveBuild(player, factory, i))
						break;
				}
            }
			
        }
    }

	void RemoveSelectBuild(List<int> id)
	{
		if (CheckData())
		{
			var player = GameMain.mainPlayer;
			var factory = player.planetData.factory;
			foreach (int i in id)
			{
				if (factory.entityPool[i].protoId > 0)
				{
					if (!Common.RemoveBuild(player, factory, i))
						break;
				}
			}

		}
	}



	static Texture2D windowsBorder = new Texture2D(0, 0);
	static GUIContent windowContent = new GUIContent();
	static GUIStyle windowStyle = new GUIStyle();
	static int ImgX = 0;
	static int ImgY = 0;
	void OnGUI()
	{
		
		if (isShowImg)
		{
			rect = GUI.Window(1935598199, rect, mywindowfunction,windowContent);

		}
		if (isDebug)
		{
			GUI.Label(new Rect(100, 100, 300, 700), Buginfo);
		}
	}

	static bool isShowImg = false;
	static PlanetFactoryImg localImg = new PlanetFactoryImg();
	static int rectx1=0;
	static int rectx2=801;
	static int recty1=0;
	static int recty2=400;
	static bool isAreaSelect;
	static int ImgHeight = 400;
	static RectImg rectImg;
	static bool isLookLocal = false;
	static string dataTipString = string.Empty;
	void mywindowfunction(int windowid)
	{
		int h = ImgHeight;
		int w = ImgHeight * 2 + 1;
		Rect ImgRect = new Rect(10, 10,w,h);
		ImgX = (int)GUI.VerticalSlider(new Rect(10+w, 100, 20, 200), ImgX, 0, 180);
		ImgY = (int)GUI.HorizontalSlider(new Rect(300, 10+h, 200, 20), ImgY, 0, 180);
        if (isLookLocal)
			GUI.Label(ImgRect, localImg.GetImg(ImgX, ImgY, GetFactory()));
		else if (SelectData != null)
			GUI.Label(ImgRect, SelectData.GetImg(ImgX, ImgY));

		
		if (ImgX < 2 && ImgY < 2)
		{
			float by = ImgRect.y + ImgRect.height - 20;
			GUI.Label(new Rect(ImgRect.x, ImgRect.y + 3, 60, 20), ST.东 + ST.北 + ST.左, noStyle);
			GUI.Label(new Rect(ImgRect.x, by, 60, 20), ST.东 + ST.南 + ST.左, noStyle);
			GUI.Label(new Rect(ImgRect.x + ImgRect.width / 2 - 60, ImgRect.y + 3, 60, 20), ST.东 + ST.北 + ST.右, noStyle);
			GUI.Label(new Rect(ImgRect.x + ImgRect.width / 2 - 60, by, 60, 20), ST.东 + ST.南 + ST.右, noStyle);
			GUI.Label(new Rect(ImgRect.x + ImgRect.width / 2 + 1, ImgRect.y + 3, 60, 20), ST.西 + ST.北 + ST.右, noStyle);
			GUI.Label(new Rect(ImgRect.x + ImgRect.width / 2 + 1, by, 60, 20), ST.西 + ST.南 + ST.右, noStyle);
			GUI.Label(new Rect(ImgRect.x + ImgRect.width - 60, ImgRect.y + 3, 60, 20), ST.西 + ST.北 + ST.左, noStyle);
			GUI.Label(new Rect(ImgRect.x + ImgRect.width - 60, by, 60, 20), ST.西 + ST.南 + ST.左, noStyle);
		}
		if (GUI.Button(new Rect(ImgRect.x + ImgRect.width, ImgRect.y + 3, 20, 20), "X"))
		{
			isShowImg = false;
			ui.ChangeRecipe.SetActive(false);
			isLookLocal = false;
			isAreaSelect = false;
		}

		if (GetFactory() != null&&isLookLocal)
		{
			if (GUI.Button(new Rect(610, h+80, 120, 40), $"{ST.移除}{ST.o}{ST.当前星球}\n{ST.全部}{ST.o}{ST.建筑}"))
			{
				RemoverAllBuilding();
			}
			if(GUI.Button(new Rect(735, h + 80, 120, 40), $"{ST.加满}{ST.o}{ST.物流站} {ST.飞船}{ST.和}{ST.翘曲}"))
			{
				AddShip();
            }
			if (GUI.Button(new Rect(860, h + 80, 120, 40), $"{ST.移除}{ST.o}{ST.noConnPaw}"))
			{
				ClearNotConnPaw();
			}
			if (GUI.Button(new Rect(860, h + 120, 120, 40), $"{ST.尝试连接断开传送带}"))
			{
				TryConnBelt();
			}
		}
        if (CheckData()&&isLookLocal)
        {
			if(GUI.Button(new Rect(500, h+80, 100, 40), isAreaSelect ?   "取消": ST.区域选择)){
				isAreaSelect = !isAreaSelect;
            }
        }
		if (isAreaSelect)
		{
			recty1 = (int)GUI.VerticalSlider(new Rect(35 + w, 10, 20, h), recty1, h, 0);
			recty2 = (int)GUI.VerticalSlider(new Rect(60 + w, 10, 20, h), recty2, h, 0);
			rectx1 = (int)GUI.HorizontalSlider(new Rect(10, h + 35, w, 20), rectx1, 0, w);
			rectx2 = (int)GUI.HorizontalSlider(new Rect(10, h + 60, w, 20), rectx2, 0, w);
			GUI.Label(new Rect(10, 10, w, h), rectImg.getRect(rectx1, rectx2, recty1, recty2));
			var fd = GetFactory();
			GUI.Label(new Rect(10, h + 80, 200, 500), $"{ rectx1},{rectx2},{recty1},{recty2}");
			if (GUI.Button(new Rect(500, h + 130, 100, 60), ST.复制选定区域))
			{
				List<int> id = new List<int>();
				localImg.SelectBuild(fd, id, rectx1, rectx2, recty1, recty2);
				AreaTrue();
				FData.CopyData(fd, id);
				for (int i = 0; i < 30; i++)
				{
					StartCoroutine(FData.Data.CheckBelt((float)(0.1 + 0.1 * i)));
				}
				SelectData = FData.Data;
				FData.Data.GetImg(ImgX, ImgY);
				info1 = ST.复制 + ST.成功 + ":" + SelectData.AllData.Count;
				string temps;
				SelectData.CheckItem(null, out string ts1, out int i1, out temps, out int i2);
				info += temps;
			}
			if (GUI.Button(new Rect(610, h + 130, 120, 60), $"{ST.移除}{ST.o}{ST.选定}{ST.o}{ST.区域}{ST.o}{ST.建筑}"))
			{
				List<int> id = new List<int>();
				localImg.SelectBuild(fd, id, rectx1, rectx2, recty1, recty2);
				RemoveSelectBuild(id);
			}
		}
		if (SelectData != null&&!isAreaSelect)
		{
			int buttonW = 160;
			int buttonH = 20;
			if (GUI.Button(new Rect(10, h+30, buttonW, buttonH), ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Null);
			}
			if (GUI.Button(new Rect(10, h + 50, buttonW, buttonH), ST.赤道 + "(Y)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Y);
			}
			if (GUI.Button(new Rect(10, h + 70, buttonW, buttonH), ST.左右 + "(Z)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Z);
			}
			if (GUI.Button(new Rect(10, h + 90, buttonW, buttonH), ST.东西 + "(X)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.X);
			}
			if (GUI.Button(new Rect(10, h + 110, buttonW, buttonH), "XY " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XY);
			}
			if (GUI.Button(new Rect(10, h + 130, buttonW, buttonH), "XZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XZ);
			}
			if (GUI.Button(new Rect(10, h + 150, buttonW, buttonH), "YZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.YZ);
			}
			if (GUI.Button(new Rect(10, h + 170, buttonW, buttonH), "XYZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XYZ);
			}

			int bc = 1;
			if (GUI.Button(new Rect(180, h + 10 +20 * bc++, 100, 20), ST.北 + ST.半球))
			{
				AreaFalse();
				area[0] = true;
				area[1] = true;
				area[4] = true;
				area[5] = true;
			}
			if (GUI.Button(new Rect(180, h + 10 +20 * bc++, 100, 20), ST.南 + ST.半球))
			{
				AreaFalse();
				area[2] = true;
				area[3] = true;
				area[6] = true;
				area[7] = true;
			}
			if (GUI.Button(new Rect(180, h + 10 +20 * bc++, 100, 20), ST.东 + ST.半球))
			{
				AreaFalse();
				area[0] = true;
				area[1] = true;
				area[2] = true;
				area[3] = true;
			}
			if (GUI.Button(new Rect(180, h + 10 +20 * bc++, 100, 20), ST.西 + ST.半球))
			{
				AreaFalse();
				area[4] = true;
				area[5] = true;
				area[6] = true;
				area[7] = true;
			}
			if (GUI.Button(new Rect(180, h + 10 +20 * bc++, 100, 20), ST.左 + ST.半球))
			{
				AreaFalse();
				area[1] = true;
				area[3] = true;
				area[5] = true;
				area[7] = true;
			}
			if (GUI.Button(new Rect(180, h + 10 +20 * bc++, 100, 20), ST.右 + ST.半球))
			{
				AreaFalse();
				area[0] = true;
				area[2] = true;
				area[4] = true;
				area[6] = true;
			}
			bc = 1;
			area[0] = GUI.Toggle(new Rect(300, h + 20 +20 * bc++, 100, 20), area[0], $"1:{ST.东},{ST.北},{ST.右}");
			area[1] = GUI.Toggle(new Rect(300, h + 20 +20 * bc++, 100, 20), area[1], $"2:{ST.东},{ST.北},{ST.左}");
			area[2] = GUI.Toggle(new Rect(300, h + 20 +20 * bc++, 100, 20), area[2], $"3:{ST.东},{ST.南},{ST.右}");
			area[3] = GUI.Toggle(new Rect(300, h + 20 +20 * bc++, 100, 20), area[3], $"4:{ST.东},{ST.南},{ST.左}");
			bc = 1;								  
			area[4] = GUI.Toggle(new Rect(400, h + 20 +20 * bc++, 100, 20), area[4], $"5:{ST.西},{ST.北},{ST.右}");
			area[5] = GUI.Toggle(new Rect(400, h + 20 +20 * bc++, 100, 20), area[5], $"6:{ST.西},{ST.北},{ST.左}");
			area[6] = GUI.Toggle(new Rect(400, h + 20 +20 * bc++, 100, 20), area[6], $"7:{ST.西},{ST.南},{ST.右}");
			area[7] = GUI.Toggle(new Rect(400, h + 20 +20 * bc++, 100, 20), area[7], $"8:{ST.西},{ST.南},{ST.左}");
			GUI.Label(new Rect(10, h + 10, 160, 20), "Data Version:" + SelectData.version);
			SelectData.tip= GUI.TextArea(new Rect(505, h + 10, 300, 150), SelectData.tip.Length>0?SelectData.tip:ST.DataTip);
			SelectData.Name= GUI.TextArea(new Rect(505, h + 167, 240, 150), SelectData.Name);
			if (GUI.Button(new Rect(740, h + 167, 60, 25), ST.保存))
            {
				SelectData.Name= SelectData.Name.Replace("\\", "").Replace("/", "").Replace("?", "").Replace("|", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("*", "").Replace("\"", "");
				SelectData.Export();
            }
			if (!isAreaSelect)
			{
				GUI.Label(new Rect(w+40, 10, 250, haveItemCount * 16), haveItem, haveStyle);
				GUI.Label(new Rect(w+40, 30 + haveItemCount * 16, 250, noItemCount * 16), noItem, noStyle);
			}
			//area[0] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[0], "1:+X,+Y,+Z");
			//area[1] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[1], "2:+X,+Y, -Z");
			//area[2] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[2], "3:+X, -Y,+Z");
			//area[3] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[3], "4:+X, -Y, -Z");
			//area[4] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[4], "5: -X,+Y,+Z");
			//area[5] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[5], "6: -X,+Y, -Z");
			//area[6] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[6], "7: -X, -Y,+Z");
			//area[7] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[7], "8: -X, -Y, -Z");

		}

		if(GUI.Button(new Rect(60, 0, 20, 20), "+"))
        {
			ImgHeight += 20;
			rect.height += 20;
			rect.width += 40;
			RefreshImg();
		}
		if (GUI.Button(new Rect(80, 0, 20, 20), "-"))
		{
			if (ImgHeight > 400)
			{
				ImgHeight -= 20;
				rect.height -= 20;
				rect.width -= 40;
				RefreshImg();
			}
		}
		if (GUI.Button(new Rect(100, 0, 20, 20), "0"))
		{
			ImgHeight = 400;
			rect.height =600;
			rect.width =1000;
			RefreshImg();
		}
		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	void RefreshImg()
    {
		PlanetFactoryImg.imgh = ImgHeight;
		RectImg.imgh = ImgHeight;
		if (isLookLocal)
			localImg.Fresh(GetFactory());
		else if (SelectData != null)
			SelectData.FreshImg(); 
	}

	private void PasteData(FactoryData data, ERotationType rotationType = ERotationType.Null)
	{
		var player = GetPlayer();
		if (player != null && PastIngData == null)
		{
			FactoryTask task = new FactoryTask(data);//新建粘贴任务
			//检测是否够物品										
			if (task.CheckCanPaste(player))
			{
				task.PasteDate(player, area, rotationType);//进行粘贴任务
				info = data.Name + ST.粘贴 + ST.成功;
				PastIngData = task;
			}
            else
            {
				//提示物品不足
				info = data.Name + ST.物品 + ST.o + ST.不足+"\n"+ST.noItemTip;
            }
		}
	}

	public void AreaFalse()
    {
		for(int i = 0; i < 8; i++)
        {
			area[i] = false;
        }
    }

	public void AreaTrue()
	{
		for (int i = 0; i < 8; i++)
		{
			area[i] = true;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlanetFactory), "AddEntityDataWithComponents")]
	static void getEntityId(PlanetFactory __instance, EntityData entity, int prebuildId, ref int __result)
	{
		
		if (PastIngData != null && prebuildId > 0 && __result > 0)
		{
			if (PastIngData.planetFactory == __instance)
			{
				if (PastIngData.Working)
				{
					PastIngData.Building(prebuildId, __result);
				}
				else
				{
					PastIngData = null;
				}
			}
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GameData), "GameTick")]
	static void getInfo(GameData __instance)
	{
        if (PastIngData != null)
        {
			if (!PastIngData.Working)
				PastIngData = null;
        }
		if (isDebug)
		{
			if (CheckData())
			{
				Player player = GameMain.data.mainPlayer;
				if (player.controller != null && player.controller.cmd.raycast != null)
				{
					var ce = player.controller.cmd.raycast.castEntity;
					int inid = ce.beltId;
					var ed = player.factory.entityPool[ce.id];
					var bd = player.factory.cargoTraffic.beltPool[inid];
					var sid = ce.stationId;
					var ejId = ce.powerExcId;
					Buginfo = "eid:" + ce.id.ToString();
					Buginfo += "\npos:" + ed.pos;
					Buginfo += "\nrot:" + ed.rot;
					Buginfo += "\nsplitterId:" + ejId;
					Buginfo += "\nmodelIndex:" + ce.modelIndex;
					Buginfo += "\nlabId:" + ce.labId;
                    //var eul = ed.rot.eulerAngles;
                    //Buginfo += "\nrot.eulerAngles:" + eul;
                    //Buginfo += "\nrot.eulerAngles.x:" + eul.x;
                    //if (ce.inserterId > 0)
                    //{
                    //	var d = player.factory.factorySystem.inserterPool[ce.inserterId];
                    //	Buginfo += "\npos2:" + d.pos2;
                    //	Buginfo += "\nrot2:" + d.rot2;
                    //}
                    //if (ce.labId > 0)
                    //{
                    //	var d = player.factory.factorySystem.labPool[ce.labId];
                    //	Buginfo += "\ntimespeed:" + d.timeSpend;
                    //	Buginfo += "\ntime:" + d.time;
                    //
                    //}
                    //Buginfo += "\nbId:" + ce.beltId;
                    Buginfo += "\ncoon:\n";
                    if (ce.id > 0)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            int conn = player.planetData.factory.entityConnPool[ce.id * 16 + i];
                            if (conn > 0)
                            {
                                player.planetData.factory.ReadObjectConn(ce.id, i, out bool isO, out int other, out int slot);
                                Buginfo += "【" + i + "】:" + conn + "," + isO + "," + other + "," + slot + "\n";
                                if (other > 0)
                                {
                                    for (int j = 0; j < 16; j++)
                                    {
                                        int conn1 = player.planetData.factory.entityConnPool[other * 16 + j];
                                        if (conn1 > 0)
                                        {
                                            player.planetData.factory.ReadObjectConn(other, j, out bool isO1, out int other1, out int slot1);
                                            Buginfo += "--【" + j + "】:" + conn1 + "," + isO1 + "," + other1 + "," + slot1 + "\n";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (ce.inserterId > 0)
                    {

						var cd = player.planetData.factory.factorySystem.inserterPool[ce.inserterId];

						Buginfo += "\n___________________";
						Buginfo += "\ninsertTarget:" + cd.insertTarget;
						Buginfo += "\npickTarget:" + cd.pickTarget;

					}
                    //if (ejId > 0)
                    //{
                    //	var da = player.factory.cargoTraffic.splitterPool[ejId];


                    //	Buginfo += "\na:" + da.beltA;
                    //	Buginfo += "\nb:" + da.beltB;
                    //	Buginfo += "\nc:" + da.beltC;
                    //	Buginfo += "\nd:" + da.beltD;
                    //}


                }

			}
		}
	}

	void AddShip()
	{
		var factory = GetFactory();
		if (factory != null)
		{

			var player = GameMain.mainPlayer;
			for (int i = 1; i < factory.transport.stationCursor; i++)
			{
				var sc = factory.transport.stationPool[i];
				if (sc != null)
				{
					ItemProto itemProto2 = LDB.items.Select((int)factory.entityPool[sc.entityId].protoId);
					if (!sc.isCollector)
					{
						int item1 = 5001;

						int max = (itemProto2 == null) ? 10 : itemProto2.prefabDesc.stationMaxDroneCount;
						int have = sc.idleDroneCount + sc.workDroneCount;
						int need = max - have;
						if (need < 0)
						{
							need = 0;
						}
						if (need > 0)
						{
							int phave = player.package.GetItemCount(item1);
							if (phave >= need)
								sc.idleDroneCount += need;
							player.package.TakeItem(item1, need);
						}
					}

					if (sc.isStellar && !sc.isCollector)
					{
						int item2 = 5002;
						int max = (itemProto2 == null) ? 10 : itemProto2.prefabDesc.stationMaxShipCount;
						int have = sc.idleShipCount + sc.workShipCount;
						int need = max - have;
						if (need < 0)
						{
							need = 0;
						}
						if (need > 0)
						{
							int phave = player.package.GetItemCount(item2);
							if (phave >= need)
								sc.idleShipCount += need;
							player.package.TakeItem(item2, need);
						}
					}

					if (sc.isStellar)
					{

						int item3 = 1210;
						int max = 50;
						int have = sc.warperCount;
						int need = max - have;
						if (need < 0)
						{
							need = 0;
						}

						if (need > 0)
						{
							int phave = player.package.GetItemCount(item3);
							if (phave >= need)
								sc.warperCount += need;
							player.package.TakeItem(item3, need);
						}
					}
				}
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameMain), "OnDestroy")]
	static void End()
    {
        if (PastIngData != null)
        {
			PastIngData.PasteClear();
			PastIngData = null;
        }
    }

	/// <summary>
	/// 清除无连接爪子
	/// </summary>
	void ClearNotConnPaw()
    {
		//获得当前的工厂对象
		var factory = GetFactory();
        if (factory != null)
        {
			//获得无连接的爪子数据
			var list = Common.FindNotConnPaw(factory);
            foreach (var i in list)
            {
				//获得爪子的实体id
				int eid = factory.factorySystem.inserterPool[i].entityId;
                if (eid > 0)
                {
					//执行移除操作
					Common.RemoveBuild(GameMain.mainPlayer, factory, eid);
                }
            }
        }
    }

	/// <summary>
	/// 尝试连接无连接传送带
	/// </summary>
	void TryConnBelt()
	{
		//获得当前的工厂对象
		var factory = GetFactory();
		if (factory != null)
		{
			List<int> Out = new List<int>();
			List<int> In = new List<int>();
			//获取无连接的传送带数据
			Common.FindDisconnectBelt(factory, Out, In);
            foreach (var i in Out)
            {
				//获取传送带数据
				var belt= factory.cargoTraffic.beltPool[i];
				var eid = belt.entityId;
				//获取与其连接的in的传送带
				factory.ReadObjectConn(eid, 1, out bool isout, out int other, out int otherSlot);
				var belt1 = factory.cargoTraffic.beltPool[factory.entityPool[other].beltId];
                if (belt1.entityId > 0)
                {
					//计算传送带之间的距离
					var dis = (factory.entityPool[belt1.entityId].pos - factory.entityPool[eid].pos).sqrMagnitude;
					//预计下一个传送带的位置
					var pos = factory.entityPool[eid].pos + (factory.entityPool[belt1.entityId].pos - factory.entityPool[eid].pos);
					int target = 0;
					//设置最小距离为两倍距离，大于两倍距离就舍去
					float minDis = 2f * dis;
                    foreach (var j in In)
                    {
						//获取传送带数据
						var belt2 = factory.cargoTraffic.beltPool[j];
						var entity = factory.entityPool[belt2.entityId];
						//获取两者之间的距离
						var dis1= (entity.pos - factory.entityPool[eid].pos).sqrMagnitude;
                        if (dis1< minDis)
                        {
							target = j;
							minDis = dis1;
                        }
					}
					//当找到合适的传送带时
                    if (target !=0)
                    {
						var belt2 = factory.cargoTraffic.beltPool[target];
						var entity = factory.entityPool[belt2.entityId];
						//进行连接
						factory.WriteObjectConn(eid, 0, true, belt2.entityId, 1);
						factory.cargoTraffic.AlterBeltConnections(i, belt2.id, belt.backInputId, belt.leftInputId, belt.rightInputId);
					}
                }
            }
		}

	}

	static bool CheckData()
    {
		return FactoryTask.CheckData();
	}

	static Player GetPlayer()
    {
        if (CheckData())
        {
			return GameMain.mainPlayer;
        }
		return null;
    }

	static PlanetFactory GetFactory()
    {
		if (CheckData())
		{
			return GameMain.mainPlayer.planetData.factory;
		}
		return null;
	}

}

