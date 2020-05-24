using Harmony;
using System;
using System.Reflection;
using Newtonsoft.Json;

namespace LoanSharks
{
    public static class ModInit
    {
        public static LoanSharksSettings Settings;
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
            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    public class LoanSharksSettings
    {
        public int LoanInterestMultiplier = 10000;
        public int LoanPaybackFee = 10000;
    }
    
}

