using BattleTech;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace LoanSharks
{

    public static class ModState
    {
        
        public static string IsSystemActionPatch = null;
        public static int InterestFromLoans = 0;
        public static int DaysInDebt = 0;
        public static int MonthlyExpenditures = 0;

        public static void Reset()
        {
            IsSystemActionPatch = null;
            //InterestFromLoans = 0;
            //DaysInDebt = 0;
            //MonthlyExpenditures = 0;
        }

    }
}