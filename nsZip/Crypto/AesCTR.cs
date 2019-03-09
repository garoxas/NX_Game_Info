using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace nsZip.Crypto
{
	internal class AesCTR
	{
		public static byte[] AesCtrTransform(
			byte[] key, byte[] salt, byte[] input, int length)
		{
			var output = new byte[length];

			SymmetricAlgorithm aes =
				new AesManaged {Mode = CipherMode.ECB, Padding = PaddingMode.None};

			aes.BlockSize = 128;
			var blockSize = aes.BlockSize / 8;

			if (salt.Length != blockSize)
			{
				throw new ArgumentException(
					string.Format(
						"Salt size must be same as block size (actual: {0}, expected: {1})",
						salt.Length, blockSize));
			}

			var counter = (byte[]) salt.Clone();

			var xorMask = new Queue<byte>();

			var zeroIv = new byte[blockSize];
			var counterEncryptor = aes.CreateEncryptor(key, zeroIv);

			for (var pos = 0; pos < length; ++pos)
			{
				if (xorMask.Count == 0)
				{
					var counterModeBlock = new byte[blockSize];

					counterEncryptor.TransformBlock(
						counter, 0, counter.Length, counterModeBlock, 0);

					for (var i2 = counter.Length - 1; i2 >= 0; i2--)
					{
						if (++counter[i2] != 0)
						{
							break;
						}
					}

					foreach (var b2 in counterModeBlock)
					{
						xorMask.Enqueue(b2);
					}
				}

				var mask = xorMask.Dequeue();
				output[pos] = (byte) (input[pos] ^ mask);
			}

			return output;
		}
	}
}