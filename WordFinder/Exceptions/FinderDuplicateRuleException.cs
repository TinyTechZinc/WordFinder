namespace WordFinder.Exceptions
{
	public class FinderDuplicateRuleException : FinderRuleException
	{
		public FinderDuplicateRuleException(string message) : base(message) { }
		public FinderDuplicateRuleException(string message, Exception innerException) : base(message, innerException) { }
	}
}
