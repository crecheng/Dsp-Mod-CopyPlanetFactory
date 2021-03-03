﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlanetFactoryImg
{
	private Texture2D Img;
	private float x = -1;
	private float y = -1;
	public PlanetFactoryImg()
	{

		Init();
	}

	public void Init()
	{
		Img = new Texture2D(801, 400);
	}

	public Texture2D GetImg(int x, int y,FactoryData data)
	{
		if (this.x == x && this.y == y)
			return Img;
		this.x = x;
		this.y = y;

		Img = new Texture2D(801, 400);
		for (int i = 1; i <= 400; i++)
		{
			Img.SetPixel(401, i, Color.black);
		}
		foreach (var d in data.AssemblerDate)
		{
			SetBuildColor(GetImgPos(d.Pos), new Color(232f / 256f, 253 / 256f, 77 / 256f), -1, 1, -1, 1);
		}
		foreach (var d in data.PowerData)
		{
			SetBuildColor(GetImgPos(d.Pos), new Color(108f / 256f, 2f / 256f, 208f / 256f), -1, 1, -1, 1);
		}
		foreach (var d in data.GammData)
		{
			SetBuildColor(GetImgPos(d.Pos), new Color(108f / 256f, 2f / 256f, 208f / 256f), -2, 2, -2, 2);
		}
		foreach (var d in data.BeltData)
		{
			SetBuildColor(GetImgPos(d.Pos), new Color(24f / 256f, 194 / 256f, 254 / 256f), 0, 1);
		}
		foreach (var d in data.StationData)
		{
			SetBuildColor(GetImgPos(d.Pos), new Color(218f / 256f, 83f / 256f, 2f / 256f), -3, 3, -3, 3);
		}
		foreach (var d in data.LabData)
		{
			SetBuildColor(GetImgPos(d.Pos), Color.white, -2, 2, -2, 2);
		}
		Img.Apply();
		return Img;
	}

	public Texture2D GetImg(int x, int y,PlanetFactory factory)
	{
		if (this.x == x && this.y == y)
			return Img;
		this.x = x;
		this.y = y;

		Img = new Texture2D(801, 400);
        if (factory == null)
        {
			return Img;
        }
		for (int i = 1; i <= 400; i++)
		{
			Img.SetPixel(401, i, Color.black);
		}

		for(int i = 1; i < factory.entityCursor; i++)
        {
			var data = factory.entityPool[i];
			if (data.protoId > 0) 
			{
                if (data.assemblerId > 0)
                {
					SetBuildColor(GetImgPos(data.pos), new Color(232f / 256f, 253 / 256f, 77 / 256f), -1, 1, -1, 1);
				}
				else if (data.powerGenId > 0)
                {
					SetBuildColor(GetImgPos(data.pos), new Color(108f / 256f, 2f / 256f, 208f / 256f), -1, 1, -1, 1);
				}
				else if (data.beltId > 0)
                {
					SetBuildColor(GetImgPos(data.pos), new Color(24f / 256f, 194 / 256f, 254 / 256f), 0, 1);
				}
				else if (data.stationId > 0)
                {
					SetBuildColor(GetImgPos(data.pos), new Color(218f / 256f, 83f / 256f, 2f / 256f), -3, 3, -3, 3);
				}
				else if (data.labId > 0)
                {
					SetBuildColor(GetImgPos(data.pos), Color.white, -2, 2, -2, 2);
				}
			}
        }
		Img.Apply();
		return Img;
	}


	void SetBuildColor(Vector3 pos, Color c, int left = 0, int right = 0, int top = 0, int bottom = 0)
	{
		int x = (int)(pos.z + 200); ;
		if (pos.x < 0)
		{
			x = -x + 801;
		}
		int y = (int)(pos.y + 200);
		FullRect(x, y, c, left, right, top, bottom);
	}

	void FullRect(int x, int y, Color c, int left, int right, int top, int bottom)
	{
		for (int i = left; i <= right; i++)
		{
			for (int j = top; j <= bottom; j++)
			{
				Img.SetPixel(x + i, y + j, c);
			}
		}
	}
	public Vector3 GetImgPos(Vector3 pos)
	{
		Quaternion rot1 = new Quaternion();
		rot1.eulerAngles = new Vector3(0, y, 0);
		pos = rot1 * pos;
		Quaternion rot2 = new Quaternion();
		rot2.eulerAngles = new Vector3(0, 0, x);
		pos = rot2 * pos;
		return pos;
	}
}
