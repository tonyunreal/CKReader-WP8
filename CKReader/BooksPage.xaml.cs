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
    public class Book
    {
        private bool isSample = false;
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsSample { get; set; }
    }

    public partial class BooksPage : PhoneApplicationPage
    {
        private List<Book> files;

        public BooksPage()
        {
            InitializeComponent();

            Application.Current.Host.Settings.EnableFrameRateCounter = false;

            // Init progress indicator
            ProgressIndicator progressIndicator = new ProgressIndicator();
            progressIndicator.IsVisible = false;
            progressIndicator.IsIndeterminate = true;
            progressIndicator.Text = "...";
            SystemTray.SetProgressIndicator(this, progressIndicator);

            this.buildFileList();
        }

        private void JumpToNextPage(object sender, EventArgs args)
        {
            // get current book
            Book currentBook = (Book)this.fileList.SelectedItem;
            if (currentBook != null)
            {
                string path = currentBook.IsSample ? "" : currentBook.Path;
                this.NavigationService.Navigate(new Uri("/MainPage.xaml?path=" + path, UriKind.Relative));

                // clear selection
                this.fileList.SelectedItem = null;
            }
        }

        private async void buildFileList()
        {
            // enables progress indicator
            //ProgressIndicator indicator = SystemTray.ProgressIndicator;
            //if (indicator != null)
            //{
            //    //indicator.Text = "载入文件中 ...";
            //    //indicator.IsVisible = true;
            //}

            this.files = new List<Book>();

            Book sampleBook = new Book();
            sampleBook.Name = "《三国演义》节选";
            sampleBook.Path = "试阅文本";
            sampleBook.IsSample = true;
            this.files.Add(sampleBook);

            // sandbox
            try
            {
                // sandbox root folder
                IStorageFolder routesFolder = ApplicationData.Current.LocalFolder;

                // Get all files from the Routes folder.
                IEnumerable<StorageFile> files = await routesFolder.GetFilesAsync();

                Debug.WriteLine("found folder");
                // Add each GPX file to the Routes collection.
                foreach (StorageFile esf in files)
                {
                    Debug.WriteLine("found file " + esf.Name);
                    if (esf.Path.EndsWith(".txtx"))
                    {
                        int loc = esf.Name.LastIndexOf(".txtx");
                        string bookname = esf.Name.Substring(0, loc);

                        // remove first part of book names
                        loc = bookname.LastIndexOf("-");
                        if (loc + 1 < bookname.Length)
                        {
                            bookname = bookname.Substring(loc + 1).Trim();
                        }

                        // add to file list
                        Book newBook = new Book();
                        newBook.Name = bookname;
                        newBook.Path = "\\" + esf.Path.Split(new String[] {"\\"}, StringSplitOptions.RemoveEmptyEntries).Last();
                        this.files.Add(newBook);
                    }
                }
                Debug.WriteLine("done");
            }
            catch (FileNotFoundException)
            {
                // No Routes folder is present.
                Debug.WriteLine("Folder not found.");
            }

            // Connect to the current SD card.
            ExternalStorageDevice _sdCard = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();

            // If the SD card is present, add GPX files to the Routes collection.
            if (_sdCard != null)
            {
                // root folder
                try
                {
                    // Look for a folder on the SD card
                    ExternalStorageFolder folder = _sdCard.RootFolder;

                    // Get all files from the Routes folder.
                    IEnumerable<ExternalStorageFile> files = await folder.GetFilesAsync();

                    Debug.WriteLine("found folder");
                    // Add each GPX file to the Routes collection.
                    foreach (ExternalStorageFile esf in files)
                    {
                        Debug.WriteLine("found file " + esf.Name);
                        if (esf.Path.EndsWith(".txtx"))
                        {
                            int loc = esf.Name.LastIndexOf(".txtx");
                            string bookname = esf.Name.Substring(0, loc);

                            // remove first part of book names
                            loc = bookname.LastIndexOf("-");
                            if (loc+1 < bookname.Length)
                            {
                                bookname = bookname.Substring(loc+1).Trim();
                            }

                            // add to file list
                            Book newBook = new Book();
                            newBook.Name = bookname;
                            newBook.Path = esf.Path;
                            this.files.Add(newBook);
                        }
                    }
                    Debug.WriteLine("done");
                }
                catch (FileNotFoundException)
                {
                    // No Routes folder is present.
                    Debug.WriteLine("Folder not found.");
                }

                // Book
                try
                {
                    // Look for a folder on the SD card named Routes.
                    ExternalStorageFolder folder = await _sdCard.GetFolderAsync("Book");

                    // Get all files from the Routes folder.
                    IEnumerable<ExternalStorageFile> files = await folder.GetFilesAsync();

                    Debug.WriteLine("found folder");
                    // Add each GPX file to the Routes collection.
                    foreach (ExternalStorageFile esf in files)
                    {
                        Debug.WriteLine("found file " + esf.Name);
                        if (esf.Path.EndsWith(".txtx"))
                        {
                            int loc = esf.Name.LastIndexOf(".txtx");
                            string bookname = esf.Name.Substring(0, loc);

                            // remove first part of book names
                            loc = bookname.LastIndexOf("-");
                            if (loc + 1 < bookname.Length)
                            {
                                bookname = bookname.Substring(loc + 1).Trim();
                            }

                            // add to file list
                            Book newBook = new Book();
                            newBook.Name = bookname;
                            newBook.Path = esf.Path;
                            this.files.Add(newBook);
                        }
                    }
                    Debug.WriteLine("done");
                }
                catch (FileNotFoundException)
                {
                    // No Routes folder is present.
                    Debug.WriteLine("Folder not found.");
                }

                // Books
                try
                {
                    // Look for a folder on the SD card named Routes.
                    ExternalStorageFolder folder = await _sdCard.GetFolderAsync("Books");

                    // Get all files from the Routes folder.
                    IEnumerable<ExternalStorageFile> files = await folder.GetFilesAsync();

                    Debug.WriteLine("found folder");
                    // Add each GPX file to the Routes collection.
                    foreach (ExternalStorageFile esf in files)
                    {
                        Debug.WriteLine("found file " + esf.Name);
                        if (esf.Path.EndsWith(".txtx"))
                        {
                            int loc = esf.Name.LastIndexOf(".txtx");
                            string bookname = esf.Name.Substring(0, loc);

                            // remove first part of book names
                            loc = bookname.LastIndexOf("-");
                            if (loc + 1 < bookname.Length)
                            {
                                bookname = bookname.Substring(loc + 1).Trim();
                            }

                            // add to file list
                            Book newBook = new Book();
                            newBook.Name = bookname;
                            newBook.Path = esf.Path;
                            this.files.Add(newBook);
                        }
                    }
                    Debug.WriteLine("done");
                }
                catch (FileNotFoundException)
                {
                    // No Routes folder is present.
                    Debug.WriteLine("Folder not found.");
                }
            }
            else
            {
                // No SD card is present.
                Debug.WriteLine("The SD card is mssing.");
            }

            this.fileList.ItemsSource = this.files;
        }
    }
}