using System.Collections.Generic;
using System.Linq;
public class MiningDataRegions : IDataRegionsFinder
{
	private ITreeMatcher treeMatcher;
	
	public MiningDataRegions(ITreeMatcher treeMatcher)
	{
		this.treeMatcher = treeMatcher;
	}
	
	public ITreeMatcher getTreeMatcher()
	{
		return treeMatcher;
	}

	public List<DataRegion> FindDataRegions(TagNode tagNode, int maxNodeInGeneralizedNodes, double similarityTreshold)
	{
		// List untuk menyimpan seluruh data region yang ditemukan
		List<DataRegion> dataRegions = new List<DataRegion>();
		// List untuk menyimpan kandidat data region, sepertinya tidak perlu
		List<DataRegion> currentDataRegions = new List<DataRegion>();
		
		if ( tagNode.CountSubTreeDepth() >= 2 )
		{
			// lakukan pembandingan2 antara generalized nodes yang mungkin dari children milik tagNode
			Dictionary<DataRegionComparer, double> comparisonResults = compareGeneralizedNodes(tagNode, maxNodeInGeneralizedNodes);
			// identifikasi data region dari hasil pembandingan generalized nodes
			currentDataRegions = identifyDataRegions(1, tagNode, maxNodeInGeneralizedNodes, similarityTreshold, comparisonResults);
			
			// tambahkan currentDataRegions yang ditemukan jika ada pada dataRegions
			if ( ! currentDataRegions.Any() )
			{
				dataRegions.AddRange(currentDataRegions);
			}
			
			// buat array yang menyatakan child mana saja dari tagNode yang termasuk dalam data region 
			bool[] childCoveredArray = new bool[tagNode.ChildrenCount];

			foreach (DataRegion dataRegion in currentDataRegions)
			{
				for (int childCounter = dataRegion.StartPoint; childCounter < dataRegion.StartPoint + dataRegion.NodesCovered; childCounter++)
				{
					childCoveredArray[ childCounter - 1 ] = true;
				}
			}
			
			// cari data regions dari children yang tidak termasuk dalam currentDataRegions, rekursif
			for (int childCounter = 0; childCounter < childCoveredArray.Length; childCounter++)
			{
				if (! childCoveredArray[childCounter] )
				{
					dataRegions.AddRange( FindDataRegions(tagNode.GetChildAtIndex( childCounter+1 ), maxNodeInGeneralizedNodes, similarityTreshold));
				}
			}
		}

		return dataRegions;
	}

	private Dictionary<DataRegionComparer, double> compareGeneralizedNodes(TagNode tagNode, int maxNodeInGeneralizedNodes)
	{
		Dictionary<DataRegionComparer, double> comparisonResults = new MultiKeyDictionary<DataRegionComparer, double>();

		// mulai dari setiap node
		for (int childCounter = 1; childCounter <= maxNodeInGeneralizedNodes; childCounter++)
		{
			// membandingkan kombinasi yang berbeda mulai dari childCounter sampai maxNodeInGeneralizedNodes
			for (int combinationSize = childCounter; combinationSize <= maxNodeInGeneralizedNodes; combinationSize++)
			{
				// minimal terdapat sepasang generalized nodes, sepertinya redundan dengan pengecekan di bawah
				// TO BE OPTIMIZED
				if ( tagNode.GetChildAtIndex( childCounter + 2*combinationSize - 1) != null )
				{
					int startPoint = childCounter;
					
					// mulai melakukan pembandingan pasangan-pasangan generalized nodes
					// BEDA DENGAN JURNAL, kondisi <=, untuk mengatasi kasus ketika childCounter = 1, combinationSize = 1, dan startPoint = tagNode.ChildrenCount - 1 
					for (int nextPoint = childCounter + combinationSize; nextPoint <= tagNode.ChildrenCount; nextPoint = nextPoint + combinationSize)
					{
						// lakukan pembandingan jika terdapat generalized nodes selanjutnya dengan ukuran yang sama
						if ( tagNode.GetChildAtIndex(nextPoint + combinationSize - 1) != null )
						{
							// buat array dari kedua generalized nodes yang akan dibandingkan
							TagNode[] A = new TagNode[combinationSize];
							TagNode[] B = new TagNode[combinationSize];

							// isi array daftar nomor children dari tagNode yang termasuk dalam generalized node A
							int arrayCounter = 0;
							for (int i = startPoint; i < nextPoint; i++)
							{
								A[arrayCounter] = tagNode.GetChildAtIndex(i);
								arrayCounter++;
							}

							// isi array daftar nomor children dari tagNode yang termasuk dalam generalized node A
							arrayCounter = 0;
							for (int i = nextPoint; i < (nextPoint + combinationSize); i++)
							{
								B[arrayCounter] = tagNode.GetChildAtIndex(i);
								arrayCounter++;
							}

							// simpan hasil pembandingan
							DataRegionComparer key = new DataRegionComparer(tagNode, combinationSize, startPoint);
							comparisonResults.Add(key, treeMatcher.normalizedMatchScore(A, B));
							startPoint = nextPoint;
						}
					}
				}
			}
		}
		
		return comparisonResults;
	}
	
