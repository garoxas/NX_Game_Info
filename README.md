# NX Game Info
[![Build status](https://ci.appveyor.com/api/projects/status/kgdq8btq7v2th8ne?svg=true)](https://ci.appveyor.com/project/garoxas/nx-game-info)  
Nightly build (Windows) https://ci.appveyor.com/project/garoxas/nx-game-info/build/artifacts  
Stable build https://github.com/garoxas/NX_Game_Info/releases  

# Features
- Windows, macOS and Linux compatible (Command Line Interface)
- NSP, XCI, NRO and installed titles on Switch SD card
- Game files structure (Scene Release, CDN Rip, Authoring Tool, Converted from other formats)
- NCA signature to verify official Nintendo titles. Unmodified titles should pass this verification, although titles converted from other formats will not
- Filesystem services permissions. Game titles should not have excessive permissions, and only trust titles with `Unsafe` and `Dangerous` from reliable source

# Information
- Title ID
- Base Title ID
- Title Name
- Display Version
  - Only available for `Base` and `Update`
- Version
- Latest Version
  - Latest title version from [tagaya CDN server](https://switchbrew.org/wiki/Network#tagaya)
- System Update
  - *XCI:* System version on `Update Partition`
  - *NSP:* `RequiredSystemVersion` from Metadata XML
- System Version
  - Minimum system version from [Metadata NCA](https://switchbrew.org/wiki/CNMT#Application_Extended_Header). Only available for `Base` and `Update`
- Application Version
  - Minimum application version from [Metadata NCA](https://switchbrew.org/wiki/CNMT#AddOnContent_Extended_Header). Only available for `DLC`
- MasterKey
- Title Key
- Publisher
  - Only available for `Base` and `Update`
- Languages
  - List of language codes as specified by [RFC 5646](https://tools.ietf.org/html/rfc5646). Only available for `Base` and `Update`
- File Name
- File Size
- Type
  - *Base*
  - *Update*
  - *DLC*
- Distribution
  - *Digital:* eShop titles (NSP)
  - *Cartridge:* Gamecard titles (XCI)
  - *Homebrew:* Homebrew titles (NRO)
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
  - Only available for `Base` and `Update`

# How to
NX Game Info uses `prod.keys`, `title.keys` and `console.keys` in the format as defined in https://github.com/garoxas/LibHac/blob/NX_Game_Info/KEYS.md and `hac_versionlist.json` from [tagaya CDN server](https://switchbrew.org/wiki/Network#tagaya)

 - *prod.keys*: Mandatory keys includes `header_key`, `aes_kek_generation_source`, `aes_key_generation_source`, `key_area_key_application_source` and `master_key_00`. Failing to provide these keys will make the application quit
  `master_key_##`, `key_area_key_application_##` and `titlekek_##` will also be necessary to decrypt titles with higher MasterKey requirement
 - *title.keys*: Optional, required for `Permission` check if `.tik` file not available
 - *console.keys*: Optional, `sd_seed` key required for `Open SD Card` feature
 - *hac_versionlist.json*: Optional, required for `Latest Version` feature

The application will look for these files at the following locations (other file locations will follow wherever `prod.keys` file was found)

 - Directory of the executable file (.exe) for Windows or (.app) for macOS
 - `$HOME/.switch` e.g. C:\\Users\\_yourname_\\.switch for Windows, /Users/_yourname_/.switch for macOS or /home/_yourname_/.switch for Linux

 `Export` menu supports exporting current list to either `CSV` or `XLSX`. For `CSV` file default delimiter to use is `comma ( , )` and user defined character can be set in `user.settings` by specifying the delimiter character in `CsvSeparator` field

 Compressed NCA is not supported, but there is an option to make the application accept `XCZ` and `NSZ` file extension by setting `NszExtension` field `user.settings`. Please note that issues related to these file extensions will not be supported

# macOS
### Open File/Directory
![NX_Game_Info_macOS.png](NX_Game_Info_macOS.png)
### Open SD Card
![NX_Game_Info_macOS_SD_Card.png](NX_Game_Info_macOS_SD_Card.png)

# Windows
### Open File/Directory
![NX_Game_Info_Windows.png](NX_Game_Info_Windows.png)
### Open SD Card
![NX_Game_Info_Windows_SD_Card.png](NX_Game_Info_Windows_SD_Card.png)

# Command Line Interface (Windows, macOS, Linux)
### Usage

#### Windows
`nxgameinfo_cli.exe [-h|--help] [-d|--debug] [-c|--sdcard] [-s(titleid|titlename|filename)|--sort=(titleid|titlename|filename)] paths...`

#### macOS, Linux
`mono nxgameinfo_cli.exe [-h|--help] [-d|--debug] [-c|--sdcard] [-s(titleid|titlename|filename)|--sort=(titleid|titlename|filename)] paths...`

#### Parameters
- -h|--help
  - Show help message and immediately exit
- -d|--debug
  - Print debug output to `debug.log`
- -c|--sdcard
  - Treat arguments passed in `paths` parameters as installed titles on Switch SD card
- -s|--sort
  - Sort output by one of the following options:
    - `titleid`
    - `titlename`
    - `filename` (default)
- paths
  - File or directory path, can pass multiple arguments

### Windows
Install .NET Core Runtime from the following link  
https://dotnet.microsoft.com/download?initial-os=windows

![NX_Game_Info_Windows_cli.png](NX_Game_Info_Windows_cli.png)

### macOS
Install .NET Core Runtime and Mono (Visual Studio channel) from the following links  
https://dotnet.microsoft.com/download?initial-os=macos  
https://www.mono-project.com/download/stable/#download-mac

![NX_Game_Info_macOS_cli.png](NX_Game_Info_macOS_cli.png)

### Linux (Ubuntu)
Install .NET Core Runtime and Mono from the following links  
https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/runtime-current  
https://www.mono-project.com/download/stable/#download-lin-ubuntu

![NX_Game_Info_Linux_cli.png](NX_Game_Info_Linux_cli.png)

# Credits
@Thealexbarney for the [LibHac](https://github.com/Thealexbarney/LibHac) library.  
@switchbrew for the [documentation](https://switchbrew.org) on the Nintendo Switch.  
@gibaBR for the [Switch-Backup-Manager](https://github.com/gibaBR/Switch-Backup-Manager) project.  
