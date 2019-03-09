using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using LibHac;
using LibHac.IO;
using nsZip.Crypto;
using nsZip.LibHacExtensions;

namespace nsZip
{
    internal static class EncryptNCA
	{
		internal static readonly string[] KakNames = {"application", "ocean", "system"};

		public static IStorage Encrypt(Keyset keyset, IStorage storage)
		{
			var DecryptedKeys = Util.CreateJaggedArray<byte[][]>(4, 0x10);
			var HeaderKey1 = new byte[16];
			var HeaderKey2 = new byte[16];
			Buffer.BlockCopy(keyset.HeaderKey, 0, HeaderKey1, 0, 16);
			Buffer.BlockCopy(keyset.HeaderKey, 16, HeaderKey2, 0, 16);

			var Input = storage.AsStream();
			var DecryptedHeader = new byte[0xC00];
			Input.Read(DecryptedHeader, 0, 0xC00);

			var Header = new NcaHeader(new BinaryReader(new MemoryStream(DecryptedHeader)), keyset);
			var CryptoType = Math.Max(Header.CryptoType, Header.CryptoType2);
			if (CryptoType > 0)
			{
				CryptoType--;
			}

			var HasRightsId = !Header.RightsId.IsEmpty();

			if (!HasRightsId)
			{
				if (keyset.KeyAreaKeys[CryptoType][Header.KaekInd].IsEmpty())
				{
					throw new ArgumentException($"key_area_key_{KakNames[Header.KaekInd]}_{CryptoType:x2}",
						"Missing area key!");
				}

				for (var i = 0; i < 4; ++i)
				{
					Crypto.Crypto.DecryptEcb(keyset.KeyAreaKeys[CryptoType][Header.KaekInd], Header.EncryptedKeys[i],
						DecryptedKeys[i], 0x10);
				}
			}
			else
			{
				var titleKey = keyset.TitleKeys[Header.RightsId];
				var TitleKeyDec = new byte[0x10];
				Crypto.Crypto.DecryptEcb(keyset.Titlekeks[CryptoType], titleKey, TitleKeyDec, 0x10);
				DecryptedKeys[2] = TitleKeyDec;
			}

			var Sections = new NcaSection[4];
			var SectionsByOffset = new Dictionary<long, int>();
			var lowestOffset = long.MaxValue;
			for (var i = 0; i < 4; ++i)
			{
				var section = NcaParseSection.ParseSection(Header, i);
				if (section == null)
				{
					continue;
				}

				SectionsByOffset.Add(section.Offset, i);
				if (section.Offset < lowestOffset)
				{
					lowestOffset = section.Offset;
				}

				Sections[i] = section;
			}

            MemoryStream Output = new MemoryStream();

			var encryptedHeader = CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, DecryptedHeader, 0);
            Output.Write(encryptedHeader, 0, DecryptedHeader.Length);

			var dummyHeader = new byte[0xC00];
			ulong dummyHeaderSector = 6;
			long dummyHeaderPos;

			for (dummyHeaderPos = 0xC00; dummyHeaderPos < lowestOffset; dummyHeaderPos += 0xC00)
			{
				var dummyHeaderWriteCount = (int) Math.Min(lowestOffset - dummyHeaderPos, DecryptedHeader.Length);
				Input.Read(dummyHeader, 0, dummyHeaderWriteCount);
				var dummyHeaderEncrypted =
					CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, dummyHeader, dummyHeaderSector);
                Output.Write(dummyHeaderEncrypted, 0, dummyHeaderWriteCount);

				dummyHeaderSector += 6;
			}

