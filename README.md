# 제스처 인식을 통한 악기 앱 - GyroPad v1.0
-----
<img src=https://user-images.githubusercontent.com/39721769/84844142-bf4eb700-b084-11ea-9899-d2eed713b646.jpg width=300 height=600>

-----

## 목차

### [1. 개요](#개요)
### [2. 설치](#설치)
### [3. 기능](#기능)
### [4. 사용 방법](#사용-방법)
### [5.Default 제스처](#default-제스처)
   1) Square
   2) Star
   3) Up Down
   4) Tilt Right
   5) Tilt Left
   6) Circle
   7) Check
### [6.Support](#support)
### [7.Contribution guidelines](#contribution-guidelines)
*****



## [개요](#목차)

핸드폰의 가속도센서를 이용하여 런치패드를 구현하는 오픈소스 소프트웨어 어플리케이션이다. 

핸드폰을 쥐고 다양한 제스처를 사용하면 매핑된 음악이 흘러나온다.  

원하는 제스처를 머신러닝을 이용해서 학습, 등록해서 원하는 음악을 재생할 수 있으며 

저장/불러오기로 상황마다 다양한 제스처 세트를 바꿔서 사용할 수 있다.

핸드폰 자체의 좌표계와 중력가속도를 이용한 월드 좌표계를 인식하여 다양한 제스처를 인식할 수 있으며 

데이터의 학습에는 선형보간을 이용해 제스처 인식률을 향상하였다.

본 소프트웨어는 유니티, C#을 이용해 빌드되었다.

------


## [설치](#목차)

Gyropad.apk 파일을 다운로드 해 안드로이드 스마트폰에 설치한다.  
그 후 사용 방법의 설명에 따라 사용한다.
   
-------
## [기능](#목차)
- 초기에 설정된 제스처 및 사용자가 임의로 제스처를 설정해 제스처에 맞는 음악을 재생한다.

- Default 제스처로 Square, Star, Up Down, Tilt Right, Tilt Left, Circle, Check 가 제공된다.

- Default 제스처 및 임의의 제스처를 설정한 후 Train 버튼을 통해 학습할 수 있고, 학습이 완료된 후 데이터를 Save, Load 할 수 있다.

- 현재 넣을 수 있는 기본 사운드로는 다음과 같다.

  7음계 - 도,레,미,파,솔,라,시,도_
  
  BASS A2
  
  BASS C3
  
  Crash Sym
  
  Drum Set
  
  Ending Kick
  
  GW C2
  
  Growl D2 1
  
  Growl D2 2
  
  MIX BASS
  
  Wobble A2
  
  Wobble D3
  
  Wobble F3
  

-----
## [사용 방법](#목차)

1. 왼쪽 상단의 edit버튼을 눌러 제스처 목록을 확인한다. 

   (default 제스처를 사용하고 싶으면 Load를 이용해 제스처를 불러온 다음 edit버튼으로 들어간다.)
<img src=https://user-images.githubusercontent.com/39721769/84846012-d68fa380-b088-11ea-825c-80d1cf9f9bfc.jpg width=200 height=400>

2. Add버튼으로 새로운 제스처를 추가하고 +버튼으로 오디오를 제스처에 매핑한다. 그 후 체크 버튼을 누른다.
<img src=https://user-images.githubusercontent.com/39721769/84847561-66831c80-b08c-11ea-8221-5aca4ac4334c.jpg width=200 height=400>

3. perform버튼을 누른 채로 핸드폰을 움직여서 원하는 제스처를 행동한 후 손을 떼서 적용한다. 최소 20회 이상 반복하고, 완료되었으면 train 버튼을 눌러 학습시킨다.
<img src=https://user-images.githubusercontent.com/39721769/84845479-b7dcdd00-b087-11ea-8a0b-43a8d563fc2b.jpg width=200 height=400>

4. train이 다 될때까지 기다린다.
<img src=https://user-images.githubusercontent.com/39721769/84845581-e0fd6d80-b087-11ea-8a5c-79a9bfe1379d.jpg width=200 height=400>

5. 1~4과정을 반복해 원하는만큼 제스처를 만들고 save버튼을 눌러 저장한다.
<img src=https://user-images.githubusercontent.com/39721769/84845686-2457dc00-b088-11ea-9867-9001639a02b1.jpg width=200 height=400>

6. play버튼을 눌러 해당 제스처를 인식하는지 확인해본다.
<img src=https://user-images.githubusercontent.com/39721769/84845742-42254100-b088-11ea-9815-8bf195b46c68.jpg width=200 height=400>

-----

## [Default 제스처](#목차)

1. Square
   
<img src="https://user-images.githubusercontent.com/55373167/83945941-cab20f00-a848-11ea-8277-1d84cafda4f4.png" width="50%">    

2. Star    
<img src="https://user-images.githubusercontent.com/55373167/83945943-cab20f00-a848-11ea-9ec1-cf94876987aa.png" width="50%">    

3. Up Down    
<img src="https://user-images.githubusercontent.com/55373167/83945944-cb4aa580-a848-11ea-9948-1dce6e88e323.png" width="15%">    

4. Tilt Right    
<img src="https://user-images.githubusercontent.com/55373167/83945945-cbe33c00-a848-11ea-930f-541afb7a6231.png" width="40%">    

5. Tilt Left    
<img src="https://user-images.githubusercontent.com/55373167/83945936-c8e84b80-a848-11ea-98de-aa541f7f3de2.png" width="40%">    

6. Circle    
<img src="https://user-images.githubusercontent.com/55373167/83945937-c980e200-a848-11ea-99f5-4ad24a346513.png" width="50%">    

7. Check    
<img src="https://user-images.githubusercontent.com/55373167/83945940-ca197880-a848-11ea-97e9-582d38761e8f.png" width="50%">    

------

## [Support](#목차)

commit하기 전에 가이드라인을 봐주시길 바랍니다. 
본 프로젝트에 error가 존재할 경우, 개선 사항이 있을 경우 issue, pull request를 통해 게시해주시면 최대한 빠르게 답변해드리겠습니다.


------
## [Contribution guidelines](#목차)

C#에 대한 코드 수정은 [microsoft c# programming guide](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/)를 참고해주시길 바랍니다.

본 프로젝트에 참여하는 것으로 라이센스와 명시된 사항에 동의하는 것으로 간주합니다.

