namespace HabitTracker.Core.Errors;

public enum HabitErrorKind
{
    ValidationFailed,     
    DuplicateName,        
    LimitReached,         
    NotFound,             
    ClockMovedBack,       
    Storage             
}

public class HabitException : Exception
{
    public HabitErrorKind Kind { get; }
    
    public IReadOnlyDictionary<string, string> FieldErrors { get; }

    public HabitException(
        HabitErrorKind kind,
        string message,
        IReadOnlyDictionary<string, string>? fieldErrors = null,
        Exception? inner = null)
        : base(message, inner)
    {
        Kind = kind;
        FieldErrors = fieldErrors ?? new Dictionary<string, string>();
    }
}