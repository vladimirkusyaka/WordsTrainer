using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Mobile.Configuration
{
    public static class AppConfig
    {
#if DEBUG
        public const string ApiBaseUrl = "https://wordstrainer-production.up.railway.app";
#else
    public const string ApiBaseUrl = "https://wordstrainer-production.up.railway.app";
#endif
    }
}
