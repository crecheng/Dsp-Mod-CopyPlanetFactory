using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("crecheng.CopyPlanetFactory", "CopyPlanetFactory", "1.4.0")]
public class CopyPlanetFactory : BaseUnityPlugin
{
	void Start()
	{
		Harmony.CreateAndPatchAll(typeof(CopyPlanetFactory), null);
		readFile();
		haveStyle.fontSize = 15;
		noStyle.fontSize = 15;
		haveStyle.normal.textColor = new Color(255f, 255f, 255f);
		noStyle.normal.textColor = new Color(255f/256f, 77f / 256f, 77f / 256f);
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
					PlanetFactoryData data = new PlanetFactoryData();
					filename = d.Split('\\').Last() ;
					data.Import(d);
					PList.Add(data);
				}
				totalPage = PList.Count / 10 + 1;
			}
		}catch(Exception e)
        {
			Debug.LogError("readFileError");
			Debug.LogError(e.Message);
			PlanetFactoryData data = new PlanetFactoryData();
			data.Name = filename + ST.导入错误+"！";
			PList.Add(data);
		}
	}

	void OnGUI()
	{

		rect = GUI.Window(1935598199, rect, mywindowfunction, "星球蓝图");

		//GUI.Label(new Rect(100, 100, 300, 700), Buginfo);
		if (isShow)
		{
			if (isShowItem)
			{
				GUI.Label(new Rect(rect.x - 250, rect.y, 250, haveItemCount * 16), haveItem, haveStyle);
				GUI.Label(new Rect(rect.x - 250, rect.y + haveItemCount * 16, 250, haveItemCount * 16), noItem, noStyle);
				if (GUI.Button(new Rect(rect.x - 20, rect.y, 20, 20), "X"))
					isShowItem = false;
			}
			if (PastIngData != null)
			{
				info1 = (confirmStop ? $"【{ST.确认强制停止}?】\n" : "") +
					$"{ST.正在建造}：{PastIngData.preIdMap.Count}\n" +
					$"{ST.建造完成}：{PastIngData.eIdMapCount}\n" +
					$"{ST.等待爪子}：{PastIngData.PreInserterDateCount}\n" +
					$"{ST.建造爪子}：{PastIngData.preInserterMap.Count}\n" +
					$"{ST.传送带队列}：{PastIngData.BeltQueueCount}\n" +
					$"{ST.等待补充}:{PastIngData.WaitBuildCount}\n";
				if (!PastIngData.Working)
				{
					PastIngData = null;
				}
			}
		}
	}

	void mywindowfunction(int windowid)
	{
		if (GUI.Button(new Rect(10, 20, 50, 20), ST.复制))
		{
			AreaTrue();
			var factory = GetFactory();
			if (factory != null)
			{
				FData.CopyPlanetFactoryDate(factory);
				rect.width = 460f;
				SelectData = FData;
			}
		}
		if (GUI.Button(new Rect(rect.width - 20, 0, 20, 20), "X"))
		{
			isShow = !isShow;
			if (isShow)
			{
				rect.width = 300;
				rect.height = 420;
			}
			else
			{
				rect.width = 30;
				rect.height = 20;
			}
		}

		if (GUI.Button(new Rect(70, 20, 50, 20), ST.粘贴))
		{
			var player = GetPlayer();
			if (player != null && PastIngData == null)
			{
				FData.PastePlanetFactoryDate(player,area);
				buildS = FData.buildS;
				buildF = FData.buildF;
				buildF1 = FData.buildF1;
				buildF2 = FData.buildF2;
				PastIngData = FData;
			}
		}
		if (GUI.Button(new Rect(130, 20, 50, 20), ST.清空))
		{
			FData.ClearData();
			SelectData = null;
			rect.width = RECT_WEIDTH;
			AreaFalse();
		}

		if (GUI.Button(new Rect(185, 20, 100, 20), ST.强制停止))
		{
			if (!confirmStop)
			{
				confirmStop = true;
			}
			else
			{
				PastIngData = null;
				confirmStop = false;
				info1 = "";
			}
		}
		if (GUI.Button(new Rect(185, 45, 100, 20), ST.补充物品))
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
			}
		}

		if (SelectData != null)
		{
			int buttonW = 160;
			int buttonH = 20;
			if (GUI.Button(new Rect(290, 20, buttonW, buttonH), ST.粘贴))
			{
				PasteData(SelectData);
			}
			if (GUI.Button(new Rect(290, 40, buttonW, buttonH), ST.赤道 + "(Y)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Y);
			}
			if (GUI.Button(new Rect(290, 60, buttonW, buttonH), ST.左右 + "(Z)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.Z);
			}
			if (GUI.Button(new Rect(290, 80, buttonW, buttonH), ST.东西 + "(X)" + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.X);
			}
			if (GUI.Button(new Rect(290, 100, buttonW, buttonH), "XY " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XY);
			}
			if (GUI.Button(new Rect(290, 120, buttonW, buttonH), "XZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XZ);
			}
			if (GUI.Button(new Rect(290, 140, buttonW, buttonH), "YZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.YZ);
			}
			if (GUI.Button(new Rect(290, 160, buttonW, buttonH), "XYZ " + ST.镜像 + ST.粘贴))
			{
				PasteData(SelectData, ERotationType.XYZ);
			}
			if (GUI.Button(new Rect(290, 200, buttonW, buttonH), ST.区域选择))
			{
				isShowMore = !isShowMore;
                if (isShowMore)
                {
					rect.width = 560f;
                }
                else
                {
					rect.width = 460f;
				}
			}
			if (isShowMore)
			{
				int bc = 1;
				if (GUI.Button(new Rect(455, 20 * bc++, 100, 20), ST.北 + ST.半球))
				{
					AreaFalse();
					area[0] = true;
					area[1] = true;
					area[4] = true;
					area[5] = true;
				}
				if (GUI.Button(new Rect(455, 20 * bc++, 100, 20), ST.南 + ST.半球))
				{
					AreaFalse();
					area[2] = true;
					area[3] = true;
					area[6] = true;
					area[7] = true;
				}
				if (GUI.Button(new Rect(455, 20 * bc++, 100, 20), ST.东 + ST.半球))
				{
					AreaFalse();
					area[0] = true;
					area[1] = true;
					area[2] = true;
					area[3] = true;
				}
				if (GUI.Button(new Rect(455, 20 * bc++, 100, 20), ST.西 + ST.半球))
				{
					AreaFalse();
					area[4] = true;
					area[5] = true;
					area[6] = true;
					area[7] = true;
				}
				if (GUI.Button(new Rect(455, 20 * bc++, 100, 20), ST.左 + ST.半球))
				{
					AreaFalse();
					area[1] = true;
					area[3] = true;
					area[5] = true;
					area[7] = true;
				}
				if (GUI.Button(new Rect(455, 20 * bc++, 100, 20), ST.右 + ST.半球))
				{
					AreaFalse();
					area[0] = true;
					area[2] = true;
					area[4] = true;
					area[6] = true;
				}
				area[0] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[0], "1:+X,+Y,+Z");
				area[1] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[1], "2:+X,+Y, -Z");
				area[2] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[2], "3:+X, -Y,+Z");
				area[3] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[3], "4:+X, -Y, -Z");
				area[4] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[4], "5: -X,+Y,+Z");
				area[5] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[5], "6: -X,+Y, -Z");
				area[6] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[6], "7: -X, -Y,+Z");
				area[7] = GUI.Toggle(new Rect(455, 20 * bc++, 100, 20), area[7], "8: -X, -Y, -Z");
			}
		}

		FData.Name = GUI.TextArea(new Rect(10, 45, 100, 20), FData.Name);
		FData.Name.Replace("\\", "").Replace("/", "").Replace("?", "").Replace("|", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("*", "").Replace("\"", "");
		if (GUI.Button(new Rect(130, 45, 50, 20), ST.保存))
		{
			if (FData.Count > 0 && FData.Name.Length > 0)
			{
				FData.Export();
				PList.Add(FData);
				info1 = "保存在BepInEx\\config\\PlanetFactoryData";
				FData = new PlanetFactoryData();

			}
		}
		GUI.Label(new Rect(10, 70, 200, 100), info);
		GUI.Label(new Rect(190, 70, 125, 400), info1);
		int j = 0;
		for (int i = atPage * 10; i < Math.Min(PList.Count, (atPage + 1) * 10); i++)
		{
			var d = PList[i];
			if (GUI.Button(new Rect(10, 155 + j * 23, 120, 20), d.Name))
			{
				if (PlanetFactoryData.CheckData())
				{
					d.CheckItem(GameMain.mainPlayer, out haveItem, out haveItemCount, out noItem, out noItemCount);
					isShowItem = true;
				}
			}
			if (GUI.Button(new Rect(130, 155 + j * 23, 50, 20), ST.选定))
			{
				SelectData = d;
				AreaTrue();
				rect.width = 460f;
			}
			j++;
		}
		for (int i = 0; i < totalPage; i++)
		{
			if (GUI.Button(new Rect(10 + i * 22, 390, 20, 20), "" + (i + 1)))
			{
				atPage = i;
			}
		}

		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	private void PasteData(PlanetFactoryData data,ERotationType rotationType=ERotationType.Null)
    {
		var player = GetPlayer();
		if (player != null && PastIngData == null)
		{
			data.PastePlanetFactoryDate(player,area, rotationType);
			buildS = data.buildS;
			buildF = data.buildF;
			buildF1 = data.buildF1;
			buildF2 = data.buildF2;
			info = data.Name + ST.粘贴 + ST.成功;
			PastIngData = data;
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
				int count = FData.Count;
				info = $"{ST.复制建筑}：{count}\n{ST.成功}：{buildS}  {ST.跳过}：{buildF}\n{ST.重叠}：{buildF1} {ST.缺物}：{buildF2}" +
					(PastIngData != null?$"\n【-{ST.正在复制}-】\n":"")+
					$"\n暂时忽略地形碰撞检测\n";
			}
		}
        if (CheckData())
        {
			Player player = GameMain.data.mainPlayer;
            if (player.controller != null && player.controller.cmd.raycast != null)
            {
				var ce = player.controller.cmd.raycast.castEntity;
				int inid = ce.beltId;
				var ed = player.factory.entityPool[ce.id];
				var bd = player.factory.cargoTraffic.beltPool[inid];
                Buginfo = "eid:"+ce.id.ToString();
				Buginfo += "\npos:" + ed.pos;
				Buginfo += "\nrot:" + ed.rot;
				Buginfo += "\nrot.eulerAngles:" + ed.rot.eulerAngles;
                if (ce.inserterId > 0)
                {
					var d = player.factory.factorySystem.inserterPool[ce.inserterId];
					Buginfo += "\npos2:" + d.pos2;
					Buginfo += "\nrot2:" + d.rot2;
				}
				//Buginfo += "\ninsertOffset:" + bd.backInputId;
				//Buginfo += "\noutputId:" + bd.outputId;
				//Buginfo += "\nleftInputId:" + bd.leftInputId;
				//Buginfo += "\nrightInputId:" + bd.rightInputId;
				//Buginfo += "\nsegIndex:" + bd.segIndex;
				//Buginfo += "\nsegLength:" + bd.segLength;
				//Buginfo += "\nsegPathId:" + bd.segPathId;
				//Buginfo += "\nsegPivotOffset:" + bd.segPivotOffset;
				//Buginfo += "\ncoon:";
				//if (ce.id >0)
				//{
				//	for(int i = 0; i < 16; i++)
				//    {
				//		int conn = player.planetData.factory.entityConnPool[ce.id * 16 + i];
				//		player.planetData.factory.ReadObjectConn(ce.id, i, out bool isO, out int other, out int slot);
				//		Buginfo +=i+":"+ conn + "," + isO + "," + other + "," + slot+"\n";
				//    }
				//}
				//if (PastIngData != null)
				//{
				//	Buginfo += "\n"+PastIngData.working;
				//	Buginfo += "\n"+PastIngData.preIdMap.Count;
				//	Buginfo += "\n"+PastIngData.BuildBeltData.Count;
				//}

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
		return PlanetFactoryData.CheckData();
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
	public static PlanetFactoryData PastIngData = null;
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
	private static PlanetFactoryData FData = new PlanetFactoryData();
	private static List<PlanetFactoryData> PList = new List<PlanetFactoryData>();
	private static string Buginfo = string.Empty;
	private const float RECT_WEIDTH = 300f;
	private static Rect rect = new Rect(330f, 30f, RECT_WEIDTH, 420f);
	private static GUIStyle haveStyle = new GUIStyle();
	private static GUIStyle noStyle = new GUIStyle();
	private static int haveItemCount = 0;
	private static int noItemCount = 0;
	private static bool[] area = new bool[8];
	private static PlanetFactoryData SelectData = null;
	private static string haveItem = string.Empty;
	private static string noItem = string.Empty;
	private static bool isShowMore = false;
}

