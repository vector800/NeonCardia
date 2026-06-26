## 作業目的

左上パーティステータスHUDの3段顔アイコンのうち、2段目メカ / ロボ顔だけ表示倍率と見え方が1段目犬・3段目少女からずれていたため、2段目だけを調整して3人の顔アイコンの統一感を揃える。

## 原因分析

- `BattlePartyStatusHUD.prefab` の3行は、`PortraitImage` / `PortraitClipMask` / `UIPortraitCoverCrop` の設定が同一。
- 3行とも `zoom=1`, `overscan=(4,2)`, `offset=(0,0)`。
- `PortraitImage` は3行とも `Image.Type.Simple`, `preserveAspect=true`。
- 3素材のImport Settingsは同等。Sprite (2D and UI), Single, alpha transparency enabled, mipmaps off, max size 2048, default compression none, bilinear filtering。
- Script側で2段目だけサイズ補正する処理はない。`BattleTimelinePrototypeController.GetPartyStatusHudFaceIcon` は2段目に `UI_BattlePartyHUD_FaceClean_Mech.png` を割り当てるのみ。
- よって原因はUI側設定ではなく、2段目Sprite `UI_BattlePartyHUD_FaceClean_Mech.png` の構図が、犬/少女に比べて引きすぎ・右寄りになっていたこと。

## 1段目・3段目との差分

- PortraitImageサイズ: 差分なし。
- PortraitImage Scale: 差分なし。
- PortraitImage Position: 差分なし。
- PortraitMaskサイズ/位置: 差分なし。
- Preserve Aspect / Image Type: 差分なし。
- Import Settings: 実質差分なし。
- Script上の設定差: 2段目だけ拡大縮小する処理なし。
- 使用Sprite: 2段目のみ `UI_BattlePartyHUD_FaceClean_Mech.png`。このSpriteの顔密度が他2人と違っていた。

## 修正方針

1段目犬と3段目少女は基準として扱い、画像・Prefab・Maskを変更しない。2段目だけ、元の `AllyFaceIcon_01.png` からHUD用横長Spriteを再構成する。白線・フレーム・文字は焼き込まない。

## 1回目の修正内容

2段目Spriteを、現状より少し大きく、左寄せに再構成した。設定は `scale=470`, `position=(120,-160)` 相当。

## 1回目の確認結果

FHD確認で、2段目の小さすぎる/引きすぎる印象はかなり改善した。ただし、3段並びではまだ顔中心が少し右寄りで、肩の見え方もやや強かった。

## 2回目の修正内容

2段目Spriteだけをさらに微調整した。設定は `scale=490`, `position=(100,-172)` 相当。顔の情報量を増やしつつ、以前のような接写には戻らない範囲にした。

## 2回目の確認結果

FHD確認で、2段目の顔密度が1段目犬・3段目少女に近づいた。2段目だけ小さい/引きすぎ/浮いて見える状態は解消。犬と少女は変更していない。

## 追加ループした場合の内容

追加ループなし。Loop 2で採点基準を満たした。

## 最終採点

- 2段目の拡大率が他2人と揃っている: 1.8 / 2
- 2段目の見える範囲が自然: 1.8 / 2
- 3人並んだときの統一感がある: 1.8 / 2
- 2段目の解像感が保たれている / 改善している: 1.9 / 2
- 他2人やHUD全体に副作用がない: 2.0 / 2
- 合計: 9.3 / 10

## 修正したPrefab

なし。今回の修正ではPrefab構造・RectTransform・Maskは変更していない。

## 修正したSprite

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`

## 修正したImport Settings

なし。既存のImport Settingsを維持。

## 修正したScript

なし。今回の修正ではScript変更なし。

## 変更ファイル一覧

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`
- `.prompts/party_portrait_middle_scale_fix.md`
- `Screenshots/party_portrait_middle_scale_fix_check.png`
- `Screenshots/party_portrait_middle_scale_fix_final_zoom.png`
- 途中確認用: `Screenshots/party_portrait_middle_scale_current_zoom.png`, `Screenshots/party_portrait_middle_scale_loop1_zoom.png`, `Screenshots/mech_middle_scale_candidates_loop1.png`, `Screenshots/mech_middle_scale_candidates_loop2.png`

## FHD確認結果

`Screenshots/party_portrait_middle_scale_fix_check.png` で1920x1080確認済み。2段目の拡大率・顔密度が犬/少女に近づき、3段並びで自然になった。HP数値、バー、RowFrame、上部タイムライン、コマンドUI、バトルフィールドへの副作用は見られない。Play Mode確認時のUnity Console errorは0。

## 未解決事項

なし。
