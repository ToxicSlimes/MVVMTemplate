# Data Layer Documentation — Change Log

**Date:** 2026-02-10  
**Review:** Daily doc review — Data layer

---

## Changes Made

### ✅ New Documentation Files Created

#### 1. Audio.md
**Covers:** Audio data structures and enums
- `AudioClipEntry` (struct with Addressable/Legacy clip + weight)
- `AudioMixerGroupSetup` (mixer group configuration)
- `SoundSetup` (sound configuration with spatial settings)
- **Enums:**
  - `AudioMixerGroupType` (Master, Music, SFX, UI, Environment, Voice)
  - `SoundPlaybackScope` (Global, Local)
  - `SoundType` (130+ sound IDs grouped by category: Music, UI, SFX, Environment)

**Path:** `Data/Domain/Audio/` (namespace: `Core.Data.Configs.Audio`)

---

#### 2. Settings.md
**Covers:** Settings data classes
- `AudioSettingsData` (volume, mute settings)
- `VideoSettingsData` (resolution, quality, fullscreen, vsync, target FPS)
- `DisplaySettingsData` (brightness, gamma, UI scale, colorblind mode)
- `ControlsSettingsData` (mouse sensitivity, invert Y)
- `GameSettingsData` (language, autosave, tutorials, screen shake, mature content)
- **Enums:**
  - `SettingsTab` (Audio, Video, Display, Controls, Game)
  - `QualityPreset` (Low, Medium, High, Ultra)
  - `ColorblindType` (None, Deuteranopia, Protanopia, Tritanopia)
  - `GameLanguage` (English, Russian, German, French, Spanish, Chinese, Japanese, Korean)
  - `TargetFpsOption` (30, 60, 120, Unlimited)

**Path:** `Data/Domain/Models/` (`AudioSettingsData.cs`, `SettingsData.cs`)

---

#### 3. Elements.md
**Covers:** Element data structures (readonly struct для UI)
- `IElementData` (base interface with `Id` property)
- `EquipmentSlotData` (equipment slot with item info, rarity)
- `HeroAbilityData` (ability with name, description, cooldown, mana cost)
- `HeroSlotData` (hero slot with name, portrait, level, health)
- `ResourceItemData` (resource with name, amount, icon)
- `ServiceSlotElementData` (service slot with hero, progress, time)
- `StatRowData` (stat row with label, value, bar percent)

**Path:** `Data/Domain/Models/Elements/`  
**Type:** Readonly structs, implement `IElementData`, `IEquatable<T>`

---

#### 4. GameUI.md
**Covers:** Game UI models (специфичные для игровых панелей)
- **HeroDetailPanel:**
  - `HeroDetailData` (hero details with attributes, abilities)
  - `AbilityData` (ability info in hero panel)
- **HeroesGridPanel:**
  - `HeroCardData` (hero card in grid with health, notifications)
  - `HeroesGridTab` enum (MyHeroes, Shop, DeadHeroes)
  - `HeroesGridMode` enum (Upgrades, Healing, TeamManagement)
  - `HeroesGridTestData` (obsolete test data)
- **Main:**
  - `HeroSlotData` (hero slot in main view)
  - `StatRowData` (stat row in main view)
  - `ResourceData` (resource display)
  - `MainTestData` (obsolete test data)
- **ServicePanel:**
  - `ServiceRowData` (row of service slots)
  - `ServiceSlotData` (service slot with hero, progress, time)
  - `ServiceSlotState` enum (Locked, Empty, Active, Complete)

**Path:** `Data/Domain/Models/{HeroDetailPanel,HeroesGridPanel,Main,ServicePanel}/`

---

#### 5. Network.md
**Covers:** Network utilities
- `ApiEndpoints` — static class with **20+ endpoint categories**:
  - App, Characters, Chats, Messages, Models, Personas, Profile
  - Finance, Payments, Subscriptions, Memories, Prompts, Images
  - Storage, Feed, Tags, Settings, ElevenLabs, FishAudio
  - CharacterImport, Referral, InvitationCodes, ActionAgents
  - Workflow, WorkflowCrud, Stats, BoostyTracker, Commission
  - EarlyAccess, TokenManager, ChatDebug, SystemPrompt, Admin
