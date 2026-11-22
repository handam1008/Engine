using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.PJW._0._4Clean_Up_Code
{
    internal interface IGachaEnterModule:IGachaModule
    {
        void Init();

        void Enter();
    }
}
