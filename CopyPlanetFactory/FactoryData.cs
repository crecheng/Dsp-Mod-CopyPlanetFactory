using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;


public class FactoryData
{
	/// <summary>
	/// 文件路径
	/// </summary>
	string path;
	/// <summary>
	/// 工作台数据
	/// </summary>
	public List<Assembler> AssemblerDate;
	/// <summary>
	/// 爪子数据
	/// </summary>
	public List<Inserter> InserterData;
	/// <summary>
	/// 传送带数据
	/// </summary>
	public List<Belt> BeltData;
	/// <summary>.
	/// 发电机数据
	/// </summary>
	public List<Assembler> PowerData;
	/// <summary>
	/// 电线杆数据
	/// </summary>
	public List<Assembler> PowerNodeData;
	/// <summary>
	/// 运输站数据
	/// </summary>
	public List<Station> StationData;
	/// <summary>
	/// 研究站数据
	/// </summary>
	public List<Lab> LabData;

	/// <summary>
	/// 全部数据
	/// </summary>
	public List<MyPreBuildData> AllData;
	/// <summary>
	/// 需求物品
	/// </summary>
	public Dictionary<int, int> ItemNeed;


	public bool isInitItem;
	public PlanetFactory factory;

	/// <summary>
	/// 建筑个数
	/// </summary>
	public int Count
	{
		get
		{
			return AssemblerDate.Count + InserterData.Count + BeltData.Count + PowerData.Count + PowerNodeData.Count + StationData.Count + LabData.Count;
		}
	}


	void Init()
    {
		path = System.Environment.CurrentDirectory + "\\BepInEx\\config\\PlanetFactoryData";
		AssemblerDate = new List<Assembler>();
		InserterData = new List<Inserter>();
		BeltData = new List<Belt>();
		PowerData = new List<Assembler>();
		PowerNodeData = new List<Assembler>();
		StationData = new List<Station>();
		LabData = new List<Lab>();
		ItemNeed = new Dictionary<int, int>();
		isInitItem = false;
		AllData = new List<MyPreBuildData>();
		Name = string.Empty;
	}
	public FactoryData()
    {
		Init();
    }

	public void Clear()
    {
		AssemblerDate.Clear();
		InserterData.Clear();
		BeltData.Clear();
		PowerData.Clear();
		PowerNodeData.Clear();
		StationData.Clear();
		LabData.Clear();
		ItemNeed.Clear();
		AllData.Clear();
	}

	public string Name;

	/// <summary>
	/// 导出文件
	/// </summary>
	public void Export()
	{

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] s = new string[Count + 7 + 7];
		//文件名
		s[0] = Name;
		s[1] = GetItemCountData(true);
		s[2] = AssemblerDate.Count.ToString();
		int i = 3;
		foreach (var d in AssemblerDate)
		{
			s[i++] = d.GetData();
		}
		s[i++] = PowerData.Count.ToString();
		foreach (var d in PowerData)
		{
			s[i++] = d.GetData();
		}
		s[i++] = PowerNodeData.Count.ToString();
		foreach (var d in PowerNodeData)
		{
			s[i++] = d.GetData();
		}
		s[i++] = InserterData.Count.ToString();
		foreach (var d in InserterData)
		{
			s[i++] = d.GetData();
		}
		s[i++] = BeltData.Count.ToString();
		foreach (var d in BeltData)
		{
			s[i++] = d.GetData();
		}
		s[i++] = StationData.Count.ToString();
		foreach (var d in StationData)
		{
			s[i++] = d.GetData();
		}
		s[i++] = LabData.Count.ToString();
		foreach (var d in LabData)
		{
			s[i++] = d.GetData();
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
							AssemblerDate.Add(new Assembler(s[i + j]));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							PowerData.Add(new Assembler(s[i + j]));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							PowerNodeData.Add(new Assembler(s[i + j]));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							InserterData.Add(new Inserter(s[i + j]));
						}
						i += c;
						c = int.Parse(s[i]);
						i++;

						for (int j = 0; j < c; j++)
						{
							BeltData.Add(new Belt(s[i + j]));
						}
						i += c;

