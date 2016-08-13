using System;

public class DataRegionComparer : IComparable
{
    TagNode TagNode;
	int CombinationSize;
	int StartChildNumber;
	public DataRegionComparer(){

	}
	public DataRegionComparer(TagNode tagNode,
	int combinationSize,
	int startChildNumber){
		this.TagNode = tagNode;
		this.CombinationSize = combinationSize;
		this.StartChildNumber = startChildNumber;
	}
    public int CompareTo(object targetObject)
    {
        if (targetObject is DataRegionComparer)
		{
			DataRegionComparer targetDataRegionComparer = (DataRegionComparer) targetObject;
			
			if ( (TagNode == targetDataRegionComparer.TagNode) && (CombinationSize == targetDataRegionComparer.CombinationSize) && (StartChildNumber == targetDataRegionComparer.StartChildNumber) )
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}
		else
		{
			return -1;
		}
    }

    public new bool Equals(Object targetObject)
	{
		if (targetObject is DataRegionComparer)
		{
			DataRegionComparer targetDataRegionComparer = (DataRegionComparer) targetObject;
			
			return (TagNode == targetDataRegionComparer.TagNode) && (CombinationSize == targetDataRegionComparer.CombinationSize) && (StartChildNumber == targetDataRegionComparer.StartChildNumber);
		}
		else
		{
			return false;
		}
	}


    public String toString()
	{
		return "Tag Node: " + TagNode.ToString() + ", Combination Size: " + CombinationSize + ", Start Child Number: " + StartChildNumber; 
	}
}