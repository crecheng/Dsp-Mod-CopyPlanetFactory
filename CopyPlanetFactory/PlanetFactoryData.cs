using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

public class PlanetFactoryData
{
	/// <summary>
	/// 工作台数据
	/// </summary>
	private List<MyPrebuildData> AssemblerDate;
	/// <summary>
	/// 爪子数据
	/// </summary>
	private List<MyPrebuildData> InserterData;
	/// <summary>
	/// 传送带数据
	/// </summary>
	private List<MyPrebuildData> BeltData;
	/// <summary>.
	/// 发电机数据
	/// </summary>
	private List<MyPrebuildData> PowerData;
	/// <summary>
	/// 电线杆数据
	/// </summary>
	private List<MyPrebuildData> PowerNodeData;
	///等待爪子数据
	/// <summary>
	/// </summary>
	private List<MyPrebuildData> PreInserterData;
	/// <summary>
	/// 查找重叠数据-模糊
	/// </summary>
	private Dictionary<int, int> posSet;
	/// <summary>
	/// 查找重叠数据-精准
	/// </summary>
	private Dictionary<long, int> floatPosSet;
	/// <summary>
	/// 是否重复复制数据
	/// </summary>
	private HashSet<int> eidSet;
	/// <summary>
	/// old EId 与new EId的映射
	/// </summary>
	private Dictionary<int, int> eIdMap;
	/// <summary>
	/// 传送带的id映射
	/// </summary>
	private Dictionary<int, int> BeltEIdMap;
	/// <summary>
	/// 等待背包补充物品数据
	/// </summary>
	private List<MyPrebuildData> WaitItemBuild;
	/// <summary>
	/// 等待补充的物品
	/// </summary>
	private Dictionary<int, int> WaitNeedItem;
	/// <summary>
	/// 预建筑数据
	/// </summary>
	public Dictionary<int, MyPrebuildData> preIdMap;
	/// <summary>
	/// 建筑完成的传送带数据，等待连接
	/// </summary>
	public List<MyPrebuildData> BuildBeltData;
	/// <summary>
	/// 建筑完成的爪子数据，等待连接
	/// </summary>
	public Dictionary<int, MyPrebuildData> preInserterMap;
	/// <summary>
	/// 需求物品
	/// </summary>
	public Dictionary<int, int> ItemNeed;
	/// <summary>
	/// 传送带队列，不出现红字
	/// </summary>
	private Queue<MyPrebuildData> BeltQueue;
	/// <summary>
	/// 已经加入预建造的传送带数量
	/// </summary>
	private int BeltCount = 0;

	public bool playerHaveBeltItem { private set; get;  }
	public bool playerHaveInserterItem { private set; get;  }
	/// <summary>
	/// 文件名
	/// </summary>
	string path;
	public int buildS = 0;
	public int buildF = 0;
	public int buildF1 = 0;
	public int buildF2 = 0;
	public string Name;
	public PlanetFactory planetFactory;
	public Player player;
	public bool haveInserter;
	public bool haveBelt;
	private string itemNeedString;
	private int itemNeedCount;
	private bool isInitItem;
	private ERotationType RotationType;
	private bool[] area;
	public int WaitBuildCount
	{
		get
		{
			return WaitItemBuild.Count;
		}
	}

	public int GetWaitItemDCount
    {
        get
        {
			return WaitNeedItem.Count;
        }
    }
	public int Count
	{
		get
		{
			return AssemblerDate.Count + InserterData.Count + BeltData.Count + PowerData.Count + PowerNodeData.Count;
		}
	}

	public int eidSetCount
	{
		get
		{
			return eidSet.Count;
		}
	}
	public int eIdMapCount
	{
		get
		{
			return eIdMap.Count;
		}
	}

	public int PreInserterDateCount
	{
		get
		{
			return PreInserterData.Count;
		}
	}

	/// <summary>
	/// 是否正在建造
	/// </summary>
	public bool Working
	{
		get
		{
			return haveInserter || haveBelt || WaitBuildCount > 0;
		}
	}

	public string GetWaitNeedItem { get; private set; }

	public int BeltQueueCount
    {
        get
        {
			return BeltQueue.Count;
        }
    }

	public string GetItemNeed
    {
        get
        {
			if (itemNeedString == string.Empty && ItemNeed.Count == 0)
				return string.Empty;
			if (itemNeedCount == ItemNeed.Count)
				return itemNeedString;
			else if (ItemNeed.Count > 0)
			{
				string s = string.Empty;
				foreach (var d in ItemNeed)
				{
					string itemName = LDB.items.Select(d.Key).name;
					s += itemName + "X " + d.Value + "\n";
				}
				itemNeedString = s;
				itemNeedCount = ItemNeed.Count;
				return itemNeedString;
			}
			else
				return string.Empty;

		}
    }

	public PlanetFactoryData(PlanetFactory planetFactory)
	{
		Init();
		this.planetFactory = planetFactory;
		CopyPlanetFactoryDate(planetFactory);
	}

	public PlanetFactoryData()
	{
		Init();
	}

	public void Init()
	{
		path = System.Environment.CurrentDirectory + "\\BepInEx\\config\\PlanetFactoryData";
		AssemblerDate = new List<MyPrebuildData>();
		InserterData = new List<MyPrebuildData>();
		BeltData = new List<MyPrebuildData>();
		PowerData = new List<MyPrebuildData>();
		PowerNodeData = new List<MyPrebuildData>();
		PreInserterData = new List<MyPrebuildData>();
		eIdMap = new Dictionary<int, int>();
		posSet = new Dictionary<int, int>();
		floatPosSet = new Dictionary<long, int>();
		eidSet = new HashSet<int>();
		preIdMap = new Dictionary<int, MyPrebuildData>();
		BuildBeltData = new List<MyPrebuildData>();
		preInserterMap = new Dictionary<int, MyPrebuildData>();
		BeltEIdMap = new Dictionary<int, int>();
		ItemNeed = new Dictionary<int, int>();
		itemNeedString = string.Empty;
		BeltQueue = new Queue<MyPrebuildData>();
		WaitNeedItem = new Dictionary<int, int>();
		WaitItemBuild = new List<MyPrebuildData>();
		buildS = 0;
		buildF = 0;
		buildF1 = 0;
		buildF2 = 0;
		Name = String.Empty;
		BeltCount = 0;
		isInitItem = false;
		RotationType = ERotationType.Null;
		playerHaveBeltItem = true;
	}

