# TLDC1.0 Unity Project Notes

This README is written for AI assistants and future developers who need to understand the project quickly before making changes.

## Project Summary

TLDC1.0 is a Unity 2D pet collection and auto-battle game. The current codebase focuses on:

- User authentication and data sync through Supabase.
- Pet ownership, pet selection, pet stats, star upgrade, and realm upgrade.
- Inventory items, EXP potions, item selling, and server-side item operations.
- Skill-driven pet combat with active skills, passive triggers, cooldowns, and effect ScriptableObjects.
- Dungeon/phoban configuration with enemy teams and reward data.
- UI screens for login, main game, pet list/details, inventory, team selection, dungeon, and battle.

The project is inside `Assets/` of a Unity project at `D:\unity\TLDC1.0`.

## Important Folders

- `1_Script/`
  Main C# gameplay and UI code.
- `1_Script/Core/`
  Supabase, authentication, currencies/resources, and global app services.
- `1_Script/Pet/`
  Pet data model, pet database lookup, pet list/detail UI, team selection, stats, and progression UI.
- `1_Script/Battle/`
  Auto-battle runtime logic, battle pet wrapper, battle UI unit, and static battle data handoff.
- `1_Script/Skills/`
  Skill ScriptableObject definitions and reusable skill effects.
- `1_Script/Inventory/`
  Inventory data model, inventory manager, item ScriptableObjects, item detail UI, and inventory UI.
- `1_Script/Dungeon/`
  Dungeon configuration, dungeon UI, dungeon manager, and battle scene loading helpers.
- `1_Script/UI/`
  Shared UI controllers such as panel manager, loading UI, main UI controller, and reward/requirement item UI.
- `1_Script/Tools/`
  Development/test helpers such as battle testing, time scale control, and damage text.
- `2_SO-Pet/`
  Pet base ScriptableObject assets.
- `3_SO-Item/`
  Item ScriptableObject assets.
- `4_SO-update/`
  Progression/update related ScriptableObject asset(s).
- `5_SO-Skill/`
  Skill and skill effect ScriptableObject assets.
- `6_SO-Dengeon/`
  Dungeon ScriptableObject assets. Note the folder name is currently spelled `Dengeon`.
- `Database/Migrations/`
  SQL migration files for Supabase RPC/database behavior.
- `Prefabs_Pet/`
  Pet model prefabs used by battle/visual systems.
- `Prefabs_UI/`
  Reusable UI prefabs.
- `Scenes/`
  Unity scenes: `MainMenu`, `MainGame`, `CharacterSelector`, `BattleScene`, `PhoBan`.
- `Image/`
  Large collection of icons, backgrounds, UI sprites, pet icons, skill icons, and other art assets.
- `Packages/`
  NuGet-imported dependencies, including Supabase C# client packages and related libraries.

## Main Runtime Services

Most major managers are Unity `MonoBehaviour` singletons and many use `DontDestroyOnLoad`.

- `SupabaseManager`
  Initializes the Supabase client and exposes `SupabaseManager.Instance.Client`.
- `AuthManager`
  Handles login/auth state and exposes the current user id.
- `ResourceManager`
  Syncs currencies/resources and claims battle rewards from the server.
- `PetManager`
  Loads user pets from Supabase, stores the currently selected pet, stores the selected team, and looks up `PetBaseSO` by `petBaseID`.
- `InventoryManager`
  Loads inventory from Supabase and calls secure server RPCs for adding items, using EXP potions, selling items, star up, and realm up.
- `DungeonManager`
  Stores the configured dungeon list, looks up dungeons by id, tracks the selected dungeon, and handles dungeon completion flow.
- `BattleManager`
  Runs the auto-battle loop in `BattleScene`.

## Data Model Overview

### PetBaseSO

Defined in `1_Script/Pet/PetBaseSO.cs`.

Represents static pet design data:

- `petBaseID`
- `speciesName`
- `element`
- `attackType`
- `defaultTier`
- base stats: HP, physical attack, magic attack, physical defense, magic defense, speed
- `icon`
- `petPrefab`
- `progressionTable`
- combat `skills`

Runtime owned pets are represented by `PetModel`, mapped to the Supabase table `user_pets`.

### PetModel

Defined in `1_Script/Pet/PetManager.cs`.

Represents a user's owned pet from Supabase:

- `id`
- `userId`
- `petName`
- `element`
- `petType`
- `tier`
- `petBaseId`
- `level`
- `currentExp`
- `star`
- `realm`

`petBaseId` connects the database pet record to a local `PetBaseSO`.

### ItemBaseSO and InventoryModel

Defined under `1_Script/Inventory/`.

`ItemBaseSO` represents static item design data. `InventoryModel` represents the user's owned inventory from Supabase.

Important server RPCs used by inventory/progression code:

- `add_item`
- `use_pet_exp_potion_secure`
- `sell_item_secure`
- `pet_star_up`
- `pet_realm_up`

### PetSkillSO

Defined in `1_Script/Skills/PetSkillSO.cs`.

Represents a pet skill. Skills can be:

- `Active`
- `Passive`

Passive skills trigger on:

- `OnTurnStart`
- `OnAttack`
- `OnAttacked`
- `OnKill`
- `OnDeath`
- `OnWaveStart`

