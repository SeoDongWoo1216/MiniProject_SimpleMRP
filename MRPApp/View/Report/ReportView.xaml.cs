using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

                // DisplayChart();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 ReportView Loaded : {ex}");
                throw ex;
            }
        }

        private void DisplayChart(List<Model.Report> list)
        {
            int[] schAmounts = list.Select(a => (int)a.SchAmount).ToArray();
            int[] prcOKAmounts = list.Select(a => (int)a.PrcOkAmount).ToArray();
            int[] prcFailAmounts = list.Select(a => (int)a.PrcFailAmount).ToArray();

            var series1 = new LiveCharts.Wpf.ColumnSeries
            {
                Title = "계획수량",
                Fill = new SolidColorBrush(Colors.Green),
                Values = new LiveCharts.ChartValues<int>(schAmounts)
            };

            var series2 = new LiveCharts.Wpf.ColumnSeries
            {
                Title = "성공 수량",
                Fill = new SolidColorBrush(Colors.Blue),
                Values = new LiveCharts.ChartValues<int>(prcOKAmounts)
            };

            var series3 = new LiveCharts.Wpf.ColumnSeries
            {
                Title = "실패 수량",
                Fill = new SolidColorBrush(Colors.Red),
                Values = new LiveCharts.ChartValues<int>(prcFailAmounts)
            };

            // 차트할당
            ChtReport.Series.Clear();
            ChtReport.Series.Add(series1);
            ChtReport.Series.Add(series2);
            ChtReport.Series.Add(series3);

            // X축 좌표값을 날짜로 표시
            ChtReport.AxisX.First().Labels = list.Select(a => a.PrcDate.ToString("yyyy-MM-dd")).ToList();
        }

        private void InitControls()
        {
            DtpSearchStartDate.SelectedDate = DateTime.Now.AddDays(-7);
            DtpSearchEndDate.SelectedDate = DateTime.Now;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(IsValidInputs())
            {
                var startDate = ((DateTime)DtpSearchStartDate.SelectedDate).ToString("yyyy-MM-dd");
                // DateTime?(DateTime물음표) 타입이라서 타입이 다르기때문에 바로 Tostring을 못씀 => DateTime으로 형변환 후에 사용해야함
                // DateTime?은 Nullable 타입임.

                var endDate = ((DateTime)DtpSearchEndDate.SelectedDate).ToString("yyyy-MM-dd");

                var searchResult = Logic.DataAccess.GetReportDatas(startDate, endDate, Commons.PLANTCODE);

                DisplayChart(searchResult);
            }
        }


        // 날짜가 빠져있거나 StartDate가 EndDate보다 최신이면 검색하면 안됨. (오류 제어)
        private bool IsValidInputs()
        {
            var result = true;

            // 둘 중 하나라도 null이면 안됨
            if (DtpSearchStartDate.SelectedDate == null || DtpSearchEndDate.SelectedDate == null)
            {
                Commons.ShowMessageAsync("검색", "검색할 일자를 선택하세요");
                result = false;
            }

            // 시작일자가 종료일자 날짜보다 더 클때(최신일때)
            if (DtpSearchStartDate.SelectedDate > DtpSearchEndDate.SelectedDate)
            {
                Commons.ShowMessageAsync("검색", "시작 일자가 종료 일자보다 최신일 수 없습니다");
                result = false;
            }

            return result;
        }
    }
}
