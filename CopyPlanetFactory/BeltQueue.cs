using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class BeltQueue
{
    private Queue<Belt> belts;
    public BeltQueue()
    {
        Init();
    }

    public int Count
    {
        get
        {
            return belts.Count;
        }
    }

    public void Init()
    {
        belts = new Queue<Belt>();
    }

    public void Clear()
    {
        belts.Clear();
    }

    /// <summary>
    /// 加入数据
    /// </summary>
    /// <param name="belt"></param>
    public void Enqueue(Belt belt)
    {
        belts.Enqueue(belt);
    }
    /// <summary>
    /// 数据出队
    /// </summary>
    /// <returns></returns>
    public Belt Dequeue()
    {
        return belts.Dequeue();
    }

    public void Sort()
    {

        if (belts.Count > 0)
        {
            List<Belt> temp = belts.ToList();
            belts.Clear();
            temp.Sort((d1, d2) =>
            {
                if (d1.Pos.y == d2.Pos.y)
                {
                    if (d1.Pos.z == d2.Pos.z)
                    {
                        return d1.Pos.x.CompareTo(d2.Pos.x);

                    }
                    else
                    {
                        return d1.Pos.z.CompareTo(d2.Pos.z);
                    }
                }
                else
                {

                    return d1.Pos.y.CompareTo(d2.Pos.y);
                }
            });
            foreach (var d in temp)
            {
                belts.Enqueue(d);
            }

        }


    }
}
