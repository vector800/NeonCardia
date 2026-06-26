# Battle Party Status HUD Generated Style Fix

## 現在の原因分析

- 左上HUDは `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab` を `BattleTimelinePrototypeController` が生成している。
- Canvas Scaler は `Scale With Screen Size`、Reference Resolution は `1920x1080`。
- 前回調整でHUD幅が約378px相当まで縮み、文字、HP/MPバー、顔領域が小さくなっていた。
- 親Scale縮小は使われておらず、原因は主にRectTransformの狭さと子要素比率。
- 旧 `BattlePartyStatusPanel` は `BuildBattlePartyStatusPanel` 内で新HUD生成成功時にreturnするため、二重表示されない構造。

## 使用した生成画像

- `Assets/References/修正案14.png`
- 新規imagegenは未使用。参照画像からUnity UI部品として切り出し、TextMeshProとImage Fillで再構築した。

## Visual Direction Card

- Asset type: Battle party status HUD UI parts
- In-game purpose: BattleScene左上の3人パーティ状態表示
- Target resolution / aspect ratio: 1920x1080
- On-screen size: 約640x220px
- Background / transparency: UI部品PNG、赤い説明枠は除外
- Style family: 2000s-2010s console JRPG battle HUD
- Color palette: black metal, silver/white trim, cyan-green HP, blue MP, red selected highlight
- Material language: black metal frame, silver corner pieces, thin white rule lines
- Lighting rule: source imageのcool metal lightingを維持
- Shape language: long horizontal rows, left socket frame, wide portrait cut-in, diagonal slash divider
- Readability requirement: names and values remain TextMeshPro; no text baked into images
- Animation / state requirement: selected highlight remains toggleable; HP/MP remain fill-controlled
- Must include: metal frame, thick bars, wide face cut-in, slash divider, red selected row
- Must avoid: parent scale shrink, one-piece pasted HUD, fake text, tiny thumbnail face, unreadable values
- Negative prompt: no new generated visual prompt was used; existing source image was cut into implementation parts

## 切り出した素材一覧

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_RowFrame.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_LeftSlotFrame.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_HPBar_Back.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_HPFill.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_MPBar_Back.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_MPFill.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_PortraitFrame.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_PortraitMask.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Portrait_01.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Portrait_02.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Portrait_03.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_SlashDivider.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_SelectedHighlight.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Frame_9Slice.png`

## Import Settings

Applied by `Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup`.

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Alpha Is Transparency: On
- Mip Maps: Off
- Compression: Uncompressed
- Filter Mode: Bilinear
- Max Size: 2048 for all HUD PNG assets

## 9-Slice / Image Type設定

- `RowFrame`: fixed-aspect target art, `Image.Type.Simple`; internal portrait/frame art must not be 9-slice distorted
- `SelectedHighlight`: fixed-aspect row overlay, `Image.Type.Simple`
- `LeftSlotFrame`: border `36,36,36,36`
- `HPBar_Back`: border `36,12,36,12`
- `MPBar_Back`: border `36,10,36,10`
- Root frame is transparent because the target design uses separated rows, not an enclosing red guide border.

## Prefab構成

- `BattlePartyStatusHUD_GeneratedStyle`
- `Rows`
- `PartyMemberStatusRow_01`
- `PartyMemberStatusRow_02`
- `PartyMemberStatusRow_03`

Each row contains:

- `RowFrame`
- `SelectedHighlight`
- `LeftSlotFrame_Image`
- `HPBar_Background`
- `HPBar_Fill`
- `MPBar_Background`
- `MPBar_Fill`
- `PortraitMask`
- `PortraitImage`
- `SlashDivider`
- `NameText_TMP`
- `HPLabel_TMP`
- `HPValue_TMP`
- `MPLabel_TMP`
- `MPValue_TMP`

## RectTransform設定

- `BattlePartyStatusHUD_GeneratedStyle`
- Anchor Min: `(0.027, 0.671)`
- Anchor Max: `(0.360, 0.875)`
- Pivot: `(0, 1)`
- Offset Min/Max: `0`
- SizeDelta: `0`
- Local Scale: `(1, 1, 1)`
- FHD換算: 約640x220px、上部タイムライン下の左上配置

## TextMeshPro設定

- NameText: 20px, bold, outline enabled, no baked text
- HP/MP labels: 12px
- HP values: 14px, compact `current/max` format
- MP values: 13px, compact `current/max` format
- Auto Size enabled with conservative min size
- Text objects are created after bar and portrait graphics so values render above the bars.

## HP/MPバー設定

- HP uses cyan-green fill sprite from `修正案14.png`
- MP uses blue fill sprite from `修正案14.png`
- Both backgrounds use darkened bar-back sprites from the same source
- `Image.Type.Filled`
- Fill Method: Horizontal
- Fill Origin: Left
- Existing runtime HP/MP update path is preserved through `PartyMemberStatusRowView.SetStatus`

## 顔アイコンMask設定

- Portrait area is a wide horizontal mask on the right side of each row.
- Preview portrait strips are cut from the source image.
- `PartyMemberStatusRowView` now preserves portrait aspect and cover-fits the sprite into the wide mask area.
- Square face sprites are cropped into the horizontal cut-in area instead of becoming tiny thumbnails.

## 既存スクリプトとの接続結果

- `BattleManager.battlePartyStatusHudPrefab` remains linked by the editor setup script.
- `BattleTimelinePrototypeController` runtime placement now uses the larger generated-style anchor range.
- `BattlePartyStatusHUD` and `PartyMemberStatusRowView` continue to handle member refresh, HP/MP fill, selected-state toggle, and portrait replacement.
- Preview members are serialized with the cut-in portrait strips from `修正案14.png`, so FHD verification shows three distinct wide portraits.
- Existing fallback panel path remains but is not double-displayed when the new HUD prefab is available.

## FHD確認結果

- HUD-only Unity FHD render: `Screenshots/battle_party_status_hud_unity_render.png` (`1920x1080`)
- Game View overlap check: `Screenshots/battle_party_status_hud_game_view_overlap_check.png` (`1920x1080`)
- Prefab setup/capture menu logs: no errors or warnings.
- Play Mode smoke check: BattleScene initialized with only the normal saved-deck log and no errors/warnings.
- Game View check result: HUD sits below the top timeline, does not overlap the COMMAND panel, and does not overlap the center `Standard Attack` label.

## 変更ファイル一覧

- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`
- `Assets/Scripts/BattleTimelinePrototypeController.cs`
- `Assets/Scripts/UI/PartyMemberStatusRowView.cs`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
- `Assets/Scenes/BattleScene.unity`
- `Assets/Art/UI/Battle/PartyStatusHUD/*.png`
- `Screenshots/battle_party_status_hud_unity_render.png`
- `Screenshots/battle_party_status_hud_game_view_overlap_check.png`
- `.prompts/battle_party_status_hud_generated_style_fix.md`

## 未解決事項

- Runtime MP still uses the current placeholder connector based on ally speed until authoritative MP data exists.
