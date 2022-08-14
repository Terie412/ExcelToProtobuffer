using System.Diagnostics;

public static class ShellUtil
{
    public static bool isRunning = false;
    public static Action<string> onProcessExited = delegate {  };
    public static string receivedData = string.Empty;

    private static Process p = new Process();

    public static void ExecuteAsync(string fileName, string args, Action<string> callback)
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
        p.StartInfo.RedirectStandardOutput = false;
        p.StartInfo.RedirectStandardError = false;
        p.StartInfo.CreateNoWindow = true;

        isRunning = true;

        receivedData = String.Empty;
        p.OutputDataReceived += OnProcessOutputDataReceived;
        p.ErrorDataReceived += OnProcessErrorDataReceived;
        p.EnableRaisingEvents = true;
        p.Exited += ExitedHandler;

        p.Start();
    }

    private static void ExitedHandler(object? sender, EventArgs e)
    {
        Logger.Info($"Handle Exit：{receivedData}");
        p.Close();
        isRunning = false;
        var handle = onProcessExited;
        onProcessExited = delegate {  };
        handle.Invoke(receivedData);
    }
    
    private static void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        receivedData += e.Data + "\n";
    }

    private static void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Logger.Error(e.Data);
        }
    }
}