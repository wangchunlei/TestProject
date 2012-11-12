using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace License
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create message and signature on your end
            string message = "Here is the license message";
            var secret = "wangchunlei";
            var converter = new ASCIIEncoding();
            byte[] plainText = converter.GetBytes(secret);

            var rsaWrite = new RSACryptoServiceProvider();
            var privateParams = rsaWrite.ExportParameters(true);

            // Generate the public key / these can be sent to the user.
            var publicParams = rsaWrite.ExportParameters(false);

            byte[] signature =
                rsaWrite.SignData(plainText, new SHA1CryptoServiceProvider());

            // Verify from the user's side. Note that only the public parameters
            // are needed.
            var rsaRead = new RSACryptoServiceProvider();
            rsaRead.ImportParameters(publicParams);
            if (rsaRead.VerifyData(plainText,
                                new SHA1CryptoServiceProvider(),
                                signature))
            {
                Console.WriteLine("Verified!");
            }
            else
            {
                Console.WriteLine("NOT verified!");
            }
        }
    }
}