	public void ClearData()
	{
		AssemblerDate.Clear();
		InserterData.Clear();
		BeltData.Clear();
		PowerData.Clear();
		PowerNodeData.Clear();
		isInitItem = false;
		itemNeedCount = 0;
		itemNeedString = string.Empty;
		ItemNeed.Clear();
		PasteClear();
	}

	public void PasteClear()
	{
		PreInserterData.Clear();
		preIdMap.Clear();
		posSet.Clear();
		floatPosSet.Clear();
		eidSet.Clear();
		eIdMap.Clear();
		BuildBeltData.Clear();
		preInserterMap.Clear();
		BeltEIdMap.Clear();
		BeltQueue.Clear();
		WaitNeedItem.Clear();
		WaitItemBuild.Clear();
		GetWaitNeedItem = string.Empty;
		buildS = 0;
		buildF = 0;
		buildF1 = 0;
		buildF2 = 0;
		BeltCount = 0;
		RotationType = ERotationType.Null;
		playerHaveBeltItem = true;
	}

	/// <summary>
	/// 添加等待补充的物品
	/// </summary>
	/// <param name="itemId"></param>
	public void AddWaitNeedIiem(int itemId)
    {
        if (itemId > 0)
        {
			if (WaitNeedItem.ContainsKey(itemId))
				WaitNeedItem[itemId]++;
			else
				WaitNeedItem.Add(itemId, 1);
        }
    }

	private void SetWaitItemString()
    {
		StringBuilder sb = new StringBuilder();
		foreach (var d in WaitNeedItem)
		{

			var item = LDB.items.Select(d.Key);
			if (item != null)
			{
				sb.Append($"{item.name}【{d.Value}】\n");
			}
			else
			{
				sb.Append($"{d.Key}【{d.Value}】\n");
			}
		}
		GetWaitNeedItem = sb.ToString() ;
	}
	/// <summary>
	/// 添加需求物品
	/// </summary>
	/// <param name="itemId">物品id</param>
	public void AddItemCount(int itemId)
    {
		if (itemId > 0)
		{
			if (ItemNeed.ContainsKey(itemId))
			{
				ItemNeed[itemId]++;
			}
			else
			{
				ItemNeed.Add(itemId, 1);
			}
		}
    }

	public void ReplenishItem()
	{
		if (CheckData())
		{
			if (GameMain.mainPlayer.factory == planetFactory)
			{
				if (WaitItemBuild.Count > 0||BeltQueue.Count>0||PreInserterData.Count>0)
				{
					var player = GameMain.mainPlayer;
					playerHaveBeltItem = true;
					playerHaveInserterItem = true;
					List<MyPrebuildData> temp = new List<MyPrebuildData>();
					temp.AddRange(WaitItemBuild);
					
					WaitItemBuild.Clear();
					WaitNeedItem.Clear();
					//Debug.Log(WaitItemBuild.Count);
					foreach (var d in temp)
					{
						var dd = new MyPrebuildData(d);
						if (dd.isBelt)
						{
							if (BeltCount < 1000)
							{
								AddPrebuildData(player, dd, out int pid, true);
								//Debug.Log(pid);
								if (pid > 0)
								{
									haveBelt = true;
									dd.preId = pid;
									preIdMap.Add(pid, dd);
								}
								BeltCount++;
							}
							else
							{
								BeltQueue.Enqueue(dd);
							}
						}
						else if (dd.isInserter)
						{
							PreInserterData.Add(dd);
						}
						else
						{
							AddPrebuildData(player, dd, out int pid);
							if (pid > 0)
							{
								dd.preId = pid;
								preIdMap.Add(pid, dd);
							}
						}
					}
					TryBuildInserter();
					SetWaitItemString();
				}
			}
		}
	}
	/// <summary>
	/// 导入需求物品数量
	/// </summary>
	/// <param name="data"></param>
	public void AddItemCount(string data)
	{
		string[] s = data.Split(',');
		foreach(var d in s)
        {
			string[] tmp = d.Split(':');
            if (tmp.Length > 1)
            {
				ItemNeed.Add(int.Parse(tmp[0]),int.Parse(tmp[1]));
            }
        }
	}

	/// <summary>
	/// 导出需求物品
	/// </summary>
	/// <param name="isFile"></param>
	/// <returns></returns>
	public string GetItemCountData(bool isFile=false)
    {
		string s=string.Empty;
		foreach(var d in ItemNeed)
        {
			s += d.Key + ":" + d.Value;
			s += isFile ? "," : "\n";
        }
		return s;
    }

	/// <summary>
	/// 重检需求物品
	/// </summary>
	public void InitItemNeed()
    {
		ItemNeed.Clear();
		foreach(var d in AssemblerDate)
        {
			AddItemCount(d.ProtoId);
        }
		foreach (var d in PowerData)
		{
			AddItemCount(d.ProtoId);
		}
		foreach (var d in PowerNodeData)
		{
			AddItemCount(d.ProtoId);
		}
		foreach (var d in InserterData)
		{
			AddItemCount(d.ProtoId);
		}
		foreach (var d in BeltData)
		{
			AddItemCount(d.ProtoId);
		}
		isInitItem = true;
	}

	/// <summary>
	/// 清除0元素
	/// </summary>
	public void Remove0Item()
    {
		List<MyPrebuildData> temp = new List<MyPrebuildData>();
		foreach (var d in AssemblerDate)
		{
			if (d.ProtoId == 0)
				temp.Add(d);
		}
		foreach(var d in temp)
        {
			AssemblerDate.Remove(d);
        }
		temp.Clear();

		foreach (var d in PowerData)
		{
			if (d.ProtoId == 0)
				temp.Add(d);
		}
		foreach (var d in temp)
		{
			AssemblerDate.Remove(d);
		}
		temp.Clear();

		foreach (var d in PowerNodeData)
		{
			if (d.ProtoId == 0)
				temp.Add(d);
		}
		foreach (var d in temp)
		{
			AssemblerDate.Remove(d);
		}
		temp.Clear();

		foreach (var d in InserterData)
		{
			if (d.ProtoId == 0)
				temp.Add(d);
		}
		foreach (var d in temp)
		{
			AssemblerDate.Remove(d);
		}
		temp.Clear();

		foreach (var d in BeltData)
		{
			if (d.ProtoId == 0)
				temp.Add(d);
		}
		foreach (var d in temp)
		{
			AssemblerDate.Remove(d);
		}
		temp.Clear();
		Export();
	}

