using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TaskManager
{
    public static class ImageConvert
    {
        [DllImport("Kernel32.dll")]
        private static extern uint QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) != 0 ?
                fileNameBuilder.ToString() :
                null;
        }
        public static Icon GetIcon(this Process process)
        {
            try
            {
                string mainModuleFileName = process.GetMainModuleFileName();
                return Icon.ExtractAssociatedIcon(mainModuleFileName);
            }
            catch
            {
                // Probably no access
                return null;
            }
        }
        public static ImageSource ToImageSource(this Icon icon)
        {
            try
            {
                ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
                return imageSource;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static ImageSource ToImageSource(this Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    } 
    public class ProcessListItem
    {
        public int? Id => Process?.Id;
        public string ProcessName => Process.ProcessName;
        public bool KeepAlive { get; set; }
        public Process Process { get; }
        public string FileName { get; }
        public string Arguments { get; }


        public ProcessListItem(Process process)
        {
            Process = process;
            FileName = process.StartInfo.FileName;
            Arguments = process.StartInfo.Arguments;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        internal void Kill()
        {
            try
            {
                Process.Kill();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message, "Eror", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void ChangePriority(ProcessPriorityClass priority)
        {
            try
            {
                Process.PriorityClass = priority;
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message, "Eror", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int NonpagedSystemMemorySize64 { get => (int)Process.NonpagedSystemMemorySize64; }
        public long PagedMemorySize64
        {
            get => Process.PagedMemorySize64;
        }

        public long PrivateMemorySize64
        {
            get => Process.PrivateMemorySize64;
        }
        public long VirtualMemorySize64
        {
            get => Process.VirtualMemorySize64;
        }
        public string StartTime
        {
            get => Process.StartTime.ToString();
        }
        public int Threads
        {
            get => Process.Threads.Count;
        }
        public ImageSource ImageSource
        {
            get
            {
                return Process.GetIcon().ToImageSource();
            }
        }


        [MonitoringDescription("ProcessPriorityClass")]
        public string PriorityClass { get => Process.PriorityClass.ToString(); }

        
    }





}
