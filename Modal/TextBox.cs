using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

public class TextBoxWriter : TextWriter
{
    private TextBox _textBox;

    public TextBoxWriter(TextBox textBox)
    {
        _textBox = textBox;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        _textBox.AppendText(value.ToString());
    }

    public override void Write(string value)
    {
        _textBox.AppendText(value + Environment.NewLine);
    }
}
