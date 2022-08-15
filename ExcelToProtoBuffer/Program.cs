#pragma warning disable CS8604
#pragma warning disable CS0219

using CommandLine;

public class Program
{
    /// Class for commandline options definition
    class CommandLineOptions
    {
        public CommandLineOptions()
        {
            excelPath = string.Empty;
            outputPath = string.Empty;
        }

        [Option("excelPath")] public string excelPath { get; set; }
        [Option("outputPath")] public string outputPath { get; set; }

        public override string ToString()
        {
            return $" excelPath={excelPath}\n outputPath={outputPath}";
        }
    }

    public static string excelPath  = string.Empty; // Path that defines the location of Excel files
    public static string outputPath = string.Empty; // Path that defines the location for saving output protobuffer binary files

    public static void Main(string[] args)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        if (!InitFromCommandline(args)) return;

        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        Directory.CreateDirectory(outputPath);

        ExcelExporter.Instance.ExportFromDirectory(excelPath, outputPath);

        Test();
    }

    public static bool InitFromCommandline(string[] args)
    {
        var options = Parser.Default.ParseArguments<CommandLineOptions>(args).Value;
        excelPath = options.excelPath;
        outputPath = options.outputPath;

        Logger.Info($"Start Generate Protobuffer binary files"); // Logger.Info($"options = \n{options}");

        if (string.IsNullOrEmpty(excelPath))
        {
            Logger.Error("excelPath could not be null. Please use --excelPath to tell the location of Excel files");
            return false;
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            Logger.Error("outputPath could not be null. Please use --protoPath to tell where to save the output protobuffer binary files");
            return false;
        }

        return true;
    }

    private static void Test()
    {
        var fileName = outputPath + "/Character.bytes";
        byte[] bytes = File.ReadAllBytes(fileName);
        if (bytes.Length < 4)
        {
            Logger.Error("长度不应该小于4");
            return;
        }

        var byteList = bytes.ToList();
        int index = 0;
        var characters = new List<DesignData.Character>();
        while (true)
        {
            if (index >= byteList.Count) break;

            var byteLength = byteList.GetRange(index, 4).ToArray();
            int length = BitConverter.ToInt32(byteLength);
            var byteData = byteList.GetRange(index + 4, length).ToArray();
            DesignData.Character character = DesignData.Character.Parser.ParseFrom(byteData);
            characters.Add(character);
            index += length + 4;
        }

        var a = 1;
    }
}