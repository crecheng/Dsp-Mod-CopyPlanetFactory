using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// 建筑数据的父类
/// </summary>
public class MyPreBuildData
{
	/// <summary>
	/// 预建筑数据
	/// </summary>
	public PrebuildData pd;
	/// <summary>
	/// 复制过来的老的eid
	/// </summary>
	public int oldEId;
	/// <summary>
	/// 新建好后的eid
	/// </summary>
	public int newEId;
	/// <summary>
	/// 预建筑id
	/// </summary>
	public int preId;
	public bool isBelt;
	public bool isInserter;
	public bool isNewRot;
	public bool isStation;
	public bool isLab;
	public bool isGamm;
	public bool isSplitter;
	public bool isNeedConn;
	public bool isCancel;
	public bool isAfterSet;
	/// <summary>
	/// 建筑类型，由子类赋值
	/// </summary>
	public EDataType type;

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
		isNeedConn = false;
		isSplitter = false;
		isCancel = false;
		isAfterSet = false;
		type = EDataType.Null;
	}

	/// <summary>
	/// 设置旋转后的旋转数据
	/// </summary>
	/// <param name="factoryTask">工厂任务</param>
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

	/// <summary>
	/// 获取保存数据的string
	/// </summary>
	/// <returns></returns>
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
	/// 预建筑连接传送带行为
	/// </summary>
	/// <param name="factory">工厂实例</param>
	/// <param name="BeltEIdMap">传送带id映射</param>
	/// <returns>是否完成连接</returns>
	public virtual bool ConnPreBelt(PlanetFactory factory, Dictionary<int, MyPreBuildData> preIdMap)
	{
		return true;
	}


	/// <summary>
	/// 设置数据
	/// </summary>
	/// <param name="factory">工厂实例</param>
	/// <param name="eId">eid</param>
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

