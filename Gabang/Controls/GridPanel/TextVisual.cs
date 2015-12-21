using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Gabang.Controls {
    public class TextVisual : DrawingVisual {
        public int Row { get; set; }

        public int Column { get; set; }

        private string _text;
        public string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                _formattedText = null;
                _drawValid = false;
            }
        }

        private FormattedText _formattedText;
        public FormattedText GetFormattedText() {
            if (_formattedText == null) {
                _formattedText = new FormattedText(
                    Text,
                    CultureInfo.CurrentUICulture,
                    CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                    new Typeface("Segoe"),
                    12.0,
                    Brushes.Black);
            }
            return _formattedText;
        }

        private bool _drawValid = false;
        public void Draw() {
            if (_drawValid) return;
            DrawingContext dc = RenderOpen();
            try {
                var formattedText = GetFormattedText();
                dc.DrawText(formattedText, new Point(0, 0));
                _drawValid = true;
            } finally {
                dc.Close();
            }
        }
    }
}
