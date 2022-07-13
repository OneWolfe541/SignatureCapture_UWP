using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using System.Text.RegularExpressions;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SignatureCapture
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string _barCode = "";

        public MainPage()
        {
            this.InitializeComponent();

            HideTitleBar();
            ApplicationView.PreferredLaunchViewSize = new Size(1200, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(1200, 800));

            // Get how big the window can be in epx.
            //var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            //ApplicationView.PreferredLaunchViewSize = new Size(bounds.Width, bounds.Height);
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;


            this.MyCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                                                      Windows.UI.Core.CoreInputDeviceTypes.Pen |
                                                      Windows.UI.Core.CoreInputDeviceTypes.Touch;
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            drawingAttributes.Color = Windows.UI.Colors.Black;
            drawingAttributes.IgnorePressure = false;
            drawingAttributes.FitToCurve = true;
            MyCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);

            //Microsoft.UI.Xaml.Media.AcrylicBrush myBrush = new Microsoft.UI.Xaml.Media.AcrylicBrush();
            //myBrush.BackgroundSource = Microsoft.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop;
            //myBrush.TintColor = Color.FromArgb(255, 202, 24, 37);
            //myBrush.FallbackColor = Color.FromArgb(255, 202, 24, 37);
            //myBrush.TintOpacity = 0.6;

            //SignatureGrid.Background = myBrush;
        }

        //private void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        //{
        //    var deferral = e.GetDeferral();
        //    e.Handled = true;
        //    deferral.Complete();
        //}

        private void HideTitleBar()
        {
            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            ApplicationViewTitleBar titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.LightGray;
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.LightGray;

            //var sysNavMgr = SystemNavigationManagerPreview.GetForCurrentView();
            //sysNavMgr.CloseRequested += OnCloseRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MyCanvas.Visibility = Visibility.Visible;

            //VoterName.Text = e.Parameter.ToString();

            //int colonLocation = ((App)Application.Current).arguments.IndexOf(":", 0);

            string arguments = ((App)Application.Current).arguments.Substring(((App)Application.Current).arguments.LastIndexOf(":") + 1);

            if (arguments != null && arguments != "")
            {
                var parameterList = arguments.Split(',');

                _barCode = parameterList[0];

                string voterName = parameterList[1] + " " + parameterList[2] + " " + parameterList[3];

                VoterName.Text = voterName.Replace("  ", " ").ToUpper();

                BirthYear.Text = parameterList[4];

                voterName = Regex.Replace(voterName, @"(^\w)|(\s\w)", m => m.Value.ToUpper());

                Oath.Text = "I, " + voterName + ", confirm that I am a Registered Voter and to my knowledge have not cast a ballot in this election.";
            }
            else
            {
                //Oath.Text = "I confirm that I am a Registered Voter and to my knowledge have not cast \r\na ballot in this election.";
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // grab output file
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            //StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync("C:\\VoterX\\Signatures\\");
            StorageFolder storageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("VoterX\\Signatures", CreationCollisionOption.OpenIfExists);
            //StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(@"\\GARYC-WIN7\Temp\SigTest");

            //var testpath = storageFolder.Path; 
            //VoterName.Text = testpath;

            //VoterName.Text = storageFolder.Path.ToString();

            string barCode = _barCode;

            var file = await storageFolder.CreateFileAsync(barCode + ".jpg", CreationCollisionOption.ReplaceExisting);

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)MyCanvas.ActualWidth, (int)MyCanvas.ActualHeight, 96);

            // grab your input file from Assets folder            
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");

            var inputFile = await assets.GetFileAsync("00000.jpg");

            //try
            //{
            //    await inputFile.CopyAsync(storageFolder);
            //}
            //catch { }

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                var image = await CanvasBitmap.LoadAsync(device, inputFile.Path);
                //draw your image first
                ds.DrawImage(image);
                //then draw contents of your ink canvas over it
                ds.DrawInk(MyCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }

            // save results
            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
            }

            //var test = FindParent<Frame>(this);

            //test.Navigate(typeof(SignatureVerify), barCode.ToString());

            Application.Current.Exit();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //CoreApplication.Exit();
            Application.Current.Exit();
        }
    }
}
