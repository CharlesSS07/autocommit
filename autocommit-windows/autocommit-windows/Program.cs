

// a script converted from bash,
// since it is easier to convert from bash to C# than to PowerShell

using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using LibGit2Sharp;

string SCRIPT_NAME = "autocommit";

string SITE_MAIN = "https://u1319464.wixsite.com/git-autocommit--chan";

string API_ENDPOINT = $"{SITE_MAIN}/_functions-dev/";

//Repository repo = new Repository(Environment.GetEnvironmentVariable("PWD"));

//Configuration config = repo.Config;

//Console.WriteLine(config.Get<string>("user.name"));

if (Environment.GetEnvironmentVariable("AUTOCOMMIT_KEY") == null)
{
    Console.WriteLine($"{SCRIPT_NAME}: You must register this command line (no Autocommit key detected). Go to this url if the browser does not opened.");

    byte[] identifierRandomBytes = new byte[6];
    byte[] hiddenKeyRandomBytes = new byte[6*4];
    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
    rng.GetBytes(identifierRandomBytes);
    rng.GetBytes(hiddenKeyRandomBytes);

    string AUTOCOMMIT_KEY = Convert.ToBase64String(identifierRandomBytes) + ":" + Convert.ToBase64String(hiddenKeyRandomBytes);

    string SITE_CLI_LOGIN = $"{SITE_MAIN}/cli-login?key={AUTOCOMMIT_KEY}";

    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            System.Diagnostics.Process.Start(SITE_CLI_LOGIN);
        else
            System.Diagnostics.Process.Start("open", SITE_CLI_LOGIN);
    } catch (Exception e)
    {
        Console.WriteLine("Could not open default browser. Error:");
        Console.WriteLine(e.Message);
    }

    Console.WriteLine();
    Console.WriteLine(SITE_CLI_LOGIN);
    Console.WriteLine();
    Console.WriteLine($"Then, append the following to your ~/.bashrc, ~/.zshrc, or profile.ps1, and re-run {SCRIPT_NAME}.");
    Console.WriteLine();
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        Console.WriteLine($"$env:AUTOCOMMIT_KEY = \"{AUTOCOMMIT_KEY}\" # Git Autocommit user key.");
    else
        Console.WriteLine($"export AUTOCOMMIT_KEY=\"{AUTOCOMMIT_KEY}\" # Git Autocommit user key.");
    System.Environment.Exit(1);
} else
{
    Console.WriteLine("Found Autocommit key");
}


