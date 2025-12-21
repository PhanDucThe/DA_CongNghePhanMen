using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public class VnPayLibrary
{
    private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value)) _requestData.Add(key, value);
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value)) _responseData.Add(key, value);
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out string retValue) ? retValue : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        StringBuilder data = new StringBuilder();

        foreach (KeyValuePair<string, string> kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                if (data.Length > 0)
                {
                    data.Append("&");
                }

                data.Append(kv.Key + "=" + Utils.JavaUrlEncode(kv.Value));
            }
        }

        string queryString = data.ToString();
        string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, queryString);

        baseUrl += "?" + queryString;
        baseUrl += "&vnp_SecureHash=" + vnp_SecureHash;

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        string rspRaw = GetResponseData();
        string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseData()
    {
        StringBuilder data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType")) _responseData.Remove("vnp_SecureHashType");
        if (_responseData.ContainsKey("vnp_SecureHash")) _responseData.Remove("vnp_SecureHash");

        foreach (KeyValuePair<string, string> kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                if (data.Length > 0)
                {
                    data.Append("&");
                }
                data.Append(kv.Key + "=" + Utils.JavaUrlEncode(kv.Value));
            }
        }
        return data.ToString();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

public class Utils
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }

    public static string JavaUrlEncode(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        string encoded = HttpUtility.UrlEncode(input);

        var regex = new System.Text.RegularExpressions.Regex(@"%[a-f0-9]{2}");
        return regex.Replace(encoded, m => m.Value.ToUpperInvariant());
    }

    public static string GetIpAddress(HttpRequestBase request)
    {
        try
        {
            string ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = request.ServerVariables["REMOTE_ADDR"];
            }
            if (ipAddress == "::1") return "127.0.0.1";
            return ipAddress;
        }
        catch
        {
            return "127.0.0.1";
        }
    }
}