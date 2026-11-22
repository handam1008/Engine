using RYU.Script.Charcter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.PJW._0._4Clean_Up_Code
{
    internal interface IGachaMarkModule:IGachaModule
    {
        void Mark(CharacterDataSO characterDataSO);
    }
}
