using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;

namespace WorkTime_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private AppDbContext _context = new AppDbContext();
        private DateTime? workStartTime;
        private DateTime? breakStartTime;
        private TimeSpan? breakDuration = TimeSpan.Zero;

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            clock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void SaveWorkTime(int employeeId, DateTime startTime, DateTime endTime, TimeSpan breakDuration)
        {
            _context.WorkTimes.Add(new WorkTime { EmployeeId = employeeId, StartTime = startTime, EndTime = endTime, BreakDuration = breakDuration });
            _context.SaveChanges();

            var list = _context.WorkTimes.ToList();
        }
        private void GenerateReport(int employeeId, DateTime startDate, DateTime endDate)
        {
            var workTimes = _context.WorkTimes.Where(w => w.EmployeeId == employeeId && w.StartTime >= startDate && w.EndTime <= endDate).ToList();
            TimeSpan totalWorkTime = TimeSpan.Zero;
            foreach (var workTime in workTimes)
            {
                totalWorkTime += (workTime.EndTime - workTime.StartTime - workTime.BreakDuration);
            }
            MessageBox.Show($"Czas przepracowany od {startDate.ToShortDateString()} do {endDate.ToShortDateString()}: {totalWorkTime.ToString(@"hh\:mm\:ss")}");
        }

        private void StartWork_Click(object sender, RoutedEventArgs e)
        {
            workStartTime = DateTime.Now;
            Status.Text = "Status: W pracy";
        }

        private void EndWork_Click(object sender, RoutedEventArgs e)
        {
            if (workStartTime.HasValue && int.TryParse(WorkerID.Text, out int __))
            {
                DateTime endTime = DateTime.Now;
                SaveWorkTime(int.Parse(WorkerID.Text), (DateTime)workStartTime, endTime, (TimeSpan)breakDuration);
                Status.Text = "Status: Poza pracą";
                workStartTime = null;
            }
            else
            {
                MessageBox.Show("Uzupełnj pole z ID pracownika");
            }
        }

        private void StartBreak_Click(object sender, RoutedEventArgs e)
        {
            breakStartTime = DateTime.Now;
            Status.Text = "Status: Przerwa";
        }

        private void EndBreak_Click(object sender, RoutedEventArgs e)
        {
            if (breakStartTime.HasValue)
            {
                breakDuration += DateTime.Now - breakStartTime.Value;
                breakStartTime = null;
                Status.Text = "Status: W pracy";
            }
        }
        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WorkerID.Text, out int __) && startDatePicker.SelectedDate.Value != null && endDatePicker.SelectedDate.Value != null)
            {
                DateTime startDate = startDatePicker.SelectedDate.Value;
                DateTime endDate = endDatePicker.SelectedDate.Value;
                GenerateReport(int.Parse(WorkerID.Text), startDate, endDate);
            }
            else
            {
                MessageBox.Show("Uzupełnij wszytkie 3 pola poprawnie");
            }
        }
    }

    public class WorkTime
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan BreakDuration { get; set; }
    }
}
