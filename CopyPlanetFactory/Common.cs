using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Common
{

	/// <summary>
	/// 获取空的连接口
	/// </summary>
	/// <param name="eid">eid</param>
	/// <param name="start">连接口开始检索数</param>
	/// <returns>空的端口号</returns>
	public static int GetEmptyConn(PlanetFactory factory, int eid, int start)
	{
		for (; start < 16; start++)
		{
			try
			{
				if (factory.entityConnPool[eid * 16 + start] == 0)
				{
					return start;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("GetEmptyConnError:"+eid);
				Debug.LogError(e.Message+"\n"+e.StackTrace);
			}
		}
		return -1;
	}

	/// <summary>
	/// 找出当前星球上的无连接的爪子
	/// </summary>
	/// <param name="factory"></param>
	/// <returns></returns>
	public static List<int> FindNotConnPaw(PlanetFactory factory)
    {
		//新建列表
		List<int> temp = new List<int>();
		//遍历工厂的全部爪子
        for (int i = 1; i < factory.factorySystem.inserterCursor; i++)
        {
			//获得爪子数据
			var inserter = factory.factorySystem.inserterPool[i];
			//当爪子有一段无连接时，加入列表
            if (inserter.insertTarget == 0 || inserter.pickTarget == 0)
            {
				temp.Add(i);
            }
        }
		return temp;
    }

	public static int FindEmtryPreBeltConn(PlanetFactory factory, int pid, int start)
    {
		for (; start < 4; start++)
		{
			try
			{
				if (factory.prebuildConnPool[pid*16+start] == 0)
				{
					return start;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("GFindEmtryPreBeltConnError:" + pid);
				Debug.LogError(e.Message + "\n" + e.StackTrace);
			}
		}
		return -1;
	}

	/// <summary>
	/// 对接口数据进行解析
	/// </summary>
	/// <param name="num">接口数据</param>
	/// <param name="isOutput">是否是出货</param>
	/// <param name="otherObjId">连接到其他物品的eid</param>
	/// <param name="otherSlot">连接到其他物品端口号0-15</param>
	public static void ReadObjectConn(int num, out bool isOutput, out int otherObjId, out int otherSlot)
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

	public static int FindItem(int ItemId, PlanetFactory factory, Player player)
	{
		if (player.package.GetItemCount(ItemId) > 0)
			return 0;
		for (int i = 1; i < factory.factoryStorage.storageCursor; i++)
		{
			var storage = factory.factoryStorage.storagePool[i];
			if (storage != null)
				if (storage.GetItemCount(ItemId) > 0)
					return i;
		}

		return -1;
	}

	public static int FindItem(int ItemId, Player player)
	{
		int count = player.package.GetItemCount(ItemId);
        if (player.factory != null)
        {
			var factory = player.factory;
			for (int i = 1; i < factory.factoryStorage.storageCursor; i++)
			{
				var storage = factory.factoryStorage.storagePool[i];
				if (storage != null)
					count += storage.GetItemCount(ItemId);
			}
			return count;
		}
        else
			return count;

	}

	public static void FindDisconnectBelt(PlanetFactory factory,List<int> OutNo, List<int> InNo)
    {
		//遍历所有传送带
        for (int i = 1; i < factory.cargoTraffic.beltCursor; i++)
        {
			//获取传送带数据
			var belt = factory.cargoTraffic.beltPool[i];
			var eid = belt.entityId;
			if (belt.outputId == 0 || factory.entityConnPool[eid * 16] == 0)
				OutNo.Add(i);
			if((belt.backInputId==0&&belt.leftInputId==0&&belt.rightInputId==0)||
				(factory.entityConnPool[eid*16+1]==0&& factory.entityConnPool[eid * 16 + 2] == 0 && factory.entityConnPool[eid * 16 + 3] == 0))
            {
				InNo.Add(i);
            }
			
        }
    }

	public static void TakeItem(int ItemId, PlanetFactory factory, Player player, int id)
	{
		if (id == 0)
		{
			player.package.TakeItem(ItemId, 1);
		}
		else if (id > 0)
		{
			var storage = factory.factoryStorage.storagePool[id];
			if (storage != null)
				storage.TakeItem(ItemId, 1);
		}
	}

	/// <summary>
	/// 移除建筑
	/// </summary>
	/// <param name="player">玩家</param>
	/// <param name="factory">工厂</param>
	/// <param name="objId">实体id/预建筑id</param>
	/// <returns></returns>
	public static bool RemoveBuild(Player player, PlanetFactory factory, int objId)
	{
		try
		{
			if (player.package.isFull)
			{
				UIRealtimeTip.Popup(ST.背包不足);
				return false;
			}
			int num = -objId;
			ItemProto itemProto=null;
			if (objId > 0)
			{
				 itemProto= LDB.items.Select((int)factory.entityPool[objId].protoId);
			}
            if (num > 0)
            {
				itemProto = LDB.items.Select((int)factory.prebuildPool[num].protoId);
            }
			int itemId = (itemProto == null) ? 0 : itemProto.ID;
			factory.DestructFinally(player, objId, ref itemId);
			player.package.AddItemStacked(itemId, 1);
			UIItemup.Up(itemId, 1);
			return true;
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
			Debug.LogError(e.StackTrace);
			return false;
		}
	}
}