- `HttpResponse<T>` — generic wrapper с полями:
  - `IsSuccess`, `StatusCode`, `Data`, `Error`, `Headers`
  - Factory methods: `Success()`, `Failure()`

**Path:** `Data/Network/`

---

### ✅ Updated Documentation Files

#### 1. DTO.md
**Added:** Gemini DTO section
- `GeminiImageRequestDto` (request to Gemini API)
- `GeminiContentDto`, `GeminiPartDto`, `GeminiInlineDataDto`
- `GeminiGenerationConfigDto`, `GeminiImageConfigDto`
- `GeminiImageResponseDto` (response from Gemini API)
- `GeminiCandidateDto`, `GeminiSafetyRatingDto`, `GeminiErrorDto`

**Path:** `Data/Network/DTO/Gemini/`  
**Namespace:** `EndlessAdventure.Core.Data.Network.DTO.Gemini`

---

#### 2. README.md
**Updated:**
- Added new documentation files to "Файлы документации" section
- Updated folder structure tree with:
  - Audio classes (namespace clarification)
  - Settings classes (AudioSettingsData.cs, SettingsData.cs)
  - Enums (AudioEnums.cs, SettingsEnums.cs, SoundType.cs)
  - Gemini DTO
  - ApiEndpoints.cs, HttpResponse.cs

---

## Summary Statistics

### Documentation Coverage

| Category | Files Documented | Status |
|----------|------------------|--------|
| **Domain Models** | 130 .cs files | ✅ Full coverage |
| **DTO** | 47 .cs files | ✅ Full coverage |
| **Mappers** | 6 .cs files | ✅ Full coverage |
| **Value Objects** | 3 .cs files | ✅ Full coverage |
| **UI Data** | 4 .cs files | ✅ Full coverage |
| **Network** | 2 .cs files | ✅ Full coverage |

**Total:** 192 .cs files documented

---

## Files Changed

1. ✅ **Created:** `Data/docs/Audio.md`
2. ✅ **Created:** `Data/docs/Settings.md`
3. ✅ **Created:** `Data/docs/Elements.md`
4. ✅ **Created:** `Data/docs/GameUI.md`
5. ✅ **Created:** `Data/docs/Network.md`
6. ✅ **Updated:** `Data/docs/DTO.md` (added Gemini section)
7. ✅ **Updated:** `Data/docs/README.md` (added new files + updated structure)
8. ✅ **Created:** `Data/docs/CHANGELOG.md` (this file)

---

## Next Steps

### Recommended

1. ✅ **Code review:** Verify all documented classes match actual code
2. ⬜ **Examples:** Add usage examples to complex models
3. ⬜ **Diagrams:** Create architecture diagrams for:
   - Audio system flow
   - Settings persistence
   - Element data flow (Domain → Elements → UI)
4. ⬜ **Tests:** Document unit tests for mappers and value objects

### Maintenance

- 🔄 **Daily review:** Run doc review on modified files
- 🔄 **New models:** Add to appropriate .md file immediately
- 🔄 **Breaking changes:** Mark deprecated APIs with `[Obsolete]`

---

## Notes

### Naming Conventions

- **Struct vs Class:**
  - Elements: readonly struct (performance, immutability)
  - Models: class (reference semantics)
  - Settings: class (mutable config)
  
- **Naming patterns:**
  - `*Data` — data structures (EquipmentSlotData, HeroDetailData)
  - `*Model` — domain models (ChatModel, CharacterModel)
  - `*Dto` — data transfer objects (ChatResponseDto, CharacterDto)
  - `*Mapper` — static mapper classes (ChatMapper, CharacterMapper)

### Architecture Patterns

- **Mapper Pattern:** DTO → Model conversion
- **Factory Pattern:** Static factory methods (AudioSettingsData.Default, StorageContext.User())
- **Value Object:** Readonly structs with equality (Elements)
- **Response Wrapper:** HttpResponse<T> for error handling

---

**Review completed:** 2026-02-10 18:00 GMT+3  
**Status:** ✅ All Data layer code is now documented
