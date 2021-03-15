using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// 爪子
/// </summary>
public class Inserter : MyPreBuildData
{
    public int outConn;//inserter
    public int inConn;//pick
    public InserterComponent inserter;
    public Inserter(PrebuildData prebuild, InserterComponent inserter,int Out,int In)
    {
        pd = prebuild;
        isInserter = true;
		type = EDataType.Inserter;
        this.inserter = inserter;
		outConn = Out;
		inConn = In;
    }

    public Inserter(string data)
    {
		pd = default;
		inserter = default;
		isInserter = true;
		type = EDataType.Inserter;
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
            if (s.Length > 23)
            {
				oldEId = int.Parse(s[23]);
            }
		}
	}

    public override string GetData()
    {
        string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w}";
        s += $",{pd.pos2.x},{pd.pos2.y},{pd.pos2.z},{pd.rot2.x},{pd.rot2.y},{pd.rot2.z},{pd.rot2.w},{pd.filterId}";
        s += $",{inserter.pickTarget},{inserter.insertTarget},{inserter.stt},{inserter.delay},{outConn},{inConn},{oldEId}";
        return s;
    }

    public override bool ConnPreBelt(PlanetFactory factory, Dictionary<int, MyPreBuildData> preIdMap)
    {
		Common.ReadObjectConn(outConn, out bool isOut1, out int Belt1, out int slot);
		Common.ReadObjectConn(inConn, out bool isOut2, out int Belt2, out int slot2);
        if (Belt1 == 0 || preIdMap.ContainsKey(Belt1))
        {
            if (Belt2 == 0 || preIdMap.ContainsKey(Belt2))
            {
                if (Belt1 > 0)
                    factory.WriteObjectConn(preId, 0, isOut1, preIdMap[Belt1].preId, slot);
                if (Belt2 > 0)
                    factory.WriteObjectConn(preId, 1, isOut2, preIdMap[Belt2].preId, slot2);
                return true;
            }
        }
        return false;
    }

    public override MyPreBuildData GetCopy()
    {
        return new Inserter(pd,inserter,outConn,inConn) 
		{ 
			oldEId=this.oldEId,
			newEId=this.newEId,
		};
    }
}

