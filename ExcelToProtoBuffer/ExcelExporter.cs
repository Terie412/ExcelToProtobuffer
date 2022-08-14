#pragma warning disable CS8604

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;

public class ExcelExporter : SingleTon<ExcelExporter>
{
    // Each fileDescriptor corresponds to a .proto file. A fileDescriptor contains all information of the .proto file.
    private Dictionary<string, FileDescriptor?> name_fileDescriptor = new Dictionary<string, FileDescriptor?>();
    private Dictionary<string, MessageDescriptor?> name_messageDescriptor = new Dictionary<string, MessageDescriptor?>();
    private bool isInit = false;

    public void Init()
    {
        if (isInit) return;

        // Init FileDescriptors
        name_fileDescriptor = GetFileDescriptorsViaReflection();
    }

    public void ExportFromDirectory(string excelPath, string outputPath)
    {
        if (!Directory.Exists(excelPath))
        {
            throw new Exception($"Director does not exist. Path = {excelPath}");
        }

        Init();

        // Export directory recursively
        var filePathList = Directory.GetFiles(excelPath);
        foreach (var filePath in filePathList)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            try
            {
                ExportFile(filePath, outputPath + $"/{fileName}.bytes");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        var dirPathList = Directory.GetDirectories(excelPath);
        foreach (var dirPath in dirPathList)
        {
            ExportFromDirectory(dirPath, outputPath);
        }
    }

    public void ExportFile(string filePath, string outputPath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        if (fileNameWithoutExtension.ToLower().StartsWith("google"))
        {
            Logger.Warning($"File Export Failed! Excel Name cannot start with \"Google\". FileName = {fileNameWithoutExtension}");
            return;
        }

        var protoFileName = fileNameWithoutExtension + ".proto";
        if (!name_fileDescriptor.TryGetValue(protoFileName, out var fileDescriptor))
        {
            Logger.Error($"FileDescriptor to this Excel could not be found. Try regenerate CSharp from proto files. Excel File : {fileNameWithoutExtension}");
            return;
        }

        var fieldName_fieldDescriptor = new Dictionary<string, FieldDescriptor>();
        List<string> validFieldNameList = new List<string>();
        MessageDescriptor? messageDescriptor = null;
        Debug.Assert(fileDescriptor != null);
        foreach (MessageDescriptor? descriptor in fileDescriptor.MessageTypes)
        {
            if (descriptor.Name != fileNameWithoutExtension) continue;

            messageDescriptor = descriptor;
            MessageDescriptor.FieldCollection? fields = descriptor.Fields;
            foreach (var field in fields.InDeclarationOrder())
            {
                validFieldNameList.Add(field.Name);
                fieldName_fieldDescriptor[field.Name] = field;
            }
        }

        Debug.Assert(messageDescriptor != null);

        var excelData = ExcelData.FromFilePath(filePath);
        var headers = excelData.headers;
        var validIndex_headerName = new Dictionary<int, string>();
        for (var i = 0; i < headers.Count; i++)
        {
            var headerName = headers[i];
            if (!validFieldNameList.Any(fieldName => fieldName.Equals(headerName))) continue;

            validIndex_headerName[i] = headerName;
        }

        // Equipment equip = new Equipment();
        // equip.ToByteArray();

        // Each row into a protobuffer byte array
        List<byte[]> bytesList = new List<byte[]>(excelData.rows.Count);
        for (var i = 0; i < excelData.rows.Count; i++)
        {
            // For each row, create a ProtoBuffer Object
            // var obj = Activator.CreateInstance(iMessageType);
            var row = excelData.rows[i];
            var cells = row.ItemArray;
            var messageObj = messageDescriptor.Parser.ParseFrom(ByteString.Empty);

            // Set Message's Field values
            for (var j = 0; j < cells.Length; j++)
            {
                object? cell = cells[j];
                Debug.Assert(cell != null);
                if (!validIndex_headerName.TryGetValue(j, out var headerName)) continue;
                if (!fieldName_fieldDescriptor.TryGetValue(headerName, out var fieldDescriptor)) continue;
                SetMessageValueByFieldDescriptor(messageObj, fieldDescriptor, Convert.ToString(cell));
            }

            // Convert message to byte[]
            var bytes = messageObj.ToByteArray();
            bytesList.Add(bytes);
        }

        IEnumerable<byte> allBytes = new byte[0];
        for (var i = 0; i < bytesList.Count; i++)
        {
            var bytes = bytesList[i];
            int byteLength = bytes.Length;
            byte[] lengthByte = BitConverter.GetBytes(byteLength);
            allBytes = allBytes.Concat(lengthByte).Concat(bytes);
        }

        File.WriteAllBytes(outputPath, allBytes.ToArray());
    }

    /// Fill message with valueString. fieldDescriptor describes the type of valueString
    private void SetMessageValueByFieldDescriptor(IMessage message, FieldDescriptor fieldDescriptor, string? valueString)
    {
        object val = new object();
        object[] valArray = new object[0];
        switch (fieldDescriptor.FieldType)
        {
            case FieldType.Double:
                if (fieldDescriptor.IsRepeated)
                {
                    valArray = ParseToDoubleArray(valueString);
                }
                else
                {
                    val = ParseToDouble(valueString);
                }

                break;
            case FieldType.Float:
                if (fieldDescriptor.IsRepeated)
                {
                    valArray = ParseToFloatArray(valueString);
                }
                else
                {
                    val = ParseToFloat(valueString);
                }

                break;
            case FieldType.Int64:
                if (fieldDescriptor.IsRepeated)
                {
                    valArray = ParseToInt64Array(valueString);
                }
                else
                {
                    val = ParseToInt64(valueString);
                }

                break;
            case FieldType.UInt64:
                if (fieldDescriptor.IsRepeated)
                {
                    valArray = ParseToUInt64Array(valueString);
                }
                else
                {
                    val = ParseToUInt64(valueString);
                }

                break;
            case FieldType.Int32:
                if (fieldDescriptor.IsRepeated)
                {
                    valArray = ParseToInt32Array(valueString);
                }
                else
                {
                    val = ParseToInt32(valueString);
                }

                break;
            case FieldType.String:
                if (fieldDescriptor.IsRepeated)
                {
                    valArray = ParseToStringArray(valueString);
                }
                else
                {
                    val = ParseToString(valueString);
                }

                break;
            case FieldType.UInt32:
                if (fieldDescriptor.IsRepeated)
                {
                    valArray = ParseToUInt32Array(valueString);
                }
                else
                {
                    val = ParseToUInt32(valueString);
                }

                break;
            default:
                throw new Exception($"Unsupport filedType : {fieldDescriptor.FieldType}");
        }

        if (fieldDescriptor.IsRepeated)
        {
            IList? list = fieldDescriptor.Accessor.GetValue(message) as IList;
            Debug.Assert(list != null);
            for (var i = 0; i < valArray.Length; i++)
            {
                list.Add(valArray[i]);
            }
        }
        else
        {
            switch (fieldDescriptor.FieldType)
            {
                case FieldType.Double:
                    fieldDescriptor.Accessor.SetValue(message, Convert.ToDouble(val));
                    break;
                case FieldType.Float:
                    fieldDescriptor.Accessor.SetValue(message, Convert.ToSingle(val));
                    break;
                case FieldType.Int64:
                    fieldDescriptor.Accessor.SetValue(message, Convert.ToInt64(val));
                    break;
                case FieldType.UInt64:
                    fieldDescriptor.Accessor.SetValue(message, Convert.ToUInt64(val));
                    break;
                case FieldType.Int32:
                    fieldDescriptor.Accessor.SetValue(message, Convert.ToInt32(val));
                    break;
                case FieldType.String:
                    fieldDescriptor.Accessor.SetValue(message, val as string);
                    break;
                case FieldType.UInt32:
                    fieldDescriptor.Accessor.SetValue(message, Convert.ToUInt32(val));
                    break;
                default:
                    throw new Exception($"Unsupport filedType : {fieldDescriptor.FieldType}");
            }
        }
    }

    private object ParseToString(string value)
    {
        return value;
    }

    private object[] ParseToStringArray(string value)
    {
        var stringValue = value;
        var splits = stringValue.Split(",");
        object[] results = new object[splits.Length];
        for (var i = 0; i < splits.Length; i++)
        {
            var split = splits[i];
            results[i] = split;
        }

        return results;
    }

    private object ParseToInt32(string value)
    {
        return Convert.ToInt32(value);
    }

    private object[] ParseToInt32Array(string value)
    {
        var stringValue = value;
        var splits = stringValue.Split(",");
        object[] results = new object[splits.Length];
        for (var i = 0; i < splits.Length; i++)
        {
            var split = splits[i];
            results[i] = Convert.ToInt32(split);
        }

        return results;
    }

    private object ParseToUInt32(string value)
    {
        return Convert.ToUInt32(value);
    }

    private object[] ParseToUInt32Array(string value)
    {
        var stringValue = value;
        var splits = stringValue.Split(",");
        object[] results = new object[splits.Length];
        for (var i = 0; i < splits.Length; i++)
        {
            var split = splits[i];
            results[i] = Convert.ToUInt32(split);
        }

        return results;
    }

    private object ParseToInt64(string value)
    {
        return Convert.ToInt64(value);
    }

    private object[] ParseToInt64Array(string value)
    {
        var stringValue = value;
        var splits = stringValue.Split(",");
        object[] results = new object[splits.Length];
        for (var i = 0; i < splits.Length; i++)
        {
            var split = splits[i];
            results[i] = Convert.ToInt64(split);
        }

        return results;
    }

    private object ParseToUInt64(string value)
    {
        return Convert.ToUInt64(value);
    }

    private object[] ParseToUInt64Array(string value)
    {
        var stringValue = value;
        var splits = stringValue.Split(",");
        object[] results = new object[splits.Length];
        for (var i = 0; i < splits.Length; i++)
        {
            var split = splits[i];
            results[i] = Convert.ToUInt64(split);
        }

        return results;
    }

    private object ParseToFloat(string value)
    {
        return Convert.ToSingle(value);
    }

    private object[] ParseToFloatArray(string value)
    {
        var stringValue = value;
        var splits = stringValue.Split(",");
        object[] results = new object[splits.Length];
        for (var i = 0; i < splits.Length; i++)
        {
            var split = splits[i];
            results[i] = Convert.ToSingle(split);
        }

        return results;
    }

    private object ParseToDouble(string value)
    {
        return Convert.ToDouble(value);
    }

    private object[] ParseToDoubleArray(string value)
    {
        var stringValue = value;
        var splits = stringValue.Split(",");
        object[] results = new object[splits.Length];
        for (var i = 0; i < splits.Length; i++)
        {
            var split = splits[i];
            results[i] = Convert.ToDouble(split);
        }

        return results;
    }

    private Dictionary<string, FileDescriptor?> GetFileDescriptorsViaReflection()
    {
        Dictionary<string, FileDescriptor?> result = new Dictionary<string, FileDescriptor?>();
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).ToList().ForEach(type =>
        {
            var pi = type.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static);
            if (!(pi is null))
            {
                var value = pi.GetValue(null);
                if (value is FileDescriptor descriptor)
                {
                    string name = descriptor.Name;
                    if (!name.StartsWith("google"))
                    {
                        result[name] = descriptor;
                    }
                }
            }
        });
        return result;
    }
}