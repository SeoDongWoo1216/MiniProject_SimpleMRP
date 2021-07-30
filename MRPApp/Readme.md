# MRP UI 구성
- WPF UI를 정리한 리드미
- DB와의 연동은 Entity Framework를 사용했습니다.

<br>

## 실행화면
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MRP_UI_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif" >

- [MainForm](#mainform) : 실행했을때 처음 뜨는 화면이며, 5개의 버튼을 통해 이벤트가 실행됩니다.
- [ScheduleForm](#scheduleform) : 공정 계획화면이며, 공정계획 데이터를 CRUD를 통해 관리할 수 있습니다.
- [ProcessForm](#processform) : 공정 모니터링화면이며, '시작'버튼을 눌렀을때 칼라센서의 값에따라 성공, 실패 개수가 Count됩니다.
- [ReportForm](#reportform) : 공정 리포트화면이며, 모니터링의 누적 데이터가 그래프를 통해 출력됩니다.(Chart는 라이브 차트를 사용했습니다)
- [SettingForm](#settingform) : 공통 코드 관리화면이며, 공장코드와 공장이름 데이터를 CRUD를 통해 관리할 수 있습니다.

## 소스분석

### MainForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MainForm.png" width = "80%" Height = "80%">

- 공정계획, 모니터링, 리포트, 설정, 종료 버튼을 통한 이벤트로 각 View를 호출합니다.

<br>

### ScheduleForm

<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ScheduleForm.png" width = "80%" Height = "80%">
- 공정계획을 관리하는 Form입니다.
- SettingForm 에서 설정한 공장의 생산 스케줄을 입력합니다.
- 공정일자에 따른 공장이름, 로드타임, 공정시간 등을 각 컴포넌트에 입력하고 '입력'버튼이나 '수정'버튼을 통해 DB에 저장합니다.(이때 데이터가 제대로 들어갔는지 유효성 검사 후에 DB에 저장됩니다)

<br><br>

```C#
// DB 데이터를 그리드에 출력하는 이벤트
private void LoadGridData()
{
    List<Model.Schedules> schedules = Logic.DataAccess.GetSchedules();  // DB 데이터를 갖고와서 List형인 settings에 넣어줌
    this.DataContext = schedules;
}
```

<br>

```C#
// 그리드를 클릭했을때 오른쪽 컴포넌트(텍스트박스, 콤보박스 등)에 출력하는 이벤트
private void GrdData_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
{
  // 그리드
  var item = GrdData.SelectedItem as Model.Schedules;   // Model.Schedules 는 DB 테이블 이름
  
  텍스트박스.Text = item.(DB Column).ToString();
  콤보박스.SelectedValue = item.(DB Column);
  데이트타임피커.Text = item.(DB Column).ToString();
  // 등등..

}
```

<br>

### ProcessForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ProcessForm.png" width = "80%" Height = "80%">

<br>

### ReportForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ReportForm.png" width = "80%" Height = "80%">

<br>

### SettingForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/SettingForm.png" width = "80%" Height = "80%">

<br>





<p align = "center">
</p>
