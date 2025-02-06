
using System;
using System.ComponentModel;
using Microsoft.Maui.Handlers;
#if ANDROID
using Android.Bluetooth;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;
using Paint = Android.Graphics.Paint;
using Path = Android.Graphics.Path;
using RectF = Android.Graphics.RectF;
#elif IOS
using CoreGraphics;
using UIKit;
#endif

namespace PickerCustomThumb
{
    public partial class ExtendedSlider : Slider
    {
        public Guid Id { get; } = Guid.NewGuid();

        public static readonly BindableProperty ThumbIndicatorProperty = 
            BindableProperty.Create(nameof(ThumbIndicator), typeof(bool), 
                typeof(ExtendedSlider), false, 
                defaultBindingMode: BindingMode.TwoWay);
        public bool ThumbIndicator
        {
            get => (bool)GetValue(ThumbIndicatorProperty);
            set => SetValue(ThumbIndicatorProperty, value);
        }
        
        public static readonly BindableProperty NameStringProperty = 
            BindableProperty.Create(nameof(ThumbIndicator), typeof(string), 
                typeof(ExtendedSlider), null, 
                defaultBindingMode: BindingMode.TwoWay);
        public string NameString
        {
            get => (string)GetValue(NameStringProperty);
            set => SetValue(NameStringProperty, value);
        }


        public ExtendedSlider()
        {
            ValueChanged += OnSliderValueChanged;
            Microsoft.Maui.Handlers.SliderHandler.Mapper.AppendToMapping("ExtendedSlider", (handler, view) =>
            {
                HandlerInit(handler, view);
            }); //);
        }
        
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            var newStep = (int)Math.Round(e.NewValue / 1.0f);

            Value = newStep;
            if (ThumbIndicator && Handler != null)
            {
                CreateLabelIndicator();
            }
        }

        private void HandlerInit(ISliderHandler handler, ISlider view)
        {
#if ANDROID
            HandlerInitAndroid(handler, view);
#elif IOS
            HandlerInitiOS(handler, view);
#endif
        }
        
        private void CreateLabelIndicator(ISliderHandler handler = null)
        {
#if ANDROID
            CreateLabelIndicatorAndroid(handler);
#elif IOS
            CreateLabelIndicatoriOS(handler);
#endif
        }

#region iOS_Section
 
#if IOS     
        private int _widthInt = 32;
        private int _heightInt = 32;
        private nfloat _width = 32;
        private nfloat _height = 32;
        
        private void HandlerInitiOS(ISliderHandler handler, ISlider view)
        {
            if (view is ExtendedSlider) //here we check and customize only MyEntry instances and not all entries, without this we can easily customize all entries if we needed it
            {
                var sliderView = view as ExtendedSlider;
                if (sliderView != null && !sliderView.Id.ToString().Equals(this.Id.ToString()))
                {
                    return;
                }
                var sliderPlatformView = handler.PlatformView;
                sliderPlatformView.ThumbTintColor = UIColor.FromRGBA(0, 0, 0, 0);
                    
                if (sliderView?.ThumbIndicator == true)
                {
                    CreateLabelIndicator(handler);
                    SetColorSlideriOS(handler, UIColor.FromRGBA(0, 0, 0, 0), UIColor.FromRGBA(0,0,0,0));
                }
                else
                {
                    SetColorSlideriOS(handler, UIColor.Red, UIColor.Blue);
                }
            }
        }
        
        public UIImage DrawText(UIImage uiImage, string sText, UIColor textColor, int iFontSize)
        {
            nfloat fWidth = _width;
            nfloat fHeight = _height;

            CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
            
            using (var ctx = new CGBitmapContext(IntPtr.Zero, (nint)fWidth, (nint)fHeight, 8, 4 * (nint)fWidth, CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedFirst))
            {
                ctx.DrawImage(new CGRect(0, 0, (double)fWidth, (double)fHeight), uiImage.CGImage);
                ctx.SelectFont("Verdana-Bold", iFontSize, CGTextEncoding.MacRoman);

                float start, end, textWidth;

                start = (float)ctx.TextPosition.X;
                ctx.SetTextDrawingMode(CGTextDrawingMode.Invisible);
                ctx.ShowText(sText);
                
                //Get the end position
                end = (float)ctx.TextPosition.X;
                textWidth = end - start;

                nfloat fRed;
                nfloat fGreen;
                nfloat fBlue;
                nfloat fAlpha;
                textColor.GetRGBA(out fRed, out fGreen, out fBlue, out fAlpha);
                ctx.SetFillColor(fRed, fGreen, fBlue, fAlpha);
                ctx.SetTextDrawingMode(CGTextDrawingMode.Fill);
                ctx.ShowTextAtPoint((nfloat)1.3 * fWidth / 4, (nfloat)0.85*fHeight/2, sText);
                var etes = ctx.ToImage();
                return UIImage.FromImage(etes);
            }
        }

