# MRP UI 구성
WPF UI를 정리한 리드미

## 실행화면
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MRP_UI_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif" >

- [MainForm](#mainform) : 실행했을때 처음 뜨는 화면이며, 5개의 버튼을 통해 이벤트가 실행됩니다.
- ScheduleForm : 공정 계획화면이며, 공정계획 데이터를 CRUD를 통해 관리할 수 있습니다.
- ProcessForm : 공정 모니터링화면이며, '시작'버튼을 눌렀을때 칼라센서의 값에따라 성공, 실패 개수가 Count됩니다.
- ReportForm : 공정 리포트화면이며, 모니터링의 누적 데이터가 그래프를 통해 출력됩니다.(Chart는 라이브 차트를 사용했습니다)
- SettingForm : 공통 코드 관리화면이며, 공장코드와 공장이름 데이터를 CRUD를 통해 관리할 수 있습니다.

## 소스분석

### MainForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MainForm.png" width = "80%" Height = "80%">

- 공정계획, 모니터링, 리포트, 설정, 종료 버튼을 통한 이벤트로 각 View를 호출합니다.

<br>

### ScheduleForm
- 공정계획을 관리하는 Form입니다.
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ScheduleForm.png" width = "80%" Height = "80%">




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