			foreach (var i in SectionsByOffset.OrderBy(i => i.Key).Select(item => item.Value))
			{
				var sect = Sections[i];
				if (sect == null)
				{
					continue;
				}

				var isExefs = Header.ContentType == ContentType.Program && i == (int) ProgramPartitionType.Code;
				var PartitionType = isExefs ? "ExeFS" : sect.Type.ToString();
				var initialCounter = new byte[0x10];

				if (sect.Header.Ctr != null)
				{
					Array.Copy(sect.Header.Ctr, initialCounter, 8);
				}

				if (Input.Position != sect.Offset)
				{
					//Input.Seek(sect.Offset, SeekOrigin.Begin);
					//Output.Seek(sect.Offset, SeekOrigin.Begin);
					//Todo: sha256NCA Gap support
					throw new NotImplementedException("Gaps between NCA sections aren't implemented yet!");
				}

				const int maxBS = 10485760; //10 MB
				int bs;
				var DecryptedSectionBlock = new byte[maxBS];
				var sectOffsetEnd = sect.Offset + sect.Size;
				switch (sect.Header.EncryptionType)
				{
					case NcaEncryptionType.None:
						while (Input.Position < sectOffsetEnd)
						{
							bs = (int) Math.Min(sectOffsetEnd - Input.Position, maxBS);
							Input.Read(DecryptedSectionBlock, 0, bs);
                            Output.Write(DecryptedSectionBlock, 0, bs);
						}

						break;
					case NcaEncryptionType.AesCtr:
						while (Input.Position < sectOffsetEnd)
						{
							SetCtrOffset(initialCounter, Input.Position);
							bs = (int) Math.Min(sectOffsetEnd - Input.Position, maxBS);
							Input.Read(DecryptedSectionBlock, 0, bs);
							var EncryptedSectionBlock = AesCTR.AesCtrTransform(DecryptedKeys[2], initialCounter,
								DecryptedSectionBlock, bs);
                            Output.Write(EncryptedSectionBlock, 0, bs);
						}

						break;
					case NcaEncryptionType.AesCtrEx:

						var info = sect.Header.BktrInfo;
						var MyBucketTree = new LibHacExtensions.BucketTree<LibHacExtensions.AesSubsectionEntry>(
							new MemoryStream(sect.Header.BktrInfo.EncryptionHeader.Header), Input,
							sect.Offset + info.EncryptionHeader.Offset);
						var SubsectionEntries = MyBucketTree.GetEntryList();
						var SubsectionOffsets = SubsectionEntries.Select(x => x.Offset).ToList();

						var subsectionEntryCounter = new byte[0x10];
						Array.Copy(initialCounter, subsectionEntryCounter, 0x10);
						foreach (var entry in SubsectionEntries)
						{
							//Array.Copy(initialCounter, subsectionEntryCounter, 0x10);
							SetCtrOffset(subsectionEntryCounter, Input.Position);
							subsectionEntryCounter[7] = (byte) entry.Counter;
							subsectionEntryCounter[6] = (byte) (entry.Counter >> 8);
							subsectionEntryCounter[5] = (byte) (entry.Counter >> 16);
							subsectionEntryCounter[4] = (byte) (entry.Counter >> 24);

							//bs = (int)Math.Min((sect.Offset + entry.OffsetEnd) - Input.Position, maxBS);
							bs = (int) (entry.OffsetEnd - entry.Offset);
							var DecryptedSectionBlockLUL = new byte[bs];
							Input.Read(DecryptedSectionBlockLUL, 0, bs);
							var EncryptedSectionBlock = AesCTR.AesCtrTransform(DecryptedKeys[2], subsectionEntryCounter,
								DecryptedSectionBlockLUL, bs);
                            Output.Write(EncryptedSectionBlock, 0, bs);
						}

						while (Input.Position < sectOffsetEnd)
						{
							SetCtrOffset(subsectionEntryCounter, Input.Position);
							bs = (int) Math.Min(sectOffsetEnd - Input.Position, maxBS);
							Input.Read(DecryptedSectionBlock, 0, bs);
							var EncryptedSectionBlock = AesCTR.AesCtrTransform(DecryptedKeys[2], subsectionEntryCounter,
								DecryptedSectionBlock,
								bs);
                            Output.Write(EncryptedSectionBlock, 0, bs);
						}

						break;

					default:
						throw new NotImplementedException();
				}
			}

			Input.Dispose();

            return Output.AsStorage();
		}

		private static void SetCtrOffset(byte[] ctr, long offset)
		{
			ctr[0xF] = (byte) (offset >> 4);
			ctr[0xE] = (byte) (offset >> 12);
			ctr[0xD] = (byte) (offset >> 20);
			ctr[0xC] = (byte) (offset >> 28);
			ctr[0xB] = (byte) (offset >> 36);
			ctr[0xA] = (byte) (offset >> 44);
			ctr[0x9] = (byte) (offset >> 52);
			ctr[0x8] = (byte) (offset >> 60);
		}
	}
}