	/// <summary>
	/// 检查玩家物品
	/// </summary>
	/// <param name="p">玩家</param>
	/// <param name="haveNeedItem">已有需求物品</param>
	/// <param name="count1">已有需求物品条</param>
	/// <param name="noNeeditem">未有需求物品</param>
	/// <param name="count2">未有需求物品条</param>
	public void CheckItem(Player p ,out string haveNeedItem,out int count1,out string noNeeditem,out int count2)
    {
		haveNeedItem = string.Empty;
		count1 = 0;
		noNeeditem = string.Empty;
		count2 = 0;
        if (CheckData())
        {
			if(!isInitItem)
				InitItemNeed();
			if(ItemNeed.ContainsKey(0))
            {
				ItemNeed.Remove(0);
				Remove0Item();
            }
			foreach(var d in ItemNeed)
            {
                if (p.package.GetItemCount(d.Key) >= d.Value)
                {
					count1++;
					var item = LDB.items.Select(d.Key);
					haveNeedItem += $"{item.name}【{d.Value}】\n";
					//Debug.Log($"【{d.Key}】");
                }
                else
                {
					count2++;
					//Debug.Log($"({ d.Key})");
					var item= LDB.items.Select(d.Key);
                    if (item != null)
                    {
						noNeeditem += $"{item.name}【{d.Value}】\n";
                    }
                    else
                    {
						noNeeditem += $"{d.Key}【{d.Value}】\n";
					}
				}
            }
        }
    }

	public long GetPosLong(Vector3 p)
    {
		long x = (long)(p.x * 100);
		long y = (long)(p.y * 100);
		long z = (long)(p.z * 100);
		return x * 10000000000 + y * 100000 + z;
    }
	/// <summary>
	/// 复制建筑
	/// </summary>
	/// <param name="factory">工厂数据</param>
	public void CopyPlanetFactoryDate(PlanetFactory factory)
	{
		if (CheckData())
		{
			planetFactory = factory;
			var fSystem = factory.factorySystem;
			ClearData();
			for (int i = 1; i < fSystem.assemblerCursor; i++)
			{
				var ap = fSystem.assemblerPool[i];
				var eId = ap.entityId;
				if (eidSet.Contains(eId))
					continue;
				eidSet.Add(eId);
				var ed = factory.entityPool[eId];
				if (ed.protoId == 0)
					continue;
				var temp = new MyPrebuildData(GetPreDate(ap, ed), 0);
				temp.oldEId = eId;
				AssemblerDate.Add(temp);
				AddItemCount(ed.protoId);
			}

			for(int i = 1; i < fSystem.ejectorCursor; i++)
            {
				var ap = fSystem.ejectorPool[i];
				var eId = ap.entityId;
				if (eidSet.Contains(eId))
					continue;
				eidSet.Add(eId);
				var ed = factory.entityPool[eId];
				if (ed.protoId == 0)
					continue;
				var temp = new MyPrebuildData(GetPreDate(ap, ed), 0);
				temp.oldEId = eId;
				AssemblerDate.Add(temp);
				AddItemCount(ed.protoId);
			}
			for (int i = 1; i < fSystem.siloCursor; i++)
			{
				var ap = fSystem.siloPool[i];
				var eId = ap.entityId;
				if (eidSet.Contains(eId))
					continue;
				eidSet.Add(eId);
				var ed = factory.entityPool[eId];
				if (ed.protoId == 0)
					continue;
				var temp = new MyPrebuildData(GetPreDate(ap, ed), 0);
				temp.oldEId = eId;
				AssemblerDate.Add(temp);
				AddItemCount(ed.protoId);
			}
			for (int i = 1; i < factory.powerSystem.genCursor; i++)
			{
				var ap = factory.powerSystem.genPool[i];
				var eId = ap.entityId;
				if (eidSet.Contains(eId))
					continue;
				eidSet.Add(eId);
				var ed = factory.entityPool[eId];
				if (ed.protoId == 0)
					continue;
				var temp = new MyPrebuildData(GetPreDate(ap, ed), 0);
				temp.oldEId = eId;
				PowerData.Add(temp);
				AddItemCount(ed.protoId);
			}
			for (int i = 1; i < factory.powerSystem.nodeCursor; i++)
			{
				var ap = factory.powerSystem.nodePool[i];
				var eId = ap.entityId;
				if (eidSet.Contains(eId))
					continue;
				eidSet.Add(eId);
				var ed = factory.entityPool[eId];
				if (ed.protoId == 0)
					continue;
				var temp = new MyPrebuildData(GetPreDate(ap, ed), 0);
				temp.oldEId = eId;
				PowerNodeData.Add(temp);
				AddItemCount(ed.protoId);
			}
			for (int i = 1; i < fSystem.inserterCursor; i++)
			{
				var ip = fSystem.inserterPool[i];
				var eid = ip.entityId;
				var ed = factory.entityPool[eid];
				if (ed.protoId == 0)
					continue;
				InserterComponent temp = ip;
				var target = factory.entityPool[temp.insertTarget];
				var pick = factory.entityPool[temp.pickTarget];
				var tempP = new MyPrebuildData(GetPreDate(temp, ed), temp);
				tempP.outConn = planetFactory.entityConnPool[eid * 16];
				tempP.inConn = planetFactory.entityConnPool[eid * 16+1];
				InserterData.Add(tempP);
				AddItemCount(ed.protoId);
			}
			for (int i = 1; i < factory.cargoTraffic.beltCursor; i++)
			{
				var ap = factory.cargoTraffic.beltPool[i];
				var eId = ap.entityId;
				var ed = factory.entityPool[eId];
				if (ed.protoId == 0)
					continue;
				bool flag2;
				int slot;
				factory.ReadObjectConn(eId, 0, out flag2, out int out1, out slot);
				int slot2;
				factory.ReadObjectConn(eId, 1, out flag2, out int in1, out slot2);
				int num5;
				factory.ReadObjectConn(eId, 2, out flag2, out int in2, out num5);
				factory.ReadObjectConn(eId, 3, out flag2, out int in3, out num5);
				var temp = new MyPrebuildData(GetPreDate(ap, ed),ap,out1,in1,in2,in3);
				//Debug.Log($"{eId},{out1},{in1},{in2},{in3}");
				temp.oldEId = eId;
				BeltData.Add(temp);
				AddItemCount(ed.protoId);

			}

		}
	}


