# MiniProject_SimpleMRP
![MRP실행화면](https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ProcessView_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif)
- MES는 'Manufacturing Execution System'으로 오더 착수부터 제품 출하까지 전 생산활동을 관리하는 시스템으로 생산 현장에서 발생하는 데이터를 실시간으로 집계/분석/모니터링하는 시스템입니다.
- 이 프로그램에서는 공정의 품질검사 부분을 집계/모니터링합니다.(칼라센서를 통하여 구현)
- 품질검사는 칼라센서를통해 빨간색이면 불량(Fail), 초록색이면 정상으로 구분하여 구현하였습니다.
- 본 프로젝트는 라즈베리파이를 이용하여 센서링한 값을 MQTT 통신을 이용해 WPF와 DB를 연동하는 IoT 프로젝트입니다.


#### 준비물
- 라즈베리파이4
- 브레드보드
- Color 센서
- 스위치 2개
- 점퍼 케이블 다수

#### Tools
- Visual Studio
- SQL Server
- MQTT Explorer
- Visual Studio Code

<br>

### 프로세스 구성 
<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MRP%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8%EB%AA%A9%ED%91%9C01.png"  >
</p>

<br>

### TCS3200 컬러감지 센서 모듈 GY-31
- 칼라 센서를 활용해서 총 10개의 제품이 라인을 지나가는데 색깔별로 값을 매김(예를들어 초록은 1, 빨강은 fail)
<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/Colorsenser01.jpg" width="30%" height="30%" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/Colorsenser02.png" width="30%" height="30%" >
</p>

<p align = "center" >
(VCC와 GND는 두개인데 하나만 연결해도됨)
</p>

기본적인 로직은 S2와 S3의 값의 높낮이로 칼라값을 센싱한다.
- S2가 LOW / S3이 LOW이면 RED 값을 센싱
- S2가 HIGH / S3가 HIGH이면 GREEN 값을 센싱
- S2가 LOW / S3가 HIGH이면 BLUE 값을 센싱

<br><br>

### 회로도 구성
<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/%ED%9A%8C%EB%A1%9C%EB%8F%8401.png" width="45%" height="45%" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/%ED%9A%8C%EB%A1%9C%EB%8F%8402.png" width="45%" height="45%" >
</p>


<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/%ED%9A%8C%EB%A1%9C%EB%8F%8403.jpg" width="50%" height="50%" >
</p>


1. TCS3200의 VCC 하나의 Pin에 Pi 브레드보드 (+)에 연결
2. S1을 브레드보드 (+)와 연결
3. S0를 브레드보드 (-)와 연결
4. LED를 브레드보드 (+)와 연결
5. GND를 브레드보드 (-)에 연결
6. 스위치 한쪽 단자는 브레드보드의 +, 나머지 단자는 Pi의 VCC와 연결
7. 3, 5의 연결했던 -부분에 Pi의 GND를 연결<br>
---------------------- 이때 스위치를 눌렀을때 불이 들어와야한다. ------------------------- <br>
(칼라센서 위쪽(브레드보드에 안꼽은) 부분)
8. S3을 Pi의 GPIO24에 연결
9. S2을 Pi의GPIO23에 연결
10. OUT을 Pi의 GPIO25에 연결

<br>

