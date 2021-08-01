# WPF UI 구성
- 솔루션 파일 구성
  - DB 쿼리를 처리하는 Logic 디렉터리에 DataAccess.cs
  - Entity Framework의 속성을 처리하는 Logic 디렉터리 
  - 필요한 이미지를 담아놓은 Resource 디렉터리 
  - 각 화면의 View를 처리하는 View 디렉터리
- DB와의 연동은 Entity Framework를 사용했습니다.
- 각 View 디자인은 .xaml 을 참고해주세요

<br>

## 실행화면
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MRP_UI_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif" >

- [MainForm](#mainform) : 실행했을때 처음 뜨는 화면이며, 5개의 버튼을 통해 이벤트가 실행됩니다.
- [ScheduleForm](#scheduleform) : 공정 계획화면이며, 공정계획 데이터를 CRUD를 통해 관리할 수 있습니다.
- [ProcessForm](#processform) : 공정 모니터링화면이며, '시작'버튼을 눌렀을때 칼라센서의 값에따라 성공, 실패 개수가 Count됩니다.
- [ReportForm](#reportform) : 공정 리포트화면이며, 모니터링의 누적 데이터가 그래프를 통해 출력됩니다.(Chart는 라이브 차트를 사용했습니다)
- [SettingForm](#settingform) : 공통 코드 관리화면이며, 공장코드와 공장이름 데이터를 CRUD를 통해 관리할 수 있습니다.

------------

## 소스분석

### MainForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MainForm.png" width = "80%" Height = "80%">

- 공정계획, 모니터링, 리포트, 설정, 종료 버튼을 통한 이벤트로 각 View를 호출합니다.

```C#
// 버튼을 선택했을때 뷰를 띄우는 버튼 이벤트
using MRPApp.View.뷰이름;
 private void Btn뷰이름_Click(object sender, RoutedEventArgs e)
{
    try
    {
        ActiveControl.Content = new 뷰이름();
    }
    catch (Exception ex)
    {
        Commons.LOGGER.Error($"예외발생 Btn뷰이름_click : {ex}");
        this.ShowMessageAsync("예외", $"예외발생 : {ex}");
    }
}
```

<br>

```C#
// 종료버튼 이벤트
private async void BtnExit_Click(object sender, RoutedEventArgs e)
     {
         var result = await this.ShowMessageAsync("종료", "프로그램을 종료하시겠습니까?",
                MessageDialogStyle.AffirmativeAndNegative, null);

         if (result == MessageDialogResult.Affirmative)  // 프로그램 종료창에서 OK를 누르면 프로그램이 종료됨
             Application.Current.Shutdown();
     }
```

<br>

------------

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

------------

### ProcessForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ProcessForm.png" width = "80%" Height = "80%">

##### - ProcessView에 대한 상호 작용 논리
1. 공정계획에서 오늘의 생산 계획 일정을 불러오기
2. 공정이 없으면 에러표시, 시작 버튼을 클릭하지 못하게 만듬
3. 공정이 있으면 오늘의 날짜를 표시, 시작 버튼 활성화
3-1 MQTT Subscription 연결. factory1/machine1/data/ 확인...
4. 시작 버튼 클릭시 새 공정 생성, DB에 입력 (공정코드 : PRC20210618001 (PRC + yyyy + MM + dd + NNN) )
5. 시작 버튼을 클릭했을때 공정처리 애니메이션 시작
6. 로드타임 후에는 애니메이션 중지
7. 센서링값 리턴될때까지 대기
8. 센서링 결과값에 따라서 생산품 색상 변경
9. 현재 공정의 DB값 업데이트
10. 결과 레이블 값 수정/표시



#### - 생산계획 일정 불러오기
```C#
private async void Page_Loaded(object sender, RoutedEventArgs e)
{

    try
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        currSchedule = Logic.DataAccess.GetSchedules().Where(s => s.PlantCode.Equals(Commons.PLANTCODE))
            .Where(s => s.SchDate.Equals(DateTime.Parse(today))).FirstOrDefault();

        if(currSchedule == null)  // 만약 생산 공정이 없으면 모든 버튼을 비활성화
        {
            await Commons.ShowMessageAsync("공정", "공정계획이 없습니다. 계획일정을 먼저 입력하세요");
            // TODO : 시작 버튼 Disable(비활성화)
            LblProcessDate.Content = string.Empty;
            LblSchLoadTime.Content = "None";
            LblSchAmount.Content = "None";
            BtnStartProcess.IsEnabled = false;
            return;
        }
        else                     // 오늘 생산 공정이 있으면 모든 버튼을 활성화
        {
            // 공정계획 표시(날짜, 로드타임, 생산한 제품 개수)
            MessageBox.Show($"{today} 공정 시작합니다");
            LblProcessDate.Content = currSchedule.SchDate.ToString("yyyy년 MM월 dd일");
            LblSchLoadTime.Content = $"{currSchedule.LoadTime} 초";
            LblSchAmount.Content = $"{currSchedule.SchAmount} 개";
            BtnStartProcess.IsEnabled = true;

            UpdateData();
            InitConnectMqttBroker();   // 공정시작시 MQTT 브로커에 연결
        }
    }
    catch (Exception ex)
    {
        Commons.LOGGER.Error($"예외발생 ProcessView Loaded : {ex}");
        throw ex;
    }
}
```

<br>

```C#

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

MqttClient client;   // MqttClient 객체의 전역변수 선언
Timer timer = new Timer();
Stopwatch sw = new Stopwatch();

// 공정시작했을때 MQTT 브로커에 연결할 메서드
private void InitConnectMqttBroker()
{
    var brokerAddress = IPAddress.Parse(" MQTT 모스키토 브로커 IP");    // MQTT 연결할 IP 입력
    client = new MqttClient(brokerAddress);
    client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
    client.Connect("Monitor");
    client.Subscribe(new string[] { "factory1/machine1/data/" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

    timer.Enabled = true;
    timer.Interval = 1000;  // 1000ms -> 1sec
    timer.Elapsed += Timer_Elapsed;
    timer.Start();
}

// 칼라센서 데이터값에 따라 애니메이션 처리
private void Timer_Elapsed(object sender, ElapsedEventArgs e)
{
    if (sw.Elapsed.Seconds >= 2)  // 2초 대기 후 일처리
    {
        sw.Stop();
        sw.Reset();
        if(currentData["PRC_MSG"] == "OK")   // 만약 칼라센서가 초록색을 감지하면
        { 
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                Product.Fill = new SolidColorBrush(Colors.Green);  // 정사각형 색깔을 초록색으로 바꿔줌
            }));
        }
        else if(currentData["PRC_MSG"] == "FAIL")  // 만약 칼라센서가 빨간색을 감지하면
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                Product.Fill = new SolidColorBrush(Colors.Red);   // 정사각형 색깔을 빨간색으로 바꿔줌
            }));
        }

        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
        {
            UpdateData();
        }));
    }
}

Dictionary<string, string> currentData = new Dictionary<string, string>();

private void Client_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
{
    var message = Encoding.UTF8.GetString(e.Message);
    currentData = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);

    if (currentData["PRC_MSG"] == "OK" || currentData["PRC_MSG"] == "FAIL")
    {
        sw.Stop();
        sw.Reset();
        sw.Start();

        StartSensorAnimation();
    }


    StartSensorAnimation();
}
```

<br>

```C#
// 생산 프로세스를 실시간으로 업데이트(DB 연동)
 private void UpdateData()
{
    // 성공수량
    var prcOkAmount = Logic.DataAccess.GetProcesses().Where(p => p.SchIdx.Equals(currSchedule.SchIdx))
                          .Where(p => p.PrcResult.Equals(true)).Count();
    // 실패수량
    var prcFailAmount = Logic.DataAccess.GetProcesses().Where(p => p.SchIdx.Equals(currSchedule.SchIdx))
                          .Where(p => p.PrcResult.Equals(false)).Count();
    // 공정 성공률
    var prcOkRate = (double)prcOkAmount / (double)currSchedule.SchAmount * 100;
    
    // 공정 실패율
    var prcFailRate = (double)prcFailAmount / (double)currSchedule.SchAmount * 100;

    // 성공, 실패 수량과 비율을 출력
    LblPrcOkAmount.Content = $"{prcOkAmount} 개";
    LblPrcFailAmount.Content = $"{prcFailAmount} 개";
    LblPrcOkRate.Content = $"{prcOkRate.ToString("#.##")} %";  // 소수점 2자리까지 짜르기
    LblPrcFailRate.Content = $"{prcFailRate.ToString("#.##")} %";
}
```


<br>

------------

### ReportForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ReportForm.png" width = "80%" Height = "80%"> 

<br>

Live차트 라이브러리를 설치하여 Visualization을 진행했습니다.

```C#
// 차트 출력
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
```

<br>

------------

### SettingForm
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/SettingForm.png" width = "80%" Height = "80%">
- 공정계획과 CRUD 원리와 동일

<br>

<p align = "center">
</p>

<br>

------------

### DataAccess.cs
- 모든 View에 DB Query가 들어가는데, 따로 클래스로 묶어주어서 호출만 할 수 있도록 구현했습니다. 
- Entity Framework를 사용하여 쿼리를 조금 더 편리하게 다룰 수 있도록 하였습니다.
  - 기존의 C#과 SQL Server연동은 SqlConnection을 사용하여 직접 쿼리문을 작성하고, 각 Column에 해당되는 데이터를 따로 변수에 저장하여 컴포넌트에 옮기는 작업을 해주어야합니다.
  - Entity Framework는 리스트에 데이터를 담고, 클래스만 선언해서 ToList, Remove, AddOrUpdate로 쿼리문이 완료되고, 커밋만 해주면되어서 좀 더 편리하다고 볼 수 있습니다.

```C#
// SELECT
public static List<Settings> Get뷰이름()
{
    // 세팅 테이블에서 데이터 가져오기
    List<Model.뷰이름> list;

    using (var ctx = new MRPEntities())
        list = ctx.뷰이름.ToList();  // Settings에있는 DB 데이터를 가져와서 list에 넣어줌.  (SELECT)

    return list;  // DB데이터가 담긴 list를 반환
}

// INSERT, UPDATE
internal static int Set뷰이름(Settings item)  // object -> int로 바꿔줬음
{
    using (var ctx = new MRPEntities())
    {
        ctx.뷰이름.AddOrUpdate(item);  // Insert or Update = AddOrUpdate 임. (데이터 삽입)
        return ctx.SaveChanges();        // COMMIT
    }
}

// DELETE
internal static int Del뷰이름(뷰이름 item)
{
    using(var ctx = new MRPEntities())
    {
        var obj = ctx.뷰이름.Find(item.BasicCode);  // 삭제할 데이터를 검색해서 그 검색된 데이터를 삭제함
        ctx.뷰이름.Remove(obj);     // obj를 Delete = Remove (데이터 삭제)
        return ctx.SaveChanges();
    }
}
```
