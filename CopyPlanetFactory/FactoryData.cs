using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections;

public class FactoryData
{
	/// <summary>
	/// 文件路径
	/// </summary>
	string path;
	/// <summary>
	/// 全部数据
	/// </summary>
	public List<MyPreBuildData> AllData;
	/// <summary>
	/// 需求物品
	/// </summary>
	public Dictionary<int, int> ItemNeed;

	public Dictionary<int, Belt> CheckBeltData;

	public PlanetFactoryImg Img;

	public bool isInitItem;
	public PlanetFactory factory;



	/// <summary>
	/// 建筑个数
	/// </summary>
	public int Count
	{
		get
		{
			return AllData.Count;
		}
	}

	public Texture2D GetImg(int x, int y)
    {
		return Img.GetImg(x, y, this);
    }

	public Texture2D FreshImg()
    {
		return Img.Fresh(this);
    }


	void Init()
    {
		path = System.Environment.CurrentDirectory + "\\BepInEx\\config\\PlanetFactoryData";
		ItemNeed = new Dictionary<int, int>();
		isInitItem = false;
		AllData = new List<MyPreBuildData>();
		CheckBeltData = new Dictionary<int, Belt>();
		Name = string.Empty;
		Img = new PlanetFactoryImg();
	}
	public FactoryData()
    {
		Init();
    }

	public void Clear()
    {
		ItemNeed.Clear();
		AllData.Clear();
		CheckBeltData.Clear();
	}

	public string Name;

	public string GetTypeString(EDataType type)
    {
		int t = (int)type;
		if (t < 10)
			return "00" + t;
		else if (t < 100)
			return "0" + t;
		else
			return t.ToString();
    }

