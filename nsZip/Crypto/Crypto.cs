using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using LibHac;

namespace nsZip.Crypto
{
	public static class Crypto
	{
		internal const int Aes128Size = 0x10;
		internal const int Sha256DigestSize = 0x20;

		public static Validity CheckMemoryHashTable(byte[] data, byte[] hash, int offset, int count)
		{
			Validity comp;
			using (var sha = SHA256.Create())
			{
				comp = Util.ArraysEqual(hash, sha.ComputeHash(data, offset, count))
					? Validity.Valid
					: Validity.Invalid;
			}

			return comp;
		}

		public static byte[] ComputeSha256(byte[] data, int offset, int count)
		{
			using (var sha = SHA256.Create())
			{
				return sha.ComputeHash(data, offset, count);
			}
		}

		public static void DecryptEcb(byte[] key, byte[] src, int srcIndex, byte[] dest, int destIndex, int length)
		{
			using (var aes = Aes.Create())
			{
				if (aes == null)
				{
					throw new CryptographicException("Unable to create AES object");
				}

				aes.Key = key;
				aes.Mode = CipherMode.ECB;
				aes.Padding = PaddingMode.None;
				Array.Copy(aes.CreateDecryptor().TransformFinalBlock(src, srcIndex, length), 0, dest, destIndex,
					length);
			}
		}

		public static void DecryptEcb(byte[] key, byte[] src, byte[] dest, int length)
		{
			DecryptEcb(key, src, 0, dest, 0, length);
		}

		public static void DecryptCbc(byte[] key, byte[] iv, byte[] src, int srcIndex, byte[] dest, int destIndex,
			int length)
		{
			using (var aes = Aes.Create())
			{
				if (aes == null)
				{
					throw new CryptographicException("Unable to create AES object");
				}

				aes.Key = key;
				aes.IV = iv;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.None;
				Array.Copy(aes.CreateDecryptor().TransformFinalBlock(src, srcIndex, length), 0, dest, destIndex,
					length);
			}
		}

		public static void DecryptCbc(byte[] key, byte[] iv, byte[] src, byte[] dest, int length)
		{
			DecryptCbc(key, iv, src, 0, dest, 0, length);
		}

		public static void EncryptCbc(byte[] key, byte[] iv, byte[] src, int srcIndex, byte[] dest, int destIndex,
			int length)
		{
			using (var aes = Aes.Create())
			{
				if (aes == null)
				{
					throw new CryptographicException("Unable to create AES object");
				}

				aes.Key = key;
				aes.IV = iv;
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.None;
				Array.Copy(aes.CreateEncryptor().TransformFinalBlock(src, srcIndex, length), 0, dest, destIndex,
					length);
			}
		}

		public static void EncryptCbc(byte[] key, byte[] iv, byte[] src, byte[] dest, int length)
		{
			EncryptCbc(key, iv, src, 0, dest, 0, length);
		}

		public static void GenerateKek(byte[] key, byte[] src, byte[] dest, byte[] kekSeed, byte[] keySeed)
		{
			var kek = new byte[Aes128Size];
			var srcKek = new byte[Aes128Size];

			DecryptEcb(key, kekSeed, kek, Aes128Size);
			DecryptEcb(kek, src, srcKek, Aes128Size);

			if (keySeed != null)
			{
				DecryptEcb(srcKek, keySeed, dest, Aes128Size);
			}
			else
			{
				Array.Copy(srcKek, dest, Aes128Size);
			}
		}

		internal static BigInteger GetBigInteger(byte[] bytes)
		{
			var signPadded = new byte[bytes.Length + 1];
			Buffer.BlockCopy(bytes, 0, signPadded, 1, bytes.Length);
			Array.Reverse(signPadded);
			return new BigInteger(signPadded);
		}

