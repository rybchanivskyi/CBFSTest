using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using callback.CBFSFilter;
using CBFSTest.Tests;

namespace CBFSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var testFilter = new CBFSFilterTest();
                var installDrivers = true;
                var filterRules = new Dictionary<string, bool>(5);
                filterRules.Add("OnBeforeCreateFile", false);
                filterRules.Add("OnBeforeWriteFile", false);
                filterRules.Add("OnBeforeReadFile", false);
                filterRules.Add("OnBeforeCleanupFile", false);
                filterRules.Add("OnAfterEnumerateDirectory", false);
                if (installDrivers)
                {
                    var reeboot = testFilter.InstallDriver();
                    if (reeboot)
                    {
                        Console.ReadKey();
                        return;
                    }
                }
                WriteCPUInfo();
                testFilter.WriteDriverInfo();

                //a
                testFilter.InstalledFilterTest();
                //б
                testFilter.ActiveFilterTest();
                //в

                for(int i = 0; i < 5; ++i)
                {
                    SetDictionaryValues(filterRules, i == 0, i == 1, i == 2, i == 3, i == 4);
                    testFilter.TestFilterWithRules(filterRules);
                }

                ChangeAllDictionaryValues(filterRules, true);
                testFilter.TestFilterWithRules(filterRules);
                //задати варіації правил можна задопомогою методу SetDictionaryValues               

                //г
                //Вказуємо чи викликають event handler функціїї полями getOriginatorProcessId та getOriginatorProcessName
                //по замовчування поля мають значення false
                testFilter.getOriginatorProcessId = true;
                testFilter.TestFilterWithRules(filterRules);

                testFilter.getOriginatorProcessId = false;
                testFilter.getOriginatorProcessName = true;                
                testFilter.TestFilterWithRules(filterRules);

                testFilter.getOriginatorProcessId = true;
                testFilter.getOriginatorProcessName = true;
                testFilter.TestFilterWithRules(filterRules);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred : {ex.Message}");
            }
        }

        public static void WriteCPUInfo()
        {
            using (ManagementObjectSearcher win32Proc = new ManagementObjectSearcher("select * from Win32_Processor"))
            {
                foreach (ManagementObject obj in win32Proc.Get())
                {
                    var clockSpeed = (uint)obj["CurrentClockSpeed"];
                    var procName = obj["Name"].ToString();
                    var manufacturer = obj["Manufacturer"].ToString();
                    var version = obj["Version"].ToString();
                    Console.WriteLine($"Name - {procName}, Version - {(string.IsNullOrEmpty(version) ? "N/A" : version)}, CurrentClockSpeed - {(clockSpeed / (double)1000)}GHz, Manufacturer - {manufacturer}.\n");
                }
            }
        }

        public static void SetDictionaryValues(Dictionary<string, bool> rules, bool onBeforeCreateFile, bool onBeforeWriteFile, bool onBeforeReadFile, bool onBeforeCleanupFile, bool onAfterEnumerateDirectory)
        {
            rules["OnBeforeCreateFile"] = onBeforeCreateFile;
            rules["OnBeforeWriteFile"] = onBeforeWriteFile;
            rules["OnBeforeReadFile"] = onBeforeReadFile;
            rules["OnBeforeCleanupFile"] = onBeforeCleanupFile;
            rules["OnAfterEnumerateDirectory"] = onAfterEnumerateDirectory;
        }

        public static void ChangeAllDictionaryValues(Dictionary<string, bool> rules, bool value)
        {
            var keys = rules.Keys.ToList();
            foreach (var key in keys)
            {
                rules[key] = value;
            }
        }
    }
}
