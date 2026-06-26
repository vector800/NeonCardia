# Party Portrait Row2 / Row3 Numeric Fix

## 作業目的
- 左上パーティステータスHUDの顔アイコン表示を、2段目メカ顔を中央に寄せた状態にする。
- 3段目少女を、修正後2段目と同じ余白感・拡大率・表示密度に近づける。
- 1段目狼、HP数値/バー、タイムライン、コマンドUI、バトルフィールド、ゲームロジックは変更しない。

## バックアップ
- 保存先: `.prompts/backups/party_portrait_row2_row3_numeric_fix/`
- 保存内容:
  - `git_status_before.txt`
  - `candidate_diff_before.patch`
  - `restore_instructions.md`
  - 関連Prefab / Sprite / Editor Script / runtime UI script候補のコピー
- 復元方法: `.prompts/backups/party_portrait_row2_row3_numeric_fix/restore_instructions.md` を参照。

## 参考画像との差分分析
- 2段目メカ顔は、横長HUD内で顔パーツが左側に寄って見えていた。
- 3段目少女は、2段目を中央寄せした後の密度基準と比べ、Loop 1ではやや顔全体を見せすぎてHUD用の目元寄り感が弱かった。
- 1段目狼は既に許容範囲で、今回の修正対象から除外した。

## 2段目だけの原因分析
- `PortraitArea` / `PortraitClipMask` / `PortraitImage` のRectTransform、Scale、Anchor、Pivotは3段とも同一。
- `SlashDivider_Image` は非表示で、2段目だけに白線や斜線が重なっている状態ではなかった。
- `UIPortraitCoverCrop` が3段とも同じZoom / Overscanで表示していた。
- 2段目の違和感はUI階層差ではなく、メカSprite内部の顔位置が左寄りで、表示オフセットがゼロだったことが主因。
- 2段目Sprite自体は768x168で解像度は十分。Import SettingsもSprite UI向けで、ぼやけの主因ではなかった。

## 1段目・3段目との差分
- 1段目: 現状維持。Loop 1からLoop 2のピクセル差分は0。
- 2段目: `UIPortraitCoverCrop.offset` のみ右方向へ調整。Loop 1からLoop 2のピクセル差分は0。
- 3段目: Sprite切り出しのみ再調整。Loop 1からLoop 2で変化したのは3段目のみ。

## 修正前の問題点
- 2段目メカ顔:
  - 目元/フェイスプレートが左寄りに見え、右側とのバランスが悪い。
  - UI側のScale差ではなく、Sprite内部構図とOffset 0の組み合わせで中心がずれていた。
- 3段目少女:
  - 既存の最終状態は2段目と比較したときに表示密度が揃いにくく、再切り出しが必要だった。

## 修正方針
1. 2段目はUI側で最小変更する。
   - `portraitOffset[1] = new Vector2(27f, 0f)`
   - Zoom / Mask / RowFrame / HP表示は変更しない。
2. 3段目は既存少女素材から再切り出しする。
   - 768x168の横長HUD用Spriteを維持。
   - 両目が見える構図を維持。
   - 2段目に近い目元密度に寄せる。
3. Import SettingsはPrefab反映メニューの`ConfigureSprite`でSprite UI向けに再適用する。

## Loop 1
- 変更:
  - `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`
    - `portraitOffset[1]` を `{x:27, y:0}` に変更。
  - `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png`
    - `g_s520_x120_y-214` 候補へ差し替え。
- 確認:
  - `Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup` 実行。
  - Unity Console error: 0。
  - `Tools/NeonCardia/Capture Battle Party Status HUD FHD Check` で1920x1080確認。
  - 保存: `Screenshots/party_portrait_row2_row3_numeric_loop1.png`
- 結果:
  - 2段目の左寄りは解消。
  - 3段目は余白は近いが、顔全体寄りでHUDの目元密度が少し弱い。

## Loop 2
- 変更:
  - 2段目はLoop 1のまま維持。
  - 3段目Spriteのみ `g_s590_x90_y-236` 候補へ差し替え。
