﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

public class FactoryTask
{
	/// <summary>
	/// 存储的数据
	/// </summary>
	public FactoryData Data;
	///等待爪子数据
	/// <summary>
	/// </summary>
	private List<Inserter> PreInserterData;
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
	private List<MyPreBuildData> WaitItemBuild;
	/// <summary>
	/// 等待补充的物品
	/// </summary>
	private Dictionary<int, int> WaitNeedItem;
	/// <summary>
	/// 预建筑数据
	/// </summary>
	public Dictionary<int, MyPreBuildData> preIdMap;
	/// <summary>
	/// 建筑完成的传送带数据，等待连接
	/// </summary>
	public List<Belt> BuildBeltData;
	/// <summary>
	/// 建筑完成的运输站数据，等待连接
	/// </summary>
	public List<Station> BuildStationData;
	/// <summary>
	/// 建筑完成的爪子数据，等待连接
	/// </summary>
	public Dictionary<int, Inserter> preInserterMap;

	/// <summary>
	/// 传送带队列，不出现红字
	/// </summary>
	private BeltQueue belts;
	/// <summary>
	/// 已经加入预建造的传送带数量
	/// </summary>
	private int BeltCount = 0;

	public bool playerHaveBeltItem { private set; get;  }
	public bool playerHaveInserterItem { private set; get;  }

