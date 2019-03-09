using System.IO;
using System.Security.Cryptography;
using XTSSharp;

namespace nsZip.Crypto
{
	internal class CryptoInitialisers
	{
		public static byte[] GenerateRandomKey(int Length)
		{
			var RandomKey = new byte[Length];
			var RNG = new RNGCryptoServiceProvider();
			RNG.GetBytes(RandomKey);
			return RandomKey;
		}

		public static byte[] GenSHA256Hash(byte[] Data)
		{
			var SHA = SHA256.Create();
			return SHA.ComputeHash(Data);
		}

		public static byte[] GenSHA256StrmHash(Stream Data)
		{
			var SHA = SHA256.Create();
			return SHA.ComputeHash(Data);
		}

		// Thanks, Falo!
		public static byte[] AES_XTS(byte[] Key1, byte[] Key2, int SectorSize, byte[] Data, ulong Sector)
		{
			byte[] BlockData;
			var XTS128 = XtsAes128.Create(Key1, Key2, true);
			int Blocks;
			var MemStrm = new MemoryStream();
			var Writer = new BinaryWriter(MemStrm);
			var CryptoTransform = XTS128.CreateEncryptor();
			BlockData = new byte[SectorSize];
			Blocks = Data.Length / SectorSize;
			for (var i = 0; i < Blocks; i++)
			{
				CryptoTransform.TransformBlock(Data, i * SectorSize, SectorSize, BlockData, 0, Sector++);
				Writer.Write(BlockData);
			}

			return MemStrm.ToArray();
		}

		public static byte[] AES_EBC(byte[] Key, byte[] Data)
		{
			var AES = new RijndaelManaged
			{
				Key = Key,
				Mode = CipherMode.ECB
			};
			var TransformedData = new byte[0x40];
			AES.CreateEncryptor().TransformBlock(Data, 0, 0x40, TransformedData, 0);
			return TransformedData;
		}
	}
}