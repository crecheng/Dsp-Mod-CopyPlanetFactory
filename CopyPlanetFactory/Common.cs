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
				Debug.LogError("GetEmptyConnError");
				Debug.LogError(e.Message);
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
}

