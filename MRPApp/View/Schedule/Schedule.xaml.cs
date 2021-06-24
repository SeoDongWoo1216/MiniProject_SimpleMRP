using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace MRPApp.View.Schedule
{
    /// <summary>
    /// MyAccount.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScheduleList : Page
    {
        public ScheduleList()
        {
            InitializeComponent();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadGridData();

                InitErrorMessages();  // 처음 로드됬을때는 텍스트박스옆의 에러 메세지를 숨김
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 StoreList Loaded : {ex}");
                throw ex;
            }
        }


        // 에러메세지를 숨기는 메서드
        private void InitErrorMessages()  
        {
            LblBasicCode.Visibility = LblCodeName.Visibility = LblCodeDesc.Visibility = Visibility.Hidden;
        }


        private void LoadGridData()
        {
            List<Model.Settings> settings = Logic.DataAccess.GetSettings();  // DB 데이터를 갖고와서 List형인 settings에 넣어줌
            this.DataContext = settings;
        }


        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //NavigationService.Navigate(new EditUser());
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnEditUser_Click : {ex}");
                throw ex;
            }
        }


        private void BtnAddStore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               // NavigationService.Navigate(new AddStore());
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 BtnAddStore_Click : {ex}");
                throw ex;
            }
        }


        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }


        private async void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            if (ValidInput() != true) return;

            var setting = new Model.Settings();

            // 텍스트 박스의 값을 DB(setting)에 넣어줌
            setting.BasicCode = TxtBasicCode.Text;
            setting.CodeName = TxtCodeName.Text;
            setting.CodeDesc = TxtCodeDesc.Text; 
            setting.RegDate = DateTime.Now;      // 등록날짜를 현재 시간으로 넣어줌
            setting.RegID = "MRP";               // 등록한 사람을 일단은 MRP로 넣어줌

            try
            {
                var result = Logic.DataAccess.Setsettings(setting);  // Setsetting이 실행되면 데이터 입력이나 수정이 될 것임.

                if (result == 0)   // result == 0 이면 데이터가 안들어갔다는 의미임
                {
                    Commons.LOGGER.Error("데이터 입력시 오류 발생");
                    await Commons.ShowMessageAsync("오류", "데이터 입력 실패");
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 입력 성공 : {setting.BasicCode}");  // 데이터 새로 입력했으면 뭘 입력했는지 알려주기
                    ClearInputs();
                    LoadGridData();   // 데이터가 바꼈으니 그리드의 데이터도 바껴야함 => 그리드를 다시 로드하면됨
                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 {ex}");
            }
        }


        // 입력 데이터 검증 메서드(데이터 입력이나 수정전에 이 함수를 통과해야 입력이나 수정이된다)
        private bool ValidInput()
        {
            var isValid = true;
            InitErrorMessages();

            if(string.IsNullOrEmpty(TxtBasicCode.Text))
            {
                LblBasicCode.Visibility = Visibility.Visible;
                LblBasicCode.Text = "코드를 입력하세요";
                isValid = false;
            }
            else if (Logic.DataAccess.GetSettings().Where(s => s.BasicCode.Equals(TxtBasicCode.Text)).Count() > 0)  // s의 베이직코드가 텍스트박스의 값과 같은지 확인
            {
                // 중복으로 썼을때 오류 제어
                LblCodeName.Visibility = Visibility.Visible;
                LblCodeName.Text =  "중복코드가 존재합니다";
                isValid = false;
            }

            if (string.IsNullOrEmpty(TxtCodeName.Text))
            {
                LblCodeName.Visibility = Visibility.Visible;
                LblCodeName.Text = "코드를 입력하세요";
                isValid = false;
            }
          
            return isValid;
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var setting = GrdData.SelectedItem as Model.Settings;

            if (ValidInput() != true) return;

            // 텍스트 박스의 값을 DB(setting)에 넣어줌

            setting.CodeName = TxtCodeName.Text;
            setting.CodeDesc = TxtCodeDesc.Text;
            setting.ModDate = DateTime.Now;      // 수정날짜를 현재 날짜로 넣어줌
            setting.ModID = "MRP";               // 수정자를 넣어줘야하는데 지금은 로그인폼이 없으니 일단은 MRP로 삽입

            try
            {
                var result = Logic.DataAccess.Setsettings(setting);  // Setsetting이 실행되면 데이터 입력이나 수정이 될 것임.

                if(result == 0)   // result == 0 이면 데이터가 안들어갔다는 의미임
                {
                    Commons.LOGGER.Error("데이터 수정시 오류 발생");
                    await Commons.ShowMessageAsync("오류", "데이터 수정 실패");
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 수정 성공 : {setting.BasicCode}");  // 데이터 수정했으면 뭘 수정했는지 알려주기
                    ClearInputs();   
                    LoadGridData();   // 데이터가 바꼈으니 그리드의 데이터도 바껴야함 => 그리드를 다시 로드하면됨
                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 {ex}");
            }
        }

        private void ClearInputs()
        {
            TxtBasicCode.IsReadOnly = false;
            TxtBasicCode.Background = new SolidColorBrush(Colors.White);

            TxtBasicCode.Text = TxtCodeName.Text = TxtCodeDesc.Text = string.Empty;  // "" 처리로 텍스트박스를 다 지워줌
            TxtBasicCode.Focus();        // 포커스를 코드 텍스트박스에 해줌(유저 편의성)
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var search = TxtSearch.Text.Trim();
            var settings = Logic.DataAccess.GetSettings().Where(s => s.CodeName.Contains(search)).ToList();
            this.DataContext = settings;
        }


        // 그리드의 데이터를 클릭했을때 이벤트
        private void GrdData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                var setting = GrdData.SelectedItem as Model.Settings;

                // 텍스트 박스에 그리드의 데이터를 넣어줌
                TxtBasicCode.Text = setting.BasicCode;
                TxtCodeName.Text = setting.CodeName;
                TxtCodeDesc.Text = setting.CodeDesc;

                // 텍스트박스에 값을 수정못하게 readonly, 색깔을 회색으로 처리
                TxtBasicCode.IsReadOnly = true;  // 값을 못바꾸게 해줌(텍스트박스를 읽기 전용으로 만듬 => 수정을 못하게됨)
                TxtBasicCode.Background = new SolidColorBrush(Colors.LightGray);
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외 발생 {ex}");
                ClearInputs();
            }
        }


        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var setting = GrdData.SelectedItem as Model.Settings;

            if(setting == null)    // 만약 아무것도 클릭안하고 삭제버튼을 클릭했을때
            {
                await Commons.ShowMessageAsync("삭제", "삭제할 데이터를 선택하세요");
                return;
            }
            else                   // 선택한 데이터가있으면 그 데이터를 삭제
            {
                try
                {
                    var result = Logic.DataAccess.DelSettings(setting);

                    if (result == 0)    // result == 0 이면 데이터가 안들어갔다는 의미임
                    {
                        Commons.LOGGER.Error("데이터 삭제시 오류 발생");
                        await Commons.ShowMessageAsync("오류", "데이터 삭제 실패");
                    }
                    else
                    {
                        Commons.LOGGER.Info($"데이터 삭제 성공 : {setting.BasicCode}");  // 로그
                        ClearInputs();
                        LoadGridData();             // 데이터가 바꼈으니 그리드의 데이터도 바껴야함 => 그리드를 다시 로드하면됨
                    }
                }
                catch (Exception ex)
                {
                    Commons.LOGGER.Error($"예외 발생 {ex}");
                   
                }
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BtnSearch_Click(sender, e);  // 검색창에서 엔터를 누르면 검색기능을 탑재 시켜줌(사용자 편의성)
        }
    }
}
