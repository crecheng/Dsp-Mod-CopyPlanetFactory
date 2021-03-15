using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class BeltQueue
{
    private Queue<Belt> belts;

    public List<Stack<Belt>> BeltStack;
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
        BeltStack = new List<Stack<Belt>>();
    }

    public void Clear()
    {
        belts.Clear();
    }

    /// <summary>
    /// 加入数据
    /// </summary>
    /// <param name="belt"></param>
    public void Add(Belt belt)
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

    public Belt Peek()
    {
        return belts.Peek();
    }

    private void ConnBelt(Dictionary<int, Belt> dic, Stack<Belt> b)
    {
        int num = 0;
        do
        {
            num++;
            var b1 = b.Peek();
            // Debug.Log(b1.beltIn1);
            if (b1.beltIn1 > 0 && dic.ContainsKey(b1.beltIn1))
            {
                b.Push(dic[b1.beltIn1]);
                dic.Remove(b1.beltIn1);
                //Debug.Log(b.Count+"--"+dic.Count+"--"+BeltStack.Count);
            }
            else
                break;
        } while (num < 1000000);
    }

    public void Sort()
    {
        if (belts.Count > 0)
        {
            List<Belt> temp = belts.ToList();
            BeltStack.Clear();
            belts.Clear();
            Dictionary<int, Belt> dic = new Dictionary<int, Belt>();
            List<int> otherBelt = new List<int>();
            foreach (var d in temp)
            {
                if (d.beltOut == 0)
                {
                    var lt = new Stack<Belt>();
                    lt.Push(d);
                    BeltStack.Add(lt);
                    continue;
                }
                if(d.HaveOtherBelt(out int b2,out int b3))
                {
                    if (b2 != 0)
                        otherBelt.Add(b2);
                    if (b3 != 0)
                        otherBelt.Add(b3);
                }
                dic.Add(d.oldEId, d);
            }
            foreach(var d in otherBelt)
            {
                if (dic.ContainsKey(d))
                {
                    var lt = new Stack<Belt>();
                    lt.Push(dic[d]);
                    dic.Remove(d);
                    BeltStack.Add(lt);
                }
            }

            foreach (var b in BeltStack)
            {
                ConnBelt(dic, b);
            }
            if (dic.Count > 0)
            {
                var b2 = dic.Values.ToList();
                foreach (var b in b2)
                {
                    if (dic.ContainsKey(b.oldEId))
                    {
                        var st = new Stack<Belt>();
                        st.Push(b);
                        BeltStack.Add(st);
                        dic.Remove(b.oldEId);
                        ConnBelt(dic, st);
                    }
                }
            }
            //temp.Sort((d1, d2) =>
            //{
            //    if (d1.Pos.y == d2.Pos.y)
            //    {
            //        if (d1.Pos.z == d2.Pos.z)
            //        {
            //            return d1.Pos.x.CompareTo(d2.Pos.x);

            //        }
            //        else
            //        {
            //            return d1.Pos.z.CompareTo(d2.Pos.z);
            //        }
            //    }
            //    else
            //    {

            //        return d1.Pos.y.CompareTo(d2.Pos.y);
            //    }
            //});
            //foreach (var d in temp)
            //{
            //    belts.Enqueue(d);
            //}

        }
    }
}
