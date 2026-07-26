using NUnit.Framework;
using SourceSerializer;

public class WhitespaceStripperTests
{
    [Test]
    public void Strip_Empty_ReturnsEmpty()
    {
        Assert.That(WhitespaceStripper.Strip(""), Is.EqualTo(""));
    }

    [Test]
    public void Strip_AllWhitespace_ReturnsEmpty()
    {
        Assert.That(WhitespaceStripper.Strip("   \t \n \r  "), Is.EqualTo(""));
    }

    [Test]
    public void Strip_NoWhitespace_ReturnsSame()
    {
        Assert.That(WhitespaceStripper.Strip("hello"), Is.EqualTo("hello"));
    }

    [Test]
    public void Strip_LeadingWhitespace()
    {
        Assert.That(WhitespaceStripper.Strip("  hello"), Is.EqualTo("hello"));
    }

    [Test]
    public void Strip_TrailingWhitespace()
    {
        Assert.That(WhitespaceStripper.Strip("hello  "), Is.EqualTo("hello"));
    }

    [Test]
    public void Strip_BetweenFields()
    {
        Assert.That(WhitespaceStripper.Strip("1.5, -2.5"), Is.EqualTo("1.5,-2.5"));
    }

    [Test]
    public void Strip_PreservesQuotedString()
    {
        Assert.That(WhitespaceStripper.Strip("\"hello world\""), Is.EqualTo("\"hello world\""));
    }

    [Test]
    public void Strip_PreservesQuotedString_StripsOutside()
    {
        Assert.That(WhitespaceStripper.Strip("  \"hello world\"  "), Is.EqualTo("\"hello world\""));
    }

    [Test]
    public void Strip_MultipleQuotedStrings_StripsBetween()
    {
        Assert.That(WhitespaceStripper.Strip("\"a\" , \"b\""), Is.EqualTo("\"a\",\"b\""));
    }

    [Test]
    public void Strip_EscapedQuote_InsideString()
    {
        // \" is a literal backslash-quote — should be preserved, and not treated as closing quote
        Assert.That(WhitespaceStripper.Strip("\"hello \\\"world\\\"\""),
            Is.EqualTo("\"hello \\\"world\\\"\""));
    }

    [Test]
    public void Strip_EscapedQuote_WithSurroundingWhitespace()
    {
        Assert.That(WhitespaceStripper.Strip("  \"say \\\"hi\\\"\"  "),
            Is.EqualTo("\"say \\\"hi\\\"\""));
    }

    [Test]
    public void Strip_StringAtEnd()
    {
        Assert.That(WhitespaceStripper.Strip("123, \"end\""), Is.EqualTo("123,\"end\""));
    }

    [Test]
    public void Strip_OnlyQuotedString()
    {
        Assert.That(WhitespaceStripper.Strip("\"only\""), Is.EqualTo("\"only\""));
    }

    [Test]
    public void Strip_UnclosedQuote_Preserved()
    {
        // 无闭合引号——整个剩余输入被视为字符串内部，空白符保留
        Assert.That(WhitespaceStripper.Strip("\"hello world"), Is.EqualTo("\"hello world"));
    }
}
