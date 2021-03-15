using System;
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
	/// 预建筑数据
	/// </summary>
	public Dictionary<int, MyPreBuildData> preIdMap;
	/// <summary>
	/// 预建筑数据
	/// </summary>
	public Dictionary<int, MyPreBuildData> preConnMap;


	/// <summary>
	/// 通过mod建造的建筑的数据
	/// </summary>
	List<MyPreBuildData> addBuildData;

	/// <summary>
	/// 传送带队列，不出现红字
	/// </summary>
	private BeltQueue belts;
	/// <summary>
	/// 已经加入预建造的传送带数量
	/// </summary>
	private int BeltCount = 0;

	public bool Working
    {
        get
        {
			return preIdMap.Count > 0;
        }
    }

	public bool playerHaveBeltItem { private set; get;  }
	public bool playerHaveInserterItem { private set; get;  }
	public PlanetFactory planetFactory;
	public Player player;
	public bool haveInserter;
	public bool haveBelt;
	public bool error;
	public string errorMsg;
	public int InserterConnt;
	public ERotationType RotationType { get; private set; }
	private bool[] area;

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
		BeltEIdMap = new Dictionary<int, int>();
		belts = new BeltQueue();
		preConnMap = new Dictionary<int, MyPreBuildData>();
		addBuildData = new List<MyPreBuildData>();
		error = false;
		BeltCount = 0;
		RotationType = ERotationType.Null;
		playerHaveBeltItem = true;
		playerHaveInserterItem = true;
		errorMsg = string.Empty;
		InserterConnt = 0;
	}

	public void ClearData()
	{
		Data.Clear();
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
		BeltEIdMap.Clear();
		belts.Clear();
		preConnMap.Clear();
		addBuildData.Clear();
		GetWaitNeedItem = string.Empty;
		BeltCount = 0;
		error = false;
		errorMsg = string.Empty;
		RotationType = ERotationType.Null;
		playerHaveBeltItem = true;
		playerHaveInserterItem = true;
		InserterConnt = 0;
	}

	/// <summary>
	/// 物品是否足够，能够粘贴
	/// </summary>
	/// <returns>物品是否够</returns>
	public bool CheckCanPaste(Player player)
	{

		Data.CheckItem(player, out string s1, out int c1, out string s2, out int c2);
		if (c2 == 0)
		{
			return true;
		}
		return false;
	}

	public void CopyData(PlanetFactory factory)
    {
		if (CheckData())
		{
			planetFactory = factory;
			Data.CopyData(factory);
		}
	}

	public void CopyData(PlanetFactory factory,List<int> id)
    {
		if (CheckData())
		{
			planetFactory = factory;
			Data.CopyData(factory,id);
		}
	}

	public long GetPosLong(Vector3 p)
    {
		long x = (long)(p.x * 100);
		long y = (long)(p.y * 100);
		long z = (long)(p.z * 100);
		return x * 10000000000 + y * 100000 + z;
    }



	public void AddPasteData(Player player1, MyPreBuildData d)
	{
		//复制一份数据
		var dd = d.GetCopy();
		//加入预建造数据
		AddPrebuildData(player1, dd, out int pid);
		//如果加入成功
		if (pid > 0)
		{
			dd.preId = -pid;
			//加入预建造数据
			preIdMap.Add(pid, dd);
		}
	}

	static bool haveAddPasteData = false;
	/// <summary>
	/// 粘贴建筑
	/// </summary>
	/// <param name="player1">玩家</param>
	public void PasteDate(Player player1 ,bool[] SelectArea,ERotationType rotationType=ERotationType.Null)
	{
		try
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
						if (!posSet.ContainsKey(p))
							posSet.Add(p, i);
						long lp = GetPosLong(pos);
						if (!floatPosSet.ContainsKey(lp))
							floatPosSet.Add(lp, i);
					}
					catch (Exception e)
					{
						Debug.LogError("AddPosSetError");
						Debug.LogError(e.Message);
					}
				}
                foreach (var d in Data.AllData)
                {
					if (IsInArea(d.Pos, area))
					{
						var copy = d.GetCopy();
						AddPasteData(player1, d);
					}
				}
				ConnPreBuild();
			}
			haveAddPasteData = false;
        }
        catch(Exception ex)
        {
			error = true;
			errorMsg = ex.Message + "\n" + ex.StackTrace;
        }
	}



	/// <summary>
	/// 有建筑建造完成时，对预建造数据进行处理
	/// </summary>
	/// <param name="preId">预建造ID</param>
	/// <param name="eid">eID</param>
	public void Building(int preId, int eid)
	{
		try
		{
			if (preIdMap.ContainsKey(preId))
			{
				var d = preIdMap[preId];
				d.newEId = eid;
				if (d.type != EDataType.Inserter)
					eIdMap.Add(d.oldEId, eid);

				d.SetData(planetFactory, eid);

				int ejectorId = planetFactory.entityPool[eid].ejectorId;
				if (ejectorId > 0)
				{
					planetFactory.factorySystem.ejectorPool[ejectorId].orbitId = 1;
				}

				preIdMap.Remove(preId);

			}
		}
		catch (Exception e)
		{
			error = true;
			errorMsg = e.Message + "\n" + e.StackTrace;
		}
	}

	public void ConnPreBuild()
    {
		List<int> temp = new List<int>();
        foreach (var d in preConnMap)
        {
			d.Value.ConnPreBelt(planetFactory, preConnMap);
			if (!d.Value.isNeedConn)
				temp.Add(d.Key);
        }
        foreach (var d in temp)
        {
			preConnMap.Remove(d);
        }
    }


	public void NewData()
    {
		Data = new FactoryData();
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
	public int AddPrebuildData(Player player, MyPreBuildData d, out int pId, bool IgnoreOverlap = false)
	{
		pId = -1;
		int id = Common.FindItem(d.ProtoId, planetFactory, player);
		if ( id> -1)
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

						if (d.type == EDataType.Inserter)
						{
							InserterConnt++;
							preConnMap.Add(-InserterConnt, d);
						}
						else
						{
							eIdMap.Add(d.oldEId, eid);
							preConnMap.Add(d.oldEId, d);
						}
                    }
					return 2;
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

						if (d.type == EDataType.Inserter)
						{
							InserterConnt++;
							preConnMap.Add(-InserterConnt, d);
						}
						else
						{
							eIdMap.Add(d.oldEId, eid);
							preConnMap.Add(d.oldEId, d);
						}
					}
					if (player.planetData.factory.entityPool[eid].beltId > 0)
					{
						BeltEIdMap.Add(d.oldEId, eid);
					}
					return 2;
				}
			}
			pId = player.factory.AddPrebuildDataWithComponents(d.pd);
            if (pId > 0)
            {
				//将建筑加入数据，用于撤销
				addBuildData.Add(d);

				d.preId = -pId;
				if (d.type == EDataType.Inserter)
				{
					InserterConnt++;
					preConnMap.Add(-InserterConnt, d);
				}
				else
				{
					preConnMap.Add(d.oldEId, d);
				}

			}
			
			Common.TakeItem(d.ProtoId, planetFactory, player, id);
			return 1;
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
				return 0;
			}
			return 0;
		}

	}

	/// <summary>
	/// 撤销任务，包括已经建好的
	/// </summary>
	/// <param name="player"></param>
	public void CancelTask(Player player)
    {
        foreach (var d in addBuildData)
        {
			//玩家背包是否满了
			if (!d.isCancel)
			{
				if (d.newEId == 0)
				{
					Common.RemoveBuild(player, planetFactory, d.preId);
					d.isCancel = true;
				}
				else if (d.newEId > 0)
				{
					Common.RemoveBuild(player, planetFactory, d.newEId);
					d.isCancel = true;
				}
			}
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

}

