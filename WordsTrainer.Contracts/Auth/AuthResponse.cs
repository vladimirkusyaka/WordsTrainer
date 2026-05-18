using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Contracts.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
