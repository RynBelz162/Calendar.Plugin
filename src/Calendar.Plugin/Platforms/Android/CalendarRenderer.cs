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
        private const double DistanceThreshold = 200;

        private float _x1;
        private float _x2;
        private float _y1;
        private float _y2;
        private bool _isSwipingRight;
        private bool _isSwipingLeft;

        public CalendarRenderer(Context context) : base(context)
        {
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            CalculateSwipe(e);

            if (!_isSwipingLeft && !_isSwipingRight)
            {
                return true;
            }

            return ExecuteSwipes();
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            CalculateSwipe(e);

            if (!_isSwipingLeft && !_isSwipingRight)
            {
                return base.DispatchTouchEvent(e);
            }

            return ExecuteSwipes();
        }

        private void CalculateSwipe(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _x1 = e.GetX();
                    _y1 = e.GetY();
                    break;
                case MotionEventActions.Up:
                    _x2 = e.GetX();
                    _y2 = e.GetY();

                    var deltaX = Math.Abs(_x2 - _x1);
                    var deltaY = Math.Abs(_y2 - _y1);
                    var angle = Math.Atan(deltaY / deltaX);

                    var distance = GetDistance(_x1, _y1, _x2, _y2);
                    if (distance > DistanceThreshold && angle < swipeAngleThreshold)
                    {
                        // swiping right
                        if (_x2 - _x1 > 0)
                        {
                            _isSwipingRight = true;
                        }

                        // swiping left
                        else
                        {
                            _isSwipingLeft = true;
                        }
                    }

                    break;
            }
        }

        private bool ExecuteSwipes()
        {
            if (!(Element is Shared.Calendar cal) || !cal.EnableSwiping)
            {
                return false;
            }

            if (_isSwipingLeft)
            {
                SwipeCalendar(cal, false);
            }

            if (_isSwipingRight)
            {
                SwipeCalendar(cal, true);
            }

            ClearValuesAfterSwipe();
            return true;
        }

        private static double GetDistance(float x1, float y1, float x2, float y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private void ClearValuesAfterSwipe()
        {
            _isSwipingLeft = _isSwipingRight = false;
            _x1 = _x2 = _y1 = _y2 = 0;
        }

        private static async void SwipeCalendar(Shared.Calendar cal, bool isSwipingRight)
        {
            if (IsViewAnimating(cal))
            {
                return;
            }

            var command = isSwipingRight ? cal.RightSwipeCommand : cal.LeftSwipeCommand;
            if (cal.IsSwipingAnimated)
            {
                await AnimateCalendarStart(!isSwipingRight, cal);
                command.Execute(null);
                await AnimateCalendarEnd(cal);
            }
            else
            {
                command.Execute(null);
            }
        }

        private static async Task AnimateCalendarStart(bool forwards, VisualElement calendar)
        {
            var originalX = calendar.AnchorX;
            var offsetX = forwards ? 100 : -100;

            await Task.WhenAll(
                calendar.FadeTo(0, 250, Easing.Linear),
                calendar.TranslateTo(originalX - offsetX, calendar.AnchorY, 250, Easing.Linear));
        }

        private static async Task AnimateCalendarEnd(VisualElement calendar)
        {
            var originalX = calendar.AnchorX;
            await calendar.TranslateTo(originalX, calendar.AnchorY, 250, Easing.Linear);
            await calendar.FadeTo(1, 500, Easing.Linear);
        }

        private static bool IsViewAnimating(IAnimatable calendar) => calendar.AnimationIsRunning("TranslateTo") || calendar.AnimationIsRunning("FadeTo");
    }
}
