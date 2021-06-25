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
                LoadControlData();  // 콤보 박스 데이터 로딩 메서드(실행했을때 콤보박스에 데이터가 동적으로 들어가있을것임)
                
                LoadGridData();  // 테이블 데이터를 그리드에 표시해줌

                InitErrorMessages();  // 처음 로드됬을때는 텍스트박스옆의 에러 메세지를 숨김
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 StoreList Loaded : {ex}");
                throw ex;
            }
        }

        private void LoadControlData()
        {
            var plantCodes = Logic.DataAccess.GetSettings().Where(c => c.BasicCode.Contains("PC01")).ToList();  // PC01 글자를 갖고있는 데이터를 갖고옴
            CboPlantCode.ItemsSource = plantCodes;

            var facilityIds = Logic.DataAccess.GetSettings().Where(c => c.BasicCode.Contains("FAC1")).ToList();
            CboSchFacilityID.ItemsSource = facilityIds;
        }


        // 라벨의 에러메세지(빨간글씨)를 숨기는 메서드
        private void InitErrorMessages()  
        {
            LblPlantCode.Visibility = LblSchDate.Visibility = 
            LblSchEndTime.Visibility = LblSchLoadTime.Visibility =
            LblSchStartTime.Visibility = LblSchFacilityID.Visibility = 
            LblSchAmount.Visibility = Visibility.Hidden;
        }


        private void LoadGridData()
        {
            List<Model.Schedules> schedules = Logic.DataAccess.GetSchedules();  // DB 데이터를 갖고와서 List형인 settings에 넣어줌
            this.DataContext = schedules;
        }


        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }


        private async void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            var item = new Model.Schedules();
            if (ValidInput() != true) return;

            // 텍스트 박스의 값을 DB(Schedules)에 넣어줌
            item.PlantCode = CboPlantCode.SelectedValue.ToString();
            item.SchDate = DateTime.Parse(DtpSchDate.Text);
            item.LoadTime = int.Parse(TxtSchLoadTime.Text);
            item.SchStartTime = TmpSchStartTime.SelectedDateTime.Value.TimeOfDay;
            item.SchEndTime = TmpSchEndTime.SelectedDateTime.Value.TimeOfDay;
            item.SchFacilityID = CboSchFacilityID.SelectedValue.ToString();
            item.SchAmount = (int)NudSchAmount.Value;

            item.RegDate = DateTime.Now;
            item.RegID = "MRP";

            try
            {
                var result = Logic.DataAccess.SetSchedules(item);  // Setsetting이 실행되면 데이터 입력이나 수정이 될 것임.

                if (result == 0)   // result == 0 이면 데이터가 안들어갔다는 의미임
                {
                    Commons.LOGGER.Error("데이터 입력시 오류 발생");
                    await Commons.ShowMessageAsync("오류", "데이터 입력 실패");
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 입력 성공 : {item.SchIdx}");  // 데이터 새로 입력했으면 뭘 입력했는지 알려주기
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

            if(CboPlantCode.SelectedValue == null)
            {
                LblPlantCode.Visibility = Visibility.Visible;
                LblPlantCode.Text = "공장을 선택하세요";
                isValid = false;
            }

            if(string.IsNullOrEmpty(DtpSchDate.Text))
            {
                LblSchDate.Visibility = Visibility.Visible;
                LblSchDate.Text = "공정일을 입력하세요";
                isValid = false;
            }


            try
            {
                // 공장별로 공정일일 DB값이 있으면 입력되면안됨
                // ex) PC01001 (수원) 2021-06-24 

                var result = Logic.DataAccess.GetSchedules().Where(s => s.PlantCode.Equals(CboPlantCode.SelectedValue.ToString()))
                    .Where(d => d.SchDate.Equals(DateTime.Parse(DtpSchDate.Text))).Count();

                // 공장코드의 날짜에 이미 있는지 확인
                if (result > 0)  // result가 0이 아니라는건 뭔가 들어갔다는 거임 => 이 경우를 막아줘야함
                {
                    LblSchDate.Visibility = Visibility.Visible;
                    LblSchDate.Text = "해당공장 공정일에 계획이 이미 있습니다";
                    isValid = false;
                }
            }
            catch (Exception ex)
            {

            }

            


            if (string.IsNullOrEmpty(TxtSchLoadTime.Text))
            {
                LblSchLoadTime.Visibility = Visibility.Visible;
                LblSchLoadTime.Text = "로드타임을 입력하세요";
                isValid = false;
            }

            if(CboSchFacilityID.SelectedValue == null)
            {
                LblSchFacilityID.Visibility = Visibility.Visible;
                LblSchFacilityID.Text = "공정설비를 선택하세요";
                isValid = false;
            }
          
            if(NudSchAmount.Value <= 0)
            {
                LblSchAmount.Visibility = Visibility.Visible;
                LblSchAmount.Text = "계획수량이 없습니다 ";
                isValid = false;
            }
          
            return isValid;
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {

            if (ValidInput() != true) return;


            // 텍스트 박스의 값을 DB(item)에 넣어줌
            var item = GrdData.SelectedItem as Model.Schedules;

            item.PlantCode = CboPlantCode.SelectedValue.ToString();
            item.SchDate = DateTime.Parse(DtpSchDate.Text);
            item.LoadTime = int.Parse(TxtSchLoadTime.Text);
            item.SchStartTime = TmpSchStartTime.SelectedDateTime.Value.TimeOfDay;
            item.SchEndTime = TmpSchEndTime.SelectedDateTime.Value.TimeOfDay;
            item.SchFacilityID = CboSchFacilityID.SelectedValue.ToString();
            item.SchAmount = (int)NudSchAmount.Value;

            item.ModDate = DateTime.Now;
            item.ModID = "MRP";

            try
            {
                var result = Logic.DataAccess.SetSchedules(item);  // 데이터 입력또는 수정할때 사용할 메서드

                if(result == 0)   // result == 0 이면 데이터가 안들어갔다는 의미임
                {
                    Commons.LOGGER.Error("데이터 수정시 오류 발생");
                    await Commons.ShowMessageAsync("오류", "데이터 수정 실패");
                }
                else
                {
                    Commons.LOGGER.Info($"데이터 수정 성공 :  {item.PlantCode}");  // 데이터 수정했으면 뭘 수정했는지 알려주기
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
            // 모든 컴포넌트를 비워주는 메서드
            TxtSchIdx.Text = "";
            CboPlantCode.SelectedItem = null;
            DtpSchDate.Text = "";
            TxtSchLoadTime.Text = "";
            TmpSchStartTime.SelectedDateTime = null;
            TmpSchEndTime.SelectedDateTime = null;
            CboSchFacilityID.SelectedItem = null;
            NudSchAmount.Value = 0;
            GrdData.SelectedItem = null;

            CboPlantCode.Focus();   // 모든 컴포넌트를 비워주면서 포커스를 공장에 해줌(유저 편의성)
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var search = DtpSearchDate.Text;
            var list = Logic.DataAccess.GetSchedules().Where(s => s.SchDate.Equals(search)).ToList();
            // 검색하는 날짜가 DB상의 데이터와 일치하는 부분을 list에 저장(search와 SchDate와 일치하는 데이터를 저장하는 것임)

            this.DataContext = list;
        }


        // 그리드의 데이터를 클릭했을때 이벤트
        private void GrdData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                // 그리드의 데이터를 각 컴포넌트에 출력
                var item = GrdData.SelectedItem as Model.Schedules;

                TxtSchIdx.Text = item.SchIdx.ToString();  // 순번이 정수라서 문자열화 시켜서 텍스트박스 SchIdx에 출력
                CboPlantCode.SelectedValue = item.PlantCode;
                DtpSchDate.Text = item.SchDate.ToString();
                TxtSchLoadTime.Text = item.LoadTime.ToString();

                TmpSchStartTime.SelectedDateTime = new DateTime(item.SchStartTime.Value.Ticks);
                TmpSchEndTime.SelectedDateTime = new DateTime(item.SchEndTime.Value.Ticks);    // item.SchEndTime;

                CboSchFacilityID.SelectedValue = item.SchFacilityID;
                NudSchAmount.Value = item.SchAmount;

            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외 발생 {ex}");
                ClearInputs();
            }
        }
    }
}
