using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;  // Sql서버를 쓰기위한 커넥션 라이브러리
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;  // TCP/IP 라이브러리
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;  // Mqtt 라이브러리
using uPLibrary.Networking.M2Mqtt.Messages;

namespace DeviceSubApp
{
    public partial class FrmMain : Form
    {
        MqttClient client;       // MqttClient 라이브러리 타입의 전역변수 선언
        string connectionString; // DB연결할때 문자열 또는 Mqtt Broker Address로 사용
        ulong lineCount;
        delegate void UpdateTextCallback(string message);  // 스레드상 윈폼 RichText에 텍스트를 출력할때 사용

        Stopwatch sw = new Stopwatch();    // 스탑워치 생성

        public FrmMain()
        {
            InitializeComponent();
            InitializeAllData();  // 화면상의 데이터와 변수들을 초기화하는 메서드
        }

        private void InitializeAllData()
        {
            connectionString = "Data Source=" + TxtConnectionString.Text + ";Initial Catalog=MRP;" +
                "User ID=sa;Password=mssql_p@ssw0rd!";
           
            lineCount = 0;
            BtnConnect.Enabled = true;
            BtnDisconnect.Enabled = false;
            IPAddress brokerAddress;

            try
            {
                brokerAddress = IPAddress.Parse(TxtConnectionString.Text);  // Parse를 이용하여 문자열을 IPAddress 형으로 바꿔줌
                client = new MqttClient(brokerAddress);

                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            Timer.Enabled = true;
            Timer.Interval = 1000;  // 1000ms -> 1초(sec)
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            LblResult.Text = sw.Elapsed.Seconds.ToString();
            if(sw.Elapsed.Seconds >= 3)
            {
                sw.Stop();
                sw.Reset();
                // TODO 실제 처리 프로세스 실행
                //UpdateText("처리!!");
                PrcCorrectDataToDB();
            }
        }

        // 실제 여러 데이터중 최종 데이터만 DB에 입력
        private void PrcCorrectDataToDB()
        {
            if(iotData.Count > 0)
            {
                var correctData = iotData[iotData.Count - 1];   // 제일 마지막 값을 뽑아서 correctData에 넣어줌

                // DB에 입력
                //UpdateText("DB처리");  // 처리과정에 오류가 없으면 텍스트 출력
                using (var conn = new SqlConnection(connectionString))   
                {
                    var prcResult = correctData["PRC_MSG"] == "OK" ? 1 : 0;   // 1이면 초록색(OK), 0이면 빨간색(FAIL)

                    string strUpQry = $"UPDATE Process_DEV  " +
                                      $"   SET PrcEndTime = '{DateTime.Now.ToString("HH:mm:ss")}'" +
                                      $"     , PrcResult = '{prcResult}'" +
                                      $"     , ModDate = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'" +
                                      $"     , ModID = '{"SYS"}'" +
                                      $" WHERE PrcIdx =  " +
                                      $" (SELECT TOP 1 PrcIdx FROM Process_DEV ORDER BY PrcIdx DESC)";

                    try
                    {
                        conn.Open();  // 참고로 using을 사용했기때문에 따로 close를 하지않아도됨.
                        SqlCommand cmd = new SqlCommand(strUpQry, conn);
                        if(cmd.ExecuteNonQuery() == 1)   // 1이면 성공
                        {
                            UpdateText("[DB] 센싱값 Update 성공");
                        }
                        else                             // 1이 아니면 실패
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

            iotData.Clear();  // 데이터 모두 삭제
        }

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

        List<Dictionary<string, string>> iotData = new List<Dictionary<string, string>>();

        // 라즈베리에서 들어온 메세지를 필터링해서 전역리스트에 입력하는 메서드
        private void PrcInputDataToList(Dictionary<string, string> currentData)
        {
            if(currentData["PRC_MSG"] == "OK" || currentData["PRC_MSG"] == "FAIL")
                iotData.Add(currentData);
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            client.Connect(TxtClientID.Text);   // SUBSCR01
            UpdateText(">>>>> Client Connected");
            client.Subscribe(new string[] { TxtSubscriptionTopic.Text },
                new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE /*AT_MOST_ONCE는 한번보내는 QOS의 0을 뜻함(enum 처리되있음)*/ });
            UpdateText(">>>>> Subscribint to : " + TxtSubscriptionTopic.Text + "\n");

            BtnConnect.Enabled = false;
            BtnDisconnect.Enabled = true;
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            client.Disconnect();
            UpdateText(">>>>> Client Disconnected\n"); // 나 접속 끊었다고 알려줌

            BtnConnect.Enabled = true;
            BtnDisconnect.Enabled = false;
        }

        private void UpdateText(string message)
        {
            // 델리게이트랑 파라미터가 똑같아야함.(대리자랑 업데이트텍스트를 만들어서하면 문제없이 출력?됨)
            // 외우세요.
            if(RtbSubscr.InvokeRequired)   // Invoke??
            {
                UpdateTextCallback callback = new UpdateTextCallback(UpdateText);
                this.Invoke(callback, new object[] { message });
            }
            else
            {
                lineCount++;
                RtbSubscr.AppendText($"{lineCount} : {message}\n");
                RtbSubscr.ScrollToCaret();
            }
        }
    }
}
