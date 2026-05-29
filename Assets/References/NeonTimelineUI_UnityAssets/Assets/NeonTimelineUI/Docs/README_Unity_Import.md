# NeonTimelineUI Unity Assets

添付画像から、Unity の UI で使いやすいように背景透過PNGへ分割した素材です。

## 入っている主な素材

- `UI_Timeline_Full_Transparent.png`  
  画像全体を背景透過した完成見本です。まずはこれを Unity の Canvas 上に置くと、見た目確認ができます。

- `UI_LeftCounterPanel_NoText.png`  
  左側の数値表示パネルです。数字は消してあるので、TextMeshPro などで `150` やCT値を重ねてください。

- `UI_SlotFrame_CurrentGold.png`  
  現在行動中のキャラ用の金フレームです。中央を透過済みです。

- `UI_SlotFrame_AllyCyan.png`  
  味方/通常枠用のシアンフレームです。中央を透過済みです。

- `UI_SlotFrame_EnemyOrange.png`  
  敵枠用のオレンジフレームです。中央を透過済みです。

- `SampleIcons/`  
  元画像から切り出した動作確認用アイコンです。実ゲームでは自作キャラアイコンに差し替える想定です。

## Unityへの入れ方

1. このZIPを展開し、`Assets/NeonTimelineUI` フォルダを Unity プロジェクトの `Assets` 配下へコピーします。
2. PNGを選択し、Inspector で以下を推奨設定にします。
   - Texture Type: `Sprite (2D and UI)`
   - Sprite Mode: `Single`
   - Alpha Is Transparency: `ON`
   - Mesh Type: `Full Rect`
   - Filter Mode: `Bilinear`
   - Compression: `None` または `High Quality`
3. Canvas の上部に `Image` を配置し、まずは `UI_Timeline_Full_Transparent.png` を置いて見た目確認します。
4. 実装時は、各スロットを以下の重ね順にすると使いやすいです。

```text
SlotRoot
  ├─ IconImage        ← キャラ顔アイコン
  └─ FrameImage       ← UI_SlotFrame_AllyCyan / EnemyOrange / CurrentGold
```

## 1920x1080想定の配置目安

- TimelineRoot
  - Anchor: Top Center
  - Pos Y: `-70` ～ `-100`
  - Width: `1800` 前後
  - Height: `260` 前後

- 通常スロット
  - Width: `150` ～ `175`
  - Height: `170` ～ `200`
  - Spacing: `10` ～ `20`

- Current（金枠）スロット
  - 通常スロットより `1.15` ～ `1.25` 倍大きめ

- 左カウンター
  - `UI_LeftCounterPanel_NoText.png` を Image に設定
  - 子要素に TextMeshPro を置いて中央寄せ
  - フォントサイズは画面に合わせて `48` ～ `72` 付近から調整

## Codexへ渡す実装指示例

```text
Assets/NeonTimelineUI/Textures にあるPNG素材を使い、BattleScene上部の行動順タイムラインHUDを実装してください。

要件:
- Canvas上部中央にTimelineRootを作成する。
- 左側に UI_LeftCounterPanel_NoText.png を置き、子要素のTextMeshProでCT値を中央表示する。
- 行動順スロットは IconImage を下、FrameImage を上に重ねる構造にする。
- 味方は UI_SlotFrame_AllyCyan.png、敵は UI_SlotFrame_EnemyOrange.png、現在行動中は UI_SlotFrame_CurrentGold.png を使う。
- Currentスロットは通常より少し大きくし、黄色い選択中感が出るようにする。
- 既存のBattleScene.unityの他要素を壊さず、UI追加部分だけを最小変更で実装する。
- 変更後、Gameビューで上部タイムラインが1920x1080基準で破綻していないか確認する。
```

## 注意点

この素材は1枚の参照画像から自動抽出したラスターPNGです。  
そのため、完全なベクター素材や本格的な9-slice素材ではありません。  
かなり大きく拡大・縮小する場合は、Unity側で見た目を確認しながらサイズ調整してください。
