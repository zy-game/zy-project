using System;
using System.Security.Cryptography;
using System.Text;

public static class ApiSignature
{
    public static string GenerateSignature(string appKey, string appSecret, string apiUrl, string httpMethod, 
        SortedDictionary<string, string> queryParams, SortedDictionary<string, string> postParams)
    {
        // 1. 将查询字符串和 POST 请求参数一起排序，并拼接到一起
        var sortedParams = new SortedDictionary<string, string>();
        if (queryParams != null)
        {
            foreach (var pair in queryParams)
            {
                sortedParams[pair.Key] = pair.Value;
            }
        }
        if (postParams != null)
        {
            foreach (var pair in postParams)
            {
                sortedParams[pair.Key] = pair.Value;
            }
        }
        var sb = new StringBuilder();
        foreach (var pair in sortedParams)
        {
            sb.Append($"{pair.Key}{pair.Value}");
        }
        var rawParams = sb.ToString();
        // 2. 生成签名原始串
        var signatureString = $"{httpMethod.ToUpper()}{apiUrl}{rawParams}";

        // 3. 计算签名
        var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
        var signatureBytes = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(signatureString));
        var signature = Convert.ToBase64String(signatureBytes);
        return signature;
    }
}
