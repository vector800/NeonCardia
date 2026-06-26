# Battle Party Status HUD Portrait Corner Clip Fix

Date: 2026-06-05

## Visual Direction Card

- Asset type: UI portrait clip mask for compact battle party status HUD
- In-game purpose: clip right-side ally face cut-ins to the same slanted shape as the metal portrait frame
- Target resolution / aspect ratio: 180x44 mask sprite, verified at 1920x1080 Game View
- On-screen size: approximately 156x36 px per portrait area in the upper-left HUD
- Background / transparency: transparent outside the white mask shape
- Style family: production UI utility mask, not visible decoration
- Color palette: white alpha mask only
- Material language: no metal, no frame, no glow; frame decoration remains in PortraitFrame_Image
- Lighting rule: none; mask is functional only
- Shape language: slanted horizontal cut-in polygon matching the portrait frame interior
- Readability requirement: no visible rectangular portrait corners; no empty padding
- Animation / state requirement: selected highlight must not move PortraitImage
- Must include: filled white slanted mask shape, transparent corners, no border
- Must avoid: RectMask2D-only clipping, using PortraitFrame_Image as a mask, shrinking the portrait to hide corners, SetNativeSize, distorted portraits
- Negative prompt: no fake text, no glow, no frame strokes, no decoration, no rectangular opaque background, no blurred border from mipmaps/compression

## Cause Analysis

The old hierarchy used a child named PortraitMask with an Image and Mask. However, the assigned sprite UI_BattlePartyHUD_PortraitMask.png had full opaque alpha across its entire 512x160 rectangle. That made the actual child clip rectangular even though PortraitFrame_Image looked slanted in front.

The frame was therefore visual decoration only. It did not define the mask shape, and RectMask2D was not present in the HUD prefab. PortraitImage was already a child of the mask object, but the mask graphic itself was not a slanted filled shape.

## Before Hierarchy

```text
PortraitArea
|- PortraitMask
|  |- Image
|  |- Mask
|  |- PortraitImage
|- PortraitFrame_Image
|- SlashDivider_Image
```

Before-state findings:

- PortraitImage was under PortraitMask.
- PortraitMask used Mask, not RectMask2D.
- RectMask2D was not present in BattlePartyStatusHUD.prefab.
- PortraitFrame_Image was only an overlay, not an actual mask.
- UI_BattlePartyHUD_PortraitMask.png was fully opaque, so it clipped as a rectangle.
- PortraitImage used no custom material in the generated prefab.

## After Hierarchy

All three rows now have the same hierarchy:

```text
PortraitArea
|- PortraitClipMask
|  |- Image
|  |- Mask
|  |- PortraitImage
|- PortraitFrame_Image
|- SlashDivider_Image
```

Verified Prefab counts:

- PortraitClipMask: 3
- PortraitMask: 0
- PortraitImage: 3
- PortraitFrame_Image: 3
- SlashDivider_Image: 3
- UIPortraitCoverCrop: 3
- RectMask2D in prefab: false

Verified child order for all rows:

```text
PortraitArea children: PortraitClipMask, PortraitFrame_Image, SlashDivider_Image
PortraitClipMask children: PortraitImage
```

## Mask / RectMask2D Usage

- PortraitClipMask has Image + UnityEngine.UI.Mask.
- Mask.showMaskGraphic is false for all 3 masks.
- RectMask2D is not used for the portrait cut-ins.
- PortraitClipMask Image has useSpriteMesh enabled.
- PortraitImage has Maskable enabled.
- PortraitImage material is None.

## Added PortraitClipMask Asset

Added:

- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattleHUD_PortraitClipMask.png

Asset contents:

- 180x44 transparent PNG
- white filled slanted polygon only
- transparent corners
- no frame line
- no decoration
- no glow

Alpha inspection:

- size: 180x44
- alpha bbox: (0, 3, 179, 41)
- corner alpha: 0, 0, 0, 0
- nonzero alpha pixels: 6020 / 7920

## PortraitImage RectTransform / Cover Crop

UIPortraitCoverCrop remains the cover-crop controller.

It binds to PortraitClipMask.rectTransform and PortraitImage, then:

- reads the mask rect size
- reads the sprite aspect ratio
- sizes PortraitImage so the mask area is covered
- applies overscan
- applies per-character zoom and offset
- keeps RectTransform localScale at (1, 1, 1)
- does not call SetNativeSize

Prefab preview crop values:

- Cyber Wolf: zoom 1.14, overscan (16, 10), offset (4, 0)
- Armor Ally: zoom 1.18, overscan (16, 10), offset (12, 0)
- Blue Girl: zoom 1.12, overscan (16, 10), offset (6, 1)

## Import Settings

Configured by BattlePartyStatusHudPrefabSetup:

- Texture Type: Sprite (2D and UI)
- Alpha Is Transparency: On
- Mip Maps: Off
- Default texture compression: Uncompressed
- Mesh Type: Tight for UI_BattleHUD_PortraitClipMask.png
- Default platform Max Size: 1024
- Image.useSpriteMesh: On for PortraitClipMask

Existing portrait source sprites remain Sprite UI assets with alpha transparency and no mipmaps.

## Changed Prefab

- Assets/Prefabs/UI/BattlePartyStatusHUD.prefab

The prefab was regenerated with Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup and linked back into BattleScene.

## Changed Scripts

- Assets/Editor/BattlePartyStatusHudPrefabSetup.cs
  - Adds PortraitClipMaskPath.
  - Configures the clip mask as Tight mesh.
  - Builds PortraitClipMask/Image/Mask/PortraitImage hierarchy.
  - Keeps PortraitFrame_Image and SlashDivider_Image as front overlays.

- Assets/Scripts/UI/PartyMemberStatusRowView.cs
  - Looks for PortraitArea/PortraitClipMask first.
  - Keeps PortraitMask fallback for older prefabs.
  - Forces PortraitImage maskable on, material None, raycast off.

## FHD Verification

Unity checks:

- Script refresh and compile completed with no new errors.
- Prefab setup menu completed: BattlePartyStatusHUD prefab setup complete. BattleScene linked: True.
- Prefab-only FHD render saved:
  - Screenshots/battle_party_status_hud_unity_render.png
- 1920x1080 Play Mode Game View capture saved:
  - Screenshots/battle_party_status_hud_portrait_corner_clip_check.png
- Play Mode console after capture had no errors or warnings, only the expected saved-deck log.

Visual check result:

- Face image corners no longer appear outside the slanted portrait frame.
- PortraitImage still fills the cut-in area.
- No transparent padding is visible inside the frame.
- Portraits are not stretched or squashed.
- The middle portrait remains aligned with the other rows.
- PortraitFrame_Image and SlashDivider_Image are in front of PortraitImage.
- Selected highlight does not move PortraitImage.
- All 3 rows use the same PortraitClipMask structure.

## Unresolved

- Existing obsolete API warnings remain elsewhere in the project and were not changed for this HUD fix.
- The old UI_BattlePartyHUD_PortraitMask.png asset remains in the folder for compatibility, but it is no longer used by BattlePartyStatusHUD.prefab.