	private List<DataRegion> identifyDataRegions(int initStartPoint, TagNode tagNode, int maxNodeInGeneralizedNodes, double similarityTreshold, Dictionary<DataRegionComparer, double> comparisonResults)
	{
		List<DataRegion> dataRegions = new List<DataRegion>();
		DataRegion maxDR = new DataRegion(tagNode, 0, 0, 0);
		DataRegion currentDR = new DataRegion(tagNode, 0, 0, 0);
		
		// mulai dari tiap kombinasi
		for (int combinationSize = 1; combinationSize <= maxNodeInGeneralizedNodes; combinationSize++)
		{
			// mulai dari tiap startPoint
			// BEDA dengan jurnal, <, untuk efisiensi karena perbandingan ke-initStartPoint+combinationSize tidak perlu
			for (int startPoint = initStartPoint; startPoint < initStartPoint+combinationSize; startPoint++)
			{
				bool flag = true;
				// BEDA dengan jurnal, childNumber+2*combinationSize-1 <=, karena belum tentu setiap DataRegionComparer(tagNode, combinationSize, childNumber) ada
				for (int childNumber = startPoint; childNumber+2*combinationSize-1 <= tagNode.ChildrenCount; childNumber += combinationSize)
				{
					DataRegionComparer key = new DataRegionComparer(tagNode, combinationSize, childNumber);

					if ( comparisonResults[ key ] >= similarityTreshold )
					{
						// jika cocok untuk pertama kali
						if (flag)
						{
							currentDR.CombinationSize = (combinationSize);
							currentDR.StartPoint = childNumber;
							currentDR.NodesCovered = ( 2 * combinationSize );
							flag = false;
						}
						// jika cocok bukan untuk pertama kali
						else
						{
							currentDR.NodesCovered = ( currentDR.NodesCovered + combinationSize );
						}
					}
					// jika tidak cocok dan sebelumnya cocok
					else if ( ! flag )
					{
						break;
					}
				}
				
				// jika currentDR yang baru ditemukan mencakup lebih banyak nodes dan dimulai dari posisi yang lebih awal atau sama dengan maxDR 
				if ( ( maxDR.NodesCovered < currentDR.NodesCovered ) && ( maxDR.StartPoint == 0 || currentDR.StartPoint <= maxDR.StartPoint ) )
				{
					maxDR.CombinationSize = ( currentDR.CombinationSize );
					maxDR.StartPoint =  currentDR.StartPoint ;
					maxDR.NodesCovered = ( currentDR.NodesCovered );
				}
			}
		}
		
		// jika ditemukan data region
		if ( maxDR.NodesCovered != 0)
		{
			dataRegions.Add(maxDR);
			
			// jika data region yang ditemukan masih menyisakan children yang belum dicari, 
			// maka cari data region mulai dari child setelah child terakhir dari data region
			if ( maxDR.StartPoint + maxDR.NodesCovered - 1 != tagNode.ChildrenCount )
			{
				dataRegions.AddRange( identifyDataRegions(maxDR.StartPoint + maxDR.NodesCovered, tagNode, maxNodeInGeneralizedNodes, similarityTreshold, comparisonResults) );
			}
		}

		return dataRegions;
	}
}