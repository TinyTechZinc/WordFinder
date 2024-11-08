﻿namespace WordFinder.Exceptions
{
    public class FinderException : Exception
    {
        public FinderException(string message) : base(message) { }
        public FinderException(string message, Exception innerException) : base(message, innerException) { }
    }
}
