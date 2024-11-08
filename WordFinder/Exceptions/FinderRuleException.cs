namespace WordFinder.Exceptions
{
    public class FinderRuleException : FinderException
    {
        public FinderRuleException(string message) : base(message) { }
        public FinderRuleException(string message, Exception innerException) : base(message, innerException) { }
    }
}
