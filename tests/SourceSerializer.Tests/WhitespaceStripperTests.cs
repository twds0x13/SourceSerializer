using NUnit.Framework;
using SourceSerializer;

public class WhitespaceStripperTests
{
    [Test]
    public void Strip_Empty_ReturnsEmpty()
    {
        using var ws = new WhitespaceStripper("");
        Assert.That(ws.Span.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void Strip_AllWhitespace_ReturnsEmpty()
    {
        using var ws = new WhitespaceStripper("   \t \n \r  ");
        Assert.That(ws.Span.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void Strip_NoWhitespace_ReturnsSame()
    {
        using var ws = new WhitespaceStripper("hello");
        Assert.That(ws.Span.ToString(), Is.EqualTo("hello"));
    }

    [Test]
    public void Strip_LeadingWhitespace()
    {
        using var ws = new WhitespaceStripper("  hello");
        Assert.That(ws.Span.ToString(), Is.EqualTo("hello"));
    }

    [Test]
    public void Strip_TrailingWhitespace()
    {
        using var ws = new WhitespaceStripper("hello  ");
        Assert.That(ws.Span.ToString(), Is.EqualTo("hello"));
    }

    [Test]
    public void Strip_BetweenFields()
    {
        using var ws = new WhitespaceStripper("1.5, -2.5");
        Assert.That(ws.Span.ToString(), Is.EqualTo("1.5,-2.5"));
    }

    [Test]
    public void Strip_PreservesQuotedString()
    {
        using var ws = new WhitespaceStripper("\"hello world\"");
        Assert.That(ws.Span.ToString(), Is.EqualTo("\"hello world\""));
    }

    [Test]
    public void Strip_PreservesQuotedString_StripsOutside()
    {
        using var ws = new WhitespaceStripper("  \"hello world\"  ");
        Assert.That(ws.Span.ToString(), Is.EqualTo("\"hello world\""));
    }

    [Test]
    public void Strip_MultipleQuotedStrings_StripsBetween()
    {
        using var ws = new WhitespaceStripper("\"a\" , \"b\"");
        Assert.That(ws.Span.ToString(), Is.EqualTo("\"a\",\"b\""));
    }

    [Test]
    public void Strip_EscapedQuote_InsideString()
    {
        // \" is a literal backslash-quote — should be preserved, and not treated as closing quote
        using var ws = new WhitespaceStripper("\"hello \\\"world\\\"\"");
        Assert.That(ws.Span.ToString(), Is.EqualTo("\"hello \\\"world\\\"\""));
    }

    [Test]
    public void Strip_EscapedQuote_WithSurroundingWhitespace()
    {
        using var ws = new WhitespaceStripper("  \"say \\\"hi\\\"\"  ");
        Assert.That(ws.Span.ToString(), Is.EqualTo("\"say \\\"hi\\\"\""));
    }

    [Test]
    public void Strip_StringAtEnd()
    {
        using var ws = new WhitespaceStripper("123, \"end\"");
        Assert.That(ws.Span.ToString(), Is.EqualTo("123,\"end\""));
    }

    [Test]
    public void Strip_OnlyQuotedString()
    {
        using var ws = new WhitespaceStripper("\"only\"");
        Assert.That(ws.Span.ToString(), Is.EqualTo("\"only\""));
    }

    [Test]
    public void Strip_UnclosedQuote_Preserved()
    {
        // 无闭合引号——整个剩余输入被视为字符串内部，空白符保留
        using var ws = new WhitespaceStripper("\"hello world");
        Assert.That(ws.Span.ToString(), Is.EqualTo("\"hello world"));
    }
}
