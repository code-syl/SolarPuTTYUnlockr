# Solar PuTTY Unlockr
This application is used for unlocking Solar PuTTY `session.dat` files, or to bruteforce it with a wordlist. The code is buildable for both Windows and Linux, and has been tested successfully on Windows 11 and Kali Linux.

## Usage
```
Description:
  Decrypts the contents of a Solar PuTTY session file.

Usage:
  SolarPuTTYSessionUnlockr [options]

Options:
  -p, --password <password>     The password to use to decrypt the session data. []
  -w, --wordlist <wordlist>     A file containing a list of passwords (one per line) to try for decryption. []
  -f, --file <file> (REQUIRED)  The session file to decrypt.
  --version                     Show version information
  -?, -h, --help                Show help and usage information
```

## Comments and references
Solar PuTTY's portable executable can be unzipped. In the resulting folder, `SolarWinds-FT-Solar-PuTTY/Solar-PuTTY/$_2_`, an executable named `Solar_PuTTY.exe` can be found. A compiler C# .NET decompiler from Jetbrains (https://www.jetbrains.com/decompiler/) was used to look at the source code of that executable. It was not obfuscated, and I was able to reuse the decryption code that was present. 

Many thanks to Voidsec and Wind010 for inspiring me to have a crack at it myself!

- https://github.com/VoidSec/SolarPuttyDecrypt
- https://github.com/Wind010/SolarPuttyDecryptor