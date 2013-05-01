using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Storage;
using Windows.Storage;
using Windows.Phone.Storage;
using Windows.Phone.Storage.SharedAccess;
using CKReader.Resources;

namespace CKReader
{
    public partial class MainPage : PhoneApplicationPage
    {
        private string filetoken;
        private string filepath;
        private string contentString;
        private List<string> contentPages;
        private int currentPage;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                filetoken = NavigationContext.QueryString["fileToken"];
                Debug.WriteLine("filetoken=" + filetoken);
            }
            catch
            {
            }
            try
            {
                filepath = NavigationContext.QueryString["path"];
                Debug.WriteLine("filepath=" + filepath);
            }
            catch
            {
            }

            if (filetoken != null && filetoken.Length > 0)
            {
                // run from other apps (e.g. Mail)
                this.readfileToken(filetoken);
            }
            else if (filepath != null && filepath.Length > 0)
            {
                if (filepath.StartsWith("\\"))
                {
                    // file within sandbox
                    filepath = filepath.Substring(1);
                    this.readfileFromSandbox(filepath);
                }
                else
                {
                    // file on sd card
                    this.readfile(filepath);
                }
            }
            else
            {
                // sample book
                this.readfile();
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Application.Current.Host.Settings.EnableFrameRateCounter = false;

            // Init progress indicator
            ProgressIndicator progressIndicator = new ProgressIndicator();
            progressIndicator.IsVisible = false;
            progressIndicator.IsIndeterminate = true;
            progressIndicator.Text = "...";
            SystemTray.SetProgressIndicator(this, progressIndicator);
            
            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton1 = new ApplicationBarIconButton(new Uri("/Assets/Tiles/back.png", UriKind.Relative));
            appBarButton1.Text = "上一页";
            appBarButton1.Click += delegate(Object o, EventArgs e) { if (this.currentPage > 0) this.currentPage--; this.displayCurrentPage();};
            appBarButton1.IsEnabled = false;
            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Assets/Tiles/next.png", UriKind.Relative));
            appBarButton2.Text = "下一页";
            appBarButton2.Click += delegate(Object o, EventArgs e) { if (this.currentPage < this.contentPages.Count-1) this.currentPage++; this.displayCurrentPage(); };
            appBarButton2.IsEnabled = false;
            ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("/Assets/Tiles/transport.rew.png", UriKind.Relative));
            appBarButton3.Text = "首页";
            appBarButton3.Click += delegate(Object o, EventArgs e) { this.currentPage = 0; this.displayCurrentPage(); };
            appBarButton3.IsEnabled = false;
            ApplicationBarIconButton appBarButton4 = new ApplicationBarIconButton(new Uri("/Assets/Tiles/transport.ff.png", UriKind.Relative));
            appBarButton4.Text = "尾页";
            appBarButton4.Click += delegate(Object o, EventArgs e) { this.currentPage = this.contentPages.Count-1; this.displayCurrentPage(); };
            appBarButton4.IsEnabled = false;

            ApplicationBar.Buttons.Add(appBarButton3);
            ApplicationBar.Buttons.Add(appBarButton1);
            ApplicationBar.Buttons.Add(appBarButton2);
            ApplicationBar.Buttons.Add(appBarButton4);

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem1 = new ApplicationBarMenuItem("字号：蛋疼小");
            appBarMenuItem1.Click += delegate(Object o, EventArgs e) { this.content.FontSize = 21; this.displayCurrentPage(); };
            ApplicationBarMenuItem appBarMenuItem2 = new ApplicationBarMenuItem("字号：一般小");
            appBarMenuItem2.Click += delegate(Object o, EventArgs e) { this.content.FontSize = 25; this.displayCurrentPage(); };
            ApplicationBarMenuItem appBarMenuItem3 = new ApplicationBarMenuItem("字号：中等");
            appBarMenuItem3.Click += delegate(Object o, EventArgs e) { this.content.FontSize = 28; this.displayCurrentPage(); };
            ApplicationBarMenuItem appBarMenuItem4 = new ApplicationBarMenuItem("字号：一般大");
            appBarMenuItem4.Click += delegate(Object o, EventArgs e) { this.content.FontSize = 30; this.displayCurrentPage(); };
            ApplicationBarMenuItem appBarMenuItem5 = new ApplicationBarMenuItem("字号：蛋疼大");
            appBarMenuItem5.Click += delegate(Object o, EventArgs e) { this.content.FontSize = 34; this.displayCurrentPage(); };

