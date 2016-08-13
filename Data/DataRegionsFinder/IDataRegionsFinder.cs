using System.Collections.Generic;
public interface IDataRegionsFinder
{
	 List<DataRegion> FindDataRegions(TagNode tagNode, int maxNodeInGeneralizedNodes, double similarityTreshold);
}