using Hangfire.Annotations;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using TaskManager.Command;
using static System.Net.Mime.MediaTypeNames;
using System.Management;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Threading;
using CpuGpuGraph;

namespace TaskManager
{
    public class ViewModel : INotifyPropertyChanged
    {
        public CpuModel CpuModel { get; } = new CpuModel();
        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private static  PerformanceCounter cpuCounter;
        private static PerformanceCounter ramCounter;
        public ViewModel()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            timer.Tick += UpdateProcessesFilter;
            timer.Tick += ShowStaticsAsync;
            timer.Start();
            LoadPriorites();
        }

        public static string getCurrentCpuUsage()
        {
            int countData = Convert.ToInt32(cpuCounter.NextValue());
            return countData.ToString() + "%";
        }

        public static string getCurrentProcessQty()
        {
            Process[] processList = Process.GetProcesses();
            return processList.Length.ToString();
            
        }

        //public static string getAvailableRAM()
        //{
        //    var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
        //    int percent = 0;
        //    var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
        //        FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
        //        TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
        //    }).FirstOrDefault();

        //    if (memoryValues != null)
        //    {
        //        percent = Convert.ToInt32(((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100);
        //    }

        //    return percent.ToString() + "%";

        //}
        void LoadPriorites()
        {
            var el= Enum.GetValues(typeof(ProcessPriorityClass)).Cast<ProcessPriorityClass>();
            foreach(var el2 in el)
            {
                Priorites.Add(el2);
            }
            SelectedPriorites = Priorites[0];
        }
        private ProcessListItem _selectedProcess;

        internal void ChangePriority()
        {
            SelectedProcess.ChangePriority(SelectedPriorites);
        }

        public ProcessListItem SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged("SelectedProcess");
            }
        }
        private ProcessPriorityClass _selectedPriorites;
        public ProcessPriorityClass SelectedPriorites
        {
            get => _selectedPriorites;
            set
            {
                _selectedPriorites = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProcessListItem> Processes { get; } = new ObservableCollection<ProcessListItem>();
        public ObservableCollection<ProcessPriorityClass> Priorites { get; set; } = new ObservableCollection<ProcessPriorityClass>();

 

        //public void ChangePriority(ProcessPriorityClass priority)
        //{
        //    SelectedProcess.PriorityClass = priority;
        //}

        public void KillSelectedProcess()
        {
            try
            {
                SelectedProcess.Kill();
            }
            catch(Exception er)
            {
                System.Windows.MessageBox.Show(er.Message, "Eror",MessageBoxButton.OK,MessageBoxImage.Error );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private RelayCommand killCommand;
        public RelayCommand KillCommand
        {
            get
            {
                return killCommand ??
                    (killCommand = new RelayCommand(obj =>
                    {
                        
                        KillSelectedProcess();
                    }));
            }
        }
        private RelayCommand changeCommand;
        public RelayCommand ChangeCommand
        {
            get
            {
                return changeCommand ??
                    (changeCommand = new RelayCommand(obj =>
                    {
                        ChangePriority();
                    }));
            }
        }

        private RelayCommand refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                return refreshCommand ??
                    (refreshCommand = new RelayCommand(obj =>
                    {
                        UpdateProcessesFilter();
                    }));
            }
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ??
                    (startCommand = new RelayCommand(obj =>
                    {
                        StartProcess();
                    }));
            }
        }
        private RelayCommand filterCommand;
        public RelayCommand FilterCommand
        {
            get
            {
                return filterCommand ??
                    (filterCommand = new RelayCommand(obj =>
                    {
                        UpdateProcessesFilter();
                    }));
            }
        }
        private RelayCommand checkedCommand;
        public RelayCommand CheckedCommand
        {
            get
            {
                return checkedCommand ??
                    (checkedCommand = new RelayCommand(obj =>
                    {
                        if(SelectedProcess.KeepAlive == false)
                        {
                            SelectedProcess.KeepAlive = true;
                        }
                        else
                        {
                            SelectedProcess.KeepAlive = false;
                        }
                    }));
            }
        }

        private void  UpdateProcessesFilter(object sender = null, EventArgs e = null)
        {

            string text = UIHelper.FindChild<System.Windows.Controls. TextBox>(System.Windows.Application.Current.MainWindow, "searchtext").Text;
            var currentIds = Processes.Select(p => p.Id).ToList(); 

            foreach (var p in Process.GetProcesses())
            {
                if(text.Replace(" ", "")== "" || text=="All" || text=="all" || text=="ALL")
                {
                    if (!currentIds.Remove(p.Id))
                    {
                        Processes.Add(new ProcessListItem(p));
                    }
                }
                else if (p.ProcessName.Contains(text))
                {
                    if (!currentIds.Remove(p.Id))
                    {
                        Processes.Add(new ProcessListItem(p));
                    }
                }
            }

            foreach (var id in currentIds)
            {
                var process = Processes.First(p => p.Id == id);
                if (process.KeepAlive)
                {
                    Process.Start(process.ProcessName, process.Arguments);
                }
                Processes.Remove(process);
            }

        }

        async void ShowStaticsAsync(object sender = null, EventArgs e = null)
        {
            UIHelper.FindChild<TextBlock>(System.Windows.Application.Current.MainWindow, "proctxt").Text = await Task.Run(() =>
            {
                return getCurrentProcessQty();
            });
            UIHelper.FindChild<TextBlock>(System.Windows.Application.Current.MainWindow, "cptxt").Text = await Task.Run(() =>
            {
                return getCurrentCpuUsage();
            });
            UIHelper.FindChild<TextBlock>(System.Windows.Application.Current.MainWindow, "memorytxt").Text = await Task.Run(() =>
            {
                return getAvailableRAM();
            });
        }

        public static string getAvailableRAM()
        {
            
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            int percent = 0;
            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                percent = Convert.ToInt32(((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100);
            }

            return percent.ToString() + "%";

        }
        private void StartProcess()
        {
            string _file;
            OpenFileDialog openFileDialog1 =new OpenFileDialog(); 
            if (openFileDialog1.ShowDialog() == DialogResult.OK) 
            {
                _file = openFileDialog1.FileName;
                try
                {
                    Process.Start(_file);
                }
                catch (IOException)
                {
                }
            }
        }
    }
  
}
