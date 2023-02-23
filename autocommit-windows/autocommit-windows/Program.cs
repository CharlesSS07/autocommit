

// a script converted from bash,
// since it is easier to convert from bash to C# than to PowerShell

using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
//using LibGit2Sharp;

string SCRIPT_NAME = "autocommit";

string SITE_MAIN = "https://u1319464.wixsite.com/git-auto-commit";

string API_ENDPOINT = $"{SITE_MAIN}/_functions-dev/";

//Repository repo = new Repository();

//Configuration config = repo.Config;

//Console.WriteLine(config.Get<string>("user.name"));

// this is the wrong way to interface with git but fuck it, it works wonderfully.
// also, c sharp is stupid, so this is the right way.
string RunGitCommmand(string args)
{
    // stolen from https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
    // Start the child process.
    Process p = new Process();
    // Redirect the output stream of the child process.
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.FileName = "git";
    p.StartInfo.Arguments = args;
    p.Start();
    // Do not wait for the child process to exit before
    // reading to the end of its redirected stream.
    // p.WaitForExit();
    // Read the output stream first and then wait.
    string output = p.StandardOutput.ReadToEnd();
    p.WaitForExit();
    return output;
}

string AUTOCOMMIT_KEY = RunGitCommmand("config --global user.autocommitkey");

if (AUTOCOMMIT_KEY == "")
{
    Console.WriteLine($"{SCRIPT_NAME}: You must register this command line (no autocommit key detected). Go to this url if the browser does not open.");

    byte[] identifierRandomBytes = new byte[6];
    byte[] hiddenKeyRandomBytes = new byte[6 * 4];
    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
    rng.GetBytes(identifierRandomBytes);
    rng.GetBytes(hiddenKeyRandomBytes);

    AUTOCOMMIT_KEY = Convert.ToBase64String(identifierRandomBytes) + ":" + Convert.ToBase64String(hiddenKeyRandomBytes);

    string SITE_CLI_LOGIN = $"{SITE_MAIN}/cli-login?key={AUTOCOMMIT_KEY}";

    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(SITE_CLI_LOGIN);
        else
            Process.Start("open", SITE_CLI_LOGIN);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Could not open '{SITE_CLI_LOGIN}' in default browser. Error:");
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
    Environment.Exit(1);
}
else
{
    Console.WriteLine("Found Autocommit key: " + AUTOCOMMIT_KEY);
}


