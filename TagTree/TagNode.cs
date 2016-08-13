using System.Collections.Generic;
using System.Linq;

public class TagNode
{
    public TagNode Parent 
    {
        get
        {
            return Parent;
        }
        set
        {
            if(Parent != null)
            {
                Parent = value;
                Parent.AddChild(this);
                Level = CountLevel();
            }
        }
    }
    public List<TagNode> Children = new List<TagNode>();
    public string TagElement;
    public string InnerText;
    public int TagNodeNumber;
    public int Level = 0;
    public int ChildNumber;
    public int ChildrenCount{
        get{
            return Children.Count;
        }
    }
    public TagNode()
    {

    }
    public void AddChild(TagNode child){
        Children.Add(child);
        child.ChildNumber = Children.Count;
    }
    public void InsertChildNodes(int position, IEnumerable<TagNode> tagNodes)
    {
        tagNodes.ToList().ForEach(
            t => {
                t.Parent = this;
                t.Level = t.CountLevel();
        });
        Children.InsertRange(position-1,tagNodes);
        for (int childListCounter=position-1; childListCounter < Children.Count; childListCounter++)
		{
            Children[childListCounter].ChildNumber = childListCounter+1;
		}
        //Children.Skip(position-1).Select((x,i) => new {})
    }
    public int CountLevel()
	{
		if (Parent == null)
		{
			return 0;
		}
		else
		{
			return 1 + Parent.CountLevel();
		}
	}

    public int CountSubTreeDepth()
    {
        if(Children.Any()){
            return Level;
        }else
        {
            int subLevel = Level + 1;
            int currentSubLevel = subLevel;

            foreach (TagNode child in Children)
			{
				currentSubLevel = child.CountSubTreeDepth();
				
				if (currentSubLevel > subLevel)
				{
					subLevel = currentSubLevel;
				}
			}
			
			return subLevel;
        }
    }

    public void AppendInnerText(string text){
        InnerText+=text;
    }

    public TagNode GetPreviousSibling(){
        return Parent.GetChildAtIndex(ChildNumber-1);

        /*
        if(Parent != null && ChildNumber > 1){
            return Parent.GetChildAtIndex(ChildNumber-1);
        }else{
            return null;
        }*/
    }

    public TagNode GetNextSibling(){
        return Parent[ChildNumber+1];
        /*
        if(Parent != null && ChildNumber > Parent.ChildCount){
            return Parent[ChildNumber+1];
        }else{
            return null;
        }*/
    }

    public TagNode GetFirstChild(){
        return Children.FirstOrDefault();
    }
    public TagNode GetLastChild(){
            return Children.LastOrDefault();
    }

    public TagNode GetChildAtIndex(int index){
        return Children.ElementAtOrDefault(index);
    }

    public TagNode this[int index]
    {
        get { 
            //return Children.ElementAt(index);
            return Children.ElementAtOrDefault(index);
        }
    }
    public int GetSubTreeSize(){
        return Children.Sum(c => c.GetSubTreeSize());
    }
    
}