	public void AddPasteData(Player player1,MyPrebuildData d)
    {
		if (IsInArea(d.Pos, area))
		{
			//复制一份数据
			var dd = new MyPrebuildData(d);
			//加入预建造数据
			AddPrebuildData(player1, dd, out int pid);
			//如果加入成功
			if (pid > 0)
			{
				dd.preId = pid;
				//加入预建造数据
				preIdMap.Add(pid, dd);
			}
		}
	}

	static bool haveAddPasteData = false;
	/// <summary>
	/// 粘贴建筑
	/// </summary>
	/// <param name="player1">玩家</param>
	public void PastePlanetFactoryDate(Player player1 ,bool[] SelectArea,ERotationType rotationType=ERotationType.Null)
	{
        if (haveAddPasteData)
        {
			return;
        }
        else
        {
			haveAddPasteData = true;
        }
		if (CheckData())
		{
			player = player1;
			if (player.planetData.type == EPlanetType.Gas)
				return;
			var factory = player.planetData.factory;
			planetFactory = factory;
			//粘贴前清空数据
			PasteClear();
			RotationType = rotationType;
			area = SelectArea;
			//复制当前星球实体位置数据，用于进行重叠检测
			for (int i = 1; i < factory.entityCursor; i++)
			{
				var pos = factory.entityPool[i].pos;
				float x = ((int)(pos.x * 10)) / 10f;
				float y = ((int)(pos.y * 10)) / 10f;
				float z = ((int)(pos.z * 10)) / 10f;
				try
				{
					int p = (int)(x * 1000000 + y * 1000 + z);
					if(!posSet.ContainsKey(p))
						posSet.Add(p, i);
					long lp = GetPosLong(pos);
					if(!floatPosSet.ContainsKey(lp))
						floatPosSet.Add(lp, i);
                }
                catch(Exception e)
                {
					Debug.LogError("AddPosSetError");
					Debug.LogError(e.Message);
				}
			}
			//粘贴工作台
			foreach (var d in AssemblerDate)
			{
				AddPasteData(player1, d);
			}
			///粘贴发电机
			foreach (var d in PowerData)
			{
				AddPasteData(player1, d);
			}
			//粘贴电线杆数据
			foreach (var d in PowerNodeData)
			{
				AddPasteData(player1, d);
			}
			//粘贴传送带数据
			foreach (var d in BeltData)
			{
				if (IsInArea(d.Pos, SelectArea))
				{
					var dd = new MyPrebuildData(d);
					if (BeltCount < 1000)
					{
						AddPrebuildData(player, dd, out int pid, true);
						//Debug.Log(pid);
						if (pid > 0)
						{
							haveBelt = true;
							dd.preId = pid;
							preIdMap.Add(pid, dd);
						}
						BeltCount++;
					}
					else
					{
						BeltQueue.Enqueue(dd);
					}
				}
			}

			if (InserterData.Count > 0)
				haveInserter = true;
			//将爪子数据加入备选列表
			foreach (var d in InserterData)
			{
				if (IsInArea(d.Pos, SelectArea))
				{
					var dd = new MyPrebuildData(d);
					PreInserterData.Add(dd);
				}
			}
			if (preIdMap.Count == 0)
			{
				TryBuildInserter();
			}

			SetWaitItemString();
		}
		haveAddPasteData = false;
	}


	/// <summary>
	/// 传送带队列出队
	/// </summary>
	public void BeltQueueDequeue()
    {
		if (BeltQueue.Count > 0&&playerHaveBeltItem)
		{
			BeltCount--;
			var dd = BeltQueue.Dequeue();
			AddPrebuildData(player, dd, out int pid, true);
			//Debug.Log(pid);
			if (pid > 0)
			{
				BeltCount++;
				haveBelt = true;
				dd.preId = pid;
				preIdMap.Add(pid, dd);
			}
		}
	}

	/// <summary>
	/// 设置爪子连接数据
	/// </summary>
	/// <param name="preId">预建筑id</param>
	/// <param name="eid">eID</param>
	public void SetInserter(int preId, int eid)
	{
		//如果有预建造爪子数据
		if (preInserterMap.ContainsKey(preId))
		{
			MyPrebuildData pd = preInserterMap[preId];
			//获取游戏数据
			int inserterId = player.planetData.factory.entityPool[eid].inserterId;
			var fs = player.planetData.factory.factorySystem;
			int target = pd.inserter.insertTarget;
			int pick = pd.inserter.pickTarget;
			fs.inserterPool[inserterId].delay = pd.inserter.delay;
			fs.inserterPool[inserterId].stt = pd.inserter.stt;
			//如果有送取目标
			if (target > 0 && eIdMap.ContainsKey(target))
			{
				//Debug.Log(target + "|" + eIdMap[target]);
				//设置送取-游戏函数
				fs.SetInserterInsertTarget(inserterId, eIdMap[target], pd.inserter.insertOffset);
				if (pd.outConn != 0)
				{
					//读取连接端口数据
					ReadObjectConn(pd.outConn, out bool isO, out int other, out int slot);
					//设置连接端口数据
					player.planetData.factory.WriteObjectConn(eid, 0, isO, eIdMap[target], slot);
				}
			}
			//如果有捡取目标
			if (pick > 0 && eIdMap.ContainsKey(pick))
			{
				//Debug.Log(pick + ":" + eIdMap[pick]);
				//设置捡取--游戏函数
				fs.SetInserterPickTarget(inserterId, eIdMap[pick], pd.inserter.pickOffset);
				if (pd.inConn != 0)
				{
					//读取连接端口数据
					ReadObjectConn(pd.inConn, out bool isO, out int other, out int slot);
					//设置连接端口数据
					player.planetData.factory.WriteObjectConn(eid, 1, isO, eIdMap[pick], slot);
				}
			}
			preInserterMap.Remove(preId);
		}
		Check();
	}