Skills can contain a list of reusable `SkillEffect` ScriptableObjects. Battle code prefers executing `skill.effects`; if no effects are assigned, it falls back to old direct damage calculation.

### DungeonConfigSO

Defined in `1_Script/Dungeon/DungeonConfigSO.cs`.

Represents static dungeon/phoban configuration, including enemy team, stamina cost, gold reward, and drops.

## Battle Flow

Primary file: `1_Script/Battle/BattleManager.cs`.

1. Another scene prepares battle data in `BattleDataStore`.
2. `BattleScene` loads.
3. `BattleManager.Start()` checks `BattleDataStore.selectedAllies` and `BattleDataStore.selectedEnemies`.
4. `StartBattle()` converts `PetModel` entries to `BattlePet` using `PetManager.GetPetBaseByID()`.
5. The first living ally and enemy are spawned at scene spawn points.
6. `OnWaveStart` passive skills are triggered.
7. `BattleLoop()` repeats until one team is defeated:
   - Faster pet acts first based on `stats.Speed`.
   - `OnTurnStart` passive skills trigger before that pet attacks.
   - `ChooseActiveSkill()` selects an active skill by cooldown and priority.
   - Animation/VFX are played if configured.
   - `OnAttack` passive skills trigger.
   - The active skill's `effects` execute. If none exist, fallback damage is used.
   - `OnAttacked` passive skills trigger.
   - UI and damage text update.
   - Dead pets are replaced by the next living team member.
   - Cooldowns are reduced.
8. On victory:
   - `DungeonManager.CompleteBattle()` is called if a dungeon is active.
   - `ResourceManager.ClaimBattleReward()` is called if a battle log id exists.
9. The game returns to `MainGame`.

`BattleDataStore` is static and used as a temporary handoff between scenes:

- `selectedAllies`
- `selectedEnemies`
- `currentBattleLogId`
- `currentDungeon`

## Scene Guide

- `Scenes/MainMenu.unity`
  Likely entry/login or first menu scene.
- `Scenes/MainGame.unity`
  Main hub UI after login.
- `Scenes/CharacterSelector.unity`
  Character or starting pet selection flow.
- `Scenes/PhoBan.unity`
  Dungeon/phoban selection scene.
- `Scenes/BattleScene.unity`
  Runtime auto-battle scene.

## Backend Notes

The game uses Supabase through NuGet packages stored in `Assets/Packages/`.

Important code paths:

- Supabase setup: `1_Script/Core/SupabaseManager.cs`
- Auth: `1_Script/Core/AuthManager.cs`
- Resources/rewards: `1_Script/Core/ResourceManager.cs`
- Pet table access: `1_Script/Pet/PetManager.cs`
- Inventory and progression RPCs: `1_Script/Inventory/InventoryManager.cs`
- SQL migrations: `Database/Migrations/`

When changing gameplay economy, item use, battle rewards, or pet progression, check both Unity C# and Supabase SQL/RPC migrations.

## Current Architectural Patterns

- Managers are mostly singleton `MonoBehaviour`s.
- Persistent services use `DontDestroyOnLoad`.
- Static design data is kept in ScriptableObject assets.
- Player-owned runtime data is stored in Supabase.
- UI controllers refresh themselves after server operations.
- Pet selection uses events in `PetManager`:
  - `OnPetSelected`
  - `OnPetStatsUpdated`
- Battle is currently asynchronous using `async Task`, `await Task.Yield()`, and custom scaled wait loops.

## Development Warnings For AI Assistants

- Do not rename folders, scenes, ScriptableObject fields, serialized fields, or enum values casually. Unity serialization depends on exact names and references.
- Do not move assets unless explicitly asked. Unity `.meta` files and GUID references are important.
- Keep `.meta` files with their assets.
- Be careful with Vietnamese text in comments/logs. Some terminal output may show mojibake/encoding corruption even if Unity displays it correctly. Do not "fix" Vietnamese strings unless the user asks and you verify the actual file encoding/editor display.
- Do not replace Supabase RPC calls with client-side economy logic. Many operations are intentionally server-authoritative.
- Before changing battle behavior, inspect:
  - `BattleManager`
  - `BattlePet`
  - `BattleUnit`
  - `PetSkillSO`
  - `SkillEffect` subclasses
- Before changing pet upgrades, inspect:
  - `PetUpgradeUI`
  - `PetStarUpUI`
  - `PetRealmUpUI`
  - `PetProgressionTableSO`
  - `InventoryManager`
  - SQL migrations
- Before changing item behavior, inspect:
  - `ItemBaseSO`
  - `InventoryModel`
  - `InventoryManager`
  - `InventoryUIController`
  - SQL migrations

## Likely Next Work Areas

- Clean up or standardize encoding for Vietnamese logs/comments if needed.
- Add stronger battle result/reward handling between dungeon, battle log, and Supabase.
- Expand dungeon entry checks such as stamina, level requirements, and team validation.
- Improve skill effect reporting so damage text reflects the actual executed effect result instead of recalculating fallback damage.
- Add validation around missing singleton managers in scenes.
- Add tests or editor tools for validating ScriptableObject references.

