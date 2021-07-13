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
