using System.Linq;
using System.Collections.Generic;
public class PartialTreeAligner  : IColumnAligner
{
	private ITreeMatcher treeAligner;
	
	public PartialTreeAligner(ITreeMatcher treeAligner)
	{
		this.treeAligner = treeAligner;
	}

	public string[][] AlignDataRecords(List<DataRecord> dataRecords)
	{
        string[][] alignedData = new string[ dataRecords.Count ][];
		// urutkan data records secara ascending berdasarkan ukuran pohon data record
		dataRecords.Sort(new DataRecordSizeComparator());
		// buat R
		List<DataRecord> R = new List<DataRecord>();
		// buat Dictionary of Dictionary
		DataRecord originalSeed = dataRecords[ dataRecords.Count-1 ];
		Dictionary<DataRecord, Dictionary<TagNode, TagNode>> mapping = new Dictionary<DataRecord, Dictionary<TagNode, TagNode>>();
		mapping.Add(originalSeed, new Dictionary<TagNode, TagNode>() );
		// dapatkan data record yang memiliki node paling banyak, salin sebagai seedDataRecord
		DataRecord seedDataRecord = copyDataRecord( dataRecords[ dataRecords.Count-1 ] );
		// hilangkan seedDataRecord dari list data records
		dataRecords.RemoveAt( dataRecords.Count-1 );
		createSeedAlignment(seedDataRecord.RecordElements.ToList(), originalSeed.RecordElements.ToList(), mapping[ originalSeed ]);		
		
		while(dataRecords.Count != 0)
		{
			// ambil dan hapus subtree data record berikutnya
			DataRecord nextDataRecord = dataRecords[ dataRecords.Count-1 ];
			dataRecords.RemoveAt( dataRecords.Count-1 );
			// jajarkan subtree yang baru saja diambil dengan seed
			IList<TreeAlignment> alignmentList = treeAligner.align( seedDataRecord.RecordElements.ToArray(), nextDataRecord.RecordElements.ToArray() ).SubTreeAlignment;
			// buat Dictionary hasil penjajaran
			mapping.Add(nextDataRecord, new Dictionary<TagNode, TagNode>() );
			
			 foreach (TreeAlignment alignment in alignmentList)
			{
				mapping[nextDataRecord].Add( alignment.FirstNode, alignment.SecondNode );
			}
			
			List< List<TagNode> > unalignedNodes = new List< List<TagNode> >();
			findUnalignedNodes( nextDataRecord.RecordElements, mapping[nextDataRecord], unalignedNodes);
			
			if ( unalignedNodes.Any() )
			{
				bool anyInsertion = false;
				Dictionary<TagNode, TagNode> reverseDictionary = new Dictionary<TagNode, TagNode>();
				
				 foreach (TagNode key in mapping[nextDataRecord].Keys )
				{
					reverseDictionary.Add( mapping[nextDataRecord][key], key);
				}
			
				// coba menyisipkan unaligned nodes ke seed
				 foreach (List<TagNode> unalignedNodesThisLevel in unalignedNodes)
				{
					// dapatkan elemen yang paling kiri
					TagNode leftMostUnaligned = unalignedNodesThisLevel[0];
					// dapatkan elemen yang paling kanan
					TagNode rightMostUnaligned = unalignedNodesThisLevel[ unalignedNodesThisLevel.Count-1];
					// prev dan next pasti match
					TagNode prevSibling = leftMostUnaligned.GetPreviousSibling();
					TagNode nextSibling = rightMostUnaligned.GetNextSibling();
				
					if ( prevSibling == null)
					{
						if ( nextSibling != null )
						{
							// berarti unalignedNodes berada pada posisi paling kiri
							TagNode nextSiblingMatch = reverseDictionary[nextSibling];
						
							if ( nextSiblingMatch.GetPreviousSibling() == null)
							{
								List<TagNode> unalignedNodesCopy = new List<TagNode>();

								 foreach (TagNode unalignedNode in unalignedNodesThisLevel)
								{
									TagNode copy = new TagNode();
									copy.TagElement = ( unalignedNode.TagElement );
									copy.InnerText = ( unalignedNode.InnerText );
									unalignedNodesCopy.Add( copy );
								}

								nextSiblingMatch.Parent.InsertChildNodes( 1, unalignedNodesCopy );
								
								 for (int counter=0; counter < unalignedNodesThisLevel.Count; counter++)
								{
									mapping[ nextDataRecord ].Add( unalignedNodesCopy[ counter ], unalignedNodesThisLevel[ counter ]);
								}

								unalignedNodesThisLevel.Clear();
								anyInsertion = true;
							}
						}
					}
					else if ( nextSibling == null)
					{
						// berarti unalignedNodes berada pada posisi paling kanan
						TagNode prevSiblingMatch = reverseDictionary[prevSibling];
					
						if ( prevSiblingMatch.GetNextSibling() == null )
						{
							List<TagNode> unalignedNodesCopy = new List<TagNode>();

							 foreach (TagNode unalignedNode in unalignedNodesThisLevel)
							{
								TagNode copy = new TagNode();
								copy.TagElement = ( unalignedNode.TagElement );
								copy.InnerText = ( unalignedNode.InnerText );
								unalignedNodesCopy.Add( copy );
							}

							prevSiblingMatch.Parent.InsertChildNodes( prevSiblingMatch.ChildNumber+ 1, unalignedNodesCopy );
							
							 for (int counter=0; counter < unalignedNodesThisLevel.Count; counter++)
							{
								mapping[ nextDataRecord ].Add( unalignedNodesCopy[ counter ], unalignedNodesThisLevel[ counter ]);
							}

							unalignedNodesThisLevel.Clear();
							anyInsertion = true;
						}
					}
					else
					{
						// berarti unalignedNodes diapit oleh dua node sibling
						TagNode prevSiblingMatch = reverseDictionary[prevSibling];
						TagNode nextSiblingMatch = reverseDictionary[nextSibling];
						
						// untuk mengatasi kasus di mana unaligned nodes berada pada bagian paling kiri/kanan dari top level generalized node
						if (prevSiblingMatch != null && nextSiblingMatch != null)
						{
							if ( nextSiblingMatch.ChildNumber - prevSiblingMatch.ChildNumber == 1 )
							{
								List<TagNode> unalignedNodesCopy = new List<TagNode>();

								 foreach (TagNode unalignedNode in unalignedNodesThisLevel)
								{
									TagNode copy = new TagNode();
									copy.TagElement = ( unalignedNode.TagElement );
									copy.InnerText = ( unalignedNode.InnerText );
									unalignedNodesCopy.Add( copy );
								}

								prevSiblingMatch.Parent.InsertChildNodes( prevSiblingMatch.ChildNumber+1, unalignedNodesCopy );
								
								 for (int counter=0; counter < unalignedNodesThisLevel.Count; counter++)
								{
									mapping[ nextDataRecord ].Add( unalignedNodesCopy[ counter ], unalignedNodesThisLevel[ counter ]);
								}

								unalignedNodesThisLevel.Clear();
								anyInsertion = true;
							}
						}
					}
				}
				
				// cek apakah ada penyisipan yang berhasil dilakukan
				if (anyInsertion)
				{
					dataRecords.AddRange( R );
					R.Clear();
				}
				
				 foreach (List<TagNode> unalignedNodesThisLevel in unalignedNodes)
				{
					if ( unalignedNodesThisLevel.Any() )
					{
						R.Add( nextDataRecord );
						break;
					}
				}
			}
		}
		
		List< List<string> > tempOutput = new List< List<string> >();
		
		 foreach (DataRecord dataRecord in dataRecords)
		{
			List<string> row = new List<string>();
			extractDataItems( seedDataRecord.RecordElements, mapping[ dataRecord ], row);
			tempOutput.Add( row );
		}
		
		TagNode[] seedElements = seedDataRecord.RecordElements.ToArray();
		int nodesInSeedCount = 0;
		
		 foreach (TagNode tagNode in seedElements)
		{
			nodesInSeedCount += tagNode.GetSubTreeSize();
		}

		bool[] isNotNullColumnArray = new bool[ nodesInSeedCount ];
		
		 for ( int columnCounter=0; columnCounter < isNotNullColumnArray.Length; columnCounter++)
		{
			 foreach (List<string> row in tempOutput)
			{
				if ( row[ columnCounter ] != null )
				{
					isNotNullColumnArray[ columnCounter ] = true;
					break;
				}
			}
		}

		int notNullColumnCount = 0;
		
		 foreach(bool isNotNullColumn in isNotNullColumnArray)
		{
			if ( isNotNullColumn )
			{
				notNullColumnCount++;
			}
		}
		
		 for ( int rowCounter=0; rowCounter < alignedData.Length; rowCounter++)
		{
			List<string> row = tempOutput[ rowCounter ];
			alignedData[ rowCounter ] = new string[ notNullColumnCount ];
			
			int columnCounter = 0;

			 for(int notNullColumnCounter=0; notNullColumnCounter < isNotNullColumnArray.Length; notNullColumnCounter++)
			{
				if ( isNotNullColumnArray[ notNullColumnCounter] )
				{
					alignedData[ rowCounter ][ columnCounter ] = row[ notNullColumnCounter ];
					columnCounter++;
				}
			}
		}
		
		if ( alignedData[0].Length == 0 )
		{
			return null;
		}
		else
		{
			return alignedData;
		}
	}
	
