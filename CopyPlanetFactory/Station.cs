using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Station:MyPreBuildData
{
	public SlotData[] slots;
	public StationStore[] storage;
	public Station(PrebuildData prebuild, StationComponent sc)
	{
		pd = prebuild;
		isStation = true;
		slots = new SlotData[sc.slots.Length];
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = sc.slots[i];

			slots[i].beltId = GameMain.mainPlayer.planetData.factory.cargoTraffic.beltPool[slots[i].beltId].entityId;
		}
		storage = new StationStore[sc.storage.Length];
		for (int i = 0; i < storage.Length; i++)
		{
			storage[i] = sc.storage[i];
			storage[i].count = 0;
		}
	}

	private Station(PrebuildData prebuild)
    {
		pd = prebuild;
		isStation = true;
    }

	public Station(string data)
	{
		pd = default;
		string[] s = data.Split(',');
		if (s.Length > 9)
		{
			isStation = true;

			pd.protoId = short.Parse(s[0]);
			pd.modelIndex = short.Parse(s[1]);
			pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
			pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
			oldEId = int.Parse(s[9]);
			int index = 10;

			int slotLength = int.Parse(s[index++]);

			if (slotLength > 0)
			{
				slots = new SlotData[slotLength];
				for (int i = 0; i < slots.Length; i++)
				{
					//Debug.Log($"{s[index]}{s[index + 1]},{s[index + 2]},{s[index + 3]},{s[index + 4]}{s[index + 5]}");
					index++;
					slots[i] = default;
					slots[i].dir = (IODir)int.Parse(s[index++]);
					slots[i].beltId = int.Parse(s[index++]);
					slots[i].counter = int.Parse(s[index++]);
					slots[i].storageIdx = int.Parse(s[index++]);
					index++;
				}
			}
			//Debug.Log(s[index]);
			int storageLength = int.Parse(s[index++]);
			if (storageLength > 0)
			{
				storage = new StationStore[storageLength];
				for (int i = 0; i < storage.Length; i++)
				{
					//Debug.Log($"{s[index]},{s[index + 1]},{s[index + 2]},{s[index + 3]},{s[index + 4]},{s[index + 5]},{s[index + 6]},{s[index + 7]}");
					index++;
					storage[i] = default;
					storage[i].itemId = int.Parse(s[index++]);
					storage[i].max = int.Parse(s[index++]);
					storage[i].localLogic = (ELogisticStorage)int.Parse(s[index++]);
					storage[i].localOrder = int.Parse(s[index++]);
					storage[i].remoteLogic = (ELogisticStorage)int.Parse(s[index++]);
					storage[i].remoteOrder = int.Parse(s[index++]);
					index++;
				}
			}
		}
	}

    public override string GetData()
    {
		string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
		s += "," + slots.Length;
		for (int i = 0; i < slots.Length; i++)
		{
			var temp = slots[i];
			s += $",[,{(int)temp.dir},{temp.beltId},{temp.counter},{temp.storageIdx},]";
		}
		s += "," + storage.Length;

		for (int i = 0; i < storage.Length; i++)
		{
			var temp = storage[i];
			s += $",[,{temp.itemId},{temp.max},{(int)temp.localLogic},{temp.localOrder},{(int)temp.remoteLogic},{temp.remoteOrder},]";
		}
		return s;
	}

    public override MyPreBuildData GetCopy()
    {
		var temp = new Station(pd);
		temp.slots = new SlotData[slots.Length];
		Array.Copy(slots, temp.slots, slots.Length);
		temp.storage = new StationStore[storage.Length];
		Array.Copy(storage, temp.storage, storage.Length);
		temp.oldEId = oldEId;
		temp.newEId = newEId;
		return temp;
    }
}
