using System;

namespace Spectrum.Bootstrap
{
    public static class Updater
    {
        public static object ManagerObject;

        public static void UpdateManager()
        {
            try
            {
                ManagerObject?.GetType().GetMethod("UpdateExtensions").Invoke(ManagerObject, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[STAGE 1] Spectrum: Can't update extensions. Read below:\n{ex}");
            }
        }
    }
}
