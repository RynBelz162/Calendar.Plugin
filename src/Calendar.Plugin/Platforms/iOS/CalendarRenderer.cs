using System.Threading.Tasks;
using Calendar.Plugin.Platforms.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Calendar.Plugin.Shared.Calendar), typeof(CalendarRenderer))]
namespace Calendar.Plugin.Platforms.iOS
{
    public class CalendarRenderer : ViewRenderer
    {
        private bool _isSwipingFromLeftEdge;

        public CalendarRenderer()
        {
            _isSwipingFromLeftEdge = false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Element is Shared.Calendar calendar)
            {
                var panGesture = new UIPanGestureRecognizer((pan) => OnPan(pan, calendar));
                AddGestureRecognizer(panGesture);
            }

            base.OnElementChanged(e);
        }

        private void OnPan(UIPanGestureRecognizer pan, Shared.Calendar calendar)
        {
            if (pan.State == UIGestureRecognizerState.Began)
            {
                var screenWidth = UIScreen.MainScreen.Bounds.Width;
                var leftEdgeThreshold = screenWidth * 0.15f;

                var staringPoint = pan.LocationInView(null);
                if (staringPoint.X < leftEdgeThreshold)
                {
                    _isSwipingFromLeftEdge = true;
                }
            }

            if (pan.State != UIGestureRecognizerState.Ended)
            {
                return;
            }

            var velocity = pan.VelocityInView(null);

            // going right
            var isSwipingRight = velocity.X > 0;

            // going left
            var isSwipingLeft = velocity.X < 0;

            if (_isSwipingFromLeftEdge)
            {
                PullOutMainMenu();
                _isSwipingFromLeftEdge = false;
                return;
            }

            if (isSwipingRight && calendar != null)
            {
                SwipeFrame(calendar, true);
            }

            if (isSwipingLeft && calendar != null)
            {
                SwipeFrame(calendar, false);
            }
        }

        private static void PullOutMainMenu()
        {
            if (Xamarin.Forms.Application.Current.MainPage is MasterDetailPage masterDetailPage)
            {
                masterDetailPage.IsPresented = true;
            }
        }

        private static async void SwipeFrame(Shared.Calendar calendar, bool isSwipingRight)
        {
            if (IsViewAnimating(calendar))
            {
                return;
            }

            var command = isSwipingRight ? calendar.RightSwipeCommand : calendar.LeftSwipeCommand;
            if (command == null)
            {
                return;
            }

            if (calendar.IsSwipingAnimated)
            {
                await AnimateListViewStart(!isSwipingRight, calendar);
                command.Execute(null);
                await AnimateListViewEnd(calendar);
            }
            else
            {
                command.Execute(null);
            }
        }

        private static async Task AnimateListViewStart(bool forwards, VisualElement calendar)
        {
            var originalX = calendar.AnchorX;
            var offsetX = forwards ? 100 : -100;

            await Task.WhenAll(
                calendar.FadeTo(0, 250, Easing.Linear),
                calendar.TranslateTo(originalX - offsetX, calendar.AnchorY, 250, Easing.Linear));
        }

        private static async Task AnimateListViewEnd(VisualElement calendar)
        {
            var originalX = calendar.AnchorX;
            await calendar.TranslateTo(originalX, calendar.AnchorY, 250, Easing.Linear);
            await calendar.FadeTo(1, 500, Easing.Linear);
        }

        private static bool IsViewAnimating(IAnimatable calendar) =>
            calendar.AnimationIsRunning("TranslateTo") || calendar.AnimationIsRunning("FadeTo");
    }
}
