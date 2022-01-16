using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationThreadingIssue.WPF
{
    public class AutomationBackgroundWorker
    {
        private ManualResetEvent manualResetEvent;
        private Thread automationThread;

        private SynchronizationContext backgroundWorkerContext;

        public AutomationBackgroundWorker()
        {
            Thread.CurrentThread.Name = "Automation Background Worker";
            backgroundWorkerContext = SynchronizationContext.Current;

            automationThread = new Thread(this.Automation);
            automationThread.Name = "Automation Thread";
            automationThread.SetApartmentState(ApartmentState.STA);

            manualResetEvent = new ManualResetEvent(false);
        }

        public void Begin()
        {
            automationThread.Start();

            manualResetEvent.WaitOne();
        }

        public void Complete()
        {
            manualResetEvent.Set();
            manualResetEvent.Close();
        }

        public void Automation()
        {
            var automationSyncContext = SynchronizationContext.Current;

            AutomationMessenger.Send("Status", "Initializing", null);

            // Initialize Automation Elements, etc..
            Task.Delay(1000).Wait();

            AutomationMessenger.Send("Status", "Initialized", null);

            // Get user input
            AutomationMessenger.Send("UserInput", "Please Enter Your Name", UserInputCallback);

            // Wait for their response
        }

        // As you would expect, this method is being called from the WPF Thread / UI Thread
        // How can I synchronize it back to the Automation Thread
        private void UserInputCallback(string userInput)
        {
            //if (Thread.CurrentThread != automationThread)
            //{
            //    automationThread.Join();
            //}
            // Doing this blocks and kills the Automation Thread

            if (string.IsNullOrWhiteSpace(userInput)) return;

            AutomationMessenger.Send("Status", $"Thanks {userInput}", null);

            Complete();
        }
    }

    public static class AutomationMessenger
    {
        public static event AutomationMessengerEventHandler Received;
        public static void Send(string name, string message, Action<string> callbackAction) => Received?.Invoke(null, new AutomationMessengerEventArgs(name, message, callbackAction));
    }

    public delegate void AutomationMessengerEventHandler(object sender, AutomationMessengerEventArgs e);

    public class AutomationMessengerEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public Action<string> CallbackAction { get; set; }
        public AutomationMessengerEventArgs(string name, string message, Action<string> callbackAction) : base()
        {
            Name = name;
            Message = message;
            CallbackAction = callbackAction;
        }
    }
}