	private void findUnalignedNodes(IEnumerable< TagNode> elements, Dictionary<TagNode, TagNode> matchDictionary, List<List<TagNode>> list)
	{
		List<TagNode> unalignedNodesThisLevel = null;
		bool continuous = false;

		 foreach (TagNode element in elements)
		{
			if ( ! matchDictionary.ContainsValue( element) )
			{
				// sekuen unaligned node berikutnya
				if ( continuous )
				{
					unalignedNodesThisLevel.Add( element );
				}
				// sekuen unaligned node pertama
				else
				{
					unalignedNodesThisLevel = new List<TagNode>();
					unalignedNodesThisLevel.Add( element );
					continuous = true;
				}
			}
			// sekuen berakhir
			else if( continuous )
			{
				list.Add( unalignedNodesThisLevel );
				unalignedNodesThisLevel = null;
				continuous = false;
			}
		}
		
		if ( unalignedNodesThisLevel != null )
		{
			list.Add( unalignedNodesThisLevel );
		}

		 foreach (TagNode element in elements)
		{	
			findUnalignedNodes(element, matchDictionary, list);
		}
	}
	
	private void findUnalignedNodes(TagNode element, Dictionary<TagNode, TagNode> matchDictionary, List<List<TagNode>> list)
	{
		List<TagNode> unalignedNodesThisLevel = null;
		bool continuous = false;

		 foreach (TagNode child in element.Children)
		{
			if ( ! matchDictionary.ContainsValue( child ) )
			{
				// sekuen unaligned node berikutnya
				if ( continuous )
				{
					unalignedNodesThisLevel.Add( child );
				}
				// sekuen unaligned node pertama
				else
				{
					unalignedNodesThisLevel = new List<TagNode>();
					unalignedNodesThisLevel.Add( child );
					continuous = true;
				}
			}
			// sekuen berakhir
			else if( continuous )
			{
				list.Add( unalignedNodesThisLevel );
				unalignedNodesThisLevel = null;
				continuous = false;
			}
		}
		
		if ( unalignedNodesThisLevel != null )
		{
			list.Add( unalignedNodesThisLevel );
		}

		 foreach (TagNode child in element.Children)
		{
			findUnalignedNodes(child, matchDictionary, list);
		}
	}

