using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
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
        const int columns = 800;  // match col/row with firmware
        const int rows = 70;
        const int barHeight = 8;
     
        public MainWindow()
        {
            InitializeComponent();

            cnv.Width = columns;
            cnv.Height = 8 * rows;

            watcher = new TeensyWatcher();
            watcher.ConnectionChanged += ConnectedTeensiesChanged;   // get notifications about plugged in/out Teensies          
            ConnectedTeensiesChanged(null, null);                    // fill combobox initially

            worker.DoWork += doWork;
        }

        #region background work -----------------------------------------------------------

        private void doWork(object sender, DoWorkEventArgs e)
        {
            if (Teensy == null) return;

            Dispatcher.Invoke(() =>   
            {
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
            });

            using (var port = new SerialPort(Teensy.Port))
            {
                port.Open();
                port.DiscardInBuffer();

                port.Write("c"); // clear memory              

                while (run)
                {
                    var cmd = new char[] { '*' };
                    try
                    {
                        port.Write("*");
                        var line = port.ReadLine();
                        Dispatcher.Invoke(() => Parse(line));
                    }
                    catch { }
                }
                port.Close();
            }

            Dispatcher.Invoke(() =>
            {
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
            });
        }

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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!worker.IsBusy)
            {
                foreach (var a in allocations)
                {
                    removeMem(a.Key);
                }
                allocs = 0; 

                run = true;
                worker.RunWorkerAsync();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            run = false;
        }

        readonly BackgroundWorker worker = new BackgroundWorker();

        private bool run = true;
        #endregion

        #region drawing -------------------------------------------------------------------

        private List<Rectangle> drawRegion(int start, int end, Brush fillCol)
        {
            int curWidth = end - start;
            int curCol = start % columns;
            int curRow = start / columns;

            var rlist = new List<Rectangle>();

            Rectangle r;

            while (curCol + curWidth >= columns)
            {
                r = new Rectangle
                {
                    Fill = fillCol,
                    Stroke = fillCol,
                    StrokeThickness = 0.2,
                    RadiusX = barHeight / 3,
                    RadiusY = barHeight / 3,
                    Height = barHeight,
                    Width = columns - curCol
                };
                Canvas.SetLeft(r, curCol);
                Canvas.SetTop(r, barHeight * curRow);
                rlist.Add(r);

                curWidth -= (columns - curCol);
                curCol = 0;
                curRow++;
            }

            r = new Rectangle
            {
                Fill = fillCol,
                Stroke = Brushes.White,
                StrokeThickness = 0.2,
                Height = barHeight,
                Width = curWidth,
                RadiusX = barHeight / 3,
                RadiusY = barHeight / 3
            };
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

        private readonly Dictionary<int, List<Rectangle>> allocations = new Dictionary<int, List<Rectangle>>();

        private int allocs = 0;
        #endregion

        #region Connected Teensy ----------------------------------------------------------
        USB_Device Teensy;
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
                Teensy = cbTeensy.SelectedItem as USB_Device;
            }
            );

        }
        private readonly TeensyWatcher watcher;

        #endregion

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            run = false;            
            while(worker.IsBusy)
            {
                await Task.Delay(10);
            }
            worker.Dispose();
        }                           
    }
}

