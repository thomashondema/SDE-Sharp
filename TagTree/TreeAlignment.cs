using System.Collections.Generic;
using System.Linq;
public class TreeAlignment
{
    public double Score;
    public TagNode FirstNode;
    public TagNode SecondNode;
    public IList<TreeAlignment> SubTreeAlignment = new List<TreeAlignment>();
    public TreeAlignment(){

    }
    public TreeAlignment(TagNode FirstNode, TagNode SecondNode){
        this.FirstNode = FirstNode;
        this.SecondNode = SecondNode;        
    }
    public TreeAlignment(double Score, TagNode FirstNode, TagNode SecondNode){
        this.Score = Score;
        this.FirstNode = FirstNode;
        this.SecondNode = SecondNode;        
    }
    public void Add(TreeAlignment alignment)
    {
        SubTreeAlignment.Add(alignment);
        if(alignment.SubTreeAlignment.Any()){
            // Add all items of the alignments subtree to the current subtree.
            SubTreeAlignment = new List<TreeAlignment>(SubTreeAlignment.Concat(alignment.SubTreeAlignment));
        }
    }
    public void AddSubTreeAlignment(IEnumerable<TreeAlignment> listAlignment)
    {
        SubTreeAlignment = new List<TreeAlignment>(SubTreeAlignment.Concat(listAlignment));
    }
}