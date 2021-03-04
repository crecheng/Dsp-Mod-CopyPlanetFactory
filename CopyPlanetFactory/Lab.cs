using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Lab:MyPreBuildData
{
	public bool isResearchMode;
	public int LabRecpId;
	public int LabTech;
	public Lab(PrebuildData prebuild, bool isResMode, int RecpId, int TachId)
	{
		Init();
		pd = prebuild;
		isLab = true;
		type = EDataType.Lab;
		isResearchMode = isResMode;
		LabRecpId = RecpId;
		LabTech = TachId;
	}

	public Lab(string data)
    {
		pd = default;
		string[] s = data.Split(',');
		if (s.Length > 10)
		{
			isLab = true;
			type = EDataType.Lab;
			pd.protoId = short.Parse(s[0]);
			pd.modelIndex = short.Parse(s[1]);
			pd.pos = new Vector3(float.Parse(s[2]), float.Parse(s[3]), float.Parse(s[4]));
			pd.pos2 = Vector3.zero;
			pd.rot = new Quaternion(float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]), float.Parse(s[8]));
			pd.rot2 = Quaternion.identity;
			pd.recipeId = int.Parse(s[9]);
			oldEId = int.Parse(s[10]);
			isResearchMode = bool.Parse(s[11]);
			LabRecpId = int.Parse(s[12]);
			LabTech = int.Parse(s[13]);
		}
	}

    public override string GetData()
    {
		string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{pd.recipeId},{oldEId}";
		s += $",{isResearchMode},{LabRecpId},{LabTech}";
		return s;
	}

    public override void SetData(PlanetFactory factory, int eId)
    {
		var labId = factory.entityPool[eId].labId;
		factory.factorySystem.labPool[labId].SetFunction(isResearchMode, LabRecpId, LabTech, factory.entitySignPool);

	}
	public override MyPreBuildData GetCopy()
    {
        return new Lab(pd,isResearchMode,LabRecpId,LabTech) 
		{
			oldEId=oldEId,
			newEId=newEId
		};
    }
}