	private void extractDataItems(IEnumerable<TagNode> seed, Dictionary<TagNode, TagNode> matchDictionary, List<string> row)
	{
		 foreach (TagNode element in seed)
		{
			TagNode original = matchDictionary[ element ];

			if ( original != null)
			{
				row.Add( original.InnerText );
			}
			else
			{
				row.Add( null );
			}
			
		
			 foreach (TagNode child in element.Children)
			{
				extractDataItems(child, matchDictionary, row);
			}
		}
	}
	
	private void extractDataItems(TagNode seed, Dictionary<TagNode, TagNode> matchDictionary, List<string> row)
	{
		TagNode original = matchDictionary[ seed ];

		if ( original != null)
		{
			row.Add( original.InnerText );
		}
		else
		{
			row.Add( null );
		}
		
		 foreach (TagNode child in seed.Children)
		{
			extractDataItems(child, matchDictionary, row);
		}
	}
	
	private List<DataRecord> convertToList(DataRecord[] array)
	{
		List<DataRecord> list = new List<DataRecord>();
		
		 foreach(DataRecord dataRecord in array)
		{
			list.Add( dataRecord );
		}
		
		return list;
	}
	
	private DataRecord copyDataRecord(DataRecord originalDataRecord)
	{
		TagNode[] original = originalDataRecord.RecordElements.ToArray();
		TagNode[] copy = new TagNode[ original.Length ];
		TagNode parentNodeOriginal = original[0].Parent;
		TagNode parentNodeCopy = new TagNode();
		parentNodeCopy.TagElement = ( parentNodeOriginal.TagElement );
		parentNodeCopy.InnerText = ( parentNodeOriginal.InnerText );
		
		 for (int arrayCounter=0; arrayCounter < original.Length; arrayCounter++)
		{
			TagNode tagNode = new TagNode();
			tagNode.Parent = ( parentNodeCopy );
			tagNode.TagElement = ( original[ arrayCounter ].TagElement );
			tagNode.InnerText = ( original[ arrayCounter ].InnerText );
			copy[ arrayCounter ] = tagNode;
			
			 foreach (TagNode child in original[ arrayCounter ].Children )
			{
				createTagNodes(child, tagNode);
			}
		}
		
		return new DataRecord(copy);
	}
	
