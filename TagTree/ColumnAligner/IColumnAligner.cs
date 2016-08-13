using System.Collections.Generic;

public interface IColumnAligner{
    string[][] AlignDataRecords(List<DataRecord> DataRecords);
}