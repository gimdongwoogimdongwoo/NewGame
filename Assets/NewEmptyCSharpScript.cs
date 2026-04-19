using System.IO;
using System.Text;

public class GenerateHangulCharacters
{
    public static void Main()
    {
        StringBuilder sb = new StringBuilder();

        // ASCII 기본 범위 (공백 포함)
        for (int i = 0x20; i <= 0x7E; i++)
        {
            sb.Append((char)i);
        }
        sb.AppendLine();

        // 한글 자모 (U+3130 ~ U+318F)
        for (int i = 0x3130; i <= 0x318F; i++)
        {
            sb.Append((char)i);
        }
        sb.AppendLine();

        // 한글 완성형 음절 (U+AC00 ~ U+D7AF)
        for (int i = 0xAC00; i <= 0xD7AF; i++)
        {
            sb.Append((char)i);
        }

        File.WriteAllText("characters.txt", sb.ToString());
    }
}