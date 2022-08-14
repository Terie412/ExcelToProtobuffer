using System.Data;
using System.Diagnostics;
using ExcelDataReader;

public class ExcelData
{
    public string filePath { get; }

    public List<string> headers = new List<string>();
    public List<DataRow> rows = new List<DataRow>();
    
    private ExcelData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new Exception($"File does not exist: {filePath}");
        }
        
        this.filePath = filePath;

        // Open Excel File
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        DataSet dataSet = reader.AsDataSet();
        DataTableCollection tables = dataSet.Tables;
        var table = tables[0];
        if (table == null)
        {
            throw new Exception($"Excel must contain at least one sheet. ExcelFile: {filePath}");
        }
        
        // Init ExcelData
        headers = new List<string>();
        rows = new List<DataRow>();
        for (int i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            
            // Header in row 0
            if (i == 0)
            {
                headers = GetHeadersFromDataRow(row).ToList();
                continue;
            }
            
            // The second row data is comment, so we skip this row
            if (i == 1) continue;
            
            rows.Add(row);
        }
    }

    // Create and Init an ExcelData from filePath
    public static ExcelData FromFilePath(string filePath)
    {
        return new ExcelData(filePath);
    }
    
    private string[] GetHeadersFromDataRow(DataRow row)
    {
        var cells = row.ItemArray;

        string[] headers = new string[cells.Length]; 
        for (var i = 0; i < cells.Length; i++)
        {
            var val = cells[i] as string;
            Debug.Assert(val != null);
            headers[i] = val;
        }

        return headers;
    }
}