using System.Collections.Generic;

public class TagTree
{
    public TagNode Root;
    public IDictionary<int, IList<TagNode>> TagNodeDictionary = new Dictionary<int, IList<TagNode>>();
    public IList<TagNode> TagNodeList = new List<TagNode>();
     public int Count
     {
         get{
        return TagNodeList.Count;
         }
    }
    public int Depth
    {
        get
        {
            return TagNodeDictionary.Count-1;
        }
    }
    public TagTree(){

    }
    public void AddTagNodeAtLevel(TagNode TagNode){
        if(!TagNodeDictionary.ContainsKey(TagNode.Level))
        {
            TagNodeDictionary[TagNode.Level] = new List<TagNode>();
        }
        TagNodeDictionary[TagNode.Level].Add(TagNode);
    }
    public IList<TagNode> GetTagNodeAtLevel(int level)
    {
        return TagNodeDictionary[level];
    }

    public void AssignNodeNumber(){
        foreach(int level in TagNodeDictionary.Keys){
            foreach(TagNode tagNode in TagNodeDictionary[level]){
                TagNodeList.Add(tagNode);
                tagNode.TagNodeNumber = TagNodeList.Count +1;
            }
        }
    }

    public TagNode GetTagNodeAtNumber(int TagNodeNumber)
    {
        return TagNodeList[TagNodeNumber];
    }
}