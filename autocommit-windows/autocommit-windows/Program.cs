

// a script converted from bash,
// since it is easier to convert from bash to C# than to PowerShell

using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
//using LibGit2Sharp;

internal class Program
{
    // this is the wrong way to interface with git but fuck it, it works wonderfully.
    // also, c sharp is stupid, so this is the right way.
    static string RunGitCommmand(string args)
    {
        // stolen from https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
        // Start the child process.
        Process p = new Process();
        // Redirect the output stream of the child process.
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            p.StartInfo.FileName = "git.exe";
        else
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

    /// <summary>
    /// Stolen from https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
    /// Calculates MD5 from input string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
    }

    /// <summary>
    /// // Hash private key with time interval in epoch.
    /// Idk if this is secure but move fast and break things for now
    /// </summary>
    /// <returns></returns>
    static string HideKey(string key)
    {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        int secondsSinceEpoch = (int)t.TotalSeconds;
        int secondsInInterval = 20; // must be same as on server
        int intervalInEpoch = secondsSinceEpoch / secondsInInterval;
        string hiddenKey = CreateMD5(ParseKey(key)[1] + intervalInEpoch);
        return hiddenKey;
    }

    static string[] ParseKey(string key)
    {
        return key.Split(":");
    }

    private static async Task Main(string[] args)
    {

        FileStream COMMIT_MSG_FILE = File.Open(args[0], FileMode.Append);
        string COMMIT_SOURCE = args[1]; // user input from -m or -F
        string SHA1 = args[2];

        string SCRIPT_NAME = "autocommit";
        string SITE_MAIN = "https://u1319464.wixsite.com/git-auto-commit";
        string API_ENDPOINT = $"{SITE_MAIN}/_functions-dev/";

        HttpClient client = new HttpClient();
        //client.BaseAddress = new Uri(API_ENDPOINT);
        //client.DefaultRequestHeaders
        //    .Accept
        //    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
            Environment.Exit(1);
        }
        else
        {
            Console.WriteLine("Found Autocommit key: " + AUTOCOMMIT_KEY);

            //string diffFilePath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";

            string lastCommit = RunGitCommmand("log -n 1 --pretty=\"format:%H\"");
            string diff = RunGitCommmand($"diff {lastCommit}");

            var hiddenKey = HideKey(AUTOCOMMIT_KEY); // replace with your implementation

            var request = WebRequest.Create($"{API_ENDPOINT}/autocommit");
            request.Method = "POST";

            var json = JsonSerializer.Serialize(new Dictionary<string, string> { { "key", hiddenKey }, { "data", diff } });
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            using var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            using var response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            using var respStream = response.GetResponseStream();

            using var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();
            Console.WriteLine(data);
        }
    }
}