		public static RSAParameters DecryptRsaKey(byte[] encryptedKey, byte[] kek)
		{
			var counter = new byte[0x10];
			Array.Copy(encryptedKey, counter, 0x10);
			var key = new byte[0x230];
			Array.Copy(encryptedKey, 0x10, key, 0, 0x230);

			AesCTR.AesCtrTransform(kek, counter, key, key.Length);

			var d = new byte[0x100];
			var n = new byte[0x100];
			var e = new byte[4];
			Array.Copy(key, 0, d, 0, 0x100);
			Array.Copy(key, 0x100, n, 0, 0x100);
			Array.Copy(key, 0x200, e, 0, 4);

			var dInt = GetBigInteger(d);
			var nInt = GetBigInteger(n);
			var eInt = GetBigInteger(e);

			var rsaParams = RecoverRsaParameters(nInt, eInt, dInt);
			TestRsaKey(rsaParams);
			return rsaParams;
		}

		private static void TestRsaKey(RSAParameters keyParams)
		{
			var rsa = new RSACryptoServiceProvider();
			rsa.ImportParameters(keyParams);

			var test = new byte[] {12, 34, 56, 78};
			var testEnc = rsa.Encrypt(test, false);
			var testDec = rsa.Decrypt(testEnc, false);

			if (!Util.ArraysEqual(test, testDec))
			{
				throw new InvalidDataException("Could not verify RSA key pair");
			}
		}

		public static Validity Rsa2048Pkcs1Verify(byte[] data, byte[] signature, byte[] modulus)
		{
			using (var rsa = RSA.Create())
			{
				rsa.ImportParameters(new RSAParameters {Exponent = new byte[] {1, 0, 1}, Modulus = modulus});

				return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
					? Validity.Valid
					: Validity.Invalid;
			}
		}

		public static Validity Rsa2048PssVerify(byte[] data, byte[] signature, byte[] modulus)
		{
#if NETFRAMEWORK
            if (Compat.IsMono)
            {
                return Compat.Rsa2048PssVerifyMono(data, signature, modulus)
                    ? Validity.Valid
                    : Validity.Invalid;
            }
#endif

			using (RSA rsa = new RSACng())
			{
				rsa.ImportParameters(new RSAParameters {Exponent = new byte[] {1, 0, 1}, Modulus = modulus});

				return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss)
					? Validity.Valid
					: Validity.Invalid;
			}
		}

		public static byte[] DecryptTitleKey(byte[] titleKeyblock, RSAParameters rsaParams)
		{
			// todo: Does this work on Mono?
			RSA rsa = new RSACng();
			rsa.ImportParameters(rsaParams);
			return rsa.Decrypt(titleKeyblock, RSAEncryptionPadding.OaepSHA256);
		}

		private static RSAParameters RecoverRsaParameters(BigInteger n, BigInteger e, BigInteger d)
		{
			using (var rng = RandomNumberGenerator.Create())
			{
				var k = d * e - 1;

				if (!k.IsEven)
				{
					throw new InvalidOperationException("d*e - 1 is odd");
				}

				BigInteger two = 2;
				var t = BigInteger.One;

				var r = k / two;

				while (r.IsEven)
				{
					t++;
					r /= two;
				}

				var rndBuf = n.ToByteArray();

				if (rndBuf[rndBuf.Length - 1] == 0)
				{
					rndBuf = new byte[rndBuf.Length - 1];
				}

				var nMinusOne = n - BigInteger.One;

				var cracked = false;
				var y = BigInteger.Zero;

				for (var i = 0; i < 100 && !cracked; i++)
				{
					BigInteger g;

					do
					{
						rng.GetBytes(rndBuf);
						g = GetBigInteger(rndBuf);
					} while (g >= n);

					y = BigInteger.ModPow(g, r, n);

					if (y.IsOne || y == nMinusOne)
					{
						i--;
						continue;
					}

					for (var j = BigInteger.One; j < t; j++)
					{
						var x = BigInteger.ModPow(y, two, n);

						if (x.IsOne)
						{
							cracked = true;
							break;
						}

						if (x == nMinusOne)
						{
							break;
						}

						y = x;
					}
				}

				if (!cracked)
				{
					throw new InvalidOperationException("Prime factors not found");
				}

				var p = BigInteger.GreatestCommonDivisor(y - BigInteger.One, n);
				var q = n / p;
				var dp = d % (p - BigInteger.One);
				var dq = d % (q - BigInteger.One);
				var inverseQ = ModInverse(q, p);

				var modLen = rndBuf.Length;
				var halfModLen = (modLen + 1) / 2;

				return new RSAParameters
				{
					Modulus = GetBytes(n, modLen),
					Exponent = GetBytes(e, -1),
					D = GetBytes(d, modLen),
					P = GetBytes(p, halfModLen),
					Q = GetBytes(q, halfModLen),
					DP = GetBytes(dp, halfModLen),
					DQ = GetBytes(dq, halfModLen),
					InverseQ = GetBytes(inverseQ, halfModLen)
				};
			}
		}

