using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 研究所
/// </summary>
public class Lab:MyPreBuildData
{
	public bool isResearchMode;
	public int LabRecpId;
	public int LabTech;
	/// <summary>
	/// 在其上一层的研究所eid
	/// </summary>
	public int nextLab;
	public Lab(PrebuildData prebuild, bool isResMode, int RecpId, int TachId,int next=0)
	{
		Init();
		pd = prebuild;
		isLab = true;
		type = EDataType.Lab;
		isNeedConn = true;
		isResearchMode = isResMode;
		LabRecpId = RecpId;
		LabTech = TachId;
		nextLab = next;
	}

	public Lab(string data)
    {
		pd = default;
		string[] s = data.Split(',');
		if (s.Length > 10)
		{
			isLab = true;
			isNeedConn = true;
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
			
            if (s.Length > 14)
            {
				nextLab = int.Parse(s[14]);
            }
		}
	}

    public override string GetData()
    {
		string s = $"{ pd.protoId},{pd.modelIndex},{pd.pos.x},{pd.pos.y},{pd.pos.z},{pd.rot.x},{pd.rot.y},{pd.rot.z},{pd.rot.w},{pd.recipeId},{oldEId}";
		s += $",{isResearchMode},{LabRecpId},{LabTech},{nextLab}";
		return s;
	}

    public override bool ConnPreBelt(PlanetFactory factory, Dictionary<int, MyPreBuildData> preIdMap)
    {
		if (nextLab == 0)
			return true;
        if (nextLab > 0)
        {
            if (preIdMap.ContainsKey(nextLab))
            {
				factory.WriteObjectConn(preId, 15, true, preIdMap[nextLab].preId, 14);
				return true;
            }
        }
		return false;
    }

    public override void Export(BinaryWriter w)
    {
		ExportBaesData(w);
		w.Write(pd.recipeId);
		w.Write(isResearchMode);
		w.Write(LabRecpId);
		w.Write(LabTech);
    }

    public override void SetData(PlanetFactory factory, int eId)
    {
		var labId = factory.entityPool[eId].labId;
		factory.factorySystem.labPool[labId].SetFunction(isResearchMode, LabRecpId, LabTech, factory.entitySignPool);

	}


	public override MyPreBuildData GetCopy()
    {
        return new Lab(pd,isResearchMode,LabRecpId,LabTech,nextLab) 
		{
			oldEId=oldEId,
			newEId=newEId
		};
    }
}

