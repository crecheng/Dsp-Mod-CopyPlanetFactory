using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;


public class RectImg
{
    int x1;
    int x2;
    int y1;
    int y2;
    Texture2D Img;
    public static int imgh = 400;
    public RectImg()
    {
        Img = new Texture2D(imgh*2+1, imgh);
    }

    public void Clear()
    {
        Img = new Texture2D(imgh * 2 + 1, imgh);
        int w = imgh * 2 + 1;
        for (int i = 0; i <w; i++)
        {
            for(int j = 0; j < imgh; j++)
            {
                Img.SetPixel(i, j, new Color(0, 0, 0, 0));
            }
        }
    }

    public Texture2D getRect(int rectx1,int rectx2,int recty1,int recty2)
    {
        if (rectx1 == x1 && rectx2 == x2 && recty1 == y1 && recty2 == y2)
        {
            return Img;
        }
        x1 = rectx1;
        x2 = rectx2;
        y1 = recty1;
        y2 = recty2;
        Clear();
        int tx1 = Math.Min(rectx1, rectx2);
        int tx2 = Math.Max(rectx1, rectx2);
        int ty1 = Math.Min(recty1, recty2);
        int ty2 = Math.Max(recty1, recty2);
        for(int i = tx1; i < tx2; i++)
        {
            Img.SetPixel(i, ty1, Color.red);
            Img.SetPixel(i, ty2, Color.red);
        }
        for (int j = ty1; j < ty2; j++)
        {
            Img.SetPixel(tx1, j, Color.red);
            Img.SetPixel(tx2,j, Color.red);
        }
        Img.Apply();
        return Img;
    }


}
