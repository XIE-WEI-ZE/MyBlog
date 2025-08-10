using System.Security.Cryptography;
using System.Text;

namespace prjMyBlog.Helpers
{
    public static class TokenUtility
    {
        public static string GenerateToken(string email, string secretKey)
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            string data = $"{email}{timestamp}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            string hashBase64 = Convert.ToBase64String(hash);
            string tokenRaw = $"{email}|{timestamp}|{hashBase64}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenRaw));
        }


        public static bool ValidateToken(string token, string secretKey, out string email)
        {
            email = "";
            try
            {
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                string[] parts = decoded.Split('|');
                if (parts.Length != 3)
                {
                    Console.WriteLine(" token parts 不合法");
                    return false;
                }

                email = parts[0];
                string timestamp = parts[1];
                string hash = parts[2];

                DateTime tokenTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmss", null);
                Console.WriteLine($" Token timestamp: {timestamp} ➜ {tokenTime}");
                Console.WriteLine($" 現在時間 (UTC): {DateTime.UtcNow}");
                Console.WriteLine($" 時間差 (分鐘): {(DateTime.UtcNow - tokenTime).TotalMinutes}");

                if ((DateTime.UtcNow - tokenTime).TotalMinutes > 10)
                {
                    Console.WriteLine("❌ Token 已過期");
                    return false;
                }

                string data = $"{email}{timestamp}";
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
                string expectedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));

                Console.WriteLine($" 驗證中：");
                Console.WriteLine($" email: {email}");
                Console.WriteLine($" data: {data}");
                Console.WriteLine($" 傳入的 hash: {hash}");
                Console.WriteLine($" 計算出的 hash: {expectedHash}");

                bool result = expectedHash == hash;
                Console.WriteLine(result ? "✅ Token 驗證成功" : "❌ Hash 不相符");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ 發生例外: " + ex.Message);
                return false;
            }
        }


    }
}
