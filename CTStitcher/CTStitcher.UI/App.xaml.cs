using CTStitcher.Context;
using CTStitcher.Logging;
using CTStitcher.UI.ViewModels;
using CTStitcher.UI.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CTStitcher.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] startupArgs = e.Args;
            ESAPIThreadContext.Initialize(Dispatcher);
            if (startupArgs.Any())
            {
                try
                {
                    EclipseContextHelper contextHelper = new EclipseContextHelper();
                    if (contextHelper.GenerateEclipseContext(startupArgs.ToList()))
                    {
                        Logger.GetInstance().LogError(contextHelper.ErrorMessage);
                        Logger.GetInstance().LogError(contextHelper.StackTraceMessage, true);
                        return;
                    }
                }
                catch (Exception except)
                {
                    Logger.GetInstance().LogError($"Failed to initialize script because: {except.Message}");
                    Logger.GetInstance().LogError(except.StackTrace, true);
                    return;
                }
            }
            else
            {
                Logger.GetInstance().LogError("No startup arguments present!", true);
            }

            Thread t = new Thread(() =>
            {
                MainView mw = new MainView() { DataContext = new MainViewModel() };
                mw.ShowDialog();
                CloseApplication();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void CloseApplication()
        {
            ESAPIThreadContext.RunOnESAPIThread(() => { Application.Current.Shutdown(); });
        }
    }
}
