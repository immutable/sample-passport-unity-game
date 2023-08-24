using UnityEngine;
using System.Collections.Generic;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HyperCasual.Core
{
    public class ApiService {

        public const string TOKEN_TOKEN_ADDRESS = "0x3765D19D5BC39b60718e43B4b12b30e87D383181";

        public async UniTask<bool> MintTokens(int num, string address)
        {
            Debug.Log($"Minting {num} tokens...");
            if (address != null)
            {
                var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("toUserWallet", address),
                    new KeyValuePair<string, string>("number", $"{num}")
                };
                using var client = new HttpClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, "http://localhost:6060/mint/token") { Content = new FormUrlEncodedContent(nvc) };
                using var res = await client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            return false;
        }

        public async UniTask<bool> MintFox(string address)
        {
            Debug.Log("Minting fox...");
            if (address != null)
            {
                var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("toUserWallet", address)
                };
                using var client = new HttpClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, "http://localhost:6060/mint/character") { Content = new FormUrlEncodedContent(nvc) };
                using var res = await client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            return false;
        }

        public async UniTask<bool> MintSkin(string address)
        {
            Debug.Log("Minting skin...");
            if (address != null)
            {
                var nvc = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("toUserWallet", address)
                };
                using var client = new HttpClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, "http://localhost:6060/mint/skin") { Content = new FormUrlEncodedContent(nvc) };
                using var res = await client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            return false;
        }

        public async Task<List<TokenModel>> GetTokens(int numOfTokens, string address)
        {
            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(
                $"https://api.sandbox.x.immutable.com/v1/assets?collection={TOKEN_TOKEN_ADDRESS}&page_size={numOfTokens}&user={address}"
                );
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                ListTokenResponse tokenResponse = JsonConvert.DeserializeObject<ListTokenResponse>(responseBody);
                return tokenResponse.result;
            }
            else
            {
                return new List<TokenModel>();
            }
        }
    }

    public class ListTokenResponse
    {
        public List<TokenModel> result;
    }

    public class TokenModel
    {
        public string token_id;
    }
}