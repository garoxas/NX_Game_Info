using LibHac;

namespace nsZip.LibHacExtensions
{
	public static class NcaParseSection
	{
		public static NcaSection ParseSection(NcaHeader Header, int index)
		{
			var entry = Header.SectionEntries[index];
			var header = Header.FsHeaders[index];
			if (entry.MediaStartOffset == 0)
			{
				return null;
			}

			var sect = new NcaSection();

			sect.SectionNum = index;
			sect.Offset = Util.MediaToReal(entry.MediaStartOffset);
			sect.Size = Util.MediaToReal(entry.MediaEndOffset) - sect.Offset;
			sect.Header = header;
			sect.Type = header.Type;

			return sect;
		}
	}
}