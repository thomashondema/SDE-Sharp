using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            testParser();
        }

        public static void testParser(){
        String resultOutput = "MDR.html";
		double similarityTreshold = 0.80;
		bool ignoreFormattingTags = false;
		bool useContentSimilarity = false;
		int maxNodeInGeneralizedNodes = 9;

            var content = getPageContent("http://stackoverflow.com/questions/10343632/httpclient-getasync-never-returns-when-using-await-async");

            ITagTreeBuilder builder = new HtmlAgilityPackTagTreeBuilder();
            TagTree tagTree = builder.buildTagTree(content);

            ITreeMatcher matcher = new SimpleTreeMatching();

            IDataRegionsFinder dataRegionsFinder = new MiningDataRegions(matcher);

            List<DataRegion> dataRegions = dataRegionsFinder.FindDataRegions(tagTree.Root,maxNodeInGeneralizedNodes,similarityTreshold); 

            IDataRecordsFinder dataRecordsFinder = new MiningDataRecords(matcher);

            var dataRecords = new IEnumerable<DataRecord>[dataRegions.Count];

            for( int dataRecordArrayCounter = 0; dataRecordArrayCounter < dataRegions.Count; dataRecordArrayCounter++)
			{
                DataRegion dataRegion = dataRegions[ dataRecordArrayCounter ];
				dataRecords[ dataRecordArrayCounter ] = dataRecordsFinder.FindDataRecords(dataRegion, similarityTreshold);
			}

			IColumnAligner aligner = null;
			if ( useContentSimilarity )
			{
				aligner = new PartialTreeAligner( new EnhancedSimpleTreeMatching() );
			}
			else
			{
				aligner = new PartialTreeAligner( matcher );
			}
			List<String[][]> dataTables = new List<String[][]>();

			// bagi tiap2 data records ke dalam kolom sehingga berbentuk tabel
			// dan buang tabel yang null
			for(int tableCounter=0; tableCounter < dataRecords.Length; tableCounter++)
			{
				String[][] dataTable = aligner.AlignDataRecords( dataRecords[tableCounter].ToList() );

				if ( dataTable != null )
				{
					dataTables.Add( dataTable );
				}
			}
			
			int recordsFound = 0;
            //recordsFound = dataTables.Sum(t=>t.Length);
			foreach ( String[][] dataTable in dataTables )
			{
				recordsFound += dataTable.Length;
			}
        }

        private static string getPageContent(string uri){
            HttpClient client = new HttpClient();
            return Task.Run(() => client.GetStringAsync(uri)).Result;
        }

        private static void testTableClass(){
            var data = new Table<string>(2,2);
            for(int column = 0; column < 2;column++)
            {
                for(int row = 0; row < 2;row++)
                {
                    data[column,row] = String.Format("Column:{0}, Row{1};",column,row);
                }
            }
            data.ForEach((t,c,r)=> 
            {
                Console.WriteLine(t);
            });
            data.ForEach((t,c,r)=> 
            {
                t = "test";
            });
             data.ForEach((t,c,r)=> 
            {
                Console.WriteLine(t);
            });
        }
    }
}
