using System.Security.Cryptography;
using System.Text;

namespace HRMS.Infrastructure.Services
{
    /// <summary>
    /// Contract for 2FA Google Authenticator TOTP operations.
    /// </summary>
    public interface ITwoFactorAuthService
    {
        string GenerateSecretKey();
        string GenerateQrCodeUri(string email, string secretKey);
        bool ValidateTotpCode(string secretKey, string code);
    }

    /// <summary>
    /// Implementation of Google Authenticator (TOTP RFC 6238) 2FA setup and verification.
    /// </summary>
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public string GenerateSecretKey()
        {
            var bytes = new byte[20];
            RandomNumberGenerator.Fill(bytes);
            return ToBase32String(bytes);
        }

        public string GenerateQrCodeUri(string email, string secretKey)
        {
            var encodedEmail = Uri.EscapeDataString(email);
            var encodedIssuer = Uri.EscapeDataString("HRMS");
            return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&digits=6";
        }

        public bool ValidateTotpCode(string secretKey, string code)
        {
            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code) || code.Length != 6)
            {
                return false;
            }

            var secretBytes = FromBase32String(secretKey);
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = unixTimestamp / 30;

            // Check current time step and +/- 1 step drift
            for (var i = -1; i <= 1; i++)
            {
                var generatedCode = ComputeTotp(secretBytes, timeStep + i);
                if (generatedCode == code)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ComputeTotp(byte[] secret, long timeStep)
        {
            var timeBytes = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeBytes);
            }

            using var hmac = new HMACSHA1(secret);
            var hash = hmac.ComputeHash(timeBytes);

            var offset = hash[^1] & 0x0F;
            var binaryCode = ((hash[offset] & 0x7F) << 24)
                           | ((hash[offset + 1] & 0xFF) << 16)
                           | ((hash[offset + 2] & 0xFF) << 8)
                           | (hash[offset + 3] & 0xFF);

            var otp = binaryCode % 1_000_000;
            return otp.ToString("D6");
        }

        private static string ToBase32String(byte[] input)
        {
            var output = new StringBuilder();
            var bitBuffer = 0;
            var bitCount = 0;

            foreach (var b in input)
            {
                bitBuffer = (bitBuffer << 8) | b;
                bitCount += 8;
                while (bitCount >= 5)
                {
                    output.Append(Base32Alphabet[(bitBuffer >> (bitCount - 5)) & 31]);
                    bitCount -= 5;
                }
            }

            if (bitCount > 0)
            {
                output.Append(Base32Alphabet[(bitBuffer << (5 - bitCount)) & 31]);
            }

            return output.ToString();
        }

        private static byte[] FromBase32String(string input)
        {
            var cleanInput = input.Trim().ToUpperInvariant().Replace("=", string.Empty);
            var output = new List<byte>();
            var bitBuffer = 0;
            var bitCount = 0;

            foreach (var c in cleanInput)
            {
                var val = Base32Alphabet.IndexOf(c);
                if (val < 0) continue;

                bitBuffer = (bitBuffer << 5) | val;
                bitCount += 5;

                if (bitCount >= 8)
                {
                    output.Add((byte)((bitBuffer >> (bitCount - 8)) & 255));
                    bitCount -= 8;
                }
            }

            return output.ToArray();
        }
    }
}
