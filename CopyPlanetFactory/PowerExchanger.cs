using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class PowerExchanger : MyPreBuildData
{
    int c0, c1, c2, c3;

    public PowerExchanger(PrebuildData prebuild,int conn0,int conn1,int conn2, int conn3)
    {
        pd = prebuild;
        isNeedConn = true;
        type = EDataType.PowerExchanger;
        c0 = conn0;
        c1 = conn1;
        c2 = conn2;
        c3 = conn3;
    }

    public override MyPreBuildData GetCopy()
    {
        return new PowerExchanger(pd, c0, c1, c2, c3)
        {
            oldEId = oldEId,
            newEId=newEId
        };
    }

    public override string GetData()
    {
        return base.GetData();
    }
}

