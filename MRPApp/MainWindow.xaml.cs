using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
using MRPApp.View;
using MRPApp.View.Account;
using MRPApp.View.Store;
using MRPApp.View.Setting;
using MRPApp.View.Schedule;
using System.Configuration;
using MRPApp.View.Process;
using MRPApp.View.Report;

namespace MRPApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {

        }

        // 메인 윈도우가 실행될때 발생하는 이벤트
        private void MetroWindow_Activated(object sender, EventArgs e)
        {
            //if (Commons.LOGINED_USER != null)
            //    BtnLoginedId.Content = $"{Commons.LOGINED_USER.UserEmail} ({Commons.LOGINED_USER.UserName})";

            // App.config에 추가한 key값을 컨트롤 박스 옆에 출력
            //var temp = ConfigurationManager.AppSettings["Test"];    // AppSettings[]에는 key값을 넣어줌
            //var temp = ConfigurationManager.AppSettings.Get("PlantCode"); 
            //BtnPlantName.Content = temp;


            // PlantCode에 해당되는 공장이름을 컨트롤 박스 옆에 출력해줌(DB의 데이터를 조회해서 동적으로 추가됨)
            Commons.PLANTCODE = ConfigurationManager.AppSettings.Get("PlantCode");
            Commons.FACILITYID = ConfigurationManager.AppSettings.Get("FacilityID");
            try
            {
                var plantName = Logic.DataAccess.GetSettings().Where(c => c.BasicCode.Equals(Commons.PLANTCODE)).FirstOrDefault().CodeName;
                BtnPlantName.Content = plantName;
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외 발생 : {ex}");
            }
        }


        private async void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("종료", "프로그램을 종료하시겠습니까?",
                   MessageDialogStyle.AffirmativeAndNegative, null);

            if (result == MessageDialogResult.Affirmative)  // 프로그램 종료창에서 OK를 누르면 프로그램이 종료됨
                Application.Current.Shutdown();
        }


        private async void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ActiveControl.Content = new MyAccount();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnAccount_Click : {ex}");
                await this.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        private async void BtnUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ActiveControl.Content = new UserList();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnUser_Click : {ex}");
                await this.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        private void BtnStore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ActiveControl.Content = new StoreList();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnStore_Click : {ex}");
                this.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        
        private void BtnSetting_click(object sender, RoutedEventArgs e)
        {
            // 세팅 버튼 클릭했을때 세팅 화면 띄우기
            try
            {
                ActiveControl.Content = new SettingList();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnSetting_click : {ex}");
                this.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }
       

        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            // 공정계획 버튼 클릭했을때 세팅 화면 띄우기

            try
            {
                ActiveControl.Content = new ScheduleList();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnSchedule_click : {ex}");
                this.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        private void BtnMonitoring_Click(object sender, RoutedEventArgs e)
        {
            // 모니터링 버튼 클릭했을때 ProcessView 화면 띄우기
            try
            {
                ActiveControl.Content = new ProcessView();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnMonitoring_Click : {ex}");
                this.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }

        private void BtnReport_Click(object sender, RoutedEventArgs e)
        {
            // 리포트 버튼 클릭했을때 ReportView 화면 띄우기
            try
            {
                ActiveControl.Content = new ReportView();
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnReport_Click : {ex}");
                this.ShowMessageAsync("예외", $"예외발생 : {ex}");
            }
        }
    }
}
