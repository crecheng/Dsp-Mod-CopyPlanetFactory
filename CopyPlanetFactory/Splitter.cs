using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 四分器
/// </summary>
public class Splitter : MyPreBuildData
{
    int c0;
    int c1;
    int c2;
    int c3;

    public Splitter(PrebuildData prebuild, int c0, int c1, int c2, int c3)
    {
        pd = prebuild;
        isSplitter = true;
        isNeedConn = true;
        isAfterSet = true;
        type = EDataType.Splitter;
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        this.c3 = c3;
    }

    public Splitter(string data)
    {
        pd = default;
        string[] s = data.Split(',');
        isSplitter = true;
        isNeedConn = true;
        isAfterSet = true;
        type = EDataType.Splitter;
        if (s.Length > 10)
        {
            isGamm = true;
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
        }
    }

    public override MyPreBuildData GetCopy()
    {
        return new Splitter(pd, c0, c1, c2, c3)
        {
            oldEId = oldEId,
            newEId = newEId,
            isNeedConn = true
        };
    }

    public override string GetData()
    {
        string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
        s += $",{c0},{c1},{c2},{c3}";
        return s;
    }

    public override void Export(BinaryWriter w)
    {
        ExportBaesData(w);
        w.Write(c0);
        w.Write(c1);
        w.Write(c2);
        w.Write(c3);
    }

    public override void SetData(PlanetFactory factory, int eId)
    {
        int sId = factory.entityPool[eId].splitterId;
        //读取接口数据
        factory.ReadObjectConn(eId, 0, out bool out0, out int belt0, out int slot0);
        factory.ReadObjectConn(eId, 1, out bool out1, out int belt1, out int slot1);
        factory.ReadObjectConn(eId, 2, out bool out2, out int belt2, out int slot2);
        factory.ReadObjectConn(eId, 3, out bool out3, out int belt3, out int slot3);
        //对数据进行连接
        if (belt0 > 0)
        {
            int beltId = factory.entityPool[belt0].beltId;
            factory.cargoTraffic.ConnectToSplitter(sId, beltId, 0, !out0);
        }
        if (belt1 > 0)
        {
            int beltId = factory.entityPool[belt1].beltId;
            factory.cargoTraffic.ConnectToSplitter(sId, beltId, 1, !out1);
        }
        if (belt2 > 0)
        {
            int beltId = factory.entityPool[belt2].beltId;
            factory.cargoTraffic.ConnectToSplitter(sId, beltId, 2, !out2);
        }
        if (belt3 > 0)
        {
            int beltId = factory.entityPool[belt3].beltId;
            factory.cargoTraffic.ConnectToSplitter(sId, beltId, 3, !out3);
        }
    }

    public override bool ConnPreBelt(PlanetFactory factory, Dictionary<int, MyPreBuildData> preIdMap)
    {
        Common.ReadObjectConn(c0, out bool isOut0, out int Belt0, out int slot0);
        Common.ReadObjectConn(c1, out bool isOut1, out int Belt1, out int slot1);
        Common.ReadObjectConn(c2, out bool isOut2, out int Belt2, out int slot2);
        Common.ReadObjectConn(c3, out bool isOut3, out int Belt3, out int slot3);
        if ((Belt0 == 0 || preIdMap.ContainsKey(Belt0)) &&
            (Belt1 == 0 || preIdMap.ContainsKey(Belt1)) &&
            (Belt2 == 0 || preIdMap.ContainsKey(Belt2)) &&
            (Belt3 == 0 || preIdMap.ContainsKey(Belt3)))
        {
            if (Belt0 > 0)
                factory.WriteObjectConn(preId, 0, isOut0, preIdMap[Belt0].preId, isOut0 ? 1 : 0);
            if (Belt1 > 0)
                factory.WriteObjectConn(preId, 1, isOut1, preIdMap[Belt1].preId, isOut1 ? 1 : 0);
            if (Belt2 > 0)
                factory.WriteObjectConn(preId, 2, isOut2, preIdMap[Belt2].preId, isOut2 ? 1 : 0);
            if (Belt3 > 0)
                factory.WriteObjectConn(preId, 3, isOut3, preIdMap[Belt3].preId, isOut3 ? 1 : 0);
            return true;
        }
        return false;
    }

}



