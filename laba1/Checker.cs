using System.Windows;

namespace laba1
{
    public enum CheckerColor { White, Black, Red }
    public enum CheckerType { Regular, King }

    public class Checker(CheckerColor color, int row, int col)
    {
        public CheckerColor Color { get; set; } = color;
        public CheckerType Type { get; set; } = CheckerType.Regular;
        public bool HasCaptured { get; set; }
        public int Row { get; set; } = row;
        public int Col { get; set; } = col;
        public UIElement? UIElement { get; set; }
    }

}
