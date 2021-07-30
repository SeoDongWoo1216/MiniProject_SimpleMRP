# Winforms(Connect UI) 구성
- DeviceSub의 Winforms UI는 Mqtt와 Visual Studio를 연동하고 Data Log를 출력합니다.
- 라즈베리파이 VS Code에서 machine01.py 소스파일을 실행한 다음 DeviceSubApp 화면을 통해 MQTT와 통신을 시작합니다.
- 칼라센서의 RED, GREEN 값에 따라 실패, 성공 문자열이 출력됩니다.
- 이때 성공, 실패했을때의 데이터값을 DB에 Update 합니다.


<br><br>

## 실행화면
<img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/DeviceSubApp_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif">

<br>

## 소스분석

### - connection string, client id, topic을 설정하여 MQTT 연결
```C#
/* MQTT Subscribe */
using uPLibrary.Networking.M2Mqtt;  // MQTT 라이브러리

client = new MqttClient(brokerAddress);
    client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

client.Connect(client_id);   // MQTT연결

client.Subscribe(new string[] { /* Topic */ },
            new byte[] { /* received_message */ });  // QoS 0

client.Disconnect();    // MQTT 연결 해제
```

<br><br>

### - MQTT를 통해 들어온 메세지를 문자열로 표시
```C#
 private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
{
    try
    {
        var message = Encoding.UTF8.GetString(e.Message);  // 그냥 메세지하면 안되므로 UTF-8 인코딩해줘야함
        UpdateText($">>>>> 받은 메세지 : {message}");

        // message(json) 을 C# 형태로 변환
        var currentData = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);   // 역직렬화
        PrcInputDataToList(currentData);

        // 메세지를 받은 이후에 처리
        sw.Stop();   // 스탑워치 스탑
        sw.Reset();  // 스탑워치를 다시 0으로 초기화
        sw.Start();  // 스탑워치를 다시 시작
    }
    catch (Exception ex)
    {
        UpdateText($">>>>> ERROR!! : {ex.Message}");
    }
}

List<Dictionary<string, string>> iotData = new List<Dictionary<string, string>>();   // iotData 라는 이름의 딕셔너리 전역 리스트 선언

// 라즈베리에서 들어온 메세지를 필터링해서 전역리스트에 입력하는 메서드
private void PrcInputDataToList(Dictionary<string, string> currentData)
{
    if(currentData["PRC_MSG"] == "OK" || currentData["PRC_MSG"] == "FAIL")  // OK, FAIL 을 받으면 리스트에 그대로 저장됨
    {
        iotData.Add(currentData);
    }
}
```

<br><br>

### - DB에 센싱값 Update
```C#
 // 실제 여러 데이터중 최종 데이터만 DB에 입력 : 센싱이 버튼 누르는 동안 계속 측정되기때문에 제일 마지막 데이터만 DB에 저장되게 구현
private void PrcCorrectDataToDB()
{
    if(iotData.Count > 0)
    {
        var correctData = iotData[iotData.Count - 1];   // 전역 리스트 iotData의 제일 마지막 값을 뽑아서 correctData에 넣어줌

        // DB에 입력
        using (var conn = new SqlConnection(connectionString))   
        {
            var prcResult = correctData["PRC_MSG"] == "OK" ? 1 : 0;   // 1이면 초록색(OK), 0이면 빨간색(FAIL)

            // 최종 센싱 데이터를 DB에 Update문으로 새로 넣어줌
            string strUpQry = $"UPDATE Process " +
                              $"   SET PrcResult = '{prcResult}'  " +
                              $"     , ModDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'  " +
                              $"     , ModID = '{"SYS"}'  " +
                              $" WHERE PrcIdx =  " +
                              $" (SELECT TOP 1 PrcIdx FROM Process ORDER BY PrcIdx DESC)";

            try
            {
                conn.Open(); 
                SqlCommand cmd = new SqlCommand(strUpQry, conn);
                if(cmd.ExecuteNonQuery() == 1)      // 1이면 성공
                {
                    UpdateText("[DB] 센싱값 Update 성공");
                }
                else                                // 1이 아니면 실패
                {
                    UpdateText("[DB] 센싱값 Update 실패");
                }
            }
            catch (Exception ex)
            {
                UpdateText($">>>> DB ERROR!! : {ex.Message}");
            }
        }
    }
    iotData.Clear();  // 데이터 모두 삭제 : 다시 새로 DB에 Update를 해줘야하니 전역 리스트의 모든 데이터를 없애줌
}
```
