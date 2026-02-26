# DTO — Data Transfer Objects

DTO классы для передачи данных по API. Содержат `[JsonProperty]` атрибуты для сериализации.

## Содержание

- [Auth](#auth)
- [Chat](#chat)
- [Character](#character)
- [Message](#message)
- [Workflow](#workflow)
- [Persona](#persona)
- [Общие](#общие)

---

## Auth

**Путь:** `Network/DTO/Auth/`  
**Namespace:** `Core.Data.Network.DTO.Auth`

### AuthSessionResponseDto

**Endpoint:** `GET /auth/session`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Session | `session` | `AuthSessionDto` |
| User | `user` | `AuthUserDto` |

### AuthSignInResponseDto

**Endpoint:** `POST /auth/sign-in`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Redirect | `redirect` | `bool` |
| Token | `token` | `string` |
| User | `user` | `AuthUserDto` |

### AuthSessionDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Token | `token` | `string` |
| ExpiresAt | `expiresAt` | `string` |

### AuthUserDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Email | `email` | `string` |
| Name | `name` | `string` |

---

## Chat

**Путь:** `Network/DTO/Chat/`  
**Namespace:** `Core.Data.Network.DTO.Chat`

### ChatResponseDto

**Endpoint:** `GET /chats/{id}`, `GET /chats`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Title | `title` | `string` |
| AvatarUrl | `avatarUrl` | `string` |
| CreatedAt | `createdAt` | `DateTime` |
| UpdatedAt | `updatedAt` | `DateTime` |
| DeletedAt | `deletedAt` | `DateTime?` |
| UserId | `userId` | `string` |
| UserName | `userName` | `string` |
| UserSlug | `userSlug` | `string` |
| UserProfileUrl | `userProfileUrl` | `string` |
| UserAvatar | `userAvatar` | `string` |
| ViewCount | `viewCount` | `int` |
| LikeCount | `likeCount` | `int` |
| FavoriteCount | `favoriteCount` | `int` |
| ShareCount | `shareCount` | `int` |
| TotalEmojiCount | `totalEmojiCount` | `int` |
| TotalMemories | `totalMemories` | `int` |
| Visibility | `visibility` | `string` |
| PinnedStatus | `pinnedStatus` | `bool` |
| Like | `like` | `bool` |
| Favorite | `favorite` | `bool` |
| CharacterId | `characterId` | `string` |
| PersonaId | `personaId` | `string` |
| Character | `character` | `ChatCharacterDto` |
| Persona | `persona` | `ChatPersonaDto` |
| Personas | `personas` | `List<ChatPersonaDto>` |
| Messages | `messages` | `List<ChatMessageDto>` |
| MessageCount | `messageCount` | `int` |
| TotalMessageCount | `totalMessageCount` | `int` |
| LastMessage | `lastMessage` | `string` |
| LastMessageAt | `lastMessageAt` | `DateTime?` |
| LastUsedPromptId | `lastUsedPromptId` | `string` |
| LastUsedModelId | `lastUsedModelId` | `string` |
| IsEncrypted | `isEncrypted` | `bool` |
| ChatBackground | `chatBackground` | `string` |
| LastUsedTemperature | `lastUsedTemperature` | `float?` |
| LastUsedTopP | `lastUsedTopP` | `float?` |
| LastUsedTopK | `lastUsedTopK` | `int?` |
| LastUsedPresencePenalty | `lastUsedPresencePenalty` | `float?` |
| LastUsedFrequencyPenalty | `lastUsedFrequencyPenalty` | `float?` |
| LastUsedMaxTokens | `lastUsedMaxTokens` | `int?` |
| LastUsedTotalTokens | `lastUsedTotalTokens` | `int?` |
| RequiresModelSelection | `requiresModelSelection` | `bool` |

---

### ChatMessageDto

**Endpoint:** Вложен в `ChatResponseDto.Messages`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| ChatId | `chatId` | `string` |
| Role | `role` | `string` |
| SendUserId | `sendUserId` | `string` |
| SendType | `sendType` | `string` |
| Content | `content` | `string` |
| TurnId | `turnId` | `string` |
| VersionIndex | `versionIndex` | `int` |
| ParentMessageId | `parentMessageId` | `string` |
| CreatedAt | `createdAt` | `DateTime` |
| UpdatedAt | `updatedAt` | `DateTime` |
| Images | `images` | `List<string>` |
| AlternativeVersions | `alternativeVersions` | `List<ChatMessageDto>` |
| IsBranchStart | `isBranchStart` | `bool` |
| DebugInfo | `debugInfo` | `string` |

---

### CreateChatRequestDto

**Endpoint:** `POST /chats`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| CharacterId | `characterId` | `string` |
| GreetingId | `greetingId` | `string` |
| PersonaId | `personaId` | `string` |
| PromptId | `promptId` | `string` |
| ModelId | `modelId` | `string` |
| IsEncrypted | `isEncrypted` | `bool` |
| Password | `password` | `string` |
| MaxTokens | `maxTokens` | `int?` |
| Temperature | `temperature` | `float?` |
| TopK | `topK` | `int?` |
| TopP | `topP` | `float?` |
| PresencePenalty | `presencePenalty` | `float?` |
| FrequencyPenalty | `frequencyPenalty` | `float?` |

---

### ChatCharacterDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Name | `name` | `string` |
| Slug | `slug` | `string` |
| CharacterUrl | `characterUrl` | `string` |
| Photo | `photo` | `string` |
| Thumb | `thumb` | `string` |
| UserId | `userId` | `string` |
| UserName | `userName` | `string` |
| UserSlug | `userSlug` | `string` |

---

### ChatPersonaDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Name | `name` | `string` |
| Description | `description` | `string` |
| AvatarUrl | `avatarUrl` | `string` |
| Visibility | `visibility` | `string` |

---

### Chat Extensions DTO

**Путь:** `Network/DTO/Chat/Extensions/`

#### ChatExtensionsUiResponseDto

**Endpoint:** `GET /chats/{id}/extensions/ui`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Components | `components` | `List<ChatUiComponentDto>` |
| Total | `total` | `int` |

#### ChatUiComponentDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Placement | `placement` | `string` |
| Type | `type` | `string` |
| WorkflowId | `workflowId` | `string` |
| Metadata | `metadata` | `ChatUiMetadataDto` |
| Config | `config` | `JObject` |

#### ChatUiMetadataDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Timestamp | `timestamp` | `string` |
| TtlSeconds | `ttl` | `int?` |
| WorkflowId | `workflowId` | `string` |

#### ChatUiButtonConfigDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Label | `label` | `string` |
| Variant | `variant` | `string` |
| Disabled | `disabled` | `bool?` |
| Action | `action` | `ChatUiActionDto` |

#### ChatUiActionDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Type | `type` | `string` |
| Payload | `payload` | `string` |
| Confirm | `confirm` | `bool?` |
| ConfirmMessage | `confirmMessage` | `string` |
| Metadata | `metadata` | `JObject` |

#### ChatUiDisplayConfigDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Content | `content` | `string` |

---

## Character

**Путь:** `Network/DTO/Character/`  
**Namespace:** `Core.Data.Network.DTO.Character`

### CharacterResponseDto

**Endpoint:** `GET /characters/{id}`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Name | `name` | `string` |
| Slug | `slug` | `string` |
| CharacterUrl | `characterUrl` | `string` |
| Photo | `photo` | `string` |
| Thumb | `thumb` | `string` |
| Descriptions | `descriptions` | `Dictionary<string, string>` |
| Traits | `traits` | `string` |
| ExampleDialogue | `exampleDialogue` | `string` |
| Scenario | `scenario` | `string` |
| Greetings | `greetings` | `List<CharacterGreetingDto>` |
| Tags | `tags` | `List<string>` |
| ChatCount | `chatCount` | `int` |
| FavoriteCount | `favoriteCount` | `int` |
| LikeCount | `likeCount` | `int` |
| DislikeCount | `dislikeCount` | `int` |
| ShareCount | `shareCount` | `int` |
| MemoryCount | `memoryCount` | `int` |
| GenPictureCount | `genPictureCount` | `int` |
| IsFavorite | `isFavorite` | `bool` |
| IsLiked | `isLiked` | `bool` |
| IsDisliked | `isDisliked` | `bool` |
| IsNsfw | `isNsfw` | `bool` |
| Featured | `featured` | `bool` |
| UserId | `userId` | `string` |
| UserName | `userName` | `string` |
| UserSlug | `userSlug` | `string` |
| CreatedAt | `gmtCreate` | `DateTime` |
| UpdatedAt | `gmtModified` | `DateTime` |
| DeletedAt | `deletedAt` | `DateTime?` |
| Visibility | `visibility` | `string` |
| DefaultModelId | `defaultModelId` | `string` |
| DefaultTemperature | `defaultTemperature` | `float?` |
| DefaultTopP | `defaultTopP` | `float?` |
| DefaultTopK | `defaultTopK` | `int?` |
| DefaultPresencePenalty | `defaultPresencePenalty` | `float?` |
| DefaultFrequencyPenalty | `defaultFrequencyPenalty` | `float?` |
| DefaultMaxTokens | `defaultMaxTokens` | `int?` |
| Gender | `gender` | `string` |
| AvatarDescription | `avatarDescription` | `string` |
| IsLatestPublic | `isLatestPublic` | `bool` |
| IsApproved | `isApproved` | `bool` |
| VersionNumber | `versionNumber` | `int` |
| RootCharacterId | `rootCharacterId` | `string` |
| PreviousVersionId | `previousVersionId` | `string` |
| TraitsVisibility | `traitsVisibility` | `string` |
| ScenarioVisibility | `scenarioVisibility` | `string` |
| ExampleDialogueVisibility | `exampleDialogueVisibility` | `string` |
| Rating | `rating` | `string` |
| IsNotesShowFirst | `isNotesShowFirst` | `bool` |
| IsDefinitionShow | `isDefinitionShow` | `bool` |
| IsProxiesAllow | `isProxiesAllow` | `bool` |
| IsModelReplaceAllow | `isModelReplaceAllow` | `bool` |

---

### CharacterListItemDto

**Endpoint:** `GET /characters` (в списке)

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Name | `name` | `string` |
| Description | `description` | `string` |
| Avatar | `avatar` | `string` |
| Slug | `slug` | `string` |
| Creator | `creator` | `CharacterCreatorDto` |
| Tags | `tags` | `List<string>` |
| Visibility | `visibility` | `string` |
| ChatCount | `chatCount` | `int` |
| LikeCount | `likeCount` | `int` |
| IsFavorite | `isFavorite` | `bool` |

---

### CharacterListResponseDto

**Endpoint:** `GET /characters`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Data | `characters` | `List<CharacterListItemDto>` |
| TotalCount | `totalCount` | `int` |
| TotalPages | `totalPages` | `int` |
| CurrentPage | `currentPage` | `int` |

---

### CharacterGreetingDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Title | `title` | `string` |
| Content | `content` | `string` |
| IsDefault | `isDefault` | `bool` |
| Language | `language` | `string` |
| Background | `background` | `string` |
| BackgroundImageId | `backgroundImageId` | `string` |
| ChatBackground | `chatBackground` | `string` |

---

### CharacterCreatorDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Username | `username` | `string` |
| Slug | `slug` | `string` |
| Avatar | `avatar` | `string` |

---

## Message

**Путь:** `Network/DTO/Message/`  
**Namespace:** `Core.Data.Network.DTO.Message`

### MessageResponseDto

**Endpoint:** `GET /messages/{id}`, `GET /messages/history/{chatId}`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| ChatId | `chatId` | `string` |
| Role | `role` | `string` |
| Content | `content` | `string` |
| CreatedAt | `createdAt` | `DateTime` |
| UpdatedAt | `updatedAt` | `DateTime` |
| Tokens | `tokens` | `int` |
| Rating | `rating` | `int?` |
| Feedback | `feedback` | `string` |

---

### SendMessageRequestDto

**Endpoint:** `POST /chats/stream`, `POST /chats/generate`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| ChatId | `chatId` | `string` |
| CharacterId | `characterId` | `string` |
| Message | `message` | `ChatMessageDto` |
| Model | `model` | `string` |
| ParentMessageId | `parentMessageId` | `string` |
| Temperature | `temperature` | `float?` |
| TopP | `topP` | `float?` |
| TopK | `topK` | `int?` |
| MaxTokens | `maxTokens` | `int?` |
| TotalTokens | `totalTokens` | `int?` |
| PresencePenalty | `presencePenalty` | `float?` |
| FrequencyPenalty | `frequencyPenalty` | `float?` |
| ChatPassword | `chatPassword` | `string` |

---

## Workflow

**Путь:** `Network/DTO/Workflow/`  
**Namespace:** `Core.Data.Network.DTO.Workflow`

### WorkflowResponseDto

**Endpoint:** `GET /workflow-crud/{id}`, `GET /workflow-crud`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Name | `name` | `string` |
| Description | `description` | `string` |
| RootWorkflowId | `rootWorkflowId` | `string` |
| PreviousVersionId | `previousVersionId` | `string` |
| VersionNumber | `versionNumber` | `int` |
| IsLatestPublic | `isLatestPublic` | `bool` |
| IsApproved | `isApproved` | `bool` |
| ContextSchema | `contextSchema` | `Dictionary<string, object>` |
| StorageSchema | `storageSchema` | `Dictionary<string, object>` |
| DefaultStorage | `defaultStorage` | `Dictionary<string, object>` |
| NodeCount | `nodeCount` | `int` |
| HasValidFlow | `hasValidFlow` | `bool` |
| LastValidatedAt | `lastValidatedAt` | `DateTime?` |
| UserId | `userId` | `string` |
| Visibility | `visibility` | `WorkflowVisibility` |
| CreatedAt | `createdAt` | `DateTime` |
| UpdatedAt | `updatedAt` | `DateTime` |
| DeletedAt | `deletedAt` | `DateTime?` |
| UsageCount | `usageCount` | `int` |
| Creator | `creator` | `WorkflowCreatorDto` |
| Triggers | `triggers` | `List<WorkflowTriggerConfigDto>` |
| Tags | `tags` | `List<WorkflowTagDto>` |
| Counts | `_count` | `WorkflowCountsDto` |
| ParentWorkflowId | `parentWorkflowId` | `string` (optional) |
| ShareStorageBetweenChats | `shareStorageBetweenChats` | `bool?` (optional) |
| Settings | `settings` | `Dictionary<string, object>` (optional) |
| Definition | `definition` | `Dictionary<string, object>` (optional) |

---

### WorkflowListResponseDto

**Endpoint:** `GET /workflow-crud`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Items | `data` | `List<WorkflowResponseDto>` |
| Meta | `meta` | `PaginationMetaDto` |

---

### WorkflowRunDto

**Endpoint:** `GET /workflow-crud/{id}/runs/{runId}`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| WorkflowId | `workflowId` | `string` |
| WorkflowVersion | `workflowVersion` | `int` |
| Status | `status` | `WorkflowRunStatus` |
| CurrentNodeId | `currentNodeId` | `string` |
| StepsTaken | `stepsTaken` | `int` |
| StepLimit | `stepLimit` | `int` |
| StartedAt | `startedAt` | `DateTime` |
| CompletedAt | `completedAt` | `DateTime?` |
| ResumeAt | `resumeAt` | `DateTime?` |
| TriggerType | `triggerType` | `string` |
| TriggerData | `triggerData` | `Dictionary<string, object>` |
| Error | `error` | `string` |
| RetryCount | `retryCount` | `int` |
| FinalResult | `finalResult` | `Dictionary<string, object>` |
| EmitChannel | `emitChannel` | `string` |
| Context | `context` | `WorkflowRunContextDto` |

**Computed (JsonIgnore):**
- `IsRunning`, `IsCompleted`, `HasError` → `bool`
- `Duration` → `TimeSpan?`

---

### WorkflowRunListResponseDto

**Endpoint:** `GET /workflow-crud/{id}/runs`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Runs | `runs` | `List<WorkflowRunDto>` |
| Total | `total` | `int` |
| Limit | `limit` | `int` |
| Offset | `offset` | `int` |

---

### WorkflowTriggerResponseDto

**Endpoint:** `POST /workflow/{id}/trigger`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Success | `success` | `bool` |
| RunId | `runId` | `string` |
| Result | `result` | `object` |
| Output | `output` | `object` |
| Storage | `storage` | `Dictionary<string, object>` |
| Steps | `steps` | `List<string>` |
| DurationMs | `durationMs` | `int` |
| Error | `error` | `string` |
| Profile | `profile` | `Dictionary<string, object>` |

**Computed (JsonIgnore):**
- `HasError` → `bool`

**Методы:**
- `GetResultAsDict()` → `Dictionary<string, object>`
- `GetOutputAsList()` → `List<object>`

---

### CreateWorkflowDto

**Endpoint:** `POST /workflow-crud`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Name | `name` | `string` |
| Description | `description` | `string` |
| ContextSchema | `contextSchema` | `Dictionary<string, object>` |
| StorageSchema | `storageSchema` | `Dictionary<string, object>` |
| DefaultStorage | `defaultStorage` | `Dictionary<string, object>` |
| Visibility | `visibility` | `WorkflowVisibility` |

---

### UpdateWorkflowDto

**Endpoint:** `PATCH /workflow-crud/{id}`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Name | `name` | `string` |
| Description | `description` | `string` |
| ContextSchema | `contextSchema` | `Dictionary<string, object>` |
| StorageSchema | `storageSchema` | `Dictionary<string, object>` |
| DefaultStorage | `defaultStorage` | `Dictionary<string, object>` |
| Visibility | `visibility` | `WorkflowVisibility?` |

---

### WorkflowExecuteRequestDto

**Endpoint:** `POST /workflow/{id}/trigger`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Payload | `payload` | `Dictionary<string, object>` |
| StorageContext | `storageContext` | `StorageContextDto` |
| IncludeSteps | `includeSteps` | `bool` |
| IncludeStorage | `includeStorage` | `bool` |
| IncludeProfile | `includeProfile` | `bool` |

---

### StorageContextDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Type | `type` | `string` (default: "user") |
| SessionId | `sessionId` | `string` |

---

### Вспомогательные Workflow DTO

#### WorkflowCreatorDto
| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Name | `name` | `string` |
| Image | `image` | `string` |

#### WorkflowTagDto
| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| Name | `name` | `string` |
| Slug | `slug` | `string` |

#### WorkflowCountsDto
| Поле | JsonProperty | Тип |
|------|-------------|-----|
| WorkflowRuns | `workflowRuns` | `int` |
| Favorites | `favorites` | `int` |

#### WorkflowTriggerConfigDto
| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| WorkflowId | `workflowId` | `string` |
| Entrypoint | `entrypoint` | `string` |
| NodeId | `nodeId` | `string` |
| IsActive | `isActive` | `bool` |

#### WorkflowRunContextDto
| Поле | JsonProperty | Тип |
|------|-------------|-----|
| WorkflowMetadata | `_wf` | `Dictionary<string, object>` |
| Storage | `storage` | `Dictionary<string, object>` |
| Outputs | `$outputs` | `Dictionary<string, object>` |

#### PaginationMetaDto
| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Total | `total` | `int` |
| Page | `page` | `int` |
| Limit | `limit` | `int` |
| TotalPages | `totalPages` | `int` |

---

### Workflow Enums

```csharp
// WorkflowEnums.cs
[JsonConverter(typeof(StringEnumConverter))]
enum WorkflowVisibility {
    [EnumMember(Value = "PUBLIC")] Public,
    [EnumMember(Value = "UNLISTED")] Unlisted,
    [EnumMember(Value = "PRIVATE")] Private
}

[JsonConverter(typeof(StringEnumConverter))]
enum WorkflowRunStatus {
    [EnumMember(Value = "RUNNING")] Running,
    [EnumMember(Value = "PAUSED")] Paused,
    [EnumMember(Value = "COMPLETED")] Completed,
    [EnumMember(Value = "ERROR")] Error,
    [EnumMember(Value = "CANCELLED")] Cancelled,
    [EnumMember(Value = "TIMEOUT")] Timeout
}

[JsonConverter(typeof(StringEnumConverter))]
enum WorkflowRunSortBy {
    [EnumMember(Value = "startedAt")] StartedAt,
    [EnumMember(Value = "duration")] Duration,
    [EnumMember(Value = "status")] Status
}

[JsonConverter(typeof(StringEnumConverter))]
enum SortOrder {
    [EnumMember(Value = "asc")] Asc,
    [EnumMember(Value = "desc")] Desc
}
```

---

### ChatWorkflowResponseDto

**Endpoint:** `GET /workflow-crud/chat/{chatId}/active`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Id | `id` | `string` |
| ChatId | `chatId` | `string` |
| WorkflowId | `workflowId` | `string` |
| IsActive | `isActive` | `bool` |
| Workflow | `workflow` | `WorkflowResponseDto` |

---

### ActiveWorkflowsResponseDto

**Endpoint:** `GET /workflow-crud/chat/{chatId}/active`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Workflows | `workflows` | `List<ChatWorkflowResponseDto>` |

---

### AttachWorkflowToChatDto

**Endpoint:** `POST /workflow-crud/chat/{chatId}/attach`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| WorkflowId | `workflowId` | `string` |

---

### DetachWorkflowFromChatDto

**Endpoint:** `POST /workflow-crud/chat/{chatId}/detach`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| WorkflowId | `workflowId` | `string` |

---

### ToggleWorkflowActiveDto

**Endpoint:** `POST /workflow-crud/chat/{chatId}/toggle-active`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| WorkflowId | `workflowId` | `string` |

---

### DuplicateWorkflowDto

**Endpoint:** `POST /workflow-crud/{id}/duplicate`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Name | `name` | `string` |

---

### TestWorkflowDto

**Endpoint:** `POST /workflow-crud/{id}/test`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Payload | `payload` | `Dictionary<string, object>` |

---

### PatchWorkflowStorageDto

**Endpoint:** `POST /workflow-crud/chat/{chatId}/workflow/{workflowId}/storage`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Storage | `storage` | `Dictionary<string, object>` |

---

### WorkflowGenerationRequestDto

**Endpoint:** `POST /workflow/generate`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Prompt | `prompt` | `string` |
| ExistingWorkflowId | `existingWorkflowId` | `string` |
| SessionId | `sessionId` | `string` |

---

### WorkflowGenerationResultDto

**Response** от `POST /workflow/generate`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| WorkflowJson | `workflowJson` | `Dictionary<string, object>` |
| StorageSchema | `storageSchema` | `Dictionary<string, object>` |
| DefaultStorage | `defaultStorage` | `Dictionary<string, object>` |
| SessionId | `sessionId` | `string` |

---

### WorkflowGenerationResetDto

**Endpoint:** `POST /workflow/generate/reset`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| SessionId | `sessionId` | `string` |

---

### WorkflowTriggerDto

**Endpoint:** `POST /workflow/{id}/trigger`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| ChatId | `chatId` | `string` |
| FakeChat | `fakeChat` | `FakeChatDto` |
| Payload | `payload` | `Dictionary<string, object>` |
| MessageCount | `messageCount` | `int?` |
| ChatPassword | `chatPassword` | `string` |

---

### FakeChatDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| CharacterId | `characterId` | `string` |
| PersonaId | `personaId` | `string` |
| ModelId | `modelId` | `string` |

---

### WorkflowEntrypointDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Name | `name` | `string` |
| Description | `description` | `string` |
| Type | `type` | `string` |

---

### WorkflowEntrypointListResponseDto

**Endpoint:** `GET /workflow/entrypoints`, `GET /workflow/{id}/entrypoints`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Entrypoints | `entrypoints` | `List<WorkflowEntrypointDto>` |

---

### RunEntrypointRequestDto

**Endpoint:** `POST /workflow/run-entrypoint`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Entrypoint | `entrypoint` | `string` |
| ChatId | `chatId` | `string` |
| Data | `data` | `Dictionary<string, object>` |

---

### RunEntrypointResponseDto

**Response** от `POST /workflow/run-entrypoint`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Success | `success` | `bool` |
| Message | `message` | `string` |

---

### Standalone DTO

**Путь:** `Network/DTO/Workflow/Standalone/`

#### StandaloneUiResponseDto

**Endpoint:** `GET /workflow/standalone/{id}/ui`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Success | `success` | `bool` |
| Components | `components` | `List<StandaloneUiComponentDto>` |

#### StandaloneUiComponentDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Type | `type` | `string` |
| Label | `label` | `string` |
| Content | `content` | `string` |
| Action | `action` | `string` |
| Id | `id` | `string` |
| Disabled | `disabled` | `bool?` |
| Config | `config` | `Dictionary<string, object>` |

#### StandaloneUiActionRequestDto

**Endpoint:** `POST /workflow/standalone/{id}/ui/action`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Payload | `payload` | `Dictionary<string, object>` |
| StorageContext | `storageContext` | `StorageContextDto` |

---

## Persona

**Путь:** `Network/DTO/Persona/`  
**Namespace:** `Core.Data.Network.DTO.Persona`

### PersonaListResponseDto

**Endpoint:** `GET /personas/me`

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Data | `data` | `List<ChatPersonaDto>` |
| Total | `total` | `int` |
| Page | `page` | `int` |
| Limit | `limit` | `int` |
| TotalPages | `totalPages` | `int` |

---

## Gemini

**Путь:** `Network/DTO/Gemini/`  
**Namespace:** `EndlessAdventure.Core.Data.Network.DTO.Gemini`

### GeminiImageRequestDto

**Endpoint:** `POST` к Gemini API для генерации изображений

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Contents | `contents` | `List<GeminiContentDto>` |
| GenerationConfig | `generationConfig` | `GeminiGenerationConfigDto` |

---

### GeminiContentDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Role | `role` | `string` |
| Parts | `parts` | `List<GeminiPartDto>` |

---

### GeminiPartDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Text | `text` | `string` |
| InlineData | `inlineData` | `GeminiInlineDataDto` |
| ThoughtSignature | `thoughtSignature` | `string` |

---

### GeminiInlineDataDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| MimeType | `mimeType` | `string` |
| Data | `data` | `string` (base64) |

---

### GeminiGenerationConfigDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| ResponseModalities | `responseModalities` | `List<string>` (default: ["IMAGE"]) |
| ImageConfig | `imageConfig` | `GeminiImageConfigDto` |

---

### GeminiImageConfigDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| AspectRatio | `aspectRatio` | `string` |
| ImageSize | `imageSize` | `string` |

---

### GeminiImageResponseDto

**Response** от Gemini API

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Candidates | `candidates` | `List<GeminiCandidateDto>` |
| Error | `error` | `GeminiErrorDto` |

**Computed:**
- `IsSuccess` → `bool` (Error == null && Candidates.Count > 0)

---

### GeminiCandidateDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Content | `content` | `GeminiContentDto` |
| FinishReason | `finishReason` | `string` |
| Index | `index` | `int` |
| SafetyRatings | `safetyRatings` | `List<GeminiSafetyRatingDto>` |

---

### GeminiSafetyRatingDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Category | `category` | `string` |
| Probability | `probability` | `string` |

---

### GeminiErrorDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Code | `code` | `int` |
| Message | `message` | `string` |
| Status | `status` | `string` |
| Details | `details` | `List<object>` |

---

### Пример использования

```csharp
var request = new GeminiImageRequestDto
{
    Contents = new List<GeminiContentDto>
    {
        new()
        {
            Role = "user",
            Parts = new List<GeminiPartDto>
            {
                new() { Text = "Generate fantasy warrior portrait" }
            }
        }
    },
    GenerationConfig = new GeminiGenerationConfigDto
    {
        ImageConfig = new GeminiImageConfigDto
        {
            AspectRatio = "1:1",
            ImageSize = "1024x1024"
        }
    }
};

var response = await geminiService.GenerateImage(request);

if (response.IsSuccess)
{
    var imageData = response.Candidates[0].Content.Parts[0].InlineData.Data;
    var bytes = Convert.FromBase64String(imageData);
    // Create Texture2D from bytes
}
else
{
    Debug.LogError($"Gemini error: {response.Error.Message}");
}
```

---

## Общие

### PaginationDto

| Поле | JsonProperty | Тип |
|------|-------------|-----|
| Page | `page` | `int` |
| Limit | `limit` | `int` |
| Total | `total` | `int` |
| Pages | `pages` | `int` |

**Computed:**
- `HasNextPage` → `bool`
- `HasPreviousPage` → `bool`
