using System.Collections.Generic;
public class MiningDataRecords : IDataRecordsFinder {
    
    private ITreeMatcher treeMatcher;
    
    public MiningDataRecords(ITreeMatcher treeMatcher) {
        this.treeMatcher = treeMatcher;
    }
    
    public IEnumerable< DataRecord> FindDataRecords(DataRegion dataRegion, double similarityTreshold) {
        //  jika tidak kembalikan dataRegion karena merupakan data records
        //  cek apakah ukuran generalized node-nya = 1
        if ((dataRegion.CombinationSize == 1)) {
            TagNode parentNode = dataRegion.Parent;
            int startPoint = dataRegion.StartPoint;
            int nodesCovered = dataRegion.NodesCovered;
            //  untuk setiap generalized node G dalam dataRegion
            for (int generalizedNodeCounter = startPoint; (generalizedNodeCounter 
                        < (startPoint + nodesCovered)); generalizedNodeCounter++) {
                TagNode generalizedNode = parentNode.GetChildAtIndex(generalizedNodeCounter);
                //  cek apakah G merupakan data table row, jika ya kembalikan tiap generalized node sebagai data records
                if ((generalizedNode.CountSubTreeDepth() <= 2)) {
                    return this.sliceDataRegion(dataRegion);
                }
                
                TagNode prevChild = generalizedNode.GetChildAtIndex(1);
                //  cek apakah semua anak dari G mirip, jika tidak kembalikan tiap generalized node sebagai data records
                for (int childCounter = 2; childCounter <= generalizedNode.ChildrenCount; childCounter++) {
                    TagNode nextChild = generalizedNode.GetChildAtIndex(childCounter);
                    if ((this.treeMatcher.normalizedMatchScore(prevChild, nextChild) < similarityTreshold)) {
                        return this.sliceDataRegion(dataRegion);
                    }
                    
                    prevChild = nextChild;
                }
                
            }
            
            List<DataRecord> dataRecordList = new List<DataRecord>();
            //  kembalikan setiap node children dari tiap2 generalized node dari data region ini sebagai data records
            for (int generalizedNodeCounter = startPoint; (generalizedNodeCounter 
                        < (startPoint + nodesCovered)); generalizedNodeCounter++) {
                TagNode generalizedNode = parentNode.GetChildAtIndex(generalizedNodeCounter);
                foreach (TagNode childOfGeneralizedNode in generalizedNode.Children) {
                    DataRecord dataRecord = new DataRecord(new TagNode[] 
                    {
                                childOfGeneralizedNode
                    });
                    dataRecordList.Add(dataRecord);
                }
                
            }
            
            return dataRecordList;
        }
        
        //  jika data region generalized node-nya terdiri lebih dari 1 node, 
        //  maka kembalikan tiap generalized node sebagai data records
        return this.sliceDataRegion(dataRegion);
    }
    
    private DataRecord[] sliceDataRegion(DataRegion dataRegion) {
        TagNode parentNode = dataRegion.Parent;
        int combinationSize = dataRegion.CombinationSize;
        int startPoint = dataRegion.StartPoint;
        int nodesCovered = dataRegion.NodesCovered;
        DataRecord[] dataRecords = new DataRecord[(nodesCovered / combinationSize)];
        int arrayCounter = 0;
        for (
            int childCounter = startPoint; 
            (childCounter + combinationSize) <= (startPoint + nodesCovered);
            childCounter = (childCounter + combinationSize)) 
            {
            TagNode[] recordElements = new TagNode[combinationSize];
            int tagNodeCounter = 0;
            for (int generalizedNodeChildCounter = childCounter; (generalizedNodeChildCounter 
                        < (childCounter + combinationSize)); generalizedNodeChildCounter++) {
                recordElements[tagNodeCounter] = parentNode.GetChildAtIndex(generalizedNodeChildCounter);
                tagNodeCounter++;
            }
            
            DataRecord dataRecord = new DataRecord(recordElements);
            dataRecords[arrayCounter] = dataRecord;
            arrayCounter++;
        }
        
        return dataRecords;
    }
}