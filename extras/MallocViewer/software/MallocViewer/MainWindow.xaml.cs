using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TeensySharp;

namespace MallocViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            cnv.Width = columns;
            cnv.Height = 8 * rows;

            watcher = new TeensyWatcher();
            watcher.ConnectionChanged += ConnectedTeensiesChanged;   // get notifications about plugged in/out Teensies          
            ConnectedTeensiesChanged(null, null);                    // fill combobox initially

            worker.DoWork += getData;
        }

        const int columns = 800;  // match with firmware
        const int rows = 70;
        const int barHeight = 8;

        void Parse(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 8 || !(parts[0] == "+" || parts[0] == "-")) return;

            bool add = parts[0] == "+";
            int idx = int.Parse(parts[1]);
            int blockStart = int.Parse(parts[2]);
            int blockEnd = int.Parse(parts[3]);
            int total = int.Parse(parts[4]);
            int totalUser = int.Parse(parts[5]);
            int totalFree = int.Parse(parts[6]);
            int nrBlocks = int.Parse(parts[7]);

            if (add) addMem(idx, blockStart, blockEnd);
            else removeMem(idx);

            allocs++;
            curAllocs.Text = allocs.ToString();
            curAllocN.Text = nrBlocks.ToString();
            curAllocB.Text = total.ToString();
            curAllocU.Text = totalUser.ToString();
            curFree.Text = totalFree.ToString();
        }

        private void getData(object sender, DoWorkEventArgs e)
        {
            while (run)
            {
                var cmd = new char[] { '*' };
                try
                {
                    port.Write(cmd, 0, 1);
                    var line = port.ReadLine();
                    Dispatcher.Invoke(() => Parse(line));
                }
                catch { }
            }
        }


        Dictionary<int, List<Rectangle>> allocations = new Dictionary<int, List<Rectangle>>();

        private List<Rectangle> drawRegion(int start, int end, Brush fillCol)
        {
            int curWidth = end - start;
            int curCol = start % columns;
            int curRow = start / columns;

            var rlist = new List<Rectangle>();

            Rectangle r;

            while (curCol + curWidth >= columns)
            {
                r = new Rectangle();
                r.Fill = fillCol;
                r.Stroke = fillCol;
                r.StrokeThickness = 0.2;
                r.RadiusX = barHeight / 2.5;
                r.RadiusY = barHeight / 2.5;
                r.Height = barHeight;
                r.Width = columns - curCol;
                Canvas.SetLeft(r, curCol);
                Canvas.SetTop(r, barHeight * curRow);
                rlist.Add(r);

                curWidth -= (columns - curCol);
                curCol = 0;
                curRow++;
            }

            r = new Rectangle();
            r.Fill = fillCol;
            r.Stroke = Brushes.White;
            r.StrokeThickness = 0.2;
            r.Height = barHeight;
            r.Width = curWidth;
            r.RadiusX = barHeight / 2.0;
            r.RadiusY = barHeight / 2.0;
            Canvas.SetLeft(r, curCol);
            Canvas.SetTop(r, barHeight * curRow);
            rlist.Add(r);

            return rlist;
        }

        private void addMem(int idx, int blockStart, int blockEnd)
        {
            if (!allocations.ContainsKey(idx))
            {
                List<Rectangle> list = new List<Rectangle>();

                Random rand = new Random();
                Brush brush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(50, 256), 0, 0));


                list.AddRange(drawRegion(blockStart, blockEnd, brush));


                allocations.Add(idx, list);

                foreach (var rect in allocations[idx])
                {
                    cnv.Children.Add(rect);
                }
            }
        }

        private void removeMem(int idx)
        {
            if (allocations.ContainsKey(idx))
            {
                foreach (var rect in allocations[idx])
                {
                    cnv.Children.Remove(rect);
                }
                allocations.Remove(idx);
            }
        }




        BackgroundWorker worker = new BackgroundWorker();

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var cmd = new char[] { 'S' };
            port.Write(cmd, 0, 1);
            var s = port.ReadLine();



            foreach(var a in allocations)
            {
                removeMem(a.Key);
            }

            run = true;

            worker = new BackgroundWorker();
            worker.DoWork += getData;

            worker.RunWorkerAsync();
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            run = false;
            btnStop.IsEnabled = false;
            btnStart.IsEnabled = true;
            while (worker.IsBusy) ;

            ConnectButton_Click(null, null);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // close if already open           
            port?.Close();
            port = null;
            conText.Text = $"Not connected";

            var Teensy = cbTeensy.SelectedItem as USB_Device;
            if (Teensy != null)
            {
                try
                {
                    port = new SerialPort(Teensy.Port);
                    port.Open();
                    port.DiscardInBuffer();
                    //port.ReadTimeout = 100;
                    //port.WriteTimeout = 100;

                    conText.Text = $"Connected to {Teensy.BoardId.ToString()} on {Teensy.Port}";
                    btnStart.IsEnabled = true;
                }
                catch { }
            }
        }

        private void ConnectedTeensiesChanged(object sender, ConnectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                cbTeensy.Items.Clear();
                foreach (var Teensy in watcher.ConnectedDevices.Where(t => t.UsbType == USB_Device.USBtype.UsbSerial))
                {
                    cbTeensy.Items.Add(Teensy);
                }
                cbTeensy.SelectedIndex = 0;
            }
            );
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            port?.Close();
            port = null;
        }

        private SerialPort port;
        private TeensyWatcher watcher;
        private int allocs = 0;
        private bool run = true;


    }
}

