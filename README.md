# MiniProject_SimpleMRP
#### 준비물
- 라즈베리파이4
- 브레드보드
- Color 센서
- 스위치 2개
- 점퍼 케이블 다수

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

### 목표 
<p align = "center" >
  <img src = "https://github.com/SeoDongWoo1216/MiniProject_SimpleMRP/blob/main/Image/MRP%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8%EB%AA%A9%ED%91%9C01.png"  >
</p>

