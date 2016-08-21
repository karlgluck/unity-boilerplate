using System.Text.RegularExpressions;
using System.Collections.Generic;

public static partial class Util
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public delegate string GetCsvCell(int row, string columnName);
    public static GetCsvCell ReadCsv(string text, out int rows)
    {
        var lines = Regex.Split(text, LINE_SPLIT_RE);

        if (lines.Length <= 1)
        {
            rows = 0;
            return delegate (int row, string column)
            {
                throw new System.ArgumentOutOfRangeException("No data in this CSV");
            };
        }

        var header = Regex.Split(lines[0], SPLIT_RE);

        var columnNameToIndex = new Dictionary<string, int>();
        var data = new string[lines.Length - 1, header.Length];

        for (var i = 0; i < header.Length; ++i)
        {
            columnNameToIndex[header[i]] = i;
        }

        var lineSplitRegex = new Regex(SPLIT_RE);
        var currentRow = 0;

        for (var i = 1; i < lines.Length; i++)
        {
            var values = lineSplitRegex.Split(lines[i]);
            if (values.Length == 0 || values[0] == "") continue;
            for (var j = 0; j < header.Length && j < values.Length; ++j)
            {
                string value = values[j];
                value = value.Trim(TRIM_CHARS).Replace("\\", "");
                data[currentRow, j] = value;
            }

            ++currentRow;
        }

        rows = currentRow;

        GetCsvCell retval = delegate (int row, string columnName)
        {
            if (row < 0 || row >= currentRow)
            {
                throw new System.ArgumentOutOfRangeException("Row index out of range");
            }
            int columnIndex;
            if (!columnNameToIndex.TryGetValue(columnName, out columnIndex))
            {
                throw new System.ArgumentException("Unknown column name: " + columnName);
            }
            return data[row, columnIndex];
        };

        return retval;
    }
}