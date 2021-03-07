using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

/// <summary>
/// 工作台
/// </summary>
public class Assembler : MyPreBuildData
{
	public Assembler(PrebuildData prebuild)
    {
		pd = prebuild;
		type = EDataType.Assembler;
	}

	public Assembler(string data)
    {
		pd = default;
		type = EDataType.Assembler;
		string[] s = data.Split(',');
		pd.protoId = short.Parse(s[0]);
		pd.modelIndex = short.Parse(s[1]);
		pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
		pd.pos2 = Vector3.zero;
		pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
		pd.rot2 = Quaternion.identity;
		pd.recipeId = int.Parse(s[9]);
		if (s.Length > 10)
		{
			oldEId = int.Parse(s[10]);
		}
	}

	public override string GetData()
	{
		string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{pd.recipeId},{oldEId}";
		return s;
	}

    public override MyPreBuildData GetCopy()
    {
		return new Assembler(this.pd)
		{
			oldEId = this.oldEId,
			newEId = this.newEId,
		};
    }
}

