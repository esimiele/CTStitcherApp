using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CTStitcher.Context;
using CTStitcher.Logging;
using CTStitcher.ViewModels;
using CTStitcher.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CTStitcher.UI.ViewModels 
{
    internal class MainViewModel : ObservableObject
    {
        private object _ctStitcherView;

        public object CTStitcherView
        {
            get { return _ctStitcherView; }
            set { SetProperty(ref _ctStitcherView, value); }
        }

        #region commands
        public ICommand WindowClosingCommand { get; set; }
        #endregion

        public MainViewModel()
        {
            Initialize();
        }
        public void Initialize()
        {
            Logger.GetInstance().LogPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\logs\";
            CTStitcherView = new CTStitcherView { DataContext = new CTStitcherViewModel() };
            WindowClosingCommand = new RelayCommand(WindowClosing);
        }

        private void WindowClosing()
        {
            ESAPIThreadContext.RunOnESAPIThreadSync(() =>
            {
                if (EclipseContext.GetInstance().IsInitialized)
                {
                    Logger.GetInstance().UserId = $"{EclipseContext.GetInstance().UserName} ({EclipseContext.GetInstance().UserId})";
                    //be sure to close the patient before closing the application. Not doing so will result in unclosed timestamps in eclipse
                    if (!ReferenceEquals(EclipseContext.GetInstance().Patient, null))
                    {
                        //no modifications made to database, don't bother saving
                        Logger.GetInstance().AppendLogOutput("No modifications made to database objects!");

                        //if a patient was open, close the patient and dump the log file
                        EclipseContext.GetInstance().Application.ClosePatient();
                    }
                    EclipseContext.GetInstance().Application.Dispose();
                }
                if (Logger.GetInstance().Dump())
                {
                    MessageBox.Show("Error! Could not save log file!");
                }
            });
        }
    }
}
