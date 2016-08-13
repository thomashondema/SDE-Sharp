using System;
public class EnhancedSimpleTreeMatching  : ITreeMatcher{
	private void countTerminalNodes(TagNode tagNode, int counter)
	{
		if ( tagNode.InnerText != null )
		{
			counter += 1;
		}
		
		foreach (TagNode child in tagNode.Children)
		{
			countTerminalNodes( child, counter );
		}
	}
	
	private void countTerminalNodes(TagNode[] tagNodeArray, int counter)
	{
		foreach (TagNode element in tagNodeArray)
		{
			countTerminalNodes(element, counter);
		}
	}

	private double contentSimilarity(TagNode A, TagNode B)
	{
		if (A.InnerText == null || B.InnerText == null)
		{
			return 0.00;
		}
		else
		{
			int lcsScore = longestCommonSubsequenceScore( A.InnerText, B.InnerText );
			
			return ( lcsScore / ( Math.Max( A.InnerText.Length, B.InnerText.Length ) ) ); 
		}
	}
	
	private int longestCommonSubsequenceScore(String s1, String s2)
	{
		if ( s1.Length == 0 || s2.Length == 0)
		{
			return 0;
		}
		else
		{
			int[,] matchMatrix = new int[ s1.Length + 1 , s2.Length + 1 ];
			
			for ( int rowCounter=1; rowCounter < matchMatrix.GetLength(0); rowCounter++ )
			{
				for ( int columnCounter=1; columnCounter < matchMatrix.GetLength(1); columnCounter++ )
				{
					int currentMatch = 0;  
					
					if (s1[rowCounter-1] == s2[columnCounter - 1 ] )
					{
						currentMatch = 1;
					}
					
					matchMatrix[ rowCounter , columnCounter ] = Math.Max( matchMatrix[ rowCounter - 1 , columnCounter ], matchMatrix[ rowCounter , columnCounter - 1 ] );
					matchMatrix[ rowCounter , columnCounter ] = Math.Max( matchMatrix[ rowCounter , columnCounter ], matchMatrix[ rowCounter - 1 , columnCounter - 1 ] + currentMatch );
				}
			}
			
			return matchMatrix[ s1.Length , s2.Length ];
		}
	}

	public double matchScore(TagNode A, TagNode B)
	{	
		if ( A.TagElement != B.TagElement )
		{
			return 0.00;
		}
		else
		{
			double[,] matchMatrix = new double[ A.ChildrenCount + 1 ,B.ChildrenCount + 1 ];
			
			for (int i = 1; i < matchMatrix.GetLength(0); i++)
			{
				for (int j = 1; j < matchMatrix.GetLength(1); j++)
				{
					matchMatrix[i,j] = Math.Max( matchMatrix[i,j-1], matchMatrix[i-1,j]);
					matchMatrix[i,j] = Math.Max( matchMatrix[i,j], matchMatrix[i-1,j-1] + matchScore( A.GetChildAtIndex(i), B.GetChildAtIndex(j) ));
				}
			}
			
			return 1.00 + matchMatrix[ matchMatrix.GetLength(0) - 1 ,matchMatrix.GetLength(1) - 1] + contentSimilarity(A, B);
		}
	}

	public double matchScore(TagNode[] A, TagNode[] B)
	{
		double[,] matchMatrix = new double[ A.Length + 1 ,B.Length + 1 ];
		
		for (int i = 1; i < matchMatrix.GetLength(0); i++)
		{
			for (int j = 1; j < matchMatrix.GetLength(1); j++)
			{
				matchMatrix[i,j] = Math.Max( matchMatrix[i,j-1], matchMatrix[i-1,j]);
				matchMatrix[i,j] = Math.Max( matchMatrix[i,j], matchMatrix[i-1,j-1] + matchScore( A[i-1], B[j-1] ));
			}
		}
		
		return 1.00 + matchMatrix[ matchMatrix.GetLength(0) - 1 ,matchMatrix.GetLength(1) - 1];
	}
	
	public double matchScore(TagTree A, TagTree B)
	{
		return matchScore(A.Root, B.Root);
	}

	public double normalizedMatchScore(TagNode A, TagNode B)
	{
		int countTerminalNodesA = 0;
		countTerminalNodes(A, countTerminalNodesA);
		int countTerminalNodesB = 0;
		countTerminalNodes(B, countTerminalNodesB);
	
		return matchScore(A, B) / ( ( A.GetSubTreeSize() + B.GetSubTreeSize() + countTerminalNodesA + countTerminalNodesB ) / 2.0);
	}
	
	public double normalizedMatchScore(TagNode[] A, TagNode[] B)
	{
		int sizeA = 1;
		
		foreach (TagNode tagNode in A)
		{
			sizeA += tagNode.GetSubTreeSize();
		}
		
		int sizeB = 1;
		
		foreach (TagNode tagNode in B)
		{
			sizeB += tagNode.GetSubTreeSize();
		}
		
		int countTerminalNodesA = 0;
		countTerminalNodes(A, countTerminalNodesA);
		int countTerminalNodesB = 0;
		countTerminalNodes(B, countTerminalNodesB);
		
		return matchScore(A, B) / ( ( sizeA + sizeB + countTerminalNodesA + countTerminalNodesB ) / 2.0);
	}
	
	public double normalizedMatchScore(TagTree A, TagTree B)
	{
		return matchScore(A, B) / ( ( A.Count + B.Count ) / 2.0);
	}
	
