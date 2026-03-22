using Supabase;

namespace ElectronicsStore.Customer.Service.Singleton
{
    public sealed class SupabaseProvider
    {
        private static Supabase.Client _instance;
        private static readonly object _lock = new object();

        private SupabaseProvider() { }

        public static Supabase.Client GetInstance(string url, string key)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var options = new SupabaseOptions { AutoConnectRealtime = true };
                        _instance = new Supabase.Client(url, key, options);
                    }
                }
            }
            return _instance;
        }
    }
}