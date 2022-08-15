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

You have to run two projects in order so that you can get correct output. You can run them from commandline. See *Run.cmd*.

# Dependencies