		private static byte[] GetBytes(BigInteger value, int size)
		{
			var bytes = value.ToByteArray();

			if (size == -1)
			{
				size = bytes.Length;
			}

			if (bytes.Length > size + 1)
			{
				throw new InvalidOperationException(
					$"Cannot squeeze value {value} to {size} bytes from {bytes.Length}.");
			}

			if (bytes.Length == size + 1 && bytes[bytes.Length - 1] != 0)
			{
				throw new InvalidOperationException(
					$"Cannot squeeze value {value} to {size} bytes from {bytes.Length}.");
			}

			Array.Resize(ref bytes, size);
			Array.Reverse(bytes);
			return bytes;
		}

		private static BigInteger ModInverse(BigInteger e, BigInteger n)
		{
			var r = n;
			var newR = e;
			BigInteger t = 0;
			BigInteger newT = 1;

			while (newR != 0)
			{
				var quotient = r / newR;
				BigInteger temp;

				temp = t;
				t = newT;
				newT = temp - quotient * newT;

				temp = r;
				r = newR;
				newR = temp - quotient * newR;
			}

			if (t < 0)
			{
				t = t + n;
			}

			return t;
		}

		public static void CalculateAesCmac(byte[] key, byte[] src, int srcIndex, byte[] dest, int destIndex,
			int length)
		{
			var l = new byte[16];
			EncryptCbc(key, l, l, l, 0x10);
			byte[] paddedMessage;
			var paddedLength = length;

			var firstSubkey = Rol(l);
			if ((l[0] & 0x80) == 0x80)
			{
				firstSubkey[15] ^= 0x87;
			}

			var secondSubkey = Rol(firstSubkey);
			if ((firstSubkey[0] & 0x80) == 0x80)
			{
				secondSubkey[15] ^= 0x87;
			}

			if (length != 0 && length % 16 == 0)
			{
				paddedMessage = new byte[paddedLength];
				Array.Copy(src, srcIndex, paddedMessage, 0, length);

				for (var j = 0; j < firstSubkey.Length; j++)
				{
					paddedMessage[length - 16 + j] ^= firstSubkey[j];
				}
			}
			else
			{
				paddedLength += 16 - length % 16;
				paddedMessage = new byte[paddedLength];
				paddedMessage[length] = 0x80;
				Array.Copy(src, srcIndex, paddedMessage, 0, length);

				for (var j = 0; j < secondSubkey.Length; j++)
				{
					paddedMessage[paddedLength - 16 + j] ^= secondSubkey[j];
				}
			}

			var encResult = new byte[paddedMessage.Length];
			EncryptCbc(key, new byte[16], paddedMessage, encResult, paddedLength);

			Array.Copy(encResult, paddedLength - 0x10, dest, destIndex, 0x10);
		}

		private static byte[] Rol(byte[] b)
		{
			var r = new byte[b.Length];
			byte carry = 0;

			for (var i = b.Length - 1; i >= 0; i--)
			{
				var u = (ushort) (b[i] << 1);
				r[i] = (byte) ((u & 0xff) + carry);
				carry = (byte) ((u & 0xff00) >> 8);
			}

			return r;
		}
	}
}