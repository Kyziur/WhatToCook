## 1. Static Workspace Structure

- [x] 1.1 Add meal-slot ordering helpers and view models without changing API contracts
- [x] 1.2 Recompose planner HTML into period toolbar, metrics, desktop week grid, mobile selected day, and contextual side regions
- [x] 1.3 Add responsive CSS for desktop, tablet, mobile sheet, cards, placeholders, and safe areas
- [x] 1.4 Recompose shopping and recipe-detail markup into the aligned responsive hierarchy

## 2. Interaction Wiring

- [x] 2.1 Wire day and meal-slot selection to the contextual recipe picker
- [x] 2.2 Wire recipe search, assignment, multiplier, removal, save, and regeneration to the new controls
- [x] 2.3 Load and render selected-plan shopping state in the desktop planner panel
- [x] 2.4 Preserve plan creation, switching, direct assignment, loading, error, busy, and dirty states

## 3. Visual Verification

- [x] 3.1 Inspect planner desktop at 1440 and 1920 pixels with populated and empty slots
- [x] 3.2 Inspect planner, shopping, details, drawer, and navigation at 375 and 768 pixels
- [x] 3.3 Resolve overflow, overlap, contrast, focus, and touch-target findings from agent-browser

## 4. Automated Verification

- [x] 4.1 Update component tests for slot ordering, desktop/mobile projections, picker, shopping panel, and aligned details
- [x] 4.2 Update Playwright scenarios only after the UI and interactions are stable
- [x] 4.3 Run build, all .NET tests, full Playwright, Husky/CSharpier, and strict OpenSpec validation
- [x] 4.4 Complete independent review and traceability matrix
