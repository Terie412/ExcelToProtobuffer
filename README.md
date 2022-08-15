[TOC]

In general, game designers like to use Excel to configure game data. We can directly read data from excels. However, this approach is not safe, not extensible, low performance and not easy to distribute.

Many game studios try to convert Excel data into other forms to read, like json, protobuf, xml or any self-defined format. And I prefer protobuf. 

This solution provides a workflow for easily converting Excel to Protobuf binary data. This solution accept two inputs:
- Excel files that contain actual data that we need to read in game
- .proto files that describe what data in Excel files we need to read, as well as the type of data. 

Sometimes Excel files contain some extra data, like comments, formulation, pictures. Designers use these extra data to help them to understand the datas better while we need them in game. That's why we need .proto files.

And the solution gets two things output:
- Csharp files that define Google.Protobuf classes to read data from binary files
- Binary files that contains actual data we extract from excel files

# Usage

There are two CSharp projects in this solution. One project (`ProtoFileToCSharp`) converts .proto files into CSharp files. The other (`ExcelToProtoBuffer`) is built with these CSharp files, reads data from Excel files and converts into protobuffer binary files.

You have to run two projects in order so that you can get correct output.

- Clone this repository into your workspace
- Make sure your computer has a .Net 6 environment (for this solution written in C# 8 or later syntax)
- Try run the `ProtoFileToCSharp` project like the following:
  - Specify the location of your .proto files through `--protoPath`
  - Specify the location of protoc.exe through `--protocPath`. If you don't have one, download from [protobuf in github](https://github.com/protocolbuffers/protobuf)
  - Specify the location of csharp output files. You can directly refer to one of folders under the second project so that it can use these CSharp files to build.
```shell
dotnet.exe run --project ./ProtoFileToCSharp/ProtoFileToCSharp.csproj --protoPath="./Proto" --protocPath="./Tools/protoc.exe" --outputPath="./ExcelToProtoBuffer/DesignDataDefinition"
```
- Try run the `ExcelToProtoBuffer` project like the following:
  - Make sure `ProtoFileToCSharp` running successfully at first.
  - Specify the location of Excel files through `--excelPath`
  - Specify the location of output Binary files through `--outputPath` 
```shell
dotnet.exe run --project ./ExcelToProtoBuffer/ExcelToProtoBuffer.csproj --excelPath="./Excels" --outputPath="./DesignRawData"
```

You can write these commands into a single powershell commandline file. See `Run.cmd`.

Then you have to copy the CSharp files and Binary files to your Unity project.

# Format of Binary File

Each row of Excel file correspond to a Protobuf Object. And it is encoded to protobuffer bytes. The binary files is organized into `4 bytes` + `bytes data` sequences.

The `4 bytes` tell the length of next `bytes data`. And you can parse a protobuf object from `bytes data`. 

```text
[4 bytes][bytes data][4 bytes][bytes data][4 bytes][bytes data]...
```

An example to read the binary file:
```c#
var filePath = "DesignRawData/Character.bytes";
var allCharacterDesignDatas = GetAllCharacterDataFromFile(filePath);
 
public List<DesignData.Character> GetAllCharacterDataFromFile(){
    var byteList = File.ReadAllBytes(fileName).ToList();
    if (byteList.Count < 4)
    {
        Logger.Error("Invalid protobuf binary file");
        return;
    }
    
    int index = 0;
    var characters = new List<DesignData.Character>();
    while (true)
    {
        if (index >= byteList.Count) 
            break;
        
        var byteLength = byteList.GetRange(index, 4).ToArray();
        int length = BitConverter.ToInt32(byteLength);
        var byteData = byteList.GetRange(index + 4, length).ToArray();
        var character = DesignData.Character.Parser.ParseFrom(byteData);
        characters.Add(character);
        index += length + 4;
    }
    
    return characters;
}
```

# WorkFlow

You can do more in `Run.cmd`. All it needs to know is where to read inputs where to place output. If you are working in a team and use git to organize your game project, an workflow example likes:
- Designer: configure data into Excel (and modified .proto file if structure of Excel is changed or the Excel is a new one)
- Designer: click `Run.cmd`. And the followings will by done in seconds:
  - generate .cs files and .bytes files
  - copy .cs file and .bytes files to the right place
  - upload Excel, .proto, .cs, .bytes files to git server
- Other people: update workspace from git server, and run the game






