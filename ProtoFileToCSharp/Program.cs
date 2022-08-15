using CommandLine;

public class Program
{
    /// Class for commandline options definition
    class CommandLineOptions
    {
        public CommandLineOptions()
        {
            protoPath = string.Empty;
            protocPath = string.Empty;
            outputPath = string.Empty;
        }

        [Option("protoPath")] public string protoPath { get; set; }
        [Option("protocPath")] public string protocPath { get; set; }
        [Option("outputPath")] public string outputPath { get; set; }

        public override string ToString()
        {
            return $" protoPath={protoPath}\n protocPath={protocPath}\n outputPath={outputPath}";
        }
    }

    public static string protoPath = string.Empty;  // Path that defines the location of .proto files
    public static string protocPath = string.Empty; // Path that defines the location of protoc.exe
    public static string outputPath = string.Empty; // Path that defines the location for saving output csharp files

    public static void Main(string[] args)
    {
        var isArgsValid = InitFromCommandline(args);
        if (!isArgsValid) return;

        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }
        Directory.CreateDirectory(outputPath);

        ShellUtil.ExecuteAsync(protocPath, $"--csharp_out={outputPath} --proto_path {protoPath} {protoPath}/*.proto", OnCSharpFileGenerationFinish);
    }

    public static bool InitFromCommandline(string[] args)
    {
        var options = Parser.Default.ParseArguments<CommandLineOptions>(args).Value;
        protoPath = options.protoPath;
        protocPath = options.protocPath;
        outputPath = options.outputPath;

        Logger.Info($"options = \n{options}");
        if (string.IsNullOrEmpty(protoPath))
        {
            Logger.Error("protoPath could not be null. Please use --protoPath to tell the location of proto files");
            return false;
        }

        if (string.IsNullOrEmpty(protocPath))
        {
            Logger.Error("protocPath could not be null. Please use --protocPath to tell the location of protoc.exe");
            return false;
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            Logger.Error("outputPath could not be null. Please use --protoPath to tell where to save the output CSharp files");
            return false;
        }
        return true;
    }

    private static void OnCSharpFileGenerationFinish(string receiveMsg, string errorMsg)
    {
        if (!string.IsNullOrEmpty(errorMsg))
        {
            Logger.Error("ExecuteAsync Error:" + errorMsg);
        }
        Logger.Info("Finish generating csharp files. " + receiveMsg);

        if (!string.IsNullOrEmpty(errorMsg))
        {
            Environment.Exit(-1);
        }
    }
}