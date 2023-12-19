[구글리의 깃허브에서 보기](https://github.com/joocfresh/joocBot/blob/joocfresh-patch-1/Help.md) 링크 클릭하고 구글리의 깃허브로 놀러와서 별주세요.

## 1.봇 명령어의 기본 구조는 다음과 같다.
```
@googlymoogly봇 /{커맨드} {파라미터}
@googlymoogly봇 /k EsaPromise
```

<img width="792" alt="image" src="https://github.com/joocfresh/joocBot/assets/15712519/cf38f5bd-8584-46f0-8cd5-a29486f0b367">

## 2.googlymoogly봇은 채널에서 멘션(@_id)과 동시에 커맨드를 입력해야 작동한다.

<img width="158" alt="image" src="https://github.com/joocfresh/joocBot/assets/15712519/545f45b0-70b2-4476-838b-22af6cf35702">

## 3.커맨드(command)라 함은 googlymoogly봇이 명령수행을 위한 지시어 이며 다음과 같은 명령어들이 있다. 동의어로 대체 가능.

- /테스트 : 구글리봇이 켜져 있는지 테스트 하는 용도. 말을 따라함.(킹받주의) [동의어: "/e","/echo","/에코"]
- /도움말: 도움말을 불러옵니다. [동의어: "하이구글리", "/h", "/help",]
- /패치노트": 패치노트를 불러옵니다. [동의어: "/p" "/patch", "/패치"]
- /킬로그: 제일 최근 킬이나 데스로그를 불러옴. [동의어: "/k" "/killlog", "/킬"]
- /구독: 봇소유자의 승인을 받은 채팅방에서 킬보드를 수신. [동의어: "/subscribe", "/구독취소"]
- /구독중지: 봇소유자의 승인을 받은 채팅방에서 킬보드를 수신중지. [동의어 "/unsubscribe", "/구독취소"]
- /디스밴드: 봇소유자의 승인을 받은 채팅방에서 킬보드인원 리스트 모두 삭제. [동의어 "/clear", "/disband", "/청소"]
- /제명: 봇소유자의 승인을 받은 채팅방에서 킬보드인원 리스트에서 지정 1명 삭제. (파라미터 없이 단독으로 사용 불가) [동의어 "/제거", "/delete"]
- /등록: 봇소유자의 승인을 받은 채팅방에서 킬보드인원 리스트를 볼 수 있음. [동의어 "/r", "/register"]
- /서버: 채팅봇이 수신할 Albion서버를 설정함. Default:동부서버. [동의어  "/server"]
- /채널: 현재 채팅방의 정보를 불러옴. [동의어 "/channel"]
- /검색: 유저를 검색함. 알비온에서 플레이어는 닉네임으로 관리되는게 아니라 고유ID로 관리되기 때문에 킬보드인원으로 추가위해서는 ID를 알아야함. [동의어 "/search"]

<img width="162" alt="image" src="https://github.com/joocfresh/joocBot/assets/15712519/9e870da6-1ef3-4103-9e59-1df007d17aee">

## 4. 파라미터(parameter)라 함은 명령을 수행하기 위한 구체적인 객체를 지정하는 식별자 이며, 주로 플레이어명, ID등을 사용 할 수 있다. 다음 커맨드 뒤에는 파라미터를 붙일 수 있다.  
- /등록 {플레이어명 Ex.Googlymoogly5404}
- /제명 {플레이어명 Ex.Googlymoogly5404}
- /킬로그 {플레이어명 Ex.Googlymoogly5404}
- /검색 {플레이어명 Ex.Googlymoogly5404}
- /에코 {아무말이나 적어보세요}
- /서버 (Private)