	/// <summary>
	/// 导出文件
	/// </summary>
	public void Export()
	{

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] s = new string[Count + 7];
		//文件名
		s[0] = Name;
		s[1] = GetItemCountData(true);
		s[2] = AssemblerDate.Count.ToString();
		int i = 3;
		foreach (var d in AssemblerDate)
		{
			s[i++] = d.GetAssemblerData();
		}
		s[i++] = PowerData.Count.ToString();
		foreach (var d in PowerData)
		{
			s[i++] = d.GetAssemblerData();
		}
		s[i++] = PowerNodeData.Count.ToString();
		foreach (var d in PowerNodeData)
		{
			s[i++] = d.GetAssemblerData();
		}
		s[i++] = InserterData.Count.ToString();
		foreach (var d in InserterData)
		{
			s[i++] = d.GetInserterData();
		}
		s[i++] = BeltData.Count.ToString();
		foreach (var d in BeltData)
		{
			s[i++] = d.GetBeltData();
		}
		for (; i < s.Length; i++)
		{
			s[i] = string.Empty;
		}
		string fileName = path + "\\" + Name + ".data";
		try
		{
			File.WriteAllLines(fileName, s);
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message + "\n" + e.StackTrace);
		}
	}


	/// <summary>
	/// 文件导入
	/// </summary>
	/// <param name="FileName">文件路径</param>
	public void Import(string FileName)
	{
		if (Directory.Exists(path))
		{
			if (File.Exists(FileName))
			{
				string[] s = File.ReadAllLines(FileName);
				try
				{
					if (s.Length > 7)
					{
						Name = s[0];
						AddItemCount(s[1]);
						int c = int.Parse(s[2]);
						int i = 3;
						for (int j = 0; j < c; j++)
						{
							AssemblerDate.Add(new MyPrebuildData(s[i + j], 0));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							PowerData.Add(new MyPrebuildData(s[i + j], 0));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							PowerNodeData.Add(new MyPrebuildData(s[i + j], 0));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							InserterData.Add(new MyPrebuildData(s[i + j], 1));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							BeltData.Add(new MyPrebuildData(s[i + j], 2));
						}
                        if (Count > 0 && ItemNeed.Count == 0)
                        {
							InitItemNeed();
							Export();
						}
					}
				}
				catch (Exception e)
				{
					Debug.LogError("ParseError");
					Debug.LogError(e.Message);
					Name = Name + "导入错误！";
					ClearData();
				}
			}
		}
	}

	/// <summary>
	/// 获取空的连接口
	/// </summary>
	/// <param name="eid">eid</param>
	/// <param name="start">连接口开始检索数</param>
	/// <returns></returns>
	public int GetEmptyConn(int eid,int start)
    {
        for (; start < 16; start++)
        {
			try
			{
				if (player.planetData.factory.entityConnPool[eid * 16 + start] == 0)
				{
					return start;
				}
            }
            catch(Exception e)
            {
				Debug.LogError("GetEmptyConnError");
				Debug.LogError(e.Message);
            }
        }
		return -1;
    }

	/// <summary>
	/// 有建筑建造完成时，对预建造数据进行处理
	/// </summary>
	/// <param name="preId">预建造ID</param>
	/// <param name="eid">eID</param>
	public void Building(int preId, int eid)
	{
		if (preIdMap.ContainsKey(preId))
		{
			var d = preIdMap[preId];
			d.newEId = eid;
			eIdMap.Add(d.oldEId, eid);
			//Debug.Log(d.isBelt);
			bool flag = d.isBelt;

			if (flag)
            {
				BuildBeltData.Add(d);
				BeltEIdMap.Add(d.oldEId, eid);
				BeltBuild();
			}
			preIdMap.Remove(preId);
            if (flag)
            {
				BeltQueueDequeue();
			}
			TryBuildInserter();
		}
		else if (preIdMap.Count == 0)
		{
			TryBuildInserter();
			BeltBuild();
		}
	}

	/// <summary>
	/// 尝试建造爪子数据
	/// </summary>
	public void TryBuildInserter()
	{
		if (!playerHaveInserterItem)
			return;
		List<MyPrebuildData> temp = new List<MyPrebuildData>();
		foreach (var d in PreInserterData)
		{
			int inser = d.inserter.insertTarget;
			int pick = d.inserter.pickTarget;
			if ((inser == 0 && pick == 0) ||
				(inser > 0 && eIdMap.ContainsKey(inser) && pick == 0) ||
				(pick > 0 && eIdMap.ContainsKey(pick) && inser == 0) ||
				(inser > 0 && eIdMap.ContainsKey(inser) && pick > 0 && eIdMap.ContainsKey(pick))
				)
			{
				AddPrebuildData(player, d, out int pid, true);
				if (pid > 0)
				{
					//planetFactory.WriteObjectConn(-pid, 0, true,inser, endSlot); // assembler connection
					//planetFactory.WriteObjectConn(-pid, 1, false, pick, startSlot);
					preInserterMap.Add(pid, d);
					temp.Add(d);
				}
			}

		}
		if (preIdMap.Count == 0 && BuildBeltData.Count == 0 && WaitItemBuild.Count == 0 && BeltQueue.Count == 0 && temp.Count == 0)
		{
			PreInserterData.Clear();
			temp.Clear();
			Check();
		}
		foreach (var re in temp)
		{
			PreInserterData.Remove(re);
		}


	}

	/// <summary>
	/// 检测是否还有建筑未建造
	/// </summary>
	public void Check()
	{
		if (preIdMap.Count == 0 && PreInserterData.Count == 0 && preInserterMap.Count == 0)
			haveInserter = false;
		if (BuildBeltData.Count == 0)
			haveBelt = false;
	}


	/// <summary>
	/// 传送带建造完成时对传送带数据尝试连接
	/// </summary>
	public void BeltBuild()
    {
		List<MyPrebuildData> tmp = new List<MyPrebuildData>();
		foreach(var d in BuildBeltData)
        {
			int out1 = d.beltOut;
			int in1 = d.beltIn1;
			int in2 = d.beltIn2;
			int in3 = d.beltIn3;
            if (out1 == 0|| BeltEIdMap.ContainsKey(out1))
            {
				if(in1==0||BeltEIdMap.ContainsKey(in1))
					if (in2 == 0 || BeltEIdMap.ContainsKey(in2))
						if (in3 == 0 || BeltEIdMap.ContainsKey(in3))
                        {
							SetBelt(d);
							tmp.Add(d);
                        }


			}
        }
		foreach(var d in tmp)
        {
			BuildBeltData.Remove(d);
        }
		Check();

    }


	/// <summary>
	/// 传送带连接
	/// </summary>
	/// <param name="d"></param>
	public void SetBelt(MyPrebuildData d)
    {
		var pf = player.planetData.factory;
		int out1 = 0;
		int in1 =0;
		int in2 =0;
		int in3 = 0;
		if (BeltEIdMap.ContainsKey(d.beltOut))
        {
			int other = BeltEIdMap[d.beltOut];
			out1 =pf.entityPool[other].beltId;
			int otherSlot = GetEmptyConn(other, 1);
			if (otherSlot > 0)
				pf.WriteObjectConn(d.newEId, 0, true, other, otherSlot);
		}
		if (BeltEIdMap.ContainsKey(d.beltIn1))
		{
			in1 = pf.entityPool[BeltEIdMap[d.beltIn1]].beltId;
		}
		if (BeltEIdMap.ContainsKey(d.beltIn2))
		{
			in2 = pf.entityPool[BeltEIdMap[d.beltIn2]].beltId;
		}
		if (BeltEIdMap.ContainsKey(d.beltIn3))
		{
			in3 = pf.entityPool[BeltEIdMap[d.beltIn3]].beltId;
		}
		int beltId = pf.entityPool[d.newEId].beltId;
		pf.cargoTraffic.AlterBeltConnections(beltId, out1, in1, in2, in3);
	}

	private bool IsInArea(Vector3 pos,bool[] area)
    {
		int count = 0;
		if (pos.x < 0)
			count += 4;
		if (pos.y < 0)
			count += 2;
		if (pos.z < 0)
			count += 1;
		return area[count];
	}

	public static Quaternion GetNewRot(Quaternion q, ERotationType type)
	{
		if (type == ERotationType.Null)
			return q;

		var euler = q.eulerAngles;
		switch (type)
		{
			case ERotationType.X:
				if (euler.x > 0.01f)
					euler.y = (540f - euler.y) % 360f;
				else
					euler.y = 360f - euler.y;
				euler.x = 360f - euler.x;
				euler.z = 360f - euler.z;
				break;
			case ERotationType.Y:
				euler.y = (euler.y + 180f) % 360f;
				euler.z = (euler.z + 180f) % 360f;
				break;
			case ERotationType.Z:
				if (euler.x > 0.01f)
					euler.y = 360f - euler.y;
				else
					euler.y = (540f - euler.y) % 360f;
				euler.x = 360f - euler.x;
				euler.z = 360f - euler.z;
				break;
			case ERotationType.XY:
				euler.y = (euler.y + 180f) % 360f;
				euler.z = (euler.z + 180f) % 360f;
				if (euler.x > 0.01f)
					euler.y = (540f - euler.y) % 360f;
				else

					euler.y = 360f - euler.y;
				euler.x = 360f - euler.x;
				euler.z = 360f - euler.z;
				break;
			case ERotationType.XZ:
				if (euler.x > 0.01f)
				{
					euler.y = 360f - euler.y;
					euler.y = (540f - euler.y) % 360f;
				}
				else
				{
					euler.y = (540f - euler.y) % 360f;
					euler.y = 360f - euler.y;
				}
					
				break;
			case ERotationType.YZ:
				euler.y = (euler.y + 180f) % 360f;
				euler.z = (euler.z + 180f) % 360f;
				if (euler.x > 0.01f)
					euler.y = 360f - euler.y;
				else
					euler.y = (540f - euler.y) % 360f;
				euler.x = 360f - euler.x;
				euler.z = 360f - euler.z;
				break;
			case ERotationType.XYZ:
				if (euler.x > 0.01f)
				{
					euler.y = 360f - euler.y;
					euler.y = (540f - euler.y) % 360f;
				}
				else
				{
					euler.y = (540f - euler.y) % 360f;
					euler.y = 360f - euler.y;
				}
				euler.y = (euler.y + 180f) % 360f;
				euler.z = (euler.z + 180f) % 360f;
				break;

		}
		q.eulerAngles = euler;
		return q;
	}

	public static Vector3 GetNewPos(Vector3 pos, ERotationType type)
    {
		if (type == ERotationType.Null)
			return pos;
		switch (type)
		{
			case ERotationType.X:
				pos.x = -pos.x;
				break;
			case ERotationType.Y:
				pos.y = -pos.y;
				break;
			case ERotationType.Z:
				pos.z = -pos.z;
				break;
			case ERotationType.XY:
				pos.x = -pos.x;
				pos.y = -pos.y;
				break;
			case ERotationType.XZ:
				pos.x = -pos.x;
				pos.z = -pos.z;
				break;
			case ERotationType.YZ:
				pos.z = -pos.z;
				pos.y = -pos.y;
				break;
			case ERotationType.XYZ:
				pos.x = -pos.x;
				pos.y = -pos.y;
				pos.z = -pos.z;
				break;

		}
		return pos;
	}
	/// <summary>
	/// 添加预建筑数据
	/// </summary>
	/// <param name="player">玩家</param>
	/// <param name="d">预建筑</param>
	/// <param name="pId">预建筑id</param>
	/// <param name="IgnoreOverlap">是否忽略重叠</param>
	/// <returns>是否添加成功</returns>
	public bool AddPrebuildData(Player player, MyPrebuildData d, out int pId, bool IgnoreOverlap = false)
	{
		pId = -1;
		if (player.package.GetItemCount(d.ProtoId) > 0)
		{
			d.SetNewRot(RotationType);
			if (!IgnoreOverlap)
			{
				int posD = d.posSetNum;
				if (posSet.ContainsKey(posD))
				{
					int eid = posSet[posD];
					//Debug.Log($"{eid},{player.planetData.factory.entityPool[eid].protoId},{d.protoId}");
					if (player.planetData.factory.entityPool[eid].protoId == d.ProtoId)
                    {
						d.newEId = eid;
						eIdMap.Add(d.oldEId, eid);
                    }
					buildF1++;
					buildF++;
					return false;
				}
			}
			else
			{
				long pos = GetPosLong(d.pd.pos);
				if (floatPosSet.ContainsKey(pos))
				{

					int eid = floatPosSet[pos];
					//Debug.Log($"{eid},{player.planetData.factory.entityPool[eid].protoId},{d.protoId}");
					if (player.planetData.factory.entityPool[eid].protoId == d.ProtoId)
					{
						d.newEId = eid;
						eIdMap.Add(d.oldEId, eid);

					}
					if (player.planetData.factory.entityPool[eid].beltId > 0)
					{
						BeltEIdMap.Add(d.oldEId, eid);
					}
					buildF1++;
					buildF++;
					return false;
				}
			}
			//Debug.Log(d.protoId);
			pId = player.factory.AddPrebuildDataWithComponents(d.pd);
			int e = player.planetData.factory.prebuildPool[pId].upEntity;
			player.package.TakeItem(d.ProtoId, 1);
			buildS++;
			//var fs= File.Open(@"D:\Debug.json",FileMode.Append);
			//StreamWriter sw = new StreamWriter(fs);
			//sw.WriteLine(d.GetBeltData());
			//sw.Close();
			//fs.Close();

            //{
			//	BeltData.Remove(d);
			//	Export();
            //}
			return true;
        }
        else
        {
            if (d.isBelt)
            {
				playerHaveBeltItem = false;
            }
            if (d.isInserter)
            {
				playerHaveInserterItem = false;

			}
			if ((d.isBelt&&!playerHaveBeltItem)||(d.isInserter&&!playerHaveInserterItem))
			{
				return false;
			}
			WaitItemBuild.Add(d);
			AddWaitNeedIiem(d.ProtoId);
			buildF2++;
			buildF++;
			return false;
		}

	}

	/// <summary>
	/// 检测是否在游戏
	/// </summary>
	/// <returns></returns>
	public static bool CheckData()
	{
		if (GameMain.data != null)
		{
			if (GameMain.data.mainPlayer != null)
			{
				if (GameMain.data.mainPlayer.planetData != null)
				{
					if (GameMain.data.mainPlayer.planetData.factory != null)
					{
						if (GameMain.data.mainPlayer.planetData.factory.factorySystem != null)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}


	/// <summary>
	/// 获取工作台主要数据
	/// </summary>
	/// <param name="ac">工作台数据</param>
	/// <param name="ed">实体数据</param>
	/// <returns></returns>
	static PrebuildData GetPreDate(AssemblerComponent ac, EntityData ed)
	{
		PrebuildData prebuild = default(PrebuildData);
		prebuild.protoId = (short)ed.protoId;
		prebuild.modelIndex = (short)ed.modelIndex;
		prebuild.pos = ed.pos;
		prebuild.pos2 = VectorLF3.zero;
		prebuild.rot = ed.rot;
		prebuild.rot2 = Quaternion.identity;
		prebuild.recipeId = ac.recipeId;
		return prebuild;
	}

	/// <summary>
	/// 获取炮台主要数据
	/// </summary>
	/// <param name="ac">炮台数据</param>
	/// <param name="ed">实体数据</param>
	/// <returns></returns>
	static PrebuildData GetPreDate(EjectorComponent ac, EntityData ed)
	{
		PrebuildData prebuild = default(PrebuildData);
		prebuild.protoId = (short)ed.protoId;
		prebuild.modelIndex = (short)ed.modelIndex;
		prebuild.pos = ed.pos;
		prebuild.pos2 = VectorLF3.zero;
		prebuild.rot = ed.rot;
		prebuild.rot2 = Quaternion.identity;
		return prebuild;
	}

	/// <summary>
	/// 获取发射井主要数据
	/// </summary>
	/// <param name="ac">发射井数据</param>
	/// <param name="ed">实体数据</param>
	/// <returns></returns>
	static PrebuildData GetPreDate(SiloComponent ac, EntityData ed)
	{
		PrebuildData prebuild = default(PrebuildData);
		prebuild.protoId = (short)ed.protoId;
		prebuild.modelIndex = (short)ed.modelIndex;
		prebuild.pos = ed.pos;
		prebuild.pos2 = VectorLF3.zero;
		prebuild.rot = ed.rot;
		prebuild.rot2 = Quaternion.identity;
		return prebuild;
	}
	static PrebuildData GetPreDate(InserterComponent ic, EntityData ed)
	{
		PrebuildData prebuild = default(PrebuildData);
		prebuild.protoId = (short)ed.protoId;
		prebuild.modelIndex = (short)ed.modelIndex;
		prebuild.pos = ed.pos;
		prebuild.pos2 = ic.pos2;
		prebuild.rot = ed.rot;
		prebuild.rot2 = ic.rot2;
		prebuild.filterId = ic.filter;
		prebuild.insertOffset = ic.insertOffset;
		prebuild.pickOffset = ic.pickOffset;
		return prebuild;
	}
	static PrebuildData GetPreDate(BeltComponent bc, EntityData ed)
	{
		PrebuildData prebuild = default(PrebuildData);
		prebuild.protoId = (short)ed.protoId;
		prebuild.modelIndex = (short)ed.modelIndex;
		prebuild.pos = ed.pos;
		prebuild.pos2 = VectorLF3.zero;
		prebuild.rot = ed.rot;
		prebuild.rot2 = Quaternion.identity;
		return prebuild;
	}
	static PrebuildData GetPreDate(PowerGeneratorComponent bc, EntityData ed)
	{
		PrebuildData prebuild = default(PrebuildData);
		prebuild.protoId = (short)ed.protoId;
		prebuild.modelIndex = (short)ed.modelIndex;
		prebuild.pos = ed.pos;
		prebuild.pos2 = VectorLF3.zero;
		prebuild.rot = ed.rot;
		prebuild.rot2 = Quaternion.identity;
		return prebuild;
	}
	static PrebuildData GetPreDate(PowerNodeComponent bc, EntityData ed)
	{
		PrebuildData prebuild = default(PrebuildData);
		prebuild.protoId = (short)ed.protoId;
		prebuild.modelIndex = (short)ed.modelIndex;
		prebuild.pos = ed.pos;
		prebuild.pos2 = VectorLF3.zero;
		prebuild.rot = ed.rot;
		prebuild.rot2 = Quaternion.identity;
		return prebuild;
	}

	/// <summary>
	/// 对接口数据进行解析
	/// </summary>
	/// <param name="num">接口数据</param>
	/// <param name="isOutput">是否是出货</param>
	/// <param name="otherObjId">连接到其他物品的eid</param>
	/// <param name="otherSlot">连接到其他物品端口号0-15</param>
	static public void ReadObjectConn(int num, out bool isOutput, out int otherObjId, out int otherSlot)
	{
		isOutput = false;
		otherObjId = 0;//连接实体id
		otherSlot = 0;//端口号
		if (num == 0)
		{
			return;
		}
		bool flag = num > 0;
		num = ((!flag) ? (-num) : num);
		isOutput = ((num & 536870912) == 0);
		otherObjId = (num & 16777215);
		otherSlot = (num & 536870911) >> 24;
		if (!flag)
		{
			otherObjId = -otherObjId;
		}

	}

	public class MyPrebuildData
	{
		public PrebuildData pd;
		public BeltComponent belt;
		public InserterComponent inserter;
		public int oldEId;
		public int newEId;
		public int preId;
		public int outConn;//inserter
		public int inConn;//pick
		public int beltOut;
		public int beltIn1;
		public int beltIn2;
		public int beltIn3;
		public bool isBelt;
		public bool isInserter;
		public bool isNewRot;

		public MyPrebuildData(MyPrebuildData data)
		{
			Init();
			pd = data.pd;
			belt = data.belt;
			inserter = data.inserter;
			oldEId = data.oldEId;
			newEId = data.newEId;
			preId = data.preId;
			outConn = data.outConn;
			inConn = data.inConn;
			beltOut = data.beltOut;
			beltIn1 = data.beltIn1;
			beltIn2 = data.beltIn2;
			beltIn3 = data.beltIn3;
			isBelt = data.isBelt;
			isInserter = data.isInserter;
		}
		public MyPrebuildData(PrebuildData prebuild, int type)
		{
			pd = prebuild;
			Init();
			switch (type)
			{
				case 1: isInserter = true; break;
				case 2: isBelt = true; break;
			}
		}
		public MyPrebuildData(PrebuildData prebuild, InserterComponent inserter)
		{
			pd = prebuild;
			Init();
			this.inserter = inserter;
			isInserter = true;
		}

		public MyPrebuildData(PrebuildData prebuild, BeltComponent belt,int out1,int in1,int in2,int in3)
		{
			pd = prebuild;
			Init();
			this.belt = belt;
			beltOut = out1;
			beltIn1 = in1;
			beltIn2 = in2;
			beltIn3 = in3;
			isBelt = true;
		}

		public MyPrebuildData(string data, int type)
		{
			Init();
			//try
			//{
			if (type == 0)
			{
				pd = default;
				string[] s = data.Split(',');
				pd.protoId = short.Parse(s[0]);
				pd.modelIndex = short.Parse(s[1]);
				pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
				pd.pos2 = Vector3.zero;
				pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
				pd.rot2 = Quaternion.identity;
				pd.recipeId = int.Parse(s[9]);
				if (s.Length > 10)
				{
					oldEId = int.Parse(s[10]);
				}
			}
			if (type == 1)
			{
				pd = default;
				inserter = default;
				isInserter = true;
				string[] s = data.Split(',');
				if (s.Length > 20)
				{
					pd.protoId = short.Parse(s[0]);
					pd.modelIndex = short.Parse(s[1]);
					pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
					pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
					pd.pos2 = new Vector3(float.Parse(s[9]), float.Parse(s[10]), float.Parse(s[11]));
					pd.rot2 = new Quaternion(float.Parse(s[12]), float.Parse(s[13]), float.Parse(s[14]), float.Parse(s[15]));
					pd.filterId = int.Parse(s[16]);
					inserter.pickTarget = int.Parse(s[17]);
					inserter.insertTarget = int.Parse(s[18]);
					inserter.stt = int.Parse(s[19]);
					inserter.delay = int.Parse(s[20]);
					if (s.Length > 22)
					{
						outConn = int.Parse(s[21]);
						inConn = int.Parse(s[22]);
					}
				}
			}
			if (type == 2)
			{
				pd = default;
				isBelt = true;
				string[] s = data.Split(',');
				if (s.Length > 13)
				{
					pd.protoId = short.Parse(s[0]);
					pd.modelIndex = short.Parse(s[1]);
					pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
					pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
					oldEId = int.Parse(s[9]);
					beltOut = int.Parse(s[10]);
					beltIn1 = int.Parse(s[11]);
					beltIn2 = int.Parse(s[12]);
					beltIn3 = int.Parse(s[13]);
				}
			}
			//}
			//catch (Exception e)
			//{
			//	Debug.LogError("DataParseError");
			//	Debug.LogError(e.Message);
			//}
		}

		public void SetNewRot(ERotationType type)
        {
			if (!isNewRot)
			{
				pd.pos = GetNewPos(pd.pos, type);
				pd.rot = GetNewRot(pd.rot, type);
				pd.pos2 = GetNewPos(pd.pos2, type);
				pd.rot2 = GetNewRot(pd.rot2, type);
				isNewRot = true;
			}
        }
		public void Init()
		{
			isBelt = false;
			isInserter = false;
			oldEId = 0;
			newEId = 0;
			isNewRot = false;
		}
		public int ProtoId
		{
			get
			{
				return pd.protoId;
			}
		}

		public Vector3 Pos
        {
            get
            {
				return pd.pos;
            }
        }


		public string GetAssemblerData()
		{
			string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{pd.recipeId}";
			if (oldEId > 0)
			{
				s += $",{oldEId}";
			}
			return s;
		}

		public string GetInserterData()
		{
			string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w}";
			s += $",{pd.pos2.x},{pd.pos2.y},{pd.pos2.z},{pd.rot2.x},{pd.rot2.y},{pd.rot2.z},{pd.rot2.w},{pd.filterId}";
			s += $",{inserter.pickTarget},{inserter.insertTarget},{inserter.stt},{inserter.delay},{outConn},{inConn}";
			return s;
		}

		public string GetBeltData()
		{
			string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
			s += $",{beltOut},{beltIn1},{beltIn2},{beltIn3}";
			return s;
		}
		public int posSetNum
		{
			get
			{
				float x = ((int)(pd.pos.x * 10)) / 10f;
				float y = ((int)(pd.pos.y * 10)) / 10f;
				float z = ((int)(pd.pos.z * 10)) / 10f;
				return (int)(x * 1000000 + y * 1000 + z);
			}
		}

	}
}