						if (s.Length < i + 1)
							return;
						c = int.Parse(s[i]);
						i++;
						if (s.Length < i + c)
							return;
						for (int j = 0; j < c; j++)
						{
							Station temp = new Station(s[i + j]);
							if (temp.isStation)
								StationData.Add(temp);
						}
						i += c;
						if (s.Length < i + 1)
							return;
						if (s[i].Length < 1)
							return;
						c = int.Parse(s[i]);
						i++;
						if (s.Length < i + c)
							return;
						for (int j = 0; j < c; j++)
						{
							Lab temp = new Lab(s[i + j]);
							if (temp.isLab)
								LabData.Add(temp);
						}
						i += c;


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
					Name = Name + "【Error!】";
					Clear();
				}
			}
		}
	}

	public bool TryAddData(EntityData entity,HashSet<int> eidSet)
    {
		var eId = entity.id;
		if (entity.id == 0)
			return false;
		if (eidSet.Contains(eId))
			return false;
		eidSet.Add(eId);
		if (entity.protoId == 0)
			return false;
		return true;
	}

	/// <summary>
	/// 复制建筑
	/// </summary>
	/// <param name="factory">工厂数据</param>
	public void CopyDate(PlanetFactory factory)
	{
		HashSet<int> eidSet = new HashSet<int>();
		Clear();
		this.factory = factory;
		var fSystem = factory.factorySystem;
		for (int i = 1; i < fSystem.assemblerCursor; i++)
		{
			var ap = fSystem.assemblerPool[i];
			var ed = factory.entityPool[ap.entityId];
			if (!TryAddData(ed, eidSet))
				continue;
			var temp = new Assembler(GetPreDate(ap, ed));
			temp.oldEId = ap.entityId;
			AssemblerDate.Add(temp);
			AllData.Add(temp);
			AddItemCount(ed.protoId);
		}

		for (int i = 1; i < fSystem.ejectorCursor; i++)
		{
			var ap = fSystem.ejectorPool[i];
			var ed = factory.entityPool[ap.entityId];
			if (!TryAddData(ed, eidSet))
				continue;
			var temp = new Assembler(GetPreDate(ap, ed));
			temp.oldEId = ap.entityId;
			AssemblerDate.Add(temp);
			AllData.Add(temp);
			AddItemCount(ed.protoId);
		}
		for (int i = 1; i < fSystem.siloCursor; i++)
		{
			var ap = fSystem.siloPool[i];
			var ed = factory.entityPool[ap.entityId];
			if (!TryAddData(ed, eidSet))
				continue;
			var temp = new Assembler(GetPreDate(ap, ed));
			temp.oldEId = ap.entityId;
			AssemblerDate.Add(temp);
			AllData.Add(temp);
			AddItemCount(ed.protoId);
		}
		for (int i = 1; i < factory.powerSystem.genCursor; i++)
		{
			var ap = factory.powerSystem.genPool[i];
			var ed = factory.entityPool[ap.entityId];
			if (!TryAddData(ed, eidSet))
				continue;
			var temp = new Assembler(GetPreDate(ap, ed));
			temp.oldEId = ap.entityId;
			PowerData.Add(temp);
			AllData.Add(temp);
			AddItemCount(ed.protoId);
		}
		for (int i = 1; i < factory.powerSystem.nodeCursor; i++)
		{
			var ap = factory.powerSystem.nodePool[i];
			var ed = factory.entityPool[ap.entityId];
			if (!TryAddData(ed, eidSet))
				continue;
			var temp = new Assembler(GetPreDate(ap, ed));
			temp.oldEId = ap.entityId;
			PowerNodeData.Add(temp);
			AllData.Add(temp);
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
			int outConn = factory.entityConnPool[eid * 16];
			int inConn = factory.entityConnPool[eid * 16 + 1];
			var tempP = new Inserter(GetPreDate(temp, ed),ip,outConn,inConn);
			InserterData.Add(tempP);
			AllData.Add(tempP);
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
			factory.ReadObjectConn(eId, 1, out flag2, out int in1, out slot);
			factory.ReadObjectConn(eId, 2, out flag2, out int in2, out slot);
			factory.ReadObjectConn(eId, 3, out flag2, out int in3, out slot);
			out1 = EIdIsBeltId(out1);
			in1 = EIdIsBeltId(in1);
			in2 = EIdIsBeltId(in2);
			in3 = EIdIsBeltId(in3);
			var temp = new Belt(GetPreDate(ap, ed), ap, out1, in1, in2, in3);
			temp.oldEId = eId;
			BeltData.Add(temp);
			AllData.Add(temp);
			AddItemCount(ed.protoId);
		}
		for (int i = 1; i < factory.transport.stationCursor; i++)
		{
			var ap = factory.transport.stationPool[i];
			if (ap != null)
			{
				var eId = ap.entityId;
				var ed = factory.entityPool[eId];
				if (ed.protoId == 0)
					continue;
				var temp = new Station(GetPreDate(ap, ed), ap);
				temp.oldEId = eId;
				StationData.Add(temp);
				AllData.Add(temp);
				AddItemCount(ed.protoId);
			}
		}
		HashSet<int> labIdSet = new HashSet<int>();
		for (int i = 1; i < factory.factorySystem.labCursor; i++)
		{
			var ap = factory.factorySystem.labPool[i];
			var eId = ap.entityId;
			var ed = factory.entityPool[eId];
			if (ed.protoId == 0)
				continue;
			if (labIdSet.Contains(i))
				continue;
			int nextId = i;
			do
			{
				ap = factory.factorySystem.labPool[nextId];
				nextId = ap.nextLabId;
				labIdSet.Add(ap.id);
				if (nextId == 0)
				{
					break;
				}

			} while (true);
			var temp = new Lab(GetPreDate(ap, ed), ap.researchMode, ap.recipeId, ap.techId);
			temp.oldEId = eId;
			temp.isLab = true;
			LabData.Add(temp);
			AllData.Add(temp);
			AddItemCount(ed.protoId);
		}
	}

	private int EIdIsBeltId(int eid)
	{

		if (factory.entityPool[eid].beltId > 0)
		{
			return eid;
		}
		else
		{
			return 0;
		}
	}

	/// <summary>
	/// 导出需求物品
	/// </summary>
	/// <param name="isFile"></param>
	/// <returns></returns>
	public string GetItemCountData(bool isFile = false)
	{
		string s = string.Empty;
		foreach (var d in ItemNeed)
		{
			s += d.Key + ":" + d.Value;
			s += isFile ? "," : "\n";
		}
		return s;
	}

	/// <summary>
	/// 导入需求物品数量
	/// </summary>
	/// <param name="data"></param>
	public void AddItemCount(string data)
	{
		string[] s = data.Split(',');
		foreach (var d in s)
		{
			string[] tmp = d.Split(':');
			if (tmp.Length > 1)
			{
				ItemNeed.Add(int.Parse(tmp[0]), int.Parse(tmp[1]));
			}
		}
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

	/// <summary>
	/// 重检需求物品
	/// </summary>
	public void InitItemNeed()
	{
		ItemNeed.Clear();
		foreach (var d in AllData)
		{
			AddItemCount(d.ProtoId);
		}
		isInitItem = true;
	}

	/// <summary>
	/// 检查玩家物品
	/// </summary>
	/// <param name="p">玩家</param>
	/// <param name="haveNeedItem">已有需求物品</param>
	/// <param name="count1">已有需求物品条</param>
	/// <param name="noNeeditem">未有需求物品</param>
	/// <param name="count2">未有需求物品条</param>
	public void CheckItem(Player p, out string haveNeedItem, out int count1, out string noNeeditem, out int count2)
	{
		haveNeedItem = string.Empty;
		count1 = 0;
		noNeeditem = string.Empty;
		count2 = 0;
		if (!isInitItem)
			InitItemNeed();
		if (ItemNeed.ContainsKey(0))
		{
			ItemNeed.Remove(0);
		}
		foreach (var d in ItemNeed)
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
				var item = LDB.items.Select(d.Key);
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
	static PrebuildData GetPreDate(StationComponent bc, EntityData ed)
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
	static PrebuildData GetPreDate(LabComponent bc, EntityData ed)
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
}

