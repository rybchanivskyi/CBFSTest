using callback.CBFSFilter;
using CBFSTest.FileUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBFSTest.Tests
{
    public class CBFSFilterTest
    {
        private Cbfilter _mFilter;
        private string _mGuid;
        private string _driverPath;
        private string _fileContent;
        private string _mask;
        private List<string> _fileNames;
        private const int ALTITUDE_FAKE_VALUE_FOR_DEBUG = 360000;
        private const uint ERROR_PRIVILEGE_NOT_HELD = 1314;
        private FilesWorker _fileWorker;
        public uint numberOfFiles;
        public int contentLength; 
        public bool getOriginatorProcessName;
        public bool getOriginatorProcessId;

        public CBFSFilterTest()
        {
            _mFilter = new Cbfilter();
            _mGuid = "{713CC6CE-B3E2-4fd9-838D-E28F558F6866}";
            _driverPath = @"C:\Program Files\Callback Technologies\CBFS Filter 2020 .NET Edition\drivers\cbfilter.cab";
            _mask = @"E:\FilesTest\*";
            _fileWorker = new FilesWorker();
            contentLength = 50;
            _fileContent = _fileWorker.GenerateFileContent(contentLength);
            numberOfFiles = 50000;
            _fileNames = _fileWorker.CreateFileNames(numberOfFiles);
            getOriginatorProcessName = false;
            getOriginatorProcessId = false;
        }

        public bool InstallDriver()
        {
            try
            {
                var reboot = _mFilter.Install(_driverPath, _mGuid, null, Constants.FS_FILTER_MODE_MINIFILTER, ALTITUDE_FAKE_VALUE_FOR_DEBUG, 0);
                if(reboot)
                    Console.WriteLine("Please, reboot the system for the changes to take effect.\nPress any key to Exit.");
                else
                    Console.WriteLine("Driver installed successfully.\n");                
                return reboot;
            }
            catch (CBFSFilterException err)
            {
                if (err.Code == ERROR_PRIVILEGE_NOT_HELD)
                    Console.WriteLine("Installation requires administrator rights. Run the app as administrator.\nPress any key to Exit.");
                else
                    Console.WriteLine($"Installation Error: {err.Message}.\nPress any key to Exit.");
                return true;
            }
        }

        public void WriteDriverInfo()
        {
            var moduleStatus = _mFilter.GetDriverStatus(_mGuid);
            var moduleVersion = (ulong)_mFilter.GetDriverVersion(_mGuid);

            var versionHigh = (uint)(moduleVersion >> 32);
            var versionLow = (uint)(moduleVersion & 0xFFFFFFFF);

            var filterActive = _mFilter.Active;

            Console.WriteLine($"Module status - {moduleStatus};\nModule Version - {moduleVersion};\nVersion High - {versionHigh};\nVersion Low - {versionLow};\nFilter active - {(filterActive ? "Active" : "Deactivated")};\n");
        }
        public bool UninstallDriver()
        {
            try
            {
                var reboot = _mFilter.Uninstall(_driverPath, _mGuid, null, 0);
                if (reboot)
                    Console.WriteLine("Please, reboot the system for the changes to take effect.\nPress any key to Exit.");
                else
                    Console.WriteLine("Driver uninstalled successfully.\n");
                return reboot;
            }
            catch (CBFSFilterException err)
            {
                if (err.Code == ERROR_PRIVILEGE_NOT_HELD)
                    Console.WriteLine("Uninstallation requires administrator rights. Run the app as administrator.\nPress any key to Exit.");
                else
                    Console.WriteLine($"Uninstallation Error: {err.Message}.\nPress any key to Exit.");
                return true;
            }
        }

        public void ActivateFilter()
        {
            try
            {
                _mFilter.Initialize(_mGuid);
                _mFilter.Config("AllowFileAccessInBeforeOpen=false");
                _mFilter.StartFilter(5000);
                _mFilter.FileFlushingBehavior = 0;
            }
            catch (CBFSFilterException err)
            {
                Console.WriteLine($"Set filter Error: {err.Message}");
            }
        }

        public void StopFilter()
        {
            try
            {
                _mFilter.StopFilter(true);
            }
            catch (CBFSFilterException err)
            {
                Console.WriteLine($"Delete filter Error: {err.Message}");
            }
        }

        public void RunFileOperations()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Start();
            _fileWorker.CreateFiles(_fileNames, _fileContent);
            watch.Stop();
            Console.WriteLine($"Create - {watch.ElapsedMilliseconds} ms.");

            watch.Restart();
            watch.Start();
            _fileWorker.ReadFromFiles(_fileNames);
            watch.Stop();
            Console.WriteLine($"Read - {watch.ElapsedMilliseconds} ms.");

            watch.Restart();
            watch.Start();
            _fileWorker.EnumerateDirectory();
            watch.Stop();
            Console.WriteLine($"Enumeration - {watch.ElapsedMilliseconds} ms.");

            _fileWorker.DeleteFiles(_fileNames);
        }

        public void InstalledFilterTest()
        {
            Console.WriteLine($"<<Installed Filter Test>>\nNumber of files = {numberOfFiles}. Content lenght = {_fileContent.Length}");
            if (_mFilter.Active)
                StopFilter();
            RunFileOperations();
            Console.WriteLine($"<<End Test>>\n");
        }

        public void ActiveFilterTest()
        {
            Console.WriteLine($"<<Activated Filter Test>>\nNumber of files = {numberOfFiles}. Content lenght = {_fileContent.Length}");
            ActivateFilter();
            RunFileOperations();
            Console.WriteLine($"<<End Test>>\n");
        }

        public void TestFilterWithRules(Dictionary<string, bool> filterRules)
        {
            WriteTestInfo(filterRules);
            SetEventHandlers();
            SetRules(filterRules);
            RunFileOperations();
            Console.WriteLine("<<End Test>>\n");
        }

        private void WriteTestInfo(Dictionary<string, bool> filterRules)
        {
            var dictionaryStr = string.Empty;
            foreach(var rule in filterRules)
            {
                dictionaryStr += $"Rule Action - {rule.Key}, Active - {(rule.Value ? "YES" : "NO")};\n";
            }
            Console.WriteLine("<<Rules Test Info>>");
            Console.WriteLine(dictionaryStr);
            Console.WriteLine($"GetOriginatorProcessName Activated - {(getOriginatorProcessName ? "YES" : "NO")}");
            Console.WriteLine($"GetOriginatorProcessId Activated - {(getOriginatorProcessId ? "YES" : "NO")}");
            Console.WriteLine($"Number of files = {numberOfFiles}. Content lenght = {_fileContent.Length}");
        }

        private void SetRules(Dictionary<string, bool> filterRules)
        {
            _mFilter.DeleteAllFilterRules();
            foreach(var rule in filterRules)
            {
                if(rule.Value)
                {
                    try
                    {
                        var ruleFlag = GetRuleFlagByName(rule.Key);
                        if(ruleFlag.ToString() != "Error")
                            _mFilter.AddFilterRule(_mask, 0, Constants.FS_CE_NONE, ruleFlag.Value);

                    }
                    catch (CBFSFilterException err)
                    {
                        Console.WriteLine($"Add rule Error : {err.Message}");
                    }
                }
            }            
        }

        private RuleFlag GetRuleFlagByName(string name)
        {
            switch (name)
            {
                case "OnBeforeCreateFile":
                    return new RuleFlag("Control: BeforeCreate", Constants.FS_CE_BEFORE_CREATE);
                case "OnBeforeWriteFile":
                    return new RuleFlag("Control: BeforeWrite", Constants.FS_CE_BEFORE_WRITE);
                case "OnBeforeReadFile":
                    return new RuleFlag("Control: BeforeRead", Constants.FS_CE_BEFORE_READ);
                case "OnBeforeCleanupFile":
                    return new RuleFlag("Control: BeforeCleanup", Constants.FS_CE_BEFORE_CLEANUP);
                case "OnAfterEnumerateDirectory":
                    return new RuleFlag("Control: AfterEnumerateDirectory", Constants.FS_CE_AFTER_ENUMERATE_DIRECTORY);
                default:
                    Console.WriteLine($"Rule Flag not found by Name = {name}");
                    return new RuleFlag("Error", 0);

            }

        }

        public void SetEventHandlers()
        {
            _mFilter.OnBeforeCreateFile += Filter_OnBeforeCreateFile;
            _mFilter.OnBeforeOpenFile += Filter_OnBeforeOpenFile;
            _mFilter.OnBeforeWriteFile += Filter_OnBeforeWriteFile;
            _mFilter.OnBeforeReadFile += Filter_OnBeforeReadFile;
            _mFilter.OnBeforeCleanupFile += Filter_OnBeforeCleanupFile;
            _mFilter.OnAfterEnumerateDirectory += Filter_OnAfterEnumerateDirectory;
        }

        #region EventHandlers
        private void Filter_OnAfterEnumerateDirectory(object sender, CbfilterAfterEnumerateDirectoryEventArgs e)
        {
            if (getOriginatorProcessId)
                _mFilter.GetOriginatorProcessId();
            if (getOriginatorProcessName)
                _mFilter.GetOriginatorProcessName();
        }

        private void Filter_OnBeforeCleanupFile(object sender, CbfilterBeforeCleanupFileEventArgs e)
        {
            if (getOriginatorProcessId)
                _mFilter.GetOriginatorProcessId();
            if (getOriginatorProcessName)
                _mFilter.GetOriginatorProcessName();
        }

        private void Filter_OnBeforeReadFile(object sender, CbfilterBeforeReadFileEventArgs e)
        {
            if (getOriginatorProcessId)
                _mFilter.GetOriginatorProcessId();
            if (getOriginatorProcessName)
                _mFilter.GetOriginatorProcessName();
        }

        private void Filter_OnBeforeWriteFile(object sender, CbfilterBeforeWriteFileEventArgs e)
        {
            if (getOriginatorProcessId)
                _mFilter.GetOriginatorProcessId();
            if (getOriginatorProcessName)
                _mFilter.GetOriginatorProcessName();
        }

        private void Filter_OnBeforeOpenFile(object sender, CbfilterBeforeOpenFileEventArgs e)
        {
            if (getOriginatorProcessId)
                _mFilter.GetOriginatorProcessId();
            if (getOriginatorProcessName)
                _mFilter.GetOriginatorProcessName();
        }

        private void Filter_OnBeforeCreateFile(object sender, CbfilterBeforeCreateFileEventArgs e)
        {
            if (getOriginatorProcessId)
                _mFilter.GetOriginatorProcessId();
            if (getOriginatorProcessName)
                _mFilter.GetOriginatorProcessName();
        }
        #endregion

        #region Rule flag

        struct RuleFlag
        {
            public readonly string Caption;
            public readonly long Value;

            public RuleFlag(string caption, long value)
            {
                Caption = caption;
                Value = value;
            }

            public override string ToString()
            {
                return Caption;
            }
        }      

        #endregion
    }
}
