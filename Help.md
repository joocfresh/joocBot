[구글리의 깃허브에서 보기](https://github.com/joocfresh/joocBot/blob/joocfresh-patch-1/Help.md) 링크 클릭하고 구글리의 깃허브로 놀러와서 별주세요.

## 1.봇 명령어의 기본 구조는 다음과 같다.
```
@googlymoogly봇 /{커맨드} {파라미터}
@googlymoogly봇 /k EsaPromise
```

<img width="792" alt="image" src="https://github.com/joocfresh/joocBot/assets/15712519/cf38f5bd-8584-46f0-8cd5-a29486f0b367">

## 2.googlymoogly봇은 채널에서 멘션(@_id)과 동시에 커맨드를 입력해야 작동한다.

<img width="158" alt="image" src="https://github.com/joocfresh/joocBot/assets/15712519/545f45b0-70b2-4476-838b-22af6cf35702">

## 3.커맨드(command)라 함은 googlymoogly봇이 명령수행을 위한 지시어 이며 다음과 같은 명령어들이 있다.

- case "e":case "echo":case "에코":case "테스트": 구글리봇이 잘 되는지 테스트 하는 용도. 말을 따라함.(킹받주의)
- case "h":case "help":case "도움말":case "하이구글리": 도움말을 불러옵니다.
- case "p":case "patch":case "패치": case"패치노트": 패치노트를 불러옵니다.
- case "k":case "killlog": case "킬": case "킬로그": 제일 최근 킬이나 데스로그를 불러옴.
- case "subscribe":case "구독": 봇소유자의 승인을 받은 채팅방에서 킬보드를 수신.
- case "unsubscribe": case "구독중지": case "구독취소": 봇소유자의 승인을 받은 채팅방에서 킬보드를 수신중지.
- case "clear": case "disband": case "청소": case "디스밴드": 봇소유자의 승인을 받은 채팅방에서 킬보드인원 리스트 모두 삭제.
- case "d": case "delete":case "제명": case"제거": 봇소유자의 승인을 받은 채팅방에서 킬보드인원 리스트에서 지정 1명 삭제. (파라미터 없이 단독으로 사용 불가)
- case "r": case "register": case "등록": 봇소유자의 승인을 받은 채팅방에서 킬보드인원 리스트를 볼 수 있음.
- case "server":case "서버": 채팅봇이 수신할 Albion서버를 설정함. Default:동부서버.
- case "channel": case "채널": 현재 채팅방의 정보를 불러옴.
- case "search":case "검색": 유저를 검색함. 알비온에서 플레이어는 닉네임으로 관리되는게 아니라 고유ID로 관리되기 때문에 킬보드인원으로 추가위해서는 ID를 알아야함.

<img width="162" alt="image" src="https://github.com/joocfresh/joocBot/assets/15712519/9e870da6-1ef3-4103-9e59-1df007d17aee">

## 4. 파라미터(parameter)라 함은 명령을 수행하기 위한 구체적인 객체를 지정하는 식별자 이며, 주로 플레이어명, ID등을 사용 할 수 있다. 다음 커맨드 뒤에는 파라미터를 붙일 수 있다.  
- case "r": case "register": case "등록": {플레이어명 Ex.Googlymoogly5404}
- case "d": case "delete":case "제명": case"제거": {플레이어명 Ex.Googlymoogly5404}
- case "k":case "killlog": case "킬": case "킬로그": {플레이어명 Ex.Googlymoogly5404}
- case "search":case "검색" {플레이어명 Ex.Googlymoogly5404}
- case "e":case "echo":case "에코": {아무말이나 적어보세요}
- case "server":case "서버": (Private)
