# BattleScene Rework Plan

## Goal

`BattleTimelinePrototypeScene` で検証した新方式を、プレイヤーが実際に入る `BattleScene` の本バトル画面へ移す。プロトタイプ Scene は参考実装として残し、戻し先や比較先として使える状態を維持する。

## 既存 BattleScene から残すもの

- `Assets/Scenes/BattleScene.unity` をメイン戦闘 Scene として継続使用する。
- `BattleManager` の Script GUID は維持し、Scene 側の接続を壊さない。
- 旧 `BattleManager` 実装は `LegacyBattleManager` として同じファイル内に残し、旧パネルバトルの参照を後から拾えるようにする。
- `DeckStorage` / `DeckValidator` / `CardData.CreateStarterDeck()` を使った保存デッキ読み込みとフォールバックデッキ。
- `BattleResultOverlay` / `BattleResultData` / `HuntingLevelEvaluator` による勝利リザルト。
- `MenuScene` / `DeckBuildScene` から `BattleScene` へ入る導線。

## BattleTimelinePrototypeScene から移植するもの

- 上部タイムラインの行動順表示。
- 味方3人の `Front` / `Middle` / `Back` 表示と選択状態。
- 敵3x3グリッド、敵1-3体配置、敵HP表示。
- 共通手札5枚、カード選択、ウエポン、隊列入れ替え、選択リセット、決定。
- スプライトベースUIの背景、フレーム、タイムラインアイコン、味方枠、敵グリッド、敵スプライト、カードフレーム。
- `showDebugLabels` による詳細ラベル切り替え。

## 置き換えるもの

- `BattleManager` は新方式コントローラを起動する入口へ変更する。
- `BattleScene` 実行時のリザルトはプロトタイプ用 `TimelineBattleResultOverlay` ではなく既存 `BattleResultOverlay` を使う。
- Menu / DeckBuild の「新バトル」導線も、プレイヤー向けには `BattleScene` へ向ける。

## 今回は見送るもの

- 敵の次行動予告。
- 敵付近の予告テキスト表示。
- タイムラインアイコン上の敵予告表示。
- `Front Attack` / `Line Attack` / `Charge Attack` / `Heal` / `Delay Attack` などの予告表示。
- 予告通りに敵が行動する分岐AI。
- 旧パネルバトルの Guard / Charge / StageChange / 特殊パネル連動の完全移植。

## 実装順序

1. `BattleScene` の入口を新方式へ切り替える。
2. プロトタイプUI/ロジックを `BattleScene` 用モードに対応させる。
3. 保存デッキとフォールバックデッキを新方式カードAdapterへ流す。
4. 味方隊列、敵グリッド、タイムライン、カード、ウエポン、隊列入れ替えを接続する。
5. 敵行動は Front -> Middle -> Back 優先の簡易攻撃にする。
6. 勝利時に既存 `BattleResultOverlay` を表示し、再戦 / メニュー / デッキ編集へ戻れるようにする。
7. README に仕様、未対応カード、確認手順、変更ファイルを追記する。
