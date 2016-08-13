using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DataRecord
{
    public IEnumerable<TagNode> RecordElements;
    public int Count{
        get{
            return RecordElements.Sum(re => re.GetSubTreeSize());
        }
    }
    public DataRecord(TagNode[] RecordElements){
        this.RecordElements = RecordElements;
    }

    public override string ToString(){
        //RecordElements.Aggregate((workingSentence, nextElement)=> workingSentence + nextElement.ToString());
        StringBuilder stringBuilder = new StringBuilder();
       foreach(TagNode element in RecordElements){
           stringBuilder.Append(element.ToString()).Append(' ');
       }
       return stringBuilder.ToString();
    }
}