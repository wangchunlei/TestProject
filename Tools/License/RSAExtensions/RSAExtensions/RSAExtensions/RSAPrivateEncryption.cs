//************************************************************************************
// RSAPrivateEncryption (RSAPrivateEncryption extension) Class Version 2.00
//
// Copyright (c) 2012 Dudi Bedner
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, provided that the above
// copyright notice(s) and this permission notice appear in all copies of
// the Software and that both the above copyright notice(s) and this
// permission notice appear in supporting documentation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
// OF THIRD PARTY RIGHTS. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
// HOLDERS INCLUDED IN THIS NOTICE BE LIABLE FOR ANY CLAIM, OR ANY SPECIAL
// INDIRECT OR CONSEQUENTIAL DAMAGES, OR ANY DAMAGES WHATSOEVER RESULTING
// FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT,
// NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION
// WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
// DO NOT TRUST THIS CLASS FOR ENCRYPTION OF COMMERCIAL, PRIVATE OR ANY KIND OF SECRETS!
//
// Disclaimer
// ----------
// Although reasonable care has been taken to ensure the correctness of this
// implementation, this code should never be used in any application without
// proper verification and testing.  I disclaim all liability and responsibility
// to any person or entity with respect to any loss or damage caused, or alleged
// to be caused, directly or indirectly, by the use of this RSAEncryption class.
// 
//************************************************************************************



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Numerics;

namespace RSAExtensions
{
    public static class RSAPrivateEncryption
    {
        public static byte[] PrivareEncryption(this RSACryptoServiceProvider rsa, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (rsa.PublicOnly)
                throw new InvalidOperationException("Private key is not loaded");

            int maxDataLength = (rsa.KeySize / 8) - 6;
            if (data.Length > maxDataLength)
                throw new ArgumentOutOfRangeException("data", string.Format(
                    "Maximum data length for the current key size ({0} bits) is {1} bytes (current length: {2} bytes)",
                    rsa.KeySize, maxDataLength, data.Length));

            // Add 4 byte padding to the data, and convert to BigInteger struct
            BigInteger numData = GetBig(AddPadding(data));

            RSAParameters rsaParams = rsa.ExportParameters(true);
            BigInteger D = GetBig(rsaParams.D);
            BigInteger Modulus = GetBig(rsaParams.Modulus);
            BigInteger encData = BigInteger.ModPow(numData, D, Modulus);
            
            return encData.ToByteArray();
        }

        public static byte[] PublicDecryption(this RSACryptoServiceProvider rsa, byte[] cipherData)
        {
            if (cipherData == null)
                throw new ArgumentNullException("cipherData");

            BigInteger numEncData = new BigInteger(cipherData);

            RSAParameters rsaParams = rsa.ExportParameters(false);
            BigInteger Exponent = GetBig(rsaParams.Exponent);
            BigInteger Modulus = GetBig(rsaParams.Modulus);

            BigInteger decData = BigInteger.ModPow(numEncData, Exponent, Modulus);

            byte[] data = decData.ToByteArray();
            byte[] result = new byte[data.Length - 1];
            Array.Copy(data, result, result.Length);
            result = RemovePadding(result);

            Array.Reverse(result);
            return result;
        }

        private static BigInteger GetBig(byte[] data)
        {
            byte[] inArr = (byte[])data.Clone();
            Array.Reverse(inArr);  // Reverse the byte order
            byte[] final = new byte[inArr.Length + 1];  // Add an empty byte at the end, to simulate unsigned BigInteger (no negatives!)
            Array.Copy(inArr, final, inArr.Length);

            return new BigInteger(final);
        }

        // Add 4 byte random padding, first bit *Always On*
        private static byte[] AddPadding(byte[] data)
        {
            Random rnd = new Random();
            byte[] paddings = new byte[4];
            rnd.NextBytes(paddings);
            paddings[0] = (byte)(paddings[0] | 128);

            byte[] results = new byte[data.Length + 4];

            Array.Copy(paddings, results, 4);
            Array.Copy(data, 0, results, 4, data.Length);
            return results;
        }

        private static byte[] RemovePadding(byte[] data)
        {
            byte[] results = new byte[data.Length - 4];
            Array.Copy(data, results, results.Length);
            return results;
        }

    }


}
