using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutomationThreadingIssue.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Thread.CurrentThread.Name = "WPF Thread";

            AutomationMessenger.Received += AutomationMessenger_Received;
        }

        private void AutomationMessenger_Received(object sender, AutomationMessengerEventArgs e)
        {
            switch (e.Name)
            {
                case "Status":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Status_TextBlock.Text = e.Message;
                    });

                    break;

                case "UserInput":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AutomationInput_Grid.IsEnabled = true;

                        Input_Button.Tag = e.CallbackAction;

                        Status_TextBlock.Text = e.Message;
                    });

                    break;
            }
        }

        private void Run_Button_Click(object sender, RoutedEventArgs e)
        {
            Run_Button.IsEnabled = false;

            var backgroundAutomationTask = new Task(() =>
            {
                var worker = new AutomationBackgroundWorker();
                worker.Begin();
            });

            backgroundAutomationTask.Start();
        }

        private void Input_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Input_Button.Tag != null && Input_Button.Tag is Action<string> callbackAction)
            {
                callbackAction.Invoke(Input_TextBox.Text);
            }
        }
    }
}
