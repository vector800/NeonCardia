## 作業目的

左上パーティステータスHUDの3段顔アイコンのうち、2段目のメカ / ロボ顔だけがアップになりすぎていたため、1段目の犬と3段目の少女を変更せず、2段目だけを自然な横長HUDポートレートへ調整する。

## 参考画像との差分分析

現在の2段目は、参考画像2枚目のような「顔全体の形が読める横長ポートレート」ではなく、額・バイザー・耳部品が大きく切り出された接写に近かった。HUD実寸では顔パーツの見える範囲が狭く、1段目/3段目より窮屈に見えていた。

## 2段目だけの原因分析

- `BattlePartyStatusHUD.prefab` の3行は、`PortraitClipMask` / `PortraitImage` / `UIPortraitCoverCrop` の設定が同一。
- `zoom=1`, `overscan=(4,2)`, `offset=(0,0)` が3行共通。
- `PortraitImage` は3行とも `Image.Type.Simple`, `preserveAspect=true`。
- Import Settings は3素材とも Sprite (2D and UI), Single, alpha transparency enabled, mipmaps off, max size 2048, default compression none, bilinear filtering。
- Script側も2段目だけScaleやMaskを変えていない。`BattleTimelinePrototypeController.GetPartyStatusHudFaceIcon` が2段目に `UI_BattlePartyHUD_FaceClean_Mech.png` を割り当てているだけ。
- よって原因はUI配置ではなく、2段目Sprite自体の切り出し/構図が近すぎること。

## 1段目・3段目との差分

- Prefab RectTransform / Mask / Crop設定: 差分なし。
- Import Settings: 実質差分なし。
- Scriptの動的表示制御: 2段目だけ拡大する処理なし。
- 使用Sprite: 2段目のみ `UI_BattlePartyHUD_FaceClean_Mech.png`。このSpriteだけ顔の占有率が大きく、接写感が強かった。

## 修正前の問題点

- メカ顔がアップすぎる。
- 表示範囲が狭く、ヘルメットとバイザーだけが窮屈に見える。
- 1段目犬、3段目少女に比べて2段目だけ顔の距離感が異なる。

## 修正方針

UI側の3行共通設定は変更しない。2段目だけのSpriteを、元の `AllyFaceIcon_01.png` から再構成する。顔画像にフレーム、文字、白線は焼き込まない。背景は既存HUDに馴染む暗い青系のみにする。

## 1回目の修正内容

`UI_BattlePartyHUD_FaceClean_Mech.png` を、元アイコンを315px相当に縮小して右側へ配置する構図に変更した。

## 1回目の確認結果

アップ感は解消したが、FHD HUD実寸ではメカ顔が右に寄りすぎ、表示面積が小さすぎた。3段並びでは2段目だけ余白が強く、統一感が不足した。

## 2回目の修正内容

元アイコンを380px相当に拡大し、1回目より左へ戻して配置した。

## 2回目の確認結果

1回目より見える範囲は自然になったが、まだ少し右寄りで小さく見えた。接写問題は解消していたが、並びのバランスをさらに改善する余地があった。

## 追加ループ内容

3回目として、元アイコンを420px相当にし、さらに左へ寄せた。顔全体の形と耳部品が読め、かつ以前のような極端な接写には戻らない位置にした。

## 最終的な採点

- 参考画像2枚目に近い見え方: 1.7 / 2
- 寄りすぎたアップ感の解消: 2.0 / 2
- 1段目・3段目と並んで自然: 1.8 / 2
- 解像感の維持/改善: 1.8 / 2
- 1段目・3段目や他UIへの副作用なし: 2.0 / 2
- 合計: 9.3 / 10

## 修正したPrefab

なし。今回のターンではPrefab構造・RectTransform・Maskは変更していない。

## 修正したSprite

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`

## 修正したImport Settings

なし。既存のSprite import設定は維持。

## 修正したScript

なし。今回のターンではScript変更なし。

## 変更ファイル一覧

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`
- `.prompts/party_portrait_middle_only_fix.md`
- `Screenshots/party_portrait_middle_fix_check.png`
- `Screenshots/party_portrait_middle_fix_final_zoom.png`
- 途中確認用: `Screenshots/party_portrait_middle_fix_loop1_zoom.png`, `Screenshots/party_portrait_middle_fix_loop2_zoom.png`, `Screenshots/mech_middle_candidates_loop1.png`, `Screenshots/mech_middle_candidates_loop2.png`, `Screenshots/mech_middle_candidates_loop3.png`

## FHD確認結果

`Screenshots/party_portrait_middle_fix_check.png` で1920x1080確認済み。2段目のアップ感は解消し、顔パーツの見える範囲が広がった。1段目犬と3段目少女は変更していない。HP数値、バー、RowFrame、他HUDへの副作用は見られない。Unity Console error: 0。

## 未解決事項

なし。より参考画像へ寄せる場合は、将来的にメカ顔専用の高解像度ポートレートを別途作る余地はあるが、今回の最小変更範囲では完了。
