using Microsoft.Win32;
using WatchdogControl.Models;

namespace WatchdogControl.Services
{
    public static class DbService
    {
        /// <summary>Список провайдеров БД в системе</summary>
        public static List<Provider> GetProviders()
        {
            var providers = new List<Provider>();

            var rootKey = Registry.ClassesRoot;

            var clsid = rootKey.OpenSubKey("CLSID");

            var clsidSubKeys = clsid?.GetSubKeyNames();

            clsid?.Close();

            foreach (var clsidSubKey in clsidSubKeys)
            {
                var subKeys = rootKey.OpenSubKey("CLSID\\" + clsidSubKey).GetSubKeyNames().ToList();
                rootKey.Close();

                if (!subKeys.Contains("OLE DB Provider") || !subKeys.Contains("ProgID")) continue;

                var keyName = rootKey.OpenSubKey("CLSID\\" + clsidSubKey + "\\ProgID");
                rootKey.Close();

                var keyDesc = rootKey.OpenSubKey("CLSID\\" + clsidSubKey + "\\OLE DB Provider");
                rootKey.Close();

                providers.Add(new Provider()
                {
                    Name = keyName?.GetValue(keyName.GetValueNames()[0]).ToString(),
                    Description = keyDesc?.GetValue(keyDesc.GetValueNames()[0]).ToString()
                });

                keyDesc?.Close();
                keyName?.Close();
            }

            providers.Sort((providerX, providerY) => providerX.Name.CompareTo(providerY.Name));

            return providers;
        }
    }
}
