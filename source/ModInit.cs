using System;
using System.Reflection;
using Newtonsoft.Json;
using BattleTech.UI;
using System.Diagnostics.Eventing.Reader;

namespace LoanSharks
{
    public static class ModInit
    {
        public static LoanSharksSettings Settings = new LoanSharksSettings();
        public const string HarmonyPackage = "us.tbone.LoanSharks";
        public static void Init(string directory, string settingsJSON)
        {
            try
            {
                ModInit.Settings = JsonConvert.DeserializeObject<LoanSharksSettings>(settingsJSON);
            }
            catch (Exception)
            {
                ModInit.Settings = new LoanSharksSettings();
            }
            //var harmony = HarmonyInstance.Create(HarmonyPackage);
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyPackage);
        }
    }
    public class LoanSharksSettings
    {
        public int LoanInterestMultiplier = 100000;
        public int LoanPaybackFee = 100000;
        public string LoanItemDefTypeAndID = "Item.UpgradeDef.Gear_Loan_Million";
        public string FreeMoneyItemDefTypeAndID = "Item.UpgradeDef.Gear_Pirate_Tribute";
        public bool StoreBeforeRemove = true;
        public bool NoNegativeStats = false;
    }
    
}

