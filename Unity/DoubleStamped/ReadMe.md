<div align="center">
  <h1>문서 감정 · 낮/밤 루프 코어</h1>
  <p><em>Interfaces · State Machine · Presenter(UI) · ScriptableObject 데이터 구동 · Repository/Service(Singleton)</em></p>
</div>

<hr/>

<section>
  <h2>파일별 역할 한 줄 요약</h2>
  <ul>
    <li><strong><code>GameLoopManager</code></strong>: 전역 <em>상태머신</em>(Start/Day/Night/GameOver/GameClear) · 씬 전환/타임스케일/일자 카운트.</li>
    <li><strong><code>DayGameLoopManager</code> / <code>IDayGameLoopManager</code></strong>: 낮 파트 <em>오케스트레이션</em>(플레이어 데이터→UI 바인딩, 판정 집계, 결과 산출·종료).</li>
    <li><strong><code>DocumentManager</code> / <code>IDocumentManager</code></strong>: 난이도 기반 <em>문서 생성 팩토리</em> · 본문/보조 위조 분기 · 현재 문서 캐시/정리.</li>
    <li><strong><code>UIDocumentDisplayManager</code></strong>: 문서 데이터→UI 바인딩(본문/보조 목록 출력) · 표시 토글.</li>
    <li><strong><code>UiSupportDocDisplay</code></strong>: 보조문서 <em>캐러셀</em>(이전/다음) · 패널 활성화 제어.</li>
    <li><strong><code>JudgmentManager</code></strong>: 판정 버튼 이벤트→낮 루프에 <em>위임</em>(승인/거절).</li>
    <li><strong><code>CodexOverlayUIManager</code></strong>: 코덱스 오버레이(매뉴얼/시그니처·직인/아티팩트 목록·상세) <em>패널 전환</em> · 목록 렌더.</li>
    <li><strong><code>ArtifactListItemUI</code></strong>: 아티팩트 목록 단일 항목 바인딩 + 클릭 콜백.</li>
    <li><strong><code>SignatureSlotUI</code></strong>: 서명 슬롯(아이콘/라벨/X 오버레이) 바인딩.</li>
    <li><strong><code>DataManager</code> / <code>IDataManager</code></strong>: <em>Repository/Service</em> — 아티팩트/시그니처/직인/블랙리스트 쿼리 허브.</li>
    <li><strong><code>ResourceManager</code></strong>: 런타임 자원(money/fame)·<em>난이도 산출</em>(fame/1000) · 음수 가드→GameOver.</li>
  </ul>
</section>

<details open>
  <summary><strong>포트폴리오 핵심 적용 기술 &amp; 의도</strong></summary>
  <ul>
    <li><strong>인터페이스 중심 설계</strong> — 계약 분리로 테스트/교체 용이(<code>IDayGameLoopManager</code>, <code>IDocumentManager</code>, <code>IDataManager</code>).</li>
    <li><strong>상태머신(State Machine)</strong> — <code>GameLoopManager</code>에서 씬 전환·일자 카운트·타임스케일을 <em>단일 진입점</em>으로 관리.</li>
    <li><strong>Presenter 패턴(UI)</strong> — 데이터 바인딩과 표시 토글을 <code>UIDocumentDisplayManager</code>/<code>CodexOverlayUIManager</code> 등으로 <em>역할 분리</em>.</li>
    <li><strong>ScriptableObject 데이터 구동</strong> — <code>ArtifactDatabase</code>, <code>ArtifactTypeData</code>, <code>SupportDocumentData</code> 기반으로 <em>디자이너 친화</em> 파이프라인.</li>
    <li><strong>Repository/Service(Singleton)</strong> — <code>DataManager</code>/<code>ResourceManager</code>로 룰/자원 조회를 집약해 <em>중복 로직 제거</em>.</li>
    <li><strong>세션 블랙리스트 정책</strong> — 시그니처 5~8개 무작위 선택 → 낮 파트 문서·코덱스에 <em>일관 적용</em>.</li>
    <li><strong>위조 분기 설계</strong> — 본문·보조문서·양쪽 위조를 난수·난이도에 연동, <em>검증 포인트</em>(서명·직인·연도·가치·아이콘) 제어.</li>
  </ul>