	private void createTagNodes(TagNode childOriginal, TagNode parentCopy)
	{
		TagNode tagNode = new TagNode();
		tagNode.Parent = ( parentCopy );
		tagNode.TagElement = ( childOriginal.TagElement );
		tagNode.InnerText = ( childOriginal.InnerText );
		
		 foreach (TagNode child in childOriginal.Children )
		{
			createTagNodes(child, tagNode);
		}
	}
	
	private void createSeedAlignment(List<TagNode> seed, List<TagNode> original, Dictionary<TagNode, TagNode> Dictionary)
	{
		 for (int arrayCounter=0; arrayCounter < seed.Count; arrayCounter++)
		{
			Dictionary.Add( seed[ arrayCounter ], original[ arrayCounter ]);

			 for(int childCounter= 1; childCounter <= seed[ arrayCounter ].ChildrenCount; childCounter++)
			{
				createSeedAlignment( seed[ arrayCounter ].GetChildAtIndex( childCounter ), original[ arrayCounter ].GetChildAtIndex( childCounter ), Dictionary);
			}
		}
	}

	private void createSeedAlignment(TagNode seed, TagNode original, Dictionary<TagNode, TagNode> Dictionary)
	{
		Dictionary.Add( seed, original);
		
		 for(int childCounter= 1; childCounter <= seed.ChildrenCount; childCounter++)
		{
			createSeedAlignment( seed.GetChildAtIndex( childCounter ), original.GetChildAtIndex( childCounter ), Dictionary);
		}
	}
}