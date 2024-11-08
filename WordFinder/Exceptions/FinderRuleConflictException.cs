namespace WordFinder.Exceptions
{
	public class FinderRuleConflictException : FinderRuleException
	{
		public FinderRuleConflictException(string message) : base(message) { }
		public FinderRuleConflictException(string message, Exception innerException) : base(message, innerException) { }
	}
}
