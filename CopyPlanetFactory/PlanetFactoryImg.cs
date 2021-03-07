using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlanetFactoryImg
{
	private Texture2D Img;
	private float x = -1;
	private float y = -1;
	public static int imgh = 400;
	public PlanetFactoryImg()
	{

		Init();
	}

	public void Init()
	{
		Img = new Texture2D(imgh*2+1, imgh);
	}


	/// <summary>
	/// 获取蓝图预览
	/// </summary>
	/// <param name="x">x旋转</param>
	/// <param name="y">y旋转</param>
	/// <param name="data">数据</param>
	/// <returns></returns>
	public Texture2D GetImg(int x, int y,FactoryData data)
	{
		if (this.x == x && this.y == y)
			return Img;
		this.x = x;
		this.y = y;
		return Fresh(data);
	}


	/// <summary>
	/// 刷新预览
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public Texture2D Fresh(FactoryData data)
    {
		Img = new Texture2D(imgh * 2 + 1, imgh);
		for (int i = 1; i <= imgh; i++)
		{
			Img.SetPixel(imgh + 1, i, Color.black);
		}
		List<Vector3> belt = new List<Vector3>();
		foreach (var d in data.AllData)
		{
			switch (d.type)
			{
				case EDataType.Assembler:
					SetBuildColor(GetImgPos(d.Pos), new Color(232f / 256f, 253 / 256f, 77 / 256f), -1, 1, -1, 1);
					break;
				case EDataType.Belt:
					belt.Add(d.Pos);
					break;
				case EDataType.Lab:
					SetBuildColor(GetImgPos(d.Pos), Color.white, -2, 2, -2, 2);
					break;
				case EDataType.Station:
					SetBuildColor(GetImgPos(d.Pos), new Color(218f / 256f, 83f / 256f, 2f / 256f), -3, 3, -3, 3);
					break;
				case EDataType.PowGen:
					SetBuildColor(GetImgPos(d.Pos), new Color(108f / 256f, 2f / 256f, 208f / 256f), -1, 1, -1, 1);
					break;
				case EDataType.Gamm:
					SetBuildColor(GetImgPos(d.Pos), new Color(108f / 256f, 2f / 256f, 208f / 256f), -2, 2, -2, 2);
					break;
			}
		}
		foreach (var d in belt)
		{
			SetBuildColor(GetImgPos(d), new Color(24f / 256f, 194 / 256f, 254 / 256f), 0, 1);
		}
		Img.Apply();
		return Img;
	}

	/// <summary>
	/// 获取星球预览
	/// </summary>
	/// <param name="x">x旋转</param>
	/// <param name="y">y旋转</param>
	/// <param name="factory">数据</param>
	/// <returns></returns>
	public Texture2D GetImg(int x, int y,PlanetFactory factory)
	{
		if (this.x == x && this.y == y)
			return Img;
		this.x = x;
		this.y = y;
		return Fresh(factory);
		
	}

	/// <summary>
	/// 刷新星球预览
	/// </summary>
	/// <param name="factory"></param>
	/// <returns></returns>
	public Texture2D Fresh(PlanetFactory factory)
    {
		Img = new Texture2D(imgh * 2 + 1, imgh);
		if (factory == null)
		{
			return Img;
		}
		for (int i = 1; i <= imgh; i++)
		{
			Img.SetPixel(imgh + 1, i, Color.black);
		}

		for (int i = 1; i < factory.entityCursor; i++)
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

	/// <summary>
	/// 筛选出选中区域的建筑id
	/// </summary>
	/// <param name="factory">数据</param>
	/// <param name="Id">筛选出的id</param>
	/// <param name="x1">矩形点1</param>
	/// <param name="x2">矩形点2</param>
	/// <param name="y1">矩形点3</param>
	/// <param name="y2">矩形点4</param>
	public void SelectBuild(PlanetFactory factory,List<int> Id,int x1,int x2,int y1,int y2)
    {
		int left = Math.Min(x1, x2);
		int right = Math.Max(x1, x2);
		int top = Math.Min(y1, y2);
		int bottom = Math.Max(y1, y2);
		for (int i = 1; i < factory.entityCursor; i++)
		{
			var data = factory.entityPool[i];
			if (data.protoId > 0)
			{
                if (BuildIsInRect(data.pos, left, right, top, bottom))
                {
					Id.Add(i);
                }
			}
		}
	}

	/// <summary>
	/// 当前建筑位置是否在矩形内
	/// </summary>
	/// <param name="pos">建筑位置</param>
	/// <param name="left">矩形左边界</param>
	/// <param name="right">矩形右边界</param>
	/// <param name="top">矩形上边界</param>
	/// <param name="bottom">矩形下边界</param>
	/// <returns>是否在矩形内</returns>
	bool BuildIsInRect(Vector3 pos,int left,int right,int top,int bottom)
    {
		var p = GetImgPos(pos);
		int x = (int)(p.z + imgh/2);
		if (p.x < 0)
		{
			x = -x + imgh*2+1;
		}
		int y = (int)(p.y + imgh/2);
        if (x >= left && x <= right && y >= top && y <= bottom)
        {
			return true;
        }
        else
        {
			return false;
        }
	}

	/// <summary>
	/// 设置预览图像素
	/// </summary>
	/// <param name="pos">建筑位置</param>
	/// <param name="c">颜色</param>
	/// <param name="left">像素矩阵左边界</param>
	/// <param name="right">像素矩阵右边界</param>
	/// <param name="top">像素矩阵上边界</param>
	/// <param name="bottom">像素矩阵下边界</param>
	void SetBuildColor(Vector3 pos, Color c, int left = 0, int right = 0, int top = 0, int bottom = 0)
	{
		int x = (int)(pos.z + imgh / 2); ;
		if (pos.x < 0)
		{
			x = -x + imgh * 2+1;
		}
		int y = (int)(pos.y + imgh / 2);
		FullRect(x, y, c, left, right, top, bottom);
	}

	/// <summary>
	/// 填充矩形
	/// </summary>
	/// <param name="x">中心x</param>
	/// <param name="y">中心y</param>
	/// <param name="c">填充颜色</param>
	/// <param name="left">填充上边界</param>
	/// <param name="right">填充下边界</param>
	/// <param name="top">填充左边界</param>
	/// <param name="bottom">填充右边界</param>
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

	/// <summary>
	/// 获取旋转以后的位置
	/// </summary>
	/// <param name="pos">原位置</param>
	/// <returns>旋转后位置</returns>
	public Vector3 GetImgPos(Vector3 pos)
	{
		Quaternion rot1 = new Quaternion();
		rot1.eulerAngles = new Vector3(0, y, 0);
		pos = rot1 * pos;
		Quaternion rot2 = new Quaternion();
		rot2.eulerAngles = new Vector3(0, 0, x);
		pos = rot2 * pos;
		pos *= imgh / 400f;
		return pos;
	}
}

