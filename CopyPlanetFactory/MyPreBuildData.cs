using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

public class MyPreBuildData
{
	public PrebuildData pd;
	public int oldEId;
	public int newEId;
	public int preId;
	public bool isBelt;
	public bool isInserter;
	public bool isNewRot;
	public bool isStation;
	public bool isLab;
	public bool isGamm;

	public MyPreBuildData()
    {
		Init();
    }

	public MyPreBuildData(PrebuildData prebuild)
	{
		pd = prebuild;
		Init();
	}
	public void Init()
	{
		isBelt = false;
		isInserter = false;
		oldEId = 0;
		newEId = 0;
		isNewRot = false;
		isStation = false;
		isLab = false;
		isGamm = false;
	}

	public void SetNewRot(FactoryTask factoryTask)
	{
		if (!isNewRot)
		{
			pd.pos = factoryTask.GetNewPos(pd.pos);
			pd.rot = factoryTask.GetNewRot(pd.rot);
			pd.pos2 = factoryTask.GetNewPos(pd.pos2);
			pd.rot2 = factoryTask.GetNewRot(pd.rot2);
			isNewRot = true;
		}
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

	public virtual string  GetData() 
	{
		return "";
	}

	public virtual MyPreBuildData GetCopy()
    {
		return new MyPreBuildData(pd)
		{
			oldEId = this.oldEId,
			newEId = this.newEId
		};
    }
	/// <summary>
	/// 建筑连接传送带行为
	/// </summary>
	/// <param name="factory">工厂实例</param>
	/// <param name="BeltEIdMap">传送带id映射</param>
	/// <returns>是否完成连接</returns>
	public virtual bool ConnBelt(PlanetFactory factory,Dictionary<int,int> BeltEIdMap)
    {
		return false;
    }

	public virtual void SetData(PlanetFactory factory,int eId)
    {

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

