using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 分馏器
/// </summary>
public class Fractionate : MyPreBuildData
{
    int c0;
    int c1;
    int c2;
    public Fractionate(PrebuildData prebuild,int c0,int c1,int c2)
    {
        pd = prebuild;
        isNeedConn = true;
        type = EDataType.Fractionate;
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
    }

    public Fractionate(string data)
    {
        pd = default;
        string[] s = data.Split(',');
        if (s.Length > 10)
        {
            isNeedConn = true;
            type =EDataType.Fractionate;
            pd.protoId = short.Parse(s[0]);
            pd.modelIndex = short.Parse(s[1]);
            pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
            pd.pos2 = Vector3.zero;
            pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
            pd.rot2 = Quaternion.identity;
            oldEId = int.Parse(s[9]);
            c0 = int.Parse(s[10]);
            c1 = int.Parse(s[11]);
            c2 = int.Parse(s[12]);
        }
    }

    public override MyPreBuildData GetCopy()
    {
        return new Fractionate(pd, c0, c1, c2) 
        { 
            oldEId=oldEId,
            newEId=newEId
        };
    }

    public override bool ConnBelt(PlanetFactory factory, Dictionary<int, int> BeltEIdMap)
    {

        Common.ReadObjectConn(c0, out bool isOut0, out int Belt0, out int slot0);
        Common.ReadObjectConn(c1, out bool isOut1, out int Belt1, out int slot1);
        Common.ReadObjectConn(c2, out bool isOut2, out int Belt2, out int slot2);
        if ((Belt1 == 0 || BeltEIdMap.ContainsKey(Belt1)) &&
            (Belt2 == 0 || BeltEIdMap.ContainsKey(Belt2)) &&
            (Belt0 == 0 || BeltEIdMap.ContainsKey(Belt0)))
        {
            int fId = factory.entityPool[newEId].fractionateId;
            if (Belt0 > 0)
            {
                int belt = BeltEIdMap[Belt0];
                int beltId = factory.entityPool[belt].beltId;
                factory.WriteObjectConn(newEId, 0, isOut0,belt , isOut0 ? 1 : 0);
                factory.factorySystem.SetFractionateBelt(fId, beltId, 0, isOut0);
            }
            if (Belt1 > 0)
            {
                int belt = BeltEIdMap[Belt1];
                int beltId = factory.entityPool[belt].beltId;
                factory.WriteObjectConn(newEId, 1, isOut1, belt, isOut1 ? 1 : 0);
                factory.factorySystem.SetFractionateBelt(fId, beltId, 1, isOut1);
            }
            if (Belt2 > 0)
            {
                int belt = BeltEIdMap[Belt2];
                int beltId = factory.entityPool[belt].beltId;
                factory.WriteObjectConn(newEId, 2, isOut2, belt, isOut2 ? 1 : 0);
                factory.factorySystem.SetFractionateBelt(fId, beltId, 2, isOut2);
            }
            return true;
        }
        return false;
    }

    public override bool ConnPreBelt(PlanetFactory factory, Dictionary<int, MyPreBuildData> preIdMap)
    {
        Common.ReadObjectConn(c0, out bool isOut0, out int Belt0, out int slot0);
        Common.ReadObjectConn(c1, out bool isOut1, out int Belt1, out int slot1);
        Common.ReadObjectConn(c2, out bool isOut2, out int Belt2, out int slot2);
        if ((Belt1 == 0 || preIdMap.ContainsKey(Belt1)) &&
            (Belt2 == 0 || preIdMap.ContainsKey(Belt2)) &&
            (Belt0 == 0 || preIdMap.ContainsKey(Belt0)))
        {
            int fId = factory.entityPool[newEId].fractionateId;
            if (Belt0 > 0)
            {
                var d = preIdMap[Belt0];
                factory.WriteObjectConn(preId, 0, isOut0, d.preId, isOut0 ? 1 : 0);
            }
            if (Belt1 > 0)
            {
                var d = preIdMap[Belt1];
                factory.WriteObjectConn(preId, 1, isOut1, d.preId, isOut1 ? 1 : 0);
            }
            if (Belt2 > 0)
            {
                var d = preIdMap[Belt2];
                factory.WriteObjectConn(preId, 2, isOut2, d.preId, isOut2 ? 1 : 0);
            }
            return true;
        }
        return false;
    }



    public override string GetData()
    {
        string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
        s += $",{c0},{c1},{c2}";
        return s;
    }
}

