echo off
dotnet.exe run --project ./ProtoFileToCSharp/ProtoFileToCSharp.csproj --protoPath="./Proto" --protocPath="./Tools/protoc.exe" --outputPath="./ExcelToProtoBuffer/DesignDataDefinition"
if errorlevel 0 (
    dotnet.exe run --project ./ExcelToProtoBuffer/ExcelToProtoBuffer.csproj --excelPath="./Excels" --outputPath="./RawData"
)

pause