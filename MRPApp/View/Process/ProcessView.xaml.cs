﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MRPApp.View.Process
{
    /// <summary>
    /// ProcessView.xaml에 대한 상호 작용 논리
    /// 1. 공정계획에서 오늘의 생산 계획 일정을 불러오기
    /// 2. 공정이 없으면 에러표시, 시작 버튼을 클릭하지 못하게 만듬
    /// 3. 공정이 있으면 오늘의 날짜를 표시, 시작 버튼 활성화
    /// 3-1 MQTT Subscription 연결. factory1/machine1/data 확인...
    /// 4. 시작 버튼 클릭시 새 공정 생성, DB에 입력
    ///   공정코드 : PRC20210618001 (PRC + yyyy + MM + dd + NNN)
    /// 5. 시작 버튼을 클릭했을때 공정처리 애니메이션 시작
    /// 6. 로드타임 후에는 애니메이션 중지
    /// 7. 센서링값 리턴될때까지 대기
    /// 8. 센서링 결과값에 따라서 생산품 색상 변경
    /// 9. 현재 공정의 DB값 업데이트
    /// 10. 결과 레이블 값 수정/표시
    /// </summary>
    public partial class ProcessView : Page
    {
        // 금일 일정
        private Model.Schedules currSchedule;
        public ProcessView()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
           
            try
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");

                currSchedule = Logic.DataAccess.GetSchedules().Where(s => s.PlantCode.Equals(Commons.PLANTCODE))
                    .Where(s => s.SchDate.Equals(DateTime.Parse(today))).FirstOrDefault();

                if(currSchedule == null)
                {
                    await Commons.ShowMessageAsync("공정", "공정계획이 없습니다. 계획일정을 먼저 입력하세요");
                    // TODO : 시작 버튼 Disable(비활성화)
                    LblProcessDate.Content = string.Empty;
                    LblSchLoadTime.Content = "None";
                    LblSchAmount.Content = "None";
                    BtnStartProcess.IsEnabled = false;
                    return;
                }
                else
                {
                    // 공정계획 표시
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

        
        // 공정시작했을때 MQTT 브로커에 연결할 메서드
        MqttClient client;   // 전역변수 선언
        Timer timer = new Timer();
        Stopwatch sw = new Stopwatch();
        private void InitConnectMqttBroker()
        {
            var brokerAddress = IPAddress.Parse("210.119.12.87");  // MQTT 모스키토 브로커 IP
            client = new MqttClient(brokerAddress);
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.Connect("Monitor");
            client.Subscribe(new string[] { "factory1/machine1/data/" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });

            timer.Enabled = true;
            timer.Interval = 1000;  // 1000ms -> 1sec
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sw.Elapsed.Seconds >= 2)  // 2초 대기 후 일처리
            {
                sw.Stop();
                sw.Reset();
                //MessageBox.Show(currentData["PRC_MSG"]);
                if(currentData["PRC_MSG"] == "OK")
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        Product.Fill = new SolidColorBrush(Colors.Green);
                    }));
                }
                else if(currentData["PRC_MSG"] == "FAIL")
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        Product.Fill = new SolidColorBrush(Colors.Red);
                    }));
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    UpdateData();
                }));
            }
        }

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

            LblPrcOkAmount.Content = $"{prcOkAmount} 개";
            LblPrcFailAmount.Content = $"{prcFailAmount} 개";
            LblPrcOkRate.Content = $"{prcOkRate} %";
            LblPrcFailRate.Content = $"{prcFailRate} %";
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

        private void StartSensorAnimation()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                DoubleAnimation ba = new DoubleAnimation();
                ba.From = 1;  // 최초 켜짐(이미지 보임)
                ba.To = 0;    // 이미지 보이지 않음
                ba.Duration = TimeSpan.FromSeconds(2);
                ba.AutoReverse = true;
                //ba.RepeatBehavior = RepeatBehavior.Forever;  // 계속 도는거

                Sensor.BeginAnimation(Canvas.OpacityProperty, ba);
            }));
        }

        private void BtnStartProcess_Click(object sender, RoutedEventArgs e)
        {
            if(InsertProcessData())
                StartAnimation();    // HMI 애니메이션 실행
        }

        private bool InsertProcessData()
        {
            var item = new Model.Process();
            item.SchIdx = currSchedule.SchIdx;
            item.PrcCD = GetProcessCodeFromDB();
            item.PrcDate = DateTime.Now;
            item.PrcLoadTime = currSchedule.LoadTime;
            item.PrcStartTime = currSchedule.SchStartTime;
            item.PrcEndTime = currSchedule.SchEndTime;
            item.PrcFacilityID = Commons.FACILITYID;
            item.PrcResult = true;   // 공정성공으로 일단 픽스하고 시작
            item.RegDate = DateTime.Now;
            item.RegID = "MRP";

            try
            {
                var result = Logic.DataAccess.SetProcesses(item);
                if(result == 0)
                {
                    Commons.LOGGER.Error("공정데이터 입력 실패");
                    Commons.ShowMessageAsync("오류", "공정시작 오류 발생, 관리자 문의");
                    return false;
                }
                else
                {
                    Commons.LOGGER.Info("공정데이터 입력!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Commons.LOGGER.Error($"예외발생 : {ex}");
                Commons.ShowMessageAsync("오류", "공정시작 오류 발생, 관리자 문의");
                return false;
            }
        }

        private string GetProcessCodeFromDB()
        {
            var prefix = "PRC";
            var prePrcCode = prefix + DateTime.Now.ToString("yyyyMMdd");  // "PRC20210629" 가 저장됨
            var resultCode = string.Empty;

            // 이전까지의 공정이 없으면(PRC20210629... => ...에 001, 002 같은 데이터가 없으면) null이 넘어오고
            // PRC20210629001, 002, 003, 004 가 있으면 PRC20210629004 이 넘어감.
            var maxPrc = Logic.DataAccess.GetProcesses().Where(p => p.PrcCD.Contains(prePrcCode))
                         .OrderByDescending(p => p.PrcCD).FirstOrDefault();
            
            if(maxPrc == null)
            {
                resultCode = prePrcCode + "001";
            }
            else
            {
                var maxPrcCd = maxPrc.PrcCD;  // PRC20210629004
                var maxVal = int.Parse(maxPrcCd.Substring(11)) + 1;  // (004를 4로 바꿔주고) + 1을 해줌

                resultCode = prePrcCode + maxVal.ToString("000");  // 최대공정코드 + 1 값
            }

            return resultCode;
        }

        private void StartAnimation()
        {
            Product.Fill = new SolidColorBrush(Colors.Gray);

            // 기어 애니메이션 속성(기어 돌리기)
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 360;
            da.Duration = TimeSpan.FromSeconds(currSchedule.LoadTime);  // 일정 계획(스케줄)의 로드타임 (만큼 느리게 돌게해줌)
            //da.RepeatBehavior = RepeatBehavior.Forever;

            RotateTransform rt = new RotateTransform();
            Gear1.RenderTransform = rt;
            Gear1.RenderTransformOrigin = new Point(0.5, 0.5);  // 정중앙으로 돌려라
            Gear2.RenderTransform = rt;
            Gear2.RenderTransformOrigin = new Point(0.5, 0.5);

            rt.BeginAnimation(RotateTransform.AngleProperty, da);

            // Product(네모) 움직이기(옆으로 움직이기)
            DoubleAnimation ma = new DoubleAnimation();
            ma.From = 151;
            ma.To = 545;   // 옮겨지는 x값의 최대값
            ma.Duration = TimeSpan.FromSeconds(currSchedule.LoadTime);
            //ma.AccelerationRatio = 0.5;
            //ma.AutoReverse = false;

            Product.BeginAnimation(Canvas.LeftProperty, ma);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // 자원해제
            if (client.IsConnected) client.Disconnect();
            timer.Dispose();
        }
    }
}