	public int buildS = 0;
	public int buildF = 0;
	public int buildF1 = 0;
	public int buildF2 = 0;
	public PlanetFactory planetFactory;
	public Player player;
	public bool haveInserter;
	public bool haveBelt;
	private string itemNeedString;
	private int itemNeedCount;
	public ERotationType RotationType { get; private set; }
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
			return	preIdMap.Count>0|| preInserterMap.Count>0 ||
					belts.Count>0 || WaitBuildCount > 0||
					BuildStationData.Count>0||BuildBeltData.Count>0;
		}
	}

	public string GetWaitNeedItem { get; private set; }

	public int BeltQueueCount
    {
        get
        {
			return belts.Count;
        }
    }

	public FactoryTask()
	{
		Init();
	}

	public FactoryTask(FactoryData data)
    {
		Init();
		this.Data= data;
    }

	public void Init()
	{
		Data = new FactoryData();
		PreInserterData = new List<Inserter>();
		eIdMap = new Dictionary<int, int>();
		posSet = new Dictionary<int, int>();
		floatPosSet = new Dictionary<long, int>();
		eidSet = new HashSet<int>();
		preIdMap = new Dictionary<int, MyPreBuildData>();
		BuildBeltData = new List<Belt>();
		BuildStationData = new List<Station>();
		preInserterMap = new Dictionary<int, Inserter>();
		BeltEIdMap = new Dictionary<int, int>();
		itemNeedString = string.Empty;
		belts = new BeltQueue();
		WaitNeedItem = new Dictionary<int, int>();
		WaitItemBuild = new List<MyPreBuildData>();
		buildS = 0;
		buildF = 0;
		buildF1 = 0;
		buildF2 = 0;
		BeltCount = 0;
		RotationType = ERotationType.Null;
		playerHaveBeltItem = true;
	}

	public void ClearData()
	{
		Data.Clear();
		itemNeedCount = 0;
		itemNeedString = string.Empty;
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
		BuildStationData.Clear();
		preInserterMap.Clear();
		BeltEIdMap.Clear();
		belts.Clear();
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
		playerHaveInserterItem = true;
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


	public void ReplenishItem()
	{
		if (CheckData())
		{
			if (GameMain.mainPlayer.factory == planetFactory)
			{
				if (WaitItemBuild.Count > 0||belts.Count>0||PreInserterData.Count>0)
				{
					var player = GameMain.mainPlayer;
					playerHaveBeltItem = true;
					playerHaveInserterItem = true;
					List<MyPreBuildData> temp = new List<MyPreBuildData>();
					temp.AddRange(WaitItemBuild);
					
					WaitItemBuild.Clear();
					WaitNeedItem.Clear();
					//Debug.Log(WaitItemBuild.Count);
					foreach (var d in temp)
					{
						var dd = d.GetCopy();

						AddPrebuildData(player, dd, out int pid);
						if (pid > 0)
						{
							dd.preId = pid;
							preIdMap.Add(pid, dd);
						}
					}

                    if (preIdMap.Count < 700&&belts.Count>0)
                    {
						BeltCount = 0;
						foreach(var d in preIdMap)
                        {
							if (d.Value.isBelt)
								BeltCount++;
						}
                    }
					BeltQueueDequeue();
					TryBuildInserter();
					SetWaitItemString();
				}
			}
		}
	}


	public void CopyData(PlanetFactory factory)
    {
		if (CheckData())
		{
			planetFactory = factory;
			Data.CopyDate(factory);
		}
	}


	public long GetPosLong(Vector3 p)
    {
		long x = (long)(p.x * 100);
		long y = (long)(p.y * 100);
		long z = (long)(p.z * 100);
		return x * 10000000000 + y * 100000 + z;
    }

	private void SetLab(int eid, MyPreBuildData d)
    {
		Lab lab = (Lab)d;
		var labId = planetFactory.entityPool[eid].labId;
		planetFactory.factorySystem.labPool[labId].SetFunction(lab.isResearchMode, lab.LabRecpId, lab.LabTech, planetFactory.entitySignPool);
    }

	private void SetStation(int eid,MyPreBuildData d)
    {
        if (CheckData())
        {
			Station station = (Station)d;
			var f = GameMain.mainPlayer.factory;
			int sId = f.entityPool[eid].stationId;
			var sc = f.transport.stationPool[sId];
			int minLen = Math.Min(sc.storage.Length, station.storage.Length);
			for(int i = 0; i < minLen; i++)
            {
				sc.storage[i] = station.storage[i];
            }
			station.newEId = eid;
			BuildStationData.Add(station);
			TryConnStation();
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
			Inserter pd = preInserterMap[preId];
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
	}

	private void TryConnStation()
	{
		List<Station> temp = new List<Station>();
		foreach (var d in BuildStationData)
		{
			bool isMissing = false;
			int sId = planetFactory.entityPool[d.newEId].stationId;
			StationComponent sc = planetFactory.transport.stationPool[sId];
			for (int i = 0; i < sc.slots.Length; i++)
			{
				int oldBeltId = d.slots[i].beltId;
				if (oldBeltId > 0&&sc.slots[i].beltId==0)
				{
					if (BeltEIdMap.ContainsKey(oldBeltId))
					{
						int NewbeltEId = BeltEIdMap[oldBeltId];
						int beltId = planetFactory.entityPool[NewbeltEId].beltId;
						sc.slots[i] = d.slots[i];
						sc.slots[i].beltId = beltId;
                        if (sc.slots[i].dir == IODir.Input)
                        {
							planetFactory.WriteObjectConn(d.newEId, i, false, NewbeltEId, 0);
                        }
						else if(sc.slots[i].dir == IODir.Output)
                        {
							planetFactory.WriteObjectConn(d.newEId, i, true, NewbeltEId, 1);
						}
					}
					else
					{
						isMissing = true;
					}
				}
			}
            if (!isMissing)
            {
				temp.Add(d);
            }
		}

		foreach(var d in temp)
        {
			BuildStationData.Remove(d);
        }
	}

	public void AddPasteData(Player player1,MyPreBuildData d)
    {
		if (IsInArea(d.Pos, area))
		{
			//复制一份数据
			var dd = d.GetCopy();
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
	public void PasteDate(Player player1 ,bool[] SelectArea,ERotationType rotationType=ERotationType.Null)
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
			foreach (var d in Data.AssemblerDate)
			{
				AddPasteData(player1, d);
			}
			///粘贴发电机
			foreach (var d in Data.PowerData)
			{
				AddPasteData(player1, d);
			}
			//粘贴电线杆数据
			foreach (var d in Data.PowerNodeData)
			{
				AddPasteData(player1, d);
			}
			//粘贴物流站数据
			foreach (var d in Data.StationData)
			{
				AddPasteData(player1, d);
			}
			foreach(var d in Data.LabData)
            {
				AddPasteData(player1, d);
			}

			//粘贴传送带数据
			foreach (var d in Data.BeltData)
			{
				if (IsInArea(d.Pos, SelectArea))
				{
					Belt dd = (Belt)d.GetCopy();
					belts.Enqueue(dd);
				}
			}
			//将爪子数据加入备选列表
			foreach (var d in Data.InserterData)
			{
				if (IsInArea(d.Pos, SelectArea))
				{
					var dd = (Inserter)d.GetCopy();
					PreInserterData.Add(dd);
				}
			}
			if (preIdMap.Count == 0)
			{
				TryBuildInserter();
			}
			belts.Sort();
			BeltQueueDequeue();
			SetWaitItemString();
		}
		haveAddPasteData = false;
	}


	/// <summary>
	/// 传送带队列出队
	/// </summary>
	public void BeltQueueDequeue()
    {
		if (belts.Count > 0&&playerHaveBeltItem)
		{
			int i = 0;
			do
			{
				i++;
				if (belts.Count > 0 && playerHaveBeltItem)
				{
					var dd = belts.Peek();
					AddPrebuildData(player, dd, out int pid, true);
					//Debug.Log(pid);
					if (pid > 0)
					{
						BeltCount++;
						haveBelt = true;
						dd.preId = pid;
						preIdMap.Add(pid, dd);
						BeltCount++;
						belts.Dequeue();
					}
				}
				if (i > 10000)
					break;
			} while (BeltCount < 1000);
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
				BeltCount--;
				BuildBeltData.Add((Belt)d);
				BeltEIdMap.Add(d.oldEId, eid);
				BeltBuild();
			}
			if (d.isStation)
			{
				SetStation(eid, d);
			}
            if (d.isLab)
            {
				SetLab(eid, d);
            }
			preIdMap.Remove(preId);
            if (flag)
            {
				BeltQueueDequeue();
				TryConnStation();
			}
            if (preIdMap.Count < 800)
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
		List<Inserter> temp = new List<Inserter>();
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
		if (preIdMap.Count == 0 && BuildBeltData.Count == 0 && WaitItemBuild.Count == 0 && belts.Count == 0 && temp.Count == 0)
		{
			PreInserterData.Clear();
			temp.Clear();
		}
		foreach (var re in temp)
		{
			PreInserterData.Remove(re);
		}


	}


	/// <summary>
	/// 传送带建造完成时对传送带数据尝试连接
	/// </summary>
	public void BeltBuild()
    {
		List<Belt> tmp = new List<Belt>();
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

    }

	public void NewData()
    {
		Data = new FactoryData();
    }

	/// <summary>
	/// 传送带连接
	/// </summary>
	/// <param name="d"></param>
	public void SetBelt(MyPreBuildData data)
    {
		Belt d = (Belt)data;
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

	public Quaternion GetNewRot(Quaternion q)
	{
		var type = RotationType;
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

	public Vector3 GetNewPos(Vector3 pos)
    {
		var type = RotationType;
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
	public bool AddPrebuildData(Player player, MyPreBuildData d, out int pId, bool IgnoreOverlap = false)
	{
		pId = -1;
		if (player.package.GetItemCount(d.ProtoId) > 0)
		{
			d.SetNewRot(this);
			if (!IgnoreOverlap)
			{
				int posD = d.posSetNum;
				if (posSet.ContainsKey(posD))
				{
					int eid = posSet[posD];
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
			pId = player.factory.AddPrebuildDataWithComponents(d.pd);
			int e = player.planetData.factory.prebuildPool[pId].upEntity;
			player.package.TakeItem(d.ProtoId, 1);
			buildS++;
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

}

