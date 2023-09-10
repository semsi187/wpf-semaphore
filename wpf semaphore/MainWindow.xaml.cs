using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf_semaphore;

public class ThreadData
{
    public Semaphore Semaphore { get; set; }
    public string Name { get; set; }
    public bool StopWaiting { get; set; } = false;


    public ThreadData(Semaphore semaphore, string name)
    {
        Semaphore = semaphore;
        Name = name;
    }
}



public partial class MainWindow : Window
{
    public ObservableCollection<string> CreatedThreads { get; set; } = new();
    public ObservableCollection<string> WaitingThreads { get; set; } = new();
    public ObservableCollection<string> WorkingThreads { get; set; } = new();
    public static int placeInSemaphore { get; set; } = 3;

    Semaphore semaphore;
    int ThreadCount = 1;



    public MainWindow()
    {
        InitializeComponent();


        semaphore = new(placeInSemaphore, placeInSemaphore);
        DataContext = this;
    }



    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var threadName = $"Thread {ThreadCount++}";
        Thread thread = new(ThreadMethod!)
        {
            Name = threadName
        };
        threads.Add(thread);
        CreatedThreads.Add(threadName);
    }

    void ThreadMethod(object state)
    {
        var s = state as Semaphore;
        bool st = false;
        bool stopSemaphore = false;
        Random rnd = new();
        while (!st)
        {
            if (stopSemaphore)
                s!.Release();


            var threadName = Thread.CurrentThread.Name;

            try
            {

                if (s!.WaitOne(1500))
                {
                    var threadInfo = $"{threadName} -> {Thread.CurrentThread.ManagedThreadId}";
                    try
                    {
                        if (WaitingThreads.Contains(threadName!))
                            Dispatcher.Invoke(() => WaitingThreads.Remove(threadName!));


                        if (!WorkingThreads.Contains(threadInfo))
                            Dispatcher.Invoke(() => WorkingThreads.Add(threadInfo));
                        Thread.Sleep(rnd.Next(2000, 20000));
                    }
                    finally
                    {
                        st = true;

                        if (!string.IsNullOrEmpty(Thread.CurrentThread.Name))
                            Dispatcher.Invoke(() =>
                            {
                                WorkingThreads.Remove(threadInfo);
                            });
                        stopSemaphore = true;
                    }
                }
            }
            catch (Exception)
            {
                stopSemaphore = true;
                //Thread.CurrentThread.Interrupt();
            }
        }
    }
    List<Thread> threads = new();

    private void WaitingListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (WaitingListBox.SelectedItem is not null)
        {
            var thread = threads.FirstOrDefault(t => t.Name == WaitingListBox.SelectedItem.ToString());
            if (thread!.ThreadState == ThreadState.WaitSleepJoin) thread.Interrupt();
            WaitingThreads.Remove(WaitingListBox.SelectedItem.ToString()!);
        }

    }

    private void CreatedThreadsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {

        if (CreatedThreadsListBox.SelectedItem is not null)
        {
            WaitingThreads.Add(CreatedThreadsListBox.SelectedItem.ToString()!);
            var thread = threads.FirstOrDefault(t => t.Name == CreatedThreadsListBox.SelectedItem.ToString());
            thread!.Start(semaphore);
            CreatedThreads.Remove(CreatedThreadsListBox.SelectedItem.ToString()!);
        }
    }

    private void PlaceInSemaphoreNumberBox_TextChanged(object sender, TextChangedEventArgs e) => placeInSemaphore = Convert.ToInt32(PlaceInSemaphoreNumberBox.Value);

}

