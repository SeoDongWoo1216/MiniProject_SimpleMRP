using System;
using System.Windows;
using System.Windows.Controls;

namespace MRPApp.View.Report
{
    /// <summary>
    /// MyAccount.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ReportView : Page
    {
        public ReportView()
        {
            InitializeComponent();
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitControls();

                DisplayChart();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 ReportView Loaded : {ex}");
                throw ex;
            }
        }

        private void DisplayChart()
        {
            double[] ys1 = new double[] { 10.4, 34.6, 22.1, 15.4, 40.0 };
            double[] ys2 = new double[] { 9.7, 8.3, 2.6, 3.4, 7.7 };

            var series1 = new LiveCharts.Wpf.ColumnSeries
            {
                Title = "First Val",
                Values = new LiveCharts.ChartValues<double>(ys1)
            };
            var series2 = new LiveCharts.Wpf.ColumnSeries
            {
                Title = "Second Val",
                Values = new LiveCharts.ChartValues<double>(ys2)
            };

            // 차트할당
            ChtReport.Series.Clear();
            ChtReport.Series.Add(series1);
            ChtReport.Series.Add(series2);
        }

        private void InitControls()
        {
            DtpSearchStartDate.SelectedDate = DateTime.Now.AddDays(-7);
            DtpSearchEndDate.SelectedDate = DateTime.Now;
        }

        private void BtnEditMyAccount_Click(object sender, RoutedEventArgs e)
        {
           // NavigationService.Navigate(new EditAccount()); // 계정정보 수정 화면으로 변경
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(IsValidInputs())
            {
                MessageBox.Show("검색시작!");
                DisplayChart();
            }
        }

        private bool IsValidInputs()
        {
            var result = true;

            // 검증은 투비 컨티뉴...

            return result;
        }
    }
}