        private UIImage DrawFigureIndicator(UIColor thumbColor)
        {
            nfloat fWidth = _width;
            nfloat fHeight = _height;

            var colorSpace = CGColorSpace.CreateDeviceRGB();

            using (var ctx = new CGBitmapContext(IntPtr.Zero, (nint)fWidth, (nint)fHeight, 8, 4 * (nint)fWidth, CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedFirst))
            {
                ctx.SetTextDrawingMode(CGTextDrawingMode.Fill);

                var trianglePoints = new PointF[] {
                    new PointF (_widthInt/2, 0),
                    new PointF (0, _heightInt/2),
                    new PointF (_widthInt, _heightInt/2),
                    new PointF (_widthInt/2, 0)
                };
                trianglePoints = trianglePoints.Select(pt => new PointF(pt.X, pt.Y += 0)).ToArray();
                ctx.SetFillColor(thumbColor.CGColor);
                ctx.MoveTo(trianglePoints[0].X, trianglePoints[0].Y);
                for (var i = 1; i < trianglePoints.Length; i++)
                {
                    ctx.AddLineToPoint(trianglePoints[i].X, trianglePoints[i].Y);
                }
                ctx.DrawPath(CGPathDrawingMode.Fill);
                var rect = new CGRect(0, 0.9 * fHeight / 4, fWidth, 3 * fHeight / 4);
                ctx.FillEllipseInRect(rect);
                return UIImage.FromImage(ctx.ToImage());
            }
        }
        
        private void CreateLabelIndicatoriOS(ISliderHandler handler)
        {
            var view = this;
            var platformView = Handler?.PlatformView as UISlider ?? handler?.PlatformView as UISlider;
            var figureIndicator = DrawFigureIndicator(UIColor.Blue/*view.ThumbColor.ToUIColor()*/ );
            var textIndicator = DrawText(figureIndicator, view.Value.ToString(), UIColor.White, 15);
            
            platformView.SetThumbImage(textIndicator, UIControlState.Normal);
            platformView.SetThumbImage(textIndicator, UIControlState.Highlighted);
        }

        private void SetColorSlideriOS(ISliderHandler handler, UIColor MinColor, UIColor MaxColor)
        {
            var platformView = Handler?.PlatformView as UISlider ?? handler?.PlatformView as UISlider;
            platformView.MinimumTrackTintColor = MinColor; 
            platformView.MaximumTrackTintColor = MaxColor;
        }
#endif
        
#endregion

#region Android_Section

#if ANDROID
        private double _width = 230*0.75;
        private double _height = 352*0.75;

        private void HandlerInitAndroid(ISliderHandler handler, ISlider view)
        {
            SeekBarThumbInit(handler);
            var seekBarView = this;
            
            if (seekBarView.ThumbIndicator)
            {
                CreateLabelIndicator(handler);
                setColorSlider(handler, Android.Graphics.Color.Transparent,Android.Graphics.Color.Transparent);
            }
            else
            {
                CreateThumbIndicator(handler);
                //setColorSlider(view.MinColor.ToAndroid(), view.MaxColor.ToAndroid());
                setColorSlider(handler, Android.Graphics.Color.Red, Android.Graphics.Color.Blue);
            }
        }

