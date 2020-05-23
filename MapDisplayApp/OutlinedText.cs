using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapDisplayApp
{
    public class OutlinedText : FrameworkElement
    {
        private GlyphRun glyphRun;
        private Geometry outline;

        public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontSizeProperty = TextBlock.FontSizeProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontFamilyProperty = TextBlock.FontFamilyProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontStyleProperty = TextBlock.FontStyleProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontWeightProperty = TextBlock.FontWeightProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty FontStretchProperty = TextBlock.FontStretchProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty ForegroundProperty = TextBlock.ForegroundProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata((o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty BackgroundProperty = TextBlock.BackgroundProperty.AddOwner(
            typeof(OutlinedText), new FrameworkPropertyMetadata(Brushes.White, (o, e) => ((OutlinedText)o).glyphRun = null) { AffectsMeasure = true });

        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            "OutlineThickness", typeof(double), typeof(OutlinedText),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure, (o, e) => ((OutlinedText)o).glyphRun = null));

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public double FontSize
        {
            get { return (double)this.GetValue(FontSizeProperty); }
            set { this.SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)this.GetValue(FontFamilyProperty); }
            set { this.SetValue(FontFamilyProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)this.GetValue(FontStyleProperty); }
            set { this.SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)this.GetValue(FontWeightProperty); }
            set { this.SetValue(FontWeightProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)this.GetValue(FontStretchProperty); }
            set { this.SetValue(FontStretchProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)this.GetValue(ForegroundProperty); }
            set { this.SetValue(ForegroundProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)this.GetValue(BackgroundProperty); }
            set { this.SetValue(BackgroundProperty, value); }
        }

        public double OutlineThickness
        {
            get { return (double)this.GetValue(OutlineThicknessProperty); }
            set { this.SetValue(OutlineThicknessProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return this.CheckGlyphRun() ? this.outline.Bounds.Size : new Size();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.CheckGlyphRun())
            {
                var location = this.outline.Bounds.Location;
                drawingContext.PushTransform(new TranslateTransform(-location.X, -location.Y));
                drawingContext.DrawGeometry(this.Background, null, this.outline);
                drawingContext.DrawGlyphRun(this.Foreground, this.glyphRun);
            }
        }

        private bool CheckGlyphRun()
        {
            if (this.glyphRun == null)
            {
                if (string.IsNullOrEmpty(this.Text))
                {
                    return false;
                }

                var typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
                GlyphTypeface glyphTypeface;

                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                {
                    return false;
                }

                var glyphIndices = new ushort[this.Text.Length];
                var advanceWidths = new double[this.Text.Length];

                for (int i = 0; i < this.Text.Length; i++)
                {
                    var glyphIndex = glyphTypeface.CharacterToGlyphMap[this.Text[i]];
                    glyphIndices[i] = glyphIndex;
                    advanceWidths[i] = glyphTypeface.AdvanceWidths[glyphIndex] * this.FontSize;
                }

                this.glyphRun = new GlyphRun(glyphTypeface, 0, false, this.FontSize, 1f, glyphIndices, new Point(), advanceWidths, null, null, null, null, null, null);

                this.outline = this.glyphRun.BuildGeometry().GetWidenedPathGeometry(new Pen(null, this.OutlineThickness * 2d));
            }

            return true;
        }
    }
}
