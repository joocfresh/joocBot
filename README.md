# joocBot
### 23년 - API연동
### 24년 - 킬보드 장비창 그래픽
### 25년 - 템창 그래픽 구현, 이미지크기 최적화
### 26년 - 아이템 이미지 온프레미스 캐싱으로 최적화(예정)

# Albion Killboard Discord Bot

Albion Online의 전투 이벤트(Killboard)를 수집하여  
킬보드 이미지 및 인벤토리 시각화 이미지를 생성하고  
Discord 채널에 자동 업로드하는 봇 애플리케이션입니다.

본 봇은 Albion Render 서버의 아이템 이미지를 활용하여  
가독성 높은 요약 이미지를 생성하며,  
로컬 캐싱을 통해 지연시간과 네트워크 부하를 최소화합니다.

---

## 주요 기능

- Albion Killboard 이벤트 수집
- 킬보드 이미지 자동 생성
  - Killer / Victim 정보
  - Total Fame
  - 장비 슬롯 시각화
- Victim 인벤토리 이미지 생성
  - 가로 9칸, 세로 가변(최대 6칸)
  - 최대 54개 아이템 렌더링
  - 아이템 미존재 시 `NO IMAGE` 플레이스홀더 표시
- 이미지 25% 축소 저장으로 스토리지 사용량 대폭 절감
- Discord CDN 업로드 자동화
- Albion Render 이미지 로컬 캐싱 지원

---

## 이미지 처리 특징

- Albion Render 서버 이미지 URL 기반 로딩
- 디스크 캐시 적용
  - 동일 URL 재요청 시 네트워크 호출 없이 즉시 로드
  - TTL 기반 만료 관리
- 병렬 이미지 다운로드 (`Task.WhenAll`)
- 고품질 리사이즈 (HighQualityBicubic)
- 격자 셀 배경 톤 적용 (UI 가독성 개선)

---

## 캐싱 구조

- 캐시 대상
  - Albion Render 아이템 이미지
  - 공통 UI 리소스 (gear, background, icon 등)
- 캐시 위치