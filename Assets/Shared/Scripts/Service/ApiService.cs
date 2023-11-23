using UnityEngine;
using System.Collections.Generic;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using HyperCasual.Runner;
using System.Linq;
using System;

namespace HyperCasual.Core
{
    public class ApiService {

        public const string TOKEN_TOKEN_ADDRESS = "";
        public const string ZK_TOKEN_TOKEN_ADDRESS = "";
        public const string SKIN_TOKEN_ADDRESS = "";
        public const string ZK_SKIN_TOKEN_ADDRESS = "";
        private const string SERVER_BASE_URL = "http://localhost:6060";

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
                string url = SaveManager.Instance.ZkEvm ? $"{SERVER_BASE_URL}/zkmint/token": $"{SERVER_BASE_URL}/mint/token";
                using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
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
                string url = SaveManager.Instance.ZkEvm ? $"{SERVER_BASE_URL}/zkmint/character": $"{SERVER_BASE_URL}/mint/character";
                using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
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
                string url = SaveManager.Instance.ZkEvm ? $"{SERVER_BASE_URL}/zkmint/skin": $"{SERVER_BASE_URL}/mint/skin";
                using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
                using var res = await client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            return false;
        }

        public async Task<List<TokenModel>> GetTokens(int numOfTokens, string address)
        {
            using var client = new HttpClient();
            string url = SaveManager.Instance.ZkEvm ? 
            $"https://api.sandbox.immutable.com/v1/chains/imtbl-zkevm-testnet/accounts/{address}/nfts?contract_address={ZK_TOKEN_TOKEN_ADDRESS}" 
            : $"https://api.sandbox.x.immutable.com/v1/assets?collection={TOKEN_TOKEN_ADDRESS}&page_size={numOfTokens}&user={address}";
            Debug.Log($"Get Tokens url: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log($"Get Tokens response: {responseBody}");
                ListTokenResponse tokenResponse = JsonUtility.FromJson<ListTokenResponse>(responseBody);
                List<TokenModel> tokens = new List<TokenModel>();
                tokens.AddRange(tokenResponse.result);
                return tokens;
            }
            else
            {
                return new List<TokenModel>();
            }
        }

        public async Task<List<TokenModel>> GetSkin(string address)
        {
            using var client = new HttpClient();
            string url = SaveManager.Instance.ZkEvm ?
            $"https://api.sandbox.immutable.com/v1/chains/imtbl-zkevm-testnet/accounts/{address}/nfts?contract_address={ZK_SKIN_TOKEN_ADDRESS}" 
            : $"https://api.sandbox.x.immutable.com/v1/assets?collection={SKIN_TOKEN_ADDRESS}&page_size=1&user={address}";
            Debug.Log($"Get Skin url: {url}");
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log($"Get Skin response: {responseBody}");
                ListTokenResponse tokenResponse = JsonUtility.FromJson<ListTokenResponse>(responseBody);
                List<TokenModel> tokens = new List<TokenModel>();
                tokens.AddRange(tokenResponse.result);
                return tokens;
            }
            else
            {
                return new List<TokenModel>();
            }
        }

        public async Task<string?> GetTokenCraftSkinEcodedData(string tokenId1, string tokenId2, string tokenId3)
        {
            var nvc = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("tokenId1", tokenId1),
                new KeyValuePair<string, string>("tokenId2", tokenId2),
                new KeyValuePair<string, string>("tokenId3", tokenId3)
            };
            using var client = new HttpClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{SERVER_BASE_URL}/zk/token/craftskin/encodeddata") { Content = new FormUrlEncodedContent(nvc) };
            using var res = await client.SendAsync(req);
            
            string responseBody = await res.Content.ReadAsStringAsync();
            EncodedDataResponse encodedDataResponse = JsonUtility.FromJson<EncodedDataResponse>(responseBody);
            return encodedDataResponse.data;
        }

        public async Task<string?> GetSkinCraftSkinEcodedData(string tokenId)
        {
            var nvc = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("tokenId", tokenId)
            };
            using var client = new HttpClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{SERVER_BASE_URL}/zk/skin/craftskin/encodeddata") { Content = new FormUrlEncodedContent(nvc) };
            using var res = await client.SendAsync(req);

            string responseBody = await res.Content.ReadAsStringAsync();
            EncodedDataResponse encodedDataResponse = JsonUtility.FromJson<EncodedDataResponse>(responseBody);
            return encodedDataResponse.data;
        }
    }

    [Serializable]
    public class ListTokenResponse
    {
        public TokenModel[] result;
    }

    [Serializable]
    public class TokenModel
    {
        public string token_id;
    }

    public class EncodedDataResponse
    {
        public string data;
    }
}