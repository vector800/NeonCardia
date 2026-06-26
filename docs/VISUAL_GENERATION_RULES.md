# VISUAL_GENERATION_RULES.md

Codex / imagegen / Imagen 系スキルで、ゲーム UI・HUD・アイコン・エフェクト素材を生成するときの品質ルール。
目的は「AI が作ったように見える派手な絵」を避け、実際のゲーム開発チームが仕様・制約・実装都合の中で作ったように見える素材にすること。

---

## 0. 最重要方針

**見た目の豪華さより、用途・制約・一貫性・実装可能性を優先する。**

生成時は必ず次の順序で進める。

1. 何に使う素材かを明確にする。
2. 画面内サイズ、ゲームジャンル、背景色、視認距離、状態差分を決める。
3. 既存 UI / 既存 VFX に合わせる。
4. 禁止表現を先に決める。
5. 生成後に「AI っぽさチェック」を行い、失敗したら再生成する。

---

## 1. AI っぽく見える典型パターン

以下が 1 つでも目立つ場合は失敗として扱う。

- 意味のない過剰な発光、ネオン、粒子、グロー
- ガラス風、メタリック、虹色グラデーションの乱用
- どこにも使えない装飾だけの UI
- すべてが中央揃え・左右対称・過剰に整いすぎている構図
- 立体感、影、反射、光源が矛盾している
- ボタンやパネルの押せる領域が不明確
- ダミー文字、読めない文字、謎の記号、意味不明なアイコン
- 画面全体が「作品紹介画像」になっていて、ゲーム内素材に見えない
- 1 枚の画像内で、素材・線幅・角丸・影・色温度が統一されていない
- “premium / futuristic / cinematic / epic / ultra detailed” のような曖昧な盛り言葉に引っ張られている

---

## 2. 禁止ワード / 原則禁止表現

画像生成プロンプトでは、明確な理由がない限り次の語を使わない。

- sleek
- futuristic
- cinematic
- epic
- ultra detailed
- award winning
- trending on ArtStation
- beautiful modern UI
- premium glassmorphism
- holographic
- neon glow
- magical particles everywhere
- highly detailed fantasy interface
- hyper realistic

代わりに、制作条件として書く。

悪い例:
> futuristic premium glowing game UI button

良い例:
> 48px high rectangular inventory button for a dark forest RPG HUD, muted brass trim, slightly worn edges, readable at gameplay scale, one small selected-state amber rim light, no glass, no neon, no fake text

---

## 3. UI 生成ルール

UI を生成する場合は、必ず以下を指定する。

### 3.1 用途

- HUD
- メニュー
- インベントリ
- スキルツリー
- ダイアログ
- ショップ
- リザルト
- ボタン
- アイコン
- カード
- ゲージ
- 通知

用途が不明な UI は生成しない。

### 3.2 画面内サイズ

必ずピクセル感を指定する。

例:
- 32x32 icon
- 64x64 skill icon
- 128x128 item icon
- 320x96 status panel
- 1920x1080 HUD mockup, actual UI scale, not poster art

### 3.3 状態差分

ボタン・カード・ゲージは、必要なら状態を分ける。

- normal
- hover
- pressed
- disabled
- selected
- warning
- cooldown

1 枚絵に全部を詰め込まず、必要な状態だけを作る。

### 3.4 可読性

- テキストは原則として画像に焼き込まない。
- どうしても必要な場合は、短い実在テキストだけを使う。
- ダミー文字、読めない文字、架空言語風の記号は禁止。
- アイコンは 50% 縮小でも意味が分かるシルエットにする。

### 3.5 素材・形状

- 角丸、線幅、影、縁取り、余白を統一する。
- 装飾は機能を邪魔しない。
- 押せるもの、読ませるもの、背景に回すものを明確に分ける。
- 反射や光沢は 1 つの光源に従う。

---

## 4. VFX / エフェクト生成ルール

エフェクトは「派手な静止画」ではなく、ゲーム内で使える発生・ピーク・消滅の素材として考える。

### 4.1 必ず決めること

- 何のエフェクトか: hit, slash, heal, charge, explosion, buff, debuff, shield, aura, pickup, level up など
- 表示時間: 0.12s, 0.25s, 0.6s など
- 発生位置: character center, weapon tip, ground contact, UI overlay など
- 視認距離: small icon scale, character scale, full-screen moment など
- 背景: transparent, dark background preview, flat chroma key background など
- 使用方法: sprite sheet, single burst, loopable texture, alpha overlay など

### 4.2 段階を分ける

可能なら次の 3 段階を意識する。

1. Anticipation: 小さな予兆、方向性、溜め
2. Impact/Core: 最も明るい中心、形が読み取れる瞬間
3. Dissolve/Residue: 粒子の消え方、残光、破片、煙