### 칼라센서 실행 코드
```python
# machine01.py  소스코드
## 라이브러리 추가
import time
import datetime as dt
from typing import OrderedDict
import RPi.GPIO as GPIO
import paho.mqtt.client as mqtt
import json

s2 = 23 # Raspberry pi PIN 23
s3 = 24 # Raspberry pi PIN 24
out = 25 # Raspberry pi PIN 25
NUM_CYCLES = 10

dev_id = 'MACHINE01'
broker_address = '210.119.12.87'       # 브로커 주소 : 본인 컴퓨터의 IP
pub_topic = 'factory1/machine1/data/'  # 토픽

# Mosquito를 활용하여 MQTT방식으로 json 형태의 데이터를 전달
def send_data(param, red, green, blue):  # 누를때마다 데이터를 넘겨줌
    message = ''
    if param == 'GREEN':  # 녹색이면 OK
        message = 'OK'
    elif param == 'RED':  # 빨강이면 FAIL
        message = 'FAIL'
    elif param == 'CONN':
        message = 'CONNECTED'
    else:
        message = 'ERROR'

    # 날짜를 저장하면서 strftime으로 우리가 원하는 날짜로 표현해줌(년, 월, 일, 시, 분, 초, ms)
    currtime = dt.datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')  

    #json data generate
    raw_data = OrderedDict()
    raw_data['DEV_ID'] = dev_id 
    raw_data['PRC_TIME'] = currtime  # 시간에 지남에따라 데이터가 바뀜
    raw_data['PRC_MSG'] = message    # 조건문에 의해 반복적으로 데이터 바뀜
    raw_data['PARAM'] = param 
    raw_data['RED'] = red 
    raw_data['GREEN'] = green 
    raw_data['BLUE'] = blue 

    # publish 데이터 변환
    pub_data = json.dumps(raw_data, ensure_ascii = False, indent = '\t')  # json으로 반환
    print(pub_data)

    # mqtt_publish
    client2.publish(pub_topic, pub_data)  # 퍼블리쉬 함수에는 토픽을 보냄



def read_value(a2, a3):  # 값 2개를 받아서 처리할 함수(Low, High 값을 받음)
    GPIO.output(s2, a2)
    GPIO.output(s3, a3)
    # 센서 조정시간 설정

    time.sleep(0.3)

    start = time.time()  # 현재 시간
    for impule_count in range(NUM_CYCLES):
        GPIO.wait_for_edge(out, GPIO.FALLING)

    end = (time.time() - start)
    return NUM_CYCLES / end  # 색상결과 리턴


## GPIO 설정
def setup():
    GPIO.setmode(GPIO.BCM)
    GPIO.setup(s2, GPIO.OUT)  # 신호를 보내주므로 out
    GPIO.setup(s3, GPIO.OUT)
    GPIO.setup(out, GPIO.IN, pull_up_down = GPIO.PUD_UP)  # 센서결과 받기
    
## 반복하면서 일처리
def loop():  
    result = ''

    while True:
        red = read_value(GPIO.LOW, GPIO.LOW) # s2 LOW, s3 LOW
        time.sleep(0.1)  # 0.1초 딜레이
        green = read_value(GPIO.HIGH, GPIO.HIGH) # s2 HIHG, s3 HIHG
        time.sleep(0.1)
        blue = read_value(GPIO.LOW, GPIO.HIGH)
        
        print('red = {0}, green = {1}, blue = {2}'.format(red, green, blue))
        if(red < 50): continue  # 센서가 빨간색을 잘 못알아먹어서 코드로 오류 제어
        #if(red > 2000 or green > 2000 or  blue > 2000): continue

        if (red > green) and (red > blue):
            result = 'RED'
            send_data(result, red, green, blue)
        elif(green > red) and (green > blue):
            result = 'GREEN'
            send_data(result, red, green, blue)
        else:
            result = 'ERROR'

        
        time.sleep(1)

# MQTT 초기화
client2 = mqtt.Client(dev_id)   # 그냥 client는 import를 추가하는 등의 얽혀있는게 많아서 client2로 선언
client2.connect(broker_address) # 브로커가 서버를 접속할 수 있게 해줌
print('MQTT Client connected')  # 접속이 잘 됬는지 확인용 print를 콘솔에 출력

if __name__ == '__main__':      # 우리가 아는 메인함수
    setup()
    send_data('CONN', None, None, None)   # 접속 시작 이후에 MQTT에 접속 성공 메세지 전달
    # None은 NULL과 같음

    try:
        loop()
    except KeyboardInterrupt:   # 오류발생하면 잡히는 catch문과 같음
        GPIO.cleanup()
```

<br>

