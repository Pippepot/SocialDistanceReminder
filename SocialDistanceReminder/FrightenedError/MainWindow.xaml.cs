using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;

namespace CoronaCaution
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // How many milliseconds per update
        public double deltaTime = 0.016;
        public CautionStateMachine stateMachine;

        Image m_Icon;
        TextBlock m_Text;

        public MainWindow()
        {
            stateMachine = new CautionStateMachine();

            // Only initialize the state machine when the content is rendered, because the it has to reference the content
            ContentRendered += MainWindow_ContentRendered;
            InitializeComponent();
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            stateMachine.Initialize(this);
            
            // Create the update loop
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += delegate
            {
                stateMachine.Update();
            };

            timer.Interval = new TimeSpan(0, 0, 0, 0, (int)(deltaTime * 1000));
            timer.Start();
        }


        public void ChangePosition(double x, double y)
        {
            Vector pos = GetPosition();
            System.Drawing.Rectangle rect = Screen.GetBounds(new System.Drawing.Point((int)pos.X, (int)pos.Y));

            double left = x - Width * 0.5;
            double top = y - Height * 0.5;

            // The box should not be able to go outside of the screen
            if (left < rect.Left)
                left = rect.Left;
            else if (left > rect.Right - Width)
                left = rect.Right - Width;

            if (top < rect.Top)
                top = rect.Top;
            else if (top > rect.Bottom - Height)
                top = rect.Bottom - Height;

            Left = left;
            Top = top;
            
        }

        public void ChangePosition(Vector vector)
        {
            ChangePosition(vector.X, vector.Y);
        }

        public Vector GetPosition()
        {
            return new Vector(Left + Width * 0.5, Top + Height * 0.5);
        }

        public double GetDistance()
        {
            return GetDirection().Length;
        }

        public Vector GetDirection()
        {
            return new Vector(GetPosition().X - GetMousePosition().X, GetPosition().Y - GetMousePosition().Y);
        }

        public Point GetMousePosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        public void ChangeIcon(CautionState state)
        {
            Uri imageUri;

            switch (state)
            {
                case CautionState.Informational:
                    imageUri = new Uri("pack://application:,,,/WindowsInfo.png", UriKind.RelativeOrAbsolute);
                    break;
                case CautionState.Fleeing:
                    imageUri = new Uri("pack://application:,,,/WindowsWarning.png", UriKind.RelativeOrAbsolute);
                    break;
                case CautionState.Panicing:
                    imageUri = new Uri("pack://application:,,,/WindowsError.png", UriKind.RelativeOrAbsolute);
                    break;
                default:
                    imageUri = new Uri("pack://application:,,,/WindowsInfo.png", UriKind.RelativeOrAbsolute);
                    break;
            }

            PngBitmapDecoder decoder = new PngBitmapDecoder(imageUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];

            m_Icon.Source = bitmapSource;
        }

        public void ChangeText(string text)
        {
            m_Text.Text = text;
        }

        #region EventHandlers

        private void Image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image image = (Image)sender;

            Uri imageUri = new Uri("pack://application:,,,/OKMouseOver.png", UriKind.RelativeOrAbsolute);
            PngBitmapDecoder decoder = new PngBitmapDecoder(imageUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];

            image.Source = bitmapSource;
        }

        private void Image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image image = (Image)sender;

            Uri imageUri = new Uri("pack://application:,,,/OKDefault.png", UriKind.RelativeOrAbsolute);
            PngBitmapDecoder decoder = new PngBitmapDecoder(imageUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];

            image.Source = bitmapSource;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;

            Uri imageUri = new Uri("pack://application:,,,/OKMousePressed.png", UriKind.RelativeOrAbsolute);
            PngBitmapDecoder decoder = new PngBitmapDecoder(imageUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];

            image.Source = bitmapSource;
            System.Windows.Application.Current.Shutdown();
        }

        private void Text_Initialized(object sender, EventArgs e)
        {
            m_Text = (TextBlock)sender;
        }

        private void Icon_Initialized(object sender, EventArgs e)
        {
            m_Icon = (Image)sender;
        }

        #endregion

    }
}
