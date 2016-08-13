using System.Collections.Generic;
public class DataRecordSizeComparator : Comparer<DataRecord>
{
    public override int Compare(DataRecord x, DataRecord y)
    {
        if(x.Count< y.Count)
        {
            return -1;
        }
        else if(x.Count > y.Count)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}