	public TreeAlignment align(TagNode[] A, TagNode[] B)
	{
		TreeAlignment returnAlignment = new TreeAlignment();
		double[,] matchMatrix = new double[ A.Length + 1 ,B.Length + 1 ];
		TreeAlignment[,] alignmentMatrix = new TreeAlignment[ A.Length ,B.Length ];
		Trackback[,] trackbackMatrix = new Trackback[ A.Length ,B.Length ];
		
		// dapatkan skor penjajaran maksimum dan buat matriks untuk trackback-nya
		for (int i = 1; i < matchMatrix.GetLength(0); i++)
		{
			for (int j = 1; j < matchMatrix.GetLength(1); j++)
			{
				if ( matchMatrix[i,j-1] > matchMatrix[i-1,j] )
				{
					matchMatrix[i,j] = matchMatrix[i,j-1];
					trackbackMatrix[i-1,j-1] = Trackback.Left;
				}
				else
				{
					matchMatrix[i,j] = matchMatrix[i-1,j];
					trackbackMatrix[i-1,j-1] = Trackback.Up;
				}
				
				alignmentMatrix[i-1,j-1] = align( A[i-1], B[j-1] );
				double diagonalScore = matchMatrix[i-1,j-1] + alignmentMatrix[i-1,j-1].Score;
				
				if ( diagonalScore > matchMatrix[i,j] )
				{
					matchMatrix[i,j] = diagonalScore;
					trackbackMatrix[i-1,j-1] = Trackback.Diagonal;
				}
			}
		}
		
		// lakukan trackback
		int trackbackRow = matchMatrix.GetLength(0)-1;
		int trackbackColumn = matchMatrix.GetLength(1)-1;
		
		while ( trackbackRow >= 0 && trackbackColumn >= 0)
		{
			// jika ada node yang match
			if ( trackbackMatrix[ trackbackRow , trackbackColumn ] == Trackback.Diagonal )
			{
				returnAlignment.Add( alignmentMatrix[ trackbackRow , trackbackColumn ] );
				trackbackRow--;
				trackbackColumn--;
			}
			else if( trackbackMatrix[ trackbackRow , trackbackColumn ] == Trackback.Up )
			{
				trackbackRow--;
			}
			else if( trackbackMatrix[ trackbackRow , trackbackColumn ] == Trackback.Left )
			{
				trackbackColumn--;
			}
		}

		returnAlignment.Score = ( 1.00 + matchMatrix[ matchMatrix.GetLength(0) - 1 ,matchMatrix.GetLength(1) - 1] );
		
		return returnAlignment;
	}
	
	public TreeAlignment align(TagNode A, TagNode B)
	{
		TreeAlignment returnAlignment;

		if ( A.TagElement != B.TagElement )
		{
			returnAlignment = new TreeAlignment();
			returnAlignment.Score = ( 0.00 );
			
			return returnAlignment;
		}
		else
		{
			returnAlignment = new TreeAlignment(A, B);
			double[,] matchMatrix = new double[ A.ChildrenCount + 1 ,B.ChildrenCount + 1 ];
			TreeAlignment[,] alignmentMatrix = new TreeAlignment[ A.ChildrenCount ,B.ChildrenCount ];
			Trackback[,] trackbackMatrix = new Trackback[ A.ChildrenCount ,B.ChildrenCount ];
			
			// dapatkan skor penjajaran maksimum dan buat matriks untuk trackback-nya
			for (int i = 1; i < matchMatrix.GetLength(0); i++)
			{
				for (int j = 1; j < matchMatrix.GetLength(1); j++)
				{
					if ( matchMatrix[i,j-1] > matchMatrix[i-1,j] )
					{
						matchMatrix[i,j] = matchMatrix[i,j-1];
						trackbackMatrix[i-1,j-1] = Trackback.Left;
					}
					else
					{
						matchMatrix[i,j] = matchMatrix[i-1,j];
						trackbackMatrix[i-1,j-1] = Trackback.Up;
					}
					
					alignmentMatrix[i-1,j-1] = align( A.GetChildAtIndex( i ), B.GetChildAtIndex( j ) );
					double diagonalScore = matchMatrix[i-1,j-1] + alignmentMatrix[i-1,j-1].Score;
					
					if ( diagonalScore > matchMatrix[i,j] )
					{
						matchMatrix[i,j] = diagonalScore;
						trackbackMatrix[i-1,j-1] = Trackback.Diagonal;
					}
				}
			}
			
			// lakukan trackback
			int trackbackRow = matchMatrix.GetLength(0)-1;
			int trackbackColumn = -1;

			if ( trackbackRow >= 0)
			{
				trackbackColumn = matchMatrix.GetLength(1)-1;
			}
			
			while ( trackbackRow >= 0 && trackbackColumn >= 0)
			{
				// jika ada node yang match
				if ( trackbackMatrix[ trackbackRow , trackbackColumn ] == Trackback.Diagonal )
				{
					returnAlignment.Add( alignmentMatrix[ trackbackRow , trackbackColumn ] );
					trackbackRow--;
					trackbackColumn--;
				}
				else if( trackbackMatrix[ trackbackRow , trackbackColumn ] == Trackback.Up )
				{
					trackbackRow--;
				}
				else if( trackbackMatrix[ trackbackRow , trackbackColumn ] == Trackback.Left )
				{
					trackbackColumn--;
				}
			}

			returnAlignment.Score = ( 1.00 + matchMatrix[ matchMatrix.GetLength(0) - 1 ,matchMatrix.GetLength(1) - 1] + contentSimilarity(A, B) );

			return returnAlignment;
		}
	}
}