            ApplicationBar.MenuItems.Add(appBarMenuItem1);
            ApplicationBar.MenuItems.Add(appBarMenuItem2);
            ApplicationBar.MenuItems.Add(appBarMenuItem3);
            ApplicationBar.MenuItems.Add(appBarMenuItem4);
            ApplicationBar.MenuItems.Add(appBarMenuItem5);

            ApplicationBarMenuItem appBarMenuItemx = new ApplicationBarMenuItem("未加载书籍");
            ApplicationBar.MenuItems.Add(appBarMenuItemx);
        }

        async public void readfile()
        {
                try
                {
                    // print its content
                    Uri linesUri = new Uri("Resources/sample.txtx", UriKind.Relative);
                    System.Windows.Resources.StreamResourceInfo stream = App.GetResourceStream(linesUri);
                    Stream x = stream.Stream;
                    byte[] buffer = new byte[x.Length];
                    await x.ReadAsync(buffer, 0, (int)x.Length);
                    x.Close();
                    string result = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    //StreamReader streamReader = new StreamReader(stream.Stream, System.Text.Encoding.UTF8);
                    //string result = streamReader.ReadToEnd();
                    //streamReader.Close();

                    //Debug.WriteLine(result);
                    //this.title.Text = "阅读器";
                    //Debug.WriteLine("title changed");
                    //this.content.Text = result.Substring(0, 10000);
                    //Debug.WriteLine("content changed");
                    this.contentString = result;

                    // cut content into pages
                    this.cutContentIntoPages();

                    // display first page
                    this.currentPage = 0;
                    this.displayCurrentPage();

                    Debug.WriteLine("done");
                }
                catch (Exception)
                {
                    // No Routes folder is present.
                    Debug.WriteLine("can't open sample text.");
                }
        }

        async public void readfile(string filepath)
        {
            // enables progress indicator
            //ProgressIndicator indicator = SystemTray.ProgressIndicator;
            //if (indicator != null)
            //{
            //    //indicator.Text = "载入文件中 ...";
            //    //indicator.IsVisible = true;
            //}

            // Connect to the current SD card.
            ExternalStorageDevice _sdCard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();

            // If the SD card is present, add GPX files to the Routes collection.
            if (_sdCard != null)
            {
                try
                {
                    // get file of the specific path
                    ExternalStorageFile esf = await _sdCard.GetFileAsync(filepath);

                    if (esf != null)
                    {
                        Debug.WriteLine("found file " + esf.Name);
                        if (esf.Path.EndsWith(".txtx"))
                        {
                            // print its content
                            Stream x = await esf.OpenForReadAsync();
                            byte[] buffer = new byte[x.Length];
                            x.Read(buffer, 0, (int)x.Length);
                            x.Close();

                            string result = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                            //Debug.WriteLine(result);
                            //this.title.Text = "阅读器";
                            //Debug.WriteLine("title changed");
                            //this.content.Text = result.Substring(0, 10000);
                            //Debug.WriteLine("content changed");
                            this.contentString = result;

                            // cut content into pages
                            this.cutContentIntoPages();

                            // display first page
                            this.currentPage = 0;
                            this.displayCurrentPage();
                        }
                    }
                    Debug.WriteLine("done");
                }
                catch (FileNotFoundException)
                {
                    // No Routes folder is present.
                    this.content.Text = "Error loading file, reason: file not found";
                    Debug.WriteLine("file not found.");
                }
            }
            else
            {
                // No SD card is present.
                Debug.WriteLine("The SD card is mssing.");
            }
        }

        async public void readfileToken(string fileToken)
        {
            // enables progress indicator
            //ProgressIndicator indicator = SystemTray.ProgressIndicator;
            //if (indicator != null)
            //{
            //    //indicator.Text = "载入文件中 ...";
            //    //indicator.IsVisible = true;
            //}

            try
            {
                // get file of the specific token
                // Create or open the routes folder.
                IStorageFolder routesFolder = ApplicationData.Current.LocalFolder;

                // Get the full file name of the route (.GPX file) from the file association.
                string incomingRouteFilename = SharedStorageAccessManager.GetSharedFileName(fileToken);

                //// purge all files from the Routes folder.
                //Debug.WriteLine("deleting all files within folder");
                //IEnumerable<StorageFile> files = await routesFolder.GetFilesAsync();

                //// Add each GPX file to the Routes collection.
                //foreach (StorageFile f in files)
                //{
                //    await f.DeleteAsync();
                //}

                // Copy the route (.GPX file) to the Routes folder.
                IStorageFile esf = await SharedStorageAccessManager.CopySharedFileAsync((StorageFolder)routesFolder, incomingRouteFilename, NameCollisionOption.ReplaceExisting, fileToken);

                if (esf != null)
                {
                    Debug.WriteLine("found file " + esf.Name);
                    if (esf.Path.EndsWith(".txtx"))
                    {
                        // print its content
                        var fileStream = await esf.OpenReadAsync();
                        Stream x = fileStream.AsStream();
                        byte[] buffer = new byte[x.Length];
                        x.Read(buffer, 0, (int)x.Length);
                        x.Close();

                        string result = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        //Debug.WriteLine(result);
                        //this.title.Text = "阅读器";
                        //Debug.WriteLine("title changed");
                        //this.content.Text = result.Substring(0, 10000);
                        //Debug.WriteLine("content changed");
                        this.contentString = result;

                        // cut content into pages
                        this.cutContentIntoPages();

                        // display first page
                        this.currentPage = 0;
                        this.displayCurrentPage();
                    }
                }
                Debug.WriteLine("done");
            }
            catch (FileNotFoundException)
            {
                // No Routes folder is present.
                this.content.Text = "Error loading file, reason: file not found";
                Debug.WriteLine("file not found.");
            }
        }

        async public void readfileFromSandbox(string fileToken)
        {
            // enables progress indicator
            //ProgressIndicator indicator = SystemTray.ProgressIndicator;
            //if (indicator != null)
            //{
            //    //indicator.Text = "载入文件中 ...";
            //    //indicator.IsVisible = true;
            //}

            try
            {
                // get file of the specific token
                // Create or open the routes folder.
                IStorageFolder routesFolder = ApplicationData.Current.LocalFolder;

                //// purge all files from the Routes folder.
                //Debug.WriteLine("deleting all files within folder");
                //IEnumerable<StorageFile> files = await routesFolder.GetFilesAsync();

                //// Add each GPX file to the Routes collection.
                //foreach (StorageFile f in files)
                //{
                //    await f.DeleteAsync();
                //}

                // Copy the route (.GPX file) to the Routes folder.
                IStorageFile esf = await routesFolder.GetFileAsync(filepath);

                if (esf != null)
                {
                    Debug.WriteLine("found file " + esf.Name);
                    if (esf.Path.EndsWith(".txtx"))
                    {
                        // print its content
                        var fileStream = await esf.OpenReadAsync();
                        Stream x = fileStream.AsStream();
                        byte[] buffer = new byte[x.Length];
                        x.Read(buffer, 0, (int)x.Length);
                        x.Close();

                        string result = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        //Debug.WriteLine(result);
                        //this.title.Text = "阅读器";
                        //Debug.WriteLine("title changed");
                        //this.content.Text = result.Substring(0, 10000);
                        //Debug.WriteLine("content changed");
                        this.contentString = result;

                        // cut content into pages
                        this.cutContentIntoPages();

                        // display first page
                        this.currentPage = 0;
                        this.displayCurrentPage();
                    }
                }
                Debug.WriteLine("done");
            }
            catch (FileNotFoundException)
            {
                // No Routes folder is present.
                this.content.Text = "Error loading file, reason: file not found";
                Debug.WriteLine("file not found.");
            }
        }

        private void cutContentIntoPages()
        {
            // detect chapter boundaries so we can cut them into pages
            int lastLocation = 0;
            int currentPosition = 0;
            int pageCount = 0;
            this.contentPages = new List<string>();
            
            // prevent crash
            int firstLocation = this.contentString.IndexOf("（", currentPosition);
            if (firstLocation < 0 && this.contentString.Length > 10000)
            {
                this.content.Text = "Error loading file, too large without chapters";
                return;
            }

            while (currentPosition < this.contentString.Length)
            {
                //Debug.WriteLine(currentPosition);
                int foundLocation = this.contentString.IndexOf("（", currentPosition);
                int number;
                if (foundLocation < 0)
                {
                    // not found, save current page
                    string currentPageString = this.contentString.Substring(lastLocation);
                    this.contentPages.Add(this.parse(currentPageString));
                    Debug.WriteLine("got last page");
                    break;
                }
                else
                {
                    // test if found a real chapter
                    int nextLocation = this.contentString.IndexOf("）", foundLocation);
                    string between = "";
                    if (nextLocation - foundLocation > 1)
                    {
                        between = this.contentString.Substring(foundLocation+1, nextLocation - foundLocation - 1);
                    }
                    if (Int32.TryParse(between, out number) && number > pageCount)
                    {
                        // chapter number found, begin to cut page
                        string currentPageString = this.contentString.Substring(lastLocation, foundLocation - lastLocation);
                        this.contentPages.Add(this.parse(currentPageString));
                        Debug.WriteLine("got new page at " + foundLocation);

                        // change values so we can search again
                        lastLocation = foundLocation;
                        pageCount++;
                    }
                    else
                    {
                        //Debug.WriteLine("what? " + this.contentString.Substring(foundLocation + 1, 1) + " is not a number");
                    }

                    currentPosition = foundLocation + 1;
                }
            }
            Debug.WriteLine("content string scanned");

            //ProgressIndicator indicator = SystemTray.ProgressIndicator;
            //// disables progress indicator on complete
            //if (indicator != null && indicator.IsVisible)
            //{
            //    indicator.IsVisible = false;
            //}
        }

        private void displayCurrentPage()
        {
            // enables progress indicator
            //ProgressIndicator indicator = SystemTray.ProgressIndicator;
            //if (indicator != null && !indicator.IsVisible)
            //{
            //    indicator.Text = "换页中 ...";
            //    indicator.IsVisible = true;
            //}

            if (this.contentPages.Count <= 0) return;

            this.title.Text = "CKReader - 第" + (this.currentPage + 1) + "章";
            Debug.WriteLine("title changed");
            this.content.Text = "";
            this.content.Text = this.contentPages[this.currentPage];
            //this.content.Text = this.contentString.Substring(0, 10000);
            Debug.WriteLine("content changed");
            //Debug.WriteLine(this.content.Text);

            try
            {
                if (content != null && content.ScrollViewer != null)
                {
                    content.ScrollViewer.ScrollToVerticalOffset(0.0);
                }
            }
            catch
            {
            }

            // disables progress indicator on complete
            //if (indicator != null)
            //{
            //    indicator.IsVisible = false;
            //}

            // updates app bar items if more than one page exists
            if (this.contentPages.Count > 0)
            {
                ApplicationBarMenuItem menuItem = (ApplicationBarMenuItem)ApplicationBar.MenuItems[5];
                menuItem.Text = "本书共" + this.contentPages.Count + "章";

                if (this.currentPage > 0)
                {
                    ApplicationBarIconButton firstButton = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                    firstButton.IsEnabled = true;
                    ApplicationBarIconButton anotherButton = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                    anotherButton.IsEnabled = true;
                }
                else
                {
                    ApplicationBarIconButton firstButton = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                    firstButton.IsEnabled = false;
                    ApplicationBarIconButton anotherButton = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
                    anotherButton.IsEnabled = false;
                }

                if (this.currentPage < this.contentPages.Count - 1)
                {
                    ApplicationBarIconButton firstButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                    firstButton.IsEnabled = true;
                    ApplicationBarIconButton anotherButton = (ApplicationBarIconButton)ApplicationBar.Buttons[3];
                    anotherButton.IsEnabled = true;
                }
                else
                {
                    ApplicationBarIconButton firstButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                    firstButton.IsEnabled = false;
                    ApplicationBarIconButton anotherButton = (ApplicationBarIconButton)ApplicationBar.Buttons[3];
                    anotherButton.IsEnabled = false;
                }
            }
        }

        private string parse(string source)
        {
            string result = source.Replace("\r\n\r\n　　", "\r\n　　").Replace("\r\n　　", "\r\n\r\n　　");
            return result;
        }
    }
}