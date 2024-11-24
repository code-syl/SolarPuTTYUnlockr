using System.Security.Cryptography;
using System.Text;
using System.CommandLine;

var rootCommand = new RootCommand("Decrypts the contents of a Solar PuTTY session file.");

var passwordOption = new Option<string?>(
        ["--password", "-p"],
        description: "The password to use to decrypt the session data.",
        getDefaultValue: () => null)
    { IsRequired = false, AllowMultipleArgumentsPerToken = false };
rootCommand.Add(passwordOption);

var wordlistOption = new Option<string?>(
        ["--wordlist", "-w"],
        description: "A file containing a list of passwords (one per line) to try for decryption.",
        getDefaultValue: () => null)
    { IsRequired = false, AllowMultipleArgumentsPerToken = false };
rootCommand.Add(wordlistOption);

var fileOption = new Option<string>(
        ["--file", "-f"],
        description: "The session file to decrypt.")
    { IsRequired = true, AllowMultipleArgumentsPerToken = false };
rootCommand.Add(fileOption);

rootCommand.SetHandler((password, file, wordlist) =>
{
    var cipher = File.ReadAllText(file);

    if (password is not null)
    {
        TryDecrypt(password, cipher);
    } 
    else if (wordlist is not null)
    {
        using var reader = new StreamReader(wordlist);
        while (reader.ReadLine() is { } p)
        {
            var good = TryDecrypt(p, cipher);
            if (good)
                return;
        }
        
        Console.WriteLine("Decryption failed: No valid password found in wordlist.");
    }
    else
    {
        Console.WriteLine("Error: Please provide either a password or wordlist. See --help for more information.");
    }
}, passwordOption, fileOption, wordlistOption);

return rootCommand.Invoke(args);

// based on Solar PuTTY's implementation of Decrypt,
// and https://github.com/Wind010/SolarPuttyDecryptor/blob/main/SolarPuttyDecryptor.py
string Decrypt(string password, string cipher)
{
    var source = Convert.FromBase64String(cipher);
    var salt = source[..24];
    var iv = source[24..48];
    var data = source[48..];

    using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 1_000);
    var bytes = rfc2898DeriveBytes.GetBytes(24);

    using var cryptoServiceProvider = new TripleDESCryptoServiceProvider();
    cryptoServiceProvider.Mode = CipherMode.CBC;
    cryptoServiceProvider.Padding = PaddingMode.PKCS7;

    using var decryptor = cryptoServiceProvider.CreateDecryptor(bytes, iv);
    using var memoryStream = new MemoryStream(data);
    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
    using var resultStream = new MemoryStream();
    cryptoStream.CopyTo(resultStream);

    return Encoding.UTF8.GetString(resultStream.ToArray());
}

bool TryDecrypt(string password, string cipher)
{
    try
    {
        Console.Write($"Password used: {password} ...\t");
        var decrypted = Decrypt(password, cipher);
        if (!decrypted.StartsWith("{\"Sessions\":"))
            throw new CryptographicException("Malformed output. Likely a wrong password.");
        
        Console.WriteLine("SUCCESS!");
        Console.WriteLine();
        Console.WriteLine(decrypted);
        return true;
    }
    catch (CryptographicException _)
    {
        Console.WriteLine("failed. Password incorrect.");
        return false;
    }
}