﻿using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using OpenUtau.Core.Ustx;
using System.Linq;

namespace OpenUtau.App.Controls {
    class PartControl : TemplatedControl {
        public static readonly DirectProperty<PartControl, double> TickWidthProperty =
            AvaloniaProperty.RegisterDirect<PartControl, double>(
                nameof(TickWidth),
                o => o.TickWidth,
                (o, v) => o.TickWidth = v);
        public static readonly DirectProperty<PartControl, double> TrackHeightProperty =
            AvaloniaProperty.RegisterDirect<PartControl, double>(
                nameof(TrackHeight),
                o => o.TrackHeight,
                (o, v) => o.TrackHeight = v);
        public static readonly DirectProperty<PartControl, Point> PositionProperty =
            AvaloniaProperty.RegisterDirect<PartControl, Point>(
                nameof(Position),
                o => o.Position,
                (o, v) => o.Position = v);
        public static readonly DirectProperty<PartControl, string> TextProperty =
            AvaloniaProperty.RegisterDirect<PartControl, string>(
                nameof(Text),
                o => o.Text,
                (o, v) => o.Text = v);
        public static readonly DirectProperty<PartControl, UPart?> PartProperty =
            AvaloniaProperty.RegisterDirect<PartControl, UPart?>(
                nameof(Part),
                o => o.Part,
                (o, v) => o.Part = v);

        // Tick width in pixel.
        public double TickWidth {
            get => tickWidth;
            set => SetAndRaise(TickWidthProperty, ref tickWidth, value);
        }
        public double TrackHeight {
            get => trackHeight;
            set => SetAndRaise(TrackHeightProperty, ref trackHeight, value);
        }
        public Point Position {
            get { return position; }
            set { SetAndRaise(PositionProperty, ref position, value); }
        }
        public string Text {
            get { return text; }
            set { SetAndRaise(TextProperty, ref text, value); }
        }
        public UPart? Part {
            get { return part; }
            set { SetAndRaise(PartProperty, ref part, value); }
        }

        private double tickWidth;
        private double trackHeight;
        private Point position;
        private string text = string.Empty;
        private UPart? part;

        private FormattedText? formattedText;
        private Pen notePen = new Pen(Brushes.White, 3);

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change) {
            base.OnPropertyChanged(change);
            if (!change.IsEffectiveValueChange) {
                return;
            }
            if (change.Property == PositionProperty) {
                Canvas.SetLeft(this, Position.X);
                Canvas.SetTop(this, Position.Y);
            }
        }

        public override void Render(DrawingContext context) {
            // Background
            context.DrawRectangle(Background, null, new Rect(1, 1, Width - 2, Height - 2), 4, 4);

            // Text
            if (formattedText == null || formattedText.Text != Text) {
                formattedText = new FormattedText(
                    Text,
                    new Typeface(TextBlock.GetFontFamily(this), FontStyle.Normal, FontWeight.Bold),
                    12,
                    TextAlignment.Left,
                    TextWrapping.NoWrap,
                    new Size(Width, Height));
            }
            context.DrawText(Foreground, new Point(3, 2), formattedText);

            // Notes
            if (Part != null && Part is UVoicePart voicePart) {
                int maxTone = voicePart.notes.Max(note => note.tone);
                int minTone = voicePart.notes.Min(note => note.tone);
                if (maxTone - minTone < 36) {
                    int additional = (36 - (maxTone - minTone)) / 2;
                    minTone -= additional;
                    maxTone += additional;
                }
                using var pushedState = context.PushPreTransform(Matrix.CreateScale(1, trackHeight / (maxTone - minTone)));
                foreach (var note in voicePart.notes) {
                    var start = new Point((int)(note.position * tickWidth), maxTone - note.tone);
                    var end = new Point((int)(note.End * tickWidth), maxTone - note.tone);
                    context.DrawLine(notePen, start, end);
                }
            }
        }
    }
}
