using System.Diagnostics;

public static class ShellUtil
{
    public static bool isRunning = false;
    public static Action<string, string> onProcessExited = delegate {  };
    public static string receivedMsg = string.Empty;
    public static string errorMsg = string.Empty;

    private static Process p = new Process();

    public static void ExecuteAsync(string fileName, string args, Action<string, string> callback)
    {
        if (isRunning)
        {
            Logger.Error("Wait for the last command line to exit");
            return;
        }

        Logger.Info($"ExecuteAsync: {fileName} {args}");

        onProcessExited = callback;

        p = new Process();
        p.StartInfo.FileName = fileName;
        p.StartInfo.Arguments = args;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        isRunning = true;
        p.EnableRaisingEvents = true;
        p.Exited += ExitedHandler;
        p.Start();
        
        receivedMsg = String.Empty;
        receivedMsg = p.StandardOutput.ReadToEnd();
        errorMsg = p.StandardError.ReadToEnd();
        p.WaitForExit();
    }

    private static void ExitedHandler(object? sender, EventArgs e)
    {
        p.Close();
        isRunning = false;
        var handle = onProcessExited;
        onProcessExited = delegate {  };
        handle.Invoke(receivedMsg, errorMsg);
    }
}