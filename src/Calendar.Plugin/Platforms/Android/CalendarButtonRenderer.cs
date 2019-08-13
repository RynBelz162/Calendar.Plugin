using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Runtime;
using Calendar.Plugin.Platforms.Android;
using Calendar.Plugin.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using Math = System.Math;
using Plugin.CurrentActivity;

[assembly: Xamarin.Forms.ExportRenderer(typeof(CalendarButton), typeof(CalendarButtonRenderer))]
namespace Calendar.Plugin.Platforms.Android
{
    [Preserve(AllMembers = true)]
    public class CalendarButtonRenderer : ButtonRenderer
    {
        public CalendarButtonRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (Control == null) return;
            if (!(Element is CalendarButton element))
            {
                return;
            }

            Control.TextChanged += (sender, a) =>
            {
                if (Control.Text == element.TextWithoutMeasure || (string.IsNullOrEmpty(Control.Text) && string.IsNullOrEmpty(element.TextWithoutMeasure))) return;
                Control.Text = element.TextWithoutMeasure;
            };
            Control.SetPadding(1, 1, 1, 1);
            Control.ViewTreeObserver.GlobalLayout += (sender, args) => ChangeBackgroundPattern();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (!(Element is CalendarButton element))
            {
                return;
            }

            if (e.PropertyName == nameof(element.TextWithoutMeasure) || e.PropertyName == "Renderer")
            {
                Control.Text = element.TextWithoutMeasure;
            }

            if (e.PropertyName == nameof(element.TextColor) || e.PropertyName == "Renderer")
            {
                Control.SetTextColor(element.TextColor.ToAndroid());
            }

            if (e.PropertyName == nameof(element.BorderWidth) || e.PropertyName == nameof(element.TintBorderColor)
                                                              || e.PropertyName == nameof(element.TintColor) || e.PropertyName == "Renderer")
            {
                if (element.BackgroundPattern == null)
                {
                    if (element.BackgroundImage == null)
                    {
                        var drawable = new GradientDrawable();
                        drawable.SetShape(ShapeType.Rectangle);
                        var borderWidth = (int)Math.Ceiling(element.BorderWidth);
                        drawable.SetStroke(borderWidth > 0 ? borderWidth + 1 : borderWidth, element.TintBorderColor.ToAndroid());
                        drawable.SetColor(element.TintColor.ToAndroid());
                        Control.SetBackground(drawable);
                    }
                    else
                    {
                        ChangeBackgroundImage();
                    }
                }
                else
                {
                    ChangeBackgroundPattern();
                }
            }

            if (e.PropertyName == nameof(element.BackgroundPattern))
            {
                ChangeBackgroundPattern();
            }

            if (e.PropertyName == nameof(element.BackgroundImage))
            {
                ChangeBackgroundImage();
            }
        }

        protected async void ChangeBackgroundImage()
        {
            var element = Element as CalendarButton;
            if (element?.BackgroundImage == null) return;

            var d = new List<Drawable>();
            var image = await getBitmap(element.BackgroundImage);
            d.Add(new BitmapDrawable(CrossCurrentActivity.Current.Activity.Resources, image));
            var drawable = new GradientDrawable();
            drawable.SetShape(ShapeType.Rectangle);
            var borderWidth = (int)Math.Ceiling(element.BorderWidth);
            drawable.SetStroke(borderWidth > 0 ? borderWidth + 1 : borderWidth, element.TintBorderColor.ToAndroid());
            drawable.SetColor(Color.Transparent);
            d.Add(drawable);
            var layer = new LayerDrawable(d.ToArray());
            layer.SetLayerInset(d.Count - 1, 0, 0, 0, 0);
            Control.SetBackground(layer);
        }

        protected void ChangeBackgroundPattern()
        {
            var element = Element as CalendarButton;
            if (element?.BackgroundPattern == null || Control.Width == 0) return;

            var d = new List<Drawable>();
            for (var i = 0; i < element.BackgroundPattern.Pattern.Count; i++)
            {
                var bp = element.BackgroundPattern.Pattern[i];
                d.Add(!string.IsNullOrEmpty(bp.Text)
                    ? new TextDrawable(bp.Color.ToAndroid()) { Pattern = bp }
                    : new ColorDrawable(bp.Color.ToAndroid()));
            }
            var drawable = new GradientDrawable();
            drawable.SetShape(ShapeType.Rectangle);
            var borderWidth = (int)Math.Ceiling(element.BorderWidth);
            drawable.SetStroke(borderWidth > 0 ? borderWidth + 1 : borderWidth, element.TintBorderColor.ToAndroid());
            drawable.SetColor(Color.Transparent);
            d.Add(drawable);
            var layer = new LayerDrawable(d.ToArray());
            for (var i = 0; i < element.BackgroundPattern.Pattern.Count; i++)
            {
                var l = (int)Math.Ceiling(Control.Width * element.BackgroundPattern.GetLeft(i));
                var t = (int)Math.Ceiling(Control.Height * element.BackgroundPattern.GetTop(i));
                var r = (int)Math.Ceiling(Control.Width * (1 - element.BackgroundPattern.Pattern[i].WidthPercent)) - l;
                var b = (int)Math.Ceiling(Control.Height * (1 - element.BackgroundPattern.Pattern[i].HightPercent)) - t;
                layer.SetLayerInset(i, l, t, r, b);
            }
            layer.SetLayerInset(d.Count - 1, 0, 0, 0, 0);
            Control.SetBackground(layer);
        }

        private Task<Bitmap> getBitmap(FileImageSource image)
        {
            var handler = new FileImageSourceHandler();
            return handler.LoadImageAsync(image, this.Control.Context);
        }
    }

    public static class Calendar
    {
        public static void Init()
        {
        }
    }

    public class TextDrawable : ColorDrawable
    {
        private readonly Paint paint;
        public Pattern Pattern { get; set; }

        public TextDrawable(Color color)
            : base(color)
        {
            paint = new Paint { AntiAlias = true };
            paint.SetStyle(Paint.Style.Fill);
            paint.TextAlign = Paint.Align.Left;
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            paint.Color = Pattern.TextColor.ToAndroid();
            paint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, Pattern.TextSize > 0 ? Pattern.TextSize : 12, CrossCurrentActivity.Current.Activity.Resources.DisplayMetrics);
            var bounds = new Rect();
            paint.GetTextBounds(Pattern.Text, 0, Pattern.Text.Length, bounds);
            var al = (int)Pattern.TextAlign;
            var x = Bounds.Left;
            if ((al & 2) == 2) // center
            {
                x = Bounds.CenterX() - Math.Abs(bounds.CenterX());
            }
            else if ((al & 4) == 4) // right
            {
                x = Bounds.Right - bounds.Width();
            }
            var y = Bounds.Top + Math.Abs(bounds.Top);
            if ((al & 16) == 16) // middle
            {
                y = Bounds.CenterY() + Math.Abs(bounds.CenterY());
            }
            else if ((al & 32) == 32) // bottom
            {
                y = Bounds.Bottom - Math.Abs(bounds.Bottom);
            }
            canvas.DrawText(Pattern.Text.ToCharArray(), 0, Pattern.Text.Length, x, y, paint);
        }
    }
}
