public interface ITreeMatcher
{
	double matchScore(TagNode A, TagNode B);
	double matchScore(TagNode[] A, TagNode[] B);
	double matchScore(TagTree A, TagTree B);
	double normalizedMatchScore(TagNode A, TagNode B);
	double normalizedMatchScore(TagNode[] A, TagNode[] B);
	double normalizedMatchScore(TagTree A, TagTree B);
	TreeAlignment align(TagNode[] A, TagNode[] B);
	TreeAlignment align(TagNode A, TagNode B);
}