        private void SeekBarThumbInit(ISliderHandler handler)
        {
            //base.OnLayout(changed, l, t, r, b);
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBean)
            {
                SeekBar ctrl = Handler?.PlatformView as SeekBar ?? handler.PlatformView as SeekBar;
                Drawable thumb = ctrl.Thumb;
                int thumbTop = ctrl.Height / 2 - thumb.IntrinsicHeight / 2;
                thumb.SetBounds(thumb.Bounds.Left, thumbTop,
                                thumb.Bounds.Left + thumb.IntrinsicWidth, thumbTop + thumb.IntrinsicHeight);
            }
        }

        private void CreateLabelIndicatorAndroid(ISliderHandler handler)
        {
            var view = this;
            var platformView = Handler?.PlatformView as SeekBar ?? handler.PlatformView;
            var imageCustom = textAsBitmap(view.Value.ToString(), (float)FontSizeToPixels(45), Android.Graphics.Color.Blue/* view.ThumbColor.ToAndroid()*/);
            var imageDrawable = new BitmapDrawable(imageCustom);

            platformView.SetThumb(imageDrawable);
            imageCustom.Dispose();
            imageDrawable.Dispose();
        }

        private void CreateThumbIndicator(ISliderHandler handler)
        {
            var view = this;
            var platformView = Handler?.PlatformView as SeekBar?? handler.PlatformView;
            var imageCustom = thumbAsBitmap(Android.Graphics.Color.Blue/* view.ThumbColor.ToAndroid()*/);
            var imageDrawable = new BitmapDrawable(imageCustom);

            platformView.SetThumb(imageDrawable);
            imageCustom.Dispose();
            imageDrawable.Dispose();
        }

        public Bitmap textAsBitmap(String text, float textSize, Android.Graphics.Color textColor)
        {
            GC.Collect();
            var textPaint = new Paint(PaintFlags.AntiAlias);
            textPaint.TextSize = textSize;
            textPaint.Color = Android.Graphics.Color.White;
            textPaint.TextAlign = Paint.Align.Center;
            textPaint.AntiAlias = true;
            textPaint.FilterBitmap = true;
            textPaint.Dither = true;
            float baseline = -textPaint.Ascent();
            int width = (int)GetPixels(64);//_width);
            int height = (int)GetPixels(64); //_height/DisplayMetrics.DensityDeviceStable;
            var image = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N) {
                image.Density = DisplayMetrics.DensityDeviceStable;
            }
            else
            {
                image.Density = 160;
            }
            
            var canvas = new Canvas(image);
            
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N) {
                canvas.Density = DisplayMetrics.DensityDeviceStable;
            }
            else
            {
                canvas.Density = 160;
            }
            
            var circlePosition = new RectF(0, 0, width, (float)3.1*height/4);
            var circlePaint = new Paint(PaintFlags.AntiAlias);
            circlePaint.Color = textColor;

            var trianglePaint = new Paint(PaintFlags.AntiAlias);
            trianglePaint.Color = textColor;
            trianglePaint.SetStyle(Paint.Style.Fill);

            var trianglePath = new Path();

            trianglePath.SetFillType(Path.FillType.EvenOdd);
            trianglePath.MoveTo(0, height / 2);
            trianglePath.LineTo(width, height / 2);
            trianglePath.LineTo(width / 2, height);
            trianglePath.LineTo(0, height / 2);
            trianglePath.Close();

            canvas.DrawColor(Android.Graphics.Color.Transparent, PorterDuff.Mode.Overlay); 
            canvas.DrawPath(trianglePath,trianglePaint);
            canvas.DrawOval(circlePosition, circlePaint);
            canvas.DrawText(text, (float)1*width/2, baseline+10, textPaint);
            return image;
        }

        public Bitmap thumbAsBitmap(Android.Graphics.Color textColor)
        {
            GC.Collect();
            int width = (int)(_width/4);
            int height = 23;
            var image = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            var canvas = new Canvas(image);

            canvas.DrawColor(textColor);
            return image;
        }

        private void setColorSlider(ISliderHandler handler, Android.Graphics.Color MinColor, Android.Graphics.Color MaxColor)
        {
            var platformView = Handler?.PlatformView as SeekBar ?? handler.PlatformView;
            platformView.ProgressTintList = Android.Content.Res.ColorStateList.ValueOf(MinColor);
            platformView.ProgressTintMode = PorterDuff.Mode.SrcIn;

            platformView.ProgressBackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(MaxColor);
            platformView.ProgressBackgroundTintMode = PorterDuff.Mode.SrcIn;
        }
        
        public double GetPixels(double dp){
            //Resources r = boardContext.getResources();
            //float px = (int)TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, dpis, r.getDisplayMetrics());

            var scale = DeviceDisplay.MainDisplayInfo.Density;//DisplayMetrics.DensityDeviceStable;//  this.boardContext.getResources().getDisplayMetrics().density;
            var px = (dp * scale + 0.5f);

            return px;
        }

        public double FontSizeToPixels(double fontSize)
        {
            var density = DeviceDisplay.MainDisplayInfo.Density;
            var px = fontSize * density;
            Label label = new Label();
            label.FontSize = 12;
            //px = (.c)label.Handler.PlatformView
            return px;
        }
#endif

#endregion

    }
}