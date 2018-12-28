# NX Game Info
- Title ID
- Title Name
- Display Version
- Version
- Latest Version
  - Latest title version from [tagaya CDN server](https://switchbrew.org/wiki/Network#tagaya)
- Firmware
  - Minimum system version from [Metadata NCA](https://switchbrew.org/wiki/NCA#Application_header) or `RequiredSystemVersion` from Metadata XML (for NSP files). Not to be confused with Gamecard Update Partition version for XCI files
- MasterKey
- File Name
- File Size
- Type
  - *Base*
  - *Update*
  - *DLC*
- Distribution
  - *Digital:* eShop titles (NSP)
  - *Cartridge:* Gamecard titles (XCI)
  - *Filesystem:* Installed titles on Switch SD card (NAX0)
- Structure
  - *Scene (for XCI files):* XCI files with `Update Partition`, `Normal Partition` and `Secure Partition`
  - *Converted (for XCI files):* XCI files with only `Secure Partition`. Commonly found in NSP converted to XCI files
  - *Scene (for NSP files):* NSP files with `legalinfo.xml`, `nacp.xml`, `programinfo.xml`, `cardspec.xml`. Commonly found in BBB Scene Release
  - *Homebrew (for NSP files):* NSP files with `authoringtoolinfo.xml`
  - *CDN (for NSP files):* NSP files with `cert` and `tik`. Commonly found in NSP files ripped from the eShop CDN server
  - *Converted (for NSP files):* NSP files without cert and tik. Commonly found in XCI converted to NSP files
  - *Filesystem (for SD Card files):* NAX0 files installed titles on Switch SD card
  - *Not complete:* XCI/NSP files with only `NCA` files
- Signature
  - *Passed:* NCA signature valid. Only available for official titles
  - *Not Passed:* NCA signature invalid. `Should only be for homebrew titles and not official titles`
- Permission
  - *Safe:* Titles without Filesystem services access or [permissions bitmask 0x8000000000000000](https://switchbrew.org/wiki/Filesystem_services#Permissions) unset
  - *Unsafe:* Titles with Filesystem services access and [permissions bitmask 0x8000000000000000](https://switchbrew.org/wiki/Filesystem_services#Permissions) set. `Has EraseMmc permission, should only be for homebrew titles and not game titles`
  - *Dangerous:* Titles with Filesystem services access and [permissions bitmask 0xffffffffffffffff](https://switchbrew.org/wiki/Filesystem_services#Permissions) set. `Has all permissions, should only be for homebrew titles and not game titles`

# macOS
### Open File/Directory
![NX_Game_Info_macOS.png](NX_Game_Info_macOS.png)

# Windows
### Open File/Directory
![NX_Game_Info_Windows.png](NX_Game_Info_Windows.png)
### Open SD Card
![NX_Game_Info_Windows_SD_Card.png](NX_Game_Info_Windows_SD_Card.png)

# Credits
@Thealexbarney for the [LibHac](https://github.com/Thealexbarney/LibHac) library.  
@switchbrew for the [documentation](https://switchbrew.org) on the Nintendo Switch.  
@gibaBR for the [Switch-Backup-Manager](https://github.com/gibaBR/Switch-Backup-Manager) project.  