- 위의 이미지처럼 회로를 구성하고, 라즈베리파이에 위의 코드를 실행해줍니다. <br>
- 이때 스위치를 누르면 칼라센서에있는 LED가 켜지면서 센서가 작동되는데, 이때 빨간색이나 초록색 물체를 갖다댄 상태로 켜주면 센서가 감지됩니다.

<p align = "center">
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/%EC%84%BC%EC%84%9C%EC%9E%91%EB%8F%99.gif">
</p>

<p align = "center">
  (스위치를 눌렀을때 센서 작동)
</p>

<p align = "center">
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/machine01%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.png">
</p>

<br>

<p align = "center">
machine01.py 실행화면 <br>
(물체의 색깔에따라 red, green, blue의 값이 확 뛰는 것을 확인할 수 있다)
</p>
<br>

### Data Transfer 구성
- 데이터를 터미널(IoT, 임베디드)인 클라이언트에서 데이터를 수집할 서버로 전달하기위해 MQTT를 사용
- MQTT 특징
  - 클라이언트 : MQTT Broker에 연결되는 모든 것
  - Broker : 모든 메시지를 수신, 필터링, 메시지 구독하는 클라이언트 결정, 메시지 보내는 역할을 하는 중간 매개체
  - Publish : Topic을 지정, Topic을 Subscribe하고 있는 클라이언트에게 메시지를 전달
  - Subscribe : Topic을 구독, Topic으로 Publish된 메시지를 수신

#### MQTT 라이브러리 paho-mqtt 설치
```Python
pip install paho-mqtt
```

<br>

<p align = "center">
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MQTT_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.png">
</p>

<p align = "center">
 라즈베리파이의 machine01.py를 실행한 후에 센서값을 감지하면 MQTT Explorer에 센서값이 전달된다
</p>


<br><br>

### DB 물리 설계
<p align = "center" >
 <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Query/DB_Diagram.PNG">
</p>

- Process는 ProcessView에서 처리할 실시간 공정과정을 저장하는 테이블 
- Settings는 공통 코드 관리 테이블
- Schedules는 공정 계획 테이블

<br><br>

## UI 구성(WPF, Winforms)

[★Winforms 소스코드 분석★](https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/tree/main/DeviceSubApp)

<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/DeviceSub%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif" width="80%" height="80%" >
</p>

<p align = "center" >
DeviceSubApp 실행화면
</p>

<br>

[☆WPF 소스코드 분석☆](https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/tree/main/MRPApp)

<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MRP_UI_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif" >
</p>

<p align = "center" >
MRPApp 실행화면
</p>

<br>


## 실행화면

### - 공정계획

<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/Schedules%EC%9B%80%EC%A7%A4.gif">
</p>

<br>

<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/Schedules_%EA%B2%BD%EA%B3%A0%EC%9B%80%EC%A7%A4.gif">
</p>
  
<p align = "center" >
각 컴포넌트가 빈값이면 입력이나 수정이 되지않도록 유효성 검사를 통해 입력할 수 있도록 오류 제어
</p>

<br>

### - 실시간 공정관리

<p align = "center" >
  <img src ="https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ProcessView_%EC%8B%A4%ED%96%89%ED%99%94%EB%A9%B4.gif">
</p>

- 라즈베리파이, 윈폼에서 실행하여 통신망을 구축하고, ProcessView에서 모니터링을 진행합니다. <br>
- 이때 칼라센서에 물체를 놔두고 작동시키면 빨간색, 초록색에따라 애니메이션 정사각형 그림이 색깔이 변하게되고, 성공/실패 수량이 count됩니다.

<br>

### - 레포트(Visualization)

<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/ReportView%EC%9B%80%EC%A7%A4.gif">
</p>

<p align = "center" >
Live Chart 라이브러리를 활용하여 Visualization
</p>

<br>

### - 공통코드관리

<p align = "center">
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/Setting%EC%9B%80%EC%A7%A4.gif">
</p>

<p align = "center">
  그리드의 값을 CRUD를 통해 데이터를 관리할 수 있도록 구현
</p>

<br>


---------------------