	const int dataVersion = 10000;
	/// <summary>
	/// 导出文件
	/// </summary>
	public void Export()
	{

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] s = new string[Count + 9 + 8];
		//文件名
		s[0] = dataVersion.ToString();
		s[1] = Name;
		s[2] = GetItemCountData(true);
		int i = 3;
		for(; i < 10; i++)
        {
			s[i] = string.Empty;
        }
		foreach(var d in AllData)
        {
			string temp = GetTypeString(d.type);
			temp += "|" + d.GetData();
			s[i++] = temp;
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
				if(int.TryParse(s[0],out int version))
                {
                    if (version == dataVersion)
                    {
                        try
                        {
							Name = s[1];
							AddItemCount(s[2]);
							int i = 10;
                            for (; i < s.Length; i++)
                            {
								if (s[i].Length > 3)
								{
									string type = s[i].Substring(0, 3);
									string data = s[i].Substring(4);
									if (int.TryParse(type, out int t))
									{
										EDataType et = (EDataType)t;
										switch (et)
										{
											case EDataType.Assembler:
												AllData.Add(new Assembler(data));
												break;
											case EDataType.Belt:
												AllData.Add(new Belt(data));
												break;
											case EDataType.Inserter:
												AllData.Add(new Inserter(data));
												break;
											case EDataType.Lab:
												AllData.Add(new Lab(data));
												break;
											case EDataType.Splitter:
												AllData.Add(new Splitter(data));
												break;
											case EDataType.Station:
												AllData.Add(new Station(data));
												break;
											case EDataType.Gamm:
												AllData.Add(new Gamm(data));
												break;
											case EDataType.PowGen:
												AllData.Add(new Assembler(data) { type=EDataType.PowGen});
												break;
											case EDataType.Fractionate:
												AllData.Add(new Fractionate(data));
												break;
											case EDataType.PowerExchanger:
												AllData.Add(new PowerExchanger(data));
												break;


										}
									}
								}
                            }
							InitItemNeed();
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
                else
                {
					OldImport(s);
                }

			}
		}
	}

	[Obsolete("老版本的数据读取")]
	public  void OldImport(string[] s) 
	{
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
					var temp = new Assembler(s[i + j]);
					AllData.Add(temp);
				}
				i += c;
				c = int.Parse(s[i]);
				i++;

				for (int j = 0; j < c; j++)
				{
					var temp = new Assembler(s[i + j]);
					temp.type = EDataType.PowGen;
					AllData.Add(temp);
				}
				i += c;
				c = int.Parse(s[i]);
				i++;

				for (int j = 0; j < c; j++)
				{
					var temp = new Assembler(s[i + j]);
					AllData.Add(temp);
				}
				i += c;
				c = int.Parse(s[i]);
				i++;

				for (int j = 0; j < c; j++)
				{
					var temp = new Inserter(s[i + j]);
					AllData.Add(temp);
				}
				i += c;
				c = int.Parse(s[i]);
				i++;

				for (int j = 0; j < c; j++)
				{
					var temp = new Belt(s[i + j]);
					AllData.Add(temp);
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
					{
						AllData.Add(temp);
					}
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
					{
						AllData.Add(temp);
					}
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
					Gamm temp = new Gamm(s[i + j]);
					if (temp.isGamm)
					{
						AllData.Add(temp);
					}
				}
				i += c;

				if (Count > 0 && ItemNeed.Count == 0)
				{
					InitItemNeed();
					
				}
				Export();
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
	public void CopyData(PlanetFactory factory)
	{
		Clear();
		this.factory = factory;
		var fSystem = factory.factorySystem;
		for(int i = 1; i < factory.entityCursor; i++)
        {
			CopyBuildData(factory, i);
		}
		CopyMultiLayerBuild(factory);
	}

	public void CopyMultiLayerBuild(PlanetFactory factory,List<int> id=null)
    {
		List<int> lab0 = new List<int>();
		Dictionary<int, int> labKey = new Dictionary<int, int>();
		for(int i = 1; i < factory.factorySystem.labCursor; i++)
        {
			var d = factory.factorySystem.labPool[i];
            if (d.entityId > 0)
            {
                if (d.nextLabId == 0)
                {
					lab0.Add(i);
                }
                else
                {
					labKey.Add(d.nextLabId, i);
                }
            }
        }
		foreach(int d in lab0)
        {
			int temp = d;
			do
			{
				if (labKey.ContainsKey(temp))
				{
					temp = labKey[temp];
				}
				else
					break;
			} while (true);
			var ld = factory.factorySystem.labPool[temp];
			int eid = ld.entityId;
			var ed = factory.entityPool[eid];

			if (id == null||(id!=null&&id.Contains(eid)))
			{
				MyPreBuildData t = new Lab(GetPreDate(ed), ld.researchMode, ld.recipeId, ld.techId);
				t.oldEId = eid;
				AllData.Add(t);
				AddItemCount(ed.protoId);
			}
		}
		List<int> storge0 = new List<int>();
		Dictionary<int, int> storgeKey = new Dictionary<int, int>();
		for (int i = 1; i < factory.factoryStorage.storageCursor; i++)
		{
			var d = factory.factoryStorage.storagePool[i];
			if (d.entityId > 0)
			{
				if (d.next == 0)
				{
					storge0.Add(i);
				}
				else
				{
					storgeKey.Add(d.next, i);
				}
			}
		}
		foreach (int d in storge0)
		{
			int temp = d;
			do
			{
				if (storgeKey.ContainsKey(temp))
				{
					temp = storgeKey[temp];
				}
				else
					break;
			} while (true);
			var ld = factory.factoryStorage.storagePool[temp];
			int eid = ld.entityId;
			var ed = factory.entityPool[eid];
			if (id == null || (id != null && id.Contains(eid)))
			{
				MyPreBuildData t = new Assembler(GetPreDate(ed));
				t.oldEId = eid;
				AllData.Add(t);
				AddItemCount(ed.protoId);
			}
		}

	}

	public void CopyBuildData(PlanetFactory factory, int i)
	{
		var fSystem = factory.factorySystem;
		var ed = factory.entityPool[i];


		if (ed.protoId > 0)
		{
			if (ed.labId > 0 || ed.storageId > 0 || ed.tankId > 0)
				return;
			MyPreBuildData temp = null;

			if (ed.splitterId > 0)
			{
				var ap = factory.cargoTraffic.splitterPool[ed.splitterId];
				int c0 = factory.entityConnPool[16 * ap.entityId];
				int c1 = factory.entityConnPool[16 * ap.entityId + 1];
				int c2 = factory.entityConnPool[16 * ap.entityId + 2];
				int c3 = factory.entityConnPool[16 * ap.entityId + 3];
				temp = new Splitter(GetPreDate(ed), c0, c1, c2, c3);
			}
			else if (ed.stationId > 0)
			{
				var ap = factory.transport.stationPool[ed.stationId];
				if (ap != null)
				{
					temp = new Station(GetPreDate(ed), ap);
				}
			}
			else if (ed.inserterId > 0)
			{
				var ip = fSystem.inserterPool[ed.inserterId];
				InserterComponent tempp = ip;
				var target = factory.entityPool[tempp.insertTarget];
				var pick = factory.entityPool[tempp.pickTarget];
				int outConn = factory.entityConnPool[i * 16];
				int inConn = factory.entityConnPool[i * 16 + 1];
				temp = new Inserter(GetPreDate(tempp, ed), ip, outConn, inConn);
			}
			else if (ed.beltId > 0)
			{
				var ap = factory.cargoTraffic.beltPool[ed.beltId];
				bool flag2;
				int slot;
				factory.ReadObjectConn(i, 0, out flag2, out int out1, out slot);
				factory.ReadObjectConn(i, 1, out flag2, out int in1, out slot);
				factory.ReadObjectConn(i, 2, out flag2, out int in2, out slot);
				factory.ReadObjectConn(i, 3, out flag2, out int in3, out slot);
				temp = new Belt(GetPreDate(ap, ed), ap, out1, in1, in2, in3);
				Belt tBelt = (Belt)temp;
				if (out1 == 0)
				{
					CheckBeltData.Add(i, tBelt);
				}

				tBelt.beltOut = EIdIsBeltId(out1);
				tBelt.beltIn1 = EIdIsBeltId(in1);
				tBelt.beltIn2 = EIdIsBeltId(in2);
				tBelt.beltIn3 = EIdIsBeltId(in3);
			}
			else if (ed.fractionateId > 0)
			{
				var ap = factory.factorySystem.fractionatePool[ed.fractionateId];
				int c0 = factory.entityConnPool[i * 16];
				int c1 = factory.entityConnPool[i * 16 + 1];
				int c2 = factory.entityConnPool[i * 16 + 2];
				temp = new Fractionate(GetPreDate(ed), c0, c1, c2);
			}
			else if (ed.powerExcId > 0)
			{
				var ap = factory.powerSystem.excPool[ed.powerExcId];
				int c0 = factory.entityConnPool[i * 16];
				int c1 = factory.entityConnPool[i * 16 + 1];
				int c2 = factory.entityConnPool[i * 16 + 2];
				int c3 = factory.entityConnPool[i * 16 + 3];
				temp = new PowerExchanger(GetPreDate(ed), c0, c1, c2, c3, ap.targetState);
			}
			else if (ed.assemblerId > 0)
			{
				var ap = fSystem.assemblerPool[ed.assemblerId];
				temp = new Assembler(GetPreDate(ap, ed));
			}
			else if (ed.ejectorId > 0)
			{
				var ap = fSystem.ejectorPool[ed.ejectorId];
				temp = new Assembler(GetPreDate(ed));
			}
			else if (ed.siloId > 0)
			{
				var ap = fSystem.siloPool[ed.siloId];
				temp = new Assembler(GetPreDate(ed));
			}
			else if (ed.powerGenId > 0)
			{
				var ap = factory.powerSystem.genPool[ed.powerGenId];
				if (ap.gamma)
				{

					int c0 = factory.entityConnPool[16 * ap.entityId];
					int c1 = factory.entityConnPool[16 * ap.entityId + 1];
					temp = new Gamm(GetPreDate(ed), ap.productId, c0, c1);
				}
				else
				{
					temp = new Assembler(GetPreDate(ed));
					temp.type = EDataType.PowGen;
				}


			}
			else if (ed.powerNodeId > 0)
			{
				var ap = factory.powerSystem.nodePool[ed.powerNodeId];
				temp = new Assembler(GetPreDate(ed));
			}
			//else if (ed.storageId > 0)
			//{
			//	var ap = factory.factoryStorage.storagePool[ed.storageId];
			//    if (ap.next == 0)
			//    {
			//		temp = new Assembler(GetPreDate(ed));
			//    }
			//}

			if (temp != null)
			{
				temp.oldEId = i;
				AllData.Add(temp);
				AddItemCount(ed.protoId);
			}
		}
	}
	public void CopyData(PlanetFactory factory, List<int> id)
	{
		Clear();
		this.factory = factory;
		var fSystem = factory.factorySystem;
		foreach (int i in id)
		{
			CopyBuildData(factory, i);
		}
		CopyMultiLayerBuild(factory, id);
	}

	public IEnumerator CheckBelt(float time)
    {

		yield return new WaitForSeconds(time);
		List<int> temp = new List<int>();
		foreach(var d in CheckBeltData)
        {
			int eid = d.Key;
			var id = factory.entityPool[eid].beltId;
			var data = factory.cargoTraffic.beltPool[id];
			var outBelt = factory.cargoTraffic.beltPool[data.outputId];
			bool flag2;
			int slot;
			factory.ReadObjectConn(eid, 0, out flag2, out int out1, out slot);
			factory.ReadObjectConn(eid, 1, out flag2, out int in1, out slot);
			factory.ReadObjectConn(eid, 2, out flag2, out int in2, out slot);
			factory.ReadObjectConn(eid, 3, out flag2, out int in3, out slot);
			out1 = EIdIsBeltId(out1);
			in1 = EIdIsBeltId(in1);
			in2 = EIdIsBeltId(in2);
			in3 = EIdIsBeltId(in3);
			if (out1 > 0)
			{
				temp.Add(d.Key);
				d.Value.beltOut = out1;
				d.Value.beltIn1 = in1;
				d.Value.beltIn2 = in2;
				d.Value.beltIn3 = in3;
			}
			else if (outBelt.entityId > 0)
            {
				temp.Add(d.Key);
				d.Value.beltOut = out1;
			}
		}
		foreach(int i in temp)
        {
			CheckBeltData.Remove(i);
        }
    }

	private int EIdIsBeltId(int eid)
	{

		if (eid > 0&&factory.entityPool[eid].beltId > 0)
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
		if (p != null)
		{

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
		else
		{
			if (!isInitItem)
				InitItemNeed();
			if (ItemNeed.ContainsKey(0))
			{
				ItemNeed.Remove(0);
			}
			foreach (var d in ItemNeed)
			{

				count2++;
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
	/// 获取建筑主要数据
	/// </summary>
	/// <param name="ed">实体数据</param>
	/// <returns></returns>
	static PrebuildData GetPreDate(EntityData ed)
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
}

