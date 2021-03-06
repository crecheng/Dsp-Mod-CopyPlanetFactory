using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PowerExchanger : MyPreBuildData
{
    int c0, c1, c2, c3;
    float state;

    public PowerExchanger(PrebuildData prebuild, int conn0, int conn1, int conn2, int conn3,float state)
    {
        pd = prebuild;
        isNeedConn = true;
        type = EDataType.PowerExchanger;
        c0 = conn0;
        c1 = conn1;
        c2 = conn2;
        c3 = conn3;
        this.state = state;
    }
    public PowerExchanger(string data)
    {
        pd = default;
        string[] s = data.Split(',');
        if (s.Length > 10)
        {
            isNeedConn = true;
            type = EDataType.PowerExchanger;
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
            c3 = int.Parse(s[13]);
            state = float.Parse(s[14]);
        }

    }
    public override bool ConnBelt(PlanetFactory factory, Dictionary<int, int> BeltEIdMap)
    {
        Common.ReadObjectConn(c0, out bool isOut0, out int Belt0, out int slot0);
        Common.ReadObjectConn(c1, out bool isOut1, out int Belt1, out int slot1);
        Common.ReadObjectConn(c2, out bool isOut2, out int Belt2, out int slot2);
        Common.ReadObjectConn(c3, out bool isOut3, out int Belt3, out int slot3);
        if ((Belt0 == 0 || BeltEIdMap.ContainsKey(Belt0)) &&
            (Belt1 == 0 || BeltEIdMap.ContainsKey(Belt1)) &&
            (Belt2 == 0 || BeltEIdMap.ContainsKey(Belt2)) &&
            (Belt3 == 0 || BeltEIdMap.ContainsKey(Belt3)))
        {
            int exId = factory.entityPool[newEId].powerExcId;
            if (Belt0 > 0)
            {
                int beltEid = BeltEIdMap[Belt0];
                int beltId = factory.entityPool[beltEid].beltId;
                factory.WriteObjectConn(newEId, 0, isOut0, beltEid, isOut0 ? 1 : 0);
                factory.powerSystem.SetExchangerBelt(exId, beltId, 0, isOut0);
            }
            else
            {
                factory.powerSystem.SetExchangerBelt(exId, 0, 0, false);
            }
            if (Belt1 > 0)
            {
                int beltEid = BeltEIdMap[Belt1];
                int beltId = factory.entityPool[beltEid].beltId;
                factory.WriteObjectConn(newEId, 1, isOut1, beltEid, isOut1 ? 1 : 0);
                factory.powerSystem.SetExchangerBelt(exId, beltId, 1, isOut1);
            }
            else
            {
                factory.powerSystem.SetExchangerBelt(exId, 0,1, false);
            }
            if (Belt2 > 0)
            {
                int beltEid = BeltEIdMap[Belt2];
                int beltId = factory.entityPool[beltEid].beltId;
                factory.WriteObjectConn(newEId, 2, isOut2, beltEid, isOut2 ? 1 : 0);
                factory.powerSystem.SetExchangerBelt(exId, beltId, 2, isOut2);
            }
            else
            {
                factory.powerSystem.SetExchangerBelt(exId, 0, 2, false);
            }
            if (Belt3 > 0)
            {
                int beltEid = BeltEIdMap[Belt3];
                int beltId = factory.entityPool[beltEid].beltId;
                factory.WriteObjectConn(newEId, 3, isOut3, beltEid, isOut3 ? 1 : 0);
                factory.powerSystem.SetExchangerBelt(exId, beltId, 3, isOut3);
            }
            else
            {
                factory.powerSystem.SetExchangerBelt(exId, 0, 3, false);
            }
            return true;
        }
        return false;
    }

    public override void SetData(PlanetFactory factory, int eId)
    {
        factory.powerSystem.excPool[factory.entityPool[eId].powerExcId].targetState = state;
    }
    public override MyPreBuildData GetCopy()
    {
        return new PowerExchanger(pd, c0, c1, c2, c3, state)
        {
            oldEId = oldEId,
            newEId = newEId,
        };
    }

    public override string GetData()
    {
        string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
        s += $",{c0},{c1},{c2},{c3},{state}";
        return s;
    }
}