- 確認:
  - Unity Asset refresh。
  - `Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup` 実行。
  - Unity Console error: 0。
  - `Tools/NeonCardia/Capture Battle Party Status HUD FHD Check` で1920x1080確認。
  - 保存:
    - `Screenshots/party_portrait_row2_row3_numeric_loop2.png`
    - `Screenshots/party_portrait_row2_row3_numeric_loop2_hud_crop_zoom.png`
    - `Screenshots/party_portrait_row2_row3_numeric_loop2_row2_mask_crop_zoom.png`
    - `Screenshots/party_portrait_row2_row3_numeric_loop2_row3_mask_crop_zoom.png`
- 結果:
  - 2段目は中央寄りで自然。
  - 3段目は両目を保ったまま、Loop 1より2段目に近い目元密度になった。
  - Loop 1 -> Loop 2 のピクセル差分:
    - row1: 0 / 5425
    - row2: 0 / 5425
    - row3: 3417 / 5735

## 最終的な数値メモ
- 画像サイズ:
  - Wolf clean: 768x168
  - Mech clean: 768x168
  - Girl clean: 768x168
- `UIPortraitCoverCrop`:
  - row1 offset: `{x:0, y:0}`
  - row2 offset: `{x:27, y:0}`
  - row3 offset: `{x:0, y:0}`
  - zoom: `1.00`
  - overscan: `{x:4, y:2}`
- Prefab内確認:
  - 2段目`UIPortraitCoverCrop.offset` は `{x:27, y:0}`。
  - 1段目/3段目のOffsetは0。

## Import Settings
- `ConfigureSprite` により以下を再適用:
  - Texture Type: Sprite
  - Sprite Mode: Single
  - Mip Maps: Off
  - Alpha Is Transparency: On
  - Compression: Uncompressed
  - Max Size: 2048
  - Filter Mode: Bilinear
- 手作業のmeta編集は行っていない。

## 修正したPrefab
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
  - 2段目`UIPortraitCoverCrop.offset` が `{x:27, y:0}` になった。
  - 3段目は同じSprite参照の内容差し替えで表示が更新された。

## 修正したSprite
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png`
  - 既存少女素材から再切り出し。
  - 768x168。
  - 両目が見えるHUD向け横長構図。
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`
  - 画像素材自体は変更なし。UI Offsetで調整。

## 修正したScript
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`
  - Prefab生成時の2段目Portrait offsetを設定。
- Runtime scriptは変更なし。

## 変更ファイル一覧
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png`
- `.prompts/party_portrait_row2_row3_numeric_fix.md`
- `.prompts/backups/party_portrait_row2_row3_numeric_fix/`
- `Screenshots/party_portrait_row2_row3_numeric_fix_check.png`
- `Screenshots/party_portrait_row2_row3_numeric_final_zoom.png`
- `Screenshots/party_portrait_row2_row3_numeric_final_row2_zoom.png`
- `Screenshots/party_portrait_row2_row3_numeric_final_row3_zoom.png`

## FHD確認結果
- 1920x1080 HUD確認レンダー:
  - `Screenshots/party_portrait_row2_row3_numeric_fix_check.png`
- 拡大確認:
  - `Screenshots/party_portrait_row2_row3_numeric_final_zoom.png`
  - `Screenshots/party_portrait_row2_row3_numeric_final_row2_zoom.png`
  - `Screenshots/party_portrait_row2_row3_numeric_final_row3_zoom.png`
- 判定:
  - 2段目メカ顔の左寄りは解消。
  - 3段目少女は2段目に近い目元密度へ改善。
  - 1段目は変更なし。
  - HP数値/バー、他HUDへの副作用は確認されなかった。
  - Unity Console error: 0。

## 採点
- 2段目が中央寄りで自然: 2 / 2
- 3段目が2段目の密度・余白感に近い: 1.8 / 2
- 1段目・3段並びの統一感: 1.8 / 2
- 解像感維持: 2 / 2
- 他UI副作用なし: 2 / 2
- 合計: 9.6 / 10

## 未解決事項
- なし。
