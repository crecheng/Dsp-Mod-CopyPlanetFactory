using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// 锅盖(射线接收器)
/// </summary>
public class Gamm : MyPreBuildData
{
    /// <summary>
    /// 接收方式(生产物品id)
    /// </summary>
    int produceId;
    int conn0;
    int conn1;
    public Gamm(PrebuildData prebuild,int ProduceId,int conn0,int conn1)
    {
        isGamm = true;
        isNeedConn = true;
        type = EDataType.Gamm;
        pd = prebuild;
        this.conn0 = conn0;
        this.conn1 = conn1;
        this.produceId = ProduceId;
    }

    public Gamm(string data)
    {
        pd = default;
        string[] s = data.Split(',');
        if (s.Length > 10)
        {
            isGamm = true;
            isNeedConn = true;
            type = EDataType.Gamm;
            pd.protoId = short.Parse(s[0]);
            pd.modelIndex = short.Parse(s[1]);
            pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
            pd.pos2 = Vector3.zero;
            pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
            pd.rot2 = Quaternion.identity;
            oldEId = int.Parse(s[9]);
            produceId = int.Parse(s[10]);
            conn0 = int.Parse(s[11]);
            conn1 = int.Parse(s[12]);
        }
    }


    public override MyPreBuildData GetCopy()
    {
        return new Gamm(pd, produceId, conn0, conn1)
        {
            oldEId = oldEId,
            newEId = newEId
        };
    }

    public override void SetData(PlanetFactory factory, int eId)
    {
        factory.powerSystem.genPool[factory.entityPool[eId].powerGenId].productId = produceId;
    }

    public override bool ConnPreBelt(PlanetFactory factory, Dictionary<int, MyPreBuildData> preIdMap)
    {
        Common.ReadObjectConn(conn0, out bool isOut1, out int Belt1, out int slot);
        Common.ReadObjectConn(conn1, out bool isOut2, out int Belt2, out int slot2);
        if (Belt1 == 0 || preIdMap.ContainsKey(Belt1))
        {
            if (Belt2 == 0 || preIdMap.ContainsKey(Belt2))
            {
                if (Belt1 > 0)
                    factory.WriteObjectConn(preId, 0, isOut1, preIdMap[Belt1].preId, isOut1 ? 1 : 0);
                if (Belt2 > 0)
                    factory.WriteObjectConn(preId, 1, isOut2, preIdMap[Belt2].preId, isOut2 ? 1 : 0);
                return true;
            }
        }
        return false;
    }

    public override string GetData()
    {
        string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
        s += $",{produceId},{conn0},{conn1}";
        return s;
    }

    public override void Export(BinaryWriter w)
    {
        ExportBaesData(w);
        w.Write(produceId);
        w.Write(conn0);
        w.Write(conn1);
    }
}

