# StringSpanBuilder and StringSpanWriter

Library that provides lower allocations and better performance than the standard StringBuilder based ones. By storing the appended strings as spans a 
lot of the duplicate copying is removed and less allocations are required to keep track of them.
Depending on usage this can be x2.5 times quicker and use 10% of the memory (excluding final string creation).

## StringSpanBuilder
A lot of the standard StringBuilder methods have been duplicated here except for the char and char[] based ones. If you need to loop through a string and append a single char at a time to a builder then StringBuilder is the best for that. Similarly if you need to often append char arrays.
For appending of strings and substrings StringSpanBuilder is the best choice.

```C#
public class StringSpanBuilder
{
    public StringSpanBuilder()
    public StringSpanBuilder(int capacity)
	public StringSpanBuilder(string value)
	public StringSpanBuilder(string value, int capacity)
	public StringSpanBuilder Append(string value)
	public StringSpanBuilder Append(string value, int startIndex, int length)
	public StringSpanBuilder AppendFormat(string format, object arg0)
	public StringSpanBuilder AppendFormat(string format, object arg0, object arg1)
	public StringSpanBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
	public StringSpanBuilder AppendFormat(string format, params object[] args)
	public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, object arg0)
	public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, object arg0, object arg1)
	public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, object arg0, object arg1, object arg2)
	public StringSpanBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
	public StringSpanBuilder AppendLine()
	public StringSpanBuilder AppendLine(string value)
	public StringSpanBuilder Clear
	public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	public bool EndsWith(char value)
	public StringSpanBuilder Prepend(string value)
	public StringSpanBuilder Prepend(string value, int startIndex, int length)
	public bool StartsWith(char value)
	public string ToString()
	public StringSpanBuilder Trim()
	public StringSpanBuilder TrimStart()
	public StringSpanBuilder TrimEnd()
	public int Length { get; set; }
}
```

## StringSpanWriter
Equivalent to the StringWriter class but uses the StringSpanBuilder as it's backing store. Best for use cases where you know strings will be appended.
When a single char is written (`Write(char)`) it will be mapped to static strings for commonly used symbols. Otherwise a new single character string will be allocated.
For the `Write(char[], int, int)` overload a new string will be allocated with a copy of that data.
