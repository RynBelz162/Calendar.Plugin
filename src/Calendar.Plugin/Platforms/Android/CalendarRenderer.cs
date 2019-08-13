using Android.Content;
using Android.Views;
using System;
using System.Threading.Tasks;
using Calendar.Plugin.Platforms.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Calendar.Plugin.Shared.Calendar), typeof(CalendarRenderer))]
namespace Calendar.Plugin.Platforms.Android
{
    public class CalendarRenderer : ViewRenderer
    {
        // 20 degree swipe angle threshold or default to scroll funtionality
        private const float swipeAngleThreshold = 0.20f;
        private const double distanceThreshold = 200;

        private float x1;
        private float x2;
        private float y1;
        private float y2;
        private bool isSwipingRight;
        private bool isSwipingLeft;

        public CalendarRenderer(Context context) : base(context)
        {
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            calculateSwipe(e);

            if (!isSwipingLeft && !isSwipingRight)
            {
                return true;
            }

            return executeSwipes();
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            calculateSwipe(e);

            if (!isSwipingLeft && !isSwipingRight)
            {
                return base.DispatchTouchEvent(e);
            }

            return executeSwipes();
        }

        private void calculateSwipe(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    x1 = e.GetX();
                    y1 = e.GetY();
                    break;
                case MotionEventActions.Up:
                    x2 = e.GetX();
                    y2 = e.GetY();

                    var deltaX = Math.Abs(x2 - x1);
                    var deltaY = Math.Abs(y2 - y1);
                    var angle = Math.Atan(deltaY / deltaX);

                    var distance = getDistance(x1, y1, x2, y2);
                    if (distance > distanceThreshold && angle < swipeAngleThreshold)
                    {
                        // swiping right
                        if (x2 - x1 > 0)
                        {
                            isSwipingRight = true;
                        }

                        // swiping left
                        else
                        {
                            isSwipingLeft = true;
                        }
                    }

                    break;
            }
        }

        private bool executeSwipes()
        {
            if (!(Element is Shared.Calendar cal) || !cal.EnableSwiping)
            {
                return false;
            }

            if (isSwipingLeft)
            {
                swipeCalendar(cal, false);
            }

            if (isSwipingRight)
            {
                swipeCalendar(cal, true);
            }

            clearValuesAfterSwipe();
            return true;
        }

        private static double getDistance(float x1, float y1, float x2, float y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private void clearValuesAfterSwipe()
        {
            isSwipingLeft = isSwipingRight = false;
            x1 = x2 = y1 = y2 = 0;
        }

        private static async void swipeCalendar(Shared.Calendar cal, bool isSwipingRight)
        {
            if (isViewAnimating(cal))
            {
                return;
            }

            var command = isSwipingRight ? cal.RightSwipeCommand : cal.LeftSwipeCommand;
            if (cal.IsSwipingAnimated)
            {
                await animateCalendarStart(!isSwipingRight, cal);
                cal.ArrowExecutionSetup(!isSwipingRight);
                command?.Execute(null);
                await animateCalendarEnd(cal);
            }
            else
            {
                cal.ArrowExecutionSetup(!isSwipingRight);
                command?.Execute(null);
            }
        }

        private static async Task animateCalendarStart(bool forwards, VisualElement calendar)
        {
            var originalX = calendar.AnchorX;
            var offsetX = forwards ? 100 : -100;

            await Task.WhenAll(
                calendar.FadeTo(0, 250, Easing.Linear),
                calendar.TranslateTo(originalX - offsetX, calendar.AnchorY, 250, Easing.Linear));
        }

        private static async Task animateCalendarEnd(VisualElement calendar)
        {
            var originalX = calendar.AnchorX;
            await calendar.TranslateTo(originalX, calendar.AnchorY, 250, Easing.Linear);
            await calendar.FadeTo(1, 500, Easing.Linear);
        }

        private static bool isViewAnimating(IAnimatable calendar)
            => calendar.AnimationIsRunning("TranslateTo") || calendar.AnimationIsRunning("FadeTo");
    }
}
