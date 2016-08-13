using System.Collections.Generic;

public interface IDataRecordsFinder
{
	 IEnumerable< DataRecord> FindDataRecords(DataRegion dataRegion, double similarityTreshold);
}