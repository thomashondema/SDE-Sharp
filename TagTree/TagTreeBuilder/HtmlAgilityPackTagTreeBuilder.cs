
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class HtmlAgilityPackTagTreeBuilder : ITagTreeBuilder
{
	public string BaseURI;

	/**
	 * array yang menyimpan tag2 yang diabaikan dalam membangun pohon tag
	 */
	//private short[] ignoredTags = {  HTMLElements.STYLE, HTMLElements.SCRIPT, HTMLElements.APPLET, HTMLElements.OBJECT};
	private static string[] ignoredTags = { "style", "script", "applet", "object"};
	/**
	 * 
	 */
	private static string filterPattern =  "^[\\s\\W]*$";
	//private static Regex filterRegex = Regex.
	/**
	 * 
	 */
	private static string absoluteURIPattern = "^.*:.*$";
	
	/**
	 * 
	 */
	private TagNodeCreator tagNodeCreator;
	
	/**
	 * Membangun TagTree dari system identifier yang diberikan. Method ini equivalent dengan 
	 * <code>parse(new InputSource(htmlDocument));</code>
	 * 
	 * @param htmlDocument
	 * @return
	 */
	public TagTree buildTagTree(string htmlDocument)
	{   
		return buildTagTree(htmlDocument, false);
	}
	
	public TagTree buildTagTree(string htmlDocument, bool ignoreFormattingTags)
	{
		HtmlDocument htmlDoc = new HtmlDocument();
		htmlDoc.LoadHtml(htmlDocument);

		return buildTagTree(htmlDoc, ignoreFormattingTags);
	}
	
	/**
	 * Membangun TagTree dari InputSource. Proses pembangunan TagTree dilakukan menggunakan parser 
	 * DOM. Root dari TagTree adalah elemen BODY.
	 * 
	 * @param inputSource
	 * @return TagTree dari InputSource
	 */
	 public TagTree buildTagTree(TextReader textReader)
	{
		return buildTagTree(textReader, false);
	}
	public TagTree buildTagTree(TextReader textReader,bool ignoreFormattingTags)
	{
		HtmlDocument htmlDoc = new HtmlDocument();
		htmlDoc.Load(textReader);

		return buildTagTree(htmlDoc, ignoreFormattingTags);
	}

	public TagTree buildTagTree(Stream stream)
	{
		return buildTagTree(stream, false);
	}
	public TagTree buildTagTree(Stream stream,bool ignoreFormattingTags)
	{
		HtmlDocument htmlDoc = new HtmlDocument();
		htmlDoc.Load(stream);

		return buildTagTree(htmlDoc, ignoreFormattingTags);
	}
	public TagTree buildTagTree(HtmlDocument htmlDocument)
	{
		return buildTagTree(htmlDocument,false);
	}
	public TagTree buildTagTree(HtmlDocument htmlDocument, bool ignoreFormattingTags)
	{
		TagTree tree = null;
		
		// jika formatting tags diabaikan dalam pembuatan pohon tag
		if ( ignoreFormattingTags )
		{
			tagNodeCreator = new IgnoreFormattingTagsTagNodeCreator(BaseURI);
		}
		// jika formatting tags tidak diabaikan dalam pembuatan pohon tag
		else
		{
			tagNodeCreator = new DefaultTagNodeCreator(BaseURI);
		}

		// parse dokumen HTML menjadi pohon DOM dan dapatkan Document-nya
		/*
		DOMParser parser = new DOMParser();
		parser.parse(inputSource);
		Document documentNode = parser.getDocument();
		*/
		HtmlNode documentNode = htmlDocument.DocumentNode;


		// dapatkan node BODY dan salin sebagai root untuk pohon tag
		HtmlNode bodyNode = documentNode.Descendants("BODY").FirstOrDefault();
		TagNode rootTagNode = new TagNode();
		tree = new TagTree();
		tree.Root = rootTagNode;
		rootTagNode.TagElement =  bodyNode.Name;
		tree.AddTagNodeAtLevel(rootTagNode);
		// salin Node DOM menjadi TagNode untuk anak2 dari root
		HtmlNode child = bodyNode.FirstChild;

		while(child != null)
		{
			tagNodeCreator.createTagNodes(child, rootTagNode, tree);
			child = child.NextSibling;
		}
			
		// berikan nomor node pada TagNode di TagTree
		tree.AssignNodeNumber();
		
		return tree;
	}
	
	
	private interface TagNodeCreator
	{
		 void createTagNodes(HtmlNode node, TagNode parent, TagTree tagTree);
	}
	
	private class DefaultTagNodeCreator : TagNodeCreator
	{
		private string baseURI;
		public DefaultTagNodeCreator(string baseURI){
			this.baseURI  = baseURI;
		}

		/**
		 * Membuat TagNode yang merupakan node pada TagTree dengan menggunakan informasi dari Node yang 
		 * dihasilkan oleh parser DOM. Method ini dipanggil secara rekursif.
		 * 
		 * @param node node yang akan dijadikan sumber informasi untuk membuat TagNode
		 * @param parent Parent dari TagNode yang akan dibuat
		 * @param tagTree TagTree yang sedang dibuat
		 */
		public void createTagNodes(HtmlNode node, TagNode parent, TagTree tagTree)
		{
			// jika node DOM bertipe ELEMENT
			if (node.NodeType == HtmlNodeType.Element)
			{
				// dapatkan tagCode-nya (representasi tag dalam short)

				// jika tagCode tidak termasuk dalam daftar tag yang diabaikan
				//if (!arrayContains(ignoredTags, tagCode) )
				if(!ignoredTags.Contains(node.Name))
				{
					// salin node DOM menjadi TagNode (tagCode-nya)
					TagNode tagNode = new TagNode();
					tagNode.TagElement = node.Name;
					// set parent dari TagNode yang baru dibuat
					tagNode.Parent = (parent);
					// tambahkan ke dalam TagTree
					tagTree.AddTagNodeAtLevel(tagNode);
					
					// jika tagCode merupakan tag IMG
					if ( node.Name == "img")
					{
						// dapatkan nilai atribut src-nya
						var attributesMap = node.Attributes;
						string imgURI = attributesMap["src"].Value;
						
						// jika URI pada atribut src bukan merupakan URI absolut (URI relatif)
						if ( ! Regex.IsMatch(imgURI,absoluteURIPattern) )
						{
							// tambahkan baseURI sehingga menjadi URI absolut
							imgURI = baseURI + imgURI;
						}
						
						// tambahkan tag IMG dengan src-nya sebagai teks HTML pada TagNode parent-nya
						string imgText = string.Format("<img src=\"{0}\" />", imgURI);
						tagNode.AppendInnerText( imgText );
					}
					// jika tagCode merupakan tag A
					else if( node.Name == "a" )
					{
						// append teks di dalam tag A ke innerText milik parent
						parent.AppendInnerText( node.InnerText );
						// dapatkan map atribut dari tag A ini
						var attributesMap = node.Attributes;
						
						// jika tag A ini memiliki atribut href
						if ( attributesMap["href"] != null)
						{
							// dapatkan nilai atribut href-nya
							string linkURI = attributesMap["href"].Value;
						
							// jika nilai atribut href bukan merupakan URI absolut
							if ( ! Regex.IsMatch(linkURI,absoluteURIPattern) )
							{
								// tambahkan baseURI sehingga menjadi URI absolut
								linkURI = baseURI + linkURI;
							}
						
							// tambahkan tag A dengan href-nya dan teks Link sebagai teks HTML pada TagNode parent-nya
							string linkText = string.Format("<a href=\"{0}\">Link&lt;&lt;</a>", linkURI);
							tagNode.AppendInnerText( linkText );
						}
					}
					
					// lakukan secara rekursif penyalinan Node DOM menjadi DOM pada anak2 dari node ini (jika memiliki anak)
					HtmlNode child = node.FirstChild;
					
					while(child != null)
					{
						createTagNodes(child, tagNode, tagTree);
						child = child.NextSibling;
					}
				}
			}
			// jika node DOM bertipe TEXT
			else if (node.NodeType == HtmlNodeType.Text)
			{
				
				// kalau Text node ini hanya berisi string yang tidak terbaca, maka tidak usah 
				// disimpan sebagai innerText pada node parent-nya
				if ( ! Regex.IsMatch(node.InnerText,filterPattern) )
				{
					// jika mengandung teks yang bisa terbaca, maka di-append pada innerText node parent-nya
					parent.AppendInnerText(node.InnerText);
				}
			}
			// selain bertipe ELEMENT dan TEXT diabaikan
		}
	}
	
	private class IgnoreFormattingTagsTagNodeCreator : TagNodeCreator
	{
		private string baseURI;
		public IgnoreFormattingTagsTagNodeCreator(string baseURI){
			this.baseURI  = baseURI;
		}
		/**
		 * array yang menyimpan tag2 formatting
		 */
		//private short[] formattingTags = { HTMLElements.B, HTMLElements.I, HTMLElements.U, HTMLElements.STRONG, HTMLElements.STRIKE, HTMLElements.EM, HTMLElements.BIG, HTMLElements.SMALL, HTMLElements.SUP, HTMLElements.SUP, HTMLElements.BDO, HTMLElements.BR};
		private string[] formattingTags = new string[] { "b","i","u","strong","strike","em","big","small","sup","bdo","br"};

		public void createTagNodes(HtmlNode node, TagNode parent, TagTree tagTree)
		{
			// jika node DOM bertipe ELEMENT
			if (node.NodeType == HtmlNodeType.Element)
			{
				// jika tagCode termasuk dalam daftar formatting tags
				if ( formattingTags.Contains( node.Name) )
				{
					// khusus untuk tag BR hanya menambahkan tag <BR /> ke innerText parent TagNode-nya
					if ( node.Name == "br")
					{
						parent.AppendInnerText( "<BR />" );
					}
					else
					{
						// append teks di dalam tagCode (beserta tagCode itu sendiri) ke dalam innerText parent-nya
						parent.AppendInnerText( string.Format("<{0}>%s</%s>", node.Name, node.InnerText) );
					}
				}
				// jika tagCode tidak termasuk dalam daftar tag yang diabaikan
				else if (!ignoredTags.Contains(node.Name) )
				{
					// salin node DOM menjadi TagNode (tagCode-nya)
					TagNode tagNode = new TagNode();
					tagNode.TagElement = node.Name;
					// set parent dari TagNode yang baru dibuat
					tagNode.Parent = (parent);
					// tambahkan ke dalam TagTree
					tagTree.AddTagNodeAtLevel(tagNode);
					
					// jika tagCode merupakan tag IMG
					if ( node.Name == "img" )
					{
						// dapatkan nilai atribut src-nya
						HtmlAttributeCollection attributesMap = node.Attributes;
						string imgURI = attributesMap["src"].Value;
						
						// jika URI pada atribut src bukan merupakan URI absolut (URI relatif)
						if ( ! Regex.IsMatch(imgURI,absoluteURIPattern) )
						{
							// tambahkan baseURI sehingga menjadi URI absolut
							imgURI = baseURI + imgURI;
						}
						
						// tambahkan tag IMG dengan src-nya sebagai teks HTML pada TagNode parent-nya
						string imgText = string.Format("<img src=\"{0}\" />", imgURI);
						tagNode.AppendInnerText( imgText );
					}
					// jika tagCode merupakan tag A
					else if( node.Name=="a")
					{         
						// append teks di dalam tag A ke innerText milik parent
						parent.AppendInnerText( node.InnerText );
						// dapatkan map atribut dari tag A ini
						HtmlAttributeCollection attributesMap = node.Attributes;
						
						// jika tag A ini memiliki atrbiut href
						if ( attributesMap["href"] != null)
						{
							// dapatkan nilai atribut href-nya
							string linkURI = attributesMap["href"].Value;
						
							// jika nilai atribut href bukan merupakan URI absolut
							if ( ! Regex.IsMatch(linkURI,absoluteURIPattern) )
							{
								// tambahkan baseURI sehingga menjadi URI absolut
								linkURI = baseURI + linkURI;
							}
						
							// tambahkan tag A dengan href-nya dan teks Link sebagai teks HTML pada TagNode parent-nya
							string linkText = string.Format("<a href=\"{0}\">Link&lt;&lt;</a>", linkURI);
							tagNode.AppendInnerText( linkText );
						}
					}
					
					// lakukan secara rekursif penyalinan Node DOM menjadi DOM pada anak2 dari node ini (jika memiliki anak)
					HtmlNode child = node.FirstChild;
					
					while(child != null)
					{
						createTagNodes(child, tagNode, tagTree);
						child = child.NextSibling;
					}
				}
			}
			// jika node DOM bertipe TEXT
			else if (node.NodeType == HtmlNodeType.Text)
			{				
				// kalau Text node ini hanya berisi string yang tidak terbaca, maka tidak usah 
				// disimpan sebagai innerText pada node parent-nya
				if ( ! Regex.IsMatch(node.InnerText,filterPattern))
				{
					// jika mengandung teks yang bisa terbaca, maka di-append pada innerText node parent-nya
					parent.AppendInnerText(node.InnerText);
				}
			}
		}
	}
}