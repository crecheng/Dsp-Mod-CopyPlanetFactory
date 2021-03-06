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
	
	public const string Version = "2.0.0";
	public const bool isDebug = false;
	public static bool isLoad = false;
	static MyUI ui;
	public long frame = 0;
	public long clickFrame = 0;
	public Texture2D RedRectX;
	public Texture2D RedRectY;
	public GUIStyle rectStyle;
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
	}

	void Update()
	{
		frame++;
		if(ui!=null&&!ui.isLoad && UIRoot.instance.overlayCanvas.transform!=null && GameMain.instance != null)
        {
			var canvas = UIRoot.instance.overlayCanvas;
			ui.LoadUI(canvas.transform);
			ui.buttonCopy.SetOnclik(CopyData);
			ui.buttonPaste.SetOnclik(PasteData);
			ui.buttonSave.SetOnclik(SaveData);
			ui.buttonClear.SetOnclik(ClearData);
			ui.buttonStop.SetOnclik(StopData);
			ui.buttonLocal.SetOnclik(ShowImg);
			ui.buttonRItem.SetOnclik(ReplenishItem);
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
			for (int i= 0; i < 7; i++){
				int index = i;
				ui.ButtonDataFile[i].SetOnclik(delegate
				{
					SelectData = GetData(index);
					isShowImg = true;
					AreaTrue();
				});
				ui.ButtonDataPage[i].SetOnclik(delegate
				{

					atPage = index;
					PageTo();
				});
            }
		}
        if (ui != null && ui.isLoad)
		{
			FData.Data.Name=ui.SaveName.text;
			FData.Data.Name.Replace("\\", "").Replace("/", "").Replace("?", "").Replace("|", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("*", "").Replace("\"", "");
			ui.SaveName.text = FData.Data.Name;

			if (PastIngData != null)
			{
				if (PastIngData.error)
				{
					isShowItem = true;
					noItem = PastIngData.errorMsg;
					ui.TaskInfo.color = Color.red;
					info1 = "error!!!\n"+PastIngData.errorMsg;
					ui.TaskInfo.text= PastIngData.errorMsg;
					var tRect = ui.TaskInfo.GetComponent<RectTransform>();
					tRect.sizeDelta = new Vector2(300f, 400f);
					
				}
				else
				{
					ui.TaskInfo.color = Color.white;
					var tRect = ui.TaskInfo.GetComponent<RectTransform>();
					tRect.sizeDelta = new Vector2(102f, 330f);
					info1 = (confirmStop ? $"【{ST.确认强制停止}?】\n" : "") +
						(PastIngData.playerHaveBeltItem ? "" : $"【{ST.缺少传送带}!!!】\n") +
						(PastIngData.playerHaveInserterItem ? "" : $"【{ST.缺少爪子}!!!】\n") +
						$"【{PastIngData.Data.Name}】\n" +
						$"{ST.正在建造}\n{PastIngData.preIdMap.Count}\n" +
						$"{ST.建造完成}\n{PastIngData.eIdMapCount}\n" +
						$"{ST.等待爪子}\n{PastIngData.PreInserterDateCount}\n" +
						$"{ST.建造爪子}\n{PastIngData.preInserterMap.Count}\n" +
						$"{ST.传送带队列}\n{PastIngData.BeltQueueCount}\n" +
						$"{ST.等待物品}\n{PastIngData.WaitBuildCount}\n" +
						$"{ST.等待连接传送带}\n{PastIngData.BuildBeltData.Count}\n" +
						$"{ST.等待连接建筑}\n{PastIngData.NeedConnData.Count}\n";
				}
				if (!PastIngData.Working)
				{
					PastIngData = null;
				}
			}
			ui.TaskInfo.text = info1;

		}
		
	}
	void CopyData()
	{
		AreaTrue();
		var factory = GetFactory();
		if (factory != null)
		{
			FData.CopyData(factory);
			for (int i = 0; i < 30; i++)
			{
				StartCoroutine(FData.Data.CheckBelt((float)(0.1 + 0.1 * i)));
			}
			SelectData = FData.Data;
			info1 = ST.复制 + ST.成功 + ":" + SelectData.AllData.Count;
		}
	}


	void PasteData()
	{
		var player = GetPlayer();
		if (player != null && PastIngData == null)
		{
			FData.PasteDate(player, area);
			buildS = FData.buildS;
			buildF = FData.buildF;
			buildF1 = FData.buildF1;
			buildF2 = FData.buildF2;
			PastIngData = FData;
		}

	}
	void ClearData()
	{
		FData.ClearData();
		SelectData = null;
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
				DataList[index].CheckItem(player,out haveItem,out haveItemCount,out noItem,out noItemCount);
			}
            else
            {
				DataList[index].CheckItem(null, out haveItem, out haveItemCount, out noItem, out noItemCount);
            }
			isAreaSelect = false;
			SelectData = DataList[index];
			return DataList[index];
        }
		return null;
    }

	void SaveData()
	{
		if (FData.Data.Count > 0 && FData.Data.Name.Length > 0)
		{
			FData.Data.Export();
			DataList.Add(FData.Data);
			info1 = "保存在BepInEx\\config\\PlanetFactoryData";
			FData.NewData();

		}
	}

	void StopData()
    {
		if (!confirmStop)
		{
			confirmStop = true;
		}
		else
		{
			PastIngData = null;
			noItem = string.Empty;
			confirmStop = false;
			info1 = "";
		}
	}

	void ShowImg()
    {
		isShowImg = true;
    }

	void LocalImg()
    {
		SelectData = null;
		isShowImg = true;
	}

	void ReplenishItem()
    {
		if (PastIngData != null)
		{
			PastIngData.ReplenishItem();
			if (PastIngData.WaitBuildCount > 0)
			{
				isShowItem = true;
				noItem = PastIngData.GetWaitNeedItem;
				noItemCount = PastIngData.GetWaitItemDCount;
				haveItem = string.Empty;
				haveItemCount = 0;
			}
			else
			{
				isShowItem = false;
				noItem = string.Empty;
				noItemCount = 0;
			}
		}
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
				var f = Directory.GetFiles(path);
				foreach (var d in f)
				{
					FactoryData data = new FactoryData();
					filename = d.Split('\\').Last() ;
					data.Import(d);
					DataList.Add(data);
				}
				totalPage = DataList.Count / 10 + 1;
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
		int pageTotal = (DataList.Count / 7) + 1;
		int min = Math.Min(pageTotal, 7);
		for (int i = 0; i < min; i++)
		{
			ui.ButtonDataPage[i].SetActive(true);
		}
		atPage = 0;
		PageTo();
	}
	void PageTo()
	{
		int start = atPage * 7;
		int c = DataList.Count - start;
		if (c > 0)
		{
			int min = Math.Min(c, 7);
			int i = 0;
			for (; i < min; i++)
			{
				ui.ButtonDataFile[i].SetActive(true);
				ui.ButtonDataFile[i].text.text = GetData(i).Name;
			}
            for (; i < 7; i++)
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
					try
					{
						if (player.package.isFull)
						{
							UIRealtimeTip.Popup(ST.背包不足);
							break;
						}
						ItemProto itemProto = LDB.items.Select((int)factory.entityPool[i].protoId);
						int itemId = (itemProto == null) ? 0 : itemProto.ID;
						factory.DestructFinally(player, i, ref itemId);
						player.package.AddItemStacked(itemId, 1);
						UIItemup.Up(itemId, 1);
					}catch(Exception e)
                    {
						Debug.LogError(e.Message);
						Debug.LogError(e.StackTrace);
                    }
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
	void mywindowfunction(int windowid)
	{
		int h = ImgHeight;
		int w = ImgHeight * 2 + 1;
		Rect ImgRect = new Rect(10, 10,w,h);
		ImgX = (int)GUI.VerticalSlider(new Rect(10+w, 100, 20, 200), ImgX, 0, 180);
		ImgY = (int)GUI.HorizontalSlider(new Rect(300, 10+h, 200, 20), ImgY, 0, 180);
		if (SelectData != null)
			GUI.Label(ImgRect, SelectData.GetImg(ImgX, ImgY));
		else
			GUI.Label(ImgRect, localImg.GetImg(ImgX, ImgY, GetFactory()));

		
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
		}

		if (GetFactory() != null)
		{
			if (GUI.Button(new Rect(610, h+80, 120, 40), $"{ST.移除} {ST.当前星球}\n{ST.全部} {ST.建筑}"))
			{
				RemoverAllBuilding();
			}
		}
        if (CheckData()&&SelectData==null)
        {
			if(GUI.Button(new Rect(500, h+80, 100, 40), isAreaSelect ?   "取消": ST.区域选择)){
				isAreaSelect = !isAreaSelect;
            }
        }
        if (isAreaSelect)
        {
			recty1 = (int)GUI.VerticalSlider(new Rect(35+w, 10, 20, h), recty1, h, 0);
			recty2 = (int)GUI.VerticalSlider(new Rect(60+w, 10, 20, h), recty2, h, 0);
			rectx1 = (int)GUI.HorizontalSlider(new Rect(10, h+35, w, 20), rectx1, 0, w);
			rectx2 = (int)GUI.HorizontalSlider(new Rect(10, h+60, w, 20), rectx2, 0, w);
			GUI.Label(new Rect(10, 10, w, h), rectImg.getRect(rectx1, rectx2, recty1, recty2));
			var fd = GetFactory();
			GUI.Label(new Rect(10, h + 80, 200, 500), $"{ rectx1},{rectx2},{recty1},{recty2}");
			if(GUI.Button(new Rect(500, h + 130, 100, 40), ST.复制选定区域))
            {
				List<int> id = new List<int>();
				localImg.SelectBuild(fd, id, rectx1, rectx2, recty1, recty2);
				AreaTrue();
				FData.CopyData(fd,id);
				for (int i = 0; i < 30; i++)
				{
					StartCoroutine(FData.Data.CheckBelt((float)(0.1 + 0.1 * i)));
				}
				SelectData = FData.Data;
				FData.Data.GetImg(ImgX, ImgY);
				info1 = ST.复制 + ST.成功 + ":" + SelectData.AllData.Count;
			}
		}
		if (SelectData != null&&!isAreaSelect)
		{
			int buttonW = 160;
			int buttonH = 20;
			if (GUI.Button(new Rect(10, 430, buttonW, buttonH), ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Null);
			}
			if (GUI.Button(new Rect(10, 450, buttonW, buttonH), ST.赤道 + "(Y)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Y);
			}
			if (GUI.Button(new Rect(10, 470, buttonW, buttonH), ST.左右 + "(Z)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Z);
			}
			if (GUI.Button(new Rect(10, 490, buttonW, buttonH), ST.东西 + "(X)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.X);
			}
			if (GUI.Button(new Rect(10, 510, buttonW, buttonH), "XY " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XY);
			}
			if (GUI.Button(new Rect(10, 530, buttonW, buttonH), "XZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XZ);
			}
			if (GUI.Button(new Rect(10, 550, buttonW, buttonH), "YZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.YZ);
			}
			if (GUI.Button(new Rect(10, 570, buttonW, buttonH), "XYZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XYZ);
			}

			int bc = 1;
			if (GUI.Button(new Rect(180, 410+20 * bc++, 100, 20), ST.北 + ST.半球))
			{
				AreaFalse();
				area[0] = true;
				area[1] = true;
				area[4] = true;
				area[5] = true;
			}
			if (GUI.Button(new Rect(180, 410+20 * bc++, 100, 20), ST.南 + ST.半球))
			{
				AreaFalse();
				area[2] = true;
				area[3] = true;
				area[6] = true;
				area[7] = true;
			}
			if (GUI.Button(new Rect(180, 410+20 * bc++, 100, 20), ST.东 + ST.半球))
			{
				AreaFalse();
				area[0] = true;
				area[1] = true;
				area[2] = true;
				area[3] = true;
			}
			if (GUI.Button(new Rect(180, 410+20 * bc++, 100, 20), ST.西 + ST.半球))
			{
				AreaFalse();
				area[4] = true;
				area[5] = true;
				area[6] = true;
				area[7] = true;
			}
			if (GUI.Button(new Rect(180, 410+20 * bc++, 100, 20), ST.左 + ST.半球))
			{
				AreaFalse();
				area[1] = true;
				area[3] = true;
				area[5] = true;
				area[7] = true;
			}
			if (GUI.Button(new Rect(180, 410+20 * bc++, 100, 20), ST.右 + ST.半球))
			{
				AreaFalse();
				area[0] = true;
				area[2] = true;
				area[4] = true;
				area[6] = true;
			}
			bc = 1;
			area[0] = GUI.Toggle(new Rect(300, 420+20 * bc++, 100, 20), area[0], $"1:{ST.东},{ST.北},{ST.右}");
			area[1] = GUI.Toggle(new Rect(300, 420+20 * bc++, 100, 20), area[1], $"2:{ST.东},{ST.北},{ST.左}");
			area[2] = GUI.Toggle(new Rect(300, 420+20 * bc++, 100, 20), area[2], $"3:{ST.东},{ST.南},{ST.右}");
			area[3] = GUI.Toggle(new Rect(300, 420+20 * bc++, 100, 20), area[3], $"4:{ST.东},{ST.南},{ST.左}");
			bc = 1;								  
			area[4] = GUI.Toggle(new Rect(400, 420+20 * bc++, 100, 20), area[4], $"5:{ST.西},{ST.北},{ST.右}");
			area[5] = GUI.Toggle(new Rect(400, 420+20 * bc++, 100, 20), area[5], $"6:{ST.西},{ST.北},{ST.左}");
			area[6] = GUI.Toggle(new Rect(400, 420+20 * bc++, 100, 20), area[6], $"7:{ST.西},{ST.南},{ST.右}");
			area[7] = GUI.Toggle(new Rect(400, 420+20 * bc++, 100, 20), area[7], $"8:{ST.西},{ST.南},{ST.左}");
			if (!isAreaSelect)
			{
				GUI.Label(new Rect(840, 10, 250, haveItemCount * 16), haveItem, haveStyle);
				GUI.Label(new Rect(840, 30 + haveItemCount * 16, 250, noItemCount * 16), noItem, noStyle);
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

		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}



	private void PasteData(FactoryData data, ERotationType rotationType = ERotationType.Null)
	{
		var player = GetPlayer();
		if (player != null && PastIngData == null)
		{
			FactoryTask task = new FactoryTask(data);

			task.PasteDate(player, area, rotationType);
			buildS = task.buildS;
			buildF = task.buildF;
			buildF1 = task.buildF1;
			buildF2 = task.buildF2;
			info = data.Name + ST.粘贴 + ST.成功;
			PastIngData = task;
			if (PastIngData.WaitBuildCount > 0)
			{
				isShowItem = true;
				noItem = PastIngData.GetWaitNeedItem;
				noItemCount = PastIngData.GetWaitItemDCount;
				haveItem = string.Empty;
				haveItemCount = 0;
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
					if (PastIngData.preInserterMap.ContainsKey(prebuildId))
					{
						PastIngData.SetInserter(prebuildId, __result);
					}
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
		if (__instance != null && __instance.mainPlayer != null && __instance.mainPlayer.planetData != null)
		{
			PlanetData planetData = __instance.mainPlayer.planetData;
			if (planetData.factory != null)
			{
				int count = FData.Data.Count;
				info = $"{ST.复制建筑}：{count}\n{ST.成功}：{buildS} {ST.重叠}：{buildF1}" +
					(PastIngData != null?$"\n【-{ST.正在复制}-】\n":"")+
					(FData.Data.CheckBeltData.Count>0?$"\n无出货传送带{FData.Data.CheckBeltData.Count}":"")+
					$"\n暂时忽略地形碰撞检测\n";
			}
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
					var ejId = ce.splitterId;
					Buginfo = "eid:" + ce.id.ToString();
					Buginfo += "\npos:" + ed.pos;
					Buginfo += "\nrot:" + ed.rot;
					Buginfo += "\nsplitterId:" + ejId;
					Buginfo += "\nmodelIndex:" + ce.modelIndex;
					Buginfo += "\nassemblerId:" + ce.assemblerId;
					Buginfo += "\npowerGenId:" + ce.powerGenId;
					Buginfo += "\npowerNodeId:" + ce.powerNodeId;
					Buginfo += "\npowerExcId:" + ce.powerExcId;
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


	private static bool isShow = true;

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
	public static FactoryTask PastIngData = null;
	public static int buildS = 0;
	public static int buildF = 0;
	public static int buildF1 = 0;
	public static int buildF2 = 0;
	public static bool isShowItem = false;
	public static bool confirmStop = false;
	private static int totalPage = 0;
	private static int atPage = 0;
	private static string info1 = string.Empty;
	private static string info = string.Empty;
	private static FactoryTask FData = new FactoryTask();
	private static List<FactoryData> DataList = new List<FactoryData>();
	private static string Buginfo = string.Empty;
	private const float RECT_WEIDTH = 300f;
	private static Rect rect = new Rect(30f, 30f, 1000, 600);
	private static GUIStyle haveStyle = new GUIStyle();
	private static GUIStyle noStyle = new GUIStyle();
	private static int haveItemCount = 0;
	private static int noItemCount = 0;
	private static bool[] area = new bool[8];
	private static FactoryData SelectData = null;
	private static string haveItem = string.Empty;
	private static string noItem = string.Empty;
}

