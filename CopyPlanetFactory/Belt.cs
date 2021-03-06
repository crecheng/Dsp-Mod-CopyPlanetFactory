using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 传送带
/// </summary>
public class Belt : MyPreBuildData
{
	public BeltComponent belt;
	public int beltOut;
	public int beltIn1;
	public int beltIn2;
	public int beltIn3;

	public Belt(PrebuildData prebuild, BeltComponent belt, int out1, int in1, int in2, int in3)
	{
		pd = prebuild;
		Init();
		this.belt = belt;
		type = EDataType.Belt;
		beltOut = out1;
		beltIn1 = in1;
		beltIn2 = in2;
		beltIn3 = in3;
		isBelt = true;
	}

	public Belt(string data)
	{
		pd = default;
		isBelt = true;
		type = EDataType.Belt;
		string[] s = data.Split(',');
		if (s.Length > 13)
		{
			pd.protoId = short.Parse(s[0]);
			pd.modelIndex = short.Parse(s[1]);
			pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
			pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
			oldEId = int.Parse(s[9]);
			beltOut = int.Parse(s[10]);
			beltIn1 = int.Parse(s[11]);
			beltIn2 = int.Parse(s[12]);
			beltIn3 = int.Parse(s[13]);
		}
	}

	public bool HaveBeletIn()
    {
		if (beltIn1 != 0|| beltIn2 != 0||beltIn3!=0)
			return true;
		return false;
    }

	public bool HaveOtherBelt(out int b1,out int b2)
    {
		int count = 0;
		b1 = 0;
		b2 = 0;
		if (beltIn2 != 0)
			count++;
		b1 = beltIn2;
		if (beltIn3 != 0)
			count++;
		b2 = beltIn3;
		return count > 0;
		
    }

	public override string GetData()
	{
		string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
		s += $",{beltOut},{beltIn1},{beltIn2},{beltIn3}";
		return s;
	}

    public override void Export(BinaryWriter w)
    {
		ExportBaesData(w);
		w.Write(beltOut);
		w.Write(beltIn1);
		w.Write(beltIn2);
		w.Write(beltIn3);
	}


    public override bool ConnPreBelt(PlanetFactory factory, Dictionary<int, MyPreBuildData> preIdMap)
	{
		if (beltOut == 0 || preIdMap.ContainsKey(beltOut))
		{
			if (beltOut>0&& preIdMap.ContainsKey(beltOut))
			{
				var other = preIdMap[beltOut];
				if (other.isBelt)
				{
					int otherSlot = Common.FindEmtryPreBeltConn(factory, -other.preId, 1);
					if (otherSlot > 0)
						factory.WriteObjectConn(preId, 0, true, other.preId, otherSlot);
				}
			}
		}
		return false;
	}
    public override MyPreBuildData GetCopy()
	{
		return new Belt(pd, belt, beltOut, beltIn1, beltIn2, beltIn3)
		{
			newEId = this.newEId,
			oldEId = this.oldEId
		};
	}
}