</details>

<details>
  <summary><strong>실행 플로우(요약)</strong></summary>
  <ol>
    <li><code>GameLoopManager</code> → <em>GameStart</em> 상태, <code>StartScene</code> 로드 &amp; 자원 리셋.</li>
    <li><em>Day</em> 전이 시 <code>DayScene</code> 로드 → <code>DayGameLoopManager.InitScene()</code>가 플레이어 데이터/난이도/문서를 초기 바인딩.</li>
    <li>플레이어 판정(승인/거절) → <code>DayGameLoopManager.OnJudgmentSubmitted</code>가 정오 판정 누계 및 다음 문서 생성.</li>
    <li>코덱스 오버레이에서 블랙리스트/시그니처·직인/아티팩트 목록 탐색.</li>
    <li>상태 전이(예: <em>DayClear</em>, <em>Playing_Night</em>, <em>GameOver</em>)에 따라 씬 전환 및 결과 수집.</li>
  </ol>
</details>

<details>
  <summary><strong>빠른 실행 가이드</strong></summary>
  <ol>
    <li>씬 구성: <code>StartScene</code>, <code>DayScene</code>, <code>NightScene</code>.</li>
    <li>전역 오브젝트:
      <ul>
        <li><code>GameLoopManager</code>(DontDestroyOnLoad), <code>ResourceManager</code>, <code>DataManager</code>(<code>ArtifactDatabase</code> 참조 연결).</li>
      </ul>
    </li>
    <li>Day 씬:
      <ul>
        <li><code>DayGameLoopManager</code>, <code>DocumentManager</code>, <code>UIDocumentDisplayManager</code>, <code>UiSupportDocDisplay</code>, <code>JudgmentManager</code>, <code>CodexOverlayUIManager</code> 배치 및 레퍼런스 연결.</li>
      </ul>
    </li>
    <li>플레이: 시작 시 <em>StartScene</em> → <em>Day</em> 전이. (디버그: <kbd>N</kbd> → Night 전환)</li>
  </ol>
</details>

<details>
  <summary><strong>데이터 모델(SO) · 외부 의존</strong></summary>
  <ul>
    <li><strong>ScriptableObjects</strong>: <code>ArtifactDatabase</code>(<code>artifactTypes</code>, <code>signatureList</code>, <code>sealList</code>), <code>SupportDocumentData</code>.</li>
    <li><strong>런타임 데이터</strong>: <code>PlayerData</code>(money, fame, difficulty), <code>DayResultData</code>/<code>NightResultData</code>, <code>DocumentData</code>.</li>
    <li><strong>외부 모듈</strong>: <code>TimeManager</code>, <code>GameSignals</code>(GameOver) — 프로젝트 공용 모듈과 연동.</li>
  </ul>
</details>

<details>
  <summary><strong>알려진 한계 &amp; 후속 계획(요약)</strong></summary>
  <ul>
    <li>UI 슬롯 범위: 보조문서 수 &gt; 슬롯 수일 때 OOB 가능(<code>UIDocumentDisplayManager</code>, <code>UiSupportDocDisplay</code>) → <em>동적 생성/풀링</em>으로 개선.</li>
    <li>데이터 정합: <code>signatureList</code> ↔ <code>sealList</code> 길이 불일치 시 인덱스 오류 가능 → <em>유효성 검증</em> 추가.</li>
    <li>중앙값 계산: 위조 로직 일부의 mid 계산 <em>FIXME</em> 반영 필요.</li>
    <li>의존 해소: 씬 이름 기반 <code>Find</code> 의존(<code>GameLoopManager</code>) → <em>DI/참조 주입</em>으로 내구성 강화.</li>
    <li>Dictionary 직렬화: <code>ResourceManager</code>의 <code>resources</code>는 인스펙터 비표시 → 에디터 툴/디버그 뷰 고려.</li>
  </ul>
</details>

<hr/>

<div align="center">
  <sub>작게 시작해도, <strong>계약과 상태</strong>를 중심에 두면 확장에 강해집니다. — 인터페이스·상태머신·SO 데이터로 현업 친화 구조를 보여줍니다.</sub>
</div>
