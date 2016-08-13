public interface ITagTreeBuilder{

	/**
	 * Membangun TagTree dari InputSource
	 * 
	 * @param inputSource
	 * @return TagTree dari InputSource
	 */
	 //TagTree buildTagTree(InputSource inputSource);
	
	/**
	 * Membangun TagTree dari system identifier yang diberikan. Method ini equivalent dengan 
	 * <code>parse(new InputSource(htmlDocument));</code>
	 * 
	 * @param htmlDocument
	 * @return
	 */
	 TagTree buildTagTree(string htmlDocument);
	
	 //TagTree buildTagTree(InputSource inputSource, bool ignoreFormattingTags);
	
	 TagTree buildTagTree(string htmlDocument, bool ignoreFormattingTags);
}