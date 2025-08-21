<div align="center">
  <h1>EchoBetweenUs</h1>
  <p><em>Interfaces · Strategy · Factory+Pooling · Event Channel · Update/FixedUpdate 분리</em></p>
</div>

<hr/>

<section>
  <h2>파일별 역할 한 줄 요약</h2>
  <ul>
    <li><strong><code>IMapManager</code> / <code>MapManager</code></strong>: 맵의 <strong>가로 경계</strong>, <strong>카메라 상단 기준</strong>, <strong>디스폰 Y</strong>, <strong>최고 스폰 높이</strong> 관리 + 진행(스크롤·점수) 오케스트레이션.</li>
    <li><strong><code>ICameraMover</code> / <code>CameraMover</code></strong>: 뷰포트 Y 임계 기반 <strong>카메라 추적/해제</strong> 전략 + <strong>게임오버선/벽</strong> 생성.</li>
    <li><strong><code>IPlatformFactory</code> / <code>PlatformFactory</code></strong>: 플랫폼 <strong>생성 + 오브젝트 풀</strong>(재활성/회수), <strong>초기 상태 리셋</strong> 훅 호출.</li>
    <li><strong><code>PlatformSpawner</code></strong>: 카메라 상단 근접 시 <strong>2~3개 등분 스폰</strong>, 난이도 기반 <strong>세로 간격 조정</strong>, <strong>장애물/광원</strong> 배치, 하단 <strong>디스폰 회수</strong>.</li>
    <li><strong><code>PlatformPhysics</code></strong>: <code>BoxCollider2D</code> + <code>PlatformEffector2D</code> 구성, 스프라이트-콜라이더 <strong>동기화</strong>, <strong>에코 트리거</strong> 생성.</li>
    <li><strong><code>PlatformInitializer</code></strong>: (옵션) <strong>폭 랜덤화</strong> → 물리/콜라이더 <strong>재동기화</strong> 트리거.</li>
    <li><strong><code>playerCtrl</code></strong>: <strong>입력(Update)</strong> / <strong>물리(FixedUpdate)</strong> 분리, <strong>차지 점프</strong>, <strong>넉백</strong>, 보행 <strong>SFX/애니</strong> 동기화.</li>
    <li><strong><code>ObstacleBehavior</code></strong>: 플레이어 충돌 시 <strong>넉백 임펄스 + SFX + 신호</strong> 발행.</li>
    <li><strong><code>KnockbackSignal</code></strong>: 전역 <strong>이벤트 채널</strong>(플레이어 넉백 알림).</li>
    <li><strong><code>DifficultyManager</code></strong>: 진행 높이 → <strong>난이도 레벨</strong>(선형 스텝).</li>
    <li><strong><code>DeadLineTrigger</code></strong>: 하단 트리거 <strong>진입/잔류 시 Game Over</strong>.</li>
  </ul>
</section>

<details open>
  <summary><strong>포트폴리오 가치가 높은 적용 기술 &amp; 의도</strong></summary>
  <ul>
    <li><strong>인터페이스 중심 설계</strong> — <em>교체/테스트 용이성</em> · 적용: <code>IMapManager</code>, <code>ICameraMover</code>, <code>IPlatformFactory</code></li>
    <li><strong>Strategy 패턴(카메라 이동)</strong> — <em>상황별 추적/해제 분리, 임계 기반 전환</em> · 적용: <code>CameraMover</code> (뷰포트 0.75 시작 / 0.6 해제)</li>
    <li><strong>Factory + Object Pool</strong> — <em>프레임 스파이크·GC 최소화</em> · 적용: <code>PlatformFactory.Create/Recycle</code> + <code>PlatformSpawner.Cleanup</code></li>
    <li><strong>Event Channel</strong> — <em>결합도 완화</em> · 적용: <code>KnockbackSignal</code> (Obstacle → playerCtrl)</li>
    <li><strong>입력/물리 프레임 분리</strong> — <em>일관된 물리와 반응성</em> · 적용: <code>playerCtrl.Update</code> ↔ <code>FixedUpdate</code></li>
    <li><strong>맵 경계·디스폰 계산</strong> — <em>객체 수명 안정화</em> · 적용: <code>MapManager.GetHorizontalBounds()</code> / <code>GetDespawnY()</code></li>
    <li><strong>Physics 구성 자동화</strong> — <em>프리팹 일관성</em> · 적용: <code>PlatformPhysics</code> (Effector/Collider/Echo 동기화)</li>
    <li><strong>난이도 곡선 분리</strong> — <em>진행·스폰·속도 연동</em> · 적용: <code>DifficultyManager</code> (height→level), 스폰 간격/확률 가변</li>
  </ul>
</details>

<details>
  <summary><strong>구현 의도 요약</strong></summary>
  <ul>
    <li><strong>작게 시작, 확장 유리</strong> — 코어 루프(스폰→진행→디스폰)를 인터페이스로 감싸 교체 가능한 구조 확보</li>
    <li><strong>현업 감각</strong> — 풀링·프레임 분리·경계 계산·이벤트 채널 등 실전 습관을 코드에 반영</li>
    <li><strong>가독성/문서화</strong> — 퍼블릭 API는 <em>XML Doc</em>, 구현부는 <em>의도/경계 중심 주석</em>으로 유지보수성 강조</li>
  </ul>
</details>
<div>
  <ul>코드 주석은 이해를 돕기 위해 AI를 이용하여 보강하였음</ul>
</div>

