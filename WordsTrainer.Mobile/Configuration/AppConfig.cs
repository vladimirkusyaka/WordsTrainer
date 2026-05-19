using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Mobile.Configuration
{
    public static class AppConfig
    {
#if DEBUG
        public const string ApiBaseUrl = "http://localhost:5261";
#else
    public const string ApiBaseUrl = "https://YOUR-AZURE-APP.azurewebsites.net";
#endif
    }
}
