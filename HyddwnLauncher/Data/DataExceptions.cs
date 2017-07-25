using System;

namespace HyddwnLauncher.Data
{
    public class DatabaseWarningException : Exception
    {
        public DatabaseWarningException(string msg)
            : base(msg)
        {
        }

        public DatabaseWarningException(string msg, string source)
            : this(msg)
        {
            Source = source;
        }

        public override string ToString()
        {
            return Source + ": " + Message;
        }
    }

    public class DatabaseErrorException : Exception
    {
        public DatabaseErrorException(string msg)
            : base(msg)
        {
        }

        public DatabaseErrorException(string msg, string source)
            : this(msg)
        {
            Source = source;
        }

        public override string ToString()
        {
            return Source + ": " + Message;
        }
    }

    public class CsvDatabaseWarningException : DatabaseWarningException
    {
        public CsvDatabaseWarningException(string source, int line, string msg, params object[] args)
            : base(string.Format(msg, args), source)
        {
            Line = line;
        }

        public CsvDatabaseWarningException(string msg)
            : base(msg)
        {
        }

        public CsvDatabaseWarningException(string msg, params object[] args)
            : base(string.Format(msg, args))
        {
        }

        public int Line { get; set; }

        public override string ToString()
        {
            return $"{Source} on line {Line}: {Message}";
        }
    }

    public class FieldCountException : CsvDatabaseWarningException
    {
        public FieldCountException(int expectedAmount, int amount)
            : base("Expected at least {0} fields, found {1}.", expectedAmount, amount)
        {
        }
    }
}