### 4.3 禁止

- 画面全体を覆うだけの意味のない粒子
- どの攻撃・属性・状態異常か分からない光
- 透明背景で使う前提なのに黒背景のグローに依存する表現
- 主役キャラクターや UI を隠す過剰な明度
- 素材として切り出せない複雑な背景込みの絵

### 4.4 良い方向性

- シルエットが一瞬で読める
- 中心、方向、拡散先が明確
- 小さなノイズや欠けがあり、完全な左右対称ではない
- 属性色は 1〜2 色に絞る
- 粒子は量より配置と減衰を重視する

---

## 5. “人間が作った感”を出すための制約

AI っぽさを消すには、意図的に制約を与える。

### 5.1 制作制約

プロンプトに次を入れる。

- limited palette
- production asset, not key art
- fits existing HUD
- gameplay scale readability
- single consistent light source
- slight asymmetry
- restrained effects
- no fake text
- no unnecessary ornament
- no poster composition

### 5.2 ほどよい不完全さ

以下は許可する。

- わずかな摩耗
- 角や縁の小さな欠け
- 左右非対称な傷
- 使い込まれた素材感
- UI の装飾密度に差をつける
- 主要部分以外の情報量を落とす

ただし、雑・低品質・破綻とは違う。あくまで「制作意図のある不均一さ」にする。

---

## 6. 生成前テンプレート

画像生成前に、Codex は必ずこの仕様カードを作る。

```md
## Visual Direction Card

- Asset type:
- In-game purpose:
- Target resolution / aspect ratio:
- On-screen size:
- Background / transparency:
- Style family:
- Color palette:
- Material language:
- Lighting rule:
- Shape language:
- Readability requirement:
- Animation / state requirement:
- Must include:
- Must avoid:
- Negative prompt:
```

このカードが曖昧なまま imagegen / Imagen を実行しない。

---

## 7. 生成プロンプトの構造

プロンプトは次の順番で書く。

```txt
[用途] + [ゲーム内文脈] + [サイズ/形式] + [素材/形状] + [色数] + [光源] + [視認性] + [制約] + [禁止事項]
```

### UI プロンプト例

```txt
Create a 320x96 game HUD status panel for a dark forest survival RPG, production asset not poster art. Muted charcoal leather base, dull brass corner brackets, 2px inner rim, readable empty center area for real UI text added later. Single top-left warm light source, subtle worn edges, slight asymmetry in scratches, limited palette of charcoal, brass, and low-saturation amber. Actual gameplay scale, no glassmorphism, no neon, no fake text, no decorative symbols, no cinematic background, no excessive glow.
```

### スキルアイコン例

```txt
Create a 64x64 skill icon for a poison trap ability in a tactical RPG. Strong readable silhouette: small metal caltrop with one cracked green vial leaking onto the floor. Limited palette: dull steel, dark moss green, muted yellow highlight. Single light source from upper left, thick readable rim, transparent background or flat removable background. Production game icon, not concept art. No fake letters, no neon, no symmetric mandala, no complex scenery, no excessive particles.
```

### エフェクト例

```txt
Create a transparent-background VFX sprite concept for a 0.25 second sword hit impact at character scale. Crescent slash shape moving left to right, pale desaturated blue core with small white impact edge, sparse angular shards trailing behind, readable silhouette at 50% scale. Single burst frame, production asset, no character, no background, no screen-filling particles, no magical fog, no cinematic poster lighting, no random symbols.
```

---

## 8. 生成後チェック

生成後、Codex は次を 0 / 1 / 2 点で採点する。

- 用途が一瞬で分かる
- 画面内サイズで読める
- 既存ゲームの UI / VFX に混ぜても浮かない
- 装飾が機能を邪魔していない
- 光源と影が矛盾していない
- 文字や記号に破綻がない
- “AI が盛った感” がない

合計 12 点未満なら再生成する。
0 点の項目が 1 つでもあれば再生成する。

---

## 9. 再生成指示の書き方

悪い再生成指示:
> もっと自然にして

良い再生成指示:
> Reduce glow by 60%, remove holographic gradients, make the brass trim flatter and more worn, leave more empty center space for actual UI text, keep only one amber highlight, make scratches asymmetrical but subtle, preserve the 320x96 silhouette.

---

## 10. 保存ルール

生成に使った最終プロンプトは必ず `.prompts/` に保存する。

推奨ファイル名:

```txt
.prompts/YYYY-MM-DD_asset-name_prompt.md
```

保存内容:

```md
# Asset Prompt: <asset-name>

## Purpose

## Final Prompt

## Negative Prompt

## Result Notes

## Regeneration Notes
```
