# Production readiness snapshot

## Current status
- Exploration rendering uses the native SkiaSharp tile-map view with nearest-neighbor pixel art and the existing procedural robot player.
- Human sprite assets introduced in PR #44 have been anonymized to neutral identifiers in both MAUI projects.
- Sprite-sheet loading now validates frame dimensions and frame indices, and the tile renderer disposes cached sprite resources when its owning view is unloaded.
- Exploration error handling now logs encounter/save failures and recovers the renderer instead of silently swallowing failures.

## Known limitations
- The MAUI client is still primarily tuned for Android, with Windows retained as a development target.
- Exploration/battle navigation is service-driven and still depends on page lifecycle timing; device-level soak testing is still needed.
- Some sprite assets included for future content are packaged but not currently exercised by the main exploration scene.
- The repository contains broad game logic coverage, but renderer/device integration coverage remains limited outside focused unit tests.

## Performance and resource considerations
- Sprite sheets are cached per tile-map view instance and released on disposal to avoid leaking decoded `SKBitmap` resources.
- The renderer only invalidates when animation, dialogue, particles, or camera settling require a redraw.
- Building collision is now computed from logical footprints instead of mutating `WorldZone` terrain data on every initialization.

## Asset ownership and licensing checklist
- [x] Personal-name sprite filenames from PR #44 have been anonymized in tracked source.
- [ ] Confirm the repository owner has redistribution rights for every packaged sprite, font, icon, and audio asset.
- [ ] Record the source/license for each third-party art dependency before store submission.
- [ ] Verify that package identifiers, app name, privacy disclosures, and store listing art are owned and final.

## Remaining Android release blockers
- No signed Android release pipeline or keystore handling is documented in the repository.
- App-store submission materials are not present here: privacy policy, content rating answers, screenshots, feature graphics, and support metadata.
- Device validation is still required for suspend/resume, low-memory behavior, touch input, and back-navigation around battles/menus.
- The current CI surface in-repo is Jenkins-focused; store submission should also include a reproducible MAUI Android release build verification step.

This repository is closer to a production-ready prototype, but it should not be described as app-store ready until the blockers above are closed.
