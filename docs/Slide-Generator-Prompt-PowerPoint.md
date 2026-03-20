# PowerPoint Slide Generator Prompt

You are a senior PowerPoint deck creator for executive technical presentations.

Build a presentation using the source documents below:

## Source documents
1. [docs/Management-Slide-Content-Final.md](docs/Management-Slide-Content-Final.md)
2. [docs/Guideline-Solution-Understanding.md](docs/Guideline-Solution-Understanding.md)

## Deck objective
Create a management-ready deck for the DGC API Guideline solution that presents final target standards only.

## Hard constraints
1. Final state only, no roadmap/gap framing.
2. Idempotency is mandatory for all endpoints requiring it.
3. Comprehensive testing is mandatory.
4. No secrets in appsettings is a hard rule.
5. Local secret handling = User Secrets.
6. Deployment-time secrets are out of scope (deployment team responsibility).
7. Messaging transports must be only:
   - Redis
   - In-Memory
   - Azure Queue Storage transport
8. Include a direct NuGet dependency slide with exact pinned versions; do not include transitive dependencies.
9. Include one architecture image slide as an onion-style layered diagram.
10. For that architecture image, enforce these labels exactly by layer:
   - Outer layer: Api / Function / Infrastructure
   - Middle layer: Application
   - Core layer: Domain
11. Include this exact dependency flow text on the slide: Api/Function/Infrastructure -> Application -> Domain.
12. Ensure these points are explicitly present in slide text:
   - Database-First approach as a standard.
   - Static mapper usage as a standard.
   - Validation attribute usage as a standard.

## Required architecture pattern emphasis
Include one dedicated slide that highlights encouraged code patterns used by the solution and why they are used.

Required patterns to cover:
- Repository Pattern: abstracts persistence access and keeps service logic persistence-agnostic.
- Unit of Work Pattern: coordinates transactional consistency across repository operations.
- Specification Pattern: encapsulates query criteria and reuseable filtering rules.
- Static Mapper Pattern: uses explicit static mapping methods for deterministic DTO transformations.
- Validation Attribute Pattern: uses custom validation attributes for consistent request validation.
- Idempotency Pattern: protects state-changing operations from duplicate processing.
- Transport Abstraction Pattern (Queue): allows selecting Redis/In-Memory/Azure Queue Storage transport without API contract changes.
- Extension-based Composition Pattern: keeps startup wiring modular and maintainable.

For each pattern on that slide, include:
- Purpose
- Where it applies in the solution
- Benefit to delivery quality/maintainability

## PowerPoint output requirements
Generate a slide-by-slide blueprint in a PPT-friendly structure.

For each slide, provide exactly these fields:

- SlideNumber
- SlideTitle
- SlideSubtitle
- LayoutType (choose one: Title, TitleAndContent, TwoContent, Comparison, SectionHeader, TitleOnly)
- OnSlideText (max 6 bullets, each max 14 words)
- VisualType (choose one: ArchitectureDiagram, ProcessFlow, MatrixTable, IconGrid, Timeline, DataTable, None)
- VisualSpec (clear instructions for shapes/icons/table columns)
- SpeakerNotes (60-100 words)
- KeyMessage (one sentence)
- Transition (one short phrase)

## Recommended slide order
Use this order unless strong reason to adjust:
1. Title
2. Executive Summary
3. Architecture Standard (with onion-style architecture image and dependency flow text)
4. Code Pattern Standard (Repository, Unit of Work, Specification, Idempotency, Transport Abstraction, Extension-based Composition)
5. API Governance Standard
6. Reliability and Idempotency Standard
7. Messaging Standard (Redis/In-Memory/Azure Queue Storage transport only)
8. Data and Database Standard (must explicitly state Database-First)
9. Security and Secret Management Standard
10. Testing Standard
11. Dependency Governance (direct NuGet versions)
12. Appendix: Key Technical Terms

## Design rules
- Executive clean style, minimal clutter.
- One strong message per slide.
- Prefer visuals over dense bullets.
- Keep language non-jargon where possible.
- Maintain consistency in heading grammar and bullet style.
- Apply this fixed color scheme across the deck:
   - Primary: #26A2DB
   - Secondary: #0F2D63

## Dependency slide format requirement
For the dependency slide, force a table with columns:
- Category
- Package
- Pinned Version

Include only direct packages from the provided source content.

## QA checklist (must run before output)
Report PASS/FAIL for each:
1. Idempotency stated as mandatory.
2. Comprehensive testing stated as mandatory.
3. No-secrets-in-appsettings hard rule present.
4. Deployment secrets marked out-of-scope.
5. Dependency versions shown and are direct-only.
6. Dedicated code-pattern slide exists and covers required patterns with purpose and benefit.
7. Architecture image is onion-style with 3 concentric layers and correct labels.
8. Architecture image slide includes exact flow text: Api/Function/Infrastructure -> Application -> Domain.
9. Database-First, Static Mapper, and Validation Attribute points are explicitly present.

## Final response format
Return in this exact section order:
1. Deck Metadata (target audience, tone, slide count)
2. Slide Blueprint (all slides with required fields)
3. Dependency Table Content (copy-ready)
4. Speaker Notes Summary (all slide key talk tracks in order)
5. QA Checklist Results
