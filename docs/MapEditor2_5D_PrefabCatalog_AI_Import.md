# Map Editor 2.5D PrefabCatalog AI Import Prep

This memo defines the minimum catalog and importer rules for externally generated map art.
The Unity editor only imports already generated PNG files. It must not call image generation tools.

## Importer MVP

Source folder:

```text
Assets/Generated/MapAssets/{packId}/
```

Recommended pack layout:

```text
Assets/Generated/MapAssets/{packId}/Floor/*.png
Assets/Generated/MapAssets/{packId}/Bridge/*.png
Assets/Generated/MapAssets/{packId}/Stair/*.png
Assets/Generated/MapAssets/{packId}/Wall/*.png
Assets/Generated/MapAssets/{packId}/Railing/*.png
Assets/Generated/MapAssets/{packId}/Prop/*.png
Assets/Generated/MapAssets/{packId}/Marker/*.png
```

The importer also accepts file-name prefixes such as `Floor_NeonDeck_A01.png`.
Uncategorized PNGs are skipped to avoid accidental map-data registration.

Generated output in the selected pack:

```text
Assets/Generated/MapAssets/{packId}/Materials/
Assets/Generated/MapAssets/{packId}/Prefabs/
```

Open the importer from `Tools/NeonCardia/Map Editor 2.5D/Import Generated Map Asset Pack`
or from the 2.5D Map Editor catalog section.

## Naming

- `prefabId` is a stable ASCII id used by map data. Do not rename it after maps reference it.
- Recommended format: `<Category>_<AssetName>_<Variant>`, for example `Floor_NeonDeck_A01`.
- `displayName` is the editor-facing name. Keep it short and readable in the palette.
- Prefab asset names should match `prefabId` unless there is a strong reason not to.
- To replace a placeholder prefab entry, name the PNG after the existing `prefabId`, for example `PlatformTile_1x1.png`.

## Categories

- `Floor`: walkable base tile.
- `Bridge`: walkable connector tile.
- `Stair`: walkable level connector.
- `Wall`: blocking vertical boundary.
- `Railing`: blocking edge guard.
- `Prop`: object placed on a cell. Explicitly decide whether it blocks movement.
- `Marker`: editor/runtime marker, not final map art.

`Floor`, `Bridge`, and `Stair` are placed as tile instances.
`Wall`, `Railing`, `Prop`, and `Marker` are placed as prop instances.

## Required Catalog Fields

- `prefabId`: stable map-data key.
- `displayName`: short palette label.
- `prefab`: concrete prefab reference.
- `category`: one of the categories above.
- `defaultWalkable`: initial walkability copied to placed tile instances.
- `defaultBlocksMovement`: initial blocking flag copied to placed prop instances.
- `defaultRotationY`: initial Y rotation used when selecting the prefab in the editor.
- `defaultOffset`: placement offset for prop/marker instances.
- `notes`: short intent, usage, or import caveat.

For `Prop`, always make an explicit `defaultBlocksMovement` decision.
Decorative props can use `false`; collision props should use `true`.

## Validation Rules

The editor Validate button should warn when:

- `prefabId` is duplicated.
- A catalog entry has an empty prefab reference.
- `Floor` or `Bridge` has `defaultWalkable = false`.
- `Prop` has no explicit `defaultBlocksMovement` decision.

## Visual Check Before Production Use

After registering a prefab, refresh the editor preview and check:

- The prefab sits in the expected cell footprint.
- The default rotation faces the intended map direction.
- Walkable tiles do not visually imply blocked movement.
- Blocking props clearly read as obstacles at gameplay camera distance.
- Markers remain distinct from production map art.
- No asset hides spawn, encounter area, transition, or player readability markers.

Keep the catalog conservative until the map runtime and editor preview both read clearly.

## Replacement Flow

To replace an existing placeholder entry:

1. Put the PNG under the matching category folder.
2. Name the PNG with the existing `prefabId`, such as `Bridge_1x1.png`.
3. Run the importer with the placeholder catalog selected in the Map Editor.
4. Refresh the preview and check footprint, rotation, scale, and walkability.

If the PNG uses a new name, the importer adds a new catalog entry instead of replacing an existing one.
