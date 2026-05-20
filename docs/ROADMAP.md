
[Back to README](../README.md)

# ROADMAP

전체 진행 상황: **40% complete** (완료 항목 대비 전체 작업 비율 — 자세한 현황은 TODO 리스트와 연동 필요)

---

## Summary

- Completed: 8+ items (see `docs/DEVELOPMENT_LOG.md`)
- Remaining: short/medium/long term tasks listed below

---

## Short-term / Urgent (단기 / 긴급)

- [ ] Fix lap time persistence (랩 타임 영속성 저장 및 메인 화면 복귀 시 Recent/Best 복원) — 구현 방식: `PlayerPrefs` 또는 파일 저장
- [ ] Implement Settings window details (설정 창: 해상도, fullscreen, graphics/sound toggles, key bindings)
- [ ] Fix main screen image / camera linkage (메인씬 카메라 직결 문제 해결 또는 안정적 캡처 대체)
- [ ] Fix duplicate main screen key guide (중복 표시 근원 제거)
- [ ] Verify tire prefab connections across all scenes and prefabs (프리팹 연결 검증)

---

## Mid-term (2주–4주)

- [ ] Audio system: engine RPM pitch shifting, skid & ambient SFX, mixing levels
- [ ] Multiple vehicle selection (data-driven prefabs / vehicle list UI)
- [ ] Add additional tracks / integrate new maps

---

## Long-term (4주–최종)

- [ ] Race flow: start lights countdown, checkpoint order validation, penalties & re-trigger rules
- [ ] Finalize track and improve off-track slip handling (enhanced traction/slip model)
- [ ] Long-term lap time storage & query system (persistent DB/file or `PlayerPrefs` + UI querying/sorting)
- [ ] Build pipeline and produce distributable builds
- [ ] Prepare submission-ready documentation (한/영, 스크린샷, quick start)

---

### Notes

- 기술 명칭은 영문 유지(예: `Linear Damping`, `PlayerPrefs`)하고 설명은 한글로 작성했습니다.
- 우선순위 변경 또는 세부 작업 분할을 원하시면 항목별로 예상 소요(인력/시간)를 추가하겠습니다.
