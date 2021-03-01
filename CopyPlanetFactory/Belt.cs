using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

    public override string GetData()
    {
		string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{oldEId}";
		s += $",{beltOut},{beltIn1},{beltIn2},{beltIn3}";
		return s;
	}

    public override MyPreBuildData GetCopy()
    {
        return new Belt(pd, belt, beltOut, beltIn1, beltIn2, beltIn3)
		{
			newEId=this.newEId,
			oldEId=this.oldEId
		};
    }
}

