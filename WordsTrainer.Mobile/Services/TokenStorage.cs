using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Mobile.Services
{
    public class TokenStorage
    {
        private const string AccessTokenKey = "access_token";

        public async Task SaveAccessTokenAsync(string token)
        {
            await SecureStorage.SetAsync(AccessTokenKey, token);
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            return await SecureStorage.GetAsync(AccessTokenKey);
        }

        public void Clear()
        {
            SecureStorage.Remove(AccessTokenKey);
        